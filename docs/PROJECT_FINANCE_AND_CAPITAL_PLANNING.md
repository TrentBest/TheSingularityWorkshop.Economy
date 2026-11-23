---
layout: default
title: Project finance and capital planning
---

<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# Project finance and capital planning

**Status: domain design and implementation contract.** This guide defines a provider-neutral, simulation-first capability for planning and controlling projects that consume money, materials, labor, time, and capacity. It does not describe a production accounting system, lender, bank, investment product, or financial service.

## 01 — The idea

A project is a temporary undertaking that turns authorized resources into an intended result. A project can be a bridge, factory, power station, neighborhood, software product, ship, research program, festival, or creator business.

Economy should make the financial story of that project inspectable from proposal through closeout:

1. What are we trying to build, and who owns the outcome?
2. What will it cost, and what assumptions support the estimate?
3. Where will the resources come from?
4. What has been authorized, reserved, committed, incurred, invoiced, and paid?
5. What has actually been delivered?
6. What changed, who approved it, and why?
7. What remains to finish, and what is the current forecast?
8. What did the completed project cost, produce, and teach us?

This is broader than a budget spreadsheet. A budget is one control inside a project-finance lifecycle.

## 02 — Two meanings of project finance

The term is used in two related but different ways. The domain must teach both without pretending they are interchangeable.

### Project financial management

The everyday discipline of estimating, budgeting, funding, authorizing, tracking, forecasting, and closing a project. It applies to a small creator project as well as a major capital program.

### Project finance as a capital structure

A financing model in which a ring-fenced project or project company is funded through a defined combination of sponsor equity, debt, grants, contracted revenue, and other sources, with repayment often dependent on project cash flows and allocated risks.

The second is a specialized financing structure, not a synonym for every project budget. The simulation may illustrate its concepts, but it must not claim to underwrite credit, determine bankability, or model legal recourse without explicit, reviewed assumptions.

## 03 — The project lifecycle

| Stage | Questions | Records and decisions |
|---|---|---|
| Proposal | What problem or opportunity exists? | Project identity, sponsor, purpose, intended outcomes |
| Feasibility | Is the proposal plausible? | Options, assumptions, constraints, risks, feasibility evidence |
| Estimate | What resources are needed? | Scope breakdown, quantities, unit assumptions, estimate class and confidence |
| Funding plan | Who supplies which resources, and under what terms? | Funding sources, commitments, restrictions, timing, conditions |
| Authorization | What may be spent, by whom, and within which limits? | Approved baseline, delegated authority, approval record |
| Procurement and commitment | What obligations have been entered? | Purchase commitments, contracts, orders, reservations |
| Delivery | What work and cost have occurred? | Milestones, progress evidence, incurred cost, invoices, payments |
| Forecast and control | What is likely to happen next? | Estimate to complete, estimate at completion, variance, risks, change requests |
| Commissioning / acceptance | Was the intended result delivered? | Inspection, acceptance, defects, handover, asset reference |
| Closeout | What did it finally cost and produce? | Final account, released commitments, residual liabilities, lessons learned |

A project may pause, fail, be cancelled, or be superseded at any stage. Those outcomes remain visible; the history is not rewritten as if the project never existed.

## 04 — The financial vocabulary

These values must not be collapsed into one "spent" counter.

- **Estimate:** a forecast based on stated scope, quantities, rates, assumptions, date, and confidence.
- **Budget:** an authorized spending envelope or control baseline. A proposed budget is not an approved budget.
- **Funding source:** the origin and restrictions of resources: sponsor allocation, internal transfer, grant, donation, equity contribution, debt proceeds, or simulated operating revenue.
- **Funding commitment:** a recorded promise or allocation under stated conditions. It is not cash received.
- **Available funding:** funding recognized as available under the scenario's explicit rules. A conditional promise is not automatically available cash.
- **Commitment / encumbrance:** an obligation or reservation against a budget, such as an accepted purchase order. It is not necessarily an incurred cost or a payment.
- **Incurred cost:** the value of resources consumed or work received under the project's recognition policy. It may exist before an invoice is received or paid.
- **Invoice / payable:** a claim for payment that has been received and is awaiting validation or settlement. It is not itself proof of payment.
- **Payment:** an actual value movement recorded through the chosen execution environment. In a simulation, cash effects use the existing `SimulationLedger`.
- **Forecast:** the current expected total or future cash/resource requirement, based on remaining work and known risks.
- **Contingency:** an explicitly governed reserve for identified uncertainty. It is not free budget or an automatic cost overrun allowance.
- **Actual final cost:** the closed project's recognized cost after adjustments and reconciliation under the declared scenario policy.

A project's remaining budget, remaining funding, and remaining cash are different measurements. A project can be under budget but short of cash because funding arrives late; it can have cash available but no authority to spend it; it can have commitments that exceed its remaining authorization.

## 05 — Funding sources and capital structure

A funding plan is a set of typed sources, not a single total.

| Source | What it represents | Required distinctions |
|---|---|---|
| Sponsor / owner equity | Value contributed by the project's sponsor | Contribution, ownership rights, dilution and return terms are not implied |
| Internal allocation | Budget transferred from a parent organization or treasury | Allocation authority, restrictions, transfer event |
| Grant | Conditional or restricted contribution | Eligibility, purpose restrictions, milestones, reporting, repayment/clawback conditions if modeled |
| Donation / gift | Voluntary contribution | Restrictions, recognition, refund policy, and no implied investment return |
| Debt | Borrowed value with an obligation to repay | Principal, rate/index assumption, fees, maturity, schedule, collateral/recourse assumptions, covenants, default scenarios |
| Operating revenue | Income generated by the project or its outputs | Forecast versus realized receipts, operating costs, timing, uncertainty |
| In-kind contribution | Labor, land, materials, equipment, or services supplied without a cash transfer | Quantity, valuation method if any, restrictions, provenance; never fabricate a cash ledger posting |
| Public or pooled funding | A modeled allocation from a public or collective budget | Authority, eligibility, appropriation, use restrictions, reporting |

Do not label a funding commitment as cash. Do not treat debt proceeds as income or free issuance. A modeled loan must create a corresponding liability and a repayment obligation in the scenario's accounting model. The current cash ledger alone does not represent that liability.

## 06 — Budget structure and cost breakdown

A project budget should be decomposable and traceable. Typical cost groups include:

- land, site access, surveys, and permitting assumptions;
- design, engineering, research, and professional services;
- labor, wages, benefits, and training;
- materials, equipment, tools, and production;
- construction, installation, integration, and commissioning;
- utilities, transport, logistics, and temporary facilities;
- software, licenses, data, and communications;
- insurance and risk allowances as scenario inputs;
- financing costs, modeled interest, and fees;
- operations, maintenance, and lifecycle replacement;
- contingency, escalation, and management reserve;
- taxes and duties only where the scenario explicitly models them.

Each line should have a stable identity, parent work package, category, quantity/unit/rate assumptions where relevant, currency identity, estimate date, confidence, funding eligibility, and change history.

A cost breakdown structure describes what the project consumes. A work breakdown structure describes what work is delivered. They are related, but not interchangeable. A cost may support multiple work packages; one work package may consume many cost categories.

## 07 — Commitments, procurement, and change control

Project finance is not just recording cash leaving an account. The system must represent the obligation before payment.

A useful lifecycle is:

`Proposed → Reviewed → Authorized → Committed → Partially fulfilled → Incurred → Invoiced → Paid`

Not every transaction follows every state, and cancellation, dispute, rejection, credit, and reversal must be explicit alternatives. A purchase order is not an invoice; an invoice is not a payment; a payment is not proof that the purchased work was accepted.

Scope and price changes should be represented as change requests with:
- stable identity and project baseline version;
- reason, evidence, and affected work/cost/funding lines;
- requested cost and schedule impact;
- contingency impact;
- reviewer and decision authority;
- approved, rejected, withdrawn, or superseded outcome;
- effective date and immutable reference to the prior baseline.

Never silently overwrite the approved budget. Keep the original baseline and approved revisions so variance can be explained.

## 08 — Progress, cash flow, and forecasting

A useful project report should distinguish:
- budget at completion (BAC);
- planned value (PV): budgeted value of work scheduled by a point in time;
- earned value (EV): budgeted value of work actually completed under a declared measurement rule;
- actual cost (AC): recognized cost of work performed;
- estimate to complete (ETC);
- estimate at completion (EAC);
- schedule and cost variance;
- committed-but-not-yet-incurred obligations;
- invoices awaiting validation or payment;
- forecast cash inflows and outflows by period.

Earned-value metrics are only meaningful when scope, baseline, progress measurement, and cost recognition rules are explicit. A visually attractive progress percentage is not evidence of completed work. The system should allow non-earned-value forecasting methods when earned-value assumptions are unsuitable.

Cash-flow forecasts must preserve timing. A project can be solvent on a total-funding basis but fail a monthly cash requirement because grant reimbursement, customer payment, or sponsor allocation arrives later than invoices.

## 09 — Risk, uncertainty, and contingency

Estimate values should carry their assumptions and uncertainty rather than presenting false precision.

Candidate modeled risks:
- quantity growth and scope discovery;
- price escalation and supply shortages;
- labor availability and productivity;
- design changes and rework;
- permitting and external approvals;
- weather and site conditions;
- schedule delay and financing cost;
- supplier failure and replacement;
- operational performance below forecast;
- demand or revenue below forecast;
- currency or interest-rate changes only when the scenario explicitly includes them.

A deterministic first implementation should use declared scenarios (base, optimistic, adverse) and explicit assumptions. Probabilistic analysis can be a later capability with seeded, inspectable random sources. Never hide stochastic assumptions behind an unexplained number.

## 10 — Project finance versus the ledger

The `SimulationLedger` is the authority for simulated cash movements it records. It is not a project-management database, general ledger, loan register, cost-recognition engine, or full double-entry accounting system.

The intended boundary is:

- Project-finance records describe budgets, funding plans, commitments, work packages, cost events, invoices, forecasts, risks, approvals, and closeout.
- The existing simulation ledger records cash issuance, transfers, and retirement for typed fictional currency.
- A payment event links to a ledger receipt only when a corresponding simulated cash posting actually occurs.
- Commitments, estimates, accrued costs, in-kind contributions, equity interests, debt liabilities, and asset acceptance must not fabricate cash postings.
- Production resource consumption belongs to the separate production capability; project finance may reference its output/provenance without duplicating its inventory engine.
- The Experience/host owns identity, authorization, presentation, persistence, and operational lifecycle.

This boundary permits a project to have a budget and cost report without pretending that its money has moved.

## 11 — Composable project types

The same core vocabulary can support very different project families through Experience-owned policies:

| Project family | Example financial questions |
|---|---|
| Buildings and infrastructure | Land, design, construction packages, change orders, contingency, commissioning, lifecycle maintenance |
| Power and utilities | Generation capacity, grid connection, fuel/resource assumptions, outage reserve, operating costs |
| Manufacturing | Plant, tooling, production ramp, yield, inventory, unit cost, maintenance |
| Software and research | Labor, compute, equipment, grants, milestones, deliverables, uncertain outcomes |
| Creator ventures | Startup costs, equipment, materials, time, sales, platform contribution, cash runway |
| City services | Public allocation, staffing, procurement, service levels, maintenance, annual budgets |
| Events and entertainment | Venue, talent, production, ticket/revenue scenarios, cancellation exposure |
| Games and RPGs | Guild treasury, castle construction, shipyard, settlement upgrades, fictional debt and grants |

Economy owns reusable financial vocabulary and invariants. The Experience owns what a bridge, factory, city service, or guild project means.

## 12 — A first deterministic vertical slice

The first implementation should prove one complete, small project without trying to model every financing instrument.

Scenario: a fictional city funds and builds a community workshop.

1. Create a project with sponsor, scope, dates, and stable identity.
2. Define a cost breakdown with design, materials, labor, equipment, and contingency lines.
3. Create two funding sources: an unrestricted sponsor allocation and a restricted grant.
4. Record funding commitments separately from cash receipts.
5. Approve a baseline budget and a spending-authority policy supplied by the scenario.
6. Place a purchase commitment for materials and a labor package.
7. Record partial delivery and incurred cost.
8. Receive an invoice, validate it, and record a simulated payment through the existing `SimulationLedger`.
9. Approve a scope change; retain the original baseline and show the revised forecast.
10. Complete and accept the workshop; release unused commitments and close the project.
11. Produce a report reconciling authorized budget, sources, commitments, incurred cost, payments, remaining obligations, forecast, and final outcome.

All values are fictional and non-redeemable. The scenario is deterministic, replayable, and tested against the existing ledger. It does not provide real financing, credit assessment, tax accounting, construction certification, or professional project controls.

## 13 — Implementation sequence

1. **Vocabulary and invariants:** project identity, work/cost breakdown, budget baseline, funding source and commitment, approval, obligation, incurred cost, invoice, payment reference, forecast, and closeout.
2. **Project control model:** baseline versions, budget allocations, authorized changes, commitments, and budget availability calculations.
3. **Funding model:** source restrictions, committed/received/available states, and explicit liability semantics for simulated debt.
4. **Cost lifecycle:** delivery evidence, incurred cost, invoice validation, partial payment, credit, cancellation, and reversal.
5. **Ledger bridge:** link only actual simulated cash movements to existing ledger receipts; preserve idempotency and currency isolation.
6. **Forecast and reporting:** cash-flow periods, ETC/EAC, variance, remaining obligations, and scenario assumptions.
7. **Experience specimen:** community workshop or small infrastructure project with deterministic replay and an inspectable report.
8. **Independent validation:** test a creator venture and a manufacturing/capital project before stabilizing shared abstractions.

Do not add a large generic accounting framework before the vertical slice reveals what is genuinely reusable. Do not force every cost, commitment, or liability into a cash transfer.

## 14 — Explicit exclusions

This exploratory capability does not:
- connect to bank accounts or payment providers;
- accept deposits or withdraw real funds;
- issue securities, offer investments, or provide investment recommendations;
- determine creditworthiness or underwrite loans;
- provide legal, tax, accounting, audit, engineering, or construction certification;
- guarantee a project's return, schedule, completion, or profitability;
- represent fictional currency as redeemable or externally valued;
- replace professional project controls or accounting systems.

Any future real-world financial operation is a separate product and governance decision requiring qualified legal, accounting, security, regulatory, and operational review, plus explicit owner authorization.

## Further reading

- [Economy architecture](ARCHITECTURE.md) — domain and execution boundaries.
- [Markets and economic playgrounds](MARKETS_AND_PLAYGROUNDS.md) — businesses, markets, labor, and supply chains.
- [Deterministic market-economy scenario](MARKET_ECONOMY_SCENARIO.md) — current multi-institution simulation.
- [Experience Capability Catalog](EXPERIENCE_CAPABILITY_CATALOG.md) — optional capability composition.
- [SimulationLedger source](../src/TheSingularityWorkshop.Economy/SimulationLedger.cs) — current process-local fictional-currency ledger.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
