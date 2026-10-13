using System;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class RunRules
    {
        public int WeaponSlots=>Mathf.Clamp((Character?.weaponSlots??6)+(Stat("weapon_slot_upgrades")>0?Math.Min(Level,(int)Stat("weapon_slot_upgrades")-1):0),0,24);
        public int MinimumWeaponTier=>Character?.minTier??1;
        public int MaximumWeaponTier=>Character?.maxTier??4;
        public bool CanAttack=>Character==null||Character.canAttackMoving||!moving;
        // item_service.gd excludes lower tiers and max-tier duplicates for unique-weapon characters.
        public bool CanOfferWeapon(Equipment w)=>CanEquip(w)&&(Stat("no_duplicate_weapons")<=0||
            !Inventory.Any(owned=>owned.id==w.id&&(w.tier<owned.tier||w.tier==owned.tier&&catalog.Weapon(w.id,w.tier+1)==null)));
        public bool CanAddWeapon(Equipment w)=>CanEquip(w)&&
            (Stat("no_duplicate_weapons")<=0||!Inventory.Any(x=>x.id==w.id))&&
            (Stat(w.melee?"max_melee_weapons":"max_ranged_weapons")<=0||Inventory.Count(x=>x.melee==w.melee)<Stat(w.melee?"max_melee_weapons":"max_ranged_weapons"));
        float SetStat(string name)=>StatWithoutSets("all_weapons_count_for_sets")>0?
            WeaponSets.All.Sum(s=>s.Bonus(Inventory.Count,name)):WeaponSets.Bonus(Inventory,name);
        float StatWithoutSets(string name)=>PermanentStat(name);
        bool convertedHalfWave;
        double stationaryIncome;
        public int ExtraBaitGroups {get;private set;}
        public bool ExtraElite {get;private set;}
        int queuedBaits;
        public int OwnedStructures=>Effects("structure").Sum(e=>(int)e.value)+Items.Count(i=>i.id=="pocket_factory")+Inventory.Count(w=>w.Trait("turret")!=null||w.Trait("garden")!=null||w.Trait("landmine")!=null)+Effects("pet").Where(e=>e.pet!=null&&e.pet.structure).Sum(e=>(int)e.value);
        public bool UsesHealthShop=>Stat("hp_shop")>0;
        public float ShopBalance=>UsesHealthShop?MaxHealth:Scrap;
        public float WantedTagChance=>Stat("boosted_wanted_item_tag")>0?.1f:.05f;
        public float BurnCurrentHealthDamage(float health,bool boss)=>Mathf.Floor(health*Effects("burning_enemy_hp_percent_damage").Sum(e=>e.value)/(boss?1000:100));
        static readonly string[] primaryStats={"max_hp","armor","crit_chance","luck","attack_speed","elemental_damage","hp_regeneration","lifesteal","melee_damage","percent_damage","dodge","engineering","range","ranged_damage","speed","harvesting"};
        void BeginCharacterWave()
        {
            convertedHalfWave=false;stationaryIncome=0;
            ExtraBaitGroups=queuedBaits;queuedBaits=0;
            foreach(var e in Effects("gain_random_primary_stats_on_go_to_next_wave"))
                if(random.NextDouble()<e.per/100)for(int i=0;i<e.value;i++)Add(primaryStats[random.Next(primaryStats.Length)],1);
        }
        void TickCharacter(float dt)
        {
            if(!convertedHalfWave&&Remaining-dt<=Durations[Wave-1]/2f)
            {convertedHalfWave=true;ConvertStats("convert_stats_half_wave",false);}
            if(moving){stationaryIncome=0;return;}
            stationaryIncome+=dt;
            while(stationaryIncome>=1)
            {
                stationaryIncome--;
                foreach(var e in Effects("temp_stats_while_not_moving"))if(e.key=="percent_materials")
                    Scrap=Math.Max(0,Scrap+(int)Mathf.Min(e.per,Mathf.Max(1,Scrap*e.value/100)));
            }
        }
        void ConvertStats(string trigger,bool permanent)
        {
            foreach(var e in Effects(trigger))
            {
                string from=e.key.Replace("stat_",""),to=e.stat.Replace("stat_","");
                float available=from=="materials"?Scrap:LinkSource(e.key,true);
                int chunks=Mathf.Max(0,Mathf.FloorToInt(available*e.chance/e.value));
                float loss=chunks*e.value,gain=chunks*e.per;
                if(from=="materials")Scrap-=Mathf.RoundToInt(loss);
                else {float g=Character?.Gain(from)??1;if(g>0)loss=Mathf.Round(loss/g);if(permanent)Add(from,-loss);else AddTemporary(from,-loss);}
                if(to=="materials")Scrap+=Mathf.RoundToInt(gain);
                else {float g=Character?.Gain(to)??1;if(g>0)gain=Mathf.Round(gain/g);if(permanent)Add(to,gain);else AddTemporary(to,gain);}
            }
        }
        int PrepareNextWave()
        {
            int repeat=(int)Stat("item_hourglass");
            foreach(var item in Items.Where(i=>i.id=="hourglass").ToArray())
            {
                Items.Remove(item);foreach(var e in item.effects)Add(e.key.Replace("stat_",""),-e.value);
                ApplyItem(Array.Find(catalog.data.items,i=>i.id=="broken_hourglass"));
            }
            ExtraElite=random.NextDouble()<Stat("extra_elite_next_wave_chance")/100;
            return Math.Max(1,Wave+1-repeat);
        }
    }
}
