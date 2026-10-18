using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    // ItemService.init_unlocked_pool snapshots item and weapon eligibility in
    // RunData.reset. Challenge rewards still become visible in the menu at once.
    public sealed class RunUnlockPool
    {
        public bool Frozen {get;private set;}
        public void Refresh(ReferenceCatalog catalog,Progression progress)
        {
            foreach(var c in catalog.data.characters)c.available|=progress.Unlocked("character_"+c.id);
            if(Frozen)return;
            foreach(var i in catalog.data.items)i.available|=progress.Unlocked("item_"+i.id);
            foreach(var w in catalog.data.equipment)w.available|=progress.Unlocked("weapon_"+w.id);
            RunRules.ConfigureShopItems(catalog.data.items);
        }
        public void BeginRun(ReferenceCatalog catalog,Progression progress)
        {Frozen=false;Refresh(catalog,progress);Frozen=true;}
        public void EndRun(ReferenceCatalog catalog,Progression progress)
        {Frozen=false;Refresh(catalog,progress);}
    }
    public sealed partial class ArenaGame
    {
        Progression progress;
        RunUnlockPool runUnlockPool;
        int selectedDifficulty;
        float progressClock;
#if UNITY_WEBGL && !UNITY_EDITOR
        float browserSaveClock;
#endif
        bool recordedFinish;
        void InitializeProgression()
        {
            catalog=Instantiate(catalog);var images=catalog.data.equipment.Select(w=>w.image).Where(s=>s!=null).Distinct().ToDictionary(s=>s,AtlasIconMask.CreateWorldSprite);foreach(var weapon in catalog.data.equipment)if(weapon.image!=null)weapon.image=images[weapon.image];
            var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text);
            bool fixture=Environment.GetCommandLineArgs().Any(a=>a.StartsWith("--"));
            progress=new Progression(definitions.challenges,fixture?null:Path.Combine(Application.persistentDataPath,"progress-v1.json"));
            runUnlockPool=new RunUnlockPool();
            progress.ChallengeCompleted+=achievementToasts.Enqueue;
            progress.ChallengeCompleted+=_=>ApplyUnlocks();
            ApplyUnlocks();InitializeEncounters();
        }
        void ApplyUnlocks()=>runUnlockPool.Refresh(catalog,progress);
        void RefreshUnlockPoolForNewRun()=>runUnlockPool.BeginRun(catalog,progress);
        bool CharacterUnlocked(CharacterProfile character)=>character.available;
        void TickProgression(float dt)
        {
            achievementToasts.Tick(dt);
            if(runUnlockPool?.Frozen==true&&(Rules.Phase==RunPhase.Title||Rules.Phase==RunPhase.Won||Rules.Phase==RunPhase.Lost))
                runUnlockPool.EndRun(catalog,progress);
#if UNITY_WEBGL && !UNITY_EDITOR
            browserSaveClock+=dt;if(browserSaveClock>=5){browserSaveClock=0;progress?.Save();}
#endif
            if(progress==null||Rules.Progress==null)return;
            progressClock+=dt;if(progressClock>=.5f){progressClock=0;progress.CheckStats(Rules);ApplyUnlocks();}
            if(!recordedFinish&&(Rules.Phase==RunPhase.Won||Rules.Phase==RunPhase.Lost))
            {recordedFinish=true;progress.Finish(Rules.Phase==RunPhase.Won,Rules.Character?.id??"well_rounded",Rules.Difficulty,Rules.UsedLocks,!Rules.Abandoned);ApplyUnlocks();}
        }
        void OnApplicationQuit(){progress?.Save();}
    }
    public sealed partial class RunRules
    {
        public Progression Progress {get;set;}
        public int Difficulty {get;set;}
        public bool UsedLocks {get;private set;}
        int runConsumables,runRecycles,projectileHits;
        float runHealing;
        int burningKills;
        public bool CanUnlockHourglassOnReturn=>(Phase==RunPhase.Playing||Phase==RunPhase.Paused&&pausedFrom==RunPhase.Playing)&&Remaining<5;
        public void RecordBurnKill(){burningKills++;Progress?.Check("chal_will_o_the_wisp",burningKills);}
        public void RecordExplosionKills(int count)=>Progress?.Check("chal_fireworks",count);
        public void RecordEnemy(string id,bool tree,float aliveSeconds,bool boss)
        {
            Progress?.Count("enemy:"+id);
            Progress?.Count(tree?"trees":"kills");if(id=="evil_mob")Progress?.Count("evil_mob");
            if(boss)Progress?.Check("chal_giant_slayer",aliveSeconds,true);
        }
        public void RecordProjectileHit(){projectileHits++;Progress?.Check("chal_jellyshield",projectileHits);}
        public void RecordWaveEnd(int trees,int groundMaterials)
        {
            Progress?.CheckCountedChallenges();
            if(Health==1)Progress?.Complete("chal_reckless");
            Progress?.Check("chal_forest",trees);Progress?.Check("chal_lootworm",groundMaterials);Progress?.CheckCompanionUnlock();Progress?.Save();
        }
    }
}
