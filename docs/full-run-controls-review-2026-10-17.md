## Current R23 native checkpoint

R23's opt-in virtual-controller player check passed all17 snapshots, process exit0, under `artifacts/gameplay-parity-r23-2026-10-17/navigation`. It exercises actual Update/OnGUI selection, movement, aim, pause/resume and intermission shortcuts including held reroll. Character, movement and pause captures were viewed. The complete suite passed1031/1031 (attempt9). This is separate from the user's earlier physical Xbox acceptance; no new hardware/full-campaign control claim is made.

Preserved R22 navigation attempts failed first confirmation without foregrounding, then reached the shop but synthetic Y reset during a hold. The opt-in R23 fixture uses Input System IgnoreFocus so desktop focus changes do not reset queued synthetic input. Its report states this exception; ordinary gameplay focus behavior and the physical logger are unchanged. This fixture cannot establish ordinary background/focus behavior. The fixed settings lookup caches launch classification only; current option values remain live.

The fresh R23 campaign at `artifacts/full-verification-2026-10-17/unity-attempt-4` uses scripted movement, not physical-controller steering. It completed20/Won/exit0 with19 shops and passed18/18 saved-trace comparison checks. These remain scripted campaign checks, separate from physical-controller use. Earlier completed R21/original evidence remains below.

# Full-run driver and reference preflight — 2026-10-17

The user now authorized the complete suite and twenty-wave comparison. This review inspects drivers and prepares fresh reference isolation; it does not report completion of either run. Root owns builds, process launch and result review.

## Current driver comparability

`CampaignCheck.cs:56` requires `--campaign-realtime` for the 60 Hz accumulator path; without it the fixture performs 24 combat steps per rendered frame and uses 240 Hz pickup physics. Root's normal-timing comparison must use `--campaign-check <fresh-output> --campaign-realtime` and omit `--campaign-preflight`.

The realtime path at `CampaignCheck.cs:111` consumes `ReferencePhysicsClock` steps, whose step is 1/60 second. The render target is 60 FPS with v-sync disabled. A bounded eight-step recovery cap prevents unbounded catch-up; this is integration timing evidence, not a performance benchmark.

Both comparison fixtures use neutral initial character effects, Danger 0, zero initial materials, six tier-II weapons (pistol, SMG, double-barrel shotgun, laser gun, knife, spear), seed 771, invulnerability, mute and fresh progression. The zero purse and six weapons are explicit fixture setup, not normal character-selection rules. Normal run starts retain their separately verified reference purse and character-dependent weapons.

The first preflight found a driver difference: frozen reference `parity_fixture.gd:56` uses `cos(wave_age*.8), -sin(wave_age*.8)` and resets `wave_age` on each Main scene at line 91. Scrapshift's then-current `ArenaGame.cs:247` used global `elapsed`. Root undertook the narrow correction to use `elapsed-campaignWaveStart` for the campaign movement phase. Verify the built source manifest includes that correction before launch. The opposite y sign is the expected Godot-down/Unity-up coordinate conversion; Unity's vector magnitude factor does not halve travel speed because `CombatInput.Velocity` normalizes it.

The shop drivers take the first offered upgrade and buy affordable offers; Godot inserts UI delays while Scrapshift processes intermissions immediately. Reference `simulated_seconds` includes fixture processing/intermission time, while Scrapshift traces active combat elapsed separately. Compare wave timer durations and labeled events, not raw cumulative timestamps as equal clocks. Identical seeds do not imply identical cross-engine random draws, acquisitions, enemy kills or pickup totals.

## Pause and physical input

Scrapshift disables focus-loss auto-pause for campaign checks (`GameSettings.cs:37`) and sets `Application.runInBackground=true` (`CampaignCheck.cs:60`). Ordinary keyboard/Start pause handling still exists before the campaign tick (`ArenaGame.cs:172` at inspection time). Keep the scripted player free of physical input; if a trace enters Paused, investigate that intervention rather than treating a stalled campaign as a gameplay-duration result.

The supplied original's InputService emits focus loss even when `on_lost_focus=0`; `main.gd:1706` forwards it to `pause_menu.gd:103`, which separately checks `pause_on_focus_lost`. The old frozen fixture did not disable that second setting. A fresh background comparison therefore explicitly disables it in the copied fixture. This is comparison instrumentation, not a change to the original game's pause behavior.

The fresh reference fixture consumes dispatched input through `_input`, but that alone would not block `Input.is_action_just_released` polling. It separately removes the `ui_pause` action's event bindings and releases that action during fixture boot. If the tree unexpectedly pauses anyway, it logs `fixture_pause_intervention`, increments the trace counter and explicitly resumes through the existing pause menu (or unpauses a non-Main tree). Any nonzero count must be disclosed in the result review.

## Fresh original fixture prepared, not launched by this agent

Clone: `E:/Codex Projects/Job Search Assistant/tmp/BrotatoFullComparison20261017`.

Launch script: `E:/Codex Projects/Job Search Assistant/tmp/BrotatoFullComparison20261017/LaunchFullComparison.cmd`.

It scopes APPDATA to `<clone>/host-data/Roaming` and LOCALAPPDATA to `<clone>/host-data/Local`, then launches:

```text
E:/Godot_v4.7.2-stable_win64.exe/Godot_v3.6.2-stable_win64.exe
--path "E:/Codex Projects/Job Search Assistant/tmp/BrotatoFullComparison20261017"
--no-fullscreen --resolution 1280x720 --position 80,80
```

The project keeps the fixture bootstrap and autoload, has a unique project/user-directory identity, and redirects every old clone save/log/modloader prefix to the new clone. No save files existed in its profile before delivery. Expected `user://` is `<clone>/host-data/Roaming/BrotatoFullComparison20261017`; the first actual boot trace records `OS.get_user_data_dir()` so root can verify that path natively. This environment/custom-directory pattern was already measured with the same Godot engine in the keyboard-review copy; the full comparison clone has not yet been launched by this preparation agent.

The fixture output is a fresh `artifacts/full-verification-2026-10-17/original` folder. The old `BrotatoParity` fixture has a hardcoded output to frozen October 15 evidence and old saves, so launching that old clone unchanged would overwrite history and would not be a fresh run. No CLI output argument exists in that old fixture; the new copied fixture's output constant was changed explicitly.

## Source and hash evidence

`artifacts/full-verification-2026-10-17/original/source-manifest.json` records 3,817 source/fixture hash pairs, 14 differing/helper files, engine/fixture/project/launcher hashes, exact setup and empty prelaunch profile. `source.diff` describes differences from supplied original files; copied `parity_fixture.gd` and `project.godot` preserve the exact launch inputs. Production differences remain the existing fixture movement hook and file-path redirects, plus inherited diagnostic prints. Bootstrap/fixture scripts are disclosed instrumentation. Supplied original and frozen evidence were not edited.

Fresh fixture SHA-256: `b04ea44bf8f9455f6017144efa2c408cb7ce4538ed752f48e2baea0dbc8754f4`.

The reusable frozen baseline was independently checked before preparation:

| Frozen input | SHA-256 |
| --- | --- |
| `gameplay-parity-reference-2026-10-15/realtime/trace.json` | `98ac455148dba90b48f89771ea5fe720a8487f95d70f2bc2162fd6799d41a535` |
| `summary.json` | `d5f4fa85c3543f08b739990f1086e866b0072ce7f75abd4dacb1cacbbd672e1e` |
| `parity_fixture.gd` | `9bb95d0efdba3ec75952dee6e8a63c419e1bb92125b5bfd63754289c2d884831` |
| `source-manifest.json` | `ce3fda2167bc1ca73b8cc2c67e89391b29f47f7bf3c5e2b66a06a4aa81b23316` |

Frozen reference render target was 120 FPS with 60 Hz physics. The fresh comparison deliberately targets 60 FPS to match current Scrapshift; it must not be described as the untouched frozen launch configuration.

## Launch disposition

The known fresh-reference blockers (old output overwrite, reused profile and background focus pause) are addressed in the new isolated setup. Root must verify its built per-wave movement correction, supply the realtime flag, launch both fresh outputs and inspect results. No remaining source-based controls blocker was found for a scripted comparison under those conditions. Completion alone will not establish normal survival balance, all-character coverage, physical input behavior or exact gameplay parity.

## Startup pause intervention observed after root launch

Root subsequently launched the fresh original fixture. Its boot trace natively confirms the isolated `user://` path, 60 Hz physics and 60 FPS render target. At 1.881 seconds, wave 1 recorded one `fixture_pause_intervention`; it was still the only intervention when wave 2 was inspected. The precise cause is **unknown**. The reference Main ready code contains no intended startup tree pause, so this must not be relabeled as expected startup behavior or a proven focus event.

The fixture's Main recovery calls the actual `PauseMenu.unpause()` method. Source `ui/menus/ingame/pause_menu.gd:69`–75 disables pause input/focus emulation, hides the panel, unpauses the tree, resets menus and emits `unpaused`; it does not merely change the tree's paused flag. Opened the fresh original `wave-01-end.png` and `wave-02-start.png`: neither displays the pause overlay. Wave 1 combat/end presentation was visible and the trace advanced to wave 2. These source and image findings support retaining the run with the intervention explicitly disclosed, rather than restarting solely because one intervention occurred.

A possible action-map restoration route exists: `ui/menus/pages/menu_options.gd:228` sets the gamepad movement checkbox, and its handler at 382–386 calls `InputService.enable_gamepad_movement`/`disable_gamepad_movement`, which reloads global Input Map settings. The current trace does not prove that route caused this pause. Repeated interventions or a visible lingering pause panel would warrant stopping the owned fixture and instrumenting the actual pause call site before a fresh restart. This run must not be reported as zero-intervention evidence.

### Wave 5 checkpoint during the live run

One subsequent read of the original trace reached the wave 6 scene at 172.276 wall seconds. The pause-intervention count remained **1**, still solely the unknown wave 1 event at 1.881 seconds. The actual boot record verifies `user://` as `E:/Codex Projects/Job Search Assistant/tmp/BrotatoFullComparison20261017/host-data/Roaming/BrotatoFullComparison20261017`, with the separate save root inside that same isolated clone; both fixture physics and render target are 60.

Opened `artifacts/full-verification-2026-10-17/original/wave-05-start.png` and `wave-05-end.png`. Neither has a pause overlay. The start capture is taken before the camera settles (HUD and part of the map visible); the end capture clearly shows the player, enemies, weapons and material drops during wave 5. These are checkpoint observations while both campaigns remain live, not final completion or a clean zero-intervention claim. No source/process/UI changes were made for this checkpoint.

## Confirmed production findings during the live comparison audit

An independent second source check confirmed two later findings reported by the combat agent. These supersede the earlier controls-only preflight disposition; root must correct/revalidate them before treating a new Scrapshift campaign as final evidence. This review made no gameplay edits.

### Player spawn origin: ordinary runs and campaign

The supplied `global/entity_spawner.gd:91`–103 places the single-player root at map center. `entities/units/player/player.tscn:1206` places its resting Animation/body registration at `(0,-24)` source pixels relative to that root. Scrapshift explicitly adapts to a body-centered coordinate: `CombatGeometry.PlayerOriginOffset` is `(0,-.24)` world units. Therefore a map-centered raw player root corresponds to **body position `(0,+.24)`** in Unity's up-positive world, not body position zero.

At inspection, `ArenaGame.Begin` assigned `playerPos=Vector2.zero`, and `ReferenceCombat.AdvanceWave` did the same. That places the adapted raw origin 24 source pixels below map center. This is a small but real world-position difference affecting normal starts, subsequent waves, target/spawn distances and the scripted campaign. Correct the shared initial/rest position rather than changing the established hurtbox/origin offsets to conceal it. The running original fixture remains a valid source reference; Scrapshift's affected run must be preserved as an earlier attempt if root rebuilds/restarts it.

### Landing freezes gameplay only in Scrapshift's ordinary run

The supplied Main starts its wave timer before `main.gd:253`–256 calls `player.land()` on wave 1. `player.gd:613`–625 plays the 1.2-second landing animation, attaches its parachute and waits for animation completion. It does **not** disable movement. `unit.gd:137` still reads movement while `_can_move` is true; `player.gd:188` updates moving/stationary effects without turning landing into a movement lock. The landing animation changes visible body/weapon transforms: `player.tscn:443` defines its 1.2-second length and the weapon-position track at 559 continues through that animation.

`weapons/weapon.gd:178`–202 continues physics cooldown/aim/fire processing. `should_shoot` at 272 onward tests shooting state, cooldown, movement-dependent character eligibility, target range/manual aim and cleanup; it has no landing prohibition. Elevated weapon transforms can naturally affect target distance, but landing itself is not a blanket attack lock.

At inspection, Scrapshift's ordinary `ArenaGame.Update` selected `TickArrival` **instead of `Simulate`** while arriving. `TickArrival` decremented weapon clocks and called `PoseArrival`, but did not advance the run timer, movement, spawn director, enemies, pickups or other simulation. `PoseArrival` called `UpdateEquipment(0)`, and `ReferenceCombat.UpdateEquipment` separately required `!Arriving` to fire. This creates a production wave-1 freeze of about 1.2 seconds that the source does not have. Preserve approved arrival artwork/animation and music while allowing source-appropriate gameplay to continue underneath it.

The scripted `CampaignCheckTick` invokes `Simulate` directly and does not enter the ordinary arrival branch, so its completion cannot validate this landing behavior. A bounded ordinary-start regression must cover timer/movement/cooldown progression and attack eligibility during the animation. The historical `PresentationReview` stage 39 assertion that landing holds the timer/elapsed/enemy count is an assertion of the old behavior, not source parity, and must be reconciled with this correction.

## Original attempt 1 terminated; fresh attempt 2 prepared

Root reported that the first fresh original process stopped during wave 12 at roughly 590 wall seconds. `original/process.json` records PID 29892 and exit code **1073807364 (`0x40010004`)**; no finished campaign summary exists. This is incomplete evidence, not a completed original comparison. A long exec-session lifetime limit is a possible explanation, not an established cause. Keep its trace, images, unknown startup-pause intervention and process result unchanged.

Before preparing another attempt, `Get-Process -Id 29892` returned no process. Only the owned inactive clone was updated. Attempt 2 uses:

- Output: `artifacts/full-verification-2026-10-17/original-attempt-2`.
- Fresh game data: `<clone>/userdata-attempt-2-20261017/`.
- Fresh process APPDATA/LOCALAPPDATA: `<clone>/host-data-attempt-2/Roaming` and `/Local`.
- Distinct user-directory name: `BrotatoFullComparison20261017Attempt2`.
- Same `LaunchFullComparison.cmd` path in the owned clone, with new environment roots; exact copied launch script is retained in attempt 2's artifact folder.

The updated fixture removes `ui_pause` event mappings on every fixture process callback, because source option initialization can reload the global Input Map after boot. It also reasserts focus-pause isolation. Unexpected pauses are still logged/counted and resumed; no claim is made that this guarantees a zero-intervention result before actual execution.

Attempt 2's manifest records 3,817 source hash pairs, 14 disclosed differing/helper files and **zero prelaunch save files**. Its fixture SHA-256 is `af486ad42235f583153f8432861fb6382aa5d68a2ca0df5c4f3fdf4a09ca6ccd`. Setup retains 60 Hz physics/60 FPS target, seed 771, neutral zero-purse/six-tier-II loadout, per-wave movement and invulnerability. No game engine was launched by this preparation agent. Root will launch a detached hidden process monitor returning immediately to the tool call, then inspect the eventual process JSON and finished trace rather than holding a twenty-minute campaign inside a long exec session.

### Attempt 2 live checkpoint: wave 7

Root launched the detached runner; a subsequent read of `original-attempt-2/process.json` records process 28780, runner 4080 and status `running`, with the exact argument/environment spec retained separately. Its trace reached the wave 7 scene at **220.852 wall seconds** with 42 events. The actual boot event verifies `E:/Codex Projects/Job Search Assistant/tmp/BrotatoFullComparison20261017/host-data-attempt-2/Roaming/BrotatoFullComparison20261017Attempt2` as its user-data directory.

There is still exactly **one** pause intervention, in wave 1 at **.951 seconds**; no additional pause is recorded through this checkpoint. Reapplying the pause mapping isolation did not eliminate the startup event, so its precise cause remains unknown. Proper recovery uses the original `PauseMenu.unpause()` method described above.

Opened the fresh `original-attempt-2/wave-06-end.png`: it shows active player/enemy/weapon/material presentation and the ordinary HUD, with no pause overlay. The correct stderr file is `runner-stderr.log`; it contains repeated Godot packed-scene warnings that an inherited `./Attach` node was modified but has vanished, a missing unpacked-mods-folder warning, and a CrashReporter error that engine logs were not found in the fresh isolated user-data directory. These must be retained and reviewed rather than calling the original launch warning-free. They have not stopped the observed progression, but that is not evidence to discard the diagnostics. This is live checkpoint evidence, not completion; no process was launched, stopped or edited for this read.

### Attempt 2 completed: original Won, exit 0

Read the final trace and detached process record: the original reached `finished/status=won`, `run_won=true`, all **20 wave timer callbacks and 19 shop entries**, at 1,146.189 wall seconds. Process 28780 exited **0**. The attempt retains exactly **one** unknown startup-pause intervention at .951 seconds; it is not a zero-intervention run. There were 43 purchases, 20 upgrades and 15 crates, with final level 21 and 58 inventory items. These outcomes are source-fixture observations, not a cross-engine equality verdict.

Opened fresh `original-attempt-2/wave-20-end.png` and `end.png`. The wave-20 capture shows ongoing boss/enemy combat with projectiles, materials and the ordinary HUD, without a pause overlay. The final capture clearly reads **Run Won — Crash Zone — Danger 0** and displays the six weapons, inventory and final statistics. A hovered progress reward tooltip covers part of the end inventory; this is an actual capture limitation, not a missing victory result.

The fixture itself writes only a trace. `scripts/summarize_original_campaign.py` now reproducibly derives this attempt's `summary.json` from its actual trace/process record and **its own** `userdata-attempt-2-20261017/user/log.txt`. It freezes those debug bytes as `reference-run-data.log`, records all input/source/script hashes, parses the last complete Player 0 dump and validates its wave, 58-item sum and six weapon IDs against the final trace. The 58 items occupy 46 distinct item IDs. No frozen historical item stacks or guessed counts were reused. Missing/mismatched final debug evidence blocks summary generation.

Each `waves[].sampled_simulated_interval` is the difference between that wave's actual Main scene event and timer callback in the same trace. Observed intervals range from 19.961 seconds on wave 1 through 89.851 on wave 20 and remain within .5 seconds of their source durations; intermission processing is not misrepresented as active combat. Fresh scene captures can precede the camera/timer HUD's first settled render and are not used as duration measurements.

Final original diagnostics are retained: **zero script errors**; one CrashReporter missing-engine-log error; three `Condition "_first != nullptr" is true` errors; one resources-still-in-use and one MemoryPool-cleanup error. Warnings comprise one missing unpacked-mods folder, 80 vanished inherited `./Attach` nodes and one ObjectDB leak warning. Exit 0 and victory do not make these diagnostics disappear or establish that the supplied original was warning-free. The original remains read-only; no speculative source fixes were made to suppress its logs.

Attribution: the user authorized the comparison and supplied the reference. Codex prepared fixture isolation and reviewed source; these activities are not represented as independently performed user work.
