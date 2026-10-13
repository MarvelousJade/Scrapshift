using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrapshift
{
    public sealed partial class RunRules
    {
        readonly Queue<bool> crates=new Queue<bool>();
        public int PendingCrates=>crates.Count;
        public ItemEntry CrateItem {get;private set;}
        public int CrateRecycleValue=>CrateItem==null?0:RecycleValue(CrateItem.value);

        public void CollectConsumable(ConsumableKind kind)
        {
            if(Phase!=RunPhase.Playing || kind==ConsumableKind.None)return;
            runConsumables++;Progress?.Check("chal_hungry",runConsumables);if(kind==ConsumableKind.Fruit&&Health>=MaxHealth)Progress?.Count("full_health_fruit");
            ConsumableFullHealthTriggers();
            Heal(Math.Max(0,3+Stat("consumable_heal")));
            if(kind==ConsumableKind.Crate || kind==ConsumableKind.LegendaryCrate)
            {crates.Enqueue(kind==ConsumableKind.LegendaryCrate);Scrap+=Math.Max(0,(int)Stat("item_box_gold"));}
        }

        void OpenRewards()
        {
            if(crates.Count>0 && catalog!=null)
            {
                int tier=crates.Peek()?4:ReferenceRarity.Roll(Wave,Stat("luck"),random.NextDouble());
                // Same intentionally small catalog and empty-tier fallback as our shop.
                var pool=ShopSelection.ItemPool(catalog.data.items,Character,tier,random.NextDouble()<WantedTagChance,Array.Empty<ItemEntry>(),
                    Items.Concat(Shop.Where(s=>s?.item!=null&&s.locked).Select(s=>s.item)));
                if(pool.Length>0)
                {
                    CrateItem=pool[random.Next(pool.Length)];Phase=RunPhase.Loot;return;
                }
                crates.Clear(); // A malformed/empty catalog must not trap the intermission.
            }
            CrateItem=null;
            if(PendingLevels>0){Phase=RunPhase.Upgrade;Draft();}
            else OpenShop();
        }

        public bool ResolveCrate(bool take)
        {
            if(Phase!=RunPhase.Loot || CrateItem==null || crates.Count==0)return false;
            if(take)ApplyItem(CrateItem);
            else Scrap+=CrateRecycleValue;
            crates.Dequeue();CrateItem=null;OpenRewards();return true;
        }

        void ApplyItem(ItemEntry item)
        {
            Items.Add(item);Progress?.Count("item:"+item.id);QueueNextWaveEffects(item);
            if(item.id=="bait"){queuedBaits++;}
            foreach(var effect in item.effects){Add(effect.key.Replace("stat_",""),effect.value);OnItemStatAdded(effect.key,effect.value);}
            if(Phase==RunPhase.Shop)UpdateShopRerolls();
            if(item.conditionalEffects!=null)foreach(var effect in item.conditionalEffects)if(effect.customKey=="stat_cap")
            {if(effect.key=="hp_cap")healthCap=MaxHealth;else if(effect.key=="speed_cap")speedCap=Stat("speed");}
        }
    }
}
