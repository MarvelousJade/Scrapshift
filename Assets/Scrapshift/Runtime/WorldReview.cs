using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool worldReview,worldOutfit;
        string worldPath;
        float worldStart,maxWorldX,maxWorldY,maxCameraX,maxCameraY;
        int worldCapture;
        void BeginWorldReview(string folder)
        {
            worldReview=true;worldPath=folder;Directory.CreateDirectory(folder);
            worldStart=Time.realtimeSinceStartup;reducedMotion=false;Begin();
        }
        Vector2 WorldReviewMovement()
        {
            float t=Time.realtimeSinceStartup-worldStart;
            return t<3?Vector2.right:t<7?Vector2.up:t<12?Vector2.left:Vector2.down;
        }
        void WorldReviewTick()
        {
            if(!worldReview)return;
            float t=Time.realtimeSinceStartup-worldStart;
            Rules.Heal(100); // Rendering fixture only; ordinary runs do not restore health.
            maxWorldX=Mathf.Max(maxWorldX,Mathf.Abs(playerPos.x));maxWorldY=Mathf.Max(maxWorldY,Mathf.Abs(playerPos.y));
            maxCameraX=Mathf.Max(maxCameraX,Mathf.Abs(gameCamera.transform.position.x));maxCameraY=Mathf.Max(maxCameraY,Mathf.Abs(gameCamera.transform.position.y));
            if(!worldOutfit&&t>8)
            {
                foreach(string id in new[]{"smg","double_barrel_shotgun","laser_gun","knife","spear"})Rules.Inventory.Add(catalog.Weapon(id,1));
                foreach(string id in new[]{"beanie","glasses","leather_vest"})Rules.Items.Add(System.Array.Find(catalog.data.items,i=>i.id==id));
                SyncEquipment();worldOutfit=true;
                for(int i=0;i<15;i++)Drop(playerPos+new Vector2((i%5-2)*.4f,-.4f-i/5*.4f),1,false);
            }
            float[] times={2.5f,6.5f,10,16};string[] names={"single-weapon-east","north-map-edge","six-weapons-items","south-map-edge"};
            if(worldCapture<times.Length&&t>times[worldCapture])
            {ScreenCapture.CaptureScreenshot(Path.Combine(worldPath,names[worldCapture]+".png"));worldCapture++;}
            if(t>18)
            {
                bool passed=worldCapture==4&&maxWorldX>9&&maxWorldY>7&&maxCameraX>2&&maxCameraY>3&&shotsFired>0&&worldOutfit;
                File.WriteAllText(Path.Combine(worldPath,"report.txt"),$"Status: {(passed?"Passed":"Failed")}\nStaged normal-camera combat; scripted four-direction movement, restored HP and supplied six-weapon/three-item outfit. Not balance or purchase validation.\nPlayer maximum travel: {maxWorldX:F2}, {maxWorldY:F2}\nCamera maximum travel: {maxCameraX:F2}, {maxCameraY:F2}\nShots: {shotsFired}; peak enemies: {peakEnemies}; captures: {worldCapture}\n");
                Application.Quit(passed?0:1);enabled=false;
            }
        }
    }
}
