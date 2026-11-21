namespace TheSingularityWorkshop.Economy;

/// <summary>Identifies the account roles in the deterministic reference market scenario.</summary>
public enum MarketEconomyAccountRole
{
    CentralBankTreasury,
    CommercialBankClearing,
    CommercialBankReserves,
    EmployerTreasury,
    HouseholdDeposit,
    MerchantTreasury
}

/// <summary>Classifies an auditable event in the reference market scenario.</summary>
public enum MarketEconomyEventKind
{
    CurrencyIssued,
    BusinessCapitalized,
    HouseholdEndowed,
    WageAcceptedForClearing,
    WageSettled,
    BankFeePosted,
    PurchaseSettled
}

/// <summary>
/// One scenario event linked to the authoritative simulated-ledger receipt.
/// These events describe a closed fictional economy; they are not external payment records.
/// </summary>
public sealed record MarketEconomyScenarioEvent(
    Guid OperationId,
    MarketEconomyEventKind Kind,
    SimulatedAmount Amount,
    DateTimeOffset OccurredAt,
    SimulationLedgerReceipt LedgerReceipt);

/// <summary>
/// Result of the deterministic multi-institution market scenario.
/// Account balances are snapshots derived from the SimulationLedger after all events.
/// </summary>
public sealed record MarketEconomyScenarioResult(
    SimulatedCurrency Currency,
    IReadOnlyDictionary<MarketEconomyAccountRole, decimal> Balances,
    IReadOnlyList<MarketEconomyScenarioEvent> Events,
    decimal TotalIssued);

/// <summary>
/// Runs a small, closed, fictional economy using the existing SimulationLedger.
/// It demonstrates issuance, business capitalization, a bank-mediated wage flow,
/// a bank fee, and a household purchase. It deliberately does not model lending,
/// deposit creation, securities, external payments, or real-world settlement.
/// </summary>
public static class MarketEconomyScenario
{
    private static readonly DateTimeOffset ScenarioTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly Guid CurrencyGuid = Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly IReadOnlyDictionary<MarketEconomyAccountRole, Guid> AccountIds =
        new Dictionary<MarketEconomyAccountRole, Guid>
        {
            [MarketEconomyAccountRole.CentralBankTreasury] = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            [MarketEconomyAccountRole.CommercialBankClearing] = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            [MarketEconomyAccountRole.CommercialBankReserves] = Guid.Parse("20000000-0000-0000-0000-000000000003"),
            [MarketEconomyAccountRole.EmployerTreasury] = Guid.Parse("20000000-0000-0000-0000-000000000004"),
            [MarketEconomyAccountRole.HouseholdDeposit] = Guid.Parse("20000000-0000-0000-0000-000000000005"),
            [MarketEconomyAccountRole.MerchantTreasury] = Guid.Parse("20000000-0000-0000-0000-000000000006")
        };

    private static readonly IReadOnlyDictionary<MarketEconomyAccountRole, Guid> OwnerIds =
        new Dictionary<MarketEconomyAccountRole, Guid>
        {
            [MarketEconomyAccountRole.CentralBankTreasury] = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            [MarketEconomyAccountRole.CommercialBankClearing] = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            [MarketEconomyAccountRole.CommercialBankReserves] = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            [MarketEconomyAccountRole.EmployerTreasury] = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            [MarketEconomyAccountRole.HouseholdDeposit] = Guid.Parse("30000000-0000-0000-0000-000000000004"),
            [MarketEconomyAccountRole.MerchantTreasury] = Guid.Parse("30000000-0000-0000-0000-000000000005")
        };

    /// <summary>Executes the same scenario with stable identities, ordering, and virtual timestamps.</summary>
    public static MarketEconomyScenarioResult Run()
    {
        var currency = new SimulatedCurrency(
            new SimulatedCurrencyId(CurrencyGuid),
            "Market Simulation Unit",
            "MSU",
            decimalPlaces: 2);

        var ledger = new SimulationLedger();
        foreach (var role in Enum.GetValues<MarketEconomyAccountRole>())
        {
            ledger.RegisterAccount(new SimulationAccount(
                AccountIds[role],
                new EconomyEntityId(OwnerIds[role]),
                currency));
        }

        var events = new List<MarketEconomyScenarioEvent>();
        var totalIssued = new SimulatedAmount(10_000m, currency.Id);

        AddIssue(
            ledger, events,
            Guid.Parse("40000000-0000-0000-0000-000000000001"),
            MarketEconomyAccountRole.CentralBankTreasury,
            totalIssued,
            MarketEconomyEventKind.CurrencyIssued);

        AddTransfer(
            ledger, events,
            Guid.Parse("40000000-0000-0000-0000-000000000002"),
            MarketEconomyAccountRole.CentralBankTreasury,
            MarketEconomyAccountRole.EmployerTreasury,
            new SimulatedAmount(500m, currency.Id),
            MarketEconomyEventKind.BusinessCapitalized);

        AddTransfer(
            ledger, events,
            Guid.Parse("40000000-0000-0000-0000-000000000003"),
            MarketEconomyAccountRole.CentralBankTreasury,
            MarketEconomyAccountRole.HouseholdDeposit,
            new SimulatedAmount(100m, currency.Id),
            MarketEconomyEventKind.HouseholdEndowed);

        AddTransfer(
            ledger, events,
            Guid.Parse("40000000-0000-0000-0000-000000000004"),
            MarketEconomyAccountRole.EmployerTreasury,
            MarketEconomyAccountRole.CommercialBankClearing,
            new SimulatedAmount(40m, currency.Id),
            MarketEconomyEventKind.WageAcceptedForClearing);

        AddTransfer(
            ledger, events,
            Guid.Parse("40000000-0000-0000-0000-000000000005"),
            MarketEconomyAccountRole.CommercialBankClearing,
            MarketEconomyAccountRole.HouseholdDeposit,
            new SimulatedAmount(40m, currency.Id),
            MarketEconomyEventKind.WageSettled);

        AddTransfer(
            ledger, events,
            Guid.Parse("40000000-0000-0000-0000-000000000006"),
            MarketEconomyAccountRole.EmployerTreasury,
            MarketEconomyAccountRole.CommercialBankReserves,
            new SimulatedAmount(2m, currency.Id),
            MarketEconomyEventKind.BankFeePosted);

        AddTransfer(
            ledger, events,
            Guid.Parse("40000000-0000-0000-0000-000000000007"),
            MarketEconomyAccountRole.HouseholdDeposit,
            MarketEconomyAccountRole.MerchantTreasury,
            new SimulatedAmount(25m, currency.Id),
            MarketEconomyEventKind.PurchaseSettled);

        var balances = Enum.GetValues<MarketEconomyAccountRole>()
            .ToDictionary(role => role, role => ledger.GetBalance(AccountIds[role]));

        return new MarketEconomyScenarioResult(
            currency,
            new System.Collections.ObjectModel.ReadOnlyDictionary<MarketEconomyAccountRole, decimal>(balances),
            Array.AsReadOnly(events.ToArray()),
            totalIssued.Value);
    }

    private static void AddIssue(
        SimulationLedger ledger,
        ICollection<MarketEconomyScenarioEvent> events,
        Guid operationId,
        MarketEconomyAccountRole account,
        SimulatedAmount amount,
        MarketEconomyEventKind kind)
    {
        var receipt = ledger.Issue(operationId, AccountIds[account], amount, ScenarioTime);
        events.Add(new MarketEconomyScenarioEvent(operationId, kind, amount, ScenarioTime, receipt));
    }

    private static void AddTransfer(
        SimulationLedger ledger,
        ICollection<MarketEconomyScenarioEvent> events,
        Guid operationId,
        MarketEconomyAccountRole from,
        MarketEconomyAccountRole to,
        SimulatedAmount amount,
        MarketEconomyEventKind kind)
    {
        var receipt = ledger.Transfer(operationId, AccountIds[from], AccountIds[to], amount, ScenarioTime);
        events.Add(new MarketEconomyScenarioEvent(operationId, kind, amount, ScenarioTime, receipt));
    }
}
