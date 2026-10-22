# Browser release - October 22, 2026

## October22 publication follow-up

The user explicitly authorized pushing main and uploading this verified build to
the existing itch.io project. Main was pushed successfully: remote
`MarvelousJade/Scrapshift` main resolved to
`444f29334d1865b8b619c1be4c0454ea20d931b7`. The ZIP still matches the SHA256 below.
Upload completed on October22. itch.io reported `Scrapshift-Web.zip - Success`,
70mb, Today at3:48AM; the saved page reports Updated22October2026@07:48UTC.
The single upload is enabled for browser play. Restricted visibility and password
access remain enabled; embed960x600/fullscreen settings are retained.
Hosted asset `html/19943159/index.html?v=1792655326` loaded the current title,
Rookie/Pistol/Danger0 selection, active wave1 and ordinary defeat with an unlock
popup. Captured warning/error console was empty. The smoke run died before the
movement/pause attempt could establish those controls; local release evidence
remains their acceptance. No hosted full campaign or physical gamepad claim.
See `artifacts/itch-release-2026-10-22/receipt.json` and screenshots.


## Current local release

Gameplay was merged to main, then `codex/crowded-wave-optimization-20261021` was
created, validated and merged back at **8fd3186**. The final Web build uses that
merged source, without `SCRAPSHIFT_WEB_REVIEW`. Local files are **`Builds/Web`**;
upload package **`Builds/Scrapshift-Web.zip`**. It was uploaded on October22.
The earlier folder/ZIP are preserved as `Builds/WebBeforeOptimization20261021`
and `Builds/Scrapshift-Web-before-20261021.zip`.

Unity6000.6.5f1 reported **Succeeded, zero errors/warnings**, process exit0.
Build size is **73,477,798 bytes**; ZIP **73,347,695 bytes**, with `index.html` at
its root. ZIP CRC and every extracted file hash passed. The built output, tested
HTTP copy and packaged folder match across all five files. All2629 source inputs
match the prebuild manifest. ZIP SHA256:
`9a074d0460a548729788aac30f4a4ce2631892cc8004bb772ae4e9a8f1099fdc`.

Compared with this pass's same-content diagnostic baseline of77,628,486 bytes,
the final build is **5.35% smaller**; compressed data falls from70,884,507 to
66,749,502 bytes (**5.83%**). This is an observed whole-build comparison, including
removal of diagnostics. Direct per-asset evidence shows the two weapon atlases
reduce packed payload by **9,437,184 bytes (9 MiB)**. This does not establish total
RAM savings. The older October14 ZIP below contains different content and is
smaller than this new release; it is not the optimization baseline.

### Accepted settings and restoration

- Keep the existing optimized release `BuildTimes` code preset. LTO remains
  optional: it enlarged Wasm and has no valid measured Web speed-gain proof here.
- Temporarily use SFX quality0.65 with authored codecs/load/preload/sample-rate/
  channel settings; music retains0.45/compressed-in-memory. The four canonical
  preview recordings remain excluded. Source recordings/approved pixels remain
  unchanged. No subjective compressed-audio listening acceptance is claimed.
- Temporarily use Web DXT5 on only the two compatible weapon atlases. Preserve
  their dimensions, pivots, readability and sprite registration. Skip21 other
  audited textures; a rejected resizing attempt was safely restored. The tested
  Chromium browser reports support and two actual DXT5 textures. Other devices'
  decompression/fallback memory behavior remains unverified.
- All **464 audio** and **23 texture** import records restored; source hashes are
  unchanged. The prior code preset also restored. Portable per-path audio hash
  summaries are committed; the147 MB raw importer receipts remain local.

### Current browser checks

The final nonreview release was served on isolated localhost port8787 at
`/release/`, with the temporary viewport override reset. This is local Chromium
evidence, not a claim about every browser or an actual itch.io iframe.

- Keyboard title entry; mouse Rookie/Pistol/Danger0/Start; ordinary wave1 combat;
  D/A movement; P pause; mouse Resume; Escape closes Options. Pause inventory
  shows exactly **Weapons (1/6)**. Health15→12→11 and materials30→31→32 establish
  an ordinary short run, not an invulnerable full campaign.
- Moving focus from the playing canvas to the page control pauses. Returning
  focus leaves the run paused. The pause timer stayed fixed through inspection.
- Fullscreen entry/exit passed. Actual stage and canvas bounds are1920x1080;
  the inspected image fits the screen. Embedded canvas is1280x673.
- Saved master volume **42%** appears in the actual release Options before and
  after a final release reload. Diagnostic progression probe1/master0.42 also
  persisted across same-URL diagnostic build replacements. This is not a new
  organic-unlock campaign or cross-origin migration test.
- Final console warnings/errors: **0** before and after reload. No save-test or
  benchmark buttons appear. First observed final-build load: **2.30 seconds on
  localhost**, not an internet loading-speed guarantee.
- The separate corrected diagnostic records music stopped before Start, playing
  during the run,323 SFX requests and a peak16 voices. Final release has no audio
  console errors. These are source/playback-state checks, not subjective listening.

The corrected, normal-exception diagnostic crowd sampled **1293 frames**, mean
**7.738 ms**/p95 **11.000 ms** (about129 FPS), with at least200 enemies, peak204,
six synchronized weapons and no console warnings/errors. Actual canvas1280x613;
ten-second sample after two-second warmup; no concurrent Unity build. It uses
original SFX imports and is separate from final-release ordinary gameplay.
Earlier current-pass crowd FPS samples are invalid because their knife view was
not initialized; [the fixture investigation](web-crowd-fixture-2026-10-21.md)
records the correction. This is not a before/after Web speed ratio or full run.

Evidence: `artifacts/optimization-2026-10-21/web-final-package.json`,
`web-final-verification.json`, `release-source-manifest.json`, `web-final/`,
`browser/final-release.json` and inspected images. See
[experiments and rejected attempts](web-build-experiment-2026-10-21.md).
Firefox, Safari, mobile, storage-denied/private modes, physical browser gamepads
and an actual hosted iframe remain untested. R40's physical Xbox acceptance is
Windows evidence. Publication and sound licensing retain their separate scope.

## Historical release notes

## October 14 release update

Rebuilt the itch.io WebGL package from the final `main` branch, which includes the crowded-combat lookup optimization. The Web release omits the photo/video showcase buttons and their F5-F10 shortcuts; the desktop build retains those local capture tools. The Web package is a release build without `SCRAPSHIFT_WEB_REVIEW`. Use the refreshed `Builds/Scrapshift-Web.zip` for upload.

The rebuilt package is 71,842,784 bytes (about 68.5 MiB). Unity reported zero build errors and warnings, and the ZIP has `index.html` at its root. The combat CPU optimization was measured in a Windows headless player; WebGL performance and the itch.io iframe still need direct browser measurement. This release is larger than the October 13 package because of subsequently added game content, so the earlier 36.52 MB figure below does not describe this build.

The desktop-browser build is in `Builds/Web`; `Builds/Scrapshift-Web.zip` has `index.html` at its root for an itch.io HTML game upload. Serve the folder over HTTP, not `file://`. Nothing has been published.

## Changes

- Browser progression uses Unity PlayerPrefs/IndexedDB with the versioned `scrapshift.progress.v1` key. Unlocks save immediately; counters checkpoint every five seconds and on focus loss. Identical progression snapshots are not rewritten. Invalid progression is preserved rather than overwritten. Windows retains its existing atomic JSON-file saves.
- Settings save immediately, including the previously deferred SFX volume and mute controls. These are local browser saves, not cloud saves or mid-run resume. A different origin/browser/profile has separate progress; clearing site data removes it.
- The canvas resizes to its container with device pixel ratio 1. Desktop resolution switching and F11 interception are disabled on Web. The page provides fullscreen and exit-fullscreen controls; P pauses without competing with browser Escape behavior.
- Keyboard capture is restricted to the focused canvas; scrolling keys and right-click menus do not interfere with play. Explicit window, visibility, and canvas focus events feed Unity's pause/mute preferences. A native Quit button is omitted on Web.
- Music still starts only when the first run starts. The Web build temporarily imports music compressed in memory at quality 0.45, using Unity's Web AAC conversion, then restores the desktop import settings. Web platform-specific AudioImporter overrides were rejected by this Unity version, so the build uses the supported default-settings path in a try/finally.
- Gzip with decompression fallback works on ordinary static hosting without requiring custom compression headers. Three unused title images were moved out of Resources into Content/MenuSource, retaining their GUIDs and source files. Enemy/tree/burning counts now use one allocation-free pass instead of three temporary lists per frame.

## Validation

Evidence is under `artifacts/web-review/`.

- Unity 6000.6.5f1 Web/IL2CPP release build: succeeded, zero errors/warnings. Test-only commands are excluded from release by the `SCRAPSHIFT_WEB_REVIEW` compile define.
- 302/302 EditMode regression tests passed after the runtime changes. Source and isolated validation project match byte-for-byte across Assets, Packages and ProjectSettings; see `artifacts/validation-copy.json`.
- Browser save test: progression counter 1 and master volume 0.42 survived reload. Test storage was isolated on localhost port 8765; the release preview uses port 8766.
- Real browser inputs traversed character/weapon/difficulty selection, started a run, moved the player, and paused/resumed. Pausing also worked from the level-up screen. Release focus-loss pause was verified by moving focus to the fullscreen control during play.
- Fullscreen initially exposed an intrinsic canvas sizing bug. Absolute positioning fixed it. The final 1920×1080 fullscreen pause screen fits fully, and its Exit fullscreen control returns to the embedded view.
- Audio instrumentation confirmed silence before Start, active music after Start, and continuing playback through pause and looping. The final AAC build starts gameplay without browser audio errors. This is playback-state validation, not a subjective listening review.
- Browser console checks returned no warnings/errors during the tested gameplay. Final release controls contain no benchmark/save-test buttons.

### Size and performance

The first compressed Web output was 41.95 MB. The final output is approximately 36.52 MB, a 12.94% reduction. See `loading-size.json` for byte counts and decoded file sizes. A fresh-origin localhost release load took 1.35 seconds; this is not an internet download-speed claim.

The release-optimized diagnostic build maintained at least 200 enemies (peak 204) and six weapons. It sampled ten seconds after a two-second warmup, then paused automatically: 765 frames, 13.07 ms average frame interval, 18 ms p95 (about 76.5 FPS average). The browser viewport override was 1920×1080; browser zoom and the diagnostic footer yielded an actual game canvas of approximately 1745×877. This synthetic crowd adds replacement enemies as others die and uses invulnerability to keep the sample running. It is not a full late-game campaign or low-end hardware guarantee.

Validation used the local Chromium-based in-app browser on Windows. Safari, Firefox, physical controllers, mobile devices, storage-denied/private modes, and an actual itch.io iframe have not been tested. Test the uploaded draft on the intended public browsers before release.

## Rebuild

Run `scripts/build-web.ps1` with Unity Web Build Support installed. Use `-ProjectPath` to build an isolated project. Add `-Review` only for diagnostics; it exposes explicit save and crowd-test buttons and must not be the uploaded build. ZIP the contents of `Builds/Web`, not its enclosing folder. On itch.io choose an HTML game and mark the ZIP as playable in the browser. This work does not publish the page.

Reference: [Unity Web audio documentation](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/audio), [itch.io HTML game documentation](https://itch.io/docs/creators/html5).
