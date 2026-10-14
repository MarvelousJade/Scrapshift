using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
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
            Assert.AreEqual(4,ShowcaseLoadouts.All.Select(loadout=>loadout.map).Distinct().Count());
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

        [Test] public void ShowcaseCombatBonusesComeFromOwnedItems()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var ranger=new RunRules(19);ranger.Configure(catalog);ranger.Start();
            foreach(var id in ShowcaseLoadouts.All[1].items)ranger.GrantItem(Array.Find(catalog.data.items,item=>item.id==id));
            Assert.AreEqual(0,ranger.Stat("projectiles"),"The showcase does not inject extra shots.");
            Assert.AreEqual(2,ranger.Stat("piercing"));
            Assert.AreEqual(2,ranger.Stat("bounce"));
            Assert.GreaterOrEqual(ranger.Stat("attack_speed"),50);

            var engineer=new RunRules(19);engineer.Configure(catalog);engineer.Start();
            foreach(var id in ShowcaseLoadouts.All[2].items)engineer.GrantItem(Array.Find(catalog.data.items,item=>item.id==id));
            Assert.AreEqual(0,engineer.Stat("projectiles"));
            Assert.GreaterOrEqual(engineer.StructureAttackSpeed,40);
            Assert.Greater(engineer.Stat("engineering"),0);
            Assert.Greater(engineer.Stat("structures_can_crit"),0);
            Assert.GreaterOrEqual(engineer.Effects("structure").Count(),9);
            Assert.GreaterOrEqual(engineer.Effects("pet").Count(),3);
            Assert.GreaterOrEqual(ShowcaseLoadouts.All[3].items.Count(id=>Array.Find(catalog.data.items,item=>item.id==id)?.conditionalEffects?.Any(effect=>effect.customKey=="pet"&&effect.pet!=null)==true),10);
            var druid=new RunRules(19);druid.Configure(catalog);druid.Start();
            foreach(var id in ShowcaseLoadouts.All[3].items)druid.GrantItem(Array.Find(catalog.data.items,item=>item.id==id));
            Assert.GreaterOrEqual(druid.StructureAttackSpeed,30);
            Assert.Greater(druid.Stat("engineering"),0);
        }

        [Test] public void ShowcaseHelpersWaitForWaveStartAndReleaseInSourceCadence()
        {
            Assert.AreEqual(0,ShowcaseSupportTiming.ReleasedBy(0));
            Assert.AreEqual(0,ShowcaseSupportTiming.ReleasedBy(1.049f));
            Assert.AreEqual(1,ShowcaseSupportTiming.ReleasedBy(1.05f));
            Assert.AreEqual(1,ShowcaseSupportTiming.ReleasedBy(1.099f));
            Assert.AreEqual(2,ShowcaseSupportTiming.ReleasedBy(1.10f));
        }

        [Test] public void ShowcaseUsesTheRealWave19ScheduleAndCountdown()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            Assert.AreEqual(19,ShowcaseLoadouts.Wave);
            var source=JsonUtility.FromJson<BaseWaveCatalog>(Resources.Load<TextAsset>("base-waves").text);
            var wave=WaveSelection.For(source,ShowcaseLoadouts.Wave,0,false,false);
            Assert.AreEqual(60,wave.duration);
            Assert.AreEqual(100,wave.cap);
            foreach(var id in wave.groups.SelectMany(group=>group.units).Select(unit=>unit.id).Distinct())
                Assert.IsNotNull(catalog.Enemy(id),id);
            int spawned=0;
            var director=new SpawnDirector(wave,19);
            director.Tick(.5f,(group,unit,count)=>spawned+=count);
            Assert.AreEqual(0,spawned,"The real schedule has no immediate crowd.");
            director.Tick(7.5f,(group,unit,count)=>spawned+=count);
            Assert.Greater(spawned,36,"The real wave can exceed the old showcase crowd limit.");

            var rules=new RunRules(19);
            rules.Configure(catalog);
            rules.Start();
            rules.SetShowcaseWave(ShowcaseLoadouts.Wave);
            Assert.AreEqual(RunPhase.Playing,rules.Phase);
            Assert.AreEqual(19,rules.Wave);
            Assert.AreEqual(RunRules.Durations[18],rules.Remaining);
            rules.Tick(1);
            Assert.AreEqual(59,rules.Remaining);
        }
    }
}
