using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartStartingSupportNativeReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--starting-support-review");
            if(index<0||index+1>=args.Length)return;
            new GameObject("Native starting item and support acceptance").AddComponent<StartingSupportNativeReview>().folder=args[index+1];
        }
        sealed class StartingSupportNativeReview:MonoBehaviour
        {
            public string folder;ArenaGame game;Gamepad pad;UnlockDefinition[] definitions;
            [Serializable] sealed class Choice
            {public string item;public bool selectedThroughOnGui,previewMatched,startedThroughOnGui,retriedThroughOnGui;public string[] startedItems,retriedItems,pets;}
            [Serializable] sealed class Placement
            {public int seed;public Vector2 turret,mine,wandering,pet;public bool mineOutsideGroup;}
            [Serializable] sealed class Report
            {
                public string status,error;
                public bool isolated,registeredTurretZeroRejected,registeredTurretFourRejected,registeredTurretFiveUnlocked;
                public string scope="Visible player fixture. Character/page/unlock availability and logical focus are staged; software Gamepad A traverses actual Update/OnGUI choice, difficulty start and retry controls. No OS input or physical controller acceptance. Support registration/placement use explicit native callbacks with staged group centers and 64 seeds; screenshots require separate visual inspection. Not a full run, ordinary focus test or human playtest.";
                public List<Choice> choices=new List<Choice>();public List<Placement> placements=new List<Placement>();public List<string> captures=new List<string>();
            }
            readonly Report report=new Report();
            static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
            IEnumerator Start()
            {
                folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);
                var runner=Run();string error=null;
                while(true)
                {
                    bool next=false;object current=null;
                    try{next=runner.MoveNext();if(next)current=runner.Current;}catch(Exception ex){error=ex.ToString();}
                    if(!next)break;yield return current;
                }
                report.status=error==null?"Passed":"Failed";report.error=error;
                File.WriteAllText(Path.Combine(folder,"starting-support.json"),JsonUtility.ToJson(report,true));
                if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
                Application.Quit(error==null?0:1);
            }
            IEnumerator Run()
            {
                yield return null;yield return null;
                Require(!Application.isBatchMode,"This fixture requires a visible native player with OnGUI.");
                Require(Array.IndexOf(Environment.GetCommandLineArgs(),"--controller-check")>=0,"Use --controller-check <isolated-folder>.");
                report.isolated=GameSettings.IsFixture;Require(report.isolated,"Isolated settings required.");
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null,"ArenaGame initialization required.");
                game.catalog=Instantiate(game.catalog);game.Rules.Configure(game.catalog);
                definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
                game.progress=new Progression(definitions);game.runUnlockPool=new RunUnlockPool();
                string[] unlocked={"character_beast_master","item_bonk_dog","item_catling_gun","item_blazemander","item_bot_o_mine"};
                foreach(var definition in definitions)if(unlocked.Contains(definition.reward))game.progress.Data.completed.Add(definition.id);
                game.RefreshUnlockPoolForNewRun();
                Application.runInBackground=true;GameSettings.Set("pauseFocus",false);GameSettings.Set("retryWave",true);
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();State(new GamepadState());game.sound.SetMuted(true);
                var beast=game.catalog.data.characters.Single(c=>c.id=="beast_master");
                game.ChangeLook(Array.IndexOf(game.catalog.data.characters,beast)-game.playerAnimation.lookIndex);
                Require(game.CharacterStartingItems.Length==4&&game.CharacterWeapons.Length==0,"Unlocked Beast Master must offer exactly four item choices, without weapons.");
                for(int choice=0;choice<4;choice++)
                {
                    game.ReturnToSelection(2);game.randomCharacter=false;game.startingWeapon=0;
                    yield return new WaitForSecondsRealtime(.2f);
                    Require(game.menuNavigationButtons.Count==6,"Item picker must register Back, Random and four choices.");
                    game.menuNavigationActive=true;game.menuFocus=choice+2;
                    yield return new WaitForSecondsRealtime(.2f);
                    string selected=game.CharacterStartingItems[choice].id;
                    Require(game.SelectedStartingItem?.id==selected,"Actual OnGUI focus must update the starting item preview.");
                    var row=new Choice{item=selected,previewMatched=Matches(game.PreviewRules(),selected)};
                    Require(row.previewMatched,"Preview must grant only chosen pet and Lootworm.");
                    yield return Capture("choice-"+selected+".png");
                    yield return Press();row.selectedThroughOnGui=game.menuPage==3&&game.SelectedStartingItem?.id==selected;
                    Require(row.selectedThroughOnGui,"Software A must confirm the selected item through OnGUI.");
                    yield return Press();row.startedThroughOnGui=game.Rules.Phase==RunPhase.Playing;
                    Require(row.startedThroughOnGui&&Matches(game.Rules,selected),"Actual difficulty button must start selected pet plus Lootworm.");
                    StabilizeCombat();row.startedItems=game.Rules.Items.Select(i=>i.id).OrderBy(i=>i).ToArray();row.pets=game.pets.Select(p=>p.spec.kind).OrderBy(i=>i).ToArray();
                    Require(row.pets.SequenceEqual(new[]{"lootworm",selected}.OrderBy(i=>i)),"Deployed pets must match the selected starting item and grant.");
                    yield return Capture("run-"+selected+".png");
                    game.Rules.Hurt(100000);
                    float deadline=Time.realtimeSinceStartup+4;
                    while((game.Rules.DeathCleanupPending||!game.Rules.CanRetryWave)&&Time.realtimeSinceStartup<deadline)yield return null;
                    Require(game.Rules.CanRetryWave,"Defeat cleanup must expose retry.");
                    yield return new WaitForSecondsRealtime(.15f);game.menuNavigationActive=true;game.menuFocus=0;
                    yield return Capture("retry-"+selected+".png");
                    yield return Press();row.retriedThroughOnGui=game.Rules.Phase==RunPhase.Playing&&game.Rules.Retries==1;
                    Require(row.retriedThroughOnGui&&Matches(game.Rules,selected),"Actual retry control must restore selected starting items.");
                    StabilizeCombat();row.retriedItems=game.Rules.Items.Select(i=>i.id).OrderBy(i=>i).ToArray();
                    Require(game.pets.Select(p=>p.spec.kind).OrderBy(i=>i).SequenceEqual(row.pets),"Retry must redeploy the same companions.");
                    report.choices.Add(row);
                }
                game.enabled=false;game.ClearCombat();CheckRegistration();CheckPlacement();
                // Manual staging bypasses the ordinary pet/player presentation
                // tick; normalize shadows without advancing the checked poses.
                game.TickPets(0);game.playerShadow.Place(game.playerPos,ActorPresentation.PlayerScale);
                game.gameCamera.orthographicSize=ArenaGeometry.CameraHalfHeight;game.gameCamera.transform.position=Vector3.back*20;
                yield return Capture("support-placement.png");
                Require(report.choices.Count==4&&report.placements.Count==64,"Complete finite acceptance coverage required.");
            }
            static bool Matches(RunRules rules,string selected)=>rules.Inventory.Count==0&&rules.Items.Select(i=>i.id).OrderBy(i=>i).SequenceEqual(new[]{"lootworm",selected}.OrderBy(i=>i));
            void StabilizeCombat()
            {
                game.invulnerable=100000;game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},771);game.births.Clear();game.arrivalAge=ArrivalMotion.Duration;
                if(game.parachute!=null)Destroy(game.parachute);game.sound.SetMuted(true);
            }
            ItemEntry Item(string id)=>game.catalog.data.items.Single(i=>i.id==id);
            StructureSpec Spec(string id)=>Item(id).conditionalEffects.Single(e=>e.customKey=="structure").structure;
            void CheckRegistration()
            {
                game.progress=new Progression(definitions);game.Rules.Progress=game.progress;
                game.Rules.Start(game.catalog.data.characters.Single(c=>c.id=="well_rounded"),"pistol");game.Rules.Inventory.Clear();
                foreach(string id in new[]{"pocket_factory","garden","landmines","wandering_bot"})game.Rules.Items.Add(Item(id));
                game.Rules.Inventory.Add(game.catalog.Weapon("screwdriver",1));game.Rules.Inventory.Add(game.catalog.Weapon("pruner",1));
                game.ResetStructures();report.registeredTurretZeroRejected=game.Rules.RegisteredTurretCount==0&&!game.progress.Completed("chal_turrets");
                for(int i=0;i<4;i++)game.Rules.Items.Add(Item("turret"));
                game.ResetStructures();report.registeredTurretFourRejected=game.Rules.RegisteredTurretCount==4&&!game.progress.Completed("chal_turrets");
                game.Rules.Inventory.Add(game.catalog.Weapon("wrench",1));game.ResetStructures();
                report.registeredTurretFiveUnlocked=game.Rules.RegisteredTurretCount==5&&game.progress.Completed("chal_turrets");
                Require(report.registeredTurretZeroRejected&&report.registeredTurretFourRejected&&report.registeredTurretFiveUnlocked,"Deployment unlock must count registered turrets, including Wrench, and exclude other actors.");
            }
            void CheckPlacement()
            {
                game.Rules.Progress=null;game.Rules.Start(game.catalog.data.characters.Single(c=>c.id=="engineer"),"wrench");game.Rules.Inventory.Clear();game.Rules.Items.Clear();
                game.playerPos=new Vector2(4,2);game.player.transform.position=game.playerPos;
                bool outside=false;
                for(int seed=0;seed<64;seed++)
                {
                    game.ClearStarterEffects();game.ClearPets();UnityEngine.Random.InitState(seed);
                    game.structureGroupCenter=new Vector2(-4,-2);game.petGroupCenter=new Vector2(4,-2);
                    game.DeployStructure(Spec("turret"),null);var turret=game.starterTurrets.Last();
                    game.DeployStructure(Spec("landmines"),null);var mine=game.mines.Last();
                    game.DeployStructure(Spec("wandering_bot"),null);var wandering=game.starterTurrets.Last();
                    game.SpawnPet(Item("bonk_dog").conditionalEffects.Single(e=>e.customKey=="pet").pet);var pet=game.pets.Last();
                    float span=SupportBehavior.GroupedSpawnSpan(game.Rules.RegisteredStructureCount);
                    Require(InArea(turret.pos,game.structureGroupCenter,span),"Grouped turret must use structure group.");
                    Require(InArea(wandering.pos,game.playerPos,2),"Wandering Bot's player-area override must precede grouping.");
                    Require(InArea(pet.pos,game.petGroupCenter,span),"Pet must use its separate pet group.");
                    Vector2 allowed=SpawnPlacement.SourceRandomPosition(1,0,1.6f);
                    Require(Mathf.Abs(mine.pos.x)<=Mathf.Abs(allowed.x)+.0001f&&Mathf.Abs(mine.pos.y)<=Mathf.Abs(allowed.y)+.0001f,"Ungrouped mine must retain source map-edge inset.");
                    bool mineOutside=!InArea(mine.pos,game.structureGroupCenter,span);outside|=mineOutside;
                    report.placements.Add(new Placement{seed=seed,turret=turret.pos,mine=mine.pos,wandering=wandering.pos,pet=pet.pos,mineOutsideGroup=mineOutside});
                }
                Require(outside,"Mines must not be forced into the structure group.");
            }
            static bool InArea(Vector2 position,Vector2 center,float span)
            {
                var low=SpawnPlacement.SourceAreaPosition(center,span,0,1);var high=SpawnPlacement.SourceAreaPosition(center,span,1,0);
                return position.x>=low.x-.0001f&&position.x<=high.x+.0001f&&position.y>=low.y-.0001f&&position.y<=high.y+.0001f;
            }
            IEnumerator Capture(string name)
            {
                yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));report.captures.Add(name);yield return null;
            }
            IEnumerator Press()
            {
                State(new GamepadState().WithButton(GamepadButton.South));yield return new WaitForSecondsRealtime(.08f);
                State(new GamepadState());yield return new WaitForSecondsRealtime(.18f);
            }
            void State(GamepadState state)=>InputSystem.QueueStateEvent(pad,state);
        }
    }
}
