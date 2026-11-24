using TheSingularityWorkshop.Economy;
using TheSingularityWorkshop.Economy.ShopConnectors;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class ShopConnectorHardeningTests
{
    [Fact]
    public async Task IdentityMapReturnsOneCanonicalIdentityForConcurrentRequests()
    {
        var identities = new InMemoryShopIdentityMap();
        var provider = new ShopProviderId("fixture");
        var shopReference = new ProviderShopReference(provider, "shop-1");
        var itemReference = new ProviderItemReference(provider, "shop-1", "item-1");

        var shopIds = await Task.WhenAll(Enumerable.Range(0, 64)
            .Select(_ => Task.Run(() => identities.GetOrCreateShopId(shopReference))));
        var itemIds = await Task.WhenAll(Enumerable.Range(0, 64)
            .Select(_ => Task.Run(() => identities.GetOrCreateItemId(itemReference))));

        Assert.Single(shopIds.Distinct());
        Assert.Single(itemIds.Distinct());
    }

    [Fact]
    public async Task DuplicateExternalItemIdsAreRejectedBeforeIdentityMapping()
    {
        var source = new FixtureCatalogSource(
            "fixture",
            new ProviderShopSnapshot(
                "shop-1",
                "Creator shop",
                null,
                DateTimeOffset.UtcNow,
                new[]
                {
                    new ProviderCatalogItem("duplicate", "First", new Money(1m, "USD")),
                    new ProviderCatalogItem("duplicate", "Second", new Money(2m, "USD"))
                }));
        var identities = new CountingIdentityMap();
        var connector = new SnapshotShopConnector(source);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            connector.SynchronizeAsync("shop-1", EconomyEntityId.New(), identities));

        Assert.Equal(0, identities.ShopCalls);
        Assert.Equal(0, identities.ItemCalls);
    }

    [Fact]
    public async Task ProductUrlRequiresExplicitExternalProductLinkCapability()
    {
        var source = new FixtureCatalogSource(
            "fixture",
            SnapshotWith(new ProviderCatalogItem(
                "item-1",
                "Product",
                new Money(5m, "USD"),
                new Uri("https://example.invalid/item"))),
            ShopConnectorCapabilities.ReadCatalog | ShopConnectorCapabilities.SynchronizeCatalog);
        var connector = new SnapshotShopConnector(source);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            connector.SynchronizeAsync("shop-1", EconomyEntityId.New(), new InMemoryShopIdentityMap()));
    }

    [Fact]
    public async Task NegativeAvailabilityQuantityIsRejected()
    {
        var source = new FixtureCatalogSource(
            "fixture",
            SnapshotWith(new ProviderCatalogItem(
                "item-1",
                "Product",
                new Money(5m, "USD"),
                Availability: new ProviderAvailabilityObservation(true, -1, DateTimeOffset.UtcNow))));
        var connector = new SnapshotShopConnector(source);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            connector.SynchronizeAsync("shop-1", EconomyEntityId.New(), new InMemoryShopIdentityMap()));
    }

    [Fact]
    public async Task IndividualItemReadDoesNotRequireCatalogSynchronizationCapabilities()
    {
        var source = new FixtureCatalogSource(
            "item-only-provider",
            SnapshotWith(new ProviderCatalogItem("item-1", "Single item", new Money(3m, "USD"))),
            ShopConnectorCapabilities.ReadItem);
        var connector = new SnapshotShopConnector(source);

        var imported = await connector.ReadItemAsync(
            "shop-1", "item-1", EconomyEntityId.New(), new InMemoryShopIdentityMap());

        Assert.Equal("Single item", imported.Item.Name);
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            connector.SynchronizeAsync("shop-1", EconomyEntityId.New(), new InMemoryShopIdentityMap()));
    }

    private static ProviderShopSnapshot SnapshotWith(ProviderCatalogItem item) =>
        new("shop-1", "Creator shop", null, DateTimeOffset.UtcNow, new[] { item });

    private sealed class FixtureCatalogSource : IShopCatalogSource
    {
        private readonly ProviderShopSnapshot _snapshot;

        public FixtureCatalogSource(
            string provider,
            ProviderShopSnapshot snapshot,
            ShopConnectorCapabilities? capabilities = null)
        {
            Provider = new ShopProviderId(provider);
            _snapshot = snapshot;
            Capabilities = capabilities ??
                (ShopConnectorCapabilities.ReadCatalog |
                 ShopConnectorCapabilities.ReadItem |
                 ShopConnectorCapabilities.SynchronizeCatalog |
                 ShopConnectorCapabilities.ExternalProductLinks |
                 ShopConnectorCapabilities.ReadAvailability);
        }

        public ShopProviderId Provider { get; }
        public ShopConnectorCapabilities Capabilities { get; }

        public Task<ProviderShopSnapshot> ReadCatalogAsync(
            string externalShopId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_snapshot);
        }

        public Task<ProviderCatalogItem> ReadItemAsync(
            string externalShopId,
            string externalItemId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_snapshot.Items.Single(item => item.ExternalItemId == externalItemId));
        }
    }

    private sealed class CountingIdentityMap : IShopIdentityMap
    {
        public int ShopCalls { get; private set; }
        public int ItemCalls { get; private set; }

        public Guid GetOrCreateShopId(ProviderShopReference reference)
        {
            ShopCalls++;
            return Guid.NewGuid();
        }

        public CommerceItemId GetOrCreateItemId(ProviderItemReference reference)
        {
            ItemCalls++;
            return CommerceItemId.New();
        }
    }
}