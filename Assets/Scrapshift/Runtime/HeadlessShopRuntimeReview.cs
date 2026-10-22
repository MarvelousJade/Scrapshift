using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartHeadlessShopReview()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--headless-shops-review");if(i<0||i+1>=args.Length)return;
            new GameObject("Headless shop action acceptance").AddComponent<HeadlessShopReview>().folder=args[i+1];
        }
        sealed class HeadlessShopReview:MonoBehaviour
        {
            public string folder;
            [Serializable] sealed class Receipt { public string name,details; }
            [Serializable] sealed class Report
            {
                public string status,error,scope="Finite production RunRules action receipts using the built catalog. Inventory and offers explicitly staged. Candy additionally exercises AddWaveEncounters and an empty SpawnDirector emission callback, without spawning world actors. No displayed-price, GUI, input, physics, audible or human evidence.";
                public bool headless=true,capturesVerified=false,isolated;
                public List<Receipt> checks=new List<Receipt>();
            }
            IEnumerator Start()
            {
                var report=new Report();Directory.CreateDirectory(folder);yield return null;
                try
                {
                    report.isolated=GameSettings.IsFixture;
                    if(!Application.isBatchMode||!report.isolated)throw new InvalidOperationException("Requires -batchmode -nographics and isolated --headless-shops-review.");
                    var game=FindAnyObjectByType<ArenaGame>();if(game==null)throw new InvalidOperationException("ArenaGame with catalog required.");
                    foreach(string name in HeadlessShopAcceptance.Cases)
                    {
                        if(name!="lock-transition")
                            report.checks.Add(new Receipt{name=name,details=HeadlessShopAcceptance.Execute(game.catalog,name)});
                        else
                        {
                            bool original=GameSettings.Flag("keepLock");
                            try
                            {
                                foreach(bool keep in new[]{true,false})
                                {
                                    GameSettings.Set("keepLock",keep);
                                    if(GameSettings.Flag("keepLock")!=keep)throw new InvalidOperationException("Headless settings overrides required; user preferences must never change.");
                                    report.checks.Add(new Receipt{name=name+"-keep-"+keep,details=HeadlessShopAcceptance.Execute(game.catalog,name)});
                                }
                            }
                            finally{GameSettings.Set("keepLock",original);}
                        }
                    }
                    report.checks.Add(CheckCandyEncounterRequests(game));
                    report.status="Passed";
                }
                catch(Exception ex){report.status="Failed";report.error=ex.ToString();}
                File.WriteAllText(Path.Combine(folder,"headless-shops-review.json"),JsonUtility.ToJson(report,true));Application.Quit(report.status=="Passed"?0:1);
            }
            static Receipt CheckCandyEncounterRequests(ArenaGame game)
            {
                if(game.encounterData==null)throw new InvalidOperationException("Loaded encounter definitions required.");
                var eligible=game.encounterData.enemies.Where(e=>e.role=="Elite").Select(e=>e.id).ToArray();
                if(eligible.Length==0)throw new InvalidOperationException("Reference-derived eligible elite pool required.");
                int success=0,failure=0,attempts=0;var selected=new List<string>();
                // Suppress scheduled elites explicitly; an empty director makes
                // every emitted group attributable to AddWaveEncounters here.
                game.eliteWaves.Clear();game.Rules.Difficulty=0;game.Rules.Progress=null;
                for(;attempts<128&&(success==0||failure==0);attempts++)
                {
                    game.Rules.Configure(game.catalog);game.Rules.Start();
                    game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="candy_bag"));
                    game.Rules.Tick(game.Rules.Remaining);while(game.Rules.Phase==RunPhase.Upgrade)game.Rules.Choose(0);
                    if(game.Rules.Phase!=RunPhase.Shop||game.Rules.Stat("extra_elite_next_wave_chance")!=10)throw new InvalidOperationException("Natural imported Candy shop boundary required.");
                    game.Rules.NextWave();bool requested=game.Rules.ExtraElite;
                    game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},attempts);
                    game.AddWaveEncounters();int emitted=0;
                    game.director.Tick(1,(group,unit,count)=>
                    {
                        if(!group.boss||count!=1||!eligible.Contains(unit.id))throw new InvalidOperationException("Candy emitted a noneligible or non-single elite request.");
                        emitted+=count;selected.Add(unit.id);
                    });
                    if(emitted!=(requested?1:0))throw new InvalidOperationException("Candy request must emit exactly one eligible group on success and none on failure.");
                    if(requested)success++;else failure++;
                }
                if(success==0||failure==0)throw new InvalidOperationException("Bounded 128 natural chances did not exercise both outcomes.");
                return new Receipt{name="candy-actual-encounter-request",details=attempts+" bounded natural trials; success="+success+", failure="+failure+"; eligible single-elite callback IDs="+string.Join(",",selected)+"; no actors/physics/UI claimed"};
            }
        }
    }
}
