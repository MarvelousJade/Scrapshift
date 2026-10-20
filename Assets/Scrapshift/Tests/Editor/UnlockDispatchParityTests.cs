using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    // Producer/dispatch + persistence evidence, not 115 native playthroughs.
    public sealed class UnlockDispatchParityTests
    {
        static readonly string[] ImportedIds={
            "unlock_difficulty_1",
            "unlock_difficulty_2",
            "unlock_difficulty_3",
            "unlock_difficulty_4",
            "unlock_difficulty_5",
            "unlock_difficulty_6",
            "chal_difficulty_0",
            "chal_difficulty_1",
            "chal_difficulty_2",
            "chal_difficulty_3",
            "chal_difficulty_4",
            "chal_difficulty_5",
            "chal_difficulty_6",
            "chal_survivor_1",
            "chal_survivor_2",
            "chal_survivor_3",
            "chal_survivor_4",
            "chal_survivor_5",
            "chal_gatherer_1",
            "chal_gatherer_2",
            "chal_gatherer_3",
            "chal_gatherer_4",
            "chal_gatherer_5",
            "chal_rookie",
            "chal_dying",
            "chal_agriculture",
            "chal_hallucination",
            "chal_fast",
            "chal_hoarder",
            "chal_turrets",
            "chal_lumberjack",
            "chal_medicine",
            "chal_perfect_vision",
            "chal_fireworks",
            "chal_recycling",
            "chal_slow",
            "chal_hungry",
            "chal_advanced_technology",
            "chal_giant_slayer",
            "chal_robust",
            "chal_baited",
            "chal_forest",
            "chal_bourgeoisie",
            "chal_student",
            "chal_reckless",
            "chal_scavenger",
            "chal_fast_learner",
            "chal_experimentation",
            "chal_magic_and_machinery",
            "chal_blood_drinker",
            "chal_well_rounded",
            "chal_brawler",
            "chal_crazy",
            "chal_ranger",
            "chal_mage",
            "chal_chunky",
            "chal_old",
            "chal_lucky",
            "chal_mutant",
            "chal_generalist",
            "chal_loud",
            "chal_multitasker",
            "chal_wildling",
            "chal_pacifist",
            "chal_gladiator",
            "chal_saver",
            "chal_sick",
            "chal_farmer",
            "chal_ghost",
            "chal_speedy",
            "chal_entrepreneur",
            "chal_engineer",
            "chal_explorer",
            "chal_doctor",
            "chal_hunter",
            "chal_artificer",
            "chal_arms_dealer",
            "chal_streamer",
            "chal_cyborg",
            "chal_glutton",
            "chal_jack",
            "chal_lich",
            "chal_apprentice",
            "chal_cryptid",
            "chal_fisherman",
            "chal_golem",
            "chal_king",
            "chal_renegade",
            "chal_one_arm",
            "chal_bull",
            "chal_soldier",
            "chal_masochist",
            "chal_knight",
            "chal_demon",
            "chal_baby",
            "chal_vagabond",
            "chal_technomage",
            "chal_vampire",
            "chal_evil_hat",
            "chal_hourglass",
            "chal_candy_bag",
            "chal_fruit_basket",
            "chal_will_o_the_wisp",
            "chal_ghost_outfit",
            "chal_vorpal_sword",
            "chal_bonk_dog",
            "chal_catling_gun",
            "chal_bot_o_mine",
            "chal_blazemander",
            "chal_doc_moth",
            "chal_lootworm",
            "chal_jellyshield",
            "chal_beast_master",
            "chal_paws_n_claws",
            "chal_wounded"
        };
        public static IEnumerable<TestCaseData> Cases=>ImportedIds.Select(id=>new TestCaseData(id).SetName("UnlockDispatch_"+id));
        const BindingFlags Fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        ReferenceCatalog catalog;
        UnlockDefinition[] definitions;
        Progression progress;
        string directory,save;
        [SetUp] public void Setup()
        {
            catalog=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset"));
            definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
            directory=Path.Combine(Path.GetTempPath(),"ScrapshiftUnlockDispatch",Guid.NewGuid().ToString("N"));save=Path.Combine(directory,"progress.json");
            progress=new Progression(definitions,save);
        }
        [TearDown] public void Cleanup()
        {
            var source=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");RunRules.ConfigureShopItems(source.data.items);
            UnityEngine.Object.DestroyImmediate(catalog);
            foreach(string name in new[]{"progress.json","progress.json.tmp","progress.json.bak"}){string file=Path.Combine(directory,name);if(File.Exists(file))File.Delete(file);}
            if(Directory.Exists(directory))Directory.Delete(directory,false);
        }
        RunRules Run(){var run=new RunRules(771){Progress=progress};run.Configure(catalog);run.Start();return run;}
        ItemEntry Item(string id)=>catalog.data.items.Single(i=>i.id==id);
        void Shop(RunRules run){run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);Assert.AreEqual(RunPhase.Shop,run.Phase);}
        void Below(string id)=>Assert.IsFalse(progress.Completed(id),"A nonqualifying producer event must not unlock "+id);
        [Test] public void CaseInventoryEqualsEveryImportedDefinition()
        {CollectionAssert.AreEquivalent(definitions.Select(d=>d.id),ImportedIds);Assert.AreEqual(115,ImportedIds.Distinct().Count());}
        [TestCaseSource(nameof(Cases))]
        public void EveryImportedProducerDispatchesPersistsAndEnablesItsNextRunReward(string id)
        {
            var definition=definitions.Single(d=>d.id==id);int notifications=0;
            progress.ChallengeCompleted+=d=>{if(d.id==id)notifications++;};
            Produce(definition);
            Assert.IsTrue(progress.Completed(id),"Producer failed to dispatch "+id);Assert.IsTrue(progress.Unlocked(definition.reward));Assert.AreEqual(1,notifications);
            progress.Save();var reload=new Progression(definitions,save);Assert.IsTrue(reload.Completed(id));Assert.IsTrue(reload.Unlocked(definition.reward));
            int replayNotifications=0;reload.ChallengeCompleted+=d=>{if(d.id==id)replayNotifications++;};
            reload.CheckCountedChallenges();Assert.AreEqual(0,replayNotifications,"Reload cannot redispatch this completed reward.");
            var pool=new RunUnlockPool();pool.BeginRun(catalog,reload);
            string reward=definition.reward;
            if(reward.StartsWith("character_"))Assert.IsTrue(catalog.data.characters.Single(c=>c.id==reward.Substring(10)).available);
            else if(reward.StartsWith("item_"))Assert.IsTrue(catalog.data.items.Single(i=>i.id==reward.Substring(5)).available);
            else if(reward.StartsWith("weapon_")){var weapons=catalog.data.equipment.Where(w=>w.id==reward.Substring(7)).ToArray();Assert.IsNotEmpty(weapons);Assert.IsTrue(weapons.All(w=>w.available));}
            else if(reward.StartsWith("difficulty_"))Assert.GreaterOrEqual(reload.Difficulty,(int)definition.value);
            else Assert.Fail("Unmapped reward kind: "+reward);
        }
        void Produce(UnlockDefinition d)
        {
            if(d.description=="CHAL_CHARACTER_DESC")
            {string character=d.id.Substring(5);progress.Finish(false,character,0,false,false);Below(d.id);progress.Finish(true,character,0,false);return;}
            if(d.description=="CHAL_STAT_DESC")
            {
                var run=Run();float before=d.value+(d.value<0?1:-1);
                run.GainWeaponStat(d.stat,before-run.Stat(d.stat));progress.CheckStats(run);Below(d.id);
                run.GainWeaponStat(d.stat,d.value-run.Stat(d.stat));progress.CheckStats(run);return;
            }
            if(d.id.StartsWith("chal_difficulty_"))
            {progress.Finish(false,"well_rounded",(int)d.value,false,false);Below(d.id);progress.Finish(true,"well_rounded",(int)d.value,false);return;}
            if(d.id.StartsWith("unlock_difficulty_"))
            {
                if(d.value<6){Below(d.id);progress.Finish(true,"well_rounded",(int)d.value-1,false);}
                else{for(int i=0;i<9;i++)progress.Finish(true,catalog.data.characters[i].id,5,false);Below(d.id);progress.Finish(true,catalog.data.characters[9].id,5,false);}
                return;
            }
            if(d.description=="CHAL_SURVIVOR_DESC"||d.description=="CHAL_GATHERER_DESC")
            {
                string key=d.description=="CHAL_SURVIVOR_DESC"?"kills":"materials";var run=Run();
                progress.Count(key,(int)d.value-1);run.RecordWaveEnd(0,0);Below(d.id);
                progress.Count(key);Below(d.id);run.RecordWaveEnd(0,0);return;
            }
            Specific(d);
        }
        void Specific(UnlockDefinition d)
        {
            string id=d.id;var run=Run();int threshold=(int)d.value;
            switch(id)
            {
                case "chal_rookie":progress.Finish(false,"well_rounded",0,false,false);Below(id);progress.Finish(false,"well_rounded",0,false,true);break;
                case "chal_hoarder":run.Collect(threshold-1);Below(id);run.Collect(1);break;
                case "chal_turrets":TurretProducer(id);break;
                case "chal_lumberjack":case "chal_evil_hat":
                    for(int i=0;i<threshold-1;i++)run.RecordEnemy(id=="chal_lumberjack"?"tree":"evil_mob",id=="chal_lumberjack",1,false);
                    run.RecordWaveEnd(0,0);Below(id);run.RecordEnemy(id=="chal_lumberjack"?"tree":"evil_mob",id=="chal_lumberjack",1,false);run.RecordWaveEnd(0,0);break;
                case "chal_medicine":
                    run.GainWeaponStat("max_hp",500);float health=run.Health;typeof(RunRules).GetProperty("Health").SetValue(run,1f);
                    run.Heal(threshold-1);Below(id);run.Heal(1);break;
                case "chal_fireworks":run.RecordExplosionKills(threshold-1);Below(id);run.RecordExplosionKills(threshold);break;
                case "chal_recycling":
                    Shop(run);for(int i=0;i<threshold;i++){if(run.Inventory.Count==0)run.Inventory.Add(catalog.Weapon("pistol",1));run.Recycle(0);if(i<threshold-1)Below(id);}break;
                case "chal_hungry":case "chal_fruit_basket":
                    for(int i=0;i<threshold;i++){run.CollectConsumable(ConsumableKind.Fruit);if(i<threshold-1)Below(id);}break;
                case "chal_advanced_technology":case "chal_magic_and_machinery":
                    run.GainWeaponStat(d.stat,d.value-run.Stat(d.stat));run.GrantItem(Item("turret"));run.GrantItem(Item("turret"));Below(id);run.GrantItem(Item("turret"));break;
                case "chal_giant_slayer":run.RecordEnemy("croc",false,d.value+.001f,true);Below(id);run.RecordEnemy("croc",false,d.value,true);break;
                case "chal_baited":for(int i=0;i<threshold;i++){run.GrantItem(Item("bait"));if(i<threshold-1)Below(id);}break;
                case "chal_forest":run.RecordWaveEnd(threshold-1,0);Below(id);run.RecordWaveEnd(threshold,0);break;
                case "chal_bourgeoisie":
                    run.Inventory.Clear();for(int i=0;i<threshold;i++){run.Inventory.Add(catalog.Weapon("pistol",4));progress.CheckStats(run);if(i<threshold-1)Below(id);}break;
                case "chal_student":while(run.Level<threshold-1)run.Collect(1);Below(id);while(run.Level<threshold)run.Collect(1);break;
                case "chal_reckless":typeof(RunRules).GetProperty("Health").SetValue(run,2f);run.RecordWaveEnd(0,0);Below(id);typeof(RunRules).GetProperty("Health").SetValue(run,1f);run.RecordWaveEnd(0,0);break;
                case "chal_scavenger":
                    var common=catalog.data.items.Where(i=>i.tier==1&&i.available).Take(threshold).ToArray();Assert.AreEqual(threshold,common.Length);
                    for(int i=0;i<common.Length;i++){run.GrantItem(common[i]);if(i<threshold-1)Below(id);}break;
                case "chal_fast_learner":run.SetShowcaseWave(6);while(run.Level<threshold)run.Collect(1);Below(id);run.SetShowcaseWave(5);progress.CheckStats(run);break;
                case "chal_experimentation":
                    run.Inventory.Clear();foreach(string weapon in new[]{"pistol","smg","laser_gun","knife","spear"})run.Inventory.Add(catalog.Weapon(weapon,1));
                    progress.CheckStats(run);Below(id);run.Inventory.Add(catalog.Weapon("wand",1));progress.CheckStats(run);break;
                case "chal_hourglass":ReturnProducer(5);Below(id);ReturnProducer(4.99f);break;
                case "chal_candy_bag":
                    Shop(run);Assert.IsNotNull(run.Shop[0]);run.ToggleLock(0);run.NextWave();Assert.IsTrue(run.UsedLocks);
                    progress.Finish(true,"well_rounded",0,run.UsedLocks);Below(id);run.Start();progress.Finish(true,"well_rounded",0,run.UsedLocks);break;
                case "chal_will_o_the_wisp":for(int i=0;i<threshold;i++){run.RecordBurnKill();if(i<threshold-1)Below(id);}break;
                case "chal_bonk_dog":case "chal_catling_gun":case "chal_bot_o_mine":case "chal_blazemander":
                    string key=id=="chal_bonk_dog"?"melee_damage":id=="chal_catling_gun"?"ranged_damage":id=="chal_bot_o_mine"?"engineering":"elemental_damage";
                    ScalingWeapons(run,key,5);progress.CheckStats(run);Below(id);ScalingWeapons(run,key,6);progress.CheckStats(run);break;
                case "chal_doc_moth":run.GainWeaponStat("hp_regeneration",10);run.GainWeaponStat("lifesteal",9);progress.CheckStats(run);Below(id);run.GainWeaponStat("lifesteal",1);progress.CheckStats(run);break;
                case "chal_lootworm":run.RecordWaveEnd(0,threshold-1);Below(id);run.RecordWaveEnd(0,threshold);break;
                case "chal_jellyshield":for(int i=0;i<threshold;i++){run.RecordProjectileHit();if(i<threshold-1)Below(id);}break;
                case "chal_paws_n_claws":
                    foreach(string stat in new[]{"melee_damage","ranged_damage","engineering","elemental_damage"}){ScalingWeapons(run,stat,6);progress.CheckStats(run);}
                    Below(id);run.RecordWaveEnd(0,20);break;
                default:Assert.Fail("Imported definition has no producer scenario: "+id);break;
            }
        }
        void ScalingWeapons(RunRules run,string stat,int count)
        {
            var weapon=catalog.data.equipment.First(w=>w.tier==1&&w.scaling!=null&&w.scaling.Any(s=>s.key==stat));
            run.Inventory.Clear();for(int i=0;i<count;i++)run.Inventory.Add(weapon);
        }
        static void Field(ArenaGame game,string name,object value)=>typeof(ArenaGame).GetField(name,Fields).SetValue(game,value);
        static object Call(ArenaGame game,string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Fields).Invoke(game,args);
        void TurretProducer(string id)
        {
            var host=new GameObject("Unlock turret producer");var game=host.AddComponent<ArenaGame>();game.enabled=false;
            game.catalog=catalog;game.Rules.Configure(catalog);game.Rules.Progress=progress;game.Rules.Start();Field(game,"progress",progress);Field(game,"loadoutShowcase",-1);
            var spec=Item("turret").conditionalEffects.First(e=>e.customKey=="structure").structure;
            var objects=new List<GameObject>();
            try
            {
                for(int i=0;i<5;i++)
                {
                    Call(game,"DeployStructure",spec,null,(Vector2?)Vector2.zero);
                    var list=(IList)typeof(ArenaGame).GetField("starterTurrets",Fields).GetValue(game);
                    var turret=list[list.Count-1];objects.Add((GameObject)turret.GetType().GetField("obj").GetValue(turret));
                    Below(id); // Live tree-created actors are not registrations.
                }
                for(int i=0;i<4;i++)game.Rules.Items.Add(Item("turret"));
                game.Rules.CheckStructureDeploymentUnlock();Below(id);
                game.Rules.Items.Add(Item("turret"));
                game.Rules.CheckStructureDeploymentUnlock();
            }
            finally{foreach(var obj in objects)UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(host);}
        }
        void ReturnProducer(float remaining)
        {
            var host=new GameObject("Unlock return producer");var game=host.AddComponent<ArenaGame>();game.enabled=false;game.catalog=catalog;
            game.Rules.Configure(catalog);game.Rules.Start();game.Rules.Tick(game.Rules.Remaining-remaining);
            Field(game,"progress",progress);Field(game,"loadoutShowcase",-1);
            var player=new GameObject("Return fixture actor");Field(game,"player",player);
            var audio=host.AddComponent<GameAudio>();audio.Initialize(AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset"),true);Field(game,"sound",audio);
            try{Call(game,"ReturnToSelection",0);}
            finally{UnityEngine.Object.DestroyImmediate(player);UnityEngine.Object.DestroyImmediate(host);}
        }
    }
}
