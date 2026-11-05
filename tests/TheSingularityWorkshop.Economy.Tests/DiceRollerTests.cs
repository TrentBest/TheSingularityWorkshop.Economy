using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class DiceRollerTests
{
    [Fact]
    public void StandardDieHasSixFaces()
    {
        Assert.Equal(6, DiceDefinition.SixSided.FaceCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void DieRejectsFewerThanTwoFaces(int faceCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiceDefinition(faceCount));
    }

    [Fact]
    public void RollUsesInjectedRandomSourceAndClock()
    {
        var now = new DateTimeOffset(2026, 11, 5, 2, 0, 0, TimeSpan.Zero);
        var random = new ScriptedDiceRandomSource(1, 6, 3);
        var roller = new DiceRoller(random, () => now);

        var roll = roller.Roll(DiceDefinition.SixSided, 3);

        Assert.NotEqual(Guid.Empty, roll.RollId);
        Assert.Equal(now, roll.RolledAt);
        Assert.Equal(new[] { 1, 6, 3 }, roll.Faces);
        Assert.Equal(10, roll.Total);
        Assert.Equal(3, random.Calls);
    }

    [Fact]
    public void SingleDieCanBeRolledWithoutAnyWagerOrLedger()
    {
        var roller = new DiceRoller(new ScriptedDiceRandomSource(4));

        var roll = roller.Roll(DiceDefinition.SixSided);

        Assert.Equal(new[] { 4 }, roll.Faces);
        Assert.Equal(4, roll.Total);
    }

    [Fact]
    public void RollRejectsInvalidQuantity()
    {
        var roller = new DiceRoller(new ScriptedDiceRandomSource(1));

        Assert.Throws<ArgumentOutOfRangeException>(() => roller.Roll(DiceDefinition.SixSided, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => roller.Roll(DiceDefinition.SixSided, 101));
    }

    [Fact]
    public void RollRejectsRandomSourceResultsOutsideDieRange()
    {
        var roller = new DiceRoller(new ScriptedDiceRandomSource(7));

        Assert.Throws<InvalidOperationException>(() => roller.Roll(DiceDefinition.SixSided));
    }

    private sealed class ScriptedDiceRandomSource(params int[] outcomes) : IDiceRandomSource
    {
        private int _index;
        public int Calls => _index;

        public int Next(int minimumInclusive, int maximumExclusive)
        {
            if (_index >= outcomes.Length)
                throw new InvalidOperationException("No scripted outcome remains.");

            var value = outcomes[_index++];
            Assert.Equal(1, minimumInclusive);
            Assert.Equal(7, maximumExclusive);
            return value;
        }
    }
}
