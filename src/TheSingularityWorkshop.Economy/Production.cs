using System.Globalization;

namespace TheSingularityWorkshop.Economy;

/// <summary>Stable identity for a material, labor unit, machine-hour, or other production resource.</summary>
public readonly record struct ProductionResourceId
{
    public ProductionResourceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A production resource identifier is required.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 128 ||
            normalized[0] == '.' ||
            normalized[^1] == '.' ||
            normalized.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_')))
            throw new ArgumentException("Production resource identifiers use 1-128 lowercase ASCII letters, digits, dots, hyphens, or underscores.", nameof(value));

        Value = normalized;
    }

    public string Value { get; }
    public override string ToString() => Value;
}

/// <summary>A positive quantity of one identified production resource.</summary>
public sealed record ProductionResourceQuantity
{
    public ProductionResourceQuantity(ProductionResourceId resourceId, decimal quantity)
    {
        if (string.IsNullOrWhiteSpace(resourceId.Value))
            throw new ArgumentException("A production resource identifier is required.", nameof(resourceId));
        if (quantity <= 0m)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A production quantity must be positive.");

        ResourceId = resourceId;
        Quantity = quantity;
    }

    public ProductionResourceId ResourceId { get; }
    public decimal Quantity { get; }
}

/// <summary>
/// Immutable recipe describing required inputs and capacity, declared outputs, and recorded waste.
/// The recipe contains domain policy only; it does not execute or authorize production.
/// </summary>
public sealed class ProductionRecipe
{
    private readonly IReadOnlyList<ProductionResourceQuantity> _inputs;
    private readonly IReadOnlyList<ProductionResourceQuantity> _capacity;
    private readonly IReadOnlyList<ProductionResourceQuantity> _outputs;
    private readonly IReadOnlyList<ProductionResourceQuantity> _waste;

    public ProductionRecipe(
        string id,
        string version,
        IEnumerable<ProductionResourceQuantity> inputs,
        IEnumerable<ProductionResourceQuantity> capacity,
        IEnumerable<ProductionResourceQuantity> outputs,
        IEnumerable<ProductionResourceQuantity>? waste = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A production recipe identifier is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("A production recipe version is required.", nameof(version));

        var inputSnapshot = Snapshot(inputs, nameof(inputs));
        var capacitySnapshot = Snapshot(capacity, nameof(capacity));
        var outputSnapshot = Snapshot(outputs, nameof(outputs));
        var wasteSnapshot = Snapshot(waste ?? Array.Empty<ProductionResourceQuantity>(), nameof(waste));

        if (inputSnapshot.Length == 0)
            throw new ArgumentException("A production recipe must consume at least one input resource.", nameof(inputs));
        if (outputSnapshot.Length == 0)
            throw new ArgumentException("A production recipe must declare at least one output resource.", nameof(outputs));

        Id = id.Trim();
        Version = version.Trim();
        _inputs = Array.AsReadOnly(inputSnapshot);
        _capacity = Array.AsReadOnly(capacitySnapshot);
        _outputs = Array.AsReadOnly(outputSnapshot);
        _waste = Array.AsReadOnly(wasteSnapshot);
        Fingerprint = BuildFingerprint();
    }

    public string Id { get; }
    public string Version { get; }
    public IReadOnlyList<ProductionResourceQuantity> Inputs => _inputs;
    public IReadOnlyList<ProductionResourceQuantity> Capacity => _capacity;
    public IReadOnlyList<ProductionResourceQuantity> Outputs => _outputs;
    public IReadOnlyList<ProductionResourceQuantity> Waste => _waste;

    internal string Fingerprint { get; }

    private static ProductionResourceQuantity[] Snapshot(
        IEnumerable<ProductionResourceQuantity> quantities,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(quantities);
        var snapshot = quantities.ToArray();
        if (snapshot.Any(quantity => quantity is null))
            throw new ArgumentException("Production resource collections cannot contain null entries.", parameterName);
        if (snapshot.Select(quantity => quantity.ResourceId).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("A resource may appear only once in each recipe collection.", parameterName);
        return snapshot;
    }

    private string BuildFingerprint()
    {
        static string Format(IEnumerable<ProductionResourceQuantity> quantities) =>
            string.Concat(quantities.OrderBy(item => item.ResourceId.Value, StringComparer.Ordinal)
                .Select(item =>
                {
                    var resource = item.ResourceId.Value;
                    var quantity = item.Quantity.ToString("G29", CultureInfo.InvariantCulture);
                    return $"{resource.Length}:{resource}{quantity.Length}:{quantity}";
                }));

        return $"{Id.Length}:{Id}{Version.Length}:{Version}I{Format(_inputs)}C{Format(_capacity)}O{Format(_outputs)}W{Format(_waste)}";
    }
}

/// <summary>Classifies a resource's role in one completed production run.</summary>
public enum ProductionProvenanceKind
{
    Input,
    Capacity,
    Yield,
    Waste
}

/// <summary>Traceable resource quantity consumed or produced by one production operation.</summary>
public sealed record ProductionProvenance(
    Guid OperationId,
    string RecipeId,
    string RecipeVersion,
    ProductionResourceId ResourceId,
    decimal Quantity,
    ProductionProvenanceKind Kind);

/// <summary>Immutable receipt for an applied or idempotently replayed production operation.</summary>
public sealed class ProductionRunReceipt
{
    internal ProductionRunReceipt(
        Guid operationId,
        string recipeId,
        string recipeVersion,
        int batches,
        DateTimeOffset occurredAt,
        IEnumerable<ProductionResourceQuantity> inputs,
        IEnumerable<ProductionResourceQuantity> capacity,
        IEnumerable<ProductionResourceQuantity> yield,
        IEnumerable<ProductionResourceQuantity> waste,
        IEnumerable<ProductionProvenance> provenance,
        bool isReplay = false)
    {
        OperationId = operationId;
        RecipeId = recipeId;
        RecipeVersion = recipeVersion;
        Batches = batches;
        OccurredAt = occurredAt;
        Inputs = Array.AsReadOnly(inputs.ToArray());
        Capacity = Array.AsReadOnly(capacity.ToArray());
        Yield = Array.AsReadOnly(yield.ToArray());
        Waste = Array.AsReadOnly(waste.ToArray());
        Provenance = Array.AsReadOnly(provenance.ToArray());
        IsReplay = isReplay;
    }

    public Guid OperationId { get; }
    public string RecipeId { get; }
    public string RecipeVersion { get; }
    public int Batches { get; }
    public DateTimeOffset OccurredAt { get; }
    public IReadOnlyList<ProductionResourceQuantity> Inputs { get; }
    public IReadOnlyList<ProductionResourceQuantity> Capacity { get; }
    public IReadOnlyList<ProductionResourceQuantity> Yield { get; }
    public IReadOnlyList<ProductionResourceQuantity> Waste { get; }
    public IReadOnlyList<ProductionProvenance> Provenance { get; }
    public bool IsReplay { get; }

    internal ProductionRunReceipt AsReplay() =>
        new(OperationId, RecipeId, RecipeVersion, Batches, OccurredAt,
            Inputs, Capacity, Yield, Waste, Provenance, isReplay: true);
}

/// <summary>
/// Process-local, thread-safe inventory and production executor for deterministic simulation.
/// It atomically consumes declared inputs and capacity, records declared yield and waste,
/// and rejects conflicting reuse of an operation identifier. It is not durable or distributed.
/// </summary>
public sealed class ProductionInventory
{
    private readonly object _sync = new();
    private readonly Dictionary<ProductionResourceId, decimal> _balances = new();
    private readonly Dictionary<Guid, (string Fingerprint, ProductionRunReceipt Receipt)> _operations = new();
    private readonly List<ProductionRunReceipt> _history = new();

    public ProductionInventory(IEnumerable<ProductionResourceQuantity>? openingStock = null)
    {
        var stock = (openingStock ?? Array.Empty<ProductionResourceQuantity>()).ToArray();
        if (stock.Any(item => item is null))
            throw new ArgumentException("Opening stock cannot contain null entries.", nameof(openingStock));
        if (stock.Select(item => item.ResourceId).Distinct().Count() != stock.Length)
            throw new ArgumentException("Opening stock must contain each resource only once.", nameof(openingStock));

        foreach (var item in stock)
            _balances.Add(item.ResourceId, item.Quantity);
    }

    /// <summary>Returns the current quantity of a resource, or zero when it is not present.</summary>
    public decimal GetBalance(ProductionResourceId resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId.Value))
            throw new ArgumentException("A production resource identifier is required.", nameof(resourceId));
        lock (_sync)
            return _balances.TryGetValue(resourceId, out var balance) ? balance : 0m;
    }

    /// <summary>Returns a detached snapshot of the applied production receipts.</summary>
    public IReadOnlyList<ProductionRunReceipt> History
    {
        get
        {
            lock (_sync)
                return Array.AsReadOnly(_history.ToArray());
        }
    }

    /// <summary>
    /// Executes the declared recipe atomically for the requested number of batches.
    /// Capacity is modeled as a consumable resource (for example, labor-hours or machine-hours).
    /// </summary>
    public ProductionRunReceipt Execute(
        Guid operationId,
        ProductionRecipe recipe,
        int batches,
        DateTimeOffset occurredAt)
    {
        if (operationId == Guid.Empty)
            throw new ArgumentException("A production operation identifier is required.", nameof(operationId));
        ArgumentNullException.ThrowIfNull(recipe);
        if (batches <= 0)
            throw new ArgumentOutOfRangeException(nameof(batches), "A production run must contain at least one batch.");

        var fingerprint = $"{recipe.Fingerprint}|batches={batches}";
        lock (_sync)
        {
            if (_operations.TryGetValue(operationId, out var previous))
            {
                if (!string.Equals(previous.Fingerprint, fingerprint, StringComparison.Ordinal))
                    throw new InvalidOperationException("A production operation identifier cannot be reused for a different recipe or batch count.");
                return previous.Receipt.AsReplay();
            }

            static ProductionResourceQuantity[] Scale(
                IReadOnlyList<ProductionResourceQuantity> quantities, int count) =>
                quantities.Select(item => new ProductionResourceQuantity(
                    item.ResourceId, checked(item.Quantity * count))).ToArray();

            var inputs = Scale(recipe.Inputs, batches);
            var capacity = Scale(recipe.Capacity, batches);
            var yield = Scale(recipe.Outputs, batches);
            var waste = Scale(recipe.Waste, batches);

            // Inputs and capacity must exist before the run begins; outputs cannot bootstrap their own inputs.
            var consumed = new Dictionary<ProductionResourceId, decimal>();
            AddDelta(consumed, inputs, 1m);
            AddDelta(consumed, capacity, 1m);
            foreach (var requirement in consumed)
            {
                _balances.TryGetValue(requirement.Key, out var available);
                if (available < requirement.Value)
                    throw new InvalidOperationException(
                        $"Insufficient production resource '{requirement.Key}': required {requirement.Value}, available {available}.");
            }

            var deltas = new Dictionary<ProductionResourceId, decimal>();

            static void AddDelta(
                IDictionary<ProductionResourceId, decimal> target,
                IEnumerable<ProductionResourceQuantity> quantities,
                decimal sign)
            {
                foreach (var item in quantities)
                {
                    target.TryGetValue(item.ResourceId, out var current);
                    target[item.ResourceId] = checked(current + checked(item.Quantity * sign));
                }
            }

            AddDelta(deltas, inputs, -1m);
            AddDelta(deltas, capacity, -1m);
            AddDelta(deltas, yield, 1m);
            AddDelta(deltas, waste, 1m);

            var nextBalances = new Dictionary<ProductionResourceId, decimal>();
            foreach (var delta in deltas)
            {
                _balances.TryGetValue(delta.Key, out var current);
                var next = checked(current + delta.Value);
                if (next < 0m)
                    throw new InvalidOperationException(
                        $"Insufficient production resource '{delta.Key}': required net balance would be {next}.");
                nextBalances.Add(delta.Key, next);
            }

            var provenance = new List<ProductionProvenance>();
            AddProvenance(provenance, operationId, recipe, inputs, ProductionProvenanceKind.Input);
            AddProvenance(provenance, operationId, recipe, capacity, ProductionProvenanceKind.Capacity);
            AddProvenance(provenance, operationId, recipe, yield, ProductionProvenanceKind.Yield);
            AddProvenance(provenance, operationId, recipe, waste, ProductionProvenanceKind.Waste);

            var receipt = new ProductionRunReceipt(
                operationId, recipe.Id, recipe.Version, batches, occurredAt,
                inputs, capacity, yield, waste, provenance);

            // No inventory or operation state changes until every scaled quantity and balance validates.
            foreach (var balance in nextBalances)
                _balances[balance.Key] = balance.Value;
            _operations.Add(operationId, (fingerprint, receipt));
            _history.Add(receipt);
            return receipt;
        }
    }

    private static void AddProvenance(
        ICollection<ProductionProvenance> target,
        Guid operationId,
        ProductionRecipe recipe,
        IEnumerable<ProductionResourceQuantity> quantities,
        ProductionProvenanceKind kind)
    {
        foreach (var item in quantities)
            target.Add(new ProductionProvenance(
                operationId, recipe.Id, recipe.Version, item.ResourceId, item.Quantity, kind));
    }
}
