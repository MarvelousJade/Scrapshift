using System;
using System.Linq;
using AssetScope;
using NUnit.Framework;
using UnityEditor;
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
            AssertCue(bank,SoundCue.Lose,Enumerable.Range(1,9).Select(i=>"Lose_"+i.ToString("00")).ToArray(),(float)Math.Pow(10,-5.0/20));
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
