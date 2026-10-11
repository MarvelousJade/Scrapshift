using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace AssetScope.Tests
{
    public sealed class CoreTests
    {
        [TestCase("Assets/Demo/a.prefab", "Assets/Demo", true)]
        [TestCase("Assets/DemoOther/a.prefab", "Assets/Demo", false)]
        [TestCase("Assets/Demo", "Assets/Demo/", true)]
        [TestCase("Packages/a.prefab", "Assets", false)]
        public void FolderBoundaryIsExact(string path, string folder, bool expected) =>
            Assert.That(AssetCatalog.IsInScope(path, folder), Is.EqualTo(expected));

        [Test] public void DirectEdgesExcludeSelfAndDuplicates()
        {
            var index = new DependencyIndex();
            index.Add("a", new[] { "a", "b", "b" });
            CollectionAssert.AreEqual(new[] { "b" }, index.Dependencies("a"));
            CollectionAssert.AreEqual(new[] { "a" }, index.Referencers("b"));
            Assert.That(index.Referencers("a"), Is.Empty);
        }

        [Test] public void ReplacingAssetRemovesOldReverseEdges()
        {
            var index = new DependencyIndex();
            index.Add("prefab", new[] { "oldMaterial" });
            index.Add("prefab", new[] { "newMaterial" });
            Assert.That(index.Referencers("oldMaterial"), Is.Empty);
            CollectionAssert.AreEqual(new[] { "prefab" }, index.Referencers("newMaterial"));
        }

        [Test] public void ReturnedEdgesCannotMutateIndex()
        {
            var index = new DependencyIndex(); index.Add("a", new[] { "b" });
            index.Dependencies("a")[0] = "corrupt";
            Assert.That(index.Dependencies("a")[0], Is.EqualTo("b"));
        }

        [Test] public void CancellationPreventsFurtherReads()
        {
            int reads = 0;
            var scan = new ScanSession(new[] { "a", "b" }, _ => { reads++; return Array.Empty<string>(); }, _ => new List<Finding>());
            scan.Step(); scan.Cancel(); scan.Step();
            Assert.That(reads, Is.EqualTo(1));
            Assert.That(scan.State, Is.EqualTo(ScanState.Cancelled));
            Assert.That(scan.Processed, Is.EqualTo(1));
        }

        [Test] public void InvalidationStopsRunningScan()
        {
            var scan = NewSession("a"); scan.Invalidate(); scan.Step();
            Assert.That(scan.State, Is.EqualTo(ScanState.Stale)); Assert.That(scan.Processed, Is.Zero);
        }

        [Test] public void CompletedResultsCanBecomeStale()
        {
            var scan = NewSession("a"); scan.Step(); scan.Invalidate();
            Assert.That(scan.State, Is.EqualTo(ScanState.Stale));
        }

        [Test] public void ExceptionsProduceFailedPartialReport()
        {
            var scan = new ScanSession(new[] { "bad" }, _ => throw new InvalidOperationException("fixture failure"), _ => new List<Finding>());
            scan.Step(); scan.Step();
            Assert.That(scan.State, Is.EqualTo(ScanState.Failed));
            StringAssert.Contains("fixture failure", scan.Error);
            Assert.That(scan.Processed, Is.Zero);
        }

        [Test] public void EmptyScopeCompletes()
        {
            var scan = NewSession(); scan.Step(); Assert.That(scan.State, Is.EqualTo(ScanState.Complete));
        }

        [Test] public void ReportPreservesPartialStatusThroughJson()
        {
            var scan = NewSession("a", "b"); scan.Step(); scan.Cancel();
            var report = JsonUtility.FromJson<ScanReport>(JsonUtility.ToJson(ScanReport.From(scan)));
            Assert.That(report.state, Is.EqualTo("Cancelled"));
            Assert.That(report.processed, Is.EqualTo(1)); Assert.That(report.total, Is.EqualTo(2));
            StringAssert.Contains("Scenes", report.scope);
        }

        [Test] public void StaleReportRetainsTheScannedFolder()
        {
            var scan = new ScanSession(new[] { "Assets/Original/a" }, _ => Array.Empty<string>(), _ => new List<Finding>(), "Assets/Original");
            scan.Step(); scan.Invalidate();
            var report = ScanReport.From(scan);
            Assert.That(report.folder, Is.EqualTo("Assets/Original"));
            Assert.That(report.state, Is.EqualTo("Stale"));
        }

        static ScanSession NewSession(params string[] paths) => new ScanSession(paths, _ => Array.Empty<string>(), _ => new List<Finding>());
    }
}
