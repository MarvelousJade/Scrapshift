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
                for(int i=0;i<bossRadii.Length;i++)e.orbitProjectiles[i]=PlasmaOrb("Rotating boss plasma",e.obj.transform);
            }
            e.orbitAngle+=dt*e.orbitSign*1.2f*(e.secondPhase?1.25f:1);
            Vector2 direction=new Vector2(Mathf.Cos(e.orbitAngle),Mathf.Sin(e.orbitAngle));
            for(int i=0;i<bossRadii.Length;i++)
            {
                // Reference projectiles extend along one rotating arm at 100 px/s.
                // Source sprite center is -88px, pivot -32px: 56px below our body center.
                Vector2 pos=e.pos+Vector2.down*.56f+direction*Mathf.Min(bossRadii[i],e.activeAge);
                e.orbitProjectiles[i].transform.position=new Vector3(pos.x,pos.y,-2);
                var parentScale=e.obj.transform.lossyScale;
                e.orbitProjectiles[i].transform.localScale=new Vector3(1/parentScale.x,1/parentScale.y,1);
                e.orbitProjectiles[i].transform.rotation=Quaternion.identity;
                if(RunRules.Touches(pos,PlayerHurtCenter,CombatGeometry.PlayerHurtRadius+CombatGeometry.HostileRadius))HurtPlayer(ProjectileDamage(e),e.type,e);
            }
        }
        void EatMaterials(Enemy e,float dt)
        {
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var p=pickups[i];
                if(p.fruit || p.motion.magnetized || Vector2.Distance(p.motion.position,e.pos)>.65f)continue;
                p.motion.position=Vector2.MoveTowards(p.motion.position,e.pos,dt*9);
                if(Vector2.Distance(p.motion.position,e.pos)>.3f)continue;
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
                if(Mathf.Abs(along-centers[frame])<widths[frame]+CombatGeometry.PlayerHurtRadius && Mathf.Abs(across+.03f)<(frame==0?.17f:frame==1?.15f:frame==2?.14f:.12f)+CombatGeometry.PlayerHurtRadius)
                { HurtPlayer(s.damage,s.owner.type,s.owner);s.hit=true; }
            }
        }
    }
}
