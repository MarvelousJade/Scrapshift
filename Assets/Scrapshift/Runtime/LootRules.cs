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
        public int CrateRecycleValue=>CrateItem==null?0:Math.Max(1,(int)(Price(CrateItem.value)*.25f));

        public void CollectConsumable(ConsumableKind kind)
        {
            if(Phase!=RunPhase.Playing || kind==ConsumableKind.None)return;
            Heal(3);
            if(kind==ConsumableKind.Crate || kind==ConsumableKind.LegendaryCrate)
                crates.Enqueue(kind==ConsumableKind.LegendaryCrate);
        }

        void OpenRewards()
        {
            if(crates.Count>0 && catalog!=null)
            {
                int tier=crates.Peek()?4:ReferenceRarity.Roll(Wave,Stat("luck"),random.NextDouble());
                // Same intentionally small catalog and empty-tier fallback as our shop.
                ItemEntry[] pool;
                do { pool=catalog.data.items.Where(i=>i.tier==tier && IsShopItem(i.id)).ToArray(); }
                while(pool.Length==0 && --tier>0);
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
            Items.Add(item);
            foreach(var effect in item.effects)Add(effect.key.Replace("stat_",""),effect.value);
        }
    }
}
