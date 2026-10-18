using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SpecialShopParityTests
    {
        ReferenceCatalog Catalog=>AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        static ItemEntry Item(string id,StatEffect[] stats=null,params ConditionalEffect[] effects)=>new ItemEntry
        {id=id,title=id,tier=1,value=10,available=true,maxCount=-1,effects=stats??Array.Empty<StatEffect>(),conditionalEffects=effects};
        RunRules Run(params ItemEntry[] items)
        {
            var run=new RunRules(31);run.Configure(Catalog);run.Start();
            foreach(var item in items)run.GrantItem(item);
            return run;
        }
        static void Shop(RunRules run)
        {run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);Assert.AreEqual(RunPhase.Shop,run.Phase);}
        [Test] public void FreeRerollsApplyStatEffectsAndConsumeOnlyOneTierToken()
        {
            var token=Item("tier-token",null,new ConditionalEffect{customKey="increase_tier_on_reroll",key="item_tier-token",value=1});
            var dice=Item("dice",new[]{new StatEffect{key="free_rerolls",value=2}},new ConditionalEffect{customKey="gain_stats_on_reroll",key="stat_luck",value=3,chance=100});
            var run=Run(token,token,dice);Shop(run);
            var locked=run.Shop[0];run.ToggleLock(0);int price=run.Price(locked);
            run.Reroll();
            Assert.AreEqual(3,run.Stat("luck"));Assert.AreEqual(1,run.Items.Count(i=>i.id==token.id));
            Assert.AreSame(locked,run.Shop[0]);Assert.AreEqual(price,run.Price(locked));
            Assert.IsTrue(run.Shop.Where(o=>o!=null&&!o.locked).All(o=>o.Tier>=2));
            Assert.AreEqual(1,run.ShopRerollCount);Assert.AreEqual(0,run.RerollPrice);
        }
        [Test] public void AllLockedRerollCannotConsumeTokensOrGrantStats()
        {
            var token=Item("tier-token",null,new ConditionalEffect{customKey="increase_tier_on_reroll",key="item_tier-token",value=1});
            var run=Run(token);Shop(run);for(int i=0;i<4;i++)run.ToggleLock(i);run.Reroll();
            Assert.Contains(token,run.Items);Assert.AreEqual(0,run.ShopRerollCount);
        }
        [Test] public void PaidRerollCanLoseStatsAndZeroChanceNeverApplies()
        {
            // base_shop.gd applies each chance/stat entry on a paid reroll too,
            // including negative gains. Merely entering the shop does not roll it.
            var dice=Item("dice",null,
                new ConditionalEffect{customKey="gain_stats_on_reroll",key="stat_luck",value=-3,chance=100},
                new ConditionalEffect{customKey="gain_stats_on_reroll",key="stat_luck",value=100,chance=0});
            var run=Run(dice);run.Collect(100);Shop(run);Assert.AreEqual(0,run.Stat("luck"));
            int balance=run.Scrap,cost=run.RerollPrice;Assert.Greater(cost,0);run.Reroll();
            Assert.AreEqual(balance-cost,run.Scrap);Assert.AreEqual(-3,run.Stat("luck"));Assert.AreEqual(1,run.ShopRerollCount);
        }
        [Test] public void TierTokenConsumptionMatchesItsEffectValueAndWeaponPricesUseWeaponIdentity()
        {
            var spent=Item("token");
            var active=Item("token",new[]{new StatEffect{key="free_rerolls",value=1}},new ConditionalEffect{customKey="increase_tier_on_reroll",key="item_token",value=2});
            var discount=Item("discount",null,new ConditionalEffect{customKey="specific_items_price",key="weapon_pistol_1",value=-50});
            var run=Run(spent,active,discount);Shop(run);
            var pistol=new ShopOffer{weapon=Catalog.Weapon("pistol",1),rolledWave=1};
            Assert.AreEqual((int)((pistol.Value+1+pistol.Value*.1f)*.5f),run.Price(pistol));
            run.Reroll();Assert.Contains(spent,run.Items);Assert.IsFalse(run.Items.Contains(active));
        }
        [Test] public void DuplicatePurchaseConsumesSourceCopiesAndRespectsTargetCap()
        {
            var source=Item("duplicator",new[]{new StatEffect{key="armor",value=2}},new ConditionalEffect{customKey="duplicate_item",key="item_duplicator",value=1});
            var target=Item("target",new[]{new StatEffect{key="luck",value=4}});target.maxCount=2;
            var run=Run(source,source);run.Collect(100);Shop(run);
            run.Shop[0]=new ShopOffer{item=target,rolledWave=run.Wave};int balance=run.Scrap,cost=run.Price(run.Shop[0]);
            Assert.IsTrue(run.Buy(0));Assert.AreEqual(balance-cost,run.Scrap);
            Assert.AreEqual(2,run.Items.Count(i=>i.id==target.id));Assert.AreEqual(1,run.Items.Count(i=>i.id==source.id));
            Assert.AreEqual(8,run.Stat("luck"));Assert.AreEqual(2,run.Stat("armor"));
        }
        [TestCase(2,1,2,2)]
        [TestCase(-1,0,3,0)]
        public void DuplicateSourcesAreConsumedOnlyWhenExtraTargetCopiesFit(int cap,int owned,int expectedTargets,int expectedSources)
        {
            var source=Item("duplicator",null,new ConditionalEffect{customKey="duplicate_item",key="item_duplicator",value=1});
            var target=Item("target");target.maxCount=cap;
            var run=Run(source,source);for(int i=0;i<owned;i++)run.GrantItem(target);
            run.Collect(100);Shop(run);run.Shop[0]=new ShopOffer{item=target,rolledWave=run.Wave};
            Assert.IsTrue(run.Buy(0));
            Assert.AreEqual(expectedTargets,run.Items.Count(i=>i.id==target.id));
            Assert.AreEqual(expectedSources,run.Items.Count(i=>i.id==source.id));
        }
        [Test] public void StealingSpendsOneChargeAndQueuesNextWaveConsequencesWithoutPayment()
        {
            var thief=Item("thief",new[]{new StatEffect{key="item_steals",value=2},new StatEffect{key="item_steals_spawns_random_elite",value=1}},
                new ConditionalEffect{customKey="item_steals_spawns_enemy",key="res://zones/common/bait/group_bait.tres",value=100});
            var run=Run(thief);Shop(run);var target=Item("stolen",new[]{new StatEffect{key="luck",value=5}});
            run.Shop[0]=new ShopOffer{item=target,rolledWave=1};int gold=run.Scrap;
            Assert.IsFalse(run.CanBuy(0));Assert.IsTrue(run.CanSteal(0));Assert.IsTrue(run.Steal(0));
            Assert.AreEqual(gold,run.Scrap);Assert.AreEqual(1,run.StealsRemaining);Assert.AreEqual(5,run.Stat("luck"));
            CollectionAssert.AreEqual(new[]{"res://zones/common/bait/group_bait.tres"},run.TakeShopSpawnGroups());
            Assert.IsEmpty(run.TakeShopSpawnGroups());Assert.That(run.StealCaughtChance(new ShopOffer{item=target}),Is.EqualTo(.95f));
        }
        [Test] public void CatchChanceUsesDisplayedPriceAndSourceZeroBasedTierConversion()
        {
            var run=Run();Shop(run);var target=Item("risk");target.value=100;target.tier=3;
            var offer=new ShopOffer{item=target,rolledWave=1};
            Assert.AreEqual(.1f*(run.Price(offer)/300f)*.75f,run.StealCaughtChance(offer),.000001f);
        }
        [Test] public void LockingOnlyDisqualifiesCandyBagWhenOfferSurvivesShopDeparture()
        {
            var run=Run();Shop(run);run.ToggleLock(0);run.ToggleLock(0);run.NextWave();Assert.IsFalse(run.UsedLocks);
            Shop(run);run.ToggleLock(0);run.NextWave();Assert.IsTrue(run.UsedLocks);
        }
        [Test] public void RemovingCapItemRestoresUncappedStatsAndDoesNotLeaveScalarBonuses()
        {
            var cap=Item("cap",new[]{new StatEffect{key="armor",value=3}},new ConditionalEffect{customKey="stat_cap",key="hp_cap"});
            var run=Run(cap);run.GrantItem(Item("growth",new[]{new StatEffect{key="max_hp",value=10}}));Assert.AreEqual(10,run.MaxHealth);
            Assert.IsTrue(run.RemoveItem(cap));Assert.AreEqual(20,run.MaxHealth);Assert.AreEqual(0,run.Stat("armor"));Assert.IsFalse(run.RemoveItem(cap));
        }
        [Test] public void ShopSpawnRegistryContainsActualSourceEliteGroup()
        {
            var data=JsonUtility.FromJson<ShopSpawnCatalog>(Resources.Load<TextAsset>("shop-effect-spawns").text);
            var entry=data.groups.Single(g=>g.path=="zones/common/elite/group_elite.tres");
            Assert.IsTrue(entry.group.boss);Assert.IsEmpty(entry.group.units);
            var actual=ShopSpawnCatalog.EliteGroup("croc");
            Assert.AreEqual(1,actual.start);Assert.IsTrue(actual.boss);Assert.IsTrue(actual.edge);
            Assert.AreEqual("croc",actual.units[0].id);Assert.AreEqual(1,actual.units[0].min);
        }
    }
}
