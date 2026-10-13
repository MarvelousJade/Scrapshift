using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class UnlockDefinition
    {public string id,name,description,stat,reward;public int kind;public float value;}
    [Serializable] public sealed class UnlockCatalog {public UnlockDefinition[] challenges;}
    [Serializable] public sealed class ProgressCounter {public string key;public long value;}
    [Serializable] public sealed class ProgressSave
    {public int version=1;public List<string> completed=new List<string>();public List<ProgressCounter> counters=new List<ProgressCounter>();}
    public sealed class Progression
    {
        readonly UnlockDefinition[] definitions;
        readonly string path;
#if UNITY_WEBGL && !UNITY_EDITOR
        string lastSaved;
        const string BrowserSaveKey="scrapshift.progress.v1";
#endif
        public ProgressSave Data {get;private set;}=new ProgressSave();
        public readonly List<string> NewlyUnlocked=new List<string>();
        public event Action<UnlockDefinition> ChallengeCompleted;
        public Progression(UnlockDefinition[] definitions,string path=null)
        {
            this.definitions=definitions;this.path=path;
#if UNITY_WEBGL && !UNITY_EDITOR
            if(path!=null&&PlayerPrefs.HasKey(BrowserSaveKey))
            {
                try {var saved=JsonUtility.FromJson<ProgressSave>(PlayerPrefs.GetString(BrowserSaveKey));if(saved==null||saved.version!=1||saved.completed==null||saved.counters==null)throw new InvalidDataException("Unsupported progression save");Data=saved;}
                catch(Exception e){Debug.LogWarning("Could not load browser progression; preserving save: "+e.Message);this.path=null;}
            }
#else
            if(path!=null&&File.Exists(path))
            {
                try {var saved=JsonUtility.FromJson<ProgressSave>(File.ReadAllText(path));if(saved==null||saved.version!=1||saved.completed==null||saved.counters==null)throw new InvalidDataException("Unsupported progression save");Data=saved;}
                catch(Exception e){Debug.LogWarning("Could not load progression; preserving the save for recovery: "+e.Message);this.path=null;}
            }
 #endif
        }
        public System.Collections.Generic.IReadOnlyList<UnlockDefinition> Definitions=>definitions;
        public bool Completed(string id)=>Data.completed.Contains(id);
        public bool Unlocked(string reward)=>definitions.Any(d=>d.reward==reward&&Completed(d.id));
        public int Difficulty=>Math.Max(0,definitions.Where(d=>d.id.StartsWith("unlock_difficulty_")&&Completed(d.id)).Select(d=>(int)d.value).DefaultIfEmpty(0).Max());
        public string Requirement(string reward)
        {
            var d=definitions.FirstOrDefault(x=>x.reward==reward);if(d==null)return "Locked";
            if(d.id.StartsWith("chal_difficulty_"))return "Win a run on Danger "+d.value;
            if(d.description=="CHAL_CHARACTER_DESC")return "Win a run with "+d.id.Substring(5).Replace('_',' ');
            if(!string.IsNullOrEmpty(d.stat))return "Reach "+d.value+" "+ItemEntry.StatName(d.stat);
            return UnlockRequirement(d);
        }
        static string UnlockRequirement(UnlockDefinition d)
        {
            if(d.id.StartsWith("chal_survivor_"))return "Defeat "+d.value+" enemies across runs";
            if(d.id.StartsWith("chal_gatherer_"))return "Collect "+d.value+" materials across runs";
            switch(d.id)
            {
                case "chal_rookie":return "Die for the first time";
                case "chal_hoarder":return "Hold 3000 materials";
                case "chal_turrets":return "Have 5 turrets at once";
                case "chal_lumberjack":return "Destroy 50 trees across runs";
                case "chal_medicine":return "Heal 200 HP in one run";
                case "chal_fireworks":return "Kill 15 enemies with one explosion";
                case "chal_recycling":return "Recycle 12 weapons in one run";
                case "chal_hungry":return "Pick up 20 consumables in one run";
                case "chal_advanced_technology":return "Have 10 Ranged Damage and 3 structures";
                case "chal_magic_and_machinery":return "Have 10 Elemental Damage and 3 structures";
                case "chal_giant_slayer":return "Defeat a boss or elite in less than 15 seconds";
                case "chal_baited":return "Own 2 Threat Beacon Implants";
                case "chal_forest":return "Finish a wave with 10 living trees";
                case "chal_bourgeoisie":return "Equip 3 tier IV weapons";
                case "chal_student":return "Reach level 20";
                case "chal_reckless":return "Finish a wave with 1 HP";
                case "chal_scavenger":return "Own 10 different tier I implants";
                case "chal_fast_learner":return "Reach level 10 before wave 6";
                case "chal_experimentation":return "Equip 6 different weapons";
                case "chal_evil_hat":return "Defeat 5 evil enemies across runs";
                case "chal_hourglass":return "Return to the menu before wave 5";
                case "chal_candy_bag":return "Win without locking shop offers";
                case "chal_fruit_basket":return "Collect 100 fruits at full HP across runs";
                case "chal_will_o_the_wisp":return "Kill 30 enemies with burning in one wave";
                case "chal_bonk_dog":return "Equip 6 weapons scaling with Melee Damage";
                case "chal_catling_gun":return "Equip 6 weapons scaling with Ranged Damage";
                case "chal_bot_o_mine":return "Equip 6 weapons scaling with Engineering";
                case "chal_blazemander":return "Equip 6 weapons scaling with Elemental Damage";
                case "chal_doc_moth":return "Have 10 HP Regeneration and 10% Life Steal";
                case "chal_lootworm":return "Finish a wave with 20 materials on the ground";
                case "chal_jellyshield":return "Take 3 projectile hits in one wave";
                case "chal_paws_n_claws":return "Unlock the impact, twin-gun, engineering, thermal and salvage companions";
                default:return "Win a run to unlock this reward";
            }
        }
        public void Complete(string id)
        {
            var d=definitions.FirstOrDefault(x=>x.id==id);if(d==null||Completed(id))return;
            Data.completed.Add(id);NewlyUnlocked.Add(d.reward);Save();ChallengeCompleted?.Invoke(d);
        }
        public void Check(string id,float value,bool below=false)
        {var d=definitions.FirstOrDefault(x=>x.id==id);if(d!=null&&(below?value<=d.value:value>=d.value))Complete(id);}
        public long Total(string key)=>Data.counters.FirstOrDefault(c=>c.key==key)?.value??0;
        public void Count(string key,int amount=1)
        {
            var c=Data.counters.FirstOrDefault(x=>x.key==key);if(c==null){c=new ProgressCounter{key=key};Data.counters.Add(c);}c.value+=amount;
            foreach(var d in definitions)
            {
                bool matches=key=="kills"&&d.name=="CHAL_SURVIVOR"||key=="materials"&&d.name=="CHAL_GATHERER"||key=="trees"&&d.id=="chal_lumberjack"||key=="evil_mob"&&d.id=="chal_evil_hat"||key=="full_health_fruit"&&d.id=="chal_fruit_basket";
                if(matches&&c.value>=d.value)Complete(d.id);
            }
        }
        public void CheckStats(RunRules run)
        {
            foreach(var d in definitions)if(!string.IsNullOrEmpty(d.stat)&&d.description=="CHAL_STAT_DESC")Check(d.id,d.stat=="max_hp"?run.MaxHealth:run.Stat(d.stat),d.value<0);
            Check("chal_hoarder",run.Scrap);Check("chal_student",run.Level);
            if(run.Wave<6)Check("chal_fast_learner",run.Level);
            Check("chal_baited",run.Items.Count(i=>i.id=="bait"));Check("chal_bourgeoisie",run.Inventory.Count(w=>w.tier==4));
            Check("chal_scavenger",run.Items.Where(i=>i.tier==1).Select(i=>i.id).Distinct().Count());
            if(run.Stat("ranged_damage")>=10&&run.OwnedStructures>=3)Complete("chal_advanced_technology");
            if(run.Stat("elemental_damage")>=10&&run.OwnedStructures>=3)Complete("chal_magic_and_machinery");
            if(run.Stat("hp_regeneration")>=10&&run.Stat("lifesteal")>=10)Complete("chal_doc_moth");
            if(new[]{"chal_bonk_dog","chal_catling_gun","chal_bot_o_mine","chal_blazemander","chal_lootworm"}.All(Completed))Complete("chal_paws_n_claws");
            foreach(var pair in new[]{("chal_bonk_dog","melee_damage"),("chal_catling_gun","ranged_damage"),("chal_bot_o_mine","engineering"),("chal_blazemander","elemental_damage")})Check(pair.Item1,run.Inventory.Count(w=>w.scaling!=null&&w.scaling.Any(s=>s.key==pair.Item2)));
            if(run.Inventory.Count>=6&&run.Inventory.Select(w=>w.id).Distinct().Count()>=6)Complete("chal_experimentation");
        }
        public void Finish(bool won,string character,int difficulty,bool usedLocks)
        {
            if(!won)Complete("chal_rookie");
            else
            {
                Complete("chal_"+character);Complete("chal_difficulty_"+difficulty);
                for(int i=1;i<=Math.Min(5,difficulty+1);i++)Complete("unlock_difficulty_"+i);
                if(difficulty>=5){string key="danger5:"+character;if(Total(key)==0)Count(key);if(Data.counters.Count(c=>c.key.StartsWith("danger5:")&&c.value>0)>=10)Complete("unlock_difficulty_6");}
                if(!usedLocks)Complete("chal_candy_bag");
            }
            Save();
        }
        public void Save()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if(path==null)return;
            string json=JsonUtility.ToJson(Data);
            if(json==lastSaved)return;
            PlayerPrefs.SetString(BrowserSaveKey,json);PlayerPrefs.Save();lastSaved=json;
#else
            if(path==null)return;Directory.CreateDirectory(Path.GetDirectoryName(path));string temporary=path+".tmp";
            File.WriteAllText(temporary,JsonUtility.ToJson(Data,true));
            if(File.Exists(path))File.Replace(temporary,path,path+".bak");else File.Move(temporary,path);
#endif
        }
    }
}
