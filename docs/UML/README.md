# Economy UML and evidence inventory

## 01 — What

This index records which structural and behavioral views exist for the current Economy source. The Workshop standard requires an explicit inventory; a missing diagram must be visible rather than silently implied complete.

## 02 — Why

A class diagram alone cannot explain an economic operation. Type neighborhoods, operation sequences, control flow, lifecycle, use cases, architecture, and test traceability answer different questions and need different evidence.

## 03 — How

| View family | Required scope | Current status | Evidence / next action |
|---|---|---|---|
| Class/type diagrams | Each public declaration in source scope | Partial | [SimulationLedger neighborhood](SimulationLedgerUML.md); [casino custody and wager lifecycle](CasinoWagerBookUML.md); [deterministic blackjack evaluation](BlackjackRulesUML.md); [blackjack round lifecycle](BlackjackRoundUML.md); [blackjack session coordination](BlackjackSessionUML.md); [session opening failure boundary](BlackjackSessionOpeningUML.md) documents the implemented staged-opening protocol; [competition and team primitives](CompetitionUML.md) documents competitor identity, immutable rosters, registration, and lifecycle; [single-elimination bracket](TournamentBracketUML.md) documents deterministic seeding, byes, and winner advancement; CasinoWagerBook is the second focal neighborhood, BlackjackRules the third, BlackjackRound the fourth, and BlackjackSession the fifth; the remaining declarations do not yet have dedicated focal neighborhoods |
| Class neighborhoods | Consumers above, focal type centered, dependencies below, package references to the right | Partial | SimulationLedger, CasinoWagerBook, BlackjackRules, BlackjackRound, BlackjackSession, and the Competition/Team and single-elimination bracket neighborhoods exist; remaining focal types are planned |
| Sequence diagrams | Important workflows and failure paths | Partial | [Blackjack host settlement sequence](BlackjackWagerIntegrationUML.md) covers the tested decision-to-ledger path; add lower-level ledger posting and wager failure sequences |
| Activity/control-flow diagrams | Nontrivial branches, validation, loops, and exceptions | Planned | Begin with transfer validation and atomic commit path |
| State/lifecycle diagrams | Stateful domain lifecycles | Planned / applicability review | Casino wager lifecycle is documented and process-local; durable lifecycle recovery remains absent |
| Use cases | Actor goals, preconditions, success, alternatives, evidence | Partial | SimulationLedger and casino wager use cases documented; remaining public types need use cases where meaningful |
| Architecture/component diagrams | Project boundaries and dependency direction | Present, manual | [Architecture guide](../ARCHITECTURE.md); target diagram is authored conceptual architecture, not a generated dependency graph |
| Deployment/runtime topology | Actual deployed runtime nodes | Not applicable to current library runtime | Economy is a library; the Pages site is documentation hosting, not a deployed economic runtime |
| Data models and data flow | Ownership, journal postings, provenance, transformations | Partial | SimulationLedger entries and signed posting flow documented; durable storage and reconciliation are absent |
| MicroBundle views | Locally defined or consumed capability contracts | No local definitions identified | CommerceItem may carry a MicroBundle reference; this repository does not define a MicroBundle runtime |
| Requirement/test traceability | Contract and issue to tests/evidence | Partial | [Simulation ledger tests](https://github.com/TrentBest/TheSingularityWorkshop.Economy/tree/development/tests/TheSingularityWorkshop.Economy.Tests); expand against issue #14 and #16 |
| Decision/constraint records | Important invariants and rationale | Partial | [Singularity currency contract](../SINGULARITY_CURRENCY_AND_CASINO.md); formal ADR set not yet established |

### Provenance rules

- **Source-derived:** relationships verified from the current source declarations.
- **Test-supported:** behavior exercised by a named test.
- **Human-authored:** use cases and design intent grounded in product goals.
- **Conceptual:** proposed architecture or workflow, not proof of implementation.
- **Runtime-captured:** reserved for observations from a named, reproducible execution; none are claimed by the current static diagrams.

No Roslyn-based repository-wide UML generator, reflection inventory, relationship coverage report, or stale-artifact CI gate is implemented. Current pages are manually maintained and their scope is declared.

## 04 — See it in a minute

The [SimulationLedger neighborhood](SimulationLedgerUML.md), [casino custody/wager lifecycle](CasinoWagerBookUML.md), [deterministic blackjack rules](BlackjackRulesUML.md), [round lifecycle](BlackjackRoundUML.md), [competition/team primitives](CompetitionUML.md), and [single-elimination tournament bracket](TournamentBracketUML.md) are the current focal views. The [host settlement sequence](BlackjackWagerIntegrationUML.md) traces the integration test from buy-in through payout claim; [BlackjackSession](BlackjackSessionUML.md) adds the tested session boundary that binds the round to its unresolved wager; [session opening](BlackjackSessionOpeningUML.md) is an implemented staged protocol that documents the remaining cross-resource failure window. The competition/team view covers the first provider-neutral organized-play foundation; the bracket view now implements single-elimination seeding, byes, and advancement. Double elimination, Swiss, round robin, and fantasy scoring remain future work. The blackjack diagram distinguishes pure hand evaluation from caller-owned card acquisition and ledger settlement. The casino view distinguishes implemented accounting transitions from game rules, persistence, and runtime integration that remain future work.

## 05 — Choose your next step

- Read the [API type inventory](../API/README.md).
- Inspect [SimulationLedger source](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/SimulationLedger.cs) and [tests](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/SimulationLedgerTests.cs).
- Expand the remaining UML views only where they add distinct evidence.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
