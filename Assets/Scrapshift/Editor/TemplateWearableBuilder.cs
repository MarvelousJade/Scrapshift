using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        // Full transparent canvases, registered once to the same body origin.
        // Pivots compensate for generated artwork placement; gameplay never fits
        // the alpha bounds into a slot or stretches a garment.
        static void ApplyTemplateWearables(ReferenceCatalog catalog)
        {
            string[] ids={"beanie","leather_vest","glasses","coffee","blindfold"};
            float[] bodyY={627f,718.5f,669f,699.5f,690f}; // top-down pixels on the 1254 canvas
            float[] pixelsPerUnit={627f,627f,627f,627f,860f};
            for(int i=0;i<ids.Length;i++)
            {
                string path=ArtRoot+"/TemplateWearables/"+ids[i]+".png";
                AssetDatabase.ImportAsset(path);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=true;importer.isReadable=true;importer.mipmapEnabled=false;
                importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit=pixelsPerUnit[i];importer.filterMode=FilterMode.Bilinear;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;
                settings.spritePivot=new Vector2(.5f,1-bodyY[i]/1254f);
                settings.spriteMeshType=SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);importer.SaveAndReimport();
                var image=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(image.rect.width!=1254||image.rect.height!=1254)throw new InvalidOperationException("Wearable template canvas changed: "+path);
                var item=catalog.data.items.Single(x=>x.id==ids[i]);
                var layer=item.appearances.Single();
                layer.image=image;layer.offset=Vector2.zero;layer.scale=1;layer.stretch=Vector2.one;
                item.appearance=layer;item.wearOffset=Vector2.zero;item.wearScale=1;
            }
        }
    }
}
