namespace TheSingularityWorkshop.Economy;

public readonly record struct CommerceItemId(Guid Value)
{
    public static CommerceItemId New() => new(Guid.NewGuid());
}

/// <summary>Describes a valid product or service without owning its rich presentation.</summary>
public sealed record CommerceItem
{
    public CommerceItem(
        CommerceItemId id,
        EconomyEntityId seller,
        string name,
        Money price,
        Uri? externalReference = null,
        string? microBundleReference = null)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A commerce item identifier is required.", nameof(id));
        if (seller.Value == Guid.Empty)
            throw new ArgumentException("A commerce item seller is required.", nameof(seller));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price.Amount < 0m || string.IsNullOrWhiteSpace(price.Currency))
            throw new ArgumentException("A commerce item requires a valid non-negative price.", nameof(price));
        if (microBundleReference is not null && string.IsNullOrWhiteSpace(microBundleReference))
            throw new ArgumentException("A MicroBundle reference must be non-empty when supplied.", nameof(microBundleReference));

        Id = id;
        Seller = seller;
        Name = name.Trim();
        Price = price;
        ExternalReference = externalReference;
        MicroBundleReference = microBundleReference?.Trim();
    }

    public CommerceItemId Id { get; }
    public EconomyEntityId Seller { get; }
    public string Name { get; }
    public Money Price { get; }
    public Uri? ExternalReference { get; }
    public string? MicroBundleReference { get; }

    public void Deconstruct(
        out CommerceItemId id,
        out EconomyEntityId seller,
        out string name,
        out Money price,
        out Uri? externalReference,
        out string? microBundleReference)
    {
        id = Id;
        seller = Seller;
        name = Name;
        price = Price;
        externalReference = ExternalReference;
        microBundleReference = MicroBundleReference;
    }
}

/// <summary>A marketplace's immutable offer to list a canonical commerce item.</summary>
public sealed record MarketplaceOffer
{
    public MarketplaceOffer(
        Guid id,
        CommerceItem item,
        EconomyEntityId marketplace,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("A marketplace offer identifier is required.", nameof(id));
        ArgumentNullException.ThrowIfNull(item);
        if (marketplace.Value == Guid.Empty)
            throw new ArgumentException("A marketplace identity is required.", nameof(marketplace));

        Id = id;
        Item = item;
        Marketplace = marketplace;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public CommerceItem Item { get; }
    public EconomyEntityId Marketplace { get; }
    public DateTimeOffset CreatedAt { get; }

    public void Deconstruct(
        out Guid id,
        out CommerceItem item,
        out EconomyEntityId marketplace,
        out DateTimeOffset createdAt)
    {
        id = Id;
        item = Item;
        marketplace = Marketplace;
        createdAt = CreatedAt;
    }
}
