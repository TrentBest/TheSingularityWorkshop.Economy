using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class BlackjackRoundTests
{
    private static readonly SimulatedCurrencyId Currency = SimulatedCurrencyId.New();
    private static readonly SimulatedAmount Stake = new(10m, Currency);
    private static BlackjackCard C(BlackjackRank rank, BlackjackSuit suit) => new(rank, suit);

    [Fact]
    public void DealsInDocumentedPlayerDealerPlayerDealerOrder()
    {
        var shoe = new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Ten, BlackjackSuit.Spades),
            C(BlackjackRank.Nine, BlackjackSuit.Diamonds),
            C(BlackjackRank.Six, BlackjackSuit.Diamonds),
            C(BlackjackRank.Two, BlackjackSuit.Hearts)
        });

        var round = BlackjackRound.Start(shoe, Stake);

        Assert.Equal(new[] { C(BlackjackRank.Ten, BlackjackSuit.Clubs), C(BlackjackRank.Nine, BlackjackSuit.Diamonds) }, round.PlayerCards);
        Assert.Equal(new[] { C(BlackjackRank.Ten, BlackjackSuit.Spades), C(BlackjackRank.Six, BlackjackSuit.Diamonds) }, round.DealerCards);
        Assert.Equal(1, shoe.Remaining);
        Assert.Equal(BlackjackRoundState.PlayerTurn, round.State);
    }

    [Fact]
    public void HitDrawsExactlyOneCardAndBustCompletesAsLoss()
    {
        var shoe = new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs), C(BlackjackRank.Five, BlackjackSuit.Clubs),
            C(BlackjackRank.Six, BlackjackSuit.Diamonds), C(BlackjackRank.Ten, BlackjackSuit.Diamonds),
            C(BlackjackRank.Eight, BlackjackSuit.Hearts)
        });
        var round = BlackjackRound.Start(shoe, Stake);

        round.Hit();

        Assert.Equal(BlackjackRoundState.Completed, round.State);
        Assert.Equal(CasinoWagerOutcome.Loss, round.Decision!.Outcome);
        Assert.Equal(0m, round.Decision.GrossPayout.Value);
        Assert.Throws<InvalidOperationException>(() => round.Hit());
    }

    [Fact]
    public void StandRunsDealerUntilHardSeventeenOrHigher()
    {
        var shoe = new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs), C(BlackjackRank.Ten, BlackjackSuit.Diamonds),
            C(BlackjackRank.Seven, BlackjackSuit.Clubs), C(BlackjackRank.Six, BlackjackSuit.Diamonds),
            C(BlackjackRank.Two, BlackjackSuit.Hearts)
        });
        var round = BlackjackRound.Start(shoe, Stake);

        round.Stand();

        Assert.Equal(BlackjackRoundState.Completed, round.State);
        Assert.Equal(18, round.Decision!.Dealer.Total);
        Assert.Equal(CasinoWagerOutcome.Win, round.Decision.Outcome);
    }

    [Fact]
    public void DealerHitsSoftSeventeenUnderH17Policy()
    {
        var shoe = new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ace, BlackjackSuit.Clubs), C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Six, BlackjackSuit.Diamonds), C(BlackjackRank.Six, BlackjackSuit.Hearts),
            C(BlackjackRank.Ace, BlackjackSuit.Spades), C(BlackjackRank.Two, BlackjackSuit.Spades)
        });
        var round = BlackjackRound.Start(shoe, Stake);

        round.Stand();

        Assert.Equal(BlackjackRoundState.Completed, round.State);
        Assert.Equal(19, round.Decision!.Dealer.Total);
        Assert.Equal(4, round.DealerCards.Count);
    }

    [Fact]
    public void NaturalCompletesImmediatelyAndShoeRequiresUniqueStandardCards()
    {
        var shoe = new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ace, BlackjackSuit.Clubs), C(BlackjackRank.Ten, BlackjackSuit.Diamonds),
            C(BlackjackRank.King, BlackjackSuit.Hearts), C(BlackjackRank.Seven, BlackjackSuit.Spades)
        });
        var round = BlackjackRound.Start(shoe, Stake);

        Assert.Equal(BlackjackRoundState.Completed, round.State);
        Assert.Equal(CasinoWagerOutcome.Win, round.Decision!.Outcome);
        Assert.Throws<InvalidOperationException>(() => round.Stand());
        Assert.Throws<ArgumentException>(() => new BlackjackShoe(new[]
        {
            C(BlackjackRank.Two, BlackjackSuit.Clubs), C(BlackjackRank.Two, BlackjackSuit.Clubs)
        }));
        Assert.Throws<ArgumentException>(() => new BlackjackShoe(new[]
        {
            new BlackjackCard((BlackjackRank)99, BlackjackSuit.Clubs)
        }));
    }

    [Fact]
    public void InsufficientInitialShoeDoesNotStartRound()
    {
        var shoe = new BlackjackShoe(new[]
        {
            C(BlackjackRank.Two, BlackjackSuit.Clubs),
            C(BlackjackRank.Three, BlackjackSuit.Clubs),
            C(BlackjackRank.Four, BlackjackSuit.Clubs)
        });

        Assert.Throws<InvalidOperationException>(() => BlackjackRound.Start(shoe, Stake));
        Assert.Equal(3, shoe.Remaining);
    }
}
