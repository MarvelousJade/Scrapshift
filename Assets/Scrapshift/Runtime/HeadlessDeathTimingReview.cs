using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartHeadlessDeathTimingReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--headless-death-timing-review");
            if(index<0||index+1>=args.Length)return;
            new GameObject("Headless death timing acceptance").AddComponent<HeadlessDeathTimingReview>().folder=args[index+1];
        }
        sealed class HeadlessDeathTimingReview:MonoBehaviour
        {
            public string folder;
            [Serializable] sealed class Row
            {
                public string name,phase;
                public float health;
                public int kills,active,corpses,captures;
                public bool passed;
            }
            [Serializable] sealed class Report
            {
                public string status,error;
                public bool isolated,startInstalledCapture,startInstalledBeforeDeathCleanup;
                public string scope="Packaged ArenaGame.Start hooks and real spawned NPCs; disabled Arena Update with staged HP, explicit Damage/Heal/Hurt callbacks and manual deferred combat boundary. Imported Lich, Giant Belt, Cyberball; no ordinary physics scheduling, original full scene, UI, input, audio listening or human playtesting.";
                public List<Row> rows=new List<Row>();
            }
            static readonly FieldInfo RandomField=typeof(RunRules).GetField("random",BindingFlags.Instance|BindingFlags.NonPublic);
            sealed class NoProcRolls:System.Random
            {public override double NextDouble(){throw new InvalidOperationException("Zero-HP candidate must abort before Cyberball probability roll.");}}
            static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
            static void Health(ArenaGame game,float value)=>typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(game.Rules,new object[]{value});
            static void Prepare(ArenaGame game,string character="lich")
            {
                game.ClearCombat();RandomField.SetValue(game.Rules,new System.Random(20201020));
                var profile=game.catalog.data.characters.Single(c=>c.id==character);
                game.Rules.Start(profile,StarterChoices.Available(game.catalog,profile).First());
                game.Rules.Progress=null;game.Rules.Inventory.Clear();game.Rules.GainWeaponStat("lifesteal",-1000);
            }
            static Enemy Spawn(ArenaGame game,float hp)
            {
                game.Spawn(game.catalog.Enemy("evil_mob"),game.playerPos+new Vector2(6,6));
                var target=game.enemies.Last();Destroy(target.marker);target.marker=null;target.obj.SetActive(true);
                target.hp=target.maxHp=hp;target.gold=0;return target;
            }
            static void Record(Report report,ArenaGame game,string name,Enemy target,int captures=0)
            {
                report.rows.Add(new Row{name=name,phase=game.Rules.Phase.ToString(),health=target.hp,kills=game.Rules.Kills,
                    active=game.enemies.Count,corpses=game.corpses.Count,captures=captures,passed=true});
            }
            IEnumerator Start()
            {
                yield return null;yield return null;
                var report=new Report();
                try
                {
                    folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);
                    report.isolated=GameSettings.IsFixture;
                    Require(Application.isBatchMode&&report.isolated,"Requires isolated -batchmode -nographics --headless-death-timing-review.");
                    var game=FindAnyObjectByType<ArenaGame>();Require(game!=null,"Initialized ArenaGame required.");
                    var capture=game.Rules.CaptureRandomDamageTarget;
                    var cleanup=game.Rules.BeforeDeathCleanup;
                    report.startInstalledCapture=capture!=null&&ReferenceEquals(capture.Target,game)&&capture.Method.Name=="CaptureImplantDamageTarget";
                    report.startInstalledBeforeDeathCleanup=cleanup!=null&&ReferenceEquals(cleanup.Target,game)&&cleanup.Method.Name=="ResolveCombatCallbacks";
                    Require(report.startInstalledCapture&&report.startInstalledBeforeDeathCleanup,"ArenaGame.Start must install both production hooks.");
                    game.enabled=false;game.sound.SetMuted(true);game.Rules.Progress=null;

                    Prepare(game);var target=Spawn(game,100);Health(game,9);
                    game.Rules.Heal(1);Require(target.hp==90&&game.implantDamage.Count==0,"Lich must damage synchronously before another callback.");
                    Record(report,game,"Lich immediate 100 to 90",target);
                    game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="giant_belt"));game.Rules.GainWeaponStat("crit_chance",1000);
                    bool critical=game.Damage(game.enemies.IndexOf(target),10,0,0,Vector2.up,game.catalog.Weapon("pistol",1),true);
                    Require(critical&&target.hp==61,"Giant Belt critical must read post-Lich current HP before flushing.");
                    Record(report,game,"Lich then Giant Belt critical 61",target);

                    Prepare(game);target=Spawn(game,10);Health(game,9);game.Rules.Heal(1);
                    Require(target.hp==0&&game.enemies.Contains(target)&&game.Rules.Kills==0,"Lethal Lich must preserve pending member before deferred die.");
                    Record(report,game,"Lethal Lich pending member",target);
                    game.Damage(game.enemies.IndexOf(target),50,-100,0,Vector2.zero,null,true);
                    Require(target.hp==0&&game.Rules.Kills==0,"Pending victim must reject later damage.");
                    game.ResolveCombatCallbacks();game.ResolveCombatCallbacks();
                    Require(game.Rules.Kills==1&&!game.enemies.Contains(target)&&game.corpses.Count==1,"Deferred death must resolve exactly once.");
                    Record(report,game,"Lethal Lich finalized once",target);

                    Prepare(game,"well_rounded");target=Spawn(game,1);
                    game.Damage(game.enemies.IndexOf(target),1,-100,0,Vector2.zero,null,true,true);
                    Require(game.Rules.Kills==0,"Enemy-first case must begin pending.");
                    game.Rules.Hurt(100000);
                    Require(game.Rules.Phase==RunPhase.Lost&&game.Rules.Kills==1&&!game.enemies.Contains(target),"Already pending enemy death must dispatch before player defeat cleanup.");
                    Record(report,game,"Enemy first then lethal player hit",target);

                    Prepare(game,"well_rounded");target=Spawn(game,1);game.Rules.Hurt(100000);
                    Require(game.Rules.Phase==RunPhase.Lost,"Player-first case must enter Lost.");
                    game.Damage(game.enemies.IndexOf(target),50,-100,0,Vector2.zero,null,true,true);game.ResolveCombatCallbacks();
                    Require(target.hp==1&&game.Rules.Kills==0,"Later enemy damage after player defeat must not grant a kill.");
                    Record(report,game,"Player first ignores later enemy hit",target);

                    foreach(int count in new[]{1,3})
                    {
                        Prepare(game,"well_rounded");game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="cyberball"));
                        for(int i=0;i<count;i++){target=Spawn(game,1);game.Damage(game.enemies.IndexOf(target),1,-100,0,Vector2.zero,null,true,true);}
                        int captures=0,events=0;Action<float> observer=_=>events++;
                        game.Rules.CaptureRandomDamageTarget=()=>
                        {
                            captures++;Require(game.enemies.Count>0&&game.enemies.All(e=>e.hp==0),"Main-equivalent callback must retain a dying member.");
                            return capture();
                        };
                        game.Rules.RandomDamageRequested+=observer;RandomField.SetValue(game.Rules,new NoProcRolls());
                        try{game.ResolveCombatCallbacks();game.ResolveCombatCallbacks();}
                        finally{game.Rules.CaptureRandomDamageTarget=capture;game.Rules.RandomDamageRequested-=observer;}
                        Require(captures==count&&events==0&&game.Rules.Kills==count&&game.enemies.Count==0&&game.corpses.Count==count,"Dying Cyberball candidates must abort without a roll and finalize once each.");
                        Record(report,game,"Dying Cyberball members "+count,target,captures);
                    }
                    game.ClearCombat();report.status="Passed";
                }
                catch(Exception ex){report.status="Failed";report.error=ex.ToString();}
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"headless-death-timing.json"),JsonUtility.ToJson(report,true));
                Application.Quit(report.status=="Passed"?0:1);
            }
        }
    }
}
