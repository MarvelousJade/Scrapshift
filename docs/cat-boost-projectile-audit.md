# Catling proximity animation and projectile audit

Reference: `entities/units/pet/catling_gun/catling_gun.gd` and `.tscn` in the local original project. Player/eligible-pet proximity stops the cat and halves its gun cooldown. Enemy proximity is not the trigger. `enter_in_mad_mode` lasts 0.2 seconds, then plays `idle_attack`.

Scrapshift now adapts the entry scale track to its connected body and uses a distinct sustained boosted pulse. Angry entry sound and half-interval alternating guns remain. It does not reproduce the original's separately animated ears, tail, and changing face textures; the extra leg mesh remains disabled.

28 support tests passed. Native fixture captures entry and sustained boost and checks the existing proximity/alternating gun behavior.

## Projectile audit

Current `StarterProjectile` creates `catalog.muzzleFrames[2]` for support shots, with a green tint for healing. Damage/burning/explosion/healing logic differs, but the visuals remain generic.

Original turret stats explicitly reference `blue_flames/blue_flame_projectile.tscn`, `rocket/rocket_projectile.tscn`, `bullet_laser/bullet_laser.tscn`, and `bullet_medical/turret_healing/bullet_turret_healing.tscn`. Distinct original projectile presentation is not implemented by this change.
