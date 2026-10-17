# Exploratory Economy Experiences: Casino, Transit, and Construction

**Status: experience brief and staged design.** These are proposed playable systems, not claims that the game loops, simulation ledger, surveillance views, or multiplayer construction service already exist. Economy defines reusable economic contracts; the city and each Experience own their world state, gameplay, policy, and presentation.

## Experience principle: reward curiosity with a real response

Singularity City should invite players to investigate systems instead of presenting a static menu of features. A player may try an unusual action, encounter a believable response, learn how the world works, and choose what to do next. Exploration is not automatically criminal, and a playful narrative branch should not be mistaken for real-world security advice.

Each exploration should have:
- a visible affordance and a legible consequence;
- a state transition the simulation can record and test;
- an exit or recovery path, not a dead end;
- a reward or explanation appropriate to the chosen path;
- clear boundaries between fictional simulation state and real-world services.

## 1. Casino back-of-house exploration

### The "Employees Only" door

The casino floor is the public-facing venue; the cashier cage, security office, service corridors, camera wall, and operations rooms are separate explorable areas. A player who chooses an **Employees Only** door enters a designed encounter rather than simply being blocked by an invisible wall.

A first scenario can branch into two narrative outcomes:

1. **Honest retreat.** Casino security approaches, explains that the area is staff-only, and gives the player a graceful response: "My bad. I felt bold, now I don't. I'm out, if that's okay." The player leaves; the event is resolved without forced escalation or an unexplained punishment.
2. **Declared security exercise.** The player presents a fictional business card identifying them as a member of "Security Infiltration Technology Specialists" and reveals that the encounter was an authorized test. The security team verifies the scenario's in-world authorization and, when the test's conditions are met, grants a recorded visitor clearance. The player can then inspect designated security systems and learn how the venue responds.

The second branch should be an authored simulation scenario, not a universal bypass where saying a phrase grants unrestricted access. The world can reward the player with full **simulation** access to the casino's back-of-house learning spaces after the scenario's explicit success conditions are satisfied. Separate access tiers can still protect staff-only interactions that are not part of the exercise.

### Security camera wall: 2D plan view, persistent 3D

The default presentation is a readable top-down, Dragon-Warrior-style city view. Interacting with the bank of security monitors opens a focused observation mode:
- the player selects a camera, cycles through feeds, and can inspect the venue's current simulated camera views;
- each feed is a view of the same underlying 3D scene and camera transform, not a disconnected painted image;
- in a 2D presentation, the feed can be rendered into a monitor surface or framed viewport while the surrounding world remains top-down;
- when the player enables 3D, the monitor, its screen, camera perspective, and surrounding objects retain their spatial relationship rather than being replaced by a separate flat interface;
- camera IDs, coverage, feed state, timestamps, and recorded events belong to the simulation domain; the renderer owns pixels and camera presentation.

This means the camera system must eventually consume real scene geometry and camera state from the shared world/renderer. Do not claim that footage is available until the scene, camera transforms, render-to-texture or equivalent capture path, and UI presentation are connected and tested. Begin with a deterministic simulated feed from the current scene, then add recording and playback as a separate capability.

## 2. Courtesy bankroll and casino play

The casino is a sandbox for play, experimentation, and learning—not a financial product. On entering the cashier cage, the player is reminded that their participation is courtesy of The Singularity Workshop and receives **1,000,000 units of fictional Singularity Denomination** to play with. If that balance is exhausted, the player may request another simulated grant. Make the grant event explicit in the history, and show the courtesy reminder each time the player visits the cage; never disguise grants as winnings or wages.

- The grant is a simulation-only issuance event, with a reason and unique operation identity.
- Chips are held by the casino or the participant, not both. Buy-in, wager, resolution, payout, and return of unused chips each have explicit state transitions.
- "Cash out" converts chips back into the participant's **in-world** Singularity Denomination balance under the venue's rules. It is not a withdrawal, payment, or real-money redemption.
- A simulated bank account may receive an in-world transfer of Singularity Denomination. It is an account inside the fictional city simulation, not a connection to an actual bank account.
- The UI must never imply that these balances can be transferred to a real bank, exchanged for fiat, or used to claim real-world value. Any future legally reviewed digital-currency project is separate, opt-in, and cannot silently convert this simulation balance.
- Casino games disclose their rules, wager, outcome, and balance change. Include free non-gambling activities and never force the player to gamble to continue exploring the city.

## 3. A city that carries its passengers for free

Ordinary player transportation is free to use: taxis, buses, trains, boats, and future city transport can take a player to destinations without charging them a fare. The transport simulation still tracks routes, schedules, vehicle capacity, service availability, time, and operational costs where useful to the world model.

Players may also choose transportation jobs:
- **Taxi driver:** pick up Digitens, navigate to their requested destinations, and deliver them safely and on time. This is a science-fiction city evolution of the arcade feel of *Crazy Taxi*, but the core loop is about driving, route choices, passenger needs, and city exploration.
- **Bus driver:** operate a route, stop for passengers, maintain a timetable, and manage capacity.
- **Train conductor/operator:** serve stations, follow schedules and dispatch instructions, and handle passenger flow.
- **Boat captain:** navigate waterways, docks, weather or traffic conditions where modeled, and passenger destinations.

Work pays the player in fictional Singularity Denomination for simulated time, completed tasks, or defined service quality. The compensation formula must be visible and testable; tips, bonuses, penalties, and expenses are explicit optional rules rather than arbitrary hidden rewards. A player's ability to travel as a passenger must not depend on accepting a job. Jobs are one path through the city, not a gate that forces everyone to work.

Transit vehicles and passenger requests should be domain entities with stable IDs. Driving controls and camera views belong to the Experience; route planning, job rules, and economic events should remain reusable. This supports the same transit capability in other worlds without coupling Economy to one WebPage or renderer.

## 4. Buildings arrive quickly; construction remains playable

The city may reveal a newly requested building immediately as a high-level planning event, but the building should also have a construction experience of its own. This is a multiplayer architectural game driven by a client brief, not a fake progress bar that pretends to construct a building.

A client brief can define:
- intended use, capacity, location, access and circulation needs;
- rooms or functional zones, required doors and windows, accessibility, and service connections;
- style, materials, budget expressed in fictional units if enabled, schedule, and constraints;
- acceptance tests that players can inspect before submitting a design.

Players collaborate to create the building:
1. establish a footprint and spatial grid;
2. place and edit walls, doors, windows, floors, stairs, and other supported elements;
3. validate connectivity, access, dimensions, and brief requirements;
4. invite teammates to contribute to separate areas or work on the same model with conflict-aware edits;
5. submit a version for client review, receive specific feedback, revise, and earn simulated compensation or reputation under declared rules.

The city may use a placeholder or planning mass while the detailed build is in progress. It must label that state honestly. On acceptance, the approved design becomes the canonical building definition consumed by the city; the construction game must not create an unrelated decorative copy.

### Composition and authority

- **Construction brief/provider:** requirements, constraints, review criteria, and acceptance.
- **Building model:** stable element IDs, geometry/relationships, versions, ownership and provenance.
- **Collaborative editing:** participant identity, edit operations, conflict handling, version history and recovery.
- **Validation:** deterministic checks for required spaces, doors/windows, connectivity, dimensions, and other declared constraints.
- **Economy:** optional simulated budget, contracts, milestones, wages, material costs, and rewards. A brief can be completed without monetary gameplay if the scenario disables it.
- **City/Experience:** site, surrounding context, visualization, controls, and the accepted building's placement in the world.

A rapid arrival animation is only the presentation of a state change. The authoritative model, design version, and acceptance event determine what has actually been built.

## 5. Implementation order and acceptance tests

Keep this as several composable Experiences instead of a single mega-system.

### Slice A — authored casino exploration
- Add explicit scenario states for public floor, unauthorized-area encounter, retreat, authorization verification, and granted exercise access.
- Test both narrative branches, exit/recovery, repeat visits, and that access is granted only by the configured exercise rule.
- Add an observation-station contract with stable camera IDs and a deterministic mock feed before claiming live 3D capture.

### Slice B — casino courtesy ledger
- Record the 1,000,000-unit courtesy grant with unique identity and clear provenance.
- Test repeated cage visits, exhausted balance and renewed grant, chip custody, duplicate-operation rejection, and exact chip-to-in-world-balance reconciliation.
- Keep all simulated bank transfers within the same fictional currency identity.

### Slice C — free transit and paid work
- Test free passenger travel separately from driver employment.
- Record route, shift/task, completion, simulated time and pay; reject duplicate completion payouts.
- Exercise taxi, bus, train and boat as distinct vehicle/role configurations over reusable route and passenger contracts.

### Slice D — multiplayer construction
- Deliver a small brief and support walls, doors, windows, versioned edits, teammate contributions, validation, submission, and client feedback.
- Test collaboration conflicts, invalid geometry/requirements, version rollback, accepted-design provenance, and optional economy events.
- Only promote the accepted building model into the city; label any interim placeholder as under construction.

## Non-negotiable boundaries

1. **Simulation only:** fictional balances and simulated jobs are not real money, income, a financial account, or redeemable value.
2. **No forced labor loop:** travel is free; employment is an optional way to play and earn in-world units.
3. **No forced gambling:** casino grants are a play mechanic, not a real financial incentive, and the city has other activities.
4. **No false implementation claims:** a concept document, placeholder, local counter, or mock camera is not an authoritative ledger, working camera capture, multiplayer editor, or accepted building.
5. **Dependency direction:** Economy remains provider-neutral and does not depend on the city UI, WebPage, or renderer. Experiences compose economic and simulation capabilities from above.
6. **Provenance:** grants, wages, wagers, transfers, construction edits, reviews, and accepted building versions must be explainable and testable.
