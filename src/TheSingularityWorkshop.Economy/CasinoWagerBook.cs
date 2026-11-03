namespace TheSingularityWorkshop.Economy;


/// <summary>Moves fictional currency between a participant wallet and their casino chip account.</summary>
/// <remarks>Buy-in and cash-out are one-for-one in-world transfers, never external deposits or withdrawals.</remarks>
public sealed class CasinoCashier
{
    private readonly SimulationLedger _ledger;

    public CasinoCashier(SimulationLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        _ledger = ledger;
    }

    public SimulationLedgerReceipt BuyIn(
        Guid operationId, Guid participantWalletAccountId, Guid playerChipAccountId,
        SimulatedAmount amount, DateTimeOffset occurredAt)
    {
        if (participantWalletAccountId == playerChipAccountId)
            throw new ArgumentException("Wallet and chip accounts must be distinct.");
        return _ledger.Transfer(operationId, participantWalletAccountId, playerChipAccountId, amount, occurredAt);
    }

    public SimulationLedgerReceipt CashOut(
        Guid operationId, Guid playerChipAccountId, Guid participantWalletAccountId,
        SimulatedAmount amount, DateTimeOffset occurredAt)
    {
        if (participantWalletAccountId == playerChipAccountId)
            throw new ArgumentException("Wallet and chip accounts must be distinct.");
        return _ledger.Transfer(operationId, playerChipAccountId, participantWalletAccountId, amount, occurredAt);
    }
}

/// <summary>Describes the economic result of a completed simulated casino wager.</summary>
public enum CasinoWagerOutcome
{
    Loss,
    Push,
    Win
}

/// <summary>
/// Process-local coordinator for casino wager custody and settlement.
/// All value movement is delegated to SimulationLedger; this class does not create value.
/// </summary>
/// <remarks>
/// The supplied accounts must use the same simulated currency. The house account is its
/// available settlement reserve, escrow holds unsettled stakes, and payouts-owed holds
/// resolved amounts until the player claims them. State is volatile and is not crash-safe.
/// </remarks>
public sealed class CasinoWagerBook
{
    private readonly object _sync = new();
    private readonly SimulationLedger _ledger;
    private readonly Guid _houseAccountId;
    private readonly Guid _escrowAccountId;
    private readonly Guid _payoutsOwedAccountId;
    private readonly Dictionary<Guid, CasinoWager> _wagers = new();

    public CasinoWagerBook(
        SimulationLedger ledger,
        Guid houseAccountId,
        Guid escrowAccountId,
        Guid payoutsOwedAccountId)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        if (houseAccountId == Guid.Empty || escrowAccountId == Guid.Empty ||
            payoutsOwedAccountId == Guid.Empty)
            throw new ArgumentException("Casino settlement account identifiers are required.");
        if (new[] { houseAccountId, escrowAccountId, payoutsOwedAccountId }.Distinct().Count() != 3)
            throw new ArgumentException("House, escrow, and payouts-owed accounts must be distinct.");

        _ledger = ledger;
        _houseAccountId = houseAccountId;
        _escrowAccountId = escrowAccountId;
        _payoutsOwedAccountId = payoutsOwedAccountId;

        // Fail during composition, not halfway through the first wager.
        _ = ledger.GetBalance(houseAccountId);
        _ = ledger.GetBalance(escrowAccountId);
        _ = ledger.GetBalance(payoutsOwedAccountId);
    }

    /// <summary>Moves a participant's chips into escrow and opens one wager exactly once.</summary>
    public CasinoWager PlaceWager(
        Guid operationId,
        Guid wagerId,
        Guid playerChipAccountId,
        SimulatedAmount stake,
        DateTimeOffset occurredAt)
    {
        if (operationId == Guid.Empty || wagerId == Guid.Empty || playerChipAccountId == Guid.Empty)
            throw new ArgumentException("Operation, wager, and player account identifiers are required.");
        if (stake.Value <= 0m || stake.CurrencyId.Value == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(stake), "A wager stake must be positive and typed.");
        if (playerChipAccountId == _houseAccountId ||
            playerChipAccountId == _escrowAccountId ||
            playerChipAccountId == _payoutsOwedAccountId)
            throw new ArgumentException("A participant account must be separate from casino settlement accounts.");

        lock (_sync)
        {
            if (_wagers.TryGetValue(wagerId, out var existing))
            {
                if (existing.PlaceOperationId != operationId ||
                    existing.PlayerChipAccountId != playerChipAccountId ||
                    existing.Stake != stake)
                    throw new InvalidOperationException("The wager identifier is already bound to a different request.");
                return existing with
                {
                    PlaceReceipt = existing.PlaceReceipt with { IsReplay = true }
                };
            }

            var receipt = _ledger.Transfer(
                operationId, playerChipAccountId, _escrowAccountId, stake, occurredAt);
            var wager = new CasinoWager(
                wagerId, playerChipAccountId, stake, operationId, receipt,
                null, null, null, null);
            _wagers.Add(wagerId, wager);
            return wager;
        }
    }

    /// <summary>
    /// Resolves an escrowed wager. Win payout is gross (stake returned plus winnings);
    /// a push returns exactly the stake. Winning value is funded by the house reserve.
    /// </summary>
    public CasinoWager ResolveWager(
        Guid operationId,
        Guid wagerId,
        CasinoWagerOutcome outcome,
        SimulatedAmount grossPayout,
        DateTimeOffset occurredAt)
    {
        if (operationId == Guid.Empty)
            throw new ArgumentException("A resolution operation identifier is required.", nameof(operationId));
        if (grossPayout.Value < 0m || grossPayout.CurrencyId.Value == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(grossPayout));

        lock (_sync)
        {
            var wager = RequireWager(wagerId);
            if (wager.ResolutionOperationId is not null)
            {
                if (wager.ResolutionOperationId != operationId ||
                    wager.Outcome != outcome || wager.GrossPayout != grossPayout)
                    throw new InvalidOperationException("The wager has already been resolved differently.");
                return wager with
                {
                    ResolutionReceipt = wager.ResolutionReceipt! with { IsReplay = true }
                };
            }

            if (grossPayout.CurrencyId != wager.Stake.CurrencyId)
                throw new InvalidOperationException("Payout currency must match the wager currency.");

            SimulationLedgerReceipt receipt;
            var postings = new List<SimulationLedgerPosting>();
            switch (outcome)
            {
                case CasinoWagerOutcome.Loss:
                    if (grossPayout.Value != 0m)
                        throw new ArgumentException("A losing wager has no payout.", nameof(grossPayout));
                    postings.Add(new(_escrowAccountId, wager.Stake.CurrencyId, -wager.Stake.Value));
                    postings.Add(new(_houseAccountId, wager.Stake.CurrencyId, wager.Stake.Value));
                    break;

                case CasinoWagerOutcome.Push:
                    if (grossPayout.Value != wager.Stake.Value)
                        throw new ArgumentException("A push returns exactly the original stake.", nameof(grossPayout));
                    postings.Add(new(_escrowAccountId, wager.Stake.CurrencyId, -wager.Stake.Value));
                    postings.Add(new(_payoutsOwedAccountId, wager.Stake.CurrencyId, grossPayout.Value));
                    break;

                case CasinoWagerOutcome.Win:
                    if (grossPayout.Value <= wager.Stake.Value)
                        throw new ArgumentException("A winning gross payout must exceed the original stake.", nameof(grossPayout));
                    var winnings = grossPayout.Value - wager.Stake.Value;
                    postings.Add(new(_escrowAccountId, wager.Stake.CurrencyId, -wager.Stake.Value));
                    postings.Add(new(_houseAccountId, wager.Stake.CurrencyId, -winnings));
                    postings.Add(new(_payoutsOwedAccountId, wager.Stake.CurrencyId, grossPayout.Value));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome));
            }

            receipt = _ledger.Post(operationId, postings, occurredAt);
            var resolved = wager with
            {
                Outcome = outcome,
                GrossPayout = grossPayout,
                ResolutionOperationId = operationId,
                ResolutionReceipt = receipt
            };
            _wagers[wagerId] = resolved;
            return resolved;
        }
    }

    /// <summary>Transfers a resolved payout from the shared payable account to its player once.</summary>
    public CasinoWager ClaimPayout(Guid operationId, Guid wagerId, DateTimeOffset occurredAt)
    {
        if (operationId == Guid.Empty)
            throw new ArgumentException("A payout operation identifier is required.", nameof(operationId));

        lock (_sync)
        {
            var wager = GetWager(wagerId);
            if (wager.ResolutionOperationId is null || wager.GrossPayout is null)
                throw new InvalidOperationException("An unresolved wager has no payable balance.");
            if (wager.GrossPayout.Value.Value == 0m)
                throw new InvalidOperationException("A losing wager has no payout to claim.");

            if (wager.ClaimOperationId is not null)
            {
                if (wager.ClaimOperationId != operationId)
                    throw new InvalidOperationException("The wager payout has already been claimed.");
                return wager with
                {
                    ClaimReceipt = wager.ClaimReceipt! with { IsReplay = true }
                };
            }

            var receipt = _ledger.Transfer(
                operationId,
                _payoutsOwedAccountId,
                wager.PlayerChipAccountId,
                wager.GrossPayout.Value,
                occurredAt);
            var claimed = wager with { ClaimOperationId = operationId, ClaimReceipt = receipt };
            _wagers[wagerId] = claimed;
            return claimed;
        }
    }

    /// <summary>Returns the current immutable wager snapshot, or null when it is unknown.</summary>
    public CasinoWager? GetWager(Guid wagerId)
    {
        lock (_sync)
            return _wagers.GetValueOrDefault(wagerId);
    }

    private CasinoWager RequireWager(Guid wagerId) =>
        _wagers.TryGetValue(wagerId, out var wager)
            ? wager
            : throw new KeyNotFoundException($"Wager {wagerId} does not exist.");
}

/// <summary>Immutable process-local snapshot of a wager and its settlement receipts.</summary>
public sealed record CasinoWager(
    Guid WagerId,
    Guid PlayerChipAccountId,
    SimulatedAmount Stake,
    Guid PlaceOperationId,
    SimulationLedgerReceipt PlaceReceipt,
    CasinoWagerOutcome? Outcome,
    SimulatedAmount? GrossPayout,
    Guid? ResolutionOperationId,
    SimulationLedgerReceipt? ResolutionReceipt,
    Guid? ClaimOperationId = null,
    SimulationLedgerReceipt? ClaimReceipt = null)
{
    /// <summary>True after the resolved amount has moved from payouts owed to player chips.</summary>
    public bool IsPayoutClaimed => ClaimOperationId.HasValue;
}
