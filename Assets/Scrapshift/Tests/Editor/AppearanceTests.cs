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
        [Test] public void ItemEyesReplaceCharacterEyesAndRemovingItemRestoresBase()
        {
            var look=Prefab.GetComponent<SpriteFrameAnimator>().looks[1];
            var glasses=Item("glasses");
            var composed=AppearanceResolver.Resolve(look,new[]{glasses});
            Assert.AreSame(glasses.appearance,composed.Single(l=>l.slot=="eyes"));
            Assert.IsTrue(composed.Any(l=>l.id=="beard"));
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
                animator.lookIndex=1;items[0]=Item("head_injury");wardrobe.Sync(items,Catalog.spriteMaterial,true);
                Assert.IsTrue(wardrobe.Layers.Any(l=>l.id=="beard"));
                var view=obj.GetComponentsInChildren<SpriteRenderer>().Single(v=>v.enabled&&v.sprite==items[0].appearance.image);
                Assert.AreSame(animator.BodyRoot,view.transform.parent);
                Assert.IsTrue(view.flipX);Assert.AreEqual(-items[0].appearance.offset.x,view.transform.localPosition.x);
            }
            finally {Object.DestroyImmediate(obj);}
        }
        [Test] public void PresetsShareBodyAndLegsButHaveDistinctEyes()
        {
            var animator=Prefab.GetComponent<SpriteFrameAnimator>();
            Assert.AreEqual(3,animator.looks.Length);
            Assert.AreEqual(3,animator.looks.Select(l=>l.layers.Single(a=>a.slot=="eyes").image).Distinct().Count());
            Assert.AreEqual("player-parts_00",animator.bodyView.sprite.name);
            Assert.AreEqual("player-parts_01",animator.footSprite.name);
            Assert.Less(animator.footSprite.bounds.size.x,animator.footSprite.bounds.size.y,"Leg must be a vertical tapered piece, not a wide shoe.");
        }
        [Test] public void CosmeticResolutionDoesNotChangeInventoryOrStats()
        {
            var rules=new RunRules();rules.Configure(Catalog);rules.Start();
            rules.Items.Add(Item("helmet"));rules.Items.Add(Item("beanie"));
            float hp=rules.MaxHealth,speed=rules.MoveSpeed;
            foreach(var look in Prefab.GetComponent<SpriteFrameAnimator>().looks)AppearanceResolver.Resolve(look,rules.Items);
            Assert.AreEqual(2,rules.Items.Count);Assert.AreEqual(hp,rules.MaxHealth);Assert.AreEqual(speed,rules.MoveSpeed);
        }
        [Test] public void PointedLegSwingMirrorsLoopsAndRespectsReducedMotion()
        {
            for(float t=0;t<1;t+=.017f)
            {
                Assert.AreEqual(-SpringMotion.LegAngle(t,-1,true),SpringMotion.LegAngle(t,1,true),.001f);
                Assert.AreEqual(SpringMotion.LegAngle(t,1,true),SpringMotion.LegAngle(t+.6f,1,true),.001f);
                Assert.AreEqual(12,SpringMotion.LegAngle(t,1,true,true));
            }
        }
        [Test] public void ModularAtlasSpritesHaveTransparentBorders()
        {
            foreach(string sheet in new[]{"player-parts-v2","wearables-v2","enemies-v5","items-v2","equipment-v2","stats-v2"})
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
                        {Assert.Less(source.GetPixel(x,(int)r.y).a,.12f,sprite.name+" bottom");Assert.Less(source.GetPixel(x,(int)r.yMax-1).a,.12f,sprite.name+" top");}
                        for(int y=(int)r.y;y<(int)r.yMax;y++)
                        {Assert.Less(source.GetPixel((int)r.x,y).a,.12f,sprite.name+" left");Assert.Less(source.GetPixel((int)r.xMax-1,y).a,.12f,sprite.name+" right");}
                    }
                }
                finally {Object.DestroyImmediate(source);}
            }
        }
    }
}
