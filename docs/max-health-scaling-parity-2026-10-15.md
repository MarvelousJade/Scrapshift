# Max HP damage scaling and boost audit — 2026-10-15

Supplied source: read-only `E:/UnityProjects/brotato-(3.x)`.

## Damage uses uncapped stats

`singletons/weapon_service.gd:469-479` sums scaling values with `Utils.get_stat`, except levels which use the run level. `Utils.get_stat:461-467` combines permanent, temporary and linked stats without the separate cap in `get_capped_stat:469+`. Consequently HP-based weapon/projectile damage uses the uncapped fractional Max HP stat, even when Handcuffs holds usable maximum health lower.

Corrected the two capped special cases in `CharacterProfile.WeaponDamage` and `ImplantAttacks.ItemExplosionDamage`. Chopper tier I supplies base 6 and 15% HP scaling (`weapons/melee/chopper/1/chopper_stats.tres`). Alien Eyes supplies base 1 and 50% HP scaling (`items/all/alien_eyes/alien_eyes_stats.tres`). Damage still truncates the complete scaled base before percentage damage and final rounding.

Nail/Frozen Heart's `AddedWeaponScaling` already uses uncapped `Stat`, so it was retained. Source `init_base_stats:207` excludes ordinary special-spawn projectiles from these added scaling effects. Alien Eyes explicitly calls `init_ranged_stats(..., true)` (`player.gd:864`), so it remains excluded. `get_explosion_damage:281-286` explicitly applies the added weapon scaling; item explosions continue to include both Nail's 20% Engineering and Frozen Heart's 10% Elemental contribution. No speculative generalized change to special spawns was made. Item agent separately owns the Lich trigger calculation.

## Player boost is an explicit exception

`player.boost:1112-1125` adds temporary Max HP **and** explicitly adds that amount to current HP. The later dirty-stat refresh through `update_player_stats:349-368` adds the positive effective maximum delta again. `Entity.boost:107-113` only sets boost state/outline and does not consume that stat delta. Both HP contributions are therefore preserved; they are not ordinary healing and may exceed maximum HP. Scrapshift's previous clamp hid that overflow. `EnemyBoost` now retains both contributions without a positive clamp. On expiry it retains the explicit clamp against the uncapped HP stat (`player.boost_ended:1134-1136`), separate from ordinary Max HP reductions that do not clamp.

A search of RunRules partial health assignments found no additional explicit positive health addition after Max HP mutation beyond this boost path; Start/NextWave/BeginItemWave initialize HP separately and remain unchanged. This bounded patch does not expand the existing boost expiry bookkeeping to every possible simultaneous stat mutation.

## Verification

Later scene-mask audit: the supplied normal and corrupted Buffer scenes mask enemy body layer 4 only, so their Player/Structure script branches are unreachable with those scenes. The two direct player-boost test cases validate the generic source-defined helper and are synthetic; they do not establish normal Buffer-to-player gameplay. The subsequent enemy movement/buffer audit removes that incorrect live candidate path.

Six authored cases in `MaxHealthScalingParityTests` exercise actual Chopper/Handcuffs, fractional HP scaling, Alien Eyes exclusion, item-explosion inclusion and full/wounded boost overflow/expiry. No Unity or native run was launched by this subtask. Integration validation and exact asynchronous frame-order checks remain separate evidence.

## Root integration result

After integration and fixture corrections, the focused selection passed **213/213**, zero failed/skipped. It includes this track's cases and directly affected older regressions. The actual XML and 496-input source manifest are in `artifacts/gameplay-parity-follow-up-2026-10-15`. Earlier statements about unexecuted agent-authored checks or a pending rerun describe the preceding stage. The whole suite, new player build and full campaign remain deferred; these focused results do not establish exact parity.
