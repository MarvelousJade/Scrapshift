using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class StructureImplantTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        StructureSpec Spec(string id)=>catalog.data.items.Single(i=>i.id==id).conditionalEffects.Single(e=>e.customKey=="structure").structure;
        [Test] public void NineDeployablesRetainDistinctSourceStatsAndEffects()
        {
            Assert.AreEqual(9,catalog.data.items.Count(i=>i.conditionalEffects.Any(e=>e.customKey=="structure")));
            Assert.AreEqual(3,Spec("turret_healing").stats.damage);Assert.AreEqual(.05f,Spec("turret_healing").stats.scaling.Single().value);Assert.IsTrue(Spec("turret_healing").healing);
            Assert.AreEqual(3,Spec("turret_laser").stats.piercing);Assert.AreEqual(0,Spec("turret_laser").stats.pierceLoss);
            Assert.AreEqual(10,Spec("tyler").stats.pellets);Assert.IsTrue(Spec("tyler").delayed);
            Assert.AreEqual(12,Spec("landmines").spawnInterval);Assert.AreEqual(1.75f*1.4734f,Spec("landmines").explosionRadius,.001);
            Assert.AreEqual(15,Spec("garden").stats.interval);Assert.IsTrue(Spec("wandering_bot").pet);
        }
        [Test] public void FlameStructureUsesEngineeringAndDoesNotGainPlayerPercentDamage()
        {
            var r=new RunRules(1);r.Configure(catalog);r.Start();r.GainWeaponStat("engineering",30);r.GainWeaponStat("elemental_damage",100);r.GainWeaponStat("percent_damage",100);
            var flame=Spec("turret_flame");Assert.IsTrue(r.RollBurn(flame.Equipment,0,out float damage,out int ticks,true));
            // turret_flame_burning_data.tres: 5 + 30 Engineering * .33 = 14.9.
            // WeaponService.apply_scaling_stats_to_damage assigns `as int`
            // before init_burning_data applies the structure percentage.
            Assert.AreEqual(14,damage);Assert.AreEqual(8,ticks);
            Assert.AreEqual(1,SpecialWeaponRules.StructureDamage(r,flame.Equipment));
            r.GainWeaponStat("structure_percent_damage",50);
            Assert.IsTrue(r.RollBurn(flame.Equipment,0,out damage,out ticks,true));
            Assert.AreEqual(21,damage,"Truncate 14.9 to 14, then apply structure +50%; ordinary Damage and Elemental do not enter this native engineering burn.");
        }
        [Test] public void WrenchTiersShareTheSameStructureDefinitionAsTheirMatchingImplants()
        {
            string[] ids={"turret","turret_flame","turret_laser","turret_rocket"};
            for(int i=0;i<4;i++)
            {var actual=catalog.Weapon("wrench",i+1).Trait("turret").structure;var expected=Spec(ids[i]);Assert.AreEqual(expected.kind,actual.kind);Assert.AreEqual(expected.stats.damage,actual.stats.damage);Assert.AreEqual(expected.explosionRadius,actual.explosionRadius);}
        }
    }
}
