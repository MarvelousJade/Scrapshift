using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class WeaponMotionTests
    {
        [TestCase("smg",.05f,.1f)] [TestCase("pistol",.1f,.25f)] [TestCase("double_barrel_shotgun",.1f,.25f)] [TestCase("laser_gun",.2f,.4f)]
        public void RecoilHitsSourceExtentsAndReturnsWithoutDrift(string id,float half,float distance)
        {
            Assert.AreEqual(0,WeaponMotion.Recoil(id,0));
            Assert.That(WeaponMotion.Recoil(id,half),Is.EqualTo(distance).Within(.0001));
            Assert.That(WeaponMotion.Recoil(id,half*.5f),Is.GreaterThan(distance*.9f));
            Assert.That(WeaponMotion.Recoil(id,half*1.5f),Is.LessThan(distance*.1f));
            Assert.AreEqual(0,WeaponMotion.Recoil(id,half*2));
        }
        [Test] public void ShootingConsumesTimeBeforeCooldownIncludingBoundaryOvershoot()
        {
            var w=new Equipment{id="pistol"};float shooting=.2f,cooldown=1;
            WeaponMotion.Step(w,.15f,ref shooting,ref cooldown);Assert.AreEqual(1,cooldown);
            WeaponMotion.Step(w,.1f,ref shooting,ref cooldown);Assert.That(cooldown,Is.EqualTo(.95f).Within(.00001));Assert.AreEqual(0,shooting);
            WeaponMotion.Step(w,2,ref shooting,ref cooldown);Assert.AreEqual(0,cooldown);
        }
        [Test] public void PositiveAttackSpeedShortensAndSoftensRecoilButNegativeDoesNotStretchIt()
        {
            var w=new Equipment{id="laser_gun"};
            Assert.AreEqual(.2f,WeaponMotion.Duration(w,2));Assert.AreEqual(.4f,WeaponMotion.Duration(w,.5f));
            Assert.That(WeaponMotion.Recoil("laser_gun",.1f,2),Is.EqualTo(.2f).Within(.0001));
        }
    }
}
