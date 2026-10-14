using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class LoadoutShowcaseTests
    {
        [Test] public void ShowcaseWeaponsAndItemsExistWithExpectedRoles()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            Assert.IsNotNull(catalog);
            Assert.AreEqual(4,ShowcaseLoadouts.All.Length);
            for(int index=0;index<ShowcaseLoadouts.All.Length;index++)
            {
                var loadout=ShowcaseLoadouts.All[index];
                Assert.AreEqual(6,loadout.weapons.Length,loadout.name);
                Assert.AreEqual(6,loadout.weapons.Distinct().Count(),loadout.name);
                Assert.IsTrue(loadout.map>=0&&loadout.map<MapThemes.Names.Length,loadout.name);
                foreach(var id in loadout.weapons)
                {
                    var weapon=catalog.data.equipment.Where(w=>w.id==id).OrderByDescending(w=>w.tier).FirstOrDefault();
                    Assert.IsNotNull(weapon,loadout.name+": "+id);
                    Assert.IsNotNull(weapon.image,loadout.name+": "+id+" image");
                    if(index==0)Assert.IsTrue(weapon.melee,id);
                    if(index==1)Assert.IsFalse(weapon.melee,id);
                }
                foreach(var id in loadout.items)Assert.IsNotNull(Array.Find(catalog.data.items,i=>i.id==id),loadout.name+": "+id);
                Assert.GreaterOrEqual(loadout.items.Count(id=>Array.Find(catalog.data.items,i=>i.id==id)?.appearance?.image!=null),3,loadout.name+" worn items");
            }
        }

        [Test] public void EngineerAndDruidDeployTheirAutomaticHelpers()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var engineer=ShowcaseLoadouts.All[2];
            var druid=ShowcaseLoadouts.All[3];
            Assert.GreaterOrEqual(engineer.items.Count(id=>Array.Find(catalog.data.items,i=>i.id==id)?.conditionalEffects?.Any(e=>e.customKey=="structure"&&e.structure!=null)==true),5);
            Assert.GreaterOrEqual(druid.items.Count(id=>Array.Find(catalog.data.items,i=>i.id==id)?.conditionalEffects?.Any(e=>e.customKey=="pet"&&e.pet!=null)==true),5);
            Assert.AreEqual(0,ShowcaseLoadouts.Index("monk"));
            Assert.AreEqual(1,ShowcaseLoadouts.Index("cyberpunk"));
            Assert.AreEqual(2,ShowcaseLoadouts.Index("engineer"));
            Assert.AreEqual(3,ShowcaseLoadouts.Index("druid"));
        }

        [Test] public void ShowcaseStartsAtWave19WithWave19Enemies()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            Assert.AreEqual(19,ShowcaseLoadouts.Wave);
            Assert.GreaterOrEqual(ShowcaseLoadouts.CrowdLimit,30);
            Assert.IsNotNull(catalog.data.waves[ShowcaseLoadouts.Wave-1]);
            var waveIds=catalog.data.waves[ShowcaseLoadouts.Wave-1].groups
                .SelectMany(group=>group.units).Select(unit=>unit.id).ToArray();
            foreach(var id in ShowcaseLoadouts.Wave19Enemies)
            {
                Assert.Contains(id,waveIds);
                Assert.IsNotNull(catalog.Enemy(id),id);
            }

            var rules=new RunRules(19);
            rules.Configure(catalog);
            rules.Start();
            rules.SetShowcaseWave(ShowcaseLoadouts.Wave);
            Assert.AreEqual(RunPhase.Playing,rules.Phase);
            Assert.AreEqual(19,rules.Wave);
            Assert.AreEqual(RunRules.Durations[18],rules.Remaining);
        }
    }
}
