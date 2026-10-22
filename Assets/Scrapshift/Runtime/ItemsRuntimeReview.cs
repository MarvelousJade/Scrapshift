using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartItemsCombinedReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--items-combined-review");
            if(index<0||index+1>=args.Length)return;
            var fixture=new GameObject("Native item combination fixture").AddComponent<NativeItemsCombinedReview>();
            fixture.folder=args[index+1];
        }

        sealed class NativeItemsCombinedReview:MonoBehaviour
        {
            public string folder;
            ArenaGame game;float started;
            readonly List<BurnCase> cases=new List<BurnCase>();
            [Serializable] sealed class BurnCase
            {
                public string name;
                public float expectedInterval,actualInterval,initialHealth,finalHealth,initialElapsed;
                public float chance,damage;
                public int initialTicks,finalTicks;
                public float[] tickElapsed,tickDamage;
                public bool timedTicks,payload,captureExists;
            }
            [Serializable] sealed class ItemsReport
            {
                public string status,error,scope="Opt-in staged native item-combination fixture. Neutral wave-one rules, empty spawn groups, high-HP stationary enemy and cleared weapons isolate one manually applied burn. The actual ArenaGame.Update 60 Hz simulation advances every tick. Synthetic setup does not establish normal combat hit probability, human input, other survival effects or a full playthrough.";
                public bool inMemoryProfile,normalUpdate;
                public BurnCase[] cases;
            }
            readonly ItemsReport report=new ItemsReport();
            IEnumerator Start()
            {
                started=Time.realtimeSinceStartup;string error=null;var routine=Run();
                while(true)
                {
                    bool running=false;object next=null;
                    try
                    {
                        Require(Time.realtimeSinceStartup-started<14,"Item combination fixture exceeded 14 seconds.");
                        running=routine.MoveNext();if(running)next=routine.Current;
                    }
                    catch(Exception ex){error=ex.ToString();}
                    if(!running)break;
                    yield return next;
                }
                report.error=error;report.cases=cases.ToArray();
                bool passed=error==null&&report.inMemoryProfile&&report.normalUpdate&&cases.Count==3;
                foreach(var sample in cases)passed&=sample.timedTicks&&sample.payload&&sample.captureExists;
                report.status=passed?"Passed":"Failed";
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder,"items-combined.json"),JsonUtility.ToJson(report,true));
                Application.Quit(passed?0:1);
            }
            IEnumerator Run()
            {
                Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&game.playerAnimation!=null,"ArenaGame initialization is required.");
                Require(Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use --controller-check for an isolated in-memory profile.");
                report.inMemoryProfile=GameSettings.IsFixture;Require(report.inMemoryProfile,"Fixture profile is required.");
                Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
                GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);
                game.sound.SetMuted(true);foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);
                foreach(string name in new[]{"global","mixed-cooldown","native"})
                {
                    game.Begin();game.ClearCombat();game.arrivalAge=ArrivalMotion.Duration;
                    game.Rules.Start();game.Rules.Inventory.Clear();game.SyncEquipment();
                    game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},1701);
                    game.births.Clear();game.invulnerable=10;
                    game.Rules.GainWeaponStat("elemental_damage",10);
                    var sausage=Array.Find(game.catalog.data.items,item=>item.id=="scared_sausage");
                    Require(sausage!=null,"Scared Sausage is missing from the catalog.");
                    game.Rules.GrantItem(sausage);game.Rules.GrantItem(sausage);
                    if(name=="mixed-cooldown")
                    {
                        var surgery=Array.Find(game.catalog.data.items,item=>item.id=="eyes_surgery");
                        var heart=Array.Find(game.catalog.data.items,item=>item.id=="frozen_heart");
                        Require(surgery!=null&&heart!=null,"Burn cooldown items are missing from the catalog.");
                        game.Rules.GrantItem(surgery);game.Rules.GrantItem(surgery);game.Rules.GrantItem(heart);
                    }
                    Equipment weapon=name=="native"?new Equipment{traits=new[]{new WeaponTrait{
                        key="effect_burning",burnChance=.3f,burnDamage=4,burnTicks=2,burnScale=.5f,burnStat="elemental_damage"}}}:new Equipment();
                    var burn=game.Rules.CaptureBurn(weapon);
                    float expected=name=="mixed-cooldown"?.8f:.5f;
                    int expectedTicks=name=="native"?2:3;
                    float expectedChance=name=="native"?.3f:.5f;
                    float expectedDamage=name=="mixed-cooldown"?22:name=="native"?11:12;
                    var sample=new BurnCase{name=name,expectedInterval=expected,actualInterval=game.Rules.BurningInterval,
                        chance=burn.chance,damage=burn.damage,initialTicks=burn.ticks};
                    cases.Add(sample);
                    sample.payload=Mathf.Abs(sample.actualInterval-expected)<.0001f&&Mathf.Abs(burn.chance-expectedChance)<.0001f&&
                        Mathf.Abs(burn.damage-expectedDamage)<.0001f&&burn.ticks==expectedTicks;
                    Require(sample.payload,"Burn payload or combined timer differs from the source-derived expectation: "+name);
                    var target=game.FeelEnemy(game.playerPos+Vector2.right*3);
                    target.hp=target.maxHp=10000;target.speed=0;
                    sample.initialHealth=target.hp;sample.initialElapsed=game.elapsed;
                    game.ApplyBurn(target,burn.damage,burn.ticks,burn.spread,burn.stat);
                    var tickTimes=new List<float>();var tickDamage=new List<float>();float previousHp=target.hp;
                    float deadline=game.elapsed+expected*expectedTicks+.35f;
                    while(target.burnTicks>0&&game.elapsed<deadline)
                    {
                        yield return null;
                        if(target.hp>=previousHp-.001f)continue;
                        tickTimes.Add(game.elapsed-sample.initialElapsed);
                        tickDamage.Add(previousHp-target.hp);previousHp=target.hp;
                    }
                    sample.finalHealth=target.hp;sample.finalTicks=target.burnTicks;
                    sample.tickElapsed=tickTimes.ToArray();sample.tickDamage=tickDamage.ToArray();
                    sample.timedTicks=sample.finalTicks==0&&tickTimes.Count==expectedTicks&&
                        Mathf.Abs(sample.initialHealth-sample.finalHealth-expectedDamage*expectedTicks)<.01f;
                    for(int i=0;i<tickTimes.Count;i++)
                    {
                        // A rendered frame can observe the fixed step just after its
                        // target boundary; allow at most two 60 Hz steps.
                        sample.timedTicks&=Mathf.Abs(tickTimes[i]-expected*(i+1))<=2f/60+.001f&&
                            Mathf.Abs(tickDamage[i]-expectedDamage)<.01f;
                    }
                    report.normalUpdate|=sample.timedTicks;
                    Require(sample.timedTicks,"Actual Update burn tick cadence/damage differs: "+name);
                    string path=Path.Combine(folder,"items-"+name+".png");ScreenCapture.CaptureScreenshot(path);
                    yield return null;yield return new WaitForSecondsRealtime(.15f);
                    sample.captureExists=File.Exists(path)&&new FileInfo(path).Length>0;
                    Require(sample.captureExists,"Native item capture missing: "+name);
                }
            }
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
