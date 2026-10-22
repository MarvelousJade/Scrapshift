using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class LootSurvivalTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var run=new RunRules(123);run.Configure(catalog);run.Start();return run;}
        void GrantStat(RunRules run,string stat,float value)
        {
            run.Collect(1000);run.Tick(run.Remaining);
            while(run.Phase==RunPhase.Upgrade)run.Choose(Array.FindIndex(run.Offers,o=>o.Stat!="hp_regeneration"&&o.Stat!="armor"));
            run.Shop[0]=new ShopOffer {item=new ItemEntry {id="fixture",value=1,effects=new[]{new StatEffect {key=stat,value=value}}}};
            Assert.IsTrue(run.Buy(0));run.NextWave();
        }

        [TestCase("tree",0,0,.19,ConsumableKind.Crate)]
        [TestCase("tree",0,1,.19,ConsumableKind.Fruit)]
        [TestCase("tree",100,1,.19,ConsumableKind.Crate)]
        [TestCase("looter",-100,100,.99,ConsumableKind.Crate)]
        [TestCase("predator",-100,100,.99,ConsumableKind.LegendaryCrate)]
        [TestCase("junkie",500,0,0,ConsumableKind.None)]
        [TestCase("evil_mob",500,0,0,ConsumableKind.None)]
        public void DropsRespectSourceChanceLuckDilutionAndGuarantees(string id,float luck,int spawned,double roll,ConsumableKind expected)
            =>Assert.AreEqual(expected,ReferenceLoot.Roll(id,luck,spawned,0,roll));

        [Test] public void OrdinaryDropHasTwoSeparateRolls()
        {
            Assert.AreEqual(ConsumableKind.None,ReferenceLoot.Roll("baby_alien",0,0,.011,0));
            Assert.AreEqual(ConsumableKind.Fruit,ReferenceLoot.Roll("baby_alien",0,0,.009,.011));
            Assert.AreEqual(ConsumableKind.Crate,ReferenceLoot.Roll("baby_alien",0,0,.009,.009));
            Assert.AreEqual(ConsumableKind.Crate,ReferenceLoot.Roll("spitter",0,0,.029,.099));
            Assert.AreEqual(ConsumableKind.None,ReferenceLoot.Roll("tree",-100,0,0,0));
        }
        [Test] public void CrateChanceModifierAppliesAfterLuckAndWaveCrateDilution()
        {
            // tree: 100% consumable drop, 20% base crate chance (tree_stats.tres).
            // main.gd: itemChance = .2 * (1 + luck / 100) / (1 + crates);
            // itemChance += itemChance * crateChance / 100.
            Assert.AreEqual(ConsumableKind.Crate,ReferenceLoot.Roll("tree",0,1,0,.19,100));
            Assert.AreEqual(ConsumableKind.Fruit,ReferenceLoot.Roll("tree",0,1,0,.201,100));
            Assert.AreEqual(ConsumableKind.Fruit,ReferenceLoot.Roll("tree",0,1,0,.001,-100));
            Assert.AreEqual(ConsumableKind.Crate,ReferenceLoot.Roll("tree",50,2,0,.14,50));
            Assert.AreEqual(ConsumableKind.Fruit,ReferenceLoot.Roll("tree",50,2,0,.151,50));
        }
        [Test] public void ReferenceEliteBossInheritanceMakesBothCratesLegendaryDuringNormalRun()
        {
            Assert.AreEqual(ConsumableKind.LegendaryCrate,ReferenceLoot.Roll("croc",0,0,.99,.99,0,"Elite",11));
            Assert.AreEqual(ConsumableKind.LegendaryCrate,ReferenceLoot.Roll("predator",0,0,.99,.99,0,"Boss",20));
        }
        [Test] public void CratesHealImmediatelyAndResolveBeforeLevelsThenShop()
        {
            var r=Run();r.Hurt(5);r.CollectConsumable(ConsumableKind.Crate);
            Assert.AreEqual(8,r.Health);Assert.AreEqual(1,r.PendingCrates);Assert.AreEqual(RunPhase.Playing,r.Phase);
            r.CollectConsumable(ConsumableKind.Crate);Assert.AreEqual(10,r.Health);
            r.Collect(16);r.Tick(r.Remaining);Assert.AreEqual(RunPhase.Loot,r.Phase);
            var item=r.CrateItem;Assert.IsTrue(RunRules.IsShopItem(item.id));Assert.AreEqual(1,item.tier);
            int money=r.Scrap;r.Reroll();r.Choose(0);r.NextWave();
            Assert.AreSame(item,r.CrateItem);Assert.AreEqual(money,r.Scrap);
            Assert.IsTrue(r.ResolveCrate(true));Assert.AreSame(item,r.Items[0]);Assert.AreEqual(1,r.PendingCrates);
            int recycle=r.CrateRecycleValue;money=r.Scrap;
            Assert.IsTrue(r.ResolveCrate(false));Assert.AreEqual(money+recycle,r.Scrap);
            Assert.AreEqual(1,r.Items.Count);Assert.AreEqual(RunPhase.Upgrade,r.Phase);
            Assert.IsFalse(r.ResolveCrate(true));r.Choose(0);Assert.AreEqual(RunPhase.Shop,r.Phase);
        }
        [Test] public void LegendaryCrateUsesItsOwnHealingEffect()
        {
            var ordinary=Run();ordinary.Hurt(9);ordinary.CollectConsumable(ConsumableKind.Crate);
            Assert.AreEqual(4,ordinary.Health);
            var legendary=Run();legendary.Hurt(9);legendary.CollectConsumable(ConsumableKind.LegendaryCrate);
            Assert.AreEqual(legendary.MaxHealth,legendary.Health);
            Assert.AreEqual(1,legendary.PendingCrates);
        }
        [Test] public void TakingItemAppliesEveryEffectExactlyOnce()
        {
            var r=Run();r.CollectConsumable(ConsumableKind.Crate);r.Tick(r.Remaining);
            var item=r.CrateItem;float[] before=Array.ConvertAll(item.effects,e=>r.Stat(e.key.Replace("stat_","")));
            Assert.IsTrue(r.ResolveCrate(true));Assert.IsFalse(r.ResolveCrate(true));
            for(int i=0;i<before.Length;i++)Assert.AreEqual(before[i]+item.effects[i].value,r.Stat(item.effects[i].key.Replace("stat_","")));
        }
        [Test] public void LegendaryCrateDrawsOnlyEligibleLegendaryItems()
        {
            for(int seed=0;seed<32;seed++)
            {
                var r=new RunRules(seed);r.Configure(catalog);r.Start();
                r.CollectConsumable(ConsumableKind.LegendaryCrate);r.Tick(r.Remaining);
                Assert.AreEqual(RunPhase.Loot,r.Phase);
                Assert.AreEqual(4,r.CrateItem.tier);
                Assert.AreNotEqual("broken_hourglass",r.CrateItem.id);
            }
        }
        [Test] public void RestartAndLossCannotLeakCrateRewards()
        {
            var r=Run();r.CollectConsumable(ConsumableKind.Crate);r.Pause();r.CollectConsumable(ConsumableKind.Crate);
            Assert.AreEqual(1,r.PendingCrates);r.Pause();r.Hurt(1000);Assert.IsFalse(r.ResolveCrate(true));
            r.Start();Assert.AreEqual(0,r.PendingCrates);Assert.IsNull(r.CrateItem);Assert.AreEqual(0,r.Items.Count);
        }
        [TestCase(1,5)] [TestCase(10,1)] [TestCase(19,5.0/9)]
        public void RegenerationUsesSourceInterval(float stat,double interval)=>Assert.AreEqual(interval,RunRules.RegenerationInterval(stat),1e-6);

        [Test] public void RegenerationTicksInWholeHealthPausesAndRetainsProgressAcrossExternalHealing()
        {
            var r=Run();GrantStat(r,"hp_regeneration",1);r.Hurt(4);float health=r.Health;
            r.Tick(4.9f);Assert.AreEqual(health,r.Health);
            r.Pause();r.Tick(20);Assert.AreEqual(health,r.Health);r.Pause();
            r.Tick(.1f);Assert.AreEqual(health+1,r.Health);
            r.Tick(2);r.Heal(100);r.Hurt(2);health=r.Health;
            r.Tick(2.9f);Assert.AreEqual(health,r.Health);r.Tick(.1f);Assert.AreEqual(health+1,r.Health);
        }
        [Test] public void NonpositiveRegenDoesNotHealOrDamage()
        {
            var r=Run();GrantStat(r,"hp_regeneration",-5);r.Hurt(3);float health=r.Health;
            r.Tick(6);Assert.AreEqual(health,r.Health);Assert.AreEqual(99,RunRules.RegenerationInterval(0));
        }
        [Test] public void LifeStealSharesCooldownAcrossHitsAndPauses()
        {
            var r=Run();GrantStat(r,"lifesteal",100);r.Hurt(6);float health=r.Health;
            Assert.IsTrue(r.TryLifeSteal(0));Assert.AreEqual(health+1,r.Health);
            for(int i=0;i<6;i++)Assert.IsFalse(r.TryLifeSteal(0));
            r.Tick(.099f);Assert.IsFalse(r.TryLifeSteal(0));
            r.Pause();r.Tick(1);Assert.IsFalse(r.TryLifeSteal(0));r.Pause();
            r.Tick(.001f);Assert.IsTrue(r.TryLifeSteal(0));Assert.AreEqual(health+2,r.Health);
        }
        [Test] public void FullHealthConsumablesDoNotMagnetizeUntilHurt()
        {
            var p=new ScrapMotion {position=Vector2.right*.8f};
            p.Step(.2f,Vector2.zero,-1,true);Assert.IsFalse(p.magnetized);
            p.Step(.02f,Vector2.zero,1.3f,true);Assert.IsTrue(p.magnetized);
        }
    }
}
