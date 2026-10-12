using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class SpecialWeaponTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(4);r.Configure(catalog);r.Start();return r;}
        [Test] public void RailGunRestartsItsCountOnDamageAndResetsAtNextWave()
        {
            var r=Run();var w=catalog.Weapon("rail_gun",1);r.Inventory.Clear();r.Inventory.Add(w);float damage=r.WeaponDamage(w);
            r.Tick(5);Assert.AreEqual(damage+3,r.WeaponDamage(w));r.Tick(2);r.Hurt(1);Assert.AreEqual(damage,r.WeaponDamage(w));
            r.Tick(3);Assert.AreEqual(damage,r.WeaponDamage(w));r.Tick(2);Assert.AreEqual(damage+3,r.WeaponDamage(w));
            r.Tick(r.Remaining);Assert.AreEqual(damage,r.WeaponDamage(w));
        }
        [TestCase(2,1)] [TestCase(3,2)] [TestCase(4,3)] public void VorpalUsesTierChance(int tier,int chance)
        {
            var w=catalog.Weapon("vorpal_sword",tier);Assert.AreEqual(chance,w.Trait("one_shot_on_hit").value);
            Assert.IsTrue(SpecialWeaponRules.Execute(w,(chance-.01f)/100));Assert.IsFalse(SpecialWeaponRules.Execute(w,chance/100f+.00001f));
        }
        [Test] public void ParticleSlowUsesNonnegativeEngineeringAndReferenceSoftCap()
        {
            var w=catalog.Weapon("particle_accelerator",4);
            Assert.AreEqual(0,SpecialWeaponRules.EngineeringSlow(w,-10,3));Assert.AreEqual(1.2f,SpecialWeaponRules.EngineeringSlow(w,20,3));
            Assert.AreEqual(2.7f,SpecialWeaponRules.Slow(0,3,100));Assert.AreEqual(2.4f,SpecialWeaponRules.Slow(2.4f,3,2));
            Assert.AreEqual(1.6f,SpecialWeaponRules.Slow(1.5f,3,2),.001);
        }
        [Test] public void StructuresUseEngineeringWithoutPlayerPercentDamage()
        {
            var r=Run();r.GainWeaponStat("engineering",7);r.GainWeaponStat("percent_damage",1000);
            var mine=catalog.Weapon("screwdriver",1).Trait("landmine");
            Assert.AreEqual(17,SpecialWeaponRules.StructureDamage(r,mine.projectile.AsEquipment()));Assert.AreEqual(2.57845f,mine.radius,.00001);
            for(int tier=1;tier<=4;tier++)Assert.AreEqual(15-tier*3,catalog.Weapon("screwdriver",tier).Trait("landmine").spawnInterval);
        }
        [Test] public void GardenUsesTierIntervalsAndTorchSpreadIsGlobal()
        {
            float[] intervals={15,14,12,10};
            for(int tier=1;tier<=4;tier++)Assert.AreEqual(intervals[tier-1],catalog.Weapon("pruner",tier).Trait("garden").projectile.interval);
            var r=Run();r.Inventory.Add(catalog.Weapon("torch",3));r.Inventory.Add(catalog.Weapon("torch",4));Assert.AreEqual(2,r.Stat("burning_spread"));
            Assert.NotNull(catalog.Weapon("thunder_sword",3).Trait("effect_slow_projectiles_on_hit").projectile);
        }
    }
}
