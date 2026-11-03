using System.Globalization;

namespace TheSingularityWorkshop.Economy;

/// <summary>Stable identity for a fictional, in-simulation currency.</summary>
public readonly record struct SimulatedCurrencyId(Guid Value)
{
    public static SimulatedCurrencyId New() => new(Guid.NewGuid());
}

/// <summary>
/// Defines a fictional currency used only inside a simulation.
/// Its identity is independent of Money's three-letter currency/unit code.
/// </summary>
public sealed record SimulatedCurrency
{
    public SimulatedCurrency(
        SimulatedCurrencyId id,
        string name,
        string symbol,
        int decimalPlaces = 2)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A currency identifier is required.", nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        if (decimalPlaces is < 0 or > 8)
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces));

        Id = id;
        Name = name.Trim();
        Symbol = symbol.Trim();
        DecimalPlaces = decimalPlaces;
    }

    public SimulatedCurrencyId Id { get; }
    public string Name { get; }
    public string Symbol { get; }
    public int DecimalPlaces { get; }
}

/// <summary>A non-negative quantity denominated in one typed simulated currency.</summary>
public readonly record struct SimulatedAmount
{
    public SimulatedAmount(decimal value, SimulatedCurrencyId currencyId)
    {
        if (value < 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "A simulated amount cannot be negative.");
        if (currencyId.Value == Guid.Empty)
            throw new ArgumentException("A simulated currency identifier is required.", nameof(currencyId));

        Value = value;
        CurrencyId = currencyId;
    }

    public decimal Value { get; }
    public SimulatedCurrencyId CurrencyId { get; }
}

/// <summary>An owner-controlled account boundary in a simulated currency.</summary>
public sealed record SimulationAccount
{
    public SimulationAccount(Guid id, EconomyEntityId owner, SimulatedCurrency currency)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("An account identifier is required.", nameof(id));
        if (owner.Value == Guid.Empty)
            throw new ArgumentException("An account owner is required.", nameof(owner));
        ArgumentNullException.ThrowIfNull(currency);

        Id = id;
        Owner = owner;
        Currency = currency;
    }

    public Guid Id { get; }
    public EconomyEntityId Owner { get; }
    public SimulatedCurrency Currency { get; }
}

/// <summary>Classifies an explicit simulated ledger operation.</summary>
public enum SimulationLedgerOperationKind
{
    Issuance,
    Transfer,
    Retirement
}

/// <summary>One signed account posting produced by a simulated ledger operation.</summary>
public sealed record SimulationLedgerEntry(
    Guid OperationId,
    Guid AccountId,
    SimulatedCurrencyId CurrencyId,
    decimal Delta,
    SimulationLedgerOperationKind Kind,
    DateTimeOffset OccurredAt);

/// <summary>One signed posting in a balanced multi-account simulated transfer.</summary>
public sealed record SimulationLedgerPosting(
    Guid AccountId,
    SimulatedCurrencyId CurrencyId,
    decimal Delta);

/// <summary>Receipt for an applied or idempotently replayed ledger operation.</summary>
public sealed record SimulationLedgerReceipt(
    Guid OperationId,
    SimulationLedgerOperationKind Kind,
    Guid? FromAccountId,
    Guid? ToAccountId,
    SimulatedAmount Amount,
    DateTimeOffset OccurredAt,
    bool IsReplay = false,
    IReadOnlyList<SimulationLedgerPosting>? Postings = null);

/// <summary>
/// Process-local, thread-safe ledger for deterministic fictional-currency simulation.
/// Balances are derived from signed journal entries; issuance and retirement are explicit.
/// This class does not provide persistence, distributed transactions, or real-money custody.
/// </summary>
public sealed class SimulationLedger
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, SimulationAccount> _accounts = new();
    private readonly Dictionary<Guid, decimal> _balances = new();
    private readonly Dictionary<SimulatedCurrencyId, SimulatedCurrency> _currencies = new();
    private readonly Dictionary<Guid, SimulationLedgerReceipt> _receipts = new();
    private readonly Dictionary<Guid, OperationFingerprint> _fingerprints = new();
    private readonly List<SimulationLedgerEntry> _entries = new();

    /// <summary>Registers an account before it can participate in ledger operations.</summary>
    public void RegisterAccount(SimulationAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        lock (_sync)
        {
            if (_accounts.ContainsKey(account.Id))
                throw new InvalidOperationException($"Account {account.Id} is already registered.");

            if (_currencies.TryGetValue(account.Currency.Id, out var existingCurrency))
            {
                if (existingCurrency != account.Currency)
                    throw new InvalidOperationException(
                        $"Currency identity {account.Currency.Id.Value} is already registered with different metadata.");
            }
            else
            {
                _currencies.Add(account.Currency.Id, account.Currency);
            }

            _accounts.Add(account.Id, account);
            _balances.Add(account.Id, 0m);
        }
    }

    /// <summary>Creates value explicitly; issuance is recorded as a one-sided credit.</summary>
    public SimulationLedgerReceipt Issue(
        Guid operationId,
        Guid accountId,
        SimulatedAmount amount,
        DateTimeOffset occurredAt)
    {
        ValidateOperationId(operationId);
        ValidatePositiveAmount(amount);

        lock (_sync)
        {
            var fingerprint = new OperationFingerprint(
                SimulationLedgerOperationKind.Issuance, null, accountId, amount);
            if (TryReplay(operationId, fingerprint, out var replay))
                return replay;

            var account = GetAccount(accountId);
            EnsureCurrency(account, amount);

            var receipt = new SimulationLedgerReceipt(
                operationId, SimulationLedgerOperationKind.Issuance, null, accountId, amount, occurredAt);
            Commit(receipt, fingerprint,
                new SimulationLedgerEntry(operationId, accountId, amount.CurrencyId, amount.Value,
                    SimulationLedgerOperationKind.Issuance, occurredAt));
            return receipt;
        }
    }

    /// <summary>Moves value atomically between two registered accounts in the same currency.</summary>
    public SimulationLedgerReceipt Transfer(
        Guid operationId,
        Guid fromAccountId,
        Guid toAccountId,
        SimulatedAmount amount,
        DateTimeOffset occurredAt)
    {
        ValidateOperationId(operationId);
        ValidatePositiveAmount(amount);
        if (fromAccountId == toAccountId)
            throw new ArgumentException("A transfer must use two different accounts.", nameof(toAccountId));

        lock (_sync)
        {
            var fingerprint = new OperationFingerprint(
                SimulationLedgerOperationKind.Transfer, fromAccountId, toAccountId, amount);
            if (TryReplay(operationId, fingerprint, out var replay))
                return replay;

            var from = GetAccount(fromAccountId);
            var to = GetAccount(toAccountId);
            EnsureCurrency(from, amount);
            EnsureCurrency(to, amount);

            var balance = GetBalanceCore(fromAccountId);
            if (balance < amount.Value)
                throw new InvalidOperationException("The source account has insufficient simulated funds.");

            var receipt = new SimulationLedgerReceipt(
                operationId, SimulationLedgerOperationKind.Transfer,
                fromAccountId, toAccountId, amount, occurredAt);

            Commit(receipt, fingerprint,
                new SimulationLedgerEntry(operationId, fromAccountId, amount.CurrencyId, -amount.Value,
                    SimulationLedgerOperationKind.Transfer, occurredAt),
                new SimulationLedgerEntry(operationId, toAccountId, amount.CurrencyId, amount.Value,
                    SimulationLedgerOperationKind.Transfer, occurredAt));
            return receipt;
        }
    }

    /// <summary>
    /// Applies a balanced transfer across two or more accounts as one atomic ledger operation.
    /// Every account must use the same currency; positive and negative postings must sum to zero.
    /// </summary>
    public SimulationLedgerReceipt Post(
        Guid operationId,
        IReadOnlyCollection<SimulationLedgerPosting> postings,
        DateTimeOffset occurredAt)
    {
        ValidateOperationId(operationId);
        ArgumentNullException.ThrowIfNull(postings);

        var requestedPostings = postings.ToArray();
        if (requestedPostings.Length < 2)
            throw new ArgumentException("A balanced transfer requires at least two postings.", nameof(postings));
        if (requestedPostings.Any(posting => posting.AccountId == Guid.Empty))
            throw new ArgumentException("Every posting must identify an account.", nameof(postings));
        if (requestedPostings.Any(posting => posting.CurrencyId.Value == Guid.Empty))
            throw new ArgumentException("Every posting must identify a currency.", nameof(postings));
        if (requestedPostings.Any(posting => posting.Delta == 0m))
            throw new ArgumentException("A posting must move a non-zero amount.", nameof(postings));
        if (requestedPostings.Select(posting => posting.AccountId).Distinct().Count() != requestedPostings.Length)
            throw new ArgumentException("An account may appear only once in a posting set.", nameof(postings));

        var orderedPostings = requestedPostings.OrderBy(posting => posting.AccountId).ToArray();
        var currencyId = orderedPostings[0].CurrencyId;
        if (orderedPostings.Any(posting => posting.CurrencyId != currencyId))
            throw new InvalidOperationException("All postings in a transfer must use the same currency.");

        var postingFingerprint = string.Join("|", orderedPostings.Select(posting =>
            $"{posting.AccountId:N}:{posting.CurrencyId.Value:N}:{posting.Delta.ToString("G29", CultureInfo.InvariantCulture)}"));

        lock (_sync)
        {
            var fingerprint = new OperationFingerprint(
                SimulationLedgerOperationKind.Transfer, null, null, default, postingFingerprint);
            if (TryReplay(operationId, fingerprint, out var replay))
                return replay;

            decimal total = 0m;
            decimal totalCredits = 0m;
            var nextBalances = new Dictionary<Guid, decimal>();
            foreach (var posting in orderedPostings)
            {
                var account = GetAccount(posting.AccountId);
                if (account.Currency.Id != posting.CurrencyId)
                    throw new InvalidOperationException("A posting currency does not match its account currency.");
                if (decimal.Round(posting.Delta, account.Currency.DecimalPlaces) != posting.Delta)
                    throw new ArgumentOutOfRangeException(nameof(postings),
                        $"A posting exceeds the currency precision of {account.Currency.DecimalPlaces} decimal places.");

                total = checked(total + posting.Delta);
                if (posting.Delta > 0m)
                    totalCredits = checked(totalCredits + posting.Delta);

                var nextBalance = checked(GetBalanceCore(posting.AccountId) + posting.Delta);
                if (nextBalance < 0m)
                    throw new InvalidOperationException("A ledger operation cannot create a negative account balance.");
                nextBalances.Add(posting.AccountId, nextBalance);
            }

            if (total != 0m)
                throw new InvalidOperationException("Ordinary transfer postings must balance to zero.");

            var receiptPostings = Array.AsReadOnly(orderedPostings);
            var entries = orderedPostings.Select(posting =>
                new SimulationLedgerEntry(operationId, posting.AccountId, posting.CurrencyId,
                    posting.Delta, SimulationLedgerOperationKind.Transfer, occurredAt)).ToArray();
            var receipt = new SimulationLedgerReceipt(
                operationId, SimulationLedgerOperationKind.Transfer, null, null,
                new SimulatedAmount(totalCredits, currencyId), occurredAt,
                Postings: receiptPostings);

            // Commit only after every posting, resulting balance, and invariant has passed validation.
            _fingerprints.Add(operationId, fingerprint);
            _receipts.Add(operationId, receipt);
            _entries.AddRange(entries);
            foreach (var balance in nextBalances)
                _balances[balance.Key] = balance.Value;

            return receipt;
        }
    }

    /// <summary>Removes value explicitly; retirement is recorded as a one-sided debit.</summary>
    public SimulationLedgerReceipt Retire(
        Guid operationId,
        Guid accountId,
        SimulatedAmount amount,
        DateTimeOffset occurredAt)
    {
        ValidateOperationId(operationId);
        ValidatePositiveAmount(amount);

        lock (_sync)
        {
            var fingerprint = new OperationFingerprint(
                SimulationLedgerOperationKind.Retirement, accountId, null, amount);
            if (TryReplay(operationId, fingerprint, out var replay))
                return replay;

            var account = GetAccount(accountId);
            EnsureCurrency(account, amount);

            var balance = GetBalanceCore(accountId);
            if (balance < amount.Value)
                throw new InvalidOperationException("The account has insufficient simulated funds to retire.");

            var receipt = new SimulationLedgerReceipt(
                operationId, SimulationLedgerOperationKind.Retirement,
                accountId, null, amount, occurredAt);

            Commit(receipt, fingerprint,
                new SimulationLedgerEntry(operationId, accountId, amount.CurrencyId, -amount.Value,
                    SimulationLedgerOperationKind.Retirement, occurredAt));
            return receipt;
        }
    }

    /// <summary>Returns the balance derived from the account's journal postings.</summary>
    public decimal GetBalance(Guid accountId)
    {
        lock (_sync)
        {
            _ = GetAccount(accountId);
            return GetBalanceCore(accountId);
        }
    }

    /// <summary>Returns a stable snapshot of the journal entries for one account.</summary>
    public IReadOnlyList<SimulationLedgerEntry> GetEntries(Guid accountId)
    {
        lock (_sync)
        {
            _ = GetAccount(accountId);
            return Array.AsReadOnly(_entries.Where(entry => entry.AccountId == accountId).ToArray());
        }
    }

    /// <summary>Returns the original receipt for an operation, if it has been applied.</summary>
    public SimulationLedgerReceipt? GetReceipt(Guid operationId)
    {
        lock (_sync)
            return _receipts.GetValueOrDefault(operationId);
    }

    private static void ValidateOperationId(Guid operationId)
    {
        if (operationId == Guid.Empty)
            throw new ArgumentException("An operation identifier is required.", nameof(operationId));
    }

    private static void ValidatePositiveAmount(SimulatedAmount amount)
    {
        if (amount.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "A ledger operation must move a positive amount.");
        if (amount.CurrencyId.Value == Guid.Empty)
            throw new ArgumentException("A simulated currency identifier is required.", nameof(amount));
    }

    private SimulationAccount GetAccount(Guid accountId)
    {
        if (!_accounts.TryGetValue(accountId, out var account))
            throw new KeyNotFoundException($"Account {accountId} is not registered.");
        return account;
    }

    private static void EnsureCurrency(SimulationAccount account, SimulatedAmount amount)
    {
        if (account.Currency.Id != amount.CurrencyId)
            throw new InvalidOperationException("The amount currency does not match the account currency.");
        if (decimal.Round(amount.Value, account.Currency.DecimalPlaces) != amount.Value)
            throw new ArgumentOutOfRangeException(nameof(amount),
                $"The amount exceeds the currency precision of {account.Currency.DecimalPlaces} decimal places.");
    }

    private decimal GetBalanceCore(Guid accountId) => _balances[accountId];

    private bool TryReplay(
        Guid operationId,
        OperationFingerprint fingerprint,
        out SimulationLedgerReceipt receipt)
    {
        if (!_fingerprints.TryGetValue(operationId, out var existing))
        {
            receipt = null!;
            return false;
        }

        if (existing != fingerprint)
            throw new InvalidOperationException(
                $"Operation identifier {operationId} was already used for a different ledger request.");

        receipt = _receipts[operationId] with { IsReplay = true };
        return true;
    }

    private void Commit(
        SimulationLedgerReceipt receipt,
        OperationFingerprint fingerprint,
        params SimulationLedgerEntry[] entries)
    {
        var nextBalances = new Dictionary<Guid, decimal>();
        foreach (var group in entries.GroupBy(entry => entry.AccountId))
        {
            var delta = group.Sum(entry => entry.Delta);
            var nextBalance = checked(GetBalanceCore(group.Key) + delta);
            if (nextBalance < 0m)
                throw new InvalidOperationException("A ledger operation cannot create a negative account balance.");
            nextBalances.Add(group.Key, nextBalance);
        }

        _fingerprints.Add(receipt.OperationId, fingerprint);
        _receipts.Add(receipt.OperationId, receipt);
        _entries.AddRange(entries);
        foreach (var balance in nextBalances)
            _balances[balance.Key] = balance.Value;
    }

    private readonly record struct OperationFingerprint(
        SimulationLedgerOperationKind Kind,
        Guid? FromAccountId,
        Guid? ToAccountId,
        SimulatedAmount Amount,
        string PostingFingerprint = "");
}
