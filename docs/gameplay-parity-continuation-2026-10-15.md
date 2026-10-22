## October20 correction to historical structure claims

The statement below that Wrench/Screwdriver/Pruner effects never register is superseded. Source `RunData.add_weapon` applies `StructureEffect`, so those weapons count as registered structures before actors spawn. Broad character links, direct structure challenges and live actors still have different counting contracts. See [corrected caller chain and verified tests](support-registration-parity-2026-10-20.md). Historical test totals do not validate the old assumption.

# Gameplay parity continuation after R6 — October 15, 2026

Branch: `codex/gameplay-parity`. The user confirmed R6's reported movement sound is gone, then requested further parallel agent fixes and continued bug/interview documentation. The accepted audio correction is retained.

## Source-backed corrections

| Area | Difference corrected | Detailed evidence |
| --- | --- | --- |
| Economy | Preserve fractional effective Harvesting through growth; truncate combined income at the source credit boundary. | [Economy report](economy-remaining-parity-2026-10-15.md) |
| Unlocks | Structure achievements count registered structure effects, excluding weapon actors, Pocket Factory and structure pets. | [Economy report](economy-remaining-parity-2026-10-15.md) |
| Shop pools | Character bans filter the preferred pool; backup fallback retains the source's separate eligibility rules. | [Economy report](economy-remaining-parity-2026-10-15.md) |
| Combat | Burning without a weapon hitbox cannot crit, heal through life steal, or gain global weapon knockback. Source burning also clears an existing knockback vector. | [Combat report](combat-remaining-parity-2026-10-15.md) |
| Melee | Automatic melee tracks its existing moving target during the swing; manual melee retains its frozen direction. | [Combat report](combat-remaining-parity-2026-10-15.md) |
| Support zones | Doc Moth uses player-body overlap rather than player-origin distance. Existing boolean life-steal doubling was correct and is preserved. | [Support report](support-remaining-parity-2026-10-15.md) |
| Mines | Bot O' Mine clamps the whole sampling square inside the map margin before sampling; interior placement already matched. | [Support report](support-remaining-parity-2026-10-15.md) |
| Enemies | Enemy healer's supplied collision mask excludes the player, despite an unused Player branch in its script. | [Enemy report](enemy-remaining-parity-2026-10-15.md) |
| Boss summons | Source half-away rounding replaces Unity half-to-even rounding for summon counts. | [Enemy report](enemy-remaining-parity-2026-10-15.md) |

Two additional corrections are integrated: Lootworm retains its selected object or idle sentinel until the source spawn/removal events refresh it, and automatic weapon aiming retains one captured accuracy spread throughout the attack. Production material-spawn integration tests cover empty batches, merge-only batches, multiple Lootworms and fruit exclusion.

## Verification and remaining scope

Fresh catalog comparison passed **4,980 checked fields**, with zero differences across 50 characters, 62 weapon identities / 205 tiers and 209 registered items. `artifacts/catalog-parity.json` records source hashes and exact scope. This verifies the checked resources, not special-effect execution or equal RNG sequences.

The first integrated run executed 594 tests: 592 passed and two new synthetic economy fixtures failed with null effect keys. Those fixtures omitted the empty-string key present in the real serialized data; initializing the fixture fields corrected the setup without changing runtime rules. `artifacts/gameplay-parity-continuation-2026-10-15/initial-tests.xml` and `.log` preserve this failure.

The inspected full rerun passed **594/594**, zero failed/skipped, in **72.8708967 seconds** (`tests.xml`, `tests.log` in that artifact directory). It includes 45 new cases: combat 5, economy 11, enemy 7, support 18 and production Lootworm material-spawn lifecycle 4.

The unpacked R7 build at `Builds/GameplayParity20261015R7` succeeded with zero errors/warnings and 401,554,375 reported bytes. `build-manifest.json` records executable, runtime and asset-payload hashes. Its isolated native `--pet-review` fixture exited zero and passed **27/27** checks covering nine pets, targeting, moth stacking, Lootworm tree priority, ten structure cases and player-triggered mine exit detonation. `native-support/pets.json`, `player.log` and 37 captures preserve the run. Doc Moth and Lootworm captures were visually inspected; this does not establish every frame or a human survival run.

All 272 processed audio WAVs in the validation clone match the accepted source bank byte for byte. `audio-preservation.json` and the 480-entry `source-manifest.json` record synchronized inputs. This continuation does not rebake audio.

The earlier R5 full suite, R6 focused audio checks and normal-time scripted full-run pair remain separate historical evidence. No new normal human full run, complete controller acceptance or blanket exact gameplay parity is claimed. Engine collision ordering and exhaustive modifier combinations remain separate comparison work.

## Following R7: beam, enemy-zone and character pass

These additional changes are validated separately in R8; the R7 checkpoint does not cover them:

- Animated beam collision: replace the travel capsule with the supplied oriented rectangle, its scene offsets/scaling and cubic growth/shrink curve. See [beam report](beam-remaining-parity-2026-10-15.md).
- Enemy healer zone: compare the source area with actual body centers/radii, exclude neutral trees via its collision mask, retain full-health entry occupancy without a false healing flash. Preserve the earlier player exclusion. See [zone report](enemy-zone-parity-2026-10-15.md).
- Streamer/Broadcaster Armor: count registered structure effects, Pocket Factory copies and structure-pet entries; exclude equipped structure-producing weapons and numeric multiplicity within one effect entry. This broader stat-link count remains distinct from the two structure achievements' narrower eligibility. See [character report](character-remaining-parity-2026-10-15.md).

Another investigation was rejected after inspecting the complete call path: `enemy.gd:get_damage_value` checks its `is_burning` argument, supplied by the damage event, not the enemy's `_is_burning` member. Greek Fire therefore remains limited to burn damage; extending it to every hit against a burning enemy would introduce a new difference. No such change was made.

### R8 verification

- Full integrated suite: **620/620 passed**, zero failed/skipped, **74.3181245 seconds**. The new pass adds 12 beam, eight enemy-zone and six character cases. `artifacts/gameplay-parity-geometry-2026-10-15/tests.xml` and `tests.log` were inspected.
- **488 source/settings/importer inputs** are synchronized and hash-verified in `source-manifest.json`. The 38 collision profiles gain body/origin/layer metadata; the source audit confirms every prior field value is unchanged.
- R8 Windows build: **Succeeded, zero errors/warnings, 401,561,639 reported bytes**, unpacked at `Builds/GameplayParity20261015R8`. Runtime SHA-256: `D0A63A27C53081D9D82DE2F04A90FEB8569389B8EC5AF7AAEFD7DAC5C91A15F8`. The frozen build manifest includes asset payload hashes.
- Native weapon fixture: **205/205 passed**, exit zero (`native-weapons/weapons.json`). Native enemy/state fixture: **53/53 passed**, exit zero (`native-enemies/report.json`). The actual staged Particle Accelerator capture (`projectile-19.png`) and healer capture (`healer-0.png`) were visually inspected. These are staged built-player fixtures, not human survival or full visual-frame equivalence.
- The full 272-clip processed audio bank remains byte-identical to the accepted R6/R7 source; `audio-preservation.json` records the hashes. No audio rebake occurred.
- The fresh R8 normal-timing campaign **passed/Won**, process exit zero, after **20 timer ends and 19 shops**, with 31 purchases, 18 upgrades and six crate takes. It used normal 60 Hz pickup physics and took 1,092.05 wall seconds including transitions. All 31 purchase debits and all recorded item-stack totals reconcile; the player log has no matched exceptions. Evidence is under `artifacts/gameplay-parity-geometry-2026-10-15/realtime`. [The R8 comparison report](full-run-r8-comparison-2026-10-15.md) compares against the frozen supplied-original trace, not a newly replayed original.
- Fresh full-run captures at waves 5, 15, 20 and the end were visually inspected. The end image displays a queued Drifter unlock, not a visible victory title; completion/Won is established by the trace/report. Scripted movement, supplied equipment and invulnerability keep this separate from ordinary human survival balance.
