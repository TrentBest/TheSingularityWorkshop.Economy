# Game store patterns: weapons, magic, consumables, and more

> Status: implementation guide for the exploratory alpha. The current-API example below uses real public types. Inventory, purchase processing, pricing rules, and player balances are Experience-owned responsibilities; this package does not implement them.

## The design principle

Build different stores from a small shared vocabulary, then let each game define its own rules.

- **Economy** describes items, prices, and a creator-defined shop.
- **The game Experience** decides what an item does, who may buy it, whether it is in stock, how prices change, and what happens after a purchase.
- **An execution adapter** would eventually process an approved exchange. Creating a shop or transfer intent does not itself buy anything.

A weapon shop and a magic shop usually do not need different economic primitives. They need different item metadata and game rules.

## 1. Start with the shared catalog vocabulary

This is a current-API example. **GLD** is fictional game currency using the package's required three-letter unit code.

\`\`\`csharp
using TheSingularityWorkshop.Economy;

var guild = EconomyEntityId.New();

var ironSword = new CommerceItem(
    CommerceItemId.New(), guild, "Iron Sword", new Money(50m, "GLD"));

var healthPotion = new CommerceItem(
    CommerceItemId.New(), guild, "Health Potion", new Money(12m, "GLD"));

var adventurerSupply = EconomyShop.Create(
    guild,
    "Adventurer's Supply",
    new[] { ironSword, healthPotion });
\`\`\`

This creates two product descriptions and a shop containing a snapshot of those product references. It does **not** add inventory, grant items to a player, check a balance, reserve stock, or execute a purchase.

The current **CommerceItem** contains an ID, seller, name, **Money** price, and optional external/MicroBundle references. It has no built-in category, rarity, stats, requirements, stock, or game-effect fields. Keep those in the Experience's richer catalog rather than pretending the shared type already supports them.

## 2. Choose store patterns by game rule

### RPG weapon shop

Use Economy for the shared item identity and asking price. Keep weapon stats and equip rules in the game's item definition.

Illustrative Experience-owned definition (application code, not an Economy type):

\`\`\`csharp
public sealed record WeaponDefinition(
    CommerceItem Commerce,
    int Damage,
    string DamageType,
    string RequiredSkill,
    int RequiredLevel);
\`\`\`

A weapon shop can apply level requirements, class restrictions, durability, upgrades, buyback prices, and limited stock. The same weapon definition may be referenced by a merchant, quest reward, crafting result, or loot table without making those systems all become shops.

**Purchase sequence:** validate the offer and current price → validate player eligibility → reserve or decrement stock → apply the game's value-transfer policy → grant the weapon → record the result. Define failure and rollback behavior; the current **EconomyShop** does not perform these steps.

### Magic shop

Magic items may need spell school, charges, attunement, rarity, usage restrictions, or spell-learning behavior.

\`\`\`csharp
public sealed record MagicItemDefinition(
    CommerceItem Commerce,
    string School,
    string Rarity,
    int? Charges,
    string? RequiredAttunement);
\`\`\`

The Experience decides whether the buyer knows the required discipline, whether a relic is unique, and whether use consumes a charge. Avoid encoding these rules in a global Economy enum: another game may have an entirely different magic system.

### Potion, food, and consumable stall

Consumables may need stack limits, expiration, effects, and restock behavior. These are inventory/gameplay concerns.

\`\`\`csharp
public sealed record ConsumableDefinition(
    CommerceItem Commerce,
    string EffectId,
    int StackLimit,
    TimeSpan? Duration);
\`\`\`

The store may use bulk discounts or daily restock rules. The game should validate quantities and calculate the final total explicitly. The current API has no cart or quantity-aware order object.

### Crafting supplier

A crafting supplier can sell ingredients and recipes. The catalog can refer to the same ingredient definitions used by gathering, crafting, quests, and player trading. Recipe eligibility and resource consumption remain game rules; a listing is not proof that an ingredient is available.

### Black market / faction vendor

A black-market shop can reuse the same item-and-shop model while the Experience owns reputation checks, faction access, hidden inventory, rotating offers, risk, and price multipliers. Keep visibility and eligibility checks on the authoritative simulation/server boundary; hiding an item in the UI alone is not authorization.

### Player-to-player market

A **MarketplaceOffer** currently describes an item, marketplace identity, and creation time. It is not a complete listing lifecycle. A game market still needs owner verification, listing state, price snapshots, expiry, cancellation, stock/ownership reservation, and atomic settlement rules. Treat the offer as a description, not as proof of ownership or a completed trade.

## 3. Keep price, stock, and player state separate

| Concern | Example | Owner |
|---|---|---|
| Shared economic description | Item ID, seller, asking price | Economy vocabulary |
| Game identity | Damage, rarity, effects, equip slot | Game catalog |
| Availability | 3 in stock, 1 reserved | Inventory/stock service |
| Eligibility | Level 10, guild rank, quest complete | Game policy |
| Price calculation | Reputation discount, sale multiplier | Game pricing policy |
| Player state | Currency balance, owned item, equipment | Authoritative game state |
| Purchase lifecycle | Requested, accepted, failed, uncertain | Transaction/execution boundary |
| Presentation | 3D model, tooltip, UI, animation | Client/Experience |

Do not store mutable stock or balances by mutating an **EconomyShop**. The shop intentionally holds an immutable snapshot of product references. When a price changes, update the Experience's authoritative catalog state and decide whether an existing offer retains its quoted price or adopts the new one.

## 4. A reusable shop-building workflow

1. **Define the Experience item.** Give it a stable game ID and the metadata the game needs.
2. **Create its Economy description.** Use a **CommerceItemId**, seller identity, display name, and **Money** asking price.
3. **Compose a shop.** Use **EconomyShop.Create** with the items offered by that merchant. It snapshots the provided product sequence.
4. **Attach game policies.** Decide availability, stock, eligibility, price modifiers, quantity limits, and whether the item is buyable, sellable, giftable, or reward-only.
5. **Build the presentation.** Render a sword rack, enchanted cabinet, potion shelf, terminal, or market board from Experience-owned data.
6. **Implement the purchase boundary.** Validate against authoritative state, prevent duplicate processing, define failure recovery, and only grant the item when the selected execution policy provides an appropriate outcome.
7. **Test the edges.** Two players buying the last item; price changes after a shop opens; insufficient funds; invalid quantity; disconnect during purchase; repeated request; failed item grant; and a shop refresh while a cart is open.

Steps 4–7 are recommended application architecture, not functionality currently supplied by this package.

## 5. One catalog, many storefronts

A game can expose the same catalog through several views:

- **Adventurer's Supply:** starter weapons, armor, food, and repair kits.
- **Arcanum:** spells, wands, scrolls, and rare relics.
- **Field Apothecary:** potions and short-duration consumables.
- **Guild Quartermaster:** rank-gated equipment and faction supplies.
- **Auction Hall:** player listings and expiring offers.
- **Traveling Merchant:** a rotating subset with regional prices.

The storefronts can share item definitions while having different shop compositions and policies. Avoid duplicating the canonical item definition just to show it in another shop. If different merchants quote different prices, model the quote/listing explicitly in the Experience; the current **CommerceItem** itself carries one price.

## Current API boundary and future opportunities

Already available: **Money**, **CommerceItem**, **EconomyShop**, **MarketplaceOffer**, **EconomyTransfer**, and **EconomicExecutionResult**.

Not currently provided: category/rarity/stat schemas, stock, carts, quantity-based orders, player inventory, balances/ledger, pricing engines, purchase orchestration, idempotency enforcement, or a working simulation/provider adapter.

A future reusable game-commerce layer could add optional, game-neutral contracts for catalog metadata references, quotes, order lines, inventory reservations, and purchase outcomes. Before adding those to Economy, establish the smallest shared contracts that multiple Experiences actually need; keep RPG-specific damage, spell schools, faction rules, and UI out of the core.

## Related reading

- [Economy architecture](ARCHITECTURE.md) — current types and explicit future boundaries.
- [What is Economy?](WHAT_IS_ECONOMY.md) — the domain model in plain language.
- [Domain tests](../tests/TheSingularityWorkshop.Economy.Tests/EconomyTests.cs) — current API invariants.
