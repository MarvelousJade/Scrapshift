using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        string supportReviewPath;int supportStage;float supportNext;bool musicSilent,musicRunning,musicContinuous;Vector3 supportFirstScale;bool supportChanged,aimChanged,limbsChanged;Quaternion firstLimb;Vector3 basePosition;
        bool supportPassed;
        [Serializable] sealed class SupportChecks {public string status;public bool musicSilentBeforeStart,musicPlayingAfterStart,repeatStartPreservesPlayback,turretPoseChanges,shadowStaysAtGround,uprightBeforeCooldownCompletes,connectedBodyMoves;}
        void BeginSupportReview(string path){supportReviewPath=Path.GetFullPath(path);Directory.CreateDirectory(supportReviewPath);supportNext=Time.realtimeSinceStartup+.5f;sound.SetMuted(true);}
        void SupportReviewTick()
        {
            if(supportReviewPath==null||Time.realtimeSinceStartup<supportNext)return;
            if(supportStage>=12)
            {
                if(supportStage>=44){Application.Quit(supportPassed?0:1);return;}
                menuPoseTime=(supportStage-12)*.2f;
                ScreenCapture.CaptureScreenshot(Path.Combine(supportReviewPath,$"menu-{supportStage-12:00}.png"));
                supportStage++;supportNext=Time.realtimeSinceStartup+.15f;return;
            }
            var audio=music.GetComponents<AudioSource>().First(a=>a.clip!=null&&a.clip.name=="NeonSalvage");
            if(supportStage==0)
            {musicSilent=!audio.isPlaying;StartSelectedRun();supportNext=Time.realtimeSinceStartup+.6f;supportStage++;return;}
            if(supportStage==1)
            {
                musicRunning=audio.isPlaying;int sample=audio.timeSamples;music.StartOnce();musicContinuous=audio.timeSamples>=sample;
                ClearCombat();Rules.Inventory.Clear();SyncEquipment();arrivalAge=2;player.SetActive(false);if(parachute!=null)Destroy(parachute);reducedMotion=false;
                var item=catalog.data.items.Single(i=>i.id=="turret");var spec=item.conditionalEffects.First(e=>e.structure!=null).structure;
                DeployStructure(spec,StructureImage(spec.kind),new Vector2(-2,0));starterTurrets[0].firing=true;starterTurrets[0].animationAge=0;starterTurrets[0].clock=10;
                foreach(var id in new[]{"bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla","doc_moth","lootworm","scapegoat"})
                {var data=catalog.data.items.Single(i=>i.id==id);SpawnPet(data.conditionalEffects.First(e=>e.pet!=null).pet);}
                playerPos=new Vector2(7,0);for(int i=0;i<pets.Count;i++){pets[i].pos=new Vector2(-5+i%4*3,3-i/4*5);pets[i].destination=playerPos;}
                gameCamera.orthographicSize=5.5f;gameCamera.transform.position=Vector3.back*20;
                supportFirstScale=starterTurrets[0].rig.AnimatedAssembly.localScale;basePosition=starterTurrets[0].obj.transform.position;
                firstLimb=pets[0].rig.Body.localRotation;
                Spawn(catalog.Enemy("baby_alien"),new Vector2(-1,3));var enemy=enemies[enemies.Count-1];Destroy(enemy.marker);enemy.marker=null;enemy.obj.SetActive(true);
            }
            if(supportStage>=2&&supportStage<=10)
            {
                TickStructure(starterTurrets[0],.0333333f);TickPets(.0333333f);
                supportChanged|=(starterTurrets[0].rig.AnimatedAssembly.localScale-supportFirstScale).sqrMagnitude>.00001f;
                aimChanged|=Mathf.Abs(Mathf.DeltaAngle(0,starterTurrets[0].rig.AimingHead.eulerAngles.z))<.01f&&starterTurrets[0].clock>0;
                limbsChanged|=Quaternion.Angle(firstLimb,pets[0].rig.Body.localRotation)>1;
            }
            if(supportStage<=10)ScreenCapture.CaptureScreenshot(Path.Combine(supportReviewPath,$"frame-{supportStage:00}.png"));
            else
            {
                bool shadow=Vector2.Distance(starterTurrets[0].obj.GetComponent<GroundShadow>().View.position,starterTurrets[0].pos)<.3f;
                shadow&=Vector3.Distance(basePosition,starterTurrets[0].obj.transform.position)<.001f;
                bool okay=musicSilent&&musicRunning&&musicContinuous&&supportChanged&&shadow&&aimChanged&&limbsChanged;
                File.WriteAllText(Path.Combine(supportReviewPath,"checks.json"),JsonUtility.ToJson(new SupportChecks{status=okay?"Passed":"Failed",musicSilentBeforeStart=musicSilent,musicPlayingAfterStart=musicRunning,repeatStartPreservesPlayback=musicContinuous,turretPoseChanges=supportChanged,shadowStaysAtGround=shadow,uprightBeforeCooldownCompletes=aimChanged,connectedBodyMoves=limbsChanged},true));
                supportPassed=okay;ReturnToSelection(0);reducedMotion=false;menuPoseTime=0;
            }
            supportStage++;supportNext=Time.realtimeSinceStartup+.15f;
        }
    }
}
