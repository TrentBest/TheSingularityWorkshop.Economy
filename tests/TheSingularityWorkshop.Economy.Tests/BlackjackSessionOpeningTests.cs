using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class BlackjackSessionOpeningTests
{
    private static readonly DateTimeOffset Time =
        new(2026, 11, 3, 6, 0, 0, TimeSpan.Zero);

    private static BlackjackCard C(BlackjackRank rank, BlackjackSuit suit) => new(rank, suit);

    private sealed record Setup(
        SimulatedCurrency Currency,
        SimulationLedger Ledger,
        SimulationAccount Chips,
        SimulationAccount House,
        SimulationAccount Escrow,
        SimulationAccount PayoutsOwed,
        CasinoWagerBook Wagers);

    private static Setup Create(decimal chipBalance = 50m)
    {
        var currency = new SimulatedCurrency(
            SimulatedCurrencyId.New(), "Singularity Denomination", "SD", 2);
        SimulationAccount Account() => new(Guid.NewGuid(), EconomyEntityId.New(), currency);
        var chips = Account();
        var house = Account();
        var escrow = Account();
        var payouts = Account();
        var ledger = new SimulationLedger();
        foreach (var account in new[] { chips, house, escrow, payouts })
            ledger.RegisterAccount(account);
        if (chipBalance > 0m)
            ledger.Issue(Guid.NewGuid(), chips.Id, new SimulatedAmount(chipBalance, currency.Id), Time);
        ledger.Issue(Guid.NewGuid(), house.Id, new SimulatedAmount(100m, currency.Id), Time);
        return new Setup(currency, ledger, chips, house, escrow, payouts,
            new CasinoWagerBook(ledger, house.Id, escrow.Id, payouts.Id));
    }

    private static BlackjackShoe Shoe() => new(new[]
    {
        C(BlackjackRank.Ten, BlackjackSuit.Clubs),
        C(BlackjackRank.Ten, BlackjackSuit.Spades),
        C(BlackjackRank.Nine, BlackjackSuit.Diamonds),
        C(BlackjackRank.Six, BlackjackSuit.Hearts),
        C(BlackjackRank.Two, BlackjackSuit.Hearts),
        C(BlackjackRank.Three, BlackjackSuit.Hearts)
    });

    [Fact]
    public void OpenBindsWagerAndRoundBeforeConsumingInitialDeal()
    {
        var setup = Create();
        var shoe = Shoe();
        var stake = new SimulatedAmount(10m, setup.Currency.Id);

        var session = BlackjackSession.Open(
            Guid.NewGuid(), shoe, setup.Wagers, Guid.NewGuid(), Guid.NewGuid(),
            setup.Chips.Id, stake, Time);

        Assert.Equal(2, shoe.Remaining);
        Assert.Equal(10m, setup.Ledger.GetBalance(setup.Escrow.Id));
        Assert.Equal(40m, setup.Ledger.GetBalance(setup.Chips.Id));
        Assert.Equal(BlackjackRoundState.PlayerTurn, session.RoundState);
        Assert.Equal(stake, session.Wager.Stake);
        Assert.Equal(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Nine, BlackjackSuit.Diamonds)
        }, session.PlayerCards);
        Assert.Equal(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Spades),
            C(BlackjackRank.Six, BlackjackSuit.Hearts)
        }, session.DealerCards);
    }

    [Fact]
    public void FailedWagerPlacementReleasesReservationWithoutConsumingCards()
    {
        var setup = Create(chipBalance: 0m);
        var shoe = Shoe();
        var stake = new SimulatedAmount(10m, setup.Currency.Id);

        Assert.Throws<InvalidOperationException>(() => BlackjackSession.Open(
            Guid.NewGuid(), shoe, setup.Wagers, Guid.NewGuid(), Guid.NewGuid(),
            setup.Chips.Id, stake, Time));

        Assert.Equal(6, shoe.Remaining);
        var round = BlackjackRound.Start(shoe, stake);
        Assert.Equal(C(BlackjackRank.Ten, BlackjackSuit.Clubs), round.PlayerCards[0]);
        Assert.Equal(C(BlackjackRank.Nine, BlackjackSuit.Diamonds), round.PlayerCards[1]);
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.Escrow.Id));
    }

    [Fact]
    public void InvalidArgumentsDoNotReserveOrConsumeTheShoe()
    {
        var setup = Create();
        var shoe = Shoe();

        Assert.Throws<ArgumentException>(() => BlackjackSession.Open(
            Guid.Empty, shoe, setup.Wagers, Guid.NewGuid(), Guid.NewGuid(),
            setup.Chips.Id, new SimulatedAmount(10m, setup.Currency.Id), Time));

        Assert.Equal(6, shoe.Remaining);
    }
}
