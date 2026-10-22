# Late-wave allocation investigation - October 17, 2026

## Observation and scope

The completed R21 campaign passed its 18 saved-trace checks, but combat windows took 1220.737 wall seconds for 1050.200 active simulation seconds, a 170.537-second gap. This is an observed slowdown, not an FPS benchmark or proof of an engine clock defect. The supplied Godot 3.6 loop also limits catch-up physics work and discards excess idle time after large stalls. Changing the game timer to wall time would therefore need separate source evidence.

R21 remains frozen at `Builds/GameplayParity20261017R21/LaunchGame.cmd`. New profiling helpers and markers are separate source changes; they are not retroactively covered by R21's source manifest.

## Initial CPU capture

Development build: `Builds/GameplayProfile20261017/Scrapshift.exe`, zero build errors/warnings. No Deep Profiling. A synthetic fixture stages the completed R21 inventory (42 item copies and 17 upgrade deltas), 134 enemies, 350 hostile projectiles, 50 friendly projectiles and material drops. Enemies have staged high HP; the player is protected. An empty-world baseline uses the same inventory. This is a diagnostic stress fixture, not a normal-play performance comparison with the original.

| Capture | Sampled player frames | Median frame interval | Update allocated bytes | Update calls |
| --- | ---: | ---: | ---: | ---: |
| Empty-world baseline | 711 | 16.704 ms | 620,088,310 | 711 |
| Synthetic crowded world | 16 | 769.094 ms | 2,407,347,742 | 16 |

The crowded export reports 33,286,955 GC allocation events, 2,083 collection-marker calls and 1,205 ms in GC. `ArenaGame.Update` totals 12,568 ms; `OnGUI` totals about90 ms. Inclusive marker totals overlap and cannot be added. The raw profiler overhead itself is part of this diagnostic capture, so these intervals must not be reported as ordinary R21 FPS.

Evidence: `artifacts/late-wave-profile-2026-10-17/allocation-comparison.json`, each capture's `crowded-profile.json` and `cpu-profile.json`, raw captures and build/source manifests.

## Attributed cost and verified focused correction

A second short marked capture attributes 1,589 ms and 322 MB to enemy updates, then414 ms and65.9 MB to projectile updates across24 simulation ticks (three rendered frames). A separate 1,000-call microbenchmark measured `Stat(enemy_speed)` at44.86 ms, `SetStat` at40.45 ms, conditional stats at2.04 ms and linked stats at1.32 ms. This supports correcting repeated weapon-set work first. The Unity Mono per-thread allocation counter returned0 despite collections; those microbenchmark byte fields are unavailable evidence, not proof of zero allocation. Profiler marker bytes remain the actual allocation evidence.

The correction replaces captured LINQ predicates/iterators in weapon-set membership and bonus calculations with direct loops. Acceptance checks compare all catalog memberships and seeded mixed/duplicate inventories with the previous calculation, plus first-match identity, duplicate effects, floating accumulation, count clamping, legacy fallback and missing-row errors. It does not cache mutable stat results. The complete suite passed1031/1031 (attempt8). Independent review confirmed supported-input behavior is preserved.

The remaining marked cost was geometry lookup inside quadratic crowd separation: each pair queried body/radius data through a captured `Array.Find` predicate. `CombatGeometry` now builds an ordinal dictionary once from the immutable resource, retaining the first row and null-query fallback. Three regressions compare every authored radius, offset and layer (38 IDs) and missing-ID behavior. `GameSettings` caches only immutable process-argument classification; live option values are still read normally.

A final capture with no concurrent Unity build reports enemy allocations falling from13,417,151 to70,409 bytes per simulation tick, and all main-thread allocation markers from18,256,944 to133,567 bytes per tick (99.27% lower in this fixture). Enemy CPU total falls66.22 to10.12ms per tick. See `final-comparison.json` for exact denominators24/32/115, hashes and caveats. The short runs have different evolving world states and profiler overhead; this is diagnostic evidence, not ordinary FPS or a reference-engine performance claim. One exploratory geometry capture overlapped a Unity build and is excluded from timing comparisons.

R22 normal build succeeded with zero errors/warnings. The final R23 normal-speed20-wave campaign completed Won/exit0. Its combat windows took1049.485831 wall seconds versus1050.199951 active seconds (excess-.714120), compared with R21's170.536661-second excess. Small negative gaps reflect trace callbacks; the runs had different inventory/enemy/boss outcomes, so this is not an identical-workload FPS benchmark. Reproduce with `scripts/compare_campaign_timing.py`; exact source-hashed output is `artifacts/full-verification-2026-10-17/wall-timing-r21-r23.json`. R22 virtual navigation attempts are preserved: first menu confirmation failed without foregrounding; the second reached the shop but synthetic Y state reset during its hold. The opt-in R23 virtual fixture uses IgnoreFocus and passed17 snapshots/exit0; this does not change ordinary gameplay focus behavior or verify physical hardware. The complete suite passed1031/1031 again after that fixture change (attempt9). Broad stat caching is inappropriate without invalidation: health, materials, movement, live counts and public inventory mutations can change results during combat.

Codex performed this investigation. The user requested the parity work and supplied the reference; no personal profiling work by the user is implied.
