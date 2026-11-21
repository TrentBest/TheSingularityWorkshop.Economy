# Round-robin schedule and standings contract

## Purpose

`RoundRobinSchedule` creates a deterministic, immutable schedule for every entrant pair. It uses the circle method: an even field plays `n - 1` rounds; an odd field adds a virtual slot and plays `n` rounds, with exactly one automatic bye in each round.

## Structural view

```mermaid
classDiagram
    class RoundRobinSchedule {
        +Guid CompetitionId
        +Rounds
        +Matches
        +StandingsPolicy
        +Standings
        +Create(competitionId, entrants)
        +RecordResult(result)
        +FindMatch(matchId)
    }
    class RoundRobinMatch {
        +RoundRobinMatchId Id
        +CompetitorId FirstCompetitorId
        +CompetitorId SecondCompetitorId
        +CompetitorId ByeCompetitorId
        +RoundRobinMatchResult Result
        +IsBye
        +IsCompleted
    }
    class RoundRobinMatchResult {
        +RoundRobinResultId Id
        +RoundRobinMatchId MatchId
        +RoundRobinOutcome Outcome
        +DateTimeOffset RecordedAt
        +string EvidenceReference
    }
    class RoundRobinStanding {
        +CompetitorId CompetitorId
        +int MatchesPlayed
        +int Wins
        +int Draws
        +int Losses
        +int Byes
        +decimal Points
    }
    class RoundRobinStandingsPolicy {
        +decimal WinPoints
        +decimal DrawPoints
        +decimal LossPoints
        +TieBreakers
    }
    class RoundRobinTieBreakCriterion {
        <<enumeration>>
        Points
        Wins
        Draws
        FewestLosses
        CompetitorId
    }
    RoundRobinSchedule "1" *-- "many" RoundRobinMatch : immutable rounds
    RoundRobinMatch "0..1" --> RoundRobinMatchResult : host result
    RoundRobinSchedule ..> RoundRobinStanding : derives
    RoundRobinSchedule --> RoundRobinStandingsPolicy : applies
    RoundRobinStandingsPolicy --> RoundRobinTieBreakCriterion : orders by
```

## Public contract

- `RoundRobinMatchId` identifies a competition, round, and match.
- `RoundRobinMatchResult` records a stable result ID, scheduled match, outcome, timestamp, and host evidence reference.
- `RoundRobinMatch` represents either a played pairing or a bye. Byes are not played matches and cannot accept results.
- `RoundRobinSchedule.Create` validates the competition identity and unique entrants, then generates all rounds in input order.
- `RecordResult` returns a new immutable schedule. Exact duplicate result submissions are idempotent; conflicting reuse of a result ID is rejected.
- `Standings` derives played, win, draw, loss, bye, and points totals from recorded results and scheduled byes.

## Points and tie-break policy

Scoring and ordering are supplied by an immutable `RoundRobinStandingsPolicy` captured by the schedule and preserved by every result snapshot. The default remains:

1. Win: 3 points.
2. Draw: 1 point.
3. Loss: 0 points.
4. Points descending, then wins descending.
5. Competitor Guid ascending as the mandatory final deterministic tie-break.

A host may select different non-negative decimal point values satisfying `win > draw >= loss`, and reorder supported aggregate criteria: points, wins, draws, or fewest losses. `CompetitorId` must be unique and final. The policy snapshots its criteria, rejects invalid/duplicate criteria, and is not mutable after schedule creation.

Head-to-head, score differential, away-goal, strength-of-schedule, and sport-specific metrics are intentionally not built into the generic engine. They require separately named policy inputs and the relevant result data; the engine does not fabricate those values.

## Evidence and authority

The host must validate the result under the activity's rules before calling `RecordResult`. Economy retains the evidence reference but does not fetch, inspect, verify, or retain the underlying evidence. ServerApp remains the orchestration boundary; activity-specific adapters own match sessions and evidence validation.

## Invariants covered by tests

- Every unordered entrant pair appears exactly once.
- Every round contains each entrant exactly once, including a bye for one entrant when the field is odd.
- Odd fields receive one bye per competitor across a complete schedule.
- Results update standings without mutating prior schedule snapshots.
- Duplicate result identity/data is idempotent; conflicting identity reuse is rejected.
- Default draws award one point to each participant; custom fractional scoring is supported.
- Custom tie-break order is applied and retained across immutable snapshots.
- Invalid point relationships, duplicate criteria, and a non-final CompetitorId tie-break are rejected.
- Byes do not award match points.

## Deliberate gaps

This is schedule generation and configurable aggregate standings, not a full league administration system. It does not allocate dates, venues, home/away policy, travel constraints, withdrawals, forfeits, score aggregation, tie-break adjudication, season lifecycle, or persistence.
