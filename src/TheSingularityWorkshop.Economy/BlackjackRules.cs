namespace TheSingularityWorkshop.Economy;

/// <summary>The four suits in a standard 52-card blackjack deck.</summary>
public enum BlackjackSuit { Clubs, Diamonds, Hearts, Spades }

/// <summary>Rank in a standard deck; Ace is low in identity and may count as 1 or 11.</summary>
public enum BlackjackRank
{
    Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
}

/// <summary>A physical card identity in a standard 52-card deck.</summary>
public readonly record struct BlackjackCard(BlackjackRank Rank, BlackjackSuit Suit)
{
    public int HardValue => Rank switch
    {
        BlackjackRank.Ace => 1,
        BlackjackRank.Jack or BlackjackRank.Queen or BlackjackRank.King => 10,
        _ => (int)Rank
    };
}

/// <summary>Computed total and natural/bust classification for a blackjack hand.</summary>
public sealed record BlackjackHandValue(int Total, bool IsSoft, bool IsBust, bool IsNatural);

/// <summary>Pure hand evaluation and settlement decision for deterministic blackjack simulations.</summary>
/// <remarks>
/// This rules slice evaluates fully supplied hands. It does not shuffle, draw, choose player actions,
/// or access the ledger. The caller supplies card order and controls the round lifecycle.
/// Wins pay even money (gross payout is twice the stake); natural-blackjack bonus payouts are not
/// part of this initial rules contract.
/// </remarks>
public static class BlackjackRules
{
    /// <summary>Calculates the best non-busting total, counting at most one ace as eleven.</summary>
    public static BlackjackHandValue EvaluateHand(IReadOnlyList<BlackjackCard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        if (cards.Count == 0)
            throw new ArgumentException("A blackjack hand must contain at least one card.", nameof(cards));
        ValidateCards(cards);

        var total = cards.Sum(card => card.HardValue);
        var aces = cards.Count(card => card.Rank == BlackjackRank.Ace);
        var soft = aces > 0 && total + 10 <= 21;
        if (soft)
            total += 10;

        return new BlackjackHandValue(
            total,
            soft,
            total > 21,
            cards.Count == 2 && total == 21);
    }

    /// <summary>Compares completed player and dealer hands and returns an accounting-ready decision.</summary>
    public static BlackjackDecision Resolve(
        IReadOnlyList<BlackjackCard> playerCards,
        IReadOnlyList<BlackjackCard> dealerCards,
        SimulatedAmount stake)
    {
        ArgumentNullException.ThrowIfNull(playerCards);
        ArgumentNullException.ThrowIfNull(dealerCards);
        if (stake.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(stake), "A blackjack stake must be positive.");

        if (playerCards.Count < 2 || dealerCards.Count < 2)
            throw new ArgumentException("A completed blackjack round requires at least two cards per hand.");

        var combined = playerCards.Concat(dealerCards).ToArray();
        ValidateCards(combined);
        var player = EvaluateHand(playerCards);
        var dealer = EvaluateHand(dealerCards);

        CasinoWagerOutcome outcome;
        if (player.IsBust)
            outcome = CasinoWagerOutcome.Loss;
        else if (dealer.IsBust)
            outcome = CasinoWagerOutcome.Win;
        else if (player.IsNatural && !dealer.IsNatural)
            outcome = CasinoWagerOutcome.Win;
        else if (dealer.IsNatural && !player.IsNatural)
            outcome = CasinoWagerOutcome.Loss;
        else if (player.Total > dealer.Total)
            outcome = CasinoWagerOutcome.Win;
        else if (player.Total < dealer.Total)
            outcome = CasinoWagerOutcome.Loss;
        else
            outcome = CasinoWagerOutcome.Push;

        var gross = outcome switch
        {
            CasinoWagerOutcome.Loss => 0m,
            CasinoWagerOutcome.Push => stake.Value,
            _ => checked(stake.Value * 2m)
        };

        return new BlackjackDecision(
            player, dealer, outcome, new SimulatedAmount(gross, stake.CurrencyId));
    }

    private static void ValidateCards(IReadOnlyList<BlackjackCard> cards)
    {
        var seen = new HashSet<BlackjackCard>();
        foreach (var card in cards)
        {
            if (!Enum.IsDefined(card.Rank) || !Enum.IsDefined(card.Suit))
                throw new ArgumentOutOfRangeException(nameof(cards), "Every card must have a standard rank and suit.");
            if (!seen.Add(card))
                throw new ArgumentException("A physical card cannot appear more than once in a round.", nameof(cards));
        }
    }
}

/// <summary>Deterministic blackjack comparison result suitable for explicit wager settlement.</summary>
public sealed record BlackjackDecision(
    BlackjackHandValue Player,
    BlackjackHandValue Dealer,
    CasinoWagerOutcome Outcome,
    SimulatedAmount GrossPayout);
