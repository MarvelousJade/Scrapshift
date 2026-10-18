using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class UnlockParityTests
    {
        ReferenceCatalog catalog;
        UnlockDefinition[] definitions;
        [SetUp] public void Setup()
        {
            catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
        }
        RunRules Run(Progression progress)
        {
            var run=new RunRules(42){Progress=progress};run.Configure(catalog);run.Start();return run;
        }
        [TestCase(15,true)]
        [TestCase(15.001f,false)]
        public void GiantSlayerIncludesTheReferenceFifteenSecondBoundary(float lifetime,bool unlocked)
        {
            var progress=new Progression(definitions);var run=Run(progress);
            run.RecordEnemy("croc",false,lifetime,true);
            Assert.AreEqual(unlocked,progress.Completed("chal_giant_slayer"));
        }
        [Test] public void QuickOrdinaryEnemyKillsDoNotUnlockGiantSlayer()
        {
            var progress=new Progression(definitions);Run(progress).RecordEnemy("baby_alien",false,1,false);
            Assert.IsFalse(progress.Completed("chal_giant_slayer"));
        }
        [TestCase(1,true)]
        [TestCase(.5f,false)]
        [TestCase(0,false)]
        public void RecklessRequiresExactlyOneHealth(float hp,bool unlocked)
        {
            var progress=new Progression(definitions);var run=Run(progress);
            typeof(RunRules).GetProperty("Health").SetValue(run,hp);
            run.RecordWaveEnd(0,0);Assert.AreEqual(unlocked,progress.Completed("chal_reckless"));
        }
        [Test] public void LastCompanionChallengeImmediatelyUnlocksBeastMaster()
        {
            var progress=new Progression(definitions);var run=Run(progress);
            foreach(string id in new[]{"chal_bonk_dog","chal_catling_gun","chal_bot_o_mine","chal_blazemander"})progress.Complete(id);
            run.RecordWaveEnd(0,19);Assert.IsFalse(progress.Completed("chal_paws_n_claws"));
            run.RecordWaveEnd(0,20);Assert.IsTrue(progress.Completed("chal_lootworm"));
            Assert.IsTrue(progress.Unlocked("character_beast_master"));
        }
        [Test] public void LastScalingWeaponUnlocksCompanionAndBeastMasterInSameCheck()
        {
            var progress=new Progression(definitions);var run=Run(progress);
            foreach(string id in new[]{"chal_bonk_dog","chal_bot_o_mine","chal_blazemander","chal_lootworm"})progress.Complete(id);
            run.Inventory.Clear();for(int i=0;i<6;i++)run.Inventory.Add(catalog.Weapon("pistol",1));
            progress.CheckStats(run);
            Assert.IsTrue(progress.Completed("chal_catling_gun"));
            Assert.IsTrue(progress.Unlocked("character_beast_master"));
        }
        [Test] public void CharacterRecordsKeepHighestDangerAcrossWinsLossesAndReload()
        {
            string directory=Path.Combine(Path.GetTempPath(),"Scrapshift-unlock-parity-"+Guid.NewGuid());
            string file=Path.Combine(directory,"progress.json");
            try
            {
                var progress=new Progression(definitions,file);Assert.AreEqual(-1,progress.BestDifficulty("well_rounded"));
                progress.Finish(true,"well_rounded",3,true);
                progress.Finish(true,"well_rounded",0,true);
                progress.Finish(false,"well_rounded",5,true);
                var reload=new Progression(definitions,file);
                Assert.AreEqual(3,reload.BestDifficulty("well_rounded"));
                Assert.AreEqual(2,reload.Total("wins:well_rounded"));
                Assert.AreEqual("Best: Danger 3",reload.CharacterRecord("well_rounded"));
                Assert.AreEqual(-1,reload.BestDifficulty("ranger"));
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test] public void LegacyCharacterWinsKeepKnownRecordWithoutInventingHigherDanger()
        {
            var progress=new Progression(definitions);
            progress.Complete("chal_well_rounded");progress.Count("danger5:ranger");
            Assert.AreEqual(-1,progress.BestDifficulty("well_rounded"));
            StringAssert.Contains("Danger not recorded",progress.CharacterRecord("well_rounded"));
            Assert.AreEqual(5,progress.BestDifficulty("ranger"));
            Assert.AreEqual(-1,progress.BestDifficulty("mage"));
        }
        [Test] public void MedicineRequirementDescribesOneWave()
        {
            StringAssert.Contains("one wave",new Progression(definitions).Requirement("character_doctor"));
        }
        [Test] public void MedicineHealingDoesNotCarryIntoTheNextWave()
        {
            var progress=new Progression(definitions);var run=Run(progress);
            run.GainWeaponStat("max_hp",500);
            typeof(RunRules).GetProperty("Health").SetValue(run,1f);run.Heal(199);
            Assert.IsFalse(progress.Completed("chal_medicine"));
            run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);run.NextWave();
            typeof(RunRules).GetProperty("Health").SetValue(run,1f);run.Heal(1);
            Assert.IsFalse(progress.Completed("chal_medicine"));
            run.Heal(199);Assert.IsTrue(progress.Completed("chal_medicine"));
        }
        [Test] public void EndingRunVoluntarilyDoesNotUnlockFirstDeathReward()
        {
            var progress=new Progression(definitions);var run=Run(progress);
            run.Pause();run.Abandon();
            progress.Finish(false,"well_rounded",0,false,!run.Abandoned);
            Assert.IsFalse(progress.Completed("chal_rookie"));
            run.Start();Assert.IsFalse(run.Abandoned);
            progress.Finish(false,"well_rounded",0,false);
            Assert.IsTrue(progress.Completed("chal_rookie"));
        }
        [Test] public void StatUnlockUsesUnderlyingMaxHpEvenWhenAnItemCapsUsableHealth()
        {
            var progress=new Progression(definitions);var run=Run(progress);
            run.GrantItem(new ItemEntry{id="unlock-test-hp-cap",effects=Array.Empty<StatEffect>(),
                conditionalEffects=new[]{new ConditionalEffect{customKey="stat_cap",key="hp_cap"}}});
            run.GainWeaponStat("max_hp",90);progress.CheckStats(run);
            Assert.AreEqual(10,run.MaxHealth);Assert.AreEqual(100,run.Stat("max_hp"));
            Assert.IsTrue(progress.Completed("chal_robust"));
        }
    }
}
