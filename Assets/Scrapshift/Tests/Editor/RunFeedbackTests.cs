using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class RunFeedbackTests
    {
        [Test] public void DetectionPrecedesAttackWithoutExtendingTheAttackGate()
        {
            float range=4;Assert.AreEqual(6,RunFeedback.DetectionRange(range));
            Assert.IsFalse(RunFeedback.CanAttack(range,5*5));Assert.IsTrue(RunFeedback.CanAttack(range,4.5f*4.5f));
            var tree=RunFeedback.SpawnColor(true,.5f);var enemy=RunFeedback.SpawnColor(false,.5f);
            Assert.Greater(tree.g,tree.r);Assert.Greater(enemy.r,enemy.g);Assert.AreEqual(.5f,tree.a);
        }
        [Test] public void StoredMaterialsAndHarvestRemainSeparateAndCratesResolveBeforeLevels()
        {
            var c=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var r=new RunRules(42);r.Configure(c);r.Start();r.GainWeaponStat("harvesting",10);
            r.Collect(20);int money=r.Scrap,xp=r.XP;r.BankMaterials(12);
            Assert.AreEqual(money,r.Scrap);Assert.AreEqual(xp,r.XP);Assert.AreEqual(12,r.Banked);
            r.CollectConsumable(ConsumableKind.Crate);r.Tick(r.Remaining);
            Assert.AreEqual(RunPhase.Loot,r.Phase);Assert.NotNull(r.CrateItem);Assert.Greater(r.PendingLevels,0);
            Assert.AreEqual(money+10,r.Scrap);Assert.AreEqual(11,r.Stat("harvesting"));
            r.ResolveCrate(true);Assert.AreEqual(RunPhase.Upgrade,r.Phase);Assert.AreEqual(0,r.PendingCrates);
            Assert.IsFalse(r.ResolveCrate(true));
        }
    }
}
