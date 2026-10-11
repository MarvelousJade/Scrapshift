using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class CombatFeelTests
    {
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
    }
}
