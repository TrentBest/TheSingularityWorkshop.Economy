# 01 — Value, scarcity, and exchange

## 01 — What

An economy is a system in which people or agents make choices about resources, goods, services, effort, and claims. The starting problem is not money. It is **choice under constraints**.

A resource is scarce when not every desired use can be satisfied at once. Scarcity does not mean that an object is rare in an absolute sense; it means that choosing one use can limit another. Time, attention, materials, productive capacity, and access can all be scarce.

**Value** describes the importance an actor assigns to an outcome in a particular context. It is not automatically an objective property stored inside an item. Two participants can value the same item differently because they have different needs, knowledge, alternatives, or constraints.

**Exchange** occurs when participants voluntarily or institutionally transfer goods, services, effort, or claims under agreed rules. An exchange needs more than a price label: it needs parties, terms, authority, and a process for determining whether the promised action occurred.

## 02 — Why

A simulation that gives every citizen an unlimited budget, unlimited time, and access to every good has little reason to model economic choice. A simulation becomes interesting when agents face trade-offs.

Consider a Digiten who has one free hour and needs food and a repaired tool. Spending the hour working may earn fictional currency, but it delays both purchases. Visiting one shop may save travel time but offer a higher price. Choosing one option changes what remains possible.

This is the idea of **opportunity cost**: the value of the best alternative that is given up when a choice is made. Opportunity cost is not necessarily a cash payment. It may be time, risk, convenience, or a foregone alternative.

Prices can help coordinate choices by expressing terms of exchange in a common unit. But a posted price does not reveal every participant's preferences, costs, alternatives, or ability to pay. Nor does it prove that an item is in stock or that a transaction has completed.

## 03 — How

### A small worked model

Imagine a fictional market with these conditions:

| Item | Asking price | Available quantity |
|---|---:|---:|
| Bread | 3 GLD | 4 |
| Repair kit | 8 GLD | 1 |

A Digiten has 10 GLD and needs both items.

The posted prices total 11 GLD, so the Digiten cannot buy both from this seller at the current terms. The actor must choose: buy bread now, buy the repair kit, seek another seller, earn more, or defer one need.

The model requires more than a price field to simulate a purchase correctly:

1. The actor has an authoritative balance in the same fictional currency system.
2. The seller and item are available, and the quantity is still sufficient.
3. The actor's choice is checked against current terms at the time of purchase.
4. The operation has a stable identity so a retry cannot charge twice.
5. A successful operation records the buyer debit, seller credit, and inventory consequence together.
6. A rejected operation leaves all economic state unchanged.

Those are design requirements for a future deterministic simulation slice, not capabilities provided by the current package.

### Distinguish the economic objects

- **Need or preference:** a reason an actor may want an outcome.
- **Resource or good:** something constrained that can satisfy a need or be used in production.
- **Offer:** the terms under which a seller makes something available.
- **Price:** the stated amount and unit requested for an exchange.
- **Intent:** a request to perform a specified exchange.
- **Execution:** an attempt to carry out the intent.
- **Record:** durable evidence of the state change and its provenance.

A price is not a balance. A balance is not inventory. A transfer intent is not a completed transfer. Keeping these concepts separate makes failure behavior testable.

### Map the theory to the current implementation

| Concept | Current source mapping | What the code currently guarantees |
|---|---|---|
| Amount and unit | [Money.cs](../../src/TheSingularityWorkshop.Economy/Money.cs) | Decimal amount, normalized three-letter code, and same-code arithmetic |
| Economic actor identity | [EconomyEntity.cs](../../src/TheSingularityWorkshop.Economy/EconomyEntity.cs) | Entity identity vocabulary |
| Account boundary | [EconomyAccount.cs](../../src/TheSingularityWorkshop.Economy/EconomyAccount.cs) | Owner and currency identity; no balance storage |
| Offer description | [Commerce.cs](../../src/TheSingularityWorkshop.Economy/Commerce.cs) | Item/seller/name/price and marketplace offer data |
| Shop catalog | [EconomyShop.cs](../../src/TheSingularityWorkshop.Economy/EconomyShop.cs) | Shop identity and read-only snapshot of product references |
| Transfer intent | [Transfer.cs](../../src/TheSingularityWorkshop.Economy/Transfer.cs) | Non-negative amount and matching account/amount currency codes |
| Fee calculation | [FeePolicy.cs](../../src/TheSingularityWorkshop.Economy/FeePolicy.cs) | Calculates a configured share of a gross amount; does not collect it |
| Execution observation | [EconomicExecutionResult.cs](../../src/TheSingularityWorkshop.Economy/EconomicExecutionResult.cs) | Records operation/transfer IDs, reporting environment, status, timestamp, and optional external reference |

The implementation does not yet supply authoritative balances, stock custody, a journal/ledger, atomic multi-account posting, idempotent execution, or reconciliation. These are not minor details: without them, the worked model cannot be honestly called an implemented transaction.

### Assumptions and limits

This chapter uses a simple market with one seller, one currency, fixed asking prices, and no taxes, fees, bargaining, credit, production, or uncertainty. It is a teaching model, not a prediction of real market behavior.

Economic models depend on assumptions about actor knowledge, preferences, constraints, institutional rules, and available alternatives. A simulation should state these assumptions and make them changeable where the research question requires it. Do not treat one agent policy as a universal theory of human behavior.

## 04 — See it in a minute

Try the reasoning exercise before reading the answer:

A Digiten has 10 GLD. Bread costs 3 GLD and a repair kit costs 8 GLD. The shop has enough bread but only one repair kit.

- Can the Digiten buy both?
- What is the opportunity cost of buying the repair kit first?
- What additional state must the simulation check before accepting either purchase?
- Why is a successful local UI animation insufficient evidence of a completed purchase?

**Discussion:** The combined price is 11 GLD, so both cannot be purchased from the stated budget. Buying the repair kit leaves 2 GLD and forgoes the bread purchase now. A correct system must check current balance, seller/offer availability, stock, currency identity, and duplicate operation identity, then record the economic and inventory effects atomically. A visual animation is presentation, not authoritative economic evidence.

## 05 — Choose your next step

- Continue to [the theory index](README.md) for the next chapter as it is developed.
- Inspect [Economy architecture](../ARCHITECTURE.md) for the current/future boundary.
- Review [Economy tests](../../tests/TheSingularityWorkshop.Economy.Tests/EconomyTests.cs) for the invariants that are actually exercised.
- Follow [issue #14](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/14) for the authoritative fictional-currency and casino simulation slice.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
