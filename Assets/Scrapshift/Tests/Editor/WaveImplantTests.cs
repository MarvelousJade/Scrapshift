using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class WaveImplantTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(18);r.Configure(catalog);r.Start();return r;}
        void Shop(RunRules r){r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);}
        void Buy(RunRules r,string id){r.Shop[0]=new ShopOffer{item=catalog.data.items.Single(i=>i.id==id),rolledWave=r.Wave};Assert.IsTrue(r.Buy(0));}
        [Test] public void PeacockBonusIsNextWaveOnlyAndRebuyQueuesAnother()
        {
            var r=Run();r.Collect(2000);Shop(r);Buy(r,"peacock");Assert.AreEqual(25,r.Stat("xp_gain"));Assert.AreEqual(0,r.Stat("enemy_damage"));
            r.NextWave();Assert.AreEqual(125,r.Stat("xp_gain"));Assert.AreEqual(50,r.Stat("enemy_damage"));Shop(r);r.NextWave();Assert.AreEqual(25,r.Stat("xp_gain"));Assert.AreEqual(0,r.Stat("enemy_damage"));
            Shop(r);Buy(r,"peacock");r.NextWave();Assert.AreEqual(150,r.Stat("xp_gain"));Assert.AreEqual(50,r.Stat("enemy_damage"));
        }
        [Test] public void CeleryKeepsPermanentWaveGainsAfterTemporaryEnemyHealthExpires()
        {
            var r=Run();r.Collect(2000);Shop(r);Buy(r,"celery_tea");r.NextWave();Assert.AreEqual(50,r.Stat("xp_gain"));Assert.AreEqual(100,r.Stat("enemy_health"));
            Shop(r);Assert.AreEqual(5,r.Stat("xp_gain"));r.NextWave();Assert.AreEqual(0,r.Stat("enemy_health"));Shop(r);Assert.AreEqual(10,r.Stat("xp_gain"));
        }
        [Test] public void AnvilUpgradesAtNextShopAndGivesArmorWhenAllWeaponsMaxed()
        {
            var r=Run();r.Collect(2000);Shop(r);Buy(r,"anvil");Assert.AreEqual(1,r.Inventory[0].tier);r.NextWave();Shop(r);Assert.AreEqual(2,r.Inventory[0].tier);
            r.Inventory[0]=catalog.Weapon("pistol",4);float armor=r.Stat("armor");r.NextWave();Shop(r);Assert.AreEqual(armor+2,r.Stat("armor"));
        }
        [Test] public void BurningKillsCountOnlyBurningVictimsAndResetWaveCap()
        {
            var r=Run();r.Items.Add(catalog.data.items.Single(i=>i.id=="will_o_the_wisp"));
            for(int i=0;i<30;i++)r.KillEffects(false);Assert.AreEqual(0,r.Stat("elemental_damage"));
            for(int i=0;i<29;i++)r.KillEffects(true);Assert.AreEqual(0,r.Stat("elemental_damage"));r.KillEffects(true);Assert.AreEqual(1,r.Stat("elemental_damage"));
            for(int i=0;i<150;i++)r.KillEffects(true);Assert.AreEqual(4,r.Stat("elemental_damage"));Shop(r);r.NextWave();for(int i=0;i<30;i++)r.KillEffects(true);Assert.AreEqual(5,r.Stat("elemental_damage"));
        }
        [Test] public void RandomDamageTriggersAggregateBeforeRequestAndUseReferenceRounding()
        {
            var r=Run();r.GainWeaponStat("luck",11);r.GainWeaponStat("percent_damage",50);
            r.Items.Add(new ItemEntry{effects=new StatEffect[0],conditionalEffects=new[]{new ConditionalEffect{key="stat_luck",value=25,chance=1,customKey="dmg_when_pickup_gold"},new ConditionalEffect{key="stat_luck",value=25,chance=1,customKey="dmg_when_pickup_gold"}}});
            float received=0;int events=0;r.RandomDamageRequested+=d=>{received=d;events++;};r.Collect(1);Assert.AreEqual(6,received);Assert.AreEqual(1,events);
        }
    }
}
