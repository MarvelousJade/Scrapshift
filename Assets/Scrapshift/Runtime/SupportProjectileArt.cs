using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public static class SupportProjectileArt
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        static ProjectileReference Existing(string scene)=>System.Array.Find(WeaponReference.Projectiles,p=>p.id==scene);
        public static Vector2 OffsetPixels(string kind)=>kind=="flame"||kind=="pet_flame"||kind=="rocket"?new Vector2(24,0):kind=="tyler"?new Vector2(4,1):new Vector2(1,-1);
        public static Sprite Frame(string kind,float age)
        {
            string path;
            if(kind=="pet_flame"||kind=="tyler"||kind!="flame"&&kind!="laser"&&kind!="healing"&&kind!="rocket")
            {
                string scene=kind=="pet_flame"?"projectiles/flames/flame_projectile.tscn":kind=="tyler"?"projectiles/bullet_lightning/lightning_projectile.tscn":"projectiles/bullet/bullet_projectile.tscn";
                var profile=Existing(scene);path=profile.frames[profile.FrameIndex(age)];
            }
            else path="SupportProjectiles/"+(kind=="flame"?"flame"+Mathf.Min(7,Mathf.FloorToInt(Mathf.Max(0,age)/.05f)):kind=="laser"?"laser"+(age<.02f?0:1):kind=="healing"?"medical"+(age<.02f?0:1):"rocket");
            string key=kind+":"+path;
            if(cache.TryGetValue(key,out var sprite))return sprite;
            var texture=Resources.Load<Texture2D>(path);var offset=OffsetPixels(kind);
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f-offset.x/texture.width,.5f-offset.y/texture.height),100,0,SpriteMeshType.FullRect);
            sprite.name="SupportProjectile_"+path.Substring(path.LastIndexOf('/')+1);cache[key]=sprite;return sprite;
        }
        public static void Pose(SpriteRenderer view,string kind,float age)
        {view.sprite=Frame(kind,age);view.transform.localScale=Vector3.one;}
    }
}
