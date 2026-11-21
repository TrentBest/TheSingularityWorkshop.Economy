namespace TheSingularityWorkshop.Economy;

/// <summary>Stable identity for one ladder.</summary>
public readonly record struct LadderId(Guid Value)
{
    public static LadderId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for an externally adjudicated ladder result.</summary>
public readonly record struct LadderResultId(Guid Value)
{
    public static LadderResultId New() => new(Guid.NewGuid());
}

/// <summary>The outcome from the first competitor's perspective.</summary>
public enum CompetitionOutcome
{
    FirstWins,
    Draw,
    SecondWins
}

/// <summary>
/// An immutable, externally adjudicated result. EvidenceReference identifies the authoritative
/// evidence retained by the host; Economy does not claim to verify that evidence itself.
/// </summary>
public sealed record LadderResult
{
    public LadderResult(
        LadderResultId id,
        CompetitorId firstCompetitorId,
        CompetitorId secondCompetitorId,
        CompetitionOutcome outcome,
        DateTimeOffset recordedAt,
        string evidenceReference)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A result identifier is required.", nameof(id));
        if (firstCompetitorId.Value == Guid.Empty)
            throw new ArgumentException("The first competitor is required.", nameof(firstCompetitorId));
        if (secondCompetitorId.Value == Guid.Empty)
            throw new ArgumentException("The second competitor is required.", nameof(secondCompetitorId));
        if (firstCompetitorId == secondCompetitorId)
            throw new ArgumentException("A competitor cannot play against itself.", nameof(secondCompetitorId));
        if (!Enum.IsDefined(outcome))
            throw new ArgumentOutOfRangeException(nameof(outcome));
        if (string.IsNullOrWhiteSpace(evidenceReference))
            throw new ArgumentException("An evidence reference is required.", nameof(evidenceReference));

        Id = id;
        FirstCompetitorId = firstCompetitorId;
        SecondCompetitorId = secondCompetitorId;
        Outcome = outcome;
        RecordedAt = recordedAt;
        EvidenceReference = evidenceReference.Trim();
    }

    public LadderResultId Id { get; }
    public CompetitorId FirstCompetitorId { get; }
    public CompetitorId SecondCompetitorId { get; }
    public CompetitionOutcome Outcome { get; }
    public DateTimeOffset RecordedAt { get; }
    public string EvidenceReference { get; }
}

/// <summary>A competitor's current immutable ladder standing.</summary>
public sealed record LadderStanding(
    CompetitorId CompetitorId,
    decimal Rating,
    int MatchesPlayed,
    int Wins,
    int Draws,
    int Losses);

/// <summary>
/// Immutable Elo-style rating ladder. Results are applied in timestamp order and retained as an
/// append-only event sequence. Hosts validate match evidence before submitting a result.
/// </summary>
/// <remarks>
/// This is a rating table, not a matchmaking or scheduling service. It has no persistence,
/// anti-cheat verifier, season reset, or correction authority. To correct history, the host must
/// rebuild a new ladder snapshot from its corrected authoritative result stream.
/// </remarks>
public sealed class CompetitionLadder
{
    private const double RatingScale = 400d;
    private readonly IReadOnlyDictionary<CompetitorId, LadderStanding> _standings;
    private readonly IReadOnlyList<LadderResult> _results;

    private CompetitionLadder(
        LadderId id,
        decimal initialRating,
        decimal kFactor,
        IReadOnlyDictionary<CompetitorId, LadderStanding> standings,
        IReadOnlyList<LadderResult> results)
    {
        Id = id;
        InitialRating = initialRating;
        KFactor = kFactor;
        _standings = new System.Collections.ObjectModel.ReadOnlyDictionary<CompetitorId, LadderStanding>(
            new Dictionary<CompetitorId, LadderStanding>(standings));
        _results = Array.AsReadOnly(results.ToArray());
    }

    public LadderId Id { get; }
    public decimal InitialRating { get; }
    public decimal KFactor { get; }
    public IReadOnlyList<LadderResult> Results => _results;

    public IReadOnlyList<LadderStanding> Standings => Array.AsReadOnly(_standings.Values
        .OrderByDescending(standing => standing.Rating)
        .ThenByDescending(standing => standing.Wins)
        .ThenBy(standing => standing.CompetitorId.Value)
        .ToArray());

    public static CompetitionLadder Create(
        LadderId id,
        IEnumerable<Competitor> entrants,
        decimal initialRating = 1000m,
        decimal kFactor = 32m)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A ladder identifier is required.", nameof(id));
        ArgumentNullException.ThrowIfNull(entrants);
        if (initialRating < 0m)
            throw new ArgumentOutOfRangeException(nameof(initialRating));
        if (kFactor <= 0m)
            throw new ArgumentOutOfRangeException(nameof(kFactor));

        var snapshot = entrants.ToArray();
        if (snapshot.Length < 2)
            throw new ArgumentException("A ladder requires at least two competitors.", nameof(entrants));
        if (snapshot.Any(entrant => entrant is null))
            throw new ArgumentException("Ladder entrants cannot contain null competitors.", nameof(entrants));
        if (snapshot.Any(entrant => entrant.Id.Value == Guid.Empty))
            throw new ArgumentException("Every ladder entrant must have a valid identity.", nameof(entrants));
        if (snapshot.Select(entrant => entrant.Id).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("A competitor can appear only once in a ladder.", nameof(entrants));

        var standings = snapshot.ToDictionary(
            entrant => entrant.Id,
            entrant => new LadderStanding(entrant.Id, initialRating, 0, 0, 0, 0));
        return new CompetitionLadder(id, initialRating, kFactor, standings,
            Array.Empty<LadderResult>());
    }

    /// <summary>Applies one validated result and returns a new ladder snapshot.</summary>
    public CompetitionLadder RecordResult(LadderResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var existing = _results.FirstOrDefault(item => item.Id == result.Id);
        if (existing is not null)
        {
            if (existing == result)
                return this;
            throw new InvalidOperationException("A result identifier cannot be reused for different result data.");
        }

        if (!_standings.TryGetValue(result.FirstCompetitorId, out var first))
            throw new ArgumentException("The first competitor is not registered in this ladder.", nameof(result));
        if (!_standings.TryGetValue(result.SecondCompetitorId, out var second))
            throw new ArgumentException("The second competitor is not registered in this ladder.", nameof(result));
        if (_results.Count > 0 && result.RecordedAt < _results[^1].RecordedAt)
            throw new InvalidOperationException("Ladder results must be applied in nondecreasing timestamp order.");

        var expectedFirst = 1d / (1d + Math.Pow(10d,
            ((double)second.Rating - (double)first.Rating) / RatingScale));
        var actualFirst = result.Outcome switch
        {
            CompetitionOutcome.FirstWins => 1d,
            CompetitionOutcome.Draw => 0.5d,
            CompetitionOutcome.SecondWins => 0d,
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
        var delta = decimal.Round(KFactor * (decimal)(actualFirst - expectedFirst),
            2, MidpointRounding.AwayFromZero);

        var updatedFirst = first with
        {
            Rating = first.Rating + delta,
            MatchesPlayed = first.MatchesPlayed + 1,
            Wins = first.Wins + (result.Outcome == CompetitionOutcome.FirstWins ? 1 : 0),
            Draws = first.Draws + (result.Outcome == CompetitionOutcome.Draw ? 1 : 0),
            Losses = first.Losses + (result.Outcome == CompetitionOutcome.SecondWins ? 1 : 0)
        };
        var updatedSecond = second with
        {
            Rating = second.Rating - delta,
            MatchesPlayed = second.MatchesPlayed + 1,
            Wins = second.Wins + (result.Outcome == CompetitionOutcome.SecondWins ? 1 : 0),
            Draws = second.Draws + (result.Outcome == CompetitionOutcome.Draw ? 1 : 0),
            Losses = second.Losses + (result.Outcome == CompetitionOutcome.FirstWins ? 1 : 0)
        };

        var standings = new Dictionary<CompetitorId, LadderStanding>(_standings)
        {
            [updatedFirst.CompetitorId] = updatedFirst,
            [updatedSecond.CompetitorId] = updatedSecond
        };
        return new CompetitionLadder(Id, InitialRating, KFactor, standings, _results.Append(result).ToArray());
    }
}
