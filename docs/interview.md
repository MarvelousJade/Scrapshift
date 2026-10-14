# Interview preparation

Use these notes to understand the project. First-person answers must match the contribution record and your actual hands-on work.

## Latest art/animation example

The user identified that presentation lacked character animation, gun firing motion and varied material shapes visible in the screenshots. The project now separates motion state, weapon aiming/firing, and pickup motion. That separation lets pause stop gameplay animation while keeping menu behavior independent, and lets reduced motion suppress bounce without removing the pickup itself.

For a concrete debugging explanation, study ART-001: a generated atlas did not follow its promised grid exactly, so one weapon's slice included a neighboring fragment. The fix changed the import boundary, not gameplay coordinates, and was checked in the actual player. QA-004 is a useful testing discussion: an inherited user preference suppressed the exact effect the fixture was meant to observe. The fix isolated fixture state without overwriting the user's preference.

For AssetScope: point to serialized frame arrays on `ScavengerAnimated.prefab` and sprite references on `ReferenceCatalog.asset`. Six texture atlases support many sprite subassets; testing that the scene has no historical reference-PNG dependency catches incomplete migration. This is a concrete creator-workflow example rather than a claim about Unity's internal tooling.

Explain that Codex generated the art/code and ran these checks under your direction. Reproduce and understand the example before describing it as your own debugging experience.

The user's stride feedback is an especially useful example of why a green test suite is insufficient: six observed frame indices do not establish a convincing gait. ART-002/003 distinguish a frame-slot count from pose quality and movement direction. The battery correction likewise came from actual interpretation of the visual symbol, not a failing arithmetic test. Discuss how these observations changed the assets and acceptance criteria.

## Why build this?

"I wanted a small Unity game that I could finish and use alongside my AssetScope Editor tool. Arena survival gave me a clear gameplay loop and real prefab, material and configuration dependencies. It connects the experience of making a game with the problem of helping creators inspect their projects."

## Why keep it small?

One arena and a bounded content catalog allow attention to responsiveness, readability, testable behavior and a usable build. Adding more enemies or maps before the loop is reliable would dilute that focus.

## How does it relate to the role?

Discuss C# state transitions, Unity assets and serialization, Editor-only package separation, reproducible validation, and how an actual project reveals authoring problems. Do not claim the game implements Unity's internal multi-context authoring or Graph Toolkit.

## What was difficult? How did you fix a bug?

Three actual investigations are in `bugs.md`: an invalid package dependency blocked import; the runtime smoke check falsely treated a screenshot request as visual success; and code inspection identified an aspect-ratio mismatch between camera and HUD. Use the one you have personally rehearsed and understand.

The strongest testing example is QA-002. A report said Passed, but the player log showed screenshot capture failure. A second run produced a black image. The lesson is that a passing process or a file on disk is weaker evidence than the actual requested output. The repair separates gameplay counters from rendered inspection and requires actual screenshot evidence. Initial invalid reports are retained for provenance rather than quietly relabeled as success.

If asked for a personal debugging story before you reproduce it yourself: "The project encountered this failure during development. The automated investigation found X; I can explain the reproduction and fix." Do not say you personally performed steps recorded as assistant-run checks.

## How did you test it?

24 EditMode cases passed: rules, collision geometry, content/AssetScope integration and aspect-ratio handling. The Windows player built with zero reported build warnings/errors. Real title/gameplay/pause rendering and keyboard/mouse controls were inspected using desktop automation. Initial hidden screenshot attempts failed visual validation and are recorded separately. See `verification.md` for the final exact evidence and remaining gaps. Automated coverage does not establish that the game is balanced or fun.

## What would you improve next?

Rebinding/controller support, player feedback-driven balance, performance measurements before pooling or architectural expansion, and broader integration checks against later AssetScope versions.

## Hands-on rehearsal checklist

- Play a full run and describe one concrete balance or readability issue.
- Trace a weapon definition to its effect prefab and shared material.
- Explain how pause and upgrades stop simulation without corrupting a wave.
- Read one real regression test and predict what happens if its fix is removed.
- Make a small tuning change, explain it, test it and commit it.
- Practice a two-minute demonstration: game -> asset workflow -> evidence.

## How would you explain the architecture?

"RunRules owns the progression state without scene references. ArenaGame reads input and coordinates combat and presentation. Enemy and weapon definitions are Unity assets referencing prefabs and shared materials. The Editor assembly contains content generation and AssetScope integration; the runtime assembly does not reference UnityEditor."

Point to `architecture.md`. Explain the limitations too: the coordinator is larger than ideal and would be split before expanding the roster.

## How do you prevent bugs around pause and upgrades?

Advance simulation only in Playing state. A level-up and a wave-clear intermission use the same upgrade screen but have different continuation behavior. Tests verify that an in-wave upgrade preserves remaining time and only a wave intermission advances the wave. Invalid or duplicate choice events cannot apply another upgrade after the state returns to Playing.

## What combat problem did you anticipate?

Fast projectiles can move from one side of an enemy to the other between frames. Checking only their current position can miss the collision. The game measures distance to the whole movement segment instead. Tests include a target between endpoints, a target beyond the segment, and a stationary segment. This is a preventative design and tested edge case, not a claim of an observed tunneling incident.

## Why ScriptableObjects?

They let you tune enemy and weapon values as assets rather than scattering constants through combat code. Their prefab and material references are also inspectable in AssetScope. The current small game still has some positional weapon assumptions; expanding it should introduce named references and broader configuration validation.

## How did you decide what to cut?

The original brief emphasized small, polished, Unity experience and AssetScope compatibility. Multiplayer, a large roster, a shop economy and procedural worlds would add work without directly proving that goal. Three recognizable weapons, three enemy types and a complete short loop are enough to show the authoring/testing workflow.

## How would you optimize it?

The first measured combat pass is documented in [performance optimization and interview Q&A](performance-optimization.md). Across two controlled Windows player runs, average simulation CPU cost fell from 17.48 to 5.36 ms at 100 enemies and from 55.66 to 12.66 ms at 200. The changes index immutable collision data, remove transient LINQ work from frequent set queries, and compute process fixture detection once. All 361 EditMode tests passed, and seeded combat aggregates matched.

Explain the limits when quoting those numbers: this is headless simulation timing, excluding rendering and UI, not an FPS claim. Allocation counters were unavailable in the release player and are explicitly marked invalid. The linked notes cover complexity, GC, caching and invalidation, floating-point parity, mean versus p95, pooling, spatial partitioning, profiling tools, and branch isolation. Reproduce the benchmark and review the code before presenting assistant-run work as your own hands-on investigation.

## How did references influence the game?

Use `art-references.md` and `combat-references.md`. Discuss movement-centered combat and readable choices from Brotato, compact mechanical silhouettes from Into the Breach, and abstract weapon effects from Nova Drift. Public community mod source informed questions about hit processing, bounded history and effective stats. No claim is made to possess the complete official Brotato source.

## How does this demonstrate attention to users?

The controls and objective are visible before play. Combat stops while reading upgrade choices. A health bar and clear timer communicate status. There are sound and reduced-motion controls. The camera and HUD share an aspect ratio so resizing cannot hide reachable parts of the arena. These are design decisions; usability-study success is not claimed.

## How did you use coding tools?

Answer honestly if asked. The user's direction covered concept, scope, references and documentation; implementation and initial automated investigation in this session used a coding assistant. Explain the code you understand, the outputs you reviewed and the hands-on changes you actually made. Do not equate directing generation with independently typing every line. The role explicitly welcomes AI tools, so clear verification and ownership boundaries are useful discussion points.

## Tell me about collaboration or user feedback

This is currently a directed portfolio project, not evidence of a professional team or user study. Use another verified experience for a teamwork question unless you have a real collaboration example here. Record the first playtest feedback in `contributions.md`, then connect the observed issue to a concrete change and verification.

## Two-minute demo order

1. Start a shift and explain movement, auto-fire and scrap collection.
2. Pause; show the options and explain why simulation is stopped.
3. In Unity, inspect a weapon definition and trace its effect prefab/material in AssetScope.
4. Show one rule test and the real bug log, explaining what each proves and does not prove.
5. End with one specific improvement based on your own playtest.

## How did you respond to negative feedback?

The user explicitly said the original art, UI and upgrade presentation were poor and that the combat lacked effects. The revision used the supplied local reference to identify specific missing signals: character silhouettes, hit confirmation, threat telegraphs and readable stat choices. It then replaced the presentation, expanded feedback, added randomized tiers/rerolls, and rechecked the build and actual screens. This demonstrates iteration from concrete critique; user approval of the redesign is still pending.

The latest suite has 27 tests. DEV-004 is an additional real debugging example: native Unity object construction during deserialization, caught by scene tests despite a nominally successful build. Explain both the lifecycle fix and the stricter build gate. See journal and verification for the newest evidence; earlier examples above describe the first baseline.

## How did you use your Editor tool in a real project?

The game now embeds AssetScope 0.3.0. It scans 72 actual game assets and 1,202 reference usages with saved-scene/property analysis enabled. A regression test locates the Fire_01 audio clip reference in the sound bank, navigates to its source and verifies the selected bank. This links creator tooling to an actual authoring task: find which sound-bank entry uses a clip and go to the owning asset to change it. It is stronger evidence than only showing synthetic demo assets, but remains a small project, not a large-project benchmark.

## Why change the audio architecture?

The user reported that the original sound was poor. Four sine tones did not distinguish enough gameplay events. The revision separates authored cue data from playback, uses multiple original variants, and limits concurrent sounds while reserving voices for important feedback. Explain the distinction between numerical/sample-playback verification and subjective listening; the latter still needs personal evaluation. AUDIO-006 records the actual Unity importer API failure and fix.

## Reference rebuild — current interview examples

These replace the earlier five-wave/three-weapon scope descriptions above.

**Why did the project grow?** The user specifically rejected the simple enemy and upgrade model and asked for a source-informed reconstruction. The response was to analyze wave resources and behavior scripts first, commit the study, then implement the encounter schedule and separate XP/reward/shop states. The bounded catalog keeps the expansion explainable: six weapon families, 24 items, eight stat upgrades, base difficulty only.

**What was a difficult bug?** REF-001: the initial scheduler's shared random sequence depended on frame size because it processed all missed repeats of each group together. Processing chronological seconds fixed the ordering. The regression compares all 20 waves at 60 Hz against a single large step with the same seed. Explain why seeded randomness alone does not ensure determinism.

**How did you test beyond unit tests?** State tests verify atomic purchases, six-slot limits, duplicate combinations, paid rerolls and preserved locks. Actual player captures caught clipped upgrade titles and white birth warnings that the rules tests could not catch. A hidden-window capture failed and was explicitly rejected. The rendered fixture grants health/equipment for coverage and is not evidence of a fair normal run.

**How did you avoid misleading portfolio claims?** Record the supplied reference paths/hashes, distinguish third-party sprites/audio from original work, and explain that Codex implemented/tested the revision under Shaoyu's direction. Describe personal code understanding and hands-on changes only after actually doing them. No fabricated production incident, performance improvement or user approval.

**How does AssetScope help here?** Inspect ReferenceCatalog, follow an enemy sprite or weapon audio reference, and navigate back to the serialized record that uses it. This demonstrates the completed tool against the real game catalog. Use the latest scan totals in verification rather than the older 72-asset baseline.


### Enemy and spawn iteration

**How did you use a reference without guessing?** The team compared the supplied Godot scheduling/placement code and attack animation timings with the Unity implementation, then wrote an evidence-to-change document before editing. The user retained twenty waves and asked for fewer items and a closer UI. Assistant-generated implementation and automated checks are identified in contributions; this is not a claim of personally handwritten code.

**A concrete bug example:** a safety retry moved a tightly grouped enemy anywhere in the arena. That met a distance check but violated the formation's purpose. The fix keeps the group's square or edge constraint during every retry and chooses the safest candidate within it when full clearance is impossible. Tests cover both ordinary and impossible requests; visual review covers what those placements look like.

**What tradeoff did you make?** Explicit attack states and locked direction make a windup readable and dodgeable. Shooter aim lock, a short recovery, and visible direction lines are deliberate adaptations for the compact arena, not claims of exact commercial-game parity. Twenty schedules remain, while eight offered items keep the shop easier to understand.


## Source-parity revision — current answers supersede earlier adaptations

**How did you decide what to change?** Compare the supplied source's scene nodes and runtime methods with the Unity behavior, write a discrepancy table, then implement and test the differences. A visual resemblance or passing tests against our previous implementation did not establish parity. See `gameplay-parity-audit.md`.

**What was the hardest bug to reason about?** A flat scene importer silently merged root and phase-two boss properties. The fix made attack profiles explicit per node/state. Explain how a data-shape error became a gameplay error, and why a compiler or missing-reference scan would not detect it.

**What did tests catch or prove?** Exact XP and rarity boundaries, charge direction commitment and cooldown, paid non-repeating upgrade choices, separate spawn lane order, the 119/120 backlog boundary, and locked-price persistence. A separate accelerated Windows campaign verifies all twenty intermissions and the boss phase transition. The fixture is invulnerable and automatically shops; it does not prove fairness or fun.

**What remains a tradeoff?** Keeping original art, a compact arena and a reduced catalog makes a manageable portfolio project, but also means this is not an exact Brotato clone. Character passives, additional weapon sets, all stats/items and endless mode are absent; Gun, Precise and Primitive were added in the next pass. The source-based rules are documented separately from our adaptations.

**Who did what?** Shaoyu requested the direction, supplied the reference, retained original art, requested incremental commits/documentation and opened the project in Unity. Codex performed source comparison, implementation and automated validation in this pass. Before interviewing, personally trace these code paths and play the project; do not claim these were personally handwritten changes or production incidents.

## Weapon-set discussion example

**Why derive the bonus instead of storing it?** The loadout has at most six slots, so recalculating its contribution keeps one source of truth and avoids stale bonuses after buying, combining or recycling. Permanent item/level stats remain separate. A higher-tier weapon still counts once; two duplicates count twice. The active threshold is a total, not a sum of all earlier thresholds.

**What did verification teach you?** One new test failed because advancing the wave also earned a Harvesting level and +1 HP. The test was corrected to isolate the combine delta. The actual rendered fixture then showed two spears giving +3 Max HP and combining them lowering HP from 30 to 27. Explain the difference between a faulty test assumption and a demonstrated game defect.

**How do you make a rule understandable?** Show weapon membership, equipped count, current effect and all five thresholds together. Inactive sets explicitly say they need two weapons. Unit tests verify the arithmetic; rendered inspection checks readability. These are Codex-authored changes directed by the user; personally trace and play them before using this example in an interview.

## Loot-loop example — state ordering and honest evidence

**How did you prioritize missing features?** The comparison found a missing everyday decision: pursue a loot enemy, collect its crate, then keep the item or recycle it. That changes the run more directly than adding many catalog entries. Survival formulas also changed how existing builds behaved, so those were corrected in the same iteration.

**What integration problem did you solve?** A crate collected during end-of-wave cleanup must enter the queue before choosing the next state. Clearing pickups or opening the shop first loses that ordering. Trace `FinishWavePickups`, `Tick`, `OpenRewards` and `ResolveCrate`: collect, resolve crates, resolve levels, then shop. The targeted fixture leaves a crate offscreen to exercise the cleanup path.

**How did you validate it?** 116 rule/integration tests, a real rendered take/recycle fixture and a current twenty-wave accelerated run. Distinguish those from balance: the separate normal-speed smoke died in wave one. Its old Passed label only measured combat/capture coverage; reporting the actual loss is more accurate than claiming a successful run.

**What remains?** Character/starter selection, broader stats/items, weapon-specific behavior and normal-run balancing. The crate pool is intentionally small. These are assistant-authored changes directed by the user, not personally handwritten production incidents; personally trace and play the implementation before discussing it as experience.

## Simplifying animation after user feedback

**Why simplify the art?** The user wanted springy body movement rather than visible limb cycles. Inspecting the reference showed separate body/feet sprites animated with position and scale. One body plus tiny foot instances removes inconsistent generated stride poses, keeps equipment independent and makes tuning straightforward.

**What did visual review catch that tests missed?** The first gallery showed a clipped enemy nose and a fragment on its neighbor. The imported grid was valid data but wrong for those pixels. Per-sprite alpha bounds corrected it; a border test then caught remaining tree contamination in another sprite. Explain both the visual observation and the actual failed regression before the final 120/120 pass.

**What remains subjective?** Tests establish timing, bounds and state behavior; they do not establish that the spring feels right. The user still needs to play it. Codex generated/implemented these changes under the user's direction; describe that contribution accurately.


## Modular appearance example: separation of data and presentation

Problem: a character's eyes and equipment were baked into one image; inventory icons pasted onto it could not cleanly change identity. Decision: use one blank body with data-driven eye, mouth, hair and item layers, following the inspected source's slot/priority model. Three cosmetic looks demonstrate reuse while keeping stats separate. Both runtime and shop share the resolver to avoid inconsistent previews.

Concrete bug example: the old wardrobe used item count as its refresh condition. A same-count replacement would keep stale art. The cache now compares item identity/order and preset identity; a regression replaces a hat without changing count and verifies the displayed sprite. Sprite border tests also caught two actual atlas cuts (126/127 initial passes); measured transparent separators resolved them. A generated enemy draft with a translucent backdrop was rejected after alpha inspection.

Tradeoff: explicit offsets suit this small catalog, while a larger roster may justify standardized authoring canvases and an appearance editor. Cosmetic variants currently have no character-specific passives. Use this as a documented project example; Codex authored the implementation/checks, and candidate explanation or manual validation should be described truthfully.


## Visual scaling and transform ownership

**How was a visual complaint investigated?** Compare actual reference sprite bounds and scene transforms before guessing scale. The first-wave enemy had an extra .72 multiplier, giving roughly half the player's width. A separate presentation policy makes the new player/enemy ratio explicit while leaving collision balance for a deliberate follow-up.

**Why were shadows separate objects?** A child renderer inherits the actor's squash and rotation. A world-space oval can follow the ground position without distorting. An owner component handles activation and deletion; serialized mesh/material dependencies remain visible to AssetScope. A new test first failed for an Editor-created owner's disable lifecycle; enabling Editor lifecycle callbacks fixed that case. Tests and staged screenshots support these implementation claims, not player acceptance or full-run balance. Codex authored this work under the user's direction.


## Diagnosing a unit-system mismatch

A useful example from the live-reference pass: correct-looking individual sprites still produced the wrong game feel because source movement and weapon ranges used 100 pixels per Unity unit, while arena dimensions and image sizes were chosen independently. Comparing the running game with its source revealed the mismatch. Shared world geometry, measured sprite dimensions and a bounded follow camera address traversal, scale and spawn density together. Automated tests check map edges and camera bounds; staged gameplay checks actual rendering with one and six weapons. See live-reference-comparison.md for facts and limits. Codex implemented and verified this work under user direction; reproduce the reasoning before presenting it as personal engineering experience.


## Debugging combat feel through separate contracts

Use combat-feel-comparison.md as the evidence trail. A single enemy radius had been reused for crowd spacing, bullet hits and outgoing contact, despite the reference defining separate shapes. Knockback also crossed an inconsistent conversion boundary (/10 in import, then *3 at runtime). Explicit profiles and force rules made both problems testable. Travel-ordered swept collisions protect piercing damage order regardless of enemy-list order; an actual runtime fixture verifies100 damage to the near target and50 to the far one. Frame-rate comparisons check push at20/30/60/120Hz. Explain why this does not prove human feel or exact Godot physics parity. Codex authored/verified these changes under user direction; rehearse and reproduce the reasoning before making personal implementation claims.


A second concrete debugging story: hidden Unity runs completed simulation but could not capture screenshots. Existing checks counted capture requests and reported Passed. Comparing log failures, file timestamps and old art in a stale screenshot exposed the false rendering claim. Fresh-file/signature checks and a missing/invalid/stale regression now separate gameplay assertions from rendering evidence. The final reruns passed; normal-speed scripted survival still failed and is reported separately.


## Presentation comparison and validation example

Study the stale-scene incident in yard-and-run-presentation.md. A migration marker described work that had happened earlier, but synchronization had replaced the actual scene. The runtime fixture checked animation state and fresh screenshots, yet initially missed the absent canopy. Inspection caught it; the fix checked the real serialized references, guarded migration skipping with those checks and verified the instantiated object at runtime. Discuss the difference between a completed-step marker and evidence that the current output is correct.

The visual comparison also illustrates scope control: source-grounded mounts/timings, original art, a small supported selection catalog, and explicit adaptations such as a timer held during arrival. Do not describe this as a full reproduction of Brotato or a completed human playthrough. Shaoyu directed the changes; Codex implemented, debugged and verified them. Reproduce and understand the example before claiming personal debugging ownership.


## Separating presentation data from gameplay offers

The stat display reused the upgrade-offer list as its icon registry. That worked for eight upgradeable stats but left other visible stats blank. A complete presentation lookup now gives every visible primary stat a serialized icon, while upgrade drafting remains a separate concern. Explain how the AssetScope scan and asset-completeness test cover references without claiming safe deletion or runtime completeness.

Weapon recoil supplied a second example: matching the numerical cooldown alone did not match cadence because the reference suspends cooldown during its shooting tween. The fix accounts for animation duration, attack-speed scaling and frame-boundary overshoot, with tests plus rendered six-weapon combat. Codex performed implementation; study and reproduce the reasoning before presenting it as personal debugging work.

## Further debugging evidence: hit audio and mirrored previews

See [player-hit-and-leg-correction.md](player-hit-and-leg-correction.md) for a concrete wrong-asset-mapping diagnosis, a GUI transform-order bug found visually, regression choices, evidence limits and collaborator attribution. Use the documented facts when preparing an answer; do not describe agent-run checks as personally executed tests.

## Data boundaries and verifying the actual display

The four-character pass offers two further examples: replacing cosmetic-only choices with shared gameplay/preview rules, and fixing a recursive Unity serialization schema by separating leaf projectile stats from weapons with effects. A requested ultrawide capture was actually clamped to 1920x1080, so the report records the real dimensions rather than claiming a test that did not occur. See fullscreen-and-starter-characters.md for source mapping, real failures and attribution. Do not present agent-run checks as personally executed work.


## Inspection UI: data filtering, coordinate spaces and evidence quality

See [inspection-and-weapon-selection.md](inspection-and-weapon-selection.md). Useful questions include why eligibility should be filtered before rendering/Random selection, why nested scrolling breaks naive hit testing, how current-value descriptions avoid stale UI, and why a fresh screenshot can still capture the wrong logical state. The document records actual failures and tests rather than inventing a debugging story. Codex performed the implementation/checks; study and reproduce the reasoning before claiming personal execution.

## Catalog and unlock integration discussion

Use full-base-implementation.md, bugs.md and verification.md as evidence. Explain why catalog identity, display alias, availability and runtime behavior are separate. Imported rows alone do not implement event-driven implants: companions, death chains, next-wave effects and progression need runtime hooks. Unlock persistence needs idempotence and recovery; fixtures must not alter the player's save.

Concrete debugging example: visual weapon-edge lines led to alpha-border regression checks, exposing overlapping atlas cells. Shared clipping geometry fixes both world/UI rendering without repainting art. Tests then caught zero-sized sprites caused by Unity Rect serialization; explicit data-transfer fields fixed the schema. Scroll inspection exposed a separate coordinate-space mismatch.

The user supplied direction, screenshots, play feedback and defect reports; Codex performed the recorded implementation/source comparisons/automated checks. Study and reproduce the reasoning before claiming personal execution. Do not claim frame-perfect parity, a human-balanced campaign or 50 bespoke portraits.


## Evidence-backed comparison: final boss attack bug

A visible difference led me to inspect the source animation rather than only compare damage numbers. Our stationary enemy shots were ordinary three-second damage orbs; the reference pillar enables its hitbox for just 0.14 seconds after a warning. I separated presentation lifetime from collision timing, corrected the source diameter/square-scatter semantics, and wrote boundary tests plus a runtime check that verifies health stays unchanged during warning and drops during the active phase. I also followed a speed-reset function into its implementation: its argument was a percentage modifier, not a speed. This prevented a plausible but incorrect port. The source study and our own procedural art are explicitly distinguished in presentation-and-final-bosses.md.

## October 13 interview example - investigating a melee miss

Shaoyu observed that moving during a melee attack could cause a miss. The useful debugging step was to separate a legitimate collision miss from an extra aim-lock mismatch. In the supplied extracted game's `weapons/weapon.gd`, ordinary attacks begin while stationary, but an auto-aimed weapon continues turning toward a live target during its swing. `melee_weapon_shooting_behavior.gd` activates a moving hitbox only for part of that swing. Scrapshift already had a timed hitbox, but its attack angle stayed fixed from the first frame.

Trace `UpdateEquipment` to `BeginMeleeAttack` and `TickMeleeAttack` in `ReferenceWeaponBehavior.cs`. The fix recalculates the auto-aim angle from the current mount and target while retaining the initial accuracy spread; manual aim remains locked. Explain why a target can still evade the physical hitbox. The focused angle regression, 331 passing EditMode tests, zero-warning build and 205 passing stationary weapon checks support the change, but they do not constitute a human moving-target playtest. See `bugs.md` (October 13 auto melee entry) and commit `137bbd4`.

Shaoyu supplied the observation and direction; Codex made the source comparison, code change and automated checks. Reproduce the scenario and understand the code before presenting it as personally performed debugging. The same day's item-pool and XP-label fixes are recorded in `bugs.md`; the cursor change was a requested feature, documented in `cursor-parity.md`, rather than a bug incident.

## October 13 interview example - tracing a missing death effect

Shaoyu noticed that enemy deaths lacked the original game's red burst. The source trace goes from `EffectsManager._on_unit_took_damage` to `hit_particles.tscn`: blood is a damage event, so the killing blow triggers it, while `entity.tscn` separately animates the corpse. That distinction prevented adding a second, oversized death-only explosion. Scrapshift already had the corpse motion but used generic sparks for hit feedback. The fix imports the original particle sprite and adds its red, half-second burst for enemy hits, excluding trees as the source does. The Windows build and staged built-player capture passed; see `bugs.md` and the enemy-death capture for limits. Shaoyu identified the mismatch; Codex implemented and checked the change. Reproduce the source trace before presenting it as personal debugging experience.
## October 13 interview example - rocket explosion parity

Shaoyu noticed that a rocket impact might not match the supplied original game. Tracing `rocket_launcher_effect.tres` to `explosion.tscn` and `player_explosion.gd` showed that the 0.05-second animation key removes the flash and hitbox but also calls `emit_smoke`; the explosion itself returns to its pool at 1 second. Scrapshift already showed the flash and played the source clips, but destroyed the effect at 0.05 seconds and never emitted smoke. The 3-pixel hitbox offset was also unscaled.

The fix ports the original smoke sprite and authored count, emission area, tint, speed, spin, fade, and lifetime into a Unity particle system. A first built-player fixture exposed a floating-point boundary at 0.01 + 0.04 seconds, so the 0.05-second trigger uses a small comparison tolerance. The final Direct3D 11 build and visible fixture passed collision, flash, smoke, and cleanup checks. Use `rocket-explosion-parity.md` and the captured images as evidence. Describe the work as source-value and sequence parity; do not claim identical random particle placement or pixel-level rendering between Godot and Unity. Shaoyu provided the observation and direction; Codex performed the source trace, implementation, and checks.
## October 13 interview example - tracing explosion and piercing effects

Shaoyu asked whether the rocket correction also covered other explosions and recalled an item firing a shot on enemy death. The useful source trace separated three paths: weapon explosion traits, item/structure/pet explosion effects, and Baby With A Beard's projectile spawn in main.gd. The earlier rocket-only smoke import left eighteen other exploding weapon tiers and item effects with generic or missing smoke. Rip and Tear also references a separate flesh explosion. RunData.handle_explode_effect sums stacked item chances and damage into one roll and one effect, while player_projectile.gd gives bounce priority and consumes piercing after a hit.

The fix imports source smoke counts and item presentation, shares the timed explosion visual across those paths, aggregates stacked item explosions, and resolves Baby's undeclared projectile scene to the source default bullet. A built-player fixture verifies 21 explosive tiers, source smoke counts, red flesh smoke, the bullet scene, and 100/50 damage through two targets. The rocket and starter/deployable reviews also passed. See explosion-and-piercing-parity.md and bugs.md for evidence and limits. Shaoyu identified the question; Codex traced, implemented, and checked it. Reproduce the source reasoning before claiming personal debugging experience.
## October 13 interview example - extra shots and post-hit damage

Shaoyu asked whether projectile count, bounce, and piercing reductions matched the supplied game. The source trace ran through WeaponService._set_common_ranged_stats, RangedWeaponShootingBehavior.shoot, player_projectile.gd, the integer Hitbox.damage field, and EntitySpawner.get_rand_enemy. That revealed a shared +0.1-radian spread per extra shot, the zero-base-projectile exception, bounce priority, target-pool limits, and integer truncation after damage loss.

Scrapshift already handled much of the sequence, but turrets and ranged pets missed the extra-shot rules, bounce redirected from the frame endpoint and could select bosses or trees, and fractional post-hit damage survived. The fix and evidence are in projectile-rules-parity.md and bugs.md. The built-player check confirms three shots with +2 projectiles, an 11-to-5 bounce, and an 11-to-7 pierce with Pumpkin; nine focused EditMode tests, the Windows build, and starter and companion reviews passed. Shaoyu prompted the audit; Codex traced and checked the implementation. Reproduce the reasoning before describing it as personally performed debugging.

## October 13 interview example - measuring crowded combat performance

The performance work isolated repeated collision-profile searches, weapon-set LINQ queries, and command-line argument copies in the combat loop. A seeded standalone benchmark measured lower CPU time after replacing those repeated operations, while matching the recorded shot, health, and position checksums. See `performance-optimization.md` for the benchmark setup, raw reports, regression tests, and limits. The measurements cover simulation CPU time for the tested workload, not rendered frame rate or every gameplay state. Shaoyu directed the work; Codex implemented and measured it. Reproduce the benchmark and trace the code before presenting it as personal experience.

## October 13 interview example - fixing a misleading showcase

The Auto Turret demo deployed all turrets and pets immediately, then placed them in hardcoded rows. The source `EntitySpawner` instead queues helpers from its one-second timer and releases one structure and one pet every three physics frames. The showcase now queues the owned helpers and releases them gradually through their normal random placement. The loadouts also use actual catalog items for attack speed, Engineering, critical hits, piercing, and bounce. The user corrected an initial assumption about extra-projectile items: Seashell exists in DLC, but the supplied base-game source does not contain it, and this build adds no synthetic projectile stat. See `showcase-loadouts.md` and `bugs.md`. Codex implemented and tested the changes under Shaoyu's direction; reproduce the timed spawn and item-stat checks before describing them as personal debugging work.

## October 13 interview example - matching the wave countdown warning

The final five seconds lacked the source game's red timer and audible clock. Reading `wave_timer.gd` and `main.gd` revealed a one-second tick that begins just below six seconds, changes the label to the configured negative color, and plays a specific WAV at ±0.1 pitch variation. The Unity version now detects displayed-second crossings, uses the byte-identical source WAV, and keeps the warning cue audible in combat without changing the rest of the mix. The build, 364 EditMode tests, and 22-cue built-player audio check passed. See `wave-countdown-parity.md` and `bugs.md`. This is a documented project fix under Shaoyu's direction; reproduce it before presenting it as personal debugging experience.

## October 13 interview example - tracing unlock audio on defeat

Shaoyu asked whether the source game shows unlocks after a lost run and then asked for the matching sound. The source trace found that `main.gd` completes Rookie on death, `ChallengeService` emits `challenge_completed`, and `challenge_completed_ui.gd` plays `Shield 4.mp3` at -5 dB with ±0.2 pitch when the popup appears, followed by a distinct whoosh on hide. Scrapshift had reused its install cue during GUI repaint. The fix moved sound playback to the challenge completion and queue transition events, imported both clips with matching hashes, and left the later reward review card silent. See `unlock-sound-parity.md` and `bugs.md` for source paths and verification. Shaoyu identified the mismatch; Codex traced and implemented it. Reproduce the source trace before claiming personal debugging work.

The final integration also resolved duplicate editor builder methods caused by overlapping branches and preserved the newer showcase and enemy behavior while combining the appearance catalog. The merged version passed 387 EditMode tests; its Windows build had zero errors or warnings, and a built-player mixer check heard all 26 cues. The loss summary inventory was narrowed to leave room for Progress and unlock review. These are merge and UI fixes to describe only after reproducing the work and its limits.
