# Singularity City: Rival Laboratories

**Status:** game and experience design brief. This document records the proposed player experience; it does not claim that the game, security simulation, NPC systems, or multiplayer economy are implemented.

## The premise

Singularity City hosts competing, player-owned laboratories. Each player builds a scientific and commercial enterprise: lease or purchase lab space under the city's rules, equip facilities, recruit researchers, fund projects, publish discoveries, and compete for contracts, grants, prizes, and other fictional City revenue.

The tone is playful techno-espionage: *Spy vs. Spy* meets a laboratory tycoon, with The Singularity Workshop's simulation-first, composable-systems approach. Players can specialize in research, management, security, infiltration, counter-intelligence, or a mixture. Competition should create stories, not reduce every interaction to combat.

The game's money is fictional Singularity City currency. Balances, wages, contracts, research revenue, theft losses, insurance, and penalties are simulated economic events. They are not real money, investment returns, or promises of income.

## The central loop

1. **Establish a lab.** Select a location, footprint, lease/cost profile, permitted activity, and initial security tier.
2. **Equip it.** Add rooms, instruments, utilities, storage, workstations, access points, and safety systems.
3. **Staff it.** Recruit researchers and support staff with skills, wages, schedules, morale, trust, and professional goals.
4. **Choose research.** Run experiments with explicit inputs, consumed capabilities, costs, evidence, and outputs.
5. **Compete.** Bid for City work, publish discoveries, enter challenges, license inventions, or sell fictional products and services.
6. **Defend and investigate.** Detect suspicious activity, audit access, respond to incidents, improve security, and learn from attempted intrusions.
7. **Adapt.** Rival tactics and defensive upgrades evolve together. A more secure lab costs more to operate and can create friction for legitimate staff.

## Security is a progression, not a binary door

Security should be a layered, observable system. The first playable encounter can be intentionally simple:

- A lab has a basic mechanical door lock.
- An opponent approaches and initiates a fictional lock-picking challenge.
- The challenge is a game interaction with abstract timing, pattern, tool-quality, and skill variables—not a real-world lock-picking tutorial.
- Success may open the door, trigger a noise/trace, consume time, or leave evidence. Failure may alert a resident, damage the fictional tool, or increase suspicion.
- The owner can inspect the event, improve the lock, change patrols, or set a different access policy.

Later tiers add distinct checks and counterplay:

| Security layer | Defensive gameplay | Attacker gameplay | Evidence and counterplay |
|---|---|---|---|
| Mechanical access | Lock quality, door condition, access schedule | Abstract lock challenge, social entry, alternate authorized routes | Door events, tamper traces, witnesses |
| Staff identity | Roles, credentials, visitor sponsorship | Impersonation as a fictional social challenge; stolen in-game credentials create risk | Credential use history, identity mismatch, staff reports |
| Gesture verification | Gesture provider, liveness/confidence policy, fallback | Learn or exploit the game's modeled gesture challenge, not real biometric bypass techniques | Confidence score, rejected attempts, fallback events |
| Retina / palm verification | Fictional biometric sensors and configurable thresholds | Choose between riskier approaches, reconnaissance, or another objective | Sensor observations, false rejects, tamper alerts |
| Cryptographic challenge | Rotating challenge seed and device-bound fictional credential | Compete against freshness, access windows, and detection pressure | Challenge IDs, replay rejection, key rotation and audit trail |
| Human and operational security | Staff training, compartmentalized access, patrols, decoys | Recruit an insider, create a distraction, or exploit a narrative opportunity | Conflicting reports, unusual schedules, provenance gaps |

The advanced wetware concept is science-fiction game progression: a character may receive a fictional surgically implanted cryptographic seed generator, bound to their in-game identity and paired with palm/retina checks. It is not a recommendation for real biometric implants or a design for real authentication hardware. In-world biometric data should be simulated traits or generated tokens, never real player biometric collection.

Security must preserve legitimate play. Researchers need access to their work; owners need emergency recovery; guests and contractors need scoped access; and players must have ways to detect, investigate, contest, and recover from incidents. A maximum-security lab that cannot function is not a winning strategy.

## Espionage, sabotage, and character takedowns

Espionage is an information game as much as an access game. Possible fictional objectives include learning a rival's research direction, discovering facility weaknesses, copying a simulated research artifact, recruiting an insider, planting a decoy, disrupting a scheduled experiment, or exposing misconduct.

Sabotage should be represented as bounded, reversible simulation events with explicit consequences:
- introduce a fault or delay into a modeled instrument;
- contaminate a fictional sample or corrupt a simulated dataset;
- disable a non-critical service temporarily;
- misdirect staff with a decoy event;
- steal or damage an in-game item;
- trigger an investigation, insurance claim, repair cost, reputation penalty, or retaliation risk.

The most severe spy-fiction branch may include an **assassination/takedown** objective, but keep it abstract and non-graphic: a character is temporarily removed from an operation, captured, incapacitated, or forced into recovery/respawn under the scenario's rules. Do not turn it into real-world violence instruction. Make the outcome legible, bounded, and recoverable; define non-lethal alternatives and explicit consent/match settings for player-versus-player modes.

Every hostile action needs:
- an authored objective and valid target;
- a game-state transition and cost/risk;
- a detection/evidence model;
- an owner response and recovery path;
- a fair-play policy, cooldowns, and anti-griefing protections;
- a clear distinction between in-game actions and real-world conduct.

## Researchers and laboratory management

Researchers should be more than production multipliers. Model a staff member as a city character with a skill profile, interests, availability, employment agreement, compensation, workplace needs, trust, and relationships. The lab owner assigns work and access scopes; the character's agency and schedule determine whether they can actually perform it.

Possible management decisions:
- recruit, interview, hire, train, promote, or dismiss;
- define wages, bonuses, research budgets, shifts, and leave;
- assign a principal investigator, team, instrument, or experiment;
- grant least-privilege room, instrument, data, and project access;
- improve working conditions, safety, and retention;
- investigate anomalies without treating every staff member as suspicious.

Researchers can discover, report, ignore, or be deceived by events according to their traits and context. Keep personnel records and security evidence distinct; suspicion is not proof.

## The lab's operating economy

A laboratory is a continuing economic commitment. Model recurring costs and opportunity costs rather than a one-time purchase:
- lease or ownership costs, utilities, maintenance, and repairs;
- staff wages, recruitment, training, and retention;
- instruments, consumables, storage, compute, and waste handling;
- research budgets, project milestones, and failed experiments;
- security installation, monitoring, false alarms, recovery, and insurance;
- revenue from City contracts, grants, licensing, services, discoveries, and competitions.

The City can allocate public contracts and research prizes through transparent criteria: quality, reproducibility, usefulness, delivery, safety, and declared competition rules. Players can pursue different business strategies; the best lab is not necessarily the lab with the most expensive equipment.

## Workshop systems and ownership boundaries

This experience should compose existing Workshop domains rather than invent a parallel stack.

| Concern | Owning system / domain |
|---|---|
| Fictional balances, wages, leases, budgets, contracts, rewards, losses | Economy |
| Experiment definitions, scientific configuration, Physics consumption, run provenance | Singularity Laboratories |
| Physical laws, equations, material and environmental models | Physics |
| Gesture interpretation, posture, temporal gesture evidence | Gestures |
| Character identity, agency, schedules, relationships, staff behavior | City / character systems |
| MicroBundle composition and actual provider identity | FSM_COS |
| Execution lifecycle, cancellation, run events | FSM_Execution |
| Authentication policy enforcement and secrets | Trusted host / security composition |
| 2D/3D world and dashboard presentation | Renderer / Visualize / Experience |

A dashboard on each experiment should show both declared dependencies and host-observed runtime providers. A security experiment can therefore show its gesture-recognition provider, identity challenge provider, and any Physics capabilities it consumes without pretending those are the same category. Runtime observations must be associated with a concrete run and exact provider/version identity.

The first mechanical-lock scenario can be a small, self-contained security experiment. Later variants can add gesture verification, fictional biometric checks, cryptographic freshness challenges, staff authorization, and environmental context. Each variant is a different experiment with its own declared capabilities, settings, costs, and evidence.

## Suggested playable milestones

### Milestone 1 — one lab, one door
- One player-owned lab, one rival, one entry point, one abstract mechanical-lock challenge.
- Record attempts, elapsed game time, alert state, access result, and evidence.
- Owner can review the event and purchase a better lock.
- Tests cover success, failure, repeated attempts, alerts, and recovery.

### Milestone 2 — staffed facility
- Add a small research team, role-based room access, shifts, wages, and staff reports.
- Test that legitimate staff can work, access is scoped, and employment costs reconcile in the fictional ledger.

### Milestone 3 — first espionage contracts
- Add a declared objective such as discovering a rival's project category or retrieving a simulated artifact.
- Add detection, counter-intelligence, decoys, evidence review, and fair-play limits.
- Keep sabotage bounded and recoverable.

### Milestone 4 — gesture and identity layers
- Add gesture-provider capability declarations and host-observed provider execution.
- Add a gesture settings panel with required inputs, confidence policy, fallback behavior, provider version, and per-run observations.
- Use simulated biometric and cryptographic challenge state; do not collect real biometrics.

### Milestone 5 — rival lab economy
- Add multiple owned labs, leases, recruitment markets, research contracts, maintenance, insurance, and City awards.
- Test economic reconciliation, fair competition, and viable strategies for low-budget players.

### Milestone 6 — advanced spy-fiction scenarios
- Add insider recruitment, instrument sabotage, data theft, counter-intelligence, and optional abstract character takedown scenarios.
- Provide private/co-op/competitive modes, opt-in PvP settings, cooldowns, and recovery rules.

## Design invariants

1. **Fictional economy only:** City currency and laboratory revenue are simulation values, not real money or investment returns.
2. **No real security bypass tutorial:** lock, biometric, gesture, and cryptographic challenges are abstract game mechanics.
3. **Privacy by design:** no real biometric capture; use synthetic identities, tokens, and simulated signals.
4. **Evidence over accusation:** suspicion and detection scores are not proof; preserve event provenance and allow review.
5. **Defensible fairness:** security has cost and usability tradeoffs; attackers have risk and counterplay; owners have recovery.
6. **Bounded hostility:** sabotage and takedown are authored, reversible game-state transitions with anti-griefing policy.
7. **Composable experiments:** each experiment declares what it consumes; the host reports what actually resolved, loaded, and executed.
8. **Implementation honesty:** this is a design brief until source, tests, and a playable vertical slice prove each capability.

## First vertical-slice recommendation

Build the first door encounter as a deterministic experiment in Singularity Laboratories, but keep player ownership, lab economics, character agency, and city competition in their owning systems. The experiment settings dashboard should display the security capability declarations and actual provider observations for the run. Begin with the abstract mechanical-lock challenge; then add one gesture-provider variant as a separate, obviously different experiment. Do not jump directly to wetware biometrics—the progression is the game.
