# Teams, leagues, tournaments, and organized play

## 01 — What

Organized play is a domain in which people or entities form competitors, register those competitors into a competition, and progress through a declared structure toward a result. The structure may be a one-off contest, a tournament bracket, a recurring league, a season, a ladder, or a fantasy competition.

The Economy package should provide shared competition infrastructure while delegating activity-specific rules to independent engines. A poker tournament, blackjack tournament, soccer league, esports event, and creator challenge can share registration and tournament administration without sharing their gameplay rules.

## 02 — Why

The platform's most reusable object is not a casino game. It is the organized competition:

- A **competitor** is an individual or team identity.
- A **team** is an assembled competitor with an explicit roster and role assignments.
- A **competition** defines participation, format, and lifecycle.
- A **tournament** organizes competitors into rounds and advancement.
- A **league or season** organizes repeated events and standings over time.
- A **fantasy team** is a participant-owned roster selected from an eligible external or simulated player pool, governed by draft, roster, lock, and scoring rules.
- An **activity ruleset** defines what happens during a match or game.
- A **result** records the outcome under an identified ruleset and provenance policy.

A "dream team" is not merely a label. It implies roster construction from a candidate pool, position/role constraints, eligibility, selection or draft rules, and often scoring based on the performance of selected members. Those rules should be explicit policies layered over the generic Team primitive.

## 03 — How

### Separate administration from gameplay

The competition layer should own competitor identity, team membership, registration, tournament structure, schedule references, standings snapshots, and lifecycle. The activity layer should own game-specific actions and outcomes. The economy layer should own simulated balances, entry-fee custody, prize allocation, and auditable settlement. Prediction markets should reference events and results but must not define or control the competition itself.

This gives us a reusable architecture:

`Competition → Event/Match → Activity Ruleset → Result`

and, when an economy is involved:

`Participant/Team → Entry or Wager → Escrow → Result → Settlement`

### Team concepts

The first Team primitive supports immutable roster snapshots and generic roles. Later layers can add:
- Eligibility windows and roster locks.
- Drafts, auctions using simulated credits, and free-agent selection.
- Role/position slots and minimum/maximum counts.
- Team ownership, co-ownership, managers, captains, and permission boundaries.
- Fantasy scoring rules that map player/event statistics to points.
- Salary caps, budgets, transfers, and season-long roster changes.

A real sports team and a fantasy/dream team should not be conflated. A real team is a competitor in an event; a fantasy team is a participant's constructed portfolio of eligible players whose aggregate scoring is evaluated by a fantasy competition.

### Tournament and league formats

The domain vocabulary includes single elimination, double elimination, round robin, league, Swiss, ladder, and season formats. Each requires its own scheduling and advancement policy. A format label alone does not implement the corresponding algorithm.

For example, single elimination needs seeding, bracket slots, byes, match result validation, and winner advancement. Double elimination additionally needs a losers bracket and explicit reset/final rules. Round robin needs pairing generation and tie-break policy. Swiss needs round pairing constraints and standings. Leagues need schedules, points systems, and season completion rules.

### Poker and blackjack

Poker and blackjack remain supported as activities, not as the organizing abstraction. They can run as casual tables, ranked ladders, leagues, or tournaments. Tournament administration can manage registration, seats, rounds, eliminations, and final placement while a game-specific rules engine owns hands, turns, and outcomes. The immutable single-elimination bracket and an Elo-style immutable competition ladder are implemented as separate progression primitives. Full tournament administration, round-robin scheduling, league tables, and poker mechanics remain future work.

The current blackjack slice is deterministic and simulation-only. Poker rules, poker hand evaluation, dealing, tournament lifecycle, and multiplayer table coordination are not implemented by this chapter.

### Current implementation boundary

Implemented:
- Individual/team competitor identity and immutable team roster changes.
- Competition format vocabulary, immutable entrant registration/capacity, and guarded lifecycle transitions.
- Deterministic single-elimination bracket generation, explicit seed order, byes, immutable match snapshots, and winner advancement.
- Immutable Elo-style ladder ratings with configurable initial rating/K-factor, win/loss/draw accounting, deterministic ranking tie-breaks, append-only result identity, evidence references, duplicate-result idempotency, and timestamp-order validation.

Not yet implemented:
- A full Tournament aggregate coordinating registration, bracket lifecycle, withdrawals, disqualifications, ties, and result correction.
- Round-robin scheduling and league points/standings policies.
- Ladder matchmaking, challenge windows, seasons/resets, rating uncertainty, and correction/rebuild tooling.
- Independent verification or durable retention of referenced match evidence.

## 04 — See it in a minute

A participant can assemble a team, then register that team in a competition. The competition sees one competitor identity; the team preserves its internal roster and roles. A tournament or ladder engine consumes registered competitor identities without changing how a team is represented. `CompetitionLadder` applies only host-adjudicated results; the host must validate evidence before recording it.

## 05 — Choose your next step

1. Add a tournament aggregate coordinating registration, bracket state, withdrawals, disqualifications, ties, and explicit correction policy.
2. Add round-robin schedule generation and configurable league standings.
3. Add ladder matchmaking/challenge policies and controlled rating-history rebuilds.
4. Add fantasy/dream-team roster construction and scoring as a separate policy family.
5. Reuse tournament administration for card games and non-card activities without coupling activity rules to the competition core.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
