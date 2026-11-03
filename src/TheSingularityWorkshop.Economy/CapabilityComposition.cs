namespace TheSingularityWorkshop.Economy;

/// <summary>Stable, provider-neutral identity for an independently selectable Experience capability.</summary>
public readonly record struct CapabilityId
{
    public CapabilityId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A capability identifier is required.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 128 ||
            normalized[0] == '.' ||
            normalized[^1] == '.' ||
            normalized.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_')))
            throw new ArgumentException("Capability identifiers use 1-128 lowercase ASCII letters, digits, dots, hyphens, or underscores.", nameof(value));

        Value = normalized;
    }

    public string Value { get; }
    public override string ToString() => Value;
}

/// <summary>How mature a capability implementation is; maturity is descriptive, not an activation permission.</summary>
public enum CapabilityMaturity
{
    Concept,
    Experimental,
    Preview,
    Stable
}

/// <summary>Value shape declared by a capability configuration field.</summary>
public enum CapabilityConfigurationValueType
{
    String,
    Boolean,
    Integer,
    Decimal,
    Enumeration
}

/// <summary>Describes one configuration value accepted by a capability.</summary>
public sealed record CapabilityConfigurationField
{
    private readonly IReadOnlyList<string> _allowedValues;

    public CapabilityConfigurationField(
        string key,
        CapabilityConfigurationValueType valueType,
        bool required = false,
        IEnumerable<string>? allowedValues = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("A configuration key is required.", nameof(key));
        if (!Enum.IsDefined(valueType))
            throw new ArgumentOutOfRangeException(nameof(valueType));

        var values = (allowedValues ?? Array.Empty<string>()).ToArray();
        if (values.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Allowed values cannot contain blank entries.", nameof(allowedValues));
        if (values.Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException("Allowed values must be unique.", nameof(allowedValues));
        if (valueType == CapabilityConfigurationValueType.Enumeration && values.Length == 0)
            throw new ArgumentException("Enumeration fields require at least one allowed value.", nameof(allowedValues));
        if (valueType != CapabilityConfigurationValueType.Enumeration && values.Length != 0)
            throw new ArgumentException("Allowed values are supported only for enumeration fields.", nameof(allowedValues));

        Key = key.Trim();
        ValueType = valueType;
        Required = required;
        _allowedValues = Array.AsReadOnly(values);
    }

    public string Key { get; }
    public CapabilityConfigurationValueType ValueType { get; }
    public bool Required { get; }
    public IReadOnlyList<string> AllowedValues => _allowedValues;
}

/// <summary>
/// Descriptive metadata for an independently selectable Experience capability.
/// A descriptor is not executable code and does not grant authority to activate or perform side effects.
/// </summary>
public sealed record CapabilityDescriptor
{
    private readonly IReadOnlyList<CapabilityId> _dependencies;
    private readonly IReadOnlyList<CapabilityId> _conflicts;
    private readonly IReadOnlyList<CapabilityConfigurationField> _configuration;

    public CapabilityDescriptor(
        CapabilityId id,
        string name,
        string category,
        string description,
        string version,
        CapabilityMaturity maturity,
        IEnumerable<CapabilityId>? dependencies = null,
        IEnumerable<CapabilityId>? conflicts = null,
        IEnumerable<CapabilityConfigurationField>? configuration = null,
        string? documentationReference = null)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
            throw new ArgumentException("A capability identifier is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A capability name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("A capability category is required.", nameof(category));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A capability description is required.", nameof(description));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("A capability descriptor version is required.", nameof(version));
        if (!Enum.IsDefined(maturity))
            throw new ArgumentOutOfRangeException(nameof(maturity));

        var dependencySnapshot = (dependencies ?? Array.Empty<CapabilityId>()).ToArray();
        var conflictSnapshot = (conflicts ?? Array.Empty<CapabilityId>()).ToArray();
        var configurationSnapshot = (configuration ?? Array.Empty<CapabilityConfigurationField>()).ToArray();

        if (dependencySnapshot.Any(item => string.IsNullOrWhiteSpace(item.Value)) ||
            dependencySnapshot.Distinct().Count() != dependencySnapshot.Length)
            throw new ArgumentException("Dependencies must be valid and unique.", nameof(dependencies));
        if (conflictSnapshot.Any(item => string.IsNullOrWhiteSpace(item.Value)) ||
            conflictSnapshot.Distinct().Count() != conflictSnapshot.Length)
            throw new ArgumentException("Conflicts must be valid and unique.", nameof(conflicts));
        if (dependencySnapshot.Contains(id))
            throw new ArgumentException("A capability cannot depend on itself.", nameof(dependencies));
        if (conflictSnapshot.Contains(id))
            throw new ArgumentException("A capability cannot conflict with itself.", nameof(conflicts));
        if (dependencySnapshot.Intersect(conflictSnapshot).Any())
            throw new ArgumentException("A capability cannot both require and conflict with the same capability.", nameof(conflicts));
        if (configurationSnapshot.Any(field => field is null) ||
            configurationSnapshot.Select(field => field.Key).Distinct(StringComparer.Ordinal).Count() != configurationSnapshot.Length)
            throw new ArgumentException("Configuration fields must be non-null and have unique keys.", nameof(configuration));

        Id = id;
        Name = name.Trim();
        Category = category.Trim();
        Description = description.Trim();
        Version = version.Trim();
        Maturity = maturity;
        _dependencies = Array.AsReadOnly(dependencySnapshot);
        _conflicts = Array.AsReadOnly(conflictSnapshot);
        _configuration = Array.AsReadOnly(configurationSnapshot);
        DocumentationReference = string.IsNullOrWhiteSpace(documentationReference) ? null : documentationReference.Trim();
    }

    public CapabilityId Id { get; }
    public string Name { get; }
    public string Category { get; }
    public string Description { get; }
    public string Version { get; }
    public CapabilityMaturity Maturity { get; }
    public IReadOnlyList<CapabilityId> Dependencies => _dependencies;
    public IReadOnlyList<CapabilityId> Conflicts => _conflicts;
    public IReadOnlyList<CapabilityConfigurationField> Configuration => _configuration;
    public string? DocumentationReference { get; }
}

/// <summary>In-memory, immutable-snapshot catalog of capability descriptors.</summary>
public sealed class CapabilityCatalog
{
    private readonly IReadOnlyDictionary<CapabilityId, CapabilityDescriptor> _descriptors;

    public CapabilityCatalog(IEnumerable<CapabilityDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        var snapshot = descriptors.ToArray();
        if (snapshot.Any(descriptor => descriptor is null))
            throw new ArgumentException("A catalog cannot contain null descriptors.", nameof(descriptors));
        if (snapshot.Select(descriptor => descriptor.Id).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Capability identifiers must be unique in a catalog.", nameof(descriptors));

        var byId = snapshot.ToDictionary(descriptor => descriptor.Id);
        foreach (var descriptor in snapshot)
        {
            var unresolved = descriptor.Dependencies.Concat(descriptor.Conflicts)
                .FirstOrDefault(reference => !byId.ContainsKey(reference));
            if (!string.IsNullOrWhiteSpace(unresolved.Value))
                throw new ArgumentException(
                    $"Capability '{descriptor.Id}' references '{unresolved}', which is not present in this catalog.",
                    nameof(descriptors));
        }

        _descriptors = new System.Collections.ObjectModel.ReadOnlyDictionary<CapabilityId, CapabilityDescriptor>(byId);
    }

    public IReadOnlyCollection<CapabilityDescriptor> Descriptors => _descriptors.Values.ToArray();

    public bool TryGet(CapabilityId id, out CapabilityDescriptor? descriptor) =>
        _descriptors.TryGetValue(id, out descriptor);
}

/// <summary>A selected capability and its Experience-owned configuration values.</summary>
public sealed record CapabilitySelection
{
    private readonly IReadOnlyDictionary<string, string> _configuration;

    public CapabilitySelection(CapabilityId capabilityId, IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        if (string.IsNullOrWhiteSpace(capabilityId.Value))
            throw new ArgumentException("A capability identifier is required.", nameof(capabilityId));

        var values = (configuration ?? Array.Empty<KeyValuePair<string, string>>()).ToArray();
        if (values.Any(pair => string.IsNullOrWhiteSpace(pair.Key) || pair.Value is null))
            throw new ArgumentException("Configuration keys must be nonblank and values cannot be null.", nameof(configuration));
        if (values.Select(pair => pair.Key).Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException("Configuration keys must be unique.", nameof(configuration));

        CapabilityId = capabilityId;
        _configuration = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
            values.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
    }

    public CapabilityId CapabilityId { get; }
    public IReadOnlyDictionary<string, string> Configuration => _configuration;
}

/// <summary>One validation finding for an Experience capability composition.</summary>
public sealed record CapabilityCompositionIssue(string Code, CapabilityId CapabilityId, string Message);

/// <summary>Result of validating an Experience's selected capability set.</summary>
public sealed record CapabilityCompositionValidation
{
    public CapabilityCompositionValidation(IEnumerable<CapabilityCompositionIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        Issues = Array.AsReadOnly(issues.ToArray());
    }

    public IReadOnlyList<CapabilityCompositionIssue> Issues { get; }
    public bool IsValid => Issues.Count == 0;
}

/// <summary>
/// Immutable declaration of the optional capabilities selected by one Experience.
/// This validates declarative dependencies and configuration only; it does not activate capabilities.
/// </summary>
public sealed class ExperienceCapabilityComposition
{
    private readonly IReadOnlyList<CapabilitySelection> _selections;

    public ExperienceCapabilityComposition(IEnumerable<CapabilitySelection>? selections = null)
    {
        var snapshot = (selections ?? Array.Empty<CapabilitySelection>()).ToArray();
        if (snapshot.Any(selection => selection is null))
            throw new ArgumentException("A composition cannot contain null selections.", nameof(selections));
        if (snapshot.Select(selection => selection.CapabilityId).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("A capability can be selected only once in a composition.", nameof(selections));
        _selections = Array.AsReadOnly(snapshot);
    }

    public IReadOnlyList<CapabilitySelection> Selections => _selections;

    public CapabilityCompositionValidation Validate(CapabilityCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var issues = new List<CapabilityCompositionIssue>();
        var selected = _selections.ToDictionary(selection => selection.CapabilityId);

        foreach (var selection in _selections)
        {
            if (!catalog.TryGet(selection.CapabilityId, out var descriptor) || descriptor is null)
            {
                issues.Add(new CapabilityCompositionIssue("unknown-capability", selection.CapabilityId,
                    $"Capability '{selection.CapabilityId}' is not present in the catalog."));
                continue;
            }

            foreach (var dependency in descriptor.Dependencies)
            {
                if (!selected.ContainsKey(dependency))
                    issues.Add(new CapabilityCompositionIssue("missing-dependency", descriptor.Id,
                        $"Capability '{descriptor.Id}' requires '{dependency}'."));
            }

            foreach (var conflict in descriptor.Conflicts)
            {
                if (selected.ContainsKey(conflict))
                    issues.Add(new CapabilityCompositionIssue("conflicting-capability", descriptor.Id,
                        $"Capability '{descriptor.Id}' conflicts with '{conflict}'."));
            }

            var fields = descriptor.Configuration.ToDictionary(field => field.Key, StringComparer.Ordinal);
            foreach (var field in descriptor.Configuration.Where(field => field.Required))
            {
                if (!selection.Configuration.ContainsKey(field.Key))
                    issues.Add(new CapabilityCompositionIssue("missing-configuration", descriptor.Id,
                        $"Required configuration '{field.Key}' is missing."));
            }

            foreach (var pair in selection.Configuration)
            {
                if (!fields.TryGetValue(pair.Key, out var field))
                {
                    issues.Add(new CapabilityCompositionIssue("unknown-configuration", descriptor.Id,
                        $"Configuration '{pair.Key}' is not declared by capability '{descriptor.Id}'."));
                    continue;
                }

                if (!IsValidValue(field, pair.Value))
                    issues.Add(new CapabilityCompositionIssue("invalid-configuration", descriptor.Id,
                        $"Configuration '{pair.Key}' is not a valid {field.ValueType} value."));
            }
        }

        return new CapabilityCompositionValidation(issues);
    }

    private static bool IsValidValue(CapabilityConfigurationField field, string value)
    {
        return field.ValueType switch
        {
            CapabilityConfigurationValueType.String => true,
            CapabilityConfigurationValueType.Boolean => bool.TryParse(value, out _),
            CapabilityConfigurationValueType.Integer => long.TryParse(value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out _),
            CapabilityConfigurationValueType.Decimal => decimal.TryParse(value, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out _),
            CapabilityConfigurationValueType.Enumeration => field.AllowedValues.Contains(value, StringComparer.Ordinal),
            _ => false
        };
    }
}
