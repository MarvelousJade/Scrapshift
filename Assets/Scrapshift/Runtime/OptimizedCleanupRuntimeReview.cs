using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartOptimizedCleanupReview()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--optimized-cleanup-review");if(i<0||i+1>=args.Length)return;
            new GameObject("Optimized cleanup staged native review").AddComponent<NativeOptimizedCleanupReview>().folder=args[i+1];
        }
        sealed class NativeOptimizedCleanupReview:MonoBehaviour
        {
            public string folder;ArenaGame game;
            [Serializable] sealed class Report
            {
                public string status,error,scope="Opt-in staged cleanup boundary fixture. Runs actual Update sweep/pickup logic with guaranteed double materials and a deliberately unrelated material-value modifier. Normal cleanup begins through production BeginWaveSweep; lethal damage is model-staged. No human campaign or physical input evidence.";
                public bool isolated,normalBankedOnce,normalFruitHealed,deathBankedOnce,deathFruitPreserved,deathNoIncome,capturesExist;
                public int normalBank,deathBank,normalMaterials,deathMaterials,normalXP,deathXP;
                public float normalHealth,deathHealth;
            }
            readonly Report report=new Report();
            IEnumerator Start()
            {
                float started=Time.realtimeSinceStartup;string error=null;var routine=Run();
                while(true)
                {
                    bool more=false;object next=null;
                    try{Require(Time.realtimeSinceStartup-started<12,"Optimized cleanup review exceeded 12 seconds.");more=routine.MoveNext();if(more)next=routine.Current;}
                    catch(Exception ex){error=ex.ToString();}
                    if(!more)break;yield return next;
                }
                report.error=error;report.capturesExist=new[]{"normal-optimized.png","death-optimized.png"}.All(n=>File.Exists(Path.Combine(folder,n))&&new FileInfo(Path.Combine(folder,n)).Length>0);
                report.status=error==null&&report.isolated&&report.normalBankedOnce&&report.normalFruitHealed&&report.deathBankedOnce&&report.deathFruitPreserved&&report.deathNoIncome&&report.capturesExist?"Passed":"Failed";
                File.WriteAllText(Path.Combine(folder,"optimized-cleanup-review.json"),JsonUtility.ToJson(report,true));Application.Quit(report.status=="Passed"?0:1);
            }
            IEnumerator Run()
            {
                Directory.CreateDirectory(folder);yield return null;yield return null;game=FindAnyObjectByType<ArenaGame>();Require(game!=null,"ArenaGame required.");
                report.isolated=GameSettings.IsFixture&&Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0;Require(report.isolated,"Use an isolated --controller-check profile.");
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);GameSettings.Set("optimizeEndWaves",true);
                Stage();game.Rules.Hurt(6);game.Drop(new Vector2(7,0),2,false);game.Drop(new Vector2(-7,0),3,false);game.Drop(game.playerPos,1,true);game.BeginWaveSweep();
                yield return null;yield return null;
                float wait=Time.realtimeSinceStartup;while(game.Rules.Health<7&&Time.realtimeSinceStartup-wait<.7f)yield return null;
                report.normalBank=game.Rules.Banked;report.normalMaterials=game.Rules.Scrap;report.normalXP=game.Rules.XP;report.normalHealth=game.Rules.Health;
                report.normalBankedOnce=report.normalBank==10&&!game.pickups.Any(p=>!p.fruit)&&report.normalMaterials==100&&report.normalXP==0;
                report.normalFruitHealed=report.normalHealth==7;
                Require(report.normalBankedOnce&&report.normalFruitHealed,"Normal optimized cleanup must bank only double raw value once while still processing fruit.");
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"normal-optimized.png"));yield return null;
                Stage();game.Drop(new Vector2(7,0),2,false);game.Drop(new Vector2(-7,0),3,false);game.Drop(game.playerPos,1,true);game.Rules.Hurt(1000);
                yield return null;yield return null;yield return new WaitForSecondsRealtime(.7f);
                report.deathBank=game.Rules.Banked;report.deathMaterials=game.Rules.Scrap;report.deathXP=game.Rules.XP;report.deathHealth=game.Rules.Health;
                report.deathBankedOnce=report.deathBank==10&&!game.pickups.Any(p=>!p.fruit);
                report.deathFruitPreserved=game.pickups.Count(p=>p.fruit)==1&&report.deathHealth==0;
                report.deathNoIncome=report.deathMaterials==100&&report.deathXP==0&&!game.Rules.WaveSettlementPending&&game.Rules.Stat("harvesting")==0;
                Require(report.deathBankedOnce&&report.deathFruitPreserved&&report.deathNoIncome,"Death optimized cleanup must bank only double raw value once, retain dead fruit and skip player pickup/harvest rewards.");
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"death-optimized.png"));yield return null;yield return new WaitForSecondsRealtime(.2f);
            }
            void Stage()
            {
                game.Begin();game.ClearCombat();game.Rules.Start(startingMaterials:100);game.Rules.Progress=null;game.Rules.Inventory.Clear();game.SyncEquipment();
                game.Rules.GainWeaponStat("chance_double_gold",100);game.Rules.GainWeaponStat("increase_material_value",100);game.Rules.GainWeaponStat("heal_when_pickup_gold",100);
                game.arrivalAge=ArrivalMotion.Duration;game.invulnerable=0;game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},771);game.births.Clear();game.sound.SetMuted(true);
            }
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
