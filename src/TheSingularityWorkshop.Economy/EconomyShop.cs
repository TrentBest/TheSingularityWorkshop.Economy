namespace TheSingularityWorkshop.Economy;

/// <summary>
/// Describes a creator-defined shop without owning its visual presentation or payment infrastructure.
/// The shop owns an immutable snapshot of its canonical catalog.
/// </summary>
public sealed record EconomyShop
{
    public EconomyShop(
        Guid id,
        EconomyEntityId owner,
        string name,
        IReadOnlyList<CommerceItem> products)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("A shop identifier is required.", nameof(id));
        if (owner.Value == Guid.Empty)
            throw new ArgumentException("A shop owner is required.", nameof(owner));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(products);

        var snapshot = products.ToArray();
        if (snapshot.Any(product => product is null))
            throw new ArgumentException("A shop catalog cannot contain null products.", nameof(products));
        if (snapshot.Any(product => product.Id.Value == Guid.Empty))
            throw new ArgumentException("Every product must have an identifier.", nameof(products));
        if (snapshot.Any(product => product.Seller.Value != owner.Value))
            throw new ArgumentException("Every product seller must match the shop owner.", nameof(products));
        if (snapshot.Select(product => product.Id).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("A shop catalog cannot contain duplicate product identifiers.", nameof(products));
        if (snapshot.Any(product => string.IsNullOrWhiteSpace(product.Name)))
            throw new ArgumentException("Every product must have a name.", nameof(products));
        if (snapshot.Any(product => product.Price.Amount < 0m ||
            string.IsNullOrWhiteSpace(product.Price.Currency)))
            throw new ArgumentException("Every product must have a valid non-negative price.", nameof(products));

        Id = id;
        Owner = owner;
        Name = name.Trim();
        Products = Array.AsReadOnly(snapshot);
    }

    public Guid Id { get; }
    public EconomyEntityId Owner { get; }
    public string Name { get; }
    public IReadOnlyList<CommerceItem> Products { get; }

    /// <summary>Creates a shop with a generated identifier and an immutable product snapshot.</summary>
    public static EconomyShop Create(
        EconomyEntityId owner,
        string name,
        IEnumerable<CommerceItem> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        return new EconomyShop(Guid.NewGuid(), owner, name, products.ToArray());
    }
}
