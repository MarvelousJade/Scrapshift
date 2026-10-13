using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool weaponFidelityReview;string weaponReviewPath;int weaponReviewStage;float weaponReviewNext;
        readonly List<string> weaponReviewPassed=new List<string>(),weaponReviewFailed=new List<string>();
        GameObject projectileSample;
        [Serializable] sealed class WeaponReviewReport {public string status,scope="All 205 weapon/tier attack checks against stationary targets; staged projectile and melee captures. Not a full survival/balance equivalence claim.";public string[] passed,failed;}
        void BeginWeaponFidelityReview(string folder)
        {weaponFidelityReview=true;weaponReviewPath=Path.GetFullPath(folder);Directory.CreateDirectory(weaponReviewPath);sound.SetMuted(true);weaponReviewNext=Time.realtimeSinceStartup+.5f;}
        void WeaponFidelityReviewTick()
        {
            if(!weaponFidelityReview||Time.realtimeSinceStartup<weaponReviewNext)return;
            if(weaponReviewStage<catalog.data.equipment.Length)
            {
                var w=catalog.data.equipment[weaponReviewStage++];Begin();ClearCombat();Rules.Inventory.Clear();Rules.Inventory.Add(w);Rules.Items.Clear();SyncEquipment();
                arrivalAge=ArrivalMotion.Duration;playerPos=Vector2.zero;
                var targets=new List<Enemy>();for(int i=0;i<8;i++)targets.Add(FeelEnemy(Quaternion.Euler(0,0,i*45)*Vector2.right*.9f));
                for(int step=0;step<480;step++){UpdateEquipment(1f/120);UpdateWeapons(1f/120);UpdateStarterEffects(1f/120);}
                bool passed=targets.Any(e=>e.hp<1000);(passed?weaponReviewPassed:weaponReviewFailed).Add(w.id+":"+w.tier);
                Debug.Log("Weapon reference fixture "+w.id+":"+w.tier+" "+passed);return;
            }
            int stage=weaponReviewStage++-catalog.data.equipment.Length;
            if(stage<WeaponReference.Projectiles.Length)
            {
                Begin();ClearCombat();Rules.Inventory.Clear();SyncEquipment();player.SetActive(false);if(projectileSample!=null)Destroy(projectileSample);
                var reference=WeaponReference.Projectiles[stage];projectileSample=SpriteObject("Reference projectile sample",reference.Frame(.06f),reference.beam?Vector2.left*2:Vector2.zero,13);
                var view=projectileSample.AddComponent<WeaponProjectileView>();view.Initialize(reference,3);view.Pose(.06f);
                gameCamera.orthographicSize=1.8f;gameCamera.transform.position=new Vector3(0,0,-20);
                ScreenCapture.CaptureScreenshot(Path.Combine(weaponReviewPath,"projectile-"+stage+".png"));weaponReviewNext=Time.realtimeSinceStartup+.15f;return;
            }
            stage-=WeaponReference.Projectiles.Length;
            if(stage<9)
            {
                if(projectileSample!=null)Destroy(projectileSample);
                string id=new[]{"knife","sword","circular_saw"}[stage/3];var w=catalog.data.equipment.First(x=>x.id==id);
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Inventory.Add(w);SyncEquipment();arrivalAge=ArrivalMotion.Duration;playerPos=Vector2.zero;
                weaponShots[0]=1;BeginMeleeAttack(0,w,Vector2.right,w.range);var a=meleeAttacks[0];
                float time=stage%3==0?a.recoilTime*.5f:stage%3==1?a.recoilTime+a.strike*.5f:a.recoilTime+a.strike+a.back*.5f;
                TickMeleeAttack(0,w,Vector2.zero,time);gameCamera.orthographicSize=2.5f;gameCamera.transform.position=new Vector3(0,0,-20);
                ScreenCapture.CaptureScreenshot(Path.Combine(weaponReviewPath,id+"-"+stage%3+".png"));weaponReviewNext=Time.realtimeSinceStartup+.15f;return;
            }
            File.WriteAllText(Path.Combine(weaponReviewPath,"weapons.json"),JsonUtility.ToJson(new WeaponReviewReport{status=weaponReviewFailed.Count==0?"Passed":"Failed",passed=weaponReviewPassed.ToArray(),failed=weaponReviewFailed.ToArray()},true));
            Application.Quit(weaponReviewFailed.Count==0?0:1);weaponFidelityReview=false;
        }
    }
}
