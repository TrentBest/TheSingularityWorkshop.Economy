# Economy theory — learning path

## 01 — What

This is the modular theory index for reasoning about economic systems in simulated worlds and creator commerce. It teaches the domain behind the Economy package; it is not a claim that the repository implements a complete economic engine.

The learning path is deliberately separable from the API reference. A reader should be able to understand the concepts before opening a C# file, then trace each concept to the exact implementation—or to an explicit gap.

## 02 — Why

Economic vocabulary is easy to misuse. A price is not a value judgment shared by every participant; a listing is not a sale; a transfer instruction is not a ledger entry; a simulated balance is not redeemable money.

The chapters build a common reasoning framework so design decisions, tests, and future simulation behavior can be challenged rather than accepted as unexplained rules.

## 03 — How

Read the chapters in order. Each chapter distinguishes conceptual theory, assumptions, current code, evidence, and open questions.

| Order | Chapter | Learning objective | Status |
|---|---|---|---|
| 01 | [Value, scarcity, and exchange](01-VALUE-SCARCITY-AND-EXCHANGE.md) | Understand choice, opportunity cost, exchange, price, and the limits of a shared unit of account | Initial chapter |
| 02 | Incentives, constraints, and decision models | Explain how rules and constraints shape actor choices | Planned |
| 03 | Money, units, and currency identity | Distinguish a numeric amount, a unit code, a currency system, and a claim on value | Planned |
| 04 | Accounts, transfers, and double-entry reasoning | Understand custody, conservation, issuance, sinks, and auditable records | Planned; implementation prerequisite |
| 05 | Markets, prices, supply, and demand | Explore price formation and why a listing is not proof of available stock | Planned |
| 06 | Labor, wages, and household choice | Model work, compensation, needs, and budget-constrained purchases | Planned |
| 07 | Businesses, costs, revenue, and contribution | Separate gross receipts, costs, margins, and policy calculations | Planned |
| 08 | Simulation, measurement, and emergent behavior | Design reproducible agents and evaluate scale without confusing activity with economic validity | Planned |

## 04 — See it in a minute

The current source is a deliberately small starting point:

- Money represents a decimal amount and a normalized three-letter code.
- EconomyAccount identifies an owner and currency boundary, but does not store a balance.
- EconomyTransfer validates an intended movement between accounts, but does not move value.
- EconomicExecutionResult records a reported environment and status, but does not prove settlement or maintain a ledger.

The first chapter maps these boundaries to the economic ideas they can—and cannot—represent.

## 05 — Choose your next step

- Start with [Chapter 01: Value, scarcity, and exchange](01-VALUE-SCARCITY-AND-EXCHANGE.md).
- For current code, see [Architecture](../ARCHITECTURE.md) and the [documentation index](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/DOCUMENTATION_INDEX.md).
- For the intended authoritative fictional-currency slice, see [Singularity currency and casino contract](../SINGULARITY_CURRENCY_AND_CASINO.md).


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
