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
}
