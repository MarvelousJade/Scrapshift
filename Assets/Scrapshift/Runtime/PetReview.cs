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
            if(petReviewStage>=petCases.Length&&petReviewStage<petCases.Length+6)
            {
                int pose=petReviewStage++-petCases.Length;var p=SetUpPetCase("catling_gun");
                player.SetActive(false);SyncEquipment();p.obj.transform.position=Vector3.zero;
                p.rig.PetPose(pose>=4?pose==4?.25f:.75f:.5f,true,pose>=2&&pose<4,false,false);
                var dir=new[]{Vector2.right,Vector2.up,Vector2.left,Vector2.down}[pose>=4?0:pose];
                p.rig.AimGun(0,dir);p.rig.AimGun(1,dir);p.rig.TickGunRecoil(1,false);
                gameCamera.orthographicSize=1.2f;gameCamera.transform.position=new Vector3(0,0,-20);
                ScreenCapture.CaptureScreenshot(Path.Combine(petReviewPath,"cat-aim-"+pose+".png"));
                petReviewNext=Time.realtimeSinceStartup+.8f;return;
            }
            if(petReviewStage>=petCases.Length+6&&petReviewStage<petCases.Length+26)
            {
                int capture=petReviewStage++-petCases.Length-6;int pose=capture%4;string turretKind=new[]{"turret","flame","rocket","laser","healing"}[capture/4];Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();
                player.SetActive(false);SyncEquipment();
                var turretItem=catalog.data.items.Single(i=>i.conditionalEffects.Any(e=>e.customKey=="structure"&&e.structure!=null&&e.structure.kind==turretKind));var spec=turretItem.conditionalEffects.First(e=>e.customKey=="structure"&&e.structure!=null&&e.structure.kind==turretKind).structure;
                DeployStructure(spec,StructureImage(spec.kind),Vector2.zero);
                starterTurrets.Last().rig.Aim(new[]{Vector2.right,Vector2.up,Vector2.left,Vector2.down}[pose],.1f,.3f,false);
                gameCamera.orthographicSize=1.2f;gameCamera.transform.position=new Vector3(0,0,-20);
                ScreenCapture.CaptureScreenshot(Path.Combine(petReviewPath,turretKind+"-aim-"+pose+".png"));
                petReviewNext=Time.realtimeSinceStartup+.8f;return;
            }
            if(petReviewStage>=petCases.Length+26&&petReviewStage<petCases.Length+28)
            {
                int pose=petReviewStage++-petCases.Length-26;var cat=SetUpPetCase("catling_gun");playerPos=cat.pos;TickPets(.01f);if(pose==1)TickPets(.25f);
                player.SetActive(false);SyncEquipment();gameCamera.orthographicSize=1.2f;gameCamera.transform.position=new Vector3(cat.pos.x,cat.pos.y,-20);
                ScreenCapture.CaptureScreenshot(Path.Combine(petReviewPath,"cat-boost-"+pose+".png"));petReviewNext=Time.realtimeSinceStartup+.8f;return;
            }
            if(petReviewStage>=petCases.Length){CheckMultipleSupports();petReport.status=petReport.failed.Count==0?"Passed":"Failed";File.WriteAllText(Path.Combine(petReviewPath,"pets.json"),JsonUtility.ToJson(petReport,true));petReview=false;Application.Quit(petReport.failed.Count==0?0:1);return;}
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
            (okay?petReport.passed:petReport.failed).Add(id);Debug.Log("Companion fixture "+id+": "+okay);
            ClearImpacts();floaters.Clear();foreach(var spark in sparks)Destroy(spark.obj);sparks.Clear();
            foreach(var enemy in enemies)enemy.obj.SetActive(false);foreach(var shot in shots)shot.obj.SetActive(false);
            player.SetActive(false);SyncEquipment();gameCamera.orthographicSize=2.5f;gameCamera.transform.position=new Vector3(pets[0].pos.x,pets[0].pos.y,-20);
            ScreenCapture.CaptureScreenshot(Path.Combine(petReviewPath,id+".png"));petReviewNext=Time.realtimeSinceStartup+.8f;
        }
        Pet SetUpPetCase(string kind,int count=1)
        {
            Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();playerPos=Vector2.one*8;
            var item=catalog.data.items.Single(i=>i.id==kind);for(int i=0;i<count;i++)Rules.Items.Add(item);ResetPets();
            foreach(var p in pets){p.pos=Vector2.zero;p.obj.transform.position=p.pos;p.destination=Vector2.right*3;}
            return pets[0];
        }
        void PetCheck(string name,bool okay)
        {(okay?petReport.passed:petReport.failed).Add(name);Debug.Log("Support behavior fixture "+name+": "+okay);}
        void CheckMultipleSupports()
        {
            try
            {
                var p=SetUpPetCase("ratzilla",2);var a=FeelEnemy(Vector2.right);var b=FeelEnemy(Vector2.right*2);
                SelectPetTarget(p);SelectPetTarget(pets[1]);PetCheck("same-kind target reservations",p.target!=pets[1].target&&p.target!=null&&pets[1].target!=null);
                p=SetUpPetCase("catling_gun",2);TickPets(.1f);PetCheck("cats wander and do not boost each other",!pets.Any(x=>x.boosted)&&p.pos.x>0);
                playerPos=p.pos;Vector2 at=p.pos;TickPets(.01f);PetCheck("cat proximity stops movement and boosts fire",p.boosted&&p.pos==at);
                ClearPets();p=SetUpPetCase("catling_gun");p.clock=p.rightClock=0;playerPos=p.pos;
                // Place targets around the actual back mounts, not the old belly-mounted origin.
                Vector2 midpoint=(p.rig.GunMount(0)+p.rig.GunMount(1))*.5f;
                a=FeelEnemy(midpoint+Vector2.left);b=FeelEnemy(midpoint+Vector2.right);TickPets(.01f);int fired=shots.Count;
                TickPets(p.spec.stats.interval/4+.001f);PetCheck("cat alternate independent guns",fired>0&&shots.Count>fired&&shots.Any(x=>x.velocity.x<0)&&shots.Any(x=>x.velocity.x>0));
                p=SetUpPetCase("doc_moth",2);playerPos=Vector2.zero;TickPets(.001f);PetCheck("overlapping moth zones stack regeneration",Rules.HealingDrones==2&&Rules.RegenerationHealMultiplier==3);
                p=SetUpPetCase("lootworm");Drop(Vector2.left,1,false);Spawn(catalog.Enemy("tree"),Vector2.right*3);var tree=enemies.Last();Destroy(tree.marker);tree.marker=null;
                TickPets(.02f);PetCheck("lootworm prioritizes tree over material",p.pos.x>0);
                p=SetUpPetCase("scapegoat");p.maxHealth=80;p.health=0;p.dead=true;playerPos=Vector2.zero;
                TickPets(p.spec.revive/2);float partial=p.health;playerPos=Vector2.one*8;TickPets(.5f);
                bool held=p.health==partial&&p.dead;playerPos=p.pos;TickPets(p.spec.revive/2+.01f);PetCheck("decoy revival retains progress and grown max HP",held&&!p.dead&&p.health==80);
                Begin();ClearCombat();Rules.Items.Clear();Rules.Inventory.Clear();starterReport=new StarterReport();CheckDeployableImplants();
                foreach(var id in new[]{"turret","turret_flame","turret_healing","turret_laser","turret_rocket","tyler","garden","landmines","wandering_bot","pocket_factory"})PetCheck("structure: "+id,starterReport.deployableImplantsPassed.Contains(id));
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();Rules.Items.Add(catalog.data.items.Single(i=>i.id=="landmines"));ResetStructures();
                var mine=mines[0];playerPos=mine.pos;UpdateStarterEffects(.01f);bool pressed=mine.playerOn&&mines.Contains(mine);
                playerPos=mine.pos+Vector2.right*2;UpdateStarterEffects(.01f);PetCheck("player presses mine; leaving detonates",pressed&&!mines.Contains(mine));
            }
            catch(Exception e){petReport.failed.Add("extended fixture exception: "+e.Message);Debug.LogException(e);}
        }
    }
}
