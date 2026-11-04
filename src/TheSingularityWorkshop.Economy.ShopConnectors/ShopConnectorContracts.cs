using TheSingularityWorkshop.Economy;

namespace TheSingularityWorkshop.Economy.ShopConnectors;

/// <summary>Capabilities a provider actually supports. Catalog access never implies purchase or inventory mutation.</summary>
[Flags]
public enum ShopConnectorCapabilities
{
    None = 0,
    ReadCatalog = 1,
    SynchronizeCatalog = 2,
    ExternalProductLinks = 4,
    ReadAvailability = 8
}

/// <summary>Stable identity of a provider, independent of its SDK or transport.</summary>
public readonly record struct ShopProviderId
{
    public ShopProviderId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim().ToLowerInvariant();
    }

    public string Value { get; }
}

/// <summary>Provider-side shop identity and provenance.</summary>
public sealed record ProviderShopReference(ShopProviderId Provider, string ExternalShopId, Uri? SourceUri = null);

/// <summary>Provider-side item identity and provenance.</summary>
public sealed record ProviderItemReference(ShopProviderId Provider, string ExternalItemId, Uri? SourceUri = null);

/// <summary>A provider's observation of an item's availability; it is not a reservation or inventory command.</summary>
public sealed record ProviderAvailabilityObservation(bool IsAvailable, int? Quantity, DateTimeOffset ObservedAt);

/// <summary>Provider-neutral source data for one catalog item.</summary>
public sealed record ProviderCatalogItem(
    string ExternalItemId,
    string Name,
    Money Price,
    Uri? ProductUri = null,
    ProviderAvailabilityObservation? Availability = null);

/// <summary>A provider-neutral snapshot returned by a shop source.</summary>
public sealed record ProviderShopSnapshot(
    string ExternalShopId,
    string Name,
    Uri? ShopUri,
    DateTimeOffset RetrievedAt,
    IReadOnlyList<ProviderCatalogItem> Items);

/// <summary>Maps external identities to stable Workshop identities. Implementations may persist this mapping.</summary>
public interface IShopIdentityMap
{
    Guid GetOrCreateShopId(ProviderShopReference reference);
    CommerceItemId GetOrCreateItemId(ProviderItemReference reference);
}

/// <summary>Imported provenance for a canonical Workshop shop.</summary>
public sealed record ImportedShopProvenance(
    ProviderShopReference ProviderShop,
    DateTimeOffset SynchronizedAt,
    IReadOnlyList<ImportedItemProvenance> Items);

/// <summary>Imported provenance for one canonical Workshop item.</summary>
public sealed record ImportedItemProvenance(
    ProviderItemReference ProviderItem,
    Uri? ProductUri,
    ProviderAvailabilityObservation? Availability);

/// <summary>Canonical Economy shop plus provider metadata retained outside Economy core.</summary>
public sealed record ImportedShopCatalog(EconomyShop Shop, ImportedShopProvenance Provenance);

/// <summary>Reads and synchronizes a provider catalog. No checkout or mutation operations are implied.</summary>
public interface IShopConnector
{
    ShopProviderId Provider { get; }
    ShopConnectorCapabilities Capabilities { get; }
    Task<ImportedShopCatalog> SynchronizeAsync(
        string externalShopId,
        EconomyEntityId seller,
        IShopIdentityMap identityMap,
        CancellationToken cancellationToken = default);
}

/// <summary>Maps external identities to Workshop IDs for tests and ephemeral hosts. Use durable storage in production.</summary>
public sealed class InMemoryShopIdentityMap : IShopIdentityMap
{
    private readonly Dictionary<(ShopProviderId Provider, string ExternalId), Guid> _shops = new();
    private readonly Dictionary<(ShopProviderId Provider, string ExternalId), CommerceItemId> _items = new();

    public Guid GetOrCreateShopId(ProviderShopReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        var key = (reference.Provider, reference.ExternalShopId);
        if (!_shops.TryGetValue(key, out var id))
            _shops.Add(key, id = Guid.NewGuid());
        return id;
    }

    public CommerceItemId GetOrCreateItemId(ProviderItemReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        var key = (reference.Provider, reference.ExternalItemId);
        if (!_items.TryGetValue(key, out var id))
            _items.Add(key, id = CommerceItemId.New());
        return id;
    }
}