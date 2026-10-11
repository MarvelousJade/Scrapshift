using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ArenaGeometryTests
    {
        [Test] public void TallArenaSupportsVerticalTravelAndClampsAtActualMapEdges()
        {
            Assert.AreEqual(new Vector2(0,7),RunRules.ClampPosition(new Vector2(0,7)));
            Assert.AreEqual(new Vector2(10.24f,-7.68f),RunRules.ClampPosition(new Vector2(999,-999)));
            var random=new System.Random(71);bool upper=false,lower=false;
            for(int i=0;i<500;i++)
            {
                var p=SpawnPlacement.Position(new SpawnGroup{area=-1},new UnitEntry(),Vector2.zero,Vector2.zero,random);
                Assert.LessOrEqual(Mathf.Abs(p.x),10.24f);Assert.LessOrEqual(Mathf.Abs(p.y),7.68f);
                upper|=p.y>6;lower|=p.y<-6;
            }
            Assert.IsTrue(upper&&lower,"Spawn distribution must cover the newly restored north/south space.");
        }
        [Test] public void CameraFollowsCenterThenStopsAtPaddedEdgesIncludingOversizeViewports()
        {
            Assert.AreEqual(new Vector2(1,2),ArenaGeometry.CameraPosition(new Vector2(1,2),1.6f));
            var edge=ArenaGeometry.CameraPosition(new Vector2(100,100),1.6f);
            Assert.That(edge.x,Is.EqualTo(2.56f).Within(.001));Assert.That(edge.y,Is.EqualTo(4.2f).Within(.001));
            Assert.That(ArenaGeometry.CameraPosition(Vector2.one*100,4).x,Is.EqualTo(0));
        }
        [Test] public void WeaponSizesAreConsistentAcrossTiersAndAttachmentsDoNotUseOneUniversalRing()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            foreach(var weapon in catalog.data.equipment)
            {
                float size=ActorPresentation.WeaponScale(weapon)*Mathf.Max(weapon.image.bounds.size.x,weapon.image.bounds.size.y);
                Assert.That(size,Is.InRange(.63f,1.63f));
                Assert.AreEqual(ActorPresentation.WeaponScale(catalog.Weapon(weapon.id,1)),ActorPresentation.WeaponScale(weapon));
            }
            Assert.AreEqual(new Vector2(0,-.3f),ActorPresentation.WeaponMount(1,0));
            for(int count=2;count<=6;count++)
                for(int i=0;i<count;i++)
                    for(int j=i+1;j<count;j++)Assert.Greater(Vector2.Distance(ActorPresentation.WeaponMount(count,i),ActorPresentation.WeaponMount(count,j)),.3f);
        }
    }
}
