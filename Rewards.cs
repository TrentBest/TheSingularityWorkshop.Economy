namespace TheSingularityWorkshop.Economy;
public sealed record EconomyReward(Guid Id,EconomyEntityId Recipient,Money Amount,TransferKind Kind,string ActivityReference,DateTimeOffset CreatedAt);
public sealed record EconomyGift(Guid Id,EconomyEntityId From,EconomyEntityId To,Money Amount,TransferKind Kind,string? Purpose,DateTimeOffset CreatedAt);