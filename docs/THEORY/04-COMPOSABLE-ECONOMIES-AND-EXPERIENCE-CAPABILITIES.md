# Composable Economies: From Capability Catalog to Experience

## 01 — What

A composable economy is a collection of independently selectable economic capabilities connected by explicit contracts. The developer sees a catalog of possibilities and selects only the capabilities that make sense for a particular Experience.

## 02 — Why

A medieval RPG, a city simulator, a spacefaring trading game, and a creator competition have different economic needs. Forcing each one to adopt a complete universal economy would make the shared package cumbersome and would blur ownership. A capability catalog makes breadth discoverable while keeping each Experience's composition small.

The old toy-store analogy is useful: the developer should be able to see the possibilities, inspect examples, try pieces, and combine them. The catalog is not a demand that every Experience become a casino, marketplace, or financial simulation.

## 03 — How

### Descriptor versus implementation

A descriptor is metadata: stable ID, name, category, version, maturity, dependencies, conflicts, configuration fields, and documentation reference. It tells a host and developer what a capability claims to provide. It does not contain executable behavior and does not authorize side effects.

### Composition versus activation

An Experience composition is a declaration of selected capabilities and their configuration. Validation checks that selected IDs exist, required dependencies are selected, declared conflicts are absent, and configuration keys and values match the descriptor.

Activation is a separate host responsibility. A host must still decide whether the Experience is authorized to activate a capability, which services are available, what lifecycle to use, and whether required persistence or scheduling guarantees exist. A valid composition is not proof that a runtime can safely execute it.

### Optionality and dependency closure

An Experience can select zero capabilities. It can select an independent capability without selecting unrelated catalog entries. If a selected capability declares a dependency, the composition must include that dependency. This first implementation validates direct declarations; transitive dependency closure, cycle detection, version ranges, and host activation order remain future work.

### Configuration

Configuration values are Experience-owned strings checked against declared metadata. Boolean, integer, decimal, string, and enumeration shapes are supported. Decimal parsing is invariant-culture. The contract does not yet provide JSON Schema, nested objects, secret storage, dynamic UI generation, or arbitrary plugin loading.

### Example compositions

| Experience | Example optional selection |
|---|---|
| RPG kingdom | fictional ledger, shops, production, training, taxation |
| Spacefaring sim | fictional ledger, shipyard, fuel, cargo, trade |
| City simulator | public budget, construction, materials, zoning proposals |
| Tournament showcase | competitor registration, bracket, score provenance |
| Cantina showcase | fictional chips, blackjack, poker adapter, tournaments |

These are illustrative composition plans; not every listed capability is implemented.

### Ownership and safety

Economy supplies reusable domain contracts. The Experience owns its world, player interaction, UI, story, and scenario policy. A host owns activation and authority. Visualize owns globe projection, textures, and rendering. Fictional simulation value remains non-redeemable and separate from real-money payments or custody.

## 04 — See it in a minute

```csharp
var ledger = new CapabilityDescriptor(
    new CapabilityId("simulation.ledger"),
    "Fictional ledger",
    "simulation",
    "Process-local accounting for fictional value.",
    "1.0",
    CapabilityMaturity.Experimental);

var shops = new CapabilityDescriptor(
    new CapabilityId("commerce.shops"),
    "Experience shops",
    "commerce",
    "Describes creator-owned shops and their items.",
    "1.0",
    CapabilityMaturity.Experimental,
    dependencies: new[] { ledger.Id });

var catalog = new CapabilityCatalog(new[] { ledger, shops });
var composition = new ExperienceCapabilityComposition(new[]
{
    new CapabilitySelection(ledger.Id),
    new CapabilitySelection(shops.Id)
});

var validation = composition.Validate(catalog);
if (!validation.IsValid)
    throw new InvalidOperationException("The Experience composition is invalid.");
```

This validates declarations only. It does not activate a ledger, create accounts, load shops, issue value, or start a runtime.

## 05 — Choose your next step

- Add sample Experience manifests that select different subsets from one shared catalog.
- Add dependency cycle detection and version compatibility.
- Connect the declarative composition to a host-owned activation protocol only when the host can enforce authority, lifecycle, and failure recovery.
- Extend configuration metadata before attempting dynamic editor generation.
