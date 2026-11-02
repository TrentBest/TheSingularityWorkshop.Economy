using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class SimulationLedgerTests
{
    private static readonly DateTimeOffset Time =
        new(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

    private static SimulatedCurrency Currency(string name = "Singularity Denomination") =>
        new(SimulatedCurrencyId.New(), name, "SD", 2);

    private static SimulationAccount Account(SimulatedCurrency currency) =>
        new(Guid.NewGuid(), EconomyEntityId.New(), currency);

    private static SimulatedAmount Amount(decimal value, SimulatedCurrency currency) =>
        new(value, currency.Id);

    [Fact]
    public void IssueCreatesAnExplicitCreditAndBalanceIsDerivedFromJournal()
    {
        var currency = Currency();
        var account = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(account);

        var receipt = ledger.Issue(Guid.NewGuid(), account.Id, Amount(100m, currency), Time);

        Assert.Equal(SimulationLedgerOperationKind.Issuance, receipt.Kind);
        Assert.Equal(100m, ledger.GetBalance(account.Id));
        var entry = Assert.Single(ledger.GetEntries(account.Id));
        Assert.Equal(100m, entry.Delta);
        Assert.Equal(SimulationLedgerOperationKind.Issuance, entry.Kind);
    }

    [Fact]
    public void TransferDebitsAndCreditsBothAccountsAtomically()
    {
        var currency = Currency();
        var source = Account(currency);
        var destination = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(destination);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(100m, currency), Time);

        var receipt = ledger.Transfer(
            Guid.NewGuid(), source.Id, destination.Id, Amount(35m, currency), Time);

        Assert.Equal(SimulationLedgerOperationKind.Transfer, receipt.Kind);
        Assert.Equal(65m, ledger.GetBalance(source.Id));
        Assert.Equal(35m, ledger.GetBalance(destination.Id));
        Assert.Equal(100m, ledger.GetBalance(source.Id) + ledger.GetBalance(destination.Id));
        Assert.Equal(-35m, ledger.GetEntries(source.Id).Last().Delta);
        Assert.Equal(35m, ledger.GetEntries(destination.Id).Single().Delta);
    }

    [Fact]
    public void ExactRetryReturnsReplayReceiptWithoutApplyingTheOperationTwice()
    {
        var currency = Currency();
        var source = Account(currency);
        var destination = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(destination);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(100m, currency), Time);
        var operationId = Guid.NewGuid();

        var first = ledger.Transfer(operationId, source.Id, destination.Id, Amount(25m, currency), Time);
        var replay = ledger.Transfer(
            operationId, source.Id, destination.Id, Amount(25m, currency), Time.AddMinutes(5));

        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(first.OccurredAt, replay.OccurredAt);
        Assert.Equal(75m, ledger.GetBalance(source.Id));
        Assert.Equal(25m, ledger.GetBalance(destination.Id));
        Assert.Equal(2, ledger.GetEntries(source.Id).Count);
        Assert.Single(ledger.GetEntries(destination.Id));
    }

    [Fact]
    public void ReusingOperationIdForDifferentRequestIsRejectedWithoutSideEffects()
    {
        var currency = Currency();
        var source = Account(currency);
        var destination = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(destination);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(100m, currency), Time);
        var operationId = Guid.NewGuid();

        ledger.Transfer(operationId, source.Id, destination.Id, Amount(25m, currency), Time);

        Assert.Throws<InvalidOperationException>(() =>
            ledger.Transfer(operationId, source.Id, destination.Id, Amount(30m, currency), Time));

        Assert.Equal(75m, ledger.GetBalance(source.Id));
        Assert.Equal(25m, ledger.GetBalance(destination.Id));
    }

    [Fact]
    public void InsufficientFundsDoesNotCreateAnyPostingOrReceipt()
    {
        var currency = Currency();
        var source = Account(currency);
        var destination = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(destination);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(10m, currency), Time);
        var operationId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() =>
            ledger.Transfer(operationId, source.Id, destination.Id, Amount(11m, currency), Time));

        Assert.Equal(10m, ledger.GetBalance(source.Id));
        Assert.Equal(0m, ledger.GetBalance(destination.Id));
        Assert.Null(ledger.GetReceipt(operationId));
        Assert.Single(ledger.GetEntries(source.Id));
        Assert.Empty(ledger.GetEntries(destination.Id));
    }

    [Fact]
    public void TypedCurrencyIdentityPreventsMixingCurrenciesWithTheSameSymbol()
    {
        var firstCurrency = Currency("Currency One");
        var secondCurrency = Currency("Currency Two");
        Assert.Equal(firstCurrency.Symbol, secondCurrency.Symbol);
        Assert.NotEqual(firstCurrency.Id, secondCurrency.Id);

        var account = Account(firstCurrency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(account);

        Assert.Throws<InvalidOperationException>(() =>
            ledger.Issue(Guid.NewGuid(), account.Id, Amount(10m, secondCurrency), Time));
    }

    [Fact]
    public void CurrencyIdentityCannotBeRegisteredWithConflictingMetadata()
    {
        var currencyId = SimulatedCurrencyId.New();
        var first = new SimulatedCurrency(currencyId, "First Name", "SD");
        var conflicting = new SimulatedCurrency(currencyId, "Different Name", "SD");
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(Account(first));

        Assert.Throws<InvalidOperationException>(() =>
            ledger.RegisterAccount(Account(conflicting)));
    }

    [Fact]
    public void RetirementIsAnExplicitDebitAndCannotOverdraw()
    {
        var currency = Currency();
        var account = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(account);
        ledger.Issue(Guid.NewGuid(), account.Id, Amount(50m, currency), Time);

        var receipt = ledger.Retire(Guid.NewGuid(), account.Id, Amount(20m, currency), Time);

        Assert.Equal(SimulationLedgerOperationKind.Retirement, receipt.Kind);
        Assert.Equal(30m, ledger.GetBalance(account.Id));
        Assert.Equal(-20m, ledger.GetEntries(account.Id).Last().Delta);
        Assert.Throws<InvalidOperationException>(() =>
            ledger.Retire(Guid.NewGuid(), account.Id, Amount(31m, currency), Time));
        Assert.Equal(30m, ledger.GetBalance(account.Id));
    }

    [Fact]
    public void LedgerRequiresRegisteredAccountsAndPositiveOperations()
    {
        var currency = Currency();
        var account = Account(currency);
        var ledger = new SimulationLedger();

        Assert.Throws<KeyNotFoundException>(() =>
            ledger.Issue(Guid.NewGuid(), account.Id, Amount(1m, currency), Time));

        ledger.RegisterAccount(account);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ledger.Issue(Guid.NewGuid(), account.Id, Amount(0m, currency), Time));
        Assert.Throws<ArgumentException>(() =>
            ledger.Issue(Guid.Empty, account.Id, Amount(1m, currency), Time));
    }

    [Fact]
    public void LedgerRejectsAmountsBeyondCurrencyPrecisionWithoutSideEffects()
    {
        var currency = Currency();
        var account = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(account);
        var operationId = Guid.NewGuid();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ledger.Issue(operationId, account.Id, Amount(1.001m, currency), Time));

        Assert.Equal(0m, ledger.GetBalance(account.Id));
        Assert.Null(ledger.GetReceipt(operationId));
        Assert.Empty(ledger.GetEntries(account.Id));
    }

    [Fact]
    public void JournalSnapshotCannotBeMutatedByCaller()
    {
        var currency = Currency();
        var account = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(account);
        ledger.Issue(Guid.NewGuid(), account.Id, Amount(1m, currency), Time);

        var entries = Assert.IsAssignableFrom<IList<SimulationLedgerEntry>>(ledger.GetEntries(account.Id));

        Assert.True(entries.IsReadOnly);
        Assert.Throws<NotSupportedException>(() =>
            entries.Add(new SimulationLedgerEntry(
                Guid.NewGuid(), account.Id, currency.Id, 10m,
                SimulationLedgerOperationKind.Issuance, Time)));
    }
}
