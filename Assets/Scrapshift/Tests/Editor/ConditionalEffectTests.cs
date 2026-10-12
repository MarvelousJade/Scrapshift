using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class ConditionalEffectTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(42);r.Configure(catalog);r.Start();return r;}
        void Equip(RunRules r,string id,int tier){r.Inventory.Clear();r.Inventory.Add(catalog.Weapon(id,tier));}
        void Install(RunRules r,string id)
        {
            if(r.Phase==RunPhase.Playing){r.Collect(1000);r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);}
            r.Shop[0]=new ShopOffer{item=catalog.data.items.Single(i=>i.id==id),rolledWave=r.Wave};Assert.IsTrue(r.Buy(0),id);
        }
        [Test] public void LanceStandingPenaltyFollowsInputAndIsVisibleDuringPause()
        {
            var r=Run();Equip(r,"jousting_lance",1);
            Assert.AreEqual(-10,r.Stat("percent_damage"));Assert.AreEqual(2,r.Stat("speed"));
            r.SetMoving(true);Assert.AreEqual(0,r.Stat("percent_damage"));r.Pause();Assert.AreEqual(0,r.Stat("percent_damage"));
            r.Pause();r.SetMoving(false);r.Pause();Assert.AreEqual(-10,r.Stat("percent_damage"));
        }
        [Test] public void StandingImplantsApplyOnlyDuringWaveAndStopImmediately()
        {
            var r=Run();Install(r,"barricade");float armor=r.Stat("armor");r.NextWave();Assert.AreEqual(armor+8,r.Stat("armor"));
            r.SetMoving(true);Assert.AreEqual(armor,r.Stat("armor"));r.SetMoving(false);r.Tick(r.Remaining);Assert.AreEqual(armor,r.Stat("armor"));
        }
        [Test] public void UniqueWeaponsIgnoreTierAndUpdateWhenInventoryChanges()
        {
            var r=Run();Install(r,"focus");float speed=r.Stat("attack_speed");
            r.Inventory.Add(catalog.Weapon("pistol",2));Assert.AreEqual(speed,r.Stat("attack_speed"));
            r.Inventory.Add(catalog.Weapon("fist",1));Assert.AreEqual(speed-3,r.Stat("attack_speed"));
            r.Inventory.RemoveAt(2);Assert.AreEqual(speed,r.Stat("attack_speed"));
        }
        [Test] public void ExcaliburCountsItselfAndPreservesSourceUnlock()
        {
            var r=Run();Equip(r,"excalibur",4);Assert.AreEqual(-2,r.Stat("armor"));
            r.Inventory.Add(catalog.Weapon("pistol",1));Assert.AreEqual(-4,r.Stat("armor"));
            Assert.IsTrue(r.Inventory[0].implemented);Assert.IsFalse(r.Inventory[0].available);
        }
        [Test] public void DrillGainsAtFiveSecondsAndResetsEachWave()
        {
            var r=Run();Equip(r,"drill",4);float start=r.Stat("attack_speed");
            r.Tick(4.99f);Assert.AreEqual(start,r.Stat("attack_speed"));r.Tick(.02f);Assert.AreEqual(start+10,r.Stat("attack_speed"));
            r.Pause();r.Tick(10);Assert.AreEqual(start+10,r.Stat("attack_speed"));r.Pause();r.Hurt(1);Assert.AreEqual(start+10,r.Stat("attack_speed"));
            r.Tick(r.Remaining);Assert.AreEqual(start,r.Stat("attack_speed"));r.NextWave();r.Tick(5);Assert.AreEqual(start+10,r.Stat("attack_speed"));
        }
        [Test] public void ScytheSelfDamageBypassesArmorTriggersGrowthAndCanKillAtWaveEnd()
        {
            var r=Run();Equip(r,"scythe",4);r.GainWeaponStat("armor",1000);r.GainWeaponStat("dodge",100);
            float damage=r.Stat("percent_damage");r.Tick(.9f);Assert.AreEqual(10,r.Health);r.Tick(.2f);
            Assert.AreEqual(7,r.Health);Assert.AreEqual(damage+3,r.Stat("percent_damage"));
            r.Tick(r.Remaining);Assert.AreEqual(RunPhase.Lost,r.Phase);Assert.AreEqual(0,r.Health);
        }
        [Test] public void OnHitPenaltyIsTemporaryAndNotAnImmediatePurchasePenalty()
        {
            var r=Run();Install(r,"triangle_of_power");float damage=r.Stat("percent_damage");r.NextWave();r.Hurt(1);
            Assert.AreEqual(damage-2,r.Stat("percent_damage"));r.Hurt(1);Assert.AreEqual(damage-4,r.Stat("percent_damage"));
            r.Tick(r.Remaining);Assert.AreEqual(damage,r.Stat("percent_damage"));
        }
        [Test] public void WaveAndLevelGainsArePermanentAndDoNotFireOnPurchase()
        {
            var r=Run();Install(r,"vigilante_ring");Install(r,"decomposing_flesh");
            float damage=r.Stat("percent_damage"),hp=r.MaxHealth,life=r.Stat("lifesteal");r.NextWave();
            r.Collect(r.NextLevel-r.XP);Assert.AreEqual(hp,r.MaxHealth);Assert.AreEqual(life+1,r.Stat("lifesteal"));
            r.Tick(r.Remaining);Assert.AreEqual(damage+3,r.Stat("percent_damage"));
            while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.NextWave();Assert.AreEqual(life+1,r.Stat("lifesteal"));
        }
        [Test] public void ResetOnHitClearsOnlyAccruedTimedBonusAndKeepsGlobalClock()
        {
            var r=Run();r.Items.Add(new ItemEntry{effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{new ConditionalEffect{key="stat_armor",value=2,interval=5,customKey="temp_stats_per_interval",resetOnHit=true}}});
            r.Tick(5);Assert.AreEqual(2,r.Stat("armor"));r.Tick(2);r.Hurt(1);Assert.AreEqual(0,r.Stat("armor"));
            r.Tick(3);Assert.AreEqual(2,r.Stat("armor"));
        }
        [Test] public void SelfDamageAndRegenAreFramePartitionIndependent()
        {
            var coarse=Run();var fine=Run();foreach(var r in new[]{coarse,fine}){Equip(r,"scythe",4);r.GainWeaponStat("hp_regeneration",20);}
            coarse.Tick(3);for(int i=0;i<300;i++)fine.Tick(.01f);
            Assert.AreEqual(coarse.Health,fine.Health,.01);Assert.AreEqual(coarse.Stat("percent_damage"),fine.Stat("percent_damage"));
        }
        [Test] public void InspectionShowsTriggerAndUsesImplantNames()
        {
            var t=new InspectionText(catalog.uiText);var r=Run();
            StringAssert.Contains("every 5 seconds",t.WeaponDescription(r,catalog.Weapon("drill",4)));
            StringAssert.Contains("Lose 3 HP",t.WeaponDescription(r,catalog.Weapon("scythe",4)));
            var item=catalog.data.items.Single(i=>i.id=="robot_arm");Assert.AreEqual("Growing Servo Arm",t.ItemName(item));
            StringAssert.Contains("end of every wave",t.ItemDescription(item));StringAssert.Contains("-1",t.ItemDescription(item));
        }
    }
}
