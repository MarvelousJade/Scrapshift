using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class FootstepAudioTests
    {
        [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)][TestCase(6)]
        public void ActualImportedAndBakedFootstepsPreserveStereoAndSourceEnergy(int variant)
        {
            string path=ContentBuilder.Root+"/Audio/Step_"+variant+".mp3";
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);
            Assert.IsFalse(importer.forceToMono,"Mono normalization amplified the movement cue.");
            Assert.IsFalse(ContentBuilder.FootstepNormalizationEnabled(importer));
            var source=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>("Assets/Scrapshift/Resources/CyberSoundLibrary.asset");
            var edited=library.Resolve(source,SoundCue.Step);
            Assert.AreNotSame(source,edited);
            var measured=CyberSoundBuilder.MeasureFootstep(source,edited);
            Assert.AreEqual(2,measured.sourceChannels);
            Assert.AreEqual(measured.sourceChannels,measured.editedChannels);
            Assert.That(measured.sourcePeak,Is.GreaterThan(.01f).And.LessThan(.65f),"Check decoded recording energy, not only importer flags.");
            Assert.That(measured.editedPeak,Is.LessThan(measured.sourcePeak*1.15f),"A quiet recording must not become a normalized .92-peak click.");
            Assert.That(measured.editedEnergy/measured.sourceEnergy,Is.EqualTo(1).Within(.003),"Retain source energy including the contextual tail.");
        }
        [Test] public void ImportGuardIsIdempotentAndDoesNotChangeOtherCueImportSettings()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            var paths=bank.entries.Where(e=>e.cue!=SoundCue.Step).SelectMany(e=>e.variants).Select(AssetDatabase.GetAssetPath).Distinct().ToArray();
            var before=paths.Select(path=>EditorJsonUtility.ToJson(AssetImporter.GetAtPath(path))).ToArray();
            ContentBuilder.EnsureFootstepImportSettings();ContentBuilder.EnsureFootstepImportSettings();
            for(int i=0;i<paths.Length;i++)Assert.AreEqual(before[i],EditorJsonUtility.ToJson(AssetImporter.GetAtPath(paths[i])),paths[i]);
            foreach(var source in bank.Find(SoundCue.Step).variants)
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source));
                Assert.IsFalse(importer.forceToMono);Assert.IsFalse(ContentBuilder.FootstepNormalizationEnabled(importer));
            }
        }
        [Test] public void FootstepBankMixAndOtherApprovedCueRoutingRemainIntact()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            var step=bank.Find(SoundCue.Step);
            Assert.AreEqual(6,step.variants.Length);Assert.AreEqual(.18f,step.gain);Assert.AreEqual(.1f,step.pitchVariation);
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>("Assets/Scrapshift/Resources/CyberSoundLibrary.asset");
            Assert.AreNotEqual(library.Resolve(step.variants[0],SoundCue.Step),library.Resolve(bank.Find(SoundCue.UiFocus).variants[0],SoundCue.UiFocus));
            foreach(var clip in bank.Find(SoundCue.Pickup).variants)Assert.AreSame(clip,library.Resolve(clip,SoundCue.Pickup));
        }
    }
}
