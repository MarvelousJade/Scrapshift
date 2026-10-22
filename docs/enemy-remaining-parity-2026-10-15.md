# Remaining enemy behavior corrections — October 15, 2026

Branch: `codex/gameplay-parity`. Supplied source at `E:/UnityProjects/brotato-(3.x)` is read-only.

| Finding | Source evidence | Correction |
| --- | --- | --- |
| Enemy healer also healed the player in proximity | `entities/units/enemies/healer/healer.tscn` BoostZone has collision mask 4; `enemies/enemy.tscn` bodies use layer 4, while `player/player.tscn` uses layer 2. Although `healer.gd` contains a Player callback branch, that branch cannot be reached through the supplied zone. | Remove unconditional player proximity healing from `ExtendedEncounters.HealNearbyEnemies`; retain enemy entry healing and exit/reentry eligibility. |
| Boss summon counts used Unity's half-to-even rounding | `entities/units/enemies/attack_behaviors/spawning_attack_behavior.gd:shoot` uses `max(1, round(nb_to_spawn * (1 + number_of_enemies/100)))`. Source half rounding differs at 2.5 and 4.5. | Use existing source half-away rounding through `EnemyReferenceRules.SummonCount`. For five summons with -50% enemies, spawn three rather than two. |

`EnemyRemainingParityTests` adds real healer-method regressions for player exclusion and enemy entry/reentry, and five summon boundary cases. Execution results will be recorded after integration; test source alone is not passed evidence.

Remaining limits: exact zone/body collision geometry, engine collision ordering, normal human combat balance and exhaustive elite/boss timing combinations require further comparison. These fixes do not establish complete enemy parity.

## R7 integrated validation - completed

Full suite passed 594/594, zero failed/skipped, 72.8708967 seconds. Native built-player support fixture passed 27/27 and exited zero. R7 build succeeded with zero errors/warnings. Evidence: artifacts/gameplay-parity-continuation-2026-10-15/tests.xml, build-manifest.json and native-support/pets.json. These are automated/native fixture results, separate from human full-run playtesting. Further beam/zone geometry comparisons continue separately after the R7 checkpoint.

