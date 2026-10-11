using NUnit.Framework;
using UnityEngine;

namespace Scrapshift.Tests
{
    public sealed class SpringMotionTests
    {
        [Test] public void SlowIdleAndFastWalkMatchReferencePoseLandmarks()
        {
            Assert.That(Vector3.Distance(new Vector3(1,1,0),SpringMotion.Pose(0,false)),Is.LessThan(.0001f));
            Assert.That(Vector3.Distance(new Vector3(1.15f,.85f,-.05f),SpringMotion.Pose(.5f,false)),Is.LessThan(.0001f));
            Assert.That(Vector3.Distance(new Vector3(1.2f,.8f,-.1f),SpringMotion.Pose(.1f,true)),Is.LessThan(.0001f));
            Assert.That(Vector3.Distance(new Vector3(.9f,1.1f,.05f),SpringMotion.Pose(.2f,true)),Is.LessThan(.0001f));
            Assert.That(Vector3.Distance(SpringMotion.Pose(.07f,true),SpringMotion.Pose(.37f,true)),Is.LessThan(.0001f));
            Assert.Greater(Vector3.Distance(SpringMotion.Pose(.07f,false),SpringMotion.Pose(.37f,false)),.05f);
        }
        [Test] public void ReducedMotionDisablesSquashAndFootBounce()
        {
            for(float t=0;t<2;t+=.017f)
            {
                Assert.AreEqual(new Vector3(1,1,0),SpringMotion.Pose(t,true,true));
                Assert.AreEqual(new Vector2(-SpringMotion.LegSpread,0),SpringMotion.Foot(t,-1,true,true));
            }
        }
        [Test] public void TinyFeetAlternateWithinBoundedRangeAndLoop()
        {
            for(float t=0;t<.6f;t+=.01f)
            {
                var a=SpringMotion.Foot(t,-1,true);var b=SpringMotion.Foot(t,1,true);
                Assert.That(a.x,Is.InRange(-.35f,-.27f));Assert.That(b.x,Is.InRange(.27f,.35f));
                Assert.LessOrEqual(a.y*b.y,.00001f,"Only one foot lifts at a time.");
                Assert.That(Vector2.Distance(a,SpringMotion.Foot(t+.6f,-1,true)),Is.LessThan(.0001f));
            }
        }
    }
}
