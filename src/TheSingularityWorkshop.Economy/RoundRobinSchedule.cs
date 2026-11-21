namespace TheSingularityWorkshop.Economy;

/// <summary>Stable identity for one scheduled round-robin match.</summary>
public readonly record struct RoundRobinMatchId(Guid CompetitionId, int RoundNumber, int MatchNumber)
{
    public bool IsValid => CompetitionId != Guid.Empty && RoundNumber > 0 && MatchNumber > 0;
}

/// <summary>Outcome from the first scheduled competitor's perspective.</summary>
public enum RoundRobinOutcome
{
    FirstWins,
    Draw,
    SecondWins
}

/// <summary>Stable identity for one externally adjudicated round-robin result.</summary>
public readonly record struct RoundRobinResultId(Guid Value)
{
    public static RoundRobinResultId New() => new(Guid.NewGuid());
}

/// <summary>
/// Host-adjudicated result for a scheduled match. EvidenceReference points to evidence retained
/// and validated by the host; the Economy package does not inspect or verify it.
/// </summary>
public sealed record RoundRobinMatchResult
{
    public RoundRobinMatchResult(
        RoundRobinResultId id,
        RoundRobinMatchId matchId,
        RoundRobinOutcome outcome,
        DateTimeOffset recordedAt,
        string evidenceReference)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A result identifier is required.", nameof(id));
        if (!matchId.IsValid)
            throw new ArgumentException("A valid match identity is required.", nameof(matchId));
        if (!Enum.IsDefined(outcome))
            throw new ArgumentOutOfRangeException(nameof(outcome));
        if (string.IsNullOrWhiteSpace(evidenceReference))
            throw new ArgumentException("An evidence reference is required.", nameof(evidenceReference));

        Id = id;
        MatchId = matchId;
        Outcome = outcome;
        RecordedAt = recordedAt;
        EvidenceReference = evidenceReference.Trim();
    }

    public RoundRobinResultId Id { get; }
    public RoundRobinMatchId MatchId { get; }
    public RoundRobinOutcome Outcome { get; }
    public DateTimeOffset RecordedAt { get; }
    public string EvidenceReference { get; }
}

/// <summary>One immutable scheduled pairing or automatic bye.</summary>
public sealed record RoundRobinMatch(
    RoundRobinMatchId Id,
    CompetitorId? FirstCompetitorId,
    CompetitorId? SecondCompetitorId,
    CompetitorId? ByeCompetitorId,
    RoundRobinMatchResult? Result = null)
{
    public bool IsBye => ByeCompetitorId.HasValue;
    public bool IsCompleted => Result is not null;
}

/// <summary>Supported general-purpose standings tie-break criteria.</summary>
public enum RoundRobinTieBreakCriterion
{
    Points,
    Wins,
    Draws,
    FewestLosses,
    CompetitorId
}

/// <summary>
/// Immutable scoring and ordering policy for round-robin standings.
/// It contains only result aggregates; activity-specific metrics must be supplied by a separate policy.
/// </summary>
public sealed class RoundRobinStandingsPolicy
{
    public static RoundRobinStandingsPolicy Default { get; } = new(
        winPoints: 3m,
        drawPoints: 1m,
        lossPoints: 0m,
        tieBreakers: new[]
        {
            RoundRobinTieBreakCriterion.Points,
            RoundRobinTieBreakCriterion.Wins,
            RoundRobinTieBreakCriterion.CompetitorId
        });

    public RoundRobinStandingsPolicy(
        decimal winPoints,
        decimal drawPoints,
        decimal lossPoints,
        IEnumerable<RoundRobinTieBreakCriterion> tieBreakers)
    {
        if (lossPoints < 0m || drawPoints < lossPoints || winPoints <= drawPoints)
            throw new ArgumentOutOfRangeException(nameof(winPoints),
                "Scoring must satisfy win points > draw points >= loss points >= 0.");

        ArgumentNullException.ThrowIfNull(tieBreakers);
        var snapshot = tieBreakers.ToArray();
        if (snapshot.Length == 0 || snapshot.Any(value => !Enum.IsDefined(value)))
            throw new ArgumentException("At least one valid tie-break criterion is required.", nameof(tieBreakers));
        if (snapshot.Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Tie-break criteria must be unique.", nameof(tieBreakers));
        if (snapshot[^1] != RoundRobinTieBreakCriterion.CompetitorId)
            throw new ArgumentException("CompetitorId must be the final tie-break criterion.", nameof(tieBreakers));

        WinPoints = winPoints;
        DrawPoints = drawPoints;
        LossPoints = lossPoints;
        TieBreakers = Array.AsReadOnly(snapshot);
    }

    public decimal WinPoints { get; }
    public decimal DrawPoints { get; }
    public decimal LossPoints { get; }
    public IReadOnlyList<RoundRobinTieBreakCriterion> TieBreakers { get; }
}

/// <summary>One competitor's derived standings row.</summary>
public sealed record RoundRobinStanding(
    CompetitorId CompetitorId,
    int MatchesPlayed,
    int Wins,
    int Draws,
    int Losses,
    int Byes,
    decimal Points);

/// <summary>
/// Immutable, deterministic round-robin schedule and result history.
/// Pairings use the circle method; odd entrant counts receive one automatic bye per round.
/// Standings use an immutable, caller-selected scoring and aggregate tie-break policy.
/// </summary>
/// <remarks>
/// The default is 3/1/0 points, then points descending, wins descending, and competitor Guid ascending.
/// Activity-specific metrics such as score differential or head-to-head are not inferred.
/// </remarks>
public sealed class RoundRobinSchedule
{
    private readonly IReadOnlyList<IReadOnlyList<RoundRobinMatch>> _rounds;

    public Guid CompetitionId { get; }
    public RoundRobinStandingsPolicy StandingsPolicy { get; }
    public IReadOnlyList<IReadOnlyList<RoundRobinMatch>> Rounds => _rounds;
    public IReadOnlyList<RoundRobinMatch> Matches => Array.AsReadOnly(_rounds.SelectMany(round => round).ToArray());

    public IReadOnlyList<RoundRobinStanding> Standings
    {
        get
        {
            var rows = _rounds.SelectMany(round => round)
                .SelectMany(match => match.Result is null
                    ? Array.Empty<(CompetitorId Id, int Played, int Wins, int Draws, int Losses, int Byes, decimal Points)>()
                    : GetResultRows(match))
                .GroupBy(row => row.Id)
                .ToDictionary(
                    group => group.Key,
                    group => new RoundRobinStanding(
                        group.Key,
                        group.Sum(row => row.Played),
                        group.Sum(row => row.Wins),
                        group.Sum(row => row.Draws),
                        group.Sum(row => row.Losses),
                        0,
                        group.Sum(row => row.Points)));

            foreach (var bye in _rounds.SelectMany(round => round).Where(match => match.IsBye))
            {
                var id = bye.ByeCompetitorId!.Value;
                if (!rows.TryGetValue(id, out var standing))
                    rows[id] = new RoundRobinStanding(id, 0, 0, 0, 0, 1, 0);
                else
                    rows[id] = standing with { Byes = standing.Byes + 1 };
            }

            // Entrants with no completed matches still appear in deterministic zeroed standings.
            foreach (var id in _entrantIds)
                rows.TryAdd(id, new RoundRobinStanding(id, 0, 0, 0, 0, 0, 0));

            return Array.AsReadOnly(OrderStandings(rows.Values).ToArray());
        }
    }

    private readonly IReadOnlyList<CompetitorId> _entrantIds;

    private RoundRobinSchedule(
        Guid competitionId,
        IReadOnlyList<CompetitorId> entrantIds,
        IEnumerable<IEnumerable<RoundRobinMatch>> rounds,
        RoundRobinStandingsPolicy standingsPolicy)
    {
        CompetitionId = competitionId;
        StandingsPolicy = standingsPolicy;
        _entrantIds = Array.AsReadOnly(entrantIds.ToArray());
        _rounds = Array.AsReadOnly(rounds
            .Select(round => (IReadOnlyList<RoundRobinMatch>)Array.AsReadOnly(round.ToArray()))
            .ToArray());
    }

    /// <summary>Creates a schedule using the default 3/1/0 standings policy.</summary>
    public static RoundRobinSchedule Create(Guid competitionId, IReadOnlyList<Competitor> entrants) =>
        Create(competitionId, entrants, RoundRobinStandingsPolicy.Default);

    /// <summary>Creates a schedule using the supplied immutable standings policy.</summary>
    public static RoundRobinSchedule Create(
        Guid competitionId,
        IReadOnlyList<Competitor> entrants,
        RoundRobinStandingsPolicy standingsPolicy)
    {
        if (competitionId == Guid.Empty)
            throw new ArgumentException("A competition identifier is required.", nameof(competitionId));
        ArgumentNullException.ThrowIfNull(entrants);
        ArgumentNullException.ThrowIfNull(standingsPolicy);
        if (entrants.Count < 2)
            throw new ArgumentException("A round-robin schedule requires at least two competitors.", nameof(entrants));
        if (entrants.Any(entrant => entrant is null))
            throw new ArgumentException("Entrants cannot contain null competitors.", nameof(entrants));
        if (entrants.Any(entrant => entrant.Id.Value == Guid.Empty))
            throw new ArgumentException("Every entrant must have a valid competitor identity.", nameof(entrants));
        if (entrants.Select(entrant => entrant.Id).Distinct().Count() != entrants.Count)
            throw new ArgumentException("A competitor can appear only once in a schedule.", nameof(entrants));

        var ids = entrants.Select(entrant => entrant.Id).ToList();
        if (ids.Count % 2 != 0)
            ids.Add(default);

        var roundCount = ids.Count - 1;
        var matchesPerRound = ids.Count / 2;
        var rounds = new List<List<RoundRobinMatch>>(roundCount);
        var rotating = ids.ToArray();

        for (var roundIndex = 0; roundIndex < roundCount; roundIndex++)
        {
            var round = new List<RoundRobinMatch>(matchesPerRound);
            for (var matchIndex = 0; matchIndex < matchesPerRound; matchIndex++)
            {
                var first = rotating[matchIndex];
                var second = rotating[rotating.Length - 1 - matchIndex];
                var firstIsBye = first.Value == Guid.Empty;
                var secondIsBye = second.Value == Guid.Empty;
                round.Add(new RoundRobinMatch(
                    new RoundRobinMatchId(competitionId, roundIndex + 1, matchIndex + 1),
                    firstIsBye || secondIsBye ? null : first,
                    firstIsBye || secondIsBye ? null : second,
                    firstIsBye ? second : secondIsBye ? first : null));
            }

            rounds.Add(round);
            var last = rotating[^1];
            for (var index = rotating.Length - 1; index > 1; index--)
                rotating[index] = rotating[index - 1];
            rotating[1] = last;
        }

        return new RoundRobinSchedule(competitionId, ids.Where(id => id.Value != Guid.Empty).ToArray(), rounds, standingsPolicy);
    }

    public RoundRobinSchedule RecordResult(RoundRobinMatchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.MatchId.CompetitionId != CompetitionId)
            throw new ArgumentException("The result belongs to a different competition.", nameof(result));

        var existing = Matches.Select(match => match.Result).FirstOrDefault(item => item?.Id == result.Id);
        if (existing is not null)
        {
            if (existing == result)
                return this;
            throw new InvalidOperationException("A result identifier cannot be reused for different result data.");
        }

        var match = FindMatch(result.MatchId);
        if (match.IsBye)
            throw new InvalidOperationException("An automatic bye cannot accept a played match result.");
        if (match.Result is not null)
            throw new InvalidOperationException("The match already has a result.");

        var updated = _rounds.Select(round => round.Select(item =>
            item.Id == result.MatchId ? item with { Result = result } : item).ToArray()).ToArray();
        return new RoundRobinSchedule(CompetitionId, _entrantIds, updated, StandingsPolicy);
    }

    public RoundRobinMatch FindMatch(RoundRobinMatchId matchId)
    {
        if (!matchId.IsValid || matchId.CompetitionId != CompetitionId)
            throw new ArgumentException("The match identity does not belong to this competition.", nameof(matchId));
        var roundIndex = matchId.RoundNumber - 1;
        var matchIndex = matchId.MatchNumber - 1;
        if (roundIndex >= _rounds.Count || matchIndex >= _rounds[roundIndex].Count)
            throw new KeyNotFoundException("The requested round-robin match does not exist.");
        return _rounds[roundIndex][matchIndex];
    }

    private IEnumerable<RoundRobinStanding> OrderStandings(IEnumerable<RoundRobinStanding> standings)
    {
        IOrderedEnumerable<RoundRobinStanding>? ordered = null;
        foreach (var criterion in StandingsPolicy.TieBreakers)
        {
            ordered = criterion switch
            {
                RoundRobinTieBreakCriterion.Points => ordered is null
                    ? standings.OrderByDescending(row => row.Points)
                    : ordered.ThenByDescending(row => row.Points),
                RoundRobinTieBreakCriterion.Wins => ordered is null
                    ? standings.OrderByDescending(row => row.Wins)
                    : ordered.ThenByDescending(row => row.Wins),
                RoundRobinTieBreakCriterion.Draws => ordered is null
                    ? standings.OrderByDescending(row => row.Draws)
                    : ordered.ThenByDescending(row => row.Draws),
                RoundRobinTieBreakCriterion.FewestLosses => ordered is null
                    ? standings.OrderBy(row => row.Losses)
                    : ordered.ThenBy(row => row.Losses),
                RoundRobinTieBreakCriterion.CompetitorId => ordered is null
                    ? standings.OrderBy(row => row.CompetitorId.Value)
                    : ordered.ThenBy(row => row.CompetitorId.Value),
                _ => throw new InvalidOperationException("The standings policy contains an unsupported tie-break criterion.")
            };
        }

        return ordered ?? throw new InvalidOperationException("The standings policy has no tie-break criteria.");
    }

    private IEnumerable<(CompetitorId Id, int Played, int Wins, int Draws, int Losses, int Byes, decimal Points)>
        GetResultRows(RoundRobinMatch match)
    {
        var first = match.FirstCompetitorId!.Value;
        var second = match.SecondCompetitorId!.Value;
        var result = match.Result!;
        var firstWins = result.Outcome == RoundRobinOutcome.FirstWins;
        var secondWins = result.Outcome == RoundRobinOutcome.SecondWins;
        var draw = result.Outcome == RoundRobinOutcome.Draw;
        yield return (first, 1, firstWins ? 1 : 0, draw ? 1 : 0, secondWins ? 1 : 0,
            0, firstWins ? StandingsPolicy.WinPoints : draw ? StandingsPolicy.DrawPoints : StandingsPolicy.LossPoints);
        yield return (second, 1, secondWins ? 1 : 0, draw ? 1 : 0, firstWins ? 1 : 0,
            0, secondWins ? StandingsPolicy.WinPoints : draw ? StandingsPolicy.DrawPoints : StandingsPolicy.LossPoints);
    }
}
