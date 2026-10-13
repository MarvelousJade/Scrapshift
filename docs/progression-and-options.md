# Progression, Codex and options â€” October 12, 2026

## Purpose and reference evidence

Make the existing base-game unlock rules visible and understandable: players need to see why something is locked, what earns it, what they have discovered, and which settings affect the game. Preserve Scrapshift's mascot, implant names and artwork.

Inspected the running Godot 3.6.2 reference using native window automation, without requiring the user to navigate. Saved nine original screenshots in `artifacts/progression-options-review`: Challenges; Items; Weapons; Enemies; Sound; Video; Gameplay; Accessibility and its lower section (nine screenshots total). Read these supplied reference sources:

- `ui/menus/pages/menu_codex.gd`: four collections, collection percentages, weapon families and tier selection.
- `ui/menus/pages/progression_ui.gd` and the progression UI resources: challenge completion, descriptions and reward panels. Difficulty progression is excluded from the Challenges grid.
- `singletons/progress_data.gd`: settings defaults, completed challenges, acquisitions and enemy counts.
- `singletons/run_data.gd`: both `add_item` and `add_weapon` count acquisitions, including starting equipment.
- `ui/menus/pages/menu_options.gd`: all exposed setting groups and callbacks.
- `ui/menus/shop/base_shop.gd`: retained offers can lose their lock on arrival in the next shop when Keep Lock is disabled.

Reference screenshots document one existing save, not the initial unlock state. No reference save was reset or edited.

## Implemented

- Codex entry points from the title screen and pause menu; Challenges, Items, Weapons and Enemies tabs, left grids and right detail panels, completion bars, scrolling and arrow navigation.
- Challenges read the existing 115-definition unlock catalog. Six difficulty unlock definitions are omitted from the Challenges grid; rewards and conditions remain visible before completion. Existing character, item and weapon availability gates remain authoritative.
- Locked entries show a padlock. Available but undiscovered entries show silhouettes and `???`; descriptions reveal after acquisition or defeat. Weapon families occupy one entry even when their minimum rarity is greater than I; tier controls expose their actual variants.
- Items/weapons sort by rarity, name or acquisition count, with reversible order. Weapon descriptions use the current run's modifiers when inspecting from pause; title-screen Codex uses default stats.
- Acquisitions are counted for starting weapons, shop purchases and accepted crate items. Enemy defeats are counted by species. Discovery counters share the existing version-1 atomic progression save; old challenge completions remain intact. Historical acquisitions from builds that did not record them cannot be reconstructed.
- Newly earned rewards appear in a sequence before the run-end summary. A reward is queued once when its challenge first completes; it does not repeatedly appear on save reload.
- Options use Sound, Video, Gameplay and Accessibility tabs, scrollable rows and immediate, persistent controls. Settings are separate from run/progression data.

## Options mapping and boundaries

| Group | Working controls |
|---|---|
| Sound | Master volume, sound-effect volume, sound-effect mute, mute on focus loss |
| Video | Fullscreen, screen shake, cosmetic effects, damage numbers, 60 FPS cap (otherwise current 120 FPS target) |
| Gameplay | Mouse-only movement, manual aim, aim only while pressed, pause on focus loss, character HP bar, boss HP bar, keep shop offers locked, hide implant appearance |
| Accessibility | Enemy health for new spawns, enemy damage, enemy speed, description font scale, projectile opacity, pet opacity, screen darkening, effect icons, reduced motion, positive/negative text palettes |

Keyboard help remains visible in Gameplay. Disabling cosmetic effects does not disable gameplay projectiles, collision or damage. Manual aim changes direction and allows firing without an auto-target; attacks still obey cooldowns, arrival and character restrictions. Focus loss never automatically resumes a paused run.

This is **not every setting in the reference**. Music playlists, DLC switches, co-op controls, localization, controller support and score-upload options have no corresponding system in this build. Reference-specific main-menu art variants are not copied. Per-object outline highlighting, alternate material audio, all tier-color pickers, optimized wave-end collection and retry-from-wave-snapshot are also not implemented in this pass. These are explicitly absent rather than nonfunctional switches; a full options parity claim would be inaccurate.

## Bugs and investigation

1. **Missing icons:** conditional weapon bonuses bypassed the icon/color formatter used by ordinary effects. They now use that formatter. Icon lookup accepts serialized `stat_` prefixes and the attack-speed alias. Character effects use an icon whenever one exists, including granted items, with a visible bullet fallback.
2. **Additional issue caught by a meaningful test:** the Fighting Stick's `levels` damage-scaling token had no sprite. Serialized our mascot body as the level icon; inline rendering composes the current mascot face. The audit checks every inline stat token across every weapon variant, including Wand burning and Jousting Lance conditional bonuses.
3. **Decorative stones:** removed creation and serialized scene instances, with a compatibility cleanup for older scenes. Grass, buried sci-fi scrap and map boundaries remain. The large scene diff is chiefly removed GameObjects and Unity's scene serialization order.
4. **Discovery versus unlocking:** completing an achievement must not falsely mark its item as purchased. Independent counters prevent that mistake, and failed purchases never count as discoveries.

## Verification

See `artifacts/editmode.xml`, `artifacts/build.txt`, `artifacts/progression-options-review/inspection.json` and the comparison page in that folder. The runtime fixture deliberately uses an isolated in-memory progression store and default settings; it never modifies the player's save. It captures screens and checks tooltip bounds and existing combat assertions, but does not certify native input or subjective audio quality.

The interview story: extend the existing progression model without resetting saves, distinguish availability from discovery, test actual acquisition paths rather than just UI labels, and use a whole-catalog icon audit to find a failure beyond the user's original report.

Native follow-up: verified title-to-Codex navigation, weapon-tab selection, keyboard entry selection, Escape/back and one real options toggle; restored the original focus-mute preference. 281 EditMode tests and all 62 staged capture checks passed.
