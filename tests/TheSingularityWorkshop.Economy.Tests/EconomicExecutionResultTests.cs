using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class EconomicExecutionResultTests
{
    [Fact]
    public void ResultLinksAnObservedOutcomeToAnOperationAndTransferIntent()
    {
        var operationId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var observedAt = new DateTimeOffset(2026, 10, 14, 4, 0, 0, TimeSpan.Zero);

        var result = new EconomicExecutionResult(
            operationId,
            transferId,
            EconomicExecutionEnvironment.Simulation,
            EconomicExecutionStatus.Completed,
            observedAt);

        Assert.Equal(operationId, result.OperationId);
        Assert.Equal(transferId, result.TransferId);
        Assert.Equal(EconomicExecutionEnvironment.Simulation, result.Environment);
        Assert.Equal(EconomicExecutionStatus.Completed, result.Status);
        Assert.Equal(observedAt, result.ObservedAt);
        Assert.Null(result.ExternalReference);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResultRequiresNonEmptyIdentifiers(bool operationIdIsEmpty)
    {
        var operationId = operationIdIsEmpty ? Guid.Empty : Guid.NewGuid();
        var transferId = operationIdIsEmpty ? Guid.NewGuid() : Guid.Empty;

        Assert.Throws<ArgumentException>(() => new EconomicExecutionResult(
            operationId,
            transferId,
            EconomicExecutionEnvironment.Simulation,
            EconomicExecutionStatus.Pending,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ResultRejectsUndefinedEnvironmentAndStatusValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EconomicExecutionResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            (EconomicExecutionEnvironment)999,
            EconomicExecutionStatus.Pending,
            DateTimeOffset.UtcNow));

        Assert.Throws<ArgumentOutOfRangeException>(() => new EconomicExecutionResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EconomicExecutionEnvironment.Simulation,
            (EconomicExecutionStatus)999,
            DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData("  provider-operation-17  ", "provider-operation-17")]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void ExternalReferenceIsTrimmedAndBlankValuesBecomeNull(string? supplied, string? expected)
    {
        var result = new EconomicExecutionResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EconomicExecutionEnvironment.ExternalProvider,
            EconomicExecutionStatus.Unknown,
            DateTimeOffset.UtcNow,
            supplied);

        Assert.Equal(expected, result.ExternalReference);
    }

    [Fact]
    public void PendingAndUnknownAreDistinctFromCompleted()
    {
        var operationId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        var pending = new EconomicExecutionResult(
            operationId, transferId, EconomicExecutionEnvironment.ExternalProvider,
            EconomicExecutionStatus.Pending, observedAt);
        var unknown = new EconomicExecutionResult(
            operationId, transferId, EconomicExecutionEnvironment.ExternalProvider,
            EconomicExecutionStatus.Unknown, observedAt);
        var completed = new EconomicExecutionResult(
            operationId, transferId, EconomicExecutionEnvironment.ExternalProvider,
            EconomicExecutionStatus.Completed, observedAt);

        Assert.NotEqual(pending.Status, completed.Status);
        Assert.NotEqual(unknown.Status, completed.Status);
        Assert.Equal(operationId, unknown.OperationId);
        Assert.Equal(transferId, unknown.TransferId);
    }
}
