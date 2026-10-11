using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool enemyReview;
        string enemyReviewPath;
        float enemyReviewStart;
        DateTime enemyReviewStartedUtc;
        int enemyReviewStep;
        bool enemyWarnSeen, chargeWindupSeen, rangedWindupSeen, dashSeen, hostileSeen;

        [Serializable] sealed class EnemyReviewReport
        {
            public string status, scope;
            public bool capturesVerified, warning, chargeWindup, rangedWindup, dash, hostileProjectile;
            public int charges, rangedAttacks, kills;
        }

        void BeginEnemyReview(string folder)
        {
            enemyReview = true; enemyReviewPath = folder; Directory.CreateDirectory(folder);
            enemyReviewStart = Time.realtimeSinceStartup; enemyReviewStartedUtc=DateTime.UtcNow;
            Begin(); ClearCombat(); Rules.Inventory.Clear(); SyncEquipment();
            director = new SpawnDirector(new WaveEntry { groups = Array.Empty<SpawnGroup>() }, 1);
            Spawn(catalog.Enemy("charger"), new Vector2(5, 0));
            Spawn(catalog.Enemy("spitter"), new Vector2(-4, 0));
            Spawn(catalog.Enemy("buffer"), new Vector2(-1, 3.8f));
            Spawn(catalog.Enemy("bruiser"), new Vector2(5.5f, 2));
            chargeReleases = rangedReleases = 0;
        }

        void EnemyReviewTick()
        {
            if (!enemyReview) return;
            float t = Time.realtimeSinceStartup - enemyReviewStart;
            Rules.Heal(100); // Staged attack-readability fixture, never ordinary gameplay.
            foreach (var enemy in enemies)
            {
                enemyWarnSeen |= enemy.marker != null;
                if (enemy.attack != null)
                {
                    bool winding = enemy.attack.Phase == AttackPhase.Windup;
                    chargeWindupSeen |= winding && enemy.type.role == "Charge";
                    rangedWindupSeen |= winding && enemy.type.role == "Shooter";
                    dashSeen |= enemy.attack.Phase == AttackPhase.Active;
                }
            }
            hostileSeen |= shots.Exists(s => s.hostile);
            if (enemyReviewStep == 0 && t > .5f)
            { ScreenCapture.CaptureScreenshot(Path.Combine(enemyReviewPath,"warnings.png")); enemyReviewStep++; }
            if (enemyReviewStep == 1 && enemies.Exists(e => e.attack != null && e.type.role == "Charge" && e.attack.Phase == AttackPhase.Windup && e.attack.Progress > .3f))
            { ScreenCapture.CaptureScreenshot(Path.Combine(enemyReviewPath,"charge.png")); enemyReviewStep++; }
            if (enemyReviewStep == 2 && enemies.Exists(e => e.attack != null && e.type.role == "Shooter" && e.attack.Phase == AttackPhase.Windup && e.attack.Progress > .4f))
            { ScreenCapture.CaptureScreenshot(Path.Combine(enemyReviewPath,"ranged.png")); enemyReviewStep++; }
            if (enemyReviewStep == 3 && hostileSeen)
            { ScreenCapture.CaptureScreenshot(Path.Combine(enemyReviewPath,"projectiles.png")); enemyReviewStep++; }
            if (enemyReviewStep == 4 && t > 7)
            {
                foreach(string id in new[]{"pistol","smg","double_barrel_shotgun","laser_gun","knife","spear"})
                    Rules.Inventory.Add(catalog.Weapon(id,2));
                SyncEquipment(); enemyReviewStep++;
            }
            if (enemyReviewStep == 5 && t > 9)
            { ScreenCapture.CaptureScreenshot(Path.Combine(enemyReviewPath,"combat.png")); enemyReviewStep++; }
            if (t < 12) return;
            bool captures=CaptureVerification.Fresh(enemyReviewPath,enemyReviewStartedUtc,"warnings","charge","ranged","projectiles","combat");
            bool passed = captures && enemyWarnSeen && chargeWindupSeen && rangedWindupSeen && dashSeen && hostileSeen &&
                chargeReleases > 0 && rangedReleases > 0 && Rules.Kills > 0 && enemyReviewStep == 6;
            var report = new EnemyReviewReport { status=passed?"Passed":"Failed",
                scope="Staged attack fixture: fixed enemies, initially unarmed stationary player, restored health, supplied six-weapon loadout after seven seconds. No balance/manual-input claim.",
                capturesVerified=captures,warning=enemyWarnSeen,chargeWindup=chargeWindupSeen,rangedWindup=rangedWindupSeen,dash=dashSeen,
                hostileProjectile=hostileSeen,charges=chargeReleases,rangedAttacks=rangedReleases,kills=Rules.Kills };
            File.WriteAllText(Path.Combine(enemyReviewPath,"enemy-review.json"),JsonUtility.ToJson(report,true));
            Application.Quit(passed?0:1); enabled=false;
        }
    }
}
