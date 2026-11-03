# Economy API reference index

## 01 — What

This index inventories the public domain types currently declared by the Economy library. Source is authoritative; this page is a navigation aid and documentation-coverage record, not a substitute for exact signatures.

## 02 — Why

A package is easier to learn and maintain when every public type has a discoverable purpose, contract, test trail, and structural neighborhood. The current API is still small enough to inventory explicitly.

## 03 — How

### Public type inventory

The current source scope is the 14 C# files under src/TheSingularityWorkshop.Economy. There are 43 public declarations in that scope.

| Source file | Public declarations | Dedicated API page | UML/use-case companion |
|---|---|---|---|
| AccessLedger.cs | EconomyAccessKind, EconomyAccessRecord, IEconomyAccessRecorder | Planned | Planned |
| Commerce.cs | CommerceItemId, CommerceItem, MarketplaceOffer | Planned | Planned |
| EconomicExecutionResult.cs | EconomicExecutionEnvironment, EconomicExecutionStatus, EconomicExecutionResult | Planned | Planned |
| EconomyAccount.cs | EconomyAccount | Planned | Planned |
| EconomyEntity.cs | EconomyEntityId, EconomyEntityKind | Planned | Planned |
| EconomyShop.cs | EconomyShop | Planned | Planned |
| FeePolicy.cs | FeePolicy | Planned | Planned |
| FinancialProvider.cs | FinancialProviderId, ExternalFinancialConnection, IFinancialProvider | Planned | Planned |
| Money.cs | Money | Planned | Planned |
| Transfer.cs | TransferKind, EconomyTransfer | Planned | Planned |
| SimulationLedger.cs | SimulatedCurrencyId, SimulatedCurrency, SimulatedAmount, SimulationAccount, SimulationLedgerOperationKind, SimulationLedgerEntry, SimulationLedgerPosting, SimulationLedgerReceipt, SimulationLedger | [SimulationLedger API](SimulationLedger.md) | [SimulationLedger UML](../UML/SimulationLedgerUML.md) |
| CasinoWagerBook.cs | CasinoCashier, CasinoWagerOutcome, CasinoWagerBook, CasinoWager | [Casino and ledger API](SimulationLedger.md#casino-custody-and-wager-lifecycle) | [Casino wager UML](../UML/CasinoWagerBookUML.md) |
| BlackjackRules.cs | BlackjackSuit, BlackjackRank, BlackjackCard, BlackjackHandValue, BlackjackRules, BlackjackDecision | [Blackjack rules API](SimulationLedger.md#blackjack-rules) | [Blackjack rules UML](../UML/BlackjackRulesUML.md) |\n| BlackjackRound.cs | BlackjackShoe, BlackjackRoundState, BlackjackRound | [Blackjack round API](SimulationLedger.md#blackjack-round-lifecycle) | [Blackjack round UML](../UML/BlackjackRoundUML.md) |

**Coverage status:** one combined API page currently explains the 9 declarations in SimulationLedger.cs; 24 declarations in the other source files have no API page yet. This is not per-type completion: individual type pages remain to be created for all 43 public declarations. No repository-wide documentation generator is claimed.

### Current contract references

- [SimulationLedger API](SimulationLedger.md) — the first typed simulated-currency ledger.
- [Economy architecture](../ARCHITECTURE.md) — responsibility boundaries and maturity.
- [Getting started](../GETTING_STARTED.md) — first shop construction.
- [Theory learning path](../THEORY/README.md) — domain reasoning independent of the API.

## 04 — See it in a minute

To inspect the exact implementation, open the [source directory](https://github.com/TrentBest/TheSingularityWorkshop.Economy/tree/development/src/TheSingularityWorkshop.Economy) and the [test directory](https://github.com/TrentBest/TheSingularityWorkshop.Economy/tree/development/tests/TheSingularityWorkshop.Economy.Tests). Tests are evidence for specific invariants; they do not imply that every public type has exhaustive behavioral coverage.

## 05 — Choose your next step

- Start with [SimulationLedger](SimulationLedger.md) for the newest API contract.
- Use the [UML inventory](../UML/README.md) to see structural-documentation coverage.
- Expand the remaining type pages in dependency order, keeping each claim linked to source and tests.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
