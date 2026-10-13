# Browser release — October 13, 2026

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
