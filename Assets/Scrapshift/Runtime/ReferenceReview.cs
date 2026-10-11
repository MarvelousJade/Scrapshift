using System;
using System.IO;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool review, setReview, setBonusesObserved, setCombineObserved;
        int setReviewStage;
        float setHealthBeforeCombine;
        string reviewPath;
        float reviewStart;
        int reviewStep;
        [Serializable] sealed class ReviewReport
        {
            public string scope,status;
            public int shots,kills,meleeHits;
            public bool gameplay,shop,upgrade,weaponSets,combineRemovesPrimitiveBonus;
        }
        void BeginReview(string folder)
        {
            review=true;
            setReview=Array.IndexOf(Environment.GetCommandLineArgs(),"--weapon-set-review")>=0;
            reviewPath=folder;
            Directory.CreateDirectory(folder);
            reviewStart=Time.realtimeSinceStartup;
            Begin();
            for(int i=0;i<8;i++)
            {
                Rules.Tick(Rules.Remaining);
                while(Rules.Phase==RunPhase.Loot)Rules.ResolveCrate(true);while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);
                Rules.NextWave();
            }
            Rules.Inventory.Clear();
            foreach(string id in setReview?new[]{"pistol","smg","knife","knife","spear","spear"}:new[]{"pistol","smg","double_barrel_shotgun","laser_gun","knife","spear"})
            Rules.Inventory.Add(catalog.Weapon(id,2));StartWave();int index=0;foreach(string id in new[]
            {
                "charger","spitter","spawner","helmet_alien","bruiser","buffer","pursuer"
            })
            {
                float a=index++*Mathf.PI*2/7;Spawn(catalog.Enemy(id),new Vector2(Mathf.Cos(a)*5,Mathf.Sin(a)*3.8f));
            }
        }
        void ReviewTick()
        {
            if(!review)return;float t=Time.realtimeSinceStartup-reviewStart;if(Rules.Phase==RunPhase.Playing)Rules.Heal(100); // Explicit opt-in presentation fixture, never normal gameplay.
            if(reviewStep==0&&t>7)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(reviewPath,"combat.png"));reviewStep++;
            }
            if(reviewStep==1&&t>10)
            {
                Rules.Collect(500);Rules.Tick(Rules.Remaining);ClearCombat();while(Rules.Phase==RunPhase.Loot)Rules.ResolveCrate(true);while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);Rules.Shop[0]=new ShopOffer
                {
                    weapon=catalog.Weapon("smg",2)
                };
                Rules.Shop[1]=new ShopOffer
                {
                    weapon=catalog.Weapon("double_barrel_shotgun",3),locked=true
                };
                Rules.Shop[2]=new ShopOffer
                {
                    item=Array.Find(catalog.data.items,i=>i.id=="coffee")
                };
                Rules.Shop[3]=new ShopOffer
                {
                    item=Array.Find(catalog.data.items,i=>i.id=="helmet")
                };
                reviewStep++;
            }
            if(reviewStep==2&&t>12)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(reviewPath,"shop.png"));reviewStep++;
                if(Array.IndexOf(Environment.GetCommandLineArgs(),"--review-interactive")>=0)
                {
                    review=false;
                    return;
                }
            }
            if(setReview && reviewStep==3)
            {
                if(setReviewStage==0 && t>12.2f)
                {
                    setBonusesObserved=Rules.SetCount(WeaponSets.Gun)==2 && Rules.SetCount(WeaponSets.Precise)==2 && Rules.SetCount(WeaponSets.Primitive)==2 &&
                        Rules.Stat("range")==10 && Rules.Stat("crit_chance")>=3;
                    setHealthBeforeCombine=Rules.MaxHealth;setReviewStage=1;
                }
                if(setReviewStage==1 && t>12.7f)
                { ScreenCapture.CaptureScreenshot(Path.Combine(reviewPath,"set-thresholds.png"));setReviewStage=2; }
                if(setReviewStage==2 && t>13.1f)
                {
                    Rules.Combine(4);SyncEquipment();
                    setCombineObserved=Rules.SetCount(WeaponSets.Primitive)==1 && Rules.MaxHealth==setHealthBeforeCombine-3 && Rules.Inventory.Count==5;
                    setReviewStage=3;
                }
                if(setReviewStage==3 && t>13.5f)
                { ScreenCapture.CaptureScreenshot(Path.Combine(reviewPath,"after-combine.png"));setReviewStage=4; }
            }
            if(reviewStep==3&&t>14)
            {
                AdvanceWave();Rules.Collect(Rules.NextLevel);Rules.Tick(Rules.Remaining);ClearCombat();reviewStep++;
            }
            if(reviewStep==4&&t>16)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(reviewPath,"upgrade.png"));reviewStep++;
            }
            if(t>18)
            {
                bool a=File.Exists(Path.Combine(reviewPath,"combat.png")),b=File.Exists(Path.Combine(reviewPath,"shop.png")),c=File.Exists(Path.Combine(reviewPath,"upgrade.png"));var report=new ReviewReport
                {
                    scope=(setReview?"Weapon-set variant: two weapons per set, forced threshold panel, scripted spear combine. ":"")+"Opt-in scripted presentation fixture: starts wave 9 with six tier-2 weapons, adds selected enemies, restores health, supplies shop currency. Proves runtime rendering/combat paths, not normal-run balance or manual input.",weaponSets=setBonusesObserved,combineRemovesPrimitiveBonus=setCombineObserved,status=a&&b&&c&&shotsFired>0&&orbitHits>0&&(!setReview||(setBonusesObserved&&setCombineObserved&&setReviewStage==4))?"Passed":"Failed",shots=shotsFired,kills=Rules.Kills,meleeHits=orbitHits,gameplay=a,shop=b,upgrade=c
                };
                File.WriteAllText(Path.Combine(reviewPath,"review.json"),JsonUtility.ToJson(report,true));Application.Quit(report.status=="Passed"?0:1);enabled=false;
            }
        }
    }
}
