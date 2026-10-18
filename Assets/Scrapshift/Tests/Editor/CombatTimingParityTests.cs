using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class CombatTimingParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;GameObject owner,spark,shotView;
        UnityEngine.Random.State randomState;
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        static void Set(object obj,string name,object value)=>obj.GetType().GetField(name).SetValue(obj,value);
        static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name).GetValue(obj);
        [SetUp] public void Setup()
        {
            randomState=UnityEngine.Random.state;owner=new GameObject("Combat timing parity");game=owner.AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();game.Rules.Inventory.Clear();game.Rules.GainWeaponStat("crit_chance",-1000);
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,owner.AddComponent<GameAudio>());
            spark=new GameObject("Combat timing spark");spark.AddComponent<SpriteRenderer>();game.sparkPrefab=spark;
        }
        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"weaponExplosions","shots","sparks","impacts","pickups"})foreach(var effect in (IEnumerable)Get(list))
            {var obj=Field<GameObject>(effect,"obj");if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
            if(shotView!=null)UnityEngine.Object.DestroyImmediate(shotView);
            UnityEngine.Object.DestroyImmediate(spark);UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Random.state=randomState;
        }
        object EnemyAtHurtCenter(Vector2 center)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(enemy,"hp",1000f);Set(enemy,"maxHp",1000f);Set(enemy,"pos",center-CombatGeometry.HurtOffset("baby_alien"));
            Set(enemy,"type",new EnemyEntry{id="baby_alien",role="Enemy"});Set(enemy,"renderers",Array.Empty<Renderer>());
            ((IList)Get("enemies")).Add(enemy);return enemy;
        }
        object AdvancedShot(float delayAfterAdvance,float lifeAfterAdvance=1)
        {
            var shot=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Shot",BindingFlags.NonPublic),true);
            shotView=new GameObject("Timing shot");
            Set(shot,"obj",shotView);Set(shot,"pos",Vector2.right*10);Set(shot,"age",.1f);Set(shot,"life",lifeAfterAdvance);
            Set(shot,"hitDelay",delayAfterAdvance);Set(shot,"projectileRef",new ProjectileReference{radius=.1f,hitOffset=new float[2]});
            return shot;
        }
        [Test] public void DelayedShotCannotHitAnEnemyPassedBeforeItsHitboxEnabled()
        {
            var enemy=EnemyAtHurtCenter(Vector2.right*2);var shot=AdvancedShot(-.025f);
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f)),
                "The first 75% of this travel occurred with the hitbox disabled.");
        }
        [Test] public void EnabledShotCanHitItsCurrentPhysicsPosition()
        {
            var enemy=EnemyAtHurtCenter(Vector2.right*10);var shot=AdvancedShot(-.1f);
            float fraction=(float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f);
            Assert.AreEqual(0,fraction,"Source physics overlaps are simultaneous; no swept travel fraction is invented.");
        }
        [Test] public void StillDisabledShotCannotHitAtItsCurrentPosition()
        {
            var enemy=EnemyAtHurtCenter(Vector2.right*10);var shot=AdvancedShot(.001f);
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f)));
        }
        [Test] public void AlreadyEnabledShotDoesNotSweepAcrossPastPositions()
        {
            var enemy=EnemyAtHurtCenter(Vector2.right*2);var shot=AdvancedShot(-.1f);
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f)));
        }
        [Test] public void OrdinaryShotKeepsItsInitialSpawnOverlapWithoutSweeping()
        {
            var enemy=EnemyAtHurtCenter(Vector2.zero);var shot=AdvancedShot(-.1f);
            Assert.AreEqual(0,(float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f));
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,.01f)),
                "Only the initial enabled source snapshot is retained, not a previous-position snapshot every tick.");
        }
        [Test] public void SourceFlameAtReachableSpeedCapMissesGrazingTargetBetweenPhysicsPositions()
        {
            // Actual source flame Collision offset(24,4), radius44; BabyHurt32.
            // Godot native fixture: target(174,174), source y100, speed6000px/s.
            var enemy=EnemyAtHurtCenter(new Vector2(1.74f,-1.74f));var shot=AdvancedShot(-.1f);
            var profile=WeaponReference.Projectile(game.catalog.Weapon("flamethrower",2));
            Set(shot,"projectileRef",profile);
            Assert.AreEqual(.44f,profile.radius,.000001f);
            foreach(float x in new[]{1f,2f})
            {
                Set(shot,"pos",new Vector2(x,-1));
                Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,new Vector2(x-1,-1),0f)));
            }
            float swept=CombatGeometry.SweepFraction(new Vector2(1.74f,-1.74f),.76f,new Vector2(1.24f,-1.04f),new Vector2(2.24f,-1.04f));
            Assert.IsFalse(float.IsPositiveInfinity(swept),"The rejected swept model would add this native-missed contact.");
        }
        [Test] public void PendingShotCannotAcquireAContactBeforeItsFirstPhysicsActivation()
        {
            var enemy=EnemyAtHurtCenter(Vector2.right*10);var shot=AdvancedShot(0);
            Set(shot,"delayedHitboxPending",true);
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f)));
        }
        [Test] public void BeamCannotUseThicknessFromAfterItsRemainingLifetime()
        {
            var enemy=EnemyAtHurtCenter(new Vector2(1,.6f));var shot=AdvancedShot(-.1f,-.07f);
            Set(shot,"pos",Vector2.zero);Set(shot,"weapon",game.catalog.Weapon("knife",1));
            var profile=new ProjectileReference{beam=true,half=new[]{2f,.5f},hitOffset=new[]{2f,0f}};
            Set(shot,"projectileRef",profile);
            Assert.IsFalse(float.IsPositiveInfinity(ReferenceBeam.IntervalHitFraction(profile,1.5f,1,0,.1f,Vector2.zero,0,new Vector2(1,.6f),CombatGeometry.Enemy("baby_alien").hurt)),
                "The later .1-second thickness would hit this target if sampled after expiry.");
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f)),
                "Only .03 seconds remained; both enable clipping and thickness sampling must use that same active interval.");
        }
        [Test] public void BeamCannotReplayAnIntermediateThicknessPeakBetweenPhysicsSamples()
        {
            var enemy=EnemyAtHurtCenter(new Vector2(1,1.31f));var shot=AdvancedShot(-.04f);
            Set(shot,"age",.12f);Set(shot,"pos",Vector2.zero);Set(shot,"weapon",game.catalog.Weapon("knife",1));
            var profile=new ProjectileReference{beam=true,half=new[]{2f,1f},hitOffset=new[]{2f,0f}};
            Set(shot,"projectileRef",profile);
            Assert.IsFalse(float.IsPositiveInfinity(ReferenceBeam.IntervalHitFraction(profile,1.5f,1,.08f,.12f,Vector2.zero,0,new Vector2(1,1.31f),.32f)));
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,.08f)),
                "Godot samples the animated Hitbox at its physics state, not the maximum thickness reached between samples.");
        }
        [Test] public void MeleeCannotReplayAnAttackThatFinishedBeforeTheCurrentPhysicsPose()
        {
            var weapon=game.catalog.Weapon("knife",1);
            Call("BeginMeleeAttack",0,weapon,Vector2.right,1.5f);
            var attacks=(Array)Get("meleeAttacks");var attack=attacks.GetValue(0);
            float recoil=Field<float>(attack,"recoilTime"),strike=Field<float>(attack,"strike"),back=Field<float>(attack,"back");
            var pose=ReferenceMeleeMotion.Pose(recoil+strike*.5f,recoil,Field<float>(attack,"recoil"),strike,back,Field<float>(attack,"range"),false,false,false);
            var profile=WeaponReference.Profile(weapon);
            var enemy=EnemyAtHurtCenter(pose.offset+Vector2.right*ActorPresentation.WeaponPivot(weapon.id)+new Vector2(profile.hitOffset[0],profile.hitOffset[1]));
            shotView=new GameObject("Melee timing view");shotView.AddComponent<SpriteRenderer>();
            ((Array)Get("equipmentViews")).SetValue(shotView,0);
            Call("TickMeleeAttack",0,weapon,Vector2.zero,recoil+strike+back+.01f);
            Assert.AreEqual(1000,Field<float>(enemy,"hp"),"An old240Hz swept pose must not create an Area2D contact after the attack is over.");
            Assert.IsTrue(Field<bool>(attack,"sounded"),"The source recoil-completion sound event still occurs even if no physics sample sees an active contact pose.");
        }
        [Test] public void CompletedExplosionCannotHitAUnitEnteringOnALaterUpdate()
        {
            var enemy=EnemyAtHurtCenter(Vector2.right*8);var weapon=game.catalog.Weapon("knife",1);
            Call("QueueSupportExplosion",Vector2.zero,weapon,10f,1.5f,true,null,null);
            Call("TickReferenceExplosions",.05f);Assert.AreEqual(0,((IList)Get("weaponExplosions")).Count);
            Set(enemy,"pos",-CombatGeometry.HurtOffset("baby_alien"));Call("TickReferenceExplosions",.01f);
            Assert.AreEqual(1000,Field<float>(enemy,"hp"),"Source explosion.tscn disables its hitbox at .05 seconds.");
        }
        [Test] public void RandomEnemyCallbacksExcludeNeutralTreesAndBossPoolEntries()
        {
            var victim=EnemyAtHurtCenter(Vector2.zero);var ordinary=EnemyAtHurtCenter(Vector2.right*8);
            foreach(string role in new[]{"Tree","Boss","Elite"})
            {
                var other=EnemyAtHurtCenter(Vector2.right);Set(other,"type",new EnemyEntry{id=role=="Tree"?"tree":"predator",role=role});
            }
            for(int i=0;i<16;i++)Assert.AreSame(ordinary,Call("RandomStarterTarget",victim),
                "EntitySpawner keeps neutrals and bosses outside get_rand_enemy's ordinary enemy array.");
            ((IList)Get("enemies")).Remove(ordinary);
            Assert.IsNull(Call("RandomStarterTarget",victim));
        }
        [TestCase(0,1)] [TestCase(1,1)] [TestCase(2,2)]
        public void ActualDelayedProjectileActivatesAfterItsSourceDeferredCountdown(int delayFrames,int contactTick)
        {
            var profile=SupportProjectileBehavior.Structure("tyler");Assert.IsTrue(profile.delayed);
            var enemy=EnemyAtHurtCenter(new Vector2(profile.hitOffset[0],profile.hitOffset[1]));
            Set(enemy,"type",game.catalog.Enemy("baby_alien"));
            var shot=AdvancedShot(delayFrames/60f);
            shotView.AddComponent<SpriteRenderer>();
            Set(shot,"projectileRef",profile);Set(shot,"pos",Vector2.zero);Set(shot,"age",0f);
            Set(shot,"weapon",game.catalog.Weapon("pistol",1));Set(shot,"damage",3f);Set(shot,"crit",-100f);Set(shot,"pierce",99);
            Set(shot,"delayedHitboxPending",true);((IList)Get("shots")).Add(shot);
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,0f)),
                "The source delayed hitbox is disabled at the initial spawn snapshot, including a zero countdown.");
            for(int tick=1;tick<=contactTick;tick++)
            {
                Call("UpdateWeapons",1f/60);
                Assert.AreEqual(tick<contactTick?1000:997,Field<float>(enemy,"hp"),
                    "Native Godot emits zero/one-frame delay contacts at deferred flush1, two-frame delay at flush2; this caller resolves after movement.");
            }
        }
    }
}
