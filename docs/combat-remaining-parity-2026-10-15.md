# Remaining combat pass — 2026-10-15

This pass compares the current runtime with the supplied read-only source at `E:/UnityProjects/brotato-(3.x)`. It preserves the accepted controller/audio work and the existing sampled melee hit windows. Historical small-catalog limitations in older audit documents are not treated as the current implementation.

## Corrections

### Burning has no weapon hitbox

- Source `entities/units/unit/unit.gd:665-696` creates burning `TakeDamageArgs` without a hitbox, disables armor/dodge, and supplies the burn sound. `take_damage_args.gd:26` defaults its hitbox to null.
- Source `unit.gd:270-285` initializes critical chance and knockback to zero; `unit.gd:333` unconditionally assigns the resulting knockback. A burn therefore **clears existing knockback**, rather than retaining or adding a global weapon impulse.
- Source `weapons/weapon.gd:419-438` and `projectiles/player_projectile.gd:113-116` dispatch life steal from actual hitbox callbacks. The burn's `on_weapon_hit_something(..., null)` call records weapon damage and returns before attack-specific processing (`weapon.gd:446-451`); it does not dispatch life steal.
- Scrapshift's shared `Damage` previously added global knockback and attempted life steal for the burn call. Its negative crit sentinel could also be overcome by a sufficiently large stacked critical stat. `ReferenceCombat.cs` now explicitly excludes these effects for the existing burning/bypass-armor path, including death knockback. Ordinary hits retain their prior behavior.

### Automatic melee follows its current target

- Source `weapons/weapon.gd:179-190` updates automatic shooting rotation through `get_direction`; `weapon.gd:265-270` derives that direction from the current target's position. `weapons/melee/melee_weapon.gd:32-33` separately freezes manual rotation while shooting.
- Scrapshift already computed a tracked automatic direction, but its sampled melee collision and final presentation always used the angle captured at launch. `ReferenceWeaponBehavior.cs` now applies the current weapon root direction before sampling the existing recoil/strike/return pose. `ReferenceCombat.cs` retains the previous root direction for manual melee during its swing.
- The captured initial knockback direction stays unchanged, as the source updates its hitbox knockback when shooting begins (`weapon.gd:366-367`). No instant-melee approximation was introduced.

### Captured accuracy spread survives automatic tracking

- Source `weapon.gd:360-362` draws `_current_shoot_spread` once when shooting, and `get_direction:265-270` adds that same angular offset to the current target direction for the remainder of the animation. Manual ranged aim instead follows the input vector each physics update; manual melee retains its last rotation.
- `ReferenceCombat.cs` now stores that draw per weapon and applies it through the shared runtime `AimEquipment` state update. It resets on equipment synchronization. Automatic target movement and target removal do not draw a new value; idle/new manual aim does not inherit the previous attack's offset.

## Validation

`CombatRemainingParityTests.cs` contains five runtime integration regressions:

1. A real `ArenaGame.Damage` burn call ignores armor, cannot crit/heal or consume the life-steal cooldown, and clears an existing knockback vector even with stacked offensive stats.
2. An ordinary weapon hit still subtracts armor, heals through guaranteed life steal, and applies the global knockback bonus.
3. A real `BeginMeleeAttack`/`TickMeleeAttack` changes the root angle when the tracked direction moves, without skipping recoil, sounding early, restarting attack age or changing the captured knockback direction.

4. The runtime aim update retains the same captured spread through automatic target movement/removal and removes it when the shooting animation ends.
5. The runtime aim update freezes manual melee, follows manual ranged input without reapplying old spread, and permits aiming a new melee attack.

Tests restore Unity random state and destroy their generated feedback objects. No user settings or saves are modified. Unity execution and combined validation belong to the coordinating agent; this document does not claim those tests have passed before their results are read.

## Limits

These are bounded source-backed corrections, not a complete combat-equivalence claim. Native moving-target melee and burn feedback still need observation in the combined build. Godot/Unity scheduling, random generators, physics overlap ordering, and our independent artwork remain distinct. Projectile beam collision still uses the current capsule-style travel query rather than the source's animated rectangle; its exact Godot cubic animation interpolation was not reconstructed in this pass.

## R7 integrated validation - completed

Full suite passed 594/594, zero failed/skipped, 72.8708967 seconds. Native built-player support fixture passed 27/27 and exited zero. R7 build succeeded with zero errors/warnings. Evidence: artifacts/gameplay-parity-continuation-2026-10-15/tests.xml, build-manifest.json and native-support/pets.json. These are automated/native fixture results, separate from human full-run playtesting. Further beam/zone geometry comparisons continue separately after the R7 checkpoint.

