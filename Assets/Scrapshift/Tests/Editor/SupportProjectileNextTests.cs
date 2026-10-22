using NUnit.Framework;
using UnityEngine;
namespace Scrapshift.Tests
{
    public sealed class SupportProjectileNextTests
    {
        [TestCase("turret",1,-1)] [TestCase("builder",1,-1)]
        [TestCase("laser",1,-1)] [TestCase("healing",1,-1)]
        [TestCase("tyler",4,1)] [TestCase("rocket",24,0)]
        [TestCase("flame",24,0)] [TestCase("pet_flame",24,0)]
        public void SpriteCentersUseSourcePerTypeOffsetWithoutChangingPixelsPerUnit(string kind,float x,float y)
        {
            var sprite=SupportProjectileArt.Frame(kind,.04f);
            Assert.AreEqual(100,sprite.pixelsPerUnit);
            Assert.AreEqual(x/100,sprite.bounds.center.x,.000001f);
            Assert.AreEqual(y/100,sprite.bounds.center.y,.000001f);
        }
        [TestCase(0,"frame0000_f29290ae")] [TestCase(.019f,"frame0000_f29290ae")]
        [TestCase(.02f,"frame0001_a728a067")] [TestCase(.5f,"frame0001_a728a067")]
        public void OrdinarySupportBulletChangesFromFlashToElongatedFrameAtTwentyMilliseconds(float age,string resource)
        {
            Assert.AreSame(Resources.Load<Texture2D>("WeaponReference/"+resource),SupportProjectileArt.Frame("turret",age).texture);
            Assert.AreSame(SupportProjectileArt.Frame("turret",age).texture,SupportProjectileArt.Frame("builder",age).texture);
        }
        [TestCase(0,"0000_60d674df")] [TestCase(.025f,"0001_4e388e95")]
        [TestCase(.045f,"0002_87ab5590")] [TestCase(.065f,"0003_ab1e58a8")]
        [TestCase(.085f,"0002_87ab5590")] [TestCase(.125f,"0001_4e388e95")]
        public void TylerUsesExistingLoopingLightningFramesInsteadOfGenericBullet(float age,string suffix)
        {
            Assert.AreSame(Resources.Load<Texture2D>("WeaponReference/bullet_lightning_"+suffix),SupportProjectileArt.Frame("tyler",age).texture);
            Assert.AreNotSame(SupportProjectileArt.Frame("turret",age).texture,SupportProjectileArt.Frame("tyler",age).texture);
        }
        [Test] public void BlazemanderRetainsOrangeFlameIdentitySeparatelyFromBlueTurretFlame()
        {
            var weapon=new Equipment{pet=true,projectileScene="projectiles/flames/flame_projectile.tscn"};
            Assert.AreEqual("pet_flame",SupportProjectileBehavior.PetArtKind(weapon));
            Assert.AreSame(Resources.Load<Texture2D>("WeaponReference/bullet_flame_0000_180a0687"),SupportProjectileArt.Frame("pet_flame",0).texture);
            Assert.AreSame(Resources.Load<Texture2D>("SupportProjectiles/flame0"),SupportProjectileArt.Frame("flame",0).texture);
            Assert.AreNotSame(SupportProjectileArt.Frame("pet_flame",.1f).texture,SupportProjectileArt.Frame("flame",.1f).texture);
        }
        [TestCase("laser","laser1")] [TestCase("healing","medical1")]
        public void ApprovedLaterFramesRemainAvailableAtTheUpdatedProjectileAge(string kind,string resource)
        {
            var owner=new GameObject("Updated support projectile age");
            try
            {
                var view=owner.AddComponent<SpriteRenderer>();SupportProjectileArt.Pose(view,kind,.04f);
                Assert.AreSame(Resources.Load<Texture2D>("SupportProjectiles/"+resource),view.sprite.texture);
                Assert.AreEqual(Vector3.one,owner.transform.localScale);
            }
            finally{Object.DestroyImmediate(owner);}
        }
    }
}
