namespace TheSingularityWorkshop.Economy;

/// <summary>Independently selectable economic behaviors for a fictional currency model.</summary>
public enum CurrencyModelAspect
{
    Issuance,
    EarningSources,
    Sinks,
    PeerTransfers,
    Expiry
}

/// <summary>A validation finding in a composed currency model.</summary>
public sealed record CurrencyModelIssue(string Code, string Message);

/// <summary>
/// Versioned, inspectable configuration for a fictional currency.
/// It composes policy choices around the existing SimulatedCurrency identity; it is not a ledger
/// and does not issue, transfer, redeem, or retire value by itself.
/// </summary>
public sealed record CurrencyModelDefinition
{
    private readonly IReadOnlyList<CurrencyModelAspect> _aspects;
    private readonly IReadOnlyList<string> _earningSources;
    private readonly IReadOnlyList<string> _sinks;

    public CurrencyModelDefinition(
        string modelId,
        int schemaVersion,
        SimulatedCurrency currency,
        IEnumerable<CurrencyModelAspect> aspects,
        string? issuanceAuthority = null,
        IEnumerable<string>? earningSources = null,
        IEnumerable<string>? sinks = null,
        int? expiryAfterDays = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        if (modelId.Length > 128 || modelId != modelId.Trim() ||
            modelId.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_')))
            throw new ArgumentException("Model identifiers use 1-128 ASCII letters, digits, dots, hyphens, or underscores without surrounding whitespace.", nameof(modelId));
        if (schemaVersion < 1)
            throw new ArgumentOutOfRangeException(nameof(schemaVersion));
        ArgumentNullException.ThrowIfNull(currency);
        ArgumentNullException.ThrowIfNull(aspects);

        var aspectSnapshot = aspects.ToArray();
        if (aspectSnapshot.Any(aspect => !Enum.IsDefined(aspect)))
            throw new ArgumentException("The model contains an unknown currency aspect.", nameof(aspects));
        if (aspectSnapshot.Distinct().Count() != aspectSnapshot.Length)
            throw new ArgumentException("Currency aspects must be unique.", nameof(aspects));

        var earningSnapshot = NormalizeIdentifiers(earningSources, nameof(earningSources));
        var sinkSnapshot = NormalizeIdentifiers(sinks, nameof(sinks));

        if (expiryAfterDays is <= 0)
            throw new ArgumentOutOfRangeException(nameof(expiryAfterDays));

        ModelId = modelId;
        SchemaVersion = schemaVersion;
        Currency = currency;
        _aspects = Array.AsReadOnly(aspectSnapshot);
        IssuanceAuthority = string.IsNullOrWhiteSpace(issuanceAuthority) ? null : issuanceAuthority.Trim();
        _earningSources = Array.AsReadOnly(earningSnapshot);
        _sinks = Array.AsReadOnly(sinkSnapshot);
        ExpiryAfterDays = expiryAfterDays;
    }

    /// <summary>Stable, creator-assigned identity for this model definition.</summary>
    public string ModelId { get; }
    /// <summary>Schema version, distinct from the currency identity.</summary>
    public int SchemaVersion { get; }
    /// <summary>The existing typed currency identity and presentation metadata.</summary>
    public SimulatedCurrency Currency { get; }
    public IReadOnlyList<CurrencyModelAspect> Aspects => _aspects;
    /// <summary>Identifier of the authority allowed to request issuance; not an authentication credential.</summary>
    public string? IssuanceAuthority { get; }
    public IReadOnlyList<string> EarningSources => _earningSources;
    public IReadOnlyList<string> Sinks => _sinks;
    public int? ExpiryAfterDays { get; }

    /// <summary>Fictional currency models never represent external redemption or real-world monetary value.</summary>
    public bool AllowsExternalRedemption => false;

    /// <summary>Checks whether the selected aspects have the configuration they require.</summary>
    public IReadOnlyList<CurrencyModelIssue> Validate()
    {
        var issues = new List<CurrencyModelIssue>();
        Require(CurrencyModelAspect.Issuance, !string.IsNullOrWhiteSpace(IssuanceAuthority),
            "missing-issuance-authority", "The issuance aspect requires an issuance authority identifier.");
        Require(CurrencyModelAspect.EarningSources, _earningSources.Count > 0,
            "missing-earning-source", "The earning-sources aspect requires at least one earning source.");
        Require(CurrencyModelAspect.Sinks, _sinks.Count > 0,
            "missing-sink", "The sinks aspect requires at least one sink.");
        Require(CurrencyModelAspect.Expiry, ExpiryAfterDays.HasValue,
            "missing-expiry-duration", "The expiry aspect requires a positive expiry duration.");

        if (!_aspects.Contains(CurrencyModelAspect.Expiry) && ExpiryAfterDays.HasValue)
            issues.Add(new CurrencyModelIssue("unexpected-expiry-duration",
                "An expiry duration was supplied without selecting the expiry aspect."));

        return Array.AsReadOnly(issues.ToArray());

        void Require(CurrencyModelAspect aspect, bool condition, string code, string message)
        {
            if (_aspects.Contains(aspect) && !condition)
                issues.Add(new CurrencyModelIssue(code, message));
        }
    }

    private static string[] NormalizeIdentifiers(IEnumerable<string>? values, string parameterName)
    {
        var snapshot = (values ?? Array.Empty<string>()).ToArray();
        if (snapshot.Any(value => string.IsNullOrWhiteSpace(value) || value != value.Trim()))
            throw new ArgumentException("Identifiers cannot be blank or contain surrounding whitespace.", parameterName);
        if (snapshot.Any(value => value.Length > 128))
            throw new ArgumentException("Identifiers cannot exceed 128 characters.", parameterName);
        if (snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
            throw new ArgumentException("Identifiers must be unique.", parameterName);
        return snapshot;
    }
}
