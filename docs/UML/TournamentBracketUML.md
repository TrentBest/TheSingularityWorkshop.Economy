# Single-elimination tournament bracket

## 01 — What

SingleEliminationBracket creates a deterministic, immutable elimination bracket from a fixed entrant set and seed order. It generates power-of-two bracket slots, assigns standard seed pairings, grants automatic first-round byes, and advances only a competitor explicitly recorded as the winner of a ready match.

The bracket is an administration primitive. It does not decide who won a sport or game; the host validates the activity result and records the winner.

## 02 — Why

Tournament administration should not be reimplemented for every activity. A sports match, esports round, poker table, or blackjack round can all produce a validated winner. The bracket only needs competitor identities and progression rules.

Keeping the bracket deterministic makes it testable and replayable. Keeping it immutable means each recorded result creates a new snapshot rather than silently mutating an earlier bracket.

## 03 — How

### Type neighborhood

```mermaid
classDiagram
    class SingleEliminationBracket {
      +Guid TournamentId
      +Rounds
      +FirstRound
      +CompetitorId? Champion
      +Create(tournamentId, entrants, seedOrder)
      +FindMatch(matchId)
      +RecordWinner(matchId, winnerId)
      +RecordResult(result)
    }
    class TournamentMatch {
      +TournamentMatchId Id
      +CompetitorId? FirstCompetitorId
      +CompetitorId? SecondCompetitorId
      +TournamentMatchStatus Status
      +CompetitorId? WinnerId
      +TournamentMatchResult? Result
    }
    class TournamentMatchResult {
      +TournamentResultId Id
      +TournamentMatchId MatchId
      +CompetitorId WinnerId
      +DateTimeOffset RecordedAt
      +string EvidenceReference
    }
    class Competitor {
      +CompetitorId Id
      +string DisplayName
      +CompetitorKind Kind
    }
    SingleEliminationBracket *-- TournamentMatch : rounds
    TournamentMatch o-- TournamentMatchResult : optional provenance
    SingleEliminationBracket ..> Competitor : registered entrant identities
```

### Progression

```mermaid
flowchart TD
    A[Validate tournament and entrant identities] --> B[Validate explicit seed order]
    B --> C[Expand to next power-of-two bracket size]
    C --> D[Place seeds in deterministic bracket order]
    D --> E[Create first-round pairings and later waiting slots]
    E --> F{First-round slot has one competitor?}
    F -->|Yes| G[Mark Bye and advance automatically]
    F -->|No| H[Mark Ready]
    G --> I[Wait for remaining match results]
    H --> J[Host validates activity result]
    J --> K[RecordWinner for one of the two competitors]
    K --> L[Advance winner into corresponding next-round slot]
    I --> M{Both next-round slots filled?}
    L --> M
    M -->|Yes| N[Next match Ready]
    M -->|No| I
    N --> J
```

### Invariants

- A bracket has at least two unique competitors.
- An explicit seed order must contain every entrant exactly once.
- The bracket size is the next power of two; unused seeds become first-round byes.
- Seed order determines pairings. The default is the supplied entrant order.
- A bye automatically advances its only competitor and cannot be manually resolved.
- A result can be recorded only for a Ready match with two competitors.
- The winner must be one of the match's competitors.
- A match can be completed only once.
- `RecordResult` retains stable result identity, timestamp, winner, and a nonempty host evidence reference.
- Replaying the exact result ID and data is idempotent; conflicting reuse of the same result ID is rejected.
- The evidence reference is not verified by Economy; the host remains responsible for validation and durable evidence retention.
- A result advances into the corresponding slot of the next round.
- The prior bracket snapshot remains unchanged after recording a result.

### Explicit maturity boundary

Implemented: deterministic single-elimination bracket creation, standard seeding order, first-round byes, immutable match snapshots, validated winner recording, provenance-aware `RecordResult`, duplicate-result idempotency, and round advancement.

Not implemented: a full Tournament aggregate, scheduling/time slots, activity result validation, ties, withdrawals, disqualification, bracket correction/reopen, double elimination, Swiss, round robin, standings, persistence, distributed coordination, entry fees, or prize distribution.

## 04 — See it in a minute

```csharp
var bracket = SingleEliminationBracket.Create(
    tournamentId,
    registeredCompetitors,
    seededCompetitorIds);

var firstMatch = bracket.FirstRound[0];
// The host runs the match and validates its result.
var result = new TournamentMatchResult(
    TournamentResultId.New(), firstMatch.Id, validatedWinnerId,
    DateTimeOffset.UtcNow, authoritativeEvidenceReference);
var nextBracket = bracket.RecordResult(result);
```

The bracket owns the progression rule, not the game. The caller must validate the activity outcome and retain the referenced evidence. `RecordWinner` remains available as a winner-only compatibility path; use `RecordResult` when result provenance is required.

## 05 — Choose your next step

- Add a Tournament aggregate that binds a competition registration snapshot to its bracket.
- Extend the result contract with explicit tie, withdrawal, disqualification, and correction policies; current evidence references are host-supplied pointers, not verified artifacts.
- Add round-robin scheduling and standings after the elimination contract is proven.
- Add persistence and recovery before multiplayer or crash-resilient tournament hosting.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
