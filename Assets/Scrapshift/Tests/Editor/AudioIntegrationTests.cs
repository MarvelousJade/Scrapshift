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
        [Test] public void EveryCueHasPlayableAudioAndRepeatedCuesHaveVariants()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath); Assert.NotNull(bank);
            foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                var entry=bank.Find(cue); Assert.NotNull(entry,cue.ToString()); Assert.IsNotEmpty(entry.variants);
                foreach (var clip in entry.variants) { Assert.NotNull(clip); Assert.Greater(clip.length,.05f); Assert.AreEqual(1,clip.channels); }
            }
            Assert.GreaterOrEqual(bank.Find(SoundCue.Fire).variants.Length,3);
            Assert.GreaterOrEqual(bank.Find(SoundCue.Impact).variants.Length,3);
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
