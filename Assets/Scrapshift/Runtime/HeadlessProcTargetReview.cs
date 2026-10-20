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
                public string scope="Built-player target reservation and dispatch guards. Actual ArenaGame.Start installs the hook; real imported Lich and private Arena candidate list, with manually staged nonrendered Enemy records. No live damage sink, physics, deferred original death scheduling, UI, controller or human evidence.";
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
                    report.startInstalledHook=hook!=null&&hook.Target==game&&hook.Method.Name=="CaptureImplantDamageTarget";
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
                    game.enemies.Clear();report.status="Passed";
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
        }
    }
}
