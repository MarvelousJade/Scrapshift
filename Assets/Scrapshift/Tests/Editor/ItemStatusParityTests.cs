using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemStatusParityTests
    {
        ReferenceCatalog catalog;

        [SetUp]
        public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");

        RunRules Buy(string id)
        {
            var run=new RunRules(8);
            run.Configure(catalog);
            run.Start();
            run.Collect(1000);
            run.Tick(run.Remaining);
            while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            run.Shop[0]=new ShopOffer{item=catalog.data.items.Single(item=>item.id==id),rolledWave=run.Wave};
            Assert.IsTrue(run.Buy(0),id);
            return run;
        }

        [Test]
        public void EmptyShopGrantsOneBonusRerollBeforeItemGrantedFreeRerolls()
        {
            var run=new RunRules(8);run.Configure(catalog);run.Start();run.Collect(1000);
            run.Tick(run.Remaining);
            while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            for(int i=1;i<run.Shop.Length;i++)run.Shop[i]=null;
            run.Shop[0]=new ShopOffer{item=catalog.data.items.Single(item=>item.id=="dangerous_bunny"),rolledWave=run.Wave};
            Assert.IsTrue(run.Buy(0));
            Assert.AreEqual(0,run.RerollPrice);
            int before=run.Scrap;
            run.Reroll();Assert.AreEqual(before,run.Scrap);
            Assert.AreEqual(0,run.RerollPrice,"The item-granted reroll remains after the empty-shop bonus.");
            run.Reroll();Assert.AreEqual(before,run.Scrap);
            Assert.Greater(run.RerollPrice,0);
        }

        [Test]
        public void FourLockedShopOffersCannotBeRerolled()
        {
            var run=new RunRules(8);run.Configure(catalog);run.Start();run.Collect(1000);
            run.Tick(run.Remaining);
            while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            for(int i=0;i<run.Shop.Length;i++)run.Shop[i]=new ShopOffer{item=catalog.data.items.First(item=>item.id=="coffee"),locked=true,rolledWave=run.Wave};
            int before=run.Scrap;
            Assert.IsFalse(run.CanReroll);
            run.Reroll();Assert.AreEqual(before,run.Scrap);
            Assert.IsTrue(run.Shop.All(offer=>offer.locked));
        }

        [Test]
        public void AlienBabyHealthModifierAlsoAppliesToBossesAndElites()
        {
            var run=Buy("alien_baby");
            var baby=catalog.data.items.Single(item=>item.id=="alien_baby");
            Assert.AreEqual(10,baby.effects.Single(effect=>effect.key=="enemy_health").value);

            // EntityService.get_final_enemy_health applies this factor before Boss.init
            // performs its separate wave-specific health adjustment.
            Assert.AreEqual(1.1f,run.EnemyHealthFactor,.0001f);
            run.GainWeaponStat("enemy_health",-110);
            Assert.AreEqual(.01f,run.EnemyHealthFactor,.0001f);
        }

        [Test]
        public void SnailCanReduceEnemySpeedToZero()
        {
            var run=Buy("snail");
            var snail=catalog.data.items.Single(item=>item.id=="snail");
            Assert.AreEqual(-8,snail.effects.Single(effect=>effect.key=="enemy_speed").value);
            Assert.AreEqual(.92f,run.EnemySpeedFactor,.0001f);
            run.GainWeaponStat("enemy_speed",-92);
            Assert.AreEqual(0,run.EnemySpeedFactor,.0001f);
        }

        [Test]
        public void WeaponStatsTruncateScalingThenRoundPercentageDamage()
        {
            var run=new RunRules(8);
            run.Start();
            var weapon=new Equipment{damage=5,scaling=new[]{new StatEffect{key="ranged_damage",value=.5f}}};
            run.GainWeaponStat("ranged_damage",1);
            Assert.AreEqual(5,run.WeaponDamage(weapon)); // 5.5 truncates to 5.
            run.GainWeaponStat("percent_damage",10);
            Assert.AreEqual(6,run.WeaponDamage(weapon)); // 5 * 1.1 rounds to 6.
            run.GainWeaponStat("percent_damage",-210);
            Assert.AreEqual(1,run.WeaponDamage(weapon));
        }

        [Test]
        public void CharacterHealthGainIncludesBaseTenHpAndTruncatesTheResult()
        {
            var ranger=new RunRules(8);
            ranger.Configure(catalog);
            ranger.Start(catalog.data.characters.Single(profile=>profile.id=="ranger"),"pistol");
            Assert.AreEqual(7.5f,ranger.Stat("max_hp"),.0001f);
            Assert.AreEqual(7,ranger.MaxHealth);
            ranger.GainWeaponStat("max_hp",3);
            Assert.AreEqual(9,ranger.MaxHealth);

            var chunky=new RunRules(8);
            chunky.Configure(catalog);
            chunky.Start(catalog.data.characters.Single(profile=>profile.id=="chunky"),"fist");
            Assert.AreEqual(12.5f,chunky.Stat("max_hp"),.0001f);
            Assert.AreEqual(12,chunky.MaxHealth);
        }
        [Test]
        public void PlayerDamageRoundsAfterPositiveAndNegativeArmor()
        {
            var run=new RunRules(8);
            run.Start();
            run.GainWeaponStat("armor",15);
            run.Hurt(3); // 3 * 0.5 = 1.5, rounded to 2 HP.
            Assert.AreEqual(8,run.Health);
            run.GainWeaponStat("armor",-30);
            run.Hurt(3); // 3 * 1.5 = 4.5, rounded to 5 HP.
            Assert.AreEqual(3,run.Health);
        }
    }
}
