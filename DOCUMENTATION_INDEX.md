# Economy documentation index

The README is the public front door. These guides have different jobs; use the one that matches your question.

| If you want to… | Read |
|---|---|
| Understand the concept without starting from code | [What is Economy?](docs/WHAT_IS_ECONOMY.md) |
| Plan the Singularity City simulation, Digiten-scale measurements, reusable/reskinnable creator shops, adoption, and a first vertical slice | [Simulated economies and creator commerce](docs/SIMULATED_ECONOMIES_AND_CREATOR_COMMERCE.md) |
| Understand the long-range banking, credit, barter, corporations, securities exchanges, casinos, economic education, scale hierarchy, and WebPage integration plan | [Economy systems roadmap and WebPage handoff](docs/ECONOMY_SYSTEMS_ROADMAP_AND_WEBPAGE_HANDOFF.md) |
| See what exists now versus the intended architecture | [Economy architecture](docs/ARCHITECTURE.md) |
| Try the shortest source-checked shop example | [README: See it in a minute](README.md#-04-see-it-in-a-minute) |
| Inspect domain types | [Commerce](src/TheSingularityWorkshop.Economy/Commerce.cs), [EconomyShop](src/TheSingularityWorkshop.Economy/EconomyShop.cs), [Money](src/TheSingularityWorkshop.Economy/Money.cs), [Transfer](src/TheSingularityWorkshop.Economy/Transfer.cs), [Accounts](src/TheSingularityWorkshop.Economy/EconomyAccount.cs), [Fees](src/TheSingularityWorkshop.Economy/FeePolicy.cs), [Provider contract](src/TheSingularityWorkshop.Economy/FinancialProvider.cs), [Execution observation](src/TheSingularityWorkshop.Economy/EconomicExecutionResult.cs) |
| Inspect access/audit vocabulary | [Access ledger contracts](src/TheSingularityWorkshop.Economy/AccessLedger.cs) |
| Run tests | [Test project](tests/TheSingularityWorkshop.Economy.Tests/TheSingularityWorkshop.Economy.Tests.csproj) and [workflow](.github/workflows/dotnet-tests.yml) |

## Status and authority

- Source and tests are authoritative for implemented behavior.
- The architecture guide distinguishes current code from proposed components.
- The long-range systems roadmap is architecture direction and a WebPage LLM handoff; its future capabilities are not claims of current implementation.
- A source-checked example is not the same as a separately built or run example.
- This package does not execute real payments or certify legal, tax, security, or compliance readiness.
- Publishing or enabling any operational financial capability remains a separate, explicitly authorized activity.
