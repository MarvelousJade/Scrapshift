using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class AppearanceTests
    {
        ReferenceCatalog Catalog => AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        GameObject Prefab => AssetDatabase.LoadAssetAtPath<GameObject>(ContentBuilder.Root+"/Prefabs/ScavengerAnimated.prefab");
        ItemEntry Item(string id)=>Catalog.data.items.Single(i=>i.id==id);

        [Test] public void HigherPriorityBlocksLaterHatAndDuplicateCannotReclaimASlot()
        {
            var helmet=Item("helmet");var hat=Item("beanie");
            CollectionAssert.AreEqual(new[]{helmet.appearance},AppearanceResolver.Resolve(null,new[]{helmet,hat}));
            var glasses=Item("glasses");var blindfold=Item("blindfold");
            CollectionAssert.AreEqual(new[]{blindfold.appearance},AppearanceResolver.Resolve(null,new[]{glasses,blindfold,glasses}));
        }
        [Test] public void ItemEyesReplaceCharacterEyesAndPreserveOtherCharacterFeatures()
        {
            var look=Prefab.GetComponent<SpriteFrameAnimator>().looks.First(l=>l.id=="old");
            var glasses=Item("glasses");
            var composed=AppearanceResolver.Resolve(look,new[]{glasses});
            Assert.AreSame(glasses.appearance,composed.Single(l=>l.slot=="eyes"));
            Assert.IsTrue(composed.Contains(look.layers.Single(l=>l.slot=="mouth")));
            Assert.AreSame(look.layers.Single(l=>l.slot=="eyes"),AppearanceResolver.Resolve(look,new ItemEntry[0]).Single(l=>l.slot=="eyes"));
        }
        [Test] public void SameCountReplacementAndPresetChangeRefreshRenderersAndMirrorOffsets()
        {
            var obj=Object.Instantiate(Prefab);
            try
            {
                var animator=obj.GetComponent<SpriteFrameAnimator>();var wardrobe=obj.AddComponent<CharacterWardrobe>();
                var items=new[]{Item("beanie")};wardrobe.Sync(items,Catalog.spriteMaterial,false);
                items[0]=Item("helmet");wardrobe.Sync(items,Catalog.spriteMaterial,false);
                Assert.IsTrue(wardrobe.Layers.Contains(items[0].appearance));
                Assert.IsFalse(wardrobe.Layers.Any(l=>l.id=="beanie"));
                animator.lookIndex=System.Array.FindIndex(animator.looks,l=>l.id=="old");items[0]=Item("head_injury");wardrobe.Sync(items,Catalog.spriteMaterial,true);
                Assert.IsTrue(wardrobe.Layers.Contains(animator.Look.layers.Single(l=>l.slot=="mouth")));
                var view=obj.GetComponentsInChildren<SpriteRenderer>().Single(v=>v.enabled&&v.name=="Appearance "+items[0].appearance.id);
                Assert.AreSame(items[0].appearance.image.texture,view.sprite.texture);
                Assert.AreSame(animator.BodyRoot,view.transform.parent);
                Assert.IsTrue(view.flipX);Assert.AreEqual(-items[0].appearance.offset.x,view.transform.localPosition.x);
            }
            finally {Object.DestroyImmediate(obj);}
        }
        [Test] public void AllCharactersUseScrapshiftFeaturesAndShareTheBaseBodyAndLegs()
        {
            var animator=Prefab.GetComponent<SpriteFrameAnimator>();
            Assert.AreEqual(50,animator.looks.Length);
            Assert.AreEqual(50,animator.looks.Select(l=>l.id).Distinct().Count());
            foreach(var layer in animator.looks.SelectMany(l=>l.layers).Where(l=>l.image!=null)) Assert.IsTrue(AssetDatabase.GetAssetPath(layer.image).StartsWith(ContentBuilder.ArtRoot+"/"),layer.id);
            Assert.AreEqual("player-parts_00",animator.bodyView.sprite.name);
            Assert.AreEqual("player-leg_00",animator.footSprite.name);
            Assert.Greater(animator.footSprite.bounds.size.x,animator.footSprite.bounds.size.y,"Rounded boat limb must be wider than it is tall.");
        }
        [Test] public void CosmeticResolutionDoesNotChangeInventoryOrStats()
        {
            var rules=new RunRules();rules.Configure(Catalog);rules.Start();
            rules.Items.Add(Item("helmet"));rules.Items.Add(Item("beanie"));
            float hp=rules.MaxHealth,speed=rules.MoveSpeed;
            foreach(var look in Prefab.GetComponent<SpriteFrameAnimator>().looks)AppearanceResolver.Resolve(look,rules.Items);
            Assert.AreEqual(2,rules.Items.Count);Assert.AreEqual(hp,rules.MaxHealth);Assert.AreEqual(speed,rules.MoveSpeed);
        }
        [Test] public void AllItemLayersAreSeparateFittedWearablesRatherThanShopIcons()
        {
            var layers=Catalog.data.items.SelectMany(i=>i.appearances).ToArray();
            Assert.AreEqual(185,layers.Length);Assert.AreEqual(185,layers.Select(l=>l.image).Distinct().Count());
            foreach(var item in Catalog.data.items)foreach(var layer in item.appearances)
            {
                Assert.AreNotSame(item.image,layer.image,item.id);
                string path=AssetDatabase.GetAssetPath(layer.image);
                Assert.IsTrue(path.Contains("/FittedWearables/")||path.Contains("/TemplateWearables/"),item.id);
                Assert.Greater(layer.Stretch.y,0);Assert.Greater(layer.scale,0);
                var regions=AtlasIconMask.Regions(layer.image,layer.image.rect);Assert.IsNotEmpty(regions);
                foreach(var region in regions)Assert.IsTrue(layer.image.rect.Contains(region.center));
            }
        }
        [System.Serializable] sealed class RegisteredWearable {public string key;public float width,height,centerX,centerY;}
        [System.Serializable] sealed class RegisteredWearables {public RegisteredWearable[] entries;}
        [Test] public void EveryAtlasWearableUsesItsAuthoredBodyOriginInWorldAndPortraits()
        {
            var fits=JsonUtility.FromJson<RegisteredWearables>(System.IO.File.ReadAllText(ContentBuilder.ArtRoot+"/FittedWearables/registrations.json")).entries;
            Assert.AreEqual(180,fits.Length);
            foreach(var fit in fits)
            {
                int split=fit.key.LastIndexOf('_');var layer=Item(fit.key.Substring(0,split)).appearances[int.Parse(fit.key.Substring(split+1))];
                Assert.AreEqual(Vector2.zero,layer.offset,fit.key);
                var image=layer.image;
                var center=Vector2.Scale((image.rect.size*.5f-image.pivot)/image.pixelsPerUnit,layer.Stretch)*layer.scale;
                Assert.AreEqual(fit.centerX,center.x,.0001f,fit.key+" horizontal registration");
                Assert.AreEqual(fit.centerY,center.y,.0001f,fit.key+" vertical registration");
                Assert.AreEqual(fit.width,image.rect.width/image.pixelsPerUnit*layer.scale,.0001f,fit.key);
                Assert.AreEqual(fit.height,image.rect.height/image.pixelsPerUnit*layer.scale*layer.Stretch.y,.0001f,fit.key);
            }
        }
        [Test] public void TemplateWearablesKeepFullCanvasAndFollowTheAnimatedBody()
        {
            var obj=Object.Instantiate(Prefab);
            try
            {
                var animator=obj.GetComponent<SpriteFrameAnimator>();animator.lookIndex=0;
                var wardrobe=obj.AddComponent<CharacterWardrobe>();
                foreach(string id in new[]{"beanie","leather_vest","glasses","coffee","blindfold"})
                {
                    wardrobe.Reset();var item=Item(id);wardrobe.Sync(new[]{item},Catalog.spriteMaterial,false);
                    var layer=item.appearances.Single();var sprite=layer.image;
                    Assert.AreEqual(new Vector2(1254,1254),sprite.rect.size,id);
                    Assert.AreEqual(id=="blindfold"?860:627,sprite.pixelsPerUnit,id);
                    Assert.AreEqual(Vector2.zero,layer.offset,id);
                    Assert.AreEqual(Vector2.one,layer.Stretch,id);Assert.AreEqual(1,layer.scale,id);
                    var view=obj.GetComponentsInChildren<SpriteRenderer>().Single(v=>v.enabled&&v.name=="Appearance "+layer.id);
                    Assert.AreSame(animator.BodyRoot,view.transform.parent);
                    animator.Tick(Vector2.left,.15f,false);wardrobe.Sync(new[]{item},Catalog.spriteMaterial,true);
                    Assert.IsTrue(view.flipX);Assert.AreEqual(0,view.transform.localPosition.x);Assert.AreEqual(0,view.transform.localPosition.y);
                    Assert.AreEqual(Vector3.one,view.transform.localScale);
                    wardrobe.Sync(new ItemEntry[0],Catalog.spriteMaterial,true);
                    Assert.AreEqual(0,wardrobe.VisibleCount);
                }
            }
            finally{Object.DestroyImmediate(obj);}
        }
        [Test] public void FullBodySkinReplacesBaseShellAndRemovalRevealsItAgain()
        {
            var obj=Object.Instantiate(Prefab);
            try
            {
                var animator=obj.GetComponent<SpriteFrameAnimator>();animator.lookIndex=0;
                var wardrobe=obj.AddComponent<CharacterWardrobe>();
                wardrobe.Sync(new[]{Item("potato")},Catalog.spriteMaterial,false);
                Assert.IsFalse(animator.bodyView.enabled);
                Assert.IsTrue(wardrobe.Layers.Any(l=>l.slot=="eyes"&&l.characterAppearance));
                wardrobe.Sync(new ItemEntry[0],Catalog.spriteMaterial,false);
                Assert.IsTrue(animator.bodyView.enabled);
            }
            finally{Object.DestroyImmediate(obj);}
        }
        [Test] public void CurvedLegSwingMirrorsLoopsAndRespectsReducedMotion()
        {
            for(float t=0;t<1;t+=.017f)
            {
                Assert.AreEqual(-SpringMotion.LegAngle(t,-1,true),SpringMotion.LegAngle(t,1,true),.001f);
                Assert.AreEqual(SpringMotion.LegAngle(t,1,true),SpringMotion.LegAngle(t+.6f,1,true),.001f);
                Assert.AreEqual(SpringMotion.LegRestAngle,SpringMotion.LegAngle(t,1,true,true));
            }
        }
        [Test] public void TorchMaskRemovesNeighborFragmentButKeepsTorchArtwork()
        {
            var sprite=AssetDatabase.LoadAllAssetsAtPath(ContentBuilder.ArtRoot+"/base-weapons-v1.png").OfType<Sprite>().Single(s=>s.name=="base-weapons_30");
            Assert.IsFalse(AtlasIconMask.Visible(sprite,1160,200));
            Assert.IsTrue(AtlasIconMask.Visible(sprite,1280,200));
            Assert.IsTrue(AtlasIconMask.Visible(sprite,1305,250));
            foreach(var region in AtlasIconMask.Regions(sprite,sprite.rect)){Assert.Greater(region.width,0);Assert.Greater(region.height,0);Assert.IsTrue(sprite.rect.Contains(region.center));}
        }
        [Test] public void ModularAtlasSpritesHaveTransparentBorders()
        {
            foreach(string sheet in new[]{"player-parts-v2","wearables-v2","enemies-v5","items-v2","equipment-v2","stats-v2","player-leg-v3","starter-weapons-v1","base-weapons-v1"})
            {
                string path=ContentBuilder.ArtRoot+"/"+sheet+".png";
                var source=new Texture2D(2,2);
                try
                {
                    source.LoadImage(System.IO.File.ReadAllBytes(path));
                    foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                    {
                        Rect r=sprite.rect;
                        for(int x=(int)r.x;x<(int)r.xMax;x++)
                        {if(AtlasIconMask.Visible(sprite,x,(int)r.y))Assert.Less(source.GetPixel(x,(int)r.y).a,.12f,sprite.name+" bottom");if(AtlasIconMask.Visible(sprite,x,(int)r.yMax-1))Assert.Less(source.GetPixel(x,(int)r.yMax-1).a,.12f,sprite.name+" top");}
                        for(int y=(int)r.y;y<(int)r.yMax;y++)
                        {if(AtlasIconMask.Visible(sprite,(int)r.x,y))Assert.Less(source.GetPixel((int)r.x,y).a,.12f,sprite.name+" left");if(AtlasIconMask.Visible(sprite,(int)r.xMax-1,y))Assert.Less(source.GetPixel((int)r.xMax-1,y).a,.12f,sprite.name+" right");}
                    }
                }
                finally {Object.DestroyImmediate(source);}
            }
        }
    }
}


