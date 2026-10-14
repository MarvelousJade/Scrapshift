using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class OriginalArtTests
    {
        [Test] public void EnemySpriteRectanglesKeepTransparentBordersAroundWholeBodies()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var texture=new Texture2D(2,2);
            try
            {
                texture.LoadImage(System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(catalog.data.enemies[0].image)));
                foreach(var enemy in catalog.data.enemies)
                {
                    Rect r=enemy.image.rect;
                    for(int x=(int)r.x;x<(int)r.xMax;x++)
                    {Assert.Less(texture.GetPixel(x,(int)r.y).a,.12f,enemy.id+" bottom cut");Assert.Less(texture.GetPixel(x,(int)r.yMax-1).a,.12f,enemy.id+" top cut");}
                    for(int y=(int)r.y;y<(int)r.yMax;y++)
                    {Assert.Less(texture.GetPixel((int)r.x,y).a,.12f,enemy.id+" left cut");Assert.Less(texture.GetPixel((int)r.xMax-1,y).a,.12f,enemy.id+" right cut");}
                }
            }
            finally {Object.DestroyImmediate(texture);}
        }
        [Test] public void SceneUsesOriginalSheetsAndNoHistoricalReferenceSprites()
        {
            var dependencies = AssetDatabase.GetDependencies(ContentBuilder.ScenePath, true);
            Assert.IsFalse(dependencies.Any(p => p.Contains("/Reference/") && p.EndsWith(".png")));
            Assert.IsTrue(dependencies.Any(p => p.EndsWith("/OriginalArt/items-v2.png")), "Retain the authored legacy item sheet alongside dedicated item art.");
            Assert.IsTrue(dependencies.Any(p=>p.EndsWith("starter-weapons-v1.png")));
            Assert.IsTrue(dependencies.Any(p=>p.EndsWith("player-parts-v2.png")));
            Assert.IsTrue(dependencies.Any(p=>p.EndsWith("wearables-v2.png")));
            Assert.IsFalse(dependencies.Any(p=>p.EndsWith("hero-spring-v1.png")));
            Assert.IsTrue(dependencies.Any(p=>p.EndsWith("enemies-v5.png")));
            Assert.IsFalse(dependencies.Any(p=>p.Contains("hero-walk")));
            Assert.IsTrue(dependencies.Any(p => p.Contains("/Reference/") && p.EndsWith(".wav")), "Retain requested reference audio.");
        }

        [Test] public void AnimationTransitionsKeepFacingAndResetTheCycle()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ContentBuilder.Root + "/Prefabs/ScavengerAnimated.prefab");
            var obj = Object.Instantiate(prefab);
            try
            {
                var animator = obj.GetComponent<SpriteFrameAnimator>();
                animator.Tick(Vector2.left, .3f, false);
                Assert.IsTrue(animator.FacingLeft); Assert.IsTrue(animator.Moving);
                animator.Tick(Vector2.zero, .01f, false);
                Assert.AreEqual(0, animator.Frame); Assert.IsTrue(animator.FacingLeft);
                var before = animator.CurrentSprite;
                var pose=animator.SpringPose;
                animator.Tick(Vector2.zero, 0, false);
                Assert.AreSame(before, animator.CurrentSprite, "A paused simulation must hold its frame.");
                Assert.AreEqual(pose,animator.SpringPose,"Pause freezes the spring too.");
                animator.Tick(Vector2.right, .01f, false);
                Assert.IsFalse(animator.FacingLeft); Assert.AreEqual(0, animator.Frame);
            }
            finally { Object.DestroyImmediate(obj); }
        }

        [Test] public void SpringCharacterUsesOneBodyAndTwoTinyFootInstances()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ContentBuilder.Root + "/Prefabs/ScavengerAnimated.prefab");
            var animator = prefab.GetComponent<SpriteFrameAnimator>();
            Assert.AreEqual(1,animator.moveFrames.Length);Assert.AreSame(animator.idleFrames[0],animator.moveFrames[0]);
            Assert.IsTrue(AssetDatabase.GetAssetPath(animator.bodyView.sprite).EndsWith("player-parts-v2.png"));
            Assert.AreSame(animator.leftFoot.sprite,animator.rightFoot.sprite);
            Assert.Less(animator.leftFoot.sprite.bounds.size.y*animator.leftFoot.transform.localScale.y,animator.bodyView.sprite.bounds.size.y*.2f);
            Assert.AreNotSame(animator.bodyView.transform,animator.leftFoot.transform.parent,"Feet must stay outside body squash.");
        }

        [TestCase(1, 240)] [TestCase(19, 240)] [TestCase(19, 1)] [TestCase(19, 3)]
        public void SplittingScrapPreservesCurrencyAndRespectsCapacity(int value, int slots)
        {
            var pieces = ScrapMotion.SplitValue(value, slots);
            Assert.AreEqual(value, pieces.Sum()); Assert.LessOrEqual(pieces.Length, slots);
            Assert.LessOrEqual(pieces.Length, 6); Assert.IsTrue(pieces.All(v => v > 0));
        }

        [Test] public void FullPickupCapacityReturnsNoPhysicalPieces()
        {
            Assert.IsEmpty(ScrapMotion.SplitValue(20, 0));
        }

        [Test] public void ScatteredScrapSettlesThenMagnetizesWithoutOvershooting()
        {
            var p = new ScrapMotion { position = new Vector2(3, 0), velocity = Vector2.right * 3,
                height = .06f, verticalSpeed = 3, spin = 200 };
            for (int i = 0; i < 240; i++) p.Step(1f / 120, Vector2.zero, 1, false);
            Assert.Less(p.height, .01f); Assert.Less(p.velocity.magnitude, .001f); Assert.Less(Mathf.Abs(p.spin), 1);
            Vector2 target = p.position + Vector2.left * .7f;
            for (int i = 0; i < 120; i++) p.Step(1f / 120, target, 1, false);
            Assert.IsTrue(p.magnetized); Assert.Less(Vector2.Distance(target, p.position), .001f);
        }

        [Test] public void GunRecoilReturnsToRestBeforeNextSlowShot()
        {
            Assert.AreEqual(0, SpriteFrameAnimator.Recoil(0));
            Assert.AreEqual(1, SpriteFrameAnimator.Recoil(.025f), .001f);
            Assert.Greater(SpriteFrameAnimator.Recoil(.06f), SpriteFrameAnimator.Recoil(.12f));
            Assert.AreEqual(0, SpriteFrameAnimator.Recoil(.18f));
        }

        [Test] public void ImportedFramesAndPickupVariantsAreDistinctSerializedSubassets()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root + "/Definitions/ReferenceCatalog.asset");
            Assert.AreEqual(5, catalog.scrapShapes.Distinct().Count());
            Assert.IsFalse(catalog.scrapShapes.Any(s => s.name == "equipment_09"), "Battery plus sign ambiguously suggests healing.");
            Assert.AreEqual("equipment_12", catalog.repairPickup.name);
            Assert.AreEqual(3, catalog.muzzleFrames.Distinct().Count());
            foreach (var sprite in catalog.scrapShapes.Concat(catalog.muzzleFrames))
            {
                Assert.IsTrue(AssetDatabase.IsSubAsset(sprite));
                Assert.Less(sprite.rect.width, sprite.texture.width / 2);
                Assert.Less(sprite.rect.height, sprite.texture.height / 2);
            }
        }

        [Test] public void AuthoredWearablesKeepAppearancesAndLatestSlotWinsWithoutRemovingInventory()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root + "/Definitions/ReferenceCatalog.asset");
            Assert.That(catalog.data.items.Count(i=>i.appearance?.image!=null),Is.EqualTo(22));
            foreach (var item in catalog.data.items.Where(i=>i.appearance?.image!=null))
            {
                Assert.IsNotEmpty(item.wearSlot, item.id);
                Assert.Greater(item.wearScale, 0, item.id);
                Assert.NotNull(item.appearance?.image,item.id);
                Assert.AreNotSame(item.image,item.appearance.image,"Wearable must be separate from its shop icon.");
            }
            var hat = catalog.data.items.Single(i => i.id == "beanie");
            var helmet = catalog.data.items.Single(i => i.id == "helmet");
            var coffee = catalog.data.items.Single(i => i.id == "coffee");
            var inventory = new[] { hat, coffee, helmet };
            var visible = CharacterWardrobe.VisibleItems(inventory);
            CollectionAssert.AreEquivalent(new[] { coffee, helmet }, visible);
            Assert.AreEqual(3, inventory.Length, "Cosmetic conflicts must not remove owned items.");
        }
    }
}
