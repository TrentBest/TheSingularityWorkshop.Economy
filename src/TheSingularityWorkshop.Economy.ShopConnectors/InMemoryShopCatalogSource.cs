namespace TheSingularityWorkshop.Economy.ShopConnectors;

/// <summary>
/// Native Workshop catalog source for local/in-memory shops. Hosts can replace this with a durable store
/// while retaining the same provider-neutral connector contract.
/// </summary>
public sealed class InMemoryShopCatalogSource : IShopCatalogSource
{
    private readonly Dictionary<string, ProviderShopSnapshot> _shops = new(StringComparer.Ordinal);

    public InMemoryShopCatalogSource(IEnumerable<ProviderShopSnapshot>? initialShops = null)
    {
        Provider = new ShopProviderId("workshop.native");
        if (initialShops is null) return;
        foreach (var shop in initialShops)
            Upsert(shop);
    }

    public ShopProviderId Provider { get; }

    public ShopConnectorCapabilities Capabilities =>
        ShopConnectorCapabilities.ReadCatalog |
        ShopConnectorCapabilities.ReadItem |
        ShopConnectorCapabilities.SynchronizeCatalog |
        ShopConnectorCapabilities.ExternalProductLinks |
        ShopConnectorCapabilities.ReadAvailability;

    public void Upsert(ProviderShopSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.ExternalShopId);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Name);
        ArgumentNullException.ThrowIfNull(snapshot.Items);
        _shops[snapshot.ExternalShopId] = snapshot with
        {
            Items = Array.AsReadOnly(snapshot.Items.ToArray())
        };
    }


    public async Task<ProviderCatalogItem> ReadItemAsync(
        string externalShopId,
        string externalItemId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadCatalogAsync(externalShopId, cancellationToken).ConfigureAwait(false);
        return snapshot.Items.FirstOrDefault(item => string.Equals(item.ExternalItemId, externalItemId, StringComparison.Ordinal))
            ?? throw new KeyNotFoundException("The requested native Workshop item was not found.");
    }

    public Task<ProviderShopSnapshot> ReadCatalogAsync(
        string externalShopId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(externalShopId);
        if (!_shops.TryGetValue(externalShopId, out var snapshot))
            throw new KeyNotFoundException($"Native Workshop shop '{externalShopId}' was not found.");
        return Task.FromResult(snapshot);
    }
}