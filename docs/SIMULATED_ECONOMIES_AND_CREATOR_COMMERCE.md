# Simulated Economies, Creator Shops, and Adoption

**Status: direction and proof plan for the exploratory Economy alpha.** This document clarifies the intended product path. It does not claim that the full city simulation, creator storefront builder, reskinning pipeline, scale test, or real-money integration is already implemented.

## The first economy is a world that works

Economy is first about **simulated economies**: worlds in which entities have identities, work, needs, shops, goods, offers, and money-like values. Singularity City is the reference world. Digitens are its digital citizens; the interesting scale question is how many can participate in a persistent, observable economy while the simulation remains useful and responsive.

That means in-world money is not a placeholder for a payment processor. It is a real part of the simulation domain: users should be able to observe economic activity, create businesses, offer goods, and see the consequences inside the world without connecting a bank account or moving real money.

The same contracts should also be useful outside the city simulation. A creator should be able to build a shop once, present it in a digital storefront, reskin its presentation, and compose that storefront into different Experiences. A market fair, game hub, gallery, RPG, or community world may all present a shop differently while retaining its creator-defined catalog and the host Experience's rules.

## Keep the layers separate

| Layer | Responsibility |
|---|---|
| **World simulation** | Digitens, ownership, jobs, homes, businesses, production, needs, demand, local rules, and simulated value movement |
| **Economy vocabulary** | Shared identities and descriptions for products, shops, offers, money-like values, transfer intent, and execution results |
| **Creator shop** | Creator's catalog and shop configuration, with stable identity that can be reused across presentations |
| **Experience** | Context, access, inventory/availability policy, interactions, presentation choices, and world-specific consequences |
| **Presentation / skin** | Layout, visual theme, branding, display format, and how the same shop is embedded in an Experience |
| **Future real-money adapter** | Separately authorized provider operations, reconciliation, security, and required professional/legal review |

Economy should not own the entire city simulation, a universal UI, or the rules of every game. The simulation and Experience can use Economy's shared language without making every world behavior an Economy primitive.

## Reskinning and composition are core adoption features

A creator-facing workflow should eventually look like this:

1. Create a shop and give it a stable creator/shop identity.
2. Add creator-defined product entries and optional in-world prices.
3. Choose a presentation skin: kiosk, storefront, stall, terminal, fantasy merchant, gallery counter, or another Experience-owned presentation.
4. Insert the shop into one or more Experiences through composition rather than copying the canonical shop definition.
5. Let each Experience apply its own context: local inventory, opening state, permitted interactions, reputation, or simulated supply and demand.
6. Inspect the shop in the running world, revise it, and reuse the same recipe elsewhere.

The reusable object is the shop/catalog definition; the skin is a presentation choice; the Experience supplies context and policy. Do not treat a skin as a second shop database or let presentation alone authorize an exchange.

This is a target workflow, not a claim that a visual shop editor, persistent creator catalog, or arbitrary cross-Experience insertion exists today. Prove it in small slices: first a source-defined shop, then a reskinned view, then composition into two Experiences, then creator editing and persistence.

## Prove scale by measuring useful behavior

The Digiten goal is not a headline count alone. Increase population in controlled steps and measure both simulation work and meaningful economic behavior.

Useful first measurements include:

- Digitens represented and active per simulation step;
- businesses, shops, offers, and simulated exchanges represented;
- tick/update cost and allocation rate as population grows;
- how work is distributed across update rates or process groups;
- time to create, reskin, compose, and inspect a shop;
- whether the world continues to behave correctly when optional commerce capabilities are absent.

Keep the workload reproducible. Separate the number of represented citizens from the number actively making decisions in a given tick. Record what the test actually simulates; do not infer full social or economic behavior from entity count alone.

## Adoption and near-term monetization

The immediate goal is to prove a compelling creator workflow and give users a reason to adopt it. A practical sequence is:

1. **Demonstrable reference world:** show Digitens and businesses participating in a visible simulated economy.
2. **Creator recipe:** let a user inspect, copy, modify, and compose a working shop into an Experience.
3. **Portable presentation:** show the same shop in different skins or environments without duplicating its identity and catalog.
4. **Creator value:** enable useful customization and reusable creations, then validate what creators and hosts actually want.
5. **Early revenue experiments:** prioritize near-term, transparent ways to charge for software, hosting, creator tools, premium presentation, or other clearly delivered digital capabilities. These are business hypotheses to validate, not existing product features or guaranteed income.
6. **Real-money commerce later:** only when the product need is proven, define a separate execution boundary and obtain appropriate provider, security, tax, consumer-protection, and legal guidance.

Do not make a real-money connection a prerequisite for testing the simulated economy or creator workflow. Do not represent in-world balances or a modeled transfer as redeemable cash, payment settlement, or legal/compliance approval.

## What the current package does and does not prove

The current Economy API includes shared concepts such as `Money`, `CommerceItem`, `EconomyShop`, `MarketplaceOffer`, `EconomyTransfer`, and `EconomicExecutionResult`. These are useful vocabulary and composition building blocks, but the presence of a type does not mean a complete service exists.

The current package does **not** by itself provide a persistent city simulation, Digitens behavior, production/inventory, creator editing UI, a reskinning engine, storefront hosting, balances/ledger, purchase orchestration, or a real payment adapter. Those should be implemented and proven in the appropriate owning layers.

## First vertical slice to prove

Use one tiny, end-to-end slice before widening the model:

- A small set of simulated Digitens and one creator-owned business;
- a shop with a few in-world product entries;
- a stable shop identity independent of its skin;
- two different presentation skins;
- composition of that same shop into two distinct Experience contexts;
- deterministic tests proving that changing the skin does not change the canonical shop identity/catalog;
- a repeatable benchmark that grows the represented Digiten population and reports update cost;
- a demo that works with no real-money provider connected.

This slice demonstrates the adoption story and tests the right seams. Expand only when its behavior is visible and measurable.

## Related documentation

- [Game store patterns](GAME_STORE_PATTERNS.md) — current API examples and Experience-owned boundaries.
- [Economy architecture](ARCHITECTURE.md) — current contracts and future boundaries.
- [What is Economy?](WHAT_IS_ECONOMY.md) — plain-language conceptual model.
- [WebPage Experience catalog](https://github.com/TrentBest/WebPage/blob/work/digiten-world-slice/docs/EXPERIENCE_CATALOG.md) — current composition direction on a review branch.
