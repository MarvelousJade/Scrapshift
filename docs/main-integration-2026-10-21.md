# Main integration - October 21, 2026

The user requested gameplay work merged into main, then a separate optimization
branch prioritizing crowded-wave FPS/memory before web loading size, followed by
another merge and an updated local WebGL build.

The previously checked-out main worktree had uncommitted asset/import changes.
Its files were preserved in place on `codex/preserved-main-worktree-20261021`.
No stash, reset, or asset replacement was performed there.

Main's presentation/audio/showcase work was integrated with the R40 gameplay
branch. Three agents resolved disjoint combat, support and audio conflicts.
Catalog comparison retained R40 gameplay fields/effect lists; all 205 weapon
profiles and 84 authored muzzle pairs remain intact. New fields describe effects.
Synchronous player death, deferred enemy callbacks, captured item damage and
2/4-second settlement remain in place. Contextual audio was regenerated in the
isolated validation project for the added cues, with unique sound-bank entries.

Integration exposed presentation assumptions in objectless rule fixtures, an
Entrepreneur staged-funding assumption after showcase wave-start effects, and
accumulating victory foot offsets. These were corrected. Returning to selection
also clears queued letter sounds. The first suite failed 45/1269; this attempt is
retained. Focused follow-up passed **208/208**, including every failed case and
three new victory/return checks. A new combined full suite and performance/native
build remain required in the optimization phase; R40 evidence is historical for
this integrated source. No exact gameplay or current WebGL acceptance is implied.

Evidence: `artifacts/optimization-2026-10-21/integration-{suite,focused}.xml`.
The user performed the Xbox test; Codex performed this merge and validation.
