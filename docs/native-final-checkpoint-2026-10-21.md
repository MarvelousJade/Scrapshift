# Native and build integrity checkpoint — October 21, 2026

Branch: `codex/gameplay-parity`. This delegated review read existing evidence only. It launched no player, editor, desktop automation or controller input. The detailed receipt is [final-integrity-review.json](../artifacts/native-followup-2026-10-20/final-integrity-review.json).

## R40 build and tests

- Independently recomputed SHA-256 for **634 recorded source/settings/import inputs** in both this checkout and the isolated Unity clone: zero mismatches against `build-r40-source-manifest.json`.
- Independently recomputed all **159 recorded packaged files** in `Builds/GameplayParity20261020R40`: zero mismatches against `build-r40-manifest.json`.
- Build summary: Succeeded, **zero errors/warnings**, 401,850,858 bytes.
- `suite-r40.xml`: **1,286/1,286 Passed**, zero failures/skips, duration 69.8165485 seconds.
- `campaign-policy-focused.xml`: **12/12 Passed**, zero failures/skips, duration 0.3157129 seconds.

This verifies the finite manifest inventory, not every conceivable project input. The generated catalog has its own normalization comparison. The source/build manifests are immutable checkpoint evidence; later edits require renewed checks before attributing their behavior to R40.

## Native evidence retained with its actual build

The R38-to-R40 source manifest difference contains only `Runtime/CampaignCheck.cs` and the new `Tests/Editor/CampaignLoadoutPolicyTests.cs` plus its meta file. Ordinary native gameplay, menu and rendering implementations therefore have no additional recorded changes in this interval. Earlier native results remain labelled with their actual executable; they are not represented as freshly executed R40 UI tests.

| Actual evidence | Independently read result | Scope |
| --- | --- | --- |
| R36 survival | Passed, process exit 0 | Staged hostile contact, regeneration, cleanup, acknowledgment/retry and unlock lifetime through actual Update; direct Pause and modifiers staged. |
| R36 shop alternatives; R38 repeat | Both Passed, process exit 0 | Staged offers/materials/focus, virtual buttons through Update/OnGUI; not organic acquisition. R36 report has 22 passing checks. |
| R37 four companion choices/supports | Passed, process exit 0 | Selected pet plus Lootworm, start/retry lifecycle and registration/placement boundaries; availability and focus staged. |
| R37 five world unlock producers | Passed, process exit 0 | Actual downstream contact/timer/explosion events, persistence and fresh-run eligibility with supplied targets. Not 115 organic unlock playthroughs. |
| R38 options scroll, 80% and 100% | Both Passed, process exit 0 | Virtual D-pad/A; rendered-coordinate maximum error 0.015271 pixels and 0 respectively; immediate and settled confirmation both activate Keep-locked. |
| R38 navigation | `passed=true`, process exit 0 | Software Gamepad events through actual player Update/OnGUI; accelerated intermissions and supplied inventory/economy. |

Eight prior visual-review JSON files identify **58 capture hashes**; this pass recomputed all 58 successfully without repeating their earlier image-by-image inspection. It additionally inspected **all eight actual R38 shop PNGs using `view_image`**, read all22 passing checks and exit0, and saved `shop-alternatives-r38/independent-visual-review.json`. The nine review files now identify **66 capture hashes**. R38 pixels show the white first Lock focus, exclusive-looking dimmed Stick popup with Combine/Recycle/Cancel, empty weapon inventory after recycle, Item found without a bottom inventory strip, Arms Dealer weapon prices1, Fisherman bait price1, repeated Wave1 HUD and Wave2 birth indicators. The Candy still shows red birth crosses, not a fully rendered elite; the runtime producer assertion establishes the elite. These are R38 captures, separate from the older R36 visuals. Original observations and their exact limits are retained in [survival/shop](native-survival-shop-followup-2026-10-20.md), [starting supports](starting-support-native-followup-2026-10-20.md) and [scroll navigation](scroll-navigation-native-followup-2026-10-20.md).

R38 navigation has **17 recorded stages**: title, character, weapon, B-release return, difficulty, left-stick combat movement, D-pad combat movement, right-stick manual aiming, combat pause/resume, staged shop pause/resume/shortcuts, staged Loot pause/resume and Upgrade pause/resume. Movement reaches y=4.256248 then x=-1.417500; manual aiming records direction (0,-1) and `manualAim=true`. All are software Gamepad events with fixture-only IgnoreFocus and accelerated/supplied intermissions. They do not establish physical Xbox input or ordinary OS focus behavior.

## Keyboard, mouse and focus boundary

`manual-r38` records title page transitions, entry into Playing, a player-position change from x=0 to x=0.07875, and later Lost. It records zero shots and manual aiming false. These records support bounded menu/run interaction; they do not independently prove sustained keyboard movement, manual aiming or firing. The recorder exposes controller axes, not keyboard key-down history. Xbox detection alone is not physical controller acceptance.

`focus-r40` identifies the actual R40 executable and support showcase, without a virtual-input fixture flag in its launch arguments. Snapshots record Escape-paused state, resumed Playing and subsequent Paused/Playing transitions around the recorded switch sequence. Position is retained; the support showcase keeps firing. The snapshots contain no `Application.isFocused` field or explicit operating-system switch event, and the logger suppresses unfocused samples. They cannot independently prove that background processing stopped during every focus interval. Ordinary OS focus acceptance remains bounded to the coordinator's actual observed interactions, not an exhaustive automated pass.

Physical Xbox testing is **deferred under the user's instruction**. Software Gamepad tests do not replace it; no request for physical button presses is needed in this pass. No new human controller playtest or audible-feedback claim is made.

## Campaign and failed attempts

The actual R40 campaign receipt reports **Passed/Won, 20 completed waves, 19 shops, exit 0**, with normal 60 Hz timing, scripted movement, contact hurtbox immunity, neutral stats and a safe-loadout policy. One self-damage offer was skipped. This is a staged headless campaign, not ordinary survival balance, human playtesting, UI acceptance or cross-engine random-sequence equality. The coordinator's separate campaign comparison report owns the source counterpart and comparator interpretation.

Preserve both historical attempts:

- R36's unfiltered campaign **Failed/Lost after 12 completed waves**. Contact immunity did not suppress item periodic self-damage; Blood Donation caused loss. Do not relabel this a full-20 success or infer a gameplay defect solely from a protected fixture losing.
- R39 had a source counter-reset/build-input mismatch. `r39-unvalidated-built-inputs.json` remains an **unvalidated build-input receipt**, not a validated R39 campaign baseline. R40 is the verified replacement.

## Remaining evidence limits

Arbitrary timed item combinations, an ordinary unprotected human full campaign, exhaustive cross-container scrolling/window resizing, current physical Xbox acceptance and browser release behavior remain outside these finite checks. Source, model, native staged fixtures and human observations remain separate. This evidence does not establish universal exact gameplay parity.

Codex agents performed this investigation, fixture authoring and evidence review under the user's direction. These records do not imply that the user independently authored or performed each debugging step.
