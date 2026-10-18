# Isolated reference keyboard review

Prepared 2026-10-17 by Codex. Original source and existing `tmp/BrotatoParity` remain unchanged.

Launch: `E:/Codex Projects/Job Search Assistant/tmp/BrotatoKeyboardReview/LaunchKeyboardReview.cmd`.

The script scopes APPDATA and LOCALAPPDATA to the review folder and starts Godot 3.6.2 with `--path`, `--no-fullscreen`, `--resolution 1280x720`, and `--position 80,80`. Launch through this script to retain that isolation. The distinct project identity is **Brotato Keyboard Review 20261017**.

## Temporary review keys

These bindings repair blank local configuration for comparison; they are not recovered original defaults.

| Action | Review bindings |
| --- | --- |
| Movement | WASD and arrows; Q/Z alternatives for left/up |
| Menu navigation | Arrows and WASD; Q/Z alternatives for left/up |
| Accept | Enter, keypad Enter, Space |
| Back | Escape |
| Pause | Escape or P |
| Select / lock | F |
| Info / reroll | R |
| Ban | X |

All 26 supplied joypad event records are unchanged. Keyboard events use explicit Godot 3 `scancode` values and device 0; `physical_scancode` remains zero so each event has one matching mode. F7/F4 are temporary debug-action mappings; they are not needed for normal review.

## Changes and isolation

- Restored the original `res://pause.tscn` startup scene.
- Removed the fixture autoload. The movement behavior was restored byte-for-byte from the original, removing its unchecked fixture-node dependency.
- Retained inherited isolated-save and modloader redirects, changing every old review prefix to the new clone. No old clone prefix remains in `.gd`, `.godot` or `.cfg` files.
- The inherited `ProgressData` diagnostic prints remain; they do not grant unlocks or alter gameplay values. Dormant fixture/helper files remain on disk but are not autoloaded or selected as the startup scene.
- Source comparisons and SHA-256 hashes are recorded in `patch-manifest.json`, `original-comparison.json` and `original-source.diff`. The inherited differences are explicit; the complete clone is not byte-identical to the supplied original.
- Fresh game saves/logs use `tmp/BrotatoKeyboardReview/userdata-realtime-20261015/`. Godot's own `user://` path was measured natively as `tmp/BrotatoKeyboardReview/host-data/Roaming/BrotatoKeyboardReview20261017`.

## Validation and limits

`checks.json` verifies unchanged joypad events, original movement code, restored startup scene, removed fixture autoload and nonblank keyboard records. `native-input-map.json` records Godot 3.6.2 loading the actual review project and recognizing the key bindings and isolated data path.

That native probe used a script instead of a current scene. Game autoloads initialized fresh isolated saves; CursorManager reported its existing `current_scene=null` error and the engine reported shutdown resource leaks. These are preserved in `probe-godot.log` and must not be described as a clean gameplay run. The standalone minimal isolation probe did not load game autoloads.

Ordinary scene launch and native keyboard/mouse checks belong to the root agent. No full campaign was launched by the preparation agent.
