# Economy Gallery

The Gallery collects small, inspectable examples that demonstrate a specific Economy boundary rather than imply that the complete economic system already exists. Each exhibit links to its source, tests, reproduction path, and explicit limitations.

## 01 — What

The current gallery begins with creator-shop composition: one canonical shop can be referenced by two Experience placements with different presentation skins.

## 02 — Why

A reusable shop should keep its economic identity when a host Experience changes its visual treatment. This boundary helps separate the creator's catalog from the experience-specific presentation.

## 03 — How

The exhibit is a consumer-owned test fixture. The test defines its own ExperienceShopPlacement and ShopPresentation records; those are not public Economy API.

## 04 — See it in a minute

- [Creator shop composition proof](CREATOR_SHOP_COMPOSITION_PROOF.md)
- [CreatorShopCompositionTests.cs](../tests/TheSingularityWorkshop.Economy.Tests/CreatorShopCompositionTests.cs)
- [EconomyShop.cs](../src/TheSingularityWorkshop.Economy/EconomyShop.cs)

Reproduce it with:

    dotnet test tests/TheSingularityWorkshop.Economy.Tests/TheSingularityWorkshop.Economy.Tests.csproj --configuration Release --filter FullyQualifiedName~CreatorShopCompositionTests

**What this proves:** the test fixture preserves one canonical shop and product identity across two consumer-owned placements.

**What this does not prove:** persistent creator projects, a visual editor, a production storefront, WebPage integration, stock management, transaction execution, or a running city economy.

## 05 — Choose your next step

- Learn the concept in [Getting started](GETTING_STARTED.md).
- Read [Game store patterns](GAME_STORE_PATTERNS.md) for Experience-owned catalog rules.
- Follow the [Economy theory path](THEORY/README.md) for the economic concepts behind future simulations.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
