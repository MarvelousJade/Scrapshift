using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        string supportReviewPath;int supportStage;float supportNext;bool musicSilent,musicRunning,musicContinuous;Vector3 supportFirstScale;bool supportChanged;
        void BeginSupportReview(string path){supportReviewPath=Path.GetFullPath(path);Directory.CreateDirectory(supportReviewPath);supportNext=Time.realtimeSinceStartup+.5f;sound.SetMuted(true);}
        void SupportReviewTick()
        {
            if(supportReviewPath==null||Time.realtimeSinceStartup<supportNext)return;
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
                supportFirstScale=starterTurrets[0].obj.transform.localScale;
            }
            if(supportStage>=2&&supportStage<=10)
            {
                TickStructure(starterTurrets[0],.0333333f);TickPets(.0333333f);
                supportChanged|=(starterTurrets[0].obj.transform.localScale-supportFirstScale).sqrMagnitude>.00001f;
            }
            if(supportStage<=10)ScreenCapture.CaptureScreenshot(Path.Combine(supportReviewPath,$"frame-{supportStage:00}.png"));
            else
            {
                bool shadow=Vector2.Distance(starterTurrets[0].obj.GetComponent<GroundShadow>().View.position,starterTurrets[0].pos)<.3f;
                bool okay=musicSilent&&musicRunning&&musicContinuous&&supportChanged&&shadow;
                File.WriteAllText(Path.Combine(supportReviewPath,"checks.json"),"{\n  \"status\":\""+(okay?"Passed":"Failed")+"\",\n  \"musicSilentBeforeStart\":"+musicSilent.ToString().ToLower()+",\n  \"musicPlayingAfterStart\":"+musicRunning.ToString().ToLower()+",\n  \"repeatStartPreservesPlayback\":"+musicContinuous.ToString().ToLower()+",\n  \"turretPoseChanges\":"+supportChanged.ToString().ToLower()+",\n  \"shadowStaysAtGround\":"+shadow.ToString().ToLower()+"\n}");Application.Quit(okay?0:1);
            }
            supportStage++;supportNext=Time.realtimeSinceStartup+.15f;
        }
    }
}
