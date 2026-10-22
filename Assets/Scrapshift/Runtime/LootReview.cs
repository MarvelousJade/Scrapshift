using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool lootReview;
        string lootReviewPath;
        float lootReviewStart;
        int lootReviewStage;
        LootReport lootReport;
        [Serializable] sealed class LootReport
        {
            public string status,scope;
            public bool looterCrate,collectedHeal,cleanupCrate,crateBeforeUpgrade,takeApplied,recyclePaid,shopReached;
        }
        void BeginLootReview(string folder)
        {
            lootReview=true;lootReviewPath=folder;Directory.CreateDirectory(folder);
            lootReviewStart=Time.realtimeSinceStartup;lootReport=new LootReport {
                scope="Opt-in Windows presentation fixture: stationary invulnerable player, no weapons, supplied looter killed by scripted damage, supplied offscreen crate and XP, scripted take/recycle. Uses actual drop/pickup/cleanup/reward paths. Not manual input, random drop-frequency validation or normal balance."};
            Begin();Rules.Inventory.Clear();SyncEquipment();Rules.Hurt(6);
            Spawn(catalog.Enemy("looter"),new Vector2(3,0));
        }
        void LootReviewTick()
        {
            if(!lootReview)return;
            float t=Time.realtimeSinceStartup-lootReviewStart;
            invulnerable=.3f;
            if(lootReviewStage==0 && t>1.5f)
            {
                int target=enemies.FindIndex(e=>e.type.id=="looter");
                if(target>=0){enemies[target].pos=new Vector2(3,0);Damage(target,10000);}
                // Observe the scripted kill after the source's deferred die.
                ResolveCombatCallbacks();
                lootReport.looterCrate=cratesSpawned==1 && pickups.Exists(p=>p.consumable==ConsumableKind.Crate);
                lootReviewStage++;
            }
            if(lootReviewStage==1 && t>2.5f)
            {ScreenCapture.CaptureScreenshot(Path.Combine(lootReviewPath,"ground-crate.png"));lootReviewStage++;}
            if(lootReviewStage==2 && t>3)
            {
                var crate=pickups.Find(p=>p.consumable==ConsumableKind.Crate);
                float healthBefore=Rules.Health;
                if(crate!=null){playerPos=crate.motion.position;UpdatePickups(.02f);}
                lootReport.collectedHeal=Rules.PendingCrates==1 && Rules.Health==Mathf.Min(Rules.MaxHealth,healthBefore+3);
                DropConsumable(new Vector2(-9,-4),ConsumableKind.Crate);
                Rules.Collect(16);Rules.Tick(Rules.Remaining-.01f);lootReviewStage++;
            }
            if(lootReviewStage==3 && t>4 && Rules.Phase==RunPhase.Loot)
            {
                lootReport.cleanupCrate=Rules.PendingCrates==2;
                lootReport.crateBeforeUpgrade=Rules.Phase==RunPhase.Loot && Rules.PendingLevels>0;
                ScreenCapture.CaptureScreenshot(Path.Combine(lootReviewPath,"take-or-recycle.png"));lootReviewStage++;
                lootReviewStart=Time.realtimeSinceStartup-4;t=4;
            }
            if(lootReviewStage==4 && t>5)
            {
                var item=Rules.CrateItem;int owned=Rules.Items.Count;
                ResolveCrate(true);lootReport.takeApplied=Rules.Items.Count==owned+1 && Rules.Items.Contains(item);
                int gold=Rules.Scrap,refund=Rules.CrateRecycleValue;
                ResolveCrate(false);lootReport.recyclePaid=Rules.Scrap==gold+refund && Rules.PendingCrates==0;
                lootReviewStage++;
            }
            if(lootReviewStage==5 && t>6)
            {ScreenCapture.CaptureScreenshot(Path.Combine(lootReviewPath,"upgrade-after-crates.png"));lootReviewStage++;}
            if(lootReviewStage==6 && t>7)
            {
                while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);
                lootReport.shopReached=Rules.Phase==RunPhase.Shop;
                ScreenCapture.CaptureScreenshot(Path.Combine(lootReviewPath,"shop-after-loot.png"));lootReviewStage++;
            }
            if(t>9)
            {
                bool passed=lootReport.looterCrate&&lootReport.collectedHeal&&lootReport.cleanupCrate&&lootReport.crateBeforeUpgrade&&lootReport.takeApplied&&lootReport.recyclePaid&&lootReport.shopReached;
                lootReport.status=passed?"Passed":"Failed";
                File.WriteAllText(Path.Combine(lootReviewPath,"loot-review.json"),JsonUtility.ToJson(lootReport,true));
                Application.Quit(passed?0:1);enabled=false;
            }
        }
    }
}
