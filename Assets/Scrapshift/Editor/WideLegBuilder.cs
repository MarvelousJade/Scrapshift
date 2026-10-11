using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureWideLegs()
        {
            if(File.Exists(Root+"/ActorPresentationV2.txt"))return;
            AssetDatabase.Refresh();
            var sprite=ImportSheet("player-leg",new[]{new Rect(0,0,1254,1254)},333,version:2)[0];
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root+"/Definitions/ReferenceCatalog.asset");
            catalog.shadowMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/Circle.asset");
            string materialPath=Root+"/Materials/GroundShadow.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null)
            {
                material=new Material(Shader.Find("Scrapshift/GroundShadow")){name="GroundShadow"};
                AssetDatabase.CreateAsset(material,materialPath);
            }
            catalog.shadowMaterial=material;EditorUtility.SetDirty(catalog);
            string path=Root+"/Prefabs/ScavengerAnimated.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            var animator=root.GetComponent<SpriteFrameAnimator>();animator.footSprite=sprite;
            foreach(var leg in new[]{animator.leftFoot,animator.rightFoot})
            {leg.sprite=sprite;leg.transform.localScale=Vector3.one*(SpringMotion.LegHeight/sprite.bounds.size.y);}
            animator.Tick(Vector2.zero,0,false);
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var game=scene.GetRootGameObjects().Select(o=>o.GetComponent<ArenaGame>()).First(g=>g!=null);
            if(game.titleModel!=null)Object.DestroyImmediate(game.titleModel);
            game.titleModel=(GameObject)PrefabUtility.InstantiatePrefab(game.playerPrefab);
            game.titleModel.name="Animated title scavenger";game.titleModel.transform.position=new Vector3(6,.2f,-1);game.titleModel.transform.localScale=Vector3.one*3.8f;
            EditorUtility.SetDirty(game);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText(Root+"/ActorPresentationV2.txt","Bold outlined curved legs; .85 player scale; 1.4 baby-alien visual scale; serialized circle mesh and transparent material for independent flat shadows.\n");
            AssetDatabase.Refresh();
        }
    }
}
