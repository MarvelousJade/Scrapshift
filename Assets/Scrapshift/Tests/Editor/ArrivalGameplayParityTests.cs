using NUnit.Framework;
using UnityEngine;
namespace Scrapshift.Tests
{
    public sealed class ArrivalGameplayParityTests
    {
        [TestCase(0,-2.65f,7.25f)]
        [TestCase(.45f,-.271691f,1.97988f)]
        [TestCase(.7f,-.01f,1.4f)] [TestCase(.95f,-.01f,1.4f)]
        [TestCase(1,-.01f,1.4f)] [TestCase(1.1f,0,0)] [TestCase(1.2f,0,0)]
        public void WeaponContainerUsesActualLandingPositionKeys(float age,float x,float y)
        {Assert.That(Vector2.Distance(new Vector2(x,y),ArrivalWeaponMotion.Offset(age)),Is.LessThan(.00001f));}
        [TestCase(0,-13.8133f)] [TestCase(.45f,-1.36924f)]
        [TestCase(.7f,0)] [TestCase(1.2f,0)]
        public void ParentRotationFlipsSourceYAxis(float age,float expected)
        {Assert.AreEqual(expected,ArrivalWeaponMotion.Rotation(age),.00001f);}
        [Test] public void CubicFirstSegmentDuplicatesFirstNeighborInsteadOfUsingAnEaseApproximation()
        {
            Assert.That(ArrivalWeaponMotion.Offset(.225f).x,Is.EqualTo(-1.4772011875f).Within(.00001f));
            Assert.That(ArrivalWeaponMotion.Offset(.225f).y,Is.EqualTo(4.6511825f).Within(.00001f));
            Assert.That(ArrivalWeaponMotion.Rotation(.225f),Is.EqualTo(-7.6768475f).Within(.00001f));
        }
        [Test] public void CubicKeysRetainSourceOvershootAfterLandingPositionReachesNeutral()
        {
            Assert.That(ArrivalWeaponMotion.Offset(1.15f).x,Is.EqualTo(.000625f).Within(.00001f));
            Assert.That(ArrivalWeaponMotion.Offset(1.15f).y,Is.EqualTo(-.0875f).Within(.00001f));
            Assert.That(ArrivalWeaponMotion.Offset(1.05f).y,Is.EqualTo(.7f).Within(.00001f));
        }
        [TestCase(-1)] [TestCase(0)] [TestCase(.55f)] [TestCase(1.05f)] [TestCase(2)]
        public void WeaponContainerNeverInheritsIllustrationSquash(float age)
        {Assert.AreEqual(Vector2.one,ArrivalWeaponMotion.Scale(age));}
        [Test] public void MountAndMuzzleComposeAnimatedParentAroundBodyNeutralOrigin()
        {
            var player=new Vector2(3,.24f);var local=Vector2.right*.5f;
            var rotated=(Vector2)(Quaternion.Euler(0,0,-13.8133f)*local);
            var expected=player+new Vector2(-2.65f,7.25f)+rotated;
            Assert.That(Vector2.Distance(expected,ArrivalWeaponMotion.TransformMount(player,local,0)),Is.LessThan(.00001f));
            var muzzle=ArrivalWeaponMotion.TransformPoint(player,local,Vector2.right*.2f,Vector2.up,0);
            Assert.That(Vector2.Distance(expected+(Vector2)(Quaternion.Euler(0,0,-13.8133f)*(Vector2.up*.2f)),muzzle),Is.LessThan(.00001f));
            Assert.AreEqual(player+local,ArrivalWeaponMotion.TransformMount(player,local,2));
        }
        [Test] public void RangedVelocityUsesLocalAimWhileVisualDirectionInheritsParentRotation()
        {
            var direction=Vector2.right;
            Assert.AreEqual(direction,ArrivalWeaponMotion.ProjectileDirection(direction));
            Assert.Less(ArrivalWeaponMotion.VisualDirection(direction,0).y,0);
            Assert.That(ArrivalWeaponMotion.VisualDirection(direction,0).magnitude,Is.EqualTo(1).Within(.00001f));
        }
    }
}
