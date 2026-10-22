using System;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class PetSpec
    {
        public string kind;
        public float speed,health,healthGrowth,doubleChance,revive=2,zone=2,explosionRadius,mineInterval,explosionSoundDb=-10;
        public int explosionSmokeAmount;
        public bool structure;
        public ProjectileStats stats,secondary;
        public StructureSpec mine;
        public StructureBurn burn;
        public string Description
        {
            get
            {
                string damage=stats==null?"":$"{stats.damage:0.##}"+string.Concat((stats.scaling??Array.Empty<StatEffect>()).Select(s=>$" + {s.value*100:0.##}% {ItemEntry.StatName(s.key)}"));
                switch(kind)
                {
                    case "lootworm":return "Deploy a salvage drone that collects materials, has a 10% chance to double them, and destroys trees";
                    case "doc_moth":return "Deploy a repair drone. Nearby, HP Regeneration and Life Steal healing are doubled";
                    case "scapegoat":return "Deploy a decoy with 40 HP. Takes 1 damage per hit; stand nearby for 2 seconds to revive it";
                    case "bonk_dog":return "Deploy an impact drone: "+damage+" damage. Leaps at the strongest enemy every 5 seconds for 8 + 40% Melee Damage";
                    case "catling_gun":return "Deploy a twin-gun drone: "+damage+" damage. Fires twice as fast near you or another companion";
                    case "bot_o_mine":return "Deploy an engineering drone: "+damage+" damage. Places a mine every 5 seconds for 10 + 100% Engineering damage";
                    case "blazemander":return "Deploy a thermal drone: "+damage+" damage. Burns for 3 + 50% Elemental Damage, 5 times, and releases 6 projectiles";
                    case "ratzilla":return "Deploy an assault drone: "+damage+" damage";
                    default:return "Deploy a projectile-blocking orbital shield";
                }
            }
        }
    }
    public sealed partial class RunRules
    {
        public int HealingDrones;
        public Equipment PetWeapon(ProjectileStats stats,PetSpec pet)
        {
            var weapon=JsonUtility.FromJson<Equipment>(JsonUtility.ToJson(stats.AsEquipment()));weapon.pet=true;weapon.specialSpawn=false;
            weapon.melee=ReferenceEquals(stats,pet.stats)&&(pet.kind=="ratzilla"||pet.kind=="bonk_dog"||pet.kind=="blazemander"||pet.kind=="lootworm");
            weapon.projectileScene=SupportProjectileBehavior.PetScene(pet.kind,weapon.melee);
            if(Stat("beast_master_effect")>0&&weapon.scaling!=null&&weapon.scaling.Length>0)
            {
                float scale=weapon.scaling[0].value;
                weapon.scaling=weapon.scaling.Concat(new[]{"melee_damage","ranged_damage","elemental_damage","engineering"}.Where(k=>!weapon.scaling.Any(s=>s.key==k)).Select(k=>new StatEffect{key=k,value=scale})).ToArray();
            }
            if(pet.burn!=null&&pet.burn.burnTicks>0)weapon.traits=new[]{new WeaponTrait{key="effect_burning",burnDamage=pet.burn.burnDamage,burnTicks=pet.burn.burnTicks,burnChance=pet.burn.burnChance,burnScale=pet.burn.burnScale,burnStat=pet.burn.burnStat}};
            return weapon;
        }
        public float PetRange(Equipment weapon,bool structure=false)=>Mathf.Max(25,Mathf.Floor(weapon.range*100+Stat(structure?"structure_range":"range")))/100;
        public float PetCooldown(float seconds)=>Mathf.Max(MinimumWeaponCooldown,Mathf.Max(2,Mathf.Floor(seconds*60+.0001f))/60);
    }
}
