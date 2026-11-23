# Economy API reference index

## 01 — What

This index inventories the public domain types currently declared by the Economy library. Source is authoritative; this page is a navigation aid and documentation-coverage record, not a substitute for exact signatures.

## 02 — Why

A package is easier to learn and maintain when every public type has a discoverable purpose, contract, test trail, and structural neighborhood. The current API is still small enough to inventory explicitly.

## 03 — How

### Public type inventory

The current source scope is the 25 C# files under src/TheSingularityWorkshop.Economy. There are 118 public declarations in that scope.

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
| CurrencyModelDefinition.cs | CurrencyModelAspect, CurrencyModelIssue, CurrencyModelDefinition | Planned | Planned |
| CurrencyModelRuntime.cs | CurrencyModelEventKind, CurrencyModelEvent, CurrencyModelRuntime | Planned | Planned |
| Dice.cs | DiceDefinition, IDiceRandomSource, SystemDiceRandomSource, DiceRoll, DiceRoller | [Dice activity contract](../DICE_ACTIVITY.md) | Planned |
| DiceRules.cs | DiceRuleKind, DiceRuleDefinition, DiceRuleEvaluation, DiceRuleEvaluator | [Dice activity contract](../DICE_ACTIVITY.md) | Planned |
| Transfer.cs | TransferKind, EconomyTransfer | Planned | Planned |
| SimulationLedger.cs | SimulatedCurrencyId, SimulatedCurrency, SimulatedAmount, SimulationAccount, SimulationLedgerOperationKind, SimulationLedgerEntry, SimulationLedgerPosting, SimulationLedgerReceipt, SimulationLedger | [SimulationLedger API](SimulationLedger.md) | [SimulationLedger UML](../UML/SimulationLedgerUML.md) |
| CasinoWagerBook.cs | CasinoCashier, CasinoWagerOutcome, CasinoWagerBook, CasinoWager | [Casino and ledger API](SimulationLedger.md#casino-custody-and-wager-lifecycle) | [Casino wager UML](../UML/CasinoWagerBookUML.md) |
| BlackjackRules.cs | BlackjackSuit, BlackjackRank, BlackjackCard, BlackjackHandValue, BlackjackRules, BlackjackDecision | [Blackjack rules API](SimulationLedger.md#blackjack-rules) | [Blackjack rules UML](../UML/BlackjackRulesUML.md) |
| BlackjackRound.cs | BlackjackShoe, BlackjackRoundState, BlackjackRound | [Blackjack round API](SimulationLedger.md#blackjack-round-lifecycle) | [Blackjack round UML](../UML/BlackjackRoundUML.md) |
| BlackjackSession.cs | BlackjackSession | [Blackjack session API](SimulationLedger.md#blackjack-session-coordination) | [Blackjack session UML](../UML/BlackjackSessionUML.md); [opening protocol](../UML/BlackjackSessionOpeningUML.md) |
| Competition.cs | CompetitionId, CompetitorId, TeamMembershipId, CompetitorKind, CompetitionFormat, CompetitionStatus, Competitor, TeamMember, Team, Competition | [Competition and team contracts](../UML/CompetitionUML.md) | [Competition and team UML](../UML/CompetitionUML.md) |
| TournamentBracket.cs | TournamentMatchId, TournamentResultId, TournamentMatchResult, TournamentMatchStatus, TournamentMatch, SingleEliminationBracket | [Single-elimination bracket contract](../UML/TournamentBracketUML.md) | [Tournament bracket UML](../UML/TournamentBracketUML.md) |
| CompetitionLadder.cs | LadderId, LadderResultId, CompetitionOutcome, LadderResult, LadderStanding, CompetitionLadder | [Competition and ladder contracts](../UML/CompetitionUML.md) | [Competition and ladder UML](../UML/CompetitionUML.md) |
| RoundRobinSchedule.cs | RoundRobinMatchId, RoundRobinOutcome, RoundRobinResultId, RoundRobinMatchResult, RoundRobinMatch, RoundRobinTieBreakCriterion, RoundRobinStandingsPolicy, RoundRobinStanding, RoundRobinSchedule | [Round-robin schedule and standings contract](../UML/RoundRobinScheduleUML.md) | [Round-robin schedule UML](../UML/RoundRobinScheduleUML.md) |
| CapabilityComposition.cs | CapabilityId, CapabilityMaturity, CapabilityConfigurationValueType, CapabilityConfigurationField, CapabilityDescriptor, CapabilityCatalog, CapabilitySelection, CapabilityCompositionIssue, CapabilityCompositionValidation, ExperienceCapabilityComposition | [Capability catalog and composition contract](../UML/CapabilityCompositionUML.md) | [Capability composition UML](../UML/CapabilityCompositionUML.md) |
| ProjectFinance.cs | CapitalProjectId, ProjectCostLineId, ProjectFundingSourceId, ProjectFundingCommitmentId, ProjectCostRecordId, ProjectInvoiceId, ProjectPaymentId, CapitalProject, ProjectBudgetLine, ProjectBudgetBaseline, ProjectFundingSource, ProjectFundingCommitment, ProjectCostRecord, ProjectInvoice, ProjectPayment | [Project finance API](ProjectFinance.md) | [Project finance UML](../UML/ProjectFinanceUML.md) |

**Coverage status:** the combined SimulationLedger API page covers its ledger and casino declarations; the Dice activity contract documents the dice-specific surface; the ProjectFinance API page documents the initial project/budget/funding/cost/invoice/payment vocabulary. Individual pages remain incomplete for the wider public surface. No repository-wide documentation generator is claimed.

### Current contract references

- [SimulationLedger API](SimulationLedger.md) — typed simulated currency, wager accounting, blackjack rules, round lifecycle, and the host session/settlement seam.
- [Competition and team contracts](../UML/CompetitionUML.md) — competitor identity, immutable team rosters, competition registration, capacity, lifecycle, and Elo-style ladder ratings.
- [Tournament bracket contract](../UML/TournamentBracketUML.md) — deterministic single-elimination seeding, byes, validated winner advancement, optional result identity/timestamp/evidence-reference provenance, and immutable bracket snapshots.
- [Round-robin schedule and standings contract](../UML/RoundRobinScheduleUML.md) — deterministic pairings, odd-field byes, host-supplied result provenance, configurable decimal scoring, and explicit aggregate tie-break order (default 3/1/0).
- [Capability catalog and composition contract](../UML/CapabilityCompositionUML.md) — optional descriptor discovery, dependency/conflict checks, and Experience-owned configuration validation.
- [Economy architecture](../ARCHITECTURE.md) — responsibility boundaries and maturity.
- [Getting started](../GETTING_STARTED.md) — first shop construction.
- [Theory learning path](../THEORY/README.md) — domain reasoning independent of the API.
- [Dice activity contract](../DICE_ACTIVITY.md) — injected randomness, deterministic rule evaluation, authoritative outcomes, and presentation/wager boundaries.

## 04 — See it in a minute

To inspect the exact implementation, open the [source directory](https://github.com/TrentBest/TheSingularityWorkshop.Economy/tree/development/src/TheSingularityWorkshop.Economy) and the [test directory](https://github.com/TrentBest/TheSingularityWorkshop.Economy/tree/development/tests/TheSingularityWorkshop.Economy.Tests). Tests are evidence for specific invariants; they do not imply that every public type has exhaustive behavioral coverage.

## 05 — Choose your next step

- Start with [SimulationLedger](SimulationLedger.md) for the newest API contract.
- Use the [UML inventory](../UML/README.md) to see structural-documentation coverage.
- Expand the remaining type pages in dependency order, keeping each claim linked to source and tests.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
