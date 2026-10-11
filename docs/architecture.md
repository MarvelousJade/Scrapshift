# Architecture and tradeoffs

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
