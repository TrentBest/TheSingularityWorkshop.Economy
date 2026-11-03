# BlackjackRound — structure and lifecycle

## 01 — What

This view documents the deterministic shoe and round coordinator. Public type relationships are source-derived; the lifecycle narrative is authored from the implementation contract.

## 02 — Why

The engine makes card order and action transitions explicit without absorbing randomization, persistence, presentation, or accounting. The same ordered shoe can reproduce the same round in tests.

## 03 — How

### Type neighborhood

```mermaid
classDiagram
    class BlackjackShoe {
        -BlackjackCard[] cards
        -int nextIndex
        +int Remaining
        +Draw() BlackjackCard
    }
    class BlackjackRound {
        -BlackjackShoe shoe
        -List~BlackjackCard~ playerCards
        -List~BlackjackCard~ dealerCards
        +SimulatedAmount Stake
        +BlackjackRoundState State
        +BlackjackDecision Decision
        +Start(shoe, stake) BlackjackRound
        +Hit() void
        +Stand() void
    }
    class BlackjackRoundState {
        <<enumeration>>
        PlayerTurn
        DealerTurn
        Completed
    }
    class BlackjackRules {
        <<static>>
        +EvaluateHand(cards) BlackjackHandValue
        +Resolve(player, dealer, stake) BlackjackDecision
    }
    class BlackjackDecision
    class CasinoWagerBook {
        +ResolveWager(operationId, wagerId, outcome, payout, time)
    }
    BlackjackRound --> BlackjackShoe : consumes ordered cards
    BlackjackRound --> BlackjackRoundState : owns lifecycle
    BlackjackRound --> BlackjackRules : evaluates / resolves
    BlackjackRules --> BlackjackDecision : produces
    CasinoWagerBook ..> BlackjackDecision : host maps result to settlement
```

### Round activity

```mermaid
flowchart TD
    A[Validate shoe and stake] --> B[Deal player, dealer, player, dealer]
    B --> C{Either initial hand natural?}
    C -- Yes --> H[Resolve completed hands]
    C -- No --> D[Player turn]
    D --> E{Action}
    E -- Hit --> F[Draw one player card]
    F --> G{Player bust?}
    G -- Yes --> H
    G -- No --> D
    E -- Stand --> I[Dealer turn: H17]
    I --> J{Dealer below 17 or soft 17?}
    J -- Yes --> K[Draw dealer card]
    K -- Yes --> I
    J -- No --> H
    H --> N[Completed decision; host settles separately]
```

### Invariants and limits

- Card order is an input, not generated state. There is no implicit random source.
- The first implementation represents one standard deck and rejects repeated rank/suit identities.
- The dealer follows hit-soft-17 (H17), not a configurable table policy.
- A player bust resolves immediately; the dealer's initial two cards are sufficient for the rules evaluator.
- Dealer shoe exhaustion preserves partial dealer progress and leaves the round in DealerTurn.
- A completed decision is not a ledger posting. The host separately calls CasinoWagerBook with an operation ID.
- Shoe and round state are volatile and not safe for crash recovery or distributed play.

## 04 — See it in a minute

Executable lifecycle evidence: [BlackjackRoundTests.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/BlackjackRoundTests.cs). Implementation: [BlackjackRound.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/BlackjackRound.cs).

## 05 — Choose your next step

- Add multi-deck card identity and table-configurable dealer policy.
- Add round-to-wager integration tests with stable caller-owned operation IDs.
- Design durable round snapshots and shoe provenance before multiplayer or restart-recovery claims.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
