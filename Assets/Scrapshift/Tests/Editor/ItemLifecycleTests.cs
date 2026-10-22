using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class ItemLifecycleTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(8);r.Configure(catalog);r.Start();return r;}
        void Shop(RunRules r){r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);}
        void Buy(RunRules r,string id){r.Shop[0]=new ShopOffer{item=catalog.data.items.Single(i=>i.id==id),rolledWave=r.Wave};Assert.IsTrue(r.Buy(0),id);}
        [Test] public void HitProtectionIsConsumedThenRefilledEachWave()
        {
            var r=Run();r.GainWeaponStat("hit_protection",2);Shop(r);r.NextWave();float hp=r.Health;
            r.Hurt(5);r.Hurt(5);Assert.AreEqual(hp,r.Health);Assert.IsTrue(r.LastHitProtected);Assert.AreEqual(0,r.HitProtection);
            r.Hurt(1);Assert.AreEqual(hp-1,r.Health);Assert.IsFalse(r.LastHitProtected);
            Shop(r);r.NextWave();Assert.AreEqual(2,r.HitProtection);
        }
        [Test] public void TortureReplacesRegenAndBlocksOtherHealing()
        {
            var r=Run();r.GainWeaponStat("max_hp",20);r.Heal(100);r.GainWeaponStat("torture",5);r.GainWeaponStat("hp_regeneration",100);
            r.Hurt(12);float hp=r.Health;r.Heal(10);r.CollectConsumable(ConsumableKind.Fruit);Assert.AreEqual(hp,r.Health);
            r.Tick(.9f);Assert.AreEqual(hp,r.Health);r.Tick(.1f);Assert.AreEqual(hp+5,r.Health);r.Tick(1);Assert.AreEqual(hp+10,r.Health);
        }
        [Test] public void OneWaveHealthPenaltyOverridesPersistentPenaltyOnlyOnce()
        {
            var r=Run();r.GainWeaponStat("hp_start_wave",-50);r.GainWeaponStat("hp_start_next_wave",-100);
            Shop(r);r.NextWave();Assert.AreEqual(1,r.Health);Assert.AreEqual(0,r.Stat("hp_start_next_wave"));
            Shop(r);r.NextWave();Assert.AreEqual(5,r.Health);
        }
        [Test] public void InterestUsesUnspentMaterialsAtWaveStartAndTruncates()
        {
            var r=Run();r.Collect(101);Shop(r);r.GainWeaponStat("gain_pct_gold_start_wave",20);int money=r.Scrap;r.NextWave();Assert.AreEqual(money+(int)(money*.2f),r.Scrap);
        }
        [Test] public void BuyingBunnyAddsFreeShopRollWithoutIncreasingPaidCost()
        {
            var r=Run();r.Collect(1000);Shop(r);int paid=r.RerollPrice;Buy(r,"dangerous_bunny");Assert.AreEqual(0,r.RerollPrice);
            int money=r.Scrap;r.Reroll();Assert.AreEqual(money,r.Scrap);Assert.AreEqual(paid,r.RerollPrice);
            r.Reroll();Assert.Less(r.Scrap,money);r.NextWave();Shop(r);Assert.AreEqual(0,r.RerollPrice);
        }
        [Test] public void PickupDuplicationIncludesBankedBonusAndXp()
        {
            var r=Run();r.GainWeaponStat("chance_double_gold",100);r.BankMaterials(2);r.Collect(r.PrepareMaterialDrop(2));
            Assert.AreEqual(8,r.Scrap);Assert.AreEqual(8,r.XP);Assert.AreEqual(0,r.Banked);
        }
        [Test] public void PickupHealingHappensOncePerPickupNotPerMaterialValue()
        {
            var r=Run();r.GainWeaponStat("heal_when_pickup_gold",100);r.Hurt(4);r.Collect(10);Assert.AreEqual(7,r.Health);
        }
        [Test] public void MaterialDropModifierHasReferenceHalfValueFloorAndFractionalRoll()
        {
            var r=Run();r.GainWeaponStat("gold_drops",-100);
            Assert.AreEqual(1,r.MaterialDropValue(1,false,.49));Assert.AreEqual(1,r.MaterialDropValue(1,false,.5));Assert.AreEqual(0,r.MaterialDropValue(1,false,.5001));
            r.GainWeaponStat("neutral_gold_drops",200);Assert.AreEqual(4,r.MaterialDropValue(2,true,.5));Assert.AreEqual(1,r.MaterialDropValue(2,false,.5));
        }
        [Test] public void BurnIntervalAndDodgeCapsRespectLimits()
        {
            var r=Run();Assert.AreEqual(.6f,r.DodgeLimit);r.GainWeaponStat("dodge_cap",70);Assert.AreEqual(.7f,r.DodgeLimit);
            r.GainWeaponStat("burning_cooldown_reduction",20);Assert.AreEqual(.4f,r.BurningInterval,.001);r.GainWeaponStat("burning_cooldown_reduction",1000);Assert.AreEqual(.1f,r.BurningInterval);
        }
        [Test] public void CriticalKillHealingNeedsBothCriticalAndSuccessfulRoll()
        {
            var r=Run();r.GainWeaponStat("heal_on_crit_kill",20);r.Hurt(3);
            Assert.IsFalse(r.TryCriticalKillHeal(false,0));Assert.IsFalse(r.TryCriticalKillHeal(true,.2001));Assert.IsTrue(r.TryCriticalKillHeal(true,.2));Assert.IsTrue(r.TryCriticalKillHeal(true,.19));Assert.AreEqual(9,r.Health);
            r.GainWeaponStat("damage_against_bosses",25);Assert.AreEqual(12,r.DamageToBoss(10));
        }
        [Test] public void EnemyAndTreeModifiersLeaveLootAliensUnchanged()
        {
            var wave=new WaveEntry{groups=new[]{new SpawnGroup{chance=1,start=1,minimum=1,interval=1,units=new[]{
                new UnitEntry{id="baby_alien",min=10,max=10,chance=1},new UnitEntry{id="tree",min=1,max=1,chance=1}}},new SpawnGroup{chance=1,start=1,loot=true,units=new[]{new UnitEntry{id="looter",min=1,max=1,chance=1}}}}};
            var emitted=new System.Collections.Generic.Dictionary<string,int>();new SpawnDirector(wave,1,50,3).Tick(1,(g,u,n)=>emitted[u.id]=n);
            Assert.AreEqual(15,emitted["baby_alien"]);Assert.AreEqual(4,emitted["tree"]);Assert.AreEqual(1,emitted["looter"]);
        }
    }
}
