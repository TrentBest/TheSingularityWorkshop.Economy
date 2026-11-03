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
    private static readonly CapabilityCatalog Catalog = new(new[]
    {
        Treasury, Shops, Production, Training, Taxation, Registration, ScoreSubmissions, RegionalAggregation
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

    private static CapabilityDescriptor Descriptor(string id, string category, string description,
        IEnumerable<CapabilityId>? dependencies = null) =>
        new(new CapabilityId(id), id, category, description, "1.0",
            CapabilityMaturity.Experimental, dependencies: dependencies);
}
