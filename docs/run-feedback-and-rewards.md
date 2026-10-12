# Run feedback and reward UI correction

October 11, 2026. Requested by Shaoyu after playing the previous build.

## Starting weapons

Every selectable profile now starts with its fixed weapon plus the selected weapon. Rookie and Sharpshot carry Pistol; Pugilist carries Fist; Hothead carries Knife. The portrait tray displays the fixed weapon. This deliberately extends the reference's bonus-weapon behavior to all four profiles at the user's request; it is not a claim that every reference character starts with two weapons. Starting eligibility and other profile modifiers remain unchanged. Regression iterates every unlocked choice for every profile.

## Targeting, trees and enemy warning

Compared `weapons/weapon.gd`: detection radius is maximum range +200 source pixels, while firing is gated at maximum range +50. Targets are measured from the weapon mount. Unity now aims at a detected enemy before it is close enough to attack, retains the existing attack/cooldown rules, and follows the target during shooting. No reference bitmap changed.

Trees already respawn through the imported repeating wave groups and their separate birth queue (first group begins at second 10, repeats every 10 seconds with source counts/chance). They are not guaranteed to appear at every interval. Tree warnings were incorrectly red. The same one-second warning now has an original green cross made from two strokes, followed by the existing spring birth. A tinted red sprite cannot reliably become green, so geometry is used rather than multiplying its colors.

The orange line before a ranged enemy shoots was an intentionally added Unity direction cue, not a projectile defect. Removed it for ranged enemies; their windup and projectile timing remain. Charge direction cues remain a documented presentation adaptation.

## Wave-end and HUD

Previously, remaining materials were banked and their objects destroyed in one frame. The new 1.8-second presentation pauses combat, draws them toward the top-left stored-material bag and visibly accumulates its count. Consumables travel to the player and resolve through the existing pickup path. Harvesting appears separately before the reward transition; its existing cash/XP and positive 5% growth rules are unchanged. Unclaimed materials remain stored for later doubled pickups, not immediate spendable money. The fixed 1.8-second duration is our presentation choice, not exact reference cleanup timing.

Pending levels now appear as green upgrade-arrow icons with level numbers at the top right; pending crates use original box glyphs beneath. The HUD shows stored-material and harvesting values below money. Icons reuse typography/code geometry, not copied art. Reference sources: `main.gd` cleanup/harvesting/on_levelled_up, `ui/hud/ui_bonus_gold.tscn`, `ui_upgrade_to_process_list.gd` and `ui_upgrade_to_process_element.tscn`.

## Weapon and crate UI

Compared `ui/menus/shop/item_popup.gd/.tscn`: owned weapons open an action card on left or right click, containing the full description, attached set thresholds and vertically stacked Combine, Recycle, Cancel actions. Combine is disabled without a matching tier/weapon. Escape closes the card; shop mouse, Enter and reroll input are blocked behind it.

Crates were already implemented: pickup healing and queued items, processed before level upgrades, with Take/Recycle. The old oversized reward panel has been replaced by a compact item description card and vertically stacked actions, following `ui/menus/ingame/upgrades_ui_player_container.tscn`. Level upgrades and crate item rewards remain distinct. Original art retained; unsupported item bans/unlocks are not introduced by this pass.

## Evidence and ownership

See verification.md and artifacts/reward-feedback-review for build/test/render evidence. Staged fixtures exercise the normal renderers and sweep method; they are not a human full-run playthrough or a pixel-perfect UI claim. Shaoyu identified the symptoms and requested behavior; Codex compared source, implemented changes, ran checks and documented limitations. Useful interview discussion: distinguishing a missing mechanic from a missing presentation, preserving economy semantics while animating cleanup, and separating target detection from attack range.

## October 12 correction: money-icon destination

Shaoyu clarified that wave-end materials should fly to the money icon, not the stored-material bag. The destination now uses the center of the money sprite (HUD y=122.5), with a brief counter pulse and rising green +amount text on arrival. Several simultaneous arrivals aggregate into one readable amount. Removed the timeout that destroyed a pickup before it reached its destination; reward transition waits for all pickups to arrive. Existing stored-material economy is unchanged. Reduced Motion suppresses the scale/vertical animation. The runtime fixture now advances cleanup in 50 ms steps rather than one large jump.

## October 12 - HUD simplification and corrected base catalog count

Harvesting is directly beneath money with a 40x40 icon (previously 28x28); stored reserves move below harvesting. Pending levels show one arrow while any upgrades remain, without a level number or repeated icons. The actual upgrade queue is unchanged.

Fresh audit uses the registered resources in the supplied base item_service.tscn before DLC injection, deduplicating item my_id values and counting weapon families instead of tiers. Base: 62 weapons / 209 items. Imported: 30 weapons / 74 items; obtainable: 24 / 64; imported but locked: 6 / 10. Missing implementation: 32 weapon families / 135 items. Thus 38 weapons / 145 items are currently unavailable when locked imports are included. artifacts/base-catalog-coverage.json lists missing IDs and item source paths.

This supersedes the previous 201/127 item claim: the folder glob included burning-effect data and a second hourglass resource under one folder, while missing registered files with nonstandard names. Registered item identities are the authoritative denominator for this local reference, not a claim about a newer release.
