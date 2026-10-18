using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartSurvivalRuntimeReview()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--survival-review");if(i<0||i+1>=args.Length)return;
            var fixture=new GameObject("Native non-invulnerable survival review").AddComponent<NativeSurvivalReview>();fixture.folder=args[i+1];
        }
        sealed class NativeSurvivalReview:MonoBehaviour
        {
            public string folder;ArenaGame game;Gamepad pad;float started;
            [Serializable] sealed class SurvivalReport
            {
                public string status,error,scope="Opt-in staged native survival fixture. Incoming hits use production hostile projectile contact through actual Update with invulnerability initially zero. Armor, regeneration, Torture, pickups and retry state are staged; pause calls use the production model API. Retry is confirmed through actual virtual A input/OnGUI; fixture-only IgnoreFocus prevents synthetic button resets, and pending full-screen unlock modals are cleared only in this in-memory profile to expose acknowledgment. Not human survival balance or a full campaign.";
                public bool inMemoryProfile,fixtureIgnoreFocus,unlockModalsSuppressed,retryConfirmedThroughUi,armorContact,iframeBlocksSecondHit,regenHealed,cleanupTortureHealed,lethalContact,deathClearedActors,
                    deathPauseFreezes,delayedAcknowledgment,rawBankNoIncome,noDeathHarvest,deadFruitUncollected,retryRestoresSameWave,capturesExist;
                public float healthAfterContact,healthAfterRegen,healthAfterCleanupFruit,deathAgeBeforePause,deathAgeAfterPause,deathAgeAtAcknowledgment;
                public int deathMaterials,deathXp,deathBanked,retries;
            }
            readonly SurvivalReport report=new SurvivalReport();
            IEnumerator Start()
            {
                started=Time.realtimeSinceStartup;string error=null;var routine=Run();
                while(true)
                {
                    bool more=false;object next=null;
                    try{Require(Time.realtimeSinceStartup-started<20,"Survival fixture exceeded 20 seconds.");more=routine.MoveNext();if(more)next=routine.Current;}
                    catch(Exception ex){error=ex.ToString();}
                    if(!more)break;yield return next;
                }
                if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
                report.error=error;report.capturesExist=new[]{"damaged.png","regenerated.png","torture-cleanup.png","defeat-ack.png","defeat-retry.png","retried.png"}.All(name=>File.Exists(Path.Combine(folder,name))&&new FileInfo(Path.Combine(folder,name)).Length>0);
                bool passed=error==null&&report.inMemoryProfile&&report.fixtureIgnoreFocus&&report.retryConfirmedThroughUi&&report.armorContact&&report.iframeBlocksSecondHit&&report.regenHealed&&report.cleanupTortureHealed&&report.lethalContact&&report.deathClearedActors&&report.deathPauseFreezes&&report.delayedAcknowledgment&&report.rawBankNoIncome&&report.noDeathHarvest&&report.deadFruitUncollected&&report.retryRestoresSameWave&&report.capturesExist;
                report.status=passed?"Passed":"Failed";Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"survival-review.json"),JsonUtility.ToJson(report,true));Application.Quit(passed?0:1);
            }
            IEnumerator Run()
            {
                Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null,"ArenaGame initialization required.");
                report.inMemoryProfile=GameSettings.IsFixture;Require(report.inMemoryProfile&&Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use isolated --controller-check profile.");
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;report.fixtureIgnoreFocus=true;
                foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();InputSystem.QueueStateEvent(pad,new GamepadState());
                Stage();game.Rules.GainWeaponStat("armor",15);game.Rules.GainWeaponStat("hp_regeneration",10);
                Contact(4);Contact(4);float wait=Time.realtimeSinceStartup;
                while(game.Rules.Health>=10&&Time.realtimeSinceStartup-wait<1)yield return null;
                report.healthAfterContact=game.Rules.Health;report.armorContact=game.Rules.Phase==RunPhase.Playing&&game.Rules.Health==8&&game.invulnerable>0;
                Require(report.armorContact,"A non-invulnerable 4-damage projectile must deal 2 damage through 15 Armor.");
                Contact(4);yield return null;report.iframeBlocksSecondHit=game.Rules.Health==8;Require(report.iframeBlocksSecondHit,"An additional projectile during iframes must not damage HP.");
                foreach(var shot in game.shots)Destroy(shot.obj);game.shots.Clear();
                Capture("damaged.png");yield return null;
                wait=Time.realtimeSinceStartup;while(game.Rules.Health<9&&Time.realtimeSinceStartup-wait<1.5f)yield return null;
                report.healthAfterRegen=game.Rules.Health;report.regenHealed=game.Rules.Health>=9;Require(report.regenHealed,"10 Regeneration must heal through the ordinary combat Update timer.");Capture("regenerated.png");yield return null;
                Stage();game.Rules.GainWeaponStat("torture",2);game.Rules.Hurt(6);game.Rules.Heal(3);Require(game.Rules.Health==4,"Torture blocks combat healing.");
                game.Drop(game.playerPos,1,true);game.BeginWaveSweep();
                wait=Time.realtimeSinceStartup;while(game.Rules.Health<7&&Time.realtimeSinceStartup-wait<.75f)yield return null;
                report.healthAfterCleanupFruit=game.Rules.Health;report.cleanupTortureHealed=game.Rules.Health==7&&game.Rules.WaveSettlementPending;
                Require(report.cleanupTortureHealed,"A real cleanup fruit must heal through Torture's cleaning_up exception.");Capture("torture-cleanup.png");yield return null;
                Stage();GameSettings.Set("retryWave",false);game.Rules.GainWeaponStat("harvesting",20);game.Rules.CaptureWaveRetryState();
                float scale=PresentationLayout.CanvasScale(Screen.width,Screen.height);
                Vector2 bag=game.gameCamera.ScreenToWorldPoint(new Vector3(42*scale,Screen.height-140.5f*scale,20));
                game.Drop(bag,5,false);game.Drop(game.playerPos,1,true);Contact(1000);
                wait=Time.realtimeSinceStartup;while(game.Rules.Phase==RunPhase.Playing&&Time.realtimeSinceStartup-wait<1)yield return null;
                report.lethalContact=game.Rules.Phase==RunPhase.Lost&&game.Rules.Health==0&&game.Rules.DeathCleanupPending;
                Require(report.lethalContact,"Production hostile contact must enter lethal cleanup.");yield return null;
                report.deathClearedActors=game.enemies.Count==0&&game.shots.Count==0&&game.pets.Count==0;
                Require(report.deathClearedActors,"Death cleanup must disable enemies, projectiles and supports.");
                report.deathAgeBeforePause=game.Rules.DeathCleanupAge;game.Rules.Pause();yield return new WaitForSecondsRealtime(.15f);
                report.deathAgeAfterPause=game.Rules.DeathCleanupAge;report.deathPauseFreezes=game.Rules.Phase==RunPhase.Paused&&Mathf.Abs(report.deathAgeBeforePause-report.deathAgeAfterPause)<.0001f;
                Require(report.deathPauseFreezes,"Pause must freeze the loss cleanup timer.");game.Rules.Pause();
                wait=Time.realtimeSinceStartup;while(game.Rules.DeathCleanupPending&&Time.realtimeSinceStartup-wait<1)yield return null;
                report.deathAgeAtAcknowledgment=game.Rules.DeathCleanupAge;report.delayedAcknowledgment=game.Rules.Phase==RunPhase.Lost&&!game.Rules.DeathCleanupPending&&game.Rules.DeathCleanupAge>=.499f;
                Require(report.delayedAcknowledgment,"Loss acknowledgment must wait half a second.");
                wait=Time.realtimeSinceStartup;while(game.pickups.Any(p=>!p.fruit)&&Time.realtimeSinceStartup-wait<1.5f)yield return null;
                report.deathMaterials=game.Rules.Scrap;report.deathXp=game.Rules.XP;report.deathBanked=game.Rules.Banked;
                report.rawBankNoIncome=game.Rules.Scrap==100&&game.Rules.XP==0&&game.Rules.Banked==5;
                report.noDeathHarvest=game.Rules.Stat("harvesting")==20&&!game.Rules.WaveSettlementPending;
                report.deadFruitUncollected=game.pickups.Count(p=>p.fruit)==1&&game.Rules.Health==0&&game.Rules.PendingCrates==0;
                Require(report.rawBankNoIncome&&report.noDeathHarvest&&report.deadFruitUncollected,"Death banks raw material only, without XP/Harvesting/healing/reward collection.");
                game.progress.NewlyUnlocked.Clear();report.unlockModalsSuppressed=true;
                yield return null;Capture("defeat-ack.png");yield return null;
                GameSettings.Set("retryWave",true);yield return null;yield return null;Capture("defeat-retry.png");yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return new WaitForSecondsRealtime(.08f);
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return new WaitForSecondsRealtime(.15f);
                report.retryConfirmedThroughUi=game.Rules.Phase==RunPhase.Playing&&game.Rules.Retries==1;
                Require(report.retryConfirmedThroughUi,"Virtual A must activate the rendered Retry wave button through actual Update/OnGUI.");
                report.retries=game.Rules.Retries;report.retryRestoresSameWave=game.Rules.Phase==RunPhase.Playing&&game.Rules.Wave==1&&game.Rules.Retries==1&&game.Rules.Scrap==100&&game.Rules.XP==0&&game.Rules.Banked==0&&game.Rules.Health==10;
                Require(report.retryRestoresSameWave,"Retry must restore this wave's checkpoint, health and economy rather than restart progression.");
                Capture("retried.png");yield return null;yield return new WaitForSecondsRealtime(.2f);
            }
            void Stage()
            {
                game.Begin();game.ClearCombat();game.Rules.Start(startingMaterials:100);game.Rules.Inventory.Clear();game.SyncEquipment();
                game.arrivalAge=ArrivalMotion.Duration;game.invulnerable=0;game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},771);game.births.Clear();game.sound.SetMuted(true);
            }
            void Contact(float damage)
            {
                // Real production projectile object and contact path. Its birth
                // position/damage are staged so actual Update tests the hurtbox.
                var owner=game.enemies.FirstOrDefault();
                if(owner==null){game.Spawn(game.catalog.Enemy("baby_alien"),new Vector2(7,0));owner=game.enemies.Last();}
                game.EnemyBullet(owner,Vector2.zero);var shot=game.shots.Last();shot.damage=damage;shot.pos=game.PlayerHurtCenter;shot.obj.transform.position=shot.pos;shot.velocity=Vector2.zero;
            }
            void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
