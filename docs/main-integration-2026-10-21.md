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
three new victory/return checks. The subsequent combined suite passed **1309/1309**
with no failures/skips and exit0. Focused optimization/death/item/victory checks
passed **40/40**. The Windows build and measured optimization are documented in
`crowded-wave-optimization-2026-10-21.md`. R40 campaign/hardware evidence remains
attached to its own source. Current Web release acceptance is documented separately.

Evidence: `artifacts/optimization-2026-10-21/integration-{suite,focused}.xml`.
The user performed the Xbox test; Codex performed this merge and validation.

Gameplay merged into main at992e4d2. The requested optimization branch then
measured/fixed combat work and Web resource handling, and merged back at8fd3186.
The tracked remote README history is also integrated, preserving the video/demo
presentation and newer verification notes. The final local Web release was built
from8fd3186 and its tested/packaged file hashes match. See [Web release](web-release.md).
No push or account upload was performed in this pass; the separate dirty worktree
remains preserved on its named backup branch.
