using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemProcInteractionFollowupTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void SetUp()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Lich()
        {
            var profile=catalog.data.characters.Single(c=>c.id=="lich");
            var run=new RunRules(202010);run.Configure(catalog);
            run.Start(profile,StarterChoices.Available(catalog,profile).First());run.Inventory.Clear();return run;
        }
        void Add(RunRules run,string id,int copies=1)
        {
            var item=catalog.data.items.Single(entry=>entry.id==id);
            if(item.maxCount>0)Assert.LessOrEqual(run.Items.Count(entry=>entry.id==id)+copies,item.maxCount,"Fixture exceeds the imported item cap.");
            for(int i=0;i<copies;i++)run.GrantItem(item);
        }
        static void Health(RunRules run,float value)
        {
            // Let acquired Max HP effects reconcile before staging missing HP;
            // otherwise the next health read correctly credits that acquisition.
            _=run.Health;
            typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(run,new object[]{value});
        }
        sealed class Rolls:Random
        {
            readonly double[] values;int index;
            public Rolls(IEnumerable<double> values){this.values=values.ToArray();}
            public override double NextDouble(){Assert.Less(index,values.Length,"Unexpected additional proc roll.");return values[index++];}
            public int Used=>index;
        }
        static Rolls Use(RunRules run,IEnumerable<double> values)
        {var rolls=new Rolls(values);typeof(RunRules).GetField("random",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(run,rolls);return rolls;}
        [TestCase(false)][TestCase(true)]
        public void CappedAdrenalineHealingEmitsOneLichAttackButFullHealthEmitsNone(bool fullHealth)
        {
            // Adrenaline has maxCount1. The source sums successful entries then
            // on_healing_effect caps actual healing to the missing HP.
            var run=Lich();Add(run,"adrenaline");Health(run,fullHealth?10:9);
            run.GainWeaponStat("dodge",100);run.GainWeaponStat("dodge_cap",100);
            var draws=new List<double>{0,.499999};
            if(!fullHealth)draws.Add(0);var rolls=Use(run,draws);
            var attacks=new List<float>();run.RandomDamageRequested+=attacks.Add;run.Hurt(1);
            Assert.IsTrue(run.LastHitDodged);
            Assert.AreEqual(10,run.Health);
            CollectionAssert.AreEqual(fullHealth?Array.Empty<float>():new[]{10f},attacks);
            Assert.AreEqual(draws.Count,rolls.Used);
        }

        [TestCase(false)][TestCase(true)]
        public void PickupHealingAndElephantDamageStaySeparateAndFullHealthSuppressesOnlyLich(bool fullHealth)
        {
            // main.on_gold_picked_up emits Monkey healing before Elephant;
            // main.handle_stat_damages aggregates duplicate Elephant entries.
            var run=Lich();Add(run,"cute_monkey");Add(run,"baby_elephant",2);
            run.GainWeaponStat("luck",8);run.GainWeaponStat("percent_damage",50);
            Assert.AreEqual(25,run.Stat("percent_damage"));
            Health(run,fullHealth?10:9);
            var draws=new List<double>{.9,0};if(!fullHealth)draws.Add(0);draws.Add(0);draws.Add(0);
            var rolls=Use(run,draws);var attacks=new List<float>();run.RandomDamageRequested+=attacks.Add;
            int money=run.Scrap,xp=run.XP;run.Collect(1);
            Assert.AreEqual(10,run.Health);Assert.AreEqual(money+1,run.Scrap);Assert.AreEqual(xp+1,run.XP);
            CollectionAssert.AreEqual(fullHealth?new[]{6f}:new[]{13f,6f},attacks);
            Assert.AreEqual(draws.Count,rolls.Used);
        }

        [Test] public void TortureBlocksMonkeyAndTentacleHealingButNotElephantOrDeathProcs()
        {
            // player.heal gates both sources before emitting healed. Neither
            // independent pickup damage nor on-death damage depends on healing.
            var run=Lich();Add(run,"cute_monkey");Add(run,"tentacle",5);
            Add(run,"baby_elephant",2);Add(run,"cyberball",2);
            Add(run,"torture");
            run.GainWeaponStat("luck",8);Health(run,2);
            var rolls=Use(run,new double[]{.9,0,0,0,0,0});
            var attacks=new List<float>();run.RandomDamageRequested+=attacks.Add;
            run.Collect(1);Assert.IsFalse(run.TryCriticalKillHeal(true,0));run.KillEffects(false);
            Assert.AreEqual(2,run.Health);CollectionAssert.AreEqual(new[]{4f,4f},attacks);Assert.AreEqual(6,rolls.Used);
        }

        [TestCase(.6,true)][TestCase(.600001,false)]
        public void ThreeTentaclesUseOneCombinedInclusiveRollRatherThanThreeHealingEvents(double roll,bool succeeds)
        {
            // unit.take_damage:373 tests the summed heal_on_crit_kill stat once
            // with Utils.get_chance_success (<=); healing is always one HP.
            var run=Lich();Add(run,"tentacle",3);Health(run,5);
            var rolls=Use(run,succeeds?new double[]{0}:Array.Empty<double>());
            var attacks=new List<float>();run.RandomDamageRequested+=attacks.Add;
            Assert.AreEqual(succeeds,run.TryCriticalKillHeal(true,roll));
            Assert.AreEqual(succeeds?6:5,run.Health);
            CollectionAssert.AreEqual(succeeds?new[]{10f}:Array.Empty<float>(),attacks);
            Assert.AreEqual(succeeds?1:0,rolls.Used);
        }

        [Test] public void CriticalTrophiesDoNotActivatePickupEffectsButTentacleAndCyberballRetainTheirOwnEvents()
        {
            var run=Lich();Add(run,"hunting_trophy",3);Add(run,"tentacle",5);
            Add(run,"cute_monkey");Add(run,"baby_elephant",2);Add(run,"cyberball",2);
            run.GainWeaponStat("luck",8);Health(run,5);run.BankMaterials(7);
            // Trophy <=33%: success/failure/success, Lich 100%, Cyberball two successes.
            var rolls=Use(run,new double[]{.33,.330001,0,0,0,0});
            var attacks=new List<float>();run.RandomDamageRequested+=attacks.Add;
            int money=run.Scrap,xp=run.XP;run.CriticalKillMaterials(true);
            Assert.AreEqual(money+2,run.Scrap);Assert.AreEqual(5,run.Health);Assert.IsEmpty(attacks);
            Assert.IsTrue(run.TryCriticalKillHeal(true,1));run.KillEffects(false);
            Assert.AreEqual(6,run.Health);CollectionAssert.AreEqual(new[]{10f,4f},attacks);
            Assert.AreEqual(xp,run.XP);Assert.AreEqual(7,run.Banked);Assert.AreEqual(6,rolls.Used);
        }
    }
}
