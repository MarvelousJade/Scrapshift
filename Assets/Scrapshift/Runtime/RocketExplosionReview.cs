using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool rocketExplosionReview;
        string rocketReviewPath;
        int rocketReviewStage;
        float rocketReviewNext;
        bool rocketAreaPassed,rocketFlashPassed,rocketSmokePassed,rocketCleanupPassed;
        int rocketParticleCount;
        bool rocketSmokeTriggered,rocketTextureLoaded;
        int rocketSmokeConfigured;
        [Serializable] sealed class RocketReport
        {
            public string status,scope;
            public bool area,flash,smoke,cleanup;
            public bool smokeTriggered,textureLoaded;
            public float hitRadius,smokeRadius,smokeLifetime;
            public int smokeParticles,smokeConfigured;
        }
        void BeginRocketExplosionReview(string folder)
        {
            rocketExplosionReview=true;
            rocketReviewPath=Path.GetFullPath(folder);
            Directory.CreateDirectory(rocketReviewPath);
            sound.SetMuted(true);
            rocketReviewNext=Time.realtimeSinceStartup+.5f;
        }
        void RocketExplosionReviewTick()
        {
            if(!rocketExplosionReview||Time.realtimeSinceStartup<rocketReviewNext)return;
            if(rocketReviewStage==0)
            {
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();SyncEquipment();
                player.SetActive(false);playerPos=new Vector2(0,6.5f);
                gameCamera.orthographicSize=3.5f;gameCamera.transform.position=new Vector3(0,0,-20);
                var rocket=catalog.Weapon("rocket_launcher",2);
                var victim=FeelEnemy(Vector2.zero);
                var inside=FeelEnemy(new Vector2(1.4f,0));
                var outside=FeelEnemy(new Vector2(2.5f,0));
                SpawnReferenceExplosion(victim,rocket,rocket.Trait("effect_explode"),20);
                TickReferenceExplosions(.01f);
                var effect=weaponExplosions[0];
                rocketAreaPassed=inside.hp<1000&&outside.hp==1000&&Mathf.Abs(effect.radius-1.76808f)<.0001f;
                rocketFlashPassed=effect.obj!=null&&effect.obj.GetComponent<SpriteRenderer>().enabled&&effect.age<.05f;
                ScreenCapture.CaptureScreenshot(Path.Combine(rocketReviewPath,"rocket-flash.png"));
                rocketReviewStage=1;rocketReviewNext=Time.realtimeSinceStartup+.25f;return;
            }
            if(rocketReviewStage==1)
            {
                TickReferenceExplosions(.04f);
                var effect=weaponExplosions[0];
                var smoke=effect.smoke;
                rocketSmokeTriggered=effect.finished;
                rocketSmokeConfigured=effect.smokeAmount;
                rocketTextureLoaded=Resources.Load<Texture2D>("WeaponReference/particle_3")!=null;
                rocketParticleCount=smoke!=null?smoke.particleCount:0;
                rocketSmokePassed=effect.finished&&!effect.obj.GetComponent<SpriteRenderer>().enabled&&smoke!=null&&rocketParticleCount==48&&
                    Mathf.Abs(smoke.shape.radius-1.536f)<.0001f&&Mathf.Abs(smoke.main.startLifetime.constant-.75f)<.0001f;
                ScreenCapture.CaptureScreenshot(Path.Combine(rocketReviewPath,"rocket-smoke.png"));
                rocketReviewStage=2;rocketReviewNext=Time.realtimeSinceStartup+.35f;return;
            }
            if(rocketReviewStage==2)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(rocketReviewPath,"rocket-smoke-mid.png"));
                rocketReviewStage=3;rocketReviewNext=Time.realtimeSinceStartup+.25f;return;
            }
            var last=weaponExplosions[0];
            float radius=last.radius,smokeRadius=last.smoke!=null?last.smoke.shape.radius:0,lifetime=last.smoke!=null?last.smoke.main.startLifetime.constant:0;
            TickReferenceExplosions(.951f);
            rocketCleanupPassed=weaponExplosions.Count==0;
            var report=new RocketReport{status=rocketAreaPassed&&rocketFlashPassed&&rocketSmokePassed&&rocketCleanupPassed?"Passed":"Failed",
                scope="Built-player rocket explosion check against source scene dimensions and animation timing; screenshots are staged captures.",
                area=rocketAreaPassed,flash=rocketFlashPassed,smoke=rocketSmokePassed,cleanup=rocketCleanupPassed,
                smokeTriggered=rocketSmokeTriggered,textureLoaded=rocketTextureLoaded,smokeConfigured=rocketSmokeConfigured,
                hitRadius=radius,smokeRadius=smokeRadius,smokeLifetime=lifetime,smokeParticles=rocketParticleCount};
            File.WriteAllText(Path.Combine(rocketReviewPath,"report.json"),JsonUtility.ToJson(report,true));
            Application.Quit(report.status=="Passed"?0:1);rocketExplosionReview=false;
        }
    }
}
