# Browser crowd fixture correction - October 21, 2026

The review command started a normal one-weapon run, then directly replaced its
inventory with six weapons without rebuilding their scene objects. The knife
accessed its missing `equipmentViews` entry in `TickMeleeAttack`. Repeated native
binding exceptions interrupted simulation. Full stack traces identified that
path; normal start/wave/shop paths already synchronize equipment.

The review-only command now calls `SyncEquipment()` after replacing the loadout.
No production combat guard was added to conceal an invalid fixture. Earlier
baseline/LTO/texture crowd FPS samples are **invalid for performance acceptance**;
their build-size, importer restoration and separate menu/save observations keep
their own scope. The independent Windows performance fixture already synchronized
its loadout, so its matching-checksum CPU comparison is unaffected.

## Checks

- A corrected full-stack-tracing build completed the crowd with no console
  warnings/errors. It sampled44 frames at227.98 ms mean/299 ms p95. This slow
  diagnostic configuration is not the release performance measurement.
- A subsequent build used the normal release exception setting, original SFX
  imports and the corrected loadout. Texture-memory enumeration runs outside the
  timed sample. No Unity build ran concurrently.
- Fresh sample: **1293 frames**, **7.738 ms mean**, **11.000 ms p95**, at least200
  enemies, peak204, six weapons. About129 FPS average. Console warnings/errors:0.
- Requested viewport1280x720; actual game canvas1280x613, document visible. The
  sample spans ten seconds after two seconds of warmup. This is a synthetic,
  invulnerable crowd with replenished enemies, not a full late-wave campaign,
  before/after Web gain or low-end-device guarantee.
- Actual rendered pause/crowd frame inspected. Both weapon atlases are DXT5 on
  this browser; supported format=true. Source dimensions and sprite registration
  are preserved. Initial music is stopped, run music plays,323 SFX calls and a
  peak16 voices are recorded. Playback-state evidence does not establish listening
  quality. Save probe1/master0.42 survived a same-URL build replacement.

Evidence: `artifacts/optimization-2026-10-21/browser/texture-diagnosis-errors.json`,
`corrected-crowd.json`, `release-like-crowd.json`, `release-like-crowd.jpg`, and the
separate `web-corrected`/`web-release-like` build/restoration receipts. The final
release without diagnostic commands still requires its own package/browser check.

Codex diagnosed and corrected this fixture under the user's direction. This is
AI-assisted project work, not a claim of independent user debugging.
