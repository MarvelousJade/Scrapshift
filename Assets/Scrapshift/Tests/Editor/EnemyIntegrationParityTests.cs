using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    // Integration of source-origin adapters and lifecycle callbacks. No native run.
    public sealed class EnemyIntegrationParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        GameObject host;
        UnityEngine.Random.State random;
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        void Set(string name,object value)=>typeof(ArenaGame).GetField(name,Hidden).SetValue(game,value);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        static void Field(object obj,string name,object value)=>obj.GetType().GetField(name).SetValue(obj,value);
        static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name).GetValue(obj);
        [SetUp] public void Setup()
        {
            random=UnityEngine.Random.state;
            host=new GameObject("Enemy source integration fixture");game=host.AddComponent<ArenaGame>();
            // Production InitializeProgression registers the extended roster.
            // Clone before that registration so this fixture never mutates the shared asset.
            game.catalog=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset"));
            Call("InitializeEncounters");
            game.Rules.Configure(game.catalog);game.Rules.Start();
        }
        [TearDown] public void Cleanup(){if(game!=null&&game.catalog!=null)UnityEngine.Object.DestroyImmediate(game.catalog);UnityEngine.Object.DestroyImmediate(host);UnityEngine.Random.state=random;}
        object Enemy(string id="baby_alien",string role="Enemy")
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Field(enemy,"type",new EnemyEntry{id=id,role=role,value=1});
            Field(enemy,"pos",-CombatGeometry.OriginOffset(id));Field(enemy,"hp",100f);Field(enemy,"maxHp",100f);
            Field(enemy,"renderers",Array.Empty<Renderer>());((IList)Get("enemies")).Add(enemy);return enemy;
        }
        object Goat(Vector2 origin)
        {
            var pet=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Pet",BindingFlags.NonPublic),true);
            Field(pet,"spec",new PetSpec{kind="scapegoat"});Field(pet,"pos",origin-SupportBehavior.ScapegoatOriginOffset);
            ((IList)Get("pets")).Add(pet);return pet;
        }
        [Test] public void TargetSelectionUsesRawOriginsAndPetWinsAnExactTie()
        {
            var enemy=Enemy();var goat=Goat(Vector2.right);
            Set("playerPos",Vector2.left-CombatGeometry.PlayerOriginOffset);
            Call("SelectEnemyTarget",enemy);
            Assert.AreSame(goat,Field<object>(enemy,"targetPet"));
            Assert.AreEqual(Vector2.right,(Vector2)Call("EnemyTarget",enemy));
        }
        [Test] public void TargetIdentityIsCachedButItsPositionRemainsLive()
        {
            var enemy=Enemy();var goat=Goat(Vector2.right);
            Set("playerPos",Vector2.right*5-CombatGeometry.PlayerOriginOffset);Call("SelectEnemyTarget",enemy);
            Field(goat,"pos",Vector2.right*8-SupportBehavior.ScapegoatOriginOffset);
            Call("TickEnemyTarget",enemy,.24f);
            Assert.AreSame(goat,Field<object>(enemy,"targetPet"));
            Assert.AreEqual(Vector2.right*8,(Vector2)Call("EnemyTarget",enemy));
            Call("TickEnemyTarget",enemy,.02f);
            Assert.IsNull(Field<object>(enemy,"targetPet"));
        }
        [Test] public void DeadTargetImmediatelySelectsAnotherTargetBeforeQuarterSecondPoll()
        {
            var enemy=Enemy();var goat=Goat(Vector2.right);
            Set("playerPos",Vector2.left*5-CombatGeometry.PlayerOriginOffset);Call("SelectEnemyTarget",enemy);
            Field(goat,"dead",true);Call("TickEnemyTarget",enemy,0f);
            Assert.IsNull(Field<object>(enemy,"targetPet"));
            Assert.AreEqual(Vector2.left*5,(Vector2)Call("EnemyTarget",enemy));
        }
        [Test] public void IntegerPixelSquaredTargetDistanceCanRetainTheSlightlyFartherPet()
        {
            var enemy=Enemy();var goat=Goat(new Vector2(.0109f,0));
            Set("playerPos",new Vector2(.0105f,0)-CombatGeometry.PlayerOriginOffset);
            Call("SelectEnemyTarget",enemy);
            Assert.AreSame(goat,Field<object>(enemy,"targetPet"),"Source stores accepted squared pixel distance in an int: 1.1881 becomes 1, so 1.1025 does not replace it.");
        }
        [Test] public void PerformanceRemovalRetainsDeathCountersAndCallbacksButProducesNoLoot()
        {
            var enemy=Enemy();int callbacks=0;
            game.Rules.Items.Add(new ItemEntry{id="cull-observer",effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{new ConditionalEffect{customKey="dmg_when_death",key="stat_luck",value=100,chance=1}}});
            game.Rules.RandomDamageRequested+=_=>callbacks++;
            Call("RemoveEnemyForPerformance",enemy);
            Assert.AreEqual(1,game.Rules.RemovedEnemies);Assert.AreEqual(1,game.Rules.Kills);Assert.AreEqual(1,callbacks);
            Assert.AreEqual(0,((IList)Get("enemies")).Count);Assert.AreEqual(1,((IList)Get("corpses")).Count);
            Assert.AreEqual(0,((IList)Get("pickups")).Count);
        }
        [TestCase("spawner",3,"junkie")]
        [TestCase("bloated_spawner",5,"dire_junkie")]
        public void SpawnerChildrenKeepSourceRawAreaAndConvertEachChildOrigin(string id,int expected,string child)
        {
            var enemy=Enemy(id,"Spawner");Call("SpawnEnemyChildren",enemy);
            object queue=Get("births");var lanes=(Array)queue.GetType().GetField("lanes",Hidden).GetValue(queue);
            var births=new List<object>();foreach(var birth in (IEnumerable)lanes.GetValue((int)BirthLane.Summon))births.Add(birth);
            Assert.AreEqual(expected,births.Count);
            foreach(var birth in births)
            {
                var type=Field<EnemyEntry>(birth,"type");Assert.AreEqual(child,type.id);
                Vector2 raw=Field<Vector2>(birth,"pos")+CombatGeometry.OriginOffset(type.id);
                Assert.That(raw.x,Is.InRange(-1f,1f));Assert.That(raw.y,Is.InRange(-1f,1f));
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void PredatorArmContactRequiresAnActiveHurtboxAndPlayingPhase(bool paused)
        {
            var enemy=Enemy("predator");Field(enemy,"behavior",EnemyBehavior.For("predator"));
            Set("activePlayerContactBatch",false);
            Assert.IsFalse((bool)Call("ResolveBossArmContact",enemy,Vector2.down*.12f));
            Set("activePlayerContactBatch",true);
            Set("invulnerable",1f); // Queued contact counts without rendering damage feedback.
            if(paused)game.Rules.Pause();
            int before=(int)typeof(RunRules).GetField("projectileHits",Hidden).GetValue(game.Rules);
            bool hit=(bool)Call("ResolveBossArmContact",enemy,Vector2.down*.12f);
            Assert.AreEqual(!paused,hit);
            Assert.AreEqual(before+(paused?0:1),(int)typeof(RunRules).GetField("projectileHits",Hidden).GetValue(game.Rules));
        }
        [Test] public void PersistentPredatorArmCountsEntriesRatherThanEveryOverlapFrame()
        {
            var enemy=Enemy("predator");Field(enemy,"behavior",EnemyBehavior.For("predator"));
            Field(enemy,"orbitPlayerContacts",new bool[9]);Set("activePlayerContactBatch",true);Set("invulnerable",1f);
            Vector2 contact=Vector2.down*.12f;
            Assert.IsTrue((bool)Call("ResolveBossArmEntry",enemy,0,contact));
            Assert.IsFalse((bool)Call("ResolveBossArmEntry",enemy,0,contact));
            Assert.IsTrue((bool)Call("ResolveBossArmEntry",enemy,1,contact),"Each projectile owns its own overlap.");
            Set("activePlayerContactBatch",false);Assert.IsFalse((bool)Call("ResolveBossArmEntry",enemy,0,contact));
            Set("activePlayerContactBatch",true);Assert.IsTrue((bool)Call("ResolveBossArmEntry",enemy,0,contact));
            Assert.AreEqual(3,(int)typeof(RunRules).GetField("projectileHits",Hidden).GetValue(game.Rules));
        }
        [Test] public void PredatorArmPivotUsesRawOriginAndHasItsOwnRadialLimit()
        {
            var enemy=Enemy("predator");Field(enemy,"activeAge",100f);Field(enemy,"orbitAngle",0f);
            Vector2 actual=(Vector2)Call("BossArmPosition",enemy,2.5f);
            Assert.That(actual.x,Is.EqualTo(2.5f).Within(.00001f));
            Assert.That(actual.y,Is.EqualTo(.32f).Within(.00001f),"Source pivot is -32 pixels relative to raw origin.");
        }
        [Test] public void PredatorArmExtensionKeepsTheSourcesSingleStepOvershootAndAccessibilitySpeed()
        {
            float advanced=(float)Call("AdvanceBossArmRadius",2.49f,2.5f,.02f,2f);
            Assert.That(advanced,Is.EqualTo(2.53f).Within(.00001f));
            float stopped=(float)Call("AdvanceBossArmRadius",advanced,2.5f,.02f,2f);
            Assert.AreEqual(advanced,stopped);
        }
    }
}
