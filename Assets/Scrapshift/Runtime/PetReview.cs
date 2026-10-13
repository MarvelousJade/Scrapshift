using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool petReview;string petReviewPath;float petReviewNext;int petReviewStage;
        [Serializable] sealed class PetReviewReport {public string status,scope="Deterministic built-player companion behavior checks with stationary targets; not a human survival run.";public List<string> passed=new List<string>();public List<string> failed=new List<string>();}
        PetReviewReport petReport;
        static readonly string[] petCases={"bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla","doc_moth","lootworm","scapegoat","jellyshield"};
        void BeginPetReview(string path){petReview=true;petReviewPath=Path.GetFullPath(path);Directory.CreateDirectory(petReviewPath);petReport=new PetReviewReport();petReviewNext=Time.realtimeSinceStartup+.5f;sound.SetMuted(true);}
        void PetReviewTick()
        {
            if(!petReview||Time.realtimeSinceStartup<petReviewNext)return;
            if(petReviewStage>=petCases.Length){petReport.status=petReport.failed.Count==0?"Passed":"Failed";File.WriteAllText(Path.Combine(petReviewPath,"pets.json"),JsonUtility.ToJson(petReport,true));petReview=false;Application.Quit(petReport.failed.Count==0?0:1);return;}
            string id=petCases[petReviewStage++];bool okay=false;Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();playerPos=Vector2.zero;
            var item=catalog.data.items.Single(i=>i.id==id);Rules.Items.Add(item);foreach(var effect in item.effects)Rules.GainWeaponStat(effect.key.Replace("stat_",""),effect.value);ResetPets();
            try
            {
                var pet=pets.Single();pet.pos=Vector2.zero;pet.destination=Vector2.right*.5f;
                if(id=="doc_moth"){TickPets(.01f);okay=Rules.HealingDrones==1&&Rules.RegenerationHealMultiplier==2;}
                else if(id=="lootworm")
                {
                    Drop(Vector2.zero,1,false);int money=Rules.Scrap;TickPets(.01f);bool collected=Rules.Scrap>money&&pickups.Count==0;
                    Spawn(catalog.Enemy("tree"),Vector2.zero);var tree=enemies.Last();Destroy(tree.marker);tree.marker=null;tree.obj.SetActive(true);float hp=tree.hp;pet.clock=0;TickPets(.01f);okay=collected&&tree.hp<hp;
                }
                else if(id=="scapegoat")
                {
                    for(int i=0;i<40;i++){pet.invulnerable=0;DamageScapegoat(pet);}bool dead=pet.dead;TickPets(2.1f);okay=dead&&!pet.dead&&pet.health==pet.spec.health;
                }
                else if(id=="jellyshield"){TickPets(.01f);okay=PetBlocksProjectile(pet.pos-Vector2.right,pet.pos+Vector2.right);}
                else
                {
                    var target=FeelEnemy(Vector2.right*.4f);pet.clock=0;pet.secondaryClock=0;
                    for(int i=0;i<480;i++){TickPets(1f/60);UpdateWeapons(1f/60);}
                    okay=target.hp<1000;
                    if(id=="bot_o_mine")okay&=mines.Count>0;
                    if(id=="blazemander")okay&=target.burnTicks>0;
                    if(id=="bonk_dog"){float hp=target.hp;pet.pos=target.pos;PetExplosion(pet);okay&=target.hp<hp;}
                }
            }
            catch(Exception e){Debug.LogException(e);}
            (okay?petReport.passed:petReport.failed).Add(id);Debug.Log("Companion fixture "+id+": "+okay);ScreenCapture.CaptureScreenshot(Path.Combine(petReviewPath,id+".png"));petReviewNext=Time.realtimeSinceStartup+.8f;
        }
    }
}
