using NUnit.Framework;

namespace Scrapshift.Tests
{
    public sealed class FootstepTimingTests
    {
        [Test] public void FirstContactIsDelayedAndLoopContactsUseReferencePhase()
        {
            var timing=new FootstepTiming();
            Assert.AreEqual(0,timing.Tick(true,.09f,4.5f));
            Assert.AreEqual(1,timing.Tick(true,.011f,4.5f));
            Assert.AreEqual(0,timing.Tick(true,.298f,4.5f));
            Assert.AreEqual(1,timing.Tick(true,.002f,4.5f));
            Assert.AreEqual(2,timing.Tick(true,.6f,4.5f));
        }
        [TestCase(4.5f,2)]
        [TestCase(9f,4)]
        [TestCase(2.25f,1)]
        public void ReferenceSpeedRatioScalesTheEntireAnimation(float speed,int expected)
        {
            var timing=new FootstepTiming();
            Assert.AreEqual(expected,timing.Tick(true,.6f,speed));
        }
        [Test] public void SpeedChangePreservesContactPhase()
        {
            var timing=new FootstepTiming();
            Assert.AreEqual(1,timing.Tick(true,.25f,4.5f));
            Assert.AreEqual(0,timing.Tick(true,.07f,9f));
            Assert.AreEqual(1,timing.Tick(true,.006f,9f));
        }
        [Test] public void StoppingAndRoomCleanupRestartTheInitialDelay()
        {
            var timing=new FootstepTiming();
            Assert.AreEqual(1,timing.Tick(true,.25f,4.5f));
            Assert.AreEqual(0,timing.Tick(false,2,4.5f));
            Assert.AreEqual(0,timing.Tick(true,.09f,4.5f));
            Assert.AreEqual(1,timing.Tick(true,.011f,4.5f));
            timing.Reset();
            Assert.AreEqual(0,timing.Tick(true,.09f,4.5f));
            Assert.AreEqual(1,timing.Tick(true,.011f,4.5f));
        }
        [Test] public void CadenceDoesNotDependOnFramePartition()
        {
            var timing=new FootstepTiming();int contacts=0;
            for(int i=0;i<180;i++)contacts+=timing.Tick(true,1f/60,4.5f);
            Assert.AreEqual(10,contacts,"Contacts occur at .1,.4,...2.8, not every .28 seconds.");
            Assert.AreEqual(10,new FootstepTiming().Tick(true,3,4.5f));
        }
    }
}
