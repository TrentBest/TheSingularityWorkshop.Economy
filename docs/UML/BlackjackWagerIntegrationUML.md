# Blackjack host settlement integration

## 01 — What

This sequence documents the host-owned boundary between a deterministic blackjack round and the simulated casino ledger. It is based on the integration tests, not a production session service.

## 02 — Why

Game rules and economic settlement have different authority. The round decides an outcome from an explicit shoe and player action sequence. The wager book moves simulated value only after the host submits that completed decision. Keeping the boundary explicit prevents a card draw, UI animation, or incomplete round from directly mutating balances.

## 03 — How

\`\`\`mermaid
sequenceDiagram
    autonumber
    participant Host as Game Host
    participant Cashier as CasinoCashier
    participant Ledger as SimulationLedger
    participant Book as CasinoWagerBook
    participant Round as BlackjackRound

    Host->>Cashier: BuyIn(wallet, chips, amount)
    Cashier->>Ledger: Transfer(wallet → chips)
    Ledger-->>Cashier: receipt
    Cashier-->>Host: receipt

    Host->>Book: PlaceWager(chips, stake)
    Book->>Ledger: Transfer(chips → escrow)
    Ledger-->>Book: receipt
    Book-->>Host: open wager

    Host->>Round: Start(ordered shoe, stake)
    Host->>Round: Hit / Stand
    Round-->>Host: Completed + BlackjackDecision

    Host->>Book: ResolveWager(outcome, gross payout)
    Book->>Ledger: Post(escrow, house reserve, payouts owed)
    Ledger-->>Book: settlement receipt
    Book-->>Host: resolved wager

    opt Positive payout
        Host->>Book: ClaimPayout()
        Book->>Ledger: Transfer(payouts owed → chips)
        Ledger-->>Book: claim receipt
        Book-->>Host: claimed wager
    end
\`\`\`

The host supplies distinct operation IDs for buy-in, wager placement, resolution, and claim. Exact retries reuse the original operation ID and request; conflicting reuse is rejected by the applicable ledger/wager boundary.

## 04 — See it in a minute

The executable counterpart is [BlackjackWagerIntegrationTests.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/BlackjackWagerIntegrationTests.cs).

The tests cover:

- A deterministic ordinary win, gross payout, resolution replay, and player claim.
- A player bust resolved as a loss with no claimable payout.
- A push returned through the payouts-owed account and claimed once.
- Insufficient house reserve rejecting a winning settlement without partially changing the wager or ledger.
- Conservation of the simulated balances across the tested win flow.

## 05 — Choose your next step

- Read [Blackjack round lifecycle](BlackjackRoundUML.md) for shoe and player/dealer state transitions.
- Read [Casino wager accounting](CasinoWagerBookUML.md) for escrow, settlement, and claims.
- Read [SimulationLedger API](../API/SimulationLedger.md) for posting and retry contracts.
- Follow [issue #14](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/14) for remaining casino simulation work.

### Scope limits

This is a process-local test seam. It does not provide durable session identity, crash recovery, a random/shuffled shoe, multiplayer coordination, table limits, a venue runtime, or browser UI. The host is responsible for validating that the decision belongs to the wager and round being settled; a future session aggregate should make that relationship explicit.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
