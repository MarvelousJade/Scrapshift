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
    public sealed class WeaponNextParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;GameObject owner,spark;
        UnityEngine.Random.State randomState;
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        static void Set(object obj,string name,object value)=>obj.GetType().GetField(name).SetValue(obj,value);
        static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name).GetValue(obj);
        [SetUp] public void Setup()
        {
            randomState=UnityEngine.Random.state;owner=new GameObject("Weapon next parity");game=owner.AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();game.Rules.Inventory.Clear();
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,owner.AddComponent<GameAudio>());
            spark=new GameObject("Weapon parity spark");spark.AddComponent<SpriteRenderer>();game.sparkPrefab=spark;
        }
        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"weaponExplosions","shots","sparks","impacts","pickups"})foreach(var effect in (IEnumerable)Get(list))
            {var obj=Field<GameObject>(effect,"obj");if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
            UnityEngine.Object.DestroyImmediate(spark);UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Random.state=randomState;
        }
        object Enemy(Vector2 pos,string role="Enemy")
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(enemy,"hp",10000f);Set(enemy,"maxHp",10000f);Set(enemy,"pos",pos);
            Set(enemy,"type",new EnemyEntry{id=role=="Boss"?"predator":"baby_alien",role=role});Set(enemy,"renderers",Array.Empty<Renderer>());
            ((IList)Get("enemies")).Add(enemy);return enemy;
        }
        void HitEffect(object enemy,Equipment weapon,bool critical,bool killed,int slot,bool secondary=false)
        {
            if(killed){Call("ApplyStarterKillStatistics",weapon,slot);if(!secondary)Call("ApplyStarterCriticalGold",weapon,critical);}
            Call("ApplyStarterHitEffects",enemy,weapon,critical,killed,secondary,false,game.Rules.WeaponDamage(weapon),slot,null);
        }

        [TestCase("ghost_axe","percent_damage")]
        [TestCase("ghost_flint","attack_speed")]
        [TestCase("ghost_scepter","max_hp")]
        public void IdenticalGhostCopiesKeepIndependentPerWaveKillCounts(string id,string stat)
        {
            var weapon=game.catalog.Weapon(id,1);game.Rules.Inventory.Add(weapon);game.Rules.Inventory.Add(weapon);
            var trait=weapon.Trait("effect_gain_stat_every_killed_enemies");int threshold=(int)trait.value;
            float before=game.Rules.Stat(stat);
            for(int i=0;i<threshold-1;i++){HitEffect(null,weapon,false,true,0);HitEffect(null,weapon,false,true,1);}
            Assert.AreEqual(before,game.Rules.Stat(stat),"Two catalog references must not combine their kills.");
            HitEffect(null,weapon,false,true,0);Assert.AreEqual(before+trait.amount,game.Rules.Stat(stat));
            Call("ClearStarterEffects");HitEffect(null,weapon,false,true,1);
            Assert.AreEqual(before+trait.amount,game.Rules.Stat(stat),"The next wave starts every physical weapon counter at zero.");
            for(int i=1;i<threshold;i++)HitEffect(null,weapon,false,true,1);
            Assert.AreEqual(before+2*trait.amount,game.Rules.Stat(stat));
        }
        [Test] public void DrillCriticalKillGrantsCurrencyWithoutPickupXpOrBankConsumption()
        {
            var weapon=game.catalog.Weapon("drill",4);game.Rules.BankMaterials(17);game.Rules.GainWeaponStat("chance_double_gold",100);
            int scrap=game.Rules.Scrap,xp=game.Rules.XP;HitEffect(null,weapon,true,true,0);
            Assert.AreEqual(scrap+1,game.Rules.Scrap);Assert.AreEqual(xp,game.Rules.XP);
            Assert.AreEqual(0,((IList)Get("pickups")).Count);
            Assert.AreEqual(2,game.Rules.PrepareMaterialDrop(1),"Immediate currency leaves the saved-material bank available to a real drop.");
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void ThiefDaggerChanceIncludesItsExactSerializedBoundary(int tier)
        {
            var trait=game.catalog.Weapon("dagger",tier).Trait("gold_on_crit_kill");
            Assert.IsTrue(SpecialWeaponRules.CriticalMaterials(trait,true,true,trait.value/100.0));
            Assert.IsFalse(SpecialWeaponRules.CriticalMaterials(trait,true,true,trait.value/100.0+.0001));
            Assert.IsFalse(SpecialWeaponRules.CriticalMaterials(trait,false,true,0));
            Assert.IsFalse(SpecialWeaponRules.CriticalMaterials(trait,true,false,0));
        }
        [TestCase("rocket_launcher",2)] [TestCase("shredder",1)] [TestCase("plank",1)] [TestCase("fireball",2)]
        public void ExplosiveDamageCombinesPercentagesBeforeOneRounding(string id,int tier)
        {
            var weapon=game.catalog.Weapon(id,tier);game.Rules.GainWeaponStat("percent_damage",25);game.Rules.GainWeaponStat("explosion_damage",50);
            float basis=weapon.damage+weapon.scaling.Sum(s=>game.Rules.Stat(s.key)*s.value);
            float expected=Math.Max(1,EnemyReferenceRules.SourceRound(Math.Max(1,(int)basis)*1.75f));
            Assert.AreEqual(expected,game.Rules.WeaponDamage(weapon));
        }
        [Test] public void RocketContactDefersBothVictimsToOneExplosionAndDoesNotReenterOrHealAgain()
        {
            var weapon=game.catalog.Weapon("rocket_launcher",2);game.Rules.GainWeaponStat("crit_chance",-1000);game.Rules.GainWeaponStat("lifesteal",100);
            game.Rules.Hurt(4);var victim=Enemy(Vector2.up);var neighbor=Enemy(Vector2.up+Vector2.right*.2f);
            float damage=game.Rules.WeaponDamage(weapon),health=game.Rules.Health;
            Call("DamageWithWeaponIndex",0,damage,weapon.crit,0f,Vector2.up,weapon,false,false,false,0,null);
            Assert.AreEqual(10000,Field<float>(victim,"hp"));Assert.AreEqual(health+1,game.Rules.Health);
            Assert.AreEqual(1,((IList)Get("weaponExplosions")).Count);
            Call("TickReferenceExplosions",.01f);
            Assert.AreEqual(10000-damage,Field<float>(victim,"hp"));Assert.AreEqual(10000-damage,Field<float>(neighbor,"hp"));
            Assert.AreEqual(health+1,game.Rules.Health,"Explosion hits have no weapon life-steal callback.");
            Assert.AreEqual(1,((IList)Get("weaponExplosions")).Count,"Explosion hitboxes have no ExplodingEffect to recurse.");
            Assert.IsFalse((bool)Get("resolvingWeaponExplosion"));
        }
        [Test] public void MeleeContactAndInheritedWeaponCallbackCanEachSpawnAnExplosion()
        {
            // Real Plank's inherited weapon.tscn connection runs a second roll
            // after Unit's interception. Find a deterministic pair of successes.
            var weapon=game.catalog.Weapon("plank",4);var victim=Enemy(Vector2.up);int seed=0;
            for(;seed<10000;seed++){UnityEngine.Random.InitState(seed);var rolls=new[]{UnityEngine.Random.value,UnityEngine.Random.value,UnityEngine.Random.value};if(rolls[0]<=.5f&&rolls[2]<=.5f)break;}
            Assert.Less(seed,10000);UnityEngine.Random.InitState(seed);
            Assert.IsTrue((bool)Call("TryInterceptWeaponExplosion",victim,weapon,game.Rules.WeaponDamage(weapon),false,0,null));
            var explosions=(IList)Get("weaponExplosions");Assert.AreEqual(2,explosions.Count);
            var firstHit=(IEnumerable)explosions[0].GetType().GetField("hit").GetValue(explosions[0]);
            var secondHit=(IEnumerable)explosions[1].GetType().GetField("hit").GetValue(explosions[1]);
            Assert.IsFalse(firstHit.Cast<object>().Contains(victim));Assert.IsTrue(secondHit.Cast<object>().Contains(victim));
        }
        [Test] public void TaserTraitDoesNotApplyASecondDirectVictimSlow()
        {
            var victim=Enemy(Vector2.up);Set(victim,"contactSlow",1.25f);
            HitEffect(victim,game.catalog.Weapon("taser",1),false,false,0);
            Assert.AreEqual(1.25f,Field<float>(victim,"contactSlow"));
        }
        [Test] public void LightningTargetSelectionUsesRandomOrdinaryEnemiesInsteadOfClosestOrBosses()
        {
            var victim=Enemy(Vector2.zero);var near=Enemy(Vector2.right*.1f);var far=Enemy(Vector2.right*8);Enemy(Vector2.right*.01f,"Boss");
            var found=new HashSet<object>();UnityEngine.Random.InitState(51);
            for(int i=0;i<64;i++)found.Add(Call("RandomStarterTarget",victim));
            CollectionAssert.AreEquivalent(new[]{near,far},found);
        }
        [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void VorpalExecutionIncludesExactPositiveChanceBoundary(int tier)
        {
            var weapon=game.catalog.Weapon("vorpal_sword",tier);float chance=weapon.Trait("one_shot_on_hit").value/100;
            Assert.IsTrue(SpecialWeaponRules.Execute(weapon,chance));Assert.IsFalse(SpecialWeaponRules.Execute(weapon,chance+.0001f));
            Assert.IsFalse(SpecialWeaponRules.Execute(new Equipment{traits=new[]{new WeaponTrait{key="one_shot_on_hit",value=0}}},0));
        }
        [TestCase("baby_alien")] [TestCase("bruiser")] [TestCase("predator")]
        public void BurnSpreadUsesSourceParticleCenterAndTargetPhysicsBodyEdge(string target)
        {
            Vector2 source=Vector2.up*2,center=source+CombatGeometry.OriginOffset("baby_alien")+Vector2.up*.24f;
            float radius=1.28f+CombatGeometry.Enemy(target).body;
            Vector2 edge=center+Vector2.right*(radius-.0001f)-CombatGeometry.BodyOffset(target);
            Assert.IsTrue(SpecialWeaponRules.BurnSpreadOverlaps("baby_alien",source,target,edge));
            Assert.IsFalse(SpecialWeaponRules.BurnSpreadOverlaps("baby_alien",source,target,edge+Vector2.right*.0002f));
            Assert.IsFalse(SpecialWeaponRules.BurnSpreadOverlaps("baby_alien",source,"tree",center),"SpreadArea mask 4 excludes tree physics layer 1.");
        }
        [TestCase(1,1,0,55f/60)] [TestCase(1,1,1,1)]
        [TestCase(5,6,0,2.5f)] [TestCase(5,6,.5f,3)] [TestCase(5,6,1,3.5f)]
        public void InitialWeaponTimerUsesCappedBasisRandomizationThenResetClamp(float seconds,int copies,float roll,float expected)
        {Assert.AreEqual(expected,SpecialWeaponRules.InitialCooldown(seconds,copies,roll),.00001f);}
        [Test] public void MineDamageTruncatesScaledBaseThenRoundsCombinedStructureAndExplosionPercent()
        {
            var stats=game.catalog.Weapon("screwdriver",1).Trait("landmine").projectile.AsEquipment();
            game.Rules.GainWeaponStat("engineering",7.5f);game.Rules.GainWeaponStat("structure_percent_damage",25);
            game.Rules.GainWeaponStat("explosion_damage",50);game.Rules.GainWeaponStat("percent_damage",1000);
            Assert.AreEqual(21,SpecialWeaponRules.StructureDamage(game.Rules,stats));
            Assert.AreEqual(30,SpecialWeaponRules.StructureDamage(game.Rules,stats,game.Rules.Stat("explosion_damage")));
        }
        [TestCase("knife",1,1.5f)] [TestCase("knife",3,1.51f)]
        [TestCase("pistol",.75f,4)] [TestCase("pistol",-.5f,3.99f)]
        public void WeaponRangeAssignsWholeSourcePixelsAfterMeleeHalving(string id,float addedRange,float expected)
        {
            game.Rules.GainWeaponStat("range",addedRange);
            Assert.AreEqual(expected,game.Rules.WeaponRange(game.catalog.Weapon(id,1)),.000001f);
        }
        [Test] public void SupportExplosionAcceptsLaterEnteringVictimAndHitsEachVictimOnce()
        {
            game.Rules.GainWeaponStat("crit_chance",-1000);
            var stats=game.catalog.Weapon("screwdriver",1).Trait("landmine").projectile.AsEquipment();
            var victim=Enemy(Vector2.right*8);
            Call("QueueSupportExplosion",Vector2.zero,stats,10f,1.5f,true,null,null);
            Call("TickReferenceExplosions",.01f);Assert.AreEqual(10000,Field<float>(victim,"hp"));
            Set(victim,"pos",Vector2.zero);Call("TickReferenceExplosions",.01f);
            Assert.AreEqual(9990,Field<float>(victim,"hp"));
            Call("TickReferenceExplosions",.01f);Assert.AreEqual(9990,Field<float>(victim,"hp"));
        }
        [Test] public void ExplosionKillCountAccumulatesAcrossItsActiveFrames()
        {
            game.Rules.GainWeaponStat("crit_chance",-1000);
            var stats=game.catalog.Weapon("screwdriver",1).Trait("landmine").projectile.AsEquipment();
            var later=new List<object>();
            for(int i=0;i<15;i++){var enemy=Enemy(i<8?Vector2.zero:Vector2.right*8);Set(enemy,"hp",1f);if(i>=8)later.Add(enemy);}
            Call("QueueSupportExplosion",Vector2.zero,stats,10f,1.5f,true,null,null);
            Call("TickReferenceExplosions",.01f);Assert.AreEqual(0,game.Rules.Kills,"Global deaths remain deferred after immediate hitbox kills.");
            var explosion=((IList)Get("weaponExplosions"))[0];Assert.AreEqual(8,Field<int>(explosion,"kills"));
            Call("ResolveCombatCallbacks");Assert.AreEqual(8,game.Rules.Kills);
            foreach(var enemy in later)Set(enemy,"pos",Vector2.zero);
            Call("TickReferenceExplosions",.01f);Assert.AreEqual(8,game.Rules.Kills);
            Call("ResolveCombatCallbacks");Assert.AreEqual(15,game.Rules.Kills);
            Assert.AreEqual(15,Field<int>(explosion,"kills"),"PlayerExplosion's Fireworks counter belongs to the whole explosion, not an individual physics tick.");
        }
        void WisdomBurnSetup(bool sausage)
        {
            game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="wisdom"));
            if(sausage)for(int i=0;i<4;i++)game.Rules.GrantItem(game.catalog.data.items.Single(item=>item.id=="scared_sausage"));
            game.Rules.GainWeaponStat("elemental_damage",10);
        }
        [TestCase("fireball",2,false,11,12)] [TestCase("pistol",1,true,12,13)]
        public void FiredBurnPayloadRetainsDamageAcrossRealWisdomTick(string id,int tier,bool sausage,float before,float after)
        {
            WisdomBurnSetup(sausage);var weapon=game.catalog.Weapon(id,tier);
            var fired=game.Rules.CaptureBurn(weapon);Assert.NotNull(fired);Assert.AreEqual(before,fired.damage);
            game.Rules.Tick(5);game.Rules.GainWeaponStat("burning_spread",2);
            var next=game.Rules.CaptureBurn(weapon);Assert.AreEqual(after,next.damage);Assert.AreEqual(2,next.spread);
            Assert.AreEqual(before,fired.damage);Assert.AreEqual(0,fired.spread);
            var victim=Enemy(Vector2.up);Assert.IsTrue((bool)Call("TryApplyWeaponBurn",victim,weapon,false,fired));
            Assert.AreEqual(before,Field<float>(victim,"burnDamage"));Assert.AreEqual(0,Field<int>(victim,"burnSpread"));
            var later=Enemy(Vector2.up);Assert.IsTrue((bool)Call("TryApplyWeaponBurn",later,weapon,false,next));
            Assert.AreEqual(after,Field<float>(later,"burnDamage"));Assert.AreEqual(2,Field<int>(later,"burnSpread"));
        }
        [Test] public void EmptyFiredPayloadCannotAcquireALaterGlobalBurnEffect()
        {
            // Preventive snapshot boundary: an explicit empty payload differs
            // from the null compatibility path that calculates a live hitbox.
            var weapon=game.catalog.Weapon("pistol",1);var empty=game.Rules.CaptureBurn(weapon);
            Assert.NotNull(empty);Assert.IsFalse(empty.Succeeds(0));
            for(int i=0;i<4;i++)game.Rules.GrantItem(game.catalog.data.items.Single(item=>item.id=="scared_sausage"));
            var victim=Enemy(Vector2.up);
            Assert.IsFalse((bool)Call("TryApplyWeaponBurn",victim,weapon,false,empty));
            Assert.AreEqual(0,Field<int>(victim,"burnTicks"));
            Assert.IsTrue((bool)Call("TryApplyWeaponBurn",victim,weapon,false,null));
        }
        [Test] public void RocketExplosionInheritsFiringPayloadAfterWisdomChangesCurrentDamage()
        {
            WisdomBurnSetup(true);game.Rules.GainWeaponStat("crit_chance",-1000);
            var weapon=game.catalog.Weapon("rocket_launcher",2);var fired=game.Rules.CaptureBurn(weapon);
            float firedDamage=game.Rules.WeaponDamage(weapon);Assert.AreEqual(12,fired.damage);
            game.Rules.Tick(5);Assert.AreEqual(13,game.Rules.CaptureBurn(weapon).damage);
            var victim=Enemy(Vector2.up);
            Call("DamageWithWeaponIndex",0,firedDamage,weapon.crit,0f,Vector2.up,weapon,false,false,false,0,fired);
            Assert.AreEqual(0,Field<int>(victim,"burnTicks"),"An intercepted contact has not yet taken explosion damage.");
            var explosion=((IList)Get("weaponExplosions"))[0];Assert.AreSame(fired,Field<BurnPayload>(explosion,"burnPayload"));
            Call("TickReferenceExplosions",.01f);Assert.AreEqual(12,Field<float>(victim,"burnDamage"));
        }
        [Test] public void MeleeCompatibilityHitUsesLiveBurnAfterWisdomStatsRefresh()
        {
            WisdomBurnSetup(true);var weapon=game.catalog.Weapon("knife",1);var before=game.Rules.CaptureBurn(weapon);
            game.Rules.Tick(5);var victim=Enemy(Vector2.up);
            Assert.IsTrue((bool)Call("TryApplyWeaponBurn",victim,weapon,false,null));
            Assert.AreEqual(12,before.damage);Assert.AreEqual(13,Field<float>(victim,"burnDamage"));
        }
    }
}
