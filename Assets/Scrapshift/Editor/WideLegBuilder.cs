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
            if(File.Exists(Root+"/WideLegsV1.txt"))return;
            AssetDatabase.Refresh();
            var sprite=ImportSheet("player-leg",new[]{new Rect(0,0,1254,1254)},333)[0];
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
            File.WriteAllText(Root+"/WideLegsV1.txt","Outward-curved mirrored leg sprite; wider .62-unit stance, .22-unit legs at lower body corners; shared world/shop layout.\n");
            AssetDatabase.Refresh();
        }
    }
}
