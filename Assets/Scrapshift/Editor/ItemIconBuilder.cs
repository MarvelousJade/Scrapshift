using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        [Serializable] sealed class ItemIconEntry {public string id,title,hash;public int x,y,width,height,pixels;}
        [Serializable] sealed class ItemIconSheet {public string path;public int width,height;public ItemIconEntry[] entries;}
        [Serializable] sealed class ItemIconManifest {public ItemIconSheet[] sheets;}
        [Serializable] sealed class ItemIconAudit {public string status;public int items,unique;public string[] issues;}
        static Sprite[] ImportItemIconSheet(string path,SpriteMetaData[] metadata,int maxTextureSize=2048)
        {
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true;importer.isReadable=true;importer.mipmapEnabled=false;
            importer.maxTextureSize=maxTextureSize;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit=100;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
#pragma warning disable CS0618
            importer.spritesheet=metadata;
#pragma warning restore CS0618
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        }
        static void ApplyAllItemIcons(ReferenceCatalog catalog)
        {
            var manifest=JsonUtility.FromJson<ItemIconManifest>(File.ReadAllText(ArtRoot+"/ItemIcons/manifest.json"));
            foreach(var sheet in manifest.sheets)
            {
                var sprites=ImportItemIconSheet(sheet.path,sheet.entries.Select(e=>new SpriteMetaData {
                    name="Item_"+e.id,rect=new Rect(e.x,e.y,e.width,e.height),alignment=(int)SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)}).ToArray());
                foreach(var entry in sheet.entries)
                {
                    var item=catalog.data.items.Single(i=>i.id==entry.id);
                    item.image=sprites.Single(s=>s.name=="Item_"+entry.id);
                }
            }
            // These 24 authored icons already portray their individual items.
            string[] legacy={"bat","beanie","blindfold","blood_leech","broken_mouth","cake","claw_tree","coffee","cyclops_worm","decomposing_flesh","defective_steroids","fertilizer","fresh_meat","gambling_token","glasses","goat_skull","gummy_berserker","head_injury","hedgehog","helmet","injection","insanity","jelly","leather_vest"};
            var old=AssetDatabase.LoadAllAssetsAtPath(ArtRoot+"/items-v2.png").OfType<Sprite>().ToArray();
            for(int i=0;i<legacy.Length;i++)catalog.data.items.Single(x=>x.id==legacy[i]).image=old.Single(x=>x.name=="items_"+i.ToString("D2"));
            ApplySupportItemIcons(catalog);
            ValidateAllItemIcons(catalog);
        }
        static void ApplySupportItemIcons(ReferenceCatalog catalog)
        {
            const string path="Assets/Scrapshift/Resources/SupportAtlas.png";
            string[] ids={"turret","turret_flame","turret_rocket","turret_laser","turret_healing","landmines","garden","tyler","wandering_bot","bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla","doc_moth","lootworm","scapegoat","jellyshield"};
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();var metadata=new SpriteMetaData[18];
            for(int i=0;i<18;i++)
            {
                int x0=i%6*texture.width/6,x1=(i%6+1)*texture.width/6,y0=(2-i/6)*texture.height/3,y1=(3-i/6)*texture.height/3;
                int left=x1,right=x0,bottom=y1,top=y0;
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*texture.width+x].a>32){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
                metadata[i]=new SpriteMetaData{name="SupportItem_"+ids[i],rect=new Rect(left,bottom,right-left+1,top-bottom+1),alignment=(int)SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)};
            }
            var sprites=ImportItemIconSheet(path,metadata);
            foreach(string id in ids)catalog.data.items.Single(x=>x.id==id).image=sprites.Single(x=>x.name=="SupportItem_"+id);
        }
        static void ValidateAllItemIcons(ReferenceCatalog catalog)
        {
            var issues=new List<string>();var images=new HashSet<Sprite>();
            var readable=new Dictionary<Texture2D,Texture2D>();
            foreach(var item in catalog.data.items)
            {
                if(item.image==null){issues.Add(item.id+": missing sprite");continue;}
                if(!images.Add(item.image))issues.Add(item.id+": duplicate sprite assignment");
                if(item.image.name.StartsWith("stats_")||item.image.name.StartsWith("stat-"))issues.Add(item.id+": stat placeholder");
                var r=item.image.rect;var original=item.image.texture;
                if(!readable.TryGetValue(original,out var tex))
                {
                    tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(original)));readable.Add(original,tex);
                }
                bool visible=false;
                for(int y=(int)r.y;y<(int)r.yMax&&!visible;y++)for(int x=(int)r.x;x<(int)r.xMax;x++)if(tex.GetPixel(x,y).a>.12f){visible=true;break;}
                if(!visible)issues.Add(item.id+": empty sprite pixels");
            }
            Directory.CreateDirectory("artifacts/item-icon-audit");
            File.WriteAllText("artifacts/item-icon-audit/catalog.json",JsonUtility.ToJson(new ItemIconAudit{status=issues.Count==0?"Passed":"Failed",items=catalog.data.items.Length,unique=images.Count,issues=issues.ToArray()},true));
            foreach(var texture in readable.Values)UnityEngine.Object.DestroyImmediate(texture);
            if(issues.Count>0)throw new InvalidOperationException(string.Join("\n",issues));
            Debug.Log("Item icon audit passed: "+images.Count+" individual item sprites.");
        }
    }
}
