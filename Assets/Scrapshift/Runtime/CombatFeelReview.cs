using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool combatFeelReview;
        string combatFeelPath;
        float combatFeelStart;
        int combatFeelStage;
        DateTime combatFeelStartedUtc;
        Enemy feelNear, feelFar;
        [Serializable] sealed class CombatFeelReport
        {
            public string status, scope;
            public bool capturesVerified, northShotSurvived, nearestFirst, whiteFlash, pushDisplaced,
                outsideContactSafe, insideContactHurt, contactSlow, secondHitBlocked, playerWhiteFlash;
            public float nearDamage, farDamage, pushDistance, invulnerabilitySeconds;
        }
        CombatFeelReport feelReport;
        Enemy FeelEnemy(Vector2 position)
        {
            Spawn(catalog.Enemy("baby_alien"), position);
            var e = enemies[enemies.Count - 1]; Destroy(e.marker); e.marker = null;
            e.obj.SetActive(true); e.hp = e.maxHp = 1000; e.speed = 0;
            UpdateEnemyPresentation(e, Vector2.zero, 0); return e;
        }
        void BeginCombatFeelReview(string folder)
        {
            combatFeelReview = true; combatFeelPath = folder; Directory.CreateDirectory(folder);
            combatFeelStart = Time.realtimeSinceStartup;combatFeelStartedUtc=DateTime.UtcNow;
            Begin(); ClearCombat(); Rules.Inventory.Clear(); SyncEquipment(); sound.SetMuted(true);
            playerPos = new Vector2(0,6.5f); player.transform.position = playerPos;
            player.SetActive(true); playerAnimation.Tick(Vector2.zero,0,false); playerShadow.Place(playerPos,ActorPresentation.PlayerScale);
            gameCamera.transform.position = (Vector3)ArenaGeometry.CameraPosition(playerPos,PresentationLayout.Aspect)+Vector3.back*20;
            feelReport = new CombatFeelReport { scope = "Controlled runtime checks with fixed enemies, supplied shots and 1000 enemy HP. Normal camera; simulation advanced explicitly for reproducible impact captures. Not normal-run balance or a human playthrough." };
        }
        void CombatFeelReviewTick()
        {
            if (!combatFeelReview) return;
            float t = Time.realtimeSinceStartup - combatFeelStart;
            if (combatFeelStage == 0 && t > .5f)
            {
                var edge = new Shot { obj=SpriteObject("North edge test shot",catalog.muzzleFrames[2],playerPos), pos=playerPos,
                    velocity=Vector2.up,life=1,damage=1,crit=-100 };
                shots.Add(edge); UpdateWeapons(.05f); feelReport.northShotSurvived=shots.Contains(edge);
                Destroy(edge.obj);shots.Remove(edge);
                feelNear=FeelEnemy(playerPos+Vector2.right);feelFar=FeelEnemy(playerPos+Vector2.right*2);
                shots.Add(new Shot { obj=SpriteObject("Piercing test",catalog.muzzleFrames[2],playerPos),pos=playerPos,
                    velocity=Vector2.right*30,life=1,damage=100,crit=-100,knockback=1.5f,pierce=1 });
                UpdateWeapons(.1f);
                feelReport.nearDamage=1000-feelNear.hp;feelReport.farDamage=1000-feelFar.hp;
                feelReport.nearestFirst=feelReport.nearDamage==100&&feelReport.farDamage==50;
                var block=new MaterialPropertyBlock();feelNear.renderers[0].GetPropertyBlock(block);
                feelReport.whiteFlash=block.GetFloat("_Flash")==1&&impacts.Count==2;
                ScreenCapture.CaptureScreenshot(Path.Combine(combatFeelPath,"piercing-impact.png"));combatFeelStage++;
            }
            else if (combatFeelStage == 1 && t > 1.3f)
            {
                float x=feelNear.pos.x;UpdateFeedback(.1f);UpdateEnemies(.1f);
                feelReport.pushDistance=feelNear.pos.x-x;
                feelReport.pushDisplaced=feelReport.pushDistance>1;
                ScreenCapture.CaptureScreenshot(Path.Combine(combatFeelPath,"knockback.png"));combatFeelStage++;
            }
            else if (combatFeelStage == 2 && t > 2)
            {
                ClearCombat();playerPos=Vector2.zero;player.transform.position=playerPos;playerShadow.Place(playerPos,ActorPresentation.PlayerScale);
                gameCamera.transform.position=Vector3.back*20;
                var contact=FeelEnemy(new Vector2(.4f,0));contact.speed=2.5f;
                float health=Rules.Health;invulnerable=0;UpdateEnemies(0);
                feelReport.outsideContactSafe=Rules.Health==health;
                contact.pos=new Vector2(.35f,0);UpdateEnemies(0);
                feelReport.insideContactHurt=Rules.Health<health;feelReport.contactSlow=contact.contactSlow>0;
                feelReport.invulnerabilitySeconds=invulnerable;
                float after=Rules.Health;HurtPlayer(1);feelReport.secondHitBlocked=Rules.Health==after;
                UpdatePlayerFlash(0);var block=new MaterialPropertyBlock();playerAnimation.bodyView.GetPropertyBlock(block);
                feelReport.playerWhiteFlash=block.GetFloat("_Flash")==1;
                ScreenCapture.CaptureScreenshot(Path.Combine(combatFeelPath,"player-hit.png"));combatFeelStage++;
            }
            else if (combatFeelStage == 3 && t > 2.8f)
            {
                feelReport.capturesVerified=CaptureVerification.Fresh(combatFeelPath,combatFeelStartedUtc,"piercing-impact","knockback","player-hit");
                bool passed=feelReport.capturesVerified&&feelReport.northShotSurvived&&feelReport.nearestFirst&&feelReport.whiteFlash&&feelReport.pushDisplaced
                    &&feelReport.outsideContactSafe&&feelReport.insideContactHurt&&feelReport.contactSlow&&feelReport.secondHitBlocked&&feelReport.playerWhiteFlash;
                feelReport.status=passed?"Passed":"Failed";
                File.WriteAllText(Path.Combine(combatFeelPath,"combat-feel.json"),JsonUtility.ToJson(feelReport,true));
                Application.Quit(passed?0:1);enabled=false;
            }
        }
    }
}
