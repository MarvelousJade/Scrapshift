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
        static void StartUnlockWorldEventsReview()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--unlock-world-events-review");
            if(i>=0&&i+1<args.Length)new GameObject("Unlock world events review").AddComponent<UnlockWorldEventsReview>().folder=args[i+1];
        }
        sealed class UnlockWorldEventsReview:MonoBehaviour
        {
            public string folder;ArenaGame game;Progression profile;UnlockDefinition[] definitions;float started;
            readonly Dictionary<string,int> notifications=new Dictionary<string,int>();
            [Serializable] sealed class Check {public string name,evidence;public bool passed;}
            [Serializable] sealed class Report
            {
                public string status,error,savePath;
                public string scope="Staged native event producers through actual Update. Imported fruit objects, hostile bullets, Torch burn payload and Rocket Launcher explosion metadata. Enemy HP/positions, explosion creation and deadlines are staged; no Record*/Count/Complete challenge shortcuts. Not natural weapon firing/proc probability, human survival balance or 115 native playthroughs.";
                public int fruits,hostileContacts,burnKills,explosionKills;public List<Check> checks=new List<Check>();public List<string> captures=new List<string>();
            }
            readonly Report report=new Report();
            IEnumerator Start()
            {
                started=Time.realtimeSinceStartup;string error=null;var stack=new Stack<IEnumerator>();stack.Push(Run());
                while(stack.Count>0)
                {
                    object next=null;bool moved=false;
                    try{Require(Time.realtimeSinceStartup-started<55,"Unlock producer fixture exceeded 55 seconds.");moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;else stack.Pop();}
                    catch(Exception ex){error=ex.ToString();break;}
                    if(!moved)continue;if(next is IEnumerator child){stack.Push(child);continue;}yield return next;
                }
                report.error=error;report.status=error==null&&report.checks.Count>=20&&report.checks.All(c=>c.passed)?"Passed":"Failed";
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"unlock-world-events.json"),JsonUtility.ToJson(report,true));Application.Quit(report.status=="Passed"?0:1);
            }
            IEnumerator Run()
            {
                folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&GameSettings.IsFixture,"Use --controller-check <input-folder> for isolated settings/profile.");
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);game.sound.SetMuted(true);
                definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
                report.savePath=Path.Combine(folder,"native-unlocks-"+Guid.NewGuid().ToString("N")+".json");profile=new Progression(definitions,report.savePath);
                game.progress=profile;game.runUnlockPool=new RunUnlockPool();
                profile.ChallengeCompleted+=d=>{notifications.TryGetValue(d.id,out int n);notifications[d.id]=n+1;game.achievementToasts.Enqueue(d);game.ApplyUnlocks();};game.ApplyUnlocks();

                Stage();game.invulnerable=100;
                yield return FruitBatch(19);CheckThat("hungry-below-20",!profile.Completed("chal_hungry"),"19 real fruit pickups in one run");
                yield return FruitBatch(1);CheckThat("hungry-at-20",profile.Completed("chal_hungry"),"20th actual pickup -> CollectPickup -> CollectConsumable -> Hungry");
                yield return FruitBatch(79);CheckThat("fruit-below-100",!profile.Completed("chal_fruit_basket")&&profile.Total("full_health_fruit")==99,"99 actual full-health fruit contacts, no counter staging");
                yield return FruitBatch(1);CheckThat("fruit-at-100",profile.Completed("chal_fruit_basket")&&profile.Total("full_health_fruit")==100,"100th actual full-health fruit contact -> saved lifetime challenge");
                CheckThat("fruit-current-pool-frozen",!Item("fruit_basket").available&&game.runUnlockPool.Frozen,"Achievement callback runs; active eligibility snapshot remains frozen");
                SaveCheck("chal_hungry");SaveCheck("chal_fruit_basket");yield return Capture("fruit-contact-unlocks.png");
                Stage();CheckThat("fruit-next-run-eligible",Item("fruit_basket").available,"Actual fresh StartSelectedRun refreshes saved item reward");

                game.invulnerable=0;Contact();yield return new WaitForSecondsRealtime(.1f);Contact();yield return new WaitForSecondsRealtime(.1f);
                CheckThat("projectiles-below-3",!profile.Completed("chal_jellyshield"),"Two real hostile bullet contacts; invulnerability reset only before each staged contact");
                Contact();yield return new WaitForSecondsRealtime(.1f);
                CheckThat("projectiles-at-3",profile.Completed("chal_jellyshield")&&report.hostileContacts==3,"Actual Update projectile hurtbox -> RecordProjectileHit before damage handling");
                CheckThat("jelly-current-pool-frozen",!Item("jellyshield").available,"Immediate achievement remains excluded from current item pool");SaveCheck("chal_jellyshield");yield return Capture("hostile-contact-unlock.png");
                Stage();CheckThat("jelly-next-run-eligible",Item("jellyshield").available,"Fresh run refreshes saved Jellyshield reward");game.invulnerable=100;

                var torch=game.catalog.Weapon("torch",1);Require(torch!=null,"Imported Torch required.");
                yield return BurnBatch(29,torch);CheckThat("burn-below-30",!profile.Completed("chal_will_o_the_wisp"),"29 actual timed burn deaths in this wave");
                yield return BurnBatch(1,torch);CheckThat("burn-at-30",profile.Completed("chal_will_o_the_wisp")&&report.burnKills==30,"30th real burn timeout -> Damage/death -> RecordBurnKill");
                CheckThat("burn-current-pool-frozen",!Item("will_o_the_wisp").available,"Current run excludes immediate item unlock");SaveCheck("chal_will_o_the_wisp");yield return Capture("burn-death-unlock.png");
                Stage();CheckThat("burn-next-run-eligible",Item("will_o_the_wisp").available,"Fresh run refreshes saved Will-o-the-Wisp reward");game.invulnerable=100;

                yield return ExplosionBatch(14);CheckThat("explosion-below-15",!profile.Completed("chal_fireworks"),"One native explosion kills 14, below required 15");
                game.ClearCombat();yield return ExplosionBatch(15);
                CheckThat("explosion-at-15",profile.Completed("chal_fireworks"),"A distinct actual timed explosion kills 15; prior explosion does not supply remainder");
                SaveCheck("chal_fireworks");CheckThat("fireworks-character-visible",game.catalog.data.characters.Single(c=>c.id=="artificer").available,"Immediate achievement callback exposes character reward");
                yield return Capture("explosion-death-unlock.png");
                Stage();CheckThat("all-rewards-still-saved",new[]{"chal_hungry","chal_fruit_basket","chal_jellyshield","chal_will_o_the_wisp","chal_fireworks"}.All(id=>new Progression(definitions,report.savePath).Completed(id)),"Saved outcomes survive final next-run transition");
            }
            void Stage()
            {
                game.playerAnimation.lookIndex=Array.FindIndex(game.catalog.data.characters,c=>c.id=="well_rounded");game.startingWeapon=0;game.StartSelectedRun();game.ClearCombat();
                game.Rules.Inventory.Clear();game.SyncEquipment();game.arrivalAge=ArrivalMotion.Duration;game.invulnerable=0;game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},1771);game.births.Clear();
            }
            IEnumerator FruitBatch(int count)
            {
                long before=profile.Total("full_health_fruit");for(int i=0;i<count;i++)game.DropConsumable(game.playerPos,ConsumableKind.Fruit);
                float until=Time.realtimeSinceStartup+3;
                while(profile.Total("full_health_fruit")<before+count&&Time.realtimeSinceStartup<until)yield return null;
                Require(profile.Total("full_health_fruit")==before+count,"Actual pickup contacts failed for fruit batch "+count);report.fruits+=count;
            }
            void Contact()
            {
                game.invulnerable=0;var owner=game.enemies.FirstOrDefault();if(owner==null){game.Spawn(game.catalog.Enemy("baby_alien"),game.playerPos+Vector2.right*7);owner=game.enemies.Last();}
                game.EnemyBullet(owner,Vector2.zero);var shot=game.shots.Last();shot.damage=1;shot.pos=game.PlayerHurtCenter;shot.obj.transform.position=shot.pos;shot.velocity=Vector2.zero;report.hostileContacts++;
            }
            IEnumerator BurnBatch(int count,Equipment torch)
            {
                int before=game.Rules.Kills;
                for(int i=0;i<count;i++)
                {
                    var target=game.FeelEnemy(game.playerPos+new Vector2(3+(i%5)*.12f,2+(i/5)*.12f));target.hp=target.maxHp=1;
                    Require(game.TryApplyWeaponBurn(target,torch,false),"Imported Torch burn application must succeed.");
                }
                float until=Time.realtimeSinceStartup+3;while(game.Rules.Kills<before+count&&Time.realtimeSinceStartup<until)yield return null;
                Require(game.Rules.Kills==before+count,"Expected actual timed burn deaths "+count);report.burnKills+=count;
            }
            IEnumerator ExplosionBatch(int count)
            {
                int before=game.Rules.Kills;Enemy center=null;
                for(int i=0;i<count;i++){var target=game.FeelEnemy(game.playerPos+new Vector2(3+(i%4)*.1f,2+(i/4)*.1f));target.hp=target.maxHp=1;if(center==null)center=target;}
                var weapon=game.catalog.Weapon("rocket_launcher",2);Require(weapon!=null,"Imported Rocket Launcher required.");
                var trait=(weapon.traits??Array.Empty<WeaponTrait>()).FirstOrDefault(t=>t.key=="effect_explode");Require(trait!=null,"Imported explosion trait required.");
                game.SpawnReferenceExplosion(center,weapon,trait,100,false);
                float until=Time.realtimeSinceStartup+2;while(game.Rules.Kills<before+count&&Time.realtimeSinceStartup<until)yield return null;
                Require(game.Rules.Kills==before+count,"Expected actual explosion overlap deaths "+count);report.explosionKills+=count;
            }
            ItemEntry Item(string id)=>game.catalog.data.items.Single(i=>i.id==id);
            void SaveCheck(string id)
            {
                profile.Save();var loaded=new Progression(definitions,report.savePath);notifications.TryGetValue(id,out int count);
                CheckThat(id+"-save-reload-once",loaded.Completed(id)&&count==1,"Production event completion saved/reloaded; exactly one target notification");
            }
            IEnumerator Capture(string name)
            {ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));yield return null;yield return new WaitForSecondsRealtime(.15f);Require(File.Exists(Path.Combine(folder,name)),"Missing capture "+name);report.captures.Add(name);}
            void CheckThat(string name,bool passed,string evidence){report.checks.Add(new Check{name=name,passed=passed,evidence=evidence});Require(passed,name);}
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
