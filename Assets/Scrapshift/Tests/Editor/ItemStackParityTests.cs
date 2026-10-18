using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemStackParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(18);r.Configure(catalog);r.Start();return r;}
        void Add(RunRules run,string id,int copies)
        {var item=catalog.data.items.Single(i=>i.id==id);for(int i=0;i<copies;i++)run.Items.Add(item);}

        [Test] public void RipAndTearCopiesShareOneSummedChanceAndAllContributeDamage()
        {
            // run_data.gd:handle_explode_effect: sum chance, one roll, sum damage.
            var r=Run();Add(r,"rip_and_tear",2);
            Assert.IsTrue(r.TryItemExplosion("explode_on_death",.35,out var effect,out float damage));
            Assert.AreSame(r.Effects("explode_on_death").First(),effect);
            Assert.AreEqual(20,damage);
            Assert.IsFalse(r.TryItemExplosion("explode_on_death",.41,out _,out damage));
            Assert.AreEqual(0,damage);
        }

        [Test] public void TwoSpicySaucesGuaranteeOneExplosionWithBothCopiesDamage()
        {
            var r=Run();Add(r,"spicy_sauce",2);
            Assert.IsTrue(r.TryItemExplosion("explode_on_consumable",.999,out _,out float damage));
            Assert.AreEqual(r.MaxHealth*2,damage);
        }

        [Test] public void EachExplosionCopyRoundsItsOwnScaledDamageBeforeSumming()
        {
            var r=Run();Add(r,"rip_and_tear",2);r.GainWeaponStat("percent_damage",5);
            Assert.IsTrue(r.TryItemExplosion("explode_on_death",0,out _,out float damage));
            Assert.AreEqual(22,damage,"Two independently rounded 10.5 hits contribute 11 each.");
        }

        [Test] public void CurrentHealthCritBonusesTruncateEachEffectBeforeAdding()
        {
            // Giant Belt is limited to one in ordinary shops; verify the effect-array rule
            // independently so future additional sources cannot change its rounding.
            var r=Run();Add(r,"giant_belt",2);
            Assert.AreEqual(2,r.CriticalCurrentHealthDamage(19,false));
            Assert.AreEqual(0,r.CriticalCurrentHealthDamage(99,true));
            Assert.AreEqual(2,r.CriticalCurrentHealthDamage(199,true));
        }

        [Test] public void ItemExplosionsReceiveAddedWeaponScalingWhileRadialProjectilesDoNot()
        {
            var r=Run();Add(r,"rip_and_tear",2);Add(r,"nail",1);Add(r,"frozen_heart",1);
            r.GainWeaponStat("engineering",10);r.GainWeaponStat("elemental_damage",20);
            Assert.IsTrue(r.TryItemExplosion("explode_on_death",0,out _,out float damage));
            Assert.AreEqual(28,damage,"Each explosion gains 1 Engineering damage and 3 Elemental damage.");
        }

        [Test] public void DuplicateBurnKillItemsHaveIndependentCapsThatResetEachWave()
        {
            var r=Run();Add(r,"will_o_the_wisp",2);
            for(int i=0;i<150;i++)r.KillEffects(true);
            Assert.AreEqual(8,r.Stat("elemental_damage"));
            r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.NextWave();
            for(int i=0;i<30;i++)r.KillEffects(true);
            Assert.AreEqual(10,r.Stat("elemental_damage"));
        }

        [Test] public void DuplicateStatLinksTruncateEachCopyAndCountDistinctItemIdentities()
        {
            // linked_stats.gd casts each linked effect separately and counts different items.
            var r=Run();var linked=new ItemEntry{id="linked",effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="stat_links",key="stat_armor",stat="different_item",value=1,per=3}}};
            r.Items.Add(linked);r.Items.Add(linked);r.Items.Add(new ItemEntry{id="other",effects=new StatEffect[0]});
            Assert.AreEqual(0,r.Stat("armor"));
            r.Items.Add(new ItemEntry{id="third",effects=new StatEffect[0]});
            Assert.AreEqual(2,r.Stat("armor"));
            r.Items.RemoveAt(r.Items.Count-1);
            Assert.AreEqual(0,r.Stat("armor"));
        }

        [Test] public void DuplicateIntervalBonusesResetTogetherOnlyForResetOnHitEffects()
        {
            var r=Run();var reset=new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="temp_stats_per_interval",key="stat_armor",value=1,interval=1,resetOnHit=true}}};
            r.Items.Add(reset);r.Items.Add(reset);r.Items.Add(new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="temp_stats_per_interval",key="stat_armor",value=3,interval=1}}});
            r.Tick(2);Assert.AreEqual(10,r.Stat("armor"));
            r.Hurt(1);Assert.AreEqual(6,r.Stat("armor"));
            r.Tick(1);Assert.AreEqual(11,r.Stat("armor"));
        }

        [Test] public void SelfDamageConsumesProtectionBeforeHealthOrOnHitBonuses()
        {
            var r=Run();r.GainWeaponStat("hit_protection",1);
            r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.NextWave();
            r.GainWeaponStat("lose_hp_per_second",1);
            r.Items.Add(new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="temp_stats_on_hit",key="stat_armor",value=3}}});
            float initial=r.Health;r.Tick(1);
            Assert.AreEqual(initial,r.Health);Assert.AreEqual(0,r.HitProtection);Assert.IsTrue(r.LastHitProtected);Assert.AreEqual(0,r.Stat("armor"));
            r.Tick(1);Assert.AreEqual(initial-1,r.Health);Assert.AreEqual(3,r.Stat("armor"));
        }

        [Test] public void DuplicateDodgeHealingEmitsOneHealingAttack()
        {
            var r=Run();r.Hurt(3);
            r.Items.Add(new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="heal_on_dodge",key="",value=1,chance=1},
                new ConditionalEffect{customKey="heal_on_dodge",key="",value=1,chance=1},
                new ConditionalEffect{customKey="dmg_when_heal",key="stat_max_hp",value=100,chance=1}}});
            r.GainWeaponStat("dodge",100);r.GainWeaponStat("dodge_cap",100);
            int attacks=0;r.RandomDamageRequested+=_=>attacks++;
            r.Hurt(1);Assert.AreEqual(r.MaxHealth-1,r.Health);Assert.AreEqual(1,attacks);
        }

        [Test] public void RiposteTruncatesBaseThenRoundsEachCopiesScaledDamage()
        {
            var r=Run();r.GainWeaponStat("melee_damage",1);r.GainWeaponStat("percent_damage",50);
            r.GainWeaponStat("dodge",100);r.GainWeaponStat("dodge_cap",100);
            r.Items.Add(new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{
                new ConditionalEffect{customKey="dmg_on_dodge",key="stat_melee_damage",value=300,chance=1},
                new ConditionalEffect{customKey="dmg_on_dodge",key="stat_melee_damage",value=300,chance=1}}});
            r.Hurt(1);Assert.AreEqual(10,r.DodgeRetaliation,"Each copy rounds 3 × 1.5 to 5.");
        }

        [Test] public void RemovingNextWaveItemCancelsOnlyStillQueuedCopyAndPreservesActiveTemporaryBonus()
        {
            var r=Run();var peacock=catalog.data.items.Single(i=>i.id=="peacock");
            r.GrantItem(peacock);r.GrantItem(peacock);Assert.IsTrue(r.RemoveItem(peacock));
            r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.NextWave();
            Assert.AreEqual(125,r.Stat("xp_gain"));
            Assert.IsTrue(r.RemoveItem(peacock));Assert.AreEqual(100,r.Stat("xp_gain"),"Already applied next-wave bonuses persist until this wave ends.");
            r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.NextWave();
            Assert.AreEqual(0,r.Stat("xp_gain"),"Removing a consumed queue entry must not schedule a negative bonus.");
        }

        [Test] public void RemovingBaitCancelsPendingGroupButCannotMakeConsumedQueueNegative()
        {
            var r=Run();var bait=catalog.data.items.Single(i=>i.id=="bait");
            r.GrantItem(bait);r.GrantItem(bait);Assert.IsTrue(r.RemoveItem(bait));
            r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.NextWave();
            Assert.AreEqual(1,r.ExtraBaitGroups);
            Assert.IsTrue(r.RemoveItem(bait));
            r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.NextWave();
            Assert.AreEqual(0,r.ExtraBaitGroups);
        }
    }
}
