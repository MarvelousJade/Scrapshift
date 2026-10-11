using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class ReferenceRulesTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()
        {
            catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            Assert.NotNull(catalog);
        }
        RunRules Run(int seed=42)
        {
            var r=new RunRules(seed);
            r.Configure(catalog);
            r.Start();
            return r;
        }
        void Shop(RunRules r,int money=0)
        {
            r.Collect(money);
            r.Tick(r.Remaining);
            while(r.Phase==RunPhase.Upgrade)r.Choose(0);
            Assert.AreEqual(RunPhase.Shop,r.Phase);
        }
        [Test] public void TwentyWavesUseReferenceDurationsAndShopDoesNotAdvanceTime()
        {
            var r=Run();
            for(int i=0;i<20;i++)
            {
                Assert.AreEqual(RunRules.Durations[i],r.Remaining);
                r.Tick(r.Remaining-.1f);
                Assert.AreEqual(RunPhase.Playing,r.Phase);
                r.Tick(.2f);
                if(i<19)
                {
                    Assert.AreEqual(RunPhase.Shop,r.Phase);
                    r.Tick(100);
                    Assert.AreEqual(i+1,r.Wave);
                    r.NextWave();
                }
            }
            Assert.AreEqual(RunPhase.Won,r.Phase);
        }
        [Test] public void XpQueuesMultipleRewardsWithoutInterruptingCombat()
        {
            var r=Run();
            r.Tick(3);
            r.Collect(62);
            Assert.AreEqual(2,r.Level);
            Assert.AreEqual(21,r.XP);
            Assert.AreEqual(62,r.Scrap);
            Assert.AreEqual(2,r.PendingLevels);
            Assert.AreEqual(RunPhase.Playing,r.Phase);
            Assert.AreEqual(17,r.Remaining);
            r.Tick(17);
            Assert.AreEqual(RunPhase.Upgrade,r.Phase);
            r.Choose(0);
            Assert.AreEqual(RunPhase.Upgrade,r.Phase);
            r.Choose(0);
            Assert.AreEqual(RunPhase.Shop,r.Phase);
            Assert.AreEqual(1,r.Wave);
        }
        [Test] public void StatDraftHasFourDistinctChoicesAndAppliesDisplayedAmount()
        {
            var r=Run();
            r.Collect(25);
            r.Tick(20);
            Assert.AreEqual(4,r.Offers.Select(o=>o.Kind).Distinct().Count());
            var o=r.Offers[0];
            float before=r.Stat(o.Stat);
            r.Choose(0);
            Assert.AreEqual(before+o.Amount,r.Stat(o.Stat));
            r.Choose(0);
            Assert.AreEqual(1,r.UpgradeCount);
        }
        [Test] public void EarlyShopsGuaranteeTwoWeapons()
        {
            for(int seed=0;seed<50;seed++)
            {
                var r=Run(seed);
                Shop(r);
                Assert.AreEqual(2,r.Shop.Count(s=>s.weapon!=null));
                r.NextWave();
                Shop(r);
                Assert.AreEqual(2,r.Shop.Count(s=>s.weapon!=null));
            }
        }
        [Test] public void RerollCostsEscalateAndCannotOverspend()
        {
            var r=Run();
            Shop(r,15);
            int before=r.Scrap,cost=r.RerollPrice;
            r.Reroll();
            Assert.AreEqual(before-cost,r.Scrap);
            Assert.Greater(r.RerollPrice,cost);
            while(r.CanReroll)r.Reroll();
            before=r.Scrap;
            var offers=r.Shop.ToArray();
            r.Reroll();
            Assert.AreEqual(before,r.Scrap);
            CollectionAssert.AreEqual(offers,r.Shop);
        }
        [Test] public void LockedOfferSurvivesRerollAndNextShop()
        {
            var r=Run();
            Shop(r,40);
            r.ToggleLock(0);
            var offer=r.Shop[0];
            r.Reroll();
            Assert.AreSame(offer,r.Shop[0]);
            r.NextWave();
            Shop(r);
            Assert.AreSame(offer,r.Shop[0]);
            Assert.IsTrue(offer.locked);
        }
        [Test] public void PurchasingConsumesCurrencyOnceAndRemovesOffer()
        {
            var r=Run();
            Shop(r,100);
            var offer=r.Shop[0];
            int money=r.Scrap,price=r.Price(offer.Value);
            Assert.IsTrue(r.Buy(0));
            Assert.AreEqual(money-price,r.Scrap);
            Assert.IsNull(r.Shop[0]);
            Assert.IsFalse(r.Buy(0));
            Assert.AreEqual(2,r.Inventory.Count);
        }
        [Test] public void FullInventoryRejectsNonMatchingWeaponButCombinesMatchingTier()
        {
            var r=Run();
            Shop(r,200);
            var w=catalog.Weapon("pistol",1);
            r.Inventory.Clear();
            for(int i=0;i<6;i++)r.Inventory.Add(w);
            r.Shop[0]=new ShopOffer
            {
                weapon=catalog.Weapon("smg",1)
            };
            int money=r.Scrap;
            Assert.IsFalse(r.Buy(0));
            Assert.AreEqual(money,r.Scrap);
            r.Shop[0]=new ShopOffer
            {
                weapon=w
            };
            Assert.IsTrue(r.Buy(0));
            Assert.AreEqual(6,r.Inventory.Count);
            Assert.IsTrue(r.Inventory.Any(x=>x.id=="pistol"&&x.tier==2));
        }
        [Test] public void CombiningDuplicateCatalogReferencesConsumesExactlyTwoSlots()
        {
            var r=Run();
            Shop(r);
            r.Inventory.Add(catalog.Weapon("pistol",1));
            Assert.IsTrue(r.CanCombine(0));
            r.Combine(0);
            Assert.AreEqual(1,r.Inventory.Count);
            Assert.AreEqual(2,r.Inventory[0].tier);
            Assert.IsFalse(r.CanCombine(0));
        }
        [Test] public void TierFourDoesNotCombine()
        {
            var r=Run();
            Shop(r);
            r.Inventory.Clear();
            r.Inventory.Add(catalog.Weapon("pistol",4));
            r.Inventory.Add(catalog.Weapon("pistol",4));
            Assert.IsFalse(r.CanCombine(0));
            r.Combine(0);
            Assert.AreEqual(2,r.Inventory.Count);
        }
        [Test] public void ItemAppliesEveryPositiveAndNegativeEffect()
        {
            var r=Run();
            Shop(r,100);
            var coffee=catalog.data.items.Single(i=>i.id=="coffee");
            r.Shop[0]=new ShopOffer
            {
                item=coffee
            };
            var before=coffee.effects.Select(e=>r.Stat(e.key.Replace("stat_",""))).ToArray();
            Assert.IsTrue(r.Buy(0));
            for(int i=0;i<coffee.effects.Length;i++)Assert.AreEqual(before[i]+coffee.effects[i].value,r.Stat(coffee.effects[i].key.Replace("stat_","")));
            Assert.AreEqual(1,r.Items.Count);
        }
        [Test] public void RecycleUsesWavePriceAndCannotRepeatInvalidSlot()
        {
            var r=Run();
            Shop(r);
            int value=r.Price(r.Inventory[0].value)/4;
            r.Recycle(0);
            Assert.AreEqual(value,r.Scrap);
            Assert.AreEqual(0,r.Inventory.Count);
            r.Recycle(0);
            Assert.AreEqual(value,r.Scrap);
        }
        [Test] public void BankedMaterialsDoubleFutureDropsWithoutBeingImmediatelySpendable()
        {
            var r=Run();
            r.BankMaterials(9);
            Assert.AreEqual(0,r.Scrap);
            r.Collect(4);
            Assert.AreEqual(8,r.Scrap);
            Assert.AreEqual(8,r.XP);
            Assert.AreEqual(5,r.Banked);
            r.Collect(10);
            Assert.AreEqual(23,r.Scrap);
            Assert.AreEqual(0,r.Banked);
        }
        [Test] public void PauseAndMenusRejectCombatMutation()
        {
            var r=Run();
            r.Pause();
            r.Tick(99);
            r.Hurt(200);
            r.Collect(100);
            Assert.AreEqual(20,r.Remaining);
            Assert.AreEqual(10,r.Health);
            Assert.AreEqual(0,r.Scrap);
            r.Pause();
            Shop(r);
            r.Hurt(99);
            r.Collect(100);
            Assert.AreEqual(10,r.Health);
            Assert.AreEqual(0,r.Scrap);
        }
        [Test] public void LethalDamageCannotBecomeWinAndRestartClearsInventoryEconomy()
        {
            var r=Run();
            r.Collect(70);
            r.Hurt(10000);
            r.Tick(1000);
            Assert.AreEqual(RunPhase.Lost,r.Phase);
            r.Start();
            Assert.AreEqual(10,r.Health);
            Assert.AreEqual(0,r.PendingLevels);
            Assert.AreEqual(0,r.Scrap);
            Assert.AreEqual(1,r.Inventory.Count);
        }
        [Test] public void BossCanOnlyEndFinalWave()
        {
            var r=Run();
            r.DefeatBoss();
            Assert.AreEqual(RunPhase.Playing,r.Phase);
            for(int i=1;i<20;i++)
            {
                Shop(r);
                r.NextWave();
            }
            r.DefeatBoss();
            Assert.AreEqual(RunPhase.Won,r.Phase);
        }
        [Test] public void UpgradeTiersUseReferenceValues()
        {
            var o=new UpgradeOffer(UpgradeKind.Damage,4,catalog.data.upgrades);
            Assert.AreEqual(16,o.Amount);
            Assert.NotNull(o.Image);
            Assert.AreEqual(9,new UpgradeOffer(UpgradeKind.Critical,4,catalog.data.upgrades).Amount);
        }
        [Test] public void NegativeInputsCannotReverseTimeOrGrantMaterials()
        {
            var r=Run();
            r.Hurt(-10);
            r.Tick(-20);
            r.Collect(-100);
            Assert.AreEqual(20,r.Remaining);
            Assert.AreEqual(10,r.Health);
            Assert.AreEqual(0,r.Scrap);
        }
        [Test] public void ReferenceMediaAndAllScheduledUnitIdsResolve()
        {
            foreach(var e in catalog.data.enemies)
            {
                Assert.NotNull(e.image,e.id);
                Assert.Greater(e.health,0,e.id);
            }
            foreach(var wave in catalog.data.waves)
            {
                Assert.AreEqual(100,wave.cap);
                foreach(var g in wave.groups)foreach(var u in g.units)Assert.NotNull(catalog.Enemy(u.id),u.id);
            }
            foreach(var w in catalog.data.equipment)
            {
                Assert.NotNull(w.image,w.id);
                Assert.IsNotEmpty(w.sounds,w.id);
            }
            foreach(var i in catalog.data.items)Assert.NotNull(i.image,i.id);
        }
        [Test] public void FirstWaveEmitsFourOrFiveAliensAtOneThenFourSeconds()
        {
            var d=new SpawnDirector(catalog.data.waves[0],1);
            var counts=new List<int>();
            d.Tick(.99f,(g,u,n)=>
            {
                if(u.id=="baby_alien")counts.Add(n);
            })
            ;Assert.IsEmpty(counts);d.Tick(.02f,(g,u,n)=>
            {
                if(u.id=="baby_alien")counts.Add(n);
            })
            ;Assert.AreEqual(1,counts.Count);Assert.That(counts[0],Is.InRange(4,5));d.Tick(3,(g,u,n)=>
            {
                if(u.id=="baby_alien")counts.Add(n);
            })
            ;Assert.AreEqual(2,counts.Count);
        }
        [Test] public void EveryWaveSchedulerIsStableAcrossFrameSizes()
        {
            foreach(var wave in catalog.data.waves)
            {
                int a=0,b=0;var d1=new SpawnDirector(wave,20);var d2=new SpawnDirector(wave,20);for(int i=0;i<(int)wave.duration*60;i++)d1.Tick(1f/60,(g,u,n)=>a+=n);d2.Tick(wave.duration,(g,u,n)=>b+=n);Assert.Greater(a,0);Assert.AreEqual(a,b,"schedule/RNG must be independent of frame sizes");
            }
        }
        [Test] public void InitialChanceMissDoesNotCancelScheduledRepeats()
        {
            var wave=new WaveEntry
            {
                groups=new[]
                {
                    new SpawnGroup
                    {
                        start=1,repeats=2,interval=3,minimum=1,chance=1,units=new[]
                        {
                            new UnitEntry
                            {
                                id="x",min=2,max=2,chance=1
                            }
                        }
                    },
                    new SpawnGroup
                    {
                        start=1,repeats=2,interval=1,minimum=1,chance=0,units=new[]
                        {
                            new UnitEntry
                            {
                                id="bad",min=100,max=100,chance=1
                            }
                        }
                    }
                }
            };
            int count=0;new SpawnDirector(wave,1).Tick(100,(g,u,n)=>count+=n);Assert.AreEqual(206,count);
        }
        [TestCase(1440,900)][TestCase(1920,1080)][TestCase(900,900)]public void ResizePreservesArena(int w,int h)
        {
            var r=PresentationLayout.Viewport(w,h);Assert.AreEqual(PresentationLayout.Aspect,w*r.width/(h*r.height),.0001f);
        }
        [Test]public void SweptProjectileCannotTunnel()
        {
            Assert.AreEqual(0,ArenaGame.SegmentDistance(Vector2.zero,Vector2.left,Vector2.right));Assert.AreEqual(1,ArenaGame.SegmentDistance(Vector2.up,Vector2.zero,Vector2.zero));
        }
    }
}
