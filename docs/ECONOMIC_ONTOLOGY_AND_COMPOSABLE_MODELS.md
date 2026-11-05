# Economic ontology and composable models

> Status: initial design foundation for issue [#25](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/25). This document proposes vocabulary and boundaries; it does not claim that a complete ontology engine or configurable currency builder exists in the current package.

## 1. Why Economy needs an ontology

An Experience should not have to invent a private meaning for every economic idea. A shop, a currency, an account, a resource, a wage, a tax, a market, and a wager should be recognizable concepts that can be inspected, composed, documented, tested, and reused.

The purpose is not to force every world into one economic theory. A cartoon town, a medieval kingdom, a factory simulation, a creator marketplace, and a classroom probability exercise have different goals. They can still share a vocabulary while choosing different assumptions and rules.

An ontology gives us a common map. It does not decide the economics for the creator.

## 2. Four layers that must remain distinct

| Layer | Question it answers | Example |
|---|---|---|
| Economic theory | Why might this system behave this way? | Scarcity changes incentives; opportunity cost shapes choices; supply and demand can influence prices |
| Ontology | What concepts and relationships can the model describe? | Actor, resource, currency, account, offer, market, policy, event |
| Model and policy | Which assumptions and rules did this world choose? | Simoleans are issued for work, accepted by town shops, and retired through selected sinks |
| Execution and evidence | What actually happened, and what records support that claim? | A wage event posted to the simulation journal; a purchase consumed an item and transferred value |

Do not collapse these layers. A theory is not a C# class. An ontology term is not a policy. A configuration is not proof that it was executed. A displayed balance is not an authoritative journal.

## 3. Economic theory is part of the product

Economy must teach the reasoning behind the systems it exposes. The learning path should cover at least:

- **Value and choice:** subjective valuation, trade-offs, opportunity cost, and why one unit of account does not make all preferences comparable.
- **Scarcity and allocation:** finite resources, rival and non-rival goods, rationing, queues, quotas, and abundance.
- **Exchange and institutions:** property rights, contracts, trust, ownership, custody, and enforcement.
- **Money and currency:** unit of account, medium of exchange, store of value, issuance, supply, acceptance, and limits of convertibility.
- **Markets and prices:** supply, demand, price discovery, competition, monopoly power, externalities, and market failure.
- **Production and labor:** inputs, productivity, wages, capital, intermediate goods, consumption, and opportunity cost.
- **Public finance and distribution:** taxes, transfers, public goods, debt, deficits, and distributional effects.
- **Dynamic systems:** inflation, shocks, expectations, feedback loops, cycles, measurement, and unintended consequences.
- **Games and risk:** probability, expected value, variance, house edge, risk limits, and the separation of a game outcome from its economic settlement.

Each chapter should connect the theory to an Experience scenario, identify assumptions, and point to the source/tests that implement it—or clearly mark the gap. The goal is informed customization, not a black box with sliders.

## 4. A first ontology map

The following is a conceptual inventory, not a final class hierarchy. Concepts may be represented by records, discriminated values, policies, or relationships as implementation evidence warrants.

| Concept family | Meaning | Example |
|---|---|---|
| Actor | An entity that can participate in economic relationships | Person, household, business, guild, institution, government |
| Role | A context-specific capacity held by an actor | Worker, seller, buyer, tax authority, casino house |
| Resource | Something scarce, useful, produced, consumed, transferred, or controlled | Food, timber, labor time, a service, a digital item |
| Value unit | A named unit in which a quantity is expressed | Simoleans, casino chips, kilograms, labor-hours |
| Currency definition | A value system with identity and explicit operating rules | Simoleans with a defined issuer and permitted sinks |
| Account | A ledger boundary associated with an owner or custodian and a value unit | Household wallet, business treasury, wager escrow |
| Ownership | A claim or right associated with a resource | A player owns a sword |
| Custody/control | Who currently holds or can operate a resource | A cashier holds casino chips in custody |
| Offer | Terms under which something is made available | A shop offers a potion for 10 Simoleans |
| Market | A mechanism or institution through which offers and exchange are coordinated | Town market, auction, labor market |
| Policy | A named rule set governing a decision or transition | Wage policy, tax policy, price policy, issuance policy |
| Event | A recorded occurrence with participants, time, and provenance | Wage paid, item crafted, tax assessed |
| Observation | A measurement or externally reported fact | Stock count, market quote, visitor count |
| Capability | A separately selectable behavior contract | Currency issuance, auction, production, casino wager |

### Relationships matter as much as entities

Examples of relationships include:

- an actor **owns** or **controls** a resource;
- an actor **holds an account** denominated in a value unit;
- a currency definition **authorizes** an issuance policy;
- a business **offers** a resource under specified terms;
- a market **matches** offers and requests according to a rule;
- a production process **consumes** inputs and **creates** outputs;
- an event **changes** one or more accounts or resource quantities;
- a policy **constrains** which events may be accepted;
- an observation **describes** a state but does not itself mutate it.

We must keep ownership separate from custody, and a reported stock observation separate from an inventory command. Similar distinctions already protect the shop connector and casino accounting boundaries.

## 5. Currency is a composed economic system

A currency should not be represented only by a three-letter string. A code is useful for a compact unit label, but it does not describe how a fictional currency works.

A currency model should be assembled from independently understandable aspects, subject to compatibility validation:

| Aspect | Questions a creator can configure |
|---|---|
| Identity and presentation | What is it called? What symbol, plural form, and localized display should be used? |
| Denomination | What is the smallest unit? Can values be divided, and to what precision? |
| Issuance | Who or what may create units? Is supply fixed, scheduled, earned, or policy-controlled? |
| Earning sources | Which activities create a claim to units—work, quests, production, rewards, grants? |
| Sinks and retirement | Where are units removed—fees, consumption, repairs, taxes, decay, or explicit retirement? |
| Transferability | Can participants transfer units freely, or are transfers limited by role, location, or policy? |
| Expiry/demurrage | Do units expire or lose nominal quantity over time? If so, under what disclosed rule? |
| Acceptance | Which actors, businesses, venues, or markets accept the unit? |
| Exchange | Is exchange against another simulated unit modeled? What rate mechanism and authority apply? |
| Redemption boundary | Is there any external redemption? For fictional currency this should be explicitly absent. |
| Audit and provenance | Which events explain issuance, transfer, and retirement? Who authorized them? |

These aspects are not all universally compatible. For example, an expiry rule needs a time basis and an explicit treatment of balances; an exchange policy needs two distinct units and a rate source or mechanism. The configuration validator should reject missing prerequisites rather than silently invent defaults.

### Worked example: Simoleans in a cartoon town

A creator selects a fictional currency called **Simoleans**:

- the town treasury is the only initial issuer;
- residents earn Simoleans through jobs and approved town activities;
- shops may accept Simoleans according to their own offer policies;
- food, repairs, entertainment, and public services are possible sinks;
- residents may transfer units to one another if the town enables peer exchange;
- the creator may enable a disclosed expiry or tax policy, or leave it absent;
- Simoleans have no external conversion, redemption, or real-world monetary value.

The creator can inspect each selected rule and learn its economic consequences. For example, increasing wages without changing production, prices, taxes, or available goods can change nominal purchasing power and create pressure on prices. Increasing a sink can reduce circulating balances, but it does not automatically create more goods. These are model outcomes to measure, not magic balance edits.

The same currency identity can be used by a cartoon shop, a town payroll capability, and a casino demonstration. Each capability still has to declare which operations it supports.

## 6. Map the ontology onto existing Economy code

The current code already contains useful domain pieces. The ontology should organize them rather than replace them with duplicate abstractions.

| Existing source concept | Ontology role | Current limit to preserve |
|---|---|---|
| `EconomyEntityId` / `EconomyEntity` | Actor identity | Does not prove external identity or authorization |
| `Money` | Amount plus compact unit code | Not a currency policy, exchange service, or complete denomination model |
| `SimulatedCurrency` / `SimulatedAmount` | Typed fictional value identity and quantity | Simulation-only; not redeemable |
| `SimulationAccount` / `SimulationLedger` | Account boundaries and journaled simulated movements | Process-local; not durable or distributed |
| `CommerceItem` / `EconomyShop` / `MarketplaceOffer` | Resource offer and shop/catalog concepts | Not a full inventory, market-clearing, order, or checkout system |
| `EconomyTransfer` | Economic intent | Does not itself move value |
| `CasinoWagerBook` | Wager lifecycle and simulated custody transitions | Not a game engine, durable custody service, or real-money gambling system |
| `CapabilityComposition` | Capability selection and configuration boundary | Not yet a universal ontology registry or full runtime activation engine |
| Competition and tournament types | Organized-play actors, roles, and result structures | Not a complete labor or market model |

The source and tests remain authoritative. Do not rename or replace working contracts merely to make names line up with a theoretical diagram.

## 7. Capability composition should be explainable

A creator-facing capability catalog should expose more than a package name. Each selectable capability should eventually describe:

- stable identifier and version;
- the ontology concepts it provides or consumes;
- prerequisites and incompatible selections;
- configuration schema and defaults;
- policies and transitions it introduces;
- emitted events and observations;
- theory topics explaining its economic assumptions;
- maturity, implementation evidence, and known limitations.

For example, a town builder might select:

1. a Simoleans currency definition;
2. a household account capability;
3. employment and wage rules;
4. a shop and offer catalog;
5. production and consumption;
6. optional taxation;
7. optional auction markets.

The composer should show why each capability is needed and validate the whole selection before activation. A capability must not silently grant unrelated powers such as redemption, payment, inventory mutation, or wager settlement.

## 8. NuGet and MicroBundles have different jobs

**The NuGet package is the developer-facing library.** It should remain independently useful to any compatible .NET application. Developers install it, reference contracts, write code, run tests, and build their application without adopting FSM_COS.

**A MicroBundle is a separately packaged, immutable runtime artifact.** It may eventually contain a selected, versioned model definition, capability manifest, configuration schema, and implementation payload. It is not the source of truth for development and should not become a required runtime dependency of Economy.

A safe future lifecycle is:

1. edit source and configuration in the normal repository workflow;
2. validate locally and in CI;
3. compose and test ephemeral artifacts in memory or local storage;
4. when explicitly preparing a release or deployment, produce a deterministic bundle from pinned inputs;
5. derive bundle identity from canonical content and versioned dependencies;
6. publish to remote storage only through an explicit host-controlled release gate;
7. retain, supersede, or garbage-collect artifacts according to an explicit policy.

This avoids uploading thousands of temporary test bundles to Azure Blob. No bundle should be uploaded merely because a developer changed a file or CI ran. The Economy core must not depend on FSM_COS, Azure Blob, or the Workshop deployment environment. If MicroBundle packaging becomes useful, it belongs in an optional companion adapter.

## 9. Design principles and open questions

- **Descriptive before prescriptive:** ontology terms describe what a model contains; policies determine how it behaves.
- **Explicit assumptions:** scarcity, issuance, convertibility, and market rules are selected, visible choices.
- **No hidden authority:** a capability's presence does not imply that a host has authorized its operations.
- **Simulation stays simulated:** fictional value never silently becomes a claim on real money.
- **History over balance edits:** state changes should be explainable through events or journal entries.
- **Composable, not monolithic:** an Experience selects only the capabilities it needs.
- **Versioned and inspectable:** schema evolution must preserve meaning and make breaking changes visible.
- **Evidence-linked education:** every implemented concept should lead to code/tests; every unimplemented concept should be marked as a design or research gap.

Open questions for the design issue include the exact stable-ID format, ontology namespace/version rules, whether the first schema should be code-first or data-first, how capability configuration references ontology terms, and how schema migrations preserve historical simulation meaning.

## 10. Current status

This is a design foundation, not an implemented ontology engine. Existing currency, ledger, casino, shop, and capability contracts remain as they are. The next implementation should begin with a source-mapped ontology vocabulary and a small versioned model definition, then prove one end-to-end Simoleans configuration without requiring WebPage, Profiles, FSM_REST, FSM_COS, or cloud storage.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
