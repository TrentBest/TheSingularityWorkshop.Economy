# Creator Shop Composition — Contract Proof

**Status: test-owned reference fixture, not a production storefront or WebPage integration.**

The [CreatorShopCompositionTests](../tests/TheSingularityWorkshop.Economy.Tests/CreatorShopCompositionTests.cs) exercise one important boundary for the planned creator workflow:

- Economy owns the canonical shop and its product identities.
- An Experience owns where the shop is placed and how it is presented.
- A presentation skin is not a new shop, catalog, owner, or product identity.
- The same canonical shop can be referenced by two different Experience placements.
- These examples use fictional `GLD`; no account, balance, checkout, or real-money operation is involved.

## What the tests prove

The first test creates one shop and places it in two consumer-owned Experience records with different skin IDs. Both placements reference the same shop object and retain the same shop and product IDs.

The second test replaces the presentation record while checking that the canonical shop and catalog identities remain unchanged.

This is intentionally a **consumer-owned contract fixture**. `ShopPresentation` and `ExperienceShopPlacement` are private test records, not public Economy API. The tests do not claim to implement a visual editor, saved creator projects, persistent catalog storage, WebPage manifest integration, runtime rendering, inventory, transaction execution, or a city simulation.

## Next proof steps

1. Move the composition example into a real consumer/Experience integration once the correct host-side API is identified.
2. Make the same shop visible in two genuinely different presentations.
3. Add a base-Experience test showing that it remains usable when Economy is not composed.
4. Add persistence and creator-editing only after the runtime composition path is real.
5. Separately benchmark a deterministic Digiten simulation workload; shop-composition tests are not evidence of population scale.

## Related direction

See [Simulated Economies and Creator Commerce](SIMULATED_ECONOMIES_AND_CREATOR_COMMERCE.md) for the broader Singularity City, adoption, and monetization plan.
