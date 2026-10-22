using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class CombatFeelTests
    {
        [Test] public void CameraShakeUsesSourceRetriggerAndPositiveRandomOffset()
        {
            float intensity = 0, remaining = 0;
            ReferenceScreenShake.Begin(ref intensity, ref remaining, .03f);
            Assert.That(remaining, Is.EqualTo(.1f));
            ReferenceScreenShake.Begin(ref intensity, ref remaining, .03f);
            Assert.That(remaining, Is.EqualTo(.1f), "Equal hits must not restart the timer");
            Vector2 offset = ReferenceScreenShake.Tick(ref intensity, ref remaining, .04f, new Vector2(.25f, .75f));
            Assert.That(offset.x, Is.EqualTo(.0075f).Within(.000001f));
            Assert.That(offset.y, Is.EqualTo(.0225f).Within(.000001f));
            ReferenceScreenShake.Begin(ref intensity, ref remaining, .02f);
            Assert.That(remaining, Is.EqualTo(.06f).Within(.00001f));
            ReferenceScreenShake.Begin(ref intensity, ref remaining, .05f);
            Assert.That(remaining, Is.EqualTo(.1f));
            ReferenceScreenShake.Tick(ref intensity, ref remaining, .11f, Vector2.one);
            Assert.That(ReferenceScreenShake.Tick(ref intensity, ref remaining, .01f, Vector2.one), Is.EqualTo(Vector2.zero));
            Assert.That(intensity, Is.Zero);
        }

        [Test] public void BloodEmitterMatchesSourceAmountRadiusSpreadSpeedAndScale()
        {
            Assert.That(ReferenceBloodSpray.Count(1), Is.EqualTo(8));
            Assert.That(ReferenceBloodSpray.Count(.5f), Is.EqualTo(4));
            Assert.That(ReferenceBloodSpray.Count(.2f), Is.EqualTo(1));
            Assert.That(ReferenceBloodSpray.Count(.1f), Is.EqualTo(1));
            Assert.That(ReferenceBloodSpray.Count(0), Is.Zero);
            Assert.That(ReferenceBloodSpray.EmissionOffset(0, 0).magnitude, Is.EqualTo(.15f).Within(.00001f));
            Assert.That(ReferenceBloodSpray.EmissionOffset(1, .5f), Is.EqualTo(Vector2.zero));
            Assert.That(ReferenceBloodSpray.Velocity(Vector2.right, .5f, 0), Is.EqualTo(Vector2.left * 3));
            Assert.That(ReferenceBloodSpray.Velocity(Vector2.right, 0, 1).x, Is.EqualTo(6).Within(.00001f),
                "The source's 180 degree spread covers the full circle");
            Assert.That(ReferenceBloodSpray.Scale(0), Is.EqualTo(.5f));
            Assert.That(ReferenceBloodSpray.Scale(1), Is.EqualTo(.625f));
            Assert.That(ReferenceBloodSpray.Lifetime, Is.EqualTo(.5f));
        }

        [Test] public void BloodEmitterWithoutAnActorDoesNotAllocateParticlesOrConsumeRandomness()
        {
            const System.Reflection.BindingFlags hidden=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var owner=new GameObject("Absent blood surface test");
            var saved=UnityEngine.Random.state;
            try
            {
                var game=owner.AddComponent<ArenaGame>();
                var enemy=System.Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",System.Reflection.BindingFlags.NonPublic),true);
                var method=typeof(ArenaGame).GetMethod("EnemyBloodParticles",hidden);
                method.Invoke(game,new object[]{enemy,Vector2.right,1f});
                method.Invoke(game,new object[]{null,Vector2.right,1f});
                Assert.AreEqual(saved,UnityEngine.Random.state);
                Assert.IsFalse((bool)typeof(ArenaGame).GetField("enemyBloodSpriteLoaded",hidden).GetValue(game));
                Assert.AreEqual(0,((System.Collections.ICollection)typeof(ArenaGame).GetField("sparks",hidden).GetValue(game)).Count);
            }
            finally{UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Random.state=saved;}
        }

        [Test] public void ImportedRapidFireWeaponsKeepSourceBloodScale()
        {
            var profiles = WeaponReference.Profiles;
            Assert.That(System.Array.Find(profiles, p => p.id == "minigun" && p.tier == 4).effectScale, Is.EqualTo(.1f));
            Assert.That(System.Array.Find(profiles, p => p.id == "smg" && p.tier == 4).effectScale, Is.EqualTo(.2f));
            Assert.That(System.Array.Find(profiles, p => p.id == "gatling_laser" && p.tier == 4).effectScale, Is.EqualTo(.1f));
        }

        [Test] public void EnemyCrowdingTargetAndContactUseDistinctCircles()
        {
            var baby = CombatGeometry.Enemy("baby_alien");
            Assert.That(baby.body, Is.EqualTo(.22f));
            Assert.That(baby.hurt, Is.EqualTo(.32f));
            Assert.That(baby.contact + CombatGeometry.PlayerHurtRadius, Is.EqualTo(.370312f).Within(.00001f));
            Assert.That(CombatGeometry.Enemy("predator").contact, Is.EqualTo(.4f));
            var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root + "/Definitions/ReferenceCatalog.asset");
            foreach (var enemy in catalog.data.enemies)
                Assert.That(CombatGeometry.Enemy(enemy.id).hurt, Is.GreaterThan(0), enemy.id);
            Assert.That(CombatGeometry.Enemy("tree").contact, Is.Zero);
            Assert.That(CombatGeometry.ContactOffset("baby_alien")-CombatGeometry.PlayerHurtOffset, Is.EqualTo(Vector2.down*.12f));
            Assert.That(CombatGeometry.HurtOffset("predator"), Is.EqualTo(Vector2.down*.72f));
        }

        [Test] public void PistolPushMatchesSixtyHzSourceAndIsStableAcrossFrameRates()
        {
            Vector2 v = CombatGeometry.Knockback(Vector2.right, Vector2.right, 1.5f, 0);
            Assert.That(v.x, Is.EqualTo(15).Within(.0001f));
            Assert.That(CombatGeometry.StepKnockback(ref v, 1f/60).x, Is.EqualTo(.225f).Within(.0001f));
            Assert.That(v.x, Is.EqualTo(13.5f).Within(.0001f));
            foreach (int fps in new[] {20, 30, 60, 120})
            {
                v = Vector2.right * 15; float distance = 0;
                for (int i = 0; i < fps; i++) distance += CombatGeometry.StepKnockback(ref v, 1f/fps).x;
                Assert.That(distance, Is.EqualTo(2.25f * (1-Mathf.Pow(.9f,60))).Within(.0001f));
            }
        }

        [Test] public void PushRespectsResistanceZeroForceAndAttackDirection()
        {
            Assert.That(CombatGeometry.Knockback(Vector2.right, Vector2.up, 1.5f, 0), Is.EqualTo(new Vector2(3,12)));
            Assert.That(CombatGeometry.Knockback(Vector2.right, Vector2.right, 1.5f, .9f).x, Is.EqualTo(1.5f).Within(.0001f));
            Assert.That(CombatGeometry.Knockback(Vector2.right, Vector2.right, 0, 0), Is.EqualTo(Vector2.zero));
            Assert.That(CombatGeometry.Knockback(Vector2.right, Vector2.right, 1.5f, 1), Is.EqualTo(Vector2.zero));
        }

        [Test] public void ContactReturnsWhenPushFallsBelowEnemyMovementSpeed()
        {
            Assert.IsFalse(CombatGeometry.ContactEnabled(Vector2.right*3,2.5f));
            Assert.IsTrue(CombatGeometry.ContactEnabled(Vector2.right*2,2.5f));
            Assert.IsFalse(CombatGeometry.ContactEnabled(Vector2.right*.5f,0));
            Assert.That(CombatGeometry.ContactSlow(0,2.5f),Is.EqualTo(2));
            Assert.That(CombatGeometry.ContactSlow(0,1),Is.EqualTo(.9f).Within(.0001f));
            Assert.That(CombatGeometry.ContactSlow(2,2.5f),Is.EqualTo(2));
        }

        [TestCase(0,100,.2f)] [TestCase(10,100,.2666667f)] [TestCase(15,100,.4f)] [TestCase(90,100,.4f)]
        public void IframesScaleWithActualHealthLost(float damage,float health,float expected)
            => Assert.That(CombatGeometry.Invulnerability(damage,health),Is.EqualTo(expected).Within(.00001f));

        [Test] public void SweptHitsAreOrderedAndIncludeGrazesWithoutTunneling()
        {
            float near = CombatGeometry.SweepFraction(Vector2.right,.2f,Vector2.zero,Vector2.right*3);
            float far = CombatGeometry.SweepFraction(Vector2.right*2,.2f,Vector2.zero,Vector2.right*3);
            Assert.That(near,Is.LessThan(far));
            Assert.That(near,Is.EqualTo(.8f/3).Within(.00001f));
            Assert.That(CombatGeometry.SweepFraction(Vector2.zero,.2f,Vector2.zero,Vector2.zero),Is.Zero);
            Assert.IsTrue(float.IsPositiveInfinity(CombatGeometry.SweepFraction(Vector2.up,.2f,Vector2.zero,Vector2.right*3)));
            Assert.IsFalse(float.IsPositiveInfinity(CombatGeometry.SweepFraction(new Vector2(1,.2f),.2f,Vector2.zero,Vector2.right*3)));
        }

        [Test] public void ProjectilesRemainAliveInRestoredNorthAndSouthMapAreas()
        {
            Assert.IsFalse(CombatGeometry.ProjectileOutside(new Vector2(0,7.5f)));
            Assert.IsFalse(CombatGeometry.ProjectileOutside(new Vector2(0,-7.5f)));
            Assert.IsTrue(CombatGeometry.ProjectileOutside(new Vector2(0,9)));
        }

        [Test] public void RenderingEvidenceRejectsMissingInvalidAndStaleFiles()
        {
            string folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(),"ScrapshiftCapture-"+System.Guid.NewGuid());
            string file = System.IO.Path.Combine(folder,"frame.png");
            System.IO.Directory.CreateDirectory(folder);
            var started = System.DateTime.UtcNow.AddMinutes(-1);
            try
            {
                Assert.IsFalse(CaptureVerification.Fresh(folder,started,"frame"));
                System.IO.File.WriteAllText(file,"This is not a screenshot.");
                Assert.IsFalse(CaptureVerification.Fresh(folder,started,"frame"));
                var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
                System.IO.File.Copy(AssetDatabase.GetAssetPath(catalog.Enemy("baby_alien").image),file,true);
                System.IO.File.SetLastWriteTimeUtc(file,started.AddSeconds(-1));
                Assert.IsFalse(CaptureVerification.Fresh(folder,started,"frame"));
                System.IO.File.SetLastWriteTimeUtc(file,System.DateTime.UtcNow);
                Assert.IsTrue(CaptureVerification.Fresh(folder,started,"frame"));
            }
            finally { System.IO.File.Delete(file); System.IO.Directory.Delete(folder); }
        }
    }
}
