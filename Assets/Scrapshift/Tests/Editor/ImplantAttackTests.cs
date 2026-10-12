using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class ImplantAttackTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(18);r.Configure(catalog);r.Start();return r;}
        void Add(RunRules r,string id)=>r.Items.Add(catalog.data.items.Single(i=>i.id==id));
        [Test] public void RadialOpticsScaleWithHealthButIgnoreAddedWeaponScaling()
        {
            var r=Run();Add(r,"alien_eyes");Add(r,"nail");r.GainWeaponStat("engineering",100);r.GainWeaponStat("percent_damage",50);
            var effect=r.Effects("alien_eyes").Single();Assert.AreEqual(6,effect.value);Assert.AreEqual(3,effect.interval);
            Assert.AreEqual((1+r.MaxHealth*.5f)*1.5f,r.WeaponDamage(effect.projectile.AsEquipment()),.001);
        }
        [Test] public void ExplosionAndOrdinaryDamagePercentagesAddBeforeApplyingTheMinimum()
        {
            var r=Run();Add(r,"rip_and_tear");r.GainWeaponStat("percent_damage",50);r.GainWeaponStat("explosion_damage",100);
            var stats=r.Effects("explode_on_death").Single().projectile.AsEquipment();
            Assert.AreEqual(25,r.WeaponDamage(stats,r.Stat("explosion_damage")),.001);
            r.GainWeaponStat("percent_damage",-200);Assert.AreEqual(5,r.WeaponDamage(stats,r.Stat("explosion_damage")),.001);
        }
        [Test] public void DodgeRetaliationStacksAndClearsOnTheNextOrdinaryHit()
        {
            var r=Run();Add(r,"riposte");Add(r,"riposte");r.GainWeaponStat("melee_damage",10);r.GainWeaponStat("dodge",100);r.GainWeaponStat("dodge_cap",100);
            r.Hurt(1);Assert.IsTrue(r.LastHitDodged);Assert.AreEqual(60,r.DodgeRetaliation);Assert.AreEqual(r.MaxHealth,r.Health);
            r.GainWeaponStat("dodge",-100);r.Hurt(1);Assert.IsFalse(r.LastHitDodged);Assert.AreEqual(0,r.DodgeRetaliation);
        }
        [Test] public void SpecialProjectilesDoNotAcquireElementalScalingFromFrozenHeart()
        {
            var r=Run();Add(r,"frozen_heart");Add(r,"baby_with_a_beard");
            Assert.IsTrue(r.WeaponScalesWith(catalog.Weapon("pistol",1),"elemental_damage"));
            Assert.IsFalse(r.WeaponScalesWith(r.Effects("projectiles_on_death").Single().projectile.AsEquipment(),"elemental_damage"));
        }
    }
}
