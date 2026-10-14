using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Tests
{
    public sealed class PerformanceRulesTests
    {
        [Serializable] sealed class CollisionEntry { public string id; public float[] radii, hurt, contact; }
        [Serializable] sealed class CollisionTable { public CollisionEntry[] entries; }

        [Test] public void IndexedGeometryPreservesEveryImportedRadiusAndOffset()
        {
            var entries=JsonUtility.FromJson<CollisionTable>(Resources.Load<TextAsset>("enemy-collisions").text).entries;
            foreach(var entry in entries)
            {
                var geometry=CombatGeometry.Enemy(entry.id);
                Assert.AreEqual(entry.radii[0],geometry.body,entry.id);
                Assert.AreEqual(entry.radii[1],geometry.hurt,entry.id);
                Assert.AreEqual(entry.radii[2],geometry.contact,entry.id);
                Assert.AreEqual(new Vector2(entry.hurt[0],entry.hurt[1]),CombatGeometry.HurtOffset(entry.id),entry.id);
                Assert.AreEqual(new Vector2(entry.contact[0],entry.contact[1]),CombatGeometry.ContactOffset(entry.id),entry.id);
            }
            Assert.Throws<ArgumentException>(()=>CombatGeometry.Enemy("unknown"));
            Assert.AreEqual(Vector2.down*.15f,CombatGeometry.HurtOffset("unknown"));
            Assert.AreEqual(Vector2.down*.24f,CombatGeometry.ContactOffset("unknown"));
        }

        [Test] public void EverySetThresholdMatchesResourceTotalsIncludingDuplicatesAndFractionalRounding()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>("Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset");
            WeaponSets.Configure(catalog.data.sets);
            foreach(var set in WeaponSets.All)
            foreach(string stat in set.Data.bonuses.SelectMany(b=>b.effects).Select(e=>e.key).Distinct().Concat(new[]{"missing"}))
            for(int count=0;count<=24;count++)
            {
                float expected=count<2?0:set.Data.bonuses.First(b=>b.count==Mathf.Clamp(count,2,6)).effects.Where(e=>e.key==stat).Sum(e=>e.value);
                Assert.AreEqual(expected,set.Bonus(count,stat),set.Id+"/"+stat+"/"+count);
            }
            var custom=new WeaponSet("test","Test","armor",3) { Data=new SetEntry { bonuses=new[]{new SetBonusEntry {count=2,effects=new[]{
                new StatEffect{key="armor",value=16777216},new StatEffect{key="armor",value=1},
                new StatEffect{key="armor",value=-16777216},new StatEffect{key="speed",value=5}}}}}};
            Assert.AreEqual(1,custom.Bonus(2,"armor"));
            Assert.AreEqual(5,custom.Bonus(2,"speed"));
            custom.Data=null;
            Assert.AreEqual(3,custom.Bonus(2,"armor"));
            Assert.AreEqual(0,custom.Bonus(2,"speed"));
        }

        [Test] public void LiveInventoryAndSetChangesRemainVisibleWithoutInvalidation()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>("Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset");
            WeaponSets.Configure(catalog.data.sets);
            var inventory=new List<Equipment>{catalog.Weapon("pistol",1)};
            Assert.AreEqual(0,WeaponSets.Bonus(inventory,"range"));
            inventory.Add(catalog.Weapon("smg",1));
            Assert.AreEqual(10,WeaponSets.Bonus(inventory,"range"));
            inventory.RemoveAt(0);
            Assert.AreEqual(0,WeaponSets.Bonus(inventory,"range"));
            Assert.AreEqual(WeaponSets.Gun,WeaponSets.For(new Equipment{id="pistol"}));
            Assert.IsNull(WeaponSets.For(new Equipment{id="pistol",setIds=new[]{"unknown"}}));
            Assert.IsNull(WeaponSets.For(null));
        }

        [Test] public void FixtureDetectionStillMatchesProcessArguments()
        {
            Assert.AreEqual(Array.Exists(Environment.GetCommandLineArgs(),a=>a.StartsWith("--")),GameSettings.IsFixture);
        }
    }
}
