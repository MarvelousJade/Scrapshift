using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed class WeaponSet
    {
        public readonly string Id, Name, Stat;
        readonly int increment;
        public SetEntry Data;
        public WeaponSet(string id, string name, string stat, int increment)
        { Id=id; Name=name; Stat=stat; this.increment=increment; }
        // Reference resources contain total effects for counts 2..6, not cumulative tiers.
        public int Bonus(int count) => Mathf.Clamp(count-1,0,5)*increment;
        public float Bonus(int count,string stat)
        {
            if(count<2)return 0;
            if(Data==null)return stat==Stat?Bonus(count):0;
            int threshold=Mathf.Clamp(count,2,6);
            foreach(var bonus in Data.bonuses)
            {
                if(bonus.count!=threshold)continue;
                // Enumerable.Sum(float) uses a double accumulator. Preserve that rounding.
                double total=0;
                foreach(var effect in bonus.effects)if(effect.key==stat)total+=effect.value;
                return (float)total;
            }
            throw new System.InvalidOperationException("Missing weapon-set threshold: "+threshold);
        }
        public string Description(int count) => Data==null?ItemEntry.FormatStat(Stat,Bonus(count)):
            string.Join(", ",Data.bonuses.First(b=>b.count==Mathf.Clamp(count,2,6)).effects.Select(e=>ItemEntry.FormatStat(e.key,e.value)));
    }

    public static class WeaponSets
    {
        public static readonly WeaponSet Gun=new WeaponSet("gun","Gun","range",10);
        public static readonly WeaponSet Precise=new WeaponSet("precise","Precise","crit_chance",3);
        public static readonly WeaponSet Primitive=new WeaponSet("primitive","Primitive","max_hp",3);
        public static IReadOnlyList<WeaponSet> All {get;private set;}=new[]{Gun,Precise,Primitive};
        public static void Configure(SetEntry[] entries)
        {
            if(entries==null)return;
            All=entries.Select(e=>{var s=e.id=="gun"?Gun:e.id=="precise"?Precise:e.id=="primitive"?Primitive:new WeaponSet(e.id,e.id=="fire"?"Elemental":e.id=="unarmed"?"Unarmed":e.name,e.bonuses[0].effects[0].key,0);s.Data=e;return s;}).ToArray();
        }
        public static WeaponSet For(Equipment weapon)
        {
            if(weapon?.setIds!=null&&weapon.setIds.Length>0)
            {
                string id=weapon.setIds[0];
                for(int i=0;i<All.Count;i++)if(All[i].Id==id)return All[i];
                return null;
            }
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
            for(int i=0;i<inventory.Count;i++)if(inventory[i].HasSet(set.Id)||For(inventory[i])==set)count++;
            return count;
        }
        public static float Bonus(IReadOnlyList<Equipment> inventory,string stat)
        {
            float result=0;
            for(int i=0;i<All.Count;i++)
            {
                var set=All[i];result+=set.Bonus(Count(inventory,set),stat);
            }
            return result;
        }
    }
}
