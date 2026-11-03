# Singularity Currency and Casino Contract

**Status: product/domain contract with a first ledger-backed casino accounting slice implemented.** The process-local typed simulation ledger supports issuance, transfers, balanced atomic multi-account postings, retirement, and derived balances. `CasinoCashier` now models one-for-one in-world buy-in/cash-out transfers; `CasinoWagerBook` models stake escrow, loss/push/win settlement, a separate payouts-owed account, and explicit payout claims. This remains volatile process-local state: durable wallet state, crash recovery, a first deterministic completed-hand blackjack evaluator is implemented, but shoe/draw/action lifecycle, venue/station integration, and browser integration are not implemented.

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
3. **Wager lifecycle:** a wager is proposed, validated against balance and limits, committed, resolved under a declared rule, and recorded. Invalid wagers have no economic effect. The first `CasinoWagerBook` slice now transfers player chips into escrow, resolves losses to the house reserve, and routes pushes/win gross payouts through a separate payouts-owed account before the player claims them.
4. **Game rules:** each game declares its rules, payout table, limits, randomness policy, and expected-value/variance information. Seeded or scripted outcomes support deterministic tests. A decorative spin or sample card must never be reported as a resolved game.
5. **House accounting:** track stakes held, unsettled wagers, payouts owed, returned stakes, net gaming result, and simulated operating costs. Do not label a simple event count as profit or a display counter as a ledger.
6. **Workers and payroll:** casino jobs include dealers, hosts, cleaners, security, maintenance, and management. Shifts, wage rules, attendance, and payroll are explicit simulation events.
7. **Venue operations:** track opening state, station capacity, staffing, maintenance, services, expenses, and guest activity. Empty or closed stations should affect the experience.
8. **Observation and explanation:** event history explains why a balance changed; reports distinguish player activity, house holdings, gross wagers, payouts, net gaming result, wages, and other operating costs.
9. **City integration:** Digitens can arrive, work, spend at city businesses, visit the casino, return home, and respond to world schedules and needs. A casino session must use the same canonical participant and economy identity as the rest of the city.
10. **Accessibility and responsible presentation:** disclose simulated status, rules, wager cost, and outcome clearly. Offer non-gambling city activities; do not pressure participation or hide losses in animation.

The initial playable slice should be small but honest: one participant, one cashier, one table game with a declared and testable rule set, one authoritative simulated transaction journal, and reproducible tests. Add game families only after the balance and wager invariants hold.

## Implemented accounting slice (development branch)

`CasinoCashier` performs one-for-one `SimulationLedger.Transfer` operations between a participant wallet and that participant's chip account. It does not issue currency, create a conversion rate, or interact with an external payment provider.

`CasinoWagerBook` coordinates the following accounts in one typed simulated currency:

| Account | Meaning | Movement |
|---|---|---|
| Participant chip account | Chips available to the participant | Buy-in credits; wager placement debits; payout claim credits |
| Wager escrow | Stakes committed to unresolved wagers | Placement credits; resolution debits |
| House reserve | Simulated funds available to cover winnings | Losing stakes credit it; winning profit debits it |
| Payouts owed | Resolved gross amounts not yet claimed | Pushes and wins credit it; player claim debits it |

A win's **gross payout includes the original stake**. For example, a 25-unit stake and 50-unit gross payout return 25 units of stake and award 25 units of winnings; the house reserve funds only the 25-unit net winnings. A push returns exactly the stake. A loss has zero payout. Every resolution is a balanced atomic `SimulationLedger.Post`; the subsequent claim is a separate idempotent transfer from payouts owed to the player chip account. A rejected settlement leaves both ledger and wager snapshot unchanged.

Wager placement, resolution, and claim have caller-supplied operation IDs; wager IDs are separately unique within the book. Exact in-process retries replay the original receipt, while conflicting reuse is rejected. The book serializes its own state transitions, but it is not a durable transaction coordinator. Restart recovery, external concurrent writers, game outcome generation, betting limits, and a full custody audit remain follow-up work. Player chip accounts are still the same fictional currency as wallets; there is no separate chip token or real-world value.

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
3. Establish the initial simulation ledger and transfer invariants before adding game logic. The ledger now includes balanced atomic multi-account postings for same-currency settlement. Extend it with separately named, auditable fees, wages, casino custody, and wager operations; do not infer those operations from generic transfers. Its current receipt model stores the aggregate positive side as a decimal amount, so a balanced posting set whose aggregate credits exceed decimal.MaxValue is rejected atomically; this representational limit must be revisited before modeling larger aggregate settlements.
4. The initial ledger and casino accounting tests cover typed currency isolation, explicit issuance/retirement, transfer conservation, atomic multi-account settlement, insufficient-balance rejection, overflow, precision, exact retries, conflicting operation-ID rejection, concurrent settlements, cashier transfers, wager escrow/settlement, and payout claims. BlackjackRules tests cover ace valuation, naturals, busts, comparisons, duplicate cards, and deterministic gross-payout decisions. Next add round/shoe/action lifecycle and integration tests.
5. Connect one game and cashier to the canonical simulation state; make the browser UI consume that state instead of maintaining independent pretend balances.
6. Add Digitens work, wages, venue staffing, business expenses, and ordinary city shopping through the same economic model.
7. Expand game variety, economic experiments, persistence, and scale tests only after the vertical slice passes.
8. Keep any future real-money work in a separate, reviewed capability boundary; never imply readiness from a simulation test.

## Current reality

The existing Economy alpha has reusable commerce vocabulary and design documentation. The WebPage casino page is currently an illustrative client-side experience: its buy-in, wager, payout, and event counters are local demo state; roulette and slots explicitly have no connected odds model, and the journal is not a production ledger. These are useful presentation scaffolds, not proof of an authoritative casino economy.

The multi-account settlement primitive passed repository CI on development commit `e8993166e818a2f5a70db43420433ce8443cb7d7` (42 tests; build 0 warnings/errors). The cashier/wager slice then passed CI with 50 tests. The first deterministic BlackjackRules evaluator has now been added with eight focused tests; the preceding CI run passed all 58 tests with 0 warnings/errors, and the latest documentation/inventory corrections are being verified by a follow-up run. This evaluator compares completed caller-supplied hands; it is not yet a playable blackjack round engine. The next milestone is a deterministic shoe/draw/action state machine, followed by one visible interaction connected to canonical simulation state. The ledger and wager book remain process-local; durable persistence and crash recovery are separate work.
