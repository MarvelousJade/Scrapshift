using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AssetScope;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class RulesTests
    {
        [Test] public void RerollIsLimitedToOnePerDraftAndOffersAreUnique()
        {
            var r = new RunRules(23); r.Start(); r.Collect(12);
            r.Reroll(); var offers = r.Offers; r.Reroll();
            Assert.AreSame(offers, r.Offers); Assert.IsFalse(r.CanReroll);
            Assert.AreEqual(3, offers.Select(o => o.Kind).Distinct().Count());
            r.Choose(0); r.Tick(60); Assert.IsTrue(r.CanReroll);
        }
        [Test] public void DraftDoesNotOfferAnAlreadyUnlockedWeapon()
        {
            var r = new RunRules(7); r.Start(); r.Collect(12); r.Choose(0);
            for (int i=0;i<30;i++)
            {
                r.Collect(r.NextLevel); r.Reroll();
                Assert.IsFalse(r.Offers.Any(o => o.Kind == UpgradeKind.Orbit));
                r.Choose(0);
            }
        }
        [Test] public void DisplayedDamageTierMatchesAppliedGain()
        {
            var r = new RunRules(42); r.Start(); bool checkedDamage = false;
            for (int i=0;i<30;i++)
            {
                r.Collect(r.NextLevel); r.Reroll();
                int index = System.Array.FindIndex(r.Offers, o => o.Kind == UpgradeKind.Damage);
                if (index >= 0)
                {
                    float before = r.DamageScale; int tier = r.Offers[index].Tier; r.Choose(index);
                    Assert.AreEqual(before + .15f * tier, r.DamageScale, .0001f); checkedDamage = true;
                }
                else r.Choose(0);
            }
            Assert.IsTrue(checkedDamage);
        }
        [TestCase(1440, 900)]
        [TestCase(1920, 1080)]
        [TestCase(900, 900)]
        public void ResizingPreservesEntireArenaAspect(int width, int height)
        {
            var rect = PresentationLayout.Viewport(width, height);
            Assert.AreEqual(PresentationLayout.Aspect, width * rect.width / (height * rect.height), .0001f);
            Assert.GreaterOrEqual(rect.x, 0); Assert.GreaterOrEqual(rect.y, 0);
            Assert.LessOrEqual(rect.xMax, 1); Assert.LessOrEqual(rect.yMax, 1);
        }
        [Test] public void FiveWavesRequireFiveMinutesAndFourIntermissions()
        {
            var r = new RunRules(); r.Start();
            for (int wave = 1; wave <= 5; wave++)
            {
                r.Tick(59); Assert.AreEqual(RunPhase.Playing, r.Phase); Assert.AreEqual(wave, r.Wave);
                r.Tick(1);
                if (wave < 5) { Assert.AreEqual(RunPhase.Upgrade, r.Phase); r.Choose(0); }
            }
            Assert.AreEqual(RunPhase.Won, r.Phase); Assert.AreEqual(4, r.UpgradeCount);
        }
        [Test] public void PauseStopsTimeDamageAndCollection()
        {
            var r = new RunRules(); r.Start(); r.Tick(5); r.Pause(); r.Tick(100); r.Hurt(100); r.Collect(20);
            Assert.AreEqual(55, r.Remaining); Assert.AreEqual(100, r.Health); Assert.AreEqual(0, r.Scrap);
            r.Pause(); r.Tick(1); Assert.AreEqual(54, r.Remaining);
        }
        [Test] public void UpgradePreservesUnspentScrapAndWaveTime()
        {
            var r = new RunRules(); r.Start(); r.Tick(12); r.Collect(15);
            Assert.AreEqual(RunPhase.Upgrade, r.Phase); Assert.AreEqual(3, r.Scrap);
            r.Tick(500); r.Choose(0);
            Assert.AreEqual(48, r.Remaining); Assert.AreEqual(1, r.Wave); Assert.IsTrue(r.OrbitUnlocked);
        }
        [Test] public void OnlyWaveIntermissionAdvancesWave()
        {
            var r = new RunRules(); r.Start(); r.Collect(12); r.Choose(1); Assert.AreEqual(1, r.Wave);
            r.Tick(60); Assert.IsTrue(r.WaveUpgrade); r.Choose(2);
            Assert.AreEqual(2, r.Wave); Assert.AreEqual(60, r.Remaining);
        }
        [Test] public void InvalidOrRepeatedChoicesCannotGrantUpgrades()
        {
            var r = new RunRules(); r.Start(); r.Choose(0); Assert.IsFalse(r.OrbitUnlocked);
            r.Tick(60); r.Choose(-1); r.Choose(3); Assert.AreEqual(RunPhase.Upgrade, r.Phase);
            r.Choose(0); r.Choose(0); Assert.AreEqual(1, r.UpgradeCount); Assert.IsFalse(r.PulseUnlocked);
        }
        [Test] public void LethalDamageCannotBecomeAWinOnSameTick()
        {
            var r = new RunRules(); r.Start(); r.Hurt(200); r.Tick(1000);
            Assert.AreEqual(RunPhase.Lost, r.Phase); Assert.AreEqual(0, r.Health);
        }
        [Test] public void RepairCapsAtNewMaximum()
        {
            var r = new RunRules(); r.Start(); r.Collect(12); r.Choose(2);
            Assert.AreEqual(115, r.Health); Assert.AreEqual(115, r.MaxHealth);
        }
        [Test] public void RestartClearsAllProgression()
        {
            var r = new RunRules(); r.Start(); r.Collect(12); r.Choose(0); r.Kill(); r.Hurt(80); r.Start();
            Assert.AreEqual(100, r.Health); Assert.AreEqual(0, r.Kills); Assert.AreEqual(1, r.Level);
            Assert.IsFalse(r.OrbitUnlocked); Assert.IsFalse(r.PulseUnlocked); Assert.AreEqual(0, r.UpgradeCount);
        }
        [Test] public void NegativeInputsCannotHealOrReverseTime()
        {
            var r = new RunRules(); r.Start(); r.Hurt(10); r.Hurt(-50); r.Tick(-30); r.Collect(-10);
            Assert.AreEqual(90, r.Health); Assert.AreEqual(60, r.Remaining); Assert.AreEqual(0, r.Scrap);
        }
        [TestCase(50, 20, 10.5f, 5.1f)]
        [TestCase(-50, -20, -10.5f, -5.1f)]
        [TestCase(3, 2, 3, 2)]
        public void MovementStaysInsideVisibleArena(float x, float y, float ex, float ey)
        { Assert.AreEqual(new Vector2(ex, ey), RunRules.ClampPosition(new Vector2(x, y))); }
        [Test] public void SweptProjectileHitsBetweenEndpoints()
        { Assert.Less(ArenaGame.SegmentDistance(Vector2.zero, new Vector2(-1, 0), new Vector2(1, 0)), .001f); }
        [Test] public void SweptProjectileDoesNotHitBeyondSegment()
        { Assert.AreEqual(1, ArenaGame.SegmentDistance(new Vector2(2, 0), Vector2.zero, Vector2.right), .001f); }
        [Test] public void StationaryProjectileDistanceIsFinite()
        { Assert.AreEqual(1, ArenaGame.SegmentDistance(Vector2.up, Vector2.zero, Vector2.zero), .001f); }
    }

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
            Assert.NotNull(game.playerPrefab); Assert.NotNull(game.gameCamera); Assert.NotNull(game.scrapPrefab);
            Assert.AreEqual(3, game.weapons.Length); Assert.AreEqual(3, game.enemyTypes.Length);
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
