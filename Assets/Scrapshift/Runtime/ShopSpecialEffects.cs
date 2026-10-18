using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class RunRules
    {
        public int StealsRemaining {get;private set;}
        public int ShopRerollCount {get;private set;}
        readonly List<string> shopSpawnGroups=new List<string>();
        int shopEliteSpawns;
        void ResetSpecialShopEffects()
        {StealsRemaining=ShopRerollCount=shopEliteSpawns=0;shopSpawnGroups.Clear();}
        void EnterSpecialShop()
        {StealsRemaining=Math.Max(0,(int)Stat("item_steals"));ShopRerollCount=0;}
        int ApplyShopRerollEffects()
        {
            // base_shop applies stat rolls on free and paid shop rerolls, then
            // consumes only the first tier-increase effect for the new offers.
            foreach(var e in Effects("gain_stats_on_reroll").ToArray())
                if(random.NextDouble()<e.chance/100)Add(e.key.Replace("stat_",""),e.value);
            var tier=Effects("increase_tier_on_reroll").FirstOrDefault();
            if(tier==null)return 0;
            var source=Items.FirstOrDefault(i=>(i.id==tier.key||"item_"+i.id==tier.key)&&
                (i.conditionalEffects??Array.Empty<ConditionalEffect>()).Any(e=>e.customKey=="increase_tier_on_reroll"&&e.value==tier.value));
            if(source!=null)RemoveItem(source);
            return (int)tier.value;
        }
        void BuyItemWithDuplicates(ItemEntry item)
        {
            foreach(var effect in Effects("duplicate_item").ToArray())
            {
                int remaining=item.maxCount<0?int.MaxValue:Math.Max(0,item.maxCount-Items.Count(i=>i.id==item.id));
                int count=Math.Max(0,Math.Min((int)effect.value,remaining-1));
                for(int n=0;n<count;n++)
                {
                    var source=Items.FirstOrDefault(i=>i.id==effect.key||"item_"+i.id==effect.key);
                    if(source==null)break;
                    RemoveItem(source);ApplyItem(item);
                }
            }
            ApplyItem(item);
        }
        bool CanAcquire(ShopOffer offer)=>offer!=null&&(offer.weapon==null||CanEquip(offer.weapon)&&
            (Inventory.Count<WeaponSlots&&CanAddWeapon(offer.weapon)||FindMatch(offer.weapon)>=0));
        public bool CanSteal(int index)=>Phase==RunPhase.Shop&&StealsRemaining>0&&index>=0&&index<Shop.Length&&CanAcquire(Shop[index]);
        public float StealCaughtChance(ShopOffer offer)=>Mathf.Min(.95f,Stat("item_steals_spawns_random_elite")+Wave*.1f*(Price(offer)/300f)*(offer.Tier*.25f));
        public bool Steal(int index)
        {
            if(!CanSteal(index))return false;
            var offer=Shop[index];StealsRemaining--;
            foreach(var e in Effects("item_steals_spawns_enemy"))
                if(random.NextDouble()<e.value/100)shopSpawnGroups.Add(e.key);
            if(random.NextDouble()<StealCaughtChance(offer))shopEliteSpawns++;
            return PurchaseOffer(index,true);
        }
        public string[] TakeShopSpawnGroups(){var result=shopSpawnGroups.ToArray();shopSpawnGroups.Clear();return result;}
        public int TakeShopEliteSpawns(){int result=shopEliteSpawns;shopEliteSpawns=0;return result;}
    }
    [Serializable] public sealed class ShopSpawnEntry {public string path;public SpawnGroup group;}
    [Serializable] public sealed class ShopSpawnCatalog
    {
        public ShopSpawnEntry[] groups;
        // wave_manager.gd fills the empty group_elite template with the chosen elite.
        public static SpawnGroup EliteGroup(string id)=>new SpawnGroup{start=1,repeats=-1,chance=1,area=-1,edge=true,boss=true,units=new[]{new UnitEntry{id=id,min=1,max=1,chance=1}}};
    }
    public sealed partial class ArenaGame
    {
        void AddShopEffectEncounters()
        {
            var groups=Rules.TakeShopSpawnGroups();
            if(groups.Length>0)
            {
                var data=JsonUtility.FromJson<ShopSpawnCatalog>(Resources.Load<TextAsset>("shop-effect-spawns").text);
                foreach(var path in groups)
                {
                    var match=data.groups.FirstOrDefault(g=>g.path==path.Replace("res://",""));
                    if(match==null)throw new InvalidOperationException("Missing shop effect spawn group: "+path);
                    if(match.group.boss&&match.group.units.Length==0)
                    {var candidates=encounterData.enemies.Where(e=>e.role=="Elite").ToArray();QueueElite(candidates[placementRandom.Next(candidates.Length)].id);}
                    else director.AddExtra(match.group);
                }
            }
            int elites=Rules.TakeShopEliteSpawns();
            var pool=encounterData.enemies.Where(e=>e.role=="Elite").ToArray();
            for(int i=0;i<elites;i++)QueueElite(pool[placementRandom.Next(pool.Length)].id);
        }
    }
}
