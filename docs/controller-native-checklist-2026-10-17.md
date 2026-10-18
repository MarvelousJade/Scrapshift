# Controller native checklist — 2026-10-17

This checklist separates queued virtual-device fixtures from physical Xbox input. No new native checks or hardware acceptance were performed by this checklist's author. Use the exact current build and record its executable path, branch, source manifest and launch arguments before assigning results.

## R10 disposition after physical feedback

The user confirmed the requested basic controls work on the physical Xbox. The R10 observer log corroborates A/B events, controller navigation, both sticks, movement/manual aim, D-pad input, Start opening pause and B resuming. The later clarification concerned B not opening pause during a wave; the user accepted the supplied source's B/Start distinction. That report is closed as expected behavior.

Original source: `E:/UnityProjects/brotato-(3.x)/project.godot:2536`/2539 maps `ui_cancel` to B; 2585/2588 maps `ui_pause` to Menu/Start. `main.gd:434` opens single-player pause only on `ui_pause` release. `ui/menus/ingame/pause_menu.gd:33`–34 allows B to return only once paused; its unpaused branches at 42/47 require pause release. Scrapshift counterparts are `CombatInput.cs:96`–97, `ArenaGame.cs:170` and `RunRules.cs:506`/509.

The expanded native virtual fixture also passed with 17 snapshots. See `docs/r10-controller-evidence-2026-10-17.md` for the physical snapshot counts and scope. The table below is retained as a reusable checklist: unperformed shortcut/disconnect/mixed-device rows are optional additional hardware coverage, not a requirement to repeat accepted basic controls or every possible button. No required control mismatch remains identified by this audit.

The current R18 player subsequently passed the same native virtual fixture with **17 snapshots and process exit 0**, saved under `artifacts/gameplay-parity-r18-2026-10-17/navigation`. Character, combat and pause captures were opened and reviewed. This confirms the fixture still integrates after later gameplay fixes; it is not a second physical acceptance claim. Shop/reward/upgrade portions have state/assertion records, not new shop screenshots.

## Existing native fixture coverage

`ControllerNavigationCheck.cs` creates an in-process virtual Gamepad and disables other Gamepads in that process. Its `--controller-navigation-check` flag must be paired with the isolated `--controller-check` profile. It mutes game sound.

The original fixture checks (historical execution evidence belongs to its dated report) include:

- Four A presses progress through title, selected character, selected weapon and selected difficulty without requiring a stick nudge first.
- The first character receives one weapon and the reference starting material purse.
- Left-stick up moves the actual player through Update.
- Movement requests footsteps and does not request menu focus cues; standing still stops requesting footsteps.
- Start press/release opens pause; A on its default focus resumes.

These are actual player/OnGUI lifecycle checks with synthetic device events. Muted cue counters do not establish that the audio sounds correct. These checks do not establish that physical controller input reaches Unity.

## Reusable physical Xbox checks

Launch the current review with the observer-only `--controller-check` logger and **without** `--controller-navigation-check`. The latter disables physical devices in the review process. Use the focused game window.

| Step | Action and acceptance | Evidence |
| --- | --- | --- |
| Device | Press/release A once. Confirm the connected Xbox appears with a real Input System layout. | `input.jsonl`: device name/layout, button mask transitions, `usingGamepad=true`. Device detection alone is insufficient. |
| Default selection | Follow title → character → weapon → difficulty using A without moving a stick. Visible white focus and activated target agree. | Native captures plus `menuPage`/`menuFocus` transitions. Confirm one weapon on first character. |
| Back timing | Hold B on weapon selection: remain there while held; release B: return exactly one page. Repeat from options and pause subpages. | B mask present before release, page transition on release, native focus afterward. |
| Left stick | Navigate horizontal/vertical; hold a direction through repeated controls and at a boundary. For a diagonal with x above .5, horizontal takes priority. | Axis samples, focus changes, captures; no rapid cue requests while focus remains unchanged. |
| D-pad | Navigate all four directions; enter a run and move with each direction and a diagonal. | D-pad axes/mask, player position, native observation. |
| Combined movement | Hold keyboard Right + D-pad Left: no horizontal movement. Hold keyboard Right+Up + D-pad Left: move only Up. | Position samples and visible movement; this specifically exercises the action-merging fix. |
| Right stick | Enable manual aim or aim-on-press in Options. Move right stick all directions, then release it. Direction tracks the stick and remembered aim persists where the selected mode applies. | Raw right axes, `manualAim`, `aimDirection`, shots and visible weapon direction. In default automatic mode, a right-stick movement alone need not enable manual aiming. |
| Automatic shots during aim | With manual aim active, hold a direction without repeatedly clicking/pressing; shots follow weapon cooldowns. | Shot count increases and native fire direction. This is not one-click-per-shot mode. |
| Input takeover | While holding right stick, move the mouse/press a keyboard key, then change the stick again. Active device and aiming switch accordingly. | Ordered `lastInputDevice`, `usingGamepad`, aim state; native cursor/weapon observation. |
| Combat cues | Move and stop with stick and D-pad. No menu switch sound in combat; footsteps remain distinct. | `playingMenuFocusCues` stays constant, footstep counts change; actual listening/user confirmation is needed for the sound itself. |
| Pause | Start release pauses combat. A resumes the same state. B release also returns from pause/subpages correctly. | Phase transitions, frozen position/timers, native captures. |
| Cross-state pause | Separately pause shop, upgrade and Item found; resume each with the same offers/reward and no acquisition or spending during pause. | Native UI captures and state observation. Do not call a combat-only pause check sufficient. |
| Shortcuts | In shop X locks/selects or steals the focused eligible offer; Y holds .5 seconds for one reroll, release cancels; Y recycles Item found; shoulder/trigger pairs switch appropriate tabs. | Native UI state before/after and relevant controller masks. Input logging does not record offer lock/ownership itself. |
| Disconnect | If practical, disconnect/reconnect in the isolated review. No stuck movement or held menu action; reconnect permits normal input. | Device list and vectors, native observation. Label this unchecked if not performed. |

The logger omits samples while the app is unfocused. A gap in the log is not evidence that an unfocused action worked or failed. Its `.1`-second sampling condition applies to continuous changes; button/state transitions are logged immediately when observed by the player Update.

## Targeted virtual fixture additions authored in this pass

The fixture was extended after root's initial build finished, then passed in the rebuilt R10 player with 17 snapshots and process exit 0. Its added checks are:

1. On weapon selection, East is held for .2 seconds and the page must remain unchanged; release returns exactly one page. A then re-enters the selected target after repaint.
2. A D-pad left pulse must move the actual player left without requesting menu focus cues.
3. The fixture explicitly enables aim-on-press, queues right-stick down, verifies manual direction, releases it, and verifies remembered direction while press-only manual mode stops. It then disables the fixture aiming option.
4. Intermission setup is explicitly accelerated: production `Collect`, `CollectConsumable` and `Tick(Remaining)` create shop/reward/upgrade states. This is not normal-timing campaign evidence. Start pauses each; B hold keeps it paused and release restores its exact state, offers and reward. X toggles a naturally generated lockable offer through an actual rendered child-control mapping; shoulders toggle statistics tabs. Y's short hold cancels, long hold rerolls once only, and Y recycles an actual crate before queued upgrades.

Native snapshot records now include aim direction, manual-mode state and statistics-tab state. Basic physical controls are accepted within the user's confirmed scope and real input log. Optional shortcut/device/disconnect checks remain distinct from that acceptance; the virtual fixture does not claim physical evidence for them. Actual heard audio remains separate from cue counters.

Existing EditMode tests already cover Cancel-release edges, digital input merging, D-pad component strengths, direction thresholds, reroll-hold cancellation, menu lifecycle and rule-level shop locking/theft/pause. Extending native fixtures would verify their integration; it would still not replace the physical checklist.

## Reporting

Record each row as passed, failed, or unchecked with evidence paths. Distinguish human/controller feedback, assistant native observation, virtual-device fixtures and source inspection. Preserve user attribution: the user reported earlier Xbox highlight/activation mismatch and combat switch sounds; Codex authored the investigation and code changes. No physical acceptance should be inferred from test totals or a detected Xbox device.
