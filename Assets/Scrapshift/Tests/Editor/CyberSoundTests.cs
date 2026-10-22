using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class CyberSoundTests
    {
        [Test] public void EveryWeaponAndImpactOverrideHasItsOwnBakedEdit()
        {
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>("Assets/Scrapshift/Resources/CyberSoundLibrary.asset");
            Assert.NotNull(library);
            CyberSoundBuilder.Validate();
            foreach(var expected in CyberSoundBuilder.Sources())
            {
                var edited=library.Resolve(expected.source,SoundCue.Fire,expected.profile);
                Assert.AreNotSame(expected.source,edited,expected.source.name);
                Assert.AreEqual(expected.source.channels,edited.channels);
                Assert.That(edited.length,Is.GreaterThan(0));
                var samples=new float[edited.samples*edited.channels];Assert.True(edited.GetData(samples,0));
                Assert.That(samples.Max(s=>Mathf.Abs(s)),Is.LessThanOrEqualTo(.941f));
                Assert.That(samples.Max(s=>Mathf.Abs(s)),Is.GreaterThan(0));
            }
            Assert.AreEqual(library.entries.Length,library.entries.Select(e=>e.edited).Distinct().Count());
        }
        [Test] public void SharedRecordingsResolveByWeaponContext()
        {
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>("Assets/Scrapshift/Resources/CyberSoundLibrary.asset");
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            foreach(var weapon in catalog.data.equipment)
                foreach(var clip in weapon.sounds??Array.Empty<AudioClip>())
                    Assert.AreNotSame(clip,library.Resolve(clip,weapon.melee?SoundCue.Swing:SoundCue.Fire,ContextSoundProfile.Weapon(weapon.id,weapon.melee)),weapon.id);
            var distinct=library.entries.GroupBy(e=>e.source).First(g=>g.Select(e=>e.profile).Distinct().Count()>1).ToArray();
            Assert.AreNotSame(library.Resolve(distinct[0].source,SoundCue.Fire,distinct[0].profile),library.Resolve(distinct[1].source,SoundCue.Fire,distinct[1].profile));
            Assert.AreSame(distinct[0].source,library.Resolve(distinct[0].source,SoundCue.Pickup,distinct[0].profile));
        }
        [Test] public void AllBankCuesAndHealingLoopHaveIntendedCoverage()
        {
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>("Assets/Scrapshift/Resources/CyberSoundLibrary.asset");
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            foreach(var entry in bank.entries)
                foreach(var clip in entry.variants)
                    if(entry.cue==SoundCue.Pickup)Assert.AreSame(clip,library.Resolve(clip,entry.cue));
                    else Assert.AreNotSame(clip,library.Resolve(clip,entry.cue),entry.cue.ToString());
            var loop=Resources.Load<AudioClip>("OriginalSupportAudio/scapegoat_heal_loop");
            var edited=library.Resolve(loop,SoundCue.Fire,ContextSoundProfile.HealLoop);
            Assert.AreNotSame(loop,edited);Assert.AreEqual(loop.samples,edited.samples);
            Assert.AreEqual(loop.frequency,edited.frequency);
        }
        [Test] public void PickupBypassesStylingAndRequestedReferenceVariantsAreRetained()
        {
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>("Assets/Scrapshift/Resources/CyberSoundLibrary.asset");
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            var mapped=bank.Find(SoundCue.Fire).variants[0];
            foreach(var cue in new[]{SoundCue.Pickup})
            {
                Assert.AreSame(mapped,library.Resolve(mapped,cue));
                foreach(var clip in bank.Find(cue).variants)Assert.AreSame(clip,library.Resolve(clip,cue));
            }
            Assert.AreEqual(1,bank.Find(SoundCue.Win).variants.Length);
            Assert.AreEqual(9,bank.Find(SoundCue.Lose).variants.Length);
            Assert.AreEqual(2,bank.Find(SoundCue.Pulse).variants.Length);
            foreach(var cue in new[]{SoundCue.Win,SoundCue.Lose,SoundCue.Pulse})
                foreach(var clip in bank.Find(cue).variants)
                {
                    StringAssert.Contains("/Reference/",AssetDatabase.GetAssetPath(clip));
                    Assert.AreNotSame(clip,library.Resolve(clip,cue));
                }
        }
    }
}
