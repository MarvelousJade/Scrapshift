using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    // Geometry masks separate overlapping atlas cells without rewriting their bitmap.
    [Serializable] public sealed class PixelRegion {public float x,y,width,height;public Rect Rect=>new Rect(x,y,width,height);}
    [Serializable] public sealed class WeaponIconMask {public string name;public PixelRegion source;public PixelRegion[] bands;}
    [Serializable] public sealed class WeaponIconMasks {public WeaponIconMask[] masks;}
    public static class AtlasIconMask
    {
        static WeaponIconMasks data;
        static WeaponIconMasks cleanData;
        static WeaponIconMasks itemData;
        public static WeaponIconMask[] Masks=>(data??(data=JsonUtility.FromJson<WeaponIconMasks>(Resources.Load<TextAsset>("weapon-icon-masks").text))).masks;
        static WeaponIconMask Find(Sprite sprite)
        {
            if(sprite.name.StartsWith("Item_"))
            {
                if(itemData==null)itemData=JsonUtility.FromJson<WeaponIconMasks>(Resources.Load<TextAsset>("item-icon-masks").text);
                return Array.Find(itemData.masks,m=>m.name==sprite.name);
            }
            if(sprite.name.StartsWith("Achievement_"))return AchievementArt.Mask(sprite.name);
            if(cleanData==null)cleanData=JsonUtility.FromJson<WeaponIconMasks>(Resources.Load<TextAsset>("clean-icon-masks").text);
            return Array.Find(cleanData.masks,m=>m.name==sprite.name)??(sprite.name.StartsWith("base-weapons_")?Array.Find(Masks,m=>m.name==sprite.name):null);
        }
        public static Rect[] Regions(Sprite sprite,Rect source)
        {
            var mask=Find(sprite);if(mask==null)return new[]{source};var result=new List<Rect>();
            foreach(var region in mask.bands){var band=region.Rect;var r=Rect.MinMaxRect(Mathf.Max(source.xMin,band.xMin),Mathf.Max(source.yMin,band.yMin),Mathf.Min(source.xMax,band.xMax),Mathf.Min(source.yMax,band.yMax));if(r.width>0&&r.height>0)result.Add(r);}
            return result.ToArray();
        }
        public static bool Visible(Sprite sprite,int x,int y){foreach(var r in Regions(sprite,sprite.rect))if(r.Contains(new Vector2(x+.5f,y+.5f)))return true;return false;}
        static readonly Dictionary<string,RenderTexture> previews=new Dictionary<string,RenderTexture>();
        static Material previewMaterial;
        public static Texture Preview(Sprite sprite)
        {
            if(Find(sprite)==null)return null;
            if(previews.TryGetValue(sprite.name,out var cached))return cached;
            Rect source=sprite.rect;var target=new RenderTexture((int)source.width,(int)source.height,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};target.Create();
            if(previewMaterial==null)previewMaterial=new Material(Shader.Find("Sprites/Default"));
            previewMaterial.mainTexture=sprite.texture;previewMaterial.SetColor("_Color",Color.white);
            var previous=RenderTexture.active;RenderTexture.active=target;GL.Clear(true,true,Color.clear);GL.PushMatrix();GL.LoadPixelMatrix(0,source.width,0,source.height);previewMaterial.SetPass(0);
            GL.Begin(GL.QUADS);GL.Color(Color.white);
            foreach(var r in Regions(sprite,source))foreach(var p in new[]{new Vector2(r.xMin,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMax,r.yMax),new Vector2(r.xMin,r.yMax)})
            {GL.TexCoord2(p.x/sprite.texture.width,p.y/sprite.texture.height);GL.Vertex3(p.x-source.x,p.y-source.y,0);}
            GL.End();GL.PopMatrix();RenderTexture.active=previous;previews[sprite.name]=target;return target;
        }
        public static Sprite CreateWorldSprite(Sprite original)
        {
            if(Find(original)==null)return original;
            var sprite=Sprite.Create(original.texture,original.rect,original.pivot/original.rect.size,original.pixelsPerUnit,0,SpriteMeshType.Tight);sprite.name=original.name;
            var vertices=new List<Vector2>();var indices=new List<ushort>();Rect source=sprite.rect;
            foreach(var r in Regions(sprite,source))
            {
                int start=vertices.Count;foreach(var point in new[]{new Vector2(r.xMin,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMax,r.yMax),new Vector2(r.xMin,r.yMax)})vertices.Add(point-source.position);
                foreach(int index in new[]{0,1,2,0,2,3})indices.Add((ushort)(start+index));
            }
            sprite.OverrideGeometry(vertices.ToArray(),indices.ToArray());return sprite;
        }
    }
}
