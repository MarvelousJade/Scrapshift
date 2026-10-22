using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartFinalSettlementReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--final-settlement-review");
            if(index<0||index+1>=args.Length)return;
            var fixture=new GameObject("Native final settlement fixture").AddComponent<NativeFinalSettlementReview>();
            fixture.folder=args[index+1];
        }

        sealed class NativeFinalSettlementReview:MonoBehaviour
        {
            public string folder;
            ArenaGame game;float started;
            readonly List<FinalSettlementSample> samples=new List<FinalSettlementSample>();
            [Serializable] sealed class FinalSettlementSample
            {
                public string route,phase;
                public bool bossDelayed,sweepObserved,incomeAtSweepStart,growthDelayed,growthAfterDelay,finalCleanupDuration,settledOnce,noFinalChoices,captureExists;
                public int materialsBefore,materialsAfter,levelBefore,levelAfter,pendingLevels,pendingCrates;
                public float harvestingBefore,harvestingAfter,xpBefore,xpAfter,remainingAfterTrigger,cleanupSimulatedSeconds,routeWallSeconds,payoutObservedAge,growthObservedAge;
            }
            [Serializable] sealed class FinalSettlementReport
            {
                public string status,error,scope="Opt-in staged final-wave native fixture, not a 20-wave playthrough. Neutral run, 100 materials and 15 Harvesting are staged through public rule APIs. Ordinary Update performs timer expiry, immediate income/XP, delayed Harvesting growth and final cleanup for timer and delayed-boss routes. Boss death itself is not reenacted.";
                public bool inMemoryProfile;
                public FinalSettlementSample[] samples;
            }
            readonly FinalSettlementReport report=new FinalSettlementReport();
            IEnumerator Start()
            {
                started=Time.realtimeSinceStartup;string error=null;var routine=Run();
                while(true)
                {
                    bool running=false;object next=null;
                    try
                    {
                        Require(Time.realtimeSinceStartup-started<20,"Fixture exceeded 20 seconds.");
                        running=routine.MoveNext();if(running)next=routine.Current;
                    }
                    catch(Exception ex){error=ex.ToString();}
                    if(!running)break;
                    yield return next;
                }
                report.error=error;report.samples=samples.ToArray();
                bool passed=error==null&&report.inMemoryProfile&&samples.Count==2;
                foreach(var sample in samples)passed&=sample.bossDelayed&&sample.sweepObserved&&sample.incomeAtSweepStart&&sample.growthDelayed&&sample.growthAfterDelay&&sample.finalCleanupDuration&&sample.settledOnce&&sample.noFinalChoices&&sample.captureExists;
                report.status=passed?"Passed":"Failed";
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder,"final-settlement.json"),JsonUtility.ToJson(report,true));
                Application.Quit(passed?0:1);
            }
            IEnumerator Run()
            {
                Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null,"ArenaGame initialization required.");
                Require(Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use --controller-check for an isolated in-memory profile.");
                report.inMemoryProfile=GameSettings.IsFixture;Require(report.inMemoryProfile,"Fixture profile is required.");
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);
                game.sound.SetMuted(true);foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);
                foreach(string route in new[]{"timer","boss"})
                {
                    game.Begin();game.ClearCombat();game.arrivalAge=ArrivalMotion.Duration;
                    game.Rules.Start(startingMaterials:100);
                    game.Rules.GainWeaponStat("harvesting",15);game.Rules.Inventory.Clear();game.SyncEquipment();
                    game.Rules.SetShowcaseWave(RunRules.WaveCount);game.invulnerable=10;
                    var sample=new FinalSettlementSample{route=route,materialsBefore=game.Rules.Scrap,
                        harvestingBefore=game.Rules.Stat("harvesting"),xpBefore=game.Rules.XPProgress,levelBefore=game.Rules.Level};
                    samples.Add(sample);
                    Require(sample.harvestingBefore==15&&sample.materialsBefore==100&&sample.xpBefore==0,"Expected neutral staged setup: 100 materials, 15 Harvesting and no XP.");
                    if(route=="boss")
                    {
                        game.Rules.DefeatBoss();sample.remainingAfterTrigger=game.Rules.Remaining;
                        sample.bossDelayed=game.Rules.Phase==RunPhase.Playing&&game.Rules.Remaining>0&&game.Rules.Remaining<=.101f&&game.Rules.Scrap==100;
                        Require(sample.bossDelayed,"Boss completion must defer settlement through the short ordinary wave timer.");
                    }
                    else
                    {
                        game.Rules.Tick(game.Rules.Remaining-.025f);sample.remainingAfterTrigger=game.Rules.Remaining;
                        sample.bossDelayed=game.Rules.Phase==RunPhase.Playing&&game.Rules.Scrap==100;
                        Require(sample.bossDelayed,"Timer staging must not settle the wave early.");
                    }
                    float routeStarted=Time.realtimeSinceStartup;
                    // Yield only; actual ArenaGame.Update owns expiry and sweep.
                    while(game.Rules.Phase==RunPhase.Playing)
                    {
                        if(game.waveSweeping)
                        {
                            float age=game.waveSweepAge;
                            if(!sample.sweepObserved)
                            {
                                sample.sweepObserved=true;sample.payoutObservedAge=age;
                                sample.incomeAtSweepStart=age<.66f&&game.Rules.Scrap==115&&game.Rules.XPProgress==15&&game.Rules.Stat("harvesting")==15;
                                Require(sample.incomeAtSweepStart,"Income and XP must settle before cleanup while Harvesting remains ungrown.");
                            }
                            if(age<.66f)
                            {
                                Require(game.Rules.Stat("harvesting")==15,"Harvesting growth must wait until 0.66 seconds of cleanup.");
                                sample.growthDelayed=true;
                            }
                            else
                            {
                                Require(game.Rules.Stat("harvesting")==16,"Harvesting must grow after the 0.66-second cleanup delay.");
                                if(!sample.growthAfterDelay)sample.growthObservedAge=age;
                                sample.growthAfterDelay=true;
                            }
                            Require(game.Rules.Scrap==115&&game.Rules.XPProgress==15,"Cleanup must not repay income or XP.");
                        }
                        yield return null;
                    }
                    yield return null;
                    sample.cleanupSimulatedSeconds=game.waveSweepAge;
                    sample.routeWallSeconds=Time.realtimeSinceStartup-routeStarted;
                    sample.finalCleanupDuration=sample.cleanupSimulatedSeconds>=4f&&sample.routeWallSeconds>=3.9f;
                    Require(sample.finalCleanupDuration,"Final cleanup must consume four simulated seconds before the end state.");
                    sample.phase=game.Rules.Phase.ToString();sample.materialsAfter=game.Rules.Scrap;
                    sample.harvestingAfter=game.Rules.Stat("harvesting");sample.xpAfter=game.Rules.XPProgress;
                    sample.levelAfter=game.Rules.Level;sample.pendingLevels=game.Rules.PendingLevels;sample.pendingCrates=game.Rules.PendingCrates;
                    sample.noFinalChoices=game.Rules.Phase==RunPhase.Won;
                    Require(sample.sweepObserved&&sample.incomeAtSweepStart&&sample.growthDelayed&&sample.growthAfterDelay&&sample.noFinalChoices,"Ordinary Update must sweep then end Won without a final shop/reward choice.");
                    Require(sample.materialsAfter==115&&sample.harvestingAfter==16&&sample.xpAfter==15&&sample.levelAfter==0,
                        "Final settlement must add 15 materials/XP and grow Harvesting from 15 to 16.");
                    game.Rules.Tick(100);yield return null;
                    sample.settledOnce=game.Rules.Phase==RunPhase.Won&&game.Rules.Scrap==sample.materialsAfter&&
                        game.Rules.Stat("harvesting")==sample.harvestingAfter&&game.Rules.XPProgress==sample.xpAfter;
                    Require(sample.settledOnce,"Repeated Tick after Won must not pay final settlement twice.");
                    string path=Path.Combine(folder,"final-"+route+".png");
                    ScreenCapture.CaptureScreenshot(path);yield return null;yield return new WaitForSecondsRealtime(.15f);
                    sample.captureExists=File.Exists(path)&&new FileInfo(path).Length>0;
                    Require(sample.captureExists,"Final settlement capture missing.");
                }
            }
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
