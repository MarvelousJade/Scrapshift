using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class DiscoveryTests
    {
        ReferenceCatalog catalog;UnlockDefinition[] definitions;
        [SetUp] public void Setup(){catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;}
        RunRules Run(Progression p){var r=new RunRules(42){Progress=p};r.Configure(catalog);r.Start();return r;}
        [Test] public void StartingWeaponsAreDiscoveredWithoutCompletingRewardChallenges()
        {var p=new Progression(definitions);var r=Run(p);Assert.AreEqual(1,p.Total("weapon:pistol"));Assert.AreEqual(0,p.Data.completed.Count);Assert.AreEqual(0,p.Total("weapon:wand"));}
        [Test] public void SuccessfulPurchasesCountButFailedPurchasesDoNot()
        {
            var p=new Progression(definitions);var r=Run(p);r.Tick(r.Remaining);var coffee=catalog.data.items.Single(i=>i.id=="coffee");r.Shop[0]=new ShopOffer{item=coffee};Assert.IsFalse(r.Buy(0));Assert.AreEqual(0,p.Total("item:coffee"));
            r.NextWave();r.GainWeaponStat("harvesting",10000);r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);r.Shop[0]=new ShopOffer{item=coffee};Assert.IsTrue(r.Buy(0));Assert.AreEqual(1,p.Total("item:coffee"));r.Shop[0]=new ShopOffer{weapon=catalog.Weapon("wand",4)};Assert.IsTrue(r.Buy(0));Assert.AreEqual(1,p.Total("weapon:wand"));
        }
        [Test] public void EnemyDiscoveryTracksSpeciesWithoutCountingTreesAsEnemyKills()
        {var p=new Progression(definitions);var r=Run(p);r.RecordEnemy("baby_alien",false,20,false);r.RecordEnemy("tree",true,20,false);Assert.AreEqual(1,p.Total("enemy:baby_alien"));Assert.AreEqual(1,p.Total("enemy:tree"));Assert.AreEqual(1,p.Total("kills"));Assert.AreEqual(1,p.Total("trees"));}
        [Test] public void DiscoveryAndUnlockAreIndependentAndSurviveReload()
        {
            string dir=Path.Combine(Path.GetTempPath(),"Scrapshift-discovery-"+Guid.NewGuid()),file=Path.Combine(dir,"progress.json");
            try{var p=new Progression(definitions,file);var reward=definitions.First(d=>d.reward.StartsWith("item_"));p.Complete(reward.id);Assert.AreEqual(0,p.Total("item:"+reward.reward.Substring(5)));p.Count("weapon:wand",2);p.Count("enemy:baby_alien",7);p.Save();var reload=new Progression(definitions,file);Assert.IsTrue(reload.Unlocked(reward.reward));Assert.AreEqual(2,reload.Total("weapon:wand"));Assert.AreEqual(7,reload.Total("enemy:baby_alien"));Assert.AreEqual(0,reload.NewlyUnlocked.Count);}
            finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void EveryWeaponInlineStatIconResolvesIncludingElementalAndConditionalBonuses()
        {
            var text=new InspectionText(catalog.uiText);var r=Run(new Progression(definitions));
            foreach(var w in catalog.data.equipment)foreach(Match m in Regex.Matches(text.WeaponDescription(r,w),@"\[\[([^\]]+)\]\]"))Assert.NotNull(StatPresentation.Icon(catalog,m.Groups[1].Value),w.id+" "+m.Value);
            StringAssert.Contains("[[elemental_damage]]",text.WeaponDescription(r,catalog.Weapon("wand",1)));
            StringAssert.Contains("[[percent_damage]]",text.WeaponDescription(r,catalog.Weapon("jousting_lance",4)));
            Assert.AreSame(StatPresentation.Icon(catalog,"elemental_damage"),StatPresentation.Icon(catalog,"stat_elemental_damage"));
        }
    }
}
