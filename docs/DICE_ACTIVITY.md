# Dice activity: domain contract and implementation boundary

## Purpose

Dice are a reusable random-outcome activity. They can support casual play, tabletop and role-playing rules, classroom probability demonstrations, and simulated casino-style activities. A roll is meaningful without a wager.

The Economy package owns the die definition and the authoritative result. It does not own a physical die, a collision world, a camera, pointer gestures, or a rendered animation.

## First executable slice

The initial implementation is intentionally small:

- `DiceDefinition` describes a die with equally likely integer faces numbered from one through its face count. `DiceDefinition.SixSided` is the standard d6.
- `IDiceRandomSource` injects the source of each outcome. Its upper bound is exclusive.
- `SystemDiceRandomSource` supplies ordinary application randomness. It makes no cryptographic fairness or external-audit claim.
- `DiceRoller` resolves one to 100 dice and produces a unique roll ID, timestamp, face count, immutable face snapshot, and total.
- Tests use a scripted random source and fixed clock; no wager book, account, or ledger is required.

Randomness, rule interpretation, economic settlement, and presentation are separate concerns. The initial slice records faces and total; named rules such as doubles, highest die, or target thresholds should be separate pure evaluators rather than hidden inside random generation.

## Presentation and physics contract

A future WebPage, Visualize, or Physics adapter may animate dice being shaken, thrown, colliding, and settling. That simulation must reconcile to the already-resolved `DiceRoll`. It may not choose or alter the authoritative face values. The animation is presentation, not proof of randomness.

Pointer and touch gestures are optional ways to request a roll. Accessible button and keyboard controls must offer the same domain operation. The Economy assembly must not acquire a dependency on WebPage, Visualize, or a physics engine.

## Wager boundary

A resolved roll may later be consumed by a separately configured simulated game rule and the existing casino wager lifecycle. Dice rolling itself does not create a stake, reserve value, pay a prize, or mutate the ledger. This package adds no real-money gambling, deposits, withdrawals, cash prizes, or redemption.

## Limits of this slice

The first implementation does not yet provide cryptographic or externally audited randomness, caller-supplied replay/idempotency keys, roll persistence/history storage, configurable weighted faces, non-cubic geometry, named rule evaluators, a WebPage demonstration, or physical animation. Those are separate increments and must not be implied by the current API.

## Evidence

- Source: `src/TheSingularityWorkshop.Economy/Dice.cs`
- Tests: `tests/TheSingularityWorkshop.Economy.Tests/DiceRollerTests.cs`
- Design and acceptance boundary: [issue #26](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/26)

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
