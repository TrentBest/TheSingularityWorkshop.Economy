using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class CommerceIntegrityTests
{
    [Fact]
    public void CommerceItemNormalizesItsNameAndPreservesItsCanonicalValues()
    {
        var seller = EconomyEntityId.New();
        var id = CommerceItemId.New();
        var item = new CommerceItem(id, seller, "  Handcrafted bowl  ", new Money(12m, "GLD"));

        Assert.Equal(id, item.Id);
        Assert.Equal(seller, item.Seller);
        Assert.Equal("Handcrafted bowl", item.Name);
        Assert.Equal(12m, item.Price.Amount);
        Assert.Equal("GLD", item.Price.Currency);
    }

    [Fact]
    public void CommerceItemRejectsInvalidIdentitySellerNameAndPrice()
    {
        var seller = EconomyEntityId.New();
        var id = CommerceItemId.New();

        Assert.Throws<ArgumentException>(() =>
            new CommerceItem(new CommerceItemId(Guid.Empty), seller, "Item", new Money(1m, "GLD")));
        Assert.Throws<ArgumentException>(() =>
            new CommerceItem(id, new EconomyEntityId(Guid.Empty), "Item", new Money(1m, "GLD")));
        Assert.Throws<ArgumentException>(() =>
            new CommerceItem(id, seller, " ", new Money(1m, "GLD")));
        Assert.Throws<ArgumentException>(() =>
            new CommerceItem(id, seller, "Item", new Money(-1m, "GLD")));
        Assert.Throws<ArgumentException>(() =>
            new CommerceItem(id, seller, "Item", default));
    }

    [Fact]
    public void CommerceItemRejectsAnExplicitlyBlankMicroBundleReference()
    {
        Assert.Throws<ArgumentException>(() =>
            new CommerceItem(
                CommerceItemId.New(),
                EconomyEntityId.New(),
                "Blueprint",
                new Money(1m, "GLD"),
                microBundleReference: "  "));
    }

    [Fact]
    public void MarketplaceOfferRequiresItsOwnIdentityMarketplaceAndItem()
    {
        var item = new CommerceItem(
            CommerceItemId.New(),
            EconomyEntityId.New(),
            "Blueprint",
            new Money(1m, "GLD"));
        var marketplace = EconomyEntityId.New();
        var createdAt = DateTimeOffset.UtcNow;

        var offer = new MarketplaceOffer(Guid.NewGuid(), item, marketplace, createdAt);

        Assert.Same(item, offer.Item);
        Assert.Equal(marketplace, offer.Marketplace);
        Assert.Equal(createdAt, offer.CreatedAt);

        Assert.Throws<ArgumentException>(() =>
            new MarketplaceOffer(Guid.Empty, item, marketplace, createdAt));
        Assert.Throws<ArgumentNullException>(() =>
            new MarketplaceOffer(Guid.NewGuid(), null!, marketplace, createdAt));
        Assert.Throws<ArgumentException>(() =>
            new MarketplaceOffer(Guid.NewGuid(), item, new EconomyEntityId(Guid.Empty), createdAt));
    }
}
