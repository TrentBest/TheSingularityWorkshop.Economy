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
    public void BalanceProjectionMatchesTheSignedJournal()
    {
        var currency = Currency();
        var source = Account(currency);
        var destination = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(destination);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(80m, currency), Time);
        ledger.Transfer(Guid.NewGuid(), source.Id, destination.Id, Amount(25m, currency), Time);
        ledger.Retire(Guid.NewGuid(), destination.Id, Amount(5m, currency), Time);

        Assert.Equal(ledger.GetEntries(source.Id).Sum(entry => entry.Delta), ledger.GetBalance(source.Id));
        Assert.Equal(ledger.GetEntries(destination.Id).Sum(entry => entry.Delta), ledger.GetBalance(destination.Id));
    }

    [Fact]
    public void ConcurrentTransfersRemainAtomicAndCannotOverdraw()
    {
        var currency = Currency();
        var source = Account(currency);
        var destination = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(destination);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(100m, currency), Time);

        Parallel.For(0, 100, _ =>
            ledger.Transfer(Guid.NewGuid(), source.Id, destination.Id, Amount(1m, currency), Time));

        Assert.Equal(0m, ledger.GetBalance(source.Id));
        Assert.Equal(100m, ledger.GetBalance(destination.Id));
        Assert.Equal(0m, ledger.GetEntries(source.Id).Sum(entry => entry.Delta) - ledger.GetBalance(source.Id));
        Assert.Equal(100m, ledger.GetEntries(destination.Id).Sum(entry => entry.Delta));
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

    [Fact]
    public void PostSettlesThreeAccountsAtomicallyAndReturnsImmutablePostingReceipt()
    {
        var currency = Currency();
        var source = Account(currency);
        var house = Account(currency);
        var escrow = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(house);
        ledger.RegisterAccount(escrow);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(100m, currency), Time);
        var operationId = Guid.NewGuid();

        var receipt = ledger.Post(operationId, new[]
        {
            new SimulationLedgerPosting(source.Id, currency.Id, -30m),
            new SimulationLedgerPosting(house.Id, currency.Id, 10m),
            new SimulationLedgerPosting(escrow.Id, currency.Id, 20m)
        }, Time);

        Assert.Equal(SimulationLedgerOperationKind.Transfer, receipt.Kind);
        Assert.Null(receipt.FromAccountId);
        Assert.Null(receipt.ToAccountId);
        Assert.Equal(30m, receipt.Amount.Value);
        Assert.Equal(3, receipt.Postings!.Count);
        Assert.Equal(70m, ledger.GetBalance(source.Id));
        Assert.Equal(10m, ledger.GetBalance(house.Id));
        Assert.Equal(20m, ledger.GetBalance(escrow.Id));
        Assert.Equal(4, ledger.GetEntries(source.Id).Count + ledger.GetEntries(house.Id).Count + ledger.GetEntries(escrow.Id).Count);
        Assert.True(Assert.IsAssignableFrom<IList<SimulationLedgerPosting>>(receipt.Postings).IsReadOnly);
    }

    [Fact]
    public void PostRejectsLateInsufficientBalanceWithoutAnyPartialCommit()
    {
        var currency = Currency();
        var funded = Account(currency);
        var underfunded = Account(currency);
        var recipient = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(funded);
        ledger.RegisterAccount(underfunded);
        ledger.RegisterAccount(recipient);
        ledger.Issue(Guid.NewGuid(), funded.Id, Amount(10m, currency), Time);
        var operationId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() => ledger.Post(operationId, new[]
        {
            new SimulationLedgerPosting(funded.Id, currency.Id, -5m),
            new SimulationLedgerPosting(underfunded.Id, currency.Id, -1m),
            new SimulationLedgerPosting(recipient.Id, currency.Id, 6m)
        }, Time));

        Assert.Equal(10m, ledger.GetBalance(funded.Id));
        Assert.Equal(0m, ledger.GetBalance(underfunded.Id));
        Assert.Equal(0m, ledger.GetBalance(recipient.Id));
        Assert.Null(ledger.GetReceipt(operationId));
        Assert.Single(ledger.GetEntries(funded.Id));
        Assert.Empty(ledger.GetEntries(underfunded.Id));
        Assert.Empty(ledger.GetEntries(recipient.Id));
    }

    [Fact]
    public void PostRejectsOverflowWithoutAnyPartialCommit()
    {
        var currency = Currency();
        var source = Account(currency);
        var full = Account(currency);
        var recipient = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(full);
        ledger.RegisterAccount(recipient);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(decimal.MaxValue, currency), Time);
        ledger.Issue(Guid.NewGuid(), full.Id, Amount(decimal.MaxValue, currency), Time);
        var operationId = Guid.NewGuid();

        Assert.Throws<OverflowException>(() => ledger.Post(operationId, new[]
        {
            new SimulationLedgerPosting(source.Id, currency.Id, -2m),
            new SimulationLedgerPosting(full.Id, currency.Id, 1m),
            new SimulationLedgerPosting(recipient.Id, currency.Id, 1m)
        }, Time));

        Assert.Equal(decimal.MaxValue, ledger.GetBalance(source.Id));
        Assert.Equal(decimal.MaxValue, ledger.GetBalance(full.Id));
        Assert.Equal(0m, ledger.GetBalance(recipient.Id));
        Assert.Null(ledger.GetReceipt(operationId));
        Assert.Single(ledger.GetEntries(source.Id));
        Assert.Single(ledger.GetEntries(full.Id));
        Assert.Empty(ledger.GetEntries(recipient.Id));
    }

    [Fact]
    public void PostRejectsAggregateReceiptAmountOverflowWithoutAnyPartialCommit()
    {
        var currency = Currency();
        var creditOne = new SimulationAccount(
            new Guid("00000000-0000-0000-0000-000000000001"), EconomyEntityId.New(), currency);
        var debitOne = new SimulationAccount(
            new Guid("00000000-0000-0000-0000-000000000002"), EconomyEntityId.New(), currency);
        var creditTwo = new SimulationAccount(
            new Guid("00000000-0000-0000-0000-000000000003"), EconomyEntityId.New(), currency);
        var debitTwo = new SimulationAccount(
            new Guid("00000000-0000-0000-0000-000000000004"), EconomyEntityId.New(), currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(creditOne);
        ledger.RegisterAccount(debitOne);
        ledger.RegisterAccount(creditTwo);
        ledger.RegisterAccount(debitTwo);
        ledger.Issue(Guid.NewGuid(), debitOne.Id, Amount(decimal.MaxValue, currency), Time);
        ledger.Issue(Guid.NewGuid(), debitTwo.Id, Amount(decimal.MaxValue, currency), Time);
        var operationId = Guid.NewGuid();

        Assert.Throws<OverflowException>(() => ledger.Post(operationId, new[]
        {
            new SimulationLedgerPosting(creditOne.Id, currency.Id, decimal.MaxValue),
            new SimulationLedgerPosting(debitOne.Id, currency.Id, -decimal.MaxValue),
            new SimulationLedgerPosting(creditTwo.Id, currency.Id, decimal.MaxValue),
            new SimulationLedgerPosting(debitTwo.Id, currency.Id, -decimal.MaxValue)
        }, Time));

        Assert.Equal(0m, ledger.GetBalance(creditOne.Id));
        Assert.Equal(decimal.MaxValue, ledger.GetBalance(debitOne.Id));
        Assert.Equal(0m, ledger.GetBalance(creditTwo.Id));
        Assert.Equal(decimal.MaxValue, ledger.GetBalance(debitTwo.Id));
        Assert.Null(ledger.GetReceipt(operationId));
        Assert.Empty(ledger.GetEntries(creditOne.Id));
        Assert.Single(ledger.GetEntries(debitOne.Id));
        Assert.Empty(ledger.GetEntries(creditTwo.Id));
        Assert.Single(ledger.GetEntries(debitTwo.Id));
    }

    [Fact]
    public void PostRequiresBalancedSameCurrencyPrecisionValidUniqueAccountPostings()
    {
        var currency = Currency();
        var otherCurrency = Currency("Other");
        var first = Account(currency);
        var second = Account(currency);
        var other = Account(otherCurrency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(first);
        ledger.RegisterAccount(second);
        ledger.RegisterAccount(other);

        Assert.Throws<InvalidOperationException>(() => ledger.Post(Guid.NewGuid(), new[]
        {
            new SimulationLedgerPosting(first.Id, currency.Id, -2m),
            new SimulationLedgerPosting(second.Id, currency.Id, 1m)
        }, Time));
        Assert.Throws<InvalidOperationException>(() => ledger.Post(Guid.NewGuid(), new[]
        {
            new SimulationLedgerPosting(first.Id, currency.Id, -1m),
            new SimulationLedgerPosting(other.Id, otherCurrency.Id, 1m)
        }, Time));
        Assert.Throws<ArgumentOutOfRangeException>(() => ledger.Post(Guid.NewGuid(), new[]
        {
            new SimulationLedgerPosting(first.Id, currency.Id, -1.001m),
            new SimulationLedgerPosting(second.Id, currency.Id, 1.001m)
        }, Time));
        Assert.Throws<ArgumentException>(() => ledger.Post(Guid.NewGuid(), new[]
        {
            new SimulationLedgerPosting(first.Id, currency.Id, -1m),
            new SimulationLedgerPosting(first.Id, currency.Id, 1m)
        }, Time));

        Assert.Equal(0m, ledger.GetBalance(first.Id));
        Assert.Equal(0m, ledger.GetBalance(second.Id));
        Assert.Equal(0m, ledger.GetBalance(other.Id));
    }

    [Fact]
    public void PostExactRetryIsIdempotentAndConflictingRetryIsRejected()
    {
        var currency = Currency();
        var source = Account(currency);
        var house = Account(currency);
        var escrow = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(house);
        ledger.RegisterAccount(escrow);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(100m, currency), Time);
        var operationId = Guid.NewGuid();
        var postings = new[]
        {
            new SimulationLedgerPosting(source.Id, currency.Id, -30m),
            new SimulationLedgerPosting(house.Id, currency.Id, 10m),
            new SimulationLedgerPosting(escrow.Id, currency.Id, 20m)
        };

        var first = ledger.Post(operationId, postings, Time);
        var replay = ledger.Post(operationId, postings.Reverse().ToArray(), Time.AddMinutes(5));

        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(first.OccurredAt, replay.OccurredAt);
        Assert.Equal(70m, ledger.GetBalance(source.Id));
        Assert.Equal(10m, ledger.GetBalance(house.Id));
        Assert.Equal(20m, ledger.GetBalance(escrow.Id));
        Assert.Throws<InvalidOperationException>(() => ledger.Post(operationId, new[]
        {
            new SimulationLedgerPosting(source.Id, currency.Id, -31m),
            new SimulationLedgerPosting(house.Id, currency.Id, 11m),
            new SimulationLedgerPosting(escrow.Id, currency.Id, 20m)
        }, Time));
        Assert.Equal(70m, ledger.GetBalance(source.Id));
        Assert.Equal(10m, ledger.GetBalance(house.Id));
        Assert.Equal(20m, ledger.GetBalance(escrow.Id));
    }

    [Fact]
    public void ConcurrentMultiAccountSettlementsCannotOverdraw()
    {
        var currency = Currency();
        var source = Account(currency);
        var house = Account(currency);
        var escrow = Account(currency);
        var ledger = new SimulationLedger();
        ledger.RegisterAccount(source);
        ledger.RegisterAccount(house);
        ledger.RegisterAccount(escrow);
        ledger.Issue(Guid.NewGuid(), source.Id, Amount(100m, currency), Time);

        Parallel.For(0, 100, _ => ledger.Post(Guid.NewGuid(), new[]
        {
            new SimulationLedgerPosting(source.Id, currency.Id, -1m),
            new SimulationLedgerPosting(house.Id, currency.Id, 0.25m),
            new SimulationLedgerPosting(escrow.Id, currency.Id, 0.75m)
        }, Time));

        Assert.Equal(0m, ledger.GetBalance(source.Id));
        Assert.Equal(25m, ledger.GetBalance(house.Id));
        Assert.Equal(75m, ledger.GetBalance(escrow.Id));
        Assert.Equal(0m, ledger.GetBalance(source.Id) + ledger.GetBalance(house.Id) + ledger.GetBalance(escrow.Id) - 100m);
        Assert.Equal(ledger.GetBalance(source.Id), ledger.GetEntries(source.Id).Sum(entry => entry.Delta));
        Assert.Equal(25m, ledger.GetEntries(house.Id).Sum(entry => entry.Delta));
        Assert.Equal(75m, ledger.GetEntries(escrow.Id).Sum(entry => entry.Delta));
    }

}
