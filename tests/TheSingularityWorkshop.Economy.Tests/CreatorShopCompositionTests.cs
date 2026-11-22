using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

/// <summary>
/// Consumer-owned fixture for the intended creator-shop boundary.
/// Skin and Experience placement are deliberately not Economy domain types:
/// Economy describes the canonical shop; a host Experience owns presentation and context.
/// </summary>
public sealed class CreatorShopCompositionTests
{
    [Fact]
    public void OneCanonicalShopCanBePresentedInTwoExperiencesWithoutDuplicatingItsIdentity()
    {
        var owner = EconomyEntityId.New();
        var catalog = new[]
        {
            new CommerceItem(
                CommerceItemId.New(),
                owner,
                "City-grown apples",
                new Money(3m, "GLD")),
            new CommerceItem(
                CommerceItemId.New(),
                owner,
                "Handmade ceramic cup",
                new Money(12m, "GLD"))
        };

        var canonicalShop = EconomyShop.Create(owner, "Mara's Corner Market", catalog);

        var marketFairPlacement = new ExperienceShopPlacement(
            canonicalShop,
            "experience.market-fair",
            new ShopPresentation("skin.open-air-stall", "Open-air Stall"));

        var cityPlazaPlacement = new ExperienceShopPlacement(
            canonicalShop,
            "experience.singularity-city-plaza",
            new ShopPresentation("skin.holographic-kiosk", "Holographic Kiosk"));

        Assert.NotEqual(marketFairPlacement.ExperienceId, cityPlazaPlacement.ExperienceId);
        Assert.NotEqual(marketFairPlacement.Presentation.SkinId, cityPlazaPlacement.Presentation.SkinId);

        // Both placements point to the same canonical shop, not independently-created copies.
        Assert.Same(canonicalShop, marketFairPlacement.Shop);
        Assert.Same(canonicalShop, cityPlazaPlacement.Shop);
        Assert.Equal(canonicalShop.Id, marketFairPlacement.Shop.Id);
        Assert.Equal(marketFairPlacement.Shop.Id, cityPlazaPlacement.Shop.Id);
        Assert.Equal(
            canonicalShop.Products.Select(product => product.Id),
            cityPlazaPlacement.Shop.Products.Select(product => product.Id));
    }

    [Fact]
    public void ChangingPresentationDoesNotChangeCanonicalShopOrCatalog()
    {
        var owner = EconomyEntityId.New();
        var item = new CommerceItem(
            CommerceItemId.New(),
            owner,
            "Workshop blueprint",
            new Money(1m, "GLD"));
        var shop = EconomyShop.Create(owner, "Workshop Exchange", new[] { item });
        var originalShopId = shop.Id;
        var originalProductIds = shop.Products.Select(product => product.Id).ToArray();

        var plain = new ExperienceShopPlacement(
            shop,
            "experience.workshop",
            new ShopPresentation("skin.plain", "Plain"));
        var reskinned = plain with
        {
            Presentation = new ShopPresentation("skin.blueprint-terminal", "Blueprint Terminal")
        };

        Assert.Equal(originalShopId, reskinned.Shop.Id);
        Assert.Equal(originalProductIds, reskinned.Shop.Products.Select(product => product.Id));
        Assert.Equal("skin.blueprint-terminal", reskinned.Presentation.SkinId);
    }

    // These are intentionally consumer-owned examples, not production Economy API claims.
    private sealed record ShopPresentation(string SkinId, string DisplayName);

    private sealed record ExperienceShopPlacement(
        EconomyShop Shop,
        string ExperienceId,
        ShopPresentation Presentation);
    [Fact]
    public void PublicConstructorTakesAnImmutableCatalogSnapshot()
    {
        var owner = EconomyEntityId.New();
        var products = new List<CommerceItem>
        {
            new(CommerceItemId.New(), owner, "Original item", new Money(2m, "GLD"))
        };

        var shop = new EconomyShop(Guid.NewGuid(), owner, "  Snapshot Shop  ", products);
        products.Add(new CommerceItem(
            CommerceItemId.New(), owner, "Later item", new Money(3m, "GLD")));

        Assert.Equal("Snapshot Shop", shop.Name);
        Assert.Single(shop.Products);
        Assert.True(Assert.IsAssignableFrom<IList<CommerceItem>>(shop.Products).IsReadOnly);
    }

    [Fact]
    public void ShopRejectsProductsOwnedByAnotherSeller()
    {
        var owner = EconomyEntityId.New();
        var otherSeller = EconomyEntityId.New();
        var product = new CommerceItem(
            CommerceItemId.New(), otherSeller, "Foreign product", new Money(2m, "GLD"));

        Assert.Throws<ArgumentException>(() =>
            new EconomyShop(Guid.NewGuid(), owner, "Shop", new[] { product }));
    }

    [Fact]
    public void ShopRejectsDuplicateProductIdentities()
    {
        var owner = EconomyEntityId.New();
        var productId = CommerceItemId.New();
        var products = new[]
        {
            new CommerceItem(productId, owner, "First", new Money(2m, "GLD")),
            new CommerceItem(productId, owner, "Second", new Money(3m, "GLD"))
        };

        Assert.Throws<ArgumentException>(() =>
            new EconomyShop(Guid.NewGuid(), owner, "Shop", products));
    }

    [Fact]
    public void ShopRejectsInvalidIdentityAndNegativeProductPrice()
    {
        var owner = EconomyEntityId.New();
        Assert.Throws<ArgumentException>(() =>
            new EconomyShop(Guid.Empty, owner, "Shop", Array.Empty<CommerceItem>()));
        Assert.Throws<ArgumentException>(() =>
            new CommerceItem(
                CommerceItemId.New(), owner, "Invalid price", new Money(-1m, "GLD")));
    }

}
