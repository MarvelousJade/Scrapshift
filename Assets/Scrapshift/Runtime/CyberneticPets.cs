using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class Pet
        {
            public PetSpec spec;public GameObject obj;public Vector2 pos,destination,jumpStart;public float clock,secondaryClock,age,health,invulnerable,jump,baseScale,posePhase,attackPose;
            public Equipment weapon,secondary;public bool dead,boosted;public SupportRig rig;
            public Enemy target;public float rightClock,patrolAngle,maxHealth,meleeDelay;public bool pendingMelee;
            public AudioSource reviveSound;public LineRenderer zoneView,healthView,healthBack;public bool inHealingZone;
        }
        readonly List<Pet> pets=new List<Pet>();
        void ClearPets(){foreach(var pet in pets)Destroy(pet.obj);pets.Clear();Rules.HealingDrones=0;Rules.PetCount=0;}
        void ResetPets()
        {
            ClearPets();
            foreach(var effect in Rules.Effects("pet"))if(effect.pet!=null&&!string.IsNullOrEmpty(effect.pet.kind))for(int i=0;i<(int)effect.value;i++)SpawnPet(effect.pet);
            int shields=(int)Rules.Stat("jellyshield_count");for(int i=0;i<shields;i++)SpawnPet(new PetSpec{kind="jellyshield",health=1,zone=1});
            Rules.PetCount=pets.Count;
        }
        void SpawnPet(PetSpec spec)
        {
            var image=SupportArt.Pet(spec.kind);float span=Mathf.Min(6,4+Rules.OwnedStructures*.1f);var pos=RunRules.ClampPosition(structureGroupCenter+new Vector2(UnityEngine.Random.Range(-span/2,span/2),UnityEngine.Random.Range(-span/2,span/2)));
            var obj=SpriteObject("Cybernetic companion: "+spec.kind,image,pos,7);obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(image,.65f);AddGroundShadow(obj,image.bounds.size.x,-.2f);
            var pet=new Pet{spec=spec,obj=obj,baseScale=obj.transform.localScale.x,pos=pos,destination=pos,health=spec.health+spec.healthGrowth*(Rules.Wave-1)};
            pet.rig=SupportRig.Create(obj,spec.kind,false);
            if(spec.kind=="catling_gun")pet.rig.ConfigureCatGuns(SupportRig.GatlingGun());
            if(spec.kind=="doc_moth"){pet.zoneView=MakeLine("Healing zone",obj.transform,new Color(.25f,1,.4f,.2f),.035f,true);pet.zoneView.useWorldSpace=true;pet.zoneView.positionCount=64;pet.zoneView.sortingOrder=2;}
            if(spec.kind=="scapegoat")
            {
                pet.reviveSound=obj.AddComponent<AudioSource>();pet.reviveSound.playOnAwake=false;pet.reviveSound.loop=true;pet.reviveSound.clip=Resources.Load<AudioClip>("OriginalSupportAudio/scapegoat_heal_loop");
                pet.healthBack=MakeLine("Decoy health background",obj.transform,Color.black,.10f);pet.healthView=MakeLine("Decoy health",obj.transform,mint,.06f);
                foreach(var line in new[]{pet.healthBack,pet.healthView}){line.useWorldSpace=true;line.positionCount=2;line.sortingOrder=15;}
                pet.healthView.sortingOrder=16;
            }
            if(spec.stats!=null&&spec.stats.interval>0){pet.weapon=Rules.PetWeapon(spec.stats,spec);pet.clock=spec.stats.interval;}
            if(spec.secondary!=null&&spec.secondary.interval>0){pet.secondary=Rules.PetWeapon(spec.secondary,spec);pet.secondaryClock=UnityEngine.Random.Range(spec.secondary.interval/4,spec.secondary.interval);}
            if(spec.kind=="bot_o_mine")pet.secondaryClock=spec.mineInterval;
            pet.maxHealth=pet.health;pet.rightClock=pet.clock;pet.patrolAngle=UnityEngine.Random.Range(0,Mathf.PI*2);pets.Add(pet);
        }
        void TickPets(float dt)
        {
            Rules.HealingDrones=0;int shieldIndex=0,shields=pets.Count(p=>p.spec.kind=="jellyshield");
            foreach(var p in pets)
            {
                Vector2 previous=p.pos;p.attackPose=Mathf.Max(0,p.attackPose-dt);
                p.age+=dt;p.clock-=dt;p.rightClock-=dt;p.secondaryClock-=dt;p.invulnerable=Mathf.Max(0,p.invulnerable-dt);
                var spec=p.spec;float speed=spec.speed*Mathf.Max(1,1+Rules.Stat("speed")/100);
                if(spec.kind=="jellyshield")
                {float angle=p.age+Mathf.PI*2*shieldIndex++/shields;p.pos=playerPos+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));}
                else if(spec.kind=="scapegoat")
                {
                    bool reviving=p.dead&&Vector2.Distance(playerPos,p.pos)<=1;
                    p.reviveSound.volume=sound.Muted?0:sound.Volume;
                    if(reviving&&!p.reviveSound.isPlaying)p.reviveSound.Play();else if(!reviving&&p.reviveSound.isPlaying)p.reviveSound.Stop();
                    if(p.dead)
                    {if(Vector2.Distance(playerPos,p.pos)<=1){p.health+=p.maxHealth/spec.revive*dt;if(p.health>=p.maxHealth){p.health=p.maxHealth;p.dead=false;OriginalSupportAudio.Play(sound,"goat_revive",p.pos);p.obj.GetComponent<SpriteRenderer>().color=Color.white;}}}
                    else WanderPet(p,speed,dt);
                }
                else if(spec.kind=="lootworm")
                {
                    var pickup=pickups.Where(x=>!x.fruit&&!x.motion.magnetized).OrderBy(x=>(x.motion.position-p.pos).sqrMagnitude).FirstOrDefault();
                    var tree=enemies.Where(e=>e.type.role=="Tree"&&e.marker==null).OrderBy(e=>(e.pos-p.pos).sqrMagnitude).FirstOrDefault();
                    Vector2 target=tree!=null?tree.pos:pickup!=null?pickup.motion.position:p.pos;
                    p.pos=Vector2.MoveTowards(p.pos,target,speed*dt);
                    foreach(var drop in pickups.Where(x=>!x.fruit&&!x.motion.magnetized&&(x.petCollector==p||x.petCollector==null&&Vector2.Distance(x.motion.position,p.pos)<2)).ToArray())
                    {
                        drop.petCollector=p;drop.petAttractSpeed+=12*dt;
                        drop.motion.position=Vector2.MoveTowards(drop.motion.position,p.pos,dt*drop.petAttractSpeed);
                        if(Vector2.Distance(drop.motion.position,p.pos)>.6f)continue;
                        if(UnityEngine.Random.value<spec.doubleChance)drop.value*=2;
                        p.attackPose=.225f;CollectPickup(drop);Destroy(drop.obj);pickups.Remove(drop);
                    }
                    if(tree!=null&&Vector2.Distance(tree.pos,p.pos)<.6f&&p.clock<=0)
                    {p.attackPose=.3f;p.clock=spec.stats.interval;Damage(enemies.IndexOf(tree),Mathf.Max(Rules.WeaponDamage(p.weapon),tree.maxHp/5),p.weapon.crit,p.weapon.knockback,tree.pos-p.pos,p.weapon,true);}
                }
                else if(spec.kind=="doc_moth")
                {
                    float radius=ArenaGeometry.HalfSize.y*.8f;p.patrolAngle+=speed/Mathf.Max(.1f,radius)*dt;
                    p.destination=new Vector2(Mathf.Cos(p.patrolAngle),Mathf.Sin(p.patrolAngle))*radius;
                    p.pos=Vector2.MoveTowards(p.pos,p.destination,speed*dt);
                    bool inside=Vector2.Distance(playerPos,p.pos)<spec.zone;if(inside)Rules.HealingDrones++;
                    if(inside!=p.inHealingZone){OriginalSupportAudio.Play(sound,inside?"moth_enter":"moth_exit",p.pos);p.inHealingZone=inside;}
                    for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;p.zoneView.SetPosition(i,p.pos+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*spec.zone);}
                }
                else
                {
                    bool cat=spec.kind=="catling_gun",ranged=cat||spec.kind=="bot_o_mine";
                    var target=ranged?PetRangedTarget(p.pos,p.weapon.range):SelectPetTarget(p);
                    bool wasBoosted=p.boosted;
                    p.boosted=cat&&(Vector2.Distance(playerPos,p.pos)<=1.5f||pets.Any(o=>o!=p&&!o.dead&&o.spec.kind!="catling_gun"&&o.spec.kind!="jellyshield"&&Vector2.Distance(o.pos,p.pos)<=1.5f));
                    if(p.boosted&&!wasBoosted)OriginalSupportAudio.Play(sound,"cat_boost",p.pos);
                    if(p.jump>0)
                    {
                        p.jump=Mathf.Max(0,p.jump-dt);float t=1-p.jump/.5f;
                        p.pos=Vector2.Lerp(p.jumpStart,p.destination,SupportBehavior.JumpProgress(t));
                        if(p.jump<=0){PetExplosion(p);p.secondaryClock=spec.secondary.interval;p.target=null;}
                    }
                    else if(cat){if(!p.boosted)WanderPet(p,speed,dt);}
                    else if(spec.kind=="bot_o_mine")p.pos=Vector2.MoveTowards(p.pos,playerPos,Vector2.Distance(p.pos,playerPos)>1?speed*dt:0);
                    else if(target!=null)p.pos=Vector2.MoveTowards(p.pos,target.pos,speed*dt);
                    if(spec.kind=="bonk_dog"&&p.jump<=0&&p.secondaryClock<=0)
                    {
                        var jumpTarget=enemies.Where(e=>e.marker==null&&e.type.role!="Tree"&&(e.pos-p.pos).sqrMagnitude>.025f).OrderByDescending(e=>e.maxHp).FirstOrDefault();
                        if(jumpTarget!=null){OriginalSupportAudio.Play(sound,"dog_attack",p.pos);p.jump=.5f;p.jumpStart=p.pos;p.destination=jumpTarget.pos;}
                        else p.secondaryClock=spec.secondary.interval*.4f;
                    }
                    if(spec.kind=="bot_o_mine"&&p.secondaryClock<=0)
                    {
                        p.attackPose=.3f;p.secondaryClock=spec.mineInterval;
                        var spawner=new StarterTurret{pos=p.pos,stats=Rules.PetWeapon(spec.mine.stats,spec),trait=new WeaponTrait{radius=spec.mine.explosionRadius}};
                        SpawnMine(spawner,SupportArt.Structure("landmine"),p.pos);
                    }
                    if(cat){p.obj.transform.position=p.pos;PoseCatGuns(p);TickCatGuns(p,dt);}
                    else if(p.jump<=0)
                    {
                        bool fireZone=enemies.Any(e=>e.marker==null&&e.type.role!="Tree"&&Vector2.Distance(e.pos,p.pos)<.8228f+HurtRadius(e));
                        if(spec.kind=="blazemander"&&p.secondaryClock<=0&&fireZone&&!p.pendingMelee)
                        {OriginalSupportAudio.Play(sound,"shot:blazemander",p.pos);p.attackPose=.3f;for(int i=0;i<6;i++){float angle=Mathf.PI*2*i/6+UnityEngine.Random.Range(-p.secondary.spread,p.secondary.spread);StarterProjectile(p.pos,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),p.secondary,true);}p.secondaryClock=spec.secondary.interval;}
                        else if(spec.kind=="blazemander"&&p.clock<=0&&fireZone&&!p.pendingMelee)
                        {p.pendingMelee=true;p.meleeDelay=.15f;p.attackPose=.3f;}
                        if(p.pendingMelee){p.meleeDelay-=dt;if(p.meleeDelay<=0){PetMelee(p);p.pendingMelee=false;p.clock=spec.stats.interval;}}
                        else if(spec.kind!="blazemander"&&p.clock<=0)
                        {
                            if(ranged&&target!=null){FirePet(p,p.pos,target.pos,p.weapon);p.clock=Rules.StructureCooldown(spec.stats.interval);}
                            else if(!ranged){PetMelee(p);p.clock=spec.stats.interval;}
                        }
                    }
                }

                bool moving=(p.pos-previous).sqrMagnitude>.000001f;
                p.posePhase+=dt*(spec.kind=="doc_moth"?6:moving?2:1);
                var renderer=p.obj.GetComponent<SpriteRenderer>();
                if(Mathf.Abs(p.pos.x-previous.x)>.001f)renderer.flipX=p.pos.x>previous.x;
                if(p.rig!=null)p.rig.PetPose(p.posePhase,moving,renderer.flipX,reducedMotion,p.dead);
                var pose=reducedMotion?Vector2.one:SupportMotion.PetScale(spec.kind,p.posePhase,moving,p.attackPose);
                if(p.dead)pose=new Vector2(1.12f,.65f);
                p.obj.transform.localScale=new Vector3(pose.x,pose.y,1)*p.baseScale;
                float jumpLift=p.jump>0&&!reducedMotion?Mathf.Sin(p.jump/.5f*Mathf.PI)*.8f:0;
                p.obj.transform.position=p.pos+Vector2.up*(jumpLift+(pose.y-1)*renderer.sprite.bounds.size.y*p.baseScale*.5f);
                p.obj.transform.rotation=Quaternion.Euler(0,0,p.dead?-18:!reducedMotion&&spec.kind=="lootworm"?Mathf.Sin(p.posePhase*Mathf.PI*2)*5:0);
                p.obj.GetComponent<GroundShadow>().Place(p.pos,p.baseScale);
                if(p.healthView!=null)
                {
                    p.healthView.enabled=p.healthBack.enabled=GameSettings.Flag("bossHp");Vector2 start=p.pos+new Vector2(-.3f,-.35f);
                    p.healthBack.SetPosition(0,start);p.healthBack.SetPosition(1,start+Vector2.right*.6f);
                    p.healthView.SetPosition(0,start);p.healthView.SetPosition(1,start+Vector2.right*(.6f*Mathf.Clamp01(p.health/p.maxHealth)));
                }
                if(spec.kind=="catling_gun"&&p.rig!=null)PoseCatGuns(p);
            }
            Rules.PetCount=pets.Count;
        }
        void PetExplosion(Pet pet)
        {
            int explosionKillsBefore=Rules.Kills;
            float radius=pet.spec.explosionRadius*Mathf.Max(0,1+Rules.Stat("explosion_size")/100);
            foreach(var e in enemies.ToArray())if(e.marker==null&&Vector2.Distance(e.pos,pet.pos)<radius)Damage(enemies.IndexOf(e),Rules.WeaponDamage(pet.secondary,Rules.Stat("explosion_damage")),pet.secondary.crit,pet.secondary.knockback,e.pos-pet.pos,pet.secondary,true);
            Rules.RecordExplosionKills(Rules.Kills-explosionKillsBefore);Burst(pet.pos,20,amber);
        }
        bool PetBlocksProjectile(Vector2 from,Vector2 to)
        {
            foreach(var p in pets)if(!p.dead&&(p.spec.kind=="jellyshield"||p.spec.kind=="scapegoat")&&SegmentDistance(p.pos,from,to)<.4f)
            {if(p.spec.kind=="jellyshield")OriginalSupportAudio.Play(sound,"shield_hit",p.pos);DamageScapegoat(p);p.attackPose=.3f;Burst(p.pos,4,mint);return true;}
            return false;
        }
        void DamageScapegoat(Pet pet)
        {
            if(pet.spec.kind!="scapegoat"||pet.invulnerable>0)return;
            pet.health--;pet.invulnerable=.6f;if(pet.health<=0){pet.health=0;pet.dead=true;OriginalSupportAudio.Play(sound,"goat_die",pet.pos);pet.obj.GetComponent<SpriteRenderer>().color=Color.gray;}
        }
    }
}
