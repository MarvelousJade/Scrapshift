using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class ItemDesignTests
    {
        [Serializable] sealed class ConditionalRows { public ConditionalEffect[] rows; }
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(12);r.Configure(catalog);r.Start();return r;}
        [Test] public void EveryImportedItemMatchesSourceAndHasOriginalPresentation()
        {
            var expected=JsonUtility.FromJson<ItemDesignData>(System.IO.File.ReadAllText(ContentBuilder.Root+"/Reference/item-design.json"));
            Assert.AreEqual(191,catalog.data.items.Length);Assert.AreEqual(154,catalog.data.items.Count(i=>i.available));
            foreach(var source in expected.items)
            {
                var actual=Array.Find(catalog.data.items,i=>i.id==source.id);
                Assert.AreEqual(source.value,actual.value,source.id);Assert.AreEqual(source.tier,actual.tier,source.id);
                Assert.AreEqual(source.available,RunRules.IsShopItem(actual.id),source.id);Assert.AreEqual(source.maxCount,actual.maxCount);
                CollectionAssert.AreEqual(source.tags,actual.tags);CollectionAssert.AreEqual(source.groups,actual.groups);
                CollectionAssert.AreEqual(source.effects.Select(e=>e.key+":"+e.value),actual.effects.Select(e=>e.key+":"+e.value));
                Assert.AreEqual(JsonUtility.ToJson(new ConditionalRows{rows=source.conditionalEffects}),JsonUtility.ToJson(new ConditionalRows{rows=actual.conditionalEffects}),source.id);
                Assert.IsNotNull(actual.image,source.id);Assert.IsFalse(AssetDatabase.GetAssetPath(actual.image).Contains("/Reference/"),source.id);
                var text=new InspectionText(catalog.uiText);Assert.IsFalse(text.ItemDescription(actual).Contains("STAT_"),source.id);
            }
        }
        [Test] public void EveryScalarItemAppliesItsImmediateEffects()
        {
            foreach(var item in catalog.data.items.Where(i=>i.available&&(i.conditionalEffects==null||i.conditionalEffects.Length==0)))
            {
                var run=Run();run.Collect(1000);run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
                var before=StatPresentation.Keys.ToDictionary(k=>k,k=>run.Stat(k));
                run.Shop[0]=new ShopOffer{item=item,rolledWave=1};Assert.IsTrue(run.Buy(0),item.id);
                foreach(var key in StatPresentation.Keys)
                    Assert.AreEqual(before[key]+item.effects.Where(e=>e.key=="stat_"+key).Sum(e=>e.value),run.Stat(key),.001f,item.id+"/"+key);
            }
        }
        [Test] public void CharacterTagsRestrictionsAndSecondaryWeaponSetsAffectCandidates()
        {
            Run();var ranger=catalog.data.characters.Single(c=>c.id=="ranger");var brawler=catalog.data.characters.Single(c=>c.id=="brawler");
            var empty=Array.Empty<ItemEntry>();
            var preferred=ShopSelection.ItemPool(catalog.data.items,ranger,1,true,empty,empty);
            Assert.IsTrue(preferred.All(i=>i.tags.Intersect(ranger.wantedTags).Any()));Assert.IsTrue(preferred.Any(i=>i.id=="glasses"));
            Assert.IsFalse(ShopSelection.ItemPool(catalog.data.items,brawler,1,false,empty,empty).Any(i=>i.id=="lens"));
            Assert.IsFalse(ShopSelection.ItemPool(catalog.data.items,ranger,2,false,empty,empty).Any(i=>i.id=="mastery"));
            Assert.IsTrue(ShopSelection.SharesSet(new Equipment{setIds=new[]{"a","shared"}},new Equipment{setIds=new[]{"b","shared"}}));
            var first=preferred[0];Assert.IsFalse(ShopSelection.ItemPool(catalog.data.items,ranger,1,false,new[]{first},empty).Contains(first));
        }
        [Test] public void LimitsRemainHardEvenWhenPreferredPoolIsEmpty()
        {
            Run();var limited=new ItemEntry{id="coffee",tier=1,maxCount=1,tags=new[]{"stat_attack_speed"}};
            Assert.IsEmpty(ShopSelection.ItemPool(new[]{limited},null,1,true,Array.Empty<ItemEntry>(),new[]{limited}));
        }
        [Test] public void ItemPenaltiesAndMeleeCadenceUseReferenceBoundaries()
        {
            var run=Run();var pistol=catalog.Weapon("pistol",1);run.GainWeaponStat("crit_chance",-10);Assert.AreEqual(-.1f,run.CritChance,.0001);
            run.GainWeaponStat("crit_chance",100);Assert.AreEqual(.9f,run.CritChance,.0001);
            run.GainWeaponStat("attack_speed",-50);Assert.AreEqual(2f/3,run.WeaponFireRate(pistol),.0001);
            run.GainWeaponStat("percent_damage",-100);Assert.AreEqual(1,run.WeaponDamage(pistol));
            run.GainWeaponStat("range",-1000);Assert.AreEqual(.25f,run.WeaponRange(pistol));
            run.Start(catalog.data.characters.Single(c=>c.id=="brawler"),"fist");var fist=catalog.Weapon("fist",1);
            Assert.AreEqual(.4697959f,run.WeaponCooldown(fist)+run.WeaponAnimationDuration(fist),.0001);
        }
        [Test] public void ParachuteHasOpenScallopedCanopyAndDistinctCollapseKeys()
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(ContentBuilder.Root+"/Meshes/SalvageCanopy.asset");
            Assert.That(mesh.bounds.min.y,Is.GreaterThan(.99f));Assert.That(mesh.bounds.size.x,Is.GreaterThan(1.5f));
            Assert.That(mesh.bounds.size.y,Is.LessThan(1));
            Assert.AreEqual(Vector2.one,ArrivalMotion.CanopyScale(.5f));Assert.That(ArrivalMotion.CanopyScale(.8f).y,Is.GreaterThan(1.4f));
            Assert.That(ArrivalMotion.CanopyScale(.9f).y,Is.LessThan(.4f));Assert.AreEqual(Vector2.zero,ArrivalMotion.CanopyScale(1));
            Assert.AreEqual(8.2f,ArrivalMotion.CanopyRotation(.5f),.001f);
        }
    }
}
