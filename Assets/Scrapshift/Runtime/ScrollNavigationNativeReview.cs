using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        ScrollNavigationNativeReview scrollNavigationReview;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartScrollNavigationReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--scroll-navigation-review");
            if(index>=0&&index+1<args.Length)new GameObject("Scroll navigation coordinate review").AddComponent<ScrollNavigationNativeReview>().folder=args[index+1];
        }
        // Opt-in observation only. The ordinary navigation, scrolling and button
        // dispatch remain unchanged; the fixture never assigns a focus index.
        sealed class ScrollNavigationNativeReview:MonoBehaviour
        {
            public string folder;
            ArenaGame game;Gamepad pad;bool repaint;
            Vector2 root,axisX,axisY,scrollAtBegin;
            public string lastRepaintFocus="",lastActivated="";
            static readonly string[] Keys={"mouseMove","gamepadMove","manualAim","aimPressed","pauseFocus","retryWave","optimizeEndWaves","bossHp","holdReroll","keepLock","hideItems"};
            [Serializable] sealed class Row
            {
                public int frame,focus,width,height;public string eventType,key,label,matrix;
                public bool repaint,focused,clicked,activationPending;
                public Vector2 root,axisX,axisY,scrollAtBegin,scrollAtControl;
                public Rect logical,actual,rawGuiToScreen,expected,previousFocused;public float maximumCoordinateError;
            }
            [Serializable] sealed class Report
            {
                public string status,error,scope="Software Gamepad events through production Update/OnGUI in an isolated native player. Options page/tab and initial preferences staged; focus indices never assigned for activation. Same-OnGUI root/scroll coordinate observation and actual setting changes provide separate receipts. IgnoreFocus policy and virtual device do not prove ordinary OS focus or physical Xbox behavior.";
                public bool actualStart,scrolled,immediateToggle,settledToggle,onlyExpectedSettingChanged;
                public bool registeredCoordinatesMatchRoot=true;
                public string focusBeforeEdge,focusAfterEdge,immediateActivated,settledActivated;
                public float scrollBeforeEdge,scrollAfterEdge;
                public List<Row> rows=new List<Row>();public List<string> captures=new List<string>();
            }
            readonly Report report=new Report();
            public void BeginFrame(bool isRepaint)
            {
                repaint=isRepaint;if(isRepaint)lastRepaintFocus="";
                root=GUIUtility.GUIToScreenPoint(Vector2.zero);
                axisX=GUIUtility.GUIToScreenPoint(Vector2.right)-root;axisY=GUIUtility.GUIToScreenPoint(Vector2.up)-root;
                scrollAtBegin=game.optionsScroll;
            }
            Vector2 RootPoint(Vector2 point)=>root+axisX*point.x+axisY*point.y;
            public void Control(Rect logical,string label,bool focused,bool clicked,bool pending)
            {
                if(game.menuPage!=4||game.optionsTab!=2)return;
                bool option=logical.x==0&&logical.width==940;
                string key=option&&Mathf.RoundToInt(logical.y/62)<Keys.Length?Keys[Mathf.RoundToInt(logical.y/62)]:label;
                if(focused&&repaint)lastRepaintFocus=key;if(clicked)lastActivated=key;
                // Bounded diagnostic storage must never disable the semantic
                // observer that drives this fixture's later input sequence.
                if(report.rows.Count>=6000||!repaint&&!clicked&&!pending)return;
                var actual=game.NavigationRect(logical);Rect expected=actual;
                var rawMin=GUIUtility.GUIToScreenPoint(logical.min);var rawMax=GUIUtility.GUIToScreenPoint(logical.max);
                var raw=Rect.MinMaxRect(rawMin.x,rawMin.y,rawMax.x,rawMax.y);
                if(option)
                {
                    Vector2 min=RootPoint(new Vector2(235,155)+logical.min-scrollAtBegin);
                    Vector2 max=RootPoint(new Vector2(235,155)+logical.max-scrollAtBegin);
                    expected=Rect.MinMaxRect(min.x,min.y,max.x,max.y);
                }
                var previous=game.menuFocus>=0&&game.menuFocus<game.menuNavigationButtons.Count?game.menuNavigationButtons[game.menuFocus]:default;
                float error=Mathf.Max((actual.min-expected.min).magnitude,(actual.max-expected.max).magnitude);
                if(option&&repaint)report.registeredCoordinatesMatchRoot&=error<.25f;
                report.rows.Add(new Row{frame=Time.frameCount,focus=game.menuFocus,width=Screen.width,height=Screen.height,eventType=Event.current.type.ToString(),
                    key=key,label=label,matrix=GUI.matrix.ToString(),repaint=repaint,focused=focused,clicked=clicked,activationPending=pending,
                    root=root,axisX=axisX,axisY=axisY,scrollAtBegin=scrollAtBegin,scrollAtControl=game.optionsScroll,logical=logical,actual=actual,rawGuiToScreen=raw,expected=expected,
                    previousFocused=previous,maximumCoordinateError=error});
            }
            IEnumerator Start()
            {
                var stack=new Stack<IEnumerator>();stack.Push(Run());float started=Time.realtimeSinceStartup;
                while(stack.Count>0)
                {
                    bool moved=false;object next=null;
                    try{Require(Time.realtimeSinceStartup-started<45,"Scroll review exceeded 45 seconds.");moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;else stack.Pop();}
                    catch(Exception ex){report.error=ex.ToString();break;}
                    if(!moved)continue;if(next is IEnumerator child){stack.Push(child);continue;}yield return next;
                }
                report.status=report.error==null&&report.actualStart&&report.scrolled&&report.immediateToggle&&report.settledToggle&&report.onlyExpectedSettingChanged&&report.registeredCoordinatesMatchRoot?"Passed":"Failed";
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"scroll-navigation.json"),JsonUtility.ToJson(report,true));
                if(game!=null)game.scrollNavigationReview=null;if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
                Application.Quit(report.status=="Passed"?0:1);
            }
            IEnumerator Run()
            {
                folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null&&GameSettings.IsFixture,"Initialized isolated native player required.");
                Require(Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use --controller-check <isolated-folder> for mutable in-memory settings.");
                report.actualStart=game.Rules.CaptureRandomDamageTarget!=null;
                Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();
                GameSettings.Set("pauseFocus",false);game.sound.SetMuted(true);game.scrollNavigationReview=this;
                game.menuPage=4;game.optionsTab=2;game.optionsScroll=Vector2.zero;
                game.menuNavigationActive=true; // Selection still follows production default focus + actual D-pad.
                yield return new WaitForSecondsRealtime(.2f);
                yield return Capture("initial-options.png");
                for(int i=0;i<18&&lastRepaintFocus!="holdReroll";i++)yield return Press(GamepadButton.DpadDown);
                Require(lastRepaintFocus=="holdReroll","D-pad did not reach the last fully visible gameplay option; observed "+lastRepaintFocus);
                report.focusBeforeEdge=lastRepaintFocus;report.scrollBeforeEdge=game.optionsScroll.y;
                yield return Capture("before-scroll-edge.png");
                var before=Keys.ToDictionary(key=>key,key=>GameSettings.Flag(key,key!="mouseMove"&&key!="manualAim"&&key!="aimPressed"&&key!="retryWave"&&key!="optimizeEndWaves"&&key!="hideItems"));
                lastActivated="";
                State(new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;
                State(new GamepadState());yield return null;
                State(new GamepadState().WithButton(GamepadButton.South));yield return null;
                State(new GamepadState());yield return new WaitForSecondsRealtime(.25f);
                report.scrollAfterEdge=game.optionsScroll.y;report.focusAfterEdge=lastRepaintFocus;
                report.scrolled=report.scrollAfterEdge>report.scrollBeforeEdge;
                report.immediateActivated=lastActivated;
                report.immediateToggle=GameSettings.Flag("keepLock")!=before["keepLock"]&&report.immediateActivated=="keepLock";
                report.onlyExpectedSettingChanged=Keys.Where(key=>key!="keepLock").All(key=>GameSettings.Flag(key,before[key])==before[key]);
                yield return Capture("after-immediate-confirm.png");
                bool settledBefore=GameSettings.Flag("keepLock");lastActivated="";
                yield return Press(GamepadButton.South);
                report.settledActivated=lastActivated;report.settledToggle=GameSettings.Flag("keepLock")!=settledBefore&&report.settledActivated=="keepLock";
                yield return Capture("after-settled-confirm.png");
            }
            void State(GamepadState state)=>InputSystem.QueueStateEvent(pad,state);
            IEnumerator Press(GamepadButton button)
            {State(new GamepadState().WithButton(button));yield return new WaitForSecondsRealtime(.06f);State(new GamepadState());yield return new WaitForSecondsRealtime(.14f);}
            IEnumerator Capture(string name)
            {ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));yield return null;yield return new WaitForSecondsRealtime(.12f);Require(File.Exists(Path.Combine(folder,name)),"Missing capture "+name);report.captures.Add(name);}
            static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        }
    }
}
