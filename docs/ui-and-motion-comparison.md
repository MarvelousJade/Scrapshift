# UI and motion comparison - October 11, 2026

Shaoyu requested removal of the Start keyboard suffix and lower-left control legend, comparison of every game UI against the running Brotato reference, silver rounded legs, and a moving/shooting/landing check. Existing character, enemy, weapon and item artwork is retained; only the requested limb sprite is replaced.

## Evidence and decisions

Live reference: user-supplied Godot 3.6.2 project E:/UnityProjects/brotato-(3.x), version 1.1.15.4. Inspected title, character selection, weapon selection, Danger 0, first-wave landing/combat, loss and summary through Computer Use. The short stationary reference runs died in wave 1; they are not successful playthroughs. Pause layout also uses the previously captured live pause and its scene. Shop/upgrades/reward layout is compared against local UI scenes and existing reference evidence. Exact recoil timings come from source, not visual frame-time measurements. Reference screenshots remain local in ignored artifacts/raw; comparison.html pairs available captures without editing their pixels.

| Surface | Change |
|---|---|
| Title | Start has no Enter suffix; removed movement/sound/pause legend; left menu, large heading, subdued crowd made from our enemy sprites. |
| Character / weapon selection | Back at upper left; wide character preview with stats beside it; separate weapon description; lower icon row; hover previews selection and click advances. Our three appearances share starting stats, which remains explicit. |
| Difficulty | Character, weapon and Danger 0 panels; click the supported danger choice to begin. No fake locked difficulties or nonfunctional multiplayer controls. |
| HUD | Removed bottom control legend and redundant top-right wave counter; larger outlined wave/time; level right-aligned in XP bar. |
| Pause | Left actions, six weapon slots and scrollable acquired items, stats on right, twenty-wave timeline; separate functional audio/motion options. |
| Shop / upgrades / reward | Consistent dark rounded buttons with light hover; removed keyboard badges; complete stat-icon coverage and percentage units. Existing buy/lock/reroll/combine/recycle rules retained. |
| Ending | Loss identifies attacker above an icon and Ok button. Summary puts stats on the left, weapons/items centrally, progress on the right, and three bottom run actions. |
| Item status | Inventory hover exposes item effects and weapon details in pause/end as well as shop. Weapon-set totals remain visible in intermission/pause. |

## Motion and art

Source weapons/weapon.gd holds cooldown while _is_shooting, retains the firing target, and tracks its current direction until it disappears. The ranged shooting behavior moves the sprite backward then forward using two exponential-out tweens. SMG: 10px and .05s per half; pistol/shotgun: 25px and .1s; laser: 40px and .2s. Our former common .17-unit recoil, seven-degree kick and continuously running cooldown did not match that contract. WeaponMotion now separates shooting and cooldown time, including the frame that crosses the boundary. Positive attack speed reduces both recoil time and distance as weapon_service.gd does; negative speed does not stretch recoil. Weapon descriptions include shooting duration in the displayed interval. Original weapon art, count-specific mounts and front/back sorting are retained. Target death preserves aim until the shooting interval finishes.

Landing squash now affects the body and worn decorations rather than scaling the entire actor and legs. The native canopy changes from a broad shallow umbrella to a taller rounded shape after the live comparison. Existing curved yard geometry and low-contrast debris passed visual inspection and were retained.

player-leg-v3.png is an ImageGen edit of our player-leg-v2.png, generated with actual transparency, copied without pixel editing, alpha-trimmed by the existing Unity sprite importer. Prompt: silver-leg-prompt.txt. Generated source: C:/Users/FanSh/.codex/generated_images/01a12d09-527c-71e2-851b-a118bb06309e/exec-4ae7f572-7b77-47ce-82aa-439e70d46771.png. Native size 1774x887; shared in world, title and wardrobe previews. Limb height .16 and center spacing +/-.31 in player-local units. Existing sprite tests were updated to the deliberately wider rounded silhouette.

Stat icons formerly depended solely on eight upgrade offers, leaving non-upgrade stats blank. A separate complete lookup reuses those eight original sprites and adds eight small code-authored transparent glyphs. All sixteen primary rows now have serialized assets discoverable by AssetScope. Engineering and elemental damage are displayed as zero with this small catalog; adding their icons does not implement elemental weapons or structures. Luck uses the existing rarity/loot stat path.

## Remaining differences

This is closer layout/interaction matching, not pixel-identical or full-content parity. Scrapshift retains its original art/font, 1440x900 authored UI, three cosmetic appearances, six weapon families, eight shop items and one difficulty. Reference primary/secondary stat tabs, item sorting, profiles, unlocks, codex, multiplayer and DLC menus are not reproduced. The first-wave timer is still held during our 1.2-second arrival; the reference starts its timer independently. Melee still uses the documented simplified hit resolution and animation. Sound content is unchanged in this pass. Human feel/balance acceptance is open.

## Verification approach

The regression suite checks serialized icon completeness, all four ranged recoil families, cooldown boundary consumption and positive/negative attack-speed scaling, alongside existing scene/animation/combat tests. The presentation fixture renders eighteen current screens/poses and requires fresh PNGs, actual saved floor/canopy, selected-weapon start, landing completion, loss/restart/title and twenty-wave transition assertions, plus pause/upgrade/shop/loot phases. It freezes AI and accelerates completion; it is not a twenty-wave gameplay run. The world fixture uses real AI/shooting, four-direction movement, restored HP and supplied gear. Numerical and image results are recorded in verification.md.

For interview discussion: the useful debugging distinction is that an animation interval can be part of firing cadence, not just decoration. A data-model mistake also caused the missing icons: the set of upgrade offers is smaller than the set of visible stats. Shaoyu supplied the design direction and reference; Codex implemented, debugged and verified these changes. Study and reproduce the reasoning before claiming personal implementation experience.
