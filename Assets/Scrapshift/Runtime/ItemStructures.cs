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
        void DeployStructure(StructureSpec spec,Sprite image,Vector2? position=null)
        {
            image=StructureImage(spec.kind);
            float span=Mathf.Min(6,4+Rules.OwnedStructures*.1f);
            var pos=position??(Rules.Stat("group_structures")>0&&spec.kind!="wandering_bot"?RunRules.ClampPosition(structureGroupCenter+new Vector2(UnityEngine.Random.Range(-span/2,span/2),UnityEngine.Random.Range(-span/2,span/2))):RandomPosition(playerPos,0));
            var trait=new WeaponTrait{key=spec.kind,projectile=spec.stats,spawnInterval=spec.spawnInterval,animationSpeed=spec.animationSpeed,radius=spec.explosionRadius};
            var obj=SpriteObject("Cybernetic "+spec.kind,image,pos,7);obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(image,.6f);
            AddGroundShadow(obj,image.bounds.size.x,-.2f);
            var structure=new StarterTurret{obj=obj,pos=pos,baseScale=obj.transform.localScale.x,animationAge=spec.kind=="garden"?0:.3f,stats=spec.Equipment,trait=trait,spec=spec,clock=spec.kind=="garden"?.2f/spec.animationSpeed:0,firing=spec.kind=="garden",angle=UnityEngine.Random.Range(0,Mathf.PI*2),orbitRadius=UnityEngine.Random.Range(1f,2f),orbitSpeed=UnityEngine.Random.Range(2f,2.5f)};
            structure.rig=SupportRig.Create(obj,spec.kind,true);
            if(spec.kind=="landmine"){Destroy(obj);structure.obj=null;structure.clock=spec.spawnInterval;mineSpawners.Add(structure);SpawnMine(structure,image);}
            else starterTurrets.Add(structure);
            progress?.Check("chal_turrets",starterTurrets.FindAll(t=>t.spec.kind!="garden"&&t.spec.kind!="wandering_bot").Count);
        }
        float StructureAnimationSpeed(StarterTurret turret)
        {float cooldown=Rules.StructureCooldown(turret.stats.interval);return Mathf.Max(turret.spec.animationSpeed,Mathf.Min(3,.3f/Mathf.Max(.001f,cooldown)));}
        void TickStructure(StarterTurret turret,float dt)
        {
            var spec=turret.spec;
            var target=spec.healing?null:NearestStarterTarget(turret.pos);
            turret.animationAge=Mathf.Min(.3f,turret.animationAge+dt*StructureAnimationSpeed(turret));
            var pose=reducedMotion||turret.rig!=null?Vector2.one:SupportMotion.TurretScale(turret.animationAge);
            turret.obj.transform.localScale=new Vector3(pose.x,pose.y,1)*turret.baseScale;
            turret.obj.transform.position=(Vector3)(turret.pos+Vector2.up*(reducedMotion||turret.rig!=null?0:SupportMotion.TurretLift(turret.animationAge)))+Vector3.back;
            if(turret.rig!=null)turret.rig.Aim(spec.healing?playerPos-turret.pos:target==null?Vector2.zero:target.pos-turret.pos,dt,turret.animationAge,reducedMotion);
            turret.obj.GetComponent<GroundShadow>().Place(turret.pos,turret.baseScale);
            if(spec.kind=="wandering_bot")
            {
                turret.angle+=dt*turret.orbitSpeed;turret.pos=playerPos+new Vector2(Mathf.Cos(turret.angle),Mathf.Sin(turret.angle))*turret.orbitRadius;
                turret.obj.transform.position=turret.pos;turret.obj.GetComponent<GroundShadow>().Place(turret.pos,turret.obj.transform.localScale.x);
                turret.touched.RemoveWhere(e=>!enemies.Contains(e)||Vector2.Distance(e.pos,turret.pos)>1.71f+HurtRadius(e));
                foreach(var e in enemies)if(e.marker==null&&Vector2.Distance(e.pos,turret.pos)<=1.71f+HurtRadius(e)&&turret.touched.Add(e))
                {e.contactSlow=SpecialWeaponRules.Slow(e.contactSlow,e.speed,2.5f);Burst(e.pos,2,mint);}
                return;
            }
            turret.clock-=dt;if(turret.clock>0)return;
            bool inRange=spec.kind=="garden"||(spec.healing?Vector2.Distance(playerPos,turret.pos)<=turret.stats.range:target!=null&&Vector2.Distance(target.pos,turret.pos)<=turret.stats.range);
            if(!turret.firing)
            {if(inRange){turret.firing=true;turret.animationAge=0;turret.clock=.2f/StructureAnimationSpeed(turret);}return;}
            turret.firing=false;
            float interval=Rules.StructureCooldown(turret.stats.interval);
            turret.clock=UnityEngine.Random.Range(Mathf.Max(1f/60,interval*.7f),interval*1.3f)+.1f/StructureAnimationSpeed(turret);
            if(!inRange)return;
            if(spec.kind=="garden"){DropConsumable(turret.pos,ConsumableKind.Fruit);sound.Play(SoundCue.Pickup,turret.pos);return;}
            Vector2 aim=spec.healing?playerPos-turret.pos:target.pos-turret.pos;
            float angle=Mathf.Atan2(aim.y,aim.x)+UnityEngine.Random.Range(-1+spec.accuracy,1-spec.accuracy);
            for(int i=0;i<turret.stats.pellets;i++)
            {
                float direction=angle+UnityEngine.Random.Range(-turret.stats.spread,turret.stats.spread);
                StarterProjectile(turret.pos,new Vector2(Mathf.Cos(direction),Mathf.Sin(direction)),turret.stats,true,structure:true,structureSpec:spec);
            }
            sound.Play(SoundCue.Fire,turret.pos);
        }
        void StructureExplosion(Vector2 position,StructureSpec spec,Enemy directHit)
        {
            int explosionKillsBefore=Rules.Kills;
            float radius=spec.explosionRadius*Mathf.Max(0,1+Rules.Stat("explosion_size")/100);
            float damage=SpecialWeaponRules.StructureDamage(Rules,spec.Equipment)*Mathf.Max(0,1+Rules.Stat("explosion_damage")/100);
            foreach(var enemy in enemies.ToArray())if(enemy!=directHit&&enemy.marker==null&&Vector2.Distance(enemy.pos,position)<=radius)
                Damage(enemies.IndexOf(enemy),damage,spec.stats.crit,0,enemy.pos-position,spec.Equipment,true,false,true);
            Rules.RecordExplosionKills(Rules.Kills-explosionKillsBefore);Burst(position,16,amber);sound.Play(SoundCue.Impact,position);
        }
    }
}
