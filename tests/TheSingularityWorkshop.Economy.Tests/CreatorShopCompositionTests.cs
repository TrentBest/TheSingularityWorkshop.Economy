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
                new Money(3m, "CITY_CREDITS")),
            new CommerceItem(
                CommerceItemId.New(),
                owner,
                "Handmade ceramic cup",
                new Money(12m, "CITY_CREDITS"))
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
            new Money(1m, "CITY_CREDITS"));
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
}
