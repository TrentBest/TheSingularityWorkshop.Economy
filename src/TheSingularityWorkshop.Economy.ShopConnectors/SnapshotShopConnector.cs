using TheSingularityWorkshop.Economy;

namespace TheSingularityWorkshop.Economy.ShopConnectors;

/// <summary>Supplies a provider-neutral catalog snapshot without exposing provider SDKs to the connector contract.</summary>
public interface IShopCatalogSource
{
    ShopProviderId Provider { get; }
    ShopConnectorCapabilities Capabilities { get; }
    Task<ProviderShopSnapshot> ReadCatalogAsync(string externalShopId, CancellationToken cancellationToken = default);
    Task<ProviderCatalogItem> ReadItemAsync(string externalShopId, string externalItemId, CancellationToken cancellationToken = default);
}

/// <summary>Adapts any catalog source into canonical Economy shop and item identities.</summary>
public sealed class SnapshotShopConnector : IShopConnector
{
    private readonly IShopCatalogSource _source;

    public SnapshotShopConnector(IShopCatalogSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(_source.Provider.Value))
            throw new ArgumentException("A catalog source must declare a provider identity.", nameof(source));
    }

    public ShopProviderId Provider => _source.Provider;
    public ShopConnectorCapabilities Capabilities => _source.Capabilities;

    public async Task<ImportedShopItem> ReadItemAsync(
        string externalShopId,
        string externalItemId,
        EconomyEntityId seller,
        IShopIdentityMap identityMap,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalShopId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalItemId);
        ArgumentNullException.ThrowIfNull(identityMap);
        if (!Capabilities.HasFlag(ShopConnectorCapabilities.ReadItem))
            throw new NotSupportedException("This provider does not support reading an individual item.");

        var item = await _source.ReadItemAsync(externalShopId, externalItemId, cancellationToken).ConfigureAwait(false);
        ValidateItem(item);
        if (!string.Equals(item.ExternalItemId, externalItemId, StringComparison.Ordinal))
            throw new InvalidOperationException("The catalog source returned a different external item identity.");

        var reference = new ProviderItemReference(Provider, externalShopId, item.ExternalItemId, item.ProductUri);
        var canonical = new CommerceItem(identityMap.GetOrCreateItemId(reference), seller, item.Name, item.Price, item.ProductUri);
        return new ImportedShopItem(canonical, new ImportedItemProvenance(reference, item.ProductUri, item.Availability));
    }

    public async Task<ImportedShopCatalog> SynchronizeAsync(
        string externalShopId,
        EconomyEntityId seller,
        IShopIdentityMap identityMap,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalShopId);
        ArgumentNullException.ThrowIfNull(identityMap);
        if (!Capabilities.HasFlag(ShopConnectorCapabilities.ReadCatalog) ||
            !Capabilities.HasFlag(ShopConnectorCapabilities.SynchronizeCatalog))
            throw new NotSupportedException("This provider does not support catalog synchronization.");

        var snapshot = await _source.ReadCatalogAsync(externalShopId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The catalog source returned no snapshot.");
        if (!string.Equals(snapshot.ExternalShopId, externalShopId, StringComparison.Ordinal))
            throw new InvalidOperationException("The catalog source returned a different external shop identity.");
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Name);
        ArgumentNullException.ThrowIfNull(snapshot.Items);

        // Validate the entire snapshot before mutating the identity map.
        var seenItemIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in snapshot.Items)
        {
            ValidateItem(item);
            if (!seenItemIds.Add(item.ExternalItemId))
                throw new InvalidOperationException($"The catalog contains duplicate external item ID '{item.ExternalItemId}'.");
        }

        var shopReference = new ProviderShopReference(Provider, snapshot.ExternalShopId, snapshot.ShopUri);
        var products = new List<CommerceItem>(snapshot.Items.Count);
        var itemProvenance = new List<ImportedItemProvenance>(snapshot.Items.Count);

        foreach (var item in snapshot.Items)
        {
            var itemReference = new ProviderItemReference(Provider, snapshot.ExternalShopId, item.ExternalItemId, item.ProductUri);
            var itemId = identityMap.GetOrCreateItemId(itemReference);
            products.Add(new CommerceItem(itemId, seller, item.Name, item.Price, item.ProductUri));
            itemProvenance.Add(new ImportedItemProvenance(itemReference, item.ProductUri, item.Availability));
        }

        var shop = new EconomyShop(
            identityMap.GetOrCreateShopId(shopReference),
            seller,
            snapshot.Name,
            Array.AsReadOnly(products.ToArray()));

        return new ImportedShopCatalog(
            shop,
            new ImportedShopProvenance(shopReference, snapshot.RetrievedAt, Array.AsReadOnly(itemProvenance.ToArray())));
    }

    private void ValidateItem(ProviderCatalogItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.ExternalItemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Name);

        if (item.ProductUri is not null && !Capabilities.HasFlag(ShopConnectorCapabilities.ExternalProductLinks))
            throw new InvalidOperationException("The source returned a product URL without declaring external product-link support.");
        if (item.Availability is not null && !Capabilities.HasFlag(ShopConnectorCapabilities.ReadAvailability))
            throw new InvalidOperationException("The source returned availability without declaring that capability.");
        if (item.Availability?.Quantity is < 0)
            throw new InvalidOperationException("The source returned a negative availability quantity.");
    }
}