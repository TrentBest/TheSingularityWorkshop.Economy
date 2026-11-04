using TheSingularityWorkshop.Economy;

namespace TheSingularityWorkshop.Economy.ShopConnectors;

/// <summary>Supplies a provider-neutral catalog snapshot without exposing provider SDKs to the connector contract.</summary>
public interface IShopCatalogSource
{
    ShopProviderId Provider { get; }
    ShopConnectorCapabilities Capabilities { get; }
    Task<ProviderShopSnapshot> ReadCatalogAsync(string externalShopId, CancellationToken cancellationToken = default);
}

/// <summary>Adapts any catalog source into canonical Economy shop and item identities.</summary>
public sealed class SnapshotShopConnector : IShopConnector
{
    private readonly IShopCatalogSource _source;

    public SnapshotShopConnector(IShopCatalogSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        if (!_source.Capabilities.HasFlag(ShopConnectorCapabilities.ReadCatalog) ||
            !_source.Capabilities.HasFlag(ShopConnectorCapabilities.SynchronizeCatalog))
            throw new ArgumentException("A catalog source must support reading and synchronization.", nameof(source));
    }

    public ShopProviderId Provider => _source.Provider;
    public ShopConnectorCapabilities Capabilities => _source.Capabilities;

    public async Task<ImportedShopCatalog> SynchronizeAsync(
        string externalShopId,
        EconomyEntityId seller,
        IShopIdentityMap identityMap,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalShopId);
        ArgumentNullException.ThrowIfNull(identityMap);

        var snapshot = await _source.ReadCatalogAsync(externalShopId, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(snapshot.ExternalShopId, externalShopId, StringComparison.Ordinal))
            throw new InvalidOperationException("The catalog source returned a different external shop identity.");

        var shopReference = new ProviderShopReference(Provider, snapshot.ExternalShopId, snapshot.ShopUri);
        var products = new List<CommerceItem>(snapshot.Items.Count);
        var itemProvenance = new List<ImportedItemProvenance>(snapshot.Items.Count);

        foreach (var item in snapshot.Items)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(item.ExternalItemId);
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Name);

            if (item.Availability is not null && !Capabilities.HasFlag(ShopConnectorCapabilities.ReadAvailability))
                throw new InvalidOperationException("The source returned availability without declaring that capability.");

            var itemReference = new ProviderItemReference(Provider, item.ExternalItemId, item.ProductUri);
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
}