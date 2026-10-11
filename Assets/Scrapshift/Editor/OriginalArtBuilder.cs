using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        public const string ArtRoot = Root + "/OriginalArt";

        static Rect[] Grid(int columns, params int[] rows)
        {
            var result = new Rect[columns * (rows.Length - 1)];
            for (int y = 0; y < rows.Length - 1; y++)
                for (int x = 0; x < columns; x++)
                    result[y * columns + x] = new Rect(x * 1536f / columns, rows[y],
                        1536f / columns, rows[y + 1] - rows[y]);
            return result;
        }

        // Rectangles use source-image coordinates (top left). Slicing never rewrites PNG pixels.
        static Sprite[] ImportSheet(string file, Rect[] regions, float ppu, bool trim = true, bool gunPivot = false)
        {
            string path = ArtRoot + "/" + file + "-v1.png";
            var source = new Texture2D(2, 2);
            if (!source.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException(path);
            var meta = new SpriteMetaData[regions.Length];
            for (int i = 0; i < regions.Length; i++)
            {
                Rect r = regions[i];
                r.y = source.height - r.y - r.height;
                if (trim)
                {
                    int minX = (int)r.xMax, minY = (int)r.yMax, maxX = -1, maxY = -1;
                    for (int y = (int)r.y; y < (int)r.yMax; y++)
                        for (int x = (int)r.x; x < (int)r.xMax; x++)
                            if (source.GetPixel(x, y).a > .12f)
                            { minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y); maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y); }
                    if (maxX >= 0) r = Rect.MinMaxRect(Mathf.Max(r.x, minX - 2), Mathf.Max(r.y, minY - 2),
                        Mathf.Min(r.xMax, maxX + 3), Mathf.Min(r.yMax, maxY + 3));
                }
                meta[i] = new SpriteMetaData { name = file + "_" + i.ToString("D2"), rect = r,
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(.5f, gunPivot && i < 4 ? .69f : .5f) };
            }
            UnityEngine.Object.DestroyImmediate(source);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
#pragma warning disable CS0618
            importer.spritesheet = meta;
#pragma warning restore CS0618
            importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length != regions.Length) throw new InvalidOperationException("Incomplete sprite sheet: " + path);
            return sprites;
        }

        static void EnsureOriginalArt()
        {
            if (File.Exists(Root + "/OriginalArtV1.txt")) return;
            AssetDatabase.Refresh();
            var hero = ImportSheet("hero", Grid(4, 0, 512, 1024), 320, false);
            var enemyRects = new Rect[18];
            int[][] edges = { new[] { 0, 265, 525, 780, 1075, 1290, 1536 },
                new[] { 0, 265, 515, 790, 1070, 1280, 1536 },
                new[] { 0, 290, 515, 780, 1060, 1280, 1536 } };
            int[] ys = { 0, 378, 660, 1024 };
            for (int y = 0; y < 3; y++) for (int x = 0; x < 6; x++)
                enemyRects[y * 6 + x] = new Rect(edges[y][x], ys[y], edges[y][x + 1] - edges[y][x], ys[y + 1] - ys[y]);
            var enemies = ImportSheet("enemies", enemyRects, 260);
            var gearRects = Grid(6, 0, 341, 683, 1024);
            int[] gearEdges = { 0, 260, 510, 775, 1040, 1280, 1536 };
            for (int x = 0; x < 6; x++) gearRects[x] = new Rect(gearEdges[x], 0, gearEdges[x + 1] - gearEdges[x], 341);
            var gear = ImportSheet("equipment", gearRects, 220, true, true);
            var items = ImportSheet("items", Grid(6, 0, 275, 515, 748, 1024), 220);
            var stats = ImportSheet("stats", Grid(4, 0, 512, 1024), 320);
            var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root + "/Definitions/ReferenceCatalog.asset");
            string[] enemyIds = { "baby_alien", "bruiser", "buffer", "charger", "chaser", "evil_mob",
                "fin_alien", "helmet_alien", "horned_bruiser", "horned_charger", "junkie", "looter",
                "predator", "pursuer", "slasher", "spawner", "spitter", "tree" };
            for (int i = 0; i < enemyIds.Length; i++) catalog.Enemy(enemyIds[i]).image = enemies[i];
            string[] weaponIds = { "pistol", "smg", "double_barrel_shotgun", "laser_gun", "knife", "spear" };
            foreach (var w in catalog.data.equipment) w.image = gear[Array.IndexOf(weaponIds, w.id)];
            string[] itemIds = { "bat", "beanie", "blindfold", "blood_leech", "broken_mouth", "cake",
                "claw_tree", "coffee", "cyclops_worm", "decomposing_flesh", "defective_steroids", "fertilizer",
                "fresh_meat", "gambling_token", "glasses", "goat_skull", "gummy_berserker", "head_injury",
                "hedgehog", "helmet", "injection", "insanity", "jelly", "leather_vest" };
            foreach (var item in catalog.data.items) item.image = items[Array.IndexOf(itemIds, item.id)];
            string[] statIds = { "percent_damage", "attack_speed", "max_hp", "crit_chance", "armor", "hp_regeneration", "harvesting", "speed" };
            foreach (var u in catalog.data.upgrades) u.image = stats[Array.IndexOf(statIds, u.stat)];
            catalog.scrapShapes = gear.Skip(6).Take(6).ToArray();
            catalog.repairPickup = gear[12];
            catalog.muzzleFrames = gear.Skip(13).Take(3).ToArray();
            catalog.warning = gear[16];
            catalog.shadow = gear[17];
            EditorUtility.SetDirty(catalog);

            var root = new GameObject("Scavenger");
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = hero[0]; renderer.sharedMaterial = catalog.spriteMaterial; renderer.sortingOrder = 10;
            var animation = root.AddComponent<SpriteFrameAnimator>();
            animation.idleFrames = hero.Take(4).ToArray(); animation.moveFrames = hero.Skip(4).ToArray();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/ScavengerAnimated.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var game = scene.GetRootGameObjects().Select(o => o.GetComponent<ArenaGame>()).First(g => g != null);
            game.playerPrefab = prefab;
            if (game.titleModel != null) UnityEngine.Object.DestroyImmediate(game.titleModel);
            game.titleModel = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            game.titleModel.name = "Animated title scavenger";
            game.titleModel.transform.position = new Vector3(6, .2f, -1);
            game.titleModel.transform.localScale = Vector3.one * 3.8f;
            EditorUtility.SetDirty(game);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Root + "/OriginalArtV1.txt", "Five original AI-generated sheets; serialized sprites; reference audio retained.\n");
            AssetDatabase.Refresh();
        }
    }
}
