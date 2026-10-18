# Native observations — October 15, 2026

Branch: `codex/gameplay-parity`. Initial ordinary player: `Builds/GameplayParityControls20261015/Scrapshift.exe`. These observations precede the settings-row navigation and later material/aim fixes.

- Title: Down selected Start, another Down selected Options, Return opened Options.
- Options: arrows moved tab focus; Return activated the selected tab. Down skipped settings rows and selected Back. This observed defect prompted the complete-row navigation fix and regression test.
- Mouse-only movement: enabled through its actual control, started Rookie with one Pistol on Danger 0. With no button held the player moved from arena center toward the stationary cursor at the bottom boundary. This establishes cursor following; it does not test held-button stopping or analog hardware.
- Escape paused combat at 14 seconds remaining. Opening Options kept the combat scene frozen; the mouse-only preference was restored to its original Off value. Escape returned to pause and a second Escape resumed combat with 14 seconds remaining.
- With no subsequent movement, the player died and the actual first-death character unlock screen appeared (Bulkhead). Captured as `ordinary-first-death-unlock.png`. Combat pause captured as `ordinary-combat-pause.png`.
- The supplied Godot project was launched through its Editor, resumed its existing wave-8 shop at 174 materials and was inspected without buying/rerolling. Its keyboard pause events have zero keycodes. See the main parity report for source evidence. Its source and saved run were not repaired or replaced.

Actual controller hardware, held mouse stopping and continuous manual-aim movement still need native input evidence. Automated tests are recorded separately. The test run changed real local progression by earning the first-death unlock; the options change was restored.

## Later R2 isolated controller review

- Keyboard navigation reached the Sound master slider; Right changed 50 to 55 and Left restored 50. Gameplay mouse-only option toggled On and Off through keyboard focus.
- A locked character showed its requirement (Reach -5 HP Regeneration); confirmation did not advance selection. Screenshots: final-keyboard-slider.png, final-keyboard-toggle.png, final-locked-focus.png.
- The isolated host player detected Controller (Xbox One For Windows). Recorded A/B button inputs advanced the title selection and returned. The user physically moved sticks and D-pad and reported no response; all six logged legacy axes remained zero.
- Inspection found the six legacy gamepad axis records outside the serialized m_Axes array. The fix adds actual Unity deserialization regression cases. Per the user's subsequent instruction, controller input is migrating to Unity Input System 1.20.1; final hardware acceptance remains pending.
- This later fixture keeps preferences and progression in memory. Its logs are under controller-isolated; it does not update the ordinary profile.

## Final R4 Input System review

- Build: `Builds/GameplayParity20261015R4/Scrapshift.exe`, branch `codex/gameplay-parity`, runtime SHA-256 `2F2C8956165804502E946CC75EF89B818F1B4B631C8F2782A6EB65D4E24C218E`. Successful Windows build, zero errors/warnings, 401072035 reported bytes. Replaces R2/R3 for controller review.
- Input System 1.20.1 detects `Xbox Controller [XInputControllerWindows] #9`. This is detection evidence; physical sticks, D-pad, aim, buttons and pause acceptance remain pending. The R3 review recorded no user input sequence before replacement.
- Includes X shop lock/theft, Y half-second reroll hold with progress/default functional setting and item-found recycle, shoulder/trigger tab/stat switching, per-axis menu threshold and shared nonlockable-item restrictions.
- Actual mouse opened Options and Gameplay. The new hold-reroll label and all rows fit visibly. Toggled On -> Off -> On, restored default; Escape returned to title. Capture: `r4-controller-options.png`. Settings stayed in memory; this is native mouse/keyboard evidence, not controller evidence.
- Full suite before the final bounded locking guard: 526/526 passed, 74.2703369 seconds. Latest affected controls/device/special-shop suite after that guard: 46/46 passed, .8029584 seconds. These are separate scopes; no full 529-test result is claimed.
- R4 is left open at the title using isolated `--controller-check`; logs are under `controller-r4`.

## Physical R4 feedback and corrected R5

The user's subsequent `controller-r4-retry` recording contains actual left-stick and horizontal D-pad values reaching 1, A events and combat movement. It also captures the bug: after entering a new selection page, A targets focus 0 (Back) while the selected character remains white. The user reported rapid UI-like sounds while moving. Right-stick values in this trace remain below .07, and vertical D-pad remains zero, so it does not establish intentional right-stick or vertical D-pad testing. See `r4-physical-feedback-metrics.json`.

R5 synchronizes navigation lifecycle before input, drops outgoing screen registrations after callbacks, waits for incoming controls, focuses the current selection and uses a single white activation target. Mouse/controller handover follows the current selection. Resume, Take and upgrade choices have inspected reference defaults. Blocked navigation and stationary mouse hover do not generate duplicate controller focus cues. Footsteps retain the approved audio assets/mix, with source first-contact .1 seconds and .3 animation-second cadence scaled by movement speed.

- Full integrated checkpoint: **541/541 passed**, zero failures/skips, 69.4470763 seconds, `controller-focus-final.xml`.
- Visible native player fixture: **passed**, exit 0, `focus-native-visible/navigation-check.json`. Virtual Gamepad A events advance title -> selected Rookie -> selected Knife -> selected Danger 0 -> actual Playing. The fixture moves the player, records **0 menu-focus requests / 3 footstep requests**, checks no footsteps while stationary, opens pause with Start release and resumes with A. Rookie inventory contains one weapon.
- Actual generated character and pause images were inspected: one white selected Rookie tile; Resume is the white pause activation target. These are automated native fixture captures, not human-controller evidence or audio listening.
- An initial hidden-window fixture did not register GUI controls and failed before entering character selection; it is preserved under `focus-native`. Acceptance uses the visible window route.
- Build `Builds/GameplayParity20261015R5`, branch `codex/gameplay-parity`, succeeded with 0 errors/warnings and 401081415 reported bytes. Runtime SHA-256 `E612D8992DEC50CCF16AFBCEDA2EEA10A7EE201B6C45F5654D183AE748CB94AB`. A later deprecated object lookup was changed only in the optional test helper; ordinary gameplay code remains the tested version. See `controller-r5-build-manifest.json` and matching 449-file source/settings manifest.
- R4 was closed and R5 launched using isolated `--controller-check` for renewed physical feedback. Logs: `controller-r5`. The ordinary physical review does not inject input.

The user confirmed Rookie should start with one weapon. The separate 50-character source audit verified 42 one-weapon, six explicit-extra-weapon and two weaponless starts; no universal second weapon was restored.

## R6: persistent movement sound report

The user still heard button-switch-like sounds while moving in R5. Its physical trace contains 129 Playing samples, zero playing menu-focus cue requests and 36 footstep requests; those counters do not establish what the user heard. Actual imported footsteps revealed mono normalization amplifying the quiet supplied recordings. R6 preserves source stereo channels and energy, rebaking exactly six footsteps; the other 266 contextual WAVs and the sound library asset remain unchanged.

- Focused audio validation: 23/23 passed, zero failed/skipped, 5.0103641 seconds; footstep-audio-final.xml.
- R6 build succeeded with zero errors/warnings; see controller-r6-build-manifest.json. The 465 selected source/audio inputs match the validation clone; controller-r6-source-audio-manifest.json.
- Builds/GameplayParity20261015R6 is running with isolated --controller-check; state.json detects Xbox Controller through Unity Input System 1.20.1. Physical listening confirmation remains pending. R5's 541-test result is a prior checkpoint, not a rerun after the audio change.

## User R6 listening confirmation

User: "Okay, the sound is gone." Movement/button-like sound symptom closed after the R6 retest; other physical-controller checks remain separate.

