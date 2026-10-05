namespace TheSingularityWorkshop.Economy;
public enum TransferKind{Purchase,Sale,Transfer,Gift,Donation,Reward,Tip,Refund,Reimbursement,Compensation,Royalty,Subscription,AdvertisingRevenue,MarketplaceSettlement,Fee,Tax,Grant,Deposit,Withdrawal,Adjustment,Custom}
/// <summary>Describes an intended movement of value.</summary>
public sealed record EconomyTransfer(Guid Id,EconomyAccount From,EconomyAccount To,Money Amount,TransferKind Kind,string? Reference=null,DateTimeOffset? CreatedAt=null){public DateTimeOffset Timestamp{get;}=CreatedAt??DateTimeOffset.UtcNow;}