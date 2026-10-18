using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartShopUnlockRuntimeReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--shop-unlock-review");
            if(index<0||index+1>=args.Length)return;
            var fixture=new GameObject("Native shop and unlock fixture").AddComponent<ShopUnlockRuntimeReview>();
            fixture.folder=args[index+1];
        }
        sealed class ShopUnlockRuntimeReview:MonoBehaviour
        {
            public string folder;
            ArenaGame game;float started;
            [Serializable] sealed class Report
            {
                public string status,error,savePath,itemId,
                    scope="Opt-in staged native shop/unlock fixture; not a normal run. Stages Demon, a real catalog Bag offer, raw HP currency, lifetime counters and an immediate Fruit Basket threshold. Actual Update handles death, Rookie recording and frozen-pool lifecycle; real RetryCurrentWave restores the checkpoint. Saves/reloads only a unique artifact-path fixture profile. Screenshots render real ShopUI; screenshot existence does not itself verify pixels.";
                public bool isolatedProfile,demonCostOne,demonFirstPurchase,demonSecondRejected,shopCapture,
                    savedReloaded,midRunItemFrozen,deathRookieVisible,deathPoolFrozen,retryPreservesProgress,
                    retryPoolFrozen,retryCapture,nextRunEligible,nextRunCapture;
                public int price,rawCurrencyBefore,rawCurrencyAfter,combatMaxHpAfter,bagCopies,retries;
                public long lifetimeKills;
            }
            readonly Report report=new Report();
            IEnumerator Start()
            {
                started=Time.realtimeSinceStartup;string error=null;var routine=Run();
                while(true)
                {
                    bool running=false;object next=null;
                    try{Require(Time.realtimeSinceStartup-started<25,"Fixture exceeded 25 seconds.");running=routine.MoveNext();if(running)next=routine.Current;}
                    catch(Exception ex){error=ex.ToString();}
                    if(!running)break;yield return next;
                }
                report.error=error;
                bool passed=error==null&&report.isolatedProfile&&report.demonCostOne&&report.demonFirstPurchase&&report.demonSecondRejected&&report.shopCapture&&
                    report.savedReloaded&&report.midRunItemFrozen&&report.deathRookieVisible&&report.deathPoolFrozen&&report.retryPreservesProgress&&report.retryPoolFrozen&&report.retryCapture&&report.nextRunEligible&&report.nextRunCapture;
                report.status=passed?"Passed":"Failed";Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder,"shop-unlock-review.json"),JsonUtility.ToJson(report,true));Application.Quit(passed?0:1);
            }
            IEnumerator Run()
            {
                folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null,"ArenaGame initialization required.");
                Require(Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use --controller-check to isolate settings/profile.");
                report.isolatedProfile=GameSettings.IsFixture;Require(report.isolatedProfile,"Fixture-only profile required.");
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);GameSettings.Set("retryWave",true);
                game.sound.SetMuted(true);foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);
                var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
                report.savePath=Path.Combine(folder,"fixture-progress-"+Guid.NewGuid().ToString("N")+".json");
                var profile=new Progression(definitions,report.savePath);game.progress=profile;game.runUnlockPool=new RunUnlockPool();
                profile.ChallengeCompleted+=game.achievementToasts.Enqueue;profile.ChallengeCompleted+=_=>game.ApplyUnlocks();game.ApplyUnlocks();

                // Stage an imported Demon and real non-HP Bag. Bag costs 1 HP
                // at wave one, without introducing a synthetic item definition.
                game.playerAnimation.lookIndex=Array.FindIndex(game.catalog.data.characters,c=>c.id=="demon");
                Require(game.playerAnimation.lookIndex>=0,"Imported Demon required.");game.startingWeapon=0;game.StartSelectedRun();game.ClearCombat();
                game.Rules.Tick(game.Rules.Remaining);while(game.Rules.Phase==RunPhase.Upgrade)game.Rules.Choose(0);
                Require(game.Rules.Phase==RunPhase.Shop,"Demon shop staging failed.");
                var bag=game.catalog.data.items.Single(i=>i.id=="bag");report.itemId=bag.id;
                var offer=new ShopOffer{item=bag,rolledWave=game.Rules.Wave};game.Rules.Shop[0]=offer;
                game.Rules.GainWeaponStat("max_hp",1-game.Rules.Stat("max_hp"));
                report.price=game.Rules.Price(offer);report.rawCurrencyBefore=(int)game.Rules.ShopBalance;
                report.demonCostOne=report.price==1&&report.rawCurrencyBefore==1;Require(report.demonCostOne,"Expected actual Bag to cost one HP.");
                report.demonFirstPurchase=game.Rules.Buy(0);Require(report.demonFirstPurchase,"First cost-one purchase should succeed.");
                report.rawCurrencyAfter=(int)game.Rules.ShopBalance;report.combatMaxHpAfter=(int)game.Rules.MaxHealth;report.bagCopies=game.Rules.Items.Count(i=>i.id==bag.id);
                game.Rules.Shop[0]=offer;report.demonSecondRejected=!game.Rules.CanBuy(0)&&!game.Rules.Buy(0)&&report.rawCurrencyAfter==0&&report.combatMaxHpAfter==1&&report.bagCopies==1;
                Require(report.demonSecondRejected,"Zero shop currency must not reuse the combat minimum as another HP.");
                yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"demon-zero-shop.png"));yield return null;yield return new WaitForSecondsRealtime(.15f);
                report.shopCapture=CaptureExists("demon-zero-shop.png");Require(report.shopCapture,"Demon shop capture missing.");

                // Fresh neutral run; source eligibility snapshot is frozen while
                // global immediate rewards and saves remain live.
                game.playerAnimation.lookIndex=Array.FindIndex(game.catalog.data.characters,c=>c.id=="well_rounded");game.startingWeapon=0;game.StartSelectedRun();game.ClearCombat();game.invulnerable=100;
                var fruit=game.catalog.data.items.Single(i=>i.id=="fruit_basket");Require(!fruit.available,"Fresh profile fruit reward must be locked.");
                profile.Count("kills",7);profile.Count("full_health_fruit",100);
                report.midRunItemFrozen=profile.Unlocked("item_fruit_basket")&&!fruit.available&&game.runUnlockPool.Frozen;
                Require(report.midRunItemFrozen,"Mid-run item reward must not enter the current eligibility snapshot.");
                var loaded=new Progression(definitions,report.savePath);
                report.savedReloaded=loaded.Completed("chal_fruit_basket")&&loaded.Total("kills")==7;
                Require(report.savedReloaded,"Actual isolated save/reload lost the item reward or lifetime counter.");
                game.Rules.Hurt(100000);Require(game.Rules.Phase==RunPhase.Lost,"Lethal staged hit must start death.");
                while(game.Rules.DeathCleanupPending)yield return null;
                yield return null;
                report.deathRookieVisible=profile.Completed("chal_rookie")&&game.catalog.data.characters.Single(c=>c.id=="chunky").available;
                report.deathPoolFrozen=game.runUnlockPool.Frozen&&!fruit.available&&!game.endSummary;
                Require(report.deathRookieVisible&&report.deathPoolFrozen,"Actual death lifecycle must expose Rookie but keep same-run item eligibility frozen.");
                Require(game.Rules.CanRetryWave,"Retry checkpoint must be available after death cleanup.");game.RetryCurrentWave();game.invulnerable=100;
                report.retries=game.Rules.Retries;report.lifetimeKills=profile.Total("kills");
                report.retryPreservesProgress=game.Rules.Phase==RunPhase.Playing&&ReferenceEquals(profile,game.progress)&&ReferenceEquals(profile,game.Rules.Progress)&&profile.Completed("chal_rookie")&&profile.Completed("chal_fruit_basket")&&report.lifetimeKills==7&&report.retries==1&&!game.recordedFinish;
                report.retryPoolFrozen=game.runUnlockPool.Frozen&&!fruit.available;
                Require(report.retryPreservesProgress&&report.retryPoolFrozen,"Retry must keep global progression and the existing frozen pool.");
                yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"retry-frozen-run.png"));yield return null;yield return new WaitForSecondsRealtime(.15f);
                report.retryCapture=CaptureExists("retry-frozen-run.png");Require(report.retryCapture,"Retry capture missing.");
                loaded=new Progression(definitions,report.savePath);Require(loaded.Completed("chal_rookie")&&loaded.Completed("chal_fruit_basket")&&loaded.Total("kills")==7,"Death/retry progression save must reload without loss.");

                // A genuinely fresh run refreshes the snapshot. The displayed
                // unlocked offer is explicitly staged, not a random shop result.
                game.StartSelectedRun();game.ClearCombat();game.invulnerable=100;
                report.nextRunEligible=fruit.available&&RunRules.IsShopItem(fruit.id)&&ShopSelection.ItemPool(game.catalog.data.items,null,fruit.tier,false,Array.Empty<ItemEntry>(),Array.Empty<ItemEntry>()).Contains(fruit);
                Require(report.nextRunEligible,"Next fresh run must include the saved unlocked item.");
                game.Rules.Tick(game.Rules.Remaining);while(game.Rules.Phase==RunPhase.Upgrade)game.Rules.Choose(0);
                Require(game.Rules.Phase==RunPhase.Shop,"Next-run shop staging failed.");game.Rules.Shop[0]=new ShopOffer{item=fruit,rolledWave=game.Rules.Wave};
                yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"next-run-unlocked-shop.png"));yield return null;yield return new WaitForSecondsRealtime(.15f);
                report.nextRunCapture=CaptureExists("next-run-unlocked-shop.png");Require(report.nextRunCapture,"Next-run shop capture missing.");
            }
            bool CaptureExists(string name){string path=Path.Combine(folder,name);return File.Exists(path)&&new FileInfo(path).Length>0;}
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
