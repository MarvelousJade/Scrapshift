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
run exited0 with both scenarios complete. Optimization comparisons are pending.

Codex implemented and measured this work under the user's direction. The user
personally performed the separately documented physical Xbox test.
