# Combat performance optimization and interview notes

October 13, 2026. Branch: `codex/performance-optimization`, based on committed `main` at `a357150`. Work was performed in a separate Git worktree while other chats continued wearable art and explosion/projectile changes. Those concurrent changes are outside this measured baseline.

## Measured result

The first pass removes repeated collision-table searches, allocation-heavy weapon-set queries, and repeated command-line argument copying. Enemy counts, hitboxes, damage rules, and simulation order are unchanged.

| Durable enemies | Baseline mean CPU ms/step | Optimized mean CPU ms/step | Reduction | Baseline p95 range | Optimized p95 range |
| --- | ---: | ---: | ---: | ---: | ---: |
| 100 | 17.48 | 5.36 | 69.3% | 20.68–21.57 ms | 6.93–7.87 ms |
| 200 | 55.66 | 12.66 | 77.3% | 69.75–69.84 ms | 17.20–17.73 ms |

Means are averages of two runs of 360 measured simulation steps each, after 120 warmup steps per scenario. Hardware: AMD Ryzen 5 3600; Unity 6000.6.5f1, Windows Mono release player, headless. Each scenario uses seed 771, fixed 1/60-second simulation steps, six tier-II weapons, four enemy types, invulnerability, and durable enemies so the population stays constant. The 200-enemy case is a stress test beyond the normal 100-enemy cleanup threshold.

The stopwatch brackets `UpdateFeedback` and `Simulate`. It excludes GPU rendering, UI, startup, and scenario setup. These numbers establish CPU savings for this workload; they do not establish a whole-game FPS improvement or performance on WebGL, mobile, or other hardware. Background work can affect timings; the repeated run ranges are retained rather than claiming statistical significance. All three changes were measured together, so their individual contributions have not been isolated.

Fixture mode uses default options and bypasses `PlayerPrefs`; ordinary interactive settings reads and user loadouts have not been separately benchmarked.

Raw evidence:

- [Baseline run](../artifacts/performance/baseline/performance.json) and [baseline repeat](../artifacts/performance/baseline-repeat/performance.json).
- [Optimized run](../artifacts/performance/optimized/performance.json) and [optimized repeat](../artifacts/performance/optimized-repeat/performance.json).
- [EditMode report](../artifacts/performance/editmode.xml): 361 passed, zero failed or skipped, including four new regression cases.
- [Final source/build check](../artifacts/performance/final/performance.json): after restoring test-generated asset metadata and rebuilding, the reusable script completed both scenarios at 5.32 and 13.34 ms mean CPU per step. This additional validation run is not included in the two-run comparison above. The final Windows build succeeded.

Both variants produced exactly matching final position checksums, remaining enemy health, and shot counts for each scenario. The 100-enemy case fired 81 shots; the 200-enemy case fired 78. These aggregate checks support behavioral equivalence for this workload, but do not prove that every possible gameplay state is identical. Tests additionally compare every imported collision radius/offset and weapon-set thresholds from counts 0 through 24, including duplicate effects, floating-point accumulation, fallback lookup, and live inventory changes.

## Changes and reasoning

### 1. Index the immutable enemy collision table

`CombatGeometry.Profile` previously called `Array.Find(profiles, p => p.id == id)` every time it needed a collision radius or offset. The lambda captures `id`, creating allocation pressure. The crowd separation loop asks for two radii for each enemy pair: roughly 19,800 profile requests per step for 100 active enemies, or 79,600 for 200, before other collision queries.

The imported table is now loaded once into an ordinal string dictionary. Lookups reuse it without a captured predicate. The first matching entry still wins, and existing fallbacks remain. This changes profile lookup from a linear search through the enemy table to expected constant time. The crowd loop itself is still quadratic in enemy count.

Tradeoff: a small dictionary consumes extra persistent memory. This is appropriate because the source table is immutable during the current domain. If live collision-data editing is introduced, the cache will need an explicit reload path.

### 2. Remove transient LINQ work from weapon-set queries

`WeaponSet.Bonus`, `WeaponSets.For`, and `WeaponSets.Bonus` now use direct loops for frequent gameplay queries. They preserve threshold selection, primary-set lookup, multiple matching effects, and set iteration order. The effect total uses a double accumulator before returning float to preserve the previous `Enumerable.Sum(float)` behavior.

This does not cache mutable inventory totals. Purchases, combining, recycling, or replacing a weapon continue to affect the next query immediately. Setup and description formatting retain LINQ where it is outside the hot path.

Tradeoff: the loops are longer than the LINQ expressions, so regression coverage protects threshold and arithmetic behavior. The asymptotic complexity of these set scans is unchanged; the improvement removes repeated iterator/delegate overhead.

### 3. Compute fixture detection once

Every `GameSettings.Value` and `Flag` call previously called `Environment.GetCommandLineArgs` through `IsFixture`, repeatedly copying process arguments. The process arguments cannot change, so fixture detection is now a static initialized property. Normal settings still read `PlayerPrefs`; changes made in the options menu remain immediately visible.

Tradeoff: this relies on immutable process arguments, not immutable user settings. No user setting values are cached.

## A measurement bug caught during the work

The first benchmark tried `GC.GetAllocatedBytesForCurrentThread`. On this player it returned zero despite code paths known to allocate. That was not accepted as proof of zero allocations. The harness now uses Unity's `ProfilerRecorder` counter, validates it with a 64 KiB allocation, and reports `allocationCounterValid: false` with `allocatedBytesPerFrame: -1` if the positive control fails.

The final release-player runs did not expose a usable allocation counter. Therefore this report makes no measured bytes-per-frame or GC-pause reduction claim. Removal of the allocation-producing code is visible in the diff; its memory effect should be quantified in a supported profiler capture later. CPU timing is measured separately and remains valid. Any future allocation figure is for the whole frame, whereas the stopwatch covers only the named simulation methods.

Unity documents the [ProfilerRecorder API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html) and [memory counters](https://docs.unity3d.com/6000.0/Documentation/Manual/ProfilerMemory.html). Check counter availability and validate the instrumentation on the actual target.

## Reproduce it

1. Use a separate worktree at the desired revision. Close Unity for that worktree before starting a batch job. The final benchmark harness must be identical in both variants; apply the three production-file diffs only to the optimized variant. The earlier benchmark commit alone predates the memory-counter correction.
2. Build the existing scene without regenerating content:

   ```powershell
   & 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.com' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod Scrapshift.Editor.PerformanceBuild.Build -logFile performance-build.log
   ```

3. Run `./scripts/benchmark-performance.ps1 -OutputDirectory artifacts/performance/current`. The script checks process success, report freshness, both enemy populations, frame counts, and weapon activity.
4. Repeat sequentially with baseline and optimized players on the same machine. Compare means and p95 values, then verify that enemy counts, shots, position checksums, and health checksums match. Avoid simultaneous benchmark players or builds.
5. Use `./scripts/verify.ps1` for the EditMode suite. Some existing tests rewrite imported asset metadata; keep those generated rewrites out of the performance commit and rebuild from the intended source afterward.

The benchmark entry point is enabled only by `--performance-review <folder>`. Ordinary runs do not enter the fixture. It uses the project's existing fixture settings isolation and does not save benchmark progress into the user's normal run.

## Interview questions and answers

Use these as rehearsal notes. Implementation and initial measurements were performed with Codex under the user's direction. Describe the steps you personally reviewed, reproduced, and understood accurately; do not present assistant-run profiling as independent hands-on work.

**Tell me about a performance problem in this project.**

The crowded combat loop repeatedly looked up collision profiles, recalculated weapon-set bonuses using LINQ, and copied command-line arguments for settings reads. A repeatable standalone CPU benchmark measured about 17.5 ms per step at 100 enemies and 55.7 ms at 200. Reusing immutable lookup data and replacing frequent allocation-heavy queries reduced those to about 5.4 and 12.7 ms. The same seeded scenarios produced matching combat aggregates, and all 361 tests passed. These are simulation timings, with rendering excluded.

**How did you find the bottleneck? Did you use the Unity Profiler?**

This pass began with code inspection of frequently executed combat paths and then measured a controlled standalone workload with a stopwatch. A Unity profiler counter was attempted for allocations, but its positive control failed in the release player. There is no captured Profiler timeline proving each function's individual share. The measured evidence supports the combined change. A development-player Profiler capture is the next step for individual attribution and allocation call stacks.

**Why does a small lookup matter so much?**

The frequency multiplies its cost. A lookup inside an all-pairs enemy loop can run tens of thousands of times per simulation step. Reducing the work of that repeated operation can matter more than optimizing a much larger operation that runs once at startup.

**Why use a dictionary? What is the complexity?**

The previous profile search was linear in the number of profile entries. Dictionary lookup is expected O(1), with extra memory and hashing overhead. The outer pairwise separation algorithm remains O(N²). This change improves its inner operation; it does not make the whole combat update O(N).

**Is LINQ always slow?**

No. It improves readability in many setup, UI, and tooling paths. Here several LINQ queries ran repeatedly inside gameplay stat evaluation, creating temporary iterators and captured delegates. Direct loops removed that overhead while preserving the result. LINQ remains in paths that were not part of this measured fix.

**Why didn't you cache all player stats?**

Player stats depend on inventory, health, movement, temporary effects, and other changing state. Broad caching would require reliable invalidation for every mutation. This pass caches only immutable collision data and process fixture detection, while computing mutable set totals directly without temporary query objects.

**How did you avoid changing gameplay while optimizing?**

No enemy cap, spawn timing, hitbox, damage formula, or update order changed. Resource-based tests verify all collision profiles and set thresholds. A rounding test catches an easy mistake when replacing a floating-point LINQ sum. The seeded runtime scenarios also match shots, enemy positions, and remaining health. This combination is stronger than either a green build or a timing result alone.

**What do mean and p95 tell you?**

The mean describes average cost across measured steps. P95 is the threshold below which approximately 95% of those step timings fall, so it exposes slower steps that an average can hide. We retain two runs and their ranges. A longer capture on an otherwise idle target machine is needed for stronger tail-latency conclusions.

**Does 5.4 ms mean the game runs at 185 FPS?**

No. The benchmark excludes rendering, UI, and other frame work. A 60 FPS frame has about 16.67 ms for the entire frame. The measurement shows that combat uses less of that budget; it does not establish the final rendered frame rate. The 200-enemy optimized p95 still exceeds that full-frame budget in these runs.

**Why not start with object pooling, jobs, or a spatial grid?**

Those are candidates for later measurements. Pooling requires careful reset and lifetime handling. Jobs require restructuring data and respecting Unity's main-thread APIs. A spatial grid must preserve nearby-collision behavior as units move, spawn, and die. This pass first removed repeated work with small changes and direct behavioral checks. A grid is a sensible next investigation if pairwise separation remains dominant.

**What did you learn from the failed memory counter?**

A plausible number is not enough. Zero allocations contradicted known allocating code, so the instrumentation needed a positive control. The final report marks unavailable memory measurements explicitly. Performance claims must distinguish measured results from conclusions inferred from code.

**How did you work safely while other changes were in progress?**

The optimization branch uses a separate Git worktree based on a committed revision. Builds, generated files, benchmarks, and documentation stay there. The performance commit does not switch or rewrite the active art/combat working copy. Integration still needs review and a rerun against the combined revision after the other branches finish.

**What would you optimize next?**

Capture a rendered development build with the Unity Profiler, confirm CPU versus GPU limits, and inspect the remaining all-pairs separation loop, conditional stat evaluation, transient effects, and UI allocations. Choose the next change from that evidence. Do not claim that unmeasured pooling, Burst, or spatial partitioning has already improved this project.

## Hands-on rehearsal

Run the benchmark yourself, open both JSON files, and calculate one percentage reduction. Trace a radius lookup from the enemy pair loop into `CombatGeometry`. Explain why fixture state is safe to cache and health-dependent stats are not. Run the floating-point regression test and explain why a float accumulator would lose the small middle term. Finally, describe one limitation of the benchmark before quoting its numbers.
