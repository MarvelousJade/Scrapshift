using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool inspectionReview,inspectionCaptured;string inspectionPath,forcedInspection,lastDrawnInspection;Rect lastInspectionPanel;
        float inspectionNext;int inspectionStage=-1;DateTime inspectionUtc;
        readonly List<string> inspectionCaptures=new List<string>();
        [Serializable] sealed class InspectionResult { public string name,phase,target,drawn;public bool panelOnScreen; }
        [Serializable] sealed class InspectionReport { public string status,scope="Staged UI using normal screen renderers and forced inspection focus. Frozen simulation; native mouse/keyboard checked separately.";public bool capturesVerified;public List<InspectionResult> checks=new List<InspectionResult>(); }
        InspectionReport inspectionReport;
        static readonly string[] inspectionCases={"rookie-weapons","pugilist-weapons","hothead-weapons","sharpshot-weapons","pause-stat","pause-weapon","pause-item","shop-stat","shop-weapon","shop-item","upgrade-weapon","loot-item"};
        void BeginInspectionReview(string path)
        {inspectionReview=true;inspectionPath=Path.GetFullPath(path);Directory.CreateDirectory(inspectionPath);inspectionUtc=DateTime.UtcNow;inspectionNext=Time.realtimeSinceStartup+.5f;inspectionReport=new InspectionReport();Begin();sound.SetMuted(true);}
        void InspectionReviewTick()
        {
            if(!inspectionReview||inspectionStage>=inspectionCases.Length||Time.realtimeSinceStartup<inspectionNext)return;
            if(inspectionStage>=0&&!inspectionCaptured)
            {
                string name=inspectionCases[inspectionStage];inspectionCaptures.Add(name);ScreenCapture.CaptureScreenshot(Path.Combine(inspectionPath,name+".png"));
                inspectionReport.checks.Add(new InspectionResult{name=name,phase=Rules.Phase.ToString(),target=forcedInspection,drawn=lastDrawnInspection,
                    panelOnScreen=forcedInspection==null||FullCanvas.Contains(lastInspectionPanel.min)&&FullCanvas.Contains(lastInspectionPanel.max)});
                inspectionCaptured=true;inspectionNext=Time.realtimeSinceStartup+.25f;return;
            }
            inspectionStage++;
            inspectionCaptured=false;
            if(inspectionStage==inspectionCases.Length){inspectionNext=Time.realtimeSinceStartup+.6f;return;}
            if(inspectionStage>inspectionCases.Length)return;
            forcedInspection=null;lastDrawnInspection=null;
            if(inspectionStage<4){ReturnToSelection(2);ChangeLook(inspectionStage-playerAnimation.lookIndex);startingWeapon=0;}
            else
            {
                Begin();ClearCombat();Rules.Inventory.Add(catalog.Weapon("wand",1));Rules.Items.Add(Array.Find(catalog.data.items,i=>i.id=="coffee"));Rules.GainWeaponStat("armor",15);SyncEquipment();
                if(inspectionStage<7)Rules.Pause();
                else if(inspectionStage<10)Rules.Tick(Rules.Remaining);
                else if(inspectionStage==10){Rules.Collect(20);Rules.Tick(Rules.Remaining);}
                else {Rules.CollectConsumable(ConsumableKind.Crate);Rules.Tick(Rules.Remaining);}
                forcedInspection=inspectionStage==4||inspectionStage==7?"stat:armor":inspectionStage==5||inspectionStage==8||inspectionStage==10?"weapon:1":"item:0";
            }
            inspectionNext=Time.realtimeSinceStartup+.8f;
        }
        void FinishInspectionReview()
        {
            if(!inspectionReview||inspectionStage!=inspectionCases.Length||Time.realtimeSinceStartup<inspectionNext)return;
            inspectionReport.capturesVerified=CaptureVerification.Fresh(inspectionPath,inspectionUtc,inspectionCaptures.ToArray());
            bool okay=inspectionReport.capturesVerified&&inspectionReport.checks.TrueForAll(c=>c.panelOnScreen&&(c.target==null||c.target==c.drawn));
            inspectionReport.status=okay?"Passed":"Failed";File.WriteAllText(Path.Combine(inspectionPath,"inspection.json"),JsonUtility.ToJson(inspectionReport,true));
            Application.Quit(okay?0:1);enabled=false;
        }
    }
}
