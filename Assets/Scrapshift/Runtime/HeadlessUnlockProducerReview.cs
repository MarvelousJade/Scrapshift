using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartHeadlessUnlockProducerReview()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--headless-unlocks-review");
            if(i<0||i+1>=args.Length)return;
            if(!Application.isBatchMode)throw new InvalidOperationException("Headless unlock review requires -batchmode -nographics.");
            new GameObject("Headless unlock producer review").AddComponent<HeadlessUnlockProducerReview>().folder=args[i+1];
        }
        // Invokes real scene callbacks synchronously with a staged clock. This
        // deliberately does not describe manually invoked callbacks as Update.
        sealed class HeadlessUnlockProducerReview:MonoBehaviour
        {
            public string folder;ArenaGame game;ReferenceCatalog baseline;UnlockDefinition[] definitions;Progression profile;
            readonly Dictionary<string,int> notices=new Dictionary<string,int>();
            readonly HashSet<string> historical=new HashSet<string>{"chal_rookie","chal_hungry","chal_fruit_basket","chal_jellyshield","chal_will_o_the_wisp","chal_fireworks"};
            [Serializable] sealed class Row {public string id,reward,producer,staging;public bool below,completed,once,reloaded,frozen,nextRun;}
            [Serializable] sealed class Result
            {
                public string status,error;
                public bool headless=true,capturesVerified=false;
                public string scope="109 staged scene-callback producer cases, not natural campaigns, Update timing, UI or human input. Lifetime lower counters and difficulty history are staged; final event and cleanup dispatch are real runtime callbacks. Six historical native producer IDs remain separately attributed.";
                public List<Row> rows=new List<Row>();public string[] historicalNativeIds;
            }
            readonly Result result=new Result();Row current;
            IEnumerator Start()
            {
                yield return null;yield return null;
                string error=null;
                try
                {
                    folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);game=FindAnyObjectByType<ArenaGame>();
                    Require(game!=null&&GameSettings.IsFixture,"Isolated initialized game required.");game.enabled=false;
                    GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);GameSettings.Set("optimizeEndWaves",false);
                    game.sound.SetMuted(true);baseline=game.catalog;definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
                    foreach(var d in definitions.Where(d=>!historical.Contains(d.id)))
                    {
                        Reset(d);Produce(d);Verify(d);result.rows.Add(current);
                    }
                    Require(result.rows.Count==109&&result.rows.Select(r=>r.id).Distinct().Count()==109,"Finite imported inventory changed.");
                }
                catch(Exception ex){error=ex.ToString();if(current!=null&&!result.rows.Contains(current))result.rows.Add(current);}
                result.error=error;result.historicalNativeIds=historical.OrderBy(s=>s).ToArray();result.status=error==null?"Passed":"Failed";
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"headless-unlocks.json"),JsonUtility.ToJson(result,true));Application.Quit(error==null?0:1);
            }
            void Reset(UnlockDefinition d)
            {
                if(game.catalog!=baseline)Destroy(game.catalog);game.catalog=Instantiate(baseline);game.Rules.Configure(game.catalog);
                // Make the target reward genuinely locked even if the supplied
                // catalog shipped it available for a review fixture.
                SetRewardAvailability(d.reward,false);
                profile=new Progression(definitions,Path.Combine(folder,d.id+".json"));Require(profile.Data.completed.Count==0,"Use a fresh output directory.");notices.Clear();
                game.progress=profile;game.runUnlockPool=new RunUnlockPool();game.recordedFinish=false;game.progressClock=0;
                profile.ChallengeCompleted+=x=>{notices.TryGetValue(x.id,out int n);notices[x.id]=n+1;game.ApplyUnlocks();};
                current=new Row{id=d.id,reward=d.reward,staging="Neutral scene initialized; target reward initially locked; synchronous production callbacks and staged clocks."};StartRun();
            }
            void StartRun(string character=null,int difficulty=0)
            {
                game.Rules.Progress=null;game.Begin();game.ClearCombat();game.Rules.Progress=profile;game.Rules.Difficulty=difficulty;
                var profileCharacter=character==null?null:game.catalog.data.characters.Single(c=>c.id==character);
                string weapon=profileCharacter==null?null:StarterChoices.Available(game.catalog,profileCharacter).FirstOrDefault();
                string item=profileCharacter==null?null:StarterChoices.AvailableItems(game.catalog,profileCharacter).FirstOrDefault()?.id;
                game.Rules.Start(profileCharacter,weapon,0,item);
                game.Rules.Inventory.Clear();game.SyncEquipment();game.loadoutShowcase=-1;game.arrivalAge=ArrivalMotion.Duration;game.invulnerable=100000;
                game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},771);game.births.Clear();game.recordedFinish=false;game.progressClock=0;
                game.runUnlockPool.BeginRun(game.catalog,profile);
            }
            void Below(){Require(!profile.Completed(current.id),"Premature unlock: "+current.id);current.below=true;}
            void ProgressTick()=>game.TickProgression(.51f);
            void Sweep(bool victory=false)
            {
                if(victory)game.Rules.SetShowcaseWave(RunRules.WaveCount);
                game.BeginWaveSweep();game.TickWaveSweep(5);ProgressTick();
            }
            void Shop()
            {game.ClearCombat();Sweep();while(game.Rules.Phase==RunPhase.Upgrade)game.Rules.Choose(0);Require(game.Rules.Phase==RunPhase.Shop,"Shop lifecycle required.");}
            void Buy(Equipment weapon)
            {
                if(game.Rules.Phase!=RunPhase.Shop)Shop();typeof(RunRules).GetProperty("Scrap").SetValue(game.Rules,100000);
                game.Rules.Shop[0]=new ShopOffer{weapon=weapon,rolledWave=game.Rules.Wave};Require(game.Rules.Buy(0),"Staged actual weapon purchase failed.");
            }
            void StageCounter(string key,long value)
            {profile.Data.counters.Add(new ProgressCounter{key=key,value=value});current.staging+=" Lifetime lower counter "+key+"="+value+" is staged; only final increment uses an event.";}
            void Kill(string id,bool tree=false,float age=1,bool elite=false)
            {
                var def=game.catalog.Enemy(id);if(def==null&&tree)def=game.catalog.data.enemies.First(e=>e.role=="Tree");
                if(def==null&&elite)def=game.catalog.data.enemies.First(e=>e.role=="Elite");Require(def!=null,"Missing lethal target "+id);
                game.Spawn(def,game.playerPos+new Vector2(6,6));var target=game.enemies.Last();target.activeAge=age;target.hp=1;Destroy(target.marker);target.marker=null;target.obj.SetActive(true);
                game.Damage(game.enemies.IndexOf(target),100000,-100,0);game.ResolveCombatCallbacks();game.ClearCombat();
            }
            void CollectMaterial(int value)
            {
                game.Drop(game.playerPos,value,false);foreach(var pickup in game.pickups.ToArray()){game.CollectPickup(pickup);Destroy(pickup.obj);game.pickups.Remove(pickup);}
            }
            void Produce(UnlockDefinition d)
            {
                string id=d.id;int n=(int)d.value;
                if(d.description=="CHAL_CHARACTER_DESC")
                {StartRun(id.Substring(5));Below();current.producer="Final-wave scene cleanup -> Won -> TickProgression -> character Finish";Sweep(true);return;}
                if(d.id.StartsWith("chal_difficulty_")||d.id.StartsWith("unlock_difficulty_"))
                {
                    if(id=="unlock_difficulty_6")for(int i=0;i<9;i++)profile.Data.counters.Add(new ProgressCounter{key="danger5:"+game.catalog.data.characters[i].id,value=1});
                    string character=id=="unlock_difficulty_6"?game.catalog.data.characters[9].id:null;
                    StartRun(character,id=="unlock_difficulty_6"?5:id.StartsWith("unlock_")?n-1:n);Below();
                    current.producer="Staged final-wave scene cleanup -> Won -> TickProgression -> danger/system Finish";
                    if(id=="unlock_difficulty_6")current.staging+=" Nine distinct prior Danger5 records staged; tenth uses final lifecycle.";
                    Sweep(true);return;
                }
                if(d.description=="CHAL_STAT_DESC")
                {game.Rules.GainWeaponStat(d.stat,d.value+(d.value<0?1:-1)-game.Rules.Stat(d.stat));ProgressTick();Below();game.Rules.GainWeaponStat(d.stat,d.value-game.Rules.Stat(d.stat));ProgressTick();current.producer="GainWeaponStat -> actual TickProgression stat sweep below/at threshold";return;}
                if(d.description=="CHAL_SURVIVOR_DESC"||d.description=="CHAL_GATHERER_DESC"||id=="chal_lumberjack"||id=="chal_evil_hat")
                {
                    string key=d.description=="CHAL_GATHERER_DESC"?"materials":id=="chal_lumberjack"?"trees":id=="chal_evil_hat"?"evil_mob":"kills";
                    StageCounter(key,n-1);Sweep();Below();StartRun();
                    if(key=="materials")CollectMaterial(1);else Kill(key=="trees"?"tree":key=="evil_mob"?"evil_mob":"baby_alien",key=="trees");
                    Below();Sweep();current.producer="Staged lifetime lower counter + real CollectPickup or Damage/death -> BeginWaveSweep counted challenge";return;
                }
                Specific(id,n);
            }
            void Specific(string id,int n)
            {
                current.producer="Production rule action plus scene progression/cleanup callback; detailed family in documentation";
                switch(id)
                {
                    case "chal_hoarder":CollectMaterial(n-1);Below();CollectMaterial(1);break;
                    case "chal_turrets":for(int i=0;i<4;i++)game.Rules.GrantItem(Item("turret"));game.ResetStructures();Below();game.Rules.GrantItem(Item("turret"));game.ResetStructures();break;
                    case "chal_medicine":game.Rules.GainWeaponStat("max_hp",500);game.Rules.Hurt(n);game.Rules.Heal(n-1);Below();game.Rules.Heal(1);break;
                    case "chal_recycling":Shop();for(int i=0;i<n;i++){Buy(game.catalog.Weapon("pistol",1));game.Rules.Recycle(0);if(i<n-1)Below();}break;
                    case "chal_advanced_technology":case "chal_magic_and_machinery":game.Rules.GainWeaponStat(id=="chal_advanced_technology"?"ranged_damage":"elemental_damage",10);for(int i=0;i<2;i++)game.Rules.GrantItem(Item("turret"));ProgressTick();Below();game.Rules.GrantItem(Item("turret"));ProgressTick();break;
                    case "chal_giant_slayer":Kill("croc",false,15.001f,true);Below();Kill("croc",false,15,true);break;
                    case "chal_baited":game.Rules.GrantItem(Item("bait"));Below();game.Rules.GrantItem(Item("bait"));break;
                    case "chal_forest":Trees(9);Sweep();Below();StartRun();Trees(10);Sweep();break;
                    case "chal_bourgeoisie":Buy(game.catalog.Weapon("pistol",4));Buy(game.catalog.Weapon("pistol",4));Below();Buy(game.catalog.Weapon("pistol",4));break;
                    case "chal_student":while(game.Rules.Level<n-1)CollectMaterial(1);Below();while(game.Rules.Level<n)CollectMaterial(1);break;
                    case "chal_fast_learner":game.Rules.SetShowcaseWave(6);while(game.Rules.Level<n)CollectMaterial(1);Below();StartRun();game.Rules.SetShowcaseWave(5);while(game.Rules.Level<n-1)CollectMaterial(1);Below();while(game.Rules.Level<n)CollectMaterial(1);break;
                    case "chal_reckless":game.Rules.Hurt(game.Rules.Health-2);Sweep();Below();StartRun();game.Rules.Hurt(game.Rules.Health-1);Sweep();break;
                    case "chal_scavenger":var common=game.catalog.data.items.Where(i=>i.tier==1&&i.available).Take(n).ToArray();Require(common.Length==n,"Imported commons missing.");for(int i=0;i<n;i++){game.Rules.GrantItem(common[i]);if(i<n-1)Below();}break;
                    case "chal_experimentation":foreach(string weapon in new[]{"pistol","smg","laser_gun","knife","spear"})Buy(game.catalog.Weapon(weapon,1));Below();Buy(game.catalog.Weapon("wand",1));break;
                    case "chal_bonk_dog":case "chal_catling_gun":case "chal_bot_o_mine":case "chal_blazemander":
                        string stat=id=="chal_bonk_dog"?"melee_damage":id=="chal_catling_gun"?"ranged_damage":id=="chal_bot_o_mine"?"engineering":"elemental_damage";
                        var w=game.catalog.data.equipment.First(e=>e.tier==1&&e.scaling!=null&&e.scaling.Any(s=>s.key==stat));for(int i=0;i<6;i++){Buy(w);if(i<5)Below();}break;
                    case "chal_doc_moth":game.Rules.GainWeaponStat("hp_regeneration",10);game.Rules.GainWeaponStat("lifesteal",9);ProgressTick();Below();game.Rules.GainWeaponStat("lifesteal",1);ProgressTick();break;
                    case "chal_lootworm":game.Drop(game.playerPos+Vector2.one*5,19,false);Sweep();Below();StartRun();game.Drop(game.playerPos+Vector2.one*5,20,false);Sweep();break;
                    case "chal_hourglass":game.Rules.Tick(game.Rules.Remaining-5);game.ReturnToSelection(0);Below();StartRun();game.Rules.Tick(game.Rules.Remaining-4.99f);game.ReturnToSelection(0);break;
                    case "chal_candy_bag":Shop();game.Rules.ToggleLock(0);game.Rules.NextWave();Sweep(true);Below();StartRun();Sweep(true);break;
                    case "chal_paws_n_claws":
                        foreach(string scale in new[]{"melee_damage","ranged_damage","engineering","elemental_damage"}){game.Rules.Inventory.Clear();var weapon=game.catalog.data.equipment.First(e=>e.tier==1&&e.scaling!=null&&e.scaling.Any(s=>s.key==scale));for(int i=0;i<6;i++)Buy(weapon);}
                        Below();StartRun();game.Drop(game.playerPos+Vector2.one*5,20,false);Sweep();break;
                    default:throw new InvalidOperationException("Unmapped imported challenge "+id);
                }
            }
            void Trees(int count)
            {
                var tree=game.catalog.data.enemies.First(e=>e.role=="Tree");for(int i=0;i<count;i++){game.Spawn(tree,game.playerPos+new Vector2(5+i*.1f,5));var e=game.enemies.Last();Destroy(e.marker);e.marker=null;e.obj.SetActive(true);}
                game.Simulate(.001f);Require(game.Rules.LivingTrees==count,"Actual scene tree census failed.");
            }
            ItemEntry Item(string id)=>game.catalog.data.items.Single(i=>i.id==id);
            void Verify(UnlockDefinition d)
            {
                current.completed=profile.Completed(d.id);notices.TryGetValue(d.id,out int count);current.once=count==1;
                Require(current.below&&current.completed&&current.once,"Producer boundary/dispatch failed "+d.id);
                profile.Save();var reload=new Progression(definitions,Path.Combine(folder,d.id+".json"));current.reloaded=reload.Completed(d.id)&&reload.Unlocked(d.reward);Require(current.reloaded,"Persistence failed "+d.id);
                // Finish can intentionally unfreeze an ended run. A separate
                // eligibility snapshot tests the same unlock under active-run rules.
                var isolated=Instantiate(baseline);SetAvailability(isolated,d.reward,false);var pool=new RunUnlockPool();pool.BeginRun(isolated,new Progression(definitions));pool.Refresh(isolated,reload);
                bool itemOrWeapon=d.reward.StartsWith("item_")||d.reward.StartsWith("weapon_");current.frozen=itemOrWeapon?!RewardAvailable(isolated,d.reward,reload):RewardAvailable(isolated,d.reward,reload);Require(current.frozen,"Frozen pool leaked "+d.id);
                pool.BeginRun(isolated,reload);current.nextRun=RewardAvailable(isolated,d.reward,reload);Require(current.nextRun,"Next-run eligibility failed "+d.id);Destroy(isolated);
            }
            void SetRewardAvailability(string reward,bool value)=>SetAvailability(game.catalog,reward,value);
            static void SetAvailability(ReferenceCatalog c,string reward,bool value)
            {
                if(reward.StartsWith("item_"))foreach(var i in c.data.items.Where(i=>i.id==reward.Substring(5)))i.available=value;
                else if(reward.StartsWith("weapon_"))foreach(var w in c.data.equipment.Where(w=>w.id==reward.Substring(7)))w.available=value;
                else if(reward.StartsWith("character_"))foreach(var ch in c.data.characters.Where(ch=>ch.id==reward.Substring(10)))ch.available=value;
            }
            static bool RewardAvailable(ReferenceCatalog c,string reward,Progression p)
            {
                if(reward.StartsWith("item_"))return c.data.items.Single(i=>i.id==reward.Substring(5)).available;
                if(reward.StartsWith("weapon_"))return c.data.equipment.Where(w=>w.id==reward.Substring(7)).All(w=>w.available);
                if(reward.StartsWith("character_"))return c.data.characters.Single(ch=>ch.id==reward.Substring(10)).available;
                return reward.StartsWith("difficulty_")&&p.Difficulty>=int.Parse(reward.Substring(11));
            }
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}




