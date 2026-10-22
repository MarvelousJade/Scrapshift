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
        static void StartHeadlessItemChainReview()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--headless-item-chain-review");
            if(index<0||index+1>=args.Length)return;
            new GameObject("Headless item explosion chain acceptance").AddComponent<HeadlessItemChainReview>().folder=args[index+1];
        }
        sealed class HeadlessItemChainReview:MonoBehaviour
        {
            public string folder;
            [Serializable] sealed class ExplosionRow
            {
                public string item;public int copies;public bool greekFire;
                public float expectedHealth,observedHealth,playerHealthBefore,playerHealthAfter;
                public int burnTicks;public bool lifeStealTimerUntouched;
            }
            [Serializable] sealed class FireworksRow
            {
                public int living,zeroHpControls,pendingDeaths,globalKillsBeforeFlush;
                public bool expected,completed;public string profile;
            }
            [Serializable] sealed class Report
            {
                public string status,error;
                public bool headless=true,capturesVerified=false,isolated,initializedProductionHook,beardBurns,beardStealsLife;
                public string scope="Opt-in built-player synchronous item dispatch using real imported spawned NPCs after production ArenaGame.Start, then ArenaGame disabled. Post-unlock capped loadouts, HP and life-steal chance are staged. Actual QueueImplantExplosions/ResolveImplantAttacks and damage callbacks run. Fireworks uses real unlock definitions and fresh isolated profiles before global death flushing. Beard control applies its actual imported projectile stats through Damage; no projectile travel, ordinary Update/physics, rendered UI, audible, input or human acceptance.";
                public List<ExplosionRow> explosions=new List<ExplosionRow>();
                public List<FireworksRow> fireworks=new List<FireworksRow>();
            }
            static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
            IEnumerator Start()
            {
                yield return null;yield return null;
                var report=new Report();
                try
                {
                    folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);
                    report.isolated=GameSettings.IsFixture;
                    Require(Application.isBatchMode&&report.isolated,"Requires -batchmode -nographics and an isolated --headless-item-chain-review profile.");
                    var game=FindAnyObjectByType<ArenaGame>();Require(game!=null,"Initialized ArenaGame required.");
                    var hook=game.Rules.CaptureRandomDamageTarget;
                    report.initializedProductionHook=hook!=null&&ReferenceEquals(hook.Target,game)&&hook.Method.Name=="CaptureImplantDamageTarget";
                    Require(report.initializedProductionHook,"Actual ArenaGame.Start must install its production hook.");
                    game.enabled=false;game.sound.SetMuted(true);
                    foreach(string item in new[]{"rip_and_tear","spicy_sauce"})foreach(bool greek in new[]{false,true})
                        report.explosions.Add(CheckExplosion(game,item,greek));
                    foreach(int living in new[]{14,15})report.fireworks.Add(CheckFireworks(game,living,folder));
                    Prepare(game);Add(game,"baby_with_a_beard",1);Add(game,"scared_sausage",4);game.Rules.Hurt(3);
                    var target=SpawnTarget(game,1000);float health=game.Rules.Health;
                    var stats=game.Rules.Effects("projectiles_on_death").Single().projectile.AsEquipment();
                    game.Damage(game.enemies.IndexOf(target),1,0,0,Vector2.up,stats,true);
                    report.beardBurns=target.burnTicks==3&&target.burnDamage==4;
                    report.beardStealsLife=game.Rules.Health==health+1;
                    Require(report.beardBurns&&report.beardStealsLife,"Beard projectile must retain its separately initialized global burning and projectile life steal.");
                    game.ClearCombat();report.status="Passed";
                }
                catch(Exception ex){report.status="Failed";report.error=ex.ToString();}
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"headless-item-chain.json"),JsonUtility.ToJson(report,true));
                Application.Quit(report.status=="Passed"?0:1);
            }
            static void Prepare(ArenaGame game)
            {
                game.ClearCombat();game.Rules.Progress=null;
                var character=game.catalog.data.characters.Single(entry=>entry.id=="well_rounded");
                game.Rules.Start(character,StarterChoices.Available(game.catalog,character).First());game.Rules.Inventory.Clear();
                game.Rules.GainWeaponStat("crit_chance",-1000);game.Rules.GainWeaponStat("lifesteal",100);
            }
            static void Add(ArenaGame game,string id,int copies)
            {
                var item=game.catalog.data.items.Single(entry=>entry.id==id);
                Require(item.maxCount<=0||copies<=item.maxCount,"Loadout exceeds imported cap for "+id);
                for(int i=0;i<copies;i++)game.Rules.GrantItem(item);
            }
            static Enemy SpawnTarget(ArenaGame game,float health)
            {
                game.Spawn(game.catalog.Enemy("baby_alien"),game.playerPos+new Vector2(6,6));
                var target=game.enemies.Last();Destroy(target.marker);target.marker=null;target.obj.SetActive(true);
                target.hp=target.maxHp=health;return target;
            }
            static ExplosionRow CheckExplosion(ArenaGame game,string item,bool greek)
            {
                Prepare(game);int copies=item=="rip_and_tear"?5:2;
                Add(game,item,copies);Add(game,"scared_sausage",4);if(greek)Add(game,"greek_fire",1);
                game.Rules.Hurt(3);float before=game.Rules.Health;
                var target=SpawnTarget(game,1000);string trigger=item=="rip_and_tear"?"explode_on_death":"explode_on_consumable";
                Require(game.Rules.TryItemExplosion(trigger,0,out _,out float damage),"Guaranteed capped item explosion required.");
                game.QueueImplantExplosions(trigger,target.pos);game.ResolveImplantAttacks();
                var row=new ExplosionRow{item=item,copies=copies,greekFire=greek,expectedHealth=1000-damage,observedHealth=target.hp,
                    playerHealthBefore=before,playerHealthAfter=game.Rules.Health,burnTicks=target.burnTicks};
                row.lifeStealTimerUntouched=game.Rules.TryLifeSteal(0);
                Require(row.observedHealth==row.expectedHealth&&row.burnTicks==0,"Plain item explosion acquired burning/Greek Fire damage: "+item);
                Require(row.playerHealthAfter==row.playerHealthBefore&&row.lifeStealTimerUntouched,"Plain item explosion consumed life steal: "+item);
                return row;
            }
            static FireworksRow CheckFireworks(ArenaGame game,int living,string folder)
            {
                Prepare(game);Add(game,"rip_and_tear",5);
                var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
                string profile=Path.Combine(folder,"fireworks-"+living+"-profile.json");
                Require(!File.Exists(profile),"Fresh isolated Fireworks profile required; use a new output directory.");
                game.Rules.Progress=new Progression(definitions,profile);
                Require(!game.Rules.Progress.Completed("chal_fireworks"),"Fireworks must begin locked.");
                for(int i=0;i<living;i++)SpawnTarget(game,1);
                SpawnTarget(game,0);
                game.QueueImplantExplosions("explode_on_death",game.enemies[0].pos);game.ResolveImplantAttacks();
                var row=new FireworksRow{living=living,zeroHpControls=1,pendingDeaths=game.pendingEnemyDeaths.Count,
                    globalKillsBeforeFlush=game.Rules.Kills,expected=living>=15,completed=game.Rules.Progress.Completed("chal_fireworks"),profile=profile};
                Require(row.pendingDeaths==living&&row.globalKillsBeforeFlush==0,"Global deaths must remain pending while counting the explosion's immediate lethal transitions.");
                Require(row.completed==row.expected,"Fireworks direct-kill threshold differs or counted an already-zero-HP candidate.");
                game.Rules.Progress.Save();return row;
            }
        }
    }
}
