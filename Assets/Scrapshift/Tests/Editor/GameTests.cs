using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AssetScope;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ContentTests
    {
        [Test] public void ActualGameContentPassesAssetScope()
        {
            var scan = ScanSession.Create(ContentBuilder.Root); while (scan.State == ScanState.Running) scan.Step();
            Assert.AreEqual(ScanState.Complete, scan.State); Assert.Greater(scan.Total, 20); Assert.IsEmpty(scan.Findings);
        }
        [Test] public void SharedMaterialHasMultipleRealPrefabReferencers()
        {
            var scan = ScanSession.Create(ContentBuilder.Root); while (scan.State == ScanState.Running) scan.Step();
            var uses = scan.Index.Referencers(ContentBuilder.Root + "/Materials/Ink.mat");
            Assert.GreaterOrEqual(uses.Count(p => p.EndsWith(".prefab")), 4);
        }
        [Test] public void WeaponsRetainSerializedEffectAndMaterialDependencies()
        {
            foreach (var path in AssetCatalog.Find("t:WeaponDefinition", ContentBuilder.Root))
            {
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
                Assert.NotNull(weapon.effectPrefab); Assert.NotNull(weapon.accent); Assert.Greater(weapon.interval, 0);
                CollectionAssert.Contains(AssetDatabase.GetDependencies(path, false), AssetDatabase.GetAssetPath(weapon.effectPrefab));
            }
            Assert.AreEqual(3, AssetCatalog.Find("t:WeaponDefinition", ContentBuilder.Root).Length);
        }
        [Test] public void SceneContainsACompleteGameConfiguration()
        {
            var scene = EditorSceneManager.OpenScene(ContentBuilder.ScenePath);
            var game = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<ArenaGame>()).Single();
            Assert.NotNull(game.playerPrefab); Assert.NotNull(game.gameCamera); Assert.NotNull(game.scrapPrefab); Assert.NotNull(game.gunPrefab);
            Assert.AreEqual(3, game.weapons.Length); Assert.NotNull(game.catalog); Assert.AreEqual(20,game.catalog.data.waves.Length); Assert.AreEqual(24,game.catalog.data.equipment.Length);
            foreach (var e in game.enemyTypes) { Assert.NotNull(e.prefab); Assert.Greater(e.health, 0); }
        }
        [Test] public void RepeatedGenerationPreservesAuthoredAssetGuids()
        {
            string path = ContentBuilder.Root + "/Definitions/RivetGun.asset";
            string guid = AssetDatabase.AssetPathToGUID(path); ContentBuilder.Generate();
            Assert.IsNotEmpty(guid); Assert.AreEqual(guid, AssetDatabase.AssetPathToGUID(path));
        }
        [Test] public void EveryRenderedPrefabPartHasMeshAndSupportedMaterial()
        {
            foreach (var path in AssetCatalog.Find("t:Prefab", ContentBuilder.Root))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>(true))
                {
                    Assert.NotNull(renderer.sharedMaterial, path); Assert.NotNull(renderer.sharedMaterial.shader, path);
                    Assert.NotNull(renderer.GetComponent<MeshFilter>().sharedMesh, path);
                }
            }
        }
    }
}
