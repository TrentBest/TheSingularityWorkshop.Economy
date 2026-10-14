namespace TheSingularityWorkshop.Economy;

/// <summary>
/// Identifies the environment that reported an economic operation outcome.
/// </summary>
/// <remarks>
/// A simulation result is not a real-money provider result. This value describes
/// the reporting environment; it is not an independent verification of that environment.
/// </remarks>
public enum EconomicExecutionEnvironment
{
    Simulation,
    ExternalProvider
}

/// <summary>
/// Describes the observed state of an attempted economic operation.
/// </summary>
/// <remarks>
/// <see cref="Accepted"/> and <see cref="Pending"/> do not mean the operation completed.
/// <see cref="Completed"/> reports completion in the named execution environment; it does
/// not, by itself, prove provider settlement, reconciliation, or legal finality.
/// <see cref="Unknown"/> represents an outcome that cannot currently be determined.
/// </remarks>
public enum EconomicExecutionStatus
{
    Accepted,
    Pending,
    Completed,
    Failed,
    Cancelled,
    Expired,
    Unknown,
    Reversed
}

/// <summary>
/// Immutable description of an execution observation for a transfer intent.
/// </summary>
/// <remarks>
/// This type records a reported outcome; it does not execute transfers, enforce idempotency,
/// validate provider messages, maintain balances, or establish settlement. The caller must
/// supply an operation identifier and the identifier of the related <see cref="EconomyTransfer"/>.
/// External references are opaque correlation values, not credentials.
/// </remarks>
public sealed record EconomicExecutionResult
{
    /// <summary>
    /// Creates an immutable execution observation.
    /// </summary>
    /// <param name="operationId">Stable identifier assigned by the orchestration boundary.</param>
    /// <param name="transferId">Identifier of the transfer intent this observation concerns.</param>
    /// <param name="environment">Environment reporting this observation.</param>
    /// <param name="status">Observed operation status.</param>
    /// <param name="observedAt">Time the observation was recorded.</param>
    /// <param name="externalReference">Optional opaque reference assigned by an external system.</param>
    /// <exception cref="ArgumentException">An identifier is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An environment or status value is undefined.</exception>
    public EconomicExecutionResult(
        Guid operationId,
        Guid transferId,
        EconomicExecutionEnvironment environment,
        EconomicExecutionStatus status,
        DateTimeOffset observedAt,
        string? externalReference = null)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("An operation identifier is required.", nameof(operationId));
        }

        if (transferId == Guid.Empty)
        {
            throw new ArgumentException("A transfer identifier is required.", nameof(transferId));
        }

        if (!Enum.IsDefined(environment))
        {
            throw new ArgumentOutOfRangeException(nameof(environment), environment, "The execution environment is not defined.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "The execution status is not defined.");
        }

        OperationId = operationId;
        TransferId = transferId;
        Environment = environment;
        Status = status;
        ObservedAt = observedAt;
        ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
    }

    /// <summary>Stable identifier for the orchestration-level operation.</summary>
    public Guid OperationId { get; }

    /// <summary>Identifier of the transfer intent associated with this observation.</summary>
    public Guid TransferId { get; }

    /// <summary>Environment that reported the observation.</summary>
    public EconomicExecutionEnvironment Environment { get; }

    /// <summary>Observed status; not a guarantee of settlement or finality.</summary>
    public EconomicExecutionStatus Status { get; }

    /// <summary>Timestamp at which this observation was recorded.</summary>
    public DateTimeOffset ObservedAt { get; }

    /// <summary>Optional opaque external correlation reference; never store credentials here.</summary>
    public string? ExternalReference { get; }
}
