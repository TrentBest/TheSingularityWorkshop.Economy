namespace TheSingularityWorkshop.Economy;

/// <summary>Describes an intended movement of value; it does not execute or settle a payment.</summary>
public sealed record EconomyTransfer
{
    /// <summary>Creates a transfer intent whose accounts and amount use the same currency.</summary>
    /// <param name="Id">Stable identifier for this transfer intent.</param>
    /// <param name="From">Account from which value is intended to move.</param>
    /// <param name="To">Account to which value is intended to move.</param>
    /// <param name="Amount">Non-negative amount intended to move.</param>
    /// <param name="Kind">Domain classification of the transfer.</param>
    /// <param name="Reference">Optional caller-defined reference.</param>
    /// <param name="CreatedAt">Optional timestamp; UTC now is used when omitted.</param>
    /// <exception cref="ArgumentNullException">An account is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The amount is negative.</exception>
    /// <exception cref="InvalidOperationException">The amount and accounts use different currencies.</exception>
    public EconomyTransfer(
        Guid Id,
        EconomyAccount From,
        EconomyAccount To,
        Money Amount,
        TransferKind Kind,
        string? Reference = null,
        DateTimeOffset? CreatedAt = null)
    {
        ArgumentNullException.ThrowIfNull(From);
        ArgumentNullException.ThrowIfNull(To);

        if (Amount.Amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(Amount), "A transfer intent cannot move a negative amount.");
        }

        if (!string.Equals(From.Currency, To.Currency, StringComparison.Ordinal) ||
            !string.Equals(From.Currency, Amount.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Transfer currency must match both accounts. From: {From.Currency}; To: {To.Currency}; Amount: {Amount.Currency}.");
        }

        this.Id = Id;
        this.From = From;
        this.To = To;
        this.Amount = Amount;
        this.Kind = Kind;
        this.Reference = Reference;
        this.CreatedAt = CreatedAt;
        Timestamp = CreatedAt ?? DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public EconomyAccount From { get; }
    public EconomyAccount To { get; }
    public Money Amount { get; }
    public TransferKind Kind { get; }
    public string? Reference { get; }
    public DateTimeOffset? CreatedAt { get; }

    /// <summary>Effective timestamp for this intent, not evidence of settlement.</summary>
    public DateTimeOffset Timestamp { get; }
}