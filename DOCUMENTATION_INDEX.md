# Economy documentation index

The README is the public front door. These guides have different jobs; use the one that matches your question.

| If you want to… | Read |
|---|---|
| Understand the concept without starting from code | [What is Economy?](docs/WHAT_IS_ECONOMY.md) |
| See what exists now versus the intended architecture | [Economy architecture](ECONOMY_ARCHITECTURE.md) |
| Try the shortest source-checked shop example | [README: See it in a minute](README.md#-04-see-it-in-a-minute) |
| Inspect domain types | [Commerce](Commerce.cs), [EconomyShop](EconomyShop.cs), [Money](Money.cs), [Transfer](Transfer.cs), [Accounts](EconomyAccount.cs), [Fees](FeePolicy.cs), [Provider contract](FinancialProvider.cs), [Execution observation](EconomicExecutionResult.cs) |
| Inspect access/audit vocabulary | [Access ledger contracts](AccessLedger.cs) |
| Run tests | [Test project](tests/TheSingularityWorkshop.Economy.Tests/TheSingularityWorkshop.Economy.Tests.csproj) and [workflow](.github/workflows/dotnet-tests.yml) |

## Status and authority

- Source and tests are authoritative for implemented behavior.
- The architecture guide distinguishes current code from proposed components.
- A source-checked example is not the same as a separately built or run example.
- This package does not execute real payments or certify legal, tax, security, or compliance readiness.
- Publishing or enabling any operational financial capability remains a separate, explicitly authorized activity.
