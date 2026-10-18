using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [Serializable] sealed class NativeContactReport
        {
            public string status,scope="Targeted built-player production UpdateWeapons checks at 60 Hz with actual source-derived profiles and 1000-HP Baby Alien targets; not a campaign or proof of engine callback ordering.";
            public List<string> passed=new List<string>(),failed=new List<string>();
        }
        void CheckNativeContactTiming(string output)
        {
            var report=new NativeContactReport();var random=UnityEngine.Random.state;
            Action<string,bool> check=(name,okay)=>
            {
                (okay?report.passed:report.failed).Add(name);
                (okay?weaponReviewPassed:weaponReviewFailed).Add("native-contact: "+name);
            };
            Action reset=()=>
            {
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();SyncEquipment();
                arrivalAge=ArrivalMotion.Duration;playerPos=Vector2.one*8;Rules.GainWeaponStat("crit_chance",-10000);
            };
            Func<Vector2,Enemy> target=center=>FeelEnemy(center-CombatGeometry.HurtOffset("baby_alien"));
            Func<Equipment,ProjectileReference,Vector2,Vector2,Shot> shot=(weapon,profile,position,velocity)=>
            {
                var s=new Shot{obj=SpriteObject("Native contact probe",catalog.muzzleFrames[2],position,13),weapon=weapon,projectileRef=profile,
                    pos=position,velocity=velocity,life=1,damage=3,crit=-10000,pierce=99,burnPayload=new BurnPayload()};
                shots.Add(s);return s;
            };
            try
            {
                foreach(bool graze in new[]{false,true})
                {
                    reset();var enemy=target(new Vector2(1.74f,graze?-1.74f:-1.04f));
                    var weapon=catalog.Weapon("flamethrower",2);shot(weapon,WeaponReference.Projectile(weapon),new Vector2(0,-1),Vector2.right*60);
                    for(int i=0;i<3;i++)UpdateWeapons(1f/60);
                    check(graze?"source flame cap grazing misses":"source flame cap center hits",graze?enemy.hp==1000:enemy.hp<1000);
                }
                for(int frames=0;frames<=2;frames++)
                {
                    reset();var profile=SupportProjectileBehavior.Structure("tyler");
                    var enemy=target(new Vector2(profile.hitOffset[0],profile.hitOffset[1]));
                    var s=shot(catalog.Weapon("pistol",1),profile,Vector2.zero,Vector2.zero);s.hitDelay=frames/60f;s.delayedHitboxPending=true;
                    bool initial=float.IsPositiveInfinity(ReferenceShotHitFraction(s,enemy,Vector2.zero,0));
                    UpdateWeapons(1f/60);bool first=enemy.hp<1000;
                    UpdateWeapons(1f/60);bool second=enemy.hp<1000;
                    check("source delayed "+frames+" frames",initial&&first==(frames<=1)&&second);
                }
                reset();var beamWeapon=catalog.Weapon("particle_accelerator",3);
                var beamTarget=target(new Vector2(2,CombatGeometry.Enemy("baby_alien").hurt+.12f));
                var beam=shot(beamWeapon,WeaponReference.Projectile(beamWeapon),Vector2.zero,Vector2.zero);beam.life=.2f;
                UpdateWeapons(1f/60);bool narrowMiss=beamTarget.hp==1000;
                for(int i=0;i<5;i++)UpdateWeapons(1f/60);
                bool grownHit=beamTarget.hp<1000;float hp=beamTarget.hp;UpdateWeapons(1f/60);
                check("current beam grows and hits once",narrowMiss&&grownHit&&beamTarget.hp==hp);
            }
            catch(Exception e){check("fixture exception: "+e.Message,false);Debug.LogException(e);}
            finally{UnityEngine.Random.state=random;}
            report.status=report.failed.Count==0?"Passed":"Failed";
            File.WriteAllText(Path.Combine(output,"native-contact.json"),JsonUtility.ToJson(report,true));
        }
    }
}
