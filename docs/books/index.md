![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Economy — SURPS Sourcebook Catalogue

This is the canonical local catalogue for Economy-owned domain sourcebooks. The [SURPS Master Rulebook Map](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/docs/rulebook-backlog/index.md) is authoritative for global book identity, ownership sign-in, and repository participation.

**Repository status:** integrated, not enrolled. Four sourcebooks are draft domain contracts; two are proposed roadmaps. A proposed book is not evidence that the described experience or capability is implemented.

## SURPS living rulebook and behavior-adapter standard

This repository is adopting the shared SURPS [Living Rulebook and Experience Standard](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/docs/living-rulebook-standard.md) and [Behavior Model and Adapters](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/docs/behavior-and-adapters.md). Economy owns accounts, ledgers, markets, production, labor, logistics, and transactional invariants. Paper economic procedures should be derived from the same economic behavior model used by digital experiences.

The standard is the target for new work; full conformance remains in progress until the linked issue is accepted. It requires a big-picture-first guide, concise Player path, deep Game Master/Workshop path, living pictures for major concepts, worked demonstrations for mechanics, and honest status labels. The tabletop adapter is derived from underlying behavior during workshop calibration; the digital adapter uses compiled behavior with live state. Neither is a separate source of truth. Adoption work: [repository issue](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/33).

| Sourcebook | LOD | Status | Descriptor | Cover |
|---|---:|---|---|---|
| [Economic Ontology and Composable Models](../ECONOMIC_ONTOLOGY_AND_COMPOSABLE_MODELS.md) | 1 | Draft | [Descriptor](economic-ontology-and-composable-models.book.json) | [Cover](covers/economic-ontology-and-composable-models.svg) |
| [Singularity Currency and Casino Contract](../SINGULARITY_CURRENCY_AND_CASINO.md) | 2 | Draft | [Descriptor](singularity-currency-and-casino.book.json) | [Cover](covers/singularity-currency-and-casino.svg) |
| [Teams, Leagues, Tournaments, and Organized Play](../THEORY/03-TEAMS-LEAGUES-AND-TOURNAMENTS.md) | 2 | Draft | [Descriptor](organized-play.book.json) | [Cover](covers/organized-play.svg) |
| [Project Finance and Capital Planning](../PROJECT_FINANCE_AND_CAPITAL_PLANNING.md) | 2 | Draft | [Descriptor](project-finance-and-capital-planning.book.json) | [Cover](covers/project-finance-and-capital-planning.svg) |
| [Markets and Economic Playgrounds](../MARKETS_AND_PLAYGROUNDS.md) | 2 | Proposed | [Descriptor](markets-and-economic-playgrounds.book.json) | [Cover](covers/markets-and-economic-playgrounds.svg) |
| [Simulated Economies, Creator Shops, and Adoption](../SIMULATED_ECONOMIES_AND_CREATOR_COMMERCE.md) | 2 | Proposed | [Descriptor](simulated-economies-and-creator-commerce.book.json) | [Cover](covers/simulated-economies-and-creator-commerce.svg) |

## Ownership boundaries

- **Economy:** provider-neutral economic vocabulary, fictional-currency identity, offers and transfer intents, simulation-ledger semantics, organized-play administration, and economic sourcebooks.
- **Experience hosts:** UI, inventory meaning, game-specific rules, economic policy, authorization, and execution choices.
- **Singularity City:** resident/person identity, civic place, shops as places, and resident-business relationships.
- **Physics:** physical models, resource properties, and physical production constraints.
- **FSM_Execution:** workflow lifecycle, cancellation, checkpointing, and execution.
- **SURPS:** shared roleplaying ontology, CORE rules, global book identity, LOD semantics, and opt-in composition.
- **Trusted providers and hosts:** authentication, authorization, credentials, and any future external execution.

## Simulation versus real-money execution

Fictional-currency issuance, transfer, wagers, and casino accounting are simulation concepts. They do not create redeemable value, payment processing, a wallet, a bank, or a settlement service. Real-money execution is a separate future capability requiring explicit policy, provider-specific integration, secure operations, reconciliation, and professional review.

## Draft versus proposed

The draft books describe domain contracts and current implementation boundaries. The proposed books are roadmap and proof-plan material; their presence in the catalogue does not assert that a stock exchange, creator storefront, persistent city economy, or production financial service exists.

## Validation

The canonical machine-readable declaration is [`.surps/participation.json`](../../.surps/participation.json). Run the local integrity gate before changing the catalogue:

```bash
python scripts/validate_surps_participation.py
```

The [SURPS Sourcebook Validation workflow](../../.github/workflows/surps-sourcebook-validation.yml) runs this check for relevant changes on `development`. A passing local check validates identity and asset consistency; it does not accept ownership claims or enroll the repository.
