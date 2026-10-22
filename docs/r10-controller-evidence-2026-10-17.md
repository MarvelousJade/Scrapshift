# R10 controller evidence — 2026-10-17

Build: `Builds/GameplayParity20261017R10`, branch `codex/gameplay-parity`. Build hashes are in `artifacts/gameplay-parity-r10-2026-10-17/build-manifest.json`. Source and focused-check evidence belong to the root's R10 reports; this document concerns controller evidence only.

## Virtual native fixture

The root reported a successful expanded native virtual-controller fixture with process exit 0. Its actual `navigation/navigation-check.json` has `passed=true`, an empty error field and 17 snapshots. It explicitly labels synthetic Gamepad events and accelerated intermission setup. Coverage includes selection/default focus, B-release timing, D-pad movement, right-stick manual-aim history, combat cue requests, pause restoration, shop locking, tab switching, reroll hold and crate recycle.

This is integration evidence through the built player's Update/OnGUI lifecycle. It does not establish physical Xbox support or actual heard audio.

### Current R18 virtual navigation rerun

The same expanded fixture was rerun against `Builds/GameplayParity20261017R18/Scrapshift.exe`, without touching either live full campaign. Actual evidence in `artifacts/gameplay-parity-r18-2026-10-17/navigation/` records **`passed=true`, 17 snapshots, empty error, process exit 0**. Process 16188 ran from 23:41:16.112 to 23:41:32.632 UTC under detached runner 20328. Exact arguments and process records are retained.

The first sandbox helper exited before a game process record or fixture outputs existed; no gameplay result came from that launch. The approved host-helper retry produced the saved result. Both launcher-attempt records remain visible rather than hiding the first launch failure.

Opened the current `character.png`, `moving-combat.png` and `paused.png` captures. The selected Rookie card is visibly white, combat shows the selected single weapon, and pause highlights Resume with the run inventory displayed. Shop/loot/upgrade checks passed their actual rule assertions and appear in the 17 state snapshots; this fixture does not save shop screenshots, so those must not be described as newly visually inspected shop captures. No new native UI mismatch was identified in the inspected images.

This R18 result is **virtual device integration**, not a new physical-controller test. The basic physical confirmation below remains the R10 user/log evidence with its original scope.

Delivery housekeeping: the root corrected R19's inherited controller launch script so it writes to the R19 physical-controller artifact path, instead of mixing new observations into R16. R19's revised launcher and build hash record are separate delivery evidence; this path correction does not establish additional hardware acceptance. The exact R18 full campaign used its recorded executable arguments, rather than that controller launcher.

## Physical Xbox result, including the user's clarification

The root launched the observer-only R10 window without the virtual-navigation flag and asked the user to exercise the physical controller. The isolated profile had aim-on-press enabled through the actual Options UI. The user confirmed the requested basic controls worked, then clarified that B returned from pause but did not open pause during a wave. After the supplied source was checked, the user accepted that this is the expected reference behavior. This closes that report as expected behavior, with no code fix required.

- The refreshed log contains 218 samples, ending at 525.306396 seconds after startup; 144 have significant controller state and 206 are XInput-owned.
- The device list contains `Xbox Controller [XInputControllerWindows] #9`.
- Both sticks span -1 to 1 on both axes. There are 88 XInput-correlated playing-movement samples and 44 manual-aim direction changes.
- Observed button press edges: A 5, B 11, Menu/Start 3, LB 1, D-pad Down 4 and D-pad Right 1.
- Start release opens pause three times (512.082153, 519.957336 and 522.245789 seconds; mask 128 → 0). B release resumes twice (520.509521 and 522.854065 seconds; mask 2 → 0).
- Controller-owned menu focus/page changes were recorded. The log also contains one paused-to-title transition after A; it is not counted as a resume.
- Combat menu-focus request count remains 0; playing footstep requests reach 27. These counters do not establish what was heard.

The user's confirmation plus the real-device evidence supports acceptance of the requested basic navigation, A/B return, movement, aiming and pause controls on this Xbox in R10. It is not acceptance of every optional shortcut or proof of complete gameplay parity. D-pad Left/Up were not individually recorded; the basic D-pad confirmation does not require repeating every direction before continuing the gameplay work.

## Why B returns but does not open pause

The supplied `E:/UnityProjects/brotato-(3.x)/project.godot:2536` defines `ui_cancel`, with joypad button 1 at line 2539. `ui_pause` is defined at line 2585 with joypad button 11 at line 2588. Those are B and Menu/Start respectively in this Xbox context.

`E:/UnityProjects/brotato-(3.x)/main.gd:417` defines `_check_for_pause`; its single-player branch at line 434 opens pause only on `ui_pause` release. `E:/UnityProjects/brotato-(3.x)/ui/menus/ingame/pause_menu.gd:33` checks whether the tree is already paused; line 34 then permits cancel or pause release to return. The unpaused branches at lines 42 and 47 accept pause release only. `manage_back` at line 53 resumes the main pause page or returns from a submenu.

Scrapshift matches that distinction: `Assets/Scrapshift/Runtime/CombatInput.cs:96` maps B release and line 97 maps Start release; `ArenaGame.cs:170` admits B only for title/back, paused state or an open weapon context, while Start can open pause in a run. `RunRules.cs:506` restores the saved underlying phase and line 509 permits pause from combat, shop, upgrade and loot.

## Reproduce the analysis

Run the bundled Python interpreter against:

```text
artifacts/gameplay-parity-r10-2026-10-17/analyze_physical.py
```

The script reads the observer's `physical-controller/input.jsonl` once, hashes that exact byte snapshot and writes `physical-controller/analysis.json`. Rerun after new physical input to refresh the report. A live writer's incomplete final JSON line is skipped and its line number reported; no polling is performed.

The report includes raw-axis min/max and both directional threshold counts, button press samples/edges, phase/page/focus transitions, XInput-correlated movement/manual aim/pause changes and the latest recorded state. Correlation is not a general acceptance verdict: native captures or user feedback must confirm visible focus, weapon behavior and intended actions. The observer omits samples while unfocused, and its logger does not record offer lock/ownership details.

## Button-mask mapping

This mapping comes from `CombatInput.ControllerButtonMask`, not an assumed platform button enum:

| Bit | Mask | Control |
| --- | --- | --- |
| 0 | 1 | A / South |
| 1 | 2 | B / East |
| 2 | 4 | X / West |
| 3 | 8 | Y / North |
| 4 | 16 | LB |
| 5 | 32 | RB |
| 6 | 64 | View / Select |
| 7 | 128 | Menu / Start |
| 8 | 256 | Left-stick click |
| 9 | 512 | Right-stick click |
| 10 | 1024 | LT |
| 11 | 2048 | RT |
| 12 | 4096 | D-pad up |
| 13 | 8192 | D-pad down |
| 14 | 16384 | D-pad left |
| 15 | 32768 | D-pad right |

The six logged axes are left x, left y, right x, right y, D-pad x and D-pad y, in that order. Unity stick y is positive upward.

## Remaining optional physical coverage

X locking/theft, Y reroll/recycle, RB/trigger shortcuts, intermission pause, mixed keyboard/D-pad cancellation and disconnect/reconnect were not separately exercised on hardware in this log. Relevant rule/device tests and the expanded native virtual fixture cover their implemented paths; optional hardware spot checks may add evidence without blocking accepted basic controls. No new required control mismatch was found in this source review. Whole-suite and full-run gates remain the root task's decision under the user's testing order.

Keep all results separated: detected device; real input observed; native visible behavior; user confirmation; virtual fixture. See `docs/controller-native-checklist-2026-10-17.md` for optional extensions and exact evidence requirements.

Attribution: the user supplied the Xbox and earlier observations. Codex authored the code, virtual fixture and reproducible log analysis. This record does not attribute the assistant's investigation or synthetic tests to the user's hands-on work.
