using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureCombatPresentation()
        {
            if (File.Exists(Root + "/CombatPresentationV1.txt")) return;
            foreach (var pair in new[] { new[]{"Deck","#5C5346"}, new[]{"Grid","#4F473B"}, new[]{"Armor","#302B27"} })
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/" + pair[0] + ".mat");
                material.color = Hex(pair[1]); EditorUtility.SetDirty(material);
            }
            var scene = EditorSceneManager.OpenScene(ScenePath);
            foreach (var root in scene.GetRootGameObjects())
            {
                var camera = root.GetComponent<Camera>();
                if (camera != null) { camera.backgroundColor = Hex("#40392F"); EditorUtility.SetDirty(camera); }
            }
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            File.WriteAllText(Root + "/CombatPresentationV1.txt", "Muted salvage-yard floor for clearer attack cues and reference-style HUD.\n");
            AssetDatabase.Refresh();
        }
    }
}
