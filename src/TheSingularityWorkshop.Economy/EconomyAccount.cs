namespace TheSingularityWorkshop.Economy;
/// <summary>Represents an account boundary inside the Workshop economy.</summary>
public sealed class EconomyAccount{public EconomyAccount(EconomyEntityId owner,string currency){Id=Guid.NewGuid();Owner=owner;Currency=new Money(0m,currency).Currency;}public Guid Id{get;}public EconomyEntityId Owner{get;}public string Currency{get;}}