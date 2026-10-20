# Item explosion chain follow-up — October 20, 2026

Branch: `codex/gameplay-parity`. Source inspection and opt-in headless checks only; no desktop interaction. The supplied original is read-only.

## Confirmed differences fixed

**Plain item explosions inherited weapon burning in R34.** Original `singletons/run_data.gd:handle_explode_effect` forwards the first imported effect's `stats.burning_data`. It initializes burning only for `ItemExplodingAndBurnEffect`. Rip & Tear and Spicy Sauce import `ItemExplodingEffect`, whose WeaponStats starts with empty burning. `WeaponService.explode`, `PlayerExplosion.set_damage` and `Hitbox.set_damage` forward that data without adding Scared Sausage. Scrapshift's prior `ResolveImplantAttacks` sent no explicit burn payload, so the common damage path recomputed global weapon burning. Four legal Sausages guaranteed burning; Greek Fire could then add current-HP damage to a hit that should not burn.

**Plain item explosions could also steal life in R34.** Their original explosion hitbox belongs to `PlayerExplosion`, whose hit callback only emits its signal. Original life steal belongs to `Weapon._on_Hitbox_hit_something` and `PlayerProjectile._on_Hitbox_hit_something`; the item emission path reaches neither. Weapon-created explosions can connect `Weapon.on_weapon_hit_something` for tracking, but that separate method does not manage life steal either. Scrapshift's prior item explosion reached the ordinary weapon life-steal callback. This could heal the player, consume the weapon life-steal cooldown and activate healing-dependent effects such as Lich.

These findings concern ordinary imported Rip & Tear and Spicy Sauce. They do not authorize removing global burning from Beard projectiles: original `init_ranged_stats(..., true)` initializes those projectiles' burning data.

## Finite acceptance scope

- `ItemChainFollowupTests`: four plain explosion/burning checks at legal maximum copies, two life-steal checks and one Beard projectile control. Each fixture stages imported earnable definitions in a post-unlock loadout, enforces stack caps, and calls the actual ArenaGame dispatch method synchronously. The imported `available` flag describes default unlock eligibility, so it is not required for an earned reward. No Update scheduling, overlap physics, controller or rendered acceptance is implied.
- `scripts/item_chain_reference.gd`: 70 legal source payload loadouts: Rip & Tear copies 1–5 or Spicy Sauce copies 1–2, Sausages 0–4, Greek Fire absent/present. It executes the original source-extracted `handle_explode_effect` function, replacing only its two deferred scene-emission calls with a payload recorder. Imported effect resources, probability calculation, damage calculation and argument assignment remain original. It records the full source hash. This verifies emitted payloads; it does not instantiate an explosion scene or compare its collision timing.

The root-run source probe Passed with all 70 payloads non-burning, exit 0 (`artifacts/death-item-followup-2026-10-20/chain-source-attempt-1`). The production baseline then reproduced all six failures in the actual synchronous ArenaGame dispatch; the Beard control passed (`item-baseline-3.xml`, 7 cases, 1 Passed / 6 Failed). Earlier attempts had a fixture error that incorrectly rejected locked but earnable items through their default-availability flag; those are not gameplay failures.

`ImplantAttacks.cs` now supplies an explicit empty burning payload and uses the existing explosion callback context to suppress weapon life steal. The context is restored in a `finally` block. It leaves Beard projectile initialization and normal critical/kill item handling intact. The final inspected focused result is **27/27 Passed**, zero failures/skips (`focused-postfix-2.xml`), including these nine cases. The integrated suite is **1,271/1,271 Passed**, zero failures/skips (`suite-attempt-2.xml`). The failing baseline is preserved separately.

Integration with deferred global enemy deaths also separates the explosion's own immediate kill counter from `Rules.Kills`. Source `PlayerExplosion._on_Hitbox_killed_something` increments immediately when its hit turns a living victim into a lethal one. The implementation counts those transitions directly, excluding candidates already at zero HP. Two boundary checks stage 14/15 living victims plus an existing zero-HP candidate, verify global death totals are still deferred, and check Fireworks eligibility against the immediate per-explosion count. This preserves the source callback contract while changing the global death lifecycle.

## Packaged headless fixture

`--headless-item-chain-review <fresh-output-folder>` selects `HeadlessItemChainReview`. It waits for actual `ArenaGame.Start`, verifies the installed production proc hook, then disables ordinary ArenaGame updates. Four actual spawned Baby Alien targets receive capped Rip & Tear/Spicy Sauce explosions with four Sausages, with/without Greek Fire. The report records enemy HP, burning, missing player HP and the remaining life-steal callback opportunity. Two further scenarios spawn 14/15 living NPCs plus a zero-HP control and check Fireworks against fresh isolated JSON progression profiles before flushing global deaths. A Beard payload control retains burning and projectile life steal. Run with `-batchmode -nographics`; these synchronous callback receipts do not establish actual projectile travel, collision scheduling or a human combat run.

R35, source `debe1cf`, **Passed/exit 0** in `artifacts/death-item-followup-2026-10-20/chain-player/headless-item-chain.json` and `process.json`. Independent read-only inspection checked every explosion row against its matching original source row and inspected both saved profiles:

| Case | Inspected result |
| --- | --- |
| Rip & Tear ×5, with/without Greek Fire | Enemy HP 950, zero burn ticks, player HP 12 unchanged, life-steal cooldown untouched. Original source payload damage is 50. |
| Spicy Sauce ×2, with/without Greek Fire | Enemy HP 958, zero burn ticks, player HP 18 unchanged, life-steal cooldown untouched. Original source payload damage is 42. |
| 14 direct lethal hits + zero-HP control | 14 pending deaths, global kills still 0, Fireworks incomplete; actual JSON profile has no completed challenges. |
| 15 direct lethal hits + zero-HP control | 15 pending deaths, global kills still 0, Fireworks complete; actual JSON profile contains `chal_fireworks`. |
| Beard control | Both burning and projectile life-steal guards true. |

The preserved proc-context review also Passed/exit 0 (`proc-player`): 192 candidate rows, forward vulnerability health 89 and reverse-expiry health 88. No anomalies were found in the reviewed rows or profiles. The 70-row original payload grid and four packaged explosion rows remain distinct coverage; this does not claim 70 packaged world scenarios.

## Remaining limits and attribution

This finite grid does not establish arbitrary item combinations, explosion/corpse overlap timing, all deferred death preemption or multiplayer ownership. Those are separate from the payload and callback contracts above. The user supplied direction and the reference; Codex agents performed the source investigation and authored the fixture. This is not a claim that the user independently discovered or debugged the issue.
