# Architecture and tradeoffs

## Original art integration

`OriginalArtBuilder` applies versioned Unity import migrations for six transparent atlases, preserving source pixels and binding sprite subassets into the catalog and animated prefab. `SpriteFrameAnimator` is driven by the arena simulation, with independent idle/walk frames and persistent facing. Weapon recoil/muzzle sprites remain separate from body animation. `ScrapMotion` owns scatter, bounce, settling and attraction; its reward-splitting helper conserves currency under the pickup cap. IMGUI icons use sprite UV rectangles. The opt-in `--art-review` fixture records observed animation/effect states and captures; it is explicitly staged, not a normal-run balance test.

## Runtime

`ArenaGame` coordinates input and scene objects. Combat advances only in Playing; Upgrade, Shop, Paused and terminal screens stop simulation without changing global Time.timeScale. `RunRules` owns currency, XP, queued levels, inventory, stat effects, shop offers and wave transitions. Its RNG and catalog are injectable.

`SpawnDirector` consumes each wave's scheduled groups. It processes elapsed integer seconds in chronological order, preserving RNG order across frame rates. Conditional buckets choose one group. Unit counts use integer random counts multiplied by spawn chance, with a fractional Bernoulli remainder. `ReferenceCombat` places groups, queues births at 0.05-second spacing and shows a one-second warning. Player overlap relocates and restarts a warning. A 100-enemy cap bounds active entities.

Enemy roles implement chase, charge, keep-distance shooting, wandering shooting, buffing, pursuing, loot movement, static trees and death spawning. A single adapted boss combines charge/projectile patterns. Dead enemies leave active collision immediately, then a separate corpse list shrinks/spins/darkens their sprites. Lightweight separation reduces crowd stacking.

Each of six equipped slots owns a cooldown and a visible weapon. Ranged attacks sweep their travel segment, track already-hit enemies for piercing and use source pellet/timing/tier data. Melee attacks sweep a short segment on cooldown. Shared stats modify damage, cadence, criticals, armor, dodge, life steal, regeneration, movement, range and harvesting. The bounded catalog does not implement weapon-set bonuses or all source scaling formulas.

## State invariants

- XP never consumes spending currency. Leveling during combat never pauses the wave.
- Only queued rewards produce upgrade cards. Only GO from Shop advances a wave.
- Purchases atomically spend currency and remove their offer. Failed purchases leave state unchanged.
- Six weapon slots maximum. Matching equal tiers can combine; tier four cannot combine further.
- Locked offers retain their identity through rerolls and across waves.
- Menu/end states reject combat mutation. Restart clears the entire run.
- A dead enemy cannot take another hit or inflict contact damage through its corpse.
- Defeating the final boss can end wave 20 early; otherwise surviving its timer wins.

## Assets and authoring

Scene -> ArenaGame -> ReferenceCatalog -> wave/enemy/equipment/item/upgrade records -> Sprite/AudioClip/Material assets. Existing player/effect prefabs still refer to meshes/materials. GameAudio refers to SoundBank, with 16 bounded voices and four reserved important voices; weapon slots can supply their own reference sound variants.

`scripts/analyze_reference.py` inventories the supplied Godot resources without executing them. `scripts/import_reference.py` copies selected media, records hashes and generates JSON. Versioned Editor migration imports sprites/audio and assigns actual Unity object references to the catalog. Re-running migration preserves GUIDs. Edit the saved catalog for ordinary authoring; changing source JSON alone does not overwrite an existing migrated catalog.

AssetScope 0.3.0 remains Editor-only. Full scans inspect saved scenes and serialized properties with cache disabled. Integration tests follow actual audio references to the owning asset.

## Tradeoffs and limits

IMGUI minimizes implementation size but needs a future navigation/accessibility pass. Distance queries and separation are quadratic at these bounds; pooling/spatial indexing should follow profiler evidence. Instantiation remains capped rather than pooled. World geometry, target-spacing details and the final boss are adaptations, so imported schedule values do not imply full behavioral/balance parity. Presentation fixtures explicitly grant equipment/currency/health and cannot establish normal-run difficulty.


SpawnPlacement owns bounded safe sampling without losing group/edge geometry; SpawnDirector invokes a group-start callback once before all unit types in an occurrence. EnemyAttack separates approach, committed windup, active attack and recovery. Runtime presentation consumes that state for tint, squash and direction cues; reduced motion suppresses squash/tilt while retaining essential cues. EnemyReview and CampaignCheck are explicit command-line-only fixtures. CampaignCheck supplies invulnerability/equipment and accelerates actual simulation; it cannot validate normal balance. Ordinary shops filter the retained serialized item catalog down to eight allowed ids.

## Equipped weapon sets

`WeaponSets` is the shared membership/threshold definition used by shop preferences, UI and `RunRules`. `RunRules.Stat` combines permanent values with the current inventory contribution; `Add` changes only permanent values. Scanning at most six equipped slots avoids a mutable bonus cache and invalidation bookkeeping. `WeaponRange` is shared by combat targeting/projectile reach and UI descriptions, including the source's half-strength Range scaling for melee. Buy/combine/recycle clamp current HP after a Primitive count reduction.

## Loot and survival sequencing

`ReferenceLoot` owns base-difficulty per-enemy drop profiles; `cratesSpawned` resets at each wave and increments on spawn, not pickup. `LootRules` keeps pending crates separate from levels. One selected item remains stable until take/recycle resolves it; the same `ApplyItem` function serves shop purchases and crate rewards. `FinishWavePickups` collects harmless consumables while Playing before `Tick` opens Loot, Upgrade or Shop. Materials retain their separate bank. Crates use runtime line geometry with the existing material; repair/material sprites are unchanged.

`SurvivalRules` advances regeneration and shared life-steal clocks only during Playing. Timed whole-HP regeneration resets at full health; life steal is limited globally across all weapons. The new Loot enum value is appended to preserve existing phase values. UI keyboard handling resolves crates after the shop-input branch so one Enter press cannot also start the next wave.

## Spring-body visual hierarchy

The player root remains at the simulation position. Its child body receives `SpringMotion` scale and vertical offset, and worn items parent to that body. Two tiny foot renderers remain siblings so body squash does not enlarge them. Weapon mounts are independent. Horizontal facing mirrors sprites/offsets while the same simple poses serve all directions. Enemy presentation samples the same spring landmarks with adapted cadence; gameplay positions/collision are not driven by the visible bounce. The versioned import migration preserves the player prefab identity and uses explicit enemy sprite rectangles.


## Modular appearance composition

PlayerLook and AppearanceLayer serialize cosmetic presets into the player prefab; ItemEntry.appearance holds a dedicated wearable sprite, slot, priority, offset, scale and drawing order. AppearanceResolver composes base layers plus unique acquired items. CharacterWardrobe pools SpriteRenderers under the spring body and refreshes when look/item identity changes, including same-count replacement. ShopUI uses the same resolver. Body and face squash together; pointed legs are siblings with alternating translation/rotation. Gameplay stats and six weapon mounts remain separate. AppearanceBuilder is a versioned editor migration preserving the prefab GUID/root identity; AssetScope can inspect the resulting serialized sprite dependencies.


ActorPresentation separates world visual scale from combat radii. GroundShadow owns a world-space circle renderer; actor activation and destruction control its lifetime while explicit ground anchors prevent inherited squash/rotation. ReferenceCatalog serializes the mesh/material for build and AssetScope dependency inspection. The v2 curved leg is shared by world/title/shop.
