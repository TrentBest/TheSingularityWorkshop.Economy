namespace TheSingularityWorkshop.Economy;

/// <summary>Stable composite identity for a match within a tournament bracket.</summary>
public readonly record struct TournamentMatchId(Guid TournamentId, int RoundNumber, int MatchNumber)
{
    public bool IsValid => TournamentId != Guid.Empty && RoundNumber > 0 && MatchNumber > 0;
}

/// <summary>Lifecycle state of one bracket slot pairing.</summary>
public enum TournamentMatchStatus
{
    Waiting,
    Ready,
    Bye,
    Completed
}

/// <summary>Immutable pairing and result snapshot for one single-elimination match.</summary>
public sealed record TournamentMatch(
    TournamentMatchId Id,
    CompetitorId? FirstCompetitorId,
    CompetitorId? SecondCompetitorId,
    TournamentMatchStatus Status,
    CompetitorId? WinnerId);

/// <summary>
/// Deterministic single-elimination bracket with explicit seeding, byes, and winner advancement.
/// The bracket is immutable: recording a result returns a new snapshot.
/// </summary>
/// <remarks>
/// The bracket manages competitor progression only. It does not schedule matches, adjudicate
/// activity rules, collect entry fees, award prizes, or persist state. Callers must validate
/// results under the relevant sport/game rules before recording a winner.
/// </remarks>
public sealed class SingleEliminationBracket
{
    private readonly IReadOnlyList<IReadOnlyList<TournamentMatch>> _rounds;

    private SingleEliminationBracket(Guid tournamentId, IEnumerable<IEnumerable<TournamentMatch>> rounds)
    {
        TournamentId = tournamentId;
        _rounds = Array.AsReadOnly(rounds
            .Select(round => (IReadOnlyList<TournamentMatch>)Array.AsReadOnly(round.ToArray()))
            .ToArray());
    }

    public Guid TournamentId { get; }
    public IReadOnlyList<IReadOnlyList<TournamentMatch>> Rounds => _rounds;
    public IReadOnlyList<TournamentMatch> FirstRound => _rounds[0];
    public CompetitorId? Champion => _rounds[^1][0].WinnerId;

    /// <summary>Creates a bracket. Seed order is strongest/highest seed first; omitted order uses entrant order.</summary>
    public static SingleEliminationBracket Create(
        Guid tournamentId,
        IReadOnlyList<Competitor> entrants,
        IReadOnlyList<CompetitorId>? seedOrder = null)
    {
        if (tournamentId == Guid.Empty)
            throw new ArgumentException("A tournament identifier is required.", nameof(tournamentId));
        ArgumentNullException.ThrowIfNull(entrants);
        if (entrants.Count < 2)
            throw new ArgumentException("A single-elimination bracket requires at least two competitors.", nameof(entrants));
        if (entrants.Any(entrant => entrant is null))
            throw new ArgumentException("Entrants cannot contain null competitors.", nameof(entrants));
        if (entrants.Any(entrant => entrant.Id.Value == Guid.Empty))
            throw new ArgumentException("Every entrant must have a valid competitor identity.", nameof(entrants));
        if (entrants.Select(entrant => entrant.Id).Distinct().Count() != entrants.Count)
            throw new ArgumentException("A competitor can appear only once in a bracket.", nameof(entrants));

        var ids = seedOrder?.ToArray() ?? entrants.Select(entrant => entrant.Id).ToArray();
        if (ids.Length != entrants.Count ||
            ids.Distinct().Count() != ids.Length ||
            !ids.ToHashSet().SetEquals(entrants.Select(entrant => entrant.Id)))
            throw new ArgumentException("Seed order must contain every entrant exactly once.", nameof(seedOrder));

        var bracketSize = 1;
        while (bracketSize < ids.Length)
            bracketSize = checked(bracketSize * 2);

        var seedPositions = GetSeedPositions(bracketSize);
        var firstRoundSlots = seedPositions
            .Select(seed => seed <= ids.Length ? ids[seed - 1] : (CompetitorId?)null)
            .ToArray();

        var roundCount = 1;
        for (var size = bracketSize; size > 2; size /= 2)
            roundCount++;

        var rounds = new List<List<TournamentMatch>>();
        var matchCount = bracketSize / 2;
        for (var roundIndex = 0; roundIndex < roundCount; roundIndex++)
        {
            var roundNumber = roundIndex + 1;
            var matches = new List<TournamentMatch>();
            for (var matchIndex = 0; matchIndex < matchCount; matchIndex++)
            {
                CompetitorId? first = null;
                CompetitorId? second = null;
                var status = TournamentMatchStatus.Waiting;
                CompetitorId? winner = null;
                if (roundIndex == 0)
                {
                    first = firstRoundSlots[matchIndex * 2];
                    second = firstRoundSlots[matchIndex * 2 + 1];
                    if (first.HasValue && second.HasValue)
                        status = TournamentMatchStatus.Ready;
                    else if (first.HasValue || second.HasValue)
                    {
                        status = TournamentMatchStatus.Bye;
                        winner = first ?? second;
                    }
                }

                matches.Add(new TournamentMatch(
                    new TournamentMatchId(tournamentId, roundNumber, matchIndex + 1),
                    first, second, status, winner));
            }
            rounds.Add(matches);
            matchCount /= 2;
        }

        // A first-round bye is an automatic advancement, not a played match.
        for (var matchIndex = 0; matchIndex < rounds[0].Count; matchIndex++)
        {
            var match = rounds[0][matchIndex];
            if (match.Status == TournamentMatchStatus.Bye)
                Advance(rounds, match, matchIndex);
        }

        return new SingleEliminationBracket(tournamentId, rounds);
    }

    /// <summary>Records a validated winner and advances that competitor into the next round.</summary>
    public SingleEliminationBracket RecordWinner(TournamentMatchId matchId, CompetitorId winnerId)
    {
        if (!matchId.IsValid || matchId.TournamentId != TournamentId)
            throw new ArgumentException("The match identity does not belong to this tournament.", nameof(matchId));
        if (winnerId.Value == Guid.Empty)
            throw new ArgumentException("A winner identity is required.", nameof(winnerId));

        var rounds = _rounds.Select(round => round.ToList()).ToList();
        var roundIndex = matchId.RoundNumber - 1;
        var matchIndex = matchId.MatchNumber - 1;
        if (roundIndex < 0 || roundIndex >= rounds.Count ||
            matchIndex < 0 || matchIndex >= rounds[roundIndex].Count)
            throw new KeyNotFoundException("The requested tournament match does not exist.");

        var match = rounds[roundIndex][matchIndex];
        if (match.Status != TournamentMatchStatus.Ready)
            throw new InvalidOperationException("Only a ready match with two competitors can accept a result.");
        if (winnerId != match.FirstCompetitorId && winnerId != match.SecondCompetitorId)
            throw new ArgumentException("The winner must be one of the two competitors in this match.", nameof(winnerId));

        var completed = match with { Status = TournamentMatchStatus.Completed, WinnerId = winnerId };
        rounds[roundIndex][matchIndex] = completed;
        if (roundIndex < rounds.Count - 1)
            Advance(rounds, completed, matchIndex);

        return new SingleEliminationBracket(TournamentId, rounds);
    }

    private static void Advance(List<List<TournamentMatch>> rounds, TournamentMatch completed, int sourceMatchIndex)
    {
        var sourceRound = completed.Id.RoundNumber - 1;
        if (sourceRound >= rounds.Count - 1)
            return;

        var targetMatchIndex = sourceMatchIndex / 2;
        var target = rounds[sourceRound + 1][targetMatchIndex];
        var firstSlot = sourceMatchIndex % 2 == 0;
        if (firstSlot && target.FirstCompetitorId.HasValue ||
            !firstSlot && target.SecondCompetitorId.HasValue)
            throw new InvalidOperationException("A bracket slot cannot receive more than one advancing competitor.");

        var updated = firstSlot
            ? target with { FirstCompetitorId = completed.WinnerId }
            : target with { SecondCompetitorId = completed.WinnerId };
        if (updated.FirstCompetitorId.HasValue && updated.SecondCompetitorId.HasValue)
            updated = updated with { Status = TournamentMatchStatus.Ready };
        rounds[sourceRound + 1][targetMatchIndex] = updated;
    }

    private static int[] GetSeedPositions(int bracketSize)
    {
        var positions = new[] { 1, 2 };
        while (positions.Length < bracketSize)
        {
            var nextSize = positions.Length * 2;
            positions = positions.SelectMany(seed => new[] { seed, nextSize + 1 - seed }).ToArray();
        }
        return positions;
    }
}
