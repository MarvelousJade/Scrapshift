# Supplied Godot project: missing keyboard bindings

Date: 2026-10-17. Read-only diagnosis of `E:/UnityProjects/brotato-(3.x)`. The original project and its user preferences were not edited by this audit agent.

## Observation and cause

The user reported that only mouse clicks work in the original Godot run: keyboard movement, Escape, Enter and Control do not respond. The root agent separately reported successful mouse flow and failed Escape/Enter attempts. Those are native observations; this document's additional evidence is source inspection.

`project.godot` has 35 serialized `InputEventKey` entries across 17 actions in its `[input]` section. **All 35 have both `scancode=0` and `physical_scancode=0`.** They include `ui_accept`, `ui_cancel`, `ui_pause`, `ui_left/right/up/down`, and `button_move_left/right/up/down`. Keyboard entries also use device 16; no claim is made that this device value independently caused the failure. The zero key codes already leave the actions without identified keyboard keys.

The same actions retain concrete joypad events and mouse controls remain separate, which is consistent with mouse controls working while Enter/Escape/movement do not. This is a defect in the supplied local reference configuration; it does not establish that the released original game has broken keyboard support. The reason those key codes became zero is unknown.

## Startup does not restore the missing keys

- `singletons/input_service.gd:_ready` selects `enable_gamepad_movement` or `disable_gamepad_movement`.
- Both routes call `InputMap.load_from_globals()`, then `_copy_device_actions()`.
- `add_key_action` copies `event.scancode` and `event.physical_scancode` unchanged. Consequently the zero-code keyboard records are copied into per-device actions as well.
- Disabling gamepad movement removes joypad events only. It does not repair blank keyboard entries.
- `singletons/progress_data.gd:init_gameplay_options` supplies `mouse_only=false`, `movement_with_gamepad=true` and aim settings, but no keyboard binding table.
- `ui/menus/pages/menu_options.gd` exposes mouse-only/gamepad/aiming choices and resets those preferences. No normal keyboard-rebinding interface or keymap-loading path was found in the inspected source.
- A whole-project search for `InputMap`, `scancode`, `physical_scancode`, rebind/keymap terms found normal mapping changes in InputService only. The debug multi-keyboard path is disabled by default; it is not a normal restoration route. The editor-only bug-reporter route adds F7 for that debug action alone.
- No `override.cfg` was present at the inspected reference root. Runtime state and externally loaded modifications were not inspected by this source-only diagnosis.

## Safe next step for runtime comparison

Preserve the supplied reference. Prepare an isolated review copy before changing Input Map settings. Give that copy a distinct Godot `application/config/custom_user_dir_name` as well as `config/use_custom_user_dir=true` **before launching it**: `ProgressData.init_save_paths` normally uses `user://`, creates a log and can load/save progress, so copying the project alone does not isolate user data.

In that isolated copy, Godot 3's Project Settings → Input Map can attach real key events to the existing actions without altering gameplay scripts. Suggested temporary review bindings are WASD/arrows for the four `button_move_*` actions, arrows for `ui_*` directions, Enter/Space for `ui_accept`, Escape for `ui_cancel` and `ui_pause`. These are proposed test bindings, **not recovered original defaults**: the supplied records do not preserve which keys their empty slots originally represented. Control has no recoverable role in the inspected zero-code entries.

Restore bindings before launch so InputService's startup copying includes them. Then verify input through native movement, menu navigation and pause, with a clear label that only the review copy's missing bindings were repaired. Do not present that comparison as a test of untouched reference keyboard configuration.

The subsequent authorized preparation created `tmp/BrotatoKeyboardReview/LaunchKeyboardReview.cmd`, with explicit temporary review keys, fresh redirected saves and process-scoped APPDATA/LOCALAPPDATA. Godot 3.6.2 natively confirmed the actual key mappings and isolated `user://` location. See `artifacts/reference-keyboard-review-2026-10-17/README.md`, its patch manifest and native input-map report. The script-only probe reported a CursorManager null-current-scene error and shutdown leaks; it is not a clean ordinary gameplay run. Root owns ordinary launch and native input verification. No Unity code changed and no full run was launched during this diagnosis/preparation.

## Attribution and interview use

The user identified the original run's keyboard failure. Codex traced the supplied serialized input entries and startup copying path. This is an example of separating an input configuration problem in a reference fixture from a gameplay defect in Scrapshift; it is not evidence that the user personally debugged the engine or implemented this investigation.
