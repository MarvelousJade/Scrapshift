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
        [Serializable] sealed class InspectionResult { public string name,phase,target,drawn;public bool panelOnScreen;public int banked,pendingLevels,pendingCrates;public float arrivalTime,canopyWidth,canopyHeight,remaining; }
        [Serializable] sealed class InspectionReport { public string status,scope="Staged UI using normal screen renderers and forced inspection focus. Frozen simulation; this fixture does not validate native mouse/keyboard input.";public bool capturesVerified;public List<InspectionResult> checks=new List<InspectionResult>(); }
        InspectionReport inspectionReport;
        static readonly string[] inspectionCases={"rookie-weapons","pugilist-weapons","hothead-weapons","sharpshot-weapons","pause-stat","pause-weapon","pause-item","shop-stat","shop-weapon","shop-item","upgrade-weapon","loot-item","rookie-character","pugilist-character","hothead-character","sharpshot-character","canopy-enter","canopy-open","canopy-sway","canopy-stretch","canopy-fold","canopy-gone","landing-finished","weapon-actions","pending-rewards","tree-warning","wave-sweep","harvest","sweep-complete","options-gameplay","options-audio","options-video","options-controls","movement-dust","intermission-tree","expanded-weapons","implant-shop","map-north","map-south"};
        void BeginInspectionReview(string path)
        {inspectionReview=true;inspectionPath=Path.GetFullPath(path);Directory.CreateDirectory(inspectionPath);inspectionUtc=DateTime.UtcNow;inspectionNext=Time.realtimeSinceStartup+.5f;inspectionReport=new InspectionReport();Begin();sound.SetMuted(true);}
        void InspectionReviewTick()
        {
            if(!inspectionReview||inspectionStage>=inspectionCases.Length||Time.realtimeSinceStartup<inspectionNext)return;
            if(inspectionStage>=0&&!inspectionCaptured)
            {
                string name=inspectionCases[inspectionStage];inspectionCaptures.Add(name);ScreenCapture.CaptureScreenshot(Path.Combine(inspectionPath,name+".png"));
                inspectionReport.checks.Add(new InspectionResult{name=name,phase=Rules.Phase.ToString(),target=forcedInspection,drawn=lastDrawnInspection,
                    panelOnScreen=forcedInspection==null||FullCanvas.Contains(lastInspectionPanel.min)&&FullCanvas.Contains(lastInspectionPanel.max),
                    banked=Rules.Banked,pendingLevels=Rules.PendingLevels,pendingCrates=Rules.PendingCrates,arrivalTime=arrivalAge,canopyWidth=parachute==null?0:parachute.transform.localScale.x,canopyHeight=parachute==null?0:parachute.transform.localScale.y,remaining=Rules.Remaining});
                inspectionCaptured=true;inspectionNext=Time.realtimeSinceStartup+.25f;return;
            }
            inspectionStage++;
            inspectionCaptured=false;
            if(inspectionStage==inspectionCases.Length){inspectionNext=Time.realtimeSinceStartup+.6f;return;}
            if(inspectionStage>inspectionCases.Length)return;
            forcedInspection=null;lastDrawnInspection=null;
            if(inspectionStage>=37){Begin();ClearCombat();playerPos=new Vector2(8,inspectionStage==37?7:-7);player.transform.position=playerPos;playerShadow.Place(playerPos,ActorPresentation.PlayerScale);UpdateEquipment(0);gameCamera.transform.position=(Vector3)ArenaGeometry.CameraPosition(playerPos,gameCamera.aspect)+Vector3.back*10;}
            else if(inspectionStage==35||inspectionStage==36)
            {
                Begin();ClearCombat();Rules.Tick(Rules.Remaining);
                if(inspectionStage==35){string[] ids={"rocket_launcher","ghost_axe","flamethrower","hammer"};int[] tiers={2,1,2,2};for(int i=0;i<4;i++)Rules.Shop[i]=new ShopOffer{weapon=catalog.Weapon(ids[i],tiers[i]),rolledWave=1};}
                else {string[] ids={"coupon","bag","bandana","alien_tongue"};for(int i=0;i<4;i++)Rules.Shop[i]=new ShopOffer{item=Array.Find(catalog.data.items,a=>a.id==ids[i]),rolledWave=1};}
            }
            else if(inspectionStage>=29&&inspectionStage<=32){Begin();Rules.Pause();pauseOptions=true;optionsTab=inspectionStage-29;}
            else if(inspectionStage==33){pauseOptions=false;Begin();ClearCombat();arrivalAge=2;for(int i=0;i<5;i++)AddFootDust(playerPos-new Vector2(i*.16f,.3f),false);}
            else if(inspectionStage==34){Begin();ClearCombat();Spawn(catalog.Enemy("tree"),playerPos+new Vector2(1,1));UpdateEnemies(1.5f);BeginWaveSweep();for(int i=0;i<40;i++)TickWaveSweep(.05f);}
            else if(inspectionStage<4){ReturnToSelection(2);ChangeLook(inspectionStage-playerAnimation.lookIndex);startingWeapon=0;}
            else if(inspectionStage>=12&&inspectionStage<16){ReturnToSelection(1);randomCharacter=false;ChangeLook(inspectionStage-12-playerAnimation.lookIndex);startingWeapon=0;}
            else if(inspectionStage>=23)
            {
                Begin();ClearCombat();Rules.GainWeaponStat("harvesting",10);Rules.Collect(35);
                Rules.CollectConsumable(ConsumableKind.Crate);
                if(inspectionStage==23){Rules.Inventory.Add(catalog.Weapon("pistol",1));Rules.Tick(Rules.Remaining);Rules.ResolveCrate(true);while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);selectedWeapon=0;}
                else if(inspectionStage==25){Spawn(catalog.Enemy("tree"),new Vector2(2,1));UpdateEnemies(.3f);}
                else if(inspectionStage>=26){for(int k=0;k<12;k++)Drop(new Vector2(k*.5f-3,1),1,false);BeginWaveSweep();for(int step=0;step<(inspectionStage==28?40:inspectionStage==27?16:3);step++)TickWaveSweep(.05f);}
            }
            else if(inspectionStage>=16){reducedMotion=false;StartSelectedRun();TickArrival(new[]{.25f,.5f,.7f,.8f,.9f,1f,1.2f}[inspectionStage-16]);}
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
            okay=okay&&inspectionReport.checks.TrueForAll(c=>c.name!="sweep-complete"||(c.banked==12&&c.phase=="Loot"&&c.pendingLevels>0&&c.pendingCrates==1));
            inspectionReport.status=okay?"Passed":"Failed";File.WriteAllText(Path.Combine(inspectionPath,"inspection.json"),JsonUtility.ToJson(inspectionReport,true));
            Application.Quit(okay?0:1);enabled=false;
        }
    }
}
