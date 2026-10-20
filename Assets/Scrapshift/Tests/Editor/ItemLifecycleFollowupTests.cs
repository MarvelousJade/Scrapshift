using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemLifecycleFollowupTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void SetUp()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        ItemEntry Item(string id)=>catalog.data.items.Single(i=>i.id==id);
        RunRules Run(string id=null)
        {
            var r=new RunRules(2020);r.Configure(catalog);
            var character=id==null?null:catalog.data.characters.Single(c=>c.id==id);
            r.Start(character,character==null?"pistol":StarterChoices.Available(catalog,character).First());
            r.Inventory.Clear();return r;
        }
        static void Shop(RunRules r)
        {
            r.Tick(r.Remaining);
            while(r.Phase==RunPhase.Upgrade)r.Choose(0);
            Assert.AreEqual(RunPhase.Shop,r.Phase);
        }

        [TestCase(false)][TestCase(true)]
        public void SnowballAcquisitionOrderRemovalAndReacquisitionPreserveOnlyEarnedBonuses(bool waterFirst)
        {
            // RunData.apply_item_effects calls its stat-added hook only for
            // newly applied positive ItemData stats. Unapply has no hook and
            // never reverses the permanent reward already earned.
            var r=Run();var water=Item("boiling_water");var snow=Item("snowball");
            Assert.AreEqual(1,snow.maxCount);
            float elemental=water.effects.Where(e=>e.key=="stat_elemental_damage").Sum(e=>e.value);
            if(waterFirst){r.GrantItem(water);r.GrantItem(snow);}
            else {r.GrantItem(snow);r.GrantItem(water);}
            float earned=waterFirst?0:1;
            Assert.AreEqual(elemental+earned,r.Stat("elemental_damage"));
            Assert.IsTrue(r.RemoveItem(water));Assert.AreEqual(earned,r.Stat("elemental_damage"));
            Assert.IsTrue(r.RemoveItem(snow));r.GrantItem(water);
            Assert.AreEqual(elemental+earned,r.Stat("elemental_damage"));
            r.GrantItem(snow);Assert.AreEqual(elemental+earned,r.Stat("elemental_damage"));
            r.GrantItem(water);Assert.AreEqual(2*elemental+earned+1,r.Stat("elemental_damage"));
            Assert.IsTrue(r.RemoveItem(snow));Assert.IsTrue(r.RemoveItem(water));Assert.IsTrue(r.RemoveItem(water));
            Assert.AreEqual(earned+1,r.Stat("elemental_damage"));
        }

        [TestCase(null,false)][TestCase(null,true)]
        [TestCase("doctor",false)][TestCase("doctor",true)]
        [TestCase("renegade",false)][TestCase("renegade",true)]
        public void MixedDuplicateWaveGrowthSurvivesRemovalAndUsesCharacterStatGains(string character,bool reverse)
        {
            // Main._on_WaveTimer_timeout iterates stats_end_of_wave entries
            // before TempStats.reset. Item unapply removes future entries, not
            // prior permanent add_stat gains. Each item below is unlimited.
            var r=Run(character);
            var ids=new[]{"grinds_magical_leaf","robot_arm","vigilante_ring"};
            var stats=new[]{"max_hp","hp_regeneration","lifesteal","melee_damage","engineering","percent_damage"};
            var gains=new[]{2f,1f,1f,3f,3f,3f}; // one Leaf + Arm + Ring per wave
            var baseline=stats.Select(r.Stat).ToArray();
            if(reverse)Array.Reverse(ids);
            foreach(string id in ids){Assert.LessOrEqual(Item(id).maxCount,0);r.GrantItem(Item(id));r.GrantItem(Item(id));}
            CollectionAssert.AreEqual(baseline,stats.Select(r.Stat).ToArray(),"Growth does not fire on acquisition.");
            Shop(r);
            for(int i=0;i<stats.Length;i++)Assert.AreEqual(baseline[i]+2*gains[i]*(r.Character?.Gain(stats[i])??1),r.Stat(stats[i]),.001,stats[i]);
            foreach(string id in ids)Assert.IsTrue(r.RemoveItem(Item(id)));
            r.NextWave();Shop(r);
            for(int i=0;i<stats.Length;i++)Assert.AreEqual(baseline[i]+3*gains[i]*(r.Character?.Gain(stats[i])??1),r.Stat(stats[i]),.001,stats[i]);
            foreach(string id in ids)Assert.IsTrue(r.RemoveItem(Item(id)));
            var earnedStats=stats.Select(r.Stat).ToArray();
            r.NextWave();int level=r.Level;r.Tick(r.Remaining);
            // Doctor's real +5 Harvesting grows each settlement and crosses
            // the first XP threshold on wave three. GainXP grants +1 Max HP
            // independently of the removed items. Do not choose an unrelated
            // upgrade before comparing these retained permanent gains.
            earnedStats[0]+=(r.Level-level)*(r.Character?.Gain("max_hp")??1);
            Assert.That(r.Phase,Is.EqualTo(RunPhase.Shop).Or.EqualTo(RunPhase.Upgrade));
            CollectionAssert.AreEqual(earnedStats,stats.Select(r.Stat).ToArray(),"Removed items stop growing; prior gains and ordinary level-up HP persist.");
        }

        [Test] public void TwoTrianglesAndMasochistTriggerTogetherOnlyForActualDamageAndClearAtWaveEnd()
        {
            // player.gd493 adds every temp_stats_on_hit entry only after real
            // damage. Two -2 Triangle entries plus Masochist's +5 net +1.
            var r=Run("masochist");r.GrantItem(Item("triangle_of_power"));r.GrantItem(Item("triangle_of_power"));
            r.GainWeaponStat("armor",-r.Stat("armor"));r.GainWeaponStat("dodge",-1000);
            float damage=r.Stat("percent_damage");
            r.Hurt(1);Assert.AreEqual(damage+1,r.Stat("percent_damage"));
            r.Hurt(1);Assert.AreEqual(damage+2,r.Stat("percent_damage"));
            Shop(r);Assert.AreEqual(damage,r.Stat("percent_damage"));
            r.GainWeaponStat("hit_protection",1);r.NextWave();
            r.Hurt(1);Assert.IsTrue(r.LastHitProtected);Assert.AreEqual(damage,r.Stat("percent_damage"));
            r.GainWeaponStat("dodge",2000);r.GainWeaponStat("dodge_cap",100);
            r.Hurt(1);Assert.IsTrue(r.LastHitDodged);Assert.AreEqual(damage,r.Stat("percent_damage"));
        }

        [Test] public void ImportedIntervalFamiliesDoNotUseTheSyntheticResetOnHitRule()
        {
            // The supplied items contain only Wisdom and Medikit interval
            // effects, both reset_on_hit=false. Existing synthetic reset-on-hit
            // tests are preventive rule coverage, not a playable item family.
            var families=catalog.data.items.Where(i=>(i.conditionalEffects??Array.Empty<ConditionalEffect>()).Any(e=>e.customKey=="temp_stats_per_interval")).ToArray();
            CollectionAssert.AreEquivalent(new[]{"wisdom","medikit"},families.Select(i=>i.id));
            Assert.IsFalse(families.SelectMany(i=>i.conditionalEffects).Any(e=>e.customKey=="temp_stats_per_interval"&&e.resetOnHit));
        }
    }
}
