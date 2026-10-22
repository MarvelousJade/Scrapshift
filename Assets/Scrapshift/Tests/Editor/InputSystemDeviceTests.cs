using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Scrapshift.Tests
{
    // Queue real Input System state events into virtual devices. These exercise
    // device layouts, event callbacks and runtime reads without claiming hardware validation.
    public sealed class InputSystemDeviceTests : InputTestFixture
    {
        Gamepad pad;
        Mouse mouse;
        Keyboard keyboard;
        public override void Setup()
        {
            base.Setup();
            CombatInput.InitializeDevices();
            pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();
            mouse=InputSystem.AddDevice<Mouse>();keyboard=InputSystem.AddDevice<Keyboard>();
        }
        public override void TearDown()
        {
            if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
            if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);
            if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);
            base.TearDown();
            CombatInput.InitializeDevices();
        }
        void State(GamepadState state)
        {InputSystem.QueueStateEvent(pad,state);InputSystem.Update();}
        [Test] public void DpadDiagonalRetainsDigitalStrengthUntilKeyboardActionsMerge()
        {
            State(new GamepadState().WithButton(GamepadButton.DpadUp).WithButton(GamepadButton.DpadLeft));
            CombatInput.ReadController(false,false,false);
            Assert.AreEqual(new Vector2(-1,1),CombatInput.DpadRaw);
            Assert.AreEqual(Vector2.up,CombatInput.PlayerMovement(Vector2.zero,Vector2.right,CombatInput.DpadRaw,Vector2.zero));
        }
        [Test] public void CancelReleaseOccursOnceAfterEastButtonIsLetGo()
        {
            State(new GamepadState());Assert.IsFalse(CombatInput.CancelReleased);
            State(new GamepadState().WithButton(GamepadButton.East));Assert.IsTrue(CombatInput.CancelPressed);Assert.IsFalse(CombatInput.CancelReleased);
            State(new GamepadState().WithButton(GamepadButton.East));Assert.IsFalse(CombatInput.CancelReleased);
            State(new GamepadState());Assert.IsTrue(CombatInput.CancelReleased);
            State(new GamepadState());Assert.IsFalse(CombatInput.CancelReleased);
        }
        [Test] public void SticksAndDpadReadFromDeviceWithReferenceDeadzoneAppliedOnce()
        {
            State(new GamepadState{leftStick=new Vector2(.6f,0),rightStick=new Vector2(0,.8f)});
            CombatInput.ReadController(true,false,false);
            Assert.That(CombatInput.MoveRaw.x,Is.EqualTo(.6f).Within(.00001f));
            Assert.That(CombatInput.GamepadMovement.x,Is.EqualTo(.2f).Within(.00001f));
            Assert.That(CombatInput.AimStick.y,Is.EqualTo(.6f).Within(.00001f));
            Assert.AreEqual(Vector2.up,CombatInput.AimDirection);
            State(new GamepadState().WithButton(GamepadButton.DpadLeft));
            CombatInput.ReadController(false,false,false);
            Assert.AreEqual(Vector2.left,CombatInput.DpadRaw);
            Assert.AreEqual(Vector2.left,CombatInput.GamepadMovement);
            Assert.AreEqual(Vector2.left,CombatInput.NavigationVector);
        }
        [Test] public void SouthEastAndStartUseActualDevicePressAndReleaseState()
        {
            // Establish a previous frame and register edge tracking before the first transition.
            State(new GamepadState());
            Assert.IsFalse(CombatInput.ConfirmPressed);
            Assert.IsFalse(CombatInput.CancelPressed);
            Assert.IsFalse(CombatInput.PauseReleased);
            State(new GamepadState().WithButton(GamepadButton.South).WithButton(GamepadButton.East).WithButton(GamepadButton.Start));
            Assert.AreSame(pad,CombatInput.CurrentGamepad);
            Assert.IsTrue(pad.buttonSouth.isPressed,"Queued south state must reach the actual device.");
            Assert.IsTrue(pad.wasUpdatedThisFrame,"The queued event must advance this device in the current input update.");
            Assert.IsTrue(CombatInput.ConfirmPressed,"South rising edge");Assert.IsTrue(CombatInput.CancelPressed,"East rising edge");
            Assert.IsFalse(CombatInput.PauseReleased);
            Assert.AreEqual(1|2|128,CombatInput.ControllerButtonMask);
            State(new GamepadState());
            Assert.IsFalse(CombatInput.ConfirmPressed);Assert.IsFalse(CombatInput.CancelPressed);
            Assert.IsTrue(CombatInput.PauseReleased);Assert.AreEqual(0,CombatInput.ControllerButtonMask);
        }
        [Test] public void OrderedMouseAndGamepadEventsSelectLastDeviceEvenWithinOneUpdate()
        {
            InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(.8f,0)});
            InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(10,5)});
            InputSystem.Update();Assert.IsFalse(CombatInput.UsingGamepad);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(20,5)});
            InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(.9f,0)});
            InputSystem.Update();Assert.IsTrue(CombatInput.UsingGamepad);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(30,5)});
            InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(.9f,0)});
            InputSystem.Update();Assert.IsFalse(CombatInput.UsingGamepad,"A steady held stick is not a new input event.");
        }
        [Test] public void KeyboardEventTakesAimFromControllerAndReleaseKeepsAimHistory()
        {
            State(new GamepadState{rightStick=Vector2.left});CombatInput.ReadController(false,false,false);
            Assert.AreEqual(Vector2.right,CombatInput.AimDirection,"Automatic aiming must not overwrite manual aim history.");
            CombatInput.ReadController(false,true,false);Assert.AreEqual(Vector2.left,CombatInput.AimDirection);
            State(new GamepadState());CombatInput.ReadController(true,false,false);
            Assert.AreEqual(Vector2.left,CombatInput.AimDirection);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));InputSystem.Update();
            Assert.IsFalse(CombatInput.UsingGamepad);
        }
        [Test] public void WestNorthAndBothShoulderOrTriggerPairsReadActualDeviceEdges()
        {
            State(new GamepadState());
            Assert.IsFalse(CombatInput.SelectPressed);Assert.IsFalse(CombatInput.InfoPressed);Assert.AreEqual(0,CombatInput.TabStep);
            State(new GamepadState().WithButton(GamepadButton.West).WithButton(GamepadButton.North).WithButton(GamepadButton.LeftShoulder));
            Assert.IsTrue(CombatInput.SelectPressed);Assert.IsTrue(CombatInput.InfoPressed);Assert.IsTrue(CombatInput.InfoHeld);Assert.AreEqual(-1,CombatInput.TabStep);
            State(new GamepadState().WithButton(GamepadButton.North));
            Assert.IsFalse(CombatInput.InfoPressed);Assert.IsTrue(CombatInput.InfoHeld);Assert.AreEqual(0,CombatInput.TabStep);
            State(new GamepadState{rightTrigger=1});Assert.AreEqual(1,CombatInput.TabStep);Assert.IsFalse(CombatInput.InfoHeld);
            State(new GamepadState{leftTrigger=1});Assert.AreEqual(-1,CombatInput.TabStep);
            State(new GamepadState().WithButton(GamepadButton.RightShoulder));Assert.AreEqual(1,CombatInput.TabStep);
            InputSystem.DisableDevice(pad);
            Assert.AreEqual(0,CombatInput.TabStep);Assert.IsFalse(CombatInput.SelectPressed);Assert.IsFalse(CombatInput.InfoHeld);
        }
        [Test] public void DiagonalDeviceInputBelowEachAxisThresholdDoesNotNavigateMenus()
        {
            State(new GamepadState{leftStick=new Vector2(.4f,.4f)});CombatInput.ReadController(false,false,false);
            Assert.AreEqual(Vector2.zero,CombatInput.MenuDirection(CombatInput.NavigationVector));
            State(new GamepadState{leftStick=new Vector2(.51f,.4f)});CombatInput.ReadController(false,false,false);
            Assert.AreEqual(Vector2.right,CombatInput.MenuDirection(CombatInput.NavigationVector));
        }
        const System.Reflection.BindingFlags Hidden=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        static object NavCall(ArenaGame game,string method,params object[] args)=>typeof(ArenaGame).GetMethod(method,Hidden).Invoke(game,args);
        static void NavSet(ArenaGame game,string field,object value)=>typeof(ArenaGame).GetField(field,Hidden).SetValue(game,value);
        static T NavGet<T>(ArenaGame game,string field)=>(T)typeof(ArenaGame).GetField(field,Hidden).GetValue(game);
        static void NavigationFrame(ArenaGame game,params Rect[] controls)
        {
            NavCall(game,"BeginNavigationFrame",true);
            for(int i=0;i<controls.Length;i++)NavCall(game,"RegisterNavigationControl",controls[i],i==controls.Length-1,true);
            NavCall(game,"CompleteNavigationFrame",true);
        }
        [Test] public void ActualMenuLifecycleDefaultsToSelectedCardAndNeverCommitsOutgoingBackFocus()
        {
            var obj=new GameObject("Controller menu lifecycle");var game=obj.AddComponent<ArenaGame>();
            try
            {
                NavSet(game,"menuNavigationActive",true);
                NavigationFrame(game,new Rect(30,545,200,65));
                NavSet(game,"menuPage",1); // Start callback changes page during OnGUI.
                NavCall(game,"CompleteNavigationFrame",true);
                Assert.AreEqual(-1,NavGet<int>(game,"menuFocus"));
                Assert.AreEqual(0,NavGet<System.Collections.Generic.List<Rect>>(game,"menuNavigationButtons").Count);
                var back=new Rect(24,24,178,44);var random=new Rect(57,603,70,80);var selected=new Rect(135,603,70,80);
                NavigationFrame(game,back,random,selected);
                Assert.AreEqual(2,NavGet<int>(game,"menuFocus"),"Selected character, not Back, is the first controller target.");
                Assert.IsFalse((bool)NavCall(game,"RegisterNavigationControl",back,false,false));
                Assert.IsTrue((bool)NavCall(game,"RegisterNavigationControl",selected,true,false));
                State(new GamepadState());NavCall(game,"TickMenuNavigation");
                State(new GamepadState().WithButton(GamepadButton.South));NavCall(game,"TickMenuNavigation");
                Assert.IsTrue(NavGet<bool>(game,"menuActivate"));Assert.AreEqual(2,NavGet<int>(game,"menuFocus"));
                foreach(int page in new[]{2,3})
                {
                    NavSet(game,"menuPage",page);NavigationFrame(game,back,random,selected);
                    Assert.AreEqual(2,NavGet<int>(game,"menuFocus"),"Weapon/danger selection must retain the selected card as the activation target.");
                }
            }
            finally{Object.DestroyImmediate(obj);}
        }
        [Test] public void ConfirmBeforeFirstMenuRepaintWaitsForSelectedControlAndDoesNotRepeatWhileHeld()
        {
            var obj=new GameObject("Controller pending confirmation");var game=obj.AddComponent<ArenaGame>();
            try
            {
                NavSet(game,"menuPage",1);
                State(new GamepadState());NavCall(game,"TickMenuNavigation");
                State(new GamepadState().WithButton(GamepadButton.South));NavCall(game,"TickMenuNavigation");
                Assert.IsFalse(NavGet<bool>(game,"menuActivate"));
                NavigationFrame(game,new Rect(24,24,178,44),new Rect(135,603,70,80));
                State(new GamepadState().WithButton(GamepadButton.South));NavCall(game,"TickMenuNavigation");
                Assert.IsTrue(NavGet<bool>(game,"menuActivate"));Assert.AreEqual(1,NavGet<int>(game,"menuFocus"));
                State(new GamepadState().WithButton(GamepadButton.South));NavCall(game,"TickMenuNavigation");
                Assert.IsFalse(NavGet<bool>(game,"menuActivate"));
            }
            finally{Object.DestroyImmediate(obj);}
        }
        [Test] public void PauseDefaultsToResumeDespiteStatsTabsBeingRegisteredBeforeIt()
        {
            var obj=new GameObject("Controller pause default");var game=obj.AddComponent<ArenaGame>();
            try
            {
                game.Rules.Start();game.Rules.Pause();NavSet(game,"menuNavigationActive",true);
                var primary=new Rect(1125,94,133,35);var secondary=new Rect(1266,94,135,35);var resume=new Rect(40,203,515,55);var restart=new Rect(40,278,515,55);
                NavCall(game,"BeginNavigationFrame",true);
                NavCall(game,"RegisterNavigationControl",primary,false,true);
                NavCall(game,"RegisterNavigationControl",secondary,false,true);
                NavCall(game,"RegisterNavigationControl",resume,true,true);
                NavCall(game,"RegisterNavigationControl",restart,false,true);
                NavCall(game,"CompleteNavigationFrame",true);
                Assert.AreEqual(2,NavGet<int>(game,"menuFocus"));
                State(new GamepadState());NavCall(game,"TickMenuNavigation");
                State(new GamepadState().WithButton(GamepadButton.South));NavCall(game,"TickMenuNavigation");
                Assert.IsTrue(NavGet<bool>(game,"menuActivate"));
                Assert.IsTrue((bool)NavCall(game,"RegisterNavigationControl",resume,true,false));
                Assert.IsFalse((bool)NavCall(game,"RegisterNavigationControl",primary,false,false));
            }
            finally{Object.DestroyImmediate(obj);}
        }
        [Test] public void MouseSelectionThenControllerTakeoverActivatesCurrentPreferredCardRatherThanStaleFocus()
        {
            var obj=new GameObject("Controller mouse takeover");var game=obj.AddComponent<ArenaGame>();
            try
            {
                NavSet(game,"menuPage",1);NavSet(game,"menuNavigationActive",true);
                var back=new Rect(24,24,178,44);var oldCard=new Rect(135,603,70,80);var mouseCard=new Rect(213,603,70,80);
                NavigationFrame(game,back,oldCard);
                Assert.AreEqual(1,NavGet<int>(game,"menuFocus"));
                NavSet(game,"menuNavigationActive",false);
                NavigationFrame(game,back,oldCard,mouseCard); // Mouse preview changed the preferred selection.
                Assert.AreEqual(1,NavGet<int>(game,"menuFocus"),"The old navigation index remains valid before takeover.");
                State(new GamepadState());NavCall(game,"TickMenuNavigation");
                State(new GamepadState().WithButton(GamepadButton.South));NavCall(game,"TickMenuNavigation");
                Assert.AreEqual(2,NavGet<int>(game,"menuFocus"));Assert.IsTrue(NavGet<bool>(game,"menuActivate"));
                NavSet(game,"menuNavigationActive",false);NavSet(game,"menuFocus",1);
                State(new GamepadState{leftStick=Vector2.left});CombatInput.ReadController(false,false,false);NavCall(game,"TickMenuNavigation");
                Assert.AreEqual(1,NavGet<int>(game,"menuFocus"),"First direction moves left from the mouse-selected card, rather than from stale Back/old focus.");
            }
            finally{Object.DestroyImmediate(obj);}
        }
        [Test] public void ActualBlockedMenuRepeatAndCombatStickMovementDoNotRequestFocusAudio()
        {
            var obj=new GameObject("Controller audio lifecycle");var game=obj.AddComponent<ArenaGame>();var audio=obj.AddComponent<GameAudio>();
            try
            {
                NavSet(game,"sound",audio);NavSet(game,"menuNavigationActive",true);
                // Last control is the default: a leftward move reaches the first control.
                NavigationFrame(game,new Rect(0,0,100,60),new Rect(150,0,100,60));
                State(new GamepadState{leftStick=Vector2.left});CombatInput.ReadController(false,false,false);NavCall(game,"TickMenuNavigation");
                Assert.AreEqual(0,NavGet<int>(game,"menuFocus"));Assert.AreEqual(1,audio.MenuFocusRequests);
                NavSet(game,"nextNavigation",-1f);NavCall(game,"TickMenuNavigation");
                Assert.AreEqual(1,audio.MenuFocusRequests,"A held direction at a boundary did not change focus.");
                game.Rules.Start();
                State(new GamepadState{leftStick=Vector2.right});CombatInput.ReadController(false,false,false);NavCall(game,"TickMenuNavigation");
                Assert.AreEqual(1,audio.MenuFocusRequests);Assert.IsFalse(NavGet<bool>(game,"menuActivate"));
                Assert.IsFalse((bool)NavCall(game,"RegisterNavigationControl",new Rect(0,0,100,60),true,true));
            }
            finally{Object.DestroyImmediate(obj);}
        }
        [Test] public void DisabledControllerClearsVectorsAndCannotLeaveAButtonHeld()
        {
            State(new GamepadState{leftStick=Vector2.up,rightStick=Vector2.left}.WithButton(GamepadButton.South));
            CombatInput.ReadController(true,false,false);Assert.AreEqual(Vector2.up,CombatInput.GamepadMovement);
            InputSystem.DisableDevice(pad);CombatInput.ReadController(true,false,false);
            Assert.AreEqual(Vector2.zero,CombatInput.MoveRaw);Assert.AreEqual(Vector2.zero,CombatInput.AimRaw);
            Assert.AreEqual(Vector2.zero,CombatInput.DpadRaw);Assert.AreEqual(Vector2.zero,CombatInput.GamepadMovement);
            Assert.AreEqual(0,CombatInput.ControllerButtonMask);Assert.IsFalse(CombatInput.ConfirmPressed);
        }
    }
}
