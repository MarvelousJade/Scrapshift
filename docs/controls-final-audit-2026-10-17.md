# Controls source audit — 2026-10-17

This is a source inspection and focused regression change. It does not establish physical Xbox acceptance. No Unity, player build, full suite or full run was launched by this audit agent.

## Confirmed differences

| Behavior | Supplied source | Correction |
| --- | --- | --- |
| Keyboard plus D-pad movement | `entities/units/movement_behaviors/player_movement_behavior.gd:get_vector_for_input_prefix` combines button actions before comparing the digital vector with the analog vector. Opposing keyboard/D-pad actions cancel; matching actions do not double their strength. | `CombatInput.ButtonMovement` merges positive/negative action strengths, normalizes the digital diagonal, then `PlayerMovement` compares it with the processed stick. Individual D-pad button strengths are retained until merging. Root integration must pass separate negative and positive keyboard strengths. |
| Diagonal left-stick menu input | `singletons/input_service.gd:_on_joystick_timer_timeout` checks horizontal above the strict .5 threshold before vertical. | `MenuDirection` now uses horizontal priority even when vertical magnitude is larger. Existing .3-second delay and .09-second repeat interval already match source. |
| Controller Back timing | `ui/menus/run/base_selection.gd:_input` handles page Back on `ui_cancel` release; `ui/menus/ingame/pause_menu.gd:_input` also handles Back on release. Base selection separately clears a selected cooperative element on press. | Exposed `CancelReleased` for the root single-player page/pause integration. `CancelPressed` remains available for diagnostics and any explicit press actions. |

## Already present / preventive coverage

- Reference `singletons/utils.gd:is_manual_aim` gives press-only aiming priority over always-manual, with right-stick squared magnitude above .05. Manual aiming selects direction and still permits cooldown-based automatic fire; it is not one shot per click.
- `ReferenceCombat.UpdateEquipment` already filters the right stick by the active input device and gamepad-movement setting. There was no confirmed stale-stick firing bug. An ownership-aware helper overload and regression now make that condition explicit; the diagnostic logger should use the same condition.
- Source `main.gd` changes remembered gamepad direction only for nonzero right-stick input during manual gamepad aim. Existing `RememberAim` preserves this behavior.
- Source `weapons/melee/melee_weapon.gd` prevents manual rotation during an active melee animation; the existing runtime helper covers this.
- Existing menu lifecycle tests exercise selected-card default focus, outgoing-page invalidation, confirmation before the first repaint, mouse-to-controller takeover and suppression of menu focus sounds in combat. They are simulated device checks, not physical controller evidence.
- Existing `RunRules.Pause` restores combat, shop, upgrade and loot state. Existing focus-loss handling covers those same phases. Source pause uses release timing; the project's explicit requirement to allow pause through rewards/shop remains in force.
- Existing controller shortcuts match the inspected mappings: South accepts; West selects/locks or steals; North invokes info/reroll; shoulder/trigger pairs switch tabs; Start release pauses. Existing reroll-hold helper fires once after .5 seconds and cancels on release.

## Focused checks added

`ControlsParityTests` adds three checks for action merging, explicit input ownership and horizontal menu priority. `InputSystemDeviceTests` adds two queued-device checks for diagonal D-pad strengths and Cancel release edges. These tests were authored in this pass; execution/results belong to the root validation report.

## Remaining evidence

Physical Xbox movement, aiming, A/B page transitions, selection highlights, shortcuts and pause/resume still require native verification. Keyboard/mouse behavior after switching devices must be observed in the same current build. The original game being open in Godot is useful runtime access, but source inspection alone does not prove its native timing or audio.

Attribution: the user supplied the parity direction, original project and controller observations. Codex inspected the supplied source and authored these changes and regressions. No hands-on completion by the user is inferred.
