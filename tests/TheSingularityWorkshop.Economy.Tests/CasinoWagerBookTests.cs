using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class CasinoWagerBookTests
{
    private static readonly DateTimeOffset Time =
        new(2026, 11, 3, 4, 0, 0, TimeSpan.Zero);

    private static (SimulationLedger Ledger, SimulatedCurrency Currency,
        SimulationAccount Player, SimulationAccount House, SimulationAccount Escrow,
        SimulationAccount PayoutsOwed, CasinoWagerBook Book) Create(decimal playerBalance = 1_000m,
            decimal houseBalance = 10_000m)
    {
        var currency = new SimulatedCurrency(
            SimulatedCurrencyId.New(), "Singularity Denomination", "SD", 2);
        SimulationAccount Account() =>
            new(Guid.NewGuid(), EconomyEntityId.New(), currency);

        var player = Account();
        var house = Account();
        var escrow = Account();
        var payoutsOwed = Account();
        var ledger = new SimulationLedger();
        foreach (var account in new[] { player, house, escrow, payoutsOwed })
            ledger.RegisterAccount(account);
        ledger.Issue(Guid.NewGuid(), player.Id, new SimulatedAmount(playerBalance, currency.Id), Time);
        ledger.Issue(Guid.NewGuid(), house.Id, new SimulatedAmount(houseBalance, currency.Id), Time);
        var book = new CasinoWagerBook(ledger, house.Id, escrow.Id, payoutsOwed.Id);
        return (ledger, currency, player, house, escrow, payoutsOwed, book);
    }

    [Fact]
    public void CashierBuyInAndCashOutMoveValueOneForOneWithoutIssuance()
    {
        var currency = new SimulatedCurrency(
            SimulatedCurrencyId.New(), "Singularity Denomination", "SD", 2);
        var wallet = new SimulationAccount(Guid.NewGuid(), EconomyEntityId.New(), currency);
        var chips = new SimulationAccount(Guid.NewGuid(), EconomyEntityId.New(), currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(wallet);
        ledger.RegisterAccount(chips);
        ledger.Issue(Guid.NewGuid(), wallet.Id, new SimulatedAmount(100m, currency.Id), Time);
        var cashier = new CasinoCashier(ledger);

        cashier.BuyIn(Guid.NewGuid(), wallet.Id, chips.Id, new SimulatedAmount(40m, currency.Id), Time);
        Assert.Equal(60m, ledger.GetBalance(wallet.Id));
        Assert.Equal(40m, ledger.GetBalance(chips.Id));

        cashier.CashOut(Guid.NewGuid(), chips.Id, wallet.Id, new SimulatedAmount(15m, currency.Id), Time);
        Assert.Equal(75m, ledger.GetBalance(wallet.Id));
        Assert.Equal(25m, ledger.GetBalance(chips.Id));
        Assert.Equal(100m, ledger.GetBalance(wallet.Id) + ledger.GetBalance(chips.Id));
    }

    [Fact]
    public void WagerMovesStakeIntoEscrowThenLossSettlesToHouse()
    {
        var (ledger, currency, player, house, escrow, payoutsOwed, book) = Create();
        var wager = book.PlaceWager(Guid.NewGuid(), Guid.NewGuid(), player.Id,
            new SimulatedAmount(25m, currency.Id), Time);

        Assert.Equal(975m, ledger.GetBalance(player.Id));
        Assert.Equal(25m, ledger.GetBalance(escrow.Id));

        var resolved = book.ResolveWager(Guid.NewGuid(), wager.WagerId,
            CasinoWagerOutcome.Loss, new SimulatedAmount(0m, currency.Id), Time);

        Assert.Equal(CasinoWagerOutcome.Loss, resolved.Outcome);
        Assert.Equal(10_025m, ledger.GetBalance(house.Id));
        Assert.Equal(0m, ledger.GetBalance(escrow.Id));
        Assert.Equal(0m, ledger.GetBalance(payoutsOwed.Id));
        Assert.Equal(975m, ledger.GetBalance(player.Id));
        Assert.False(resolved.IsPayoutClaimed);
    }

    [Fact]
    public void WinMovesGrossPayoutToPayablesBeforePlayerClaimsIt()
    {
        var (ledger, currency, player, house, escrow, payoutsOwed, book) = Create();
        var wager = book.PlaceWager(Guid.NewGuid(), Guid.NewGuid(), player.Id,
            new SimulatedAmount(25m, currency.Id), Time);

        var resolved = book.ResolveWager(Guid.NewGuid(), wager.WagerId,
            CasinoWagerOutcome.Win, new SimulatedAmount(50m, currency.Id), Time);

        Assert.Equal(CasinoWagerOutcome.Win, resolved.Outcome);
        Assert.Equal(9_975m, ledger.GetBalance(house.Id));
        Assert.Equal(0m, ledger.GetBalance(escrow.Id));
        Assert.Equal(50m, ledger.GetBalance(payoutsOwed.Id));
        Assert.Equal(975m, ledger.GetBalance(player.Id));

        var claimed = book.ClaimPayout(Guid.NewGuid(), wager.WagerId, Time);
        Assert.True(claimed.IsPayoutClaimed);
        Assert.Equal(1_025m, ledger.GetBalance(player.Id));
        Assert.Equal(0m, ledger.GetBalance(payoutsOwed.Id));
    }

    [Fact]
    public void PushReturnsStakeThroughPayablesAndCanBeClaimedOnce()
    {
        var (ledger, currency, player, _, escrow, payoutsOwed, book) = Create();
        var wager = book.PlaceWager(Guid.NewGuid(), Guid.NewGuid(), player.Id,
            new SimulatedAmount(40m, currency.Id), Time);
        book.ResolveWager(Guid.NewGuid(), wager.WagerId,
            CasinoWagerOutcome.Push, new SimulatedAmount(40m, currency.Id), Time);

        Assert.Equal(40m, ledger.GetBalance(payoutsOwed.Id));
        var claimId = Guid.NewGuid();
        var claimed = book.ClaimPayout(claimId, wager.WagerId, Time);
        var replay = book.ClaimPayout(claimId, wager.WagerId, Time.AddMinutes(1));

        Assert.True(claimed.IsPayoutClaimed);
        Assert.True(replay.ClaimReceipt!.IsReplay);
        Assert.Equal(1_000m, ledger.GetBalance(player.Id));
        Assert.Equal(0m, ledger.GetBalance(escrow.Id));
        Assert.Equal(0m, ledger.GetBalance(payoutsOwed.Id));
    }

    [Fact]
    public void FailedWagerAndResolutionLeaveBalancesAndStateUnchanged()
    {
        var (ledger, currency, player, house, escrow, payoutsOwed, book) = Create(playerBalance: 10m, houseBalance: 0m);
        var wagerId = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => book.PlaceWager(
            Guid.NewGuid(), wagerId, player.Id, new SimulatedAmount(11m, currency.Id), Time));
        Assert.Null(book.GetWager(wagerId));
        Assert.Equal(10m, ledger.GetBalance(player.Id));
        Assert.Equal(0m, ledger.GetBalance(escrow.Id));

        var wager = book.PlaceWager(Guid.NewGuid(), wagerId, player.Id,
            new SimulatedAmount(10m, currency.Id), Time);
        Assert.Throws<InvalidOperationException>(() => book.ResolveWager(
            Guid.NewGuid(), wagerId, CasinoWagerOutcome.Win,
            new SimulatedAmount(20m, currency.Id), Time));
        Assert.Null(book.GetWager(wagerId)!.Outcome);
        Assert.Equal(10m, ledger.GetBalance(escrow.Id));
        Assert.Equal(0m, ledger.GetBalance(house.Id));
        Assert.Equal(0m, ledger.GetBalance(payoutsOwed.Id));
    }

    [Fact]
    public void ExactWagerAndResolutionRetriesAreIdempotent()
    {
        var (ledger, currency, player, _, escrow, _, book) = Create();
        var placeId = Guid.NewGuid();
        var wagerId = Guid.NewGuid();
        var stake = new SimulatedAmount(10m, currency.Id);
        var first = book.PlaceWager(placeId, wagerId, player.Id, stake, Time);
        var replay = book.PlaceWager(placeId, wagerId, player.Id, stake, Time.AddMinutes(1));
        Assert.True(replay.PlaceReceipt.IsReplay);

        var resolveId = Guid.NewGuid();
        var resolved = book.ResolveWager(resolveId, wagerId, CasinoWagerOutcome.Loss,
            new SimulatedAmount(0m, currency.Id), Time);
        var resolveReplay = book.ResolveWager(resolveId, wagerId, CasinoWagerOutcome.Loss,
            new SimulatedAmount(0m, currency.Id), Time.AddMinutes(1));

        Assert.True(resolveReplay.ResolutionReceipt!.IsReplay);
        Assert.Equal(0m, ledger.GetBalance(escrow.Id));
        Assert.Equal(990m, ledger.GetBalance(player.Id));
        Assert.Equal(CasinoWagerOutcome.Loss, resolved.Outcome);
    }

    [Fact]
    public void ConflictingWagerIdentityAndPayoutRequestsAreRejected()
    {
        var (_, currency, player, _, _, _, book) = Create();
        var wagerId = Guid.NewGuid();
        book.PlaceWager(Guid.NewGuid(), wagerId, player.Id,
            new SimulatedAmount(10m, currency.Id), Time);

        Assert.Throws<InvalidOperationException>(() => book.PlaceWager(
            Guid.NewGuid(), wagerId, player.Id, new SimulatedAmount(10m, currency.Id), Time));
        Assert.Throws<InvalidOperationException>(() => book.ResolveWager(
            Guid.NewGuid(), wagerId, CasinoWagerOutcome.Loss,
            new SimulatedAmount(0m, currency.Id), Time));
    }
}
