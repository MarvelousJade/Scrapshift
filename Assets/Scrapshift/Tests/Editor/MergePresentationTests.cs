using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class MergePresentationTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        GameObject owner,player;
        ArenaGame game;
        SpriteFrameAnimator animation;
        SoundBank bank;
        GameAudio sound;
        UnityEngine.Random.State randomState;
        void Set(string name,object value)=>typeof(ArenaGame).GetField(name,Hidden).SetValue(game,value);
        T Get<T>(string name)=>(T)typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        void Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        SpriteRenderer Part(string name,Vector3 position)
        {
            var obj=new GameObject(name);obj.transform.SetParent(player.transform,false);obj.transform.localPosition=position;
            return obj.AddComponent<SpriteRenderer>();
        }
        [SetUp] public void Setup()
        {
            randomState=UnityEngine.Random.state;
            owner=new GameObject("Merged presentation regression");game=owner.AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
            player=new GameObject("Presentation player");player.transform.SetParent(owner.transform,false);
            animation=player.AddComponent<SpriteFrameAnimator>();
            animation.bodyView=Part("Body",Vector3.zero);
            animation.leftFoot=Part("Left foot",new Vector3(-.17f,-.43f,.01f));
            animation.rightFoot=Part("Right foot",new Vector3(.17f,-.41f,.01f));
            Set("player",player);Set("playerAnimation",animation);Set("playerShadow",player.AddComponent<GroundShadow>());Set("reducedMotion",false);
            bank=ScriptableObject.CreateInstance<SoundBank>();bank.entries=System.Array.Empty<SoundEntry>();
            sound=owner.AddComponent<GameAudio>();sound.Initialize(bank,true);Set("sound",sound);
        }
        [TearDown] public void Cleanup()
        {
            Object.DestroyImmediate(owner);Object.DestroyImmediate(bank);UnityEngine.Random.state=randomState;
        }
        [TestCase(.45f)]
        [TestCase(1.7f)]
        public void RepeatedVictoryPoseKeepsFeetAttachedWithoutAccumulatingLift(float age)
        {
            var leftOrigin=animation.leftFoot.transform.localPosition;
            var rightOrigin=animation.rightFoot.transform.localPosition;
            game.Rules.SetShowcaseWave(RunRules.WaveCount);Call("BeginWaveSweep");
            Call("PoseVictory",age);
            var left=animation.leftFoot.transform.localPosition;
            var right=animation.rightFoot.transform.localPosition;
            var body=animation.bodyView.transform.localPosition;
            for(int i=0;i<120;i++)Call("PoseVictory",age);
            Assert.That(Vector3.Distance(left,animation.leftFoot.transform.localPosition),Is.LessThan(1e-6f));
            Assert.That(Vector3.Distance(right,animation.rightFoot.transform.localPosition),Is.LessThan(1e-6f));
            Assert.That(Vector3.Distance(left-leftOrigin,body),Is.LessThan(1e-6f),"Left foot must follow the body's absolute jump/beam offset.");
            Assert.That(Vector3.Distance(right-rightOrigin,body),Is.LessThan(1e-6f),"Right foot must follow the body's absolute jump/beam offset.");
        }
        [Test] public void ReturningDuringSettlementCancelsPendingLetterCues()
        {
            Call("BeginWaveLetters",14);
            Assert.AreEqual(14,Get<int>("waveLettersRemaining"));
            Call("ReturnToSelection",1);
            Assert.AreEqual(RunPhase.Title,game.Rules.Phase);
            Assert.AreEqual(0,Get<int>("waveLettersRemaining"));
            Assert.AreEqual(0,Get<float>("waveLetterClock"));
            Call("TickEndAudio",1f);
            Assert.AreEqual(0,Get<int>("waveLettersRemaining"));
            Assert.AreEqual(0,sound.Played);
        }
    }
}
