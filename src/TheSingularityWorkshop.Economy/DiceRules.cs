namespace TheSingularityWorkshop.Economy;

/// <summary>Identifies a pure interpretation applied to an already-resolved dice roll.</summary>
public enum DiceRuleKind
{
    Sum,
    Highest,
    Doubles,
    AtLeastTotal
}

/// <summary>Describes a named, deterministic rule without generating random outcomes.</summary>
public sealed record DiceRuleDefinition
{
    private DiceRuleDefinition(DiceRuleKind kind, int? targetTotal = null)
    {
        Kind = kind;
        TargetTotal = targetTotal;
    }

    /// <summary>The interpretation to apply.</summary>
    public DiceRuleKind Kind { get; }

    /// <summary>Required total for <see cref="DiceRuleKind.AtLeastTotal"/>; otherwise null.</summary>
    public int? TargetTotal { get; }

    /// <summary>Returns the sum of all resolved faces.</summary>
    public static DiceRuleDefinition Sum { get; } = new(DiceRuleKind.Sum);

    /// <summary>Returns the highest resolved face.</summary>
    public static DiceRuleDefinition Highest { get; } = new(DiceRuleKind.Highest);

    /// <summary>Checks whether exactly two dice show the same face.</summary>
    public static DiceRuleDefinition Doubles { get; } = new(DiceRuleKind.Doubles);

    /// <summary>Checks whether the sum of all resolved faces reaches a threshold.</summary>
    public static DiceRuleDefinition AtLeastTotal(int targetTotal)
    {
        if (targetTotal < 1)
            throw new ArgumentOutOfRangeException(nameof(targetTotal), "The target total must be positive.");

        return new DiceRuleDefinition(DiceRuleKind.AtLeastTotal, targetTotal);
    }
}

/// <summary>The deterministic output of evaluating one rule against one authoritative roll.</summary>
/// <param name="Rule">The rule that was evaluated.</param>
/// <param name="Value">The numeric value produced by the rule (or total for threshold rules).</param>
/// <param name="IsSatisfied">Whether a predicate rule matched; null for value-only rules.</param>
public sealed record DiceRuleEvaluation(
    DiceRuleDefinition Rule,
    int Value,
    bool? IsSatisfied);

/// <summary>Evaluates dice rules without generating outcomes or performing economic settlement.</summary>
public static class DiceRuleEvaluator
{
    /// <summary>Applies a pure rule to an already-resolved roll.</summary>
    public static DiceRuleEvaluation Evaluate(DiceRoll roll, DiceRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(roll);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(roll.Faces);

        if (roll.FaceCount < 2)
            throw new ArgumentException("The roll must describe a die with at least two faces.", nameof(roll));
        if (roll.Faces.Count == 0)
            throw new ArgumentException("The roll must contain at least one resolved face.", nameof(roll));
        if (roll.Faces.Any(face => face < 1 || face > roll.FaceCount))
            throw new ArgumentException("The roll contains a face outside its die definition.", nameof(roll));

        var total = roll.Faces.Sum();
        return rule.Kind switch
        {
            DiceRuleKind.Sum => new DiceRuleEvaluation(rule, total, null),
            DiceRuleKind.Highest => new DiceRuleEvaluation(rule, roll.Faces.Max(), null),
            DiceRuleKind.Doubles when roll.Faces.Count == 2 =>
                new DiceRuleEvaluation(rule, total, roll.Faces[0] == roll.Faces[1]),
            DiceRuleKind.Doubles =>
                throw new ArgumentException("The doubles rule requires exactly two dice.", nameof(roll)),
            DiceRuleKind.AtLeastTotal when rule.TargetTotal is > 0 =>
                new DiceRuleEvaluation(rule, total, total >= rule.TargetTotal.Value),
            DiceRuleKind.AtLeastTotal =>
                throw new ArgumentException("The threshold rule requires a positive target total.", nameof(rule)),
            _ => throw new ArgumentOutOfRangeException(nameof(rule), "The rule kind is not supported.")
        };
    }
}
