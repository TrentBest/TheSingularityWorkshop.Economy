# Capability Catalog and Composition UML

**Status: implemented declarative contracts; no runtime activation or editor.**

## 01 — What

The capability catalog describes optional features an Experience can discover and select. A descriptor advertises stable identity, category, maturity, dependencies, conflicts, configuration metadata, and a documentation reference. An Experience composition holds selected descriptors and Experience-owned string configuration values.

## 02 — Why

A single package can offer a broad set of possibilities without imposing every feature on every Experience. A city simulator may select budgets and construction; an RPG may select shops and training; a tournament Experience may select competition capabilities. The composition contract makes those selections inspectable and validates their declared compatibility before a host chooses whether and how to activate anything.

## 03 — How

```mermaid
classDiagram
    class CapabilityId {
      +string Value
    }
    class CapabilityDescriptor {
      +CapabilityId Id
      +string Name
      +string Category
      +string Description
      +string Version
      +CapabilityMaturity Maturity
      +IReadOnlyList Dependencies
      +IReadOnlyList Conflicts
      +IReadOnlyList Configuration
      +string DocumentationReference
    }
    class CapabilityCatalog {
      +IReadOnlyCollection Descriptors
      +TryGet(id)
    }
    class CapabilityConfigurationField {
      +string Key
      +CapabilityConfigurationValueType ValueType
      +bool Required
      +IReadOnlyList AllowedValues
    }
    class CapabilitySelection {
      +CapabilityId CapabilityId
      +IReadOnlyDictionary Configuration
    }
    class ExperienceCapabilityComposition {
      +IReadOnlyList Selections
      +Validate(catalog)
    }
    class CapabilityCompositionValidation {
      +bool IsValid
      +IReadOnlyList Issues
    }
    CapabilityCatalog "1" o-- "*" CapabilityDescriptor
    CapabilityDescriptor --> CapabilityId
    CapabilityDescriptor o-- CapabilityConfigurationField
    ExperienceCapabilityComposition "1" o-- "*" CapabilitySelection
    CapabilitySelection --> CapabilityId
    ExperienceCapabilityComposition --> CapabilityCatalog : validates against
    ExperienceCapabilityComposition --> CapabilityCompositionValidation : returns
```

### Validation flow

```mermaid
flowchart TD
    A[Experience selects zero or more capabilities] --> B[Resolve each stable ID in catalog]
    B --> C{Descriptor found?}
    C -- No --> X[Report unknown capability]
    C -- Yes --> D[Check required dependencies]
    D --> E[Check declared conflicts]
    E --> F[Check required and unknown configuration keys]
    F --> G[Validate declared value types]
    G --> H[Return all findings]
    H --> I{No findings?}
    I -- Yes --> J[Composition is declaratively valid]
    I -- No --> K[Host must reject or ask developer to correct]
    J --> L[Host separately decides activation and authority]
```

## 04 — See it in a minute

The tests compose an optional fictional ledger capability and a shop capability that depends on it, then show that selecting only the ledger is valid while selecting the shop without the ledger is not. A second example declares mutually incompatible live-provider and simulation modes. Configuration is validated against descriptor metadata without embedding a universal editor or executing any selected capability.

See [CapabilityCompositionTests.cs](../../tests/TheSingularityWorkshop.Economy.Tests/CapabilityCompositionTests.cs) and [CapabilityComposition.cs](../../src/TheSingularityWorkshop.Economy/CapabilityComposition.cs).

## 05 — Choose your next step

- Add sample Experience manifests that select different subsets from one shared catalog.
- Define capability dependency cycles and version compatibility once descriptor version semantics are formalized.
- Add a host-owned activation protocol only when a real host integration is ready to enforce lifecycle, permissions, and side-effect boundaries.
- Do not mistake descriptor maturity for operational authorization. No capability in this contract can initiate payments, issue fictional value, or activate itself.
