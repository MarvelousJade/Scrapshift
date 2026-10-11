using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class OriginalArtTests
    {
        [Test] public void SceneUsesOriginalSheetsAndNoHistoricalReferenceSprites()
        {
            var dependencies = AssetDatabase.GetDependencies(ContentBuilder.ScenePath, true);
            Assert.IsFalse(dependencies.Any(p => p.Contains("/Reference/") && p.EndsWith(".png")));
            Assert.AreEqual(6, dependencies.Count(p => p.Contains("/OriginalArt/") && p.EndsWith(".png")));
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
                var before = obj.GetComponent<SpriteRenderer>().sprite;
                animator.Tick(Vector2.zero, 0, false);
                Assert.AreSame(before, obj.GetComponent<SpriteRenderer>().sprite, "A paused simulation must hold its frame.");
                animator.Tick(Vector2.right, .01f, false);
                Assert.IsFalse(animator.FacingLeft); Assert.AreEqual(0, animator.Frame);
            }
            finally { Object.DestroyImmediate(obj); }
        }

        [Test] public void WalkUsesSixDistinctPosesAtTheAuthoredSlowerCadence()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ContentBuilder.Root + "/Prefabs/ScavengerAnimated.prefab");
            var animator = prefab.GetComponent<SpriteFrameAnimator>();
            Assert.AreEqual(6, animator.moveFrames.Distinct().Count());
            Assert.AreEqual(7, animator.walkFramesPerSecond);
            for (int i = 0; i < 6; i++)
                Assert.AreEqual(i, SpriteFrameAnimator.FrameAt((i + .5f) / 7, true, animator.moveFrames.Length, animator.walkFramesPerSecond));
            Assert.AreEqual(0, SpriteFrameAnimator.FrameAt(6.1f / 7, true, animator.moveFrames.Length, animator.walkFramesPerSecond));
            Assert.Less(animator.idleFrames[0].bounds.size.y / animator.moveFrames[0].bounds.size.y, 1.2f);
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

        [Test] public void EveryPurchasableItemHasAnAppearanceAndLatestSlotWinsWithoutRemovingInventory()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root + "/Definitions/ReferenceCatalog.asset");
            foreach (var item in catalog.data.items)
            {
                Assert.IsNotEmpty(item.wearSlot, item.id);
                Assert.Greater(item.wearScale, 0, item.id);
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
