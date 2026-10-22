using System;
namespace Scrapshift
{
    [Serializable] public sealed class StructureSpec
    {
        public string kind;
        public ProjectileStats stats;
        public float spawnInterval=-1,animationSpeed=1,accuracy=1,explosionRadius,explosionSoundDb=-10;
        public int explosionSmokeAmount;
        public bool healing,delayed,pet;
        public StructureBurn burn;
        [NonSerialized] Equipment equipment;
        public Equipment Equipment
        {
            get
            {
                if(equipment!=null)return equipment;
                equipment=stats.AsEquipment();
                if(burn!=null&&burn.burnTicks>0)equipment.traits=new[]{new WeaponTrait{key="effect_burning",burnDamage=burn.burnDamage,burnChance=burn.burnChance,burnTicks=burn.burnTicks,burnScale=burn.burnScale,burnStat=burn.burnStat}};
                return equipment;
            }
        }
        public string Description
        {
            get
            {
                if(kind=="garden")return $"A garden produces fruit about every {stats.interval:0.##} seconds";
                if(kind=="wandering_bot")return "An orbiting bot slows nearby enemies by 250 speed";
                string scaling=stats.scaling==null?"":string.Join(" + ",Array.ConvertAll(stats.scaling,s=>$"{s.value*100:0.##}% {ItemEntry.StatName(s.key)}"));
                string damage=$"{stats.damage:0.##}"+(scaling.Length>0?" + "+scaling:"");
                if(kind=="landmine")return $"Deploy a mine every {spawnInterval:0.##} seconds dealing {damage} damage";
                if(healing)return $"Deploy a repair turret restoring {damage} HP (base cooldown {stats.interval:0.##}s)";
                if(burn!=null&&burn.burnTicks>0)return $"Deploy a flame turret burning for {burn.burnDamage:0.##} + {burn.burnScale*100:0.##}% {ItemEntry.StatName(burn.burnStat)}, {burn.burnTicks:0} times";
                return $"Deploy a {kind} turret dealing {damage} damage (base cooldown {stats.interval:0.##}s)";
            }
        }
    }
    // Leaf data avoids a recursive Unity serialization graph through WeaponTrait.
    [Serializable] public sealed class StructureBurn
    {public string key,burnStat;public float burnDamage,burnChance,burnTicks,burnScale;}
}
