using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class DiceRuleEvaluatorTests
{
    [Fact]
    public void SumRuleReturnsTotalWithoutPredicate()
    {
        var result = DiceRuleEvaluator.Evaluate(Roll(2, 5, 6), DiceRuleDefinition.Sum);

        Assert.Equal(13, result.Value);
        Assert.Null(result.IsSatisfied);
    }

    [Fact]
    public void HighestRuleReturnsHighestFace()
    {
        var result = DiceRuleEvaluator.Evaluate(Roll(2, 5, 6), DiceRuleDefinition.Highest);

        Assert.Equal(6, result.Value);
        Assert.Null(result.IsSatisfied);
    }

    [Theory]
    [InlineData(4, 4, true)]
    [InlineData(4, 5, false)]
    public void DoublesRuleRequiresAndEvaluatesTwoMatchingDice(int first, int second, bool expected)
    {
        var result = DiceRuleEvaluator.Evaluate(Roll(6, first, second), DiceRuleDefinition.Doubles);

        Assert.Equal(expected, result.IsSatisfied);
    }

    [Fact]
    public void DoublesRuleRejectsAnythingOtherThanTwoDice()
    {
        Assert.Throws<ArgumentException>(() =>
            DiceRuleEvaluator.Evaluate(Roll(1, 2, 3), DiceRuleDefinition.Doubles));
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(11, false)]
    public void ThresholdRuleComparesTheTotalToItsTarget(int target, bool expected)
    {
        var result = DiceRuleEvaluator.Evaluate(Roll(4, 6), DiceRuleDefinition.AtLeastTotal(target));

        Assert.Equal(10, result.Value);
        Assert.Equal(expected, result.IsSatisfied);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThresholdRuleRejectsNonPositiveTargets(int target)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DiceRuleDefinition.AtLeastTotal(target));
    }

    [Fact]
    public void EvaluatorRejectsFacesOutsideTheDeclaredDie()
    {
        Assert.Throws<ArgumentException>(() =>
            DiceRuleEvaluator.Evaluate(Roll(1, 7), DiceRuleDefinition.Sum));
    }

    private static DiceRoll Roll(int faceCount, params int[] faces) =>
        new(Guid.NewGuid(), DateTimeOffset.UnixEpoch, faceCount, Array.AsReadOnly(faces));
}
