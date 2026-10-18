using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Profiling;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartCrowdedWaveProfile()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--crowded-wave-profile");
            if(i<0||i+1>=args.Length)return;
            var fixture=new GameObject("Synthetic crowded-wave CPU profile").AddComponent<CrowdedProfile>();fixture.folder=args[i+1];
        }
        sealed class CrowdedProfile:MonoBehaviour
        {
            public string folder;ArenaGame game;Gamepad pad;bool recording;float start,recordStart,simStart,lastWall;
            readonly List<FrameSample> frames=new List<FrameSample>();
            [Serializable] sealed class WeaponRow{public string id;public int tier;}
            [Serializable] sealed class ItemRow{public string id;public int count;}
            [Serializable] sealed class Loadout{public WeaponRow[] finalWeapons;public ItemRow[] finalItemStacks;public int finalLevel;public float finalHarvesting,finalLuck;}
            [Serializable] sealed class EventRow{public string @event,detail;}
            [Serializable] sealed class Trace{public EventRow[] events;}
            [Serializable] sealed class FrameSample{public float wall,delta,unityDelta,combat;public int enemies,shots,pickups,sparks;}
            [Serializable] sealed class ProfileReport
            {
                public string status,error,loadoutPath,tracePath,rawPath,scope="Synthetic crowded-wave Development-player CPU profile, not a reproduction of R21 or a parity/performance benchmark. Final weapon/item stacks and upgrade deltas are replayed if supplied. Enemy HP, 350 hostile and 50 friendly slow long-lived projectiles, 200 material piles and player protection are staged. Actual ArenaGame.Update drives all combat/rendering; non-Deep profile after three-second warmup.";
                public bool development,profilerSupported,inMemoryProfile,baseline,allocationCallstacksBefore,allocationCallstacksDuring;
                public StatAllocationSample[] statAllocationBench;
                public int frames,initialEnemies,initialShots,initialHostileShots,initialFriendlyShots,initialPickups,items,upgradesApplied;
                public float wallSeconds,combatSeconds,p50FrameMs,p95FrameMs,maxFrameMs;
                public FrameSample[] samples;
            }
            readonly ProfileReport report=new ProfileReport();
            static string Arg(string name){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,name);return i>=0&&i+1<a.Length?a[i+1]:null;}
            IEnumerator Start()
            {
                start=Time.realtimeSinceStartup;string error=null;var run=Run();
                while(true)
                {
                    bool more=false;object next=null;
                    try{Require(Time.realtimeSinceStartup-start<60,"Profile exceeded 60-second safety limit.");more=run.MoveNext();if(more)next=run.Current;}
                    catch(Exception ex){error=ex.ToString();}
                    if(!more)break;yield return next;
                }
                recording=false;Profiler.enabled=false;Profiler.logFile="";Profiler.enableBinaryLog=false;
                if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
                report.error=error;report.samples=frames.ToArray();report.frames=frames.Count;
                var times=frames.Select(f=>f.delta*1000).OrderBy(v=>v).ToArray();
                if(times.Length>0){report.p50FrameMs=times[(int)((times.Length-1)*.5)];report.p95FrameMs=times[(int)((times.Length-1)*.95)];report.maxFrameMs=times[times.Length-1];}
                bool raw=File.Exists(report.rawPath??"")&&new FileInfo(report.rawPath).Length>0;
                report.status=error==null&&raw&&frames.Count>2?"Captured":"Failed";
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"crowded-profile.json"),JsonUtility.ToJson(report,true));
                Application.Quit(report.status=="Captured"?0:1);
            }
            IEnumerator Run()
            {
                Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null,"ArenaGame must initialize.");
                report.development=Debug.isDebugBuild;report.profilerSupported=Profiler.supported;report.inMemoryProfile=GameSettings.IsFixture;
                Require(report.development&&report.profilerSupported&&report.inMemoryProfile,"Use Development build and --controller-check isolated profile.");
                Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
                GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",true);
                foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();
                game.Begin();game.ClearCombat();game.Rules.Start(startingMaterials:100);game.Rules.SetShowcaseWave(19);
                game.arrivalAge=ArrivalMotion.Duration;game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},771);game.births.Clear();
                game.Rules.Inventory.Clear();report.loadoutPath=Arg("--profile-loadout");report.tracePath=Arg("--profile-trace");
                if(!string.IsNullOrEmpty(report.loadoutPath))
                {
                    var load=JsonUtility.FromJson<Loadout>(File.ReadAllText(report.loadoutPath));
                    foreach(var w in load.finalWeapons??Array.Empty<WeaponRow>()){var equipment=game.catalog.Weapon(w.id,w.tier);Require(equipment!=null,"Missing weapon "+w.id);game.Rules.Inventory.Add(equipment);}
                    foreach(var row in load.finalItemStacks??Array.Empty<ItemRow>())
                    {var item=Array.Find(game.catalog.data.items,x=>x.id==row.id);Require(item!=null,"Missing item "+row.id);for(int n=0;n<row.count;n++)game.Rules.GrantItem(item);}
                    game.Rules.GainWeaponStat("max_hp",load.finalLevel);
                    if(!string.IsNullOrEmpty(report.tracePath))foreach(var row in JsonUtility.FromJson<Trace>(File.ReadAllText(report.tracePath)).events)
                    {
                        if(row.@event!="upgrade")continue;var parts=row.detail.Split(':');float amount;
                        if(parts.Length>=2&&float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out amount)){game.Rules.GainWeaponStat(parts[0],amount);report.upgradesApplied++;}
                    }
                    game.Rules.GainWeaponStat("harvesting",load.finalHarvesting-game.Rules.Stat("harvesting"));
                    game.Rules.GainWeaponStat("luck",load.finalLuck-game.Rules.Stat("luck"));
                }
                else foreach(string id in new[]{"pistol","smg","double_barrel_shotgun","laser_gun","knife","spear"})game.Rules.Inventory.Add(game.catalog.Weapon(id,2));
                game.Rules.GainWeaponStat("max_hp",10000);game.Rules.Heal(10000);game.SyncEquipment();game.ResetStructures();game.sound.SetMuted(true);
                report.baseline=Array.IndexOf(Environment.GetCommandLineArgs(),"--profile-baseline")>=0;
                string[] types={"chaser","charger","spitter","buffer","bruiser","helmet_alien","horned_charger","horned_bruiser"};
                for(int i=0;i<(report.baseline?0:134);i++)
                {
                    float a=i*2.39996323f,r=2+(i%17)*.35f;var type=game.catalog.Enemy(types[i%types.Length]);Require(type!=null,"Missing enemy type.");
                    game.Spawn(type,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r);var enemy=game.enemies[game.enemies.Count-1];enemy.hp=enemy.maxHp=1000000;
                }
                var pistol=game.catalog.Weapon("pistol",2);
                if(!report.baseline)
                {
                    var shooter=game.enemies.Find(e=>e.type.id=="spitter");
                    for(int i=0;i<350&&game.shots.Count<350;i++)game.FireEncounter(shooter);
                    Require(game.shots.Count>=350,"Staged spitter volleys must produce 350 hostile projectiles.");
                    foreach(var shot in game.shots){shot.velocity=shot.velocity.normalized*.2f;shot.life=60;}
                }
                for(int i=0;i<(report.baseline?0:50);i++)
                {
                    float a=i*2.39996323f;Vector2 direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Vector2 position=direction*(1+(i%19)*.3f);
                    var obj=game.SpriteObject("Synthetic persistent Pistol tracer",game.catalog.muzzleFrames[2],position,13);
                    var shot=new Shot{obj=obj,pos=position,velocity=direction*.2f,weapon=pistol,weaponIndex=0,damage=.01f,pierce=10000,life=60};
                    game.ConfigureReferenceShot(shot,true);shot.life=60;game.shots.Add(shot);
                }
                for(int i=0;i<(report.baseline?0:200);i++)game.Drop(new Vector2((i%20-10)*.8f,(i/20-5)*.8f),1,false);
                report.initialEnemies=game.enemies.Count;report.initialShots=game.shots.Count;report.initialHostileShots=game.shots.Count(x=>x.hostile);report.initialFriendlyShots=game.shots.Count(x=>!x.hostile);report.initialPickups=game.pickups.Count;report.items=game.Rules.Items.Count;
                report.statAllocationBench=game.Rules.ProfileStatAllocations();
                yield return new WaitForSecondsRealtime(3);
                Profiler.enabled=false;Profiler.logFile="";report.rawPath=Path.Combine(Path.GetFullPath(folder),"crowded.raw");
                report.allocationCallstacksBefore=Profiler.enableAllocationCallstacks;Profiler.enableAllocationCallstacks=false;report.allocationCallstacksDuring=Profiler.enableAllocationCallstacks;
                Profiler.logFile=report.rawPath;Profiler.enableBinaryLog=true;Profiler.maxUsedMemory=256*1024*1024;Profiler.enabled=true;
                recordStart=lastWall=Time.realtimeSinceStartup;simStart=game.elapsed;recording=true;
                float seconds=12;float.TryParse(Arg("--profile-seconds")??"12",NumberStyles.Float,CultureInfo.InvariantCulture,out seconds);seconds=Mathf.Clamp(seconds,1,15);
                while(Time.realtimeSinceStartup-recordStart<seconds)yield return null;
                recording=false;report.wallSeconds=Time.realtimeSinceStartup-recordStart;report.combatSeconds=game.elapsed-simStart;
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"crowded.png"));
                Profiler.enabled=false;Profiler.logFile="";Profiler.enableBinaryLog=false;yield return null;yield return new WaitForSecondsRealtime(.25f);
            }
            void Update()
            {
                if(game==null)return;game.invulnerable=10;
                float age=Time.realtimeSinceStartup-start;
                if(pad!=null)InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(Mathf.Cos(age*.8f),Mathf.Sin(age*.8f))});
            }
            void LateUpdate()
            {
                if(!recording)return;float now=Time.realtimeSinceStartup;
                frames.Add(new FrameSample{wall=now-recordStart,delta=now-lastWall,unityDelta=Time.unscaledDeltaTime,combat=game.elapsed-simStart,enemies=game.enemies.Count,shots=game.shots.Count,pickups=game.pickups.Count,sparks=game.sparks.Count});lastWall=now;
            }
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
    [Serializable] public sealed class StatAllocationSample
    {
        public string method;public bool allocationCounterAvailable;public int calls,collections;public long allocatedBytes;public double elapsedMs,bytesPerCall;
    }
    public sealed partial class RunRules
    {
        // Opt-in profiling helper only: no gameplay call sites use this benchmark.
        internal StatAllocationSample[] ProfileStatAllocations()
        {
            var result=new List<StatAllocationSample>();
            long probeBefore=GC.GetAllocatedBytesForCurrentThread();var probe=new byte[8192];GC.KeepAlive(probe);bool counterAvailable=GC.GetAllocatedBytesForCurrentThread()>probeBefore;
            void Measure(string name,Action action)
            {
                for(int i=0;i<20;i++)action();
                long before=GC.GetAllocatedBytesForCurrentThread();int collections=GC.CollectionCount(0);
                var watch=System.Diagnostics.Stopwatch.StartNew();
                for(int i=0;i<1000;i++)action();
                watch.Stop();long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
                result.Add(new StatAllocationSample{method=name,allocationCounterAvailable=counterAvailable,calls=1000,allocatedBytes=counterAvailable?bytes:-1,bytesPerCall=counterAvailable?bytes/1000d:-1,elapsedMs=watch.Elapsed.TotalMilliseconds,collections=GC.CollectionCount(0)-collections});
            }
            foreach(string key in new[]{"enemy_speed","range","max_hp","all_weapons_count_for_sets","percent_damage"})
                Measure("Stat("+key+")",()=>Stat(key));
            Measure("PermanentStat",()=>PermanentStat("enemy_speed"));
            Measure("SetStat",()=>SetStat("enemy_speed"));
            Measure("EquipmentStat",()=>EquipmentStat("enemy_speed"));
            Measure("ConditionalStat",()=>ConditionalStat("enemy_speed"));
            Measure("LinkedStat",()=>LinkedStat("enemy_speed"));
            Measure("WeaponSets.Bonus",()=>WeaponSets.Bonus(Inventory,"enemy_speed"));
            Measure("WeaponSets.Count",()=>WeaponSets.Count(Inventory,WeaponSets.Gun));
            if(Inventory.Count>0)Measure("WeaponSets.For",()=>WeaponSets.For(Inventory[0]));
            Measure("ConditionalEffects enumeration",()=>{foreach(var effect in ConditionalEffects())GC.KeepAlive(effect);});
            Measure("GameSettings.Value",()=>GameSettings.Value("enemySpeed"));
            Measure("GameSettings.Flag",()=>GameSettings.Flag("gamepadMove"));
            Measure("CombatGeometry.Enemy",()=>CombatGeometry.Enemy("chaser"));
            return result.ToArray();
        }
    }

}
