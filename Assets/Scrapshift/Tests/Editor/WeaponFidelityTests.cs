using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public class WeaponFidelityTests
    {
        [Test] public void EveryCatalogWeaponHasExplicitReferenceMetadata()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            WeaponReference.Apply(catalog);Assert.AreEqual(205,WeaponReference.Profiles.Length);
            foreach(var w in catalog.data.equipment)
            {
                var p=WeaponReference.Profile(w);Assert.NotNull(p,w.id+":"+w.tier);
                if(!w.melee)Assert.NotNull(WeaponReference.Projectile(w),w.id);
                foreach(var x in p.secondary)Assert.NotNull(WeaponReference.Projectile(w.traits[x.index].projectile.AsEquipment()),w.id+" secondary");
                Assert.IsTrue(p.half.All(v=>v>0),w.id);
            }
        }
        [Test] public void AllProjectileFramesAndBeamCapsResolve()
        {
            Assert.AreEqual(23,WeaponReference.Projectiles.Length);
            foreach(var p in WeaponReference.Projectiles)
            {
                Assert.AreEqual(p.frames.Length,p.times.Length,p.id);
                foreach(var time in p.times)Assert.NotNull(p.Frame(time),p.id);
                if(!string.IsNullOrEmpty(p.trail))Assert.NotNull(Resources.Load<Texture2D>(p.trail),p.id);
                if(p.beam){Assert.NotNull(Resources.Load<Texture2D>(p.start));Assert.NotNull(Resources.Load<Texture2D>(p.end));Assert.AreEqual(.2f,p.stop);}
            }
            Assert.NotNull(Resources.Load<Shader>("WeaponReferenceTone"));
            Assert.NotNull(Resources.Load<Shader>("WeaponReferenceAdditive"));
        }
        [Test] public void OriginalImpactTypesHaveDistinctBanks()
        {
            var normal=WeaponReference.ImpactClips(false,false);var crit=WeaponReference.ImpactClips(true,false);var burn=WeaponReference.ImpactClips(false,true);
            Assert.AreEqual(8,normal.Length);Assert.AreEqual(4,crit.Length);Assert.AreEqual(4,burn.Length);
            Assert.IsTrue(normal.Concat(crit).Concat(burn).All(c=>c!=null&&c.samples>0));Assert.IsFalse(normal.Intersect(crit).Any());
        }
        [Test] public void SpreadIsRandomWithinSourceRangeAndZeroForAccurateSingleShot()
        {
            var w=new Equipment{spread=.2f};Assert.AreEqual(-.2f,WeaponReference.Spread(w,0,0));Assert.AreEqual(.2f,WeaponReference.Spread(w,0,1));
            Assert.AreEqual(0,WeaponReference.Spread(w,0,.5f));w.spread=0;Assert.AreEqual(0,WeaponReference.Spread(w,0,.9f));
        }
        [Test] public void RangeSpeedAndManualLifetimeFollowReference()
        {
            Assert.AreEqual(.3f,WeaponReference.Lifetime(2,10,false),.0001f);Assert.AreEqual(.25f,WeaponReference.Lifetime(2,10,true),.0001f);
            Assert.AreEqual(1,WeaponReference.DamageAfterHit(1,.9f,0));Assert.AreEqual(8,WeaponReference.DamageAfterHit(10,.5f,30),.0001f);
        }
        [Test] public void ThrustDoesNotHitDuringWindupOrReturnUnlessConfigured()
        {
            Assert.IsFalse(ReferenceMeleeMotion.Pose(.05f,.1f,.25f,.3f,.2f,2,false,false,false).active);
            Assert.IsTrue(ReferenceMeleeMotion.Pose(.25f,.1f,.25f,.3f,.2f,2,false,false,false).active);
            Assert.IsFalse(ReferenceMeleeMotion.Pose(.5f,.1f,.25f,.3f,.2f,2,false,false,false).active);
            Assert.IsTrue(ReferenceMeleeMotion.Pose(.5f,.1f,.25f,.3f,.2f,2,false,false,true).active);
        }
        [Test] public void SweepCrossesBothSidesAndReturnsToRest()
        {
            var a=ReferenceMeleeMotion.Pose(.1f,.1f,.25f,.4f,.2f,2,true,false,false);
            var b=ReferenceMeleeMotion.Pose(.499f,.1f,.25f,.4f,.2f,2,true,false,false);
            Assert.Less(a.offset.y,0);Assert.Greater(b.offset.y,0);Assert.Less(a.angle,0);Assert.Greater(b.angle,0);
            Assert.AreEqual(Vector2.zero,ReferenceMeleeMotion.Pose(.7f,.1f,.25f,.4f,.2f,2,true,false,false).offset);
        }
        [Test] public void RotatedHitboxRejectsEnemiesOutsideWeaponWidth()
        {
            Assert.IsTrue(ReferenceMeleeMotion.Overlaps(Vector2.up,.1f,Vector2.zero,90,new Vector2(1,.1f)));
            Assert.IsFalse(ReferenceMeleeMotion.Overlaps(Vector2.right,.1f,Vector2.zero,90,new Vector2(1,.1f)));
        }
    }
}
