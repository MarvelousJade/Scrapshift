using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool explosionParityReview;
        string explosionParityPath;
        float explosionParityNext;
        int explosionParityStage;
        DateTime explosionParityStarted;
        [Serializable] sealed class ExplosionParityReport
        {
            public string status,scope;
            public bool weaponProfiles,babyBullet,babyPiercing,weaponSmoke,fleshSmoke,structureData,capturesVerified;
            public int explosiveWeaponTiers,shredderSmoke,nuclearSmoke,fleshParticles;
            public float babyNearDamage,babyFarDamage;
        }
        ExplosionParityReport explosionParityReport;
        void BeginExplosionParityReview(string folder)
        {
            explosionParityReview=true;explosionParityPath=Path.GetFullPath(folder);
            Directory.CreateDirectory(explosionParityPath);
            explosionParityStarted=DateTime.UtcNow;
            explosionParityNext=Time.realtimeSinceStartup+.5f;
            explosionParityReport=new ExplosionParityReport{scope="Built-player checks of source explosion data, smoke, Baby With A Beard projectile art and two-target piercing."};
            sound.SetMuted(true);
        }
        void ExplosionParityReviewTick()
        {
            if(!explosionParityReview||Time.realtimeSinceStartup<explosionParityNext)return;
            if(explosionParityStage==0)
            {
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();
                player.SetActive(false);playerPos=new Vector2(0,6.5f);
                gameCamera.orthographicSize=3.5f;gameCamera.transform.position=new Vector3(0,0,-20);
                var profiles=WeaponReference.Profiles.SelectMany(p=>p.explosions??Array.Empty<ExplosionReference>()).ToArray();
                explosionParityReport.explosiveWeaponTiers=profiles.Length;
                explosionParityReport.weaponProfiles=profiles.Length==21&&profiles.All(p=>p.smokeAmount>0);
                var baby=Array.Find(catalog.data.items,i=>i.id=="baby_with_a_beard");
                Rules.Items.Add(baby);Rules.GainWeaponStat("piercing",1);
                var dead=FeelEnemy(Vector2.zero);var near=FeelEnemy(Vector2.right);var far=FeelEnemy(Vector2.right*2);
                EnemyImplantDeath(dead);
                var shot=shots.Count==1?shots[0]:null;
                explosionParityReport.babyBullet=shot?.projectileRef?.id=="projectiles/bullet/bullet_projectile.tscn"&&shot.hit.Contains(dead);
                explosionParityReport.babyPiercing=shot!=null&&shot.pierce==1;
                if(shot!=null)
                {
                    shot.velocity=Vector2.right*30;shot.damage=100;shot.crit=-100;shot.life=1;
                    shot.obj.transform.rotation=Quaternion.identity;
                    UpdateWeapons(.1f);
                    explosionParityReport.babyNearDamage=1000-near.hp;
                    explosionParityReport.babyFarDamage=1000-far.hp;
                    explosionParityReport.babyPiercing&=explosionParityReport.babyNearDamage==100&&explosionParityReport.babyFarDamage==50;
                }
                ScreenCapture.CaptureScreenshot(Path.Combine(explosionParityPath,"baby-piercing.png"));
                explosionParityStage=1;explosionParityNext=Time.realtimeSinceStartup+.3f;return;
            }
            if(explosionParityStage==1)
            {
                Begin();ClearCombat();Rules.Items.Clear();
                var shredder=catalog.Weapon("shredder",3);var nuclear=catalog.Weapon("nuclear_launcher",3);
                FeelEnemy(Vector2.zero);var victim=enemies[enemies.Count-1];
                SpawnReferenceExplosion(victim,shredder,Array.Find(shredder.traits,t=>t.key.StartsWith("effect_explode")),10);
                explosionParityReport.shredderSmoke=weaponExplosions[0].smokeAmount;
                SpawnReferenceExplosion(victim,nuclear,nuclear.Trait("effect_explode"),10);
                explosionParityReport.nuclearSmoke=weaponExplosions[1].smokeAmount;
                TickReferenceExplosions(.05f);
                explosionParityReport.weaponSmoke=explosionParityReport.shredderSmoke==9&&explosionParityReport.nuclearSmoke==80&&
                    weaponExplosions[0].smoke!=null&&weaponExplosions[0].smoke.particleCount==9&&
                    weaponExplosions[1].smoke!=null&&weaponExplosions[1].smoke.particleCount==80;
                ScreenCapture.CaptureScreenshot(Path.Combine(explosionParityPath,"weapon-smoke.png"));
                explosionParityStage=2;explosionParityNext=Time.realtimeSinceStartup+.3f;return;
            }
            if(explosionParityStage==2)
            {
                Begin();ClearCombat();Rules.Items.Clear();
                var rip=Array.Find(catalog.data.items,i=>i.id=="rip_and_tear");
                Rules.Items.Add(rip);Rules.Items.Add(rip);
                for(int i=0;i<100&&implantExplosions.Count==0;i++)QueueImplantExplosions("explode_on_death",Vector2.zero);
                bool oneExplosion=implantExplosions.Count==1;
                ResolveImplantAttacks();TickReferenceExplosions(.05f);
                var flesh=weaponExplosions.Count==1?weaponExplosions[0]:null;
                explosionParityReport.fleshParticles=flesh?.smoke?.particleCount??0;
                explosionParityReport.fleshSmoke=oneExplosion&&flesh?.flesh==true&&explosionParityReport.fleshParticles==1&&
                    Resources.Load<Texture2D>("WeaponReference/particle_1")!=null;
                var rocket=Array.Find(catalog.data.items,i=>i.id=="turret_rocket");
                var landmines=Array.Find(catalog.data.items,i=>i.id=="landmines");
                var rocketSpec=Array.Find(rocket.conditionalEffects,e=>e.structure!=null)?.structure;
                var mineSpec=Array.Find(landmines.conditionalEffects,e=>e.structure!=null)?.structure;
                explosionParityReport.structureData=rocketSpec?.explosionSmokeAmount==20&&mineSpec?.explosionSmokeAmount==70;
                ScreenCapture.CaptureScreenshot(Path.Combine(explosionParityPath,"flesh-smoke.png"));
                explosionParityStage=3;explosionParityNext=Time.realtimeSinceStartup+.5f;return;
            }
            explosionParityReport.capturesVerified=CaptureVerification.Fresh(explosionParityPath,explosionParityStarted,"baby-piercing","weapon-smoke","flesh-smoke");
            bool passed=explosionParityReport.weaponProfiles&&explosionParityReport.babyBullet&&explosionParityReport.babyPiercing&&
                explosionParityReport.weaponSmoke&&explosionParityReport.fleshSmoke&&explosionParityReport.structureData&&explosionParityReport.capturesVerified;
            explosionParityReport.status=passed?"Passed":"Failed";
            File.WriteAllText(Path.Combine(explosionParityPath,"report.json"),JsonUtility.ToJson(explosionParityReport,true));
            Application.Quit(passed?0:1);explosionParityReview=false;
        }
    }
}
