![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# ✳️ 00 The Singularity Workshop — Economy

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet version](https://img.shields.io/nuget/v/TheSingularityWorkshop.Economy?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/TheSingularityWorkshop.Economy)
[![NuGet downloads](https://img.shields.io/nuget/dt/TheSingularityWorkshop.Economy?logo=nuget&style=flat-square)](https://www.nuget.org/packages/TheSingularityWorkshop.Economy)
[![Build and tests](https://img.shields.io/github/actions/workflow/status/TrentBest/TheSingularityWorkshop.Economy/dotnet-tests.yml?branch=development&style=flat-square&logo=github)](https://github.com/TrentBest/TheSingularityWorkshop.Economy/actions/workflows/dotnet-tests.yml)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Economy/development/docs/assets/economy-boundary.svg" alt="An Experience owns its presentation and policies; Economy provides shared commerce vocabulary, while simulation and any future real-provider execution remain distinct." width="100%">
</p>

**Economy is an exploratory, provider-neutral domain package for describing exchange in simulated worlds and creator commerce. It is not a financial service.**

## 🟦 01 What

Economy gives an Experience shared concepts for money-like values, products, shops, offers, transfer intent, and reported execution outcomes. An RPG, simulation, creator storefront, or marketplace can reuse this vocabulary without handing over its UI, inventory, or world rules.

The first major proving ground is **Singularity City**: a simulated economy populated by Digitens, with businesses and creator-owned shops that can eventually be reused, reskinned, and composed into different Experiences. In-world economic behavior must be testable without a real-money provider.

## 🟣 02 Why

Each application can invent its own shop and offer models, but those models are harder to reuse together. Economy explores a small shared language so an Experience can describe an economic concept once and retain control over its own rules and presentation.

## 🩵 03 How

Keep four things distinct: **offer, intent, execution, and outcome**. A shop is not a checkout; a transfer intent is not a payment; a reported outcome is not automatically proof of settlement.

Simulation is intended to work without a payment provider. Real-money execution is a separate future capability requiring explicit policy, provider-specific integration, secure operations, reconciliation, and professional review. See the [architecture guide](docs/ARCHITECTURE.md) for what exists now and what remains proposed.

## 🟢 04 See it in a minute

Install the package into a compatible .NET project:

```bash
dotnet add package TheSingularityWorkshop.Economy
```

This source-checked example models a shop using fictional in-world currency `GLD`; it does not execute a payment or move money.

```csharp
using System;
using TheSingularityWorkshop.Economy;

var owner = EconomyEntityId.New();

var sword = new CommerceItem(
    CommerceItemId.New(), owner, "Iron Sword", new Money(50m, "GLD"));
var potion = new CommerceItem(
    CommerceItemId.New(), owner, "Healing Potion", new Money(10m, "GLD"));

var shop = EconomyShop.Create(owner, "The Adventurer's Supply", new[] { sword, potion });
Console.WriteLine($"{shop.Name}: {shop.Products.Count} items");
```

Expected output:

```text
The Adventurer's Supply: 2 items
```

The example follows the shop/product composition covered by the [domain tests](tests/TheSingularityWorkshop.Economy.Tests/EconomyTests.cs). It has been checked against current source but has not been independently compiled or run as a standalone sample.

## 🟪 05 Available documentation and theory

- [Simulated economies and creator commerce](docs/SIMULATED_ECONOMIES_AND_CREATOR_COMMERCE.md) — Singularity City, Digiten-scale measurement, reusable/reskinnable creator shops, adoption, and the first vertical slice.
- [Singularity currency and casino contract](docs/SINGULARITY_CURRENCY_AND_CASINO.md) — non-redeemable in-world currency, casino chip custody, wager lifecycle, house accounting, and the first authoritative simulation slice.
- [Exploratory Economy Experiences](docs/EXPLORATORY_ECONOMY_EXPERIENCES.md) — casino back-of-house exploration, simulated camera feeds, courtesy bankroll, free transit and paid transport jobs, and multiplayer building construction.
- [What is Economy?](docs/WHAT_IS_ECONOMY.md) — build the conceptual model and understand the intended boundary without starting from API details.
- [Economy architecture](docs/ARCHITECTURE.md) — inspect current types, ownership boundaries, and the proposed path from simulation to any future provider-backed operations.
- [Game store patterns](docs/GAME_STORE_PATTERNS.md) — current API examples and Experience-owned store rules.
- [Documentation index](DOCUMENTATION_INDEX.md) — navigate the source, tests, and workflow by task.
- [Domain tests](tests/TheSingularityWorkshop.Economy.Tests/EconomyTests.cs) and [execution-result tests](tests/TheSingularityWorkshop.Economy.Tests/EconomicExecutionResultTests.cs) — inspect the invariants currently exercised.
- [Build and test workflow](.github/workflows/dotnet-tests.yml) — see the automated restore, build, and test commands.

## Ecosystem fit and status

Economy should remain independently useful. Workshop experiences may compose it with other contracts, but no Workshop package is required just to describe a shop or an economic concept. The Experience owns its UI, inventory, gameplay rules, and execution policy.

**Current status: exploratory alpha.** The package does not provide a production storefront, cart, inventory system, ledger, payment processor, wallet, settlement service, live provider adapter, or compliance determination. Creator workflows and near-term revenue models remain hypotheses to validate, not implemented capabilities or promises of income. Real-money integration is deliberately separate from proving the simulated economy.

---

<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://avatars.githubusercontent.com/u/16405167?v=4" alt="Trent Best — The Singularity Workshop" width="64">
  </a>
  <br>
  <a href="https://github.com/TrentBest">GitHub — The Singularity Workshop</a>
  ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.Economy">Economy source</a>
</p>

<p align="center">
  <a href="https://github.com/TrentBest/WebPage" aria-label="Visit The Singularity Workshop">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex of information converging on its central eye" width="112">
  </a>
  <br>
  <strong>The Singularity Workshop</strong>
  <br>
  <em>Tools for the curious, the bold, and the systemically inclined.</em>
</p>
