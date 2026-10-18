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
            // This runs for every queried stat. Preserve First/Sum semantics
            // without constructing captured predicates and LINQ iterators.
            if(Data.bonuses==null)throw new System.ArgumentNullException("source");
            int target=Mathf.Clamp(count,2,6);SetBonusEntry row=null;
            for(int i=0;i<Data.bonuses.Length;i++)if(Data.bonuses[i].count==target){row=Data.bonuses[i];break;}
            if(row==null)throw new System.InvalidOperationException("Sequence contains no matching element");
            if(row.effects==null)throw new System.ArgumentNullException("source");
            double total=0;
            for(int i=0;i<row.effects.Length;i++)if(row.effects[i].key==stat)total+=row.effects[i].value;
            return (float)total;
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
            for(int i=0;i<inventory.Count;i++)
            {
                var weapon=inventory[i];
                // An explicit first membership can resolve to this set only
                // when HasSet already matched its Id. Legacy fallback is needed
                // solely for weapons without authored membership IDs.
                if(weapon.HasSet(set.Id)||((weapon.setIds==null||weapon.setIds.Length==0)&&For(weapon)==set))count++;
            }
            return count;
        }
        public static float Bonus(IReadOnlyList<Equipment> inventory,string stat)
        {
            float result=0;
            for(int i=0;i<All.Count;i++){var set=All[i];result+=set.Bonus(Count(inventory,set),stat);}
            return result;
        }
    }
}
