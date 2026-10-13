#if UNITY_WEBGL && SCRAPSHIFT_WEB_REVIEW
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [DllImport("__Internal")] static extern void ScrapshiftReport(string json);
        [Serializable] class BrowserReport
        {
            public string phase,objectName;public int enemies,shots,frames,peak,minEnemies,weapons;public float averageMs,p95Ms,playerX,playerY,master;
            public float listenerVolume;public bool musicPlaying;public float musicTime;public long savedProbe;
        }
        readonly System.Collections.Generic.List<float> webFrames=new System.Collections.Generic.List<float>();
        float webReportClock,webBenchmarkAge=-1;
        int webMinEnemies=99999,webSpawnIndex;
        EnemyEntry[] webKinds;
        public void WebReview(string command)
        {
            if(command=="save") {progress.Count("browser-test");progress.Save();GameSettings.Set("master",.42f);ApplyAudioSettings();}
            if(command=="crowd") {StartSelectedRun();arrivalAge=ArrivalMotion.Duration;webBenchmarkAge=0;webFrames.Clear();ClearCombat();webMinEnemies=99999;webSpawnIndex=0;Rules.Inventory.Clear();foreach(var id in new[]{"smg","shotgun","pistol","wand","knife","laser_gun"}){var w=catalog.Weapon(id,1);if(w!=null)Rules.Inventory.Add(w);}while(Rules.Inventory.Count<6)Rules.Inventory.Add(catalog.Weapon("pistol",1));webKinds=catalog.data.enemies.Where(e=>e.role!="Tree"&&e.role!="Boss").Take(8).ToArray();RefillWebCrowd();}
        }
        void RefillWebCrowd(){while(enemies.Count<200){int i=webSpawnIndex++;float a=i*2.399963f;Spawn(webKinds[i%webKinds.Length],new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(5+i%13));}}
        void TickWebReview()
        {
            if(webBenchmarkAge>=0&&Rules.Phase==RunPhase.Playing){webBenchmarkAge+=Time.unscaledDeltaTime;invulnerable=10;RefillWebCrowd();webMinEnemies=Mathf.Min(webMinEnemies,enemies.Count);if(webBenchmarkAge>2&&webBenchmarkAge<12)webFrames.Add(Time.unscaledDeltaTime*1000);if(webBenchmarkAge>=12){Rules.Pause();webBenchmarkAge=-1;}}
            webReportClock+=Time.unscaledDeltaTime;if(webReportClock<1)return;webReportClock=0;
            var sorted=webFrames.OrderBy(v=>v).ToArray();
            ScrapshiftReport(JsonUtility.ToJson(new BrowserReport{phase=Rules.Phase.ToString(),objectName=gameObject.name,enemies=enemies.Count,peak=peakEnemies,minEnemies=webMinEnemies,weapons=Rules.Inventory.Count,listenerVolume=AudioListener.volume,shots=shots.Count,frames=sorted.Length,averageMs=sorted.Length>0?sorted.Average():0,p95Ms=sorted.Length>0?sorted[(int)((sorted.Length-1)*.95f)]:0,playerX=playerPos.x,playerY=playerPos.y,master=GameSettings.Value("master",.5f),savedProbe=progress.Total("browser-test"),musicPlaying=music.IsPlaying,musicTime=music.PlaybackTime}));
        }
    }
}
#endif
