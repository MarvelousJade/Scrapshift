using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class FullBaseTests
    {
        ReferenceCatalog catalog;UnlockDefinition[] unlocks;
        [SetUp] public void Setup(){catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");unlocks=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;}
        [TearDown] public void ResetGeometry()=>ArenaGeometry.SetMapSize(0);
        RunRules Run(string id=null){var r=new RunRules(42);r.Configure(catalog);var c=catalog.data.characters.FirstOrDefault(x=>x.id==id);r.Start(c,c==null?null:StarterChoices.Available(catalog,c).FirstOrDefault());return r;}
        ItemEntry Item(string id)=>catalog.data.items.Single(i=>i.id==id);
        void Shop(RunRules r){r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);Assert.AreEqual(RunPhase.Shop,r.Phase);}
        void Buy(RunRules r,string id){r.Shop[0]=new ShopOffer{item=Item(id)};Assert.IsTrue(r.Buy(0),id);}
        [Test] public void PacifistCountsCulledEnemiesAndHalvesHordeIncome()
        {
            var normal=Run("pacifist");normal.LivingEnemies=10;normal.RemovedEnemies=11;int harvest=(int)normal.Stat("harvesting");normal.Tick(normal.Remaining);Assert.AreEqual(harvest+14,normal.Scrap);
            var horde=Run("pacifist");horde.LivingEnemies=10;horde.RemovedEnemies=11;horde.IsHordeWave=true;horde.Tick(horde.Remaining);Assert.AreEqual(harvest+7,horde.Scrap);
        }
        [Test] public void EveryLockedResourceHasOneReachableRewardDefinition()
        {
            Assert.AreEqual(115,unlocks.Length);Assert.AreEqual(115,unlocks.Select(d=>d.id).Distinct().Count());
            foreach(var c in catalog.data.characters.Where(c=>!c.available))Assert.IsTrue(unlocks.Any(d=>d.reward=="character_"+c.id),c.id);
            foreach(var i in catalog.data.items.Where(i=>!i.available))Assert.IsTrue(unlocks.Any(d=>d.reward=="item_"+i.id),i.id);
            foreach(var w in catalog.data.equipment.Where(w=>!w.available))Assert.IsTrue(unlocks.Any(d=>d.reward=="weapon_"+w.id),w.id);
        }
        [Test] public void UnlocksPersistCountersAndNeverDuplicateRewards()
        {
            string dir=Path.Combine(Path.GetTempPath(),"Scrapshift-unlock-test-"+Guid.NewGuid()),file=Path.Combine(dir,"progress.json");
            try{var p=new Progression(unlocks,file);p.Count("kills",299);Assert.IsFalse(p.Unlocked("character_old"));p.Count("kills");p.Count("materials",300);p.Finish(true,"well_rounded",0,false);p.Finish(true,"well_rounded",0,false);
                var reloaded=new Progression(unlocks,file);Assert.AreEqual(300,reloaded.Total("kills"));Assert.IsTrue(reloaded.Unlocked("character_old"));Assert.IsTrue(reloaded.Unlocked("character_lucky"));Assert.IsTrue(reloaded.Unlocked("character_one_arm"));Assert.IsTrue(reloaded.Unlocked("item_candy_bag"));Assert.AreEqual(1,reloaded.Difficulty);Assert.AreEqual(reloaded.Data.completed.Count,reloaded.Data.completed.Distinct().Count());}
            finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void BurnAndProjectileChallengesResetAtWaveBoundaries()
        {
            var r=Run();var p=new Progression(unlocks);r.Progress=p;for(int i=0;i<29;i++)r.RecordBurnKill();r.RecordProjectileHit();r.RecordProjectileHit();Shop(r);r.NextWave();r.RecordBurnKill();r.RecordProjectileHit();Assert.IsFalse(p.Completed("chal_will_o_the_wisp"));Assert.IsFalse(p.Completed("chal_jellyshield"));
            for(int i=0;i<29;i++)r.RecordBurnKill();r.RecordProjectileHit();r.RecordProjectileHit();Assert.IsTrue(p.Completed("chal_will_o_the_wisp"));Assert.IsTrue(p.Completed("chal_jellyshield"));
        }
        [Test] public void FullRosterStartsSafelyAndRespectsItsWeaponCapacity()
        {
            foreach(var c in catalog.data.characters){var r=Run(c.id);Assert.LessOrEqual(r.Inventory.Count,r.WeaponSlots,c.id);Assert.Greater(r.Health,0,c.id);foreach(var w in r.Inventory)Assert.IsTrue(c.Allows(w),c.id);r.SetMoving(true);r.Tick(.1f);foreach(var key in StatPresentation.Keys)Assert.IsFalse(float.IsNaN(r.Stat(key)),c.id+key);}
            Assert.AreEqual(0,Run("bull").Inventory.Count);Assert.AreEqual(1,Run("one_arm").Inventory.Count);Assert.AreEqual(12,Run("multitasker").WeaponSlots);Assert.AreEqual(1,Run("baby").WeaponSlots);
        }
        [Test] public void AllSixteenMissingImplantsHaveSerializedExecutableEffects()
        {
            foreach(var id in new[]{"bait","blazemander","bonk_dog","bot_o_mine","broken_hourglass","campfire","candle","candy_bag","catling_gun","doc_moth","greek_fire","hourglass","jellyshield","lootworm","ratzilla","scapegoat"})
            {var item=Item(id);Assert.NotNull(item.image,id);Assert.IsTrue(item.effects.Length>0||item.conditionalEffects.Length>0,id);Assert.IsFalse(new InspectionText(catalog.uiText).ItemDescription(item).Contains("EFFECT_"),id);}
        }
        [Test] public void GreekFireUsesCurrentHealthAndBossReduction()
        {var r=Run();r.Items.Add(Item("greek_fire"));Assert.AreEqual(10,r.BurnCurrentHealthDamage(100,false));Assert.AreEqual(1,r.BurnCurrentHealthDamage(100,true));Assert.AreEqual(5,r.BurnCurrentHealthDamage(50,false));}
        [Test] public void TemporalReplayConsumesItselfAndStartsRepeatedWaveAtOneHp()
        {
            var r=Run();r.GainWeaponStat("harvesting",1000);Shop(r);Buy(r,"hourglass");int wave=r.Wave;r.NextWave();Assert.AreEqual(wave,r.Wave);Assert.AreEqual(1,r.Health);Assert.IsFalse(r.Items.Any(i=>i.id=="hourglass"));Assert.IsTrue(r.Items.Any(i=>i.id=="broken_hourglass"));Shop(r);r.NextWave();Assert.AreEqual(wave+1,r.Wave);Assert.AreEqual(r.MaxHealth,r.Health);
        }
        [Test] public void ThreatBeaconQueuesOnlyTheFollowingWave()
        {
            var r=Run("fisherman");r.GainWeaponStat("harvesting",1000);Shop(r);float damage=r.Stat("percent_damage");Buy(r,"bait");Assert.AreEqual(damage+8,r.Stat("percent_damage"));r.NextWave();Assert.AreEqual(1,r.ExtraBaitGroups);Shop(r);r.NextWave();Assert.AreEqual(0,r.ExtraBaitGroups);
        }
        [Test] public void CandyCacheAddsEightPrimaryPointsPerWave()
        {var r=Run();r.Items.Add(Item("candy_bag"));Shop(r);float total=StatPresentation.Keys.Sum(r.Stat);r.NextWave();Assert.AreEqual(total+8,StatPresentation.Keys.Sum(r.Stat));}
        [Test] public void CompanionStatsAndBeastMasterScalingKeepSourceCoefficients()
        {
            var ordinary=Run();var beast=Run("beast_master");
            foreach(var id in new[]{"bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla"}){var pet=Item(id).conditionalEffects.First(e=>e.customKey=="pet").pet;Assert.Greater(pet.stats.interval,0,id);var weapon=ordinary.PetWeapon(pet.stats,pet);Assert.AreEqual(pet.stats.damage,weapon.damage);var boosted=beast.PetWeapon(pet.stats,pet);foreach(var stat in new[]{"melee_damage","ranged_damage","elemental_damage","engineering"})Assert.IsTrue(boosted.scaling.Any(s=>s.key==stat),id+stat);}
            ordinary.GainWeaponStat("attack_speed",500);Assert.AreEqual(1,ordinary.PetCooldown(1));ordinary.HealingDrones=1;Assert.AreEqual(2,ordinary.RegenerationHealMultiplier);
        }
        [Test] public void SourceMapTileRoundingAppliesToVeteranAndSurveyor()
        {ArenaGeometry.SetMapSize(-33);Assert.AreEqual(new Vector2(21*.32f,16*.32f),ArenaGeometry.HalfSize);ArenaGeometry.SetMapSize(33);Assert.AreEqual(new Vector2(42*.32f,31*.32f),ArenaGeometry.HalfSize);}
        [Test] public void AllSixteenPrimaryUpgradeTracksAndTiersExist()
        {CollectionAssert.AreEquivalent(StatPresentation.Keys,catalog.data.upgrades.Select(u=>u.stat));Assert.AreEqual(16,catalog.data.upgrades.Length);}
        [Test] public void NightmareRequiresTenDistinctDangerFiveWins()
        {var p=new Progression(unlocks);for(int i=0;i<9;i++)p.Finish(true,catalog.data.characters[i].id,5,true);Assert.AreEqual(5,p.Difficulty);p.Finish(true,catalog.data.characters[0].id,5,true);Assert.AreEqual(5,p.Difficulty);p.Finish(true,catalog.data.characters[9].id,5,true);Assert.AreEqual(6,p.Difficulty);}
        [Test] public void DangerSchedulesIncludeRealHordeGroupsAndRegisteredEnemies()
        {
            var data=JsonUtility.FromJson<BaseWaveCatalog>(Resources.Load<TextAsset>("base-waves").text);var extra=JsonUtility.FromJson<ExtendedEncounterData>(Resources.Load<TextAsset>("extended-encounters").text);var known=catalog.data.enemies.Select(e=>e.id).Concat(extra.enemies.Select(e=>e.id)).ToHashSet();
            Assert.AreEqual(20,data.waves.Length);Assert.AreEqual(5,data.hordes.Length);
            for(int wave=1;wave<=20;wave++)for(int danger=0;danger<=6;danger++)foreach(bool horde in new[]{false,true})foreach(var g in WaveSelection.For(data,wave,danger,false,horde).groups){Assert.IsFalse(g.boss);foreach(var u in g.units)Assert.IsTrue(known.Contains(u.id),u.id);}
            Assert.Greater(WaveSelection.For(data,3,1,false,false).groups.Length,WaveSelection.For(data,3,0,false,false).groups.Length);
            Assert.IsTrue(WaveSelection.For(data,11,5,false,true).groups.Any(g=>g.repeats==999&&g.reduce==1));
        }
        [Test] public void NightmarePatternsComeFromTwelveRegisteredDirectionalResources()
        {var data=JsonUtility.FromJson<ProjectilePatternCatalog>(Resources.Load<TextAsset>("projectile-patterns").text);Assert.AreEqual(12,data.patterns.Length);foreach(var p in data.patterns){Assert.Greater(p.rate,1);Assert.Greater(p.generators.Length,0);foreach(var g in p.generators){Assert.Greater(g.direction.sqrMagnitude,0);Assert.Greater(g.rate,0);}}}
        [Test] public void SoldierAttackAndDemonPurchaseRulesAreDistinct()
        {var s=Run("soldier");Assert.IsTrue(s.CanAttack);s.SetMoving(true);Assert.IsFalse(s.CanAttack);s.SetMoving(false);Assert.IsTrue(s.CanAttack);var d=Run("demon");Shop(d);d.Shop[0]=new ShopOffer{item=Item("coffee")};int price=d.Price(d.Shop[0]);float hp=d.MaxHealth;Assert.IsTrue(d.Buy(0));Assert.AreEqual(hp-price,d.MaxHealth);}
    }
}
