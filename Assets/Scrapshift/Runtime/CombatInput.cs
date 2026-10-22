using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
namespace Scrapshift
{
    public static class CombatInput
    {
        // Reference Utils.is_manual_aim: mouse-press mode takes priority and is independent.
        public static bool ManualAim(bool always,bool onPress,bool pressed)=>onPress?pressed:always;
        public static bool ManualAim(bool always,bool onPress,bool pressed,Vector2 stick)=>ManualAim(always,onPress,pressed||stick.sqrMagnitude>.05f);
        public static bool ManualAim(bool always,bool onPress,bool pressed,Vector2 stick,bool usingGamepad)=>ManualAim(always,onPress,pressed,usingGamepad?stick:Vector2.zero);
        // PlayerMovementBehavior.MIN_MOVE_DIST is 20 reference pixels, or .2 world units.
        // The left mouse button stops mouse-only movement; movement otherwise follows the cursor.
        public static Vector2 MouseMovement(Vector2 cursorDelta,bool pressed)=>pressed||Mathf.Abs(cursorDelta.x)<.2f&&Mathf.Abs(cursorDelta.y)<.2f?Vector2.zero:cursorDelta;
        public static Vector2 Stick(Vector2 raw)
        {
            float length=raw.magnitude;
            return length<=.5f?Vector2.zero:raw/length*Mathf.Min(1,(length-.5f)/.5f);
        }
        public static Vector2 Movement(Vector2 buttons,Vector2 analog)=>analog.sqrMagnitude>buttons.sqrMagnitude?analog:buttons;
        // Godot combines all events belonging to each button action before it
        // subtracts opposing actions and compares that vector with analog input.
        public static Vector2 ButtonMovement(Vector2 negative,Vector2 positive,Vector2 dpad)=>Vector2.ClampMagnitude(new Vector2(
            Mathf.Max(positive.x,Mathf.Max(0,dpad.x))-Mathf.Max(negative.x,Mathf.Max(0,-dpad.x)),
            Mathf.Max(positive.y,Mathf.Max(0,dpad.y))-Mathf.Max(negative.y,Mathf.Max(0,-dpad.y))),1);
        public static Vector2 PlayerMovement(Vector2 negative,Vector2 positive,Vector2 dpad,Vector2 rawStick)=>Movement(ButtonMovement(negative,positive,dpad),Stick(rawStick));
        public static Vector2 Velocity(Vector2 movement,float speed)=>movement.normalized*speed;
        public static bool ShouldRotateManual(bool melee,bool shooting)=>!melee||!shooting;
        public static Vector2 RememberAim(Vector2 previous,Vector2 stick,bool manual,bool usingGamepad)=>manual&&usingGamepad&&stick!=Vector2.zero?stick.normalized:previous;
        public static bool GamepadMotionEvent(Vector2 previous,Vector2 current)=>previous.x!=current.x&&Mathf.Abs(current.x)>.5f||previous.y!=current.y&&Mathf.Abs(current.y)>.5f;
        // Historical polling helper retained for regression cases. Runtime device
        // ownership now follows OnDeviceEvent's ordered Input System event stream.
        public static bool DeviceAfterEvents(bool previous,bool gamepadEvent,bool pointerOrKeyboardEvent)=>pointerOrKeyboardEvent?false:gamepadEvent?true:previous;
        public static bool PreviewFocus(bool navigationActive,bool navigationFocused,bool hovered)=>navigationActive?navigationFocused:hovered;
        public static int Navigate(Rect[] buttons,int current,Vector2 direction)
        {
            if(buttons.Length==0)return -1;
            if(current<0||current>=buttons.Length)return 0;
            int next=current;float best=float.PositiveInfinity;
            for(int i=0;i<buttons.Length;i++)
            {
                Vector2 delta=buttons[i].center-buttons[current].center;
                float forward=Vector2.Dot(delta,direction),side=Mathf.Abs(delta.x*direction.y-delta.y*direction.x);
                if(forward<=1)continue;
                float score=delta.magnitude+side*3;
                if(score<best){best=score;next=i;}
            }
            return next;
        }
        public static bool UsingGamepad { get; private set; }
        public static Vector2 AimStick { get; private set; }
        public static Vector2 AimDirection { get; private set; }=Vector2.right;
        public static Vector2 GamepadMovement { get; private set; }
        public static Vector2 NavigationVector { get; private set; }
        public static Vector2 MoveRaw { get;private set; }
        public static Vector2 AimRaw { get;private set; }
        public static Vector2 DpadRaw { get;private set; }
        public static string LastInputDevice {get;private set;}="None";
        public static Gamepad CurrentGamepad=>Gamepad.current!=null&&Gamepad.current.added&&Gamepad.current.enabled?Gamepad.current:null;
        public static bool SelectPressed=>CurrentGamepad?.buttonWest.wasPressedThisFrame??false;
        public static bool InfoPressed=>CurrentGamepad?.buttonNorth.wasPressedThisFrame??false;
        public static bool InfoHeld=>CurrentGamepad?.buttonNorth.isPressed??false;
        public static int TabStep=>CurrentGamepad==null?0:
            (CurrentGamepad.rightShoulder.wasPressedThisFrame||CurrentGamepad.rightTrigger.wasPressedThisFrame?1:0)-
            (CurrentGamepad.leftShoulder.wasPressedThisFrame||CurrentGamepad.leftTrigger.wasPressedThisFrame?1:0);
        // RerollButton._load_button: one activation after .5 seconds; release cancels.
        public sealed class RerollHold
        {
            float elapsed;bool fired,armed;
            public float Progress=>fired?0:Mathf.Clamp01(elapsed/.5f);
            public bool Tick(bool pressed,bool held,bool eligible,bool requireHold,float dt)
            {
                if(!held||!eligible){Reset();return false;}
                if(pressed)armed=true;
                if(fired||!armed)return false;
                elapsed+=Mathf.Max(0,dt);
                if(requireHold&&elapsed<.5f)return false;
                fired=true;return true;
            }
            public void Reset(){elapsed=0;fired=armed=false;}
        }
        public static Vector2 MenuDirection(Vector2 raw)=>Mathf.Abs(raw.x)>.5f?new Vector2(Mathf.Sign(raw.x),0):Mathf.Abs(raw.y)>.5f?new Vector2(0,Mathf.Sign(raw.y)):Vector2.zero;
        public static int OfferAtFocus(Rect[] cards,Rect focus)
        {for(int i=0;i<cards.Length;i++)if(cards[i].Contains(focus.center))return i;return -1;}
        // Return 2 for theft, 1 for lock, 0 for no eligible offer.
        public static int SelectShopOffer(RunRules run,int index)
        {
            if(run.Phase!=RunPhase.Shop||index<0||index>=run.Shop.Length||run.Shop[index]==null)return 0;
            if(run.StealsRemaining>0&&run.CanSteal(index)&&run.Steal(index))return 2;
            if(!run.CanLock(index))return 0;
            run.ToggleLock(index);return 1;
        }
        public static bool ConfirmPressed=>CurrentGamepad?.buttonSouth.wasPressedThisFrame??false;
        public static bool CancelPressed=>CurrentGamepad?.buttonEast.wasPressedThisFrame??false;
        public static bool CancelReleased=>CurrentGamepad?.buttonEast.wasReleasedThisFrame??false;
        public static bool PauseReleased=>CurrentGamepad?.startButton.wasReleasedThisFrame??false;
        public static int ControllerButtonMask
        {
            get
            {
                var pad=CurrentGamepad;if(pad==null)return 0;
                var buttons=new[]{pad.buttonSouth,pad.buttonEast,pad.buttonWest,pad.buttonNorth,pad.leftShoulder,pad.rightShoulder,pad.selectButton,pad.startButton,pad.leftStickButton,pad.rightStickButton};
                int mask=0;for(int i=0;i<buttons.Length;i++)if(buttons[i].isPressed)mask|=1<<i;
                if(pad.leftTrigger.isPressed)mask|=1<<10;if(pad.rightTrigger.isPressed)mask|=1<<11;
                if(pad.dpad.up.isPressed)mask|=1<<12;if(pad.dpad.down.isPressed)mask|=1<<13;
                if(pad.dpad.left.isPressed)mask|=1<<14;if(pad.dpad.right.isPressed)mask|=1<<15;
                return mask;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void InitializeDevices()
        {
            InputSystem.onEvent-=OnDeviceEvent;InputSystem.onEvent+=OnDeviceEvent;
            InputSystem.onDeviceChange-=OnDeviceChange;InputSystem.onDeviceChange+=OnDeviceChange;
            UsingGamepad=false;AimDirection=Vector2.right;AimStick=GamepadMovement=NavigationVector=MoveRaw=AimRaw=DpadRaw=Vector2.zero;
            LastInputDevice="None";
        }
        static void OnDeviceChange(InputDevice device,InputDeviceChange change)
        {
            if(!(device is Gamepad)||change!=InputDeviceChange.Removed&&change!=InputDeviceChange.Disconnected&&change!=InputDeviceChange.Disabled)return;
            UsingGamepad=false;AimStick=GamepadMovement=NavigationVector=MoveRaw=AimRaw=DpadRaw=Vector2.zero;
        }
        static bool MotionEvent(InputControl<Vector2> control,InputEventPtr inputEvent)
            =>control.ReadUnprocessedValueFromEvent(inputEvent,out var value)&&GamepadMotionEvent(control.ReadUnprocessedValue(),value);
        static void OnDeviceEvent(InputEventPtr inputEvent,InputDevice device)
        {
            if(!inputEvent.IsA<StateEvent>()&&!inputEvent.IsA<DeltaStateEvent>())return;
            if(device is Gamepad pad)
            {
                bool valid=MotionEvent(pad.leftStick,inputEvent)||MotionEvent(pad.rightStick,inputEvent)||MotionEvent(pad.dpad,inputEvent);
                if(!valid)foreach(var control in pad.allControls)
                {
                    if(!(control is ButtonControl button)||control.parent is StickControl)continue;
                    if(button.ReadValueFromEvent(inputEvent,out var value)&&(value>=button.pressPointOrDefault)!=button.isPressed){valid=true;break;}
                }
                if(!valid)return;
                UsingGamepad=true;
            }
            else if(device is Keyboard||device is Mouse)
            {
                bool changed=false;
                foreach(var control in inputEvent.EnumerateChangedControls(device))if(!control.noisy){changed=true;break;}
                if(!changed)return;
                UsingGamepad=false;
            }
            else return;
            // onEvent sees the actual ordered event stream, before state is applied.
            // A steady held stick no longer overrides a later mouse/keyboard event.
            LastInputDevice=device.displayName+" ["+device.layout+"]";
        }
        public static void ReadController(bool alwaysManual,bool aimOnPress,bool mousePressed)
        {
            var pad=CurrentGamepad;
            MoveRaw=pad?.leftStick.ReadUnprocessedValue()??Vector2.zero;
            AimRaw=pad?.rightStick.ReadUnprocessedValue()??Vector2.zero;
            // DpadControl's vector normalizes diagonals. Preserve the individual
            // digital action strengths until keyboard and D-pad events merge.
            DpadRaw=pad==null?Vector2.zero:new Vector2((pad.dpad.right.isPressed?1:0)-(pad.dpad.left.isPressed?1:0),(pad.dpad.up.isPressed?1:0)-(pad.dpad.down.isPressed?1:0));
            // Avoid Input System's default stick processors: the reference radial
            // deadzone is applied exactly once by Stick.
            GamepadMovement=Movement(Vector2.ClampMagnitude(DpadRaw,1),Stick(MoveRaw));
            AimStick=Stick(AimRaw);NavigationVector=Movement(DpadRaw,MoveRaw);
            bool manual=ManualAim(alwaysManual,aimOnPress,mousePressed,AimStick,UsingGamepad);
            AimDirection=RememberAim(AimDirection,AimStick,manual,UsingGamepad&&GameSettings.Flag("gamepadMove"));
        }
        public static void Poll()=>ReadController(GameSettings.Flag("manualAim",false),GameSettings.Flag("aimPressed",false),Input.GetMouseButton(0));
    }
}
