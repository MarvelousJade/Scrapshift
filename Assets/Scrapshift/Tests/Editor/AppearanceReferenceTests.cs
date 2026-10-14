using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Scrapshift.Tests
{
    public sealed class AppearanceReferenceTests
    {
        ReferenceCatalog Catalog=>AssetDatabase.LoadAssetAtPath<ReferenceCatalog>("Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset");
        ItemEntry Item(string id)=>Catalog.data.items.Single(x=>x.id==id);
        [Test] public void SourceEligibilityIncludes178ItemsAndSevenMultipleLayerItems()
        {
            Assert.AreEqual(178,Catalog.data.items.Count(x=>x.appearances?.Length>0));
            Assert.AreEqual(7,Catalog.data.items.Count(x=>x.appearances?.Length>1));
            foreach(string id in new[]{"turret","garden","catling_gun","landmines","heavy_bullets"})Assert.IsEmpty(Item(id).appearances,id);
            Assert.AreEqual("mouth",Item("pumpkin").appearance.slot);Assert.AreEqual(0,Item("pumpkin").appearance.priority);
            Assert.AreEqual(250,Item("pumpkin").appearance.depth);
        }
        [Test] public void CharacterFeaturesRemainWhenItemAppearanceIsHidden()
        {
            var c=Catalog.data.characters.Single(x=>x.id=="well_rounded");var look=new PlayerLook{layers=c.appearances};
            var layers=AppearanceResolver.Resolve(look,new[]{Item("pumpkin"),Item("beanie")},true);
            CollectionAssert.AreEqual(c.appearances.OrderBy(a=>a.order),layers);
        }
        [Test] public void MultipleLayersUseIndependentPositionsAndExactDepthOrder()
        {
            var item=Item("goat_skull");var result=AppearanceResolver.Resolve(null,new[]{item});
            Assert.AreEqual(2,result.Count);Assert.AreEqual("top-right",result[0].slot);Assert.Less(result[0].order,10);
            Assert.AreEqual("top-left",result[1].slot);Assert.Greater(result[1].order,10);
        }
        [Test] public void RemovedWinnerDoesNotRevivePreviousAppearance()
        {
            var state=new AppearanceState();var look=new PlayerLook{layers=Catalog.data.characters.Single(x=>x.id=="well_rounded").appearances};
            var glasses=Item("glasses");state.Sync(look,new[]{glasses});
            var removed=state.Sync(look,Array.Empty<ItemEntry>());
            Assert.IsFalse(removed.Any(x=>x.slot=="eyes"));Assert.IsTrue(removed.Any(x=>x.slot=="mouth"));
        }
        [Test] public void HiddenAppearanceSettingSuppressesNewAdditionsWithoutErasingExistingLayers()
        {
            var state=new AppearanceState();var look=new PlayerLook{layers=Catalog.data.characters.Single(x=>x.id=="well_rounded").appearances};
            var hat=Item("beanie");var glasses=Item("glasses");state.Sync(look,new[]{hat});
            Assert.IsTrue(state.Sync(look,new[]{hat},true).Contains(hat.appearance));
            Assert.IsFalse(state.Sync(look,new[]{hat,glasses},true).Contains(glasses.appearance));
            Assert.IsFalse(state.Sync(look,new[]{hat,glasses},false).Contains(glasses.appearance));
        }
        [Test] public void RemovingOneDuplicateClearsItsAppearanceAsInSource()
        {
            var state=new AppearanceState();var hat=Item("beanie");state.Sync(null,new[]{hat});state.Sync(null,new[]{hat,hat});
            Assert.IsFalse(state.Sync(null,new[]{hat}).Contains(hat.appearance));
        }
        [Test] public void NullSpriteCharacterLayerStillBlocksLowerPriorityPosition()
        {
            var c=Catalog.data.characters.Single(x=>x.id=="saver");var nose=c.appearances.Single(x=>x.slot=="nose");Assert.IsNull(nose.image);
            var item=new ItemEntry{id="test",appearance=new AppearanceLayer{slot="nose",priority=nose.priority-1,image=Item("beanie").appearance.image}};
            Assert.AreSame(nose,AppearanceResolver.Resolve(new PlayerLook{layers=c.appearances},new[]{item}).Single(x=>x.slot=="nose"));
        }
        [Test] public void OtherPositionCanStackAndDoesNotConflict()
        {
            var image=Item("beanie").appearance.image;var a=new ItemEntry{id="a",appearance=new AppearanceLayer{slot="",image=image}};var b=new ItemEntry{id="b",appearance=new AppearanceLayer{slot="",image=image}};
            Assert.AreEqual(2,AppearanceResolver.Resolve(null,new[]{a,b,a}).Count);
        }
    }
}
