# TheSingularityWorkshop.Economy

Economic infrastructure for The Singularity Workshop.

This is deliberately not a Square wrapper, bank SDK wrapper, wallet, or marketplace implementation. It defines a provider-neutral economic language so banks, payment processors, marketplaces, experiences, creators, companies, and users can connect through adapters.

## What belongs here

Purchases, sales, transfers, gifts, donations, tips, creator rewards, advertising revenue, subscriptions, royalties, refunds, reimbursements, grants, marketplace settlement, fees, taxes, deposits, withdrawals, and future forms of value exchange.

The core describes economic intent. A provider adapter decides how that intent is fulfilled externally.

## Sovereignty

A connected bank or payment account is not the user's Workshop account. Users choose their connections and economic access should be observable through an owner-facing access ledger.

The boundary separates economic identity, Workshop accounts, external financial connections, transfers, access records, and fee policy.

## Provider adapters

No Square, bank, card-network, or wallet SDK belongs in this core. A future adapter implements IFinancialProvider and handles provider authentication, webhooks, idempotency, disputes, refunds, and compliance requirements.

A virtual bank branch can therefore visualize a financial connection without becoming the financial institution.

## AI-assisted creation

Profiles can supply private information while ProtocolAi and GrammarAi represent structured questions, jurisdictional requirements, application data, and machine-readable workflows. An LLM may help determine what must be asked and which workflow applies; it must not silently become the authority that declares a business licensed, a payment settled, or a person financially authorized.

## Products and marketplaces

A photographed physical object can become a CommerceItem linked to a URL, Micro Bundle, experience, or marketplace. The economy handles exchange while the creator controls the rich representation. This creates a path from photo to product to marketplace to purchase to settlement to digital counterpart without turning Economy into a giant product database.

## Platform share

FeePolicy makes a sustainable platform share explicit rather than hidden. Provider costs, taxes, refunds, and other obligations remain separate allocations. The goal is to take as little as the system needs to remain healthy while letting creators and users retain the value they generate.

## Status

Alpha architecture. No live financial provider integration is included yet. The next boundary is a provider-neutral transaction lifecycle and sandbox adapter, followed by a real provider only after its requirements are explicitly modeled.
