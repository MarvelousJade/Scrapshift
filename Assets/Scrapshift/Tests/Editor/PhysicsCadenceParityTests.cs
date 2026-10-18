using NUnit.Framework;
namespace Scrapshift.Tests
{
    public sealed class PhysicsCadenceParityTests
    {
        [TestCase(30)] [TestCase(60)] [TestCase(120)] [TestCase(144)]
        public void OneSecondHasSixtyCombatStepsAcrossRenderRates(int fps)
        {
            double remainder=0;int steps=0;
            for(int i=0;i<fps;i++)steps+=ReferencePhysicsClock.Consume(ref remainder,1f/fps);
            Assert.AreEqual(60,steps);Assert.That(remainder,Is.LessThan(.00001));
        }
        [Test] public void AStallHasBoundedCatchupAndNoNegativeRemainder()
        {
            double remainder=0;Assert.AreEqual(8,ReferencePhysicsClock.Consume(ref remainder,10));
            Assert.That(remainder,Is.InRange(0,1e-8));
            Assert.AreEqual(0,ReferencePhysicsClock.Consume(ref remainder,-1));
        }
    }
}
