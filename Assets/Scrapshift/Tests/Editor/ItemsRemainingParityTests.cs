using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemsRemainingParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void SetUp()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");

        [Test] public void RenegadeGainScalesDistinctCommonItemLinkThroughAcquireAndRemove()
        {
            // linked_stats.gd adds each truncated raw link into LinkedStats;
            // inherited TempStats.get_stat multiplies the result by the target
            // stat's gain. Renegade has -80% gain_percent_damage and +10 raw
            // Damage per distinct common item, yielding +2 effective Damage.
            // Coffee also has -2 direct Damage per copy, scaled to -0.4.
            var character=catalog.data.characters.Single(c=>c.id=="renegade");
            var coffee=catalog.data.items.Single(i=>i.id=="coffee");
            var beanie=catalog.data.items.Single(i=>i.id=="beanie");
            var run=new RunRules(1701);run.Configure(catalog);run.Start(character,"pistol");
            float baseline=run.Stat("percent_damage");
            Assert.AreEqual(.2f,character.Gain("percent_damage"),.0001f);
            Assert.AreEqual(1,coffee.tier);Assert.AreEqual(1,beanie.tier);
            run.GrantItem(coffee);Assert.AreEqual(baseline+1.6f,run.Stat("percent_damage"),.0001f);
            run.GrantItem(coffee);Assert.AreEqual(baseline+1.2f,run.Stat("percent_damage"),.0001f);
            run.GrantItem(beanie);Assert.AreEqual(baseline+3.2f,run.Stat("percent_damage"),.0001f);
            Assert.IsTrue(run.RemoveItem(coffee));Assert.AreEqual(baseline+3.6f,run.Stat("percent_damage"),.0001f);
            Assert.IsTrue(run.RemoveItem(coffee));Assert.AreEqual(baseline+2,run.Stat("percent_damage"),.0001f);
            Assert.IsTrue(run.RemoveItem(beanie));Assert.AreEqual(baseline,run.Stat("percent_damage"),.0001f);
        }
    }
}
