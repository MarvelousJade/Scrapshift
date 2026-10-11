# Purpose and scope

Build a Unity portfolio game that gives AssetScope real authoring problems: a scene refers to a catalog, the catalog owns sprites and audio, and multiple weapons share media/materials. The user selected a Brotato-style game and later explicitly expanded the brief from a tiny survival loop to a close reference study of spawning, progression and presentation.

## Current playable loop

A run contains the reference's 20 base Crash Zone wave durations. Wave 1 lasts 20 seconds; later waves lengthen to 60, with 90 seconds for the finale. Timed groups choose an area, queue births and display red warning Xs before enemies appear. Enemy health/damage grow with wave number. Movement, melee/ranged attacks, knockback, hit flashes, damage numbers and shrinking/spinning corpses communicate combat.

Collected materials grant spendable currency and XP independently. Crossing an XP threshold queues a level reward without pausing combat. At wave end, choose one of four stat upgrades per queued level, then enter a separate four-offer shop. Buy items with positive/negative stat effects, equip up to six weapons, combine duplicates, recycle, pay to reroll and lock individual offers. Health resets for the next wave. Uncollected materials are banked and double later pickups until the bank is consumed.

## Content boundaries

Six weapon families (pistol, SMG, shotgun, laser gun, knife, spear), four tiers each, eight offered stat items (24 retained catalog records) and eight level-up stats. 18 reference enemy entries cover the base schedule plus spawned junkies/slashers and one adapted Predator finale. No higher-difficulty roster, DLC, alternate bosses, curses, multiplayer, meta-progression or full-game parity claim. The arena keeps the existing 21-by-10.2 playable bounds rather than reproducing the reference's tilemap dimensions. Weapon class bonuses and every special item interaction are not present.

The original procedural yard remains; six AI-generated sheets now provide the animated scavenger, mechanical enemy silhouettes, weapon/item/stat icons, warnings and green material shapes. See `original-art.md` and its full prompts. Twelve effect-cue families still use user-supplied reference audio; remaining terminal/legacy cues use the prior original bank. Historical borrowed sprites retain provenance but are no longer scene dependencies. Generated art is explicitly attributed to the assistant/tool, not claimed as candidate hand-drawn art.

## Engineering choices

- Pure run rules and an injectable seeded spawn director make economy and timing reproducible.
- A serialized ReferenceCatalog preserves real Sprite, AudioClip and Material dependencies for AssetScope.
- Bounded lists and swept collision keep the prototype inspectable. They are not evidence of measured production performance.
- IMGUI and keyboard input remain appropriate for this local study; controller navigation, rebinding and accessibility need a later UI/input pass.
- Keep the finished AssetScope 0.3.0 package embedded at its tested snapshot. Runtime code does not depend on Editor assemblies.

## Acceptance

Build and test the rules, inspect actual rendered combat/shop/upgrades, sample real mixer output and document real defects. Keep scripted fixture results distinct from normal play and human balance feedback. The earlier five-wave/three-weapon scope is historical and superseded by the explicit reference-expansion request.
