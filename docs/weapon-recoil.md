# Weapon recoil and firing cadence

Compared the supplied Godot 3.6.2 reference weapons/weapon.gd, ranged_weapon_shooting_behavior.gd, weapon_service.gd and the four ranged families' stats resources.

The old Unity animation applied the same recoil and angular kick to every gun and counted down cooldown while the gun was still recoiling. Source recoil is a backward/forward exponential-out translation, retains the target during shooting, and holds cooldown until shooting ends. SMG: 10px/.05s per half; pistol and shotgun: 25px/.1s; laser: 40px/.2s, converted at 100px per world unit. Positive attack speed reduces both duration and distance; negative speed changes cooldown without stretching recoil.

WeaponMotion centralizes that contract. Runtime consumes any leftover frame time after shooting before reducing cooldown, preventing frame-boundary double counting. A dead target preserves the previous firing direction until shooting ends. No angular kick is added. Existing original weapon textures, attachment slots, scale and sorting are retained. Melee continues to use the documented simplified .18s attack and instant segment resolution; this is not full melee parity.

Regression checks cover four recoil profiles, start/peak/end, exponential return, cooldown overshoot and positive/negative attack-speed scaling. Final verification also uses scripted moving six-weapon combat. See ui-and-motion-comparison.md and verification.md for UI follow-through, observed evidence and limitations. Shaoyu directed the comparison; Codex implemented and tested it.
