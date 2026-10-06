using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class EconomyTests
{
    [Fact]
    public void MoneyRejectsMixedCurrencies()
    {
        var usd = new Money(10m, "usd");
        var eur = new Money(10m, "eur");
        Assert.Throws<InvalidOperationException>(() => usd.Add(eur));
    }

    [Fact]
    public void FeePolicyIsExplicitAndDoesNotAlterGrossAmount()
    {
        var gross = new Money(100m, "USD");
        var policy = new FeePolicy(0.10m);
        Assert.Equal(10m, policy.Calculate(gross).Amount);
        Assert.Equal(100m, gross.Amount);
    }

    [Fact]
    public void TransferCanRepresentDonationGiftOrRewardWithoutProviderKnowledge()
    {
        var from = new EconomyAccount(EconomyEntityId.New(), "USD");
        var to = new EconomyAccount(EconomyEntityId.New(), "USD");
        var transfer = new EconomyTransfer(Guid.NewGuid(), from, to, new Money(25m, "USD"), TransferKind.Donation);
        Assert.Equal(TransferKind.Donation, transfer.Kind);
        Assert.Equal(25m, transfer.Amount.Amount);
    }

    [Fact]
    public void CommerceItemCanReferenceDigitalRepresentation()
    {
        var item = new CommerceItem(
            CommerceItemId.New(),
            EconomyEntityId.New(),
            "Garage-sale bicycle",
            new Money(75m, "USD"),
            new Uri("https://example.invalid/item"),
            "microbundle://bicycle");
        Assert.Equal("microbundle://bicycle", item.MicroBundleReference);
    }

    [Fact]
    public void FinancialConnectionIsProviderBoundaryNotProviderImplementation()
    {
        var connection = new ExternalFinancialConnection(
            Guid.NewGuid(),
            EconomyEntityId.New(),
            new FinancialProviderId("sandbox-bank"),
            "external-account-1",
            DateTimeOffset.UtcNow);
        Assert.Equal("sandbox-bank", connection.Provider.Value);
    }

    [Fact]
    public void Shop_CanComposeOwnerNameAndProducts()
    {
        var owner = EconomyEntityId.New();
        var sword = new CommerceItem(CommerceItemId.New(), owner, "Iron Sword", new Money(50m, "GLD"));
        var potion = new CommerceItem(CommerceItemId.New(), owner, "Healing Potion", new Money(10m, "GLD"));

        var shop = EconomyShop.Create(owner, "The Adventurer's Supply", new[] { sword, potion });

        Assert.Equal("The Adventurer's Supply", shop.Name);
        Assert.Equal(owner, shop.Owner);
        Assert.Equal(2, shop.Products.Count);
        Assert.Contains(sword, shop.Products);
        Assert.Contains(potion, shop.Products);
    }

    [Fact]
    public void Shop_CanExposeDigitalRepresentationForCreatorDefinedProduct()
    {
        var owner = EconomyEntityId.New();
        var armor = new CommerceItem(
            CommerceItemId.New(),
            owner,
            "Dragon Armor",
            new Money(500m, "GLD"),
            new Uri("https://example.invalid/dragon-armor"),
            "armor-microbundle");

        var shop = EconomyShop.Create(owner, "Dragonforge Armory", new[] { armor });

        Assert.Equal("Dragon Armor", shop.Products.Single().Name);
        Assert.Equal("armor-microbundle", shop.Products.Single().MicroBundleReference);
    }
}
