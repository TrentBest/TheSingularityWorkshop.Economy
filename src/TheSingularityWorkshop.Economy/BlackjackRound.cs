namespace TheSingularityWorkshop.Economy;

/// <summary>An explicit, ordered sequence of cards. No shuffle or random source is used.</summary>
/// <remarks>The shoe is consumed in the supplied order and is process-local mutable state.</remarks>
public sealed class BlackjackShoe
{
    private readonly object _sync = new();
    private readonly BlackjackCard[] _cards;
    private int _nextIndex;

    public BlackjackShoe(IEnumerable<BlackjackCard> orderedCards)
    {
        ArgumentNullException.ThrowIfNull(orderedCards);
        _cards = orderedCards.ToArray();
        if (_cards.Length == 0)
            throw new ArgumentException("A shoe must contain at least one card.", nameof(orderedCards));
        _ = BlackjackRules.EvaluateHand(new[] { _cards[0] });
        if (_cards.Distinct().Count() != _cards.Length)
            throw new ArgumentException("A shoe cannot contain duplicate physical cards.", nameof(orderedCards));
    }

    public int Remaining
    {
        get { lock (_sync) return _cards.Length - _nextIndex; }
    }

    internal BlackjackCard Draw()
    {
        lock (_sync)
        {
            if (_nextIndex >= _cards.Length)
                throw new InvalidOperationException("The blackjack shoe is exhausted.");
            return _cards[_nextIndex++];
        }
    }
}

/// <summary>Lifecycle states for one deterministic blackjack round.</summary>
public enum BlackjackRoundState
{
    PlayerTurn,
    DealerTurn,
    Completed
}

/// <summary>
/// Deterministic single-player blackjack round using a caller-supplied ordered shoe.
/// Dealer policy is hit soft 17 (H17). This engine never accesses a ledger.
/// </summary>
/// <remarks>
/// Deal order is player, dealer, player, dealer. A natural on either side ends the round
/// immediately; otherwise the player may Hit or Stand. Dealer play occurs after Stand,
/// drawing while below 17 or holding a soft 17. A player bust ends immediately.
/// </remarks>
public sealed class BlackjackRound
{
    private readonly object _sync = new();
    private readonly BlackjackShoe _shoe;
    private readonly List<BlackjackCard> _playerCards;
    private readonly List<BlackjackCard> _dealerCards;

    private BlackjackRound(BlackjackShoe shoe, SimulatedAmount stake,
        BlackjackCard[] playerCards, BlackjackCard[] dealerCards)
    {
        _shoe = shoe;
        Stake = stake;
        _playerCards = playerCards.ToList();
        _dealerCards = dealerCards.ToList();
        State = BlackjackRoundState.PlayerTurn;
        CompleteIfNatural();
    }

    public SimulatedAmount Stake { get; }
    public BlackjackRoundState State { get; private set; }
    public BlackjackDecision? Decision { get; private set; }

    public IReadOnlyList<BlackjackCard> PlayerCards
    {
        get { lock (_sync) return Array.AsReadOnly(_playerCards.ToArray()); }
    }

    public IReadOnlyList<BlackjackCard> DealerCards
    {
        get { lock (_sync) return Array.AsReadOnly(_dealerCards.ToArray()); }
    }

    public static BlackjackRound Start(BlackjackShoe shoe, SimulatedAmount stake)
    {
        ArgumentNullException.ThrowIfNull(shoe);
        if (stake.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(stake), "A blackjack stake must be positive.");
        if (shoe.Remaining < 4)
            throw new InvalidOperationException("At least four cards are required to deal a blackjack round.");

        var playerFirst = shoe.Draw();
        var dealerFirst = shoe.Draw();
        var playerSecond = shoe.Draw();
        var dealerSecond = shoe.Draw();
        return new BlackjackRound(shoe, stake,
            new[] { playerFirst, playerSecond }, new[] { dealerFirst, dealerSecond });
    }

    /// <summary>Draws one card during the player's turn; a bust completes as a loss.</summary>
    public void Hit()
    {
        lock (_sync)
        {
            EnsureState(BlackjackRoundState.PlayerTurn);
            _playerCards.Add(_shoe.Draw());
            if (BlackjackRules.EvaluateHand(_playerCards).IsBust)
                Complete();
        }
    }

    /// <summary>Ends the player's turn and runs the dealer under the H17 policy.</summary>
    public void Stand()
    {
        lock (_sync)
        {
            if (State == BlackjackRoundState.DealerTurn)
            {
                PlayDealer();
                return;
            }

            EnsureState(BlackjackRoundState.PlayerTurn);
            State = BlackjackRoundState.DealerTurn;
            PlayDealer();
        }
    }

    private void CompleteIfNatural()
    {
        var player = BlackjackRules.EvaluateHand(_playerCards);
        var dealer = BlackjackRules.EvaluateHand(_dealerCards);
        if (player.IsNatural || dealer.IsNatural)
            Complete();
    }

    private void PlayDealer()
    {
        while (true)
        {
            var value = BlackjackRules.EvaluateHand(_dealerCards);
            if (value.Total > 17 || (value.Total == 17 && !value.IsSoft))
            {
                Complete();
                return;
            }

            // If the shoe runs out, preserve DealerTurn and the already drawn cards.
            // The caller may provide no new cards; a fresh shoe is intentionally not implicit.
            _dealerCards.Add(_shoe.Draw());
            if (BlackjackRules.EvaluateHand(_dealerCards).IsBust)
            {
                Complete();
                return;
            }
        }
    }

    private void Complete()
    {
        Decision = BlackjackRules.Resolve(_playerCards, _dealerCards, Stake);
        State = BlackjackRoundState.Completed;
    }

    private void EnsureState(BlackjackRoundState expected)
    {
        if (State != expected)
            throw new InvalidOperationException($"This action requires state {expected}; current state is {State}.");
    }
}
