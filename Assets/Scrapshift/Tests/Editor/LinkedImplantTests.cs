using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class LinkedImplantTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(4);r.Configure(catalog);r.Start();return r;}
        void Link(RunRules r,string id)=>r.Items.Add(catalog.data.items.Single(i=>i.id==id));
        void Buy(RunRules r,string id)
        {
            r.Collect(1000);r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);
            r.Shop[0]=new ShopOffer{item=catalog.data.items.Single(i=>i.id==id),rolledWave=r.Wave};Assert.IsTrue(r.Buy(0));
        }
        [Test] public void PaddingTracksMaterialsAndTruncatesAfterScaling()
        {
            var r=Run();Link(r,"padding");float hp=r.MaxHealth;r.Collect(79);Assert.AreEqual(hp+3,r.MaxHealth); // Three normal level-up HP.
            float before=r.MaxHealth;r.Collect(1);Assert.AreEqual(before+1,r.MaxHealth);
        }
        [Test] public void NegativeSpeedRegenUsesSignedDivisor()
        {var r=Run();Link(r,"estys_couch");r.GainWeaponStat("speed",-15);Assert.AreEqual(30,r.Stat("hp_regeneration"));r.GainWeaponStat("speed",20);Assert.AreEqual(-10,r.Stat("hp_regeneration"));}
        [Test] public void CommunityAndBurningCountsChangeLinkedStats()
        {var r=Run();Link(r,"community_support");Link(r,"fried_rice");r.LivingEnemies=12;r.BurningEnemies=5;Assert.AreEqual(12,r.Stat("attack_speed"));Assert.AreEqual(5,r.Stat("hp_regeneration"));r.LivingEnemies=3;r.BurningEnemies=0;Assert.AreEqual(3,r.Stat("attack_speed"));Assert.AreEqual(0,r.Stat("hp_regeneration"));}
        [Test] public void FairyCountsDistinctItemIdsInsteadOfStacks()
        {
            var r=Run();Link(r,"fairy");float before=r.Stat("hp_regeneration");Link(r,"coffee");Link(r,"coffee");Assert.AreEqual(before+1,r.Stat("hp_regeneration"));
            Link(r,"robot_arm");Assert.AreEqual(before-2,r.Stat("hp_regeneration"));
        }
        [Test] public void CapFreezesAtPurchaseButDoesNotDeleteLaterGains()
        {
            var r=Run();Buy(r,"handcuffs");float cap=r.MaxHealth;r.GainWeaponStat("max_hp",100);Assert.AreEqual(cap,r.MaxHealth);Assert.Greater(r.Stat("max_hp"),cap);
            r.Start();Assert.AreEqual(10,r.MaxHealth);
        }
        [Test] public void SpeedCapAffectsMovementWithoutErasingDisplayedStats()
        {
            var r=Run();Buy(r,"shackles");float speed=r.MoveSpeed;r.GainWeaponStat("speed",100);Assert.AreEqual(speed,r.MoveSpeed);Assert.GreaterOrEqual(r.Stat("speed"),100);
        }
        [Test] public void LinksDoNotRecursivelyUseOtherLinkedBonuses()
        {
            var r=Run();Link(r,"power_generator");Link(r,"retromations_hoodie");r.GainWeaponStat("speed",12);r.GainWeaponStat("dodge",7);
            Assert.AreEqual(12,r.Stat("percent_damage"));Assert.AreEqual(14,r.Stat("attack_speed"));
        }
    }
}
