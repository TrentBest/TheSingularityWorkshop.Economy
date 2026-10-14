namespace TheSingularityWorkshop.Economy;
/// <summary>Identifies a participant in an economic system.</summary>
public readonly record struct EconomyEntityId(Guid Value){public static EconomyEntityId New()=>new(Guid.NewGuid());}
public enum EconomyEntityKind{Individual,Company,Organization,Group,Experience,Marketplace,Charity,FinancialInstitution,Platform,Custom}