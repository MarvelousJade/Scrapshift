# Supplied Brotato reference: 20-wave integration fixture — October 15, 2026

Result: the supplied reference completed all 20 wave timers, visited 19 real shops and reached its actual `end_run.tscn` screen with `RunData.run_won == true`. Process exit code was 0. This is an accelerated integration fixture, not an ordinary human playthrough or evidence of complete Scrapshift parity.

## Runtime and isolation

- Original, read-only source: `E:/UnityProjects/brotato-(3.x)`.
- Isolated working copy: `E:/Codex Projects/Job Search Assistant/tmp/BrotatoParity`.
- Runtime: `E:/Godot_v4.7.2-stable_win64.exe/Godot_v3.6.2-stable_win64.exe`.
- Runtime-reported version: `3.6.2.stable.official.3cd3caab6`; game version: `1.1.15.4`.
- Engine SHA-256: `e748acc92ee5205dd3cc1754d4422870f6a68ae6e00a16ff88bd3438c957adfd`.
- `artifacts/gameplay-parity-reference-2026-10-15/source-manifest.json` records original and fixture SHA-256 for 3,816 GDScript/scene/resource/project files. The actual `main.gd`, `singletons/run_data.gd`, wave/spawn/combat scripts, registered weapons/items and shop/reward scripts were unchanged.
- Twelve copied files differ: project identity/startup, literal save/log/config path redirects, two startup diagnostic prints, and the player movement fixture hook. Their paths and hashes are in the manifest. The new `parity_fixture.gd` is archived alongside the evidence.
- Regular save/mod-loader/config paths were redirected from literal `user://` paths into the clone's `userdata` directory. Project identity was also changed to `Scrapshift Reference Parity Fixture 20261015`. Startup settings/save creation is confirmed under `BrotatoParity/userdata/user`; fixture boot then enables `DebugService.disable_saving`. The original reference and its actual user saves were not edited. The user's independently opened Godot run was not operated.
- Engine-default CrashReporter logging still attempted the unique fixture identity's user-data path and was denied. This produced two nonfatal startup errors; it did not select the real Brotato save identity. See preserved stderr.

## Explicit fixture overrides

The fixture mirrors Scrapshift's existing supplied-gear campaign check where practical:

- Single-player Crash Zone, Danger 0; 0 starting materials; neutral base stats; no starting items.
- Well-Rounded supplies the character identity required by reference UI, but its effects are deliberately not applied. This corresponds to Scrapshift CampaignCheck's null character rather than an ordinary Well-Rounded start.
- Six tier-II weapons: pistol, SMG, double barrel shotgun, laser gun, knife and spear, added with the real `RunData.add_weapon` API.
- Source `RunData.invulnerable` enabled; deterministic circular movement returned by the copied movement behavior and normalized by the original unit controller.
- Engine time scale 8, physics iteration setting 240, target FPS 120, vsync disabled, master audio muted. The recorded wall time is fixture execution time, not a performance benchmark.
- Seed 771 is set before gameplay. Matching numeric Godot/Unity seeds does not make their random generators, item choices or draw ordering equivalent.
- Real Main combat, wave timers, spawn schedules and transitions execute. Real UpgradesUI callbacks accept the first upgrade and take crate items. Real ShopItemsContainer validates affordability, weapon eligibility and buy delay; its normal purchase callback buys each eligible affordable offer, then BaseShop's normal Go callback advances the wave.
- No enemies are manually killed, no wave timers are shortened, no shop currency is injected and no mock wave/loot implementation substitutes for the source systems.

## Observed result

| Observation | Recorded result |
| --- | --- |
| Actual waves started / timer ends | 20 / 20 |
| Real shop visits | 19 |
| Validated purchases | 38 |
| Upgrade selections | 19 |
| Crate takes | 5 |
| Final state | Won; real end-run scene |
| Final materials / level | 335 / 19 |
| Final item entries | 41, representing 35 unique item IDs |
| Final weapons | Four original tier-II ranged weapons; knife and spear became tier III through real shop combining |
| Wall time from fixture boot | 176.967 seconds |
| PNG viewport captures | 41 |

`trace.json` records wave/shop transitions, sampled simulated and wall times, materials, levels, item counts, complete weapon inventories, purchased IDs/prices and selected upgrades/crates. `summary.json` includes each wave's sampled timer interval and final source-log item stacks. Process scheduling and frame sampling introduce small differences between observed intervals and configured timers; these traces are not frame-accurate timing measurements.

The source's own `DebugService.log_run_info` output is preserved as `reference-run-data.log`. Its final inventory contains two each of Propeller Hat, Pencil, Turret, Mastery, Broken Mouth and Plant. The final end-screen capture visibly displays multiple `x2` item stacks. This establishes that the actual source run acquired and displayed these duplicates; separate formula tests are needed to establish every effect's stacking semantics in Scrapshift.

Viewport readback produced native Godot pixels without desktop inputs. `wave-04-end.png`, `wave-20-end.png` and `end.png` were visually inspected. The last image visibly reads **Run Won — Crash Zone — Danger 0**, with the six weapons, items/stacks and end-run options. Start-of-scene captures may show an early or previous rendered frame; the first wave's start capture is mostly blank and is not relied upon.

Runtime diagnostics are preserved in `godot-stderr.log`: no `SCRIPT ERROR` entries; two startup CrashReporter path errors; one missing optional unpacked-mod folder warning; 80 inherited missing `Attach` node warnings while instancing weapon scenes; and Godot resource/ObjectDB cleanup diagnostics at exit. Successful run completion does not make this a warning-free execution.

## Reproduction and remaining comparison

With the isolated copy retained, launch the exact Godot 3.6.2 executable with:

```text
--path "E:/Codex Projects/Job Search Assistant/tmp/BrotatoParity" --no-window --audio-driver Dummy --video-driver GLES2
```

Use a hidden process with stdout/stderr redirected, and keep its parent shell waiting until exit. The fixture is the final autoload, after reference singletons. Earlier attempts placing it first did not instantiate it correctly; those attempts are not counted as completed runs. This launch overwrites the fixture artifact trace/images, so archive a prior result before rerunning.

The source run is now a concrete comparison baseline for Scrapshift's supplied-gear integration check. Different random offers, purchases and enemy interactions are expected across engines. The fixture does not validate controller hardware, ordinary manual controls, complete unlock persistence, audio listening, normal-run survival balance, every item effect/stack, or equal random outcome sequences. Enemy kills, projectile-release counts and frame timings were not instrumented in this fixture and must not be inferred from the transition counts. A normal full run against the original remains a separate acceptance check.
