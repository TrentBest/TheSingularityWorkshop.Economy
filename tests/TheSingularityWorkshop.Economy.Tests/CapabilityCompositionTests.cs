using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class CapabilityCompositionTests
{
    private static CapabilityDescriptor Capability(
        string id,
        IEnumerable<CapabilityId>? dependencies = null,
        IEnumerable<CapabilityId>? conflicts = null,
        IEnumerable<CapabilityConfigurationField>? configuration = null) =>
        new(new CapabilityId(id), id, "simulation", $"The {id} capability.", "1.0",
            CapabilityMaturity.Experimental, dependencies, conflicts, configuration, $"docs/capabilities/{id}.md");

    [Fact]
    public void CatalogProvidesStableLookupAndImmutableDescriptorSnapshots()
    {
        var dependency = Capability("ledger");
        var shop = Capability("shops", new[] { dependency.Id });
        var catalog = new CapabilityCatalog(new[] { dependency, shop });

        Assert.Equal(2, catalog.Descriptors.Count);
        Assert.True(catalog.TryGet(new CapabilityId("SHOPS"), out var found));
        Assert.Equal(shop, found);
        Assert.False(catalog.TryGet(new CapabilityId("unknown"), out _));
        Assert.Throws<ArgumentException>(() => new CapabilityCatalog(new[] { dependency, dependency }));
    }

    [Fact]
    public void CatalogRejectsUnresolvedDependencyAndConflictReferences()
    {
        var missing = new CapabilityId("missing");
        Assert.Throws<ArgumentException>(() => new CapabilityCatalog(new[]
        {
            Capability("shops", new[] { missing })
        }));
        Assert.Throws<ArgumentException>(() => new CapabilityCatalog(new[]
        {
            Capability("simulation", conflicts: new[] { missing })
        }));
    }

    [Fact]
    public void CompositionCanSelectNoCapabilitiesOrOnlyOneOptionalCapability()
    {
        var ledger = Capability("ledger");
        var shops = Capability("shops", new[] { ledger.Id });
        var catalog = new CapabilityCatalog(new[] { ledger, shops });

        Assert.True(new ExperienceCapabilityComposition().Validate(catalog).IsValid);

        var independent = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(ledger.Id)
        });
        Assert.True(independent.Validate(catalog).IsValid);
    }

    [Fact]
    public void CompositionReportsMissingDependenciesAndUnknownCapabilities()
    {
        var ledger = Capability("ledger");
        var shops = Capability("shops", new[] { ledger.Id });
        var catalog = new CapabilityCatalog(new[] { ledger, shops });
        var composition = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(shops.Id),
            new CapabilitySelection(new CapabilityId("not-in-catalog"))
        });

        var result = composition.Validate(catalog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "missing-dependency");
        Assert.Contains(result.Issues, issue => issue.Code == "unknown-capability");
    }

    [Fact]
    public void CompositionRejectsDeclaredConflicts()
    {
        var realProvider = Capability("provider-live");
        var simulation = Capability("provider-simulation", conflicts: new[] { realProvider.Id });
        var catalog = new CapabilityCatalog(new[] { realProvider, simulation });
        var composition = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(realProvider.Id),
            new CapabilitySelection(simulation.Id)
        });

        var result = composition.Validate(catalog);

        Assert.Contains(result.Issues, issue => issue.Code == "conflicting-capability");
    }

    [Fact]
    public void CompositionValidatesRequiredAndTypedConfiguration()
    {
        var descriptor = Capability("blackjack", configuration: new[]
        {
            new CapabilityConfigurationField("dealerHitsSoft17", CapabilityConfigurationValueType.Boolean, required: true),
            new CapabilityConfigurationField("deckCount", CapabilityConfigurationValueType.Integer, required: true),
            new CapabilityConfigurationField("payoutMode", CapabilityConfigurationValueType.Enumeration,
                allowedValues: new[] { "even-money", "custom" })
        });
        var catalog = new CapabilityCatalog(new[] { descriptor });
        var invalid = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(descriptor.Id, new Dictionary<string, string>
            {
                ["dealerHitsSoft17"] = "sometimes",
                ["deckCount"] = "one",
                ["unexpected"] = "value"
            })
        });

        var result = invalid.Validate(catalog);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Issues.Count);
        Assert.Contains(result.Issues, issue => issue.Code == "invalid-configuration");
        Assert.Contains(result.Issues, issue => issue.Code == "unknown-configuration");
    }

    [Fact]
    public void CompositionAcceptsValidConfigurationAndKeepsItExperienceOwned()
    {
        var descriptor = Capability("kingdom-tax", configuration: new[]
        {
            new CapabilityConfigurationField("rate", CapabilityConfigurationValueType.Decimal, required: true),
            new CapabilityConfigurationField("model", CapabilityConfigurationValueType.Enumeration,
                allowedValues: new[] { "flat", "progressive" })
        });
        var catalog = new CapabilityCatalog(new[] { descriptor });
        var selection = new CapabilitySelection(descriptor.Id, new Dictionary<string, string>
        {
            ["rate"] = "0.125",
            ["model"] = "progressive"
        });
        var composition = new ExperienceCapabilityComposition(new[] { selection });

        Assert.True(composition.Validate(catalog).IsValid);
        Assert.Equal("0.125", composition.Selections.Single().Configuration["rate"]);
    }

    [Fact]
    public void DescriptorAndSelectionRejectInvalidOrDuplicateMetadata()
    {
        Assert.Throws<ArgumentException>(() => new CapabilityId("bad id"));
        Assert.Throws<ArgumentException>(() => new CapabilityConfigurationField("mode",
            CapabilityConfigurationValueType.Enumeration));
        Assert.Throws<ArgumentException>(() => Capability("ledger", dependencies: new[]
        {
            new CapabilityId("ledger")
        }));
        var id = new CapabilityId("shops");
        Assert.Throws<ArgumentException>(() => new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(id),
            new CapabilitySelection(id)
        }));
    }
}
