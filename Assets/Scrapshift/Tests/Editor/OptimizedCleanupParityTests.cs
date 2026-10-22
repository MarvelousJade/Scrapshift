using NUnit.Framework;
using Scrapshift.Editor;
using UnityEditor;

namespace Scrapshift.Tests
{
    public sealed class OptimizedCleanupParityTests
    {
        RunRules Run()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(771);run.Configure(catalog);run.Start(startingMaterials:100);return run;
        }
        [TestCase(false,100,5)] [TestCase(true,0,5)] [TestCase(true,100,10)]
        public void CleanupBanksSourceCommonBoostOnly(bool optimized,float chance,int expected)
        {
            var run=Run();run.GainWeaponStat("chance_double_gold",chance);run.GainWeaponStat("increase_material_value",100);
            run.GainWeaponStat("heal_when_pickup_gold",100);run.Hurt(2);float hp=run.Health;
            Assert.AreEqual(expected,run.BankCleanupMaterials(5,optimized));Assert.AreEqual(expected,run.Banked);
            Assert.AreEqual(100,run.Scrap);Assert.AreEqual(0,run.XP);Assert.AreEqual(hp,run.Health);
        }
        [Test] public void OptimizedDeathBankingPreservesDeadStateAndDoesNotHarvest()
        {
            var run=Run();run.GainWeaponStat("chance_double_gold",100);run.GainWeaponStat("harvesting",20);run.Hurt(1000);
            Assert.AreEqual(10,run.BankCleanupMaterials(5,true));Assert.AreEqual(10,run.Banked);
            Assert.AreEqual(0,run.Health);Assert.IsTrue(run.DeathCleanupPending);Assert.AreEqual(20,run.Stat("harvesting"));
            Assert.AreEqual(100,run.Scrap);Assert.AreEqual(0,run.XP);
        }
        [Test] public void OptimizedBankingHandlesEachPileIndependentlyAndNeverBanksNegativeValue()
        {
            var run=Run();run.GainWeaponStat("chance_double_gold",100);
            Assert.AreEqual(2,run.BankCleanupMaterials(1,true));Assert.AreEqual(8,run.BankCleanupMaterials(4,true));
            Assert.AreEqual(0,run.BankCleanupMaterials(-2,true));Assert.AreEqual(10,run.Banked);
        }
    }
}
