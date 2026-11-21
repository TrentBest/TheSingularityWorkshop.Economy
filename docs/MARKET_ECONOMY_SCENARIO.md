# Deterministic Market Economy Scenario

**Maturity: source-implemented; automated CI verification pending.** This is the first small, closed, deterministic scenario for the multi-institution market-economy direction. It is a fictional simulation, not a financial service.

## What runs

`MarketEconomyScenario.Run()` creates a typed fictional `Market Simulation Unit` currency and six account roles:

- Central-bank treasury
- Commercial-bank clearing
- Commercial-bank reserves
- Employer treasury
- Household deposit
- Merchant treasury

The scenario applies seven stable, ordered ledger operations:

1. Issue 10,000 units to the central-bank treasury.
2. Capitalize the employer with 500 units.
3. Endow the household with 100 units.
4. Move a 40-unit wage payment from the employer to commercial-bank clearing.
5. Settle the 40-unit clearing balance into the household deposit account.
6. Post a 2-unit bank fee from the employer to commercial-bank reserves.
7. Settle a 25-unit household purchase to the merchant.

All timestamps, identities, operation IDs, and ordering are fixed so independent runs produce the same event and receipt sequence. The balances are read back from `SimulationLedger`; the scenario does not maintain parallel UI counters.

## Expected final account balances

| Account role | Balance |
|---|---:|
| Central-bank treasury | 9,400 |
| Commercial-bank clearing | 0 |
| Commercial-bank reserves | 2 |
| Employer treasury | 458 |
| Household deposit | 115 |
| Merchant treasury | 25 |
| **Total** | **10,000** |

The total remains equal to the explicitly issued supply. The wage is deliberately represented as two separate operations so clearing acceptance and final household credit are not conflated.

## What this proves—and what it does not

The scenario exercises existing typed simulated currency, account ownership, ledger issuance, transfer, derived balances, and receipt linkage. Its test suite checks deterministic replay across fresh runs, value conservation, expected balances, unique operation identities, and the separate wage-clearing/settlement steps.

It does **not** implement:
- a real central bank, commercial bank, deposit guarantee, credit underwriting, or lending;
- deposit creation, balance-sheet accounting, monetary policy, or fractional-reserve mechanics;
- securities, an order book, price discovery, market liquidity, or investment returns;
- durable ledger storage, distributed transactions, multi-host execution, or live financial integration;
- real money, redemption, deposits, withdrawals, or external settlement.

The bank in this first slice is a fictional settlement intermediary with clearing and reserve accounts. The next economic steps must model assets, liabilities, ownership, custody, lending, and market exchange explicitly rather than treating a ledger transfer as proof of those concepts.

## Ownership boundary

Economy owns the economic actors, currency, accounts, policy, events, and ledger semantics. ServerApp will host independent participant roles and coordinate messages in a separate simulation fabric. The fabric must not decide whether an economic operation is valid or settled.

Source: [MarketEconomyScenario.cs](../src/TheSingularityWorkshop.Economy/MarketEconomyScenario.cs). Tests: [MarketEconomyScenarioTests.cs](../tests/TheSingularityWorkshop.Economy.Tests/MarketEconomyScenarioTests.cs). Tracking: [Economy issue #30](https://github.com/TrentBest/TheSingularityWorkshop.Economy/issues/30) and [ServerApp issue #15](https://github.com/TrentBest/TheSingularityWorkshop.ServerApp/issues/15).

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
