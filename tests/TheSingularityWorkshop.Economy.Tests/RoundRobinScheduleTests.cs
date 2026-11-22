using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class RoundRobinScheduleTests
{
    private static Competitor CompetitorNamed(string name) =>
        new(CompetitorId.New(), name, CompetitorKind.Individual);

    [Fact]
    public void EvenEntrantSchedulePairsEveryCompetitorExactlyOncePerRound()
    {
        var entrants = Enumerable.Range(1, 6).Select(i => CompetitorNamed($"Player {i}")).ToArray();
        var schedule = RoundRobinSchedule.Create(Guid.NewGuid(), entrants);

        Assert.Equal(5, schedule.Rounds.Count);
        Assert.All(schedule.Rounds, round =>
        {
            Assert.Equal(3, round.Count);
            Assert.All(round, match =>
            {
                Assert.False(match.IsBye);
                Assert.Equal(2, new[] { match.FirstCompetitorId, match.SecondCompetitorId }.Distinct().Count());
            });
            var participants = round.SelectMany(match => new[] { match.FirstCompetitorId, match.SecondCompetitorId });
            Assert.Equal(entrants.Length, participants.Distinct().Count());
        });

        var pairings = schedule.Matches.Select(match =>
            new[] { match.FirstCompetitorId!.Value, match.SecondCompetitorId!.Value }
                .OrderBy(id => id.Value).ToArray()).ToArray();
        Assert.Equal(entrants.Length * (entrants.Length - 1) / 2, pairings.Length);
        Assert.Equal(pairings.Length, pairings.Select(pair => $"{pair[0].Value}:{pair[1].Value}").Distinct().Count());
    }

    [Fact]
    public void OddEntrantScheduleAssignsExactlyOneByePerRoundAndOneByePerCompetitor()
    {
        var entrants = Enumerable.Range(1, 5).Select(i => CompetitorNamed($"Player {i}")).ToArray();
        var schedule = RoundRobinSchedule.Create(Guid.NewGuid(), entrants);

        Assert.Equal(5, schedule.Rounds.Count);
        Assert.All(schedule.Rounds, round =>
        {
            Assert.Equal(3, round.Count);
            Assert.Single(round, match => match.IsBye);
            Assert.Equal(2, round.Count(match => !match.IsBye));
        });
        Assert.All(entrants, entrant =>
            Assert.Single(schedule.Matches, match => match.ByeCompetitorId == entrant.Id));
    }

    [Fact]
    public void ResultsProduceDeterministicPointsStandingsAndAreIdempotent()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B"), CompetitorNamed("C") };
        var schedule = RoundRobinSchedule.Create(Guid.NewGuid(), entrants);
        var firstMatch = schedule.Rounds.SelectMany(round => round).First(match => !match.IsBye);
        var result = new RoundRobinMatchResult(
            RoundRobinResultId.New(), firstMatch.Id, RoundRobinOutcome.FirstWins,
            DateTimeOffset.Parse("2026-11-21T09:00:00Z"), "server://evidence/match-1");

        var updated = schedule.RecordResult(result);

        Assert.Same(updated, updated.RecordResult(result));
        Assert.Null(schedule.FindMatch(firstMatch.Id).Result);
        var winner = updated.Standings.Single(row => row.CompetitorId == firstMatch.FirstCompetitorId);
        var loser = updated.Standings.Single(row => row.CompetitorId == firstMatch.SecondCompetitorId);
        Assert.Equal((1, 1, 0, 0, 3m), (winner.MatchesPlayed, winner.Wins, winner.Draws, winner.Losses, winner.Points));
        Assert.Equal((1, 0, 0, 1, 0m), (loser.MatchesPlayed, loser.Wins, loser.Draws, loser.Losses, loser.Points));
        Assert.All(updated.Standings, row => Assert.Equal(1, row.Byes));
    }

    [Fact]
    public void DrawAwardsOnePointToEachCompetitor()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B") };
        var schedule = RoundRobinSchedule.Create(Guid.NewGuid(), entrants);
        var match = schedule.Matches.Single();
        var result = new RoundRobinMatchResult(
            RoundRobinResultId.New(), match.Id, RoundRobinOutcome.Draw,
            DateTimeOffset.UtcNow, "evidence://draw/1");

        var standings = schedule.RecordResult(result).Standings;

        Assert.All(standings, row =>
        {
            Assert.Equal(1, row.Draws);
            Assert.Equal(1, row.Points);
            Assert.Equal(1, row.MatchesPlayed);
        });
    }

    [Fact]
    public void ConflictingResultIdentityAndInvalidScheduleInputsAreRejected()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B") };
        var schedule = RoundRobinSchedule.Create(Guid.NewGuid(), entrants);
        var match = schedule.Matches.Single();
        var id = RoundRobinResultId.New();
        var result = new RoundRobinMatchResult(id, match.Id, RoundRobinOutcome.FirstWins,
            DateTimeOffset.UtcNow, "evidence://match/1");
        var completed = schedule.RecordResult(result);

        Assert.Throws<InvalidOperationException>(() => completed.RecordResult(
            new RoundRobinMatchResult(id, match.Id, RoundRobinOutcome.SecondWins,
                result.RecordedAt, "evidence://match/2")));
        Assert.Throws<ArgumentException>(() => RoundRobinSchedule.Create(Guid.NewGuid(), new[] { entrants[0], entrants[0] }));
        Assert.Throws<ArgumentException>(() => new RoundRobinMatchResult(
            RoundRobinResultId.New(), match.Id, (RoundRobinOutcome)999, DateTimeOffset.UtcNow, "evidence://invalid"));
    }
    [Fact]
    public void ByesAndAlreadyCompletedMatchesRejectAdditionalResults()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B"), CompetitorNamed("C") };
        var schedule = RoundRobinSchedule.Create(Guid.NewGuid(), entrants);
        var bye = schedule.Matches.Single(match => match.IsBye);
        var byeResult = new RoundRobinMatchResult(
            RoundRobinResultId.New(), bye.Id, RoundRobinOutcome.FirstWins,
            DateTimeOffset.UtcNow, "evidence://invalid-bye");
        Assert.Throws<InvalidOperationException>(() => schedule.RecordResult(byeResult));

        var played = schedule.Matches.Single(match => !match.IsBye);
        var firstResult = new RoundRobinMatchResult(
            RoundRobinResultId.New(), played.Id, RoundRobinOutcome.FirstWins,
            DateTimeOffset.UtcNow, "evidence://played/1");
        var completed = schedule.RecordResult(firstResult);
        var secondResult = new RoundRobinMatchResult(
            RoundRobinResultId.New(), played.Id, RoundRobinOutcome.SecondWins,
            DateTimeOffset.UtcNow, "evidence://played/2");
        Assert.Throws<InvalidOperationException>(() => completed.RecordResult(secondResult));
    }

    [Fact]
    public void CustomScoringAndTieBreakersAreAppliedAndPreservedAcrossSnapshots()
    {
        var entrants = new[]
        {
            CompetitorNamed("A"),
            CompetitorNamed("B"),
            CompetitorNamed("C"),
            CompetitorNamed("D")
        };
        var policy = new RoundRobinStandingsPolicy(
            winPoints: 3m,
            drawPoints: 1.5m,
            lossPoints: 0m,
            tieBreakers: new[]
            {
                RoundRobinTieBreakCriterion.FewestLosses,
                RoundRobinTieBreakCriterion.Points,
                RoundRobinTieBreakCriterion.CompetitorId
            });
        var schedule = RoundRobinSchedule.Create(Guid.NewGuid(), entrants, policy);

        schedule = Record(schedule, entrants[0].Id, entrants[1].Id, RoundRobinOutcome.FirstWins);
        schedule = Record(schedule, entrants[2].Id, entrants[0].Id, RoundRobinOutcome.FirstWins);
        schedule = Record(schedule, entrants[3].Id, entrants[0].Id, RoundRobinOutcome.FirstWins);
        schedule = Record(schedule, entrants[1].Id, entrants[2].Id, RoundRobinOutcome.Draw);
        schedule = Record(schedule, entrants[1].Id, entrants[3].Id, RoundRobinOutcome.Draw);
        schedule = Record(schedule, entrants[2].Id, entrants[3].Id, RoundRobinOutcome.Draw);

        var standings = schedule.Standings;
        var first = standings.Single(row => row.CompetitorId == entrants[0].Id);
        var second = standings.Single(row => row.CompetitorId == entrants[1].Id);

        Assert.Equal(3m, first.Points);
        Assert.Equal(3m, second.Points);
        Assert.Equal(2, first.Losses);
        Assert.Equal(1, second.Losses);
        var orderedRows = standings.ToList();
        Assert.True(orderedRows.IndexOf(second) < orderedRows.IndexOf(first));
        Assert.Same(policy, schedule.StandingsPolicy);
    }

    [Fact]
    public void StandingsPolicyRejectsInvalidScoringAndAmbiguousTieBreakers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RoundRobinStandingsPolicy(
            1m, 1m, 0m, new[] { RoundRobinTieBreakCriterion.CompetitorId }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RoundRobinStandingsPolicy(
            3m, 1m, -1m, new[] { RoundRobinTieBreakCriterion.CompetitorId }));
        Assert.Throws<ArgumentException>(() => new RoundRobinStandingsPolicy(
            3m, 1m, 0m, new[]
            {
                RoundRobinTieBreakCriterion.Points,
                RoundRobinTieBreakCriterion.Points,
                RoundRobinTieBreakCriterion.CompetitorId
            }));
        Assert.Throws<ArgumentException>(() => new RoundRobinStandingsPolicy(
            3m, 1m, 0m, new[]
            {
                RoundRobinTieBreakCriterion.CompetitorId,
                RoundRobinTieBreakCriterion.Points
            }));
    }

    private static RoundRobinSchedule Record(
        RoundRobinSchedule schedule,
        CompetitorId firstId,
        CompetitorId secondId,
        RoundRobinOutcome outcomeFromFirstId)
    {
        var match = schedule.Matches.Single(item =>
            (item.FirstCompetitorId == firstId && item.SecondCompetitorId == secondId) ||
            (item.FirstCompetitorId == secondId && item.SecondCompetitorId == firstId));
        var outcome = match.FirstCompetitorId == firstId
            ? outcomeFromFirstId
            : outcomeFromFirstId switch
            {
                RoundRobinOutcome.FirstWins => RoundRobinOutcome.SecondWins,
                RoundRobinOutcome.SecondWins => RoundRobinOutcome.FirstWins,
                _ => RoundRobinOutcome.Draw
            };
        return schedule.RecordResult(new RoundRobinMatchResult(
            RoundRobinResultId.New(),
            match.Id,
            outcome,
            DateTimeOffset.UnixEpoch,
            $"evidence://{match.Id.RoundNumber}/{match.Id.MatchNumber}"));
    }

}
