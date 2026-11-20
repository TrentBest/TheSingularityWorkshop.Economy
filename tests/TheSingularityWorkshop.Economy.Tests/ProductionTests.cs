using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class ProductionTests
{
    private static readonly ProductionResourceId Ore = new("ore.iron");
    private static readonly ProductionResourceId Coal = new("fuel.coal");
    private static readonly ProductionResourceId Labor = new("capacity.labor-hour");
    private static readonly ProductionResourceId Sword = new("item.iron-sword");
    private static readonly ProductionResourceId Slag = new("waste.slag");

    private static ProductionRecipe SwordRecipe() => new(
        "blacksmith.iron-sword", "1.0",
        new[] { new ProductionResourceQuantity(Ore, 3m), new ProductionResourceQuantity(Coal, 2m) },
        new[] { new ProductionResourceQuantity(Labor, 1.5m) },
        new[] { new ProductionResourceQuantity(Sword, 1m) },
        new[] { new ProductionResourceQuantity(Slag, 0.5m) });

    private static ProductionInventory Inventory() => new(new[]
    {
        new ProductionResourceQuantity(Ore, 30m),
        new ProductionResourceQuantity(Coal, 20m),
        new ProductionResourceQuantity(Labor, 15m)
    });

    [Fact]
    public void RecipeSnapshotsAndExposesDeclaredInputsCapacityYieldAndWaste()
    {
        var inputs = new List<ProductionResourceQuantity> { new(Ore, 3m) };
        var recipe = new ProductionRecipe("mine.iron", "2", inputs,
            new[] { new ProductionResourceQuantity(Labor, 1m) },
            new[] { new ProductionResourceQuantity(Ore, 2m) },
            new[] { new ProductionResourceQuantity(Slag, 1m) });
        inputs.Clear();

        Assert.Single(recipe.Inputs);
        Assert.Equal(3m, recipe.Inputs[0].Quantity);
        Assert.Single(recipe.Capacity);
        Assert.Single(recipe.Outputs);
        Assert.Single(recipe.Waste);
    }

    [Fact]
    public void ExecutesBatchesAtomicallyAndRecordsYieldWasteAndProvenance()
    {
        var inventory = Inventory();
        var operationId = Guid.NewGuid();

        var receipt = inventory.Execute(operationId, SwordRecipe(), 2, DateTimeOffset.UnixEpoch);

        Assert.Equal(2, receipt.Batches);
        Assert.Equal(6m, receipt.Inputs.Single(item => item.ResourceId == Ore).Quantity);
        Assert.Equal(3m, receipt.Capacity.Single().Quantity);
        Assert.Equal(2m, receipt.Yield.Single().Quantity);
        Assert.Equal(1m, receipt.Waste.Single().Quantity);
        Assert.Equal(24m, inventory.GetBalance(Ore));
        Assert.Equal(16m, inventory.GetBalance(Coal));
        Assert.Equal(12m, inventory.GetBalance(Labor));
        Assert.Equal(2m, inventory.GetBalance(Sword));
        Assert.Equal(1m, inventory.GetBalance(Slag));
        Assert.Contains(receipt.Provenance, item =>
            item.ResourceId == Ore && item.Kind == ProductionProvenanceKind.Input && item.Quantity == 6m);
        Assert.Contains(receipt.Provenance, item =>
            item.ResourceId == Sword && item.Kind == ProductionProvenanceKind.Yield && item.Quantity == 2m);
        Assert.Contains(receipt.Provenance, item =>
            item.ResourceId == Slag && item.Kind == ProductionProvenanceKind.Waste && item.Quantity == 1m);
    }

    [Fact]
    public void RejectsInsufficientInputOrCapacityWithoutPartialMutation()
    {
        var inventory = new ProductionInventory(new[]
        {
            new ProductionResourceQuantity(Ore, 3m),
            new ProductionResourceQuantity(Coal, 2m),
            new ProductionResourceQuantity(Labor, 1m)
        });

        Assert.Throws<InvalidOperationException>(() =>
            inventory.Execute(Guid.NewGuid(), SwordRecipe(), 1, DateTimeOffset.UnixEpoch));

        Assert.Equal(3m, inventory.GetBalance(Ore));
        Assert.Equal(2m, inventory.GetBalance(Coal));
        Assert.Equal(1m, inventory.GetBalance(Labor));
        Assert.Equal(0m, inventory.GetBalance(Sword));
        Assert.Empty(inventory.History);
    }

    [Fact]
    public void ReplaysIdenticalOperationWithoutDuplicatingProduction()
    {
        var inventory = Inventory();
        var operationId = Guid.NewGuid();
        var recipe = SwordRecipe();

        var first = inventory.Execute(operationId, recipe, 2, DateTimeOffset.UnixEpoch);
        var replay = inventory.Execute(operationId, recipe, 2, DateTimeOffset.UnixEpoch.AddDays(1));

        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(first.OccurredAt, replay.OccurredAt);
        Assert.Equal(2m, inventory.GetBalance(Sword));
        Assert.Single(inventory.History);
    }

    [Fact]
    public void RejectsOperationIdReuseForDifferentRecipeOrBatchCount()
    {
        var inventory = Inventory();
        var operationId = Guid.NewGuid();
        inventory.Execute(operationId, SwordRecipe(), 1, DateTimeOffset.UnixEpoch);

        Assert.Throws<InvalidOperationException>(() =>
            inventory.Execute(operationId, SwordRecipe(), 2, DateTimeOffset.UnixEpoch));
        var changedRecipe = new ProductionRecipe("blacksmith.iron-sword", "1.0",
            new[] { new ProductionResourceQuantity(Ore, 4m), new ProductionResourceQuantity(Coal, 2m) },
            new[] { new ProductionResourceQuantity(Labor, 1.5m) },
            new[] { new ProductionResourceQuantity(Sword, 1m) });
        Assert.Throws<InvalidOperationException>(() =>
            inventory.Execute(operationId, changedRecipe, 1, DateTimeOffset.UnixEpoch));
    }

    [Fact]
    public void FailedRunDoesNotCreateReceiptOrMutateInventory()
    {
        var inventory = new ProductionInventory();
        var operationId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() =>
            inventory.Execute(operationId, SwordRecipe(), 1, DateTimeOffset.UnixEpoch));

        Assert.Empty(inventory.History);
        Assert.Equal(0m, inventory.GetBalance(Sword));
    }

    [Fact]
    public void RecipeRejectsMissingInputsOutputsAndDuplicateResourcesWithinACollection()
    {
        Assert.Throws<ArgumentException>(() => new ProductionRecipe("bad", "1",
            Array.Empty<ProductionResourceQuantity>(),
            Array.Empty<ProductionResourceQuantity>(),
            new[] { new ProductionResourceQuantity(Sword, 1m) }));
        Assert.Throws<ArgumentException>(() => new ProductionRecipe("bad", "1",
            new[] { new ProductionResourceQuantity(Ore, 1m) },
            Array.Empty<ProductionResourceQuantity>(),
            Array.Empty<ProductionResourceQuantity>()));
        Assert.Throws<ArgumentException>(() => new ProductionRecipe("bad", "1",
            new[] { new ProductionResourceQuantity(Ore, 1m), new ProductionResourceQuantity(Ore, 2m) },
            Array.Empty<ProductionResourceQuantity>(),
            new[] { new ProductionResourceQuantity(Sword, 1m) }));
    }

    [Fact]
    public void RejectsInvalidQuantitiesAndBatchCounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProductionResourceQuantity(Ore, 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProductionResourceQuantity(Ore, -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Inventory().Execute(Guid.NewGuid(), SwordRecipe(), 0, DateTimeOffset.UnixEpoch));
        Assert.Throws<ArgumentException>(() =>
            Inventory().Execute(Guid.Empty, SwordRecipe(), 1, DateTimeOffset.UnixEpoch));
    }

    [Fact]
    public void AllowsAResourceToBeConsumedAndProducedInOneRun()
    {
        var inventory = new ProductionInventory(new[]
        {
            new ProductionResourceQuantity(Ore, 10m),
            new ProductionResourceQuantity(Labor, 2m)
        });
        var refining = new ProductionRecipe("ore.refine", "1",
            new[] { new ProductionResourceQuantity(Ore, 5m) },
            new[] { new ProductionResourceQuantity(Labor, 1m) },
            new[] { new ProductionResourceQuantity(Ore, 4m) });

        inventory.Execute(Guid.NewGuid(), refining, 1, DateTimeOffset.UnixEpoch);

        Assert.Equal(9m, inventory.GetBalance(Ore));
        Assert.Equal(1m, inventory.GetBalance(Labor));
    }
}
