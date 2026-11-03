# SimulationLedger — API and contract

## 01 — What

SimulationLedger is a process-local, thread-safe journal for fictional-currency simulation. It registers typed currency accounts, records explicit issuance, transfers, and retirement, and derives account balances from signed journal entries.

The supporting public types are:

| Type | Responsibility |
|---|---|
| SimulatedCurrencyId | Stable identity distinct from a three-letter currency code |
| SimulatedCurrency | Name, display symbol, and allowed decimal precision for a fictional unit |
| SimulatedAmount | Non-negative amount paired with a SimulatedCurrencyId |
| SimulationAccount | Account identity, owner, and currency |
| SimulationLedgerOperationKind | Issuance, transfer, or retirement classification |
| SimulationLedgerEntry | Signed account posting |
| SimulationLedgerPosting | Caller-supplied signed posting for a balanced multi-account transfer |
| SimulationLedgerReceipt | Applied-operation receipt, including replay indication and immutable multi-account posting snapshot |
| SimulationLedger | Registration, posting, balance, journal snapshot, and receipt lookup |
| CasinoCashier | One-for-one simulated wallet/chip buy-in and cash-out transfers |
| CasinoWagerOutcome | Loss, push, and win classifications |
| CasinoWager | Immutable wager state and operation receipts |
| CasinoWagerBook | Escrow, atomic resolution, payouts owed, and payout claim coordination |

## 02 — Why

A shop price and a transfer intention do not constitute a balance system. SimulationLedger establishes a small authoritative in-process boundary where a successful operation has inspectable postings and rejected operations do not mutate the journal.

The ledger uses typed currency identity so two fictional currencies with the same display symbol remain distinct. It does not reinterpret the existing Money type or its normalized three-letter code.

## 03 — How

### Register accounts

Create a SimulatedCurrency with a stable ID, descriptive name, display symbol, and decimal precision. Create SimulationAccount instances that reference that currency, then register them with one SimulationLedger instance.

The same currency ID cannot be registered with conflicting metadata inside one ledger. Account IDs must be unique within the ledger.

### Record economic operations

- Issue credits an account and records an explicit issuance posting.
- Transfer validates both accounts and the currency, checks available funds, then records a debit and credit while holding the ledger lock.
- Post applies a balanced transfer across two or more accounts. Each account appears once; every posting uses the same registered currency and valid precision; signed deltas total exactly zero. All resulting balances are checked before any state changes. The receipt stores the aggregate positive side in a decimal SimulatedAmount, so the total of positive postings must also fit decimal; a larger otherwise-balanced settlement is rejected with overflow before commit.
- Retire debits an account and records an explicit retirement posting.
- GetBalance sums the account's signed journal entries.
- GetEntries returns a read-only snapshot for the selected account.
- GetReceipt returns the original operation receipt when the operation ID is known.

Every operation requires a non-empty caller-supplied operation ID, a positive amount, and an explicit timestamp. Amount precision cannot exceed the currency's configured decimal places.

### Retry semantics

An exact retry with the same operation ID and normalized request returns the original receipt with IsReplay set to true. For Post, posting order does not affect the fingerprint; account IDs, currency IDs, and signed amounts do. The original timestamp remains authoritative. Reusing an operation ID for a different request throws InvalidOperationException.

This is an in-memory idempotency boundary. It does not survive process restart and is not a distributed idempotency guarantee.

### Source and evidence

- Implementation: [SimulationLedger.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/SimulationLedger.cs)
- Tests: [SimulationLedgerTests.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/SimulationLedgerTests.cs)
- Related legacy vocabulary: [Money.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/Money.cs), [EconomyAccount.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/EconomyAccount.cs), and [Transfer.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/Transfer.cs)

The tests exercise explicit issuance, two-account transfer conservation, three-account atomic settlement, late insufficient-funds rejection, account-balance overflow and aggregate receipt-amount overflow rejection, balanced totals, currency and precision validation, exact and conflicting retries, concurrent competing settlements, retirement, account registration, and read-only journal/receipt snapshots.

### Casino custody and wager lifecycle

`CasinoCashier.BuyIn` transfers from the participant wallet account to the participant chip account; `CashOut` reverses that transfer. Both are ordinary same-currency ledger transfers and cannot create value.

`CasinoWagerBook` is composed with registered house-reserve, wager-escrow, and payouts-owed accounts. `PlaceWager` transfers a positive stake from the player's chip account into escrow. `ResolveWager` accepts an explicit outcome and gross payout: loss requires zero, push returns exactly the stake, and win requires a gross payout greater than the stake. Win settlement debits the house reserve by net winnings and credits payouts owed by the gross payout, while debiting escrow by the original stake. `ClaimPayout` transfers the payable amount to the player's chip account once.

Every transition has a caller-supplied operation ID, and every wager has its own stable wager ID. This coordinator is process-local and volatile; it does not generate random outcomes, enforce table limits, persist state, or recover after a crash. A future game engine must supply deterministic/reproducible outcomes and explicit rules rather than asking this accounting component to invent them.

### Blackjack round lifecycle

`BlackjackShoe` consumes a caller-supplied ordered sequence without shuffling. `BlackjackRound.Start` deals player/dealer/player/dealer and exposes PlayerTurn, DealerTurn, and Completed states. Hit draws one player card and completes a bust immediately. Stand runs the dealer under an explicit hit-soft-17 (H17) policy. A natural on either initial hand completes immediately. The round returns a `BlackjackDecision` but never mutates the ledger; the host must explicitly pass that decision to `CasinoWagerBook.ResolveWager`. The shoe and round are process-local. The first shoe rejects duplicate physical rank/suit identities and therefore models one deck; multi-deck support requires deck-instance identity. If the dealer exhausts the shoe during play, the state remains DealerTurn with already drawn cards preserved.

Source: [BlackjackRound.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/BlackjackRound.cs). Tests: [BlackjackRoundTests.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/BlackjackRoundTests.cs). UML: [BlackjackRound UML](../UML/BlackjackRoundUML.md).

### Blackjack rules

The pure BlackjackRules evaluator accepts fully supplied player/dealer card lists and a positive typed stake. EvaluateHand calculates the best hand total, soft-ace status, bust status, and two-card natural status. Resolve validates a completed round (at least two cards per hand, no duplicate physical card across the round) and returns a BlackjackDecision containing both hand values, CasinoWagerOutcome, and gross payout.

A natural beats an ordinary 21; equal totals push; a player bust loses; a dealer bust wins only if the player has not busted. This initial payout schedule is deliberately even-money: a win returns twice the stake as gross payout, a push returns the stake, and a loss returns zero. A natural bonus is not implemented.

The evaluator is deterministic and has no random source, shoe, draw lifecycle, player-action state machine, or ledger dependency. The host supplies card inputs and then explicitly passes the decision to CasinoWagerBook.ResolveWager. Dealer soft-17 policy is not applicable until a dealer drawing policy exists.

Source: [BlackjackRules.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/BlackjackRules.cs). Tests: [BlackjackRulesTests.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/BlackjackRulesTests.cs). Structural and decision-flow views: [BlackjackRules UML](../UML/BlackjackRulesUML.md).

## 04 — See it in a minute

    var currency = new SimulatedCurrency(
        SimulatedCurrencyId.New(),
        "Singularity Denomination",
        "SD",
        decimalPlaces: 2);

    var owner = EconomyEntityId.New();
    var account = new SimulationAccount(Guid.NewGuid(), owner, currency);

    var ledger = new SimulationLedger();
    ledger.RegisterAccount(account);

    ledger.Issue(
        Guid.NewGuid(),
        account.Id,
        new SimulatedAmount(100m, currency.Id),
        DateTimeOffset.UtcNow);

    var balance = ledger.GetBalance(account.Id); // 100

    var house = new SimulationAccount(Guid.NewGuid(), EconomyEntityId.New(), currency);
    var escrow = new SimulationAccount(Guid.NewGuid(), EconomyEntityId.New(), currency);
    ledger.RegisterAccount(house);
    ledger.RegisterAccount(escrow);

    ledger.Post(
        Guid.NewGuid(),
        new[]
        {
            new SimulationLedgerPosting(account.Id, currency.Id, -30m),
            new SimulationLedgerPosting(house.Id, currency.Id, 10m),
            new SimulationLedgerPosting(escrow.Id, currency.Id, 20m)
        },
        DateTimeOffset.UtcNow);

This example is illustrative source usage; the automated test suite is the executable verification. The ledger is process-local and volatile.

## 05 — Choose your next step

- See the [target-centered UML and use cases](../UML/SimulationLedgerUML.md).
- Read [the Economy theory path](../THEORY/README.md).
- Follow [issue #14](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/14) for casino custody and the remaining authoritative simulation slice.
- Read [Architecture](../ARCHITECTURE.md) for the wider ownership boundary.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
