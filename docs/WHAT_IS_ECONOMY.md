# What is Economy?

Economy is an exploratory, provider-neutral .NET domain package for describing economic concepts across simulated worlds and creator-led commerce.

It gives experiences a shared vocabulary for **who offers something, what is offered, what it costs, and what exchange is intended**—without requiring every game, simulation, storefront, or marketplace to invent its own incompatible models.

## Start with something familiar

Imagine an RPG with a blacksmith:

```text
Dragonforge Armory
  Owner: Blacksmith Guild
  Products:
    Iron Sword      — 50 GLD
    Dragon Armor    — 500 GLD
    Repair Kit      — 15 GLD
```

The game already owns its world, characters, inventory, art, quests, and rules. Economy can describe the shop and its products without taking over those responsibilities. `GLD` is a fictional in-world unit in this example; it is not a real currency or payment.

The same shared vocabulary can describe a creator's shop, a digital service offer, a marketplace listing, or a reward in a simulation. The package should make interoperability easier without requiring an Experience to adopt the entire Workshop.

## What the current package provides

The current source includes small domain primitives for:

- **Money** — a decimal amount and normalized three-letter unit/currency code, with same-code arithmetic.
- **Commerce items and offers** — a seller, name, price, optional external reference, optional MicroBundle reference, and a marketplace offer.
- **Economy shops** — an owner, name, and read-only snapshot of product references.
- **Accounts and transfer intent** — an owner/currency boundary and a validated description of intended value movement.
- **Fee calculation** — a platform-share calculation, not fee collection.
- **Provider and access contracts** — early abstractions, not a working financial integration.
- **Execution observations** — a value describing a reported operation state and environment, not an executor or proof of settlement.

See [Architecture](ARCHITECTURE.md) for the source-checked inventory and the distinction between implemented types and the intended larger system.

## The four meanings that must stay separate

1. **Offer** — what a seller makes available under stated terms.
2. **Intent** — what a participant requests or agrees should happen.
3. **Execution** — a simulation or provider adapter attempts an operation.
4. **Outcome and records** — what can be substantiated after the attempt and any required reconciliation.

Creating a shop, offer, or transfer intent does not mean a buyer paid, a seller received funds, stock was reserved, or a provider accepted the operation. A reported “completed” state is not automatically proof of settlement or legal finality.

## Two execution environments, one vocabulary

**Simulation** should be useful on its own. A game can use fictional units, rewards, crafting costs, trading rules, and deterministic scenarios without payment credentials, a bank account, network access, or a real-money provider.

**Real-money connections** are a separate future capability. They require deliberately selected provider adapters, explicit permissions, secure credentials, operation identity and idempotency, reliable outcome handling, records and reconciliation, operational safeguards, and qualified professional review appropriate to the actual service and jurisdictions.

Adding this package to an Experience must never silently enable real-money operations. The present repository does not implement a production checkout, ledger, payment processor, wallet, settlement system, or compliance determination.

## Creator freedom and ecosystem fit

Economy is intended to remain independently useful. A creator may use its domain vocabulary without adopting FSM_COS or any other Workshop package. An Experience owns its presentation, inventory, policies, and interaction design; Economy does not prescribe a storefront UI or the rules of every virtual world.

Compatible Workshop contracts may make future composition and discovery easier, but this repository does not promise a marketplace, revenue, provider access, or operational financial capability.

## Why the boundary matters

The goal is to prepare clear concepts before making operational commitments. A domain library can help organize questions and engineering responsibilities; it cannot establish that a financial workflow is legally viable or compliant. Any real-money launch requires independent assessment by the appropriate providers and qualified legal, accounting, compliance, security, and other professionals.

## Current status

**Exploratory alpha.** The current code is useful as a starting point for shared vocabulary and creator-defined shop composition. It is not a production commerce or real-money platform.

For exact contracts and the proposed implementation sequence, read [Economy architecture](ARCHITECTURE.md). For a map of the repository's guides, read [Documentation index](../DOCUMENTATION_INDEX.md).
