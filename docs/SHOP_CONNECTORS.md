# Shop connectors

The optional `TheSingularityWorkshop.Economy.ShopConnectors` companion adapts provider catalogs into canonical Economy shop and item identities. Economy core does not reference this companion, a provider SDK, Profiles, FSM_REST, Experiences, or WebPage.

## Current implementation

- `IShopConnector` exposes provider identity, declared capabilities, and catalog synchronization.
- `IShopCatalogSource` is the narrow source boundary. A provider-specific adapter translates its API or storage into `ProviderShopSnapshot` and `ProviderCatalogItem` values.
- `SnapshotShopConnector` maps those snapshots to `EconomyShop` and `CommerceItem`.
- `IShopIdentityMap` maps provider/shop/item identities to stable Workshop IDs. The included `InMemoryShopIdentityMap` is for tests and ephemeral hosts; durable hosts must supply a persistent implementation.
- `InMemoryShopCatalogSource` is the native Workshop source for the first vertical slice. The tests also use a deterministic fake marketplace source to prove the contract is not tied to the native provider.

Provider item identity is scoped by provider, external shop ID, and external item ID. A provider changing a product URL or display name therefore does not create a new Workshop item. A durable identity-map implementation is required to preserve those IDs across process restarts.

## Capabilities and safety boundary

Capabilities are declared by each source. The initial contract distinguishes catalog read, catalog synchronization, external product links, and optional availability observations. Availability is descriptive provider data, not an inventory command.

The current connector does **not** implement checkout, purchase, reservation, stock mutation, refund, settlement, payment processing, account authentication, or provider credentials. A catalog read or external product URL must never be interpreted as authorization to perform those operations.

## Minimal usage

```csharp
using TheSingularityWorkshop.Economy;
using TheSingularityWorkshop.Economy.ShopConnectors;

var source = new InMemoryShopCatalogSource(new[]
{
    new ProviderShopSnapshot(
        "creator-shop-1",
        "Mara's Workshop",
        new Uri("https://example.invalid/shops/creator-shop-1"),
        DateTimeOffset.UtcNow,
        new[]
        {
            new ProviderCatalogItem(
                "ceramic-cup",
                "Handmade ceramic cup",
                new Money(12m, "USD"),
                new Uri("https://example.invalid/items/ceramic-cup"))
        })
});

var connector = new SnapshotShopConnector(source);
var imported = await connector.SynchronizeAsync(
    "creator-shop-1",
    EconomyEntityId.New(),
    new InMemoryShopIdentityMap());

Console.WriteLine(imported.Shop.Name);
```

For production use, replace the in-memory source and identity map with host-owned durable implementations. A marketplace adapter can implement `IShopCatalogSource` and translate provider responses without adding marketplace SDK types to Economy.

## Integration seams

- **Profiles:** optional host-level identity and publisher attribution. A profile ID is not authentication or proof of shop ownership.
- **FSM_REST:** optional transport for providers whose APIs fit its REST contract. It is not required by the connector interface.
- **Experiences:** own placement, eligibility, behavior, and presentation.
- **WebPage:** owns publishing, discovery, embedding, visitor presentation, and host policy.

The existing Experience composition test proves one canonical shop can be placed in two Experiences with different presentations. Connector tests separately prove provider identity mapping and synchronization. They do not yet prove durable persistence, a hosted storefront, or WebPage publication.
