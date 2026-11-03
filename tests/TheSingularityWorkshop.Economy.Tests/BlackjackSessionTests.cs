using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class BlackjackSessionTests
{
    private static readonly DateTimeOffset Time =
        new(2026, 11, 3, 5, 0, 0, TimeSpan.Zero);

    private static BlackjackCard C(BlackjackRank rank, BlackjackSuit suit) => new(rank, suit);

    private sealed record Setup(
        SimulatedCurrency Currency,
        SimulationLedger Ledger,
        SimulationAccount Chips,
        SimulationAccount House,
        SimulationAccount Escrow,
        SimulationAccount PayoutsOwed,
        CasinoWagerBook Wagers);

    private static Setup Create()
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
        ledger.Issue(Guid.NewGuid(), chips.Id, new SimulatedAmount(50m, currency.Id), Time);
        ledger.Issue(Guid.NewGuid(), house.Id, new SimulatedAmount(100m, currency.Id), Time);
        return new Setup(currency, ledger, chips, house, escrow, payouts,
            new CasinoWagerBook(ledger, house.Id, escrow.Id, payouts.Id));
    }

    private static (CasinoWager Wager, BlackjackRound Round, BlackjackSession Session) Open(
        Setup setup, SimulatedAmount? requestedStake = null)
    {
        var stake = requestedStake ?? new SimulatedAmount(10m, setup.Currency.Id);
        var wager = setup.Wagers.PlaceWager(
            Guid.NewGuid(), Guid.NewGuid(), setup.Chips.Id, stake, Time);
        var round = BlackjackRound.Start(new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Ten, BlackjackSuit.Spades),
            C(BlackjackRank.Nine, BlackjackSuit.Diamonds),
            C(BlackjackRank.Six, BlackjackSuit.Hearts),
            C(BlackjackRank.Two, BlackjackSuit.Clubs)
        }), stake);
        var session = new BlackjackSession(Guid.NewGuid(), round, wager, setup.Wagers);
        return (wager, round, session);
    }

    [Fact]
    public void SessionBindsRoundToWagerAndOwnsSettlementAndClaim()
    {
        var setup = Create();
        var (_, _, session) = Open(setup);

        Assert.Equal(BlackjackRoundState.PlayerTurn, session.RoundState);
        session.Stand();
        Assert.Equal(BlackjackRoundState.Completed, session.RoundState);
        Assert.Equal(CasinoWagerOutcome.Win, session.Decision!.Outcome);

        var resolutionId = Guid.NewGuid();
        var settled = session.Settle(resolutionId, Time);
        Assert.Equal(CasinoWagerOutcome.Win, settled.Outcome);
        var replay = session.Settle(resolutionId, Time.AddMinutes(1));
        Assert.True(replay.ResolutionReceipt!.IsReplay);

        var claimId = Guid.NewGuid();
        var claimed = session.ClaimPayout(claimId, Time);
        Assert.True(claimed.IsPayoutClaimed);
        Assert.True(session.ClaimPayout(claimId, Time.AddMinutes(1)).ClaimReceipt!.IsReplay);
        Assert.Equal(60m, setup.Ledger.GetBalance(setup.Chips.Id));
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.PayoutsOwed.Id));
        Assert.Throws<InvalidOperationException>(() => session.Hit());
    }

    [Fact]
    public void SessionRejectsRoundAndWagerWithDifferentTypedStakes()
    {
        var setup = Create();
        var wager = setup.Wagers.PlaceWager(Guid.NewGuid(), Guid.NewGuid(),
            setup.Chips.Id, new SimulatedAmount(10m, setup.Currency.Id), Time);
        var round = BlackjackRound.Start(new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Ten, BlackjackSuit.Spades),
            C(BlackjackRank.Nine, BlackjackSuit.Diamonds),
            C(BlackjackRank.Six, BlackjackSuit.Hearts)
        }), new SimulatedAmount(9m, setup.Currency.Id));

        Assert.Throws<ArgumentException>(() =>
            new BlackjackSession(Guid.NewGuid(), round, wager, setup.Wagers));
        Assert.Equal(40m, setup.Ledger.GetBalance(setup.Chips.Id));
        Assert.Equal(10m, setup.Ledger.GetBalance(setup.Escrow.Id));
    }

    [Fact]
    public void SessionRejectsUnknownOrAlreadyResolvedWagerSnapshots()
    {
        var setup = Create();
        var (wager, round, _) = Open(setup);
        setup.Wagers.ResolveWager(Guid.NewGuid(), wager.WagerId,
            CasinoWagerOutcome.Loss, new SimulatedAmount(0m, setup.Currency.Id), Time);

        Assert.Throws<InvalidOperationException>(() =>
            new BlackjackSession(Guid.NewGuid(), round, wager, setup.Wagers));
    }

    [Fact]
    public void SessionCannotSettleBeforeRoundCompletesOrClaimBeforeSettlement()
    {
        var setup = Create();
        var (_, _, session) = Open(setup);

        Assert.Throws<InvalidOperationException>(() => session.Settle(Guid.NewGuid(), Time));
        Assert.Throws<InvalidOperationException>(() => session.ClaimPayout(Guid.NewGuid(), Time));
        Assert.Equal(10m, setup.Ledger.GetBalance(setup.Escrow.Id));
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.PayoutsOwed.Id));
    }

    [Fact]
    public void SessionDetectsSettlementPerformedOutsideItsBoundary()
    {
        var setup = Create();
        var (wager, _, session) = Open(setup);
        setup.Wagers.ResolveWager(Guid.NewGuid(), wager.WagerId,
            CasinoWagerOutcome.Loss, new SimulatedAmount(0m, setup.Currency.Id), Time);

        Assert.Throws<InvalidOperationException>(() => session.Hit());
        Assert.Throws<InvalidOperationException>(() => session.Settle(Guid.NewGuid(), Time));
    }
}
