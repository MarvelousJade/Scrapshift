using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class MaterialCollectionParityTests
    {
        ArenaGame game;
        readonly List<GameObject> actorObjects=new List<GameObject>();
        const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
        IList Pickups=>(IList)typeof(ArenaGame).GetField("pickups",PrivateInstance).GetValue(game);
        static int Value(object pickup)=>(int)pickup.GetType().GetField("value").GetValue(pickup);
        static bool Fruit(object pickup)=>(bool)pickup.GetType().GetField("fruit").GetValue(pickup);
        [SetUp] public void Setup()
        {
            game=new GameObject("Material parity test").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
        }
        [TearDown] public void Cleanup()
        {
            foreach(object pickup in Pickups)
                UnityEngine.Object.DestroyImmediate((GameObject)pickup.GetType().GetField("obj").GetValue(pickup));
            foreach(var actor in actorObjects)UnityEngine.Object.DestroyImmediate(actor);actorObjects.Clear();
            UnityEngine.Object.DestroyImmediate(game.gameObject);
        }
        void Drop(int value,bool fruit=false)=>typeof(ArenaGame).GetMethod("Drop",PrivateInstance).Invoke(game,new object[]{Vector2.zero,value,fruit});
        [Test] public void OverflowMergesOnGroundAtFiftyObjectsAndDoesNotSpendBankBonus()
        {
            game.Rules.BankMaterials(60);Drop(52);
            Assert.AreEqual(50,Pickups.Count);
            Assert.AreEqual(102,Pickups.Cast<object>().Sum(Value));
            Assert.AreEqual(10,game.Rules.Banked);Assert.AreEqual(0,game.Rules.Scrap);
            Drop(100);
            Assert.AreEqual(50,Pickups.Count);Assert.AreEqual(202,Pickups.Cast<object>().Sum(Value));
            Assert.AreEqual(10,game.Rules.Banked);
        }
        [Test] public void FruitDoesNotUseMaterialObjectCapacityOrConsumeBankBonus()
        {
            game.Rules.BankMaterials(1);Drop(1,true);Drop(50);
            Assert.AreEqual(51,Pickups.Count);
            Assert.AreEqual(1,Pickups.Cast<object>().Count(Fruit));
            Assert.AreEqual(51,Pickups.Cast<object>().Where(p=>!Fruit(p)).Sum(Value));
        }
        [Test] public void BankBonusBelongsToSpawnedPickupAndIsNotAddedAgainOnCollection()
        {
            var run=game.Rules;run.BankMaterials(3);int drop=run.PrepareMaterialDrop(1);
            Assert.AreEqual(2,drop);Assert.AreEqual(2,run.Banked);Assert.AreEqual(0,run.Scrap);
            run.Collect(drop);Assert.AreEqual(2,run.Scrap);Assert.AreEqual(2,run.XP);Assert.AreEqual(2,run.Banked);
            run.BankMaterials(run.PrepareMaterialDrop(1));Assert.AreEqual(3,run.Banked);
        }
        [Test] public void GathererCountsObjectsWhileCurrencyAndExperienceUseTheirBoostedValues()
        {
            var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
            var progress=new Progression(definitions);var run=game.Rules;run.Progress=progress;
            run.GainWeaponStat("chance_double_gold",100);run.BankMaterials(1);
            run.Collect(run.PrepareMaterialDrop(1));
            Assert.AreEqual(4,run.Scrap);Assert.AreEqual(4,run.XP);Assert.AreEqual(1,progress.Total("materials"));
            run.Collect(10);Assert.AreEqual(24,run.Scrap);Assert.AreEqual(2,progress.Total("materials"));
        }
        [Test] public void PushbackPreventsContactAndAttractionEvenForInstantAttractDrops()
        {
            var motion=new ScrapMotion{position=Vector2.zero,PushDestination=Vector2.right,magnetized=true};
            motion.Step(1f/60,Vector2.left*10,20,true);
            Assert.IsFalse(motion.CanCollect);Assert.IsFalse(motion.CanMoveTowardCollector);
            Assert.AreEqual(1f/12,motion.position.x,.00001f);
        }
        [Test] public void IdleAllowsContactAndAcquiresTargetButDoesNotMoveYet()
        {
            var motion=new ScrapMotion{position=Vector2.zero,PushDestination=Vector2.zero};
            motion.Step(1f/60,Vector2.right,2,true);
            Assert.IsTrue(motion.CanCollect);Assert.IsTrue(motion.magnetized);
            Assert.IsFalse(motion.CanMoveTowardCollector);Assert.AreEqual(Vector2.zero,motion.position);
            for(int i=0;i<9;i++)motion.Step(1f/60,Vector2.right,2,true);
            Assert.IsTrue(motion.CanMoveTowardCollector);Assert.AreEqual(Vector2.zero,motion.position);
            motion.Step(1f/60,Vector2.right,2,true);
            Assert.AreEqual(5f/60,motion.position.x,.00001f);Assert.AreEqual(5.2f,motion.AttractionSpeed,.00001f);
        }
        [Test] public void ItemMotionUsesTheSamePhysicsTicksAcrossRenderFrameSizes()
        {
            var coarse=new ScrapMotion{PushDestination=Vector2.right};
            var fine=new ScrapMotion{PushDestination=Vector2.right};
            for(int i=0;i<60;i++)coarse.Step(1f/60,Vector2.right*100,200,true);
            for(int i=0;i<120;i++)fine.Step(1f/120,Vector2.right*100,200,true);
            Assert.AreEqual(coarse.position.x,fine.position.x,.00001f);
            Assert.AreEqual(coarse.AttractionSpeed,fine.AttractionSpeed,.00001f);
        }
        [Test] public void AttractionHasNoArtificialSixteenUnitSpeedCap()
        {
            var motion=new ScrapMotion{PushDestination=Vector2.zero};
            for(int i=0;i<180;i++)motion.Step(1f/60,Vector2.right*1000,2000,true);
            Assert.Greater(motion.AttractionSpeed,16);
        }
        [Test] public void CampaignRateMatchesExplicitReferenceFixtureTickAcceleration()
        {
            var normal=new ScrapMotion{PushDestination=Vector2.zero,physicsRate=60};
            var fixture=new ScrapMotion{PushDestination=Vector2.zero,physicsRate=240};
            for(int i=0;i<120;i++)
            {normal.Step(1f/120,Vector2.right*1000,2000,true);fixture.Step(1f/120,Vector2.right*1000,2000,true);}
            Assert.That(normal.AttractionSpeed,Is.EqualTo(15).Within(.21f));
            Assert.That(fixture.AttractionSpeed,Is.EqualTo(45).Within(.21f));
        }
        [Test] public void ScatterDestinationStaysWithinTheMapAtItsEdges()
        {
            for(int i=0;i<20;i++)
            {
                var motion=ScrapMotion.CreateDrop(ArenaGeometry.HalfSize);
                Assert.LessOrEqual(Mathf.Abs(motion.PushDestination.x),ArenaGeometry.HalfSize.x);
                Assert.LessOrEqual(Mathf.Abs(motion.PushDestination.y),ArenaGeometry.HalfSize.y);
            }
        }
        object Actor(string name,Vector2 position,bool dead=false)
        {
            var actor=Activator.CreateInstance(typeof(ArenaGame).GetNestedType(name,BindingFlags.NonPublic),true);
            var obj=new GameObject("Pickup collector test");actorObjects.Add(obj);
            actor.GetType().GetField("obj").SetValue(actor,obj);actor.GetType().GetField("pos").SetValue(actor,position);
            if(name=="Enemy"){actor.GetType().GetField("hp").SetValue(actor,dead?0f:100f);actor.GetType().GetField("type").SetValue(actor,new EnemyEntry{id="evil_mob"});}
            else actor.GetType().GetField("dead").SetValue(actor,dead);
            return actor;
        }
        object ReadyPickup(Vector2 position)
        {
            Drop(1);var pickup=Pickups[0];var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
            motion.position=position;motion.PushDestination=position;motion.Step(.2f,Vector2.zero,-1,true);return pickup;
        }
        void UpdatePickups()=>typeof(ArenaGame).GetMethod("UpdatePickups",PrivateInstance).Invoke(game,new object[]{1f/60});
        [Test] public void EnemyCollectorAcquiresAtSourceRadiusAndUsesSharedMotion()
        {
            var enemy=Actor("Enemy",Vector2.zero);var pickup=ReadyPickup(Vector2.right+CombatGeometry.OriginOffset("evil_mob"));
            typeof(ArenaGame).GetField("playerPos",PrivateInstance).SetValue(game,Vector2.right*10);
            typeof(ArenaGame).GetMethod("EatMaterials",PrivateInstance).Invoke(game,new[]{enemy,(object)(1f/60)});
            Assert.AreSame(enemy,pickup.GetType().GetField("enemyCollector").GetValue(pickup));
            var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);UpdatePickups();
            Assert.AreEqual(1f-5f/60,motion.position.x,.00001f);Assert.AreEqual(5.2f,motion.AttractionSpeed,.00001f);
        }
        [TestCase("Enemy","enemyCollector")]
        [TestCase("Pet","petCollector")]
        public void DeadCollectorReleasesDropAndResetsAttractionSpeed(string actorType,string field)
        {
            var pickup=ReadyPickup(Vector2.right*10);var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
            motion.Step(.5f,Vector2.right*100,-1,true,true);motion.position=Vector2.right*10;
            Assert.Greater(motion.AttractionSpeed,5);
            pickup.GetType().GetField(field).SetValue(pickup,Actor(actorType,Vector2.zero,true));UpdatePickups();
            Assert.IsNull(pickup.GetType().GetField(field).GetValue(pickup));Assert.AreEqual(5,motion.AttractionSpeed);
        }
        [TestCase("Enemy","enemyCollector")]
        [TestCase("Pet","petCollector")]
        public void CloserPlayerRetargetsMaterialWithoutResettingItsSpeed(string actorType,string field)
        {
            var pickup=ReadyPickup(Vector2.right);var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
            motion.Step(.2f,Vector2.right*100,-1,true,true);float speed=motion.AttractionSpeed;motion.position=Vector2.right;
            pickup.GetType().GetField(field).SetValue(pickup,Actor(actorType,Vector2.right*10));UpdatePickups();
            Assert.IsNull(pickup.GetType().GetField(field).GetValue(pickup));Assert.IsTrue(motion.magnetized);
            Assert.Greater(motion.AttractionSpeed,speed);
        }
        [Test] public void PickupCollisionUsesLogicalBoxScaleAndRotation()
        {
            var motion=new ScrapMotion();
            // Check either side of contact rather than requiring an exact
            // decimal tangency contract that the source physics does not give.
            Assert.IsTrue(motion.OverlapsCollector(Vector2.right*.4799f,.32f));
            Assert.IsFalse(motion.OverlapsCollector(Vector2.right*.4801f,.32f));
            motion.collisionScale=1.25f;Assert.IsTrue(motion.OverlapsCollector(Vector2.right*.5199f,.32f));
            Assert.IsFalse(motion.OverlapsCollector(Vector2.right*.5201f,.32f));
            motion.collisionScale=1;motion.collisionAngle=45;
            Assert.IsTrue(motion.OverlapsCollector(Vector2.right*.54f,.32f));
        }
        [Test] public void ExternalCollectorRespectsIdleAndDoesNotClaimPlayerMagnetization()
        {
            var motion=new ScrapMotion{PushDestination=Vector2.zero};motion.Step(1f/60,Vector2.right,-1,true,true);
            Assert.IsTrue(motion.CanCollect);Assert.IsFalse(motion.magnetized);Assert.AreEqual(Vector2.zero,motion.position);
            motion.Step(.2f,Vector2.right,-1,true,true);Assert.Greater(motion.position.x,0);Assert.IsFalse(motion.magnetized);
        }
        [TestCase(1.1f,true)]
        [TestCase(-.2f,false)]
        public void SourceTakeoverComparesPlayerToCollectorRatherThanItemToCollector(float collectorX,bool takeover)
        {
            var motion=new ScrapMotion{position=Vector2.right,PushDestination=Vector2.right};motion.Step(.2f,Vector2.zero,-1,true);
            Assert.AreEqual(takeover,motion.PlayerCanRetarget(Vector2.zero,Vector2.right*collectorX,2));
        }
        [Test] public void FailedTakeoverOnEntryWaitsForExitAndReentryEvenWhenDistanceComparisonChanges()
        {
            var pickup=ReadyPickup(Vector2.right);var enemy=Actor("Enemy",Vector2.left*.2f);
            var owner=pickup.GetType().GetField("enemyCollector");owner.SetValue(pickup,enemy);UpdatePickups();
            Assert.AreSame(enemy,owner.GetValue(pickup));
            enemy.GetType().GetField("pos").SetValue(enemy,Vector2.right*3);UpdatePickups();
            Assert.AreSame(enemy,owner.GetValue(pickup));
            typeof(ArenaGame).GetField("playerPos",PrivateInstance).SetValue(game,Vector2.right*10);UpdatePickups();
            typeof(ArenaGame).GetField("playerPos",PrivateInstance).SetValue(game,Vector2.zero);UpdatePickups();
            Assert.IsNull(owner.GetValue(pickup));
        }
        [Test] public void BlockedPlayerContactDoesNotPickUpOnCollectorDeathWithoutNewContactEntry()
        {
            var pickup=ReadyPickup(Vector2.right*.4f);var enemy=Actor("Enemy",Vector2.right*.1f);
            var owner=pickup.GetType().GetField("enemyCollector");owner.SetValue(pickup,enemy);UpdatePickups();
            Assert.AreSame(enemy,owner.GetValue(pickup));Assert.AreEqual(1,Pickups.Count);
            enemy.GetType().GetField("hp").SetValue(enemy,0f);UpdatePickups();
            Assert.IsNull(owner.GetValue(pickup));Assert.AreEqual(1,Pickups.Count);Assert.AreEqual(0,game.Rules.Scrap);
        }
        [Test] public void ContactEntryIsConsumedUntilCollectorLeavesAndReenters()
        {
            var motion=new ScrapMotion{PushDestination=Vector2.zero};motion.Step(.2f,Vector2.zero,-1,true);
            var collector=new object();Assert.IsTrue(motion.ContactEntered(collector,Vector2.zero,.3f));
            Assert.IsFalse(motion.ContactEntered(collector,Vector2.zero,.3f));motion.ReleaseCollector();
            Assert.IsFalse(motion.ContactEntered(collector,Vector2.zero,.3f));
            Assert.IsFalse(motion.ContactEntered(collector,Vector2.right*10,.3f));
            Assert.IsTrue(motion.ContactEntered(collector,Vector2.zero,.3f));
        }
        [Test] public void DamageRechecksAlreadyOverlappingConsumablesThatWereIgnoredAtFullHealth()
        {
            Drop(1,true);var pickup=Pickups[0];var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
            motion.position=Vector2.right*.8f;motion.PushDestination=motion.position;motion.Step(.2f,Vector2.zero,-1,true);
            UpdatePickups();Assert.IsFalse(motion.magnetized);
            game.Rules.Hurt(1);typeof(ArenaGame).GetMethod("AttractNearbyConsumables",PrivateInstance).Invoke(game,null);
            Assert.IsTrue(motion.magnetized);
        }
        [Test] public void ConsumableAlreadyAttractedKeepsMovingWhenHealthBecomesFull()
        {
            Drop(1,true);var pickup=Pickups[0];var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
            motion.position=Vector2.right;motion.PushDestination=motion.position;motion.Step(.2f,Vector2.zero,-1,true);
            game.Rules.Hurt(1);UpdatePickups();Assert.IsTrue(motion.magnetized);
            game.Rules.Heal(1);float before=motion.position.x;UpdatePickups();
            Assert.IsTrue(motion.magnetized);Assert.Less(motion.position.x,before);
        }
    }
}
