using TheSingularityWorkshop.Economy;
using TheSingularityWorkshop.Economy.ShopConnectors;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class ShopConnectorTests
{
    [Fact]
    public async Task RepeatedSynchronizationPreservesCanonicalShopAndItemIds()
    {
        var source = new MutableCatalogSource("native", "shop-1");
        var connector = new SnapshotShopConnector(source);
        var identities = new InMemoryShopIdentityMap();
        var seller = EconomyEntityId.New();

        var first = await connector.SynchronizeAsync("shop-1", seller, identities);
        source.RenameItem("item-1", "Updated ceramic cup");
        var second = await connector.SynchronizeAsync("shop-1", seller, identities);

        Assert.Equal(first.Shop.Id, second.Shop.Id);
        Assert.Equal(first.Shop.Products.Single().Id, second.Shop.Products.Single().Id);
        Assert.Equal("Updated ceramic cup", second.Shop.Products.Single().Name);
        Assert.Equal("native", second.Provenance.ProviderShop.Provider.Value);
        Assert.Equal("item-1", second.Provenance.Items.Single().ProviderItem.ExternalItemId);
    }

    [Fact]
    public async Task DifferentProvidersCanUseTheSameConnectorContract()
    {
        var native = new SnapshotShopConnector(new MutableCatalogSource("native", "native-shop"));
        var external = new SnapshotShopConnector(new MutableCatalogSource("fixture-marketplace", "external-shop"));
        var identities = new InMemoryShopIdentityMap();
        var seller = EconomyEntityId.New();

        var nativeCatalog = await native.SynchronizeAsync("native-shop", seller, identities);
        var externalCatalog = await external.SynchronizeAsync("external-shop", seller, identities);

        Assert.Equal("native", nativeCatalog.Provenance.ProviderShop.Provider.Value);
        Assert.Equal("fixture-marketplace", externalCatalog.Provenance.ProviderShop.Provider.Value);
        Assert.NotEqual(nativeCatalog.Shop.Id, externalCatalog.Shop.Id);
    }

    [Fact]
    public void ConnectorDoesNotClaimCheckoutOrInventoryMutation()
    {
        var connector = new SnapshotShopConnector(new MutableCatalogSource("fixture", "shop"));
        var capabilities = connector.Capabilities;

        Assert.True(capabilities.HasFlag(ShopConnectorCapabilities.ReadCatalog));
        Assert.True(capabilities.HasFlag(ShopConnectorCapabilities.SynchronizeCatalog));
        Assert.False(Enum.GetNames<ShopConnectorCapabilities>().Any(name =>
            name.Contains("Checkout", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Purchase", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Reserve", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("InventoryMutation", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task AvailabilityIsRetainedOnlyWhenProviderDeclaresSupport()
    {
        var source = new MutableCatalogSource("fixture", "shop", includeAvailability: true);
        var result = await new SnapshotShopConnector(source).SynchronizeAsync(
            "shop", EconomyEntityId.New(), new InMemoryShopIdentityMap());

        var availability = Assert.Single(result.Provenance.Items).Availability;
        Assert.NotNull(availability);
        Assert.Equal(7, availability.Quantity);
        Assert.True(availability.IsAvailable);
        Assert.True(source.Capabilities.HasFlag(ShopConnectorCapabilities.ReadAvailability));
    }

    private sealed class MutableCatalogSource : IShopCatalogSource
    {
        private readonly string _shopId;
        private readonly bool _includeAvailability;
        private string _itemName = "Handmade ceramic cup";

        public MutableCatalogSource(string provider, string shopId, bool includeAvailability = false)
        {
            Provider = new ShopProviderId(provider);
            _shopId = shopId;
            _includeAvailability = includeAvailability;
        }

        public ShopProviderId Provider { get; }
        public ShopConnectorCapabilities Capabilities =>
            ShopConnectorCapabilities.ReadCatalog |
            ShopConnectorCapabilities.SynchronizeCatalog |
            ShopConnectorCapabilities.ExternalProductLinks |
            (_includeAvailability ? ShopConnectorCapabilities.ReadAvailability : ShopConnectorCapabilities.None);

        public void RenameItem(string externalItemId, string name)
        {
            if (externalItemId == "item-1") _itemName = name;
        }

        public Task<ProviderShopSnapshot> ReadCatalogAsync(
            string externalShopId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var items = new[]
            {
                new ProviderCatalogItem(
                    "item-1",
                    _itemName,
                    new Money(12m, "USD"),
                    new Uri($"https://example.invalid/{Provider.Value}/items/item-1"),
                    _includeAvailability
                        ? new ProviderAvailabilityObservation(true, 7, DateTimeOffset.UtcNow)
                        : null)
            };

            return Task.FromResult(new ProviderShopSnapshot(
                _shopId,
                "Creator Shop",
                new Uri($"https://example.invalid/{Provider.Value}/shops/{_shopId}"),
                DateTimeOffset.UtcNow,
                items));
        }
    }
}