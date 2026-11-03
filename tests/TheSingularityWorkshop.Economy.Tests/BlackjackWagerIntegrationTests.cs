using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

/// <summary>
/// Verifies the host-owned seam between deterministic blackjack decisions and
/// ledger-backed wager settlement. The round engine itself remains ledger-agnostic.
/// </summary>
public sealed class BlackjackWagerIntegrationTests
{
    private static readonly DateTimeOffset Time =
        new(2026, 11, 3, 4, 0, 0, TimeSpan.Zero);

    private static BlackjackCard C(BlackjackRank rank, BlackjackSuit suit) => new(rank, suit);

    private static (SimulationLedger Ledger, SimulatedCurrency Currency,
        SimulationAccount Wallet, SimulationAccount Chips, SimulationAccount House,
        SimulationAccount Escrow, SimulationAccount PayoutsOwed,
        CasinoCashier Cashier, CasinoWagerBook Wagers) Create()
    {
        var currency = new SimulatedCurrency(
            SimulatedCurrencyId.New(), "Singularity Denomination", "SD", 2);
        SimulationAccount Account() => new(Guid.NewGuid(), EconomyEntityId.New(), currency);

        var wallet = Account();
        var chips = Account();
        var house = Account();
        var escrow = Account();
        var payoutsOwed = Account();
        var ledger = new SimulationLedger();
        foreach (var account in new[] { wallet, chips, house, escrow, payoutsOwed })
            ledger.RegisterAccount(account);

        ledger.Issue(Guid.NewGuid(), wallet.Id, new SimulatedAmount(100m, currency.Id), Time);
        ledger.Issue(Guid.NewGuid(), house.Id, new SimulatedAmount(100m, currency.Id), Time);

        return (ledger, currency, wallet, chips, house, escrow, payoutsOwed,
            new CasinoCashier(ledger),
            new CasinoWagerBook(ledger, house.Id, escrow.Id, payoutsOwed.Id));
    }

    [Fact]
    public void DeterministicBlackjackWinFlowsThroughEscrowSettlementAndClaim()
    {
        var setup = Create();
        var stake = new SimulatedAmount(10m, setup.Currency.Id);
        setup.Cashier.BuyIn(Guid.NewGuid(), setup.Wallet.Id, setup.Chips.Id,
            new SimulatedAmount(20m, setup.Currency.Id), Time);

        var wagerId = Guid.NewGuid();
        setup.Wagers.PlaceWager(Guid.NewGuid(), wagerId, setup.Chips.Id, stake, Time);

        var shoe = new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Ten, BlackjackSuit.Spades),
            C(BlackjackRank.Nine, BlackjackSuit.Diamonds),
            C(BlackjackRank.Six, BlackjackSuit.Hearts),
            C(BlackjackRank.Two, BlackjackSuit.Clubs)
        });
        var round = BlackjackRound.Start(shoe, stake);
        round.Stand();

        var decision = Assert.IsType<BlackjackDecision>(round.Decision);
        Assert.Equal(CasinoWagerOutcome.Win, decision.Outcome);
        Assert.Equal(20m, decision.GrossPayout.Value);

        var resolutionId = Guid.NewGuid();
        var resolved = setup.Wagers.ResolveWager(
            resolutionId, wagerId, decision.Outcome, decision.GrossPayout, Time);
        var replay = setup.Wagers.ResolveWager(
            resolutionId, wagerId, decision.Outcome, decision.GrossPayout, Time.AddMinutes(1));

        Assert.True(replay.ResolutionReceipt!.IsReplay);
        Assert.Equal(20m, setup.Ledger.GetBalance(setup.PayoutsOwed.Id));
        Assert.Equal(90m, setup.Ledger.GetBalance(setup.Chips.Id));
        Assert.Equal(90m, setup.Ledger.GetBalance(setup.Wallet.Id));
        Assert.Equal(90m, setup.Ledger.GetBalance(setup.House.Id));
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.Escrow.Id));

        var claimed = setup.Wagers.ClaimPayout(Guid.NewGuid(), wagerId, Time);
        Assert.True(claimed.IsPayoutClaimed);
        Assert.Equal(110m, setup.Ledger.GetBalance(setup.Chips.Id));
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.PayoutsOwed.Id));
        Assert.Equal(200m,
            setup.Ledger.GetBalance(setup.Wallet.Id) +
            setup.Ledger.GetBalance(setup.Chips.Id) +
            setup.Ledger.GetBalance(setup.House.Id) +
            setup.Ledger.GetBalance(setup.Escrow.Id) +
            setup.Ledger.GetBalance(setup.PayoutsOwed.Id));
    }

    [Fact]
    public void DeterministicPlayerBustSettlesAsLossWithoutPayoutClaim()
    {
        var setup = Create();
        var stake = new SimulatedAmount(10m, setup.Currency.Id);
        setup.Wagers.PlaceWager(Guid.NewGuid(), Guid.NewGuid(), setup.Chips.Id, stake, Time);
        var wager = setup.Wagers.GetWager(setup.Wagers.GetWager(Guid.Empty)?.WagerId ?? Guid.Empty);
        Assert.Null(wager);

        var wagerId = setup.Wagers.GetWager(Guid.Empty)?.WagerId ?? Guid.NewGuid();
        var actualWager = setup.Wagers.PlaceWager(Guid.NewGuid(), wagerId, setup.Chips.Id, stake, Time);
        var round = BlackjackRound.Start(new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Seven, BlackjackSuit.Clubs),
            C(BlackjackRank.Six, BlackjackSuit.Diamonds),
            C(BlackjackRank.Ten, BlackjackSuit.Diamonds),
            C(BlackjackRank.Eight, BlackjackSuit.Hearts)
        }), stake);
        round.Hit();

        var decision = Assert.IsType<BlackjackDecision>(round.Decision);
        Assert.Equal(CasinoWagerOutcome.Loss, decision.Outcome);
        Assert.Equal(0m, decision.GrossPayout.Value);
        var resolved = setup.Wagers.ResolveWager(
            Guid.NewGuid(), actualWager.WagerId, decision.Outcome, decision.GrossPayout, Time);

        Assert.Equal(CasinoWagerOutcome.Loss, resolved.Outcome);
        Assert.Equal(110m, setup.Ledger.GetBalance(setup.House.Id));
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.Escrow.Id));
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.PayoutsOwed.Id));
        Assert.Throws<InvalidOperationException>(() =>
            setup.Wagers.ClaimPayout(Guid.NewGuid(), actualWager.WagerId, Time));
    }

    [Fact]
    public void DeterministicPushReturnsTheStakeThroughTheClaimPath()
    {
        var setup = Create();
        var stake = new SimulatedAmount(10m, setup.Currency.Id);
        var wager = setup.Wagers.PlaceWager(Guid.NewGuid(), Guid.NewGuid(),
            setup.Chips.Id, stake, Time);
        var round = BlackjackRound.Start(new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Ten, BlackjackSuit.Spades),
            C(BlackjackRank.Eight, BlackjackSuit.Diamonds),
            C(BlackjackRank.Eight, BlackjackSuit.Hearts)
        }), stake);
        round.Stand();

        var decision = Assert.IsType<BlackjackDecision>(round.Decision);
        Assert.Equal(CasinoWagerOutcome.Push, decision.Outcome);
        Assert.Equal(10m, decision.GrossPayout.Value);

        setup.Wagers.ResolveWager(Guid.NewGuid(), wager.WagerId,
            decision.Outcome, decision.GrossPayout, Time);
        Assert.Equal(10m, setup.Ledger.GetBalance(setup.PayoutsOwed.Id));

        setup.Wagers.ClaimPayout(Guid.NewGuid(), wager.WagerId, Time);
        Assert.Equal(100m, setup.Ledger.GetBalance(setup.Chips.Id));
        Assert.Equal(0m, setup.Ledger.GetBalance(setup.PayoutsOwed.Id));
        Assert.Equal(100m, setup.Ledger.GetBalance(setup.House.Id));
    }

    [Fact]
    public void FailedHouseFundingDoesNotPartiallySettleAWinningRound()
    {
        var currency = new SimulatedCurrency(
            SimulatedCurrencyId.New(), "Singularity Denomination", "SD", 2);
        SimulationAccount Account() => new(Guid.NewGuid(), EconomyEntityId.New(), currency);
        var chips = Account();
        var house = Account();
        var escrow = Account();
        var payoutsOwed = Account();
        var ledger = new SimulationLedger();
        foreach (var account in new[] { chips, house, escrow, payoutsOwed })
            ledger.RegisterAccount(account);
        ledger.Issue(Guid.NewGuid(), chips.Id, new SimulatedAmount(10m, currency.Id), Time);

        var wagers = new CasinoWagerBook(ledger, house.Id, escrow.Id, payoutsOwed.Id);
        var stake = new SimulatedAmount(10m, currency.Id);
        var wager = wagers.PlaceWager(Guid.NewGuid(), Guid.NewGuid(), chips.Id, stake, Time);
        var round = BlackjackRound.Start(new BlackjackShoe(new[]
        {
            C(BlackjackRank.Ten, BlackjackSuit.Clubs),
            C(BlackjackRank.Ten, BlackjackSuit.Spades),
            C(BlackjackRank.Nine, BlackjackSuit.Diamonds),
            C(BlackjackRank.Six, BlackjackSuit.Hearts),
            C(BlackjackRank.Two, BlackjackSuit.Clubs)
        }), stake);
        round.Stand();
        var decision = Assert.IsType<BlackjackDecision>(round.Decision);
        Assert.Equal(CasinoWagerOutcome.Win, decision.Outcome);

        Assert.Throws<InvalidOperationException>(() => wagers.ResolveWager(
            Guid.NewGuid(), wager.WagerId, decision.Outcome, decision.GrossPayout, Time));

        Assert.Null(wagers.GetWager(wager.WagerId)!.Outcome);
        Assert.Equal(0m, ledger.GetBalance(house.Id));
        Assert.Equal(10m, ledger.GetBalance(escrow.Id));
        Assert.Equal(0m, ledger.GetBalance(payoutsOwed.Id));
        Assert.Equal(0m, ledger.GetBalance(chips.Id));
    }
}
