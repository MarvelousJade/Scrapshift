# Item expansion and reference design comparison

**Count correction, October 12:** the registered base catalog has 209 unique items, with 135 not imported. The 201/127 folder-scan counts below are historical and superseded by `artifacts/base-catalog-coverage.json`.

October 11, 2026. Reference: user-supplied Brotato 1.1.15.4 Godot 3.x project. Original character, enemy, weapon and item artwork retained.

## Scope and result

Expanded the eight-item shop pool to 64 obtainable items. The serialized catalog contains 74 supported base items: 64 default-unlocked and 10 locked. All 74 use the reference tier, base price, flat effects, tags, group membership and maximum-count metadata. The source base catalog contains 201 items; 127 require special mechanics and remain excluded. This is a completed flat-stat expansion, not full gameplay parity or identical full-catalog probabilities. Unlock progression is not implemented.

The importer accepts only ordinary flat-stat effect resources with supported keys. It rejects scripted/custom effects even when their fields resemble stat changes. `scripts/audit_item_design.py` produces the reproducible data and source hashes in `artifacts/item-design-audit.json`; there were zero conflicting tier/price/effect values among previously imported matching records. English names and effect templates are exported separately from the supplied translation resources.

Twenty-two existing authored item illustrations and wearable appearances survive migration. Fifty-two additional definitions use our existing stat glyphs as icons and have no unique wearable decoration yet. No new reference sprite pixels were placed into the game.

## Shop and combat data

Compared `singletons/item_service.gd`, item resources and four character resources. Shops retain four slots, two guaranteed weapons in waves 1–2 and one in waves 3–5, then 35% weapon chance. Preferred-item tags use the reference 5% branch for the included profiles. Character bans, owned/locked item limits and current-offer exclusions apply. Preferred weapon matching now accepts any shared set, including secondary sets. Empty preferred pools fall back; hard character restrictions remain enforced. The smaller catalog necessarily changes individual item odds.

Expanded penalties exposed old stat boundaries: negative critical chance must subtract from a weapon's base chance; critical chance must not cap at 75%; damage modifiers may reach zero before the final minimum-one damage rule; minimum weapon range is 25 reference pixels. Negative attack speed lengthens cooldown multiplicatively. Cooldown ticks and melee animation duration now follow the inspected formulas. Live Brawler/Fist inspection showed 0.47 seconds, reproduced by the new regression. Melee collision shapes, timing integration and weapon cooldown randomness still have adaptations; this is not a claim that every combat mechanic is identical.

## UI and arrival comparison

Live reference screenshots are saved beside Unity captures in `artifacts/design-parity-review`. Character modifiers preserve source order with an icon, signed colored value and regular label text. The extra starting weapon sits beneath the portrait. The imported reference font is retained. Weapon-set threshold panels attach to the inspected weapon in pause, shop and rewards; active thresholds are highlighted, inactive thresholds dimmed. Removed the persistent set list under Stats. Tooltip backgrounds are opaque so underlying text does not interfere.

The reference parachute enters already open: it has no separate inflation clip. Its scene holds the open scale through 0.7 seconds, stretches at 0.8, flattens at 0.9 and disappears at 1.0, with landing finishing at 1.2. Our native mesh now has a dome, curved lower recesses and five suspension lines, using those scale keys and the inspected sway angles. Body artwork remains unchanged. Reduced Motion remains available.

## Bugs found and resolved

- Catalog migration initially retained only the eight old shop appearances and discarded other authored decorations. Preserve any original non-reference image/appearance before merging source metadata. The retained set is 22, not all 24 old definitions: two require unsupported special effects.
- An existing health test assumed a weapon stat change always restored full health. Expanded shop RNG exposed a prior maximum-health increase without healing. The correct invariant is clamping existing health to the new maximum; the assertion now checks that behavior.
- The previous oval-canopy migration overwrote the intended parachute silhouette. Replaced its shape and migration marker. Render inspection then exposed a center-fan triangulation artifact at concave scallops; a vertical triangle strip preserves the curved edge.
- Semi-transparent inspection panels allowed stat labels to show through; made their fill opaque.

## Verification and interview attribution

The final verification record is in `verification.md`. Tests compare all 74 serialized item records, purchase all 64 obtainable items and check all primary stat deltas, exercise bans/tags/limits/secondary sets, check penalty boundaries and source melee cadence, and verify saved parachute geometry/motion keys. Runtime screenshots are staged through the normal UI/render code with forced inspection focus; they are not a human full-run playthrough.

Shaoyu supplied requirements, the reference and visual feedback. Codex performed source comparison, implementation, debugging, automated validation and documentation. Interview discussion should explain the data-driven catalog, migration preservation bug and visual verification catching a geometry defect; do not claim personal manual implementation or user acceptance that has not occurred. The project demonstrates a Unity consumer for AssetScope as well as gameplay/data/debugging work. Remaining work includes special-effect items, bespoke new item art/wearables, unlock progression and broader balance/playtesting.
