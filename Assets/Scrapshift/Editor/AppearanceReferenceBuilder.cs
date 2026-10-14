using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        [Serializable] sealed class SourceAppearanceLayer {public string id,texture,slot;public int position,priority,width,height;public float depth;public bool character;}
        [Serializable] sealed class SourceAppearanceEntry {public string id,source;public SourceAppearanceLayer[] layers;}
        [Serializable] sealed class SourceAppearanceData {public SourceAppearanceEntry[] items,characters;public float[] sourceBody;}
        [Serializable] sealed class FittedWearable {public string id,key,layerId;public int x,y,pixelWidth,pixelHeight;public float width,height,offsetX,offsetY;}
        [Serializable] sealed class FittedSheet {public string path;public FittedWearable[] entries;}
        [Serializable] sealed class FittedManifest {public FittedSheet[] sheets;}
        [Serializable] sealed class WearableRegistration {public string key;public float width,height,centerX,centerY;public bool reviewed;}
        [Serializable] sealed class WearableRegistrations {public WearableRegistration[] entries;}
        static System.Collections.Generic.Dictionary<string,AppearanceLayer> FittedAppearances(SourceAppearanceData source)
        {
            var manifest=JsonUtility.FromJson<FittedManifest>(File.ReadAllText(ArtRoot+"/FittedWearables/manifest.json"));
            var registrations=JsonUtility.FromJson<WearableRegistrations>(File.ReadAllText(ArtRoot+"/FittedWearables/registrations.json")).entries.ToDictionary(e=>e.key);
            if(registrations.Count!=181||registrations.Values.Any(e=>!e.reviewed||e.width<=0||e.height<=0))throw new InvalidOperationException("Incomplete wearable registration table.");
            var result=new System.Collections.Generic.Dictionary<string,AppearanceLayer>();
            foreach(var sheet in manifest.sheets)
            {
                var sprites=ImportItemIconSheet(sheet.path,sheet.entries.Select(e=>{
                    registrations.TryGetValue(e.key,out var fit);
                    // Register the art to the body origin, including attachments whose
                    // origin lies outside their cropped atlas rectangle. This is equivalent
                    // to transparent canvas padding, without duplicating large textures.
                    var pivot=fit==null?new Vector2(.5f,.5f):new Vector2(.5f-fit.centerX/fit.width,.5f-fit.centerY/fit.height);
                    return new SpriteMetaData{name="Fitted_"+e.key,rect=new Rect(e.x,e.y,e.pixelWidth,e.pixelHeight),alignment=(int)SpriteAlignment.Custom,pivot=pivot};
                }).ToArray(),4096);
                foreach(var entry in sheet.entries)
                {
                    var rule=source.items.Single(i=>i.id==entry.id).layers.First(l=>l.id==entry.layerId);
                    var image=sprites.Single(s=>s.name=="Fitted_"+entry.key);
                    registrations.TryGetValue(entry.key,out var fit);
                    var layer=OwnAppearance(rule,image,fit?.width??entry.width,fit==null?new Vector2(entry.offsetX,entry.offsetY):Vector2.zero);
                    layer.stretch=new Vector2(1,(fit?.height??entry.height)/(image.bounds.size.y*layer.scale));
                    result.Add(entry.key,layer);
                }
            }
            if(result.Count!=185)throw new InvalidOperationException("Expected 185 fitted wearable layers.");
            return result;
        }
        static Sprite OwnPart(string sheet,int index)=>AssetDatabase.LoadAllAssetsAtPath(ArtRoot+"/"+sheet+".png").OfType<Sprite>().Single(s=>s.name.EndsWith("_"+index.ToString("D2")));
        // Only the eligibility, slots, priorities and depth come from the reference.
        // Every visible layer is bound to Scrapshift's authored artwork.
        static AppearanceLayer OwnAppearance(SourceAppearanceLayer source,Sprite image,float width,Vector2 offset)
            =>new AppearanceLayer{id=source.id,slot=source.slot,priority=source.priority,depth=source.depth,sourceDefined=true,
                characterAppearance=source.character,image=image,offset=offset,scale=image==null?1:width/image.bounds.size.x,
                stretch=Vector2.one,order=source.depth<-1?Mathf.RoundToInt(source.depth):11+Mathf.RoundToInt(source.depth)};
        static void OwnAnchor(int position,out float width,out Vector2 offset)
        {
            width=.32f;offset=new Vector2(0,-.28f);
            switch(position)
            {
                case 1:width=.38f;offset=new Vector2(0,.77f);break;
                case 2:width=.38f;offset=new Vector2(-.38f,.5f);break;
                case 3:width=.38f;offset=new Vector2(.38f,.5f);break;
                case 4:width=.68f;offset=new Vector2(0,.52f);break;
                case 5:width=.25f;offset=new Vector2(-.18f,.34f);break;
                case 6:width=.44f;offset=new Vector2(0,.11f);break;
                case 7:width=.18f;offset=new Vector2(0,-.07f);break;
                case 8:width=.28f;offset=new Vector2(0,-.22f);break;
                case 9:width=.92f;offset=new Vector2(0,-.08f);break;
                case 10:width=.27f;offset=new Vector2(0,-.39f);break;
                case 11:case 14:width=.5f;offset=new Vector2(0,-.33f);break;
                case 12:width=.27f;offset=new Vector2(-.34f,-.34f);break;
                case 13:width=.27f;offset=new Vector2(.34f,-.34f);break;
                case 15:width=.48f;offset=new Vector2(.48f,-.3f);break;
                case 16:width=.42f;offset=new Vector2(-.5f,-.12f);break;
                case 17:width=.42f;offset=new Vector2(.5f,-.12f);break;
            }
        }
        static void EnsureReferenceAppearances(ReferenceCatalog catalog)
        {
            var source=JsonUtility.FromJson<SourceAppearanceData>(File.ReadAllText("Assets/Scrapshift/Resources/appearance-reference.json"));
            var fitted=FittedAppearances(source);
            foreach(var entry in source.items)
            {
                var item=catalog.data.items.Single(i=>i.id==entry.id);
                item.appearances=entry.layers.Select((s,index)=>fitted[entry.id+"_"+index]).ToArray();
                item.appearance=item.appearances.FirstOrDefault();item.wearSlot=item.appearance?.slot;item.wearOffset=item.appearance?.offset??Vector2.zero;
                item.wearScale=item.appearance?.scale??1;item.wearOrder=item.appearance?.order??11;
            }
            ApplyTemplateWearables(catalog);
            string path=Root+"/Prefabs/ScavengerAnimated.prefab";var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var animation=root.GetComponent<SpriteFrameAnimator>();
                animation.looks=catalog.data.characters.Select((c,index)=>{
                    var entry=source.characters.Single(e=>e.id==c.id);
                    c.appearances=entry.layers.Select(s=>{
                        OwnAnchor(s.position,out var width,out var offset);Sprite image=null;
                        if(!string.IsNullOrEmpty(s.texture))
                        {
                            int archetype=index%5;
                            if(s.position==6){image=OwnPart("player-parts-v2",archetype==0?2:archetype<3?3:4);width=archetype<3?.54f:.55f;offset=new Vector2(0,.11f);}
                            else if(s.position==8){image=OwnPart("player-parts-v2",archetype<3?5:6);width=archetype<3?.23f:.62f;offset=new Vector2(0,-.22f);}
                            else if(s.position==4){image=OwnPart("player-parts-v2",archetype<3?8:7);width=.62f;offset=new Vector2(0,.55f);}
                            else if(s.position==14){image=OwnPart("player-parts-v2",0);width=animation.bodyView.sprite.bounds.size.x;offset=Vector2.zero;}
                            else {string itemId=s.position==5?"head_injury":s.position==7?"injection":s.position==9?"bat":s.position==10?"blood_leech":s.position==2||s.position==3?"goat_skull":"leather_vest";image=catalog.data.items.Single(i=>i.id==itemId).image;}
                        }
                        return OwnAppearance(s,image,width,offset);
                    }).ToArray();
                    return new PlayerLook{id=c.id,name=c.title,layers=c.appearances};
                }).ToArray();
                var layers=catalog.data.items.SelectMany(i=>i.appearances).Concat(animation.looks.SelectMany(l=>l.layers)).Where(l=>l.image!=null);
                foreach(var layer in layers)if(!AssetDatabase.GetAssetPath(layer.image).StartsWith(ArtRoot+"/"))throw new InvalidOperationException("Non-Scrapshift appearance art: "+layer.id);
                PrefabUtility.SaveAsPrefabAsset(root,path);EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
                Debug.Log("185 fitted wearable parts bound to 178 items; reference appearance rules retained.");
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
    }
}
