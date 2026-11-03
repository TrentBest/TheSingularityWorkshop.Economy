---
layout: default
title: Markets and economic playgrounds
---

<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# Markets and economic playgrounds

**Status: design roadmap.** The activities described here are proposed simulation experiences, not implemented games or a production financial service. The first goal is to let a visitor learn by experimenting with a coherent fictional economy.

## 01 — What

Economy should support more than a shop and a casino. It should provide reusable economic rules that Experiences can turn into interactive playgrounds:

- a casino where players learn probability, risk, variance, bankroll management, and house accounting;
- a stock exchange where players explore ownership, liquidity, order books, price formation, and market shocks;
- businesses where players manage inventory, costs, pricing, wages, and customer demand;
- auctions where players compare bids, reserve prices, and different allocation rules;
- city jobs and households where work, wages, budgets, needs, and purchases connect;
- supply-chain experiments where scarcity, production constraints, transport costs, and shocks affect prices.

These are different experiences over shared economic foundations—not separate wallets or unrelated counter systems.

## 02 — Why

A user should be able to try a decision, observe the consequence, and inspect why it happened. The goal is not to imitate a financial institution. It is to make economic ideas visible, testable, and fun.

The same fictional currency and account/ledger identity should be reused across the city. A casino wager, a shop purchase, a wage, and a simulated share trade are different domain operations, but their cash effects must reconcile through one authoritative simulation ledger.

## 03 — How

### Shared foundation

- Use the typed `SimulatedCurrency` and `SimulationLedger` for fictional currency.
- Give every economic mutation a stable operation identity and an inspectable result.
- Keep game-specific rules in composable capabilities: casino custody and wagers, exchange orders and matching, inventory and sales, or labor and payroll.
- Keep world state and presentation in the Experience/host. Economy must not depend on WebPage, a renderer, or a specific city.
- Make deterministic scenarios the default for tests; if a user-facing experience includes randomness, expose the rules and use a controlled random source that can be seeded in tests.
- Clearly label fictional values and simulated outcomes. Never imply real-world denomination, redemption, income, investment returns, or external settlement.

### Casino playground

The first playable casino slice should be deliberately narrow: one participant, one cashier, one table game, and an auditable wager lifecycle. The existing design contract describes a courtesy grant of 1,000,000 fictional Singularity Denomination units, with repeat grants after depletion. Each grant must be an explicit, provenance-bearing issuance event.

The casino must distinguish participant-held chips, casino-held chips, unsettled wagers, payouts owed, and house results. A visible chip counter is not a ledger. Start with one fully specified game and expand only after custody, wager, settlement, and retry invariants pass.

### Simulated stock exchange

The exchange is a separate market capability sharing the same fictional currency and ledger. A first version can use a small set of fictional companies and one share class per company.

Its first vertical slice should include:

1. company and share-class identity, with explicit issued-share limits;
2. buy and sell limit orders with stable IDs and lifecycle states;
3. deterministic price-time matching, partial fills, and cancellation;
4. atomic, auditable cash and share settlement;
5. positions derived from share events, not UI counters;
6. an order book, trade tape, portfolio, price history, and explanations of each match;
7. seeded fictional news and supply/demand events to demonstrate price movement.

Keep bid, ask, last trade, market price, and estimated portfolio value distinct. A chart is an explanation of simulated events, not a promise or forecast.

### Other playgrounds

| Playground | Economic idea made visible | First useful interaction |
|---|---|---|
| Business operator | Revenue, costs, margin, inventory | Set a price and restock a small shop |
| Auction house | Bids, reserve price, allocation | Submit and resolve a deterministic auction |
| City work | Labor, wages, household budgets | Complete a shift and inspect one wage event |
| Supply chain | Scarcity, capacity, transport cost | Introduce a shortage and observe availability/prices |
| Household planner | Needs, trade-offs, opportunity cost | Choose between competing purchases under a budget |
| Market events | Shocks, expectations, volatility | Apply a scripted event and inspect its modeled effects |

These are candidate experiences. They should be prioritized by the quality of the learning loop and the amount of reusable domain behavior they prove—not by the number of screens or game modes.

### Supporter recognition is a separate boundary

The initial playgrounds should be freely playable with simulation-only grants. Do not connect donations, purchases, payment credentials, or payment-provider callbacks to casino credits, wagers, exchange capital, simulated returns, or gameplay advantage.

If supporter recognition is considered later, it should be a separate product decision with no conversion into simulated gambling bankroll or market capital. Any payment collection or donation program requires an established operating entity, appropriate provider approval, and qualified legal/tax review before implementation. This document does not establish legal or compliance readiness.

## 04 — See it in a minute

A target user journey could look like this:

1. Enter the city and receive a clearly labeled fictional starting balance.
2. Visit the casino, receive a courtesy grant, place a small simulated wager, and inspect the exact event trail.
3. Visit the exchange and place a limit order for a fictional company.
4. Trigger a scripted market event and inspect the order book, trade tape, and portfolio changes.
5. Visit a shop or take an optional job and see the same currency and account identity used in another context.

This is a target journey, not a claim that these connected experiences exist today. The process-local ledger is the current foundation; persistence, casino accounting, market matching, share custody, and the integrated city journey remain to be implemented and tested.

## 05 — Choose your next step

- [Casino currency and custody contract](SINGULARITY_CURRENCY_AND_CASINO.md) — first playable casino vertical slice.
- [Simulated stock exchange issue](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/20) — proposed deterministic market implementation.
- [Simulated economy and creator commerce](SIMULATED_ECONOMIES_AND_CREATOR_COMMERCE.md) — city and creator-commerce architecture.
- [Economy architecture](ARCHITECTURE.md) — current ownership and implementation boundaries.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
