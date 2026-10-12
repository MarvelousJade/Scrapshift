# Fullscreen and four starter characters — October 11, 2026

The requested starting roster is Random, four playable characters, then locked slots. This replaces cosmetic-only choices with gameplay profiles while retaining the original modular shell, eyes, hair, beard and silver legs. The supplied reference actually marks five characters unlocked by default; four is an explicit project choice, not a claim about that reference's default unlock state. The roster has 50 character positions: four playable, 46 locked, plus the Random tile. Unlock progression is not implemented.

## Reference and implementation

Read-only reference: `E:/UnityProjects/brotato-(3.x)`. `singletons/item_service.tscn` establishes roster order. `items/characters/{well_rounded,brawler,crazy,ranger}/*_data.tres` and their effect resources establish profiles and starting choices. `scripts/import_starter_profiles.py` extracts the data; `artifacts/starter-provenance.json` records source hashes. Reference character and weapon artwork is not imported.

| Project name | Reference profile | Character modifiers, before weapon-set bonuses |
| --- | --- | --- |
| Rookie | Well-Rounded | +5 Max HP, +5% Speed, +8 Harvesting |
| Pugilist | Brawler | +50% attack speed for Unarmed weapons; extra Fist; +15% Dodge; -50 Range; -50 Ranged Damage |
| Hothead | Crazy | +100 Range for Precise weapons; +25% Attack Speed; extra Knife; -30% Dodge; -10 Engineering; -10 Ranged Damage |
| Sharpshot | Ranger | +50 Range; extra Pistol; +50% Ranged Damage gains; no melee weapons; -25% Max HP gains |

The extra weapon is added to the selected weapon. Choosing Fist as Pugilist therefore starts with two Fists. Stat gain multipliers apply to accumulated effects and set bonuses, not base HP. Class attack speed and range apply only to matching weapon sets. Starting previews use the same RunRules calculation as gameplay. For example, two Pistols give Sharpshot 60 Range after the Gun set's +10, and two Unarmed weapons give Pugilist 18 Dodge after the set's +3.

Historical imported lists below include locked definitions. The subsequent inspection/selection pass hides unavailable weapons and groups melee before ranged in the actual selector; see inspection-and-weapon-selection.md.

- Rookie: Knife, Stick, Fist, Chopper (locked), Fighting Stick (locked), Cactus Club, Hatchet (locked), Spiky Shield (locked), Ghost Flint, Wrench, Pistol, SMG, Double Barrel Shotgun, Slingshot, Taser, Wand, Icicle, Rail Gun (locked).
- Pugilist: Fist, Hand, Claw (locked).
- Hothead: Knife, Dagger, Scissors, Lightning Shiv, Claw (locked), Sharp Tooth, Shuriken, Icicle.
- Sharpshot: Pistol, SMG, Revolver, Double Barrel Shotgun, Laser Gun, Shredder, Crossbow, Slingshot, Rail Gun (locked).

The union plus the existing Spear contains 30 families / 120 tier definitions; 24 families are available. Exact source numerical data now includes scaling coefficients, critical multipliers, recoil/cooldown, projectile speed, lifesteal, piercing/bounce loss, reload cadence and 14 weapon sets. Secondary projectiles, burning, slowing, stick stacking, critical material drops, ghost stat gains and Wrench turrets have Unity implementations. These additions are needed to make the selected weapons functional rather than icons alone.

This is not complete weapon parity. Melee sweeps and projectile presentation use our existing Unity combat model. Turrets use our Wrench art and adapted placement. Ghost progress is shared by identical catalog weapons of the same tier, retained between waves and reset per run; source per-instance bookkeeping is a remaining difference. Burn cadence is one second. The source banned-item groups are recorded but have no members in our eight-item shop. The larger source item catalog, wanted-tag weighting, unlock achievements and all other character mechanics remain outside this pass.

## Fullscreen diagnosis

The camera deliberately maintained a 1440:900 viewport, so a 16:9 display received pillarboxing. Removing only a background color would not fix that viewport. The camera now fills the window, the UI remains uniformly scaled, background panels cover the extended canvas, floating damage labels use the same coordinate mapping, and the health/material HUD anchors to the physical left edge. Camera bounds use the actual camera aspect ratio.

Windows builds default to native-resolution borderless fullscreen. F11 and the Options display button switch between fullscreen and 1440×900 windowed mode; the choice persists. Explicit `-screen-*` capture arguments and the Unity editor Game view remain caller-controlled.

## Art provenance and slicing bug

Built-in image generation produced `Assets/Scrapshift/Content/OriginalArt/starter-weapons-v1.png` (1536×1024 RGBA), using our previous equipment sheet as a style reference. Generation brief, summarized: a 6×4 atlas of 24 separate weapon objects, thick black outlines, clean wood and silver metal, simple two-tone shading, transparent background, no labels; rows ordered Stick/Fist/Cactus/Ghost Flint/Wrench/Slingshot, Taser/Wand/Icicle/Hand/Dagger/Scissors, Lightning Shiv/Tooth/Shuriken/Revolver/Shredder/Crossbow, Cleaver/Staff/Hatchet/Shield/Railgun/Claw. The Hand and Fist are floating weapons, not hands attached to the character.

The border regression found the shield intersected an assumed 256-pixel row boundary. Instead of changing the generated bitmap, the importer now slices at measured transparent gutters. The existing seven sheets, character body, face layers and enemy art are retained. Source WAVs are copied only for weapon sound references, as requested; they are not original audio.

## Interview evidence and attribution

Useful discussion: separate cosmetic composition from gameplay profiles; apply modifiers at a single stat boundary; preserve weapon-specific exceptions; validate serialized asset references and runtime behavior independently; distinguish a requested screen size from the size the OS actually supplied.

Real debugging evidence includes obsolete appearance tests after roster reordering, a clipping failure caught by a new alpha-border check, and an ultrawide request clamped by the display. Do not invent an initial failing fullscreen test: that issue came from the user's observation and source inspection. Shaoyu directed the scope and art requirements. Codex implemented and ran the recorded checks. Understand and reproduce these changes before presenting them as personal debugging work. Final measured results are in `verification.md` and the runtime reports.
