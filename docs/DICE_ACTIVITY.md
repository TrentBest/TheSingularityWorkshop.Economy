# Dice activity: domain contract and implementation boundary

## Purpose

Dice are a reusable random-outcome activity. They can support casual play, tabletop and role-playing rules, classroom probability demonstrations, and simulated casino-style activities. A roll is meaningful without a wager.

The Economy package owns die definitions, authoritative results, and deterministic rule interpretation. It does not own a physical die, a collision world, a camera, pointer gestures, or a rendered animation.

## Outcome generation

- `DiceDefinition` describes a die with equally likely integer faces numbered from one through its face count. `DiceDefinition.SixSided` is the standard d6.
- `IDiceRandomSource` injects the source of each outcome. Its upper bound is exclusive.
- `SystemDiceRandomSource` supplies ordinary application randomness. It makes no cryptographic fairness or external-audit claim.
- `DiceRoller` resolves one to 100 dice and produces a unique roll ID, timestamp, face count, immutable face snapshot, and total.
- Tests use a scripted random source and fixed clock; no wager book, account, or ledger is required.

## Rule interpretation

Rule evaluation is deliberately downstream of rolling. `DiceRuleDefinition` describes an interpretation and `DiceRuleEvaluator` applies it to a completed `DiceRoll`; it never asks the random source for another value and never mutates the result.

The first pure rules are:

| Rule | Meaning | Output |
|---|---|---|
| Sum | Add every resolved face | Numeric total |
| Highest | Select the greatest resolved face | Numeric face |
| Doubles | Require exactly two dice and compare their faces | Boolean match |
| AtLeastTotal(target) | Compare the sum with a positive threshold | Total and Boolean match |

A value-only rule returns no predicate. Predicate rules return an explicit match value. Invalid face data and a doubles rule applied to anything other than two dice are rejected. These are intentionally small primitives; game-specific scoring, payouts, and probability disclosures belong to their own contracts.

## Presentation and physics contract

A future WebPage, Visualize, or Physics adapter may animate dice being shaken, thrown, colliding, and settling. That simulation must reconcile to the already-resolved `DiceRoll`. It may not choose or alter the authoritative face values. The animation is presentation, not proof of randomness.

Pointer and touch gestures are optional ways to request a roll. Accessible button and keyboard controls must offer the same domain operation. The Economy assembly must not acquire a dependency on WebPage, Visualize, or a physics engine.

## Wager boundary

A resolved roll may later be consumed by a separately configured simulated game rule and the existing casino wager lifecycle. Dice rolling and rule evaluation do not create a stake, reserve value, pay a prize, or mutate the ledger. This package adds no real-money gambling, deposits, withdrawals, cash prizes, or redemption.

## Limits of this slice

The implementation does not yet provide cryptographic or externally audited randomness, caller-supplied replay/idempotency keys, roll persistence/history storage, configurable weighted faces, non-cubic geometry, a WebPage demonstration, or physical animation. Named rules are currently limited to the four listed above. Those capabilities must not be implied by the current API.

## Evidence

- Outcome source: `src/TheSingularityWorkshop.Economy/Dice.cs`
- Rule source: `src/TheSingularityWorkshop.Economy/DiceRules.cs`
- Tests: `tests/TheSingularityWorkshop.Economy.Tests/DiceRollerTests.cs` and `DiceRuleEvaluatorTests.cs`
- Design and acceptance boundary: [issue #26](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/26)

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
