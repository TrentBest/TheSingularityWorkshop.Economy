# Production API

## 01 — What

The production contracts model a declared transformation of economic resources:

- Inputs are materials or other stocks consumed by a run.
- Capacity is a consumable resource such as labor-hours or machine-hours.
- Yield is the declared output of the recipe.
- Waste is an explicitly recorded by-product or loss.
- Provenance ties every consumed and produced quantity to an operation and recipe version.

The current executor is process-local and in-memory. It does not provide durable storage, distributed coordination, scheduling, worker assignment, or authorization.

## 02 — Why

Different Experiences should be able to describe a mine, farm, blacksmith, barracks, or shipyard without the Economy kernel learning those domain-specific meanings. The recipe declares resource identities and quantities; the Experience owns what those resources mean and which recipe is allowed in its world.

## 03 — How

### Define a recipe

```csharp
var ore = new ProductionResourceId("material.iron-ore");
var coal = new ProductionResourceId("material.coal");
var labor = new ProductionResourceId("capacity.blacksmith-hour");
var sword = new ProductionResourceId("item.iron-sword");
var slag = new ProductionResourceId("waste.slag");

var recipe = new ProductionRecipe(
    "blacksmith.iron-sword",
    "1.0",
    inputs: new[]
    {
        new ProductionResourceQuantity(ore, 3m),
        new ProductionResourceQuantity(coal, 2m)
    },
    capacity: new[]
    {
        new ProductionResourceQuantity(labor, 1.5m)
    },
    outputs: new[]
    {
        new ProductionResourceQuantity(sword, 1m)
    },
    waste: new[]
    {
        new ProductionResourceQuantity(slag, 0.5m)
    });
```

### Execute against an inventory

```csharp
var inventory = new ProductionInventory(new[]
{
    new ProductionResourceQuantity(ore, 30m),
    new ProductionResourceQuantity(coal, 20m),
    new ProductionResourceQuantity(labor, 15m)
});

var receipt = inventory.Execute(
    operationId: Guid.NewGuid(),
    recipe: recipe,
    batches: 2,
    occurredAt: DateTimeOffset.UtcNow);
```

The two-batch run consumes 6 ore, 4 coal, and 3 labor-hours; it records 2 swords as yield and 1 slag as waste. The receipt includes a provenance entry for each declared resource flow.

### Contract guarantees

- Resource identifiers are normalized and validated.
- Recipe collections are copied into immutable snapshots and reject duplicate resources within each role.
- Batch scaling and balance arithmetic are checked for decimal overflow.
- A run validates all net inventory changes before committing any balance, receipt, or history entry.
- Insufficient stock or capacity leaves inventory and operation history unchanged.
- Repeating an operation ID with the same recipe definition and batch count returns a replay receipt without producing again.
- Reusing an operation ID for a different recipe definition or batch count is rejected.
- Inputs, capacity, yield, and waste are preserved as separate receipt/provenance roles.

A resource may appear in more than one role. For example, a refining process may consume 5 units of ore and yield 4 units of the same resource identity; the net inventory delta is calculated atomically.

## 04 — Boundaries

- The inventory is an in-memory simulation primitive, not a durable stock ledger.
- Capacity is modeled as a consumable resource. Scheduling, concurrent machine reservations, worker availability, and time windows are not implemented.
- Recipes declare deterministic quantities. Variable yield, quality grades, stochastic loss, and process control belong in a future explicit policy/adapter.
- The receipt records declared resource flows; it does not claim physical causality, independently verified measurement, or external provenance.
- No currency account is credited by production. Creating goods is distinct from issuing fictional currency or recording a sale.
- The host/Experience decides whether a run may be requested. The recipe itself does not grant execution authority.

## 05 — Evidence

- [Production source](../../src/TheSingularityWorkshop.Economy/Production.cs)
- [Production tests](../../tests/TheSingularityWorkshop.Economy.Tests/ProductionTests.cs)
- [Experience capability catalog](../EXPERIENCE_CAPABILITY_CATALOG.md)

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
