using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool campaignCheck, campaignFinished;
        string campaignPath;
        float campaignStart, campaignFinishAt;
        DateTime campaignStartedUtc;
        int campaignCompleted, campaignPurchases;
        readonly HashSet<string> campaignEnemyIds = new HashSet<string>();
        readonly List<WaveCheck> campaignWaves = new List<WaveCheck>();
        readonly HashSet<int> campaignCaptures = new HashSet<int>();

        [Serializable] sealed class WaveCheck { public int wave,kills,shots; public float simulatedSeconds; }
        [Serializable] sealed class CampaignReport
        {
            public string status,scope,phase;
            public bool capturesVerified;
            public int completedWaves,purchases,peakEnemies,charges,rangedAttacks,bossPhaseChanges;
            public float wallSeconds;
            public string[] enemyIds;
            public WaveCheck[] waves;
        }

        void BeginCampaignCheck(string folder)
        {
            campaignCheck=true;campaignPath=folder;Directory.CreateDirectory(folder);
            campaignStart=Time.realtimeSinceStartup;campaignStartedUtc=DateTime.UtcNow;
            UnityEngine.Random.InitState(771);
            placementRandom=new System.Random(771);
            Begin();
            // Explicit fixture loadout and invulnerability. Normal runs never enter this branch.
            Rules.Inventory.Clear();
            foreach(string id in new[]{"pistol","smg","double_barrel_shotgun","laser_gun","knife","spear"})
                Rules.Inventory.Add(catalog.Weapon(id,2));
            SyncEquipment();sound.SetMuted(true);
        }

        void CampaignCheckTick()
        {
            if(!campaignCheck)return;
            if(campaignFinished)
            {
                if(Time.realtimeSinceStartup-campaignFinishAt<1)return;
                bool captures=CaptureVerification.Fresh(campaignPath,campaignStartedUtc,"wave-01","wave-05","wave-10","wave-15","wave-20","end");
                bool passed=captures&&Rules.Phase==RunPhase.Won&&campaignCompleted==20&&bossMutations>0&&campaignPurchases>0;
                var report=new CampaignReport { status=passed?"Passed":"Failed",phase=Rules.Phase.ToString(),capturesVerified=captures,
                    scope="Accelerated Windows integration fixture: eight 50ms simulation steps per rendered frame, seeded spawn placement, scripted movement, supplied six tier-2 weapons, invulnerability and mute. Uses actual wave schedules, combat, queued upgrades and affordable shop purchases. Not normal-run balance, performance benchmarking or a human playthrough.",
                    completedWaves=campaignCompleted,purchases=campaignPurchases,peakEnemies=peakEnemies,
                    charges=chargeReleases,rangedAttacks=rangedReleases,bossPhaseChanges=bossMutations,wallSeconds=Time.realtimeSinceStartup-campaignStart,
                    enemyIds=campaignEnemyIds.OrderBy(x=>x).ToArray(),waves=campaignWaves.ToArray() };
                File.WriteAllText(Path.Combine(campaignPath,"campaign-check.json"),JsonUtility.ToJson(report,true));
                Application.Quit(passed?0:1);enabled=false;return;
            }
            for(int step=0;step<8;step++)
            {
                if(Rules.Phase==RunPhase.Playing)
                {
                    invulnerable=.3f;UpdateFeedback(.05f);Simulate(.05f);
                    if(Rules.Remaining<RunRules.Durations[Rules.Wave-1]*.5f&&
                        (Rules.Wave==1||Rules.Wave==5||Rules.Wave==10||Rules.Wave==15||Rules.Wave==20)&&campaignCaptures.Add(Rules.Wave))
                    { ScreenCapture.CaptureScreenshot(Path.Combine(campaignPath,$"wave-{Rules.Wave:00}.png"));break; }
                }
                if(Rules.Phase!=RunPhase.Playing&&campaignCompleted<Rules.Wave)
                {
                    campaignCompleted=Rules.Wave;
                    campaignWaves.Add(new WaveCheck { wave=Rules.Wave,kills=Rules.Kills,shots=shotsFired,simulatedSeconds=elapsed });
                }
                while(Rules.Phase==RunPhase.Loot)Rules.ResolveCrate(true);
                while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);
                if(Rules.Phase==RunPhase.Shop)
                {
                    for(int i=0;i<4;i++)if(Rules.CanBuy(i)&&Rules.Buy(i))campaignPurchases++;
                    AdvanceWave();
                }
                if(Rules.Phase==RunPhase.Won||Rules.Phase==RunPhase.Lost)
                {
                    campaignFinished=true;campaignFinishAt=Time.realtimeSinceStartup;
                    ScreenCapture.CaptureScreenshot(Path.Combine(campaignPath,"end.png"));break;
                }
            }
        }
    }
}
