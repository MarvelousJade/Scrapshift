using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class InspectionTests
    {
        ReferenceCatalog catalog;InspectionText text;RunRules rules;
        [SetUp] public void Setup(){catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");text=new InspectionText(catalog.uiText);rules=new RunRules(42);rules.Configure(catalog);rules.Start();}
        [Test] public void SelectionHidesLockedWeaponsAndKeepsCharacterSpecificOrder()
        {
            CollectionAssert.AreEqual(new[]{13,2,7,8},catalog.data.characters.Select(c=>StarterChoices.Available(catalog,c).Length));
            CollectionAssert.AreEqual(new[]{"fist","hand"},StarterChoices.Available(catalog,catalog.data.characters[1]));
            foreach(var c in catalog.data.characters)
            {
                var choices=StarterChoices.Available(catalog,c);Assert.IsFalse(choices.Any(id=>!catalog.Weapon(id,1).available));
                foreach(var id in choices){rules.Start(c,id);Assert.AreEqual(id,rules.Inventory[0].id);}
            }
        }
        [Test] public void SourceFontAndAllPrimaryStatTemplatesAreSerialized()
        {
            Assert.AreEqual("Anybody-Medium.ttf",System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(catalog.referenceFont)));
            foreach(var stat in StatPresentation.Keys)
            {
                Assert.IsFalse(text.StatName(stat).StartsWith("STAT_"));
                foreach(int value in new[]{-10,0,10})
                {rules.Start();rules.GainWeaponStat(stat,value);string description=text.StatDescription(rules,stat);Assert.IsFalse(description.Contains("INFO_"),stat);Assert.IsFalse(description.Contains("{"),stat);}
            }
        }
        [Test] public void DescriptionsUseCurrentArmorAndRegenerationValues()
        {
            rules.GainWeaponStat("armor",15);Assert.AreEqual("You take 50% less damage",text.StatDescription(rules,"armor"));
            Assert.AreEqual("You regenerate 0 HP every 5 seconds (0 HP / second)",text.StatDescription(rules,"hp_regeneration"));
            rules.GainWeaponStat("hp_regeneration",1);Assert.AreEqual("You regenerate 1 HP every 5 seconds (0.2 HP / second)",text.StatDescription(rules,"hp_regeneration"));
        }
        [Test] public void ItemEffectsKeepPercentageUnitsWithSerializedStatPrefixes()
        {
            Assert.AreEqual("+10% Attack Speed",text.StatEffect("stat_attack_speed",10));
            Assert.AreEqual("-2% Damage",text.StatEffect("stat_percent_damage",-2));
            Assert.AreEqual("+3 Armor",text.StatEffect("stat_armor",3));
        }
        [Test] public void EveryWeaponDescriptionResolvesTemplatesAndUsesReferenceUnits()
        {
            foreach(var w in catalog.data.equipment){string d=text.WeaponDescription(rules,w);Assert.IsFalse(d.Contains("{"),w.id);Assert.IsFalse(d.Contains("EFFECT_"),w.id);Assert.IsTrue(d.Contains(w.melee?"(Melee)":"(Ranged)"),w.id);}
            StringAssert.Contains("400",text.WeaponDescription(rules,catalog.Weapon("pistol",1)));
            Assert.AreEqual("Thief Dagger",text.WeaponName(catalog.Weapon("dagger",1)));
        }
    }
}
