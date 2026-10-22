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
        [Serializable] sealed class PetReviewReport {public string status,scope="Deterministic built-player companion behavior checks with stationary targets; not a human survival run.";public List<string> passed=new List<string>();public List<string> failed=new List<string>();public List<string> diagnostics=new List<string>();}
        PetReviewReport petReport;
        static readonly string[] petCases={"bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla","doc_moth","lootworm","scapegoat","jellyshield"};
        static readonly string[] supportSequenceKinds={"turret","tyler","pet_flame","flame"};
        static readonly int[] supportSequenceTicks={0,1,2,3,6};
        Shot[] supportSequenceShots;
        int supportSequenceTick;
        bool supportSequencePassed;
        readonly HashSet<Texture2D> supportSequenceFrames=new HashSet<Texture2D>();
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
                if(pose==0)
                {
                    var sample=SpriteObject("Reference projectile sample",SupportProjectileArt.Frame(turretKind,.1f),new Vector2(.7f,.055f),10);
                    SupportProjectileArt.Pose(sample.GetComponent<SpriteRenderer>(),turretKind,.1f);
                    Destroy(sample,1);
                }
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
            if(petReviewStage>=petCases.Length+28&&petReviewStage<petCases.Length+34)
            {
                string kind=new[]{"turret","flame","rocket","laser","healing","tyler"}[petReviewStage++-petCases.Length-28];
                CaptureLiveStructureProjectile(kind);petReviewNext=Time.realtimeSinceStartup+.8f;return;
            }
            if(petReviewStage>=petCases.Length+34&&petReviewStage<petCases.Length+54)
            {
                int sequence=petReviewStage++-petCases.Length-34;
                CaptureSupportProjectileSequence(supportSequenceKinds[sequence/5],sequence%5);
                petReviewNext=Time.realtimeSinceStartup+.25f;return;
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
                    Drop(Vector2.zero,1,false);
                    // Source items become monitorable after pushback. Advance
                    // that preparation without letting the player take the drop.
                    var material=pickups.Last();
                    for(int step=0;step<120&&!material.motion.CanCollect;step++)material.motion.Step(1f/60,Vector2.zero,-1,true);
                    int money=Rules.Scrap;TickPets(.01f);bool collected=Rules.Scrap>money&&pickups.Count==0;
                    Spawn(catalog.Enemy("tree"),Vector2.zero);var tree=enemies.Last();Destroy(tree.marker);tree.marker=null;tree.obj.SetActive(true);float hp=tree.hp;pet.clock=0;TickPets(.01f);okay=collected&&tree.hp<hp;
                }
                else if(id=="scapegoat")
                {
                    for(int i=0;i<40;i++){pet.invulnerable=0;DamageScapegoat(pet);}bool dead=pet.dead;TickPets(2.1f);TickPets(.01f);okay=dead&&!pet.dead&&pet.health==pet.spec.health;
                }
                else if(id=="jellyshield"){TickPets(.01f);okay=!PetBlocksProjectile(pet.pos-Vector2.right,pet.pos+Vector2.right)&&PetBlocksProjectile(pet.pos-Vector2.right,pet.pos);}
                else
                {
                    var target=FeelEnemy(Vector2.right*.4f);pet.clock=0;pet.secondaryClock=0;
                    for(int i=0;i<480;i++){TickPets(1f/60);UpdateWeapons(1f/60);}
                    okay=target.hp<1000;
                    if(id=="bot_o_mine")okay&=mines.Count>0;
                    if(id=="blazemander")okay&=target.burnTicks>0;
                    if(id=="bonk_dog"){float hp=target.hp;pet.pos=target.pos;PetExplosion(pet);TickReferenceExplosions(.01f);okay&=target.hp<hp;}
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
            foreach(var p in pets){p.pos=Vector2.zero;p.obj.transform.position=p.pos;p.destination=Vector2.right*3;p.obj.GetComponent<GroundShadow>().Place(p.pos,p.baseScale);}
            return pets[0];
        }
        void PetCheck(string name,bool okay)
        {(okay?petReport.passed:petReport.failed).Add(name);Debug.Log("Support behavior fixture "+name+": "+okay);}
        void CaptureLiveStructureProjectile(string kind)
        {
            // These captures exercise the production emitter and projectile
            // update. The earlier aim samples only demonstrate static artwork.
            Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();playerPos=Vector2.right*2;
            player.SetActive(false);SyncEquipment();
            var spec=catalog.data.items.SelectMany(i=>i.conditionalEffects).Where(e=>e.structure!=null).Select(e=>e.structure).First(s=>s.kind==kind);
            DeployStructure(spec,StructureImage(kind),Vector2.zero);var turret=starterTurrets.Last();
            var target=kind=="healing"?null:FeelEnemy(Vector2.right*2);
            turret.firing=true;turret.clock=0;TickStructure(turret,.001f);
            var emitted=shots.Where(s=>s.structureSpec==spec).ToArray();
            PetCheck("production projectile profile: "+kind,emitted.Length>0&&emitted.All(s=>s.projectileRef!=null&&s.obj.GetComponent<SpriteRenderer>().sprite!=null));
            if(target!=null){target.hp=0;target.obj.SetActive(false);enemies.Remove(target);}
            playerPos=Vector2.one*8;UpdateWeapons(.04f);
            PetCheck("production projectile survives initial frame: "+kind,emitted.Length>0&&emitted.All(s=>shots.Contains(s)));
            gameCamera.orthographicSize=2;gameCamera.transform.position=new Vector3(.7f,0,-20);
            ScreenCapture.CaptureScreenshot(Path.Combine(petReviewPath,kind+"-live-projectile.png"));
        }
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
                float firstLeft=p.clock,firstRight=p.rightClock;bool leftHandoff=firstLeft>firstRight&&firstRight>0;
                string firstShots=string.Join(";",shots.Select(s=>"origin="+s.pos+",velocity="+s.velocity));
                TickPets(firstRight+.001f);
                petReport.diagnostics.Add("Cat timer handoff: targets="+a.pos+","+b.pos+"; first clocks="+firstLeft+","+firstRight+"; second clocks="+p.clock+","+p.rightClock+"; shots="+fired+"->"+shots.Count+"; first="+firstShots+"; all="+string.Join(";",shots.Select(s=>"origin="+s.pos+",velocity="+s.velocity)));
                // Each source gun independently chooses its nearest target;
                // both may validly aim at the same enemy. Verify its alternating
                // cooldown handoff instead of requiring opposite directions.
                PetCheck("cat alternate independent guns",fired>0&&shots.Count>fired&&leftHandoff&&p.rightClock>p.clock&&p.clock>0);
                p=SetUpPetCase("doc_moth",2);playerPos=Vector2.zero;TickPets(.001f);PetCheck("overlapping moth zones stack regeneration",Rules.HealingDrones==2&&Rules.RegenerationHealMultiplier==3);
                p=SetUpPetCase("lootworm");Drop(Vector2.left,1,false);Spawn(catalog.Enemy("tree"),Vector2.right*3);var tree=enemies.Last();Destroy(tree.marker);tree.marker=null;
                TickPets(.02f);PetCheck("lootworm prioritizes tree over material",p.pos.x>0);
                p=SetUpPetCase("scapegoat");p.maxHealth=80;p.health=0;p.dead=true;playerPos=Vector2.zero;
                TickPets(p.spec.revive/2);float partial=p.health;playerPos=Vector2.one*8;TickPets(.5f);
                bool held=p.health==partial&&p.dead;playerPos=p.pos;TickPets(p.spec.revive/2+.01f);
                float filled=p.health;bool pendingRise=p.dead;TickPets(1f/60);
                petReport.diagnostics.Add("Decoy revival: partial="+partial+"; held="+held+"; filled="+filled+"; pending next-tick rise="+pendingRise+"; final health="+p.health+"; max="+p.maxHealth+"; dead="+p.dead);
                PetCheck("decoy revival retains progress and grown max HP",held&&filled==80&&pendingRise&&!p.dead&&p.health==80);
                Begin();ClearCombat();Rules.Items.Clear();Rules.Inventory.Clear();starterReport=new StarterReport();CheckDeployableImplants();
                foreach(var id in new[]{"turret","turret_flame","turret_healing","turret_laser","turret_rocket","tyler","garden","landmines","wandering_bot","pocket_factory"})PetCheck("structure: "+id,starterReport.deployableImplantsPassed.Contains(id));
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();Rules.Items.Add(catalog.data.items.Single(i=>i.id=="landmines"));ResetStructures();
                var mine=mines[0];playerPos=mine.pos;UpdateStarterEffects(.01f);bool pressed=mine.playerOn&&mines.Contains(mine);
                playerPos=mine.pos+Vector2.right*2;UpdateStarterEffects(.01f);PetCheck("player presses mine; leaving detonates",pressed&&!mines.Contains(mine));
            }
            catch(Exception e){petReport.failed.Add("extended fixture exception: "+e.Message);Debug.LogException(e);}
        }
        void CaptureSupportProjectileSequence(string kind,int sample)
        {
            try
            {
                if(sample==0)
                {
                    supportSequenceTick=0;supportSequencePassed=true;supportSequenceFrames.Clear();supportSequenceShots=Array.Empty<Shot>();
                    if(kind=="pet_flame")
                    {
                        var pet=SetUpPetCase("blazemander");pet.clock=999;pet.secondaryClock=0;
                        FeelEnemy(pet.pos+Vector2.right*.4f);TickPets(.001f);
                        // This production branch emits six shots immediately,
                        // then sets rangedPulse; its reset is on the next tick.
                        if(shots.Count==0||!pet.rangedPulse)throw new InvalidOperationException("Blazemander ranged pulse did not emit its initial production shots.");
                    }
                    else
                    {
                        Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();SyncEquipment();playerPos=Vector2.one*8;
                        var spec=catalog.data.items.SelectMany(i=>i.conditionalEffects).Where(e=>e.structure!=null).Select(e=>e.structure).First(s=>s.kind==kind);
                        DeployStructure(spec,StructureImage(kind),Vector2.zero);
                        FeelEnemy(Vector2.right*2);
                        var turret=starterTurrets.Last();turret.clock=0;
                        // Advance the actual windup state, including Tyler's
                        // eligible target, without aging newly emitted shots.
                        int preparation=0;
                        while(shots.Count==0&&preparation++<120)TickStructure(turret,1f/60);
                        petReport.diagnostics.Add("Projectile sequence emitter "+kind+": preparation ticks="+preparation+", shots="+shots.Count+", firing="+turret.firing+", clock="+turret.clock);
                        if(shots.Count==0)throw new InvalidOperationException("Structure windup did not emit production shots: "+kind);
                    }
                    supportSequenceShots=shots.ToArray();supportSequencePassed=supportSequenceShots.Length>0;
                    foreach(var enemy in enemies.ToArray()){enemy.obj.SetActive(false);enemies.Remove(enemy);}
                    player.SetActive(false);playerPos=Vector2.one*8;
                    gameCamera.orthographicSize=3.2f;gameCamera.transform.position=new Vector3(.3f,0,-20);
                }
                while(supportSequenceTick<supportSequenceTicks[sample]){UpdateWeapons(1f/60);supportSequenceTick++;}
                string expected=SupportSequenceResource(kind,sample),file="sequence-"+kind+"-"+supportSequenceTicks[sample].ToString("00")+".png";
                var texture=Resources.Load<Texture2D>(expected);
                foreach(var shot in supportSequenceShots)
                {
                    var renderer=shot.obj.GetComponent<SpriteRenderer>();
                    supportSequencePassed&=shots.Contains(shot)&&!float.IsNaN(shot.pos.x)&&!float.IsNaN(shot.pos.y)&&!float.IsInfinity(shot.pos.x)&&!float.IsInfinity(shot.pos.y)&&renderer.sprite.texture==texture;
                    supportSequenceFrames.Add(renderer.sprite.texture);
                    petReport.diagnostics.Add("Projectile sequence "+kind+": capture="+file+", tick="+supportSequenceTick+", age="+shot.age+", position="+shot.pos+", texture="+renderer.sprite.texture.name+", expected="+expected);
                }
                ScreenCapture.CaptureScreenshot(Path.Combine(petReviewPath,file));
                if(sample==4)PetCheck("production projectile sequence: "+kind,supportSequencePassed&&supportSequenceFrames.Count>1);
            }
            catch(Exception e){petReport.failed.Add("projectile sequence "+kind+": "+e.Message);Debug.LogException(e);}
        }
        static string SupportSequenceResource(string kind,int sample)
        {
            if(kind=="turret")return "WeaponReference/"+(sample>=2?"frame0001_a728a067":"frame0000_f29290ae");
            if(kind=="tyler")return "WeaponReference/bullet_lightning_"+(sample==2?"0001_4e388e95":sample==3?"0002_87ab5590":"0000_60d674df");
            int frame=sample==4?2:sample==3?1:0;
            if(kind=="flame")return "SupportProjectiles/flame"+frame;
            return "WeaponReference/bullet_flame_"+(frame==2?"0002_c4473734":frame==1?"0001_eb86f4c2":"0000_180a0687");
        }
    }
}

