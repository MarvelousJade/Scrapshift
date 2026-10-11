using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class RunPresentationTests
    {
        [Test] public void SavedSceneContainsCurvedFloorAndSerializedParachute()
        {
            var scene=EditorSceneManager.OpenPreviewScene(ContentBuilder.ScenePath);
            try
            {
                bool floor=false,canopy=false;
                foreach(var root in scene.GetRootGameObjects())
                {
                    if(root.name=="Deck art")floor=root.transform.Find("Soft yard floor")!=null&&root.transform.Find("Top rail")==null;
                    var game=root.GetComponent<ArenaGame>();if(game!=null)canopy=game.parachutePrefab!=null;
                }
                Assert.IsTrue(floor,"Migration marker alone does not prove the saved scene was migrated.");Assert.IsTrue(canopy,"The actual scene must reference the parachute prefab.");
            }
            finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
        [Test] public void WeaponsRestAlongFacingAndRearSlotsAreBehindBody()
        {
            Assert.AreEqual(Vector2.right,ActorPresentation.RestDirection(false));Assert.AreEqual(Vector2.left,ActorPresentation.RestDirection(true));
            Assert.That(ActorPresentation.WeaponOrder(6,0),Is.GreaterThan(10));Assert.That(ActorPresentation.WeaponOrder(6,4),Is.LessThan(10));
            Assert.That(ActorPresentation.WeaponPivot("spear"),Is.GreaterThan(ActorPresentation.WeaponPivot("knife")));
        }
        [Test] public void LandingHoversThenTouchesGroundAndRecoversWithoutResidualScale()
        {
            Assert.That(ArrivalMotion.Offset(0).y,Is.GreaterThan(7));
            Assert.AreEqual(ArrivalMotion.Offset(.7f),ArrivalMotion.Offset(.95f));
            Assert.That(ArrivalMotion.BodyScale(1.05f).y,Is.GreaterThan(1.9f));
            Assert.That(ArrivalMotion.BodyScale(1.1f).x,Is.GreaterThan(1.4f));
            Assert.AreEqual(Vector2.zero,ArrivalMotion.Offset(ArrivalMotion.Duration));
            Assert.That(Vector2.Distance(Vector2.one,ArrivalMotion.BodyScale(ArrivalMotion.Duration)),Is.LessThan(.00001f));
            Assert.AreEqual(Vector2.zero,ArrivalMotion.CanopyScale(1));
        }
        [Test] public void ReturningToMenuFreezesRulesAndNewRunResetsLoss()
        {
            var rules=new RunRules(3);rules.Start();rules.Hurt(10000);Assert.AreEqual(RunPhase.Lost,rules.Phase);
            rules.ReturnToTitle();rules.Tick(100);Assert.AreEqual(RunPhase.Title,rules.Phase);
            rules.Start();Assert.AreEqual(20,rules.Remaining);Assert.AreEqual(rules.MaxHealth,rules.Health);Assert.AreEqual(1,rules.Wave);
        }
        [Test] public void ThickerLegsKeepTheirHeightAfterRepeatedAnimationTicks()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ContentBuilder.Root+"/Prefabs/ScavengerAnimated.prefab");
            var obj=Object.Instantiate(prefab);
            try
            {
                var animator=obj.GetComponent<SpriteFrameAnimator>();animator.Tick(Vector2.right,.1f,false);
                var size=animator.leftFoot.transform.localScale;
                for(int i=0;i<100;i++)animator.Tick(Vector2.left,.1f,false);
                Assert.AreEqual(size,animator.leftFoot.transform.localScale);
                Assert.That(size.x/size.y,Is.EqualTo(SpringMotion.LegWidth).Within(.001));
                Assert.That(size.y*animator.leftFoot.sprite.bounds.size.y,Is.EqualTo(SpringMotion.LegHeight).Within(.001));
            }
            finally{Object.DestroyImmediate(obj);}
        }
    }
}
