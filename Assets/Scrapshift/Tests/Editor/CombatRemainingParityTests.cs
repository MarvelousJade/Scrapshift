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
    public sealed class CombatRemainingParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        readonly List<GameObject> objects=new List<GameObject>();
        UnityEngine.Random.State previousRandom;
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        void Set(string name,object value)=>typeof(ArenaGame).GetField(name,Hidden).SetValue(game,value);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        static void Field(object obj,string name,object value)=>obj.GetType().GetField(name).SetValue(obj,value);
        static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name).GetValue(obj);
        GameObject Object(string name)
        {var obj=new GameObject(name);objects.Add(obj);return obj;}
        [SetUp] public void Setup()
        {
            previousRandom=UnityEngine.Random.state;
            game=Object("Combat parity test").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
            Set("sound",game.gameObject.AddComponent<GameAudio>());
            game.sparkPrefab=Object("Combat parity spark");game.sparkPrefab.AddComponent<SpriteRenderer>();
        }
        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"sparks","impacts","pickups","shots"})foreach(var effect in (IEnumerable)Get(list))
            {var obj=Field<GameObject>(effect,"obj");if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
            foreach(var obj in objects)if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);
            objects.Clear();UnityEngine.Random.state=previousRandom;
        }
        object Enemy(float health=100,float armor=0)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Field(enemy,"hp",health);Field(enemy,"maxHp",health);Field(enemy,"pos",Vector2.up);
            Field(enemy,"type",new EnemyEntry{id="combat-parity-target",role="Enemy",armor=armor});
            Field(enemy,"renderers",Array.Empty<Renderer>());((IList)Get("enemies")).Add(enemy);return enemy;
        }
        [Test] public void BurningTickBypassesArmorWithoutCritLifeStealOrGlobalKnockback()
        {
            // unit.gd BurningTimer creates TakeDamageArgs without a hitbox;
            // weapon.gd _on_Hitbox_hit_something owns the life-steal callback.
            game.Rules.GainWeaponStat("lifesteal",100);game.Rules.GainWeaponStat("knockback",10);
            game.Rules.GainWeaponStat("crit_chance",20000);game.Rules.Hurt(4);
            var enemy=Enemy(100,99);Field(enemy,"knockback",Vector2.right);
            float health=game.Rules.Health;
            bool critical=(bool)Call("Damage",0,3f,-100f,0f,Vector2.up,null,true,true,false);
            Assert.IsFalse(critical,"A burn has no critical hitbox even above the old sentinel's crit threshold.");
            Assert.AreEqual(97,Field<float>(enemy,"hp"));Assert.AreEqual(health,game.Rules.Health);
            Assert.AreEqual(Vector2.zero,Field<Vector2>(enemy,"knockback"));
            Assert.IsTrue(game.Rules.TryLifeSteal(0),"The burn must not consume the weapon life-steal cooldown either.");
        }
        [Test] public void OrdinaryWeaponHitStillAppliesArmorLifeStealAndGlobalKnockback()
        {
            game.Rules.GainWeaponStat("lifesteal",100);game.Rules.GainWeaponStat("knockback",10);game.Rules.Hurt(4);
            var enemy=Enemy(100,2);float health=game.Rules.Health;
            Call("Damage",0,5f,0f,0f,Vector2.up,game.catalog.Weapon("pistol",1),false,false,false);
            Assert.AreEqual(97,Field<float>(enemy,"hp"));Assert.AreEqual(health+1,game.Rules.Health);
            Assert.That(Field<Vector2>(enemy,"knockback").y,Is.EqualTo(10).Within(.0001f));
        }
        [TestCase("Boss",3,25,true,0,0,false,6)]
        [TestCase("Elite",3,25,true,0,0,false,6)]
        [TestCase("Boss",7,50,false,3,0,false,7)]
        [TestCase("Boss",3,25,false,0,10,true,3)]
        [TestCase("Enemy",15,0,false,0,10,false,17)]
        [TestCase("Enemy",15,0,false,2,10,false,15)]
        [TestCase("Enemy",15,0,false,99,10,true,17)]
        [TestCase("Boss",5,50,true,1,10,false,14)]
        public void DamageAppliesBossBonusBeforeCriticalThenRoundsVulnerabilityBeforeArmor(string role,float incoming,float bossBonus,bool critical,float armor,float vulnerability,bool burn,float expected)
        {
            // boss.gd:53 -> unit.gd:take_damage -> get_damage_value:500.
            // These values distinguish truncating the boss input, Godot's half
            // rounding, flat armor and hitbox-free burning from the old order.
            game.Rules.GainWeaponStat("damage_against_bosses",bossBonus);
            game.Rules.GainWeaponStat("crit_chance",critical?1000:-1000);
            var enemy=Enemy(100,armor);Field<EnemyEntry>(enemy,"type").role=role;
            Field(enemy,"iceClock",vulnerability>0?3f:0f);Field(enemy,"icePercent",vulnerability);
            Call("Damage",0,incoming,0f,0f,Vector2.up,burn?null:new Equipment{critMultiplier=2},true,burn,false);
            Assert.AreEqual(100-expected,Field<float>(enemy,"hp"));
        }
        [Test] public void ItemDamageWithoutAHitboxCannotCritEvenWithExtremePlayerCriticalChance()
        {
            game.Rules.GainWeaponStat("crit_chance",20000);var enemy=Enemy(100,2);
            Assert.IsFalse((bool)Call("Damage",0,3f,-100f,0f,Vector2.up,null,true,false,false));
            Assert.AreEqual(99,Field<float>(enemy,"hp"));
        }
        [TestCase(true,13)]
        [TestCase(false,3)]
        public void GreekFireIncludesASuccessfulBurnApplyingHitButNotAnOrdinaryHitOnAnAlreadyBurningEnemy(bool appliesBurn,float expected)
        {
            game.Rules.Items.Add(Array.Find(game.catalog.data.items,item=>item.id=="greek_fire"));
            game.Rules.GainWeaponStat("crit_chance",-1000);
            var enemy=Enemy();Field(enemy,"burnTicks",3);Field(enemy,"burnDamage",3f);
            Call("Damage",0,3f,0f,0f,Vector2.up,game.catalog.Weapon(appliesBurn?"wand":"pistol",1),false,false,false);
            Assert.AreEqual(100-expected,Field<float>(enemy,"hp"));
        }
        [Test] public void BurningTransitionsResetToothSlowButRefreshPreservesItAndTheStrongerScalingStat()
        {
            var enemy=Enemy();Field(enemy,"itemSlow",.2f);Field(enemy,"contactSlow",.3f);
            Call("ApplyBurn",enemy,5f,3,0,"engineering");
            Assert.AreEqual(0,Field<float>(enemy,"itemSlow"));Assert.AreEqual(.3f,Field<float>(enemy,"contactSlow"));
            Field(enemy,"itemSlow",.4f);Call("ApplyBurn",enemy,3f,2,0,"elemental_damage");
            Assert.AreEqual(.4f,Field<float>(enemy,"itemSlow"));Assert.AreEqual("engineering",Field<string>(enemy,"burnStat"));
            Field(enemy,"burnTicks",1);Field(enemy,"burnClock",.01f);Call("UpdateStarterEffects",.02f);
            Assert.AreEqual(0,Field<int>(enemy,"burnTicks"));Assert.AreEqual(0,Field<float>(enemy,"itemSlow"));
        }
        [Test] public void NonElementalBurnApplicationWaitsForItsBurnTickBeforeApplyingIceCube()
        {
            game.Rules.Items.Add(Array.Find(game.catalog.data.items,item=>item.id=="ice_cube"));
            var enemy=Enemy();Call("ApplyBurn",enemy,15f,3,0,"elemental_damage");
            Assert.IsFalse(Field<bool>(enemy,"iceApplied"),"PercentDamageTaken.on_burned is called by BurningTimer, not apply_burning.");
            Field(enemy,"burnClock",.01f);Call("UpdateStarterEffects",.02f);
            Assert.IsTrue(Field<bool>(enemy,"iceApplied"));Assert.AreEqual(83,Field<float>(enemy,"hp"));
        }
        [TestCase("slasher_egg")]
        [TestCase("infected_slasher_egg")]
        public void EggHatchingFiresNormalGlobalDeathAndMaterialCallbacksWithoutAWeaponHit(string id)
        {
            var enemy=Enemy();Field(enemy,"type",new EnemyEntry{id=id,role="Chaser",value=1});
            ((IDictionary)Get("encounterDefinitions")).Add(id,new EncounterEntry{id=id});
            int callbacks=0;
            game.Rules.Items.Add(new ItemEntry{id="hatch-death-observer",effects=Array.Empty<StatEffect>(),
                conditionalEffects=new[]{new ConditionalEffect{customKey="dmg_when_death",key="stat_luck",value=100,chance=1}}});
            game.Rules.RandomDamageRequested+=_=>callbacks++;
            Call("FireEncounter",enemy,new EncounterAttack{spawn=true,summon="slasher",count=1});
            Assert.AreEqual(0,((IList)Get("enemies")).Count);Assert.AreEqual(1,game.Rules.Kills);
            Assert.AreEqual(1,callbacks);Assert.AreEqual(1,((IList)Get("pickups")).Count);
            Assert.IsFalse(Field<bool>(((IList)Get("pickups"))[0],"fruit"));
        }
        object IncomingShot(bool healing)
        {
            var shot=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Shot",BindingFlags.NonPublic),true);
            Field(shot,"pos",Vector2.down*.12f);Field(shot,"life",1f);Field(shot,"damage",1f);Field(shot,"hostile",!healing);
            if(healing)Field(shot,"structureSpec",new StructureSpec{healing=true});return shot;
        }
        int ProjectileContacts=>(int)typeof(RunRules).GetField("projectileHits",Hidden).GetValue(game.Rules);
        [TestCase(false)] [TestCase(true)]
        public void DisabledPlayerHurtboxDoesNotConsumeOrCountNewProjectileContacts(bool healing)
        {
            var shot=IncomingShot(healing);
            Assert.IsFalse((bool)Call("ResolvePlayerProjectileContact",shot,Vector2.down*.12f,false));
            Assert.AreEqual(1,Field<float>(shot,"life"));Assert.AreEqual(0,ProjectileContacts);
        }
        [Test] public void ProtectedProjectileContactCountsWithoutHealthLoss()
        {
            typeof(RunRules).GetProperty("HitProtection").SetValue(game.Rules,1);
            var shot=IncomingShot(false);
            Assert.IsTrue((bool)Call("ResolvePlayerProjectileContact",shot,Vector2.down*.12f,true));
            Assert.AreEqual(10,game.Rules.Health);Assert.IsTrue(game.Rules.LastHitProtected);
            Assert.AreEqual(1,ProjectileContacts);Assert.AreEqual(0,Field<float>(shot,"life"));
        }
        [Test] public void ContactsAlreadyQueuedInOneBatchStillCountDuringDamageInvulnerability()
        {
            Set("invulnerable",.2f);var shot=IncomingShot(false);
            Assert.IsTrue((bool)Call("ResolvePlayerProjectileContact",shot,Vector2.down*.12f,true));
            Assert.AreEqual(10,game.Rules.Health);Assert.AreEqual(1,ProjectileContacts);
        }
        [Test] public void HealingProjectileContactCountsEvenAtFullHealth()
        {
            var shot=IncomingShot(true);
            Assert.IsTrue((bool)Call("ResolvePlayerProjectileContact",shot,Vector2.down*.12f,true));
            Assert.AreEqual(10,game.Rules.Health);Assert.AreEqual(1,ProjectileContacts);
            Assert.AreEqual(0,Field<float>(shot,"life"));
        }
        [Test] public void OutOfRangeHealingProjectileDoesNotCountOrDisappear()
        {
            var shot=IncomingShot(true);Field(shot,"pos",Vector2.right*4);
            Assert.IsFalse((bool)Call("ResolvePlayerProjectileContact",shot,Vector2.right*4,true));
            Assert.AreEqual(0,ProjectileContacts);Assert.AreEqual(1,Field<float>(shot,"life"));
        }
        [Test] public void ActiveMeleePoseUsesTheCurrentTrackedRootDirectionWithoutRestartingWindup()
        {
            // weapon.gd _physics_process/get_direction tracks the existing target
            // while shooting; melee should_rotate_manual separately freezes manual aim.
            var weapon=game.catalog.Weapon("knife",1);
            var view=Object("Combat melee view");view.AddComponent<SpriteRenderer>();
            ((GameObject[])Get("equipmentViews"))[0]=view;
            ((Vector2[])Get("weaponDirections"))[0]=Vector2.right;
            Call("BeginMeleeAttack",0,weapon,Vector2.right,1f);
            var attacks=(Array)Get("meleeAttacks");var attack=attacks.GetValue(0);
            float recoil=Field<float>(attack,"recoilTime");Assert.Greater(recoil,0);
            ((Vector2[])Get("weaponDirections"))[0]=Vector2.up;
            Call("TickMeleeAttack",0,weapon,Vector2.zero,recoil*.25f);
            Assert.That(view.transform.eulerAngles.z,Is.EqualTo(90).Within(.0001f));
            Assert.That(Field<float>(attack,"age"),Is.EqualTo(recoil*.25f).Within(.0001f));
            Assert.IsFalse(Field<bool>(attack,"sounded"),"Changing tracked direction must not skip recoil or enable damage early.");
            Assert.AreEqual(Vector2.right,Field<Vector2>(attack,"direction"),"The source hitbox captures its knockback vector when the attack starts.");
        }
        [Test] public void IncomingProjectileDoesNotHitAlongAnUnsampledTravelSegment()
        {
            var shot=IncomingShot(false);Field(shot,"pos",Vector2.right*2);
            int before=ProjectileContacts;
            Assert.IsFalse((bool)Call("ResolvePlayerProjectileContact",shot,Vector2.left*2,true));
            Assert.AreEqual(before,ProjectileContacts);Assert.AreEqual(1,Field<float>(shot,"life"));
        }
        static void Direction(float degrees,Vector2 actual)
        {Vector2 expected=Quaternion.Euler(0,0,degrees)*Vector2.right;Assert.Less(Vector2.Distance(expected,actual),.0001f);}
        [Test] public void AutomaticShotTrackingRetainsOneCapturedSpreadAsItsTargetMoves()
        {
            var weapon=game.catalog.Weapon("knife",1);var spreads=(float[])Get("weaponSpreads");
            spreads[0]=Mathf.PI/6; // The accuracy draw captured when this attack fired.
            Direction(120,(Vector2)Call("AimEquipment",0,weapon,true,false,Vector2.up,true,Vector2.zero,Vector2.right));
            Direction(210,(Vector2)Call("AimEquipment",0,weapon,true,false,Vector2.left,true,Vector2.zero,Vector2.right));
            Assert.AreEqual(Mathf.PI/6,spreads[0],"Tracking cannot rerandomize the launch spread.");
            Direction(210,(Vector2)Call("AimEquipment",0,weapon,true,false,Vector2.zero,false,Vector2.zero,Vector2.right));
            Direction(180,(Vector2)Call("AimEquipment",0,weapon,false,false,Vector2.left,true,Vector2.zero,Vector2.right));
        }
        [Test] public void ManualMeleeFreezesItsCapturedAngleWhileManualRangedAimFollowsInputWithoutOldSpread()
        {
            var knife=game.catalog.Weapon("knife",1);var pistol=game.catalog.Weapon("pistol",1);
            ((float[])Get("weaponSpreads"))[0]=Mathf.PI/6;
            var before=(Vector2)Call("AimEquipment",0,knife,true,false,Vector2.up,true,Vector2.zero,Vector2.right);
            Assert.AreEqual(before,(Vector2)Call("AimEquipment",0,knife,true,true,Vector2.left,true,Vector2.right,Vector2.right));
            Direction(0,(Vector2)Call("AimEquipment",0,pistol,true,true,Vector2.left,true,Vector2.right,Vector2.right));
            Direction(90,(Vector2)Call("AimEquipment",0,knife,false,true,Vector2.left,true,Vector2.up,Vector2.right));
        }
    }
}
