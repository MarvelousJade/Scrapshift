using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class WeaponMuzzleParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        [Test] public void EveryRegisteredRangedWeaponHasAnAuthoredNumericMuzzle()
        {
            var ranged=catalog.data.equipment.Where(w=>!w.melee).ToArray();Assert.Greater(ranged.Length,0);
            foreach(var weapon in ranged)
            {
                var profile=WeaponReference.Profile(weapon);
                Assert.NotNull(profile,weapon.id+":"+weapon.tier);
                Assert.AreEqual(2,profile.muzzle?.Length,weapon.id+":"+weapon.tier);
                Assert.AreEqual(2,profile.muzzleRecoil?.Length,weapon.id+":"+weapon.tier);
            }
        }
        [TestCase("pistol",.56f,0)] [TestCase("smg",.56f,0)]
        [TestCase("double_barrel_shotgun",.74f,0)] [TestCase("taser",.46f,-.08f)]
        [TestCase("rocket_launcher",.66f,.02f)] [TestCase("shuriken",.13f,0)]
        [TestCase("slingshot",.08f,.03f)] [TestCase("ghost_scepter",.64f,-.05f)]
        [TestCase("crossbow",.54f,0)] [TestCase("fireball",.26f,-.02f)]
        [TestCase("flamethrower",.57f,-.02f)] [TestCase("rail_gun",.71f,0)]
        [TestCase("wand",.88f,0)] [TestCase("potato_thrower",.36f,-.01f)]
        [TestCase("icicle",.19f,0)]
        public void ResolvedSourceSpriteAndChildMuzzleOffsetsDetermineTheSpawn(string id,float x,float y)
        {
            var actual=WeaponReference.AuthoredMuzzleOffset(catalog.Weapon(id,2));
            Assert.AreEqual(x,actual.x,.000001f);Assert.AreEqual(y,actual.y,.000001f);
        }
        [TestCase(0,.56f)] [TestCase(.25f,.31f)] [TestCase(.125f,.435f)]
        public void RecoilMovesTheSpritePositionBeforeItsChildMuzzle(float recoil,float expected)
        {
            var actual=WeaponReference.AuthoredMuzzleOffset(catalog.Weapon("pistol",2),recoil);
            Assert.AreEqual(expected,actual.x,.000001f);Assert.AreEqual(0,actual.y);
        }
        [Test] public void SpawnGeometryDoesNotDependOnOwnIllustrationOrTextureBounds()
        {
            var withoutArt=new Equipment{id="pistol",tier=2};
            Assert.IsNull(withoutArt.image);
            Assert.AreEqual(new Vector2(.56f,0),WeaponReference.AuthoredMuzzleOffset(withoutArt));
        }
        [Test] public void WorldMuzzleComposesLocalAimAndAnimatedParentButProjectileAimDoesNot()
        {
            var weapon=catalog.Weapon("taser",2);var mount=new Vector2(3,2);
            var authored=new Vector2(.46f,-.08f);
            var expected=mount+(Vector2)(Quaternion.Euler(0,0,90-13.8133f)*authored);
            Assert.Less(Vector2.Distance(expected,WeaponReference.MuzzlePosition(weapon,mount,Vector2.up,0)),.00001f);
            Assert.AreEqual(Vector2.up,ArrivalWeaponMotion.ProjectileDirection(Vector2.up));
            Assert.AreEqual(mount+authored,WeaponReference.MuzzlePosition(weapon,mount,Vector2.right,2));
        }
    }
}
