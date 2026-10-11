# Crash Zone reference study and implementation contract

Source: user-supplied `E:/UnityProjects/Brotato`, an extracted Godot project. This is reference evidence, not a claim that Brotato's source is officially open source. `scripts/analyze_reference.py` produces the auditable inventory in `artifacts/reference-analysis.json`. Analysis precedes implementation in Git.

## Findings

- `zones/zone_1`: 20 waves, lasting 20, 25, 30, 35, 40, 45, 50, 55, eleven 60-second waves, then 90 seconds. Each has a 100-enemy cap. Wave 1 starts a 4–5 baby-alien group at second 1, repeated every 3 seconds. Later waves add mixtures, clusters, edge groups and conditional alternatives; this is not a continuously accelerating edge-only spawner.
- `wave_manager.gd`, `entity_spawner.gd`: group start, repeats, interval reduction, difficulty/wave gates, fractional unit count chance and area are data. Births are queued three physics frames apart. Positions initially avoid the player by 300 pixels. Red X warnings last one second; an overlapping player relocates/restarts the warning.
- Enemy stats grow by base + per-wave increment * (wave - 1). Distinct roles include chase, charge, ranged spacing, wandering shooters, healers, buffers, eggs and death-spawners. A spawner releases three units on death, not periodically. Eggs hatch after four seconds. Charger preparation is 0.4 seconds followed by a 0.75-second charge.
- `entities/entity.tscn`: death disables gameplay immediately but retains a visual corpse. Over 0.5 seconds it rotates 360 degrees, shrinks nearly to zero and darkens to 0.349. Pool cleanup occurs around one second.
- XP and spendable materials are separate. XP required for the next level is `(3 + nextLevel)^2`; level 2 requires 25. Level rewards are deferred until wave end. Four stat choices precede a separate four-offer shop.
- Shop: six weapon slots, duplicate equal-tier combinations, four tiers, items with stat tradeoffs, individual offer locking, paid rerolls, recycling. First two shops guarantee two weapons; through wave five guarantee one. Base price becomes `int(base + wave + base * wave * 0.1)`. Recycling returns 25%. Reroll costs grow with wave and repeat count.
- UI separates currency, XP, health, countdown, stat upgrades and inventory. Dark cards, white type, rarity borders, green prices, large icons and visible weapon slots do the work; decorative panels are secondary.

## Unity scope

Recreate the base solo difficulty-0 wave data and enemy roles, and a bounded but functional equipment/item catalog. Preserve AssetScope's real serialized references. Reuse selected supplied enemy/weapon/item sprites and sound clips at the user's request, with per-file provenance. Unity runtime implementation is separate from the Godot reference.

Do not claim full commercial-game parity: DLC, multiplayer, all characters/items, curses, achievements, endless progression and every boss mutation are outside this bounded project. Validate the economy/state transitions and all scheduled waves in tests, then validate actual Windows gameplay and shop images. Full-run balance requires longer human playtesting.

## Interview framing

The engineering problem is translating data-driven encounter composition and a multi-stage economy into testable Unity rules while preserving readable feedback. Record actual failures and fixes, not fictional debugging stories. Shaoyu directed requirements and reference feedback; Codex performs this implementation and automated verification. Supplied Brotato art/audio remain third-party work.
