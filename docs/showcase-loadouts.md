# Wave 19 showcase loadouts and helper spawning

October 13, 2026. The showcase uses the existing item effects and weapon rules. No projectile-count stat or invented multishot item is added.

| Build | Added item effects | Visible result |
| --- | --- | --- |
| Cyberpunk Ranger | 2 Bandanas, 2 Ricochets, Pumpkin, Coffees, Small Magazines, Banner | Faster attacks, +2 pierce, +2 bounce, and less damage lost after piercing. Each shot retains its weapon's own projectile count. |
| Auto Turret Engineer | Extra Turret, Laser Turret, Rocket Turret, Tyler, three Clockwork Wasps, Improved Tools, Coffees, Diploma, Pile of Books, Bandana, Ricochet, Pumpkin, and three pets | More deployed helpers; faster turret and ranged-pet attacks, Engineering scaling, structure critical hits, pierce and bounce. |
| Druid Companion | Extra Blazemander, Bonk Dog, Catling Gun, Bot O Mine, Jellyshield, Scapegoat, three Clockwork Wasps, Diploma, Pile of Books | More companions, faster ranged-pet attacks, Engineering and structure critical-hit effects. |

`BeginLoadoutShowcase` grants the catalog items through `RunRules.GrantItem`, the same application path used by ordinary item acquisition. `ResetStructures` queues helpers only for loadout showcases. The first helper release occurs 1.05 seconds after wave start, then one structure and one pet are released every 0.05 seconds until the queues empty. `DeployStructure` and `SpawnPet` retain their ordinary random placement. Other game modes keep their existing spawn path.

The timing follows the supplied base-game `global/entity_spawner.gd`: its `StructureTimer` starts at the default one-second interval, then `_physics_process` releases one queued structure and one queued pet every `SPAWN_DELAY = 3` physics frames. The showcase uses a 60 Hz timing approximation. The original timer's phase and Unity's frame phase can differ.

The base-game source has Renegade's +2 projectile character effect, but no ordinary item that directly adds projectile count to every weapon. Seashell is an Abyssal Terrors DLC item that adds three projectiles to every fifth ranged-weapon projectile and does not generally affect turrets. It is outside this project's base-game item catalog and was not added after the user chose item-based attack speed, piercing, and bounce instead.

Verification: 363/363 EditMode tests passed, including loadout item effects and the first two helper release boundaries. The Windows build succeeded with zero errors and warnings. In hidden headless player runs, the Engineer started with 12 queued structures and 3 queued pets, then reported 11 structures and 3 pets deployed after 1.61 seconds; the remaining structure request is the landmine spawner, which produces mines instead of entering `starterTurrets`. The Druid started with 1 queued structure and 12 queued pets and reported all of them deployed after 1.61 seconds. Logs are in `artifacts/showcase-turret-player.log` and `artifacts/showcase-druid-player.log`. These checks prove the queue drains in built players; they do not prove visual placement or play feel.
