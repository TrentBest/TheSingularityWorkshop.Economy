namespace TheSingularityWorkshop.Economy;

/// <summary>
/// Coordinates one deterministic blackjack round with the exact wager placed for that round.
/// </summary>
/// <remarks>
/// This is a process-local host-session boundary, not durable session storage. The caller
/// creates the round and places its wager, then supplies both snapshots here. The constructor
/// verifies wager identity, unresolved state, and exact typed-stake equality. All subsequent
/// game actions, settlement, and payout claims must pass through this session instance.
/// The shoe must be dedicated to this round; sharing a mutable shoe between sessions is unsupported.
/// </remarks>
public sealed class BlackjackSession
{
    private readonly object _sync = new();
    private readonly BlackjackRound _round;
    private readonly CasinoWagerBook _wagerBook;
    private CasinoWager _wager;

    /// <summary>
    /// Opens a session by reserving the initial deal, placing its wager, binding both,
    /// and only then committing the four-card shoe reservation.
    /// </summary>
    /// <remarks>
    /// If wager placement fails, the reservation is released without consuming cards.
    /// This is an in-process staged protocol, not a durable transaction spanning the shoe
    /// and ledger. A process crash after value movement is not recoverable by this method.
    /// </remarks>
    public static BlackjackSession Open(
        Guid sessionId,
        BlackjackShoe shoe,
        CasinoWagerBook wagerBook,
        Guid placeOperationId,
        Guid wagerId,
        Guid playerChipAccountId,
        SimulatedAmount stake,
        DateTimeOffset occurredAt)
    {
        if (sessionId == Guid.Empty || placeOperationId == Guid.Empty ||
            wagerId == Guid.Empty || playerChipAccountId == Guid.Empty)
            throw new ArgumentException("Session, placement, wager, and player account identifiers are required.");
        ArgumentNullException.ThrowIfNull(shoe);
        ArgumentNullException.ThrowIfNull(wagerBook);
        if (stake.Value <= 0m || stake.CurrencyId.Value == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(stake), "A session stake must be positive and typed.");

        var reservation = shoe.ReserveInitialDeal();
        BlackjackRound round;
        try
        {
            round = BlackjackRound.StartReserved(shoe, reservation, stake);
        }
        catch
        {
            shoe.ReleaseReservation(reservation);
            throw;
        }

        CasinoWager wager;
        try
        {
            wager = wagerBook.PlaceWager(
                placeOperationId, wagerId, playerChipAccountId, stake, occurredAt);
        }
        catch
        {
            shoe.ReleaseReservation(reservation);
            throw;
        }

        // All constructor invariants are prevalidated by the arguments and PlaceWager result.
        // No generic compensating refund is attempted if an unexpected failure occurs here.
        var session = new BlackjackSession(sessionId, round, wager, wagerBook);
        shoe.CommitReservation(reservation);
        return session;
    }

    public BlackjackSession(
        Guid sessionId,
        BlackjackRound round,
        CasinoWager wager,
        CasinoWagerBook wagerBook)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("A session identifier is required.", nameof(sessionId));
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(wager);
        ArgumentNullException.ThrowIfNull(wagerBook);

        if (wager.WagerId == Guid.Empty || wager.PlaceOperationId == Guid.Empty)
            throw new ArgumentException("The supplied wager must have valid identity.", nameof(wager));
        if (wager.Stake != round.Stake)
            throw new ArgumentException("The round stake must exactly match the wager amount and currency.", nameof(wager));
        if (wager.Outcome is not null || wager.ResolutionOperationId is not null ||
            wager.ResolutionReceipt is not null || wager.IsPayoutClaimed)
            throw new InvalidOperationException("A session can only be created for an unresolved, unclaimed wager.");

        var current = wagerBook.GetWager(wager.WagerId);
        if (current is null ||
            current.PlaceOperationId != wager.PlaceOperationId ||
            current.PlayerChipAccountId != wager.PlayerChipAccountId ||
            current.Stake != wager.Stake ||
            current.Outcome is not null)
            throw new InvalidOperationException("The wager snapshot is not the current unresolved wager in the supplied book.");

        SessionId = sessionId;
        _round = round;
        _wagerBook = wagerBook;
        _wager = current;
    }

    /// <summary>Stable identity for this process-local host session.</summary>
    public Guid SessionId { get; }

    /// <summary>Current game lifecycle state.</summary>
    public BlackjackRoundState RoundState
    {
        get { lock (_sync) return _round.State; }
    }

    /// <summary>Cards currently held by the player in this session.</summary>
    public IReadOnlyList<BlackjackCard> PlayerCards
    {
        get { lock (_sync) return _round.PlayerCards; }
    }

    /// <summary>Cards currently held by the dealer in this session.</summary>
    public IReadOnlyList<BlackjackCard> DealerCards
    {
        get { lock (_sync) return _round.DealerCards; }
    }

    /// <summary>The completed decision, or null while the round is still in play.</summary>
    public BlackjackDecision? Decision
    {
        get { lock (_sync) return _round.Decision; }
    }

    /// <summary>The latest immutable economic wager snapshot.</summary>
    public CasinoWager Wager
    {
        get { lock (_sync) return _wager; }
    }

    /// <summary>Draws one card while the wager remains unresolved.</summary>
    public void Hit()
    {
        lock (_sync)
        {
            EnsureUnsettled();
            _round.Hit();
        }
    }

    /// <summary>Ends the player's turn and runs the dealer policy.</summary>
    public void Stand()
    {
        lock (_sync)
        {
            EnsureUnsettled();
            _round.Stand();
        }
    }

    /// <summary>
    /// Settles this session's wager using only the decision produced by its bound round.
    /// Retrying with the same operation ID replays the wager-book receipt.
    /// </summary>
    public CasinoWager Settle(Guid operationId, DateTimeOffset occurredAt)
    {
        lock (_sync)
        {
            var decision = _round.Decision ??
                throw new InvalidOperationException("A blackjack round must be completed before settlement.");
            if (_wager.Outcome is not null)
            {
                if (_wager.ResolutionOperationId != operationId)
                    throw new InvalidOperationException("This session has already been settled with another operation.");
                _wager = _wagerBook.ResolveWager(
                    operationId, _wager.WagerId, decision.Outcome, decision.GrossPayout, occurredAt);
                return _wager;
            }

            EnsureCurrentUnresolvedWager();
            if (decision.GrossPayout.CurrencyId != _wager.Stake.CurrencyId)
                throw new InvalidOperationException("The round decision currency does not match the bound wager.");
            _wager = _wagerBook.ResolveWager(
                operationId, _wager.WagerId, decision.Outcome, decision.GrossPayout, occurredAt);
            return _wager;
        }
    }

    /// <summary>Claims this session's positive resolved payout exactly once.</summary>
    public CasinoWager ClaimPayout(Guid operationId, DateTimeOffset occurredAt)
    {
        lock (_sync)
        {
            if (_wager.Outcome is null)
                throw new InvalidOperationException("The session must be settled before a payout can be claimed.");
            _wager = _wagerBook.ClaimPayout(operationId, _wager.WagerId, occurredAt);
            return _wager;
        }
    }

    private void EnsureUnsettled()
    {
        EnsureCurrentUnresolvedWager();
        if (_round.State == BlackjackRoundState.Completed)
            throw new InvalidOperationException("A completed blackjack round cannot accept further player actions.");
    }

    private void EnsureCurrentUnresolvedWager()
    {
        var current = _wagerBook.GetWager(_wager.WagerId);
        if (current is null ||
            current.PlaceOperationId != _wager.PlaceOperationId ||
            current.PlayerChipAccountId != _wager.PlayerChipAccountId ||
            current.Stake != _wager.Stake ||
            current.Outcome is not null)
            throw new InvalidOperationException("The bound wager is no longer the current unresolved wager.");
    }
}
