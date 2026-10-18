using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class Slash
        {
            public Enemy owner; public GameObject obj; public Vector2 pos, direction;
            public float age, damage; public bool hit,forward,deleteOnDeath;
        }
        readonly List<Slash> slashes = new List<Slash>();
        static readonly float[] bossRadii = {2.5f,3,3.5f,6,6.5f,7,9.5f,10,10.5f};
        float ProjectileDamage(Enemy e) => EnemyReferenceRules.ScaledDamage(1+e.behavior.projectileGrowth*(Rules.Wave-1),Rules.EnemyDamageFactor);
        GameObject PlasmaOrb(string name, Transform parent=null)
        {
            var obj=HostileProjectile("projectiles/bullet_enemy/enemy_projectile.tscn");obj.name=name;
            if(parent!=null)obj.transform.SetParent(parent,false);return obj;
        }

        void UpdateBossArm(Enemy e,float dt)
        {
            if(e.orbitProjectiles==null)
            {
                e.orbitProjectiles=new GameObject[bossRadii.Length];
                e.orbitDistances=new float[bossRadii.Length];e.orbitPlayerContacts=new bool[bossRadii.Length];e.orbitPetContacts=new HashSet<Pet>[bossRadii.Length];
                for(int i=0;i<bossRadii.Length;i++)e.orbitPetContacts[i]=new HashSet<Pet>();
                for(int i=0;i<bossRadii.Length;i++)e.orbitProjectiles[i]=PlasmaOrb("Rotating boss plasma",e.obj.transform);
            }
            e.orbitAngle+=dt*e.orbitSign*1.2f*(e.secondPhase?1.25f:1)*GameSettings.Value("enemySpeed");
            for(int i=0;i<bossRadii.Length;i++)
            {
                // Both extension and rotation use the source accessibility speed.
                e.orbitDistances[i]=AdvanceBossArmRadius(e.orbitDistances[i],bossRadii[i],dt,GameSettings.Value("enemySpeed"));
                Vector2 pos=BossArmPosition(e,e.orbitDistances[i]);
                e.orbitProjectiles[i].transform.position=new Vector3(pos.x,pos.y,-2);
                var parentScale=e.obj.transform.lossyScale;
                e.orbitProjectiles[i].transform.localScale=new Vector3(1/parentScale.x,1/parentScale.y,1);
                e.orbitProjectiles[i].transform.rotation=Quaternion.identity;
                ResolveBossArmPetContacts(e,i,pos);
                ResolveBossArmEntry(e,i,pos);
            }
        }
        float AdvanceBossArmRadius(float current,float target,float dt,float speed)=>current<target?current+dt*speed:current;
        Vector2 BossArmPosition(Enemy e,float radius)
        {
            Vector2 direction=new Vector2(Mathf.Cos(e.orbitAngle),Mathf.Sin(e.orbitAngle));
            return EnemyOrigin(e)+Vector2.up*.32f+direction*radius;
        }
        bool ResolveBossArmEntry(Enemy e,int index,Vector2 position)
        {
            bool touching=PlayerHurtboxActive&&Rules.Phase==RunPhase.Playing&&RunRules.Touches(position,PlayerHurtCenter,CombatGeometry.PlayerHurtRadius+CombatGeometry.HostileRadius);
            bool entered=touching&&!e.orbitPlayerContacts[index];e.orbitPlayerContacts[index]=touching;
            return entered&&ResolveBossArmContact(e,position);
        }
        void ResolveBossArmPetContacts(Enemy e,int index,Vector2 position)
        {
            var contacts=e.orbitPetContacts[index];
            foreach(var pet in pets)
            {
                bool goat=pet.spec.kind=="scapegoat",shield=pet.spec.kind=="jellyshield";
                bool touching=!pet.dead&&(shield||goat&&pet.invulnerable<=0)&&RunRules.Touches(position,pet.pos+(goat?SupportBehavior.ScapegoatHurtOffset:Vector2.zero),(goat?.32f:.3f)+CombatGeometry.HostileRadius);
                if(!touching){contacts.Remove(pet);continue;}
                if(!contacts.Add(pet))continue;
                if(shield)OriginalSupportAudio.Play(sound,"shield_hit",pet.pos);
                DamageScapegoat(pet);pet.attackPose=.3f;Burst(pet.pos,4,mint);
            }
            // Source destroy_on_hit=false keeps the arm alive after either contact.
        }
        bool ResolveBossArmContact(Enemy e,Vector2 position)
        {
            if(!PlayerHurtboxActive||Rules.Phase!=RunPhase.Playing||!RunRules.Touches(position,PlayerHurtCenter,CombatGeometry.PlayerHurtRadius+CombatGeometry.HostileRadius))return false;
            // These are persistent Projectile subclasses: the hurtbox contact
            // counts even when protection prevents health loss.
            Rules.RecordProjectileHit();HurtPlayer(ProjectileDamage(e),e.type,e);return true;
        }
        void EatMaterials(Enemy e,float dt)
        {
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var p=pickups[i];
                if(p.fruit)continue;
                Vector2 origin=e.pos+CombatGeometry.OriginOffset(e.type.id);
                bool entered=p.motion.AttractionEntered(e,origin,1);
                if(entered&&p.petCollector==null&&p.enemyCollector==null&&!p.motion.magnetized)p.enemyCollector=e;
                if(!p.motion.ContactEntered(e,origin+Vector2.up*.1f,.3f))continue;
                e.gold+=p.value;Destroy(p.obj);pickups.RemoveAt(i);e.hasDestination=false;
                if(e.evolution<2 && e.gold>=(e.evolution+1)*10)
                {
                    e.evolution++;int factor=(int)(1+.5f*e.evolution);
                    e.hp*=factor;e.maxHp*=factor;FloatText(e.pos,"EVOLVE",amber,false);
                }
            }
        }
        void EnemySlash(Enemy e)
        {
            Vector2 pos=playerPos+new Vector2(UnityEngine.Random.Range(-1f,1f),UnityEngine.Random.Range(-1f,1f));
            var obj=new GameObject("Slash warning");obj.transform.position=pos;
            var line=MakeLine("Slash",obj.transform,coral,.05f);line.sortingOrder=13;line.positionCount=2;
            line.SetPosition(0,Vector3.left*1.5f);line.SetPosition(1,Vector3.right*1.5f);
            float angle=UnityEngine.Random.Range(-Mathf.PI,Mathf.PI);obj.transform.rotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg);
            slashes.Add(new Slash { owner=e,obj=obj,pos=pos,direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),damage=ProjectileDamage(e) });
        }
        void UpdateSlashes(float dt)
        {
            for(int i=slashes.Count-1;i>=0;i--)
            {
                var s=slashes[i];s.age+=dt;
                if(s.age>=.63f || s.deleteOnDeath&&!enemies.Contains(s.owner)) { Destroy(s.obj);slashes.RemoveAt(i);continue; }
                bool active=s.age>=.48f && s.age<.6f;
                s.obj.GetComponent<EnemyProjectileView>()?.Pose(s.age);
                if(!active || s.hit)continue;
                int frame=Mathf.Clamp((int)((s.age-.48f)/.03f),0,3);
                float[] centers={-1.22f,-.71f,-.065f,.655f};
                float[] widths={.3f,.79f,1.365f,.885f};
                Vector2 local=PlayerHurtCenter-s.pos;
                float along=Vector2.Dot(local,s.direction)-(s.forward?1.65f:0),across=Vector2.Dot(local,new Vector2(-s.direction.y,s.direction.x));
                if(PlayerHurtboxActive&&Rules.Phase==RunPhase.Playing&&Mathf.Abs(along-centers[frame])<widths[frame]+CombatGeometry.PlayerHurtRadius && Mathf.Abs(across+.03f)<(frame==0?.17f:frame==1?.15f:frame==2?.14f:.12f)+CombatGeometry.PlayerHurtRadius)
                {Rules.RecordProjectileHit();HurtPlayer(s.damage,s.owner.type,s.owner);s.hit=true;}
            }
        }
    }
}
