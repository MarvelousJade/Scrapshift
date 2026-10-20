using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class HeadlessDynamicProcTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void SetUp()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(string character=null)
        {
            var run=new RunRules(2010);run.Configure(catalog);
            var profile=character==null?null:catalog.data.characters.Single(c=>c.id==character);
            run.Start(profile,profile==null?"pistol":StarterChoices.Available(catalog,profile).First());
            return run;
        }
        void Add(RunRules run,string id)=>run.GrantItem(catalog.data.items.Single(item=>item.id==id));
        sealed class Rolls:Random
        {
            readonly double[] values;int index;
            public Rolls(params double[] values){this.values=values;}
            public override double NextDouble()
            {Assert.Less(index,values.Length,"Unexpected additional RNG draw changes event semantics.");return values[index++];}
            public int Used=>index;
        }
        static Rolls SetRolls(RunRules run,params double[] values)
        {
            var rolls=new Rolls(values);
            typeof(RunRules).GetField("random",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(run,rolls);
            return rolls;
        }

        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)]
        [TestCase(4)][TestCase(5)][TestCase(6)][TestCase(7)]
        public void ThreeLegalTrophiesRollIndependentlyAndPayOnlySuccessfulEntries(int mask)
        {
            // Unit.take_damage iterates each Hunting Trophy entry, checks
            // Utils.get_chance_success(33/100), then RunData.add_gold(total).
            // This is immediate currency: no material pickup, XP or healing.
            var run=Run();for(int i=0;i<3;i++)Add(run,"hunting_trophy");
            Assert.AreEqual(3,catalog.data.items.Single(item=>item.id=="hunting_trophy").maxCount);
            run.GainWeaponStat("heal_when_pickup_gold",100);run.Hurt(3);run.BankMaterials(7);
            int money=run.Scrap,xp=run.XP;float hp=run.Health;
            var rolls=SetRolls(run,Enumerable.Range(0,3).Select(bit=>(mask&(1<<bit))!=0?.33:.330001).ToArray());
            run.CriticalKillMaterials(true);
            Assert.AreEqual(Enumerable.Range(0,3).Count(bit=>(mask&(1<<bit))!=0),run.Scrap-money);
            Assert.AreEqual(3,rolls.Used);Assert.AreEqual(xp,run.XP);Assert.AreEqual(7,run.Banked);Assert.AreEqual(hp,run.Health);
        }

        [Test] public void NoncriticalKillDoesNotRollTrophyEntries()
        {
            var run=Run();Add(run,"hunting_trophy");var rolls=SetRolls(run);
            int money=run.Scrap;run.CriticalKillMaterials(false);
            Assert.AreEqual(0,rolls.Used);Assert.AreEqual(money,run.Scrap);
        }

        [TestCase(.499999,true)][TestCase(.5,false)][TestCase(.500001,false)]
        public void AdrenalineUsesStrictHalfChanceAndLichReceivesOneHealingEvent(double healRoll,bool heals)
        {
            // player.take_damage intentionally uses randf() < 0.5 for
            // Adrenaline, unlike Utils.get_chance_success's <= for trophies.
            var run=Run("lich");Add(run,"adrenaline");
            typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(run,new object[]{4f});
            run.GainWeaponStat("dodge",100);run.GainWeaponStat("dodge_cap",100);
            int attacks=0;float damage=0;run.RandomDamageRequested+=value=>{attacks++;damage=value;};
            var rolls=SetRolls(run,heals?new[]{0,healRoll,0}:new[]{0,healRoll});run.Hurt(1);
            Assert.IsTrue(run.LastHitDodged);Assert.AreEqual(heals?9:4,run.Health);
            Assert.AreEqual(heals?1:0,attacks);Assert.AreEqual(heals?10:0,damage);
            Assert.AreEqual(heals?3:2,rolls.Used);
        }

        [Test] public void GolemHalfHealthBonusPersistsDuringCombatPauseButClearsOnLethalDefeat()
        {
            // Main.check_half_health_stats adds temporary stats while alive.
            // Player.die clears TempStats at line650, and end_run._ready resets
            // them again before showing the final stats. HP0 must not recreate
            // these bonuses in the Lost phase after death cleanup reset.
            var run=Run("golem");run.Inventory.Clear();
            float speed=run.Stat("speed"),attack=run.Stat("attack_speed");
            run.Hurt(run.Health-1);Assert.AreEqual(1,run.Health);
            Assert.AreEqual(speed+20,run.Stat("speed"));Assert.AreEqual(attack+40,run.Stat("attack_speed"));
            run.Pause();Assert.AreEqual(speed+20,run.Stat("speed"));Assert.AreEqual(attack+40,run.Stat("attack_speed"));run.Pause();
            run.Hurt(1);Assert.AreEqual(RunPhase.Lost,run.Phase);Assert.AreEqual(0,run.Health);
            Assert.AreEqual(speed,run.Stat("speed"));Assert.AreEqual(attack,run.Stat("attack_speed"));
        }

        [Test] public void GolemHalfHealthBonusClearsAfterOrdinaryFinalWaveSettlement()
        {
            var run=Run("golem");run.Inventory.Clear();
            for(int wave=1;wave<20;wave++)
            {
                run.Tick(run.Remaining);
                while(run.Phase==RunPhase.Upgrade)run.Choose(0);
                Assert.AreEqual(RunPhase.Shop,run.Phase);run.NextWave();
            }
            Assert.AreEqual(20,run.Wave);
            run.GainWeaponStat("armor",-run.Stat("armor")/run.Character.Gain("armor"));
            run.GainWeaponStat("dodge",-1000);
            float speed=run.Stat("speed"),attack=run.Stat("attack_speed");
            run.Hurt(run.Health-1);Assert.AreEqual(1,run.Health);
            Assert.AreEqual(speed+20,run.Stat("speed"));Assert.AreEqual(attack+40,run.Stat("attack_speed"));
            run.Tick(run.Remaining);Assert.AreEqual(RunPhase.Won,run.Phase);
            Assert.AreEqual(speed,run.Stat("speed"));Assert.AreEqual(attack,run.Stat("attack_speed"));
        }
    }
}
