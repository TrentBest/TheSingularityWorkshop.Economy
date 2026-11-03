# Economy API reference index

## 01 — What

This index inventories the public domain types currently declared by the Economy library. Source is authoritative; this page is a navigation aid and documentation-coverage record, not a substitute for exact signatures.

## 02 — Why

A package is easier to learn and maintain when every public type has a discoverable purpose, contract, test trail, and structural neighborhood. The current API is still small enough to inventory explicitly.

## 03 — How

### Public type inventory

The current source scope is the 17 C# files under src/TheSingularityWorkshop.Economy. There are 57 public declarations in that scope.

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
| BlackjackRules.cs | BlackjackSuit, BlackjackRank, BlackjackCard, BlackjackHandValue, BlackjackRules, BlackjackDecision | [Blackjack rules API](SimulationLedger.md#blackjack-rules) | [Blackjack rules UML](../UML/BlackjackRulesUML.md) |
| BlackjackRound.cs | BlackjackShoe, BlackjackRoundState, BlackjackRound | [Blackjack round API](SimulationLedger.md#blackjack-round-lifecycle) | [Blackjack round UML](../UML/BlackjackRoundUML.md) |
| BlackjackSession.cs | BlackjackSession | [Blackjack session API](SimulationLedger.md#blackjack-session-coordination) | [Blackjack session UML](../UML/BlackjackSessionUML.md); [opening protocol](../UML/BlackjackSessionOpeningUML.md) |
| Competition.cs | CompetitionId, CompetitorId, TeamMembershipId, CompetitorKind, CompetitionFormat, CompetitionStatus, Competitor, TeamMember, Team, Competition | [Competition and team contracts](../UML/CompetitionUML.md) | [Competition and team UML](../UML/CompetitionUML.md) |
| TournamentBracket.cs | TournamentMatchId, TournamentMatchStatus, TournamentMatch, SingleEliminationBracket | [Single-elimination bracket contract](../UML/TournamentBracketUML.md) | [Tournament bracket UML](../UML/TournamentBracketUML.md) |

**Coverage status:** one combined API page currently explains the 9 declarations in SimulationLedger.cs; 48 declarations in the other source files have no API page yet. This is not per-type completion: individual type pages remain to be created for all 57 public declarations. No repository-wide documentation generator is claimed.

### Current contract references

- [SimulationLedger API](SimulationLedger.md) — typed simulated currency, wager accounting, blackjack rules, round lifecycle, and the host session/settlement seam.
- [Competition and team contracts](../UML/CompetitionUML.md) — competitor identity, immutable team rosters, competition registration, capacity, and lifecycle.
- [Tournament bracket contract](../UML/TournamentBracketUML.md) — deterministic single-elimination seeding, byes, validated winner advancement, and immutable bracket snapshots.
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
