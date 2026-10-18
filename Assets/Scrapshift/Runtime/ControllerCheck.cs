using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        string controllerCheckPath;
        ControllerSample lastControllerSample;
        int controllerPlayingMenuCues,controllerPlayingStepCues;
        int controllerLastMenuCues,controllerLastStepCues;
        [Serializable] sealed class ControllerSample
        {
            public float seconds;
            public string[] devices;
            public string backend,lastInputDevice;
            public float[] axes;
            public int buttons,menuPage,menuFocus,shots,menuFocusCues,footstepCues,playingMenuFocusCues,playingFootstepCues;
            public string phase;
            public bool usingGamepad,manualAim,isolatedProfile;
            public Vector2 playerPosition,aimDirection;
        }
        // Settings and progression stay in memory in this opt-in fixture.
        // This logger observes Unity input; it never synthesizes input.
        void BeginControllerCheck(string folder)
        {
            controllerCheckPath=folder;Directory.CreateDirectory(folder);
            TickControllerCheck(true);
        }
        void TickControllerCheck(bool force=false)
        {
            if(controllerCheckPath==null||!force&&!Application.isFocused)return;
            int buttons=CombatInput.ControllerButtonMask;
            int menuCues=sound?.MenuFocusRequests??0,stepCues=sound?.FootstepRequests??0;
            if(Rules.Phase==RunPhase.Playing&&lastControllerSample?.phase=="Playing")
            {controllerPlayingMenuCues+=menuCues-controllerLastMenuCues;controllerPlayingStepCues+=stepCues-controllerLastStepCues;}
            controllerLastMenuCues=menuCues;controllerLastStepCues=stepCues;
            var sample=new ControllerSample
            {
                seconds=Time.realtimeSinceStartup,devices=Gamepad.all.Select(p=>p.displayName+" ["+p.layout+"] #"+p.deviceId).ToArray(),
                backend="Unity Input System 1.20.1",lastInputDevice=CombatInput.LastInputDevice,
                axes=new[]{CombatInput.MoveRaw.x,CombatInput.MoveRaw.y,CombatInput.AimRaw.x,CombatInput.AimRaw.y,CombatInput.DpadRaw.x,CombatInput.DpadRaw.y},
                menuFocusCues=menuCues,footstepCues=stepCues,playingMenuFocusCues=controllerPlayingMenuCues,playingFootstepCues=controllerPlayingStepCues,
                buttons=buttons,menuPage=menuPage,menuFocus=menuFocus,shots=shotsFired,phase=Rules.Phase.ToString(),usingGamepad=CombatInput.UsingGamepad,
                manualAim=CombatInput.ManualAim(GameSettings.Flag("manualAim",false),GameSettings.Flag("aimPressed",false),Input.GetMouseButton(0),CombatInput.AimStick,CombatInput.UsingGamepad&&GameSettings.Flag("gamepadMove")),isolatedProfile=GameSettings.IsFixture,
                playerPosition=playerPos,aimDirection=CombatInput.AimDirection
            };
            bool changed=lastControllerSample==null||sample.buttons!=lastControllerSample.buttons||sample.phase!=lastControllerSample.phase||sample.menuPage!=lastControllerSample.menuPage||sample.menuFocus!=lastControllerSample.menuFocus||sample.usingGamepad!=lastControllerSample.usingGamepad||sample.manualAim!=lastControllerSample.manualAim||!sample.devices.SequenceEqual(lastControllerSample.devices);
            if(!changed&&Time.realtimeSinceStartup-lastControllerSample.seconds>=.1f)
                changed=sample.axes.Where((a,i)=>Mathf.Abs(a-lastControllerSample.axes[i])>.05f).Any()||sample.shots!=lastControllerSample.shots||(sample.playerPosition-lastControllerSample.playerPosition).sqrMagnitude>.01f||(sample.aimDirection-lastControllerSample.aimDirection).sqrMagnitude>.01f;
            if(!force&&!changed)return;
            string json=JsonUtility.ToJson(sample);
            File.AppendAllText(Path.Combine(controllerCheckPath,"input.jsonl"),json+Environment.NewLine);
            File.WriteAllText(Path.Combine(controllerCheckPath,"state.json"),json);
            lastControllerSample=sample;
        }
    }
}
