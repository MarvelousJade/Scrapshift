using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class CharacterProfileTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Start(int character,string weapon){var r=new RunRules(42);r.Configure(catalog);r.Start(catalog.data.characters[character],weapon);return r;}
        [Test] public void FourProfilesKeepReferenceOrderAndCompleteStartingLists()
        {
            CollectionAssert.AreEqual(new[]{"well_rounded","brawler","crazy","ranger"},catalog.data.characters.Select(c=>c.id));
            CollectionAssert.AreEqual(new[]{18,3,8,9},catalog.data.characters.Select(c=>c.startingWeapons.Length));
            Assert.AreEqual(50,catalog.data.rosterSlots);
            foreach(var c in catalog.data.characters)foreach(var id in c.startingWeapons)
                for(int tier=1;tier<=4;tier++){var w=catalog.Weapon(id,tier);Assert.NotNull(w,id);Assert.NotNull(w.image,id);Assert.IsNotEmpty(w.sounds,id);}
        }
        [Test] public void EveryUnlockedChoiceStartsWithSelectedWeaponAndReferenceBonus()
        {
            foreach(var c in catalog.data.characters)foreach(var id in c.startingWeapons)
            {
                if(!catalog.Weapon(id,1).available)continue;
                var r=new RunRules(42);r.Configure(catalog);r.Start(c,id);
                Assert.AreEqual(id,r.Inventory[0].id);
                Assert.AreEqual(string.IsNullOrEmpty(c.bonusWeapon)?1:2,r.Inventory.Count,c.id);
                if(r.Inventory.Count==2)Assert.AreEqual(c.bonusWeapon,r.Inventory[1].id);
                Assert.AreEqual(r.MaxHealth,r.Health);
            }
        }
        [Test] public void RookieHasReferenceStartingStats()
        {
            var r=Start(0,"knife");Assert.AreEqual(15,r.MaxHealth);Assert.AreEqual(8,r.Stat("harvesting"));Assert.AreEqual(4.725f,r.MoveSpeed,.001);
        }
        [Test] public void PugilistAttackSpeedAppliesOnlyToUnarmedWeapons()
        {
            var r=Start(1,"hand");Assert.AreEqual(1.5f,r.WeaponFireRate(catalog.Weapon("fist",1)));
            Assert.AreEqual(1,r.WeaponFireRate(catalog.Weapon("knife",1)));Assert.AreEqual(-50,r.Stat("ranged_damage"));
            Assert.AreEqual(-50,r.Stat("range"));Assert.AreEqual(18,r.Stat("dodge"));Assert.AreEqual(3,r.Stat("harvesting"));
        }
        [Test] public void HotheadRangeBonusAppliesOnlyToPreciseWeapons()
        {
            var r=Start(2,"knife");Assert.AreEqual(2.5f,r.WeaponRange(catalog.Weapon("knife",1)));
            Assert.AreEqual(4,r.WeaponRange(catalog.Weapon("pistol",1)));Assert.AreEqual(1.25f,r.FireRate);
            Assert.AreEqual(-30,r.Stat("dodge"));Assert.AreEqual(-10,r.Stat("engineering"));Assert.AreEqual(-10,r.Stat("ranged_damage"));
        }
        [Test] public void SharpshotModifiesGainsAndRejectsMeleePurchases()
        {
            var r=Start(3,"pistol");Assert.AreEqual(60,r.Stat("range"));Assert.AreEqual(10,r.MaxHealth);
            r.GainWeaponStat("ranged_damage",2);r.GainWeaponStat("max_hp",4);
            Assert.AreEqual(3,r.Stat("ranged_damage"));Assert.AreEqual(13,r.MaxHealth);
            r.Collect(500);r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);
            Assert.IsTrue(r.Shop.Where(s=>s?.weapon!=null).All(s=>!s.weapon.melee));
            r.Shop[0]=new ShopOffer{weapon=catalog.Weapon("knife",1)};Assert.IsFalse(r.CanBuy(0));
            r.Start();Assert.IsNull(r.Character);Assert.AreEqual(10,r.MaxHealth);Assert.AreEqual(0,r.Stat("ranged_damage"));
        }
        [Test] public void SharpToothUsesCurrentMaxHealthIncludingPrimitiveSet()
        {
            var r=Start(2,"sharp_tooth");r.Inventory.Add(catalog.Weapon("stick",1));r.Inventory.Add(catalog.Weapon("stick",1));
            r.Heal(100);Assert.AreEqual(0,r.Stat("lifesteal"));
            r.Hurt(r.MaxHealth*.51f);Assert.AreEqual(2,r.Stat("lifesteal"));
        }
        [Test] public void WeaponScalingAndStackingUseImportedCoefficients()
        {
            var r=Start(0,"stick");Assert.AreEqual(8,r.WeaponDamage(catalog.Weapon("stick",1)));
            r.Inventory.Add(catalog.Weapon("stick",1));Assert.AreEqual(12,r.WeaponDamage(catalog.Weapon("stick",1)));
            r.GainWeaponStat("ranged_damage",2);Assert.AreEqual(4,r.WeaponDamage(catalog.Weapon("smg",1)));
            Assert.AreEqual(48,r.WeaponDamage(catalog.Weapon("laser_gun",1)));
        }
    }
}
