using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class MarketEconomyScenarioTests
{
    [Fact]
    public void ScenarioRunsAClosedMultiInstitutionEconomy()
    {
        var result = MarketEconomyScenario.Run();

        Assert.Equal(7, result.Events.Count);
        Assert.Equal(10_000m, result.TotalIssued);
        Assert.Equal(9_400m, result.Balances[MarketEconomyAccountRole.CentralBankTreasury]);
        Assert.Equal(0m, result.Balances[MarketEconomyAccountRole.CommercialBankClearing]);
        Assert.Equal(2m, result.Balances[MarketEconomyAccountRole.CommercialBankReserves]);
        Assert.Equal(458m, result.Balances[MarketEconomyAccountRole.EmployerTreasury]);
        Assert.Equal(115m, result.Balances[MarketEconomyAccountRole.HouseholdDeposit]);
        Assert.Equal(25m, result.Balances[MarketEconomyAccountRole.MerchantTreasury]);
    }

    [Fact]
    public void ScenarioConservesIssuedValueAcrossAllAccounts()
    {
        var result = MarketEconomyScenario.Run();

        Assert.Equal(result.TotalIssued, result.Balances.Values.Sum());
    }

    [Fact]
    public void WageFlowSeparatesClearingAcceptanceFromFinalHouseholdCredit()
    {
        var result = MarketEconomyScenario.Run();
        var accepted = Assert.Single(result.Events.Where(
            item => item.Kind == MarketEconomyEventKind.WageAcceptedForClearing));
        var settled = Assert.Single(result.Events.Where(
            item => item.Kind == MarketEconomyEventKind.WageSettled));

        Assert.Equal(40m, accepted.Amount.Value);
        Assert.Equal(40m, settled.Amount.Value);
        Assert.Equal(accepted.OperationId, accepted.LedgerReceipt.OperationId);
        Assert.Equal(settled.OperationId, settled.LedgerReceipt.OperationId);
        Assert.NotEqual(accepted.OperationId, settled.OperationId);
    }

    [Fact]
    public void ScenarioIsDeterministicAcrossIndependentRuns()
    {
        var first = MarketEconomyScenario.Run();
        var second = MarketEconomyScenario.Run();

        Assert.Equal(first.Currency, second.Currency);
        Assert.Equal(first.Balances, second.Balances);
        Assert.Equal(
            first.Events.Select(item => (item.OperationId, item.Kind, item.Amount, item.OccurredAt)),
            second.Events.Select(item => (item.OperationId, item.Kind, item.Amount, item.OccurredAt)));
        Assert.Equal(
            first.Events.Select(item => item.LedgerReceipt),
            second.Events.Select(item => item.LedgerReceipt));
    }

    [Fact]
    public void ScenarioEventsAreLinkedToLedgerReceipts()
    {
        var result = MarketEconomyScenario.Run();

        Assert.All(result.Events, item =>
        {
            Assert.Equal(item.OperationId, item.LedgerReceipt.OperationId);
            Assert.Equal(item.Amount, item.LedgerReceipt.Amount);
            Assert.False(item.LedgerReceipt.IsReplay);
        });

        Assert.Equal(
            result.Events.Count,
            result.Events.Select(item => item.OperationId).Distinct().Count());
    }
}
