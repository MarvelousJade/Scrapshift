using System;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        Rect FullCanvas=>PresentationLayout.CanvasBounds(Screen.width,Screen.height);
        void InitializeDisplay(string[] args)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput=false;
            Application.targetFrameRate=60;
 #else
            // Editor Game view and explicit command-line capture sizes remain under their caller's control.
            if(Application.isEditor || Array.Exists(args,a=>a.StartsWith("-screen-")))return;
            SetFullscreen(PlayerPrefs.GetInt("fullscreen",1)!=0,false);
#endif
        }
        void SetFullscreen(bool enabled,bool save=true)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Screen.fullScreen=enabled;
 #else
            if(enabled)
            {
                var native=Screen.currentResolution;
                Screen.SetResolution(native.width,native.height,FullScreenMode.FullScreenWindow);
            }
            else Screen.SetResolution(1440,900,FullScreenMode.Windowed);
            if(save){PlayerPrefs.SetInt("fullscreen",enabled?1:0);PlayerPrefs.Save();}
#endif
        }
        void ToggleFullscreen()=>SetFullscreen(!Screen.fullScreen);
        string DisplayLabel=>Screen.fullScreen?"Display: Fullscreen":"Display: Windowed";
    }
}
