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
        static void StartHeadlessProcTargetReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--headless-proc-target-review");
            if(index<0||index+1>=args.Length)return;
            new GameObject("Headless item proc target acceptance").AddComponent<HeadlessProcTargetReview>().folder=args[index+1];
        }
        sealed class HeadlessProcTargetReview:MonoBehaviour
        {
            public string folder;
            [Serializable] sealed class Row {public string mode;public int seed;public bool captured;}
            [Serializable] sealed class Report
            {
                public string status,error;
                public bool headless=true,capturesVerified=false,isolated,startInstalledHook,zeroHpSkippedRolls,deadTargetNotRedirected,removedTargetNotRedirected;
                public bool vulnerabilityBaseline,vulnerabilityMatchesSource;
                public float vulnerabilityExpectedHealth,vulnerabilityObservedHealth;
                public bool expiredVulnerabilityMatchesSource;
                public float expiredVulnerabilityExpectedHealth,expiredVulnerabilityObservedHealth;
                public string scope="Built-player target reservation and dispatch guards. Actual ArenaGame.Start installs the hook; real imported Lich and private Arena candidate controls. Vulnerability check additionally spawns a real enemy and applies actual Wand Damage between Lich healing and queued dispatch. Enemy HP, callback order and timer contexts are staged; no ordinary Update/physics, deferred original death scheduling, UI, controller or human evidence.";
                public List<Row> rows=new List<Row>();
            }
            sealed class NoProcRolls:System.Random
            {public override double NextDouble(){throw new InvalidOperationException("Rejected target consumed a proc probability roll.");}}
            static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
            static Enemy Target(float hp)=>new Enemy{hp=hp,maxHp=hp,type=new EnemyEntry{id="headless-proc-target",role="Enemy"}};
            IEnumerator Start()
            {
                // Allow the ordinary ArenaGame.Start to install its delegate;
                // the fixture does not install or replace that hook itself.
                yield return null;yield return null;
                var report=new Report();
                try
                {
                    folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);
                    report.isolated=GameSettings.IsFixture;
                    Require(Application.isBatchMode&&report.isolated,"Requires -batchmode -nographics and isolated --headless-proc-target-review.");
                    var game=FindAnyObjectByType<ArenaGame>();Require(game!=null,"Initialized ArenaGame required.");
                    var hook=game.Rules.CaptureRandomDamageTarget;
                    report.startInstalledHook=hook!=null&&ReferenceEquals(hook.Target,game)&&hook.Method.Name=="CaptureImplantDamageTarget";
                    Require(report.startInstalledHook,"ArenaGame.Start must install the production target capture hook.");
                    game.enabled=false;game.sound.SetMuted(true);game.ClearCombat();game.Rules.Progress=null;
                    var lich=game.catalog.data.characters.Single(c=>c.id=="lich");
                    game.Rules.Start(lich,StarterChoices.Available(game.catalog,lich).First());game.Rules.Inventory.Clear();
                    int mixedCaptured=0;
                    foreach(string mode in new[]{"zero_hp_only","alive_only","pending_and_alive"})
                    for(int seed=0;seed<64;seed++)
                    {
                        game.enemies.Clear();
                        if(mode!="alive_only")game.enemies.Add(Target(0));
                        if(mode!="zero_hp_only")game.enemies.Add(Target(1000));
                        UnityEngine.Random.InitState(seed);
                        bool captured=hook()!=null;
                        if(mode=="zero_hp_only")Require(!captured,"Zero-HP-only pool must abort.");
                        if(mode=="alive_only")Require(captured,"Live-only pool must capture.");
                        if(mode=="pending_and_alive"&&captured)mixedCaptured++;
                        report.rows.Add(new Row{mode=mode,seed=seed,captured=captured});
                    }
                    Require(mixedCaptured>0&&mixedCaptured<64,"Mixed pool must preserve both live and pending-dead selection outcomes.");
                    game.enemies.Clear();game.enemies.Add(Target(0));
                    typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(game.Rules,new object[]{9f});
                    var randomField=typeof(RunRules).GetField("random",BindingFlags.Instance|BindingFlags.NonPublic);
                    object previous=randomField.GetValue(game.Rules);int requests=0;
                    Action<float> observer=_=>requests++;game.Rules.RandomDamageRequested+=observer;
                    try {randomField.SetValue(game.Rules,new NoProcRolls());game.Rules.Heal(1);}
                    finally {randomField.SetValue(game.Rules,previous);game.Rules.RandomDamageRequested-=observer;}
                    report.zeroHpSkippedRolls=game.Rules.Health==10&&requests==0&&game.implantDamage.Count==0;
                    Require(report.zeroHpSkippedRolls,"Rejected Lich target must heal normally without rolling or queuing damage.");
                    report.deadTargetNotRedirected=CheckStaleTarget(game,false);
                    report.removedTargetNotRedirected=CheckStaleTarget(game,true);
                    Require(report.deadTargetNotRedirected&&report.removedTargetNotRedirected,"Captured stale target must never redirect to a replacement.");
                    game.enemies.Clear();
                    report.vulnerabilityBaseline=Array.IndexOf(Environment.GetCommandLineArgs(),"--proc-vulnerability-baseline")>=0;
                    CheckVulnerability(game,report);
                    if(report.vulnerabilityBaseline)
                        Require(!report.vulnerabilityMatchesSource&&report.vulnerabilityObservedHealth==88,"Baseline must reproduce the old extra vulnerability damage (88 HP versus source89).");
                    else Require(report.vulnerabilityMatchesSource&&report.expiredVulnerabilityMatchesSource,"Queued Lich damage must retain its source-time vulnerability context (new vulnerability89HP; expired vulnerability88HP).");
                    game.ClearCombat();report.status="Passed";
                }
                catch(Exception ex){report.status="Failed";report.error=ex.ToString();}
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"headless-proc-target.json"),JsonUtility.ToJson(report,true));
                Application.Quit(report.status=="Passed"?0:1);
            }
            static bool CheckStaleTarget(ArenaGame game,bool remove)
            {
                game.enemies.Clear();var selected=Target(1000);game.enemies.Add(selected);
                var submit=game.Rules.CaptureRandomDamageTarget();Require(submit!=null,"Live initial reservation required.");
                var replacement=Target(1000);game.enemies.Add(replacement);submit(10);
                if(remove)game.enemies.Remove(selected);else selected.hp=0;
                game.ResolveImplantDamage();return replacement.hp==1000&&game.implantDamage.Count==0;
            }
            static void CheckVulnerability(ArenaGame game,Report report)
            {
                var target=PrepareVulnerabilityTarget(game);
                game.Rules.Heal(1);Require(game.implantDamage.Count==1,"Imported Lich must queue one healing attack.");
                game.Damage(game.enemies.IndexOf(target),1,-100,0,Vector2.zero,game.catalog.Weapon("wand",1));
                Require(target.iceClock>0&&target.icePercent==10&&target.hp==99,"Actual elemental hit must activate imported Ice Cube before dispatch.");
                game.ResolveImplantDamage();report.vulnerabilityExpectedHealth=89;report.vulnerabilityObservedHealth=target.hp;
                report.vulnerabilityMatchesSource=target.hp==89;

                target=PrepareVulnerabilityTarget(game);
                game.Damage(game.enemies.IndexOf(target),1,-100,0,Vector2.zero,game.catalog.Weapon("wand",1));
                Require(target.hp==99&&target.iceClock>0,"Reverse case needs the actual Ice Cube hit first.");
                game.Rules.Heal(1);Require(game.implantDamage.Count==1,"Reverse case needs one Lich attack while vulnerability is active.");
                target.iceClock=0; // Explicit timer-expiry context, not an ordinary Update timing claim.
                game.ResolveImplantDamage();report.expiredVulnerabilityExpectedHealth=88;report.expiredVulnerabilityObservedHealth=target.hp;
                report.expiredVulnerabilityMatchesSource=target.hp==88;
            }
            static Enemy PrepareVulnerabilityTarget(ArenaGame game)
            {
                game.ClearCombat();
                var lich=game.catalog.data.characters.Single(c=>c.id=="lich");
                game.Rules.Start(lich,StarterChoices.Available(game.catalog,lich).First());game.Rules.Inventory.Clear();
                game.Rules.GrantItem(game.catalog.data.items.Single(item=>item.id=="ice_cube"));
                game.Rules.GainWeaponStat("crit_chance",-1000);
                // Isolate the explicit healing callback from Lich's native life
                // steal; the elemental hit must only change the enemy context.
                game.Rules.GainWeaponStat("lifesteal",-game.Rules.Stat("lifesteal")/lich.Gain("lifesteal"));
                game.Spawn(game.catalog.Enemy("baby_alien"),game.playerPos+new Vector2(6,6));
                var target=game.enemies.Last();Destroy(target.marker);target.marker=null;target.obj.SetActive(true);target.hp=target.maxHp=100;
                typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(game.Rules,new object[]{9f});return target;
            }
        }
    }
}
