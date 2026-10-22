using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool economyNativeReview;
        string economyNativePath;
        int economyNativeStage;
        float economyNativeNext,economyNativeStarted;
        ItemEntry economyCoupon,economyPeacock;
        [Serializable] sealed class EconomyNativeReport
        {
            public string status,error,scope="Opt-in built-player economy fixture with source-derived assertions and staged real screen renderers. Synthetic shop effects are not base catalog content; this is not a human playthrough.";
            public bool inMemoryProfile,unlockFrozenInRun,unlockAvailableNextRun,characterUnlockImmediate,
                itemAcquisitionAndRemoval,itemLimitAndLockedPrice,syntheticSpecialShop,screenshotsExist;
            public int purchasedCoupons,remainingCoupons,startingLockedPrice,nextWaveLockedPrice,freeRerollCost,paidRerollCost;
        }
        EconomyNativeReport economyNativeReport;

        void BeginEconomyNativeReview(string folder)
        {
            economyNativeReview=true;economyNativePath=Path.GetFullPath(folder);Directory.CreateDirectory(economyNativePath);
            economyNativeStarted=Time.realtimeSinceStartup;economyNativeNext=economyNativeStarted+.5f;
            economyNativeReport=new EconomyNativeReport{inMemoryProfile=GameSettings.IsFixture};
            Application.runInBackground=true;sound.SetMuted(true);
        }
        void EconomyNativeReviewTick()
        {
            if(!economyNativeReview)return;
            if(Time.realtimeSinceStartup-economyNativeStarted>25)
            {FinishEconomyNativeReview("Timed out before completing staged cases.");return;}
            if(Time.realtimeSinceStartup<economyNativeNext)return;
            try
            {
                switch(economyNativeStage++)
                {
                    case 0: EconomyNativeUnlockCase();CaptureEconomyNative("unlock-next-run");break;
                    case 1: EconomyNativeAcquireCase();CaptureEconomyNative("acquired-items");break;
                    case 2: EconomyNativeRemoveCase();CaptureEconomyNative("removed-items");break;
                    case 3: EconomyNativeLimitCase();CaptureEconomyNative("locked-price");break;
                    case 4: EconomyNativeSpecialCase();CaptureEconomyNative("synthetic-special-shop");break;
                    default: FinishEconomyNativeReview(null);return;
                }
                economyNativeNext=Time.realtimeSinceStartup+.8f;
            }
            catch(Exception error){FinishEconomyNativeReview(error.ToString());}
        }
        void CaptureEconomyNative(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(economyNativePath,name+".png"));
        void EconomyNativeUnlockCase()
        {
            var fruit=catalog.data.items.Single(i=>i.id=="fruit_basket");
            var chunky=catalog.data.characters.Single(c=>c.id=="chunky");
            bool initiallyLocked=!fruit.available;
            StartSelectedRun();Rules.Progress=progress;
            progress.Complete("chal_fruit_basket");progress.Complete("chal_rookie");
            economyNativeReport.unlockFrozenInRun=initiallyLocked&&!fruit.available&&!RunRules.IsShopItem("fruit_basket");
            economyNativeReport.characterUnlockImmediate=chunky.available;
            StartSelectedRun();Rules.Progress=progress;
            economyNativeReport.unlockAvailableNextRun=fruit.available&&RunRules.IsShopItem("fruit_basket");
        }
        void EconomyNativeAcquireCase()
        {
            Begin();Rules.Progress=progress;Rules.Start(startingMaterials:500);Rules.Tick(Rules.Remaining);
            economyCoupon=catalog.data.items.Single(i=>i.id=="coupon");economyPeacock=catalog.data.items.Single(i=>i.id=="peacock");
            int balance=Rules.Scrap;
            Rules.Shop[0]=new ShopOffer{item=economyCoupon,rolledWave=Rules.Wave};int first=Rules.Price(Rules.Shop[0]);bool boughtFirst=Rules.Buy(0);
            Rules.Shop[0]=new ShopOffer{item=economyCoupon,rolledWave=Rules.Wave};int second=Rules.Price(Rules.Shop[0]);bool boughtSecond=Rules.Buy(0);
            Rules.Shop[0]=new ShopOffer{item=economyPeacock,rolledWave=Rules.Wave};int third=Rules.Price(Rules.Shop[0]);bool boughtThird=Rules.Buy(0);
            economyNativeReport.purchasedCoupons=Rules.Items.Count(i=>i.id=="coupon");
            economyNativeReport.itemAcquisitionAndRemoval=boughtFirst&&boughtSecond&&boughtThird&&
                Rules.Scrap==balance-first-second-third&&economyNativeReport.purchasedCoupons==2&&
                Rules.Items.Count(i=>i.id=="peacock")==1&&Rules.Stat("items_price")==-10&&Rules.Stat("xp_gain")==25;
        }
        void EconomyNativeRemoveCase()
        {
            bool removedCoupon=Rules.RemoveItem(economyCoupon),removedPeacock=Rules.RemoveItem(economyPeacock);
            economyNativeReport.remainingCoupons=Rules.Items.Count(i=>i.id=="coupon");
            Rules.NextWave();
            economyNativeReport.itemAcquisitionAndRemoval&=removedCoupon&&removedPeacock&&
                economyNativeReport.remainingCoupons==1&&Rules.Stat("items_price")==-5&&Rules.Stat("xp_gain")==0;
            pauseOptions=false;pauseCodex=false;
            Rules.Pause();
        }
        void EconomyNativeLimitCase()
        {
            Begin();Rules.Progress=progress;Rules.Start(startingMaterials:500);
            var coupon=catalog.data.items.Single(i=>i.id=="coupon");
            for(int i=0;i<coupon.maxCount;i++)Rules.GrantItem(coupon);
            bool excluded=!ShopSelection.ItemPool(catalog.data.items,null,coupon.tier,false,Array.Empty<ItemEntry>(),Rules.Items).Contains(coupon);
            Rules.RemoveItem(coupon);
            bool eligible=ShopSelection.ItemPool(catalog.data.items,null,coupon.tier,false,Array.Empty<ItemEntry>(),Rules.Items).Contains(coupon);
            Rules.Tick(Rules.Remaining);Rules.Shop[0]=new ShopOffer{item=coupon,rolledWave=Rules.Wave};
            economyNativeReport.startingLockedPrice=Rules.Price(Rules.Shop[0]);Rules.ToggleLock(0);
            var locked=Rules.Shop[0];Rules.NextWave();Rules.Tick(Rules.Remaining);
            economyNativeReport.nextWaveLockedPrice=Rules.Price(Rules.Shop[0]);
            economyNativeReport.itemLimitAndLockedPrice=excluded&&eligible&&ReferenceEquals(locked,Rules.Shop[0])&&
                economyNativeReport.startingLockedPrice==economyNativeReport.nextWaveLockedPrice;
        }
        void EconomyNativeSpecialCase()
        {
            Begin();Rules.Progress=progress;Rules.Start(startingMaterials:500);
            var token=new ItemEntry{id="economy-tier-token",effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{
                new ConditionalEffect{customKey="increase_tier_on_reroll",key="item_economy-tier-token",value=1}}};
            var dice=new ItemEntry{id="economy-dice",effects=new[]{new StatEffect{key="free_rerolls",value=1}},conditionalEffects=new[]{
                new ConditionalEffect{customKey="gain_stats_on_reroll",key="stat_luck",value=3,chance=100}}};
            var thief=new ItemEntry{id="economy-thief",effects=new[]{new StatEffect{key="item_steals",value=1}},conditionalEffects=new[]{
                new ConditionalEffect{customKey="item_steals_spawns_enemy",key="res://zones/common/bait/group_bait.tres",value=100}}};
            Rules.GrantItem(token);Rules.GrantItem(token);Rules.GrantItem(dice);Rules.GrantItem(thief);
            Rules.Tick(Rules.Remaining);var locked=Rules.Shop[0];Rules.ToggleLock(0);int lockedPrice=Rules.Price(locked);
            economyNativeReport.freeRerollCost=Rules.RerollPrice;int initial=Rules.Scrap;
            Rules.Reroll();bool first=Rules.Stat("luck")==3&&Rules.Items.Count(i=>i.id==token.id)==1&&Rules.Scrap==initial;
            economyNativeReport.paidRerollCost=Rules.RerollPrice;int paid=Rules.RerollPrice;Rules.Reroll();
            bool second=Rules.Stat("luck")==6&&Rules.Items.All(i=>i.id!=token.id)&&Rules.Scrap==initial-paid;
            var duplicator=new ItemEntry{id="economy-duplicator",effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{
                new ConditionalEffect{customKey="duplicate_item",key="item_economy-duplicator",value=1}}};
            var target=new ItemEntry{id="economy-target",value=5,tier=1,maxCount=2,effects=Array.Empty<StatEffect>()};
            Rules.GrantItem(duplicator);Rules.Shop[1]=new ShopOffer{item=target,rolledWave=Rules.Wave};
            bool bought=Rules.Buy(1)&&Rules.Items.Count(i=>i.id==target.id)==2&&Rules.Items.All(i=>i.id!=duplicator.id);
            Rules.Shop[2]=new ShopOffer{item=new ItemEntry{id="economy-stolen",value=5,tier=1,effects=Array.Empty<StatEffect>()},rolledWave=Rules.Wave};
            int beforeSteal=Rules.Scrap;bool stole=Rules.Steal(2)&&Rules.Scrap==beforeSteal&&
                Rules.TakeShopSpawnGroups().Contains("res://zones/common/bait/group_bait.tres");
            economyNativeReport.syntheticSpecialShop=economyNativeReport.freeRerollCost==0&&economyNativeReport.paidRerollCost>0&&
                first&&second&&ReferenceEquals(locked,Rules.Shop[0])&&Rules.Price(locked)==lockedPrice&&bought&&stole;
        }
        void FinishEconomyNativeReview(string error)
        {
            economyNativeReport.error=error;
            economyNativeReport.screenshotsExist=new[]{"unlock-next-run","acquired-items","removed-items","locked-price","synthetic-special-shop"}
                .All(name=>{var path=Path.Combine(economyNativePath,name+".png");return File.Exists(path)&&new FileInfo(path).Length>0;});
            bool passed=error==null&&economyNativeReport.inMemoryProfile&&economyNativeReport.unlockFrozenInRun&&
                economyNativeReport.unlockAvailableNextRun&&economyNativeReport.characterUnlockImmediate&&
                economyNativeReport.itemAcquisitionAndRemoval&&economyNativeReport.itemLimitAndLockedPrice&&
                economyNativeReport.syntheticSpecialShop&&economyNativeReport.screenshotsExist;
            economyNativeReport.status=passed?"Passed":"Failed";
            File.WriteAllText(Path.Combine(economyNativePath,"economy-native.json"),JsonUtility.ToJson(economyNativeReport,true));
            economyNativeReview=false;Application.Quit(passed?0:1);enabled=false;
        }
    }
}
