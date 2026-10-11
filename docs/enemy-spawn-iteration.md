# Enemy, spawn and UI comparison — October 10, 2026

User direction: keep twenty waves, reduce the offered item pool, and bring combat and UI closer to the supplied Brotato reference. The brief briefly changed to five waves, then explicitly returned to twenty before implementation.

## Evidence read before implementation

- `E:/UnityProjects/Brotato/zones/wave_manager.gd`: one-second scheduling; the initial group chance does not cancel its future repeats. First repeat uses the original interval; later repeats reduce the interval before scheduling the next repeat.
- `global/entity_spawner.gd`: one shared group origin; square group areas, per-unit additional safe distance, edge-preserving placement, one birth every three physics ticks. The queue threshold accelerates draining; it is not a hard cutoff that silently discards the rest of a group.
- `entities/birth/entity_birth.gd`: a one-second X warning, faster flicker near activation, relocation and a fresh warning if the player occupies the spawn.
- Charger scene and `charging_attack_behavior.gd`: direction locked at preparation, 0.4-second preparation and 0.75-second dash. Spitter scene: range spacing, visible 0.8-second preparation before its projectile.
- Buffer scene: holds about 500 pixels from the player instead of wandering like a looter. Pursuers increase pressure with time and reset on a successful contact.
- Screenshots `Brotato_j2seABbqdB.png`, `Brotato_1I4vrFxXQE.png`, `Brotato_o9JlhVG5Sb.png`: four black shop cards, small icons beside names, green/red stat changes, reroll above cards, stats on the right, inventory below; compact top-left health/XP/materials and centered wave/time.

## Intended changes and boundaries

Preserve the imported twenty-wave schedule and six weapon families. Offer eight readable items: Coffee, Beanie, Glasses, Helmet, Leather Vest, Bat, Fertilizer and Cake. Existing catalog entries remain available for reference/evidence, but ordinary shop rolls use the reduced pool.

Repair schedule and placement discrepancies; add charge preparation/locked direction/recovery, ranged windup, visible role size/facing/motion differences, spawn emergence and distinct hostile bullets. Keep generated original sprites and supplied reference audio. Rebuild HUD/shop/upgrade presentation around the screenshot hierarchy.

This remains an adaptation in a smaller arena, not full Brotato parity. Complete boss mutations, thief evolution, extra difficulty enemies, crates, all stat systems and characters remain outside this pass. Automated fixtures are staged and cannot establish subjective combat quality or human balance acceptance.


## Implemented and checked

The imported twenty waves remain. Eight items are offered in ordinary rolls, while historical catalog media stays traceable. The UI now follows the reference's four-card/right-stats arrangement, with green/red item effects, calculated weapon details, compact HUD and a wardrobe preview. The floor palette is muted so warning and attack colors stand out.

`SpawnPlacement` preserves a group's square or edge constraints and honors additional unit clearance where feasible; impossible requests use the safest bounded sample within that region. `EnemyAttack` drives committed preparation, active movement/fire, and recovery. Small grunts, heavier bruisers/spawners and the boss have distinct sizes; facing and body motion respond to movement. Buffers hold their distance. Hostile bullets have coral rims and bright centers and are removed at arena bounds. Ranged shots now carry their weapon's knockback.

Verification: 53 passing tests, clean build, AssetScope 198 assets/1,337 usages/zero findings, inspected UI/enemy captures and a normal first-wave smoke. An accelerated invulnerable fixture progressed through all twenty waves and won, making 44 real affordable purchases; this is integration evidence rather than normal balance acceptance. See `verification.md` for exact scope and reports.
