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
        static void StartArrivalNativeReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--arrival-native-review");
            if(index<0||index+1>=args.Length)return;
            var fixture=new GameObject("Native arrival acceptance fixture").AddComponent<NativeArrivalReview>();
            fixture.folder=args[index+1];
        }

        // Opt-in built-player check. Virtual input goes through the real Update
        // loop; only clock staging and intermission acceleration are synthetic.
        sealed class NativeArrivalReview:MonoBehaviour
        {
            public string folder;
            ArenaGame game;Gamepad pad;float started;
            [Serializable] sealed class ArrivalSample
            {
                public string stage,phase;
                public float age,remaining,elapsed,weaponClock,logicalX,logicalY,rawOriginX,rawOriginY;
                public float renderedBodyX,renderedBodyY,renderedWeaponX,renderedWeaponY,expectedMountX,expectedMountY;
                public int shots;
                public bool arriving;
            }
            [Serializable] sealed class ArrivalReport
            {
                public string status,error,selectedWeapon,scope="Opt-in native fixture: actual Update with virtual Gamepad; a ranged starter, manual aim and a weapon clock are staged, and the later shop transition is accelerated. Not a human or physical-controller run.";
                public bool inMemoryProfile,rawOriginCentered,arrivalMovement,arrivalTimer,oneClockStep,
                    sourceWeaponKeys,firedWhileArriving,animatedWeaponMount,muzzleFromAnimatedMount,
                    arrivalFinished,secondWaveNoArrival,secondWaveRenderedAligned,capturesExist;
                public float authoredMuzzleError,secondWaveBodyError,secondWaveWeaponMountDistance;
                public ArrivalSample[] samples;
            }
            readonly System.Collections.Generic.List<ArrivalSample> samples=new System.Collections.Generic.List<ArrivalSample>();
            ArrivalReport report=new ArrivalReport();
            IEnumerator Start()
            {
                started=Time.realtimeSinceStartup;
                string error=null;var routine=Run();
                while(true)
                {
                    bool running=false;object next=null;
                    try
                    {
                        Require(Time.realtimeSinceStartup-started<12,"Fixture exceeded 12 seconds.");
                        running=routine.MoveNext();if(running)next=routine.Current;
                    }
                    catch(Exception ex){error=ex.ToString();}
                    if(!running)break;
                    yield return next;
                }
                if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
                report.error=error;report.samples=samples.ToArray();
                report.capturesExist=new[]{"arrival-moving.png","arrival-firing.png","arrival-finished.png","wave-two.png"}
                    .All(name=>{var path=Path.Combine(folder,name);return File.Exists(path)&&new FileInfo(path).Length>0;});
                bool passed=error==null&&report.inMemoryProfile&&report.rawOriginCentered&&report.arrivalMovement&&
                    report.arrivalTimer&&report.oneClockStep&&report.sourceWeaponKeys&&report.firedWhileArriving&&
                    report.animatedWeaponMount&&report.muzzleFromAnimatedMount&&
                    report.arrivalFinished&&report.secondWaveNoArrival&&report.secondWaveRenderedAligned&&report.capturesExist;
                report.status=passed?"Passed":"Failed";
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder,"arrival-native.json"),JsonUtility.ToJson(report,true));
                Application.Quit(passed?0:1);
            }
            IEnumerator Run()
            {
                Directory.CreateDirectory(folder);
                yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null,"ArenaGame initialization is required.");
                Require(Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use --controller-check for isolated in-memory options.");
                report.inMemoryProfile=GameSettings.IsFixture;
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);game.sound.SetMuted(true);
                foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);
                pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();State(new GamepadState());
                yield return new WaitForSecondsRealtime(.1f);
                Require(game.Rules.Phase==RunPhase.Title,"Fixture starts at title.");
                var choices=game.CharacterWeapons;
                int ranged=Array.FindIndex(choices,id=>id=="pistol"&&game.catalog.Weapon(id,game.SelectedCharacter.minTier)?.melee==false);
                if(ranged<0)ranged=Array.FindIndex(choices,id=>game.catalog.Weapon(id,game.SelectedCharacter.minTier)?.melee==false);
                Require(ranged>=0,"Selected character needs an eligible ranged starting weapon for this fixture.");
                game.startingWeapon=ranged;report.selectedWeapon=choices[ranged];
                game.StartSelectedRun();
                Require(game.Rules.Phase==RunPhase.Playing&&game.Arriving,"First selected run enters combat with arrival active.");
                Require(game.Rules.Inventory.Count>0&&!game.Rules.Inventory[0].melee,"Fixture requires its selected ranged weapon.");
                Vector2 raw=game.playerPos+CombatGeometry.PlayerOriginOffset;
                report.rawOriginCentered=raw.sqrMagnitude<.0001f&&Mathf.Abs(game.playerPos.y-.24f)<.01f;
                Require(report.rawOriginCentered,"StartSelectedRun must place the logical body .24 above raw origin zero.");
                report.sourceWeaponKeys=(ArrivalWeaponMotion.Offset(0)-new Vector2(-2.65f,7.25f)).sqrMagnitude<.0001f&&
                    (ArrivalWeaponMotion.Offset(.45f)-new Vector2(-.271691f,1.97988f)).sqrMagnitude<.0001f&&
                    ArrivalWeaponMotion.Offset(1.2f).sqrMagnitude<.0001f;
                Require(report.sourceWeaponKeys,"Weapon arrival transform must retain the supplied source's start, .45-second and end position keys.");
                Snapshot("start");
                game.invulnerable=5;game.weaponClocks[0]=5;
                Vector2 startPosition=game.playerPos;float startRemaining=game.Rules.Remaining,startElapsed=game.elapsed;
                State(new GamepadState{leftStick=Vector2.right});
                yield return new WaitForSecondsRealtime(.32f);
                Snapshot("arrival-moving");Capture("arrival-moving.png");
                report.arrivalMovement=game.Arriving&&game.playerPos.x>startPosition.x+.08f;
                report.arrivalTimer=game.Arriving&&game.Rules.Remaining<startRemaining-.2f&&game.elapsed>startElapsed+.2f;
                float clockDecrease=5-game.weaponClocks[0],simulated=game.elapsed-startElapsed;
                report.oneClockStep=Mathf.Abs(clockDecrease-simulated)<.07f;
                Require(report.arrivalMovement&&report.arrivalTimer&&report.oneClockStep,
                    $"Arrival must move, advance timer, and step clock once (clock {clockDecrease:0.###}, time {simulated:0.###}).");
                GameSettings.Set("manualAim",true);GameSettings.Set("aimPressed",false);
                game.weaponClocks[0]=0;
                int shotsBefore=game.shotsFired;
                State(new GamepadState{leftStick=Vector2.right,rightStick=Vector2.right});
                float fireStarted=Time.realtimeSinceStartup;
                while(game.shotsFired==shotsBefore&&Time.realtimeSinceStartup-fireStarted<.35f)yield return null;
                Snapshot("arrival-firing");Capture("arrival-firing.png");
                var shot=game.shots.LastOrDefault(s=>s.weaponIndex==0&&!s.hostile);
                report.firedWhileArriving=game.Arriving&&game.shotsFired>shotsBefore&&shot!=null&&
                    Vector2.Dot(shot.velocity.normalized,Vector2.right)>.9f;
                Vector2 mount=ArrivalWeaponMotion.TransformMount(game.playerPos,
                    ActorPresentation.WeaponMount(game.Rules.Inventory.Count,0),game.arrivalAge);
                var view=game.equipmentViews[0];
                report.animatedWeaponMount=view!=null&&Mathf.Abs(view.transform.position.y-mount.y)<1.5f&&
                    Mathf.Abs(view.transform.position.y-(game.playerPos+ActorPresentation.WeaponMount(game.Rules.Inventory.Count,0)).y)>.8f;
                report.authoredMuzzleError=-1;
                if(shot!=null)
                {
                    // This fixture holds constant movement/aim, and Pistol has
                    // no sinusoid/bounce. Undo its flight and intervening player
                    // steps to compare the authored source origin at firing.
                    float age=shot.age,bornArrival=Mathf.Max(0,game.arrivalAge-age);
                    Vector2 bornPlayer=game.playerPos-game.playerVelocity*Mathf.Max(0,age-ReferencePhysicsClock.Step);
                    Vector2 bornMount=ArrivalWeaponMotion.TransformMount(bornPlayer,ActorPresentation.WeaponMount(game.Rules.Inventory.Count,0),bornArrival);
                    Vector2 expectedMuzzle=WeaponReference.MuzzlePosition(shot.weapon,bornMount,shot.velocity.normalized,bornArrival);
                    Vector2 actualMuzzle=shot.pos-shot.velocity*age;
                    report.authoredMuzzleError=Vector2.Distance(expectedMuzzle,actualMuzzle);
                }
                report.muzzleFromAnimatedMount=shot!=null&&view!=null&&report.authoredMuzzleError<.035f;
                Require(report.firedWhileArriving&&report.animatedWeaponMount&&report.muzzleFromAnimatedMount,
                    "Selected weapon must fire from its animated arrival mount before landing while preserving local aim.");
                State(new GamepadState());
                float landingWait=Time.realtimeSinceStartup;
                while(game.Arriving&&Time.realtimeSinceStartup-landingWait<1.3f)yield return null;
                Snapshot("arrival-finished");Capture("arrival-finished.png");
                report.arrivalFinished=!game.Arriving&&game.arrivalAge>=ArrivalMotion.Duration-.01f&&
                    game.Rules.Remaining<startRemaining-1.1f;
                Require(report.arrivalFinished,"Landing must complete while the combat timer continues.");
                // CaptureScreenshot is queued until rendering. Let this frame
                // finish before the accelerated transition queues wave-two.png.
                yield return null;
                game.Rules.Tick(game.Rules.Remaining);
                for(int i=0;i<20&&game.Rules.Phase!=RunPhase.Shop;i++)
                {
                    if(game.Rules.Phase==RunPhase.Loot)game.ResolveCrate(true);
                    else if(game.Rules.Phase==RunPhase.Upgrade)game.Rules.Choose(0);
                    else break;
                }
                Require(game.Rules.Phase==RunPhase.Shop,"Accelerated first intermission must reach shop.");
                game.AdvanceWave();
                // AdvanceWave stages logical state and new equipment views.
                // Observe the normal Update pose before sampling or capturing;
                // a same-frame fixture capture otherwise retains the old body
                // transform and the equipment's temporary creation position.
                yield return null;
                raw=game.playerPos+CombatGeometry.PlayerOriginOffset;
                report.secondWaveNoArrival=game.Rules.Phase==RunPhase.Playing&&game.Rules.Wave==2&&
                    !game.Arriving&&raw.sqrMagnitude<.0001f&&Mathf.Abs(game.playerPos.y-.24f)<.01f;
                Vector2 secondMount=ArrivalWeaponMotion.TransformMount(game.playerPos,
                    ActorPresentation.WeaponMount(game.Rules.Inventory.Count,0),game.arrivalAge);
                var secondView=game.equipmentViews[0];
                report.secondWaveBodyError=Vector2.Distance(game.player.transform.position,game.playerPos);
                report.secondWaveWeaponMountDistance=secondView!=null?
                    Vector2.Distance(secondView.transform.position,secondMount):-1;
                report.secondWaveRenderedAligned=report.secondWaveBodyError<.01f&&secondView!=null&&
                    report.secondWaveWeaponMountDistance<=ActorPresentation.WeaponPivot(game.Rules.Inventory[0].id)+.03f;
                Snapshot("wave-two");Capture("wave-two.png");
                Require(report.secondWaveNoArrival,"Later wave starts at raw origin zero without replaying arrival.");
                Require(report.secondWaveRenderedAligned,
                    $"Later-wave rendering must follow logical state (body error {report.secondWaveBodyError:0.###}, weapon mount distance {report.secondWaveWeaponMountDistance:0.###}).");
                yield return new WaitForSecondsRealtime(.45f);
            }
            void State(GamepadState state)=>InputSystem.QueueStateEvent(pad,state);
            void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));
            void Snapshot(string stage)
            {
                Vector2 raw=game.playerPos+CombatGeometry.PlayerOriginOffset;
                Vector2 mount=ArrivalWeaponMotion.TransformMount(game.playerPos,
                    ActorPresentation.WeaponMount(game.Rules.Inventory.Count,0),game.arrivalAge);
                samples.Add(new ArrivalSample{stage=stage,phase=game.Rules.Phase.ToString(),age=game.arrivalAge,
                    remaining=game.Rules.Remaining,elapsed=game.elapsed,weaponClock=game.weaponClocks[0],
                    logicalX=game.playerPos.x,logicalY=game.playerPos.y,rawOriginX=raw.x,rawOriginY=raw.y,
                    renderedBodyX=game.player.transform.position.x,renderedBodyY=game.player.transform.position.y,
                    renderedWeaponX=game.equipmentViews[0]?.transform.position.x??0,renderedWeaponY=game.equipmentViews[0]?.transform.position.y??0,
                    expectedMountX=mount.x,expectedMountY=mount.y,shots=game.shotsFired,arriving=game.Arriving});
            }
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
