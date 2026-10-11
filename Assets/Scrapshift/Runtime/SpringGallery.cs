using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool springGallery;
        string springGalleryPath;
        float springGalleryStart;
        int galleryStage;
        void BeginSpringGallery(string folder)
        {
            springGallery=true;springGalleryPath=folder;Directory.CreateDirectory(folder);
            springGalleryStart=Time.realtimeSinceStartup;reducedMotion=false;Begin();
            gameCamera.orthographicSize=7.5f;Rules.Inventory.Clear();SyncEquipment();player.SetActive(false);
            string[] ids={"baby_alien","bruiser","buffer","charger","chaser","evil_mob","fin_alien","helmet_alien","horned_bruiser","horned_charger","junkie","looter","predator","pursuer","slasher","spawner","spitter","tree"};
            for(int i=0;i<ids.Length;i++)
            {
                Spawn(catalog.Enemy(ids[i]),new Vector2(-8.25f+i%6*3.3f,3.2f-i/6*2.8f));
                var enemy=enemies[enemies.Count-1];Destroy(enemy.marker);enemy.marker=null;enemy.obj.SetActive(true);enemy.activeAge=1;
            }
        }
        void SpringGalleryTick()
        {
            if(!springGallery)return;
            player.SetActive(false);
            float t=Time.realtimeSinceStartup-springGalleryStart;
            foreach(var enemy in enemies)
            {enemy.activeAge+=Time.deltaTime;UpdateEnemyPresentation(enemy,enemy.type.role=="Tree"?Vector2.zero:Vector2.right,Time.deltaTime);}
            if(galleryStage==0&&t>2){ScreenCapture.CaptureScreenshot(Path.Combine(springGalleryPath,"all-enemies.png"));galleryStage++;}
            if(galleryStage==1&&t>2.15f){ScreenCapture.CaptureScreenshot(Path.Combine(springGalleryPath,"all-enemies-stretch.png"));galleryStage++;}
            if(t>4)
            {
                File.WriteAllText(Path.Combine(springGalleryPath,"scope.txt"),"Staged gallery: 18 actual enemy renderers at fixed grid positions, code-driven movement squash; no AI/combat simulation or balance claim.\n");
                Application.Quit(enemies.Count==18?0:1);enabled=false;
            }
        }
    }
}
