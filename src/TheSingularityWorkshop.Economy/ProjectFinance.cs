namespace TheSingularityWorkshop.Economy;

/// <summary>Stable identity for a capital or delivery project.</summary>
public readonly record struct CapitalProjectId(Guid Value)
{
    public static CapitalProjectId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for a project budget/cost line.</summary>
public readonly record struct ProjectCostLineId(Guid Value)
{
    public static ProjectCostLineId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for a project funding source.</summary>
public readonly record struct ProjectFundingSourceId(Guid Value)
{
    public static ProjectFundingSourceId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for a funding commitment.</summary>
public readonly record struct ProjectFundingCommitmentId(Guid Value)
{
    public static ProjectFundingCommitmentId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for a recognized project cost record.</summary>
public readonly record struct ProjectCostRecordId(Guid Value)
{
    public static ProjectCostRecordId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for a project invoice.</summary>
public readonly record struct ProjectInvoiceId(Guid Value)
{
    public static ProjectInvoiceId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for a simulated project payment record.</summary>
public readonly record struct ProjectPaymentId(Guid Value)
{
    public static ProjectPaymentId New() => new(Guid.NewGuid());
}

/// <summary>Broad cost classification; project-specific meaning remains Experience-owned.</summary>
public enum ProjectCostCategory
{
    LandAndSite,
    DesignAndResearch,
    Labor,
    Materials,
    Equipment,
    ConstructionAndInstallation,
    Logistics,
    SoftwareAndData,
    OperationsAndMaintenance,
    Financing,
    TaxesAndDuties,
    Contingency,
    Other
}

/// <summary>Origin of a project's funding; these labels do not establish legal or financial rights.</summary>
public enum ProjectFundingSourceKind
{
    SponsorContribution,
    InternalAllocation,
    Grant,
    Donation,
    Debt,
    OperatingRevenue,
    InKindContribution,
    PublicAllocation
}

/// <summary>Recorded project lifecycle state, not an authorization decision.</summary>
public enum CapitalProjectStatus
{
    Proposed,
    Feasibility,
    Approved,
    Active,
    OnHold,
    Completed,
    Cancelled,
    Closed
}

/// <summary>Validation state for a supplier or contractor invoice.</summary>
public enum ProjectInvoiceStatus
{
    Draft,
    Submitted,
    Validated,
    Disputed,
    Rejected,
    Cancelled
}

/// <summary>A project identity and its sponsor-owned scope description.</summary>
public sealed record CapitalProject
{
    public CapitalProject(
        CapitalProjectId id,
        EconomyEntityId sponsor,
        string name,
        string scope,
        DateTimeOffset createdAt)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A project identifier is required.", nameof(id));
        if (sponsor.Value == Guid.Empty)
            throw new ArgumentException("A project sponsor reference is required.", nameof(sponsor));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        Id = id;
        Sponsor = sponsor;
        Name = name.Trim();
        Scope = scope.Trim();
        CreatedAt = createdAt;
        Status = CapitalProjectStatus.Proposed;
    }

    public CapitalProjectId Id { get; }
    public EconomyEntityId Sponsor { get; }
    public string Name { get; }
    public string Scope { get; }
    public DateTimeOffset CreatedAt { get; }
    public CapitalProjectStatus Status { get; }
}

/// <summary>A single authorized or proposed allocation in a project budget baseline.</summary>
public sealed record ProjectBudgetLine
{
    public ProjectBudgetLine(
        ProjectCostLineId id,
        string name,
        ProjectCostCategory category,
        SimulatedAmount amount,
        string estimateBasis)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A budget-line identifier is required.", nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(estimateBasis);

        Id = id;
        Name = name.Trim();
        Category = category;
        Amount = amount;
        EstimateBasis = estimateBasis.Trim();
    }

    public ProjectCostLineId Id { get; }
    public string Name { get; }
    public ProjectCostCategory Category { get; }
    public SimulatedAmount Amount { get; }
    public string EstimateBasis { get; }
}

/// <summary>
/// Immutable project budget snapshot. Approval is a recorded decision reference, not host authorization.
/// A revised baseline is a new version; prior snapshots remain unchanged.
/// </summary>
public sealed record ProjectBudgetBaseline
{
    private ProjectBudgetBaseline(
        CapitalProjectId projectId,
        int version,
        IReadOnlyList<ProjectBudgetLine> lines,
        SimulatedCurrencyId currencyId,
        DateTimeOffset? approvedAt,
        Guid? approvalReference)
    {
        ProjectId = projectId;
        Version = version;
        Lines = lines;
        CurrencyId = currencyId;
        ApprovedAt = approvedAt;
        ApprovalReference = approvalReference;
    }

    public CapitalProjectId ProjectId { get; }
    public int Version { get; }
    public IReadOnlyList<ProjectBudgetLine> Lines { get; }
    public SimulatedCurrencyId CurrencyId { get; }
    public DateTimeOffset? ApprovedAt { get; }
    public Guid? ApprovalReference { get; }
    public bool IsApproved => ApprovedAt.HasValue && ApprovalReference.HasValue;
    public decimal Total => Lines.Sum(line => line.Amount.Value);

    public static ProjectBudgetBaseline Propose(
        CapitalProjectId projectId,
        int version,
        IEnumerable<ProjectBudgetLine> lines)
    {
        if (projectId.Value == Guid.Empty)
            throw new ArgumentException("A project identifier is required.", nameof(projectId));
        if (version < 1)
            throw new ArgumentOutOfRangeException(nameof(version));

        ArgumentNullException.ThrowIfNull(lines);
        var snapshot = lines.ToArray();
        if (snapshot.Length == 0)
            throw new ArgumentException("A budget baseline requires at least one line.", nameof(lines));
        if (snapshot.Any(line => line is null))
            throw new ArgumentException("A budget baseline cannot contain null lines.", nameof(lines));
        if (snapshot.Select(line => line.Id).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Budget-line identifiers must be unique.", nameof(lines));

        var currencyId = snapshot[0].Amount.CurrencyId;
        if (snapshot.Any(line => line.Amount.CurrencyId != currencyId))
            throw new InvalidOperationException("A budget baseline cannot mix simulated currencies.");

        return new ProjectBudgetBaseline(
            projectId, version, Array.AsReadOnly(snapshot), currencyId, null, null);
    }

    /// <summary>Returns an approved snapshot while preserving the original proposal.</summary>
    public ProjectBudgetBaseline Approve(DateTimeOffset approvedAt, Guid approvalReference)
    {
        if (approvalReference == Guid.Empty)
            throw new ArgumentException("An approval reference is required.", nameof(approvalReference));
        if (IsApproved)
            throw new InvalidOperationException("This budget baseline is already approved.");

        return new ProjectBudgetBaseline(
            ProjectId, Version, Lines, CurrencyId, approvedAt, approvalReference);
    }
}

/// <summary>A funding source with explicit currency and optional use restrictions.</summary>
public sealed record ProjectFundingSource
{
    public ProjectFundingSource(
        ProjectFundingSourceId id,
        string name,
        ProjectFundingSourceKind kind,
        SimulatedCurrencyId currencyId,
        string? restriction = null)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A funding-source identifier is required.", nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (currencyId.Value == Guid.Empty)
            throw new ArgumentException("A simulated currency identity is required.", nameof(currencyId));
        if (restriction is not null && string.IsNullOrWhiteSpace(restriction))
            throw new ArgumentException("A restriction must be meaningful or omitted.", nameof(restriction));

        Id = id;
        Name = name.Trim();
        Kind = kind;
        CurrencyId = currencyId;
        Restriction = restriction?.Trim();
    }

    public ProjectFundingSourceId Id { get; }
    public string Name { get; }
    public ProjectFundingSourceKind Kind { get; }
    public SimulatedCurrencyId CurrencyId { get; }
    public string? Restriction { get; }
    public bool IsRestricted => Restriction is not null;
}

/// <summary>
/// A promise or allocation of funding. It is not evidence that cash has been received.
/// Receipt must be represented separately and linked to a simulated ledger operation.
/// </summary>
public sealed record ProjectFundingCommitment
{
    public ProjectFundingCommitment(
        ProjectFundingCommitmentId id,
        CapitalProjectId projectId,
        ProjectFundingSource source,
        SimulatedAmount amount,
        DateTimeOffset committedAt,
        DateTimeOffset? expectedAt = null,
        bool isConditional = false)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A funding-commitment identifier is required.", nameof(id));
        if (projectId.Value == Guid.Empty)
            throw new ArgumentException("A project identifier is required.", nameof(projectId));
        ArgumentNullException.ThrowIfNull(source);
        if (amount.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "A funding commitment must be positive.");
        if (amount.CurrencyId != source.CurrencyId)
            throw new InvalidOperationException("Funding commitment currency must match its source.");
        if (expectedAt.HasValue && expectedAt.Value < committedAt)
            throw new ArgumentOutOfRangeException(nameof(expectedAt));

        Id = id;
        ProjectId = projectId;
        Source = source;
        Amount = amount;
        CommittedAt = committedAt;
        ExpectedAt = expectedAt;
        IsConditional = isConditional;
    }

    public ProjectFundingCommitmentId Id { get; }
    public CapitalProjectId ProjectId { get; }
    public ProjectFundingSource Source { get; }
    public SimulatedAmount Amount { get; }
    public DateTimeOffset CommittedAt { get; }
    public DateTimeOffset? ExpectedAt { get; }
    public bool IsConditional { get; }
}

/// <summary>
/// A recognized non-cash project cost. It does not create a ledger posting or prove payment.
/// </summary>
public sealed record ProjectCostRecord
{
    public ProjectCostRecord(
        ProjectCostRecordId id,
        CapitalProjectId projectId,
        ProjectCostLineId budgetLineId,
        SimulatedAmount amount,
        DateTimeOffset incurredAt,
        string evidenceReference)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A cost-record identifier is required.", nameof(id));
        if (projectId.Value == Guid.Empty)
            throw new ArgumentException("A project identifier is required.", nameof(projectId));
        if (budgetLineId.Value == Guid.Empty)
            throw new ArgumentException("A budget-line identifier is required.", nameof(budgetLineId));
        if (amount.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "A recognized cost must be positive.");
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceReference);

        Id = id;
        ProjectId = projectId;
        BudgetLineId = budgetLineId;
        Amount = amount;
        IncurredAt = incurredAt;
        EvidenceReference = evidenceReference.Trim();
    }

    public ProjectCostRecordId Id { get; }
    public CapitalProjectId ProjectId { get; }
    public ProjectCostLineId BudgetLineId { get; }
    public SimulatedAmount Amount { get; }
    public DateTimeOffset IncurredAt { get; }
    public string EvidenceReference { get; }
}

/// <summary>An invoice submitted against a recognized project cost.</summary>
public sealed record ProjectInvoice
{
    public ProjectInvoice(
        ProjectInvoiceId id,
        CapitalProjectId projectId,
        ProjectCostRecordId costRecordId,
        SimulatedAmount amount,
        string supplierReference,
        DateTimeOffset receivedAt,
        ProjectInvoiceStatus status = ProjectInvoiceStatus.Submitted)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("An invoice identifier is required.", nameof(id));
        if (projectId.Value == Guid.Empty)
            throw new ArgumentException("A project identifier is required.", nameof(projectId));
        if (costRecordId.Value == Guid.Empty)
            throw new ArgumentException("A cost-record reference is required.", nameof(costRecordId));
        if (amount.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "An invoice amount must be positive.");
        ArgumentException.ThrowIfNullOrWhiteSpace(supplierReference);

        Id = id;
        ProjectId = projectId;
        CostRecordId = costRecordId;
        Amount = amount;
        SupplierReference = supplierReference.Trim();
        ReceivedAt = receivedAt;
        Status = status;
    }

    public ProjectInvoiceId Id { get; }
    public CapitalProjectId ProjectId { get; }
    public ProjectCostRecordId CostRecordId { get; }
    public SimulatedAmount Amount { get; }
    public string SupplierReference { get; }
    public DateTimeOffset ReceivedAt { get; }
    public ProjectInvoiceStatus Status { get; }
}

/// <summary>
/// A simulated payment linked to the authoritative cash-ledger receipt. This record does not
/// initiate payment, validate a provider, or infer invoice approval.
/// </summary>
public sealed record ProjectPayment
{
    public ProjectPayment(
        ProjectPaymentId id,
        CapitalProjectId projectId,
        ProjectInvoiceId invoiceId,
        SimulatedAmount amount,
        DateTimeOffset paidAt,
        SimulationLedgerReceipt ledgerReceipt)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A payment identifier is required.", nameof(id));
        if (projectId.Value == Guid.Empty)
            throw new ArgumentException("A project identifier is required.", nameof(projectId));
        if (invoiceId.Value == Guid.Empty)
            throw new ArgumentException("An invoice identifier is required.", nameof(invoiceId));
        if (amount.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "A payment amount must be positive.");
        ArgumentNullException.ThrowIfNull(ledgerReceipt);
        if (ledgerReceipt.Kind != SimulationLedgerOperationKind.Transfer)
            throw new InvalidOperationException("A project payment must reference a simulated transfer receipt.");
        if (ledgerReceipt.Amount != amount)
            throw new InvalidOperationException("Payment amount must match the referenced ledger receipt.");
        if (ledgerReceipt.OperationId == Guid.Empty)
            throw new ArgumentException("A ledger operation identity is required.", nameof(ledgerReceipt));

        Id = id;
        ProjectId = projectId;
        InvoiceId = invoiceId;
        Amount = amount;
        PaidAt = paidAt;
        LedgerReceipt = ledgerReceipt;
    }

    public ProjectPaymentId Id { get; }
    public CapitalProjectId ProjectId { get; }
    public ProjectInvoiceId InvoiceId { get; }
    public SimulatedAmount Amount { get; }
    public DateTimeOffset PaidAt { get; }
    public SimulationLedgerReceipt LedgerReceipt { get; }
}
