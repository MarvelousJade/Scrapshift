# Crowded-wave optimization - October 21, 2026

Requested priority: crowded-wave FPS and memory, then browser loading/build size.
Branch: `codex/crowded-wave-optimization-20261021`, based on the integrated main.

## Measurement tooling

The opt-in `--performance-review <folder> --performance-rendered` fixture uses
100/200 durable enemies and six tier-II weapons. Each scenario has120 warmup
steps and360 measured steps. The benchmark forces neutral movement, uses automatic
aim from isolated fixture defaults, and does not inject controller events.

Simulation CPU timing excludes rendering. Frame intervals include rendering and
scheduling at one fixed60Hz simulation step per uncapped rendered frame. The
ArenaGame Update/OnGUI are disabled; this is a controlled combat/rendering
benchmark, not normal whole-game FPS or a low-end hardware guarantee.

Managed-used and Unity allocated/reserved snapshots, GC collection deltas, and
frame interval mean/p95/max are reported. Allocation bytes are available only
when the existing64KiB positive control succeeds. In the first baseline it failed,
so `allocatedBytesPerFrame=-1` must not be interpreted as zero allocation.

Baseline source/package hashes and raw measurements are under
`artifacts/optimization-2026-10-21`. The Windows build succeeded; the first rendered
run exited0 with both scenarios complete.

## Accepted runtime changes and results

- Pool the enemy identity snapshot with a separate buffer for each nested call.
  Reverse traversal and live membership checks are unchanged; `finally` clears
  retained enemy references on ordinary completion, terminal returns and errors.
- Hoist the current enemy's immutable body offset/radius outside separation's
  inner loop. Neighbor positions, summation order and arithmetic remain live.
- Lazily construct the prefixed equipment-stat key once per query instead of
  once per eligible trait. Weaponless/custom-only queries still create none;
  health, inventory and trait values are evaluated afresh. No stat-value cache.

Two baseline and two final runs, without concurrent Unity builds, produced:

| Enemies | Mean simulation CPU, before | After | Reduction | Mean rendered interval, before / after |
| --- | ---: | ---: | ---: | ---: |
|100|5.333 ms|4.218 ms|20.9%|5.716 /4.673 ms|
|200|20.012 ms|12.002 ms|40.0%|20.575 /12.511 ms|

Position, health, shots-fired and remaining-shot checksums match exactly for
every scenario/run. `final-comparison.json` binds the input reports by SHA256.
Direct3D12,1280x720, Ryzen5 3600/RX5700. Timing variation remains visible in the
individual receipts; neither these values nor their reciprocals are normal-game
FPS. UI is disabled in this fixture.

Memory optimization removes repeated snapshot arrays and reduces string
construction from N to min(1,N) per query. Peak snapshot capacity is retained for
reuse, with its references cleared. Gen0-reported collection counts were27 to26
at100 enemies and47 to46 at200 over360 measured frames. The byte-allocation
counter remained unavailable. Managed/end and engine memory snapshots do **not**
establish a retained-RAM reduction or absence of leaks; no such percentage is
claimed.

## Validation and packaging

Full integrated suite **1309/1309 Passed**, no skips/failures, exit0. Focused
snapshot/death/item/victory checks **40/40 Passed**. An independent agent reviewed
the traversal, nested-buffer lifetime, arithmetic and lazy-key semantics.
All2626 synchronized Unity inputs matched before the Web build began.
Launch `Builds/Optimization20261021/LaunchGame.cmd`; keep the full folder together.

The native victory fixture reached Won/summary at4.0167 seconds/exit0. Its manual
ReadPixels helper returned blank images with diagnostics on both D3D12 and D3D11;
those attempts are retained and **are not visual acceptance**. Foot attachment
is covered by repeated-pose regression tests. R40's earlier native/hardware/full20
evidence remains attached to its own source, not silently upgraded to this build.

Web audio/build-size experiments and browser acceptance are recorded separately
when complete. No new full20 comparison was performed for this optimization.

Codex implemented and measured this work under the user's direction. The user
personally performed the separately documented physical Xbox test.
