using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class TournamentBracketTests
{
    private static Competitor CompetitorNamed(string name) =>
        new(CompetitorId.New(), name, CompetitorKind.Individual);

    [Fact]
    public void FourSeedBracketAdvancesWinnersDeterministicallyToChampion()
    {
        var entrants = new[]
        {
            CompetitorNamed("Seed 1"), CompetitorNamed("Seed 2"),
            CompetitorNamed("Seed 3"), CompetitorNamed("Seed 4")
        };
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants);
        var firstRound = bracket.FirstRound;

        Assert.Equal(2, bracket.Rounds.Count);
        Assert.Equal(2, firstRound.Count);
        Assert.Equal(entrants[0].Id, firstRound[0].FirstCompetitorId);
        Assert.Equal(entrants[3].Id, firstRound[0].SecondCompetitorId);
        Assert.Equal(entrants[1].Id, firstRound[1].FirstCompetitorId);
        Assert.Equal(entrants[2].Id, firstRound[1].SecondCompetitorId);

        var afterFirst = bracket
            .RecordWinner(firstRound[0].Id, entrants[0].Id)
            .RecordWinner(firstRound[1].Id, entrants[1].Id);
        var final = afterFirst.Rounds[1][0];

        Assert.Equal(TournamentMatchStatus.Ready, final.Status);
        Assert.Equal(entrants[0].Id, final.FirstCompetitorId);
        Assert.Equal(entrants[1].Id, final.SecondCompetitorId);
        Assert.Null(bracket.Champion);

        var completed = afterFirst.RecordWinner(final.Id, entrants[1].Id);
        Assert.Equal(entrants[1].Id, completed.Champion);
        Assert.Equal(TournamentMatchStatus.Completed, completed.Rounds[1][0].Status);
    }

    [Fact]
    public void FiveEntrantsReceiveThreeAutomaticByesAndCorrectProgression()
    {
        var entrants = Enumerable.Range(1, 5).Select(i => CompetitorNamed($"Seed {i}")).ToArray();
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants);

        Assert.Equal(4, bracket.FirstRound.Count);
        Assert.Equal(3, bracket.FirstRound.Count(match => match.Status == TournamentMatchStatus.Bye));
        Assert.Equal(1, bracket.FirstRound.Count(match => match.Status == TournamentMatchStatus.Ready));
        Assert.Equal(entrants[0].Id, bracket.Rounds[1][0].FirstCompetitorId);
        Assert.Equal(TournamentMatchStatus.Waiting, bracket.Rounds[1][0].Status);

        var played = bracket.FirstRound.Single(match => match.Status == TournamentMatchStatus.Ready);
        var progressed = bracket.RecordWinner(played.Id, played.FirstCompetitorId!.Value);
        Assert.Equal(TournamentMatchStatus.Ready, progressed.Rounds[1][0].Status);
    }

    [Fact]
    public void ExplicitSeedOrderControlsPairingsAndMustContainEveryEntrantOnce()
    {
        var entrants = Enumerable.Range(1, 4).Select(i => CompetitorNamed($"Player {i}")).ToArray();
        var order = new[] { entrants[2].Id, entrants[0].Id, entrants[3].Id, entrants[1].Id };
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants, order);

        Assert.Equal(entrants[2].Id, bracket.FirstRound[0].FirstCompetitorId);
        Assert.Equal(entrants[1].Id, bracket.FirstRound[0].SecondCompetitorId);
        Assert.Throws<ArgumentException>(() => SingleEliminationBracket.Create(
            Guid.NewGuid(), entrants, new[] { entrants[0].Id, entrants[0].Id, entrants[2].Id, entrants[3].Id }));
    }

    [Fact]
    public void BracketRejectsUnknownWinnerWrongMatchAndPrematureResults()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B"), CompetitorNamed("C"), CompetitorNamed("D") };
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants);
        var match = bracket.FirstRound[0];

        Assert.Throws<ArgumentException>(() => bracket.RecordWinner(match.Id, CompetitorId.New()));
        Assert.Throws<ArgumentException>(() => bracket.RecordWinner(
            new TournamentMatchId(Guid.NewGuid(), 1, 1), entrants[0].Id));
        Assert.Throws<InvalidOperationException>(() => bracket.RecordWinner(
            new TournamentMatchId(bracket.TournamentId, 2, 1), entrants[0].Id));

        var completed = bracket.RecordWinner(match.Id, entrants[0].Id);
        Assert.Throws<InvalidOperationException>(() => completed.RecordWinner(match.Id, entrants[0].Id));
    }

    [Fact]
    public void ByeCannotBeManuallyResolvedAndBracketSnapshotsRemainImmutable()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B"), CompetitorNamed("C") };
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants);
        var bye = bracket.FirstRound.Single(match => match.Status == TournamentMatchStatus.Bye);

        Assert.Throws<InvalidOperationException>(() => bracket.RecordWinner(bye.Id, bye.WinnerId!.Value));
        Assert.Equal(TournamentMatchStatus.Ready,
            bracket.RecordWinner(bracket.FirstRound.Single(m => m.Status == TournamentMatchStatus.Ready).Id,
                bracket.FirstRound.Single(m => m.Status == TournamentMatchStatus.Ready).FirstCompetitorId!.Value)
                .Rounds[1][0].Status);
        Assert.Equal(TournamentMatchStatus.Waiting, bracket.Rounds[1][0].Status);
    }

    [Fact]
    public void ResultRecordsIdentityAndEvidenceAndIsIdempotent()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B") };
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants);
        var match = bracket.FirstRound[0];
        var result = new TournamentMatchResult(
            TournamentResultId.New(),
            match.Id,
            entrants[0].Id,
            DateTimeOffset.Parse("2026-11-21T09:00:00Z"),
            "server-sc2://matches/match-42/replay");

        var completed = bracket.RecordResult(result);

        Assert.Equal(TournamentMatchStatus.Completed, completed.FirstRound[0].Status);
        Assert.Equal(result, completed.FirstRound[0].Result);
        Assert.Equal(entrants[0].Id, completed.Champion);
        Assert.Same(completed, completed.RecordResult(result));
        Assert.Null(bracket.FirstRound[0].Result);
        Assert.Equal(TournamentMatchStatus.Ready, bracket.FirstRound[0].Status);
    }

    [Fact]
    public void ResultRejectsConflictingIdentityAndInvalidProvenance()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B") };
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants);
        var match = bracket.FirstRound[0];
        var resultId = TournamentResultId.New();
        var result = new TournamentMatchResult(
            resultId, match.Id, entrants[0].Id, DateTimeOffset.UtcNow, "evidence://match/1");

        var completed = bracket.RecordResult(result);
        var conflicting = new TournamentMatchResult(
            resultId, match.Id, entrants[1].Id, result.RecordedAt, "evidence://match/2");

        Assert.Throws<InvalidOperationException>(() => completed.RecordResult(conflicting));
        Assert.Throws<ArgumentException>(() => new TournamentMatchResult(
            TournamentResultId.New(), match.Id, entrants[0].Id, DateTimeOffset.UtcNow, " "));
        Assert.Throws<ArgumentException>(() => bracket.RecordResult(new TournamentMatchResult(
            TournamentResultId.New(), match.Id, CompetitorId.New(), DateTimeOffset.UtcNow, "evidence://match/3")));
    }

    [Fact]
    public void ResultCannotBeAppliedToAnotherTournamentOrMatch()
    {
        var entrants = new[] { CompetitorNamed("A"), CompetitorNamed("B") };
        var bracket = SingleEliminationBracket.Create(Guid.NewGuid(), entrants);
        var otherTournamentMatch = new TournamentMatchId(Guid.NewGuid(), 1, 1);
        var result = new TournamentMatchResult(
            TournamentResultId.New(), otherTournamentMatch, entrants[0].Id,
            DateTimeOffset.UtcNow, "evidence://match/4");

        Assert.Throws<ArgumentException>(() => bracket.RecordResult(result));
    }
}
