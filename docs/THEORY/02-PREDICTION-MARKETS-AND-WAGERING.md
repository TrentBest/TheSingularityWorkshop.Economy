# Prediction markets and simulated wagering

## 01 — What

A prediction market is a structured system in which participants express a view about a future event by selecting a defined outcome under published rules. A wager is an economic commitment attached to that prediction. A market is not merely a button labeled “Team A wins”: it is a versioned contract describing exactly what question is being answered, which outcomes are possible, when participation closes, what evidence determines the result, and how each accepted position is settled.

The Economy roadmap should build a provider-neutral **prediction and wagering domain**. Sports is an excellent first vertical because a single match naturally exposes the full lifecycle: a competition, scheduled event, participants, markets, selections, odds, accepted bets, official result, and settlement. The same core can later host non-sports simulations without making the ledger understand teams or game rules.

This chapter describes intended architecture. Except for the existing simulated ledger, casino wager book, and blackjack slice, the prediction-market components below are not implemented yet.

## 02 — Why

A robust domain must separate concepts that are often collapsed in a casual betting interface:

- An **event** is something that happens: a match, race, tournament, or simulated contest.
- A **market** is one precise question about an event: match winner, winner including overtime, total points over/under a threshold, or first scorer.
- A **selection** is one answer available within that market.
- An **odds quote** describes a potential payout calculation at a point in time. It is not a promise until accepted under the market's rules.
- A **bet** is an accepted commitment by a participant, with a frozen price/rules snapshot.
- A **result** is an authoritative or explicitly simulated observation about the event.
- A **settlement** is the deterministic application of the market's settlement rules to the accepted bet and a finalized result.

This separation prevents a later UI, sports provider, or casino game from becoming the authority for money movement. The domain should own the contracts; an adapter may supply schedules and results; the ledger owns simulated value movement.

## 03 — How

### Domain layers

| Layer | Responsibility | Examples |
|---|---|---|
| Competition | Organizes a season or tournament | League, cup, season, round |
| Event | Defines the contest and its lifecycle | Fixture, race, match, simulated duel |
| Participant | Identifies a side or entrant | Team, player, racer, contestant |
| Market | Defines one answerable question | Moneyline, spread, total, exact score |
| Selection | Enumerates valid answers | Home, away, draw; over, under |
| Odds quote | Captures price and calculation basis | Decimal odds, quoted at, quote version |
| Bet | Records an accepted participant position | Single, multi-leg accumulator |
| Result | Records the event facts and provenance | Final score, winner, void reason |
| Settlement | Applies frozen rules and creates ledger postings | Return, loss, push, void, partial result |

The event model must not assume every sport is a two-team match. A race can have many entrants; a tournament can have a bracket; an individual event can have a winner and ranked finishers. Team-vs-team fixtures are one event subtype, not the root abstraction.

### Event lifecycle

A proposed event lifecycle is:

`Scheduled → OpenForMarkets → InProgress → ResultPending → Finalized`

Exceptional transitions include postponed, abandoned, cancelled, disputed, and corrected. These are not interchangeable. A postponed event may retain its identity and reschedule; a cancelled event may void all attached markets; an abandoned event may require market-specific rules; a corrected result must create a new result revision and an auditable resettlement policy rather than silently overwriting history.

### Market lifecycle

`Draft → Open → Suspended → Closed → ResultPending → Settled`

A market may also become Void. Suspension temporarily prevents new acceptance without changing the underlying event. Closing stops new positions at the declared cutoff. Settlement requires a finalized result and a deterministic market rule. The engine must reject bets against closed, suspended, void, or already-settled markets.

### First sports market catalogue

Start with simple, auditable markets before expanding:

1. **Match winner / moneyline** — choose the winner; for competitions where a draw is possible, draw is an explicit selection unless the market says otherwise.
2. **Three-way result** — home win, draw, away win, with regulation-time scope stated.
3. **Winner including overtime** — a different market contract from regulation time.
4. **Point/goal spread** — selection plus a signed handicap and a declared tie/push rule.
5. **Total points/goals** — over or under a threshold, with an explicit treatment of an exact tie.
6. **Tournament winner** — futures market settled only when the competition is finalized.
7. Later: player props, first/last scorer, period results, exact score, and multi-event accumulators.

Each market definition must state its measurement scope, result source, tie rules, postponement policy, abandonment policy, and correction policy. A “winner” is not well-defined until those details are fixed.

### Odds and payout arithmetic

Use decimal odds as the initial internal representation because the gross return is easy to explain:

`gross return = stake × accepted decimal odds`

`net result = gross return − stake`

The accepted bet stores the exact odds snapshot and a deterministic rounding policy. The market's current quote may change after acceptance, but it must never rewrite an existing bet. For a losing selection, gross return is zero. A push or void returns the original stake under the declared contract. The implementation must validate precision against the simulated currency and avoid floating-point arithmetic for ledger amounts.

Odds are not probabilities by themselves unless the market and margin model are specified. A quote can include an implied probability for analysis, but the system must distinguish raw implied probability, normalized probabilities, and an operator's overround. Do not pretend a displayed price is a calibrated forecast.

### Bet acceptance and custody

A bet is accepted only when all of the following are true:

- The participant, market, and selection identities are valid.
- The market is open and the event has not crossed its acceptance cutoff.
- The quote version is still acceptable under the declared quote policy.
- The stake is positive, typed, and within configured simulation limits.
- The participant has sufficient simulated balance.
- The acceptance operation is idempotent and records a frozen market/selection/odds/rules snapshot.

Acceptance transfers the stake into a dedicated simulated escrow account using the atomic ledger. A bet is not accepted merely because a client displayed a success animation. The server-side domain response and ledger receipt are authoritative.

### Settlement and accumulators

A single bet settles against one market result. A multi-leg bet stores each leg's own event, market, selection, quote, and rules snapshot. The accumulator's combined price is computed deterministically at acceptance and frozen. Settlement must define how a void leg affects the combined bet; it cannot be improvised by a UI.

The settlement engine should produce a typed settlement decision first, then submit a balanced atomic posting to the ledger. A retry with the same operation identity must replay the same receipt; a conflicting retry must fail. A corrected result requires an explicit correction workflow, not a second arbitrary payout.

### Provider and authority boundaries

Sports data providers are adapters, not domain authorities. Their feeds can be late, unavailable, duplicated, contradictory, or corrected. Store source identity, retrieval time, source event ID, source revision, and the normalized result used for settlement. A human-reviewed result can be a source too, but its provenance and reviewer action must be recorded.

The domain should support a deterministic, manually supplied fixture and result before integrating live feeds. That gives us reproducible tests without coupling the core package to a sports API, web client, or vendor.

### Risks, limits, and explicit exclusions

This repository's first wagering implementation is a fictional, non-redeemable simulation. It must not add deposits, withdrawals, cash prizes, real-money odds, paid bankroll top-ups, donation-to-credit conversion, or external exchange. Courtesy credits, if introduced, are free simulation grants and not debts. Supporter recognition must remain separate from gameplay balances and outcomes.

The architecture described here is not legal, licensing, tax, or compliance guidance. Real-money gambling or payment features would be a separate product decision requiring professional review and explicit authorization; they are not a hidden extension of this simulation.

### Proposed implementation sequence

1. Complete the typed ledger and current casino custody invariants.
2. Define generic competition/event/participant identities and immutable event snapshots.
3. Define market, selection, market lifecycle, and explicit settlement rules.
4. Add deterministic odds quotes and immutable accepted bet snapshots.
5. Add single-bet acceptance, escrow, result finalization, and idempotent settlement.
6. Prove a two-team match-winner market end-to-end with manually supplied fixture and result.
7. Add draw, overtime scope, postponement, abandonment, result correction, spread, and totals.
8. Add multi-event accumulators only after single-market settlement is reliable.
9. Add optional provider adapters and live-feed reconciliation.
10. Add richer sports and non-sports event adapters without changing ledger semantics.

### Evidence boundary

Current implemented evidence is the typed simulated ledger, atomic balanced postings, casino wager custody, deterministic blackjack rules, round lifecycle, session coordination, and staged session opening. No sports event, market, odds, bet slip, sports provider, accumulator, or sports settlement API is implemented by this chapter.

## 04 — See it in a minute

A match-winner bet should flow like this:

`Fixture → Market("Match winner") → Selection("Home") → OddsQuote(1.80) → AcceptedBet(stake, frozen quote) → FinalResult(Home wins) → Settlement(gross return = stake × 1.80) → Atomic ledger posting`

The fixture and market define the question. The accepted bet freezes the answer and price. The result supplies evidence. Settlement calculates the return and moves only fictional simulated value.

## 05 — Choose your next step

- Read [the current typed ledger and casino contract](../SINGULARITY_CURRENCY_AND_CASINO.md).
- Trace the implemented blackjack path through [the session-opening protocol](../UML/BlackjackSessionOpeningUML.md).
- Implement the generic event and market vocabulary before building a sports-specific betting API.
- Keep a two-team match-winner market as the first acceptance-to-settlement vertical slice.
 
<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
