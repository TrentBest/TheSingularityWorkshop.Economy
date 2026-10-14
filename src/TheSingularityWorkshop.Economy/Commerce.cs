namespace TheSingularityWorkshop.Economy;
public readonly record struct CommerceItemId(Guid Value){public static CommerceItemId New()=>new(Guid.NewGuid());}
/// <summary>Describes a product or service without owning its rich presentation.</summary>
public sealed record CommerceItem(CommerceItemId Id,EconomyEntityId Seller,string Name,Money Price,Uri? ExternalReference=null,string? MicroBundleReference=null);
public sealed record MarketplaceOffer(Guid Id,CommerceItem Item,EconomyEntityId Marketplace,DateTimeOffset CreatedAt);