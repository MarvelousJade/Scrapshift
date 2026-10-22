using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class EquipmentStatOptimizationTests
    {
        static readonly MethodInfo Query=typeof(RunRules).GetMethod("EquipmentStat",BindingFlags.Instance|BindingFlags.NonPublic);
        RunRules run;
        float EquipmentStat(string key)=>(float)Query.Invoke(run,new object[]{key});
        [SetUp] public void Setup()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            run=new RunRules(42);run.Configure(catalog);run.Start();run.Inventory.Clear();
        }
        [Test] public void DirectAndPrefixedKeysSumOnceAndRetainNullAndCustomKeyFiltering()
        {
            run.Inventory.Add(new Equipment{traits=null});
            run.Inventory.Add(new Equipment{traits=new[]{
                new WeaponTrait{key="armor",value=2,customKey=null},
                new WeaponTrait{key="stat_armor",value=1.5f,customKey=""},
                new WeaponTrait{key="stat_armor",value=100,customKey="stats_below_half_health"},
                new WeaponTrait{key="stat_speed",value=50},
                new WeaponTrait{key=null,value=4},
                new WeaponTrait{key="stat_",value=6}}});
            Assert.AreEqual(3.5f,EquipmentStat("armor"));
            Assert.AreEqual(10,EquipmentStat(null),"A null query retains both its direct-null and concatenated stat_ matches.");
            Assert.AreEqual(0,EquipmentStat("engineering"));
        }
        [Test] public void HealthFractionsAndLiveInventoryTraitChangesRemainVisibleOnEveryQuery()
        {
            var direct=new WeaponTrait{key="life_steal",per=20,value=2};
            var prefixed=new WeaponTrait{key="stat_life_steal",per=25,value=3};
            var weapon=new Equipment{traits=new[]{direct,prefixed,new WeaponTrait{key="life_steal",value=1.5f}}};
            run.Inventory.Add(weapon);
            Assert.AreEqual(1.5f,EquipmentStat("life_steal"));
            run.Hurt(run.MaxHealth/2);
            Assert.AreEqual(11.5f,EquipmentStat("life_steal"));
            run.Heal(run.MaxHealth/4);
            Assert.AreEqual(6.5f,EquipmentStat("life_steal"));
            prefixed.value=7;
            Assert.AreEqual(10.5f,EquipmentStat("life_steal"),"Trait edits are evaluated live rather than cached.");
            run.Inventory.Remove(weapon);
            Assert.AreEqual(0,EquipmentStat("life_steal"));
            run.Inventory.Add(weapon);
            Assert.AreEqual(10.5f,EquipmentStat("life_steal"));
        }
    }
}
