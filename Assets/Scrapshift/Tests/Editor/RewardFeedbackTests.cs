using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class RewardFeedbackTests
    {
        RunRules Run()
        {var r=new RunRules(42);r.Configure(AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset"));r.Start();return r;}
        [TestCase(RunPhase.Playing)] [TestCase(RunPhase.Shop)] [TestCase(RunPhase.Upgrade)] [TestCase(RunPhase.Loot)]
        public void PausePreservesPhaseChoicesAndEconomy(RunPhase phase)
        {
            var r=Run();
            if(phase==RunPhase.Upgrade)r.Collect(20);
            if(phase==RunPhase.Loot)r.CollectConsumable(ConsumableKind.Crate);
            if(phase!=RunPhase.Playing)r.Tick(r.Remaining);
            Assert.AreEqual(phase,r.Phase);var shop=r.Shop;var offers=r.Offers;var crate=r.CrateItem;int money=r.Scrap;float timer=r.Remaining;
            r.Pause();Assert.AreEqual(RunPhase.Paused,r.Phase);r.Tick(5);
            Assert.AreEqual(timer,r.Remaining);r.Pause();Assert.AreEqual(phase,r.Phase);
            Assert.AreSame(shop,r.Shop);Assert.AreSame(offers,r.Offers);Assert.AreSame(crate,r.CrateItem);Assert.AreEqual(money,r.Scrap);
        }
        [Test] public void CompletionQueuesOnceAndDoesNotConsumeEndRewards()
        {
            var a=new UnlockDefinition{id="a",reward="item_a"};var b=new UnlockDefinition{id="b",reward="item_b"};
            var p=new Progression(new[]{a,b});var queue=new AchievementToastQueue();p.ChallengeCompleted+=queue.Enqueue;
            p.Complete("a");p.Complete("a");p.Complete("b");Assert.AreSame(a,queue.Current);
            queue.Tick(AchievementToastQueue.Duration);Assert.AreSame(b,queue.Current);
            queue.Tick(AchievementToastQueue.Duration);Assert.IsNull(queue.Current);Assert.AreEqual(2,p.NewlyUnlocked.Count);
        }
        [Test] public void MusicHasStereoLoopAndHeadroom()
        {
            var clip=Resources.Load<AudioClip>("Music/NeonSalvage");Assert.NotNull(clip);Assert.AreEqual(2,clip.channels);Assert.That(clip.length,Is.InRange(60f,62f));
            var samples=new float[clip.samples*clip.channels];Assert.IsTrue(clip.GetData(samples,0));float peak=0;double power=0;
            foreach(float s in samples){Assert.IsFalse(float.IsNaN(s));peak=Mathf.Max(peak,Mathf.Abs(s));power+=s*s;}
            Assert.That(peak,Is.InRange(.5f,.99f));Assert.Greater(power/samples.Length,.001);
        }
        [Test] public void ConsumablesHaveThreeBitesAndMaterialsHaveAudibleGain()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            Assert.AreEqual(3,bank.Find(SoundCue.Consume).variants.Length);
            Assert.GreaterOrEqual(bank.Find(SoundCue.Pickup).gain,.4f);
            Assert.AreNotSame(bank.Find(SoundCue.Consume).variants[0],bank.Find(SoundCue.Pickup).variants[0]);
        }
    }
}
