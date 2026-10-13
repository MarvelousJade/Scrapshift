using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        Progression progress;
        int selectedDifficulty;
        float progressClock;
        bool recordedFinish;
        void InitializeProgression()
        {
            catalog=Instantiate(catalog);var images=catalog.data.equipment.Select(w=>w.image).Where(s=>s!=null).Distinct().ToDictionary(s=>s,AtlasIconMask.CreateWorldSprite);foreach(var weapon in catalog.data.equipment)if(weapon.image!=null)weapon.image=images[weapon.image];
            var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text);
            bool fixture=Environment.GetCommandLineArgs().Any(a=>a.StartsWith("--"));
            progress=new Progression(definitions.challenges,fixture?null:Path.Combine(Application.persistentDataPath,"progress-v1.json"));
            ApplyUnlocks();InitializeEncounters();
        }
        void ApplyUnlocks()
        {
            foreach(var c in catalog.data.characters)c.available|=progress.Unlocked("character_"+c.id);
            foreach(var i in catalog.data.items)i.available|=progress.Unlocked("item_"+i.id);
            foreach(var w in catalog.data.equipment)w.available|=progress.Unlocked("weapon_"+w.id);
            RunRules.ConfigureShopItems(catalog.data.items);
        }
        bool CharacterUnlocked(CharacterProfile character)=>character.available;
        void TickProgression(float dt)
        {
            if(progress==null||Rules.Progress==null)return;
            progressClock+=dt;if(progressClock>=.5f){progressClock=0;progress.CheckStats(Rules);ApplyUnlocks();}
            if(!recordedFinish&&(Rules.Phase==RunPhase.Won||Rules.Phase==RunPhase.Lost))
            {recordedFinish=true;progress.Finish(Rules.Phase==RunPhase.Won,Rules.Character?.id??"well_rounded",Rules.Difficulty,Rules.UsedLocks);ApplyUnlocks();}
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
        public void RecordBurnKill(){burningKills++;Progress?.Check("chal_will_o_the_wisp",burningKills);}
        public void RecordExplosionKills(int count)=>Progress?.Check("chal_fireworks",count);
        public void RecordEnemy(string id,bool tree,float aliveSeconds,bool boss)
        {
            Progress?.Count(tree?"trees":"kills");if(id=="evil_mob")Progress?.Count("evil_mob");
            if(boss&&aliveSeconds<15)Progress?.Complete("chal_giant_slayer");
        }
        public void RecordProjectileHit(){projectileHits++;Progress?.Check("chal_jellyshield",projectileHits);}
        public void RecordWaveEnd(int trees,int groundMaterials)
        {
            if(Health<=1&&Health>0)Progress?.Complete("chal_reckless");
            Progress?.Check("chal_forest",trees);Progress?.Check("chal_lootworm",groundMaterials);Progress?.Save();
        }
    }
}
