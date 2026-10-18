using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class TriggeredImplantTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(18);r.Configure(catalog);r.Start();return r;}
        void Add(RunRules r,string id)=>r.Items.Add(catalog.data.items.Single(i=>i.id==id));
        void Shop(RunRules r){r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);}
        void Buy(RunRules r,string id){r.Shop[0]=new ShopOffer{item=catalog.data.items.Single(i=>i.id==id),rolledWave=r.Wave};Assert.IsTrue(r.Buy(0));}
        [Test] public void ExtraStomachGainsAreLimitedPerWaveAndDoNotTriggerWhenInjured()
        {
            var r=Run();Add(r,"extra_stomach");for(int i=0;i<12;i++)r.CollectConsumable(ConsumableKind.Fruit);
            Assert.AreEqual(18,r.MaxHealth);r.Hurt(5);r.CollectConsumable(ConsumableKind.Fruit);Assert.AreEqual(18,r.MaxHealth);
            Shop(r);r.NextWave();r.CollectConsumable(ConsumableKind.Fruit);Assert.AreEqual(19,r.MaxHealth);
        }
        [Test] public void FullHealthConsumableCapsAreTrackedPerEffectEvenForTheSameStat()
        {
            var r=Run();r.Items.Add(new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="consumable_stats_while_max",key="stat_armor",value=1,per=1},
                new ConditionalEffect{customKey="consumable_stats_while_max",key="stat_armor",value=2,per=3}}});
            r.CollectConsumable(ConsumableKind.Fruit);
            Assert.AreEqual(3,r.Stat("armor"));
            r.CollectConsumable(ConsumableKind.Fruit);
            Assert.AreEqual(5,r.Stat("armor"),"The first effect reached its own cap; the second did not.");
            r.CollectConsumable(ConsumableKind.Fruit);
            Assert.AreEqual(5,r.Stat("armor"));
        }
        [Test] public void DuplicateItemCopiesKeepIndependentConsumableCounters()
        {
            var r=Run();var item=new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="consumable_stats_while_max",key="stat_armor",value=1,per=1}}};
            r.Items.Add(item);r.Items.Add(item);
            r.CollectConsumable(ConsumableKind.Fruit);Assert.AreEqual(2,r.Stat("armor"));
            r.CollectConsumable(ConsumableKind.Fruit);Assert.AreEqual(2,r.Stat("armor"));
        }
        [Test] public void MissingHealthLinksRoundTheSourcePercentBeforeApplyingScaling()
        {
            var r=Run();r.Items.Add(new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="stat_links",key="stat_percent_damage",stat="percent_player_missing_health",value=2,per=1}}});
            // The reference keeps current HP when maximum HP falls mid-wave.
            // Hurt first so the intended source ratio is 5/6, not 9/6.
            r.Hurt(5);r.GainWeaponStat("max_hp",-4);
            Assert.AreEqual(6,r.MaxHealth);Assert.AreEqual(5,r.Health);
            Assert.AreEqual(34,r.Stat("percent_damage"),"The reference rounds 16.67% missing HP to 17% before applying 2 damage per percent.");
        }
        [Test] public void RegenBonusStopsAtHalfHealthAndDoesNotBoostTorture()
        {
            var r=Run();Add(r,"potion");r.GainWeaponStat("hp_regeneration",1);r.Hurt(6);
            Assert.AreEqual(2,r.RegenerationHealMultiplier);r.Tick(5);Assert.AreEqual(6,r.Health);Assert.AreEqual(1,r.RegenerationHealMultiplier);
            r.GainWeaponStat("torture",5);r.Hurt(3);r.Tick(1);Assert.AreEqual(8,r.Health);
        }
        [Test] public void TeethSlowIsPersistentAndChecksCapBeforeStackedSubtraction()
        {
            var r=Run();Add(r,"ugly_tooth");float slow=0;for(int i=0;i<8;i++)slow=r.ItemSpeedReduction(slow,4);Assert.AreEqual(.8f,slow,.001);
            Add(r,"ugly_tooth");Add(r,"ugly_tooth");slow=r.ItemSpeedReduction(.6f,4);Assert.AreEqual(1.2f,slow,.001);
        }
        [Test] public void GiantBeltUsesCurrentHealthAndBossReduction()
        {
            var r=Run();Add(r,"giant_belt");Assert.AreEqual(10,r.CriticalCurrentHealthDamage(100,false));Assert.AreEqual(1,r.CriticalCurrentHealthDamage(100,true));Assert.AreEqual(5,r.CriticalCurrentHealthDamage(50,false));
        }
        [Test] public void AddedScalingUsesStatBeforePercentDamageAndDoesNotReplaceNativeScaling()
        {
            var r=Run();Add(r,"nail");Add(r,"frozen_heart");r.GainWeaponStat("engineering",10);r.GainWeaponStat("elemental_damage",20);r.GainWeaponStat("ranged_damage",3);r.GainWeaponStat("percent_damage",50);
            var pistol=catalog.Weapon("pistol",1);float baseDamage=UnityEngine.Mathf.Max(1,(int)(pistol.damage+pistol.scaling.Sum(s=>r.Stat(s.key)*s.value)+4));float expected=UnityEngine.Mathf.Max(1,UnityEngine.Mathf.Floor(baseDamage*1.5f+.5f));Assert.AreEqual(expected,r.WeaponDamage(pistol),.001);
        }
        [Test] public void CooldownFloorIncludesWeaponAnimationAndDoesNotStackDuplicateFloors()
        {
            var r=Run();Add(r,"ball_and_chain");Add(r,"ball_and_chain");r.GainWeaponStat("attack_speed",100);
            var smg=catalog.Weapon("smg",1);Assert.AreEqual(.75f,r.WeaponCooldown(smg)+r.WeaponAnimationDuration(smg),.001);
        }
        [Test] public void StructureSpeedIncludesConvertedPlayerSpeedAndHandlesNegativeValues()
        {
            var r=Run();Add(r,"improved_tools");r.GainWeaponStat("attack_speed",100);Assert.AreEqual(40f/60,r.StructureCooldown(1),.001);
            r.GainWeaponStat("attack_speed",-200);Assert.AreEqual(1.5f,r.StructureCooldown(1),.001);
        }
        [Test] public void SnowballRewardsNewPositiveItemStatsWithoutRetroactiveOrUpgradeRewards()
        {
            var r=Run();r.Collect(2000);Shop(r);Buy(r,"snowball");float before=r.Stat("elemental_damage");Buy(r,"boiling_water");
            var water=catalog.data.items.Single(i=>i.id=="boiling_water");Assert.AreEqual(before+1+water.effects.Where(e=>e.key=="stat_elemental_damage").Sum(e=>e.value),r.Stat("elemental_damage"));
            before=r.Stat("elemental_damage");r.GainWeaponStat("elemental_damage",3);Assert.AreEqual(before+3,r.Stat("elemental_damage"));
        }
    }
}
