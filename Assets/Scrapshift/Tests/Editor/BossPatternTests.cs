using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class BossPatternTests
    {
        [TestCase(.0f,false)][TestCase(.539f,false)][TestCase(.54f,true)][TestCase(.679f,true)][TestCase(.68f,false)][TestCase(1.04f,false)]
        public void PillarCanOnlyDamageDuringSourceAnimationFrames(float age,bool active)=>Assert.AreEqual(active,BossPatterns.PillarActive(age));
        [Test] public void PillarScatterUsesSquareDiameterAndBorderAttackUsesThreeUnitRing()
        {
            Assert.AreEqual(new Vector2(2.5f,-2.5f),BossPatterns.SpawnOffset(5,false,false,0,10,1,-1));
            for(int i=0;i<24;i++)Assert.That(BossPatterns.SpawnOffset(6,true,true,i,24,0,0).magnitude,Is.EqualTo(3).Within(.0001));
            Assert.That(BossPatterns.SpawnOffset(6,true,true,6,24,0,0).y,Is.EqualTo(3).Within(.0001));
        }
        [Test] public void FistAndPistolUseDistinctReferenceProportions()
        {
            Assert.AreEqual(.44f,WeaponVisualMetrics.Length("fist"));Assert.AreEqual(.65f,WeaponVisualMetrics.Length("pistol"));
            Assert.AreEqual(1.62f,WeaponVisualMetrics.Length("spear"));
        }
        [Test] public void BurningDescriptionHasBulletAndSpawnCueHasClip()
        {
            var c=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var rules=new RunRules(42);rules.Configure(c);rules.Start();
            StringAssert.Contains("• ",new InspectionText(c.uiText).WeaponDescription(rules,c.Weapon("wand",1)));
            var cue=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset").Find(SoundCue.Spawn);
            Assert.NotNull(cue);Assert.IsTrue(cue.variants.Length>0);Assert.NotNull(cue.variants[0]);
        }
    }
}
