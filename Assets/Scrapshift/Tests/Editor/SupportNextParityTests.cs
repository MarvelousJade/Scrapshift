using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SupportNextParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        GameObject spark;
        UnityEngine.Random.State randomState;
        IList Enemies=>(IList)typeof(ArenaGame).GetField("enemies",Hidden).GetValue(game);
        IList Pets=>(IList)typeof(ArenaGame).GetField("pets",Hidden).GetValue(game);
        static void Set(object obj,string field,object value)=>obj.GetType().GetField(field).SetValue(obj,value);
        static T Get<T>(object obj,string field)=>(T)obj.GetType().GetField(field).GetValue(obj);
        object Call(string method,params object[] args)=>typeof(ArenaGame).GetMethod(method,Hidden).Invoke(game,args);
        [SetUp] public void Setup()
        {
            randomState=UnityEngine.Random.state;
            game=new GameObject("Support next source parity").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,game.gameObject.AddComponent<GameAudio>());
            spark=new GameObject("Support parity spark");spark.AddComponent<SpriteRenderer>();game.sparkPrefab=spark;
        }
        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"shots","sparks","impacts"})
                foreach(object effect in (IEnumerable)typeof(ArenaGame).GetField(list,Hidden).GetValue(game))
                    UnityEngine.Object.DestroyImmediate(Get<GameObject>(effect,"obj"));
            UnityEngine.Object.DestroyImmediate(spark);
            UnityEngine.Object.DestroyImmediate(game.gameObject);
            UnityEngine.Random.state=randomState;
        }
        object Enemy(string id,Vector2 position,string role="Chaser",float maxHp=100)
        {
            var e=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(e,"type",new EnemyEntry{id=id,role=role});Set(e,"pos",position);Set(e,"hp",100f);Set(e,"maxHp",maxHp);Set(e,"renderers",Array.Empty<Renderer>());Enemies.Add(e);return e;
        }
        object Pet(string kind,Vector2 position=default)
        {
            var p=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Pet",BindingFlags.NonPublic),true);
            var spec=new PetSpec{kind=kind,stats=new ProjectileStats{damage=1,interval=1,range=3,scaling=Array.Empty<StatEffect>()}};
            Set(p,"spec",spec);Set(p,"weapon",game.Rules.PetWeapon(spec.stats,spec));Set(p,"pos",position);Set(p,"clock",1f);Pets.Add(p);return p;
        }
        [Test] public void RatRetainsLiveTargetWhenItLeavesAnOccupiedRange()
        {
            var p=Pet("ratzilla");var first=Enemy("baby_alien",Vector2.right);Enemy("baby_alien",Vector2.right*2);
            Assert.AreSame(first,Call("SelectPetTarget",p));Set(first,"pos",Vector2.right*8);
            Assert.AreSame(first,Call("SelectPetTarget",p),"Range exit does not emit current-target death or clear the reservation.");
        }
        [Test] public void EmptyRangeEntryReplacesTheRetainedOutsideTarget()
        {
            var p=Pet("ratzilla");var original=Enemy("baby_alien",Vector2.right*8);
            Assert.AreSame(original,Call("SelectPetTarget",p));var entrant=Enemy("baby_alien",Vector2.right*3);
            Assert.AreSame(entrant,Call("SelectPetTarget",p));
        }
        [Test] public void DeadTargetReleasesReservationForAnotherPet()
        {
            var first=Pet("ratzilla");var second=Pet("ratzilla");var a=Enemy("baby_alien",Vector2.right);var b=Enemy("baby_alien",Vector2.right*2);
            Assert.AreSame(a,Call("SelectPetTarget",first));Assert.AreSame(b,Call("SelectPetTarget",second));
            Set(a,"hp",0f);Assert.IsNull(Call("SelectPetTarget",first));
            Enemies.Remove(b);Assert.IsNull(Call("SelectPetTarget",second));
        }
        [Test] public void BlazemanderKeepsBurningTargetUntilAnUnburnedBodyEnters()
        {
            var p=Pet("blazemander");var burning=Enemy("baby_alien",Vector2.right);Set(burning,"burnTicks",2);
            Assert.AreSame(burning,Call("SelectPetTarget",p));
            var fresh=Enemy("baby_alien",Vector2.right*3);Assert.AreSame(fresh,Call("SelectPetTarget",p));
            Set(fresh,"pos",Vector2.right*8);Assert.AreSame(fresh,Call("SelectPetTarget",p));
        }
        [Test] public void MeleeAcquisitionIncludesTheEnemyBodyPastTheAreaEdge()
        {
            var p=Pet("ratzilla");var outsideFallback=Enemy("baby_alien",Vector2.left*8);
            var overlapping=Enemy("baby_alien",new Vector2(6.1f,-.03f));
            Assert.AreSame(overlapping,Call("SelectPetTarget",p));Set(overlapping,"pos",Vector2.right*7);Set(overlapping,"hp",0f);
            Assert.AreSame(outsideFallback,Call("SelectPetTarget",p));
        }
        [Test] public void CatUsesItsFixedDetectionAreaAsWellAsModifiedWeaponReach()
        {
            var p=Pet("catling_gun");game.Rules.GainWeaponStat("range",500);
            var outside=Enemy("baby_alien",Vector2.right*5);Assert.IsNull(Call("CatGunTarget",p,Vector2.zero));
            Set(outside,"pos",Vector2.right*4.1f);Assert.AreSame(outside,Call("CatGunTarget",p,Vector2.zero));
        }
        [Test] public void RangedPetAcquisitionUsesRawEnemyOrigin()
        {
            var p=Pet("catling_gun");var e=Enemy("butcher",new Vector2(0,3.7f));
            Assert.AreSame(e,Call("CatGunTarget",p,Vector2.zero),"The butcher's raw origin is .88 below its adapted art center.");
        }
        [TestCase(false,100,0,4)][TestCase(true,100,0,3)][TestCase(true,0,100,4)]
        [TestCase(false,-1000,0,.25f)]
        public void PetRangeUsesTheCorrectStatAndSourceMinimum(bool structure,float range,float structureRange,float expected)
        {
            game.Rules.GainWeaponStat("range",range);game.Rules.GainWeaponStat("structure_range",structureRange);
            Assert.That(game.Rules.PetRange(new Equipment{range=3},structure),Is.EqualTo(expected).Within(.0001));
        }
        [TestCase("ratzilla",true)][TestCase("bonk_dog",true)][TestCase("blazemander",true)]
        [TestCase("lootworm",true)][TestCase("catling_gun",false)][TestCase("bot_o_mine",false)]
        public void PrimaryMeleePetsAreNotMisclassifiedAsRangedHits(string kind,bool melee)
            =>Assert.AreEqual(melee,Get<Equipment>(Pet(kind),"weapon").melee);
        [Test] public void PetProjectileCountIncludesTheRegisteredProjectilesStat()
        {
            var p=Pet("catling_gun");game.Rules.GainWeaponStat("projectiles",2);
            Call("FirePet",p,Vector2.up,Vector2.right*10,Get<Equipment>(p,"weapon"));
            var shots=(IList)typeof(ArenaGame).GetField("shots",Hidden).GetValue(game);Assert.AreEqual(3,shots.Count);
        }
        [Test] public void PetShotAimsFromSourceRootWhileSpawningAtTheGunMuzzle()
        {
            var p=Pet("catling_gun");var muzzle=new Vector2(0,1);
            Call("FirePet",p,muzzle,new Vector2(10,-.24f),Get<Equipment>(p,"weapon"));
            var shots=(IList)typeof(ArenaGame).GetField("shots",Hidden).GetValue(game);
            Assert.AreEqual(muzzle,Get<Vector2>(shots[0],"pos"));
            Assert.That(Get<Vector2>(shots[0],"velocity").normalized,Is.EqualTo(Vector2.right));
        }
        [Test] public void LootwormTreeDamagePersistsAfterTheLargeTreeLeaves()
        {
            var p=Pet("lootworm");var large=Enemy("tree",Vector2.up*1.1f,"Tree",500);
            Call("TickLootwormAttack",p);Assert.AreEqual(100,Get<float>(p,"treeDamage"));
            Enemies.Remove(large);Enemy("tree",Vector2.up*1.1f,"Tree",10);Call("TickLootwormAttack",p);
            Assert.AreEqual(100,Get<float>(p,"treeDamage"));
            var another=Pet("lootworm");Call("TickLootwormAttack",another);Assert.AreEqual(2,Get<float>(another,"treeDamage"));
        }
        [Test] public void LootwormTreeAreaCanStartAPulseWithoutANavigationTarget()
        {
            var p=Pet("lootworm");Enemy("tree",Vector2.up*1.1f,"Tree");Set(p,"clock",0f);
            // The tree body overlaps detection but its shifted hurtbox is outside
            // this pulse. This exercises activation without dispatching a hit.
            Assert.IsFalse(SupportBehavior.PetHitOverlaps("lootworm",Vector2.zero,"tree",Vector2.up*1.1f));
            Call("TickLootwormAttack",p);Assert.IsTrue(Get<bool>(p,"meleePulse"));Assert.IsNull(Get<object>(p,"target"));
            Call("TickLootwormAttack",p);Assert.IsFalse(Get<bool>(p,"meleePulse"));Assert.AreEqual(1,Get<float>(p,"clock"));
        }
        [TestCase("ratzilla",-.34f)][TestCase("bonk_dog",-.04f)][TestCase("blazemander",-.04f)][TestCase("lootworm",-.12f)]
        public void PetHitShapesKeepTheirOwnSourceOffsets(string kind,float y)=>Assert.AreEqual(new Vector2(0,y),SupportBehavior.PetHitOffset(kind));
        [Test] public void PetMeleeUsesHurtCenterRatherThanArtOrigin()
        {
            Assert.IsTrue(SupportBehavior.PetHitOverlaps("bonk_dog",Vector2.zero,"butcher",Vector2.up));
            Assert.IsFalse(SupportBehavior.PetHitOverlaps("bonk_dog",Vector2.zero,"baby_alien",Vector2.up));
        }
        [Test] public void MeleePetCanHitATreeWithoutSelectingItForMovement()
        {
            var p=Pet("ratzilla");var tree=Enemy("tree",Vector2.zero,"Tree");
            Assert.IsNull(Call("SelectPetTarget",p));Call("PetMelee",p,false);
            Assert.AreEqual(99,Get<float>(tree,"hp"),"Neutral Hurtbox mask1032 accepts the companion melee layer1024, although targeting Range mask4 excludes Trees.");
        }
        [Test] public void PersistentBlazeHitboxHitsNewEntrantsOnceUntilTheNextAttackReset()
        {
            var p=Pet("blazemander");var first=Enemy("baby_alien",Vector2.zero);var entering=Enemy("baby_alien",Vector2.right*2);
            Call("PetMelee",p,true);Call("PetMelee",p,true);Assert.AreEqual(99,Get<float>(first,"hp"));Assert.AreEqual(100,Get<float>(entering,"hp"));
            Set(entering,"pos",Vector2.zero);Call("PetMelee",p,true);Assert.AreEqual(99,Get<float>(entering,"hp"));
            Set(p,"meleePulse",true);Call("AdvancePetAttack",p,0f);Call("PetMelee",p,true);
            Assert.AreEqual(98,Get<float>(first,"hp"));Assert.AreEqual(98,Get<float>(entering,"hp"));
        }
        [TestCase("ratzilla",true)][TestCase("bonk_dog",true)][TestCase("bot_o_mine",true)]
        [TestCase("blazemander",true)][TestCase("catling_gun",false)][TestCase("lootworm",false)]
        [TestCase("doc_moth",false)][TestCase("scapegoat",false)][TestCase("jellyshield",false)]
        public void CatBoostRespectsTheSourcePetBodyLayer(string kind,bool expected)=>Assert.AreEqual(expected,SupportBehavior.CatBoostFromPet(Vector2.zero,kind,Vector2.zero));
        [Test] public void CatAndRevivalAreasIncludePlayerBodyRadius()
        {
            Assert.IsTrue(SupportBehavior.CatBoostFromPlayer(Vector2.zero,new Vector2(1.7f,-.12f)));
            Assert.IsTrue(SupportBehavior.CanReviveScapegoat(new Vector2(1.2f,.08f),Vector2.zero));
            Assert.IsFalse(SupportBehavior.CanReviveScapegoat(new Vector2(1.25f,.08f),Vector2.zero));
        }
        [Test] public void InvulnerableScapegoatDoesNotConsumeAnotherProjectile()
        {
            var p=Pet("scapegoat");Set(p,"invulnerable",.5f);
            Assert.IsFalse((bool)Call("PetBlocksProjectile",Vector2.left,Vector2.right,CombatGeometry.HostileRadius,false));
        }
        [Test] public void FriendlyHealingProjectilePassesShieldButHitsScapegoatForOne()
        {
            Pet("jellyshield");Assert.IsFalse((bool)Call("PetBlocksProjectile",Vector2.left,Vector2.right,1.04f,true));
            var goat=Pet("scapegoat");Set(goat,"health",10f);
            Assert.IsTrue((bool)Call("PetBlocksProjectile",Vector2.left,Vector2.right,1.04f,true));Assert.AreEqual(9,Get<float>(goat,"health"));
            Assert.IsFalse((bool)Call("PetBlocksProjectile",Vector2.left,Vector2.right,1.04f,true),"The goat's hurtbox is now disabled.");
        }
        [Test] public void ScapegoatRevivalRetainsFractionalProgressAndRisesOnTheFollowingTick()
        {
            var p=Pet("scapegoat");Set(p,"maxHealth",40f);Set(p,"health",0f);
            Assert.IsFalse((bool)Call("AdvanceScapegoatRevival",p,.025f));Assert.AreEqual(0,Get<float>(p,"health"));
            Assert.IsFalse((bool)Call("AdvanceScapegoatRevival",p,.025f));Assert.AreEqual(1,Get<float>(p,"health"));
            Assert.IsFalse((bool)Call("AdvanceScapegoatRevival",p,1.95f));Assert.AreEqual(40,Get<float>(p,"health"));
            Assert.IsTrue((bool)Call("AdvanceScapegoatRevival",p,0f));
        }
        [Test] public void BlazemanderAttackResetAndDelayedHitboxAreIndependent()
        {
            var p=Pet("blazemander");Set(p,"meleePulse",true);Set(p,"pendingMelee",true);Set(p,"meleeDelay",.15f);Set(p,"clock",0f);
            Assert.IsTrue((bool)Call("AdvancePetAttack",p,.01f));
            Assert.AreEqual(1,Get<float>(p,"clock"));Assert.IsFalse(Get<bool>(p,"meleePulse"));Assert.IsTrue(Get<bool>(p,"pendingMelee"));Assert.IsFalse(Get<bool>(p,"meleeHitActive"));
            Call("AdvancePetAttack",p,.15f);Assert.IsTrue(Get<bool>(p,"meleeHitActive"));Assert.IsFalse(Get<bool>(p,"pendingMelee"));
            Call("AdvancePetAttack",p,.1f);Assert.IsTrue(Get<bool>(p,"meleeHitActive"),"The source hitbox remains active, with per-enemy ignored objects, until the next attack reset.");
            Set(p,"meleePulse",true);Call("AdvancePetAttack",p,0f);Assert.IsFalse(Get<bool>(p,"meleeHitActive"));
        }
        [Test] public void StructureSelectionUsesRawOriginsAndTylerExcludesTrees()
        {
            var tree=Enemy("tree",Vector2.zero,"Tree");var butcher=Enemy("butcher",Vector2.up*2.7f);
            Assert.AreSame(tree,Call("StructureTarget",Vector2.zero,2f,true));
            Assert.AreSame(butcher,Call("StructureTarget",Vector2.zero,2f,false));
        }
        [TestCase("turret",-.24f,.12f)][TestCase("flame",-.24f,.12f)][TestCase("laser",-.24f,.12f)]
        [TestCase("rocket",-.24f,.12f)][TestCase("healing",-.24f,.12f)][TestCase("tyler",-.4f,.16f)]
        [TestCase("builder",-.24f,-.02f)]
        public void StructureRootsAndMuzzlesAreDistinct(string kind,float root,float muzzle)
        {
            Assert.AreEqual(new Vector2(0,root),SupportBehavior.StructureOriginOffset(kind));
            Assert.AreEqual(new Vector2(0,muzzle),SupportBehavior.StructureMuzzleOffset(kind));
        }
        [Test] public void WanderingZoneUsesShiftedHurtShapes()
        {
            Assert.IsTrue(SupportBehavior.WanderingZoneOverlaps(Vector2.zero,"butcher",Vector2.up*2.5f));
            Assert.IsFalse(SupportBehavior.WanderingZoneOverlaps(Vector2.zero,"baby_alien",Vector2.up*2.5f));
        }
        [Test] public void WanderingSlowCapsAgainstCurrentSpeedAfterPermanentReduction()
        {
            var enemy=Enemy("baby_alien",Vector2.zero);Set(enemy,"speed",2f);Set(enemy,"itemSlow",.5f);
            Call("ApplyWanderingSlow",enemy);
            Assert.That(Get<float>(enemy,"contactSlow"),Is.EqualTo(1.35f).Within(.0001),"Source current speed is 150px/s; the 250px slow is capped to 90%, or 135px/s.");
        }
        [Test] public void BotMineRetainsPetAndStructureDamageBonuses()
        {
            var stats=new ProjectileStats{damage=10,scaling=Array.Empty<StatEffect>()};
            var pet=new PetSpec{kind="bot_o_mine",stats=new ProjectileStats(),mine=new StructureSpec{kind="landmine",stats=stats,pet=true}};
            var mine=game.Rules.PetWeapon(stats,pet);
            game.Rules.GainWeaponStat("percent_damage",50);game.Rules.GainWeaponStat("structure_percent_damage",20);game.Rules.GainWeaponStat("explosion_damage",30);
            Assert.IsTrue(mine.pet,"The supplied bot_o_mine_landmines_effect.tres explicitly sets is_pet=true.");
            Assert.AreEqual(20,SpecialWeaponRules.StructureDamage(game.Rules,mine,game.Rules.Stat("explosion_damage")));
            Assert.AreEqual(15,SpecialWeaponRules.StructureDamage(game.Rules,new Equipment{damage=10,scaling=Array.Empty<StatEffect>()},game.Rules.Stat("explosion_damage")),"An ordinary mine does not receive the extra 50% ordinary Damage.");
        }
        [Test] public void ExplosionsUseTheSourceMinimumScaleAndOffsetHurtOverlap()
        {
            Assert.That(SupportBehavior.ExplosionRadius(1,-100),Is.EqualTo(.14734f).Within(.000001));
            Assert.That(SupportBehavior.ExplosionRadius(1,-500),Is.EqualTo(.14734f).Within(.000001));
            Assert.IsTrue(SupportBehavior.ExplosionOverlaps(Vector2.zero,1,"baby_alien",new Vector2(1.3f,.15f-.03f/1.4734f)));
            Assert.IsFalse(SupportBehavior.ExplosionOverlaps(Vector2.zero,1,"baby_alien",Vector2.right*1.4f));
        }
        object StructureShot(string kind,float speed=10)
        {
            var spec=new StructureSpec{kind=kind,stats=new ProjectileStats{range=3,damage=1,projectileSpeed=speed,scaling=Array.Empty<StatEffect>()},delayed=kind=="tyler"};
            Call("StarterProjectile",Vector2.zero,Vector2.right,spec.Equipment,true,null,false,true,spec);
            var shots=(IList)typeof(ArenaGame).GetField("shots",Hidden).GetValue(game);return shots[shots.Count-1];
        }
        [Test] public void FlameTurretUsesTheSourceStopTimeAndForwardHitOffset()
        {
            var shot=StructureShot("flame");var profile=Get<ProjectileReference>(shot,"projectileRef");
            Assert.That(Get<float>(shot,"life"),Is.EqualTo(.35f).Within(.0001));
            Assert.AreEqual(new[]{.24f,-.04f},profile.hitOffset);
            Assert.AreEqual(1,Get<float>(shot,"damage"));
            Assert.AreEqual("SupportProjectile_flame0",Get<GameObject>(shot,"obj").GetComponent<SpriteRenderer>().sprite.name,"Collision binding must preserve the approved support presentation.");
        }
        [Test] public void OnlyFlameTurretSpeedChangesWithOrdinaryRange()
        {
            game.Rules.GainWeaponStat("range",300);
            Assert.AreEqual(20,Get<Vector2>(StructureShot("flame"),"velocity").magnitude);
            Assert.AreEqual(10,Get<Vector2>(StructureShot("laser"),"velocity").magnitude);
            Assert.AreEqual(10,Get<Vector2>(StructureShot("rocket"),"velocity").magnitude);
        }
        [Test] public void HealingTurretAndTylerKeepTheirDistinctSourceContactRules()
        {
            var healing=StructureShot("healing");var profile=Get<ProjectileReference>(healing,"projectileRef");
            Assert.AreEqual(1.04f,profile.radius);Assert.AreEqual(new[]{0f,0f},profile.hitOffset);
            var tyler=StructureShot("tyler");Assert.IsTrue(Get<ProjectileReference>(tyler,"projectileRef").delayed);
            Assert.That(Get<float>(tyler,"hitDelay"),Is.InRange(0,.05f));
            Assert.AreEqual(new[]{.03f,.01f},Get<ProjectileReference>(tyler,"projectileRef").hitOffset);
        }
        [Test] public void BlazemanderKeepsFlameBindingThroughPetWeaponCopy()
        {
            var spec=new PetSpec{kind="blazemander",secondary=new ProjectileStats{damage=1,range=3,projectileSpeed=10,interval=1,scaling=Array.Empty<StatEffect>()}};
            var weapon=game.Rules.PetWeapon(spec.secondary,spec);
            Assert.AreEqual("projectiles/flames/flame_projectile.tscn",weapon.projectileScene);
            Call("StarterProjectile",Vector2.zero,Vector2.right,weapon,true,null,false,false,null);
            var shots=(IList)typeof(ArenaGame).GetField("shots",Hidden).GetValue(game);var shot=shots[shots.Count-1];
            Assert.AreEqual(.35f,Get<float>(shot,"life"));Assert.AreEqual("pet_flame",SupportProjectileBehavior.PetArtKind(weapon));
        }
    }
}
