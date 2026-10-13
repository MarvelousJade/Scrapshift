using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class SupportParityTests
    {
        [Test] public void ManualPressModeWorksWithoutAlwaysOnAndReleasesToAuto()
        {
            Assert.IsTrue(CombatInput.ManualAim(false,true,true));
            Assert.IsFalse(CombatInput.ManualAim(true,true,false));
            Assert.IsTrue(CombatInput.ManualAim(true,false,false));
            Assert.IsFalse(CombatInput.ManualAim(false,false,true));
        }
        [Test] public void AllSupportActorsHaveSeparateNonemptyTransparentSprites()
        {
            var sprites=new[]{"turret","flame","rocket","laser","healing","landmine","garden","tyler","wandering_bot"}.Select(SupportArt.Structure)
                .Concat(new[]{"bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla","doc_moth","lootworm","scapegoat","jellyshield"}.Select(SupportArt.Pet)).ToArray();
            Assert.AreEqual(18,sprites.Distinct().Count());
            foreach(var sprite in sprites){Assert.Greater(sprite.rect.width,40);Assert.Greater(sprite.rect.height,40);Assert.AreEqual("SupportAtlas",sprite.texture.name);}
        }
        [Test] public void WaveSixPricesAndRerollModifierMatchReference()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var r=new RunRules(42);r.Configure(catalog);r.Start();r.Collect(1000);
            for(int wave=1;wave<=6;wave++)
            {
                r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);
                if(wave<6)r.NextWave();
            }
            Assert.AreEqual(6,r.Wave);Assert.AreEqual(38,r.Price(20));
            foreach(int cost in new[]{6,8,10}){Assert.AreEqual(cost,r.RerollPrice);int before=r.Scrap;r.Reroll();Assert.AreEqual(before-cost,r.Scrap);}
            r.GainWeaponStat("reroll_price",50);Assert.AreEqual(18,r.RerollPrice);
            r.GainWeaponStat("reroll_price",-250);Assert.AreEqual(2,r.RerollPrice);
        }
        [Test] public void UniqueWeaponShopRejectsDowngradesAndFinalTierDuplicates()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var rules=new RunRules(42);rules.Configure(catalog);rules.Start(catalog.data.characters.First(c=>c.id=="well_rounded"),"knife");
            rules.GainWeaponStat("no_duplicate_weapons",1);rules.Inventory.Clear();rules.Inventory.Add(catalog.Weapon("knife",3));
            Assert.IsFalse(rules.CanOfferWeapon(catalog.Weapon("knife",2)));
            Assert.IsTrue(rules.CanOfferWeapon(catalog.Weapon("knife",3)));
            Assert.IsTrue(rules.CanOfferWeapon(catalog.Weapon("knife",4)));
            rules.Inventory.Clear();rules.Inventory.Add(catalog.Weapon("knife",4));
            Assert.IsFalse(rules.CanOfferWeapon(catalog.Weapon("knife",4)));
        }
    }
}
