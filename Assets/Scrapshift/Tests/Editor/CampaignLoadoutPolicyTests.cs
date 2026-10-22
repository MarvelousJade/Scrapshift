using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class CampaignLoadoutPolicyTests
    {
        const BindingFlags Hidden=BindingFlags.Static|BindingFlags.NonPublic;
        static bool Allowed(ItemEntry item,bool enabled)=>(bool)typeof(ArenaGame).GetMethod("CampaignAllowsItem",Hidden).Invoke(null,new object[]{item,enabled});
        static bool WeaponAllowed(Equipment weapon,bool enabled)=>(bool)typeof(ArenaGame).GetMethod("CampaignAllowsWeapon",Hidden).Invoke(null,new object[]{weapon,enabled});
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        [TestCase(false,false,false)][TestCase(false,true,false)][TestCase(true,true,false)][TestCase(true,false,true)]
        public void PolicyRequiresExplicitOptInAndNeverChangesSurvival(bool requested,bool survival,bool expected)
            =>Assert.AreEqual(expected,typeof(ArenaGame).GetMethod("CampaignSafeLoadoutEnabled",Hidden).Invoke(null,new object[]{requested,survival}));
        [Test] public void ImportedBloodDonationIsExcludedOnlyFromTheOptInPolicy()
        {
            var item=catalog.data.items.Single(i=>i.id=="blood_donation");
            Assert.IsTrue(Allowed(item,false));Assert.IsFalse(Allowed(item,true));
            Assert.IsTrue(Allowed(catalog.data.items.Single(i=>i.id=="coffee"),true));
        }
        [TestCase("lose_hp_per_second",1,false)][TestCase("stat_lose_hp_per_second",2,false)]
        [TestCase("lose_hp_per_second",0,true)][TestCase("lose_hp_per_second",-1,true)][TestCase("percent_damage",-20,true)]
        public void PolicyUsesPositiveEffectSemanticsRatherThanAnItemId(string key,float value,bool expected)
        {
            var item=new ItemEntry{id="synthetic-policy-carrier",effects=new[]{new StatEffect{key=key,value=value}}};
            Assert.AreEqual(expected,Allowed(item,true));Assert.IsTrue(Allowed(item,false));
        }
        [Test] public void ConditionalAndWeaponEffectCarriersAreRecognizedWithoutMutation()
        {
            var effect=new ConditionalEffect{key="lose_hp_per_second",value=1,customKey="temp_stats_per_interval"};
            var item=new ItemEntry{conditionalEffects=new[]{effect}};
            var trait=new WeaponTrait{key="lose_hp_per_second",value=1};var weapon=new Equipment{traits=new[]{trait}};
            Assert.IsFalse(Allowed(item,true));Assert.IsFalse(WeaponAllowed(weapon,true));Assert.IsTrue(WeaponAllowed(weapon,false));
            Assert.AreSame(effect,item.conditionalEffects[0]);Assert.AreEqual(1,effect.value);
            Assert.AreSame(trait,weapon.traits[0]);Assert.AreEqual(1,trait.value);
            Assert.IsTrue(Allowed(null,true));Assert.IsTrue(WeaponAllowed(null,true));Assert.IsTrue(Allowed(new ItemEntry(),true));
        }
        [Test] public void OrdinaryBloodDonationStillAppliesItsModifierAndPeriodicDamage()
        {
            var run=new RunRules(771);run.Configure(catalog);run.Start();
            run.GrantItem(catalog.data.items.Single(i=>i.id=="blood_donation"));
            Assert.AreEqual(1,run.Stat("lose_hp_per_second"));
            float health=run.Health,harvesting=run.Stat("harvesting");
            Assert.Greater(harvesting,0);run.Tick(1);
            Assert.AreEqual(health-1,run.Health);Assert.AreEqual(harvesting,run.Stat("harvesting"));
        }
    }
}
