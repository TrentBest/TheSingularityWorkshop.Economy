# Experience Capability Catalog and Composition

**Status: product architecture and implementation plan.** This document defines the intended developer experience. It is not a claim that a production capability browser, manifest resolver, editor, or runtime activation service exists today.

## The toy-store principle

Economy should feel like walking into an old toy store: the developer can see what is possible, inspect it, try a small example, combine it with something else, and imagine a world that does not exist yet.

The catalog is not a mandatory bundle of features. It is a shelf of independently useful capabilities. An Experience chooses only the capabilities it needs. One world might use a treasury, shops, crafting, and taxation; another might use shipyards, fuel, cargo, and trade; a third might use tournaments and score aggregation without adopting shops or banking.

**Discover broadly; compose selectively.** The catalog should encourage imagination without making every developer pay the complexity cost of every feature.

## Ownership boundaries

| Concern | Owner |
|---|---|
| Capability identity, reusable economic contracts, transaction invariants, simulation value accounting | Economy |
| Which capabilities are selected, their configuration, activation order, and world-specific policy | Experience / host |
| NPCs, world state, narrative, gameplay, spatial context, and player interaction | Experience |
| Visual editor, cards, maps, globe, color overlays, animation, and rendering | Experience / Visualize |
| Activity rules and adjudication (poker, blackjack, sports, gladiators, creator challenges) | Activity-specific adapter |
| Durable storage, distributed coordination, and production operations | Explicit infrastructure adapters; not implied by in-memory domain contracts |

An Experience must remain valid when an optional capability is absent. A base Experience should not acquire a hidden dependency on Economy merely because another Experience uses a market, tournament, or treasury.

## Capability descriptor

A future catalog entry should describe at least:

- Stable capability ID and version.
- Human-readable name, purpose, category, and discoverable examples.
- Maturity: concept, experimental, preview, or stable.
- Required and optional dependencies, conflicts, and compatible versions.
- Configuration schema, defaults, validation rules, and safe limits.
- Contracts exposed and events emitted/consumed.
- Extension points and policy hooks.
- Required persistence, scheduling, authority, or external services.
- Explicit exclusions and known limitations.
- Documentation, UML, tests, and sample Experience references.

The catalog is descriptive metadata, not permission to execute. Activation belongs to the host and must validate dependencies and authority before the capability participates in an Experience.

## Composition model

The intended composition flow is:

1. Browse the catalog and inspect a capability's behavior, dependencies, maturity, and examples.
2. Select zero or more capabilities for an Experience.
3. Configure each selected capability with Experience-owned policy and scenario data.
4. Validate the composition: required dependencies, incompatible combinations, duplicate providers, missing authority, and unsupported persistence guarantees.
5. Activate the validated set in a host-controlled lifecycle.
6. Observe emitted events and economic outcomes through the Experience's own presentation.
7. Remove or replace optional capabilities without corrupting unrelated world state.

A capability must not silently install every dependency, create accounts, issue value, start a game, or initiate an external operation merely because it was selected. Side effects require explicit host policy and operation calls.

## One transaction kernel, many economic verbs

The shared economic foundation should make different operations understandable and auditable without teaching the ledger the rules of every game.

Examples of named operations include:

- **Production:** consume materials and capacity; create a declared output quantity; record waste, yield, and provenance.
- **Training:** consume currency, materials, time, or capacity; advance a unit's declared training state.
- **Purchase:** quote, reserve availability, accept an order, post value movement, and record fulfillment separately.
- **Upkeep:** charge recurring costs for units, buildings, fleets, equipment, or services.
- **Construction:** reserve budget/materials, consume labor and resources over stages, and produce an accepted asset.
- **Legislation:** model proposal, review, enactment, and explicit administrative/economic costs.
- **Wager or contest entry:** reserve fictional value under declared rules; accept an activity result; settle or void under a stated policy.
- **Reward or grant:** issue fictional value through a named, authorized source with a reason and operation identity.

These are policies and workflows over shared typed parties, accounts, value units, resources, quotes, reservations, postings, and event records. The ledger remains responsible for accounting invariants; it should not know what a blacksmith, barracks, shipyard, city council, or poker table means.

Do not collapse offer, intent, authorization, execution, and outcome into one method or status. Production may create value because an explicitly modeled process created goods; that is not permission to fabricate account balances. Every source and use must be named and traceable.

## Narrative economies are explicit scenario policy

A story may need an economy that was prosperous and is now under strain. The framework should make that easy to author without falsifying its records.

Scenario configuration may set:
- opening treasuries, inventories, debts, obligations, and productive capacity;
- tax, tariff, wage, price, fee, and upkeep policies;
- resource abundance, scarcity, spoilage, and supply constraints;
- demand, trade access, embargoes, routes, and market rules;
- shocks such as drought, war, disaster, discovery, migration, or technological change;
- recovery rules and policy choices available to players or NPC governments.

A scenario preset should explain its initial conditions and causal levers. A famine should emerge from declared scarcity, damaged production, disrupted trade, or policy—not from unexplained negative balances. A kingdom can be intentionally distorted for storytelling while its ledger remains internally coherent.

Keep scenario definitions separate from runtime events. The host can replay, compare, reset, or branch a scenario when the underlying simulation supports those operations.

## The catalog should contain demonstrations, not just API names

A capability is easier to understand when the developer can see it in context. Provide small inspectable reference Experiences, each composed from a deliberately small subset:

| Reference Experience | Example selected capabilities |
|---|---|
| RPG kingdom | treasury, taxation, trade, production, training, upkeep, blacksmith shop |
| Spacefaring economy | shipyard, vessel ownership, fuel, cargo, maintenance, interplanetary trade |
| City simulator | budgets, construction, materials, labor, zoning/districting proposal costs |
| Cantina / casino | cashier, fictional chips, blackjack, poker adapter, tournament administration |
| Arena | competitor registration, match rules adapter, validated results, bracket, fictional rewards |
| Creator challenge | competition registration, score submissions, anti-cheat/provenance adapter, leaderboard |

These are composition examples and roadmap scenarios, not a declaration that all listed capabilities exist.

A lively cantina or casino is a useful first demonstration because people immediately recognize the activities and economic exchanges. It is an invitation to understand the machinery—not the definition of Economy and not a requirement imposed on other Experiences.

## Regional competition and globe visualization

A score competition can define geographic regions, accept validated score records, and aggregate a chosen metric for each region. The Experience may then colorize the regions and wrap that data onto a globe, showing each region's leading score and allowing drill-down to the competition and result evidence.

The responsibilities should remain distinct:

- Competition/result contracts provide participant identity, metric, score, event time, provenance, validation state, and correction history.
- A region-assignment policy maps an eligible participant or submission to a region under explicit privacy and anti-cheat rules.
- Aggregation computes a declared statistic such as regional best score, top-N average, or participation count.
- Experience/Visualize owns geographic boundaries, globe projection, surface texture generation, color mapping, labels, interaction, and rendering.

Do not put a globe renderer in Economy. Do not infer a player's precise location from unrelated data. Region assignment should be opt-in or otherwise explicitly authorized, and public displays should expose only the identity/detail level the Experience has chosen to publish.

## Current proof versus intended platform

Already implemented in the repository are reusable shop and commerce primitives, a process-local typed fictional-currency ledger, creator-shop composition contract tests, casino wager accounting and deterministic blackjack lifecycle, plus generic competitor/team/competition and single-elimination bracket foundations.

The creator-shop composition proof is a test-owned fixture, not a production editor or runtime catalog. Tournament brackets are not yet a full tournament administration service. Poker, fantasy roster policy, production/training workflows, scenario presets, capability discovery/activation, regional aggregation, and globe visualization are not implemented by this document.

## Implementation sequence

1. Finish tournament administration and validated result provenance under issue #21.
2. Define capability descriptor and Experience composition contracts without building a universal editor.
3. Prove optional composition with two sharply different reference Experiences.
4. Extend shared transaction workflows beyond shops and wagers, beginning with production and training.
5. Add explicit scenario configuration and event history for narrative economic distortion.
6. Add region assignment and aggregation to competition results; leave globe rendering to the Experience/Visualize layer.
7. Add activity adapters (poker, sports, gladiators, creator challenges) independently of the shared transaction core.

All simulation value remains fictional and non-redeemable. This architecture does not authorize real-money wagering, prizes, payments, redemption, or donation-to-game-credit conversion.
