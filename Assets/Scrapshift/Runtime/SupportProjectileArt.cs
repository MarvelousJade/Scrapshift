using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public static class SupportProjectileArt
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        public static Sprite Frame(string kind,float age)
        {
            string key=kind=="flame"?"flame"+Mathf.Min(7,Mathf.FloorToInt(age/.05f)):kind=="laser"?"laser"+(age<.02f?0:1):kind=="healing"?"medical"+(age<.02f?0:1):kind=="rocket"?"rocket":"bullet";
            if(cache.TryGetValue(key,out var sprite))return sprite;
            var texture=Resources.Load<Texture2D>("SupportProjectiles/"+key);
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f-24f/texture.width,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name="SupportProjectile_"+key;cache[key]=sprite;return sprite;
        }
        public static void Pose(SpriteRenderer view,string kind,float age)
        {view.sprite=Frame(kind,age);view.transform.localScale=Vector3.one;}
    }
}
