using System;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed class EnemyProjectileView:MonoBehaviour
    {
        [Serializable] sealed class Data {public ProjectileReference[] projectiles;}
        static Data data;static Material material;
        ProjectileReference profile;SpriteRenderer sprite;
        public static ProjectileReference[] Profiles => (data??(data=JsonUtility.FromJson<Data>(Resources.Load<TextAsset>("enemy-projectiles").text))).projectiles;
        public static ProjectileReference Profile(string path)=>Profiles.First(x=>x.id==path);
        public void Initialize(string path)
        {
            profile=Profile(path);
            if(material==null)material=new Material(Resources.Load<Shader>("EnemyProjectileTone"));
            sprite=new GameObject("Source hostile projectile").AddComponent<SpriteRenderer>();sprite.transform.SetParent(transform,false);
            sprite.sharedMaterial=material;sprite.sortingOrder=13;Pose(0);
        }
        public void Pose(float age)
        {
            sprite.sprite=profile.Frame(age);
            sprite.transform.localPosition=new Vector3(profile.offset[0]/100,-profile.offset[1]/100,0);
            float scale=profile.id.Contains("pillar")&&age>=.68f?Mathf.Max(0,(1.04f-age)/.36f):1;
            sprite.transform.localScale=new Vector3(profile.scale[0]*scale,profile.scale[1]*scale,1);
        }
    }
    public sealed partial class ArenaGame
    {
        GameObject HostileProjectile(string path)
        {var obj=new GameObject("Hostile "+path);obj.AddComponent<EnemyProjectileView>().Initialize(path);return obj;}
    }
}
