using System;
using System.Linq;
using AssetScope;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class AudioIntegrationTests
    {
        const string BankPath = ContentBuilder.Root + "/Definitions/GameSounds.asset";
        [Test] public void PlayerDamageUsesFourBodyImpactsWithoutZombieVoice()
        {
            var hurt=AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath).Find(SoundCue.Hurt);
            CollectionAssert.AreEqual(new[]{"Impact_01","Impact_02","Impact_03","Impact_04"},hurt.variants.Select(c=>c.name));
            Assert.AreEqual(.2f,hurt.pitchVariation);
            Assert.GreaterOrEqual(hurt.priority,2);
        }
        [Test] public void EveryCueHasPlayableAudioAndRepeatedCuesHaveVariants()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath); Assert.NotNull(bank);
            foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                var entry=bank.Find(cue); Assert.NotNull(entry,cue.ToString()); Assert.IsNotEmpty(entry.variants);
                foreach (var clip in entry.variants) { Assert.NotNull(clip); Assert.Greater(clip.length,.05f); Assert.That(clip.channels,Is.InRange(1,2)); }
            }
            Assert.GreaterOrEqual(bank.Find(SoundCue.Fire).variants.Length,3);
            Assert.GreaterOrEqual(bank.Find(SoundCue.Impact).variants.Length,3);
        }
        [Test] public void CrateAndEndingsUseOriginalStereoCuesAtOriginalRelativeGain()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath);
            AssertCue(bank,SoundCue.CratePickup,new[]{"CratePickup_01"},1);
            AssertCue(bank,SoundCue.WaveClear,new[]{"WaveClear_01"},1);
            AssertCue(bank,SoundCue.WaveLetter,new[]{"WaveLetter_01","WaveLetter_02"},(float)Math.Pow(10,-10.0/20));
            AssertCue(bank,SoundCue.Win,new[]{"Win_01"},(float)Math.Pow(10,-5.0/20));
            AssertCue(bank,SoundCue.WinBeam,new[]{"WinBeam_01"},1);
            Assert.IsNotNull(Resources.Load<Texture2D>("WinRing"),"The victory light must use the original blue circle sprite.");
            AssertCue(bank,SoundCue.Lose,Enumerable.Range(1,9).Select(i=>"Lose_"+i.ToString("00")).ToArray(),(float)Math.Pow(10,-5.0/20));
        }
        [Test] public void AchievementPopupUsesTheOriginalTwoSoundsAndTiming()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath);
            AssertCue(bank,SoundCue.AchievementPopup,new[]{"AchievementPopup_01"},(float)Math.Pow(10,-5.0/20));
            AssertCue(bank,SoundCue.AchievementPopout,new[]{"AchievementPopout_01"},1);
            Assert.AreEqual(.2f,bank.Find(SoundCue.AchievementPopup).pitchVariation);
            Assert.AreEqual(0,bank.Find(SoundCue.AchievementPopout).pitchVariation);
            var queue=new AchievementToastQueue();
            var challenge=new UnlockDefinition{id="chal_rookie",reward="character_chunky"};
            queue.Enqueue(challenge);
            Assert.AreSame(challenge,queue.Current);
            Assert.AreEqual(0,queue.Tick(AchievementToastQueue.Duration-.01f));
            Assert.AreEqual(1,queue.Tick(.02f));
            Assert.IsNull(queue.Current);
        }
        static void AssertCue(SoundBank bank,SoundCue cue,string[] names,float gain)
        {
            var entry=bank.Find(cue);Assert.NotNull(entry);
            CollectionAssert.AreEqual(names,entry.variants.Select(c=>c.name));
            Assert.That(entry.gain,Is.EqualTo(gain).Within(.001f));
            Assert.That(entry.priority,Is.GreaterThanOrEqualTo(2));
            foreach(var clip in entry.variants)Assert.AreEqual(2,clip.channels,clip.name);
        }
        [Test] public void VariantSelectionNeverImmediatelyRepeats()
        {
            for (int count=2;count<=5;count++)
                for (int previous=0;previous<count;previous++)
                    for (int roll=0;roll<100;roll++)
                    { int pick=GameAudio.VariantIndex(count,previous,roll); Assert.AreNotEqual(previous,pick); Assert.That(pick,Is.InRange(0,count-1)); }
        }
        [Test] public void LastFiveSecondsUseTheOriginalClockTick()
        {
            var tick=AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath).Find(SoundCue.WaveTick);
            Assert.NotNull(tick);
            Assert.AreEqual("WaveTick_01",tick.variants.Single().name);
            Assert.AreEqual(2,tick.priority);
            Assert.AreEqual(1f,tick.gain);
            Assert.AreEqual(.9f,tick.cooldown);
            Assert.AreEqual(.1f,tick.pitchVariation);
            Assert.AreEqual(2,tick.variants.Single().channels);
            Assert.IsFalse(WaveCountdownFeedback.IsWarning(6));
            for(int second=1;second<=5;second++)Assert.IsTrue(WaveCountdownFeedback.IsWarning(second));
            Assert.IsFalse(WaveCountdownFeedback.IsWarning(0));
            Assert.IsFalse(WaveCountdownFeedback.TickDue(5.9f,5.1f));
            Assert.IsTrue(WaveCountdownFeedback.TickDue(5.01f,4.99f));
            Assert.IsTrue(WaveCountdownFeedback.TickDue(4.01f,3.99f));
            Assert.IsFalse(WaveCountdownFeedback.TickDue(4.9f,4.1f));
            Assert.IsFalse(WaveCountdownFeedback.TickDue(1.01f,0f));
        }
        [Test] public void AssetScopeNavigatesAnActualSoundReferenceToItsBank()
        {
            var scan=ScanSession.Create(ContentBuilder.Root,new ScanOptions { inspectSavedScenes=true, useCache=false });
            while (scan.State==ScanState.Running) scan.Step();
            Assert.AreEqual(ScanState.Complete,scan.State); Assert.IsEmpty(scan.Findings);
            var usage=scan.Usages.First(u=>u.sourceAssetPath==BankPath && u.targetAssetPath.EndsWith("Fire_01.wav"));
            StringAssert.Contains("variants",usage.propertyPath);
            var result=SourceNavigator.Navigate(usage);
            Assert.IsTrue(result.Success,result.Message);
            Assert.AreEqual(AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath),Selection.activeObject);
            Assert.IsTrue(scan.Usages.Any(u=>u.sourceAssetPath==ContentBuilder.ScenePath && u.propertyPath=="soundBank"));
        }
    }
}
