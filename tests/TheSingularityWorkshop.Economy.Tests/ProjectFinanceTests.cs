using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class ProjectFinanceTests
{
    private static readonly DateTimeOffset Time =
        new(2026, 11, 23, 12, 0, 0, TimeSpan.Zero);

    private static SimulatedCurrency Currency(string name = "Project Unit") =>
        new(SimulatedCurrencyId.New(), name, "PU", 2);

    private static SimulatedAmount Amount(decimal value, SimulatedCurrency currency) =>
        new(value, currency.Id);

    private static CapitalProject Project() =>
        new(CapitalProjectId.New(), EconomyEntityId.New(),
            "Community Workshop", "Build and commission a shared workshop.", Time);

    private static ProjectBudgetLine Line(SimulatedCurrency currency, string name = "Materials", decimal amount = 100m) =>
        new(ProjectCostLineId.New(), name, ProjectCostCategory.Materials,
            Amount(amount, currency), "Quantity 10 × assumed unit rate 10 PU.");

    [Fact]
    public void ProjectRequiresIdentitySponsorNameAndScope()
    {
        var project = Project();

        Assert.Equal(CapitalProjectStatus.Proposed, project.Status);
        Assert.Equal("Community Workshop", project.Name);
        Assert.Throws<ArgumentException>(() =>
            new CapitalProject(default, EconomyEntityId.New(), "Workshop", "Scope", Time));
        Assert.Throws<ArgumentException>(() =>
            new CapitalProject(CapitalProjectId.New(), default, "Workshop", "Scope", Time));
        Assert.Throws<ArgumentException>(() =>
            new CapitalProject(CapitalProjectId.New(), EconomyEntityId.New(), " ", "Scope", Time));
    }

    [Fact]
    public void BudgetBaselineCalculatesTotalFromImmutableLines()
    {
        var currency = Currency();
        var baseline = ProjectBudgetBaseline.Propose(
            Project().Id, 1, new[] { Line(currency, "Design", 25m), Line(currency, "Materials", 75m) });

        Assert.Equal(100m, baseline.Total);
        Assert.False(baseline.IsApproved);
        Assert.Equal(currency.Id, baseline.CurrencyId);
        Assert.True(baseline.Lines is IList<ProjectBudgetLine> { IsReadOnly: true });
    }

    [Fact]
    public void BudgetBaselineRejectsMixedCurrenciesAndDuplicateLineIds()
    {
        var projectId = Project().Id;
        var firstCurrency = Currency();
        var secondCurrency = Currency();

        Assert.Throws<InvalidOperationException>(() =>
            ProjectBudgetBaseline.Propose(projectId, 1,
                new[] { Line(firstCurrency), Line(secondCurrency) }));

        var duplicateId = ProjectCostLineId.New();
        var first = new ProjectBudgetLine(duplicateId, "One", ProjectCostCategory.Labor,
            Amount(10m, firstCurrency), "Estimate A");
        var second = new ProjectBudgetLine(duplicateId, "Two", ProjectCostCategory.Labor,
            Amount(20m, firstCurrency), "Estimate B");
        Assert.Throws<ArgumentException>(() =>
            ProjectBudgetBaseline.Propose(projectId, 1, new[] { first, second }));
    }

    [Fact]
    public void ApprovalCreatesANewSnapshotAndPreservesProposal()
    {
        var currency = Currency();
        var proposed = ProjectBudgetBaseline.Propose(Project().Id, 1, new[] { Line(currency) });
        var approvalReference = Guid.NewGuid();

        var approved = proposed.Approve(Time, approvalReference);

        Assert.False(proposed.IsApproved);
        Assert.True(approved.IsApproved);
        Assert.Equal(approvalReference, approved.ApprovalReference);
        Assert.Equal(proposed.Total, approved.Total);
        Assert.Throws<InvalidOperationException>(() => approved.Approve(Time, Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => proposed.Approve(Time, Guid.Empty));
    }

    [Fact]
    public void BudgetBaselineRequiresProjectVersionAndAtLeastOneLine()
    {
        var currency = Currency();
        Assert.Throws<ArgumentException>(() =>
            ProjectBudgetBaseline.Propose(default, 1, new[] { Line(currency) }));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProjectBudgetBaseline.Propose(Project().Id, 0, new[] { Line(currency) }));
        Assert.Throws<ArgumentException>(() =>
            ProjectBudgetBaseline.Propose(Project().Id, 1, Array.Empty<ProjectBudgetLine>()));
    }

    [Fact]
    public void FundingSourceRetainsItsTypeAndRestriction()
    {
        var currency = Currency();
        var source = new ProjectFundingSource(
            ProjectFundingSourceId.New(), "Regional Workshop Grant",
            ProjectFundingSourceKind.Grant, currency.Id, "Materials and accessibility improvements only.");

        Assert.True(source.IsRestricted);
        Assert.Equal(ProjectFundingSourceKind.Grant, source.Kind);
        Assert.Contains("accessibility", source.Restriction);
    }

    [Fact]
    public void FundingCommitmentIsNotAReceiptAndMustMatchSourceCurrency()
    {
        var project = Project();
        var currency = Currency();
        var source = new ProjectFundingSource(
            ProjectFundingSourceId.New(), "Sponsor allocation",
            ProjectFundingSourceKind.InternalAllocation, currency.Id);
        var commitment = new ProjectFundingCommitment(
            ProjectFundingCommitmentId.New(), project.Id, source,
            Amount(500m, currency), Time, Time.AddDays(30), isConditional: true);

        Assert.Equal(500m, commitment.Amount.Value);
        Assert.True(commitment.IsConditional);
        Assert.Throws<InvalidOperationException>(() =>
            new ProjectFundingCommitment(ProjectFundingCommitmentId.New(), project.Id, source,
                Amount(10m, Currency()), Time));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ProjectFundingCommitment(ProjectFundingCommitmentId.New(), project.Id, source,
                Amount(0m, currency), Time));
    }

    [Fact]
    public void CostRecordRequiresEvidenceAndDoesNotContainCashReceipt()
    {
        var project = Project();
        var currency = Currency();
        var line = Line(currency);

        var cost = new ProjectCostRecord(
            ProjectCostRecordId.New(), project.Id, line.Id,
            Amount(35m, currency), Time, "delivery-note:DN-104");

        Assert.Equal(35m, cost.Amount.Value);
        Assert.Equal(line.Id, cost.BudgetLineId);
        Assert.Equal("delivery-note:DN-104", cost.EvidenceReference);
        Assert.Throws<ArgumentException>(() =>
            new ProjectCostRecord(ProjectCostRecordId.New(), project.Id, line.Id,
                Amount(35m, currency), Time, " "));
    }

    [Fact]
    public void InvoiceIsAClaimAndItsStatusIsNotPayment()
    {
        var project = Project();
        var cost = new ProjectCostRecord(
            ProjectCostRecordId.New(), project.Id, ProjectCostLineId.New(),
            Amount(60m, Currency()), Time, "acceptance:stage-1");
        var invoice = new ProjectInvoice(
            ProjectInvoiceId.New(), project.Id, cost.Id, cost.Amount,
            "supplier-invoice:INV-44", Time);

        Assert.Equal(ProjectInvoiceStatus.Submitted, invoice.Status);
        Assert.Equal(60m, invoice.Amount.Value);
        Assert.Equal(cost.Id, invoice.CostRecordId);
    }

    [Fact]
    public void PaymentMustReferenceMatchingSimulatedTransferReceipt()
    {
        var project = Project();
        var currency = Currency();
        var amount = Amount(25m, currency);
        var receipt = new SimulationLedgerReceipt(
            Guid.NewGuid(), SimulationLedgerOperationKind.Transfer,
            Guid.NewGuid(), Guid.NewGuid(), amount, Time);
        var payment = new ProjectPayment(
            ProjectPaymentId.New(), project.Id, ProjectInvoiceId.New(),
            amount, Time, receipt);

        Assert.Equal(receipt.OperationId, payment.LedgerReceipt.OperationId);
        Assert.Equal(amount, payment.Amount);
        Assert.Throws<InvalidOperationException>(() =>
            new ProjectPayment(ProjectPaymentId.New(), project.Id, ProjectInvoiceId.New(),
                Amount(20m, currency), Time, receipt));
    }

    [Fact]
    public void PaymentRejectsIssuanceReceiptBecauseFundingIsNotSupplierPayment()
    {
        var project = Project();
        var currency = Currency();
        var amount = Amount(25m, currency);
        var issuance = new SimulationLedgerReceipt(
            Guid.NewGuid(), SimulationLedgerOperationKind.Issuance,
            null, Guid.NewGuid(), amount, Time);

        Assert.Throws<InvalidOperationException>(() =>
            new ProjectPayment(ProjectPaymentId.New(), project.Id, ProjectInvoiceId.New(),
                amount, Time, issuance));
    }

    [Fact]
    public void PaymentRequiresStableReferencesAndPositiveAmount()
    {
        var project = Project();
        var currency = Currency();
        var amount = Amount(25m, currency);
        var receipt = new SimulationLedgerReceipt(
            Guid.NewGuid(), SimulationLedgerOperationKind.Transfer,
            Guid.NewGuid(), Guid.NewGuid(), amount, Time);

        Assert.Throws<ArgumentException>(() =>
            new ProjectPayment(default, project.Id, ProjectInvoiceId.New(), amount, Time, receipt));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ProjectPayment(ProjectPaymentId.New(), project.Id, ProjectInvoiceId.New(),
                Amount(0m, currency), Time, receipt));
    }
}
