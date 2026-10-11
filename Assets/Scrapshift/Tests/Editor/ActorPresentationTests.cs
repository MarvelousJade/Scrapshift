using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ActorPresentationTests
    {
        ReferenceCatalog Catalog => AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        [Test] public void BasicEnemyIsSlightlyWiderThanPlayerAtGameplayScale()
        {
            var player=AssetDatabase.LoadAssetAtPath<GameObject>(ContentBuilder.Root+"/Prefabs/ScavengerAnimated.prefab").GetComponent<SpriteFrameAnimator>();
            float pw=player.bodyView.sprite.bounds.size.x*ActorPresentation.PlayerScale;
            var baby=Catalog.Enemy("baby_alien");float ew=baby.image.bounds.size.x*ActorPresentation.EnemyScale(baby);
            Assert.That(ew/pw,Is.InRange(1.1f,1.4f));
            Assert.That(AssetDatabase.GetAssetPath(player.footSprite),Does.EndWith("player-leg-v3.png"));
        }
        [Test] public void GroundOvalStaysFlatDuringSquashRotationAndCleansUpWithActor()
        {
            Assert.NotNull(Catalog.shadowMesh);Assert.NotNull(Catalog.shadowMaterial);
            Assert.AreEqual("Scrapshift/GroundShadow",Catalog.shadowMaterial.shader.name);
            var actor=new GameObject("Shadow test");Transform view=null;
            try
            {
                var shadow=actor.AddComponent<GroundShadow>();shadow.Initialize(Catalog.shadowMesh,Catalog.shadowMaterial,1,-.5f);
                view=shadow.View;shadow.Place(new Vector2(2,3),1);
                var bounds=view.GetComponent<Renderer>().bounds;
                actor.transform.localScale=new Vector3(1.5f,.6f,1);actor.transform.rotation=Quaternion.Euler(0,0,37);
                shadow.Place(new Vector2(2,3),1);
                Assert.That(Vector3.Distance(bounds.size,view.GetComponent<Renderer>().bounds.size),Is.LessThan(.0001f));
                Assert.That(bounds.size.y/bounds.size.x,Is.EqualTo(.3f).Within(.001f));
                Assert.AreEqual(new Vector3(2,2.5f,0),view.position);
                actor.SetActive(false);Assert.IsFalse(view.gameObject.activeSelf);
                actor.SetActive(true);Assert.IsTrue(view.gameObject.activeSelf);
            }
            finally {Object.DestroyImmediate(actor);}
            Assert.IsTrue(view==null,"Actor destruction must not leave an orphan shadow.");
        }
        [Test] public void EveryDisplayedStatHasAnAssignedBuildAsset()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            foreach(string key in StatPresentation.Keys)Assert.IsNotNull(StatPresentation.Icon(catalog,key),key);
        }
    }
}
