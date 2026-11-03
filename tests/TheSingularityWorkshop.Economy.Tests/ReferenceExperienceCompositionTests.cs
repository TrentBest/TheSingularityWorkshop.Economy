using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

/// <summary>Executable metadata fixtures proving distinct Experiences can select from one shared catalog.</summary>
public sealed class ReferenceExperienceCompositionTests
{
    private static readonly CapabilityDescriptor Treasury = Descriptor("treasury", "Foundations", "Fictional-value accounts and balanced simulation postings.");
    private static readonly CapabilityDescriptor Shops = Descriptor("shops", "Commerce", "Quotes, reservations, purchases, and fulfillment.", new[] { Treasury.Id });
    private static readonly CapabilityDescriptor Production = Descriptor("production", "Industry", "Material inputs, yields, waste, and produced inventory.", new[] { Treasury.Id });
    private static readonly CapabilityDescriptor Training = Descriptor("training", "Progression", "Training costs, time, capacity, and unit progression.", new[] { Treasury.Id });
    private static readonly CapabilityDescriptor Taxation = Descriptor("taxation", "Government", "Scenario-owned tax policy and treasury receipts.", new[] { Treasury.Id });
    private static readonly CapabilityDescriptor Registration = Descriptor("competition.registration", "Competition", "Competitor registration and eligibility.");
    private static readonly CapabilityDescriptor ScoreSubmissions = Descriptor("competition.scores", "Competition", "Validated score submissions with provenance.", new[] { Registration.Id });
    private static readonly CapabilityDescriptor RegionalAggregation = Descriptor("competition.regions", "Competition", "Aggregation of eligible scores by an Experience-assigned region.", new[] { ScoreSubmissions.Id });
    private static readonly CapabilityDescriptor Employment = Descriptor("society.employment", "Society", "Occupations, employment agreements, skills, and wage obligations.", new[] { Treasury.Id });
    private static readonly CapabilityDescriptor PropertyTitle = Descriptor("society.property-title", "Society", "Asset identity, title records, ownership interests, and transfer history.");
    private static readonly CapabilityDescriptor IncidentRecords = Descriptor("society.incidents", "Society", "Reports and provenance for alleged theft, fraud, and other incidents.");
    private static readonly CapabilityDescriptor JusticeCases = Descriptor("society.justice", "Society", "Evidence references, investigations, findings, appeals, and remedies.", new[] { IncidentRecords.Id });
    private static readonly CapabilityDescriptor InstitutionalIntegrity = Descriptor("society.integrity", "Society", "Bribery, conflicts of interest, favoritism, and oversight policy.", new[] { JusticeCases.Id });
    private static readonly CapabilityDescriptor CivicRegistry = Descriptor("society.civic-registry", "Society", "Public registries, permits, and civic records.");
    private static readonly CapabilityCatalog Catalog = new(new[]
    {
        Treasury, Shops, Production, Training, Taxation, Registration, ScoreSubmissions, RegionalAggregation,
        Employment, PropertyTitle, IncidentRecords, JusticeCases, InstitutionalIntegrity, CivicRegistry
    });

    [Fact]
    public void RpgKingdomSelectsCommerceAndSimulationWithoutCompetitionFeatures()
    {
        var kingdom = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(Treasury.Id), new CapabilitySelection(Shops.Id),
            new CapabilitySelection(Production.Id), new CapabilitySelection(Training.Id),
            new CapabilitySelection(Taxation.Id)
        });
        Assert.True(kingdom.Validate(Catalog).IsValid);
        Assert.Equal(new[] { "treasury", "shops", "production", "training", "taxation" },
            kingdom.Selections.Select(selection => selection.CapabilityId.Value));
        Assert.DoesNotContain(kingdom.Selections, selection => selection.CapabilityId == Registration.Id);
    }

    [Fact]
    public void GlobalScoreCompetitionSelectsRegionalScoresWithoutCommerceFeatures()
    {
        var competition = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(Registration.Id), new CapabilitySelection(ScoreSubmissions.Id),
            new CapabilitySelection(RegionalAggregation.Id)
        });
        Assert.True(competition.Validate(Catalog).IsValid);
        Assert.Equal(new[] { "competition.registration", "competition.scores", "competition.regions" },
            competition.Selections.Select(selection => selection.CapabilityId.Value));
        Assert.DoesNotContain(competition.Selections, selection => selection.CapabilityId == Shops.Id);
        Assert.DoesNotContain(competition.Selections, selection => selection.CapabilityId == Treasury.Id);
    }

    [Fact]
    public void ReferenceExperiencesShareCatalogWithoutSharingEveryCapability()
    {
        var kingdom = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(Treasury.Id), new CapabilitySelection(Shops.Id)
        });
        var competition = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(Registration.Id), new CapabilitySelection(ScoreSubmissions.Id)
        });
        Assert.True(kingdom.Validate(Catalog).IsValid);
        Assert.True(competition.Validate(Catalog).IsValid);
        Assert.NotEqual(kingdom.Selections.Select(x => x.CapabilityId).ToHashSet(),
            competition.Selections.Select(x => x.CapabilityId).ToHashSet());
    }

    [Fact]
    public void FeudalKingdomCanComposeHereditaryPropertyAndCorruptCourt()
    {
        var feudalKingdom = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(PropertyTitle.Id),
            new CapabilitySelection(IncidentRecords.Id),
            new CapabilitySelection(JusticeCases.Id),
            new CapabilitySelection(InstitutionalIntegrity.Id)
        });

        Assert.True(feudalKingdom.Validate(Catalog).IsValid);
        Assert.Contains(feudalKingdom.Selections, item => item.CapabilityId == PropertyTitle.Id);
        Assert.Contains(feudalKingdom.Selections, item => item.CapabilityId == InstitutionalIntegrity.Id);
        Assert.DoesNotContain(feudalKingdom.Selections, item => item.CapabilityId == Employment.Id);
    }

    [Fact]
    public void ModernCityCanComposeEmploymentPropertyRegistryAndOversight()
    {
        var modernCity = new ExperienceCapabilityComposition(new[]
        {
            new CapabilitySelection(Treasury.Id),
            new CapabilitySelection(Employment.Id),
            new CapabilitySelection(PropertyTitle.Id),
            new CapabilitySelection(CivicRegistry.Id),
            new CapabilitySelection(IncidentRecords.Id),
            new CapabilitySelection(JusticeCases.Id),
            new CapabilitySelection(InstitutionalIntegrity.Id)
        });

        Assert.True(modernCity.Validate(Catalog).IsValid);
        Assert.Contains(modernCity.Selections, item => item.CapabilityId == Employment.Id);
        Assert.Contains(modernCity.Selections, item => item.CapabilityId == CivicRegistry.Id);
        Assert.DoesNotContain(modernCity.Selections, item => item.CapabilityId == Shops.Id);
    }
    private static CapabilityDescriptor Descriptor(string id, string category, string description,
        IEnumerable<CapabilityId>? dependencies = null) =>
        new(new CapabilityId(id), id, category, description, "1.0",
            CapabilityMaturity.Experimental, dependencies: dependencies);
}
