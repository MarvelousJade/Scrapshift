using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartNativeControllerCheck()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--controller-navigation-check");
            if(index<0||index+1>=args.Length)return;
            var fixture=new GameObject("Native controller acceptance fixture").AddComponent<NativeControllerCheck>();
            fixture.folder=args[index+1];
        }

        // Opt-in in-process input fixture. It never sends operating-system input.
        // Ordinary gameplay and the physical-controller logger do not create virtual devices.
        sealed class NativeControllerCheck:MonoBehaviour
        {
            public string folder;
            ArenaGame game;Gamepad pad;
            readonly List<NavigationSnapshot> snapshots=new List<NavigationSnapshot>();
            [Serializable] sealed class NavigationSnapshot
            {
                public string stage,phase,character,weapon;
                public int page,focus,weapons,materials,menuCues,steps;
                public Vector2 position,aim;
                public bool manualAim,secondaryStats;
            }
            [Serializable] sealed class NavigationReport
            {
                public bool passed;
                public string error,evidence="Virtual Gamepad events through actual player Update/OnGUI, with fixture-only IgnoreFocus to prevent focus changes resetting synthetic held buttons; not a physical-controller or ordinary-focus test. Intermission checks explicitly accelerate wave timers and grant fixture materials/XP/crates through production rules; not a full run.";
                public NavigationSnapshot[] snapshots;
            }
            IEnumerator Start()
            {
                Directory.CreateDirectory(folder);
                var checks=Run();string error=null;
                while(true)
                {
                    bool running=false;object next=null;
                    try{running=checks.MoveNext();if(running)next=checks.Current;}
                    catch(Exception ex){error=ex.ToString();}
                    if(!running)break;
                    yield return next;
                }
                var report=new NavigationReport{passed=error==null,error=error,snapshots=snapshots.ToArray()};
                File.WriteAllText(Path.Combine(folder,"navigation-check.json"),JsonUtility.ToJson(report,true));
                if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
                Application.Quit(report.passed?0:1);
            }
            IEnumerator Run()
            {
                yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null,"Game must finish initialization.");
                Require(Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use the isolated controller-check profile alongside this fixture.");
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);game.sound.SetMuted(true);
                // Synthetic events must survive desktop focus changes. This opt-in
                // fixture policy does not run in ordinary gameplay or hardware checks.
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                // Isolate only this player's input backend from physical devices.
                foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);
                pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();State(new GamepadState());
                yield return new WaitForSecondsRealtime(.15f);
                Snapshot("title");Require(game.Rules.Phase==RunPhase.Title&&game.menuPage==0,"Fixture begins at title.");
                yield return Press(GamepadButton.South);
                Require(game.menuPage==1,"First A enters character selection.");Snapshot("character");Capture("character.png");
                yield return Press(GamepadButton.South);
                Require(game.menuPage==2,"A without stick movement confirms the visibly selected character.");Snapshot("weapon");Capture("weapon.png");
                State(new GamepadState().WithButton(GamepadButton.East));yield return new WaitForSecondsRealtime(.2f);
                Require(game.menuPage==2,"B hold must not leave weapon selection before release.");
                State(new GamepadState());yield return new WaitForSecondsRealtime(.15f);
                Require(game.menuPage==1,"B release returns exactly one selection page.");Snapshot("back-release");
                yield return Press(GamepadButton.South);
                Require(game.menuPage==2,"A re-enters the selected character after Back and repaint.");
                yield return Press(GamepadButton.South);
                Require(game.menuPage==3,"A without stick movement confirms the visibly selected weapon.");Snapshot("difficulty");Capture("difficulty.png");
                yield return Press(GamepadButton.South);
                Require(game.Rules.Phase==RunPhase.Playing,"A on selected difficulty starts the run.");
                Require(game.Rules.Inventory.Count==1,"The first character starts with its one selected weapon.");
                Require(game.Rules.Scrap==RunRules.ReferenceStartingMaterials,"Ordinary run flow uses the supplied reference starting purse.");
                yield return new WaitForSecondsRealtime(1.4f);
                int menuBefore=game.sound.MenuFocusRequests,stepsBefore=game.sound.FootstepRequests;
                var positionBefore=game.playerPos;
                State(new GamepadState{leftStick=Vector2.up});yield return new WaitForSecondsRealtime(.9f);
                Require((game.playerPos-positionBefore).sqrMagnitude>.1f,"Actual gamepad input moves the player.");
                Require(game.sound.MenuFocusRequests==menuBefore,"Combat movement must not request menu focus sounds.");
                Require(game.sound.FootstepRequests>stepsBefore,"Combat movement requests distinct footstep cues.");
                Snapshot("moving-combat");Capture("moving-combat.png");
                State(new GamepadState());yield return new WaitForSecondsRealtime(.15f);
                int stoppedSteps=game.sound.FootstepRequests;
                yield return new WaitForSecondsRealtime(.35f);
                Require(game.sound.FootstepRequests==stoppedSteps,"Standing still must not request footsteps.");
                positionBefore=game.playerPos;menuBefore=game.sound.MenuFocusRequests;
                State(new GamepadState().WithButton(GamepadButton.DpadLeft));yield return new WaitForSecondsRealtime(.3f);
                Require(game.playerPos.x<positionBefore.x-.1f,"Actual D-pad input moves the player left.");
                Require(game.sound.MenuFocusRequests==menuBefore,"D-pad combat movement must not request menu cues.");
                Snapshot("dpad-combat");State(new GamepadState());yield return new WaitForSecondsRealtime(.1f);
                GameSettings.Set("aimPressed",true);GameSettings.Set("manualAim",false);
                State(new GamepadState{rightStick=Vector2.down});yield return new WaitForSecondsRealtime(.15f);
                Require(Vector2.Dot(CombatInput.AimDirection,Vector2.down)>.99f,"Right stick updates remembered manual direction.");
                Require(CombatInput.ManualAim(false,true,false,CombatInput.AimStick,CombatInput.UsingGamepad),"Right-stick press mode enables manual aim.");
                Snapshot("right-stick-manual");State(new GamepadState());yield return new WaitForSecondsRealtime(.1f);
                Require(Vector2.Dot(CombatInput.AimDirection,Vector2.down)>.99f,"Right-stick release retains aim history.");
                Require(!CombatInput.ManualAim(false,true,false,CombatInput.AimStick,CombatInput.UsingGamepad),"Right-stick release leaves press-only manual mode.");
                GameSettings.Set("aimPressed",false);
                yield return Press(GamepadButton.Start);
                Require(game.Rules.Phase==RunPhase.Paused,"Start release opens pause.");Snapshot("paused");Capture("paused.png");
                yield return Press(GamepadButton.South);
                Require(game.Rules.Phase==RunPhase.Playing,"Default pause focus resumes with A.");Snapshot("resumed");
                // Explicitly accelerated fixture states, using ordinary production
                // rule transitions. These checks do not represent a full campaign.
                game.Rules.Collect(100);game.Rules.Tick(game.Rules.Remaining);
                while(game.Rules.Phase==RunPhase.Upgrade)game.Rules.Choose(0);
                Require(game.Rules.Phase==RunPhase.Shop,"Accelerated wave opens a real shop.");
                yield return new WaitForSecondsRealtime(.2f);
                var offers=(ShopOffer[])game.Rules.Shop.Clone();int money=game.Rules.Scrap;
                yield return PauseAndReturn(RunPhase.Shop,"fixture-shop");
                Require(game.Rules.Scrap==money,"Paused shop must not spend materials.");
                for(int i=0;i<offers.Length;i++)Require(ReferenceEquals(offers[i],game.Rules.Shop[i]),"Shop pause retains offer objects.");
                bool secondary=game.secondaryStats;
                yield return Press(GamepadButton.RightShoulder);
                Require(game.secondaryStats!=secondary,"Right shoulder changes the actual statistics tab.");
                yield return Press(GamepadButton.LeftShoulder);
                Require(game.secondaryStats==secondary,"Left shoulder restores statistics tab.");
                int lockIndex=-1;
                for(int i=0;i<game.Rules.Shop.Length;i++)if(game.Rules.CanLock(i)){lockIndex=i;break;}
                Require(lockIndex>=0,"The natural shop must contain a lockable offer.");
                int lockFocus=-1;
                for(int i=0;i<game.menuNavigationButtons.Count;i++)
                    if(CombatInput.OfferAtFocus(game.controllerOfferRects,game.menuNavigationButtons[i])==lockIndex){lockFocus=i;break;}
                Require(lockFocus>=0,"A rendered child control must map to the eligible offer.");
                game.menuNavigationActive=true;game.menuFocus=lockFocus;
                var locked=game.Rules.Shop[lockIndex];bool beforeLock=locked.locked;
                yield return Press(GamepadButton.West);
                Require(locked.locked!=beforeLock,"X changes the focused natural offer's lock state.");
                if(locked.locked){yield return Press(GamepadButton.West);Require(!locked.locked,"Second X unlocks the same offer.");}
                Require(game.Rules.CanReroll,"Fixture materials allow a shop reroll.");
                GameSettings.Set("holdReroll",true);int rerolls=game.Rules.ShopRerollCount;
                State(new GamepadState().WithButton(GamepadButton.North));yield return new WaitForSecondsRealtime(.2f);
                State(new GamepadState());yield return new WaitForSecondsRealtime(.1f);
                Require(game.Rules.ShopRerollCount==rerolls,"Releasing Y before .5 seconds cancels reroll.");
                State(new GamepadState().WithButton(GamepadButton.North));yield return new WaitForSecondsRealtime(.7f);
                Require(game.Rules.ShopRerollCount==rerolls+1,"Holding Y fires one shop reroll.");
                yield return new WaitForSecondsRealtime(.6f);
                Require(game.Rules.ShopRerollCount==rerolls+1,"Continued Y hold does not reroll twice.");
                State(new GamepadState());yield return new WaitForSecondsRealtime(.1f);Snapshot("fixture-shop-shortcuts");
                game.AdvanceWave();game.Rules.Collect(100);game.Rules.CollectConsumable(ConsumableKind.Crate);game.Rules.Tick(game.Rules.Remaining);
                Require(game.Rules.Phase==RunPhase.Loot,"Fixture crate enters actual Item found state.");
                yield return new WaitForSecondsRealtime(.2f);
                var reward=game.Rules.CrateItem;int pending=game.Rules.PendingCrates;
                yield return PauseAndReturn(RunPhase.Loot,"fixture-loot");
                Require(ReferenceEquals(reward,game.Rules.CrateItem)&&game.Rules.PendingCrates==pending,"Pause retains the exact crate reward.");
                money=game.Rules.Scrap;int recycle=game.Rules.CrateRecycleValue;
                yield return Press(GamepadButton.North);
                Require(game.Rules.PendingCrates==pending-1&&game.Rules.Scrap==money+recycle,"Y recycles the visible crate through the runtime shortcut.");
                Require(game.Rules.Phase==RunPhase.Upgrade,"Queued fixture XP opens upgrades after crate recycle.");
                yield return new WaitForSecondsRealtime(.2f);
                var upgrades=game.Rules.Offers;int levels=game.Rules.PendingLevels;
                yield return PauseAndReturn(RunPhase.Upgrade,"fixture-upgrade");
                Require(ReferenceEquals(upgrades,game.Rules.Offers)&&game.Rules.PendingLevels==levels,"Upgrade pause preserves the offered choices and pending levels.");
            }
            IEnumerator PauseAndReturn(RunPhase phase,string stage)
            {
                float remaining=game.Rules.Remaining;
                yield return Press(GamepadButton.Start);
                Require(game.Rules.Phase==RunPhase.Paused,"Start release pauses "+phase);
                game.Rules.Tick(1);Require(game.Rules.Remaining==remaining,"Paused rules do not advance the timer.");
                Snapshot(stage+"-paused");
                State(new GamepadState().WithButton(GamepadButton.East));yield return new WaitForSecondsRealtime(.15f);
                Require(game.Rules.Phase==RunPhase.Paused,"B hold keeps "+phase+" paused until release.");
                State(new GamepadState());yield return new WaitForSecondsRealtime(.15f);
                Require(game.Rules.Phase==phase,"B release restores "+phase);Snapshot(stage+"-resumed");
            }
            IEnumerator Press(GamepadButton button)
            {
                State(new GamepadState().WithButton(button));yield return new WaitForSecondsRealtime(.08f);
                State(new GamepadState());yield return new WaitForSecondsRealtime(.15f);
            }
            void State(GamepadState state)=>InputSystem.QueueStateEvent(pad,state);
            void Snapshot(string stage)=>snapshots.Add(new NavigationSnapshot{stage=stage,phase=game.Rules.Phase.ToString(),page=game.menuPage,focus=game.menuFocus,
                character=game.SelectedCharacter.id,weapon=game.SelectedWeapon?.id,weapons=game.Rules.Inventory.Count,materials=game.Rules.Scrap,position=game.playerPos,
                menuCues=game.sound.MenuFocusRequests,steps=game.sound.FootstepRequests,aim=CombatInput.AimDirection,secondaryStats=game.secondaryStats,
                manualAim=CombatInput.ManualAim(GameSettings.Flag("manualAim",false),GameSettings.Flag("aimPressed",false),false,CombatInput.AimStick,CombatInput.UsingGamepad)});
            void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));
            static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        }
    }
}
