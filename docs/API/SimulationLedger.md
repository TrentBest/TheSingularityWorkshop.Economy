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
| SimulationLedgerReceipt | Applied-operation receipt, including replay indication |
| SimulationLedger | Registration, posting, balance, journal snapshot, and receipt lookup |

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
- Retire debits an account and records an explicit retirement posting.
- GetBalance sums the account's signed journal entries.
- GetEntries returns a read-only snapshot for the selected account.
- GetReceipt returns the original operation receipt when the operation ID is known.

Every operation requires a non-empty caller-supplied operation ID, a positive amount, and an explicit timestamp. Amount precision cannot exceed the currency's configured decimal places.

### Retry semantics

An exact retry with the same operation ID, operation kind, account identities, amount, and currency returns the original receipt with IsReplay set to true. The original timestamp remains authoritative. Reusing an operation ID for a different request throws InvalidOperationException.

This is an in-memory idempotency boundary. It does not survive process restart and is not a distributed idempotency guarantee.

### Source and evidence

- Implementation: [SimulationLedger.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/SimulationLedger.cs)
- Tests: [SimulationLedgerTests.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/tests/TheSingularityWorkshop.Economy.Tests/SimulationLedgerTests.cs)
- Related legacy vocabulary: [Money.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/Money.cs), [EconomyAccount.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/EconomyAccount.cs), and [Transfer.cs](https://github.com/TrentBest/TheSingularityWorkshop.Economy/blob/development/src/TheSingularityWorkshop.Economy/Transfer.cs)

The tests exercise explicit issuance, transfer conservation, idempotent retry, conflicting operation IDs, insufficient funds, currency isolation, conflicting currency metadata, retirement, account registration, precision, and read-only journal snapshots.

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

This example is illustrative source usage; the automated test suite is the executable verification. The ledger is process-local and volatile.

## 05 — Choose your next step

- See the [target-centered UML and use cases](../UML/SimulationLedgerUML.md).
- Read [the Economy theory path](../THEORY/README.md).
- Follow [issue #14](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/14) for casino custody and the remaining authoritative simulation slice.
- Read [Architecture](../ARCHITECTURE.md) for the wider ownership boundary.


<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
