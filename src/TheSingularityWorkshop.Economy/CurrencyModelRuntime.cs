namespace TheSingularityWorkshop.Economy;

/// <summary>Classifies an executed operation under a currency model.</summary>
public enum CurrencyModelEventKind
{
    Issued,
    Transferred,
    Retired
}

/// <summary>
/// Inspectable evidence that a currency-model policy authorized a ledger operation.
/// The ledger receipt remains the authoritative record of the balance-changing operation.
/// </summary>
public sealed record CurrencyModelEvent(
    string ModelId,
    Guid OperationId,
    CurrencyModelEventKind Kind,
    SimulatedAmount Amount,
    DateTimeOffset OccurredAt,
    string? PolicyIdentifier,
    SimulationLedgerReceipt LedgerReceipt);

/// <summary>
/// Applies a validated currency model's selected policies to the existing simulation ledger.
/// This is an orchestration boundary, not a second ledger: all balances, atomicity, and
/// idempotency remain owned by <see cref="SimulationLedger"/>.
/// </summary>
public sealed class CurrencyModelRuntime
{
    private readonly CurrencyModelDefinition _definition;
    private readonly SimulationLedger _ledger;

    public CurrencyModelRuntime(CurrencyModelDefinition definition, SimulationLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(ledger);

        var issues = definition.Validate();
        if (issues.Count > 0)
            throw new ArgumentException(
                $"The currency model is invalid: {string.Join("; ", issues.Select(issue => issue.Code))}.",
                nameof(definition));

        _definition = definition;
        _ledger = ledger;
    }

    /// <summary>Issues value only when both the authority and earning source are selected by the model.</summary>
    public CurrencyModelEvent Issue(
        Guid operationId,
        Guid accountId,
        SimulatedAmount amount,
        string authorityIdentifier,
        string earningSourceIdentifier,
        DateTimeOffset occurredAt)
    {
        RequireAspect(CurrencyModelAspect.Issuance);
        RequireAspect(CurrencyModelAspect.EarningSources);
        RequireExactIdentifier(_definition.IssuanceAuthority, authorityIdentifier, "issuance authority");
        RequireAllowedIdentifier(_definition.EarningSources, earningSourceIdentifier, "earning source");

        var receipt = _ledger.Issue(operationId, accountId, amount, occurredAt);
        return new CurrencyModelEvent(
            _definition.ModelId, operationId, CurrencyModelEventKind.Issued, amount,
            occurredAt, earningSourceIdentifier, receipt);
    }

    /// <summary>Retires value only for a sink explicitly declared by the model.</summary>
    public CurrencyModelEvent Retire(
        Guid operationId,
        Guid accountId,
        SimulatedAmount amount,
        string sinkIdentifier,
        DateTimeOffset occurredAt)
    {
        RequireAspect(CurrencyModelAspect.Sinks);
        RequireAllowedIdentifier(_definition.Sinks, sinkIdentifier, "sink");

        var receipt = _ledger.Retire(operationId, accountId, amount, occurredAt);
        return new CurrencyModelEvent(
            _definition.ModelId, operationId, CurrencyModelEventKind.Retired, amount,
            occurredAt, sinkIdentifier, receipt);
    }

    /// <summary>Transfers value only when peer transfers are enabled in the model.</summary>
    public CurrencyModelEvent Transfer(
        Guid operationId,
        Guid fromAccountId,
        Guid toAccountId,
        SimulatedAmount amount,
        DateTimeOffset occurredAt)
    {
        RequireAspect(CurrencyModelAspect.PeerTransfers);

        var receipt = _ledger.Transfer(operationId, fromAccountId, toAccountId, amount, occurredAt);
        return new CurrencyModelEvent(
            _definition.ModelId, operationId, CurrencyModelEventKind.Transferred, amount,
            occurredAt, null, receipt);
    }

    private void RequireAspect(CurrencyModelAspect aspect)
    {
        if (!_definition.Aspects.Contains(aspect))
            throw new InvalidOperationException($"Currency model '{_definition.ModelId}' does not enable '{aspect}'.");
    }

    private static void RequireExactIdentifier(string? expected, string actual, string description)
    {
        if (string.IsNullOrWhiteSpace(actual) || !string.Equals(expected, actual, StringComparison.Ordinal))
            throw new InvalidOperationException($"The supplied {description} is not authorized by the currency model.");
    }

    private static void RequireAllowedIdentifier(
        IReadOnlyList<string> allowed,
        string actual,
        string description)
    {
        if (string.IsNullOrWhiteSpace(actual) || !allowed.Contains(actual, StringComparer.Ordinal))
            throw new InvalidOperationException($"The supplied {description} is not enabled by the currency model.");
    }
}
