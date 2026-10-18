# Combat contact timing audit — October 17, 2026

Branch: `codex/gameplay-parity`. Supplied reference inspected read-only at `E:/UnityProjects/brotato-(3.x)`. This is a finite source audit following the earlier 434 passing selected checks. No Unity, native game, build or full run was executed by this audit agent.

## Confirmed differences corrected

### Delayed projectiles must not replay disabled travel

Reference `projectiles/delayed_player_projectile.gd:shoot` disables the damage hitbox, then its physics countdown enables it after a random zero-to-three-frame delay. `overlap/hitbox.gd:enable/disable` applies the collision-shape change deferred. A projectile cannot acquire a damage contact along a path traversed before that hitbox became enabled.

Scrapshift advanced the countdown and then swept the entire old-to-new path once the new countdown reached zero. A target passed earlier during the disabled portion could incorrectly take damage. Following native evidence below, damage contacts now use discrete physics positions. Source delayed profiles start with `delayedHitboxPending`; the parent caller advances the countdown and resolves the resulting endpoint overlap after movement, representing the deferred event flush. Direct helper calls reject a pending hitbox or positive countdown. The lifetime-clamped movement in the caller is retained.

The separate Godot fixture measured deferred activation with forced zero-, one- and two-frame delays. Ordinary initial overlap arrives before the first root physics callback; a delayed zero/one-frame overlap is emitted during the first deferred flush. Scrapshift preserves an initial ordinary spawn overlap and rejects delayed spawn overlaps before activation; zero/one-frame delayed contacts resolve at the first after-movement flush, two-frame contacts at the second. Event-flush ordering across engines remains a limit; this is not a claim of identical callback phase.

### Random callback targets exclude neutral trees

Reference `global/entity_spawner.gd:399-415` keeps ordinary enemies, bosses and neutrals in distinct arrays; `get_rand_enemy` at lines 485 onward samples only the ordinary `enemies` array. Scrapshift's `RandomStarterTarget` already excluded bosses/elites but still included trees from its combined list. The helper now excludes Tree as well. This applies to lightning callbacks and the root integration's ordinary projectile bounce selection.

The parent owns the separate bounce caller correction: reuse this helper and aim toward the selected unit's raw origin. This report does not attribute an unreviewed caller edit to the audit agent.

### Native evidence requires discrete contacts

The parent coordinated `artifacts/gameplay-parity-contact-native-2026-10-17/`: actual source projectile scenes and the Baby Alien Hurtbox ran in Godot 3.6.2 at 60 Hz with a custom receiver. This is native collision evidence, not production damage or human playtesting. Source file hashes were checked separately.

The reachable 6000 px/s flame speed cap moves 100 pixels per tick. Its radius-44 hitbox at offset `(24,4)` passes a radius-32 Baby Hurtbox centered at `(174,174)`, while the horizontal centerline is y=104. Neither discrete x=124 nor x=224 position overlaps, although a swept segment would. Native Godot reported no contact. Scrapshift now tests nonbeam circles at their current physics endpoint, with an additional initial enabled spawn snapshot; it no longer invents swept contacts or travel-distance ordering for simultaneous overlaps. The parent owns the real 60 Hz combat accumulator and incoming/slow/support endpoint integration.

Melee similarly now tests the current source-shaped pose once per physics tick rather than sampling at 240 Hz between ticks. The source beam Hitbox is a child of animated `Contents` in `particle_accelerator_projectile.tscn`; production collision now samples its current cubic scale rather than the maximum thickness reached anywhere between samples. The cubic interpolation itself remains unchanged. Simultaneous contact ordering remains an engine-specific limit.

Source `weapons/shooting_behaviors/melee_weapon_shooting_behavior.gd:63,105` plays its sound after the recoil tween completes, before enabling the hitbox, in both sweep and thrust coroutines. That event is independent of a physics overlap. Scrapshift now triggers the existing approved cue once when recoil time is crossed, even if a short attack has no active sampled pose. The finished-attack regression case checks its event flag as well as zero damage. No audio assets or processing were changed, and this assertion is not a listening test.

## Source checks retained without new behavior

- `projectiles/explosion.tscn` disables its hitbox at 0.05 seconds; the common explosion queue already expires at that active-window boundary and retains per-explosion ignored targets and cumulative kills. A new test checks that a later entrant receives no hit after removal.
- `projectiles/player_explosion.gd` forwards hit/kill callbacks and tracks Fireworks across the explosion, rather than resetting per update. Earlier integration cases already exercise later entry, one hit per victim and kills split across updates.
- `projectiles/bullet_taser/delayed_taser_projectile.gd` enables its separate SlowHitbox immediately in `shoot`, before delegating to delayed damage-hitbox setup. Slow application before the damage delay is intentional and remains unchanged.
- `projectiles/player_projectile.gd` checks whether range time is already exhausted before decrementing its timer. Native frame-order differences around expiry remain an explicit comparison requirement; this audit does not invent a new timer rule from static source alone.
- Source `Unit.hurt_area_entered_deferred` checks active state, ignored objects and pending death. Existing same-batch player-contact handling follows the separately documented deferred hurtbox disable semantics.

## New focused acceptance checks

`Assets/Scrapshift/Tests/Editor/CombatTimingParityTests.cs` adds **15 cases**:

- Five projectile-contact boundary cases: passed disabled positions, enabled current position, still-disabled endpoint, no sweep across past positions, and pending activation.
- One initial ordinary spawn overlap case and one actual flame-cap grazing case reproducing the native no-contact geometry.
- One melee case prevents replaying an already finished attack through intermediate poses.
- One beam case rejects an intermediate thickness peak that does not overlap at the current physics sample.
- One beam interval case prevents using a later, thicker animation state after its remaining active lifetime; enable clipping and beam geometry use the same shortened interval.
- One explosion expiry/later-entry case.
- One random target eligibility case containing ordinary enemies, trees, bosses and elites.
- Three real `UpdateWeapons` cases use source delayed-lightning geometry and forced zero-, one- and two-frame countdowns, including initial pending rejection and the resulting after-movement contact flush. The obsolete swept-subinterval math helper and its five arithmetic-only cases were removed.

The tests are authored and pending parent execution. Countdown checks compare the observed event flush rather than asserting that the two engines share an identical callback phase. They supplement the earlier selected results; they are not a new passing total.

## Remaining native evidence

The native source fixture now covers selected moving/delayed contact cases and established the discrete flame grazing mismatch. Combined Unity focused validation is still pending. Compare actual review gameplay timing, simultaneous overlaps, melee/beam contacts, bounce contact positions and range expiry before asserting those interactions match. Godot animation/deferred-event order and crowd physics remain limits, even when independent rule fixtures pass.

## Attribution

The user supplied gameplay requirements and made the original available in Godot. Codex inspected the source and authored the two owned fixes, tests and this report. No human playtest, physical controller verification or independent user debugging claim is made.
