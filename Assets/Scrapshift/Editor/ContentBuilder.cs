using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using AssetScope;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        public const string Root = "Assets/Scrapshift/Content";
        public const string ScenePath = Root + "/Scenes/Deck.unity";
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        static Mesh quad, circle, octagon, triangle, ring;

        [MenuItem("Scrapshift/Generate Game Content")]
        public static void Generate()
        {
            // Safe to invoke again: authored content is never overwritten by the generator.
            if (File.Exists(ScenePath)) { UpgradePresentation(); EnsureWeaponArt(); EnsureAudio(); EnsureReference(); EnsurePlayerHitAudio(); EnsureMovementUiAudio(); EnsureWaveTickAudio(); EnsureFeedbackAudio(); EnsureOriginalArt(); EnsureWardrobe(); EnsureCombatPresentation(); EnsureSpringArt(); EnsureModularAppearance(); EnsureWideLegs(); EnsureArenaGeometry(); EnsureYardPresentation(); EnsureSilverLegs(); EnsureStatIcons(); EnsureRoundCanopy(); EnsureStarterProfiles(); EnsureInspectionUI(); return; }
            foreach (var sub in new[] { "Meshes", "Materials", "Prefabs", "Definitions", "Scenes" }) Directory.CreateDirectory(Root + "/" + sub);
            AssetDatabase.Refresh();
            quad = Polygon("Quad", 4, 45, 1.41421356f);
            circle = Polygon("Circle", 32, 0, 1);
            octagon = Polygon("Octagon", 8, 22.5f, 1);
            triangle = Polygon("Triangle", 3, 90, 1);
            ring = Ring();
            AddMaterial("Ink", "#101D2A"); AddMaterial("Deck", "#152633"); AddMaterial("Grid", "#1E3543");
            AddMaterial("Armor", "#385564"); AddMaterial("Mint", "#7CF8C5"); AddMaterial("MintShade", "#3C9D91");
            AddMaterial("Coral", "#FF736A"); AddMaterial("CoralShade", "#AD4256"); AddMaterial("Amber", "#FFC660");
            AddMaterial("Purple", "#B196F3"); AddMaterial("White", "#E4F5EF"); AddMaterial("Steel", "#7295A4");
            var player = Robot("RepairBot", "Mint", "MintShade", 0);
            var scout = Robot("Scout", "Coral", "CoralShade", 1);
            var runner = Robot("Runner", "Amber", "CoralShade", 2);
            var tank = Robot("Bulwark", "Purple", "Armor", 3);
            var rivet = SimplePrefab("Rivet", quad, "Mint", new Vector2(.4f, .12f));
            var blade = SimplePrefab("OrbitBlade", triangle, "Amber", new Vector2(.65f, .4f));
            var pulse = SimplePrefab("PulseRing", ring, "MintShade", Vector2.one);
            var scrap = SimplePrefab("Scrap", octagon, "Amber", new Vector2(.20f, .20f));
            var spark = SimplePrefab("Spark", quad, "White", new Vector2(.11f, .06f));
            var weapons = new[] {
                Weapon("RivetGun", "Rivet gun", "Automatic bolts target the nearest drone.", WeaponKind.Rivet, 12, .35f, 10, rivet, "Mint"),
                Weapon("OrbitBlades", "Orbit blades", "Two blades circle the robot.", WeaponKind.Orbit, 12, .30f, 1.5f, blade, "Amber"),
                Weapon("PulseCore", "Pulse core", "A periodic shockwave clears nearby drones.", WeaponKind.Pulse, 28, 3, 2.8f, pulse, "MintShade") };
            var enemies = new[] {
                Enemy("Scout", scout, 20, 1.3f, .35f, 2, "Coral"),
                Enemy("Runner", runner, 14, 2.2f, .28f, 2, "Amber"),
                Enemy("Bulwark", tank, 65, .8f, .52f, 5, "Purple") };

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camObject = new GameObject("Camera"); var camera = camObject.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 7.5f; camera.nearClipPlane = .1f; camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Hex("#0C1722");
            camera.transform.position = new Vector3(0, 0, -20); camObject.tag = "MainCamera"; camObject.AddComponent<AudioListener>();
            var arena = new GameObject("Deck art");
            Shape(arena, "Floor", quad, "Deck", 0, 0, 5, 23, 11.4f);
            for (int x = -11; x <= 11; x++) Shape(arena, "Vertical seam", quad, "Grid", x, 0, 4.9f, .014f, 11);
            for (int y = -5; y <= 5; y++) Shape(arena, "Horizontal seam", quad, "Grid", 0, y, 4.9f, 22.5f, .014f);
            Shape(arena, "Top rail", quad, "Armor", 0, 5.65f, 4.8f, 23, .10f);
            Shape(arena, "Bottom rail", quad, "Armor", 0, -5.65f, 4.8f, 23, .10f);
            Shape(arena, "Left rail", quad, "Armor", -11.45f, 0, 4.8f, .10f, 11.4f);
            Shape(arena, "Right rail", quad, "Armor", 11.45f, 0, 4.8f, .10f, 11.4f);
            for (int i = -5; i <= 5; i++)
            {
                Shape(arena, "Edge light", quad, i % 2 == 0 ? "MintShade" : "Armor", i * 2, 5.50f, 4.7f, .6f, .035f);
                Shape(arena, "Edge light", quad, i % 2 == 0 ? "MintShade" : "Armor", i * 2, -5.50f, 4.7f, .6f, .035f);
            }
            Shape(arena, "Center ring", ring, "Grid", 0, 0, 4.6f, 4, 4);
            Shape(arena, "Center cross", quad, "Armor", 0, 0, 4.5f, .65f, .025f);
            Shape(arena, "Center cross", quad, "Armor", 0, 0, 4.5f, .025f, .65f);
            for (int side = -1; side <= 1; side += 2)
            {
                var panel = new GameObject("Maintenance panel"); panel.transform.SetParent(arena.transform);
                Shape(panel, "Panel", quad, "Ink", side * 9, side * 3.8f, 4.6f, 2.5f, 1.2f);
                for (int i = 0; i < 7; i++) Shape(panel, "Vent", quad, "Grid", side * 9 - .9f + i * .3f, side * 3.8f, 4.5f, .09f, .8f);
            }
            var game = new GameObject("Scrapshift").AddComponent<ArenaGame>();
            game.playerPrefab = player; game.scrapPrefab = scrap; game.sparkPrefab = spark;
            game.weapons = weapons; game.enemyTypes = enemies; game.gameCamera = camera;
            var display = (GameObject)PrefabUtility.InstantiatePrefab(player); display.name = "Title display bot";
            display.transform.position = new Vector3(6, .2f, -1); display.transform.localScale = Vector3.one * 3.8f; display.transform.rotation = Quaternion.Euler(0, 0, -8);
            game.titleModel = display;
            PlayerSettings.companyName = "Shaoyu"; PlayerSettings.productName = "Scrapshift";
            PlayerSettings.defaultScreenWidth = 1440; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow; PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true; PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 4;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            UpgradePresentation(); EnsureWeaponArt(); EnsureAudio(); EnsureReference(); EnsurePlayerHitAudio(); EnsureMovementUiAudio(); EnsureWaveTickAudio(); EnsureFeedbackAudio(); EnsureOriginalArt(); EnsureWardrobe(); EnsureSpringArt(); EnsureModularAppearance(); EnsureWideLegs(); EnsureArenaGeometry(); EnsureYardPresentation(); EnsureSilverLegs(); EnsureStatIcons(); EnsureRoundCanopy(); EnsureStarterProfiles(); EnsureInspectionUI(); Scan(); Debug.Log("Scrapshift content generated.");
        }

        static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
        static void AddMaterial(string name, string hex)
        {
            var mat = new Material(Shader.Find("Scrapshift/FlatColor")) { name = name, color = Hex(hex) };
            AssetDatabase.CreateAsset(mat, Root + "/Materials/" + name + ".mat"); materials[name] = mat;
        }
        static Mesh Polygon(string name, int sides, float rotation, float multiplier)
        {
            var vertices = new Vector3[sides + 1]; var triangles = new int[sides * 3];
            for (int i = 0; i < sides; i++)
            {
                float angle = (rotation + 360f * i / sides) * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (.5f * multiplier);
                triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = (i + 1) % sides + 1;
            }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles }; mesh.RecalculateBounds(); mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh, Root + "/Meshes/" + name + ".asset"); return mesh;
        }
        static Mesh Ring()
        {
            const int n = 64; var vertices = new Vector3[n * 2]; var triangles = new int[n * 6];
            for (int i = 0; i < n; i++)
            {
                Vector3 p = new Vector3(Mathf.Cos(i * 2 * Mathf.PI / n), Mathf.Sin(i * 2 * Mathf.PI / n), 0);
                vertices[i * 2] = p * .5f; vertices[i * 2 + 1] = p * .48f;
                int a = i * 2, b = (i + 1) % n * 2;
                int k = i * 6; triangles[k] = a; triangles[k + 1] = b; triangles[k + 2] = a + 1;
                triangles[k + 3] = b; triangles[k + 4] = b + 1; triangles[k + 5] = a + 1;
            }
            var mesh = new Mesh { name = "Ring", vertices = vertices, triangles = triangles }; mesh.RecalculateBounds(); mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh, Root + "/Meshes/Ring.asset"); return mesh;
        }
        static GameObject Shape(GameObject parent, string name, Mesh mesh, string material, float x, float y, float z, float width, float height, float rotation = 0)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent.transform, false);
            obj.transform.localPosition = new Vector3(x, y, z); obj.transform.localScale = new Vector3(width, height, 1);
            obj.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            obj.AddComponent<MeshFilter>().sharedMesh = mesh; obj.AddComponent<MeshRenderer>().sharedMaterial = materials[material]; return obj;
        }
        static GameObject Robot(string name, string color, string shade, int type)
        {
            var root = new GameObject(name);
            Shape(root, "Shadow", circle, "Ink", .05f, -.22f, .15f, .95f, .5f);
            float size = type == 3 ? 1.2f : type == 2 ? .66f : .86f;
            Shape(root, "Left tread", quad, "Ink", -size * .43f, -.05f, 0, .23f, .60f);
            Shape(root, "Right tread", quad, "Ink", size * .43f, -.05f, 0, .23f, .60f);
            Shape(root, "Lower armor", octagon, shade, 0, -.04f, -.01f, size, size * .9f);
            Shape(root, "Main armor", type == 2 ? triangle : octagon, color, 0, .10f, -.03f, size * .88f, size * .88f);
            Shape(root, "Top glint", quad, "White", -.07f, .33f, -.04f, size * .38f, .045f);
            Shape(root, "Face recess", quad, "Ink", 0, .07f, -.05f, size * .64f, .24f);
            Shape(root, "Left eye", quad, type == 0 ? "White" : color, -.14f, .09f, -.06f, .09f, .08f);
            Shape(root, "Right eye", quad, type == 0 ? "White" : color, .14f, .09f, -.06f, .09f, .08f);
            if (type == 0)
            {
                Shape(root, "Antenna", quad, "Steel", .15f, .55f, -.02f, .04f, .2f);
                Shape(root, "Antenna light", circle, "Amber", .15f, .66f, -.04f, .12f, .12f);
                Shape(root, "Tool grip", quad, "Armor", .50f, -.12f, -.04f, .16f, .28f);
                Shape(root, "Rivet barrel", quad, "White", .55f, .07f, -.05f, .17f, .28f);
            }
            if (type == 3)
                Shape(root, "Heavy armor stripe", quad, "Ink", 0, -.30f, -.05f, .65f, .10f);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/" + name + ".prefab"); UnityEngine.Object.DestroyImmediate(root); return prefab;
        }
        static GameObject SimplePrefab(string name, Mesh mesh, string material, Vector2 scale)
        {
            var root = new GameObject(name); Shape(root, name + " visual", mesh, material, 0, 0, 0, scale.x, scale.y);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/" + name + ".prefab"); UnityEngine.Object.DestroyImmediate(root); return prefab;
        }
        static WeaponDefinition Weapon(string file, string title, string description, WeaponKind kind, float damage, float interval, float range, GameObject effect, string material)
        {
            var def = ScriptableObject.CreateInstance<WeaponDefinition>(); def.title = title; def.description = description; def.kind = kind;
            def.damage = damage; def.interval = interval; def.range = range; def.effectPrefab = effect; def.accent = materials[material];
            AssetDatabase.CreateAsset(def, Root + "/Definitions/" + file + ".asset"); return def;
        }
        static EnemyDefinition Enemy(string title, GameObject prefab, float health, float speed, float radius, int scrap, string material)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>(); def.title = title; def.prefab = prefab;
            def.health = health; def.speed = speed; def.radius = radius; def.scrap = scrap; def.color = materials[material].color;
            AssetDatabase.CreateAsset(def, Root + "/Definitions/" + title + ".asset"); return def;
        }
        [MenuItem("Scrapshift/Export AssetScope Scan")]
        public static void Scan()
        {
            var session = ScanSession.Create(Root, new ScanOptions { inspectSavedScenes = true, inspectProperties = true, useCache = false }); while (session.State == ScanState.Running) session.Step();
            Directory.CreateDirectory("artifacts"); File.WriteAllText("artifacts/assetscope-report.json", JsonUtility.ToJson(ScanReport.From(session), true));
            if (session.State != ScanState.Complete || session.Total == 0 || session.Findings.Count != 0) throw new Exception("Game content failed AssetScope validation.");
            Debug.Log($"AssetScope scanned {session.Total} game assets: zero validation findings.");
        }
        [MenuItem("Scrapshift/Build Windows Player")]
        public static void Build()
        {
            Generate(); Scan(); Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/Scrapshift.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            File.WriteAllText("artifacts/build.txt", $"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\n");
            File.WriteAllLines("artifacts/build-messages.txt",report.steps.SelectMany(s=>s.messages)
                .Where(m=>m.type==LogType.Warning||m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.type+": "+m.content));
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0) throw new Exception("Windows build failed or logged errors; inspect artifacts/raw/build.log.");
        }
    }
}

