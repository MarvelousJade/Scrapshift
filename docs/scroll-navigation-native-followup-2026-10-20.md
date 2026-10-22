# Scroll navigation native follow-up — October 20, 2026

Branch: `codex/gameplay-parity`. R37's opt-in software input fixture ran through actual player Update/OnGUI. Physical Xbox testing was skipped under the user's instruction. No hardware or ordinary OS focus acceptance is claimed.

## Result and inspected evidence

`artifacts/native-followup-2026-10-20/scroll-navigation-r37/scroll-navigation.json` reports **Passed**; `process.json` records exit 0. Independent inspection checked every semantic result and all four PNG captures, rather than treating screenshot existence as visual acceptance.

| Check | Observed result |
| --- | --- |
| Actual initialization | Production start/hook present. |
| Navigation | Virtual D-pad reached `holdReroll`, then `keepLock`, without assigning an activation focus index. |
| Scroll edge | Vertical scroll changed from 0 to 41. |
| Immediate A after edge | Semantic activation `keepLock`; actual setting changed On → Off. |
| Settled A control | Semantic activation `keepLock`; actual setting changed Off → On. |
| Other settings | Unchanged. |
| Native captures | Initial Gameplay tab; white Hold-reroll focus before scrolling; white Keep-locked focus showing Off after immediate A and On after settled A. |

Both activations were recorded on Layout events with `activationPending=true`, at frames 285 and 332. The final trace contains 5,932 rows. The proposed lost-confirmation behavior was **not reproduced** in this sequence.

`visual-review.json` in the R37 fixture folder records the independently reviewed images and hashes. This review concerns the four settings captures only; R37's separate selection-heading change has its own evidence.

## Coordinate discrepancy: measured, but not a failed activation

The screenshots establish a real distinction between rendered canvas coordinates and raw `GUIUtility.GUIToScreenPoint` values returned inside the scroll view at this tested 1280×720 resolution:

- The root transform has scale 0.8 and horizontal canvas offset 64.
- The first option's rendered strip begins around **(252, 124)**, agreeing with the same-event root formula `root + scale × (viewport + local − scroll)`.
- The inside-scroll conversion instead records **(299, 155)** for that option, with the correct scaled size, giving a positional discrepancy of **(47, 31)**.
- After scrolling 41 logical units, Keep-locked renders around **(252, 538)**. The root formula predicts `(252.0007, 537.6000)`; the inside-scroll conversion records `(299, 560.4000)`.

The raw registered rectangles therefore should not be assumed to describe the rendered pixel bounds across containers. That observation explains why a root-canvas coordinate cannot simply replace an inside-scroll registration in older fixture lookup code. It does **not** establish that the focused control activates the wrong action: these receipts show the visible highlight, semantic button identity and actual setting change agreeing in both tested confirmations.

R37 contained no functional navigation correction. The original holds a Control object as focus identity and calls its parent ScrollContainer's `ensure_control_visible` (`ui/menus/global/focus_emulator.gd:239–249,429–432`); Scrapshift's current focus matching instead uses registered rectangles. The pixel-backed geometric discrepancy therefore warrants correcting the registered coordinates even though this tested confirmation was not lost.

## Focused coordinate correction and R38 acceptance

The follow-up implementation captures each navigation scroll view's origin and unit axes in its parent coordinate context **before** entering `GUI.BeginScrollView`. It keeps the scroll position actually returned by that call as the rendered snapshot. Navigation rectangles inside the view are then transformed from that snapshot instead of using the mismatched raw inside-scroll conversion. A stack supports nested views; each paired end restores its parent context. The stack is reset at the start of each OnGUI event.

Options, shop inventory/stat/detail views, pause/end-summary inventories, intermission inventories and Codex views use the paired wrappers. Root controls continue to use the original root conversion. `EnsureNavigationVisible` still adjusts scrolling for a later GUI pass; it does not retroactively change the stored transform for the current rendered view. The separate selection-heading change is untouched.

The opt-in fixture now records both corrected registered rectangles and the raw inside-scroll conversion. Its independent same-event root formula must agree with registered option bounds within 0.25 pixels, in addition to the actual D-pad/A semantic checks.

Both R38 packaged checks **Passed/exit 0**, independently inspected in `scroll-navigation-r38-80` and `scroll-navigation-r38-100` under the same artifact directory:

| Resolution / canvas scale | Coordinate check | Semantic check |
| --- | --- | --- |
| 1280×720 / 80% | Maximum error 0.015271 pixels across 4,015 option repaint rows. Corrected Keep-locked position `(252, 537.6016)` agrees with root prediction and rendered strip; raw conversion remains `(299, 560.4000)`. | D-pad reaches Hold-reroll then Keep-locked; scroll 0 → 41; immediate and settled A activate `keepLock`, toggling On → Off → On; other settings unchanged. |
| 1440×900 / 100% | Maximum error 0 pixels across 4,015 option repaint rows. Corrected and raw positions agree at this scale. | The same focus, scrolling, semantic activation and unchanged-other-setting checks pass. |

Each final trace contains 5,868 rows. All eight native PNGs were visually inspected: Gameplay tab focus, pre-edge Hold-reroll focus, immediate Keep-locked Off focus and settled Keep-locked On focus at both resolutions. Each folder's `visual-review.json` records actual SHA-256 image hashes, geometry metrics and semantic fields. The four 80% captures are pixel-identical to their R37 counterparts: the correction changes registered navigation geometry without changing these rendered screens.

The full R38 suite and broader shop/generic-navigation fixtures are recorded by root integration; this document does not claim pending results. Root-to-scroll transitions across other containers, window movement/resizing, Codex scrolling and inventory scrolling retain their separate acceptance limits; no broad cross-container test result is inferred from one options edge.

## Corrected fixture failure

R36's `scroll-navigation/scroll-navigation.json` failed before the edge sequence. Its raw trace contains exactly 6,000 rows. Focus had progressed normally from Gameplay through the option rows to `optimizeEndWaves`; then the recorder's storage cap also returned before updating the semantic focus observer. Each subsequent frame cleared the observer, so the fixture could no longer recognize `holdReroll` even if navigation reached it.

The fixture correction keeps semantic observation running after the bounded storage limit and records repaint/activation events rather than every ordinary Layout event. An initial screenshot was added before the navigation loop so a later failure still retains rendered coordinate evidence. The earlier invalid 33-character asset GUID was also a fixture/import error, corrected to a unique valid 32-character GUID before this successful build. Neither failure is classified as a gameplay regression.

## Scope and attribution

The options page/tab and starting preferences are staged. The fixture creates an in-process virtual Gamepad, uses IgnoreFocus to preserve synthetic events, and sends actual D-pad/A events through the game's input backend. It does not send OS input. R37's production-file additions were opt-in observation hooks. The subsequent coordinate correction changes registered scroll-view geometry and has the finite R38 acceptance recorded above.

This is a finite software navigation and visual check, not a physical-pad, ordinary-focus, audible-feedback or full-campaign result. Codex agents authored the probe, investigated its failure and reviewed these captures under the user's direction. No independent user debugging or new human playtesting is claimed.
