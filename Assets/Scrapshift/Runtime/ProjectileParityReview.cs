using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool projectileParityReview;
        string projectileParityPath;
        float projectileParityNext;
        int projectileParityStage;
        DateTime projectileParityStarted;
        [Serializable] sealed class ProjectileParityReport
        {
            public string status,scope;
            public bool extraShots,spread,bounceAtContact,bounceDamage,bounceSecondHit,pierceDamage,capturesVerified;
            public float bouncePosition,firstDamage,secondDamage,pierceFirstDamage,pierceSecondDamage;
        }
        ProjectileParityReport projectileParityReport;
        void BeginProjectileParityReview(string folder)
        {
            projectileParityReview=true;projectileParityPath=Path.GetFullPath(folder);Directory.CreateDirectory(projectileParityPath);
            projectileParityStarted=DateTime.UtcNow;projectileParityNext=Time.realtimeSinceStartup+.5f;
            projectileParityReport=new ProjectileParityReport{scope="Built-player check of source projectile count/spread, contact-point bounce, integer bounce loss, and Pumpkin piercing bonus."};
            sound.SetMuted(true);
        }
        void ProjectileParityReviewTick()
        {
            if(!projectileParityReview||Time.realtimeSinceStartup<projectileParityNext)return;
            if(projectileParityStage==0)
            {
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();
                player.SetActive(false);playerPos=new Vector2(0,6.5f);
                gameCamera.orthographicSize=3.5f;gameCamera.transform.position=new Vector3(0,0,-20);
                var pistol=catalog.Weapon("pistol",1);
                Rules.GainWeaponStat("projectiles",2);Rules.Inventory.Add(pistol);SyncEquipment();
                arrivalAge=ArrivalMotion.Duration;playerPos=Vector2.zero;FeelEnemy(Vector2.right*2);
                int before=shotsFired;
                for(int i=0;i<120&&shotsFired==before;i++)UpdateEquipment(1f/120);
                projectileParityReport.extraShots=shotsFired-before==3&&WeaponReference.ProjectileCount(0,2)==0;
                projectileParityReport.spread=Mathf.Abs(WeaponReference.Spread(pistol,2,1)-(pistol.spread+.2f))<.0001f;
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();
                var near=FeelEnemy(Vector2.right);FeelEnemy(Vector2.right*2);
                var shot=new Shot{obj=SpriteObject("Bounce sample",catalog.muzzleFrames[2],Vector2.zero,13),pos=Vector2.zero,
                    velocity=Vector2.right*30,life=1,damage=11,crit=-100,weapon=pistol,bounces=1};
                shots.Add(shot);UpdateWeapons(.1f);
                projectileParityReport.bouncePosition=shot.pos.x;
                projectileParityReport.firstDamage=1000-near.hp;
                projectileParityReport.bounceAtContact=shots.Contains(shot)&&shot.pos.x>0&&shot.pos.x<1&&shot.bounces==0;
                projectileParityReport.bounceDamage=shot.damage==5&&projectileParityReport.firstDamage==11;
                ScreenCapture.CaptureScreenshot(Path.Combine(projectileParityPath,"bounce-contact.png"));
                projectileParityStage=1;projectileParityNext=Time.realtimeSinceStartup+.3f;return;
            }
            if(projectileParityStage==1)
            {
                UpdateWeapons(.1f);
                projectileParityReport.secondDamage=1000-enemies[1].hp;
                projectileParityReport.bounceSecondHit=projectileParityReport.secondDamage==5;
                Begin();ClearCombat();Rules.Items.Clear();Rules.GainWeaponStat("piercing_damage",15);
                var near=FeelEnemy(Vector2.right);var far=FeelEnemy(Vector2.right*2);
                shots.Add(new Shot{obj=SpriteObject("Piercing bonus sample",catalog.muzzleFrames[2],Vector2.zero,13),pos=Vector2.zero,
                    velocity=Vector2.right*30,life=1,damage=11,crit=-100,weapon=catalog.Weapon("pistol",1),pierce=1});
                UpdateWeapons(.1f);
                projectileParityReport.pierceFirstDamage=1000-near.hp;
                projectileParityReport.pierceSecondDamage=1000-far.hp;
                projectileParityReport.pierceDamage=projectileParityReport.pierceFirstDamage==11&&projectileParityReport.pierceSecondDamage==7;
                ScreenCapture.CaptureScreenshot(Path.Combine(projectileParityPath,"piercing-bonus.png"));
                projectileParityStage=2;projectileParityNext=Time.realtimeSinceStartup+.5f;return;
            }
            projectileParityReport.capturesVerified=CaptureVerification.Fresh(projectileParityPath,projectileParityStarted,"bounce-contact","piercing-bonus");
            bool passed=projectileParityReport.extraShots&&projectileParityReport.spread&&projectileParityReport.bounceAtContact&&
                projectileParityReport.bounceDamage&&projectileParityReport.bounceSecondHit&&projectileParityReport.pierceDamage&&projectileParityReport.capturesVerified;
            projectileParityReport.status=passed?"Passed":"Failed";
            File.WriteAllText(Path.Combine(projectileParityPath,"report.json"),JsonUtility.ToJson(projectileParityReport,true));
            Application.Quit(passed?0:1);projectileParityReview=false;
        }
    }
}
