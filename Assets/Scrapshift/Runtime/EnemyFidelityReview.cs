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
        int slashReviewIndex;GameObject pendingExpiredSlash;SlashFrameRecord pendingExpiredRecord;
        readonly List<SlashFrameRecord> slashFrames=new List<SlashFrameRecord>();
        [Serializable] sealed class SlashFrameRecord
        {
            public string variant,capture;public float age,healthBefore,healthAfter;
            public int renderedFrame,spriteFrame;public bool expectedActive,live,contactRegistered,rendererMatchesFrame,rendererGoneAfterFrame;
        }
        [Serializable] sealed class EnemyFidelityReport {public string status,scope="Built-player checks of every imported enemy/state, 12 logical seconds of simulation per state, attack payloads, phase selection and source-specific slash warnings. Eight separately rendered slash lifecycle frames also check warning/active/expiry contacts. Screenshots are staged; this is not identical physics, art, random sequences or a normal-timing full run.";public string[] passed,failed;public SlashFrameRecord[] slashSequence;}
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
                if(pendingExpiredRecord!=null)
                {
                    pendingExpiredRecord.rendererGoneAfterFrame=pendingExpiredSlash==null;
                    if(!pendingExpiredRecord.rendererGoneAfterFrame)enemyFidelityFailed.Add("Slash renderer survived expiry frame: "+pendingExpiredRecord.variant);
                    pendingExpiredRecord=null;
                }
                if(slashReviewIndex<8)
                {
                    int stage=slashReviewIndex++;
                    try{StageSlashFrame(stage);}catch(Exception ex){enemyFidelityFailed.Add("slash-sequence-"+stage+": "+ex.Message);Debug.LogException(ex);}
                    enemyFidelityNext=Time.realtimeSinceStartup+.2f;return;
                }
                foreach(var frame in slashFrames)
                {
                    string capture=Path.Combine(enemyFidelityPath,frame.capture);
                    if(!File.Exists(capture)||new FileInfo(capture).Length==0)enemyFidelityFailed.Add("Missing slash capture: "+frame.capture);
                }
                File.WriteAllText(Path.Combine(enemyFidelityPath,"report.json"),JsonUtility.ToJson(new EnemyFidelityReport{status=enemyFidelityFailed.Count==0?"Passed":"Failed",passed=enemyFidelityPassed.ToArray(),failed=enemyFidelityFailed.ToArray(),slashSequence=slashFrames.ToArray()},true));
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
        void StageSlashFrame(int stage)
        {
            bool forward=stage>=4;int index=stage%4;
            float[] ages={.2f,.48f,.54f,.63f};int[] expectedFrames={0,1,3,6};
            string variant=forward?"forward":"standard",name="slash-"+variant+"-"+index+".png";
            Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();SyncEquipment();
            arrivalAge=ArrivalMotion.Duration;invulnerable=0;activePlayerContactBatch=null;
            director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},1);
            var definition=encounterData.enemies.First(x=>x.id==(forward?"tentacle":"slasher"));
            Spawn(catalog.Enemy(definition.id),new Vector2(-2,0));var owner=enemies.Last();
            Destroy(owner.marker);owner.marker=null;owner.obj.SetActive(true);
            owner.behavior=EncounterBehavior(definition,0);owner.attack=MakeAttack(owner.behavior);encounterStates[owner]=0;
            FireEncounter(owner);if(slashes.Count==0)throw new Exception("Source attack did not create slash projectiles.");
            var slash=slashes[0];
            // Isolate one real production projectile, preserving its source variant.
            foreach(var other in slashes.Skip(1).ToArray()){Destroy(other.obj);slashes.Remove(other);}
            if(slash.forward!=forward)throw new Exception("Wrong slash variant.");
            // At .48 and .54 the source collision centers are -122 and -6.5 px.
            // Warning/expired probes use the first center and must never hit.
            float center=index==2?-.065f:-1.22f;
            Vector2 normal=new Vector2(-slash.direction.y,slash.direction.x);
            playerPos=slash.pos+slash.direction*((forward?1.65f:0)+center)-normal*.03f-CombatGeometry.PlayerHurtOffset;
            player.transform.position=new Vector3(playerPos.x,playerPos.y,-1);player.SetActive(true);
            Rules.Heal(100);float before=Rules.Health;float age=ages[index];
            var obj=slash.obj;var profile=EnemyProjectileView.Profile(forward?"projectiles/slash/slash_projectile_2.tscn":"projectiles/slash/slash_projectile.tscn");
            UpdateSlashes(age);bool active=index==1||index==2,live=slashes.Contains(slash);
            var renderer=obj.GetComponentInChildren<SpriteRenderer>();
            var record=new SlashFrameRecord{variant=variant,capture=name,age=age,renderedFrame=Time.frameCount,
                spriteFrame=profile.FrameIndex(age),expectedActive=active,live=live,contactRegistered=slash.hit,
                healthBefore=before,healthAfter=Rules.Health,rendererMatchesFrame=live&&renderer!=null&&renderer.enabled&&renderer.sprite==profile.Frame(age)};
            slashFrames.Add(record);
            if(record.spriteFrame!=expectedFrames[index])throw new Exception("Source frame boundary mismatch.");
            if(live!=(index<3)||active!=slash.hit||active!=(Rules.Health<before))throw new Exception("Slash warning/active/expiry contact mismatch.");
            if(live&&!record.rendererMatchesFrame)throw new Exception("Live slash renderer does not match source frame.");
            if(index==3){pendingExpiredSlash=obj;pendingExpiredRecord=record;}
            gameCamera.orthographicSize=5;gameCamera.transform.position=new Vector3(playerPos.x,playerPos.y,-20);
            ScreenCapture.CaptureScreenshot(Path.Combine(enemyFidelityPath,name));
        }
    }
}
