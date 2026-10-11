using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool presentationReview;
        string presentationPath;
        float presentationStart;
        int presentationStage;
        DateTime presentationUtc;
        readonly string[] presentationCaptures={"title","character-selection","weapon-selection","run-setup","landing-descent","landing-canopy-collapse","landing-touchdown","six-weapons-right","six-weapons-left","arena-edge","run-lost","run-summary","run-won"};
        [Serializable] sealed class PresentationReport
        {
            public string status,scope;
            public bool capturesVerified,authoredScenePresent,canopyInstantiated,selectedWeaponStarted,landingHeldTimer,landingFinished,restartReset,returnToTitle,twentyWaveWin;
        }
        PresentationReport presentationReport;
        void BeginPresentationReview(string folder)
        {
            presentationReview=true;presentationPath=folder;Directory.CreateDirectory(folder);presentationUtc=DateTime.UtcNow;
            presentationStart=Time.realtimeSinceStartup;presentationReport=new PresentationReport{scope="Staged rendered menu/arrival/loadout/death/win checks, frozen AI. Actual menu methods and RunRules transitions; accelerated twenty-wave completion without combat. Not a human playthrough or balance test."};
            sound.SetMuted(true);reducedMotion=false;
            presentationReport.authoredScenePresent=parachutePrefab!=null&&GameObject.Find("Soft yard floor")!=null;
        }
        void PresentationReviewTick()
        {
            if(!presentationReview||Time.realtimeSinceStartup-presentationStart<1.5f+presentationStage*.9f)return;
            switch(presentationStage)
            {
                case 0:break;
                case 1:ConfirmMenu();break;
                case 2:ChangeLook(1);ConfirmMenu();startingWeapon=1;break;
                case 3:ConfirmMenu();break;
                case 4:
                    ConfirmMenu();presentationReport.canopyInstantiated=parachute!=null;presentationReport.selectedWeaponStarted=Rules.Inventory.Count==1&&Rules.Inventory[0].id=="smg";
                    TickArrival(.45f);presentationReport.landingHeldTimer=Rules.Remaining==20&&elapsed==0&&enemies.Count==0;break;
                case 5:TickArrival(.38f);break;
                case 6:TickArrival(.27f);break;
                case 7:
                    TickArrival(.1f);presentationReport.landingFinished=!Arriving;
                    Rules.Inventory.Clear();foreach(var id in startingWeapons)Rules.Inventory.Add(catalog.Weapon(id,1));SyncEquipment();
                    playerAnimation.Tick(Vector2.right,0,true);UpdateEquipment(0);break;
                case 8:playerAnimation.Tick(Vector2.left,0,true);UpdateEquipment(0);break;
                case 9:
                    playerPos=new Vector2(-9.6f,6.8f);player.transform.position=playerPos;playerShadow.Place(playerPos,ActorPresentation.PlayerScale);UpdateEquipment(0);
                    gameCamera.transform.position=(Vector3)ArenaGeometry.CameraPosition(playerPos,PresentationLayout.Aspect)+Vector3.back*20;
                    var edgeEnemy=FeelEnemy(playerPos+new Vector2(1,-.5f));edgeEnemy.activeAge=1;UpdateEnemyPresentation(edgeEnemy,Vector2.zero,0);break;
                case 10:invulnerable=0;HurtPlayer(10000,catalog.Enemy("baby_alien"));break;
                case 11:ConfirmMenu();break;
                case 12:
                    StartSelectedRun();presentationReport.restartReset=Rules.Health==Rules.MaxHealth&&Rules.Wave==1&&!endSummary&&Arriving&&Rules.Inventory[0].id=="smg";
                    TickArrival(ArrivalMotion.Duration);
                    for(int i=1;i<=20;i++){Rules.Tick(Rules.Remaining);while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);if(Rules.Phase==RunPhase.Shop)Rules.NextWave();}
                    presentationReport.twentyWaveWin=Rules.Phase==RunPhase.Won;break;
                default:
                    ReturnToSelection(0);presentationReport.returnToTitle=Rules.Phase==RunPhase.Title&&!Arriving&&!endSummary;
                    presentationReport.capturesVerified=CaptureVerification.Fresh(presentationPath,presentationUtc,presentationCaptures);
                    bool passed=presentationReport.capturesVerified&&presentationReport.authoredScenePresent&&presentationReport.canopyInstantiated&&presentationReport.selectedWeaponStarted&&presentationReport.landingHeldTimer&&presentationReport.landingFinished&&presentationReport.restartReset&&presentationReport.returnToTitle&&presentationReport.twentyWaveWin;
                    presentationReport.status=passed?"Passed":"Failed";File.WriteAllText(Path.Combine(presentationPath,"presentation.json"),JsonUtility.ToJson(presentationReport,true));
                    Application.Quit(passed?0:1);enabled=false;return;
            }
            ScreenCapture.CaptureScreenshot(Path.Combine(presentationPath,presentationCaptures[presentationStage]+".png"));presentationStage++;
        }
    }
}
