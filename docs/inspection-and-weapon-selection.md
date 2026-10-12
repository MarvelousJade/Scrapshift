# Weapon selection and equipment inspection — October 11, 2026

The user identified two missing interactions: character-specific weapon selection should hide locked weapons, and the pause/intermission screens should let the player inspect stats and owned equipment. This pass addresses those interactions while preserving original character, enemy, item and weapon sprites.

## Reference evidence

Read-only source: `E:/UnityProjects/brotato-(3.x)`.

- `ui/menus/run/weapon_selection.gd`: `_is_locked_elements_displayed()` returns false; `_get_unlocked_elements()` intersects character starting choices with unlocked weapons. `singletons/item_service.gd` orders melee choices before ranged choices. Our four profiles now expose 13, 2, 7 and 8 weapons respectively, plus Random. Pugilist has only Fist and Hand. The character screen's 46 locked slots remain.
- `ui/menus/shop/stat_container.gd` and `stat_popup.gd/.tscn`: primary stat names, signed colors, dynamic descriptions and reference typography. Stat labels carry the percentage prefix; the value column does not repeat it.
- `resources/fonts/raw/Anybody-Medium.ttf` and the size-22/outlined-size-26 font resources supply the inspection typeface and size reference. The copied font SHA-256 is `6f35a2800d096a30418ea9bcb211aeb55598d4b48b5c60a8e22b2881d846159d`. It is reference material, not original project artwork.
- `resources/translations/translations.en.translation`, `singletons/item_service.gd:get_stat_description_text` and `weapons/weapon_stats/weapon_stats.gd` supply English labels/templates, modifiers and weapon description structure. `scripts/export_ui_text.gd` exports only the selected keys through Godot 3.6.2 into `ui-text.json`.

## Interaction and implementation

Hover a primary stat, owned weapon or owned item to see its description in pause, shop, level-up or crate screens. Tab/Shift+Tab cycle visible inspection targets; arrow keys move toward a nearby target. Moving the mouse returns to pointer inspection. Inspection navigation does not purchase, combine or recycle gear. Those existing actions retain their separate buttons.

`InspectionText` formats all sixteen primary stats and 120 weapon definitions from current run values. Weapon details include damage/scaling, critical multiplier/chance, cooldown including shooting animation, range in reference units, knockback, applicable piercing/bounce/lifesteal/reload and special effects. Modified damage/range/cooldown are colored, with base damage/range shown in brackets. Names include the reference's Thief Dagger, Cacti Club and tier suffixes.

`InspectionUI` registers screen-space targets each GUI pass and draws one popup after all other panels. Explicit scroll offsets keep item hit targets aligned with their icons; partially hidden targets are clipped. Popup size follows text height and stays inside the visible canvas. Level-up and crate screens now retain an owned-equipment strip.

Reference body/title sizes are adapted to our uniformly scaled 1440×900 UI: 18/22 logical pixels produce approximately 22/26 physical pixels at 1920×1080. The actual reference font file is used. This is not a claim of pixel-identical full UI: original icons remain, scaling coefficients use stat names rather than inline icons, popup placement uses available left/right space, and existing large branding/buttons retain their styles. Existing combat adaptations listed in fullscreen-and-starter-characters.md remain; source wording does not establish complete gameplay parity.

## Actual bugs found

1. The first expanded test run passed 170/171. A higher-tier Wrench projectile had an empty scaling list; the turret description indexed element zero. Formatting now handles the complete list and the empty case. Every weapon description is checked for unresolved templates and exceptions.
2. Scroll-view coordinates caused owned item popups to miss their targets. Explicit viewport origin minus scroll offset fixed pause, shop and intermission inventories.
3. The capture fixture advanced screen state immediately after `CaptureScreenshot`, whose write is deferred until frame end. Files could contain the next screen under the previous label. A capture/wait/advance sequence now preserves the labeled state. PNG freshness alone could not detect this semantic error.
4. Visual review showed Coffee missing percent signs because serialized effects use `stat_`-prefixed keys. Normalizing keys and sharing effect formatting restored `+10% Attack Speed` and `-2% Damage`; a regression covers signed percentage and flat effects.

The saved catalog was also reserialized using the already-existing nonrecursive ProjectileStats schema; obsolete nested equipment fields disappear from the YAML. This pass does not reintroduce or redesign that schema.

## Verification and interview use

Final build: zero errors/warnings. EditMode: 172/172 pass. Twelve staged screens cover four weapon selections and inspection in pause/shop/level-up/crate phases, with fresh captures, matching requested/rendered targets and on-screen bounds. See `artifacts/inspection-review/inspection.json` and `verification.md`.

Native input: Start → Pugilist → Fist → Danger 0; Escape paused at wave 1 with 16 seconds remaining and 10/10 HP. Pointer inspection showed 18% Dodge with a 60% cap; Tab moved to Speed; Left moved to the second Fist with its adjusted cooldown/range. The run remains paused. This confirms mouse/keyboard inspection, not a full survival playthrough. Item and intermission coverage is staged rendering, not a native-input playthrough of every reward screen.

Interview discussion: distinguish eligibility filtering from locked presentation; centralize current-value formatting; reason about nested GUI coordinate spaces; explain why fresh screenshots still require visual inspection. Shaoyu supplied requirements and corrections. Codex implemented, debugged and ran these checks. Review and reproduce the reasoning before describing agent-run work as personally executed debugging.
