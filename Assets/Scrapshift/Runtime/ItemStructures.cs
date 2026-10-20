using System;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        void DeployTreeTurrets(Vector2 position)
        {
            int count=(int)Rules.Stat("tree_turrets");if(count<=0)return;
            var implant=Array.Find(catalog.data.items,i=>i.id=="turret");var spec=Array.Find(implant.conditionalEffects,e=>e.customKey=="structure").structure;
            // Source area=200 is a two-unit square, centered after clamping its bounds.
            Vector2 margin=ArenaGeometry.HalfSize-Vector2.one*1.64f;
            var center=new Vector2(Mathf.Clamp(position.x,-margin.x,margin.x),Mathf.Clamp(position.y,-margin.y,margin.y));
            for(int i=0;i<count;i++)DeployStructure(spec,StructureImage(spec.kind),center+new Vector2(UnityEngine.Random.Range(-1f,1f),UnityEngine.Random.Range(-1f,1f)));
        }
        Sprite StructureImage(string kind)
        {
            return SupportArt.Structure(kind);
        }
        Vector2 structureGroupCenter;
        Vector2 RandomSupportAreaPosition(Vector2 center,float span)
        =>SpawnPlacement.SourceAreaPosition(center,span,UnityEngine.Random.value,UnityEngine.Random.value);
        Vector2 RandomSupportGroupCenter()=>RandomSupportAreaPosition(Vector2.zero,Mathf.Floor(ArenaGeometry.HalfSize.x*200/3)/100);
        void DeployStructure(StructureSpec spec,Sprite image,Vector2? position=null)
        {
            image=StructureImage(spec.kind);
            float span=SupportBehavior.GroupedSpawnSpan(Rules.RegisteredStructureCount);
            var pos=position??(spec.kind=="wandering_bot"?RandomSupportAreaPosition(playerPos,2):Rules.Stat("group_structures")>0&&SupportBehavior.CanGroupStructure(spec.kind)?RandomSupportAreaPosition(structureGroupCenter,span):SpawnPlacement.SourceRandomPosition(UnityEngine.Random.value,UnityEngine.Random.value,1.6f));
            var trait=new WeaponTrait{key=spec.kind,projectile=spec.stats,spawnInterval=spec.spawnInterval,animationSpeed=spec.animationSpeed,radius=spec.explosionRadius};
            var obj=SpriteObject("Cybernetic "+spec.kind,image,pos,7);obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(image,.6f);
            AddGroundShadow(obj,image.bounds.size.x,-.2f);
            obj.GetComponent<GroundShadow>().Place(pos,obj.transform.localScale.x);
            var structure=new StarterTurret{obj=obj,pos=pos,baseScale=obj.transform.localScale.x,animationAge=spec.kind=="garden"?0:.3f,stats=spec.Equipment,trait=trait,spec=spec,clock=spec.kind=="garden"?.2f/spec.animationSpeed:0,firing=spec.kind=="garden",angle=spec.kind=="wandering_bot"?UnityEngine.Random.Range(0,Mathf.PI*2):0,orbitRadius=UnityEngine.Random.Range(1f,2f),orbitSpeed=UnityEngine.Random.Range(2f,2.5f)};
            structure.rig=SupportRig.Create(obj,spec.kind,true);
            if(spec.kind=="landmine"){Destroy(obj);structure.obj=null;structure.clock=spec.spawnInterval;mineSpawners.Add(structure);SpawnMineAt(structure,image,pos);}
            else starterTurrets.Add(structure);
        }
        float StructureAnimationSpeed(StarterTurret turret)
        {float cooldown=Rules.StructureCooldown(turret.stats.interval*(turret.boostClock>0?.8f:1));return Mathf.Max(turret.spec.animationSpeed,Mathf.Min(3,.3f/Mathf.Max(.001f,cooldown)));}
        void TickStructure(StarterTurret turret,float dt)
        {
            var spec=turret.spec;turret.boostClock=Mathf.Max(0,turret.boostClock-dt);
            float boostedRange=(int)Mathf.Max(25,turret.stats.range*100*(turret.boostClock>0?1.2f:1)+Rules.Stat("structure_range"))/100f;
            Vector2 origin=turret.pos+SupportBehavior.StructureOriginOffset(spec.kind);
            var target=spec.healing?null:StructureTarget(origin,boostedRange,spec.kind!="tyler");
            turret.animationAge=Mathf.Min(.3f,turret.animationAge+dt*StructureAnimationSpeed(turret));
            var pose=reducedMotion||turret.rig!=null?Vector2.one:SupportMotion.TurretScale(turret.animationAge);
            turret.obj.transform.localScale=new Vector3(pose.x,pose.y,1)*turret.baseScale;
            turret.obj.transform.position=(Vector3)(turret.pos+Vector2.up*(reducedMotion||turret.rig!=null?0:SupportMotion.TurretLift(turret.animationAge)))+Vector3.back;
            if(turret.rig!=null)turret.rig.Aim(spec.healing?playerPos-turret.pos:target==null?Vector2.zero:target.pos-turret.pos,dt,turret.animationAge,reducedMotion);
            turret.obj.GetComponent<GroundShadow>().Place(turret.pos,turret.baseScale);
            if(spec.kind=="wandering_bot")
            {
                turret.angle+=dt*turret.orbitSpeed;turret.pos=playerPos+CombatGeometry.PlayerOriginOffset+new Vector2(Mathf.Cos(turret.angle),Mathf.Sin(turret.angle))*turret.orbitRadius;
                turret.obj.transform.position=turret.pos;turret.obj.GetComponent<GroundShadow>().Place(turret.pos,turret.obj.transform.localScale.x);
                turret.touched.RemoveWhere(e=>!enemies.Contains(e)||e.hp<=0||!SupportBehavior.WanderingZoneOverlaps(turret.pos,e.type.id,e.pos));
                foreach(var e in enemies)if(e.marker==null&&e.hp>0&&SupportBehavior.WanderingZoneOverlaps(turret.pos,e.type.id,e.pos)&&turret.touched.Add(e))
                {ApplyWanderingSlow(e);Burst(e.pos,2,mint);}
                return;
            }
            turret.clock-=dt;if(turret.clock>0)return;
            bool playerInPool=spec.healing&&SupportBehavior.StructurePlayerInRange(origin,boostedRange,playerPos);
            Vector2 targetOrigin=spec.healing?playerPos+CombatGeometry.PlayerOriginOffset:target==null?origin:target.pos+CombatGeometry.OriginOffset(target.type.id);
            bool inRange=spec.kind=="garden"||((spec.healing?playerInPool:target!=null)&&SupportBehavior.StructureTargetDistance(targetOrigin-origin)>0&&SupportBehavior.StructureFiringReach(targetOrigin-origin,boostedRange));
            if(!turret.firing)
            {if(inRange){turret.firing=true;turret.animationAge=0;turret.clock=.2f/StructureAnimationSpeed(turret);}return;}
            turret.firing=false;
            float interval=Rules.StructureCooldown(turret.stats.interval*(turret.boostClock>0?.8f:1));
            turret.shotsTaken++;
            bool reload=turret.stats.reloadEvery>0&&turret.shotsTaken%turret.stats.reloadEvery==0;
            turret.clock=(reload?interval*turret.stats.reloadMultiplier:UnityEngine.Random.Range(Mathf.Max(1f/60,interval*.7f),interval*1.3f))+.1f/StructureAnimationSpeed(turret);
            if(spec.kind=="garden"){DropConsumable(origin,ConsumableKind.Fruit);OriginalSupportAudio.Play(sound,"shot:garden",turret.pos);return;}
            // The source animation callback still emits when its target died or
            // exited during windup, retaining _next_proj_rotation (initially 0).
            // Range gates starting the animation, not the later pellet spawn.
            if(spec.healing?playerInPool:target!=null)
            {
                Vector2 aim=(spec.healing?playerPos+CombatGeometry.PlayerOriginOffset:target.pos+CombatGeometry.OriginOffset(target.type.id))-origin;
                float jitter=Mathf.Abs(1-spec.accuracy-Rules.Stat("accuracy")/100);
                turret.angle=Mathf.Atan2(aim.y,aim.x)+UnityEngine.Random.Range(-jitter,jitter);
            }
            float spread=turret.stats.spread+Rules.Stat("projectiles")*.1f;
            for(int i=0;i<turret.stats.pellets+(int)Rules.Stat("projectiles");i++)
            {
                float direction=turret.angle+UnityEngine.Random.Range(-spread,spread);
                StarterProjectile(turret.pos+SupportBehavior.StructureMuzzleOffset(spec.kind),new Vector2(Mathf.Cos(direction),Mathf.Sin(direction)),turret.stats,true,structure:true,structureSpec:spec);
                if(turret.boostClock>0&&shots.Count>0)shots[shots.Count-1].damage+=turret.stats.damage*.2f*Mathf.Max(0,1+Rules.Stat("structure_percent_damage")/100);
            }
            OriginalSupportAudio.Play(sound,"shot:"+spec.kind,turret.pos);
        }
        void ApplyWanderingSlow(Enemy enemy)
        {
            // Unit.add_decaying_speed caps against current_stats.speed, which
            // already includes run modifiers and permanent on-hit reductions.
            enemy.contactSlow=SpecialWeaponRules.Slow(enemy.contactSlow,EnemyStatSpeed(enemy)-enemy.itemSlow,2.5f);
        }
        Enemy StructureTarget(Vector2 origin,float range,bool trees)
        {
            Enemy best=null;int distance=int.MaxValue;
            foreach(var e in enemies)if(e.hp>0&&e.marker==null&&(CombatGeometry.BodyLayer(e.type.id)&(trees?5:4))!=0&&SupportBehavior.StructureBodyInRange(origin,range,e.type.id,e.pos))
            {
                int next=SupportBehavior.StructureTargetDistance(e.pos+CombatGeometry.OriginOffset(e.type.id)-origin);
                if(next>0&&next<distance){best=e;distance=next;}
            }
            return best;
        }
        void StructureExplosion(Vector2 position,StructureSpec spec,Enemy directHit)
        {
            StructureExplosionWithDamage(position,spec,SpecialWeaponRules.StructureDamage(Rules,spec.Equipment,Rules.Stat("explosion_damage")));
        }
        void StructureExplosionWithDamage(Vector2 position,StructureSpec spec,float damage,BurnPayload payload=null)
        {
            float radius=SupportBehavior.ExplosionRadius(spec.explosionRadius,Rules.Stat("explosion_size"));
            QueueSupportExplosion(position,spec.Equipment,damage,radius,true,burnPayload:payload);
            Burst(position,16,amber);sound.Play(SoundCue.Impact,position,contextProfile:ContextSoundProfile.Explosion);
        }
    }
}
