## Controller clarification resolved — October 17

The user clarified that the reported issue was B doing nothing during an active wave, and accepted the explanation. This matches the supplied original: `project.godot:2536/2539` maps B to ui_cancel; `2585/2588` maps Start to ui_pause; `main.gd:417-435` opens pause from ui_pause release, and `ui/menus/ingame/pause_menu.gd:32-53` handles cancel while paused. Scrapshift's `CombatInput.cs:96-97` and `ArenaGame.cs:170` use the same distinction. No gameplay change is required for this report.

The requested basic physical Xbox checks are accepted from the user's confirmation and recorded movement/aim/menu/pause actions. This is scoped acceptance, not verification of every shortcut/device combination. Native gameplay follow-up continues before the full run; see the current dated native check reports. Earlier pending-menu statements below are superseded.

## Physical Xbox follow-up — October 17, superseding the idle snapshot

The user first answered that all requested controls worked, then reported that B returns but a menu could not be opened (exact menu/button clarification pending). Do not mark overall hardware acceptance complete. Refreshed observer evidence contains218 samples,144 significant controller-state samples and206 XInput-owned samples; both sticks span -1 to1. A/B, D-pad down/right and Menu/Start events are present. The log records Playing→Paused three times on Start release and Paused→Playing twice on B release, plus controller-correlated movement/aim changes. These observations support the performed actions but do not dismiss the user's latest menu problem. Other shortcuts and unexercised directional buttons are not newly accepted. Full run remains held. See the refreshed `physical-controller/analysis.json` and pending clarification.

# Native review checkpoint — October 17, 2026

Branch: `codex/gameplay-parity`. Latest unpacked build: `Builds/GameplayParity20261017R11/LaunchGame.cmd`. Its `LaunchControllerTest.cmd` uses isolated in-memory settings/progression and records real input; it does not generate controller events. Build succeeded with zero errors/warnings. No whole suite or full campaign ran during this checkpoint.

## Checks actually completed

| Evidence | Result | Scope |
| --- | --- | --- |
| Final focused source selection | 524 passed | EditMode checks before native fixture updates; not the whole suite |
| R10 visible virtual-controller navigation | Passed, 17 snapshots | Actual player menu/combat loop with a synthetic Gamepad; includes selection, B release, movement, aiming, pause and shop/reward/upgrade shortcuts |
| R10 weapon fixture | 211 passed | 205 weapon/tier smoke cases and six native contact probes at 60 Hz |
| R10 enemy fixture | 53 passed | Enemy/state cases; not a crowded human campaign |
| R11 support fixture, hidden and visible | 39 passed in each | Separate executions of the same cases; visible execution produced 43 PNGs |

Each player process result was read, including exit zero. Do not add repeated executions to produce a larger coverage count. Artifacts are under `artifacts/gameplay-parity-r10-2026-10-17/` and `artifacts/gameplay-parity-r11-2026-10-17/`.

R10's first support execution failed two assertions (37 passed). Source tracing showed unsupported fixture expectations: independent Cat guns need not fire in opposite directions, and Scapegoat rises on the tick after its HP fills. R11 corrects those assertions and positions the staged Cat shadow at its actual base scale. No production pet behavior or artwork changed between R10 and R11. Preserve the failed report as investigation evidence.

`source-comparison.json` records that only `Runtime/PetReview.cs` changed between the R10 and R11 build inputs; all 518 inputs matched the validation clone after the R11 build. R10 navigation, weapon and enemy results are carried evidence with unchanged relevant inputs, not executions falsely labeled R11. Build manifests record 160 file hashes. The earlier 524-check snapshot has a separate preserved manifest; native fixture additions are listed separately.

## Native observations

The virtual navigation run observed an ordinary first-character start with one selected weapon and 30 materials. Intermission setup accelerates timers and grants resources deliberately; it does not constitute a full run. Beam/contact probes exercise current production weapon updates, while staged visual captures remain distinct from normal play.

Fresh R11 Cat aiming captures show the large staging-shadow ovals removed and connected body/gun art. Six support projectile captures were inspected. See `r11-support-visual-review-2026-10-17.md`. Bright projectile discs remain large relative to structures; equivalent-scale original presentation, continuous motion and audible cues have not been established by those still images.

The read-only original has blank keyboard events, consistent with the user's corrected report that keyboard and Ctrl do not work. In the isolated `tmp/BrotatoKeyboardReview` copy, temporary review bindings allowed Escape back, Enter selection/start/resume, arrow selection and P pause. Native start showed 15 HP, 30 materials and a 20-second first wave. A brief D tap did not establish movement. Original project files were not repaired. Temporary bindings are not claimed as recovered reference defaults. That copy's logging/shutdown diagnostics remain retained.

## Physical controller and remaining gate

The observer-only R10 window detects `Xbox Controller [XInputControllerWindows]`. At the last log inspection there were no significant controller actions, only neutral axis noise and mouse interaction. Physical left stick/D-pad navigation, highlight/activation agreement, movement, right-stick aiming and Menu pause/resume therefore remain unchecked. Virtual input and device detection cannot close this gate. `analyze_physical.py` summarizes the sparse observer log without assigning an overall hardware pass; its synthetic analyzer checks are separate from game evidence.

The full run remains held pending physical feedback and resolution of any confirmed failures. Exact simultaneous engine contact ordering, crowd response, continuous presentation/audio and human survival comparison remain limits; these fixtures do not prove universal gameplay parity. WebGL was not revalidated.

## Attribution and interview rehearsal

The user supplied direction, the reference and earlier hardware/audio observations. Codex performed source comparison, implementation, fixture repairs, builds and automated/native screenshot checks. Useful rehearsal: explain why a failed assertion can represent a faulty expectation; trace Cat gun clocks and Scapegoat's pre-increment rise check; show separate synthetic and physical input evidence; distinguish staged renderer setup from production behavior. Reproduce and explain the example before presenting personal debugging experience.
