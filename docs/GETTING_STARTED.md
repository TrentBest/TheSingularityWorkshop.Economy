# Getting started with Economy

## 01 — What

This guide creates a small, fictional shop using the current public Economy API. It is a first contact with the package's commerce vocabulary—not a checkout tutorial, wallet setup, inventory implementation, or payment integration.

Economy currently describes money-like values, commerce items, shops, offers, transfer intent, and reported execution observations. It does not maintain balances or execute a purchase.

## 02 — Why

A game or creator experience should not need to invent a new shape for every seller, item, and asking price. A shared vocabulary makes those descriptions easier to pass between independently owned experiences.

The boundary is intentional: the Experience still owns its presentation, stock, gameplay rules, purchase validation, and what happens after a successful exchange.

## 03 — How

### Prerequisites

- .NET 8 SDK.
- A project that can reference TheSingularityWorkshop.Economy.
- Familiarity with basic C# records and object construction.

The repository currently targets .NET 8 through its shared build properties. The package project sets GeneratePackageOnBuild to false; this guide does not ask you to publish a package.

### Create a shop

Add the package reference to your own compatible project:

    dotnet add package TheSingularityWorkshop.Economy

Then create a seller and two catalog items:

    using System;\n    using TheSingularityWorkshop.Economy;

    var owner = EconomyEntityId.New();

    var sword = new CommerceItem(
        CommerceItemId.New(),
        owner,
        "Iron Sword",
        new Money(50m, "GLD"));

    var potion = new CommerceItem(
        CommerceItemId.New(),
        owner,
        "Healing Potion",
        new Money(10m, "GLD"));

    var shop = EconomyShop.Create(
        owner,
        "The Adventurer's Supply",
        new[] { sword, potion });

    Console.WriteLine($"{shop.Name}: {shop.Products.Count} items");

GLD is a fictional three-letter unit used for this example. It has no real-world denomination, redemption, exchange rate, or monetary value.

### Understand the objects

- EconomyEntityId identifies the seller in the domain vocabulary.
- CommerceItemId gives each catalog item an identity.
- Money stores a decimal amount and a normalized three-letter code. Arithmetic rejects mismatched codes.
- CommerceItem describes a seller's item, name, and asking price, with optional external and MicroBundle references.
- EconomyShop.Create captures a read-only snapshot of the supplied product references.

This does not create inventory, check a customer's balance, reserve stock, transfer value, or record a completed sale.

## 04 — See it in a minute

Expected console output:

    The Adventurer's Supply: 2 items

The construction pattern is covered by [EconomyTests](../tests/TheSingularityWorkshop.Economy.Tests/EconomyTests.cs). The creator-owned composition boundary is covered separately by [CreatorShopCompositionTests](../tests/TheSingularityWorkshop.Economy.Tests/CreatorShopCompositionTests.cs).

**Evidence status:** source-checked against development source commit `06d2bc1202d226284e5ac6ef52d5b55699d33dd0`; the snippet is not a separately built sample project. Run the repository test project to verify the current implementation:

    dotnet test tests/TheSingularityWorkshop.Economy.Tests/TheSingularityWorkshop.Economy.Tests.csproj --configuration Release

## 05 — Choose your next step

- Learn the domain boundary in [What is Economy?](WHAT_IS_ECONOMY.md).
- Follow the [theory learning path](THEORY/README.md) to understand exchange, value, scarcity, and incentives.
- Inspect exact current responsibilities in [Architecture](ARCHITECTURE.md).
- Explore reusable store composition in [Game store patterns](GAME_STORE_PATTERNS.md).
- Browse the [Gallery](GALLERY.md) for a source-backed composition proof.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
