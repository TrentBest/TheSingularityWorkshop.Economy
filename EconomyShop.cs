namespace TheSingularityWorkshop.Economy;

/// <summary>
/// Describes a creator-defined shop or storefront without owning its visual presentation or payment infrastructure.
/// </summary>
public sealed record EconomyShop(
    Guid Id,
    EconomyEntityId Owner,
    string Name,
    IReadOnlyList<CommerceItem> Products)
{
    /// <summary>Creates a shop with a generated identifier and an immutable product snapshot.</summary>
    public static EconomyShop Create(
        EconomyEntityId owner,
        string name,
        IEnumerable<CommerceItem> products)
    {
        ArgumentNullException.ThrowIfNull(products);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A shop name is required.", nameof(name));
        }

        return new EconomyShop(Guid.NewGuid(), owner, name, products.ToArray());
    }
}
