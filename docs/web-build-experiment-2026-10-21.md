# Web build experiments — October 21, 2026

## Scope and current decision

The first six builds below are diagnostic WebGL experiments from the optimization branch, with `SCRAPSHIFT_WEB_REVIEW` enabled. The corrected normal-exception crowd passed. The branch subsequently merged to main8fd3186, and a final nonreview local release also passed its package/browser checks; [current release acceptance](web-release.md) records that separate result. Nothing was uploaded in this pass.

The builder now defaults to `BuildTimes`, authored SFX codecs with quality 0.65, and compression on two eligible weapon atlases. `RuntimeSpeedLTO`, `--web-vorbis-sfx` and `--web-full-stacktraces` remain explicit alternatives; full stack traces are diagnostic only. Baseline is already an optimized release: Development and debugging are disabled, C++ compilation uses `-O3`, and its linker uses `-O2` with assertions disabled. The `debug_WebGL_wasm` intermediate directory name does not establish a debug build. Its dedicated runtime-speed/size/LTO preset flags were false.

## Build measurements

The first three builds succeeded with zero reported errors and warnings. Byte counts below distinguish Unity's build-report total from the compressed files actually in each `Build` folder.

| Diagnostic build | Code preset | SFX import experiment | Build-report total | Compressed data | Compressed Wasm |
| --- | --- | --- | ---: | ---: | ---: |
| Baseline | BuildTimes | Authored settings | 77,628,486 | 70,884,507 | 6,617,162 |
| Quality only | BuildTimes | Quality 0.65 | 76,292,168 | 69,548,185 | 6,617,162 |
| Codec and LTO | RuntimeSpeedLTO | Vorbis request, quality 0.65 | 76,992,632 | 69,548,212 | 7,318,124 |

Subsequent diagnostic builds also succeeded with zero errors/warnings:

| Build | SFX / exception settings | Build-report total | Compressed data | Compressed Wasm |
| --- | --- | ---: | ---: | ---: |
| Initial two-atlas texture build | Vorbis request / full stack traces / old crowd fixture | 74,133,162 | 66,774,316 | 7,232,033 |
| Corrected crowd | Original SFX / full stack traces | 75,468,790 | 68,109,930 | 7,232,039 |
| Release-like diagnostic | Original SFX / normal release exceptions | 74,818,683 | 68,086,658 | 6,605,196 |

These builds use `BuildTimes` and the two eligible compressed weapon atlases. The release-like diagnostic deliberately uses original SFX, so it is not the final default quality-0.65 release configuration. It retains `SCRAPSHIFT_WEB_REVIEW`.

The quality-only experiment reduces the reported build total by 1,336,318 bytes (1.72%); its compressed data reduction is approximately 1.89%. The later Vorbis request adds no useful data saving: that candidate's data is 27 bytes larger. Its Wasm grows by 700,962 bytes. Codec and LTO changed together, so this final comparison does not isolate every cause of a whole-build difference. Unity's Web pipeline performs AAC conversion; source codec labels alone do not establish the shipped representation or savings. See [Unity 6000.6 Web audio documentation](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-audio.html).

Each build temporarily retains music quality 0.45 and compressed-in-memory loading. SFX load type, preload, sample rate and channel settings are preserved. Four approved canonical preview assets are excluded from the SFX changes. In each experiment, all 464 inspected importer records restored successfully, all 464 source-file hashes remained unchanged, and no entry reported a restoration error. The LTO build also restored its previous `BuildTimes` preset successfully. No PCM source recording or approved preview waveform was rewritten.

## Invalidated browser crowd observations

| Earlier diagnostic sample | Frames | Average interval | p95 interval | Enemy count minimum / peak |
| --- | ---: | ---: | ---: | ---: |
| Baseline | 1,439 | 6.949 ms | 7.000 ms | 200 / 213 |
| Codec and LTO | 1,293 | 7.732 ms | 8.000 ms | 200 / 211 |

**These prior browser frame/FPS samples are invalid for performance acceptance.** The crowd fixture replaced the inventory after run initialization without rebuilding weapon views. When its knife attacked, `TickMeleeAttack` dereferenced a missing view, throwing through `UpdateEquipment` and `SimulateCombatStep` and interrupting physics. The samples remain diagnostic records; they cannot establish a speed gain, slowdown or stable six-weapon simulation.

The source correction is fixture-only: `WebReview` now calls `SyncEquipment()` immediately after filling the six-weapon loadout. Normal wave/start paths already synchronize equipment. No production combat null guard was added to conceal the initialization error. The saved stack is in `browser/texture-diagnosis-errors.json`. The fixture correction is committed as `f21c43e`. Its fresh results are recorded below.

The separate native Windows `PerformanceReview` already calls `SyncEquipment()` after its loadout change. Its measured 200-enemy CPU reduction of approximately 40.03% is unaffected by this Web fixture defect; see `final-comparison.json`. That remains a bounded synthetic CPU result.

### Fixture bug note

- Symptom: repeated browser `NullReferenceException` in melee updates, despite six inventory icons.
- Cause: the diagnostic inventory and world weapon views diverged after `StartSelectedRun`.
- Correction: initialize all six weapon views through the existing synchronization lifecycle.
- Verification: the corrected release-like diagnostic completes its crowd window with an empty console error/warning receipt. Earlier browser timing claims remain withdrawn.

The baseline receipt records a 2.69-second localhost load, successful isolated save/reload, initial music silence, and keyboard title/weapon navigation. That is a local browser observation, not an internet download-speed claim. The invalidated crowd reports also record music playing, which does not rehabilitate their performance results. Instrumented playback does not establish subjective audio quality or exhaustive browser controls.

## Fresh corrected browser diagnostic

With the corrected six-weapon fixture, normal release exception settings and no concurrent Unity build, the 1280×613 canvas sampled 1,293 frames over the ten-second collection window: average 7.738 ms, p95 11.000 ms, minimum 200 enemies and peak 204. The sample ended paused with six weapons, 323 played SFX requests, peak 16 voices, music playing and `console: []`. Initial music was silent. Localhost load was 2.07 seconds. See `browser/release-like-crowd.json`.

This is a successful bounded synthetic crowd observation. It is not a before/after browser speed ratio, a full campaign, a low-end hardware guarantee or final release acceptance. The earlier samples cannot be used as its performance baseline. Playback-state counters do not establish subjective sound quality.

A separate corrected full-stack diagnostic also completed without console errors, but sampled only 44 frames at 227.977 ms average. Full stack traces and diagnostics were enabled; retain this as stability evidence only. It is not representative release performance. See `browser/corrected-crowd.json` and [the fixture investigation](web-crowd-fixture-2026-10-21.md).

## Texture preparation guard and bounded candidate

The first texture attempt was rejected before `BuildPlayer`: the preparation guard detected that its imported-dimension/readability/source invariant failed for `FittedWearables/wearables-09.png`. Settings were restored; the failed attempt is preserved under `web-texture-rejected`, with its exception in `web-texture-build.log`. This was a rejected experiment, not an accepted compressed build.

The revised candidate checks existing dimensions against the preserved maximum and DXT5 block alignment before mutation. Of 23 inspected textures, it attempts compression on only `base-weapons-v1.png` and `starter-weapons-v1.png`, skipping the other 21. All three subsequent build receipts record 464 restored audio importers with unchanged source hashes, plus 23 restored texture importers with unchanged source hashes: two attempted and 21 skipped. The browser reports DXT5 support and two DXT5 textures. This does not justify saying all 23 textures were compressed. The two atlases' packed asset bytes fall from 12,650,516 to 3,213,332, a reduction of **9,437,184 bytes**. Baseline compressed data is 70,884,507 bytes versus 68,086,658 in the release-like diagnostic, an observed saving of 2,797,849 bytes (3.95%). Diagnostic settings/fixture versions also differ, so use the per-asset packed delta as direct atlas evidence and the whole-data delta as an observed package result. Corrected crowd stability passed; final release visual/control/save/audio/fullscreen acceptance remains pending.

## Evidence and provenance

- `artifacts/optimization-2026-10-21/web-comparison.json` records build summaries, compressed-file sizes/hashes, audio restoration counts, source-receipt hashes, browser observations and limits.
- All six diagnostic build folders preserve separate build receipts and compact `audio-import-summary.json` files. Each summary records all 464 source hashes and exact UTF-8 before/during/after importer-string hashes, restore flags, and the complete raw receipt SHA-256. The approximately 147 MB raw audio receipts remain local and must not be committed. Texture receipts preserve all 23 entries separately.
- `browser/baseline.json` and `browser/lto.json` preserve the bounded measurements above.
- `runtime-source-manifest.json` preserves the original committed 2,626-input Windows checkpoint byte-for-byte. `final-package.json` now explicitly references this immutable snapshot, rather than the subsequently updated Web candidate manifest. Package file hashes are unchanged.
- `scripts/sync_validation_project.py --manifest release-source-manifest.json` can write a distinct future release manifest. Synchronization rejects extra, missing or mismatched clone inputs; it does not silently delete them.

## Final local release

Main8fd3186 produced a nonreview release with default quality-only SFX and the two
eligible atlases: **73,477,798 bytes**, zero errors/warnings, exit0. This is
4,150,688 bytes/5.35% below the current-source baseline; compressed data is
66,749,502 bytes (5.83% below baseline). All464 audio and23 texture records restore.
The ZIP is73,347,695 bytes; its CRC and every extracted hash pass. Built, browser
and packaged copies match across all five files, and2629 source inputs match the
prebuild manifest. These whole-build savings include omitted diagnostic code;
the9MiB packed atlas delta remains the direct texture evidence.

Actual release controls, an ordinary wave1 pistol run, pause/focus, fullscreen,
42% saved volume after reload and zero console errors passed. Source/playback
audio evidence keeps its diagnostic/listening limits. See `web-final/`,
`web-final-package.json`, `web-final-verification.json`, `browser/final-release.json`
and [the release record](web-release.md). The release-like crowd is separately
verified; neither result is a fresh full20 campaign or universal-browser claim.
