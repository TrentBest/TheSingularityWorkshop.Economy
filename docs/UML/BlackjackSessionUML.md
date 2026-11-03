# Blackjack session UML

## 01 — What

**BlackjackSession** is a process-local host coordinator that binds one unresolved **CasinoWager** to one deterministic **BlackjackRound**. It owns the application-level path from player actions through settlement and payout claim.

## 02 — Why

The game engine and economic book deliberately have separate responsibilities. Without a session boundary, a host can accidentally settle a wager with a decision from another round, use a mismatched stake, or expose lifecycle actions in the wrong order. The session narrows that integration surface and checks the wager snapshot before each state-changing operation.

## 03 — How

### Type neighborhood

```mermaid
classDiagram
    class BlackjackSession {
        +Guid SessionId
        +BlackjackRoundState RoundState
        +BlackjackDecision Decision
        +CasinoWager Wager
        +Hit()
        +Stand()
        +Settle(operationId, occurredAt) CasinoWager
        +ClaimPayout(operationId, occurredAt) CasinoWager
    }
    class BlackjackRound {
        +SimulatedAmount Stake
        +BlackjackRoundState State
        +BlackjackDecision Decision
        +Hit()
        +Stand()
    }
    class CasinoWagerBook {
        +PlaceWager(...)
        +ResolveWager(...)
        +ClaimPayout(...)
        +GetWager(wagerId)
    }
    class CasinoWager {
        +Guid WagerId
        +Guid PlayerChipAccountId
        +SimulatedAmount Stake
        +CasinoWagerOutcome Outcome
    }
    BlackjackSession --> BlackjackRound : owns lifecycle access
    BlackjackSession --> CasinoWagerBook : coordinates
    BlackjackSession --> CasinoWager : binds immutable snapshot
```

### Session flow

```mermaid
sequenceDiagram
    participant Host
    participant Book as CasinoWagerBook
    participant Round as BlackjackRound
    participant Session as BlackjackSession
    Host->>Book: PlaceWager(operationId, wagerId, chips, stake)
    Book-->>Host: unresolved wager snapshot
    Host->>Round: Start(dedicated shoe, same stake)
    Round-->>Host: deterministic round
    Host->>Session: new(sessionId, round, wager, book)
    Session->>Book: GetWager(wagerId)
    Book-->>Session: current wager snapshot
    Note over Session: Validate identity, unresolved state, exact typed stake
    Host->>Session: Hit() / Stand()
    Session->>Round: player action
    Round-->>Session: completed decision
    Host->>Session: Settle(operationId)
    Session->>Book: ResolveWager(decision outcome and gross payout)
    Book-->>Session: settlement receipt
    opt Positive payout
        Host->>Session: ClaimPayout(operationId)
        Session->>Book: ClaimPayout(wagerId)
        Book-->>Session: claim receipt
    end
```

### Invariants and failure boundaries

- The session ID, wager ID, and wager placement operation ID must be non-empty.
- The supplied wager must be the current unresolved wager in the supplied book.
- The round's typed stake must exactly equal the wager's typed stake, including currency identity.
- A round cannot be settled until it produces a decision.
- Only the decision exposed by the bound round is passed to ResolveWager; callers do not supply an arbitrary outcome or payout to the session.
- Game actions stop after round completion or when the wager is externally resolved.
- Settlement and claim operation IDs remain caller-supplied and are idempotent through the wager book when retried exactly.
- A loss has no positive payout to claim; the wager book rejects such a claim.
- The session serializes its own calls, but does not make the round or wager book durable or distributed.

### Composition boundary

The host currently places the wager and starts the round before constructing the session. This avoids hiding value movement inside a game object, but means the host must handle composition failures. A dedicated shoe per round is required; concurrent sharing of one mutable shoe is unsupported. The session cannot recover a process crash or reconcile a partially composed host workflow.

## 04 — See it in a minute

BlackjackSessionTests cover successful win/settlement/claim and exact retries, mismatched typed stakes, stale or already-resolved wager snapshots, invalid lifecycle ordering, and settlement performed outside the session boundary. The session is a coordination layer, not a new game engine or ledger.

## 05 — Choose your next step

- Read the [API contract](../API/SimulationLedger.md#blackjack-session-coordination).
- Inspect [session tests](../../tests/TheSingularityWorkshop.Economy.Tests/BlackjackSessionTests.cs).
- Continue toward a durable session aggregate only after defining persistence, recovery, ownership, and concurrency contracts.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
