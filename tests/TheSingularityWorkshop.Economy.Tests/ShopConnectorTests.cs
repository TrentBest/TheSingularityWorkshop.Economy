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
        Assert.DoesNotContain(Enum.GetNames<ShopConnectorCapabilities>(), name =>
            name.Contains("Checkout", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Purchase", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Reserve", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("InventoryMutation", StringComparison.OrdinalIgnoreCase));
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


    [Fact]
    public async Task IndividualItemReadPreservesProviderProvenanceAndCanonicalIdentity()
    {
        var source = new MutableCatalogSource("fixture-marketplace", "shop-1");
        var connector = new SnapshotShopConnector(source);
        var identities = new InMemoryShopIdentityMap();
        var seller = EconomyEntityId.New();

        var catalogItem = await connector.ReadItemAsync("shop-1", "item-1", seller, identities);
        var synchronized = await connector.SynchronizeAsync("shop-1", seller, identities);

        Assert.Equal(synchronized.Shop.Products.Single().Id, catalogItem.Item.Id);
        Assert.Equal("fixture-marketplace", catalogItem.Provenance.ProviderItem.Provider.Value);
        Assert.Equal("shop-1", catalogItem.Provenance.ProviderItem.ExternalShopId);
    }

    [Fact]
    public async Task ImportedShopCanBePlacedInTwoExperiencesWithoutChangingItsIdentity()
    {
        var connector = new SnapshotShopConnector(new MutableCatalogSource("fixture", "shop-1"));
        var imported = await connector.SynchronizeAsync(
            "shop-1", EconomyEntityId.New(), new InMemoryShopIdentityMap());
        var first = new ExperiencePlacement(imported.Shop, "market-fair", "open-air-stall");
        var second = new ExperiencePlacement(imported.Shop, "city-plaza", "holographic-kiosk");

        Assert.Same(first.Shop, second.Shop);
        Assert.Equal(first.Shop.Id, second.Shop.Id);
        Assert.NotEqual(first.SkinId, second.SkinId);
        Assert.Equal(first.Shop.Products.Select(item => item.Id), second.Shop.Products.Select(item => item.Id));
    }

    [Fact]
    public async Task NativeInMemorySourceFeedsTheSameCanonicalConnectorContract()
    {
        var nativeShop = new ProviderShopSnapshot(
            "workshop-shop-1",
            "Native Creator Shop",
            new Uri("https://workshop.example/shops/workshop-shop-1"),
            DateTimeOffset.UtcNow,
            new[]
            {
                new ProviderCatalogItem(
                    "native-item-1",
                    "Workshop sculpture",
                    new Money(25m, "GLD"),
                    new Uri("https://workshop.example/items/native-item-1"))
            });
        var source = new InMemoryShopCatalogSource(new[] { nativeShop });
        var connector = new SnapshotShopConnector(source);

        var imported = await connector.SynchronizeAsync(
            "workshop-shop-1", EconomyEntityId.New(), new InMemoryShopIdentityMap());

        Assert.Equal("Native Creator Shop", imported.Shop.Name);
        Assert.Equal("Workshop sculpture", Assert.Single(imported.Shop.Products).Name);
        Assert.Equal("workshop.native", imported.Provenance.ProviderShop.Provider.Value);
    }

    [Fact]
    public void SameExternalItemIdInDifferentShopsDoesNotCollide()
    {
        var identities = new InMemoryShopIdentityMap();
        var provider = new ShopProviderId("fixture");
        var first = identities.GetOrCreateItemId(new ProviderItemReference(provider, "shop-a", "item-1"));
        var second = identities.GetOrCreateItemId(new ProviderItemReference(provider, "shop-b", "item-1"));

        Assert.NotEqual(first, second);
        Assert.Equal(first, identities.GetOrCreateItemId(
            new ProviderItemReference(provider, "shop-a", "item-1", new Uri("https://example.invalid/changed-url"))));
    }

    private sealed record ExperiencePlacement(EconomyShop Shop, string ExperienceId, string SkinId);

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
            ShopConnectorCapabilities.ReadItem |
            ShopConnectorCapabilities.SynchronizeCatalog |
            ShopConnectorCapabilities.ExternalProductLinks |
            (_includeAvailability ? ShopConnectorCapabilities.ReadAvailability : ShopConnectorCapabilities.None);

        public void RenameItem(string externalItemId, string name)
        {
            if (externalItemId == "item-1") _itemName = name;
        }


        public Task<ProviderCatalogItem> ReadItemAsync(
            string externalShopId,
            string externalItemId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (externalShopId != _shopId || externalItemId != "item-1")
                throw new KeyNotFoundException();
            return Task.FromResult(new ProviderCatalogItem(
                "item-1",
                _itemName,
                new Money(12m, "USD"),
                new Uri($"https://example.invalid/{Provider.Value}/items/item-1"),
                _includeAvailability
                    ? new ProviderAvailabilityObservation(true, 7, DateTimeOffset.UtcNow)
                    : null));
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