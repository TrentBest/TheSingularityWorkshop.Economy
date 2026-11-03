# BlackjackRound — deterministic round lifecycle

## 01 — What

`BlackjackShoe` and `BlackjackRound` provide a deterministic, single-player blackjack lifecycle over an explicitly supplied card order. The public lifecycle states are `PlayerTurn`, `DealerTurn`, and `Completed`.

## 02 — Why

A pure hand evaluator cannot prove that a game can be played reproducibly. The round engine owns card consumption and legal action transitions while keeping randomness, persistence, UI, and money movement outside the rules boundary.

## 03 — How

- The host supplies the complete ordered shoe. There is no implicit shuffle, RNG, or seed.
- A shoe rejects empty input, invalid ranks/suits, and duplicate physical cards. This first implementation models one standard deck; multi-deck shoes need deck-instance identity before repeated rank/suit pairs can be represented.
- Initial deal order is player, dealer, player, dealer.
- A player may Hit once per call while in PlayerTurn. A bust immediately completes as a loss.
- Stand enters DealerTurn. The dealer hits below 17 and also hits soft 17 (H17); dealer play stops on hard 17 or higher, or bust.
- If the shoe is exhausted during dealer play, the round remains in DealerTurn with all successfully drawn cards preserved. Calling Stand again resumes the deterministic dealer process, but the same exhausted shoe will continue to reject draws unless a future explicit continuation mechanism is added.
- A natural on either initial hand completes immediately. The existing `BlackjackRules.Resolve` decides natural-vs-natural push and natural-vs-ordinary-21 outcomes.
- The round exposes immutable card snapshots and an optional completed `BlackjackDecision`. It does not call `CasinoWagerBook`; a host must explicitly settle the wager using a caller-owned operation ID.

## 04 — See it in a minute

    var shoe = new BlackjackShoe(orderedCards);
    var round = BlackjackRound.Start(shoe, stake);
    round.Hit();   // only while PlayerTurn
    round.Stand(); // dealer follows H17 policy
    if (round.Decision is { } decision)
        wagerBook.ResolveWager(operationId, wagerId, decision.Outcome,
            decision.GrossPayout, occurredAt);

This is a deterministic simulation contract, not a production gambling service. The shoe and round are in-memory objects; there is no persistence, recovery, randomization, insurance, split, double-down, surrender, betting-limit enforcement, or table integration.

## 05 — Choose your next step

- Read [Blackjack rules API](SimulationLedger.md#blackjack-rules) for hand evaluation and payout semantics.
- Read [Casino custody and wager lifecycle](SimulationLedger.md#casino-custody-and-wager-lifecycle) for separate accounting ownership.
- Inspect [BlackjackRound UML](../UML/BlackjackRoundUML.md) and [round lifecycle tests](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/BlackjackRoundTests.cs).


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
