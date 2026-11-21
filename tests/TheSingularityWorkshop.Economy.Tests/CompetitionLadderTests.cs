using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class CompetitionLadderTests
{
    private static Competitor CompetitorNamed(string name) =>
        new(CompetitorId.New(), name, CompetitorKind.Individual);

    [Fact]
    public void NewLadderStartsEveryEntrantAtConfiguredRatingAndRanksDeterministically()
    {
        var first = CompetitorNamed("First");
        var second = CompetitorNamed("Second");
        var ladder = CompetitionLadder.Create(LadderId.New(), new[] { second, first }, 1200m);

        Assert.All(ladder.Standings, standing => Assert.Equal(1200m, standing.Rating));
        Assert.Equal(
            new[] { first.Id, second.Id }.OrderBy(id => id.Value),
            ladder.Standings.Select(standing => standing.CompetitorId));
    }

    [Fact]
    public void WinUpdatesBothRatingsAndCountersWithoutMutatingPriorSnapshot()
    {
        var first = CompetitorNamed("First");
        var second = CompetitorNamed("Second");
        var original = CompetitionLadder.Create(LadderId.New(), new[] { first, second });
        var result = new LadderResult(LadderResultId.New(), first.Id, second.Id,
            CompetitionOutcome.FirstWins, DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "match/evidence/1");

        var updated = original.RecordResult(result);
        var firstStanding = updated.Standings.Single(item => item.CompetitorId == first.Id);
        var secondStanding = updated.Standings.Single(item => item.CompetitorId == second.Id);

        Assert.Equal(1016m, firstStanding.Rating);
        Assert.Equal(984m, secondStanding.Rating);
        Assert.Equal(1, firstStanding.Wins);
        Assert.Equal(1, secondStanding.Losses);
        Assert.Empty(original.Results);
        Assert.Equal(1000m, original.Standings[0].Rating);
    }

    [Fact]
    public void DrawPreservesEqualRatingsAndCountsForBothCompetitors()
    {
        var first = CompetitorNamed("First");
        var second = CompetitorNamed("Second");
        var ladder = CompetitionLadder.Create(LadderId.New(), new[] { first, second });
        var result = new LadderResult(LadderResultId.New(), first.Id, second.Id,
            CompetitionOutcome.Draw, DateTimeOffset.UtcNow, "match/evidence/draw");

        var updated = ladder.RecordResult(result);

        Assert.All(updated.Standings, standing =>
        {
            Assert.Equal(1000m, standing.Rating);
            Assert.Equal(1, standing.Draws);
            Assert.Equal(1, standing.MatchesPlayed);
        });
    }

    [Fact]
    public void DuplicateResultIsIdempotentButConflictingReuseIsRejected()
    {
        var first = CompetitorNamed("First");
        var second = CompetitorNamed("Second");
        var ladder = CompetitionLadder.Create(LadderId.New(), new[] { first, second });
        var id = LadderResultId.New();
        var timestamp = DateTimeOffset.UtcNow;
        var result = new LadderResult(id, first.Id, second.Id,
            CompetitionOutcome.FirstWins, timestamp, "match/evidence/1");

        var updated = ladder.RecordResult(result);

        Assert.Same(updated, updated.RecordResult(result));
        Assert.Throws<InvalidOperationException>(() => updated.RecordResult(
            new LadderResult(id, first.Id, second.Id, CompetitionOutcome.SecondWins,
                timestamp, "match/evidence/corrected")));
    }

    [Fact]
    public void LadderRejectsUnknownCompetitorsAndOutOfOrderResults()
    {
        var first = CompetitorNamed("First");
        var second = CompetitorNamed("Second");
        var third = CompetitorNamed("Third");
        var ladder = CompetitionLadder.Create(LadderId.New(), new[] { first, second });
        var later = new LadderResult(LadderResultId.New(), first.Id, second.Id,
            CompetitionOutcome.FirstWins, DateTimeOffset.Parse("2026-01-02T00:00:00Z"), "evidence/later");
        var updated = ladder.RecordResult(later);

        Assert.Throws<ArgumentException>(() => ladder.RecordResult(new LadderResult(
            LadderResultId.New(), first.Id, third.Id, CompetitionOutcome.FirstWins,
            DateTimeOffset.Parse("2026-01-03T00:00:00Z"), "evidence/unknown")));
        Assert.Throws<InvalidOperationException>(() => updated.RecordResult(new LadderResult(
            LadderResultId.New(), first.Id, second.Id, CompetitionOutcome.SecondWins,
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "evidence/earlier")));
    }

    [Fact]
    public void LadderRejectsInvalidEntrantsAndMissingEvidenceReference()
    {
        var first = CompetitorNamed("First");
        Assert.Throws<ArgumentException>(() => CompetitionLadder.Create(LadderId.New(), new[] { first, first }));
        Assert.Throws<ArgumentException>(() => new LadderResult(LadderResultId.New(), first.Id,
            CompetitorId.New(), CompetitionOutcome.FirstWins, DateTimeOffset.UtcNow, " "));
    }
}
