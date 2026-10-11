using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureArenaGeometry()
        {
            if(File.Exists(Root+"/ArenaGeometryV1.txt"))return;
            var scene=EditorSceneManager.OpenScene(ScenePath);
            foreach(var root in scene.GetRootGameObjects())
            {
                if(root.name!="Deck art")continue;
                foreach(Transform child in root.transform)
                {
                    Vector3 p=child.localPosition,s=child.localScale;
                    switch(child.name)
                    {
                        case "Floor": s.x=20.48f;s.y=15.36f;break;
                        case "Top rail": p.y=7.73f;s.x=20.68f;break;
                        case "Bottom rail": p.y=-7.73f;s.x=20.68f;break;
                        case "Left rail": p.x=-10.29f;s.y=15.56f;break;
                        case "Right rail": p.x=10.29f;s.y=15.56f;break;
                        case "Dust patch": p.x*=20.48f/23;p.y*=15.36f/11.4f;break;
                    }
                    child.localPosition=p;child.localScale=s;
                }
            }
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(Root+"/ArenaGeometryV1.txt","Base reference map 20.48 x 15.36 world units. Runtime gameplay camera half-height 5.4 follows player within padded map bounds; menus retain authored framing.\n");
            AssetDatabase.Refresh();AssetDatabase.SaveAssets();
        }
    }
}
