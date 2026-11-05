using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class CurrencyModelRuntimeTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);

    private static CurrencyModelDefinition Model(params CurrencyModelAspect[] aspects) =>
        new(
            "cartoon-town.simoleans",
            1,
            new SimulatedCurrency(
                new SimulatedCurrencyId(Guid.Parse("a2d4f4b5-1b2c-4c3d-8e9f-0123456789ab")),
                "Simoleans",
                "§",
                0),
            aspects,
            issuanceAuthority: aspects.Contains(CurrencyModelAspect.Issuance) ? "town-treasury" : null,
            earningSources: aspects.Contains(CurrencyModelAspect.EarningSources) ? new[] { "quest.reward", "employment.wages" } : null,
            sinks: aspects.Contains(CurrencyModelAspect.Sinks) ? new[] { "shops.purchase", "public-services.fee" } : null);

    private static (SimulationLedger Ledger, Guid First, Guid Second, SimulatedAmount Amount) Setup()
    {
        var currency = new SimulatedCurrency(
            new SimulatedCurrencyId(Guid.Parse("a2d4f4b5-1b2c-4c3d-8e9f-0123456789ab")),
            "Simoleans",
            "§",
            0);
        var ledger = new SimulationLedger();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        ledger.RegisterAccount(new SimulationAccount(first, EconomyEntityId.New(), currency));
        ledger.RegisterAccount(new SimulationAccount(second, EconomyEntityId.New(), currency));
        return (ledger, first, second, new SimulatedAmount(10m, currency.Id));
    }

    [Fact]
    public void IssueRequiresDeclaredAuthorityAndEarningSourceAndRecordsEvidence()
    {
        var (ledger, account, _, amount) = Setup();
        var runtime = new CurrencyModelRuntime(
            Model(CurrencyModelAspect.Issuance, CurrencyModelAspect.EarningSources),
            ledger);

        Assert.Throws<InvalidOperationException>(() =>
            runtime.Issue(Guid.NewGuid(), account, amount, "other-treasury", "quest.reward", Now));
        Assert.Throws<InvalidOperationException>(() =>
            runtime.Issue(Guid.NewGuid(), account, amount, "town-treasury", "unlisted.source", Now));

        var operationId = Guid.NewGuid();
        var result = runtime.Issue(operationId, account, amount, "town-treasury", "quest.reward", Now);

        Assert.Equal(CurrencyModelEventKind.Issued, result.Kind);
        Assert.Equal("quest.reward", result.PolicyIdentifier);
        Assert.Equal(operationId, result.LedgerReceipt.OperationId);
        Assert.Equal(10m, ledger.GetBalance(account));
    }

    [Fact]
    public void RetireRequiresDeclaredSink()
    {
        var (ledger, account, _, amount) = Setup();
        ledger.Issue(Guid.NewGuid(), account, amount, Now);
        var runtime = new CurrencyModelRuntime(Model(CurrencyModelAspect.Sinks), ledger);

        Assert.Throws<InvalidOperationException>(() =>
            runtime.Retire(Guid.NewGuid(), account, amount, "unlisted.sink", Now));

        var result = runtime.Retire(Guid.NewGuid(), account, amount, "shops.purchase", Now);

        Assert.Equal(CurrencyModelEventKind.Retired, result.Kind);
        Assert.Equal("shops.purchase", result.PolicyIdentifier);
        Assert.Equal(0m, ledger.GetBalance(account));
    }

    [Fact]
    public void PeerTransferIsDeniedUnlessSelected()
    {
        var (ledger, first, second, amount) = Setup();
        ledger.Issue(Guid.NewGuid(), first, amount, Now);
        var disabled = new CurrencyModelRuntime(Model(), ledger);

        Assert.Throws<InvalidOperationException>(() =>
            disabled.Transfer(Guid.NewGuid(), first, second, amount, Now));

        var enabled = new CurrencyModelRuntime(Model(CurrencyModelAspect.PeerTransfers), ledger);
        var result = enabled.Transfer(Guid.NewGuid(), first, second, amount, Now);

        Assert.Equal(CurrencyModelEventKind.Transferred, result.Kind);
        Assert.Equal(0m, ledger.GetBalance(first));
        Assert.Equal(10m, ledger.GetBalance(second));
    }

    [Fact]
    public void RuntimeRejectsInvalidModelBeforeAnyExecution()
    {
        var (ledger, _, _, _) = Setup();
        var invalid = new CurrencyModelDefinition(
            "invalid.model",
            1,
            new SimulatedCurrency(
                new SimulatedCurrencyId(Guid.Parse("a2d4f4b5-1b2c-4c3d-8e9f-0123456789ab")),
                "Simoleans",
                "§",
                0),
            new[] { CurrencyModelAspect.Issuance });

        Assert.Throws<ArgumentException>(() => new CurrencyModelRuntime(invalid, ledger));
    }
}
