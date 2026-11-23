# Project finance API

**Status: experimental domain vocabulary.** These types describe project financial records. They do not provide persistence, host authorization, accounting certification, payment execution, or a full project-control workflow.

Source: [ProjectFinance.cs](../../src/TheSingularityWorkshop.Economy/ProjectFinance.cs)  
Tests: [ProjectFinanceTests.cs](../../tests/TheSingularityWorkshop.Economy.Tests/ProjectFinanceTests.cs)  
Concept and boundaries: [Project finance and capital planning](../PROJECT_FINANCE_AND_CAPITAL_PLANNING.md)

## Identity and classification

- `CapitalProjectId`, `ProjectCostLineId`, `ProjectFundingSourceId`, `ProjectFundingCommitmentId`, `ProjectCostRecordId`, `ProjectInvoiceId`, and `ProjectPaymentId` provide distinct stable identity types.
- `CapitalProject` records sponsor reference, name, scope, creation time, and an initial proposed status.
- `ProjectCostCategory`, `ProjectFundingSourceKind`, `CapitalProjectStatus`, and `ProjectInvoiceStatus` classify records without imposing Experience-specific business policy.

## Budget

- `ProjectBudgetLine` records one category, typed fictional amount, and estimate basis.
- `ProjectBudgetBaseline.Propose` creates an immutable versioned snapshot.
- A baseline requires at least one line, unique line identities, and one simulated currency.
- `Total` is derived from line amounts.
- `Approve` returns a new snapshot with an approval timestamp and opaque approval reference. It does not authorize the caller or enforce spending permissions.
- A revised baseline should be created with a new version; prior versions should remain available to the host.

## Funding

- `ProjectFundingSource` identifies a source kind, currency, and optional restriction.
- `ProjectFundingCommitment` records a promise/allocation and optional condition/expected date.
- A commitment is not a receipt, cash balance, or ledger posting. The source currency must match the commitment amount.

## Cost, invoice, and payment

- `ProjectCostRecord` records recognized cost with a budget-line reference and evidence reference. It is not a cash movement.
- `ProjectInvoice` records a supplier claim linked to a cost record and its validation status. It is not proof of payment.
- `ProjectPayment` links a payment record to a `SimulationLedgerReceipt`. The receipt must describe a transfer and match the payment amount exactly.

## Current limitations

The current types do not aggregate commitments, enforce project-level spending authority, track cumulative partial payments, reconcile invoices, calculate forecasts, manage change requests, or close projects. They do not model debt liabilities, equity rights, tax, interest schedules, or accounting statements. Those are subsequent capabilities and must not be inferred from these records.

All amounts in this initial vertical are typed fictional-currency amounts. Real-money execution is excluded.
