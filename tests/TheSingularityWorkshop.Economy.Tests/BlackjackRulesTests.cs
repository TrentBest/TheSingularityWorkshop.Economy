using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class BlackjackRulesTests
{
    private static readonly SimulatedCurrencyId Currency = SimulatedCurrencyId.New();
    private static readonly SimulatedAmount Stake = new(10m, Currency);

    private static BlackjackCard Card(BlackjackRank rank, BlackjackSuit suit = BlackjackSuit.Clubs) =>
        new(rank, suit);

    [Fact]
    public void AceIsElevenWhenSafeAndOneWhenNeeded()
    {
        var soft = BlackjackRules.EvaluateHand(new[]
        {
            Card(BlackjackRank.Ace), Card(BlackjackRank.Six)
        });
        var adjusted = BlackjackRules.EvaluateHand(new[]
        {
            Card(BlackjackRank.Ace), Card(BlackjackRank.Six), Card(BlackjackRank.Nine)
        });

        Assert.Equal(17, soft.Total);
        Assert.True(soft.IsSoft);
        Assert.False(soft.IsBust);
        Assert.Equal(16, adjusted.Total);
        Assert.False(adjusted.IsSoft);
    }

    [Fact]
    public void MultipleAcesAreAdjustedWithoutBustWhenPossible()
    {
        var hand = BlackjackRules.EvaluateHand(new[]
        {
            Card(BlackjackRank.Ace, BlackjackSuit.Clubs),
            Card(BlackjackRank.Ace, BlackjackSuit.Diamonds),
            Card(BlackjackRank.Nine)
        });

        Assert.Equal(21, hand.Total);
        Assert.True(hand.IsSoft);
        Assert.False(hand.IsNatural);
    }

    [Fact]
    public void TwoCardTwentyOneIsNaturalButThreeCardTwentyOneIsNot()
    {
        var natural = BlackjackRules.EvaluateHand(new[]
        {
            Card(BlackjackRank.Ace), Card(BlackjackRank.King)
        });
        var ordinary = BlackjackRules.EvaluateHand(new[]
        {
            Card(BlackjackRank.Seven), Card(BlackjackRank.Seven, BlackjackSuit.Diamonds),
            Card(BlackjackRank.Seven, BlackjackSuit.Hearts)
        });

        Assert.True(natural.IsNatural);
        Assert.True(ordinary.Total == 21);
        Assert.False(ordinary.IsNatural);
    }

    [Fact]
    public void PlayerBustAlwaysLosesAndPaysNothing()
    {
        var decision = BlackjackRules.Resolve(
            new[] { Card(BlackjackRank.King), Card(BlackjackRank.Seven, BlackjackSuit.Diamonds),
                Card(BlackjackRank.Six, BlackjackSuit.Hearts) },
            new[] { Card(BlackjackRank.Ten), Card(BlackjackRank.Eight, BlackjackSuit.Diamonds) },
            Stake);

        Assert.Equal(CasinoWagerOutcome.Loss, decision.Outcome);
        Assert.Equal(0m, decision.GrossPayout.Value);
        Assert.True(decision.Player.IsBust);
    }

    [Fact]
    public void NaturalBeatsOrdinaryTwentyOneAndUsesDeclaredEvenMoneyPayout()
    {
        var decision = BlackjackRules.Resolve(
            new[] { Card(BlackjackRank.Ace), Card(BlackjackRank.King, BlackjackSuit.Diamonds) },
            new[] { Card(BlackjackRank.Seven), Card(BlackjackRank.Seven, BlackjackSuit.Diamonds),
                Card(BlackjackRank.Seven, BlackjackSuit.Hearts) },
            Stake);

        Assert.Equal(CasinoWagerOutcome.Win, decision.Outcome);
        Assert.Equal(20m, decision.GrossPayout.Value);
        Assert.Equal(Currency, decision.GrossPayout.CurrencyId);
    }

    [Fact]
    public void EqualHandsPushAndDealerNaturalBeatsOrdinaryTwentyOne()
    {
        var push = BlackjackRules.Resolve(
            new[] { Card(BlackjackRank.Ten), Card(BlackjackRank.Seven) },
            new[] { Card(BlackjackRank.Nine), Card(BlackjackRank.Eight, BlackjackSuit.Diamonds) },
            Stake);
        var naturalLoss = BlackjackRules.Resolve(
            new[] { Card(BlackjackRank.Seven), Card(BlackjackRank.Seven, BlackjackSuit.Diamonds),
                Card(BlackjackRank.Seven, BlackjackSuit.Hearts) },
            new[] { Card(BlackjackRank.Ace), Card(BlackjackRank.King, BlackjackSuit.Diamonds) },
            Stake);

        Assert.Equal(CasinoWagerOutcome.Push, push.Outcome);
        Assert.Equal(10m, push.GrossPayout.Value);
        Assert.Equal(CasinoWagerOutcome.Loss, naturalLoss.Outcome);
    }

    [Fact]
    public void DuplicatePhysicalCardsAndIncompleteHandsAreRejected()
    {
        var duplicate = Card(BlackjackRank.Ace);
        Assert.Throws<ArgumentException>(() => BlackjackRules.EvaluateHand(new[] { duplicate, duplicate }));
        Assert.Throws<ArgumentException>(() => BlackjackRules.Resolve(
            new[] { Card(BlackjackRank.Ace) },
            new[] { Card(BlackjackRank.King), Card(BlackjackRank.Seven, BlackjackSuit.Diamonds) },
            Stake));
    }

    [Fact]
    public void DealerBustWinsUnlessPlayerHasAlreadyBust()
    {
        var decision = BlackjackRules.Resolve(
            new[] { Card(BlackjackRank.Ten), Card(BlackjackRank.Nine, BlackjackSuit.Diamonds) },
            new[] { Card(BlackjackRank.King), Card(BlackjackRank.Six, BlackjackSuit.Diamonds),
                Card(BlackjackRank.Seven, BlackjackSuit.Hearts) },
            Stake);

        Assert.Equal(CasinoWagerOutcome.Win, decision.Outcome);
        Assert.True(decision.Dealer.IsBust);
    }
}
