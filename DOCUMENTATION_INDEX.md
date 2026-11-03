# Economy documentation index

The README is the public front door. These guides have different jobs; use the one that matches your question.

| If you want to… | Read |
|---|---|
| Understand the concept without starting from code | [What is Economy?](docs/WHAT_IS_ECONOMY.md) |
| Build and run a first shop example | [Getting started](docs/GETTING_STARTED.md) |
| Study the economic concepts in a modular textbook path, including sports prediction, teams, leagues, tournaments, and organized play | [Economy theory index](docs/THEORY/README.md), [prediction markets chapter](docs/THEORY/02-PREDICTION-MARKETS-AND-WAGERING.md), and [teams/tournaments chapter](docs/THEORY/03-TEAMS-LEAGUES-AND-TOURNAMENTS.md) |
| See a source-backed example and its limits | [Gallery](docs/GALLERY.md) |
| Explore the casino, simulated stock exchange, business, auction, work, and supply-chain playground roadmap | [Markets and economic playgrounds](docs/MARKETS_AND_PLAYGROUNDS.md) |
| Define Singularity currency, casino chips, buy-in/cash-out, house accounting, and the staged casino vertical slice | [Singularity currency and casino contract](docs/SINGULARITY_CURRENCY_AND_CASINO.md) |
| Explore casino back-of-house encounters, simulated security cameras, courtesy bankroll, free city transit and paid driver roles, and multiplayer building construction | [Exploratory Economy Experiences](docs/EXPLORATORY_ECONOMY_EXPERIENCES.md) |
| Plan the Singularity City simulation, Digiten-scale measurements, reusable/reskinnable creator shops, adoption, and a first vertical slice | [Simulated economies and creator commerce](docs/SIMULATED_ECONOMIES_AND_CREATOR_COMMERCE.md) |
| Inspect the first test-owned proof of one canonical shop reused across two Experience placements with different skins | [Creator shop composition proof](docs/CREATOR_SHOP_COMPOSITION_PROOF.md) and [its tests](tests/TheSingularityWorkshop.Economy.Tests/CreatorShopCompositionTests.cs) |
| See what exists now versus the intended architecture | [Economy architecture](docs/ARCHITECTURE.md) |
| Design reusable RPG weapon, magic, consumable, faction, and player-market stores | [Game store patterns](docs/GAME_STORE_PATTERNS.md) |
| Try the shortest source-checked shop example | [README: See it in a minute](README.md#-04-see-it-in-a-minute) |
| Inspect domain types | [Commerce](src/TheSingularityWorkshop.Economy/Commerce.cs), [EconomyShop](src/TheSingularityWorkshop.Economy/EconomyShop.cs), [Money](src/TheSingularityWorkshop.Economy/Money.cs), [Transfer](src/TheSingularityWorkshop.Economy/Transfer.cs), [Accounts](src/TheSingularityWorkshop.Economy/EconomyAccount.cs), [Fees](src/TheSingularityWorkshop.Economy/FeePolicy.cs), [Provider contract](src/TheSingularityWorkshop.Economy/FinancialProvider.cs), [Execution observation](src/TheSingularityWorkshop.Economy/EconomicExecutionResult.cs) |
| Inspect access/audit vocabulary | [Access ledger contracts](src/TheSingularityWorkshop.Economy/AccessLedger.cs) |
| Inspect typed fictional currency and simulated custody | [Simulation ledger](src/TheSingularityWorkshop.Economy/SimulationLedger.cs) and [ledger invariant tests](tests/TheSingularityWorkshop.Economy.Tests/SimulationLedgerTests.cs) |
| Understand the implemented team, competitor, and competition lifecycle primitives | [Competition/team UML](docs/UML/CompetitionUML.md), [single-elimination bracket UML](docs/UML/TournamentBracketUML.md), and [tests](tests/TheSingularityWorkshop.Economy.Tests/CompetitionTests.cs) |\n| Browse public API type coverage | [API reference index](docs/API/README.md) |
| Browse UML and evidence coverage | [UML inventory](docs/UML/README.md) |
| Read the ledger API and structural neighborhood | [SimulationLedger API](docs/API/SimulationLedger.md) and [SimulationLedger UML](docs/UML/SimulationLedgerUML.md) |
| Run tests and verify package contents | [Test project](tests/TheSingularityWorkshop.Economy.Tests/TheSingularityWorkshop.Economy.Tests.csproj) and [CI workflow](.github/workflows/dotnet-tests.yml) |
| Build the Pages site artifact without deployment | [Pages preview workflow](.github/workflows/pages-build.yml) |

## Status and authority

- Source and tests are authoritative for implemented behavior.
- The architecture guide distinguishes current code from proposed components.
- The creator shop composition fixture is consumer-owned test code; it proves a boundary contract, not a production editor, renderer, persistence service, or WebPage integration.
- A source-checked example is not the same as a separately built or run example.
- This package does not execute real payments or certify legal, tax, security, or compliance readiness.
- Publishing or enabling any operational financial capability remains a separate, explicitly authorized activity.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
