using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureSpringArt()
        {
            if(File.Exists(Root+"/SpringArtV1.txt") && File.ReadAllText(Root+"/SpringArtV1.txt").Contains("atlas rectangles revision 3"))return;
            AssetDatabase.Refresh();
            var hero=ImportSheet("hero-spring",new[]{new Rect(0,0,820,1254),new Rect(820,0,434,1254)},500,true);
            // The generated bodies do not obey exact cell edges; preserve each full silhouette.
            int[,] bounds={{49,152,219,356},{256,130,508,364},{540,78,741,359},{784,130,1012,374},{1052,118,1248,356},{1270,118,1519,380},
                {23,410,241,660},{276,454,502,649},{518,389,765,657},{798,409,1068,657},{1083,432,1242,643},{1275,413,1515,663},
                {47,675,233,961},{278,716,507,956},{546,714,755,941},{786,722,1004,965},{1061,762,1280,958},{1281,684,1516,958}};
            var regions=new Rect[18];for(int i=0;i<18;i++)regions[i]=Rect.MinMaxRect(bounds[i,0]-3,bounds[i,1]-3,bounds[i,2]+3,bounds[i,3]+3);
            // Spitter and tree have only one transparent column between their silhouettes.
            regions[16].xMax=1281;regions[17].xMin=1280;
            var enemy=ImportSheet("enemies",regions,260,true,version:2);
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root+"/Definitions/ReferenceCatalog.asset");
            string[] ids={"baby_alien","bruiser","buffer","charger","chaser","evil_mob","fin_alien","helmet_alien","horned_bruiser","horned_charger","junkie","looter","predator","pursuer","slasher","spawner","spitter","tree"};
            for(int i=0;i<ids.Length;i++)catalog.Enemy(ids[i]).image=enemy[i];
            EditorUtility.SetDirty(catalog);
            // Mutate the existing prefab hierarchy to retain its identity and root file ID.
            string path=Root+"/Prefabs/ScavengerAnimated.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            var original=root.GetComponent<SpriteRenderer>();
            var existingBody=root.transform.Find("Spring body");
            var body=existingBody!=null?existingBody.gameObject:new GameObject("Spring body");body.transform.SetParent(root.transform,false);
            var renderer=body.GetComponent<SpriteRenderer>()??body.AddComponent<SpriteRenderer>();renderer.sprite=hero[0];renderer.sharedMaterial=catalog.spriteMaterial;renderer.sortingOrder=10;
            var animation=root.GetComponent<SpriteFrameAnimator>();
            animation.bodyView=renderer;animation.footSprite=hero[1];
            animation.idleFrames=new[]{hero[0]};animation.moveFrames=new[]{hero[0]};
            animation.leftFoot=CreateFoot(root.transform,hero[1],catalog.spriteMaterial,-1);
            animation.rightFoot=CreateFoot(root.transform,hero[1],catalog.spriteMaterial,1);
            if(original!=null)Object.DestroyImmediate(original);
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var game=scene.GetRootGameObjects().Select(o=>o.GetComponent<ArenaGame>()).First(g=>g!=null);
            game.playerPrefab=prefab;
            if(game.titleModel!=null)Object.DestroyImmediate(game.titleModel);
            game.titleModel=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            game.titleModel.name="Animated title scavenger";
            game.titleModel.transform.position=new Vector3(6,.2f,-1);game.titleModel.transform.localScale=Vector3.one*3.8f;
            EditorUtility.SetDirty(game);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText(Root+"/SpringArtV1.txt","Armless body, separate tiny feet, body-only enemy atlas; procedural spring motion; atlas rectangles revision 3.\n");
            AssetDatabase.Refresh();
        }
        static SpriteRenderer CreateFoot(Transform parent,Sprite sprite,Material material,int side)
        {
            string name=side<0?"Tiny left foot":"Tiny right foot";var existing=parent.Find(name);
            var obj=existing!=null?existing.gameObject:new GameObject(name);obj.transform.SetParent(parent,false);
            obj.transform.localPosition=new Vector3(side*.13f,-.64f,.01f);
            obj.transform.localScale=Vector3.one*(.17f/sprite.bounds.size.y);
            var view=obj.GetComponent<SpriteRenderer>()??obj.AddComponent<SpriteRenderer>();view.sprite=sprite;view.sharedMaterial=material;view.sortingOrder=9;
            return view;
        }
    }
}
