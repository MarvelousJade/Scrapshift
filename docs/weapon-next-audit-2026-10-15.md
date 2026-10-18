## October 17 root validation result

See `gameplay-parity-systems-2026-10-17.md`. Evidence: `artifacts/gameplay-parity-systems-2026-10-17/focused-summary.json`, both XML/log attempts, input manifest and catalog comparison. The same selected checks passed 434/434 on attempt 2, zero failures/skips, Unity exit zero. All 508 recorded input hashes still match the isolated clone after execution. No whole suite, new build, full campaign or physical-controller retest was run. R8 remains the last delivered gameplay build and predates these changes. Earlier pending statements below reflect the handoff before root execution.

# Weapon behavior follow-up audit — October 15, 2026

## Scope and evidence status

This pass inspected the supplied read-only Brotato source at `E:/UnityProjects/brotato-(3.x)` against the registered Scrapshift base single-player weapons, their native traits, and item interactions. It preserves the previously integrated burn application, Greek Fire, Ice Cube, probability, and damage-order fixes. Approved art and audio are unchanged.

The preceding focused selection passed **213/213** cases before this pass. The new `WeaponNextParityTests.cs` contains **39 source-derived cases**. On October 17 the parent executed attempt 1: 33 passed, six failed; the corrections and pending rerun are recorded below. The parent agent owns integration and validation. No new full suite, player build, campaign, physical-controller test, or listening claim is made here.

## Confirmed differences corrected

### Physical Ghost weapons own separate counters

Reference `weapons/weapon.gd:57,211-232` initializes `_enemies_killed_this_wave_count` on each weapon node and increments it from that node's kill signal. Each gain occurs at an exact integer multiple of the serialized effect threshold. New weapon nodes begin again on the next wave.

Scrapshift keyed its counter by the shared catalog `Equipment` object, so two identical weapons combined kills; the dictionary also survived wave resets. It now keys counters by the physical inventory slot carried through normal melee and projectile damage, retains the cumulative count for modulo checks, and clears counters with the other wave actors. The debug wrapper has a catalog-index fallback; ordinary runtime damage supplies the slot explicitly. Tests exercise all three real Ghost families with two identical copies and a wave reset.

### Critical weapon income is immediate currency

Reference `entities/units/unit/unit.gd:367-382` resolves Hunting Trophy, Tentacle, and then the killing hitbox's `gold_on_crit_kill`; it calls `RunData.add_gold` before deferred death loot. The weapon roll is explicitly inclusive (`randf() <= value / 100.0`). The five registered assignments are four Thief Dagger tiers and tier-four Drill.

The former implementation spawned a material pickup, incorrectly delaying income and exposing it to pickup XP, saved-material bonuses, doubling, and pickup-triggered items. `GainWeaponCriticalMaterials` now adds currency directly. Separate `ApplyStarterKillStatistics` and `ApplyStarterCriticalGold` hooks let the parent integration preserve Ghost gain → critical item income → Tentacle healing → weapon income → hit callbacks → deferred loot. The Drill test verifies unchanged XP, zero pickup creation, and an untouched saved-material bank; Dagger tests cover every tier's exact chance boundary.

### Explosive contacts and callback explosions

Reference `singletons/weapon_service.gd:185-248` detects an `ExplodingEffect` while initializing weapon stats and adds Explosion Damage to ordinary Damage **before one rounding**, even when the later explosion chance fails. The previous weapon damage calculation omitted that contribution and instead multiplied it again when an explosion spawned.

Reference `entities/units/unit/unit.gd:552-609` rolls the effect before burn and direct damage. A successful roll creates an explosion including the original target and skips the direct hit. The old runtime first dealt direct damage and then excluded that target from the explosion. The new interception queues an explosion and defers all affected targets to its hitbox. The scoped `resolvingWeaponExplosion` flag prevents repeated interception and weapon-only hitbox callbacks while retaining the copied critical chance, burning data, scaling identity, and global item effects.

Melee has a second, independently rolled callback: `weapons/weapon.tscn` connects its hitbox's `hit_something` signal to `weapon.gd:_on_Hitbox_hit_something` (lines 417 onward), which creates another explosion with `ignored_objects = [thing_hit]`. Plank, Power Fist, Plasma Sledgehammer, and Dextroyer inherit this connection. This source behavior is intentionally retained: the Unit explosion includes the original victim; the callback explosion excludes it. The test uses real Plank data and verifies both exclusion sets.

`unit.gd` creates on-hit projectiles before emitting `hit_something`; the weapon callback then attempts life steal before its melee explosion roll. The handler now follows that order instead of serialized trait order. Explosion hitboxes emit tracking callbacks, with no weapon life-steal callback; the rocket test checks that nearby victims do not grant extra healing or recursively create explosions. Positions use the source unit origin, distinct from the center of Scrapshift's artwork.

The support agent found that Bonk, structure rockets, and mines still used one immediate overlap scan. They now share the source explosion's **0.05-second active hitbox window**, with independent per-explosion hit sets; a victim entering during that window can be hit once. `PlayerExplosion.nb_killed` accumulates across the entire active explosion before testing the 15-kill Fireworks requirement, so the shared queue preserves that lifetime count. Two integration tests cover later entry/one hit and kills split across frames. Support visuals and sounds stay with their existing callers. The source explosion center's `(0,3)` pixel offset is scaled with its area; the support agent's shared overlap helper provides that geometry for ordinary weapon and support explosions.

### Lightning, Taser, execution, and fire spread

- `weapon_service.gd:339-360` and `global/entity_spawner.gd:485-496` choose a **random ordinary enemy**, excluding the original target, for Lightning Shiv/Dextroyer auto-targeted secondary lightning. The former closest-enemy selection also included bosses. A fixed-seed selection test observes both near and far ordinary targets and excludes the boss.
- Taser's `SlowInZoneEffect` is descriptive; `projectiles/bullet_taser/taser_projectile.gd` applies its actual `-200` pixel decaying speed through the separate `SlowHitbox`. The runtime already applied that zone, then added a second unbounded direct-target slow from the descriptive trait. That duplicate is removed. The existing 45% damping threshold, 70% early-return threshold, 90% per-application cap, and integer pixel assignment were checked against `Unit.add_decaying_speed`.
- Vorpal Sword's execution uses `Utils.get_chance_success` (`unit.gd:286-295`): a positive chance includes its exact boundary. The old strict comparison excluded that value. All three registered Vorpal tiers have boundary tests.
- `particles/burning/burning_particles.gd:38-45` spreads once to an eligible nonburning body, decrements the inherited spread, then sets the source spread to zero. That state behavior already matched. Its geometry did not: the source uses a radius-128 physics-body area, with collision mask 4, parented to Unit's `BurningParticles` at `(0,-24)` pixels. The runtime now includes the target physics-body radius and offset, uses the actual particle origin, and excludes tree layer 1. Tests cover body-edge overlap for Baby Alien, Bruiser, and Predator. Source physics entry order remains an engine-dependent ordering limit for simultaneous candidates.

### Initial timers and structure rounding

Reference `weapon.gd:init_stats`, `get_next_cooldown(true)`, and `reset_cooldown` randomize each initial timer, using a maximum basis of 180 frames, then cap the result at the ordinary cooldown. They do not apply a big reload at wave start. The former runtime zeroed every timer. `SpecialWeaponRules.InitialCooldown` implements that arithmetic; five boundary cases distinguish randomization, the three-second basis cap, and the final ordinary-timer clamp. The parent integrates timer assignment and landing-time decrement. Source `_physics_process` decrements a nonshooting weapon during landing; `main.gd` calls `player.land()` without waiting for it.

Reference `weapon_service.gd:init_base_stats` also truncates a structure's scaled base before rounding combined structure and explosion percentages. `StructureDamage` previously returned fractional raw products, and mine/rocket callers multiplied Explosion Damage separately. The helper now accepts an additional percentage and combines it before source rounding; the mine caller is updated and the support agent updates the rocket caller. The meaningful boundary is a tier-one Screwdriver mine with 7.5 Engineering: base `17.5 → 17`, structure +25% gives `21`; adding explosion +50% gives `30`. Ordinary Damage does not affect this nonpet structure.

Pet/structure projectiles now preserve the support agent's source-derived `PetRange` when initializing and binding their native projectile profile, instead of replacing it with ordinary weapon Range. This is a coordinated integration hook; support behavior and its own tests belong to the support agent.

Reference `init_melee_stats` and `init_ranged_stats` assign final range to an integer pixel stat. The parent corrected ordinary `WeaponRange` to perform that assignment after the melee half-Range modifier. Four cases show the meaningful boundaries: Knife +1 Range stays at 150 pixels, +3 reaches 151; Pistol +0.75 stays at 400, while -0.5 becomes 399.

## Coverage checked without inventing new behavior

The registered trait inventory covers native burning; ordinary/custom/melee explosions; Ghost gains; native/projectile slowing; Cactus/Sniper projectiles, Lightning Shiv/Dextroyer targeting, Thunder Sword delayed slowing; finite critical bounce/pierce charges; Stick copies across tiers; Railgun no-hit growth; consumable/XP/armor/harvesting/life-steal/speed/HP/damage modifiers; self-damage; and turret/garden/landmine deployment. Existing support-owned deployment mechanics were coordinated with that agent.

Source ranged special spawns deliberately omit the ordinary added weapon-scaling effects while retaining their own scaling and common ranged modifiers. Their spawning loop uses the effect's projectile count; it does not fire a second pellet loop from `nb_projectiles`. The serialized `auto_target_enemy` assignments match the current key mapping: Lightning Shiv and Dextroyer true, Cactus Mace/Sniper Gun/Thunder Sword false. All registered weapon stat resources use `min_range = 0`; there is no basis to invent a minimum-range weapon rule.

Cooldown, recoil, ranged reload, critical bounce/pierce charge consumption, and Stick counting were traced through the current runtime and supplied source. Source evidence establishes these particular rules; it does not establish full numerical or experiential parity across all possible runs. These changes still need the parent's focused integration validation and later ordinary gameplay comparison after the remaining audit tracks finish.

## Fire-time burning data

The final source trace found a fire-time burn-data discrepancy requiring shared projectile threading. `WeaponService.spawn_projectile`/`HitboxArgs.set_from_weapon_stats` and `PlayerProjectile.shoot_ex` retain the already initialized burning data when firing; an intercepted explosion copies that hitbox data. Scrapshift recomputed `Rules.RollBurn` at impact. A real Wisdom temporary Damage increase while a Fireball or Sausage-capable projectile was in flight could therefore incorrectly change its burning damage. Direct projectile damage already captured its fire-time value.

The follow-up implementation adds `BurnPayload`, capturing chance, damage, duration, spread, and scaling-stat identity at firing. Native and support projectiles retain an explicit payload, and native/support explosions copy it instead of recalculating. An explicit empty payload is significant: it prevents an old shot from acquiring a later burn effect. The null compatibility path still captures a live contact. `Rules.RollBurn` delegates to the same capture formula, preserving its previously verified rounding and positive-inclusive chance behavior.

Melee deliberately remains live. `TempStats.add_stat` marks stats dirty; `RunData._emit_stats_updated` informs `global/stats_manager.gd`, which calls `weapon.init_stats(false)` and reassigns the shared melee hitbox's burning data. Freezing it at swing start would introduce a different mismatch. A melee callback explosion captures the contact's data once for its own lifetime. The source's deferred, budgeted stats-refresh queue remains a timing limit of the current immediate-stat architecture.

Five additional cases cover real Wisdom composition and temporal boundaries: with +10 Elemental Damage, Fireball burn changes `11 → 12` at the five-second Wisdom tick; Pistol with four Sausages changes `12 → 13`. Old shots retain the first values and spread count, new shots receive the second, and a rocket explosion inherits its fired payload. The empty-payload case is a preventive state-boundary test; the document does not claim that ordinary gameplay adds Sausages during combat.

## October 17 integration review

The parent has now threaded the explicit fire-time payload through ordinary shots, support shots, rocket explosions, both surviving and killing hit callbacks, and the shared explosion window. A null melee payload is captured once at contact, so the two independently rolled melee explosions retain the same contact data. Source integration was reviewed separately from the focused results below.

Focused attempt 1 subsequently executed all 39 weapon cases: **33 passed and six failed** within the parent's 434-case selection. Three burn failures were erroneous fixture arithmetic, not runtime failures: `effects/burning_data.gd:merge` adds each Sausage's base damage, so four copies supply `4 + 10 Elemental = 14` before Wisdom's percentage. `weapon_service.gd:apply_damage_bonus` rounds `14 × 0.85 = 11.9` to 12, then `14 × 0.90 = 12.6` to 13 after five seconds. The fixtures and explanation now use those independently derived values instead of 9/10.

Three Dagger boundary failures exposed inconsistent precision between the float test argument and the comparison expression. The reference uses `randf() <= effect.value / 100.0`; `CriticalMaterials` now takes a double roll and divides in double precision, with exact serialized percentage boundaries supplied by the tests. Actual Unity random samples still enter through the existing call site. No epsilon or widened success range is added. Attempt 1 evidence is preserved at `artifacts/gameplay-parity-systems-2026-10-17/focused-attempt-1.xml`; the same focused selection must be rerun before reporting success.

One additional script lead was checked and rejected as a base-catalog mismatch: `weapon.gd:on_killed_something` handles `gain_stat_when_attack_killed_enemies` for Dwarf, and `on_weapon_hit_something` refers to `chal_unstoppable_force`. Neither corresponding character/challenge resource is present in the supplied base reference tree or registered in Scrapshift's catalog/unlock definitions. These script hooks therefore do not justify adding a new base-game rule or an unsupported unlock. If those resources are supplied later, their attack-ID tracking will need a separate implementation and tests.

## Attribution

The user requested exact reference gameplay and concurrent audits. Codex inspected the supplied source, authored these runtime changes and regression cases, and coordinated shared hooks with the parent and support agents. The supplied game remains third-party reference material; no independent user authorship, audio listening, hardware test, or release-rights claim is implied.
