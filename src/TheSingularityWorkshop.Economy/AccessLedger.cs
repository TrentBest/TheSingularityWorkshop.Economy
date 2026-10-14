namespace TheSingularityWorkshop.Economy;
public enum EconomyAccessKind{AccountRead,TransactionRead,FinancialConnectionRead,PaymentInitiated,PaymentCompleted,MarketplaceRead,RewardRead,LicenseRead,Other}
/// <summary>Records an access event for owner-facing transparency and auditing.</summary>
public sealed record EconomyAccessRecord(EconomyEntityId Owner,EconomyEntityId Observer,EconomyAccessKind Kind,DateTimeOffset AccessedAt,string? ResourceReference=null);
public interface IEconomyAccessRecorder{void Record(EconomyAccessRecord access);}