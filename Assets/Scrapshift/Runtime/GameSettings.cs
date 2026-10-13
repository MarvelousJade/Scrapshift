using UnityEngine;
namespace Scrapshift
{
    // Independent from run/progression saves. Fixtures never read or write the user's settings.
    public static class GameSettings
    {
        public static bool IsFixture=>System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a.StartsWith("--"));
        public static bool Flag(string key,bool fallback=true)=>!IsFixture?PlayerPrefs.GetInt("options."+key,fallback?1:0)!=0:fallback;
        public static float Value(string key,float fallback=1)=>!IsFixture?PlayerPrefs.GetFloat("options."+key,fallback):fallback;
        public static void Set(string key,bool value){if(IsFixture)return;PlayerPrefs.SetInt("options."+key,value?1:0);PlayerPrefs.Save();}
        public static void Set(string key,float value){if(IsFixture)return;PlayerPrefs.SetFloat("options."+key,value);PlayerPrefs.Save();}
        public static Color Positive=>new[]{new Color(0,1,0),Color.cyan,Color.yellow}[(int)Value("positiveColor",0)%3];
        public static Color Negative=>new[]{Color.red,Color.magenta,new Color(1,.5f,0)}[(int)Value("negativeColor",0)%3];
        public static Color TextColor(Color color)=>color.g>.9f&&color.r<.5f&&color.b<.5f?Positive:color.r>.9f&&color.g<.35f&&color.b<.35f?Negative:color;
    }
    public sealed partial class ArenaGame
    {
        Vector2 optionsScroll;
        bool focused=true;
        void OnApplicationFocus(bool focus)
        {
            focused=focus;
            if(!focus&&GameSettings.Flag("pauseFocus")&&Rules.Phase==RunPhase.Playing)Rules.Pause();
            ApplyAudioSettings();
        }
        void ApplyAudioSettings()=>AudioListener.volume=!focused&&GameSettings.Flag("muteFocus")?0:GameSettings.Value("master",.5f);
        float OptionsSlider(Rect rect,float value,float min,float max)
        {
            float next=GUI.HorizontalSlider(rect,value,min,max);
            float fraction=Mathf.InverseLerp(min,max,next);
            RoundedBox(new Rect(rect.x,rect.y+4,rect.width,8),new Color(.3f,.32f,.31f),4);
            RoundedBox(new Rect(rect.x,rect.y+4,rect.width*fraction,8),paper,4);
            RoundedBox(new Rect(rect.x+(rect.width-18)*fraction,rect.y-3,18,22),paper,5);
            return next;
        }
        void OptionToggle(string label,string key,ref float y,bool fallback=true)
        {
            bool value=GameSettings.Flag(key,fallback);
            RoundedBox(new Rect(0,y,940,53),new Color(0,0,0,.82f),8);
            Text(15,y+5,765,43,label,body);
            if(Button(new Rect(803,y+7,118,39),value?"On":"Off"))GameSettings.Set(key,!value);
            y+=62;
        }
        void OptionSlider(string label,string key,ref float y,float min,float max,float fallback=1)
        {
            float value=GameSettings.Value(key,fallback);
            RoundedBox(new Rect(0,y,940,65),new Color(0,0,0,.82f),8);
            Text(15,y+6,480,45,label,body);Text(817,y+7,100,40,$"{value*100:0}%",center);
            float next=OptionsSlider(new Rect(500,y+27,290,25),value,min,max);
            if(Mathf.Abs(next-value)>.005f)GameSettings.Set(key,Mathf.Round(next*100)/100);y+=74;
        }
        void DrawTabbedOptions(bool paused)
        {
            string[] tabs={"Sound","Video","Gameplay","Accessibility"};
            for(int i=0;i<4;i++)if(MenuButton(new Rect(160+i*282,55,272,65),tabs[i],optionsTab==i)){optionsTab=i;optionsScroll=Vector2.zero;}
            var view=new Rect(235,155,970,570);float content=optionsTab==2?805:optionsTab==3?1050:545;
            optionsScroll=GUI.BeginScrollView(view,optionsScroll,new Rect(0,0,947,content));float y=0;
            if(optionsTab==0)
            {
                OptionSlider("Master volume","master",ref y,0,1,.5f);
                OptionSlider("Music volume","music",ref y,0,1,.35f);
                Text(15,y+8,450,45,"Sound effects volume",body);float v=OptionsSlider(new Rect(500,y+27,290,25),sound.Volume,0,1);Text(817,y+7,100,40,$"{sound.Volume*100:0}%",center);
                if(Mathf.Abs(v-sound.Volume)>.005f)sound.SetVolume(v);y+=74;
                if(Button(new Rect(0,y,940,53),muted?"Sound effects: Off":"Sound effects: On"))ToggleSound();y+=62;
                OptionToggle("Mute when focus is lost","muteFocus",ref y);ApplyAudioSettings();
            }
            else if(optionsTab==1)
            {
                if(Button(new Rect(0,y,940,53),DisplayLabel))ToggleFullscreen();y+=62;
                OptionToggle("Screen shake","shake",ref y);OptionToggle("Visual effects","effects",ref y);OptionToggle("Damage display","damage",ref y);
                OptionToggle("Limit FPS to 60","limitFps",ref y,false);Application.targetFrameRate=GameSettings.Flag("limitFps",false)?60:120;
                Text(15,y+12,910,45,Screen.width+" × "+Screen.height+"    F11: toggle fullscreen",body);
            }
            else if(optionsTab==2)
            {
                OptionToggle("Mouse-only movement","mouseMove",ref y,false);
                OptionToggle("Manual aiming","manualAim",ref y,false);
                OptionToggle("Manual aiming only while mouse is pressed","aimPressed",ref y,false);
                OptionToggle("Pause when focus is lost","pauseFocus",ref y);
                OptionToggle("Boss HP bar","bossHp",ref y);
                OptionToggle("Keep items locked between shops","keepLock",ref y);
                OptionToggle("Hide item appearance","hideItems",ref y,false);
                Text(15,y+15,910,210,"Move: WASD / Arrow keys (or hold left mouse)\nPause / Back: Escape     Confirm: Enter\nInspect: Tab / Shift+Tab\nWeapon actions: Left / Right click\nManual aiming keeps automatic firing.",body);
            }
            else
            {
                OptionSlider("Enemy health (new spawns)","enemyHealth",ref y,.1f,2);
                OptionSlider("Enemy damage","enemyDamage",ref y,.1f,2);
                OptionSlider("Enemy speed","enemySpeed",ref y,.1f,2);
                OptionSlider("Description font size","fontScale",ref y,.8f,1.3f);
                OptionSlider("Projectile opacity","projectileOpacity",ref y,.2f,1);
                OptionSlider("Pet opacity","petOpacity",ref y,.2f,1);
                OptionToggle("Screen darkening","darkening",ref y);
                OptionToggle("Show effect icons","effectIcons",ref y);
                if(Button(new Rect(0,y,940,53),reducedMotion?"Reduced motion: On":"Reduced motion: Off")){reducedMotion=!reducedMotion;PlayerPrefs.SetInt("reducedMotion",reducedMotion?1:0);PlayerPrefs.Save();}y+=62;
                foreach(var key in new[]{"positiveColor","negativeColor"}){int value=(int)GameSettings.Value(key,0);if(Button(new Rect(0,y,940,53),(key=="positiveColor"?"Positive text: ":"Negative text: ")+(key=="positiveColor"?new[]{"Green","Cyan","Yellow"}:new[]{"Red","Magenta","Orange"})[value%3]))GameSettings.Set(key,(float)((value+1)%3));y+=62;}
            }
            GUI.EndScrollView();
            if(MenuButton(new Rect(470,770,500,60),"Back")){if(paused)pauseOptions=false;else menuPage=0;}
        }
    }
}
