using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class PickupNextParityTests
    {
        sealed class DropFixture:IDisposable
        {
            const BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance;
            readonly UnityEngine.Random.State random;
            public readonly ArenaGame game;
            public readonly IList pickups;
            readonly System.Collections.Generic.List<GameObject> petObjects=new System.Collections.Generic.List<GameObject>();
            public DropFixture()
            {
                random=UnityEngine.Random.state;UnityEngine.Random.InitState(2915);
                game=new GameObject("Enemy loot path test").AddComponent<ArenaGame>();
                game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
                game.Rules.Configure(game.catalog);game.Rules.Start();
                pickups=(IList)typeof(ArenaGame).GetField("pickups",Hidden).GetValue(game);
            }
            public void Drop(string id,string role,int value)
            {
                var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
                enemy.GetType().GetField("type").SetValue(enemy,new EnemyEntry{id=id,role=role,value=value});
                typeof(ArenaGame).GetMethod("DropEnemyLoot",Hidden).Invoke(game,new[]{enemy});
            }
            public int MaterialCount
            {
                get{int count=0;foreach(object pickup in pickups)if(!(bool)pickup.GetType().GetField("fruit").GetValue(pickup))count++;return count;}
            }
            public void Clear()
            {
                foreach(object pickup in pickups)UnityEngine.Object.DestroyImmediate((GameObject)pickup.GetType().GetField("obj").GetValue(pickup));
                pickups.Clear();
            }
            public object AddPet(string kind,bool dead=false)
            {
                var pet=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Pet",BindingFlags.NonPublic),true);
                var obj=new GameObject("Collector fixture "+kind);petObjects.Add(obj);
                pet.GetType().GetField("spec").SetValue(pet,new PetSpec{kind=kind});pet.GetType().GetField("obj").SetValue(pet,obj);pet.GetType().GetField("dead").SetValue(pet,dead);
                ((IList)typeof(ArenaGame).GetField("pets",Hidden).GetValue(game)).Add(pet);return pet;
            }
            public object AddEnemyCollector(Vector2 position)
            {
                var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
                var obj=new GameObject("Enemy collector fixture");petObjects.Add(obj);
                enemy.GetType().GetField("type").SetValue(enemy,new EnemyEntry{id="evil_mob"});enemy.GetType().GetField("pos").SetValue(enemy,position);
                enemy.GetType().GetField("hp").SetValue(enemy,100f);enemy.GetType().GetField("obj").SetValue(enemy,obj);return enemy;
            }
            public object ReadyMaterial(Vector2 position)
            {
                game.Rules.GainWeaponStat("luck",-100);Drop("baby_alien","Chaser",1);
                var pickup=pickups[pickups.Count-1];var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
                motion.position=position;motion.PushDestination=position;motion.Step(.2f,Vector2.zero,-1,true);return pickup;
            }
            public void TickPickups(float dt=0)=>typeof(ArenaGame).GetMethod("UpdatePickups",Hidden).Invoke(game,new object[]{dt});
            public void SetPlayer(Vector2 position)=>typeof(ArenaGame).GetField("playerPos",Hidden).SetValue(game,position);
            public void Dispose(){Clear();foreach(var obj in petObjects)UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(game.gameObject);UnityEngine.Random.state=random;}
        }
        [TestCase("junkie")][TestCase("dire_junkie")][TestCase("evil_mob")]
        [TestCase("slasher_egg")][TestCase("infected_slasher_egg")][TestCase("corrupted_tree")]
        public void SourceIneligibleUnitsCannotDropConsumablesEvenWithHighLuck(string id)
        {
            Assert.IsFalse(ReferenceLoot.CanDropConsumables(id));
            Assert.AreEqual(ConsumableKind.None,ReferenceLoot.Roll(id,10000,0,0,0,10000));
        }
        [TestCase("healer",.029,.029,ConsumableKind.Crate)]
        [TestCase("healer",.031,0,ConsumableKind.None)]
        [TestCase("mad_slasher",.029,.031,ConsumableKind.Fruit)]
        [TestCase("horned_spitter",.029,.099,ConsumableKind.Crate)]
        [TestCase("horned_spitter",.029,.101,ConsumableKind.Fruit)]
        [TestCase("tentacle",.019,.019,ConsumableKind.Crate)]
        [TestCase("tentacle",.021,0,ConsumableKind.None)]
        public void AdditionalEnemyDropProfilesUseTheirSuppliedStats(string id,double drop,double crate,ConsumableKind expected)
            =>Assert.AreEqual(expected,ReferenceLoot.Roll(id,0,0,drop,crate));

        [Test] public void LuckCrateCountAndCrateModifierApplyAtTheirSeparateSourceStages()
        {
            // Healer: base .03*2=.06; item .03*2/(1+2)*1.5=.03.
            Assert.AreEqual(ConsumableKind.Crate,ReferenceLoot.Roll("healer",100,2,.0599,.0299,50));
            Assert.AreEqual(ConsumableKind.Fruit,ReferenceLoot.Roll("healer",100,2,.0599,.0301,50));
            Assert.AreEqual(ConsumableKind.None,ReferenceLoot.Roll("healer",100,2,.0601,0,50));
        }
        [TestCase("butcher")][TestCase("croc")][TestCase("invoker")][TestCase("predator")]
        public void NormalRunBossClassOverridesNegativeLuckAndCrateModifiers(string id)
            =>Assert.AreEqual(ConsumableKind.LegendaryCrate,ReferenceLoot.Roll(id,-100,100,.99,.99,-100,null,20));

        [Test] public void LooterAlwaysDropsAnOrdinaryCrateWhileTreeStillNeedsItsLuckRoll()
        {
            Assert.AreEqual(ConsumableKind.Crate,ReferenceLoot.Roll("looter",-100,100,.99,.99,-100,null,20));
            Assert.IsTrue(ReferenceLoot.CanDropConsumables("tree"));
            Assert.AreEqual(ConsumableKind.None,ReferenceLoot.Roll("tree",-100,0,0,0));
        }
        [TestCase("tree",1,.75f)][TestCase("looter",1,.75f)][TestCase("evil_mob",1,1.25f)]
        [TestCase("baby_alien",1.5f,.125f)][TestCase("baby_alien",20,2f)]
        public void MaterialSpreadUsesSourceFloorCeilingAndUnroundedAmount(string id,float amount,float expected)
            =>Assert.AreEqual(expected,ReferenceLoot.MaterialSpread(id,amount));

        [TestCase("baby_alien",4,false,1f)]
        [TestCase("baby_alien",5,false,.925f)]
        [TestCase("baby_alien",20,false,.7f)]
        [TestCase("baby_alien",100,false,.5f)]
        [TestCase("baby_alien",4,true,.65f)]
        [TestCase("baby_alien",20,true,.455f)]
        [TestCase("baby_alien",100,true,.325f)]
        [TestCase("tree",20,true,1f)]
        [TestCase("evil_mob",20,true,1f)]
        [TestCase("looter",20,true,1f)]
        [TestCase("croc",20,true,1f)]
        public void SourceMaterialBatchGateUsesWaveHordeAndAlwaysDropFlags(string id,int wave,bool horde,float expected)
            =>Assert.That(ReferenceLoot.MaterialBatchChance(id,wave,horde),Is.EqualTo(expected).Within(.00001f));

        [Test] public void SourceChanceComparisonIncludesItsPositiveBoundaryButRejectsZero()
        {
            Assert.AreEqual(ConsumableKind.Crate,ReferenceLoot.Roll("healer",0,0,.03f,.03f));
            Assert.AreEqual(ConsumableKind.None,ReferenceLoot.Roll("tree",-100,0,0,0));
            Assert.AreEqual(ConsumableKind.LegendaryCrate,ReferenceLoot.Roll("croc",-100,100,1,1,-100,"Elite",20));
        }

        [Test] public void SourceScatterUsesTheExtendedSquareAndKeepsItWithinTheMap()
        {
            var random=UnityEngine.Random.state;ArenaGeometry.SetMapSize(0);
            try
            {
                UnityEngine.Random.InitState(1715);bool beyondOldSquare=false;
                for(int i=0;i<100;i++)
                {
                    var drop=ScrapMotion.CreateDrop(Vector2.zero,60,.75f);
                    if(Mathf.Abs(drop.PushDestination.x)>.5f||Mathf.Abs(drop.PushDestination.y)>.5f)beyondOldSquare=true;
                    Assert.That(Mathf.Abs(drop.PushDestination.x),Is.LessThanOrEqualTo(.875f));
                    Assert.That(Mathf.Abs(drop.PushDestination.y),Is.LessThanOrEqualTo(.875f));
                    var edge=ScrapMotion.CreateDrop(ArenaGeometry.HalfSize,60,2);
                    Assert.That(Mathf.Abs(edge.PushDestination.x),Is.LessThanOrEqualTo(ArenaGeometry.HalfSize.x));
                    Assert.That(Mathf.Abs(edge.PushDestination.y),Is.LessThanOrEqualTo(ArenaGeometry.HalfSize.y));
                }
                Assert.IsTrue(beyondOldSquare,"Tree/Looter spread must not remain confined to the former .5-unit half-width.");
            }
            finally{UnityEngine.Random.state=random;ArenaGeometry.SetMapSize(0);}
        }
        [TestCase("dire_junkie")][TestCase("slasher_egg")]
        public void ActualEnemyLootPathDoesNotApplyFruitFallbackToIneligibleCreatures(string id)
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("enemy_fruit_drops",100);
                fixture.Drop(id,"Chaser",0);
                Assert.AreEqual(0,fixture.pickups.Count,"A 100% fallback cannot bypass can_drop_consumables=false.");
            }
        }
        [Test] public void ActualEnemyLootPathAppliesItsHordeMaterialGate()
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("luck",-100);fixture.game.Rules.SetShowcaseWave(20);fixture.game.Rules.IsHordeWave=true;
                bool success=false,rejected=false;
                for(int i=0;i<100;i++)
                {
                    fixture.Drop("baby_alien","Chaser",1);
                    success|=fixture.MaterialCount==1;rejected|=fixture.MaterialCount==0;
                    Assert.That(fixture.MaterialCount,Is.InRange(0,1));fixture.Clear();
                }
                Assert.IsTrue(success);Assert.IsTrue(rejected,"The former unconditional material batch must not survive the source horde gate.");
            }
        }
        [Test] public void ActualNeutralLootBypassesTheGateAndUsesTreeSpread()
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("luck",-100);fixture.game.Rules.SetShowcaseWave(20);fixture.game.Rules.IsHordeWave=true;
                bool beyondOldSquare=false;
                for(int i=0;i<60;i++)
                {
                    fixture.Drop("tree","Tree",1);Assert.AreEqual(1,fixture.MaterialCount);
                    var motion=(ScrapMotion)fixture.pickups[0].GetType().GetField("motion").GetValue(fixture.pickups[0]);
                    var scatter=motion.PushDestination-motion.position;
                    beyondOldSquare|=Mathf.Abs(scatter.x)>.5f||Mathf.Abs(scatter.y)>.5f;fixture.Clear();
                }
                Assert.IsTrue(beyondOldSquare,"The real death path must forward the tree's 75px spread.");
            }
        }
        [Test] public void ConsumableDropRemainsIndependentWhenTheMaterialGateRejectsTheBatch()
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("luck",-100);fixture.game.Rules.GainWeaponStat("enemy_fruit_drops",100);
                fixture.game.Rules.SetShowcaseWave(20);fixture.game.Rules.IsHordeWave=true;bool found=false;
                for(int i=0;i<100;i++)
                {
                    fixture.Drop("healer","Chaser",1);
                    Assert.AreEqual(1,fixture.pickups.Count-fixture.MaterialCount,"Source consumables are resolved before the gold gate.");
                    found|=fixture.MaterialCount==0;fixture.Clear();
                }
                Assert.IsTrue(found);
            }
        }
        [Test] public void InstantGoldPrefersTheLastLiveLootwormInTheActualDeathPath()
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("luck",-100);fixture.game.Rules.GainWeaponStat("instant_gold_attracting",100);
                fixture.AddPet("lootworm");var chosen=fixture.AddPet("lootworm");fixture.AddPet("lootworm",true);fixture.AddPet("catling_gun");
                fixture.Drop("baby_alien","Chaser",1);Assert.AreEqual(1,fixture.MaterialCount);
                var pickup=fixture.pickups[0];Assert.AreSame(chosen,pickup.GetType().GetField("petCollector").GetValue(pickup));
                Assert.IsFalse(((ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup)).magnetized,"External owner drives the ordinary shared pickup motion.");
            }
        }
        [Test] public void InstantGoldFallsBackToThePlayerWhenNoLootwormExists()
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("luck",-100);fixture.game.Rules.GainWeaponStat("instant_gold_attracting",100);
                fixture.AddPet("catling_gun");fixture.Drop("baby_alien","Chaser",1);
                var pickup=fixture.pickups[0];Assert.IsNull(pickup.GetType().GetField("petCollector").GetValue(pickup));
                Assert.IsTrue(((ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup)).magnetized);
            }
        }
        [Test] public void OrdinaryDropsDoNotAcquireALootwormWithoutTheInstantEffect()
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("luck",-100);fixture.AddPet("lootworm");fixture.Drop("baby_alien","Chaser",1);
                var pickup=fixture.pickups[0];Assert.IsNull(pickup.GetType().GetField("petCollector").GetValue(pickup));
                Assert.IsFalse(((ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup)).magnetized);
            }
        }
        [TestCase(-1.85f,true)][TestCase(1.6f,false)]
        public void PlayerAttractionUsesTheSourceRootBelowItsBodyCenter(float y,bool expected)
        {
            using(var fixture=new DropFixture())
            {
                var pickup=fixture.ReadyMaterial(Vector2.up*y);fixture.TickPickups();
                Assert.AreEqual(expected,((ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup)).magnetized);
            }
        }
        [TestCase(-.45f,true)][TestCase(.55f,false)]
        public void PlayerContactIsCenteredOnTheBodyRatherThanAboveIt(float y,bool expected)
        {
            using(var fixture=new DropFixture())
            {
                var pickup=fixture.ReadyMaterial(Vector2.up*y);pickup.GetType().GetField("enemyCollector").SetValue(pickup,fixture.AddEnemyCollector(Vector2.zero));
                fixture.TickPickups();
                var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
                var entries=(System.Collections.Generic.HashSet<object>)typeof(ScrapMotion).GetField("contactOverlaps",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(motion);
                Assert.AreEqual(expected,entries.Contains(ScrapMotion.PlayerCollector));Assert.AreEqual(1,fixture.pickups.Count,"External ownership still blocks actual player consumption.");
            }
        }
        [TestCase("player")][TestCase("lootworm")][TestCase("evil_mob")]
        public void AttractionMovementTargetsEachCollectorsSourceRoot(string owner)
        {
            using(var fixture=new DropFixture())
            {
                var pickup=fixture.ReadyMaterial(new Vector2(1,-.24f));var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
                if(owner=="player")motion.magnetized=true;
                else
                {
                    fixture.SetPlayer(Vector2.right*10);
                    if(owner=="lootworm")pickup.GetType().GetField("petCollector").SetValue(pickup,fixture.AddPet("lootworm"));
                    else pickup.GetType().GetField("enemyCollector").SetValue(pickup,fixture.AddEnemyCollector(Vector2.zero));
                }
                fixture.TickPickups(1f/60);
                Assert.That(motion.position.x,Is.EqualTo(1-5f/60).Within(.00001f));Assert.That(motion.position.y,Is.EqualTo(-.24f).Within(.00001f));
            }
        }
        [Test] public void EvilMobContactUsesItsAdaptedOffsetRatherThanAnAboveBodyCircle()
        {
            using(var fixture=new DropFixture())
            {
                fixture.ReadyMaterial(Vector2.up*.4f);var enemy=fixture.AddEnemyCollector(Vector2.zero);fixture.SetPlayer(Vector2.right*10);
                typeof(ArenaGame).GetMethod("EatMaterials",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(fixture.game,new[]{enemy,(object)0f});
                Assert.AreEqual(1,fixture.pickups.Count);Assert.AreEqual(0,(int)enemy.GetType().GetField("gold").GetValue(enemy));
                Assert.That((CombatGeometry.OriginOffset("evil_mob")+Vector2.up*.1f).y,Is.EqualTo(-.14f).Within(.00001f));
                Assert.AreEqual(Vector2.down*.14f,SupportBehavior.LootwormPickupOffset);
            }
        }
        [Test] public void ActualEnemyLootStartsAtTheSourceOriginBelowTheArtCenter()
        {
            using(var fixture=new DropFixture())
            {
                fixture.game.Rules.GainWeaponStat("luck",-100);fixture.Drop("tree","Tree",1);
                var motion=(ScrapMotion)fixture.pickups[0].GetType().GetField("motion").GetValue(fixture.pickups[0]);
                Assert.AreEqual(CombatGeometry.OriginOffset("tree"),motion.position);
            }
        }
        [Test] public void ConsumableRuntimeForwardsItsSourceSpreadToTheDropMotion()
        {
            const BindingFlags hidden=BindingFlags.NonPublic|BindingFlags.Instance;
            var random=UnityEngine.Random.state;var game=new GameObject("Consumable spread test").AddComponent<ArenaGame>();
            var pickups=(IList)typeof(ArenaGame).GetField("pickups",hidden).GetValue(game);
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            try
            {
                UnityEngine.Random.InitState(1815);bool beyondOldSquare=false;
                for(int i=0;i<50;i++)
                {
                    typeof(ArenaGame).GetMethod("DropConsumable",hidden).Invoke(game,new object[]{Vector2.zero,ConsumableKind.Fruit,.75f});
                    var pickup=pickups[pickups.Count-1];var motion=(ScrapMotion)pickup.GetType().GetField("motion").GetValue(pickup);
                    beyondOldSquare|=Mathf.Abs(motion.PushDestination.x)>.5f||Mathf.Abs(motion.PushDestination.y)>.5f;
                }
                Assert.IsTrue(beyondOldSquare);
            }
            finally
            {
                foreach(object pickup in pickups)UnityEngine.Object.DestroyImmediate((GameObject)pickup.GetType().GetField("obj").GetValue(pickup));
                UnityEngine.Object.DestroyImmediate(game.gameObject);UnityEngine.Random.state=random;
            }
        }
    }
}
