# Reference Experience compositions

**Status: executable composition-contract examples.** The tests build metadata descriptors and validate selections. They do not activate production, taxation, score submission, or regional aggregation runtimes.

The point of these two examples is that one catalog can offer many capabilities while each Experience selects only a small, intentional subset.

## RPG kingdom

A kingdom scenario may select:
- treasury — fictional-value accounting foundation;
- shops — blacksmith and market purchases;
- production — workshops, mines, farms, and material yields;
- training — barracks, guilds, and unit progression;
- taxation — scenario-owned policy for a prosperous kingdom under fiscal strain.

The kingdom does not need tournament registration or regional score aggregation. Narrative conditions (war damage, drought, trade disruption, changed tax policy) belong to the Experience's scenario data and simulation rules; they must not be disguised as unexplained ledger mutations.

```csharp
var kingdom = new ExperienceCapabilityComposition(new[]
{
    new CapabilitySelection(new CapabilityId("treasury")),
    new CapabilitySelection(new CapabilityId("shops")),
    new CapabilitySelection(new CapabilityId("production")),
    new CapabilitySelection(new CapabilityId("training")),
    new CapabilitySelection(new CapabilityId("taxation"))
});

var validation = kingdom.Validate(sharedCatalog);
if (!validation.IsValid)
{
    // The host reports validation issues and does not activate the composition.
}
```

## Global score competition

A Flappy Bird-style competition can instead select:
- competition.registration — entrant eligibility and registration;
- competition.scores — validated score submissions and provenance;
- competition.regions — aggregation by an explicitly assigned region.

The Experience can present regional leaders on a globe, but globe geometry, boundary data, color mapping, and rendering remain in the Experience/Visualize layer. Economy-side regional aggregation is a data contract, not a renderer. This example deliberately does not select treasury, shops, taxation, or production.

```csharp
var competition = new ExperienceCapabilityComposition(new[]
{
    new CapabilitySelection(new CapabilityId("competition.registration")),
    new CapabilitySelection(new CapabilityId("competition.scores")),
    new CapabilitySelection(new CapabilityId("competition.regions"))
});

var validation = competition.Validate(sharedCatalog);
if (!validation.IsValid)
{
    // The host reports validation issues and does not activate the composition.
}
```

## Executable evidence

[`ReferenceExperienceCompositionTests.cs`](../tests/TheSingularityWorkshop.Economy.Tests/ReferenceExperienceCompositionTests.cs) verifies that both compositions validate against one shared catalog, and that neither Experience is forced to select the other's capabilities.

The descriptors in that test are deliberately lightweight fixtures. They establish the shape of composition, not the existence of production implementations for every named capability. Production, training, taxation, score-submission, and regional-aggregation engines remain future work.

## Boundary

- The catalog and composition contracts are declarative and in-memory.
- Selection does not create accounts, issue value, start a competition, or execute a world simulation.
- Dependency closure is currently checked as direct selected dependencies; transitive dependency expansion and cycle detection remain future contract work.
- Hosts own activation, authorization, persistence, and recovery.
- All simulated currency and rewards remain fictional and non-redeemable.