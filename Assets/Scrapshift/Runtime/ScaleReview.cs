using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool scaleReview;
        string scalePath;
        float scaleStart;
        int scaleStage;
        void BeginScaleReview(string folder)
        {
            scaleReview=true;scalePath=folder;Directory.CreateDirectory(folder);
            scaleStart=Time.realtimeSinceStartup;reducedMotion=false;Begin();
            Rules.Inventory.Clear();SyncEquipment();playerPos=new Vector2(-1,0);
            Spawn(catalog.Enemy("baby_alien"),new Vector2(1,0));
            var enemy=enemies[enemies.Count-1];Destroy(enemy.marker);enemy.marker=null;
            enemy.obj.SetActive(true);enemy.activeAge=1;
        }
        void ScaleReviewTick()
        {
            if(!scaleReview)return;
            float t=Time.realtimeSinceStartup-scaleStart;
            player.SetActive(true);player.transform.position=new Vector3(playerPos.x,playerPos.y,-1);
            playerAnimation.Tick(t>6?Vector2.right:Vector2.zero,Time.deltaTime,false);
            playerShadow.Place(playerPos,ActorPresentation.PlayerScale);
            foreach(var enemy in enemies)
            {enemy.activeAge+=Time.deltaTime;UpdateEnemyPresentation(enemy,t>6?Vector2.right:Vector2.zero,Time.deltaTime);}
            if(scaleStage==0&&t>2){ScreenCapture.CaptureScreenshot(Path.Combine(scalePath,"gameplay-scale.png"));scaleStage++;}
            if(scaleStage==1&&t>3){gameCamera.orthographicSize=2.8f;scaleStage++;}
            if(scaleStage==2&&t>5){ScreenCapture.CaptureScreenshot(Path.Combine(scalePath,"outline-and-shadow.png"));scaleStage++;}
            if(scaleStage==3&&t>7){ScreenCapture.CaptureScreenshot(Path.Combine(scalePath,"walking-shadows.png"));scaleStage++;}
            if(t>9)
            {
                float pw=playerAnimation.bodyView.sprite.bounds.size.x*ActorPresentation.PlayerScale;
                var baby=catalog.Enemy("baby_alien");float ew=baby.image.bounds.size.x*ActorPresentation.EnemyScale(baby);
                bool passed=scaleStage==4&&ew/pw>1.1f&&ew/pw<1.4f;
                File.WriteAllText(Path.Combine(scalePath,"report.txt"),$"Status: {(passed?"Passed":"Failed")}\nStaged player/basic-enemy presentation, no AI/combat. Camera 7.5 for gameplay-scale, 2.8 for detail.\nNeutral sprite widths: player {pw:F3}, baby {ew:F3}; ratio {ew/pw:F3}.\n");
                Application.Quit(passed?0:1);enabled=false;
            }
        }
    }
}
