using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed class WeaponSet
    {
        public readonly string Id, Name, Stat;
        readonly int increment;
        public WeaponSet(string id, string name, string stat, int increment)
        { Id=id; Name=name; Stat=stat; this.increment=increment; }
        // Reference resources contain total effects for counts 2..6, not cumulative tiers.
        public int Bonus(int count) => Mathf.Clamp(count-1,0,5)*increment;
        public string Description(int count) => ItemEntry.FormatStat(Stat,Bonus(count));
    }

    public static class WeaponSets
    {
        public static readonly WeaponSet Gun=new WeaponSet("gun","Gun","range",10);
        public static readonly WeaponSet Precise=new WeaponSet("precise","Precise","crit_chance",3);
        public static readonly WeaponSet Primitive=new WeaponSet("primitive","Primitive","max_hp",3);
        public static readonly IReadOnlyList<WeaponSet> All=new[]{Gun,Precise,Primitive};
        public static WeaponSet For(Equipment weapon)
        {
            switch(weapon?.id)
            {
                case "pistol": case "smg": case "double_barrel_shotgun": case "laser_gun": return Gun;
                case "knife": return Precise;
                case "spear": return Primitive;
                default: return null;
            }
        }
        public static int Count(IReadOnlyList<Equipment> inventory,WeaponSet set)
        {
            int count=0;
            for(int i=0;i<inventory.Count;i++)if(For(inventory[i])==set)count++;
            return count;
        }
        public static int Bonus(IReadOnlyList<Equipment> inventory,string stat)
        {
            int result=0;
            foreach(var set in All)if(set.Stat==stat)result+=set.Bonus(Count(inventory,set));
            return result;
        }
    }
}
