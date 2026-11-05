namespace TheSingularityWorkshop.Economy;

/// <summary>Describes a die with a fixed number of equally likely numbered faces.</summary>
/// <remarks>This domain definition does not describe physical geometry or rendering.</remarks>
public sealed record DiceDefinition
{
    /// <summary>Creates a die whose faces are numbered from one through <paramref name="faceCount"/>.</summary>
    public DiceDefinition(int faceCount)
    {
        if (faceCount < 2)
            throw new ArgumentOutOfRangeException(nameof(faceCount), "A die must have at least two faces.");

        FaceCount = faceCount;
    }

    /// <summary>Number of equally likely faces on the die.</summary>
    public int FaceCount { get; }

    /// <summary>A standard six-sided die.</summary>
    public static DiceDefinition SixSided { get; } = new(6);
}

/// <summary>Supplies an integer outcome for a die roll.</summary>
/// <remarks>The upper bound is exclusive, matching <see cref="Random.Next(int, int)"/>.</remarks>
public interface IDiceRandomSource
{
    /// <summary>Returns a value in the inclusive-minimum, exclusive-maximum range.</summary>
    int Next(int minimumInclusive, int maximumExclusive);
}

/// <summary>Production random source backed by the platform random-number generator.</summary>
/// <remarks>This is not a cryptographic or externally audited randomness guarantee.</remarks>
public sealed class SystemDiceRandomSource : IDiceRandomSource
{
    /// <inheritdoc />
    public int Next(int minimumInclusive, int maximumExclusive) =>
        Random.Shared.Next(minimumInclusive, maximumExclusive);
}

/// <summary>An immutable authoritative result for a completed dice roll.</summary>
public sealed record DiceRoll(
    Guid RollId,
    DateTimeOffset RolledAt,
    int FaceCount,
    IReadOnlyList<int> Faces)
{
    /// <summary>Sum of the resolved face values.</summary>
    public int Total => Faces.Sum();
}

/// <summary>Generates authoritative dice outcomes independently of wagers and presentation.</summary>
public sealed class DiceRoller
{
    private readonly IDiceRandomSource _random;
    private readonly Func<DateTimeOffset> _utcNow;

    /// <summary>Creates a roller with injected randomness and an optional deterministic clock.</summary>
    public DiceRoller(IDiceRandomSource random, Func<DateTimeOffset>? utcNow = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    /// <summary>Resolves one or more dice without creating or requiring an economic wager.</summary>
    public DiceRoll Roll(DiceDefinition die, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(die);
        if (quantity < 1 || quantity > 100)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be between 1 and 100.");

        var faces = new int[quantity];
        for (var index = 0; index < faces.Length; index++)
        {
            var face = _random.Next(1, die.FaceCount + 1);
            if (face < 1 || face > die.FaceCount)
                throw new InvalidOperationException("The random source returned a face outside the die definition.");

            faces[index] = face;
        }

        return new DiceRoll(
            Guid.NewGuid(),
            _utcNow(),
            die.FaceCount,
            Array.AsReadOnly(faces));
    }
}
