using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemNextParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(string character=null)
        {
            var run=new RunRules(42);run.Configure(catalog);
            var profile=catalog.data.characters.FirstOrDefault(c=>c.id==character);
            run.Start(profile,profile==null?null:StarterChoices.Available(catalog,profile).FirstOrDefault());
            return run;
        }
        ItemEntry Item(string id)=>catalog.data.items.Single(i=>i.id==id);
        void OpenShop(RunRules run)
        {run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);Assert.AreEqual(RunPhase.Shop,run.Phase);}

        [TestCase(5,11)]
        [TestCase(25,13)]
        [TestCase(-15,9)]
        [TestCase(-55,5)]
        [TestCase(-100,0)]
        public void LichHealingDamageRoundsHalfAwayFromZeroAfterBaseTruncation(float percentDamage,float expected)
        {
            // main.handle_stat_damages: floor(max(1, HP * percent)), then
            // Godot round after the ordinary damage modifier; emit one request.
            var run=Run("lich");run.Inventory.Clear();
            // Lich receives only half of ordinary Damage modifiers. These
            // cases name the effective percentage used by handle_stat_damages.
            run.GainWeaponStat("percent_damage",percentDamage/run.Character.Gain("percent_damage"));
            Assert.AreEqual(percentDamage,run.Stat("percent_damage"),.0001f);
            Assert.AreEqual(10,run.MaxHealth);
            float received=0;int requests=0;
            run.RandomDamageRequested+=value=>{received=value;requests++;};
            run.Hurt(1);run.Heal(1);
            Assert.AreEqual(expected,received);
            Assert.AreEqual(expected>0?1:0,requests);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void CyberballCopiesRoundEachSuccessfulEntryBeforeAggregating(int copies)
        {
            var run=Run();run.GainWeaponStat("luck",8);run.GainWeaponStat("percent_damage",25);
            for(int i=0;i<copies;i++)run.Items.Add(Item("cyberball"));
            var received=new List<float>();run.RandomDamageRequested+=received.Add;
            for(int i=0;i<64;i++)run.KillEffects(false);
            Assert.IsNotEmpty(received,"The fixed seed must exercise successful actual 25% item rolls.");
            Assert.IsTrue(received.All(value=>value==3||copies==2&&value==6),
                "Each proc is round(floor(8 * .25) * 1.25) = 3; successful entries share one request.");
        }

        [Test] public void AnvilRemovesOneWeaponAndAppendsUpgradeAfterUnchangedWeapons()
        {
            var run=Run();run.Inventory.Clear();
            run.Inventory.Add(catalog.Weapon("pistol",1));
            run.Inventory.Add(catalog.Weapon("knife",4));
            run.Inventory.Add(catalog.Weapon("smg",4));
            run.Items.Add(Item("anvil"));
            OpenShop(run);
            CollectionAssert.AreEqual(new[]{"knife:4","smg:4","pistol:2"},run.Inventory.Select(w=>w.id+":"+w.tier));
            Assert.AreEqual(3,run.Inventory.Count,"Anvil upgrades one copy without consuming a pair.");
        }

        [Test] public void AnvilFallbackKeepsMaxedWeaponOrderAndAddsArmor()
        {
            var run=Run();run.Inventory.Clear();
            run.Inventory.Add(catalog.Weapon("pistol",4));run.Inventory.Add(catalog.Weapon("knife",4));
            run.Items.Add(Item("anvil"));float before=run.Stat("armor");
            OpenShop(run);
            CollectionAssert.AreEqual(new[]{"pistol:4","knife:4"},run.Inventory.Select(w=>w.id+":"+w.tier));
            Assert.AreEqual(before+2,run.Stat("armor"));
        }

        [TestCase(0,50,5)]
        [TestCase(-5,50,2)]
        [TestCase(0,-100,1)]
        public void NativeWandBurnClampsBaseThenRoundsPercentAsSeparateIntegerStages(float elemental,float percent,float expected)
        {
            var run=Run();run.GainWeaponStat("elemental_damage",elemental);run.GainWeaponStat("percent_damage",percent);
            Assert.IsTrue(run.RollBurn(catalog.Weapon("wand",1),0,out float damage,out int ticks));
            Assert.AreEqual(expected,damage);Assert.AreEqual(3,ticks);
        }

        [Test] public void MageFractionalBurnScalingTruncatesBeforeApplyingDamagePercent()
        {
            var run=Run("mage");run.GainWeaponStat("elemental_damage",1);run.GainWeaponStat("percent_damage",10);
            Assert.AreEqual(1.25f,run.Stat("elemental_damage"));
            Assert.IsTrue(run.RollBurn(catalog.Weapon("wand",1),0,out float damage,out int ticks));
            // Native Wand 3 + starting Sausage 1 + Elemental 1.25 = 5.25;
            // truncate to 5, multiply by 1.1, then Godot round to 6.
            Assert.AreEqual(6,damage);Assert.AreEqual(3,ticks);
        }

        [Test] public void GlobalSausageBurnUsesTheSameIntegerDamageStages()
        {
            var run=Run();run.Items.Add(Item("scared_sausage"));
            run.GainWeaponStat("elemental_damage",2);run.GainWeaponStat("percent_damage",50);
            Assert.IsTrue(run.RollBurn(catalog.Weapon("pistol",1),0,out float damage,out int ticks));
            Assert.AreEqual(5,damage);Assert.AreEqual(3,ticks);
            Assert.IsTrue(run.RollBurn(catalog.Weapon("pistol",1),.25,out _,out _),"Positive source chance includes its boundary.");
            Assert.IsFalse(run.RollBurn(catalog.Weapon("pistol",1),.2501,out _,out _));
        }

        [TestCase(1,5)]
        [TestCase(4,6)]
        public void NativeStructureBurnTruncatesEngineeringScalingAndIgnoresPlayerDamage(float engineering,float expected)
        {
            var run=Run();run.GainWeaponStat("engineering",engineering);run.GainWeaponStat("percent_damage",100);
            var flame=Item("turret_flame").conditionalEffects.Single(e=>e.customKey=="structure").structure;
            Assert.IsTrue(run.RollBurn(flame.Equipment,0,out float damage,out int ticks,true));
            Assert.AreEqual(expected,damage);Assert.AreEqual(8,ticks);
        }

        [Test] public void MaterialDropAmountPreservesFractionBeforeItsExistingRandomRounding()
        {
            var run=Run();run.GainWeaponStat("gold_drops",-25);
            Assert.AreEqual(.75f,run.MaterialDropAmount(1));
            Assert.AreEqual(1,run.MaterialDropValue(1,false,.749));
            Assert.AreEqual(1,run.MaterialDropValue(1,false,.75));
            Assert.AreEqual(0,run.MaterialDropValue(1,false,.7501));
            run.GainWeaponStat("gold_drops",-100);
            Assert.AreEqual(.5f,run.MaterialDropAmount(1),"The existing half-base floor remains unchanged.");
        }

        [Test] public void IntegerMaterialAmountDoesNotAddAnExtraDropAtZeroRoll()
        {
            var run=Run();Assert.AreEqual(1,run.MaterialDropAmount(1));
            Assert.AreEqual(1,run.MaterialDropValue(1,false,0));
        }

        [TestCase(1,.08f)]
        [TestCase(2,.15f)]
        [TestCase(3,.22f)]
        public void UglyToothTruncatesRemainingIntegerPixelSpeedAfterEachHit(int copies,float expected)
        {
            var run=Run();for(int i=0;i<copies;i++)run.Items.Add(Item("ugly_tooth"));
            Assert.AreEqual(expected,run.ItemSpeedReduction(0,1.42f),.00001f);
        }

        [Test] public void StackedTeethRetainIntegerSubtractionAndSourceCapOvershootAcrossHits()
        {
            var run=Run();run.Items.Add(Item("ugly_tooth"));run.Items.Add(Item("ugly_tooth"));
            float first=run.ItemSpeedReduction(0,1.42f);
            float second=run.ItemSpeedReduction(first,1.42f);
            Assert.AreEqual(.15f,first,.00001f);Assert.AreEqual(.30f,second,.00001f);
            Assert.AreEqual(second,run.ItemSpeedReduction(second,1.42f),.00001f);
        }

        [Test] public void ToothPercentageUsesFinalScaledMaximumSpeed()
        {
            var run=Run();run.Items.Add(Item("ugly_tooth"));
            float scaled=EnemyReferenceRules.ScaledSpeed(1.42f,2);
            Assert.AreEqual(.15f,run.ItemSpeedReduction(0,scaled),.00001f);
        }

        [Test] public void BurnChanceAcceptsCertainBoundaryButRejectsZeroChanceAtZeroRoll()
        {
            var run=Run();
            Assert.IsTrue(run.RollBurn(catalog.Weapon("wand",1),1,out _,out _));
            Assert.IsFalse(run.RollBurn(catalog.Weapon("pistol",1),0,out _,out _));
        }

        [TestCase(.2,true)]
        [TestCase(.2001,false)]
        [TestCase(.1999,true)]
        public void ActualTentacleUsesSourcePositiveInclusiveCriticalHealChance(double roll,bool expected)
        {
            var run=Run();run.GrantItem(Item("tentacle"));run.Hurt(3);
            Assert.AreEqual(expected,run.TryCriticalKillHeal(true,roll));
            Assert.AreEqual(expected?8:7,run.Health);
        }

        [Test] public void ZeroCriticalHealChanceCannotSucceedAtZeroRoll()
        {
            var run=Run();run.Hurt(3);
            Assert.IsFalse(run.TryCriticalKillHeal(true,0));Assert.AreEqual(7,run.Health);
        }

        [Test] public void FiveTentaclesAcceptTheCertainChanceEndpoint()
        {
            var run=Run();for(int i=0;i<5;i++)run.GrantItem(Item("tentacle"));run.Hurt(3);
            Assert.AreEqual(100,run.Stat("heal_on_crit_kill"));
            Assert.IsTrue(run.TryCriticalKillHeal(true,1));Assert.AreEqual(8,run.Health);
        }

        [Test] public void LichDamageReadsUncappedMaxHpStatWhenHandcuffsCapsUsableHealth()
        {
            var run=Run("lich");run.Inventory.Clear();run.GrantItem(Item("handcuffs"));
            run.GainWeaponStat("max_hp",10);
            Assert.AreEqual(10,run.MaxHealth);Assert.AreEqual(20,run.Stat("max_hp"));
            float received=0;run.RandomDamageRequested+=value=>received=value;
            run.Hurt(1);run.Heal(1);
            Assert.AreEqual(20,received,"main.handle_stat_damages reads Utils.get_stat, not get_capped_stat.");
        }
    }
}
