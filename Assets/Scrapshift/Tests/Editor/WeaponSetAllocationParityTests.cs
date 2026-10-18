using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
using UnityEngine;

namespace Scrapshift.Tests
{
    public sealed class WeaponSetAllocationParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup(){catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");WeaponSets.Configure(catalog.data.sets);}
        [TearDown] public void Restore()=>WeaponSets.Configure(catalog.data.sets);
        static WeaponSet LegacyFor(Equipment weapon)
        {
            if(weapon?.setIds!=null&&weapon.setIds.Length>0)return WeaponSets.All.FirstOrDefault(s=>s.Id==weapon.setIds[0]);
            switch(weapon?.id){case "pistol":case "smg":case "double_barrel_shotgun":case "laser_gun":return WeaponSets.Gun;case "knife":return WeaponSets.Precise;case "spear":return WeaponSets.Primitive;default:return null;}
        }
        static int LegacyCount(IReadOnlyList<Equipment> inventory,WeaponSet set)
        {int count=0;for(int i=0;i<inventory.Count;i++)if(inventory[i].HasSet(set.Id)||LegacyFor(inventory[i])==set)count++;return count;}
        static float LegacyBonus(WeaponSet set,int count,string stat)=>count<2?0:set.Data==null?(stat==set.Stat?set.Bonus(count):0):set.Data.bonuses.First(b=>b.count==Mathf.Clamp(count,2,6)).effects.Where(e=>e.key==stat).Sum(e=>e.value);
        static float LegacyTotal(IReadOnlyList<Equipment> inventory,string stat)
        {float sum=0;foreach(var set in WeaponSets.All)sum+=LegacyBonus(set,LegacyCount(inventory,set),stat);return sum;}
        [Test] public void EveryCatalogMembershipRetainsConfiguredIdentity()
        {foreach(var weapon in catalog.data.equipment)Assert.AreSame(LegacyFor(weapon),WeaponSets.For(weapon),weapon.id+":"+weapon.tier);}
        [Test] public void CatalogTotalsMatchLegacyAcrossMixedAndDuplicateLoadouts()
        {
            string[] stats=catalog.data.sets.SelectMany(s=>s.bonuses).SelectMany(b=>b.effects).Select(e=>e.key).Distinct().Concat(new[]{"absent_stat"}).ToArray();
            var random=new System.Random(771);
            for(int sample=0;sample<80;sample++)
            {
                var inventory=Enumerable.Range(0,sample%9).Select(_=>catalog.data.equipment[random.Next(catalog.data.equipment.Length)]).ToArray();
                foreach(string stat in stats)Assert.AreEqual(LegacyTotal(inventory,stat),WeaponSets.Bonus(inventory,stat),$"sample {sample}, {stat}");
            }
        }
        [TestCase(null,"pistol")] [TestCase("empty","knife")] [TestCase("missing","pistol")]
        public void LegacyFallbackAndUnknownExplicitSetMatch(string mode,string id)
        {var weapon=new Equipment{id=id,setIds=mode==null?null:mode=="empty"?Array.Empty<string>():new[]{"missing"}};Assert.AreSame(LegacyFor(weapon),WeaponSets.For(weapon));}
        [Test] public void MultipleAndDuplicateMembershipsCountOncePerWeapon()
        {
            var inventory=new[]{new Equipment{id="custom",setIds=new[]{"precise","gun","gun"}},new Equipment{id="pistol",setIds=Array.Empty<string>()}};
            Assert.AreSame(WeaponSets.Precise,WeaponSets.For(inventory[0]));
            Assert.AreEqual(2,WeaponSets.Count(inventory,WeaponSets.Gun));Assert.AreEqual(1,WeaponSets.Count(inventory,WeaponSets.Precise));
        }
        [Test] public void FirstDuplicateConfiguredIdentityWins()
        {
            var rows=new[]{new SetEntry{id="duplicate",name="first",bonuses=new[]{new SetBonusEntry{count=2,effects=new[]{new StatEffect{key="range",value=1}}}}},new SetEntry{id="duplicate",name="second",bonuses=new[]{new SetBonusEntry{count=2,effects=new[]{new StatEffect{key="range",value=2}}}}}};
            WeaponSets.Configure(rows);Assert.AreSame(WeaponSets.All[0],WeaponSets.For(new Equipment{setIds=new[]{"duplicate"}}));
        }
        [Test] public void DuplicateRowsEffectsAndFloatingSumRetainLegacyRules()
        {
            var set=new WeaponSet("test","test","range",7){Data=new SetEntry{bonuses=new[]{
                new SetBonusEntry{count=2,effects=new[]{new StatEffect{key="range",value=100000000},new StatEffect{key="range",value=1},new StatEffect{key="range",value=-100000000},new StatEffect{key="other",value=50}}},
                new SetBonusEntry{count=2,effects=new[]{new StatEffect{key="range",value=99}}},
                new SetBonusEntry{count=6,effects=new[]{new StatEffect{key="range",value=19}}}}}};
            foreach(int count in new[]{-1,0,1,2,6,9})foreach(string stat in new[]{"range","other","absent"})Assert.AreEqual(LegacyBonus(set,count,stat),set.Bonus(count,stat));
        }
        [Test] public void MissingCountStillThrowsInsteadOfSilentlyReturningZero()
        {var set=new WeaponSet("test","test","range",7){Data=new SetEntry{bonuses=Array.Empty<SetBonusEntry>()}};Assert.Throws<InvalidOperationException>(()=>set.Bonus(2,"range"));}
        [Test] public void DataNullUsesUnchangedLegacyIncrementAndStatFilter()
        {var set=new WeaponSet("test","test","range",7);foreach(int count in Enumerable.Range(-1,10))foreach(string stat in new[]{"range","other"})Assert.AreEqual(LegacyBonus(set,count,stat),set.Bonus(count,stat));}
    }
}
