# Economy Capability Selections and the Portable Experience Manifest

**Status: boundary clarification; no portable-manifest adapter is implemented here.**

This document aligns Economy's declarative capability catalog with the existing Workshop MicroBundle identity and FSM_COS composition contracts. It is intentionally an integration contract, not a new Economy runtime or packaging format.

## Identity is layered

The following identities are different and must not be substituted for one another:

| Layer | Identity | Owner |
|---|---|---|
| Economy capability concept | `CapabilityId` (currently a normalized string slug such as `competition.round-robin`) | Economy |
| Portable capability/artifact identity | Canonical ontology string + exact immutable version, with SHA-256 where required | MicroBundleDomain / Experience manifest |
| Repository storage locator | Repository-private storage index | MicroBundleRepository |
| Platform runtime binding | Platform-local numeric slot | FSM_COS / host cache |
| Existing executable domain object | Numeric `MicroBundleDescriptor.Id` / `IMicroBundle.Id` compatibility identity | MicroBundleDomain runtime contract |

The Economy `CapabilityId` is currently a local semantic catalog key. It is **not yet a canonical ontology address**, MicroBundle ID, repository handle, or FSM_COS runtime slot. Do not cast, hash, derive, or otherwise silently convert it into one of those identities. A future explicit catalog/registration mapping must bind the Economy concept to its canonical ontology identity.

The portable manifest is ontology-string based. FSM_COS's current `RuntimeManifest` root contract remains numeric (`BundleId + Version`) for its existing in-process compatibility path. The portable-to-runtime adapter is an additive boundary; the numeric contract must not be reinterpreted as a portable identity.

## Version fields have different meanings

- `CapabilityDescriptor.Version` versions Economy's descriptive metadata/schema contract. It does not prove that an executable MicroBundle revision exists.
- A portable Experience manifest pins the selected MicroBundle's exact artifact version and, where required by the artifact contract, its SHA-256 digest.
- Repository storage indexes and FSM_COS runtime slots are not versions or portable identity.

A selected capability is not executable merely because its descriptor validates. Missing or ambiguous canonical identity, unavailable artifact bytes, mismatched version/digest, or an unresolved runtime binding must fail closed before activation.

## Configuration is not artifact identity

Economy's `CapabilitySelection.Configuration` is an in-memory, declarative selection/configuration model. It is not itself the serialized FSM_COS RuntimeManifest and must not be copied into artifact identity fields.

At the Experience/host boundary:

1. The portable manifest declares selected canonical ontology identities and exact artifact versions (plus digest where required).
2. The Experience owns scenario-specific configuration and policy.
3. A separate host-provided configuration source supplies opaque configuration bytes/options for the resolved runtime bundle and version.
4. FSM_COS composes the resolved MicroBundles; it does not parse the configuration format.
5. The capability interprets its own configuration.

Configuration must be applied only after the exact artifact revision is resolved. Changing configuration does not change the artifact identity. An absent configuration source entry retains the existing FSM_COS semantics: the MicroBundle may use its defaults.

An adapter may translate Economy's typed configuration fields and values into the host's configuration representation, but that translation must be explicit, validated, and owned above Economy. Economy does not add an FSM_COS dependency or serialize runtime manifests.

## Responsibility boundaries

- **Economy:** semantic capability catalog keys, domain APIs, descriptor/configuration metadata, and declarative composition validation.
- **Experience / authoring host:** selected capabilities, canonical identity mapping, exact artifact pins, scenario policy, and publication of a new manifest version after author approval.
- **MicroBundleDomain:** portable capability/dependency meaning and the executable domain contract, including the existing numeric compatibility surface.
- **MicroBundleRepository:** retrieval and verification of immutable artifact bytes; repository-private handles stay private.
- **FSM_COS / host:** portable identity resolution at the platform boundary, local numeric binding, dependency closure, composition, and runtime lifecycle.
- **Presentation / Visualize:** all visual editing, display, and rendering.

Creator updates must not silently rewrite a published Experience or running assembly. An approved update produces a new Experience manifest version. No Economy selection or descriptor operation authorizes activation, artifact upload, publication, or external side effects.

## Explicit integration gaps

The following are not implemented by Economy's current catalog/composition code:

- canonical ontology registration/mapping for each Economy `CapabilityId`;
- an adapter from Economy descriptors to MicroBundleDomain's portable ontology descriptor/dependency contract;
- an Experience manifest serializer or publisher;
- immutable artifact/version/digest resolution and verification;
- a mapping from portable ontology identity to an FSM_COS platform slot and the existing numeric domain-object identity;
- an adapter from Economy selection configuration to the host's separate configuration source;
- runtime activation or MicroBundle loading.

These gaps must be implemented and tested at their owning boundaries. Do not solve them by adding MicroBundleRepository or FSM_COS dependencies to Economy.

## Authoritative cross-repository decisions

- [MicroBundleDomain #28 — portable ontology manifests and platform-local runtime indexes](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/issues/28)
- [MicroBundleDomain #17 — portable dependency identity separate from configuration](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/issues/17)
- [FSM_COS #51 — resolve portable ontology dependencies into platform-local runtime slots](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/issues/51)
- [Economy #22 — composable Experience capability catalog](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/22)

All Economy currency and reward examples remain fictional and non-redeemable. This document authorizes no package publication, merge, deployment, artifact upload, or branch deletion.
