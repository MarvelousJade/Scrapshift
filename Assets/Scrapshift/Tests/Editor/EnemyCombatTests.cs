using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class EnemyCombatTests
    {
        [Test] public void ChargeLocksDirectionAndReturnsToMovementWithoutInventedRecovery()
        {
            var attack = new EnemyAttack(EnemyBehavior.For("bruiser"),()=>0);
            Assert.IsFalse(attack.Step(.01f, Vector2.right, true));
            Assert.AreEqual(AttackPhase.Windup, attack.Phase);
            Assert.IsFalse(attack.Step(.39f, Vector2.up, true));
            Assert.IsTrue(attack.Step(.01f, Vector2.up, false));
            Assert.AreEqual(Vector2.right, attack.Direction);
            Assert.AreEqual(AttackPhase.Active, attack.Phase);
            Assert.IsFalse(attack.Step(.75f, Vector2.up, true));

            Assert.AreEqual(AttackPhase.Approach, attack.Phase);
        }

        [Test] public void ShooterWarnsBeforeSingleReleaseAndCannotChargeOutOfRange()
        {
            var attack = new EnemyAttack(EnemyBehavior.For("spitter"),()=>0);
            attack.Step(3, Vector2.right, false);
            Assert.AreEqual(AttackPhase.Approach, attack.Phase);
            attack.Step(.01f, Vector2.left, true);
            Assert.IsFalse(attack.Step(.79f, Vector2.right, true));
            Assert.IsFalse(attack.Step(0, Vector2.right, true));
            Assert.IsTrue(attack.Step(.01f, Vector2.right, true));
            Assert.AreEqual(Vector2.right, attack.Direction);
            Assert.IsFalse(attack.Step(.01f, Vector2.right, true));
        }

        [Test] public void AcceleratingGroupsUseInitialThenReducedIntervalsWithoutRoundingDrift()
        {
            var group = new SpawnGroup { start=1, interval=3, reduce=.5f, minimum=1, repeats=4, chance=1,
                units=new[]{new UnitEntry { id="x", min=1, max=1, chance=1 }} };
            var director = new SpawnDirector(new WaveEntry { groups=new[]{group} }, 1);
            var timings = new List<int>();
            for (int second=1; second<=12; second++)
                director.Tick(1, (g,u,n) => timings.Add(second));
            CollectionAssert.AreEqual(new[]{1,4,7,9,10}, timings);
        }

        [Test] public void EdgeSpawnsPreserveEdgeAndAdditionalPlayerClearance()
        {
            var group = new SpawnGroup { area=-1, edge=true, inset=.2f };
            var unit = new UnitEntry { distance=2 };
            var random = new System.Random(13);
            for(int i=0;i<250;i++)
            {
                Vector2 player = new Vector2(9, 3);
                var pos = SpawnPlacement.Position(group,unit,Vector2.zero,player,random);
                Assert.IsTrue(Mathf.Abs(Mathf.Abs(pos.x)-9.60f)<.001f || Mathf.Abs(Mathf.Abs(pos.y)-7.04f)<.001f);
                Assert.Greater(Vector2.Distance(pos,player),.25f);
            }
        }

        [Test] public void ClusterRetriesRetainSharedSquareAndNeverFallbackAcrossArena()
        {
            var group = new SpawnGroup { area=2 };
            var random = new System.Random(42);
            var anchor = SpawnPlacement.Anchor(group,Vector2.zero,random);
            for(int i=0;i<100;i++)
            {
                var pos=SpawnPlacement.Position(group,new UnitEntry(),anchor,Vector2.zero,random);
                Assert.LessOrEqual(Mathf.Abs(pos.x-anchor.x),1.001f);
                Assert.LessOrEqual(Mathf.Abs(pos.y-anchor.y),1.001f);
                Assert.Greater(pos.magnitude,.25f);
            }
            // Impossible safe distance progressively relaxes, preserving the actual region.
            var constrained=SpawnPlacement.Position(group,new UnitEntry { distance=100 },anchor,anchor,random);
            Assert.LessOrEqual(Mathf.Abs(constrained.x-anchor.x),1.001f);
        }

        [Test] public void TwentyWavesRemainPlayableWithExpandedItemShopPool()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            Assert.AreEqual(20,RunRules.WaveCount);
            Assert.AreEqual(127,catalog.data.items.Count(i=>RunRules.IsShopItem(i.id)));
            var run=new RunRules(5); run.Configure(catalog); run.Start();
            var offered=new HashSet<string>();
            for(int wave=1;wave<20;wave++)
            {
                run.Collect(10000);run.Tick(run.Remaining);
                while(run.Phase==RunPhase.Upgrade)run.Choose(0);
                for(int roll=0;roll<20;roll++)
                {
                    foreach(var offer in run.Shop) if(offer?.item!=null)
                    { Assert.IsTrue(RunRules.IsShopItem(offer.item.id)); offered.Add(offer.item.id); }
                    run.Reroll();
                }
                run.NextWave();
            }
            Assert.That(offered.Count,Is.GreaterThan(40),"Seeded multi-wave sampling should reach many eligible items across tiers.");
            run.Tick(run.Remaining);Assert.AreEqual(RunPhase.Won,run.Phase);
        }
    }
}
