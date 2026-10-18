using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool campaignCheck, campaignFinished,campaignRealtime,campaignPreflight;
        double campaignAccumulator;
        int PickupPhysicsRate=>campaignCheck&&!campaignRealtime?240:60;
        string campaignPath;
        float campaignStart, campaignFinishAt;
        DateTime campaignStartedUtc;
        int campaignCompleted, campaignPurchases,campaignShops,campaignUpgrades,campaignCrates,campaignStartedWave;
        float campaignWaveStart;
        readonly List<CampaignEvent> campaignEvents=new List<CampaignEvent>();
        readonly HashSet<string> campaignEnemyIds = new HashSet<string>();
        readonly List<WaveCheck> campaignWaves = new List<WaveCheck>();
        readonly HashSet<int> campaignCaptures = new HashSet<int>();

        [Serializable] sealed class CampaignStack {public string id;public int count;}
        [Serializable] sealed class CampaignWeapon {public string id;public int tier;}
        [Serializable] sealed class CampaignOffer {public string id;public int price;public bool locked;}
        [Serializable] sealed class CampaignEvent
        {
            public string @event,phase,detail;
            public int settlementWave,materialsBeforeSettlement,materialsAfterSettlement;
            public float harvestingAtSettlement;
            public int wave,materials,materialsBefore,level,itemCount,price,kills,bankedMaterials,materialObjects,groundMaterialValue;
            public long enemyKills,materialPickups;
            public float wallSeconds,simulatedSeconds,waveSimulatedSeconds,health,maxHealth,remaining,harvesting,luck;
            public CampaignWeapon[] weapons;
            public CampaignStack[] itemStacks;
            public CampaignOffer[] offers;
        }
        [Serializable] sealed class CampaignTrace {public CampaignEvent[] events;}
        [Serializable] sealed class WaveCheck { public int wave,kills,shots,materials,level,itemCount,bankedMaterials,materialObjects,groundMaterialValue; public long enemyKills,materialPickups;public float simulatedSeconds,waveSimulatedSeconds,harvesting,luck; }
        [Serializable] sealed class CampaignReport
        {
            public string status,scope,phase;
            public bool capturesVerified,realtime,preflight;
            public int pickupPhysicsRate;
            public int completedWaves,purchases,shops,upgrades,crates,peakEnemies,charges,rangedAttacks,bossPhaseChanges,finalMaterials,finalLevel,finalItemCount,finalBankedMaterials,finalKills;
            public long finalEnemyKills,finalMaterialPickups;
            public float wallSeconds,finalHarvesting,finalLuck;
            public string[] enemyIds;
            public WaveCheck[] waves;
            public CampaignWeapon[] finalWeapons;
            public CampaignStack[] finalItemStacks;
        }

        void BeginCampaignCheck(string folder)
        {
            campaignCheck=true;campaignPath=folder;Directory.CreateDirectory(folder);
            campaignRealtime=Array.IndexOf(Environment.GetCommandLineArgs(),"--campaign-realtime")>=0;
            campaignPreflight=Array.IndexOf(Environment.GetCommandLineArgs(),"--campaign-preflight")>=0;
            campaignAccumulator=0;
            if(campaignRealtime){Application.targetFrameRate=60;QualitySettings.vSyncCount=0;}
            Application.runInBackground=true;
            campaignStart=Time.realtimeSinceStartup;campaignStartedUtc=DateTime.UtcNow;
            UnityEngine.Random.InitState(771);
            placementRandom=new System.Random(771);
            // Start a fresh in-memory progression profile, as the isolated reference fixture does.
            var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text);
            progress=new Progression(definitions.challenges);
            progress.ChallengeCompleted+=achievementToasts.Enqueue;
            progress.ChallengeCompleted+=_=>ApplyUnlocks();
            Rules.Progress=null;Rules.Difficulty=0;recordedFinish=false;progressClock=0;
            RefreshUnlockPoolForNewRun();Begin();Rules.Progress=progress;PlanEncounters(1);
            // Explicit fixture loadout and invulnerability. Normal runs never enter this branch.
            Rules.Inventory.Clear();
            foreach(string id in new[]{"pistol","smg","double_barrel_shotgun","laser_gun","knife","spear"})
            {Rules.Inventory.Add(catalog.Weapon(id,2));progress.Count("weapon:"+id);}
            SyncEquipment();sound.SetMuted(true);
            // Source Main invokes land on wave one while its timer and physics
            // remain active. Exercise that same ordinary arrival path here.
            arrivalAge=0;arrivalImpact=false;
            if(parachutePrefab!=null)parachute=Instantiate(parachutePrefab);
            PoseArrival(0);
            CampaignSnapshot("boot","Neutral base stats, no character effects, Danger 0; six tier-II weapons; fresh in-memory progression; seed 771. Random sequences are engine-specific.");
        }

        CampaignWeapon[] CampaignWeapons()=>Rules.Inventory.Select(w=>new CampaignWeapon{id=w.id,tier=w.tier}).ToArray();
        CampaignStack[] CampaignItems()=>Rules.Items.GroupBy(i=>i.id).OrderBy(g=>g.Key).Select(g=>new CampaignStack{id=g.Key,count=g.Count()}).ToArray();
        void CampaignSnapshot(string kind,string detail=null,int price=0,int beforeMaterials=-1)
        {
            campaignEvents.Add(new CampaignEvent { @event=kind,phase=Rules.Phase.ToString(),detail=detail,wave=Rules.Wave,
                materials=Rules.Scrap,materialsBefore=beforeMaterials<0?Rules.Scrap:beforeMaterials,level=Rules.Level,itemCount=Rules.Items.Count,price=price,
                settlementWave=Rules.WaveSettlementReceiptWave,materialsBeforeSettlement=Rules.MaterialsBeforeSettlement,
                materialsAfterSettlement=Rules.MaterialsAfterSettlement,harvestingAtSettlement=Rules.HarvestingAtSettlement,
                kills=Rules.Kills,enemyKills=progress.Total("kills"),materialPickups=progress.Total("materials"),bankedMaterials=Rules.Banked,
                materialObjects=pickups.Count(p=>!p.fruit),groundMaterialValue=pickups.Where(p=>!p.fruit).Sum(p=>p.value),harvesting=Rules.Stat("harvesting"),luck=Rules.Stat("luck"),
                wallSeconds=Time.realtimeSinceStartup-campaignStart,simulatedSeconds=elapsed,waveSimulatedSeconds=elapsed-campaignWaveStart,
                health=Rules.Health,maxHealth=Rules.MaxHealth,remaining=Rules.Remaining,weapons=CampaignWeapons(),itemStacks=CampaignItems(),
                offers=Rules.Phase==RunPhase.Shop?Rules.Shop.Where(o=>o!=null).Select(o=>new CampaignOffer{id=o.item?.id??o.weapon?.id,price=Rules.Price(o),locked=o.locked}).ToArray():Array.Empty<CampaignOffer>() });
            File.WriteAllText(Path.Combine(campaignPath,"campaign-trace.json"),JsonUtility.ToJson(new CampaignTrace{events=campaignEvents.ToArray()},true));
        }

        void CampaignCheckTick()
        {
            if(!campaignCheck)return;
            if(campaignFinished)
            {
                if(Time.realtimeSinceStartup-campaignFinishAt<1)return;
                bool captures=campaignPreflight?CaptureVerification.Fresh(campaignPath,campaignStartedUtc,"wave-01","end"):CaptureVerification.Fresh(campaignPath,campaignStartedUtc,"wave-01","wave-05","wave-10","wave-15","wave-20","end");
                bool passed=captures&&(campaignPreflight?Rules.Phase==RunPhase.Shop&&campaignCompleted==1:Rules.Phase==RunPhase.Won&&campaignCompleted==20&&bossMutations>0&&campaignPurchases>0);
                var report=new CampaignReport { status=passed?"Passed":"Failed",phase=Rules.Phase.ToString(),capturesVerified=captures,realtime=campaignRealtime,preflight=campaignPreflight,pickupPhysicsRate=PickupPhysicsRate,
                    scope=(campaignPreflight?"ONE-WAVE PREFLIGHT ONLY; no full campaign. ":"")+(campaignRealtime?"Normal timing Windows integration fixture: reference 60Hz accumulator, 60fps render target and normal 60Hz pickup physics. ":"Accelerated Windows integration fixture: twenty-four 60Hz combat steps per rendered frame and 240Hz pickup physics. ")+"Seeded spawn placement, scripted movement, neutral base stats without character effects, Danger 0, supplied six tier-2 weapons, invulnerability, mute and fresh in-memory progression. Uses actual wave schedules, combat, queued upgrades and affordable shop purchases. Trace simulatedSeconds is active combat time and excludes shop/reward processing and wave cleanup. Not normal-run survival balance, cross-engine RNG equality, performance benchmarking or a human playthrough. Capture freshness checks do not verify image content.",
                    completedWaves=campaignCompleted,purchases=campaignPurchases,shops=campaignShops,upgrades=campaignUpgrades,crates=campaignCrates,peakEnemies=peakEnemies,
                    charges=chargeReleases,rangedAttacks=rangedReleases,bossPhaseChanges=bossMutations,wallSeconds=Time.realtimeSinceStartup-campaignStart,
                    enemyIds=campaignEnemyIds.OrderBy(x=>x).ToArray(),waves=campaignWaves.ToArray(),finalMaterials=Rules.Scrap,finalLevel=Rules.Level,finalItemCount=Rules.Items.Count,
                    finalBankedMaterials=Rules.Banked,finalKills=Rules.Kills,finalEnemyKills=progress.Total("kills"),finalMaterialPickups=progress.Total("materials"),finalHarvesting=Rules.Stat("harvesting"),finalLuck=Rules.Stat("luck"),
                    finalWeapons=CampaignWeapons(),finalItemStacks=CampaignItems() };
                File.WriteAllText(Path.Combine(campaignPath,"campaign-check.json"),JsonUtility.ToJson(report,true));
                Application.Quit(passed?0:1);enabled=false;return;
            }
            int steps=campaignRealtime?ReferencePhysicsClock.Consume(ref campaignAccumulator,Time.deltaTime):24;
            const float delta=ReferencePhysicsClock.Step;
            for(int step=0;step<steps;step++)
            {
                if(Rules.Phase==RunPhase.Playing)
                {
                    if(campaignStartedWave!=Rules.Wave){campaignStartedWave=Rules.Wave;campaignWaveStart=elapsed;CampaignSnapshot("wave_start");}
                    bool sweepingBefore=waveSweeping;
                    invulnerable=.3f;UpdateFeedback(delta);Simulate(delta);AdvanceArrivalPresentation(delta);
                    if(!sweepingBefore&&waveSweeping)CampaignSnapshot("wave_timer_end");
                    if(Rules.Remaining<RunRules.Durations[Rules.Wave-1]*.5f&&
                        (Rules.Wave==1||Rules.Wave==5||Rules.Wave==10||Rules.Wave==15||Rules.Wave==20)&&campaignCaptures.Add(Rules.Wave))
                    { ScreenCapture.CaptureScreenshot(Path.Combine(campaignPath,$"wave-{Rules.Wave:00}.png"));break; }
                }
                if(Rules.Phase!=RunPhase.Playing&&Rules.Phase!=RunPhase.Paused&&Rules.Phase!=RunPhase.Title&&campaignCompleted<Rules.Wave)
                {
                    campaignCompleted=Rules.Wave;
                    campaignWaves.Add(new WaveCheck { wave=Rules.Wave,kills=Rules.Kills,shots=shotsFired,simulatedSeconds=elapsed,waveSimulatedSeconds=elapsed-campaignWaveStart,materials=Rules.Scrap,level=Rules.Level,itemCount=Rules.Items.Count,
                        enemyKills=progress.Total("kills"),materialPickups=progress.Total("materials"),bankedMaterials=Rules.Banked,materialObjects=pickups.Count(p=>!p.fruit),groundMaterialValue=pickups.Where(p=>!p.fruit).Sum(p=>p.value),harvesting=Rules.Stat("harvesting"),luck=Rules.Stat("luck") });
                    CampaignSnapshot("wave_completed");
                }
                while(Rules.Phase==RunPhase.Loot)
                {string id=Rules.CrateItem?.id;if(!Rules.ResolveCrate(true))break;campaignCrates++;CampaignSnapshot("crate_take",id);}
                while(Rules.Phase==RunPhase.Upgrade)
                {var choice=Rules.Offers[0];Rules.Choose(0);campaignUpgrades++;CampaignSnapshot("upgrade",$"{choice.Stat}:{choice.Amount}:tier{choice.Tier}");}
                if(Rules.Phase==RunPhase.Shop)
                {
                    campaignShops++;CampaignSnapshot("shop_enter");
                    for(int i=0;i<4;i++)if(Rules.CanBuy(i))
                    {
                        var offer=Rules.Shop[i];string id=offer.item?.id??offer.weapon?.id;int price=Rules.Price(offer),beforeMaterials=Rules.Scrap;
                        if(Rules.Buy(i)){campaignPurchases++;CampaignSnapshot("purchase",id,price,beforeMaterials);}
                    }
                    CampaignSnapshot("shop_exit");
                    if(campaignPreflight)
                    {
                        campaignFinished=true;campaignFinishAt=Time.realtimeSinceStartup;
                        CampaignSnapshot("preflight_finished","One wave and shop only; full campaign was not run.");
                        ScreenCapture.CaptureScreenshot(Path.Combine(campaignPath,"end.png"));break;
                    }
                    AdvanceWave();
                }
                if(Rules.Phase==RunPhase.Won||Rules.Phase==RunPhase.Lost)
                {
                    campaignFinished=true;campaignFinishAt=Time.realtimeSinceStartup;
                    CampaignSnapshot("finished",Rules.Phase==RunPhase.Won?"won":"lost");
                    ScreenCapture.CaptureScreenshot(Path.Combine(campaignPath,"end.png"));break;
                }
            }
        }
    }
}
