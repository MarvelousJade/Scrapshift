using UnityEngine;
namespace Scrapshift
{
    // Independent from run/progression saves. Fixtures never read or write the user's settings.
    public static class GameSettings
    {
        // Process arguments cannot change after launch. Do not re-read/copy them
        // for every enemy/projectile settings query; option values stay live.
        static readonly bool fixture,mutableFixture;
        static GameSettings()
        {
            var args=System.Environment.GetCommandLineArgs();
            fixture=System.Array.Exists(args,a=>a.StartsWith("--"));
            mutableFixture=System.Array.IndexOf(args,"--controller-check")>=0||System.Array.Exists(args,a=>a.StartsWith("--headless-"));
        }
        public static bool IsFixture=>fixture;
        static readonly System.Collections.Generic.Dictionary<string,float> fixtureOptions=new System.Collections.Generic.Dictionary<string,float>();
        static bool FixtureOverrides=>mutableFixture;
        public static bool Flag(string key,bool fallback=true)=>IsFixture?(FixtureOverrides&&fixtureOptions.TryGetValue(key,out float value)?value!=0:fallback):PlayerPrefs.GetInt("options."+key,fallback?1:0)!=0;
        public static float Value(string key,float fallback=1)=>IsFixture?(FixtureOverrides&&fixtureOptions.TryGetValue(key,out float value)?value:fallback):PlayerPrefs.GetFloat("options."+key,fallback);
        public static void Set(string key,bool value){if(IsFixture){if(FixtureOverrides)fixtureOptions[key]=value?1:0;return;}PlayerPrefs.SetInt("options."+key,value?1:0);PlayerPrefs.Save();}
        public static void Set(string key,float value){if(IsFixture){if(FixtureOverrides)fixtureOptions[key]=value;return;}PlayerPrefs.SetFloat("options."+key,value);PlayerPrefs.Save();}
        public static Color Positive=>new[]{new Color(0,1,0),Color.cyan,Color.yellow}[(int)Value("positiveColor",0)%3];
        public static Color Negative=>new[]{Color.red,Color.magenta,new Color(1,.5f,0)}[(int)Value("negativeColor",0)%3];
        public static Color TextColor(Color color)=>color.g>.9f&&color.r<.5f&&color.b<.5f?Positive:color.r>.9f&&color.g<.35f&&color.b<.35f?Negative:color;
    }
    public sealed partial class ArenaGame
    {
        Vector2 optionsScroll;
        GUIStyle settingsLabel,settingsValue;
        bool SettingsButton(Rect r,string label,bool selected=false,Rect? navigationRect=null)
        {
            var focusRect=navigationRect??r;
            if((navigationRect.HasValue||r.width==940)&&NavigationButton(focusRect))EnsureNavigationVisible(focusRect,new Rect(0,0,947,570),ref optionsScroll);
            return MenuButton(r,label,selected,fontSize:27,navigationRect:navigationRect);
        }
        bool focused=true;
#if UNITY_WEBGL && !UNITY_EDITOR
        // DOM visibility events also cover browser tabs and embedded player frames.
        public void BrowserFocus(string value)=>OnApplicationFocus(value=="1");
#endif
        void OnApplicationFocus(bool focus)
        {
            focused=focus;
            if(!focus){progress?.Save();if(!GameSettings.IsFixture)PlayerPrefs.Save();}
            if(!focus&&!campaignCheck&&GameSettings.Flag("pauseFocus")&&(Rules.Phase==RunPhase.Playing||Rules.Phase==RunPhase.Shop||Rules.Phase==RunPhase.Upgrade||Rules.Phase==RunPhase.Loot))Rules.Pause();
            ApplyAudioSettings();
        }
        void ApplyAudioSettings()=>AudioListener.volume=!focused&&GameSettings.Flag("muteFocus")?0:GameSettings.Value("master",.5f);
        float OptionsSlider(Rect rect,float value,float min,float max,Rect? navigationRect=null)
        {
            var focusRect=navigationRect??rect;
            bool navigationFocus=NavigationButton(focusRect);menuNavigationSliders.Add(NavigationRect(focusRect));
            if(navigationFocus)EnsureNavigationVisible(focusRect,new Rect(0,0,947,570),ref optionsScroll);
            float next=GUI.HorizontalSlider(rect,value,min,max);
            if(navigationFocus&&menuSliderStep!=0){next=Mathf.Clamp(value+menuSliderStep*(max-min)*.05f,min,max);menuSliderStep=0;}
            float fraction=Mathf.InverseLerp(min,max,next);
            if(navigationFocus)RoundedBox(new Rect(rect.x-5,rect.y-8,rect.width+10,33),new Color(.48f,1,.77f,.35f),5);
            RoundedBox(new Rect(rect.x,rect.y+4,rect.width,8),new Color(.3f,.32f,.31f),4);
            RoundedBox(new Rect(rect.x,rect.y+4,rect.width*fraction,8),paper,4);
            RoundedBox(new Rect(rect.x+(rect.width-18)*fraction,rect.y-3,18,22),paper,5);
            return next;
        }
        void OptionToggle(string label,string key,ref float y,bool fallback=true)
        {
            bool value=GameSettings.Flag(key,fallback);
            RoundedBox(new Rect(0,y,940,53),new Color(0,0,0,.82f),8);
            Text(15,y+5,765,43,label,settingsLabel);
            if(SettingsButton(new Rect(803,y+7,118,39),value?"On":"Off",navigationRect:new Rect(0,y,940,53)))GameSettings.Set(key,!value);
            y+=62;
        }
        void OptionSlider(string label,string key,ref float y,float min,float max,float fallback=1)
        {
            float value=GameSettings.Value(key,fallback);
            RoundedBox(new Rect(0,y,940,65),new Color(0,0,0,.82f),8);
            Text(15,y+6,480,45,label,settingsLabel);Text(817,y+7,100,40,$"{value*100:0}%",settingsValue);
            float next=OptionsSlider(new Rect(500,y+27,290,25),value,min,max,new Rect(0,y,940,65));
            if(Mathf.Abs(next-value)>.005f)GameSettings.Set(key,Mathf.Round(next*100)/100);y+=74;
        }
        void SwitchOptionsTab(int tab)
        {
            optionsTab=Mathf.Clamp(tab,0,3);optionsScroll=Vector2.zero;
            menuFocus=-1;menuActivate=false;
        }
        void DrawTabbedOptions(bool paused)
        {
            settingsLabel=new GUIStyle(body){fontSize=27,alignment=TextAnchor.MiddleLeft};
            settingsValue=new GUIStyle(center){fontSize=27};
            string[] tabs={"Sound","Video","Gameplay","Accessibility"};
            for(int i=0;i<4;i++)if(SettingsButton(new Rect(160+i*282,55,272,65),tabs[i],optionsTab==i))SwitchOptionsTab(i);
            var view=new Rect(235,155,970,570);float content=optionsTab==2?762:optionsTab==3?(paused?1050:1130):optionsTab==1&&mapPicker?1010:545;
            optionsScroll=GUI.BeginScrollView(view,optionsScroll,new Rect(0,0,947,content));float y=0;
            if(optionsTab==0)
            {
                OptionSlider("Master volume","master",ref y,0,1,.5f);
                OptionSlider("Music volume","music",ref y,0,1,.35f);
                RoundedBox(new Rect(0,y,940,65),new Color(0,0,0,.82f),8);
                Text(15,y+8,450,45,"Sound effects volume",settingsLabel);float v=OptionsSlider(new Rect(500,y+27,290,25),sound.Volume,0,1,new Rect(0,y,940,65));Text(817,y+7,100,40,$"{sound.Volume*100:0}%",settingsValue);
                if(Mathf.Abs(v-sound.Volume)>.005f)sound.SetVolume(v);y+=74;
                if(SettingsButton(new Rect(0,y,940,53),muted?"Sound effects: Off":"Sound effects: On"))ToggleSound();y+=62;
                OptionToggle("Mute when focus is lost","muteFocus",ref y);ApplyAudioSettings();
            }
            else if(optionsTab==1)
            {
                if(SettingsButton(new Rect(0,y,940,53),DisplayLabel))ToggleFullscreen();y+=62;
                DrawMapOption(ref y);
                OptionToggle("Screen shake","shake",ref y);OptionToggle("Visual effects","effects",ref y);OptionToggle("Damage display","damage",ref y);
                OptionToggle("Limit FPS to 60","limitFps",ref y,false);Application.targetFrameRate=GameSettings.Flag("limitFps",false)?60:(Application.platform==RuntimePlatform.WebGLPlayer?-1:120);
            }
            else if(optionsTab==2)
            {
                OptionToggle("Mouse-only movement","mouseMove",ref y,false);
                OptionToggle("Movement and aiming with gamepad","gamepadMove",ref y);
                OptionToggle("Manual aiming","manualAim",ref y,false);
                OptionToggle("Manual aiming only while mouse is pressed","aimPressed",ref y,false);
                OptionToggle("Pause when focus is lost","pauseFocus",ref y);
                OptionToggle("Retry failed waves","retryWave",ref y,false);
                OptionToggle("Optimize end waves","optimizeEndWaves",ref y,false);
                OptionToggle("Boss HP bar","bossHp",ref y);
                OptionToggle("Hold controller button to reroll","holdReroll",ref y);
                OptionToggle("Keep items locked between shops","keepLock",ref y);
                OptionToggle("Hide item appearance","hideItems",ref y,false);
            }
            else
            {
                if(!paused)
                {
                    int mode=Mathf.Clamp(Mathf.RoundToInt(GameSettings.Value("constantProjectile",1)),0,2);
                    RoundedBox(new Rect(0,y,940,53),new Color(0,0,0,.82f),8);
                    Text(15,y+5,640,43,"Constant projectiles in Nightmare",settingsLabel);
                    if(SettingsButton(new Rect(660,y+7,260,39),new[]{"None","Events","Every wave"}[mode]))GameSettings.Set("constantProjectile",(float)((mode+1)%3));
                    y+=62;
                }
                OptionSlider("Enemy health (new spawns)","enemyHealth",ref y,.1f,2);
                OptionSlider("Enemy damage","enemyDamage",ref y,.1f,2);
                OptionSlider("Enemy speed","enemySpeed",ref y,.1f,2);
                OptionSlider("Description font size","fontScale",ref y,.8f,1.3f);
                OptionSlider("Projectile opacity","projectileOpacity",ref y,.2f,1);
                OptionSlider("Pet opacity","petOpacity",ref y,.2f,1);
                OptionToggle("Screen darkening","darkening",ref y);
                OptionToggle("Show effect icons","effectIcons",ref y);
                if(SettingsButton(new Rect(0,y,940,53),reducedMotion?"Reduced motion: On":"Reduced motion: Off")){reducedMotion=!reducedMotion;if(!GameSettings.IsFixture){PlayerPrefs.SetInt("reducedMotion",reducedMotion?1:0);PlayerPrefs.Save();}}y+=62;
                foreach(var key in new[]{"positiveColor","negativeColor"}){int value=(int)GameSettings.Value(key,0);if(SettingsButton(new Rect(0,y,940,53),(key=="positiveColor"?"Positive text: ":"Negative text: ")+(key=="positiveColor"?new[]{"Green","Cyan","Yellow"}:new[]{"Red","Magenta","Orange"})[value%3]))GameSettings.Set(key,(float)((value+1)%3));y+=62;}
            }
            GUI.EndScrollView();
            if(SettingsButton(new Rect(470,770,500,60),"Back")){if(paused)pauseOptions=false;else menuPage=0;}
        }
    }
}
