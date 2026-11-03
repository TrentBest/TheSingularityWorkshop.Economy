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

The current package now provides a process-local ledger primitive for simulated value movement, but the complete market purchase described here still needs atomic inventory consequences and seller/offer availability checks. The example remains a design requirement for that broader commerce workflow.

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
| Amount and unit | [Money.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/Money.cs) | Decimal amount, normalized three-letter code, and same-code arithmetic |
| Fictional currency identity and amount | [SimulationLedger.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/SimulationLedger.cs) | Typed identity and amount that cannot be confused merely because two currencies share a display symbol |
| Simulated balances and journal | [SimulationLedger.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/SimulationLedger.cs) | Process-local signed postings, explicit issuance/retirement, two-account transfers, balanced multi-account settlement, and derived balances; no durable storage |
| Deterministic blackjack rules | [BlackjackRules.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/BlackjackRules.cs) | Pure completed-hand evaluation, ace adjustment, natural comparison, and gross payout decision; no shoe, random source, player-action state machine, or ledger writes |
| Casino cashier and wager accounting | [CasinoWagerBook.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/CasinoWagerBook.cs) | One-for-one wallet/chip transfers, stake escrow, loss/push/win settlement, separate payouts owed, and one-time claims; no game rules, RNG, persistence, or real-money boundary |
| Blackjack round lifecycle | [BlackjackRound.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/BlackjackRound.cs) | Consumes an explicit ordered shoe, applies Hit/Stand and H17 dealer policy, and returns a decision; no RNG, wager settlement, or ledger dependency |
| Host settlement integration | [BlackjackWagerIntegrationTests.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/BlackjackWagerIntegrationTests.cs) | Host explicitly connects buy-in, wager placement, deterministic round decision, atomic resolution, and payout claim; no durable session or UI integration |
| Economic actor identity | [EconomyEntity.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/EconomyEntity.cs) | Entity identity vocabulary |
| Account boundary | [EconomyAccount.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/EconomyAccount.cs) | Owner and currency identity; no balance storage |
| Offer description | [Commerce.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/Commerce.cs) | Item/seller/name/price and marketplace offer data |
| Shop catalog | [EconomyShop.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/EconomyShop.cs) | Shop identity and read-only snapshot of product references |
| Transfer intent | [Transfer.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/Transfer.cs) | Non-negative amount and matching account/amount currency codes |
| Fee calculation | [FeePolicy.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/FeePolicy.cs) | Calculates a configured share of a gross amount; does not collect it |
| Execution observation | [EconomicExecutionResult.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/EconomicExecutionResult.cs) | Records operation/transfer IDs, reporting environment, status, timestamp, and optional external reference |

The implementation supplies a process-local simulation ledger with derived balances, atomic in-memory multi-account postings, and idempotent operation identities. The casino accounting slice builds on that primitive: wallet/chip buy-in and cash-out are transfers; a wager moves into escrow; a loss moves escrow to the house reserve; a push or win creates a payable; and a claim transfers the payable to player chips. The house funds only net winnings, while gross payout includes the returned stake. The initial BlackjackRules evaluator now supplies a deterministic outcome from complete caller-provided hands; it does not shuffle, draw, or manage player actions, and its first payout schedule is even-money with no natural bonus. The game host owns card order and round lifecycle, and now has an integration-tested path from a completed decision through wager resolution and payout claim. None of these process-local records survive restart, and there is no crash recovery, distributed atomicity, durable reconciliation, inventory custody, or completed city-wide commerce workflow.

### Assumptions and limits

This chapter uses a simple market with one seller, one currency, fixed asking prices, and no taxes, fees, bargaining, credit, production, or uncertainty. It is a teaching model, not a prediction of real market behavior.

Economic models depend on assumptions about actor knowledge, preferences, constraints, institutional rules, and available alternatives. A simulation should state these assumptions and make them changeable where the research question requires it. Do not treat one agent policy as a universal theory of human behavior.

### Determinism, rules, and economic settlement

A game outcome and an economic posting are different facts. The blackjack round engine consumes an explicitly ordered shoe and applies declared action rules; it does not choose a random sequence or move simulated currency. The resulting decision describes whether the wager won, lost, or pushed and what gross payout the rules specify. A separate wager-book operation moves value through escrow and payouts owed. This separation makes the outcome reproducible and prevents a visual game action from becoming an unreviewed balance mutation. A complete production system would still need a trusted source of shuffled shoe order, durable round history, auditability, and clear disclosure of the rules.

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
- Review [Economy tests](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/EconomyTests.cs) for the invariants that are actually exercised.
- Follow [issue #14](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/14) for the authoritative fictional-currency and casino simulation slice.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
