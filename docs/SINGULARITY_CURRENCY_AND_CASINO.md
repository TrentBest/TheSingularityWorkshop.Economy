# Singularity Currency and Casino Contract

**Status: product/domain contract and staged implementation plan.** This document records the intended Economy behavior for Singularity City. It does not claim that a persistent wallet, authoritative casino ledger, complete game rules, real-money service, or legally reviewed currency exists today.

## Product intent

Singularity City is a persistent simulated city populated by Digitens. They can work, receive wages or rewards, operate businesses, buy goods and services, pay rent, participate in civic life, visit venues, and choose entertainment—including casino games. The Singularity Casino is the flagship demonstration that these behaviors can share one coherent economic world.

The casino is not the entire Economy package. Economy must also support ordinary work, business activity, shopping, housing, services, creator storefronts, transfers, and world-specific economic experiments. Gambling is an optional capability composed into a venue or Experience, never a requirement for every Experience.

## Singularity currency: useful in-world, without real-world value

The default city currency is **Singularity currency**. Casino chips are a casino-specific representation of a participant's simulated Singularity-currency balance or a clearly defined temporary casino stake. The UI may show both a chip count and a Singularity-currency count, but must explain exactly when one is exchanged for the other.

The defining constraints are:

- **No denomination against real money.** There is no USD, EUR, crypto-asset, or other real-world price for Singularity currency.
- **No evaluatable real-world value.** A large balance does not imply wealth, an asset, a claim, a liability to a real person, or a right to redeem anything.
- **No redemption or cash-out to money.** In the simulation, “cash out” means return casino chips to the participant's in-world balance under the venue's rules. It never means a withdrawal to a bank, card, wallet, or external account.
- **No hidden exchange rate.** Do not add a conversion ratio, price oracle, market quote, external listing, or implied equivalent value.
- **In-world usefulness is allowed.** Currency can buy fictional goods and services, pay simulated wages or rent, fund businesses, and be wagered. Its usefulness is governed by world rules, not external monetary value.
- **No real-money promises.** UI, docs, examples, telemetry, and generated descriptions must not call balances redeemable, investable, withdrawable, or income.
- **Simulation records remain simulation records.** A simulation event cannot be represented as a real payment result simply because it uses shared commerce vocabulary.

This deliberately differs from a real-world currency. The city needs an internal unit of account to model economic relationships; it does not need to assign that unit a real-world exchange value.

### Keep chip conversion explicit

A casino venue defines its own rules for buy-in, table stakes, wager limits, payout, return of unused chips, and chip reconciliation. If chips are just a visual denomination of Singularity currency, conversion must be explicitly defined as an in-world transfer at the venue's declared rate (normally one-for-one) and recorded once. If chips are a separate temporary token, their backing and conversion rules must be explicit and must not create value silently.

Never let the same participant spend both the converted currency and the chips backing it. Prevent duplicate buy-ins, duplicate payouts, negative balances where disallowed, and returning more chips than the participant owns. A displayed counter is not an authoritative balance.

## A future real digital currency is a separate system

A possible future legal digital-currency integration is not an extension that silently changes existing Singularity balances into money. It would require a separate, explicit product decision and a reviewed boundary, including qualified legal and tax advice, jurisdiction and licensing analysis where applicable, security, identity and privacy decisions, provider approval, operational controls, reconciliation, and an explicit launch authorization.

Until that work is complete:

- no external monetary valuation;
- no cash redemption or fiat conversion;
- no blockchain or wallet requirement for simulated participation;
- no collection of payment credentials for the city simulation;
- no real-money gambling, deposits, withdrawals, or prizes;
- no implication that simulated wages are income payable outside the world.

A future adapter must not revalue, migrate, or redeem the existing fictional economy by default. It should be opt-in, separately named and versioned, and unable to reinterpret a simulation record as a real settlement.

## Singularity Casino: a real simulation, not decorative counters

The casino should become a living venue in the city, with Digitens as guests, dealers, hosts, security, and service workers. The floor plan is a presentation of actual venue and station identities—not the authority for balances or outcomes.

The intended capabilities are staged:

1. **Venue and station identity:** cashier cage, blackjack, poker, roulette, slot machines, and future games are individually selectable stations with stable IDs, rules, capacity, staffing, and operating status.
2. **Chip custody:** buy-in transfers in-world currency into casino custody and issues the corresponding chip balance; cash-out reverses that simulated relationship exactly once. Casino-held chips and player-held chips must be distinguishable.
3. **Wager lifecycle:** a wager is proposed, validated against balance and limits, committed, resolved under a declared rule, and recorded. Invalid wagers have no economic effect.
4. **Game rules:** each game declares its rules, payout table, limits, randomness policy, and expected-value/variance information. Seeded or scripted outcomes support deterministic tests. A decorative spin or sample card must never be reported as a resolved game.
5. **House accounting:** track stakes held, unsettled wagers, payouts owed, returned stakes, net gaming result, and simulated operating costs. Do not label a simple event count as profit or a display counter as a ledger.
6. **Workers and payroll:** casino jobs include dealers, hosts, cleaners, security, maintenance, and management. Shifts, wage rules, attendance, and payroll are explicit simulation events.
7. **Venue operations:** track opening state, station capacity, staffing, maintenance, services, expenses, and guest activity. Empty or closed stations should affect the experience.
8. **Observation and explanation:** event history explains why a balance changed; reports distinguish player activity, house holdings, gross wagers, payouts, net gaming result, wages, and other operating costs.
9. **City integration:** Digitens can arrive, work, spend at city businesses, visit the casino, return home, and respond to world schedules and needs. A casino session must use the same canonical participant and economy identity as the rest of the city.
10. **Accessibility and responsible presentation:** disclose simulated status, rules, wager cost, and outcome clearly. Offer non-gambling city activities; do not pressure participation or hide losses in animation.

The initial playable slice should be small but honest: one participant, one cashier, one table game with a declared and testable rule set, one authoritative simulated transaction journal, and reproducible tests. Add game families only after the balance and wager invariants hold.

## Shared responsibilities

| Component | Responsibility |
|---|---|
| Economy package | Provider-neutral value/offer/intent vocabulary and, as implemented, explicit contracts for simulated economic operations |
| Singularity City simulation | Canonical Digiten, job, household, business, venue, schedule, and world state |
| Casino domain/venue capability | Station rules, chip custody, wager lifecycle, house accounting, staffing and operating events |
| Experience / WebPage | Interactive floor plan, perspective/elevation views, user input, explanation and presentation |
| Persistence and authoritative journal | Durable state, unique operation identity, append-only events or equivalent auditable records, recovery and reconciliation |
| Future real-money boundary | Separate, opt-in integration only after professional review and explicit authorization |

Do not make the Economy package depend upward on WebPage, a particular renderer, or Singularity City presentation. Do not turn the shared core into one giant casino or city engine. Keep venue policy and game-specific behavior composable.

## First implementation and acceptance sequence

1. Inventory the current Economy types and tests; label each capability as implemented, partial, or proposed.
2. Define a typed fictional currency/unit identity that cannot be mistaken for a real ISO currency merely because the current `Money` type uses a three-letter code. Preserve compatibility deliberately rather than silently changing existing public semantics.
3. Define the simulation ledger and transfer invariants before adding game logic. Model issuance, destruction, fees, wages, and casino custody as named, auditable operations.
4. Add invariant-focused tests: currency isolation, conservation except named issuance/sinks, duplicate-operation rejection, insufficient balance, exact buy-in/cash-out, wager commit/resolve, payout calculation, and deterministic outcomes.
5. Connect one game and cashier to the canonical simulation state; make the browser UI consume that state instead of maintaining independent pretend balances.
6. Add Digitens work, wages, venue staffing, business expenses, and ordinary city shopping through the same economic model.
7. Expand game variety, economic experiments, persistence, and scale tests only after the vertical slice passes.
8. Keep any future real-money work in a separate, reviewed capability boundary; never imply readiness from a simulation test.

## Current reality

The existing Economy alpha has reusable commerce vocabulary and design documentation. The WebPage casino page is currently an illustrative client-side experience: its buy-in, wager, payout, and event counters are local demo state; roulette and slots explicitly have no connected odds model, and the journal is not a production ledger. These are useful presentation scaffolds, not proof of an authoritative casino economy.

The next milestone is not to add more decorative casino controls. It is to connect a small visible interaction to tested, canonical simulation state and then build outward.
