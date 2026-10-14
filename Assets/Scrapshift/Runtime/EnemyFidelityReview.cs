using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool enemyFidelityReview;string enemyFidelityPath;int enemyFidelityIndex;float enemyFidelityNext;
        readonly List<string> enemyFidelityPassed=new List<string>(),enemyFidelityFailed=new List<string>();
        readonly List<(EncounterEntry enemy,int state)> enemyCases=new List<(EncounterEntry,int)>();
        [Serializable] sealed class EnemyFidelityReport {public string status,scope="Built-player checks of every imported enemy/state, 12 seconds of combat per state, attack payloads, phase selection and source-specific slash warnings. Screenshots are staged; this is not a claim of identical physics, art, or random sequences.";public string[] passed,failed;}
        void BeginEnemyFidelityReview(string folder)
        {
            enemyFidelityReview=true;enemyFidelityPath=Path.GetFullPath(folder);Directory.CreateDirectory(enemyFidelityPath);sound.SetMuted(true);
            foreach(var d in encounterData.enemies)for(int i=0;i<d.states.Length;i++)enemyCases.Add((d,i));enemyFidelityNext=Time.realtimeSinceStartup+.5f;
        }
        void ExerciseEnemy(EncounterEntry profile,int state)
        {
            Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();SyncEquipment();playerPos=Vector2.zero;invulnerable=999;
            Spawn(catalog.Enemy(profile.id),new Vector2(-2,0));var e=enemies.Last();Destroy(e.marker);e.marker=null;e.obj.SetActive(true);
            e.behavior=EncounterBehavior(profile,state);e.attack=MakeAttack(e.behavior);encounterStates[e]=state;ResetEnemyMovement(e);
            for(int frame=0;frame<720;frame++)
            {
                UpdateEnemies(1f/60);UpdateBirthQueue(1f/60);UpdateSlashes(1f/60);UpdateWeapons(1f/60);
                foreach(var enemy in enemies)if(float.IsNaN(enemy.pos.x)||float.IsNaN(enemy.pos.y))throw new Exception("Non-finite enemy movement");
            }
        }
        void EnemyFidelityReviewTick()
        {
            if(!enemyFidelityReview||Time.realtimeSinceStartup<enemyFidelityNext)return;
            if(enemyFidelityIndex>=enemyCases.Count)
            {
                File.WriteAllText(Path.Combine(enemyFidelityPath,"report.json"),JsonUtility.ToJson(new EnemyFidelityReport{status=enemyFidelityFailed.Count==0?"Passed":"Failed",passed=enemyFidelityPassed.ToArray(),failed=enemyFidelityFailed.ToArray()},true));
                enabled=false;Application.Quit(enemyFidelityFailed.Count==0?0:1);return;
            }
            var c=enemyCases[enemyFidelityIndex++];string label=c.enemy.id+"-"+c.state;
            try
            {
                ExerciseEnemy(c.enemy,c.state);
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();SyncEquipment();arrivalAge=ArrivalMotion.Duration;playerPos=Vector2.zero;invulnerable=999;
                director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},1);
                Spawn(catalog.Enemy(c.enemy.id),new Vector2(-2,0));var e=enemies.Last();Destroy(e.marker);e.marker=null;e.obj.SetActive(true);
                e.behavior=EncounterBehavior(c.enemy,c.state);e.attack=MakeAttack(e.behavior);encounterStates[e]=c.state;ResetEnemyMovement(e);
                var a=c.enemy.states[c.state].attack;
                if(a.charge)
                {
                    e.attack=new EnemyAttack(e.behavior,()=>0);e.attack.Step(.01f,Vector2.right,true,()=>Vector2.right);e.attack.Step(a.windup*.5f,Vector2.right,true);
                    UpdateEnemyPresentation(e,Vector2.zero,0);
                    e.attack.Step(a.windup*.5f,Vector2.up,true,()=>Vector2.up);if(e.attack.Phase!=AttackPhase.Active)throw new Exception("Charge did not release");
                    e.attack=new EnemyAttack(e.behavior,()=>0);e.attack.Step(.01f,Vector2.right,true,()=>Vector2.right);e.attack.Step(a.windup*.6f,Vector2.right,true);
                }
                else if(a.enabled)
                {
                    FireEncounter(e);
                    if(!a.spawn&&shots.Count+slashes.Count!=a.count)throw new Exception("Wrong attack projectile count");
                    foreach(var shot in shots){shot.age=shot.pillar?.55f:.1f;shot.obj.GetComponent<EnemyProjectileView>()?.Pose(shot.age);}
                    foreach(var slash in slashes)slash.obj.GetComponent<EnemyProjectileView>()?.Pose(.2f);
                }
                if(e.obj!=null&&e.hp>0)UpdateEnemyPresentation(e,Vector2.zero,0);
                gameCamera.orthographicSize=5;gameCamera.transform.position=new Vector3(0,0,-20);
                enemyFidelityPassed.Add(label);Debug.Log("Enemy fidelity "+label+" passed");
                // Hide pause UI for the capture; simulation remains gated by the review branch.
                ScreenCapture.CaptureScreenshot(Path.Combine(enemyFidelityPath,label+".png"));
            }
            catch(Exception ex){enemyFidelityFailed.Add(label+": "+ex.Message);Debug.LogException(ex);}
            enemyFidelityNext=Time.realtimeSinceStartup+.12f;
        }
    }
}
