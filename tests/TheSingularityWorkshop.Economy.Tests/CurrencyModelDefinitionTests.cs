using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class CurrencyModelDefinitionTests
{
    private static SimulatedCurrency Currency() =>
        new(new SimulatedCurrencyId(Guid.Parse("a2d4f4b5-1b2c-4c3d-8e9f-0123456789ab")), "Simoleans", "§", 0);

    [Fact]
    public void ValidSimoleansModelComposesIssuanceEarningAndSinks()
    {
        var model = new CurrencyModelDefinition(
            "cartoon-town.simoleans",
            1,
            Currency(),
            new[]
            {
                CurrencyModelAspect.Issuance,
                CurrencyModelAspect.EarningSources,
                CurrencyModelAspect.Sinks,
                CurrencyModelAspect.PeerTransfers
            },
            issuanceAuthority: "town-treasury",
            earningSources: new[] { "employment.wages", "quest.rewards" },
            sinks: new[] { "shops.purchases", "public-services.fees" });

        Assert.Empty(model.Validate());
        Assert.False(model.AllowsExternalRedemption);
        Assert.Equal("cartoon-town.simoleans", model.ModelId);
        Assert.Equal(1, model.SchemaVersion);
        Assert.Equal("Simoleans", model.Currency.Name);
        Assert.Equal(2, model.EarningSources.Count);
        Assert.Equal(2, model.Sinks.Count);
    }

    [Fact]
    public void RequiredAspectsReportMissingConfiguration()
    {
        var model = new CurrencyModelDefinition(
            "cartoon-town.simoleans",
            1,
            Currency(),
            new[]
            {
                CurrencyModelAspect.Issuance,
                CurrencyModelAspect.EarningSources,
                CurrencyModelAspect.Sinks,
                CurrencyModelAspect.Expiry
            });

        var issues = model.Validate();

        Assert.Contains(issues, issue => issue.Code == "missing-issuance-authority");
        Assert.Contains(issues, issue => issue.Code == "missing-earning-source");
        Assert.Contains(issues, issue => issue.Code == "missing-sink");
        Assert.Contains(issues, issue => issue.Code == "missing-expiry-duration");
    }

    [Fact]
    public void ExpiryAspectRequiresPositiveDuration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CurrencyModelDefinition(
            "test.currency", 1, Currency(), new[] { CurrencyModelAspect.Expiry }, expiryAfterDays: 0));
    }

    [Fact]
    public void ExpiryDurationCannotBeConfiguredWithoutSelectingExpiryAspect()
    {
        var model = new CurrencyModelDefinition(
            "test.currency", 1, Currency(), Array.Empty<CurrencyModelAspect>(), expiryAfterDays: 30);

        Assert.Contains(model.Validate(), issue => issue.Code == "unexpected-expiry-duration");
    }

    [Fact]
    public void DuplicateAspectsAndIdentifiersAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new CurrencyModelDefinition(
            "test.currency", 1, Currency(),
            new[] { CurrencyModelAspect.Sinks, CurrencyModelAspect.Sinks }));

        Assert.Throws<ArgumentException>(() => new CurrencyModelDefinition(
            "test.currency", 1, Currency(),
            new[] { CurrencyModelAspect.EarningSources },
            earningSources: new[] { "jobs.wages", "jobs.wages" }));
    }

    [Fact]
    public void ModelIdentityAndSchemaVersionAreValidated()
    {
        Assert.Throws<ArgumentException>(() => new CurrencyModelDefinition(
            " test.currency", 1, Currency(), Array.Empty<CurrencyModelAspect>()));

        Assert.Throws<ArgumentOutOfRangeException>(() => new CurrencyModelDefinition(
            "test.currency", 0, Currency(), Array.Empty<CurrencyModelAspect>()));
    }
}
