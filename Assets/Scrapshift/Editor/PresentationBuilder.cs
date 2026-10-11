using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        // Explicit versioned migration: update our generated art in place, preserving dependency GUIDs.
        static void UpgradePresentation()
        {
            if (File.Exists(Root + "/PresentationV2.txt")) return;
            materials.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { Root }))
            { var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); materials[m.name] = m; }
            quad = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Meshes/Quad.asset");
            circle = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Meshes/Circle.asset");
            triangle = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Meshes/Triangle.asset");
            SetColor("Ink", "#29232B"); SetColor("Deck", "#AAA282"); SetColor("Grid", "#979473");
            SetColor("Armor", "#676855"); SetColor("Mint", "#B5DE72"); SetColor("MintShade", "#658D51");
            SetColor("White", "#FFF0C9"); SetColor("Coral", "#E98670"); SetColor("CoralShade", "#A94C50");
            SetColor("Amber", "#F5C966"); SetColor("Purple", "#B1A1D3");
            Creature("RepairBot", 0); Creature("Scout", 1); Creature("Runner", 2); Creature("Bulwark", 3);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != "Deck art") continue;
                for (int i = root.transform.childCount - 1; i >= 0; i--)
                {
                    var child = root.transform.GetChild(i);
                    if (child.name.Contains("seam") || child.name.Contains("Center") || child.name.Contains("Maintenance") || child.name.Contains("Edge light"))
                        Object.DestroyImmediate(child.gameObject);
                }
                var random = new System.Random(17);
                for (int i = 0; i < 95; i++)
                {
                    float x = (float)random.NextDouble() * 21 - 10.5f, y = (float)random.NextDouble() * 10 - 5;
                    Shape(root, "Dust patch", circle, "Grid", x, y, 4.9f, .12f + (float)random.NextDouble() * .3f, .07f);
                }
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Root + "/PresentationV2.txt", "Original outlined creature art; generated presentation migration v2. No reference assets imported.\n");
            AssetDatabase.Refresh();
        }
        static void SetColor(string name, string hex) { materials[name].color = Hex(hex); EditorUtility.SetDirty(materials[name]); }
        static void Oval(GameObject root, string name, string color, float x, float y, float z, float w, float h)
        {
            Shape(root, name + " outline", circle, "Ink", x, y, z, w + .10f, h + .10f);
            Shape(root, name, circle, color, x, y + .012f, z - .01f, w, h);
        }
        static void Creature(string name, int type)
        {
            var root = new GameObject(name);
            string color = type == 0 ? "White" : type == 1 ? "Coral" : type == 2 ? "Mint" : "Purple";
            float w = type == 3 ? 1.15f : type == 2 ? .6f : .76f;
            float h = type == 3 ? .95f : type == 2 ? .85f : .83f;
            Shape(root, "Ground shadow", circle, "Armor", .05f, -.42f, .2f, w + .25f, .22f);
            Oval(root, "Left foot", "CoralShade", -w * .27f, -.40f, .05f, .23f, .16f);
            Oval(root, "Right foot", "CoralShade", w * .27f, -.40f, .05f, .23f, .16f);
            if (type == 2)
            {
                Shape(root, "Left horn", triangle, "Ink", -.23f, .48f, .01f, .25f, .45f, 18);
                Shape(root, "Right horn", triangle, "Ink", .23f, .48f, .01f, .25f, .45f, -18);
            }
            if (type == 3)
                for (int i = 0; i < 5; i++) Shape(root, "Shell spike", triangle, "Ink", -.53f + i * .26f, .45f, .01f, .30f, .40f);
            Oval(root, "Body", type == 1 ? "CoralShade" : "MintShade", 0, 0, 0, w, h);
            Shape(root, "Face", circle, color, -.015f, .07f, -.03f, w * .95f, h * .91f);
            Shape(root, "Soft highlight", circle, "White", -w * .28f, .29f, -.04f, .09f, .17f, -25);
            if (type == 0)
            {
                Shape(root, "Scarf", quad, "Coral", 0, -.25f, -.07f, .68f, .14f);
                Shape(root, "Scarf tail", triangle, "CoralShade", -.43f, -.28f, -.06f, .38f, .24f, 70);
                Shape(root, "Goggle strap", quad, "CoralShade", 0, .11f, -.05f, .79f, .09f);
                Oval(root, "Left goggle", "Amber", -.18f, .12f, -.07f, .28f, .30f);
                Oval(root, "Right goggle", "Amber", .18f, .12f, -.07f, .28f, .30f);
            }
            if (type == 3)
            {
                Oval(root, "Cyclops eye", "White", 0, .13f, -.08f, .49f, .30f);
                Shape(root, "Pupil", circle, "Ink", .035f, .1f, -.11f, .13f, .18f);
            }
            else
            {
                Shape(root, "Left eye", circle, "Ink", -.15f, .1f, -.11f, .075f, .14f);
                Shape(root, "Right eye", circle, "Ink", .15f, .1f, -.11f, .075f, .14f);
                if (type != 0)
                {
                    Shape(root, "Left brow", quad, "Ink", -.15f, .22f, -.12f, .21f, .045f, -18);
                    Shape(root, "Right brow", quad, "Ink", .15f, .22f, -.12f, .21f, .045f, 18);
                }
            }
            Shape(root, "Mouth", circle, "Ink", .025f, -.13f, -.12f, type == 0 ? .12f : .25f, .09f);
            if (type > 0) Shape(root, "Tooth", triangle, "White", .03f, -.13f, -.13f, .075f, .10f, 180);
            PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/" + name + ".prefab"); Object.DestroyImmediate(root);
        }
    }
}
