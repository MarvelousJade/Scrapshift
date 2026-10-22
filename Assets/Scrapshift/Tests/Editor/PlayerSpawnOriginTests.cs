using NUnit.Framework;
using UnityEngine;
namespace Scrapshift.Tests
{
    public sealed class PlayerSpawnOriginTests
    {
        [SetUp] public void Setup()=>ArenaGeometry.SetMapSize(0);
        [TearDown] public void Teardown()=>ArenaGeometry.SetMapSize(0);
        [Test] public void SinglePlayerSpawnsWithRawOriginAtMapCenter()
        {
            // EntitySpawner places the raw root at center; Animation sits -24px.
            Assert.AreEqual(new Vector2(0,.24f),CombatGeometry.PlayerBodyStart);
            Assert.AreEqual(Vector2.zero,CombatGeometry.PlayerBodyStart+CombatGeometry.PlayerOriginOffset);
        }
        [TestCase(-999,999,-10,7.56f)]
        [TestCase(999,-999,10,-7.32f)]
        public void PlayerBoundaryGeometryKeepsItsOffsetCollisionCircleInsideSourceWalls(float x,float y,float expectedX,float expectedY)
        {
            var actual=CombatGeometry.ClampPlayerBody(new Vector2(x,y));
            Assert.AreEqual(expectedX,actual.x,.00001f);Assert.AreEqual(expectedY,actual.y,.00001f);
        }
        [TestCase(0)] [TestCase(.225f)] [TestCase(.45f)] [TestCase(.8f)] [TestCase(1.15f)]
        public void RegisteredBodyAndWeaponContainerShareTheLandingPosition(float age)
        {Assert.AreEqual(ArrivalWeaponMotion.Offset(age),ArrivalMotion.Offset(age));}
    }
}
