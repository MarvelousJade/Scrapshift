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
            public Equipment weapon,secondary;public bool dead;
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
            var image=SupportArt.Pet(spec.kind);var pos=playerPos+UnityEngine.Random.insideUnitCircle*2;
            var obj=SpriteObject("Cybernetic companion: "+spec.kind,image,pos,7);obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(image,.65f);AddGroundShadow(obj,image.bounds.size.x,-.2f);
            var pet=new Pet{spec=spec,obj=obj,baseScale=obj.transform.localScale.x,pos=pos,destination=pos,health=spec.health+spec.healthGrowth*(Rules.Wave-1)};
            if(spec.stats!=null&&spec.stats.interval>0){pet.weapon=Rules.PetWeapon(spec.stats,spec);pet.clock=spec.stats.interval;}
            if(spec.secondary!=null&&spec.secondary.interval>0){pet.secondary=Rules.PetWeapon(spec.secondary,spec);pet.secondaryClock=UnityEngine.Random.Range(spec.secondary.interval/4,spec.secondary.interval);}
            if(spec.kind=="bot_o_mine")pet.secondaryClock=spec.mineInterval;
            pets.Add(pet);
        }
        void TickPets(float dt)
        {
            Rules.HealingDrones=0;int shieldIndex=0,shields=pets.Count(p=>p.spec.kind=="jellyshield");
            foreach(var p in pets)
            {
                Vector2 previous=p.pos;p.attackPose=Mathf.Max(0,p.attackPose-dt);
                p.age+=dt;p.clock-=dt;p.secondaryClock-=dt;p.invulnerable=Mathf.Max(0,p.invulnerable-dt);
                var spec=p.spec;float speed=spec.speed*Mathf.Max(1,1+Rules.Stat("speed")/100);
                if(spec.kind=="jellyshield")
                {float angle=p.age+Mathf.PI*2*shieldIndex++/shields;p.pos=playerPos+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));}
                else if(spec.kind=="scapegoat")
                {
                    if(p.dead)
                    {if(Vector2.Distance(playerPos,p.pos)<=1){p.health+=spec.health/spec.revive*dt;if(p.health>=spec.health){p.health=spec.health;p.dead=false;p.obj.GetComponent<SpriteRenderer>().color=Color.white;}}}
                    else p.pos=Vector2.MoveTowards(p.pos,playerPos,Vector2.Distance(p.pos,playerPos)>1?speed*dt:0);
                }
                else if(spec.kind=="lootworm")
                {
                    var pickup=pickups.Where(x=>!x.fruit&&!x.motion.magnetized).OrderBy(x=>(x.motion.position-p.pos).sqrMagnitude).FirstOrDefault();
                    var tree=enemies.Where(e=>e.type.role=="Tree"&&e.marker==null).OrderBy(e=>(e.pos-p.pos).sqrMagnitude).FirstOrDefault();
                    Vector2 target=pickup!=null?pickup.motion.position:tree!=null?tree.pos:playerPos;
                    p.pos=Vector2.MoveTowards(p.pos,target,speed*dt);
                    foreach(var drop in pickups.Where(x=>!x.fruit&&!x.motion.magnetized&&Vector2.Distance(x.motion.position,p.pos)<3).ToArray())
                    {
                        drop.motion.position=Vector2.MoveTowards(drop.motion.position,p.pos,dt*9);
                        if(Vector2.Distance(drop.motion.position,p.pos)>.5f)continue;
                        if(UnityEngine.Random.value<spec.doubleChance)drop.value*=2;
                        p.attackPose=.225f;CollectPickup(drop);Destroy(drop.obj);pickups.Remove(drop);
                    }
                    if(tree!=null&&Vector2.Distance(tree.pos,p.pos)<.6f&&p.clock<=0)
                    {p.attackPose=.3f;p.clock=spec.stats.interval;Damage(enemies.IndexOf(tree),Mathf.Max(Rules.WeaponDamage(p.weapon),tree.maxHp/5),p.weapon.crit,p.weapon.knockback,tree.pos-p.pos,p.weapon,true);}
                }
                else if(spec.kind=="doc_moth")
                {
                    if(Vector2.Distance(p.pos,p.destination)<.3f)p.destination=RandomPosition(playerPos,0);
                    p.pos=Vector2.MoveTowards(p.pos,p.destination,speed*dt);
                    if(Vector2.Distance(playerPos,p.pos)<spec.zone)Rules.HealingDrones=1;
                }
                else
                {
                    var target=NearestStarterTarget(p.pos);bool ranged=spec.kind=="catling_gun"||spec.kind=="bot_o_mine";
                    bool boosted=spec.kind=="catling_gun"&&(Vector2.Distance(playerPos,p.pos)<=1.5f||pets.Any(o=>o!=p&&o.spec.kind!="catling_gun"&&Vector2.Distance(o.pos,p.pos)<=1.5f));
                    if(p.jump>0)
                    {
                        p.jump=Mathf.Max(0,p.jump-dt);p.pos=Vector2.Lerp(p.jumpStart,p.destination,1-p.jump/.5f);
                        if(p.jump<=0){PetExplosion(p);p.secondaryClock=spec.secondary.interval;}
                    }
                    else if(!boosted)
                    {Vector2 destination=ranged?playerPos:target?.pos??playerPos;p.pos=Vector2.MoveTowards(p.pos,destination,Vector2.Distance(p.pos,destination)>(ranged?1:.45f)?speed*dt:0);}
                    if(spec.kind=="bonk_dog"&&p.jump<=0&&p.secondaryClock<=0)
                    {
                        var jumpTarget=enemies.Where(e=>e.marker==null&&e.type.role!="Tree"&&(e.pos-p.pos).sqrMagnitude>.2f).OrderByDescending(e=>e.maxHp).FirstOrDefault();
                        if(jumpTarget!=null){p.jump=.5f;p.jumpStart=p.pos;p.destination=jumpTarget.pos;}
                    }
                    if(spec.kind=="bot_o_mine"&&p.secondaryClock<=0)
                    {
                        p.attackPose=.3f;p.secondaryClock=spec.mineInterval;
                        var spawner=new StarterTurret{pos=p.pos+UnityEngine.Random.insideUnitCircle*.5f,stats=Rules.PetWeapon(spec.mine.stats,spec),trait=new WeaponTrait{radius=spec.mine.explosionRadius}};
                        SpawnMine(spawner,catalog.Weapon("screwdriver",1).image,p.pos);
                    }
                    if(target!=null&&p.jump<=0)
                    {
                        if(spec.kind=="blazemander"&&p.secondaryClock<=0&&Vector2.Distance(target.pos,p.pos)<1.5f)
                        {p.attackPose=.3f;for(int i=0;i<6;i++){float angle=Mathf.PI*2*i/6+UnityEngine.Random.Range(-p.secondary.spread,p.secondary.spread);StarterProjectile(p.pos,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),p.secondary,true);}p.secondaryClock=spec.secondary.interval;}
                        if(p.clock<=0&&Vector2.Distance(target.pos,p.pos)<(ranged?p.weapon.range:.7f))
                        {
                            p.attackPose=.3f;p.clock=(spec.structure?Rules.StructureCooldown(spec.stats.interval):Rules.PetCooldown(spec.stats.interval))*(spec.kind=="catling_gun"?(boosted?.25f:.5f):1);
                            if(ranged)StarterProjectile(p.pos,(target.pos-p.pos).normalized,p.weapon,true,structure:spec.structure);
                            else foreach(var enemy in enemies.ToArray())if(enemy.marker==null&&Vector2.Distance(enemy.pos,p.pos)<.7f)Damage(enemies.IndexOf(enemy),Rules.WeaponDamage(p.weapon),p.weapon.crit,p.weapon.knockback,enemy.pos-p.pos,p.weapon,true);
                        }
                    }
                }
                bool moving=(p.pos-previous).sqrMagnitude>.000001f;
                p.posePhase+=dt*(spec.kind=="doc_moth"?6:moving?2:1);
                var renderer=p.obj.GetComponent<SpriteRenderer>();
                if(Mathf.Abs(p.pos.x-previous.x)>.001f)renderer.flipX=p.pos.x>previous.x;
                var pose=reducedMotion?Vector2.one:SupportMotion.PetScale(spec.kind,p.posePhase,moving,p.attackPose);
                if(p.dead)pose=new Vector2(1.12f,.65f);
                p.obj.transform.localScale=new Vector3(pose.x,pose.y,1)*p.baseScale;
                float jumpLift=p.jump>0&&!reducedMotion?Mathf.Sin(p.jump/.5f*Mathf.PI)*.8f:0;
                p.obj.transform.position=p.pos+Vector2.up*(jumpLift+(pose.y-1)*renderer.sprite.bounds.size.y*p.baseScale*.5f);
                p.obj.transform.rotation=Quaternion.Euler(0,0,p.dead?-18:!reducedMotion&&spec.kind=="lootworm"?Mathf.Sin(p.posePhase*Mathf.PI*2)*5:0);
                p.obj.GetComponent<GroundShadow>().Place(p.pos,p.baseScale);
            }
            Rules.PetCount=pets.Count;
        }
        void PetExplosion(Pet pet)
        {
            int explosionKillsBefore=Rules.Kills;
            float radius=pet.spec.explosionRadius*Mathf.Max(0,1+Rules.Stat("explosion_size")/100);
            foreach(var e in enemies.ToArray())if(e.marker==null&&Vector2.Distance(e.pos,pet.pos)<radius)Damage(enemies.IndexOf(e),Rules.WeaponDamage(pet.secondary,Rules.Stat("explosion_damage")),pet.secondary.crit,pet.secondary.knockback,e.pos-pet.pos,pet.secondary,true);
            Rules.RecordExplosionKills(Rules.Kills-explosionKillsBefore);Burst(pet.pos,20,amber);sound.Play(SoundCue.Impact,pet.pos);
        }
        bool PetBlocksProjectile(Vector2 from,Vector2 to)
        {
            foreach(var p in pets)if(!p.dead&&(p.spec.kind=="jellyshield"||p.spec.kind=="scapegoat")&&SegmentDistance(p.pos,from,to)<.4f)
            {DamageScapegoat(p);Burst(p.pos,4,mint);return true;}
            return false;
        }
        void DamageScapegoat(Pet pet)
        {
            if(pet.spec.kind!="scapegoat"||pet.invulnerable>0)return;
            pet.health--;pet.invulnerable=.6f;if(pet.health<=0){pet.dead=true;pet.obj.GetComponent<SpriteRenderer>().color=Color.gray;}
        }
    }
}
