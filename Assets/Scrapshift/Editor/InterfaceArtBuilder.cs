using System.IO;
using UnityEngine;
using UnityEditor;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureSilverLegs()
        {
            string path=Root+"/Prefabs/ScavengerAnimated.prefab";
            var saved=AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<SpriteFrameAnimator>();
            bool silver=AssetDatabase.GetAssetPath(saved.footSprite).EndsWith("player-leg-v3.png");
            if(silver && Mathf.Abs(Mathf.DeltaAngle(saved.rightFoot.transform.localEulerAngles.z,SpringMotion.LegRestAngle))<.01f
                && Mathf.Abs(saved.rightFoot.transform.localPosition.y-SpringMotion.LegY)<.001f)return;
            var sprite=silver?saved.footSprite:ImportSheet("player-leg",new[]{new Rect(0,0,1774,887)},333,version:3)[0];
            var root=PrefabUtility.LoadPrefabContents(path);var animator=root.GetComponent<SpriteFrameAnimator>();
            animator.footSprite=sprite;animator.leftFoot.sprite=sprite;animator.rightFoot.sprite=sprite;
            animator.Tick(Vector2.zero,0,false);
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
        }
        // Original small code-authored glyphs; no reference-game textures are embedded.
        static void EnsureStatIcons()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root+"/Definitions/ReferenceCatalog.asset");
            if(catalog.extraStatIcons!=null&&catalog.extraStatIcons.Length==StatPresentation.ExtraKeys.Length&&System.Array.TrueForAll(catalog.extraStatIcons,s=>s!=null))return;
            Directory.CreateDirectory(Root+"/StatIcons");
            catalog.extraStatIcons=new Sprite[StatPresentation.ExtraKeys.Length];
            for(int i=0;i<catalog.extraStatIcons.Length;i++)
            {
                var tex=new Texture2D(96,96,TextureFormat.RGBA32,false);var pixels=new Color[96*96];
                for(int y=0;y<96;y++)for(int x=0;x<96;x++)
                {
                    var p=new Vector2((x-47.5f)/40,(y-47.5f)/40);
                    float d=GlyphDistance(i,p);
                    Color fill=i==0?new Color(.86f,.21f,.27f):i==3?new Color(1,.65f,.18f):i==7?new Color(.37f,.85f,.36f):new Color(.81f,.87f,.89f);
                    pixels[y*96+x]=d<0?fill:d<.10f?new Color(.035f,.032f,.028f):Color.clear;
                }
                tex.SetPixels(pixels);tex.Apply();string file=Root+"/StatIcons/"+StatPresentation.ExtraKeys[i]+".png";
                File.WriteAllBytes(file,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(file);
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);importer.textureType=TextureImporterType.Sprite;
                importer.alphaIsTransparency=true;importer.spritePixelsPerUnit=96;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                catalog.extraStatIcons[i]=AssetDatabase.LoadAssetAtPath<Sprite>(file);
            }
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        static float Line(Vector2 p,Vector2 a,Vector2 b,float r)=>Vector2.Distance(p,a+(b-a)*Mathf.Clamp01(Vector2.Dot(p-a,b-a)/(b-a).sqrMagnitude))-r;
        static float GlyphDistance(int i,Vector2 p)
        {
            if(i==0) return Mathf.Min((p-new Vector2(-.28f,.22f)).magnitude-.33f,Mathf.Min((p-new Vector2(.28f,.22f)).magnitude-.33f,Line(p,new Vector2(-.38f,.13f),new Vector2(0,-.53f),.17f))); // heart + fang
            if(i==1) return Mathf.Min(Line(p,new Vector2(-.55f,-.60f),new Vector2(.57f,.64f),.12f),Line(p,new Vector2(-.52f,-.05f),new Vector2(-.08f,-.45f),.1f));
            if(i==2) return Mathf.Min(Line(p,new Vector2(-.42f,-.58f),new Vector2(-.42f,.50f),.16f),Line(p,new Vector2(.30f,-.34f),new Vector2(.30f,.70f),.16f));
            if(i==3) return Mathf.Min(Line(p,new Vector2(-.2f,-.65f),new Vector2(.1f,.55f),.19f),Line(p,new Vector2(.15f,-.45f),new Vector2(.42f,.2f),.18f));
            if(i==4) return Mathf.Min(Line(p,new Vector2(-.55f,-.6f),new Vector2(.4f,.4f),.14f),Mathf.Max(Mathf.Abs((p-new Vector2(.38f,.4f)).magnitude-.30f)-.11f,-p.y+.3f));
            if(i==5) return Mathf.Min(Mathf.Abs(p.magnitude-.54f)-.065f,Mathf.Min(Line(p,new Vector2(-.8f,0),new Vector2(.8f,0),.035f),Line(p,new Vector2(0,-.8f),new Vector2(0,.8f),.035f)));
            if(i==6) return Mathf.Min(Line(p,new Vector2(-.65f,-.5f),new Vector2(.55f,.5f),.1f),Mathf.Min(Line(p,new Vector2(-.65f,-.15f),new Vector2(-.27f,.18f),.07f),Line(p,new Vector2(-.65f,.17f),new Vector2(-.28f,.5f),.07f)));
            return Mathf.Min(Mathf.Min((p-new Vector2(-.3f,.3f)).magnitude-.29f,(p-new Vector2(.3f,.3f)).magnitude-.29f),Mathf.Min((p-new Vector2(-.3f,-.27f)).magnitude-.29f,(p-new Vector2(.3f,-.27f)).magnitude-.29f));
        }
    }
}
