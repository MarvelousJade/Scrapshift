# Full-run combat review â€” October 17, 2026


## Current validation — R23 full run completed

R23 is the fresh normal Windows review build at `Builds/GameplayParity20261017R23/LaunchGame.cmd`. Its build summary records success, zero errors/warnings and 401,680,286 bytes. Its saved complete-suite summary records **1,031/1,031 Passed, zero failures/skips, exit 0**, duration 58.5371699 seconds. The R23 native navigation report records `passed=true` (17 snapshots), and the native final-settlement report records Passed for both timer and delayed-boss routes. Navigation used virtual Gamepad events and fixture-only focus isolation; it is not a new physical-controller verification. These native reports and the suite summary were read directly.

The fresh R23 normal-clock campaign under `artifacts/full-verification-2026-10-17/unity-attempt-4` completed **Passed / Won, all 20 waves, 19 shops and native process exit 0**. Its actual runner selects the R23 executable with `--campaign-check` and `--campaign-realtime`, without the one-wave preflight flag. The final result confirms `realtime=true`, `preflight=false`, `pickupPhysicsRate=60` and fresh required captures. The trace contains 20 timer ends and finishes `finished / Won`. Process UTC start was `2026-10-18T02:06:41.8179839Z`; finish was `2026-10-18T02:24:59.9887827Z` (both October 17 in Toronto). Reported fixture wall duration was 1,093.956 seconds. Per-wave active-versus-wall analysis belongs to the consolidated comparison, rather than subtracting incompatible original/Unity total clocks.

| R23 recorded combat/progression counter | Actual value |
| --- | ---: |
| Charge releases | 1,222 |
| Enemy ranged attack releases | 1,451 |
| Boss phase changes | 2 |
| Peak enemies | 129 |
| Enemy kills / combined kills | 4,554 / 4,604 |
| Purchases / chosen upgrades / crates taken | 37 / 19 / 9 |
| Final materials / level / item copies | 1,336 / 20 / 45 |

The 16 recorded enemy IDs are `baby_alien`, `bruiser`, `buffer`, `charger`, `chaser`, `fin_alien`, `helmet_alien`, `horned_bruiser`, `horned_charger`, `invoker`, `junkie`, `looter`, `pursuer`, `spawner`, `spitter` and `tree`. R21 encountered Predator instead of Invoker; R23 exercised the other boss family. This is broader combined execution evidence, not deterministic equality between the random runs or proof of every boss attack. Two phase changes do not prove a boss kill.

### R23 native capture and log review

The parent reviewed fresh wave 1/5/10/15/20 captures. This combat reviewer additionally opened `wave-15.png`, `wave-20.png` and `end.png`. All three are 1280×720. Wave 15 shows a dense mixed crowd, hostile bullets, damage/critical/impact feedback, drops, spawn markers, player equipment and support actors. Wave 20 visibly shows the Invoker health bar, red circular warnings, hostile bullets, enemy crowd, friendly impacts and support actors. `end.png` shows the Drifter unlock modal plus a bottom-left Survivor achievement popup over the darkened final scene. It is not an exposed Victory screen; the runtime phase/result establishes Won.

No new combat defect was confirmed from those images or the final counters. R23 `player.log` was searched for `Exception`, `NullReference`, `ERROR`, `Error`, `Warning` and `warning`; no matches were found. Sparse images and aggregate counters still do not establish continuous timing, sound, exact trajectories or universal gameplay parity. Historical failed/interrupted attempts and the original's one pause intervention remain preserved. No competing native player or Unity process was launched by this reviewer during the campaign.

### Allocation fixes and what was measured

After the completed R21 run exposed a wall-time gap, a separate Development-player investigation measured repeated allocations in weapon-set calculations, immutable launch-argument classification and collision-profile lookups inside crowd loops. The corrections use direct loops, cache only process-constant classification, and index immutable geometry rows. They do not cache mutable player stats or alter the eight-step clock cap. See the [allocation investigation](late-wave-allocation-investigation-2026-10-17.md) and `artifacts/late-wave-profile-2026-10-17/final-comparison.json`.

Across the short synthetic marked captures, enemy allocation fell from 13,417,151 to 70,409 bytes per simulation tick; all main-thread allocation markers fell from 18,256,944 to 133,567 bytes per tick. Denominators were 24, 32 and 115 recorded simulation ticks. These are diagnostic Development-player results, with profiler overhead and evolving synthetic world states. They do not establish ordinary-game FPS; the completed R23 campaign supplies separate normal-player wall-time evidence. The Mono per-thread allocation API was unavailable; earlier zero-byte microbenchmark fields are invalid, while raw-profiler allocation markers remain the evidence. A capture overlapping a Unity build is excluded from timing comparisons. R23's completed fresh normal run checks the actual integrated campaign after these changes, with the same scripted/invulnerable scope limits.

### Explicit limits of this campaign's gameplay coverage

These are boundaries of the recorded fixture, not newly discovered gameplay defects:

- It uses neutral character effects, Danger 0 and six fixed tier-II weapons. It does not play a full campaign with every character, weapon family, difficulty or item combination. Catalog/source audits and focused tests are separate evidence for those cases.
- It refreshes invulnerability before combat steps. Survival balance, a lethal hit/defeat sequence and recovery after taking ordinary damage are not established by this campaign, even when damage rules have focused tests.
- Its intermission driver takes every crate, chooses the first upgrade and buys affordable eligible offers. It does not execute alternative crate disposal, reroll/lock/recycle policies or every special-shop effect during a complete run; focused shop/native checks must be cited separately.
- Aggregate charge/ranged/boss-phase counters establish execution, not every attack subtype, projectile trajectory, contact order or boss death. The final win may follow timer expiry with a boss alive, as allowed by the supplied source.
- Scripted movement and automatic choices do not recheck physical Xbox hardware throughout 20 waves. Earlier user-confirmed basic controller checks and the latest virtual navigation checks remain distinct evidence.
- Muted execution and sparse still captures do not validate sound, continuous animation or subjective play feel. Different engine RNG sequences prevent exact kills, drops, purchases or material totals from being treated as deterministic equality checks.

## Historical R21 suite, native settlement and full campaign completed

The R21 complete suite was independently read: **1,018/1,018 Passed, zero failures or skips**, duration 55.3793378 seconds (`artifacts/gameplay-parity-r21-2026-10-17/complete-suite-summary.json`). The revised native final-settlement fixture also Passed with process exit 0. Both timer-expiry and delayed-boss routes observed payout at cleanup age 0, growth at 0.6666667 seconds and Won after 4.0166636 simulated cleanup seconds. Each route changed materials 100→115, XP 0→15 and Harvesting 15→16; repeated Tick did not repay. Route wall durations were 4.03425 and 4.14308 seconds. All required flags and both saved captures were present; the parent inspected those images. This is a staged two-route native check, not a campaign or actual boss-kill reenactment.

The production settlement pipeline now separates immediate end-wave effects/income/XP, delayed positive Harvesting growth and cleanup completion. RunFeedback begins settlement before removing enemies or collecting cleanup pickups, advances growth after 0.66 seconds and completes after two seconds for ordinary waves or four for the final wave (and after remaining pickups are handled). This supersedes the earlier immediate-Won and payout-at-sweep-completion gaps documented historically below.

The fresh normal-time R21 campaign under `artifacts/full-verification-2026-10-17/unity-attempt-3` completed **Passed / Won, all 20 waves and 19 shops, native process exit 0**. The actual result records `realtime=true`, `preflight=false`, 60 Hz pickup physics and fresh required captures. Its trace contains 20 timer-end events and ends `finished / Won`. Process UTC start was `2026-10-18T01:14:38.2919843Z`; finish was `2026-10-18T01:35:46.0781795Z` (both October 17 in Toronto). Reported fixture wall duration was 1,263.670 seconds. R19 remains completed pre-settlement-fix evidence, not validation of this pipeline.

| R21 combat counter | Actual value |
| --- | ---: |
| Charge releases | 1,049 |
| Enemy ranged attack releases | 1,868 |
| Boss phase changes | 1 |
| Encountered enemy IDs | 16, including Predator |
| Peak enemies | 134 |
| Enemy kills / combined kills | 4,676 / 4,715 |

All six fresh R21 required PNGs were opened with the image viewer, at 1280×720. Wave 1 shows the player, six weapons, enemies, material drops and damage. Wave 5 shows acquired appearances and an extended melee attack. Wave 10 shows mixed enemies, ranged/melee attack presentation, consumables, materials and spawn markers. Wave 15 shows a dense mixed crowd, hostile projectiles, damage/critical feedback, trees and support actors. Wave 20 shows the Predator health bar, enemy crowd, hostile and friendly projectile presentation and support actors. `end.png` shows the Drifter unlock modal over the darkened final-wave scene; it is not an exposed Victory screen. Won is established by the runtime phase/result.

No new combat defect was confirmed from these fresh images or counters. Per-attack trajectories, audio and exact contact timing cannot be established from stills or aggregate counts. The final player log search found no `Exception`, `NullReference`, `ERROR`, `Error`, `Warning` or `warning` matches. This does not erase the historical R19 D3D12 cache diagnostic. Invulnerability, neutral setup, engine-specific RNG, scripted choices and the original's one pause intervention remain limits; this is an integration comparison, not human survival or complete parity proof.

### Clock policy and observed wall-time lag

R21 wave 15 took roughly 95.88 wall seconds for a 60-second combat timer, per the economy review. This is a measured wall-time difference, not a measured cause. No frame-time/focus/interaction telemetry was captured. Official [Godot 3.6 main loop](https://raw.githubusercontent.com/godotengine/godot/3.6/main/main.cpp) caps recovery at eight physics steps and also subtracts discarded physics time from idle delta (lines 2169–2173, 2229). The [Timer implementation](https://raw.githubusercontent.com/godotengine/godot/3.6/scene/main/timer.cpp) defaults to idle processing; the supplied WaveTimer has no override. Its timer can therefore also slow under long stalls. Scrapshift's eight-step accumulator cap follows the same broad policy; subframe remainder and timer sampling are not identical. This source finding does not identify the cause of this run's lag or establish performance parity.

### Historical profiling plan, subsequently executed

The initial plan was to use a separate Development player to collect a bounded crowded-wave sample using `-profiler-enable -profiler-log-file <capture.raw> -profiler-capture-frame-count N`, supported by the [Unity 6000.6 command-line reference](https://docs.unity3d.com/6000.6/Documentation/Manual/profiler-command-line-arguments.html). Normal build methods use `BuildOptions.None`; separate Development profiling builds were subsequently produced, preserving R21. The official [6000.6 ProfilerDriver bindings](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/6000.6/Modules/ProfilerEditor/Public/ProfilerAPI.bindings.cs) expose `LoadProfile` and `GetHierarchyFrameDataView`; an isolated Editor batch exporter can load the capture and aggregate sample total/self time, calls and allocations by frame/thread. This export workflow was subsequently implemented and executed; the allocation investigation linked above records its actual results.

The initial candidate list was not a diagnosis. Subsequent non-Deep captures, subsystem markers and controlled microbenchmarks attributed repeated allocation costs before the bounded fixes were made. No clock-policy change was made. The completed R21 full run remains preserved pre-optimization evidence.

## Historical R19 completed campaign

The fresh normal-time R19 campaign under `artifacts/full-verification-2026-10-17/unity-attempt-2` completed: **Passed / Won, 20 waves, 19 shops, native process exit 0**. `campaign-check.json` records `realtime=true`, `preflight=false`, 60 Hz pickup physics and fresh required captures. `process.json` records UTC start `2026-10-18T00:40:20.1055533Z` and finish `2026-10-18T00:59:17.7369775Z` (both October 17 in Toronto). Its reported fixture wall time is 1,131.888 seconds; this excludes some process startup/shutdown time.

The original attempt 2 summary was independently read: all 20 wave timer ends, 19 shop entries, `run_won=true`, status `won` and process exit 0. Original totals include 4,867 enemy kills and 42 tree kills. Unity records 5,074 enemy kills and 5,113 combined kills. These totals are observations from engine-specific random runs; they are not a deterministic equality test or evidence of an incorrect kill rate.

### Executed combat evidence

| R19 final counter | Recorded value | What it establishes |
| --- | ---: | --- |
| Charge releases | 586 | Charge-release paths executed during the campaign |
| Ranged attack releases | 1,113 | Enemy ranged-release paths executed |
| Boss phase changes | 2 | Boss phase transitions executed |
| Peak enemies | 128 | Recorded campaign peak; not a performance benchmark |
| Enemy IDs encountered | 17 | Includes `invoker`, chargers, spitters, spawners, buffers and trees |

The driver has cumulative release/phase counters, not a complete per-attack-type trace. These counters cannot establish exact projectile trajectories, every subtype, contact timing or boss death. The original `wave-20-end.png` was inspected and still shows a large boss with timer 1; do not describe its Won result as proof that the boss was killed.

### Actual native captures inspected

All six fresh R19 required images were opened with the image viewer, independently of capture-freshness checks:

- `wave-01.png`: player, six weapon views, live enemies, material drops and floating damage.
- `wave-05.png`: player at the upper wall, an acquired fitted eye appearance, a projectile, damage numbers, material drops and several enemy sizes.
- `wave-10.png`: active weapon impacts/projectiles, consumable and crate drops, spawn markers and multiple enemy types.
- `wave-15.png`: dense enemy mix, red hostile projectiles, spawn markers, critical/damage numbers, materials and acquired appearances.
- `wave-20.png`: INVOKER boss and health bar, dense red hostile projectile field, hit effects/damage, player/loadout and spawned support actors.
- `end.png`: post-run Drifter unlock modal and a bottom-left achievement popup over a darkened wave-20 scene. It is not an exposed Victory screen; Won is established by the recorded runtime phase and result.

No confirmed gameplay defect was identified in these stills or final combat counters alone. Subsequent source/trace review confirmed a final-wave settlement defect, documented below; therefore the completed R19 result does not close overall parity. Images establish visible states, not movement, sound or exact timing. Captures switch from 1280×720 in early samples to 1920×1080 in later samples; do not claim a fixed captured display size.

### Build, regression and runtime-log scope

Validation supplied by the parent: **R17 complete suite 1000/1000 passed**; R19 build and native arrival passed. R19 changes only the opt-in arrival capture harness relative to R18, waiting for a real Update after its accelerated wave transition and asserting rendered alignment. Production gameplay is unchanged. The earlier immediate pre-Update fixture image was a staging artifact, not proof of an ordinary GUI transition defect.

Final R19 `player.log` was searched for `Exception`, `NullReference`, `ERROR`, `Error`, `Warning` and `warning`. The only matching line was the startup engine diagnostic `d3d12: ERROR D3D12PipelineLibrary must be recreated due to invalid disk cache`; no exception or NullReference match was found. Zero build warnings must not be restated as a warning-free player log. No engine cache directory was modified.

### Preserved intermediate attempts and limits

R18 evidence under `artifacts/full-verification-2026-10-17/unity-final` stopped after 12 waves without a final campaign result; both game and runner were gone following a runtime interruption. Preserve that run as partial evidence. Its previously inspected wave-1/5/10 images are historical; they do not replace the fresh R19 captures.

Both completed fixtures use invulnerability, scripted movement, automatic progression, a neutral character setup and a fixed six-weapon tier-II loadout. The original records one unexpected-pause intervention. Different engine random sequences, shop choices and contact ordering remain comparison limits. This completed integration check does not establish exact overall gameplay parity, human survival balance, physical-controller campaign play, audio quality or performance.

Attribution: the user directed the comparison and provided the reference. Codex performed source investigation, fixture implementation and artifact review. No independently user-authored testing or personal debugging experience is implied.

## Confirmed post-run finding — final-wave settlement

The economy comparison identified that R19's final-wave material/XP settlement was omitted. A read-only source audit independently confirmed the required order; this reviewer changed no runtime files. The economy agent owns the runtime correction and tests, while the parent owns combined validation and subsequent build evidence. R19 remains complete pre-fix evidence.

### Original timer expiry and early boss death

1. `main.gd:1221` (`_on_WaveTimer_timeout`) checks end-of-wave challenges, applies each player's `stats_end_of_wave` effects, converts `convert_stats_end_of_wave`, and emits `end_of_the_wave`.
2. `main.gd:1272` calls `manage_harvesting()` unconditionally, including the final non-endless wave. Its combined income uses effective Harvesting, pacifist, cryptid, elite-killed, living-enemy and charmed-enemy bonuses before integer gold/XP APIs. Nonnegative income adds gold and XP; negative income removes gold without subtracting XP. Positive Harvesting starts `HarvestingTimer`.
3. It then calls `clean_up_room()` and resets temporary stats. `clean_up_room` calls `_set_run_states()` first; a live player on the final non-endless wave wins regardless of surviving bosses. It applies run-won state, starts a four-second end-wave timer, cleans up actors and attracts remaining materials/consumables.
4. `HarvestingTimer` is one-shot **0.66 seconds** (`main.tscn:947`). Its callback `main.gd:1521` grows positive effective Harvesting with `ceil(harvesting * harvesting_growth / 100)` at or before the final configured wave. Final-wave growth therefore occurs during the win cleanup interval.
5. `_on_EndWaveTimer_timeout` calls `RunData.on_wave_end()` and resets linked stats. At `main.gd:1143`, won/lost runs route directly to the end-run scene. Only ordinary continuing waves process queued crate and upgrade choices before the shop. A final wave must settle income/XP, but must not open a new shop or final upgrade/crate selection sequence.

The early-boss path shares this timer-expiry logic: `main.gd:510–521` checks that the dying Boss is the last boss/elite alive on the final wave. In a non-endless run it sets the ordinary wave timer wait to **0.1 seconds** and starts it. It does not mark Won immediately or skip the timer callback. Endless mode instead schedules extra groups and records that the final bosses were killed. This distinction preserves death callbacks/drops before normal final settlement.

### XP and queued rewards

`RunData.add_xp` (`singletons/run_data.gd:818`) applies XP gain and repeatedly invokes `level_up` for crossed thresholds. `main.gd:899` handles each emitted level-up immediately: it queues an upgrade, updates the label, adds one Max HP and applies `stats_on_level_up`. Thus final harvesting XP can increase level and apply those automatic effects even though the won-run scene branch skips choosing the queued upgrades. Crates collected during cleanup can likewise be queued without opening a final item-choice screen.

### R19 discrepancy and acceptance scope

Before the correction, `RunRules.Tick` applied permanent end-wave stats/conversion, then returned Won at `Wave==WaveCount` before the common harvesting income, XP and growth code. `RunRules.DefeatBoss` also marked Won immediately. The latter was invoked from `ReferenceCombat` before the rest of the boss death/drop callbacks, unlike the source's short timer restart.

Required focused checks: final positive and negative harvesting, XP crossing a level threshold with automatic level-up effects, final positive harvesting growth, retained end-wave stat/conversion ordering, no final shop/reward choices, and last-boss death shortening the timer without prematurely ending death/drop processing. Test completion and native post-fix evidence must be reported separately from this source finding.

### Final cleanup duration follow-up

R20 native timer and delayed-boss routes were reported Passed by the parent: staged materials 100→115, Harvesting 15→16 and XP 0→15, both with exit 0. Those amount checks exposed a separate timing gap: `RunFeedback.TickWaveSweep` used the same 1.8-second wait for the final wave, while source `main.gd:962–964` selects four seconds for a won run. A bounded correction now uses four seconds only at `RunRules.WaveCount`, preserving the existing ordinary-wave sweep duration. The native final-settlement fixture now has a 20-second timeout and records/checks each route's simulated cleanup duration (at least four seconds) and wall duration (at least 3.9 seconds). Combined validation of that follow-up is pending the parent's R21 build.

Historical timing gap, superseded by R21: the intermediate implementation applied end-wave stat/conversion, harvesting payout and growth together at sweep completion. Source applies effects/income before cleanup and positive Harvesting growth after 0.66 seconds. R21 introduces a staged settlement pipeline and its native fixture verifies those intermediate milestones; see current validation above.

## Historical investigation and intermediate evidence

Everything below records earlier frozen checkpoints and the investigation leading to R18. Statements about pending corrections or live R15 runs are historical, not the current status. The current result above takes precedence.

## Historical R15 / fresh-original follow-up

The complete suite's actual `suite-attempt-2.xml` was independently read: **951/951 passed, zero failed/skipped**, duration 68.3595964 seconds. The parent reports the R15 build has zero errors/warnings. Both fresh 20-wave fixtures are running; this document does not claim their completion. Current traces are `artifacts/full-verification-2026-10-17/{original/trace.json,unity/campaign-trace.json}`.

The per-wave movement-clock correction is now present in `ArenaGame.Simulate`: campaign scripted age uses `elapsed-campaignWaveStart`. Current original render target is 60 FPS and physics is 60 Hz; its driver explicitly suppresses human pause input and records unexpected pause interventions. One intervention is recorded in the first-wave trace, so that historical fact must remain visible rather than describing the run as having no interventions.

### First-wave collection difference: what is actually known

Both fresh traces record **27 enemy kills plus one tree kill**. Unity's combined `kills=28` is therefore consistent with the original's separate enemy/tree totals. Material accounting at timer end is:

| First wave | Collected materials | Ground value | Combined |
| --- | ---: | ---: | ---: |
| Original | 11 | 19 | 30 |
| Unity R15 | 3 | 27 | 30 |

The collection split differs while recorded total yield is equal. This does not establish an incorrect drop rate. Different random spawn/drop locations and cross-engine movement/contact timing can affect collection; no position trace was recorded to identify the eight-material difference causally.

### Two remaining setup limits established from source

1. **Spawn-coordinate convention:** source `EntitySpawner.init` spawns the single player's raw origin at map center. Its animated body and pickup center are 24 source pixels above that origin (`Player.Animation` and `ItemPickupArea/CollisionShape2D`). Unity `Begin`/`AdvanceWave` place the body-centered `playerPos` at zero, while `PlayerOrigin` is `playerPos + down*.24`. Thus Unity's raw origin starts 0.24 world units below center under the same map-centered coordinate conversion. Moving campaign-only initial body position up 0.24 would align raw origins; no change was made during the live frozen run. This is a comparison setup difference, not proof of a general movement-speed bug or the cause of the collection split.
2. **First-wave weapon arrival:** source `Player.land()` does not explicitly lock movement, so an invented movement wait would be incorrect. However, `player.tscn`'s 1.2-second landing animation tracks the actual `Weapons` container position/rotation/scale: position starts at `(-265,-749)` and reaches `(0,-24)` at 1.1 seconds. Its descendant weapon range/hit shapes therefore do not occupy their ordinary final mounts throughout that initial window. Unity's campaign begins with arrival already completed and equipment at final mounts. The first-wave attack setup is consequently not an exact reenactment. This narrows the earlier statement that landing was merely visual; movement remains unlocked, but weapon descendants are affected.

Neither observation proves the pickup delta's cause. The later production-path inspection below establishes that both extend beyond the fixture, so they require corrections and fresh validation rather than merely a comparison disclaimer. R15 remains frozen intermediate evidence while the original run can continue as reference.

### Production-path confirmation and bounded fix proposal

Read-only inspection found the following explicit source behavior:

- `Unit._can_move` initializes true; `Player._ready` and `Player.land` do not clear it. `Player.update_animation` switches only idle/move states when already in those states, leaving landing animation intact while movement continues. The landing animation has no track disabling movement, player timers or weapon processing.
- `main.gd` starts its wave timer at line 170 and initializes wave spawning, then calls `player.land()` only under `RunData.current_wave == 1` at lines 253â€“256. Every later Main scene spawns the player at map center without landing.
- `weapon.gd:_physics_process` at lines 179â€“202 updates targeting/cooldowns and can defer shooting. Its `should_shoot` predicate has movement/cleanup/manual/range conditions, but no arrival condition. Its actual animated parent transforms determine range/hit/muzzle positions during first-wave landing.
- Unity's ordinary `Update` instead runs **only** `TickArrival` while `Arriving`. That method decrements weapon clocks and poses graphics, but never advances player movement, `Rules.Tick`, spawn scheduling, enemies or item updates. `UpdateEquipment` additionally forbids shooting with `!Arriving`. This creates a real 1.2-second gameplay hold absent from the supplied source, including with reduced motion.
- Unity's `Begin` and `AdvanceWave` set the body-centered `playerPos` to zero; mapping the original raw center requires `-CombatGeometry.PlayerOriginOffset`, i.e. up 0.24. This is also used by the ordinary path, not just scripted fixtures.

Proposed bounded correction, assigned for integration by the parent:

1. Place the body at up 0.24 **before** starting each wave's actor setup, including Begin and AdvanceWave. Derive the value from the existing origin offset; do not scatter new literal offsets or move collision shapes individually.
2. Advance the ordinary 60 Hz simulation throughout first-wave landing. Treat arrival as presentation/animated weapon-transform state, then apply its visual pose after simulation. Remove the separate arrival-only cooldown decrement to avoid counting twice, and remove the arrival firing prohibition.
3. Apply the supplied animated Weapons-container position/rotation to mounts, target-range queries, muzzle positions and melee contact geometry during first-wave landing. Keep player hurtbox, physics body and pickup areas at the moving ground/raw position, as source does. Current `ArrivalMotion.Offset` is an approximation; preserve actual imported landing key values/interpolation if they are now used for gameplay geometry.
4. Keep the source's first-wave-only landing condition. Later-wave transition still resets raw spawn to map center and starts normal gameplay immediately.
5. Update scripted campaign setup to exercise the corrected first-wave path; currently `BeginCampaignCheck` calls Begin with arrival already completed. Retain existing seeded loadout, invulnerability, callback recording and per-wave movement policy.

Meaningful acceptance: at one normal 60 Hz tick after first Start, movement can change the raw position, the wave timer/spawn clock and cooldowns advance once, moving/not-moving item state updates, and an eligible weapon can shoot from its current animated mount. Pause must freeze that state and resume it; reduced motion cannot pause mechanics. Later waves must start at the same raw center without a new arrival hold. Existing `PresentationReview.landingHeldTimer` and any equivalent test expecting an unchanged wave timer during TickArrival are stale source assumptions and require source-backed replacement, not preservation as a gate.

No runtime changes were made by this reviewer during either live run. The parent and controls agent coordinate implementation and validation; do not report R15 completion as verification of these as-yet unimplemented corrections.

### Bounded first-Pistol muzzle check

Source `weapons/ranged/pistol/pistol.tscn` authors `Sprite.position=(24,0)` and child `Sprite/Muzzle.position=(32,0)`, with unit scale. Its neutral projectile origin is therefore 56 source pixels forward from the weapon mount, before recoil and parent composition. Base `weapons/weapon.tscn` supplies a Position2D muzzle defaulting to zero; `weapon.gd` obtains that node, without calculating its position from sprite width. `ranged_weapon_shooting_behavior.gd` passes its global position to projectile creation.

Unity's then-current calculation uses the own-art view position plus `image.bounds.max.x * ActorPresentation.WeaponScale`. That is art-dependent rather than the source-authored offset. For a centered, horizontally dominant image fitted to the current Pistol visual length 0.65, it would produce `0.24 + 0.325 = 0.565`, instead of 0.56; actual own-image bounds should be measured before reporting an exact runtime delta. Preserving own art does not require deriving mechanical spawn geometry from its width.

The existing numeric importer `scripts/import_weapon_fidelity.py` already resolves the weapon scene through `scene(d['scene'])`. A bounded follow-up can extract `Sprite` and `Sprite/Muzzle` authored position/scale into numeric metadata, preserving approved artwork. This check inspected only Pistol/base weapon; it is not an assertion that every ranged weapon has the same offset. No runtime/importer edit was made during the frozen build.

### Authored ranged-muzzle correction implemented after R16

The parent assigned the confirmed discrepancy for correction before the final restart. `scripts/import_weapon_fidelity.py --numeric-muzzles-only` now resolves all inherited registered ranged weapon scenes and updates **84 profiles** with composed `muzzle` and `muzzleRecoil` numeric fields. Weapon root scale, Sprite translation/scale/rotation and the child Muzzle translation are composed in source coordinates, then converted at 100 px/unit with the y-axis flipped. The Muzzle node's own rotation/scale cannot change its origin; weapon-root rotation is written dynamically by the aiming code and layout supplies root position.

The numeric-only branch exits before any projectile/media import call. It verified all non-muzzle JSON content remained semantically identical. No original textures/audio, import metadata or generated gameplay catalog were copied. Evidence: `artifacts/authored-weapon-muzzles-2026-10-17.json`, containing 84 resulting numeric profiles, 112 source-file hashes and the unrelated-fields check. Python syntax parsing and an isolated synthetic inherited-transform check passed; the latter composes root scale `(2,3)`, Sprite position `(10,20)`, scale `(2,4)`, rotation 90Â° and muzzle `(5,6)` to world `(-.28,-.9)`, with recoil axis `(-2,0)`.

`WeaponReference.AuthoredMuzzleOffset` and `MuzzlePosition` now supply the actual ranged projectile spawn in `ReferenceCombat`. Own-art bounds no longer determine mechanical origins. Source ranged shooting samples its muzzle **before** starting the new recoil tween, so ordinary new-shot calls use rest recoil zero. The helper also models an existing Sprite recoil displacement by applying the imported root-scaled recoil axis; reduced-motion visual treatment does not alter its mechanics. Landing-parent rotation composes into global muzzle position while projectile velocity retains the source's local aim behavior.

`WeaponMuzzleParityTests.cs` adds **21 focused cases**: complete ranged-catalog coverage, fifteen source-authored family offsets, three recoil samples, independence from artwork, and landing/aim composition. Source examples include Pistol `.56,0`, Taser `.46,-.08`, Slingshot `.08,.03`, Rocket Launcher `.66,.02`, and Fireball `.26,-.02`; the authored off-axis offsets matter as well as forward length. At implementation time these tests were pending combined validation; the later R17 complete suite passed 1000/1000. No Unity run was launched by this agent.

The opt-in arrival fixture's former 2.5-unit proximity assertion was replaced with a source-authored muzzle comparison. Under its staged constant movement/aim and Pistol's straight flight, it reconstructs the birth point from current shot position/age/velocity, reconstructs the firing body's position and arrival age, then compares the imported authored origin within .035 world units and records the numeric error. This was initially an unexecuted fixture assertion; the later R18 native run passed with authored muzzle error zero.

## Historical confirmed setup discrepancy before the final comparison

The original driver uses `movement() = Vector2(cos(wave_age*.8), -sin(wave_age*.8))`; it resets `wave_age=0` on each new Main scene. Unity's historical campaign branch used `cos(elapsed*.8), sin(elapsed*.8)` while `elapsed` accumulates across waves. `AdvanceWave` resets player position but not that clock. Therefore later waves begin in different movement directions even before random outcomes diverge.

The parent confirmed this finding and is correcting only the campaign fixture to use its per-wave combat age (`elapsed-campaignWaveStart`). Keep ordinary play and the shared cumulative trace clock unchanged. Verify the final source/build includes that correction before calling the policy aligned. An old R8 trace cannot establish the correction.

The apparent factor `.5f` in Unity's scripted input is **not** a speed discrepancy: `CombatInput.Velocity` normalizes the input before applying `Rules.MoveSpeed`. Source `Unit.get_move_input` also normalizes. Source y-down versus Unity y-up explains the sine sign. No half-speed fix is warranted.

## Matched fixture conditions / required run flags

| Condition | Supplied original driver | Unity campaign driver |
| --- | --- | --- |
| Combat cadence | `Engine.iterations_per_second=60`, time scale 1 | `ReferencePhysicsClock.Step=1/60`; real-time mode consumes wall delta through its accumulator |
| Rendering | Frozen historical target 120 FPS; fresh October 17 target 60 FPS, vsync disabled | Target 60 FPS, vsync disabled in real-time mode; rendering rate is not physics cadence |
| Run type | Danger 0, base zone 0 | Difficulty 0; current source schedules and map setup |
| Character effects | Well-Rounded identity assigned without applying effects | `Begin` uses `Rules.Start()` with null character, neutral base stats |
| Starting loadout | Six tier-II weapons: Pistol, SMG, Double Barrel Shotgun, Laser Gun, Knife, Spear | Same six IDs and tiers, explicitly replaced after Begin |
| Damage protection | Debug invulnerability and `RunData.invulnerable`, disabling player hurtbox | Refreshes invulnerability before each explicit combat step; player contact window stays disabled |
| Inputs | Scripted movement, manual aim off | Scripted movement overrides input; fixture preferences/defaults must retain automatic aiming |
| Intermission | Take every crate, select first upgrade, buy first affordable eligible active offers, no rerolls | Take crates, choose first upgrade, buy affordable eligible four offers, no rerolls |
| Persistence | Isolated profile, saving disabled, fresh initial counters | Fresh in-memory progression; live unlock changes apply through callbacks |
| Randomness | Seed 771 set after reference loadout | Seed 771 and placement seed set before initialization; engines consume unrelated sequences |

Use both `--campaign-check <new-output-folder>` and `--campaign-realtime` for the intended normal-time comparison. Without the latter, Unity deliberately runs 24 combat steps per render and 240 Hz pickup physics; that accelerated mode is not comparable to the frozen reference's normal cadence. Do not use the one-wave `--campaign-preflight` result as a 20-wave result.

## Hash/evidence checks performed

The frozen original fixture hash matches its manifest addition:

- `parity_fixture.gd`: `9bb95d0efdba3ec75952dee6e8a63c419e1bb92125b5bfd63754289c2d884831`.
- `trace.json`: `98ac455148dba90b48f89771ea5fe720a8487f95d70f2bc2162fd6799d41a535`.
- `summary.json`: `d5f4fa85c3543f08b739990f1086e866b0072ce7f75abd4dacb1cacbbd672e1e`.

Current supplied `main.gd`, `unit.gd`, `player.gd`, `player_projectile.gd`, `weapon_service.gd` and `entity_spawner.gd` each match their frozen source hashes; their manifest source/fixture hashes were equal. The frozen fixture intentionally changed movement input and save/log paths in the copied project. Its manifest records Godot 3.6.2, game 1.1.15.4, time scale 1 and 60 Hz. The profile directory was created before launch but had no prelaunch save files; this is more precise than treating the directory's existence as an old-save indication.

These are selected source/hash checks against historical evidence, not a claim that every file in the new reference clone has already been verified. The agent preparing the fresh original run owns its new manifest, profile and output paths.

## Comparison limits to retain

- Different shop offers, crates, pickups, weapon upgrades and enemies' random choices are expected from different engine RNG streams. Seed 771 does not make outcomes identical. Validate each run's own transaction debits and progression consistency before comparing aggregate outcomes.
- Original `simulated_seconds` comes from `_process(delta)` and includes scene/intermission time; Unity's `elapsed` is active combat time excluding shop/reward processing and wave cleanup. Use wave timer-end events and per-wave durations; never subtract these two total clocks as if they measured the same interval.
- Source fixture action delays (0.8 seconds after scene changes and 0.3 seconds between choices) differ from Unity's immediate scripted intermission choices. They affect wall duration, not the intended active-wave length. Neither result is a human survival test.
- Source Godot rigid-body/Area2D ordering and Unity's custom crowd/contact ordering remain different implementations. The previously executed native contact probes close specific cases, not complete crowd physics equivalence.
- The historical campaign fixture started with Unity arrival completed. The corrected R18/R19 fixture exercises first-wave arrival while simulation continues; source `main.gd` starts the wave timer and calls `player.land()` without an explicit movement lock. Its animated Weapons container affects initial attack geometry as detailed above.
- Both fixtures use invulnerability. Victory demonstrates the integrated wave/shop/reward/boss path under that setup; it does not validate lethal damage, defeat behavior, survival balance, physical controller play or performance.

## Full-suite triage status

At initial review, `artifacts/full-verification-2026-10-17/suite-attempt-1.log` was still being written and no result XML existed. No complete-suite success or failure count is asserted here. Once the XML is available, inspect failed test names/messages and distinguish source regressions from stale fixtures before proposing changes. The parent owns final results and any rerun.

### Complete-suite attempt 1: owned failure triage

The parent subsequently read the complete result: **949/951 passed, two failures, exit 2**. This agent was assigned only `StructureImplantTests.FlameStructureUsesEngineeringAndDoesNotGainPlayerPercentDamage`: its historical fractional expectation 14.9 contradicted the already corrected source rule and the runtime's 14.

Source `items/all/turret_flame/turret_flame_burning_data.tres` specifies base damage 5, duration 8 and Engineering scaling 0.33. `weapon_service.gd:489-490` returns scaled damage `as int`; `init_burning_data` at lines 328â€“330 then applies structure percentage. Therefore `5 + 30*.33 = 14.9 â†’ 14`. Ordinary +100% Damage and +100 Elemental must not enter this native engineering burn. The fixture now expects exactly 14 and eight ticks, retains its direct-shot damage-one check, then adds +50% structure damage and expects exactly 21 to distinguish truncation-before-percentage from rounding or a retained fractional base.

This was a fixture correction, not a runtime change or relaxed tolerance. No rerun was launched by this agent; the other failure and combined rerun belong to the parent. Preserve attempt 1 XML/log as evidence.

Attribution: the user authorized complete validation; Codex inspected the drivers/source/evidence and identified the per-wave movement setup discrepancy. This is not a claim of independently user-authored testing.
