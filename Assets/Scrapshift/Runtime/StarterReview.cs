using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool starterReview;string starterReviewPath;int starterStage;float starterNext;DateTime starterUtc;
        readonly List<string> starterCaptures=new List<string>();
        [Serializable] sealed class StarterResult { public string id; public float maxHealth,speed,range,dodge,attackSpeed,rangedDamage,engineering,harvesting;public string[] weapons; }
        [Serializable] sealed class StarterReport
        {
            public string status,scope="Staged character-selection/start checks with normal UI and run methods, frozen AI. Not a human survival run.";
            public int width,height,playable,locked;public bool fullViewport,randomSelectedPlayable,capturesVerified;
            public List<StarterResult> characters=new List<StarterResult>();
            public List<string> weaponsDamagedTarget=new List<string>();
            public int availableWeapons; public bool burning,slow,secondaryProjectiles,turretFired,ghostProgress;
            public bool gardenFruit,mineExplosion,burningSpread,engineeringSlow,thunderProjectiles,vorpalExecution;
        }
        StarterReport starterReport;
        void BeginStarterReview(string folder)
        {
            starterReview=true;starterReviewPath=Path.GetFullPath(folder);Directory.CreateDirectory(starterReviewPath);starterUtc=DateTime.UtcNow;
            starterNext=Time.realtimeSinceStartup+.7f;starterReport=new StarterReport{playable=catalog.data.characters.Length,locked=catalog.data.rosterSlots-catalog.data.characters.Length};
            Begin();ReturnToSelection(1);sound.SetMuted(true);
        }
        void StarterReviewTick()
        {
            if(!starterReview||Time.realtimeSinceStartup<starterNext)return;
            if(starterStage<catalog.data.characters.Length*2)
            {
                int index=starterStage/2;
                if(starterStage%2==0){ReturnToSelection(1);ChangeLook(index-playerAnimation.lookIndex);startingWeapon=0;randomCharacter=false;}
                else
                {
                    StartSelectedRun();arrivalAge=ArrivalMotion.Duration;PoseArrival(arrivalAge);if(parachute!=null)Destroy(parachute);
                    gameCamera.orthographicSize=ArenaGeometry.CameraHalfHeight;
                    starterReport.characters.Add(new StarterResult{id=SelectedCharacter.id,maxHealth=Rules.MaxHealth,speed=Rules.Stat("speed"),
                        range=Rules.Stat("range"),dodge=Rules.Stat("dodge"),attackSpeed=Rules.Stat("attack_speed"),rangedDamage=Rules.Stat("ranged_damage"),
                        engineering=Rules.Stat("engineering"),harvesting=Rules.Stat("harvesting"),weapons=Rules.Inventory.ConvertAll(w=>w.id).ToArray()});
                }
                string name=SelectedCharacter.id+(starterStage%2==0?"-selection":"-loadout");
                starterCaptures.Add(name);ScreenCapture.CaptureScreenshot(Path.Combine(starterReviewPath,name+".png"));
            }
            else if(starterStage==catalog.data.characters.Length*2)
            {
                ReturnToSelection(1);randomCharacter=true;ConfirmMenu();
                starterReport.randomSelectedPlayable=menuPage==2&&playerAnimation.lookIndex>=0&&playerAnimation.lookIndex<catalog.data.characters.Length;
                starterCaptures.Add("random-weapons");ScreenCapture.CaptureScreenshot(Path.Combine(starterReviewPath,"random-weapons.png"));
            }
            else if(starterStage==catalog.data.characters.Length*2+1)CheckStarterCombat();
            else
            {
                starterReport.width=Screen.width;starterReport.height=Screen.height;
                starterReport.fullViewport=gameCamera.rect==new Rect(0,0,1,1);
                starterReport.capturesVerified=CaptureVerification.Fresh(starterReviewPath,starterUtc,starterCaptures.ToArray());
                bool passed=starterReport.fullViewport&&starterReport.randomSelectedPlayable&&starterReport.capturesVerified&&starterReport.characters.Count==catalog.data.characters.Length
                    &&starterReport.weaponsDamagedTarget.Count==starterReport.availableWeapons&&starterReport.burning&&starterReport.slow
                    &&starterReport.secondaryProjectiles&&starterReport.turretFired&&starterReport.ghostProgress
                    &&starterReport.gardenFruit&&starterReport.mineExplosion&&starterReport.burningSpread&&starterReport.engineeringSlow&&starterReport.thunderProjectiles&&starterReport.vorpalExecution;
                starterReport.status=passed?"Passed":"Failed";
                File.WriteAllText(Path.Combine(starterReviewPath,"starter-review.json"),JsonUtility.ToJson(starterReport,true));
                Application.Quit(passed?0:1);enabled=false;
            }
            starterStage++;starterNext=Time.realtimeSinceStartup+.8f;
        }
        void CheckStarterCombat()
        {
            foreach(var weapon in catalog.data.equipment)
            {
                if(!weapon.available)continue;
                starterReport.availableWeapons++;Begin();ClearCombat();Rules.Inventory.Clear();Rules.Inventory.Add(weapon);SyncEquipment();
                var target=FeelEnemy(new Vector2(.6f,0));
                for(int i=0;i<180;i++){UpdateEquipment(1f/60);UpdateWeapons(1f/60);UpdateStarterEffects(1f/60);}
                if(target.hp<1000)starterReport.weaponsDamagedTarget.Add(weapon.id+"/"+weapon.tier);
            }
            Begin();ClearCombat();var victim=FeelEnemy(Vector2.right);var other=FeelEnemy(Vector2.right*2);
            ApplyStarterHit(victim,catalog.Weapon("wand",1),false,false,false);
            UpdateStarterEffects(1.01f);starterReport.burning=victim.burnTicks==2&&victim.hp<1000;
            ApplyStarterHit(victim,catalog.Weapon("taser",1),false,false,false);starterReport.slow=victim.contactSlow>0;
            int before=shots.Count;ApplyStarterHit(victim,catalog.Weapon("cactus_mace",1),false,false,false);
            starterReport.secondaryProjectiles=shots.Count==before+3;
            var ghost=catalog.Weapon("ghost_flint",1);float attack=Rules.Stat("attack_speed");
            for(int i=0;i<10;i++)ApplyStarterHit(victim,ghost,false,true,false);
            ClearCombat();victim=FeelEnemy(Vector2.right);
            for(int i=0;i<10;i++)ApplyStarterHit(victim,ghost,false,true,false);
            starterReport.ghostProgress=Rules.Stat("attack_speed")==attack+1;
            Rules.Inventory.Clear();Rules.Inventory.Add(catalog.Weapon("wrench",1));ResetStructures();
            ClearShotsForStarterReview();victim.pos=starterTurrets[0].pos+Vector2.right;
            UpdateStarterEffects(.01f);starterReport.turretFired=shots.Count>0;
            CheckRemainingWeaponBehaviors();
            ReturnToSelection(1);
        }
        void CheckRemainingWeaponBehaviors()
        {
            Begin();ClearCombat();Rules.Inventory.Clear();Rules.Inventory.Add(catalog.Weapon("pruner",1));ResetStructures();UpdateStarterEffects(.5f);starterReport.gardenFruit=pickups.Exists(p=>p.fruit);
            ClearCombat();Rules.Inventory.Clear();Rules.Inventory.Add(catalog.Weapon("screwdriver",1));ResetStructures();
            var mine=mines[0];var target=FeelEnemy(mine.pos+Vector2.up*.16f);UpdateStarterEffects(.01f);target.pos=mine.pos+Vector2.right*.9f;UpdateStarterEffects(.01f);
            starterReport.mineExplosion=mines.Count==0&&target.hp<1000;
            ClearCombat();Rules.Inventory.Clear();Rules.Inventory.Add(catalog.Weapon("torch",4));
            target=FeelEnemy(Vector2.zero);var nearby=FeelEnemy(Vector2.right*.5f);ApplyStarterHit(target,Rules.Inventory[0],false,false,false);UpdateStarterEffects(.01f);starterReport.burningSpread=nearby.burnTicks>0;
            target.speed=3;Rules.GainWeaponStat("engineering",20);Damage(enemies.IndexOf(target),1,-100,0,Vector2.right,catalog.Weapon("particle_accelerator",4));starterReport.engineeringSlow=target.contactSlow>0;
            int before=shots.Count;ApplyStarterHit(target,catalog.Weapon("thunder_sword",4),false,false,false);starterReport.thunderProjectiles=shots.Count==before+4&&shots[shots.Count-1].slow==2;
            var execution=new Equipment{traits=new[]{new WeaponTrait{key="one_shot_on_hit",value=100}}};Damage(enemies.IndexOf(target),1,-100,0,Vector2.right,execution);starterReport.vorpalExecution=!enemies.Contains(target);
        }
        void ClearShotsForStarterReview(){foreach(var shot in shots)Destroy(shot.obj);shots.Clear();}
    }
}
