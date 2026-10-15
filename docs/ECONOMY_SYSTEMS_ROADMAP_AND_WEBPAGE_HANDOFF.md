# Economy Systems Roadmap and WebPage Integration Handoff

**Status: architecture and implementation direction.** This document is a handoff for the WebPage LLM and a long-range design guide for TheSingularityWorkshop.Economy. It describes a target system, staged proofs, and ownership boundaries; it does not claim that the banking, credit, securities, casino, interplanetary, or educational systems below are implemented.

## 1. The purpose: simulate economies, not merely add a checkout button

Economy is intended to model economic systems inside digital worlds. Singularity City is the reference environment: Digitens live, work, form households and businesses, produce and consume goods, save, borrow, invest, trade, and respond to incentives. Creator-owned businesses and reusable shops make this world inspectable and extensible.

The ambition is to simulate **large, diverse, interacting economies**—including flows whose *modeled notional value* reaches trillions of digital dollars or more—while being precise about what has actually been simulated. A large number written on a balance sheet is not itself proof of scale. Scale evidence must report entities, active behaviors, transactions or decisions per tick, update cost, memory/allocation, fidelity assumptions, and reproducible workloads. Do not confuse simulated value with real assets, real money, or financial settlement.

This is also an educational system. Users should be able to inspect why an economy behaves as it does: supply and demand, scarcity, productivity, prices, wages, inflation/deflation, savings, credit, interest, risk, default, ownership, dividends, market liquidity, speculation, concentration, monopoly power, externalities, and policy. Economic outcomes should not feel like unexplained magic.

## 2. Design principles

1. **Simulation first.** A world must work with entirely fictional units and simulated transfers; no bank account, payment processor, or real-money connection is required.
2. **Optional by composition.** An Experience can be a concert, fair, soccer match, gallery, workshop, or magic-casting environment without Economy. Economic capabilities may be composed when useful; they are never a mandatory dependency of every Experience.
3. **Shared semantics, local rules.** Economy provides reusable economic vocabulary and mechanisms. A city, game, classroom, or creator Experience defines its own world rules, policy, presentation, and consequences.
4. **Canonical identity, reusable presentation.** A shop, bank, corporation, security, or exchange has stable domain identity. An Experience can present it with a kiosk, street storefront, terminal, branch interior, holographic exchange, or another skin without copying the canonical entity.
5. **Deterministic and observable.** Where possible, simulations should be reproducible from explicit initial state, rules, inputs, and time-step configuration. Record provenance and explain state transitions.
6. **Scale through work scheduling, not slogans.** Separate represented population from active agents, and independent slow-changing aggregates from high-frequency local interactions. Measure before claiming trillion-agent behavior.
7. **No accidental real-money semantics.** Fictional balances, offers, loans, securities, chips, and transfers are not redeemable cash or real payment settlement. Any future real-money adapter is a distinct, opt-in, reviewed boundary.
8. **Plural economic models.** Support barter, gift economies, planned allocation, market pricing, mixed systems, multiple currencies, and world-specific rules rather than assuming every Experience must use one contemporary fiat model.
9. **Economics should be legible.** Provide event explanations, ledgers, charts, causal summaries, and controlled experiments alongside the simulation—not only dashboards of numbers.
10. **No premature mega-object.** Prefer composable domain contracts and provider-owned behavior to one giant Economy class or a hard-coded Singularity City engine.

## 3. Domain map: the economic civilization

Treat the following as capability families, not a promise that each is already available.

### 3.1 Value, exchange, and markets
- Fictional currencies and units of account, with explicit identifiers and precision.
- Products, services, commodities, inventory, production recipes, offers, orders, bids, asks, quotes, and price histories.
- Barter and multi-party exchange: an item or service can be offered for another item/service, a bundle, a promise, or a mixed settlement.
- Market rules such as fixed price, negotiated price, auctions, order books, rationing, allocation, and supply/demand pricing.
- Fees, taxes, tariffs, subsidies, transfers, gifts, wages, rent, royalties, and creator revenue shares where a world enables them.
- Time and location context: local market, city, region, planet, system, or interstellar trade network.

### 3.2 Accounts, banks, and payment-like infrastructure (simulation)
- Person, household, business, institution, government, and treasury accounts.
- Multiple independent banks and credit unions, each with policy, products, reserves/capital model, fees, service areas, and a distinct identity.
- Bank branches as world entities: offices, staff, opening schedules, service desks, queues, vaults, and branch-level activity.
- ATMs and self-service terminals as access points to simulated account services. An ATM is an interface/device capability, not a bank or ledger itself.
- Deposits, withdrawals, account-to-account transfers, internal clearing, interbank transfers, statements, transaction references, and audit history.
- Savings products, term deposits, interest accrual, fees, overdrafts, reserve/liquidity models, and configurable banking rules.
- Explicit separation between a user-visible balance, a ledger-derived balance, a bank's aggregate state, and the world's accounting/reporting view.
- Reconciliation and invariant checks so a modeled transfer cannot silently create or destroy value unless a named mechanism explicitly does so (e.g. issuance, fees, interest, tax, loss, or write-off).

These are simulated financial mechanisms. Do not present them as a real bank, deposit protection, regulated financial service, or connection to real accounts.

### 3.3 Borrowing, lending, and credit
- Loan offers and applications; principal, currency, interest model, schedule, maturity, fees, collateral, covenants, grace periods, and repayment rules.
- Fixed, variable, simple, compound, indexed, subsidized, zero-interest, and negative-interest models when a scenario deliberately needs them.
- Installments, early repayment, refinancing, delinquency, restructuring, default, collateral seizure, bankruptcy, and recovery.
- Lenders may be banks, peer-to-peer actors, businesses, governments, or fictional institutions. Borrowers may be individuals, businesses, or public entities.
- Creditworthiness must be a scenario-defined model with explainable inputs—not a mysterious universal score. Provide alternatives for worlds that prohibit credit or use communal allocation.
- Aggregate measures: debt service, leverage, default rate, liquidity pressure, credit availability, and concentration of lending.

Every loan must state which rules govern its time basis, interest calculation, rounding, missed payments, and terminal outcomes. Test edge cases such as zero rates, zero duration, rounding, repayment caps, and default.

### 3.4 Businesses, corporations, and ownership
- Sole proprietors, cooperatives, partnerships, corporations, subsidiaries, franchises, holding companies, state-owned entities, and other world-specific forms.
- Owners, equity stakes, voting rights, governance rules, management, employees, payroll, production, costs, revenue, liabilities, retained earnings, dividends, and bankruptcy.
- Supply chains, contracts, procurement, logistics, resource constraints, trade relationships, and business formation/dissolution.
- Conglomerates and mega-corporations spanning cities, planets, and star systems. Model ownership and control as explicit relationships; do not flatten a corporate group into one opaque balance.
- Market concentration, vertical integration, labor bargaining, externalities, monopoly/oligopoly behavior, regulatory capture, and unequal bargaining power as things a world may study—not assumptions to endorse.
- Public and cooperative alternatives should be representable alongside private corporations.

The long-range setting can include corporations with assets and operations across planetary systems. Their reach should emerge from modeled ownership, production, transport, information, law/policy, and resource constraints rather than from a nameplate alone.

### 3.5 Securities and exchanges
- Shares/equity, debt securities, issuance, ownership registry, corporate actions, dividends, splits, buybacks, maturity, coupons, and default.
- Local, city, national, planetary, orbital, solar-system, interplanetary, and eventually intergalactic exchange venues.
- Order books or other explicit matching rules, market hours, liquidity, fees, halts, settlement cycles, currency conversion, clearing, custody, and market data.
- Cross-market and cross-currency trading; inter-system exchange must account for travel/communication delay, jurisdiction, settlement finality, and information asymmetry where a scenario includes them.
- Primary issuance differs from secondary trading. A quote differs from an executed trade. An executed trade differs from final settlement. Keep these states distinct.
- Price discovery and volatility should follow declared mechanics and inputs. A graph of arbitrary prices is not a simulated stock exchange.

Interstellar scale is a design horizon, not a near-term milestone. Before adding a solar-system exchange, prove one deterministic local exchange with order matching, ownership changes, fees, audit trail, and invariants.

### 3.6 Casinos, games of chance, and risk
- In-world casino venues may contain tables, machines, games, hosts, service workers, entry rules, and chips/tokens.
- Casino games are optional Experience capabilities; neither Economy nor the world requires them.
- Rules, payout tables, random-number source/seed policy, house edge, volatility, limits, and event history should be explicit and testable.
- Keep chips/tokens and simulated winnings within the fictional world unless a separate reviewed policy explicitly says otherwise. Do not imply that casino credits are real currency.
- Educational modes can expose expected value, variance, probability, and the effect of a house edge. Avoid hiding losses behind decorative animation.
- For deterministic testing, support seeded or scripted outcomes; production-like simulations must state their randomness model and reproducibility limits.

### 3.7 Public institutions and economic infrastructure
- Governments, treasuries, tax authorities, central-bank-like policy institutions, regulators, courts/arbitration, public services, insurers, pension funds, cooperatives, credit unions, commodity warehouses, ports, freight yards, and clearinghouses.
- Policy levers such as taxation, public spending, subsidies, money issuance, interest-rate rules, reserve rules, price controls, sanctions, tariffs, and public investment.
- Model policy as a named actor and explicit action with scope and effective time. Keep policy decisions auditable and scenario-configurable.
- Physical infrastructure matters: production sites, warehouses, transit, ports, cargo networks, communications, and energy systems constrain supply and delivery. The economy should be able to consume logistics/physics capabilities rather than pretending goods teleport.

### 3.8 Education, experiments, and explanations
- Explain a concept in plain language, show its variables, and let a user change assumptions.
- Provide scenario controls and compare outcomes: e.g. demand shock, crop failure, new transit link, interest-rate change, tax change, lending freeze, new competitor, supply-chain interruption, resource discovery, or corporate merger.
- Show causal traces: which rules and events changed price, inventory, wages, debt, liquidity, ownership, or household welfare.
- Distinguish correlation from causation, and state when a result follows only from a simplified model.
- Offer guided lessons, sandbox experiments, scenario challenges, and inspectable world recipes.
- Use a visible glossary and explain units and time-step conventions. Every chart should disclose population, interval, modeled variables, and assumptions.

## 4. A scale hierarchy for economic scope

Use scope as a domain dimension, not as a hard-coded class tree:

1. **Individual / household / business**
2. **Neighborhood / city / region**
3. **Nation / planetary economy**
4. **Orbital and planetary infrastructure**
5. **Solar-system economy**
6. **Interstellar network**
7. **Intergalactic or civilization-scale network**

A scope can describe the entities, jurisdiction, currency, market, or exchange involved. Not every world needs all scopes. Cross-scope commerce is a composed capability with explicit conversion, transport, delay, jurisdiction, and settlement rules.

The same modeling language should permit a market stall and a solar-system securities exchange, but it must not force them to share irrelevant execution details. Reuse concepts where they genuinely match; isolate scale-specific rules behind providers.

## 5. Suggested ownership boundaries across the Workshop

| Concern | Primary owner |
|---|---|
| Generic money/value, commerce items, shops, offers, transfer/financial intent and execution observations | Economy |
| World citizens, jobs, households, production, needs, business behavior, population and city-specific policy | Singularity City / world simulation provider |
| Bank, ATM, loan, exchange, casino, corporation and policy implementations | Focused Economy providers or domain MicroBundles; shared primitives only when genuinely reusable |
| Experience definition, manifest, provider composition, lifecycle, and presentation host | WebPage / WebApp |
| Creator-owned shop/catalog identity | Economy domain contract; creator workflow and persistence may be hosted by a separate creator/catalog service |
| Placement in an Experience, access, local inventory policy, interaction, visual skin and layout | Experience/provider/presentation layer |
| Tick scheduling, deterministic state transitions and lifecycle infrastructure | FSM_API / FSM_Layer / FSM_COS according to their established contracts |
| Rendering and visual explanation | WebPage host and relevant Renderer/Visualize integration |
| Physical transport, resource movement, environmental constraints | Appropriate logistics/physics/world providers |
| Real-money movement or regulated financial operations, if ever pursued | A separately authorized, provider-specific integration with security, operational, legal, tax and compliance review |

Do not add every financial capability directly to the core Economy assembly merely because the vocabulary is related. Start with small contracts, provider-owned policy, deterministic tests, and explicit boundaries. Lower layers must not depend on WebPage or the city.

## 6. WebPage LLM handoff: what to integrate

**This section is written for the WebPage LLM.** Treat it as design input to the work already underway at the user's direction. Do not replace the existing WebPage architecture with a parallel framework, and do not assume this document authorizes a broad implementation sweep.

### The requested consumer-side experience

Build toward a visible, inspectable **Singularity City economic district** that can later expand into a civilization-scale simulation:

- A visitor can enter the city Experience without Economy enabled and still use the non-economic world.
- When the optional economic capability is present, the visitor can inspect Digitens, businesses, a reusable shop, and its in-world goods/offers.
- The same canonical shop can appear in two different Experience placements or skins without duplicating its identity/catalog.
- Later, the district can contain a bank branch, ATM, lending desk, market, casino, exchange terminal, corporation offices, warehouse, and freight/transit links as separately composed capabilities.
- Interactions should show meaningful state and outcomes from the provider. A painted ATM or stock ticker without functioning domain behavior is not proof of an implementation.
- The visitor can inspect explanations and model assumptions, not only a rendered scene.

### Integration rules

1. Inspect the current WebPage manifest, MicroBundle, provider, presentation, lifecycle, and repository-source conventions before choosing integration points.
2. Keep WebPage as the Experience composition/presentation host. Do not embed all Economy domain rules in page components.
3. Keep Economy optional. Prove the base Experience works when the Economy provider is missing.
4. Do not make this handoff imply that an Economy NuGet version is published or available. Verify the actual package and branch state. If the required Economy work is only on a review branch, use the repository's established development-source workflow or leave the integration as a documented adapter plan until the dependency is legitimately consumable.
5. Economy should own canonical shop/product and future economic domain identity; WebPage owns Experience placement, UI, visual presentation, and visitor journey. Each provider owns its own behavior and rules.
6. Avoid copying canonical entities into each skin. Presentation records should reference stable domain identity; a skin must not itself authorize an exchange or mutate the shop catalog.
7. Start with the smallest vertical slice: one source-defined creator shop, a few fictional products, two skins, and two Experience contexts. Make the interactions real and testable before adding a full banking district.
8. Show current implementation honestly. Label planned systems as planned. Do not describe the city as simulating a large population until a repeatable workload and measurements support the statement.
9. Preserve the existing user-directed priorities and work already in progress. This document is a design handoff, not permission to modify unrelated repositories or replace other LLMs' work.
10. Report the exact files changed, tests run and their results, package/source mode used, and remaining gaps. Do not merge or publish without explicit user authorization.

### WebPage proof checklist

- [ ] A non-economic Experience loads without the Economy provider.
- [ ] An Economy-enabled Experience resolves the canonical shop via a provider contract.
- [ ] Two Experience placements use the same shop/catalog identity with different presentation choices.
- [ ] A presentation change does not mutate canonical shop/product identity.
- [ ] At least one interaction calls real provider behavior and displays its resulting state.
- [ ] The UI distinguishes an offer, a requested exchange, an executed simulation result, and any future settlement state.
- [ ] The Experience explains that all values are fictional and simulated.
- [ ] Tests cover absent-provider behavior and the composition boundary.
- [ ] Performance reporting names the actual population/workload and measured update costs.
- [ ] The page links to this roadmap and explains which capabilities are current, planned, or illustrative.

## 7. Staged implementation roadmap

### Stage 0 — Contract and evidence hygiene
- Inventory current types and invariants; document what exists today.
- Keep source, tests, and architecture docs aligned.
- Preserve Economy's optional boundary and the WebPage composition contract.
- Do not add speculative public APIs for all future financial systems at once.

### Stage 1 — Creator commerce and city seed
- A small deterministic set of Digitens and one business.
- Canonical shop/catalog and a few fictional products.
- Two presentation skins and two Experience placements.
- A minimal simulated exchange path only if current contracts support it; otherwise define and test the next smallest provider contract.
- Repeatable tests and benchmark with workload disclosure.

### Stage 2 — Simulated accounts and ledger
- Define account identity, ledger entries, transfer intent, execution result, and accounting invariants.
- Prove deposits/withdrawals and same-bank transfers with fictional units.
- Add statements and audit views; demonstrate reconciliation and explicit value creation/destruction events.
- Only then add cross-bank transfer/clearing.

### Stage 3 — Bank, branch, and ATM providers
- Multiple simulated banks with explicit policies.
- Bank branches and ATMs as separate world/provider entities that call the same account/ledger domain.
- Branch/ATM service state, queues, fees, access policy, outages, and audit events.
- A WebPage district experience composed from real provider behavior.

### Stage 4 — Lending and corporate fundamentals
- Loan schedules, interest accrual, repayment, delinquency, default, and recovery.
- Business accounts, payroll, expenses, revenues, ownership, and basic financial reporting.
- Tests for rounding, time advancement, missed payments, and ledger reconciliation.

### Stage 5 — Barter, goods, and production networks
- Exchange of goods/services without requiring money.
- Inventory and production where owned by the world provider.
- Multi-party exchange and explicit fulfillment/failure states.
- Supply-chain dependencies, logistics constraints, and price formation.

### Stage 6 — Securities and local exchange
- One deterministic exchange with issuance, orders, matching, fees, ownership registry, settlement states, and audit trail.
- Separate quotes, trades, clearing, and settlement.
- Corporation ownership, debt securities, dividends, and corporate actions only after the basic market proof works.

### Stage 7 — Casino and educational scenarios
- One well-specified fictional game with deterministic test outcomes and explicit probability/payout assumptions.
- Educational visualizations of expected value, variance, risk, and house edge.
- Scenario experiments and causal explanations across banking, credit, prices, production, and policy.

### Stage 8 — Planetary to interstellar systems
- Multiple jurisdictions, currencies, planetary exchanges, communication/travel delay, trade constraints, and cross-scope settlement.
- Solar-system markets before interstellar markets; intergalactic markets only when their mechanics are specified and testable.
- Corporation groups and supply chains at those scales, with explicit assumptions for time, distance, information, law, and resource limits.

### Stage 9 — Measured scale and validated adoption
- Grow the benchmark by controlled factors and profile CPU, memory, allocations, tick latency, active-agent fraction, and transaction/decision throughput.
- Use FSM_API scheduling/process groups where the actual workload benefits from different update frequencies; do not assume every entity needs a full decision each tick.
- Report exactly what is represented, what is active, what is aggregated, and which behaviors are simulated.
- Validate creator workflow, audience comprehension, and a practical path to income such as creator tools, premium customization, hosting, or other delivered digital capabilities. Treat revenue ideas as hypotheses to validate, not guaranteed outcomes.

## 8. Economic correctness and auditability

As the system grows, establish explicit invariants for each capability. Examples:

- A transfer has a stable identity and a clear lifecycle; requested, accepted, executed, failed, reversed, and settled are not interchangeable.
- Ledger entries balance according to the declared accounting model; any value creation or destruction is attributable to an explicit event.
- Interest accrual is reproducible from principal, rate model, time basis, schedule, and rounding policy.
- Ownership changes are auditable and cannot exceed the declared issuance or ownership rules.
- An order cannot be reported as a trade before matching; a trade cannot be reported as settled before settlement rules say it is.
- Inventory and fulfillment remain distinct from an offer or a price quote.
- Barter does not require an implicit cash leg unless the selected scenario explicitly adds one.
- A simulated casino outcome is traceable to the declared rules and random source.
- Aggregated statistics disclose what is omitted and do not masquerade as per-agent simulation.

These examples are design targets. Add them to the actual contract and tests when implementing the relevant capability; do not document an invariant as enforced merely because this roadmap lists it.

## 9. Non-goals and guardrails

- This is not a plan to build a real bank, real brokerage, or payment processor inside the core package.
- It does not claim legal, tax, consumer-protection, security, deposit-protection, gambling, or financial-regulatory compliance.
- It does not treat simulated currency or “trillions of digital dollars” as real-world wealth or as a substitute for performance measurements.
- It does not require every Experience to use markets, money, credit, casinos, or securities.
- It does not make intergalactic exchanges a near-term deliverable.
- It does not prescribe one ideology or one economic policy. Different scenarios should make their assumptions explicit and allow users to compare outcomes.

## 10. Related documents

- [Economy architecture](ARCHITECTURE.md) — current package types and boundaries.
- [What is Economy?](WHAT_IS_ECONOMY.md) — current concepts and implemented-scope summary.
- [Economy PR #12: creator commerce and shop patterns](https://github.com/TrentBest/TheSingularityWorkshop.Economy/pull/12) — companion review branch containing the simulated-economy overview, creator shop composition proof, and game-store guide. These files are not yet on the development branch.
- [WebPage Experience catalog](https://github.com/TrentBest/WebPage/blob/work/digiten-world-slice/docs/EXPERIENCE_CATALOG.md) — review-branch direction, not a released WebPage feature.
