using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class MaxHealthScalingParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var run=new RunRules(81);run.Configure(catalog);run.Start();return run;}
        void Cap(RunRules run){run.GrantItem(catalog.data.items.Single(i=>i.id=="handcuffs"));run.GainWeaponStat("max_hp",100);}
        [Test] public void ChopperUsesUncappedHealthStatInsteadOfHandcuffsMaximum()
        {
            var run=Run();Cap(run);run.GainWeaponStat("melee_damage",-run.Stat("melee_damage"));
            Assert.AreEqual(10,run.MaxHealth);Assert.AreEqual(110,run.Stat("max_hp"));
            Assert.AreEqual(22,run.WeaponDamage(catalog.Weapon("chopper",1)),"Source base 6 + 15% of 110, truncated once.");
        }
        [Test] public void HealthScalingKeepsFractionalStatUntilTheWholeBaseDamageIsTruncated()
        {
            var run=Run();run.GainWeaponStat("max_hp",.9f);
            var weapon=new Equipment{damage=1,scaling=new[]{new StatEffect{key="max_hp",value=2}}};
            Assert.AreEqual(10,run.MaxHealth);Assert.AreEqual(22,run.WeaponDamage(weapon));
        }
        [Test] public void AlienEyesUseUncappedHealthButDoNotAcquireNailOrFrozenHeartScaling()
        {
            var run=Run();Cap(run);
            foreach(string id in new[]{"alien_eyes","nail","frozen_heart"})run.Items.Add(catalog.data.items.Single(i=>i.id==id));
            run.GainWeaponStat("engineering",100);run.GainWeaponStat("elemental_damage",100);
            var eyes=run.Effects("alien_eyes").Single().projectile.AsEquipment();
            Assert.AreEqual(56,run.WeaponDamage(eyes),"1 + half of uncapped 110; special spawn excludes extra weapon scaling.");
        }
        [Test] public void ItemExplosionsUseUncappedHealthAndStillAcquireBothAddedScalingEffects()
        {
            var run=Run();Cap(run);
            foreach(string id in new[]{"nail","frozen_heart"})run.Items.Add(catalog.data.items.Single(i=>i.id==id));
            run.GainWeaponStat("engineering",100);run.GainWeaponStat("elemental_damage",100);
            run.Items.Add(new ItemEntry{id="hp-scaling-explosion",effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{new ConditionalEffect{key="",customKey="explode_on_hit",chance=1,projectile=new ProjectileStats{damage=1,scaling=new[]{new StatEffect{key="max_hp",value=.5f}}}}}});
            Assert.IsTrue(run.TryItemExplosion("explode_on_hit",0,out _,out float damage));
            Assert.AreEqual(86,damage,"1 + 55 HP scaling + 20 Nail + 10 Frozen Heart.");
        }
        [TestCase(0,14)]
        [TestCase(4,10)]
        public void GenericPlayerBoostHelperRetainsExplicitHpAdditionAndDirtyMaximumRefreshWithoutClamping(int damage,int expected)
        {
            var run=Run();if(damage>0)run.Hurt(damage);run.GainWeaponStat("no_heal",1);
            run.EnemyBoost(2,20);Assert.AreEqual(12,run.MaxHealth);Assert.AreEqual(expected,run.Health);
            run.EnemyBoost(-2,-20);Assert.AreEqual(10,run.MaxHealth);Assert.AreEqual(10,run.Health);
        }
    }
}
