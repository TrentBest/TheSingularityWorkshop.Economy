# Project finance domain neighborhood

**Status: current vocabulary view; not a complete workflow or authorization diagram.**

```mermaid
classDiagram
    class CapitalProject {
      +CapitalProjectId Id
      +EconomyEntityId Sponsor
      +string Name
      +string Scope
      +CapitalProjectStatus Status
    }
    class ProjectBudgetBaseline {
      +CapitalProjectId ProjectId
      +int Version
      +IReadOnlyList~ProjectBudgetLine~ Lines
      +SimulatedCurrencyId CurrencyId
      +DateTimeOffset? ApprovedAt
      +Guid? ApprovalReference
      +decimal Total
      +Propose()
      +Approve()
    }
    class ProjectBudgetLine {
      +ProjectCostLineId Id
      +ProjectCostCategory Category
      +SimulatedAmount Amount
      +string EstimateBasis
    }
    class ProjectFundingSource {
      +ProjectFundingSourceId Id
      +ProjectFundingSourceKind Kind
      +SimulatedCurrencyId CurrencyId
      +string? Restriction
    }
    class ProjectFundingCommitment {
      +ProjectFundingCommitmentId Id
      +CapitalProjectId ProjectId
      +ProjectFundingSource Source
      +SimulatedAmount Amount
      +bool IsConditional
    }
    class ProjectCostRecord {
      +ProjectCostRecordId Id
      +CapitalProjectId ProjectId
      +ProjectCostLineId BudgetLineId
      +SimulatedAmount Amount
      +string EvidenceReference
    }
    class ProjectInvoice {
      +ProjectInvoiceId Id
      +CapitalProjectId ProjectId
      +ProjectCostRecordId CostRecordId
      +SimulatedAmount Amount
      +ProjectInvoiceStatus Status
    }
    class ProjectPayment {
      +ProjectPaymentId Id
      +CapitalProjectId ProjectId
      +ProjectInvoiceId InvoiceId
      +SimulatedAmount Amount
      +SimulationLedgerReceipt LedgerReceipt
    }
    class SimulationLedger {
      +Issue()
      +Transfer()
      +Retire()
      +Post()
    }
    CapitalProject "1" --> "*" ProjectBudgetBaseline
    ProjectBudgetBaseline "1" --> "*" ProjectBudgetLine
    CapitalProject "1" --> "*" ProjectFundingCommitment
    ProjectFundingCommitment "*" --> "1" ProjectFundingSource
    CapitalProject "1" --> "*" ProjectCostRecord
    ProjectCostRecord "1" --> "*" ProjectInvoice
    ProjectInvoice "1" --> "*" ProjectPayment
    ProjectPayment --> SimulationLedger : references transfer receipt
```

## Boundary rules

- A funding commitment is not cash received.
- A budget baseline is not spending authority by itself.
- A cost record is not an invoice or payment.
- An invoice is not proof of payment.
- A payment references an existing simulated ledger transfer; it does not initiate one.
- Estimates, commitments, costs, and invoices do not create ledger postings.
- Project-specific identity, authorization, persistence, and presentation belong to the host/Experience.
