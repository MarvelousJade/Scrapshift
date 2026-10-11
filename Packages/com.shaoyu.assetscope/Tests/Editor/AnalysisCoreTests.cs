using System;
using System.IO;
using NUnit.Framework;

namespace AssetScope.Tests
{
    public sealed class AnalysisCoreTests
    {
        [Test] public void TransitiveTraversalTerminatesOnCyclesAndExcludesRoot()
        {
            var graph = new DependencyIndex(); graph.Add("a", new[] { "b" }); graph.Add("b", new[] { "a", "c" });
            CollectionAssert.AreEqual(new[] { "b", "c" }, graph.Transitive("a"));
            CollectionAssert.AreEqual(new[] { "a", "b" }, graph.Transitive("c", true));
        }
        [Test] public void ShortestChainPrefersFewestEdges()
        {
            var graph = new DependencyIndex(); graph.Add("a", new[] { "b", "c" }); graph.Add("b", new[] { "d" }); graph.Add("c", new[] { "target" }); graph.Add("d", new[] { "target" });
            CollectionAssert.AreEqual(new[] { "a", "c", "target" }, graph.ShortestChain("a", "target"));
            Assert.That(graph.ShortestChain("target", "a"), Is.Empty);
            CollectionAssert.AreEqual(new[] { "a" }, graph.ShortestChain("a", "a"));
        }
        [TestCase("Assets/Demo/a", false)]
        [TestCase("Assets/DemoOther/a", true)]
        [TestCase("Assets/Keep/a", true)]
        public void ExclusionsRespectFolderBoundaries(string path, bool expected) => Assert.That(new ScanOptions { excludedFolders = " Assets/Demo/ ; Assets/Other " }.Includes(path), Is.EqualTo(expected));

        [Test] public void CacheChecksHashPathAndOptionsAndCopiesData()
        {
            var cache = new AnalysisCache();
            cache.Put(new AssetAnalysis { guid = "g", assetPath = "a", dependencyHash = "h", signature = "s", dependencies = new[] { "b" } });
            Assert.That(cache.TryGet("g", "a", "h", "s", out var value), Is.True);
            value.dependencies[0] = "mutated";
            Assert.That(cache.TryGet("g", "a", "h", "s", out var fresh), Is.True);
            Assert.That(fresh.dependencies[0], Is.EqualTo("b"));
            Assert.That(cache.TryGet("g", "moved", "h", "s", out _), Is.False);
            Assert.That(cache.TryGet("g", "a", "changed", "s", out _), Is.False);
            Assert.That(cache.TryGet("g", "a", "h", "different", out _), Is.False);
        }
        [Test] public void CacheClearAdvancesGeneration()
        {
            var cache = new AnalysisCache(); cache.Clear(); Assert.That(cache.Generation, Is.EqualTo(1));
        }
        [Test] public void CachePersistsAndIgnoresCorruption()
        {
            var path = Path.Combine(Path.GetTempPath(), "assetscope-cache-" + Guid.NewGuid() + ".json");
            try
            {
                var cache = new AnalysisCache(path); cache.Put(new AssetAnalysis { guid = "g", assetPath = "a", dependencyHash = "h", signature = "s" }); cache.Save();
                Assert.That(new AnalysisCache(path).TryGet("g", "a", "h", "s", out _), Is.True);
                File.WriteAllText(path, "{invalid"); var broken = new AnalysisCache(path);
                Assert.That(broken.Count, Is.Zero); Assert.That(broken.Warning, Is.Not.Empty);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }
        [Test] public void CsvEscapesQuotesNewlinesAndFormulaPrefixes()
        {
            var csv = ReportWriter.FindingsCsv(new ScanReport { state = "Complete", folder = "Assets", findings = new[] { new Finding { assetPath = "=SUM(1,2)", message = "line \"one\"\nline two" } } });
            StringAssert.Contains("\"'=SUM(1,2)\"", csv);
            StringAssert.Contains("\"line \"\"one\"\"\nline two\"", csv);
        }
        [Test] public void EmptyCsvStillDisclosesIncompleteScanState()
        {
            var csv = ReportWriter.FindingsCsv(new ScanReport { state = "Cancelled", processed = 0, total = 50 });
            StringAssert.Contains("Cancelled", csv); StringAssert.Contains("0/50", csv);
        }
        [Test] public void SavedGuidFallbackSkipsAssignedBuiltinAndTextContent()
        {
            var text = "%YAML 1.1\n  note: |\n    fake: {fileID: 1, guid: 22222222222222222222222222222222, type: 2}\n" +
                "  assigned: {fileID: 1, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 2}\n" +
                "  builtin: {fileID: 1, guid: 0000000000000000f000000000000000, type: 0}\n" +
                "  target: {fileID: 1, guid: 22222222222222222222222222222222, type: 2}\n";
            var findings = SavedReferenceValidator.InspectText("a.asset", text, guid => guid.StartsWith("a"));
            Assert.That(findings.Length, Is.EqualTo(1)); Assert.That(findings[0].propertyPath, Is.EqualTo("target"));
        }
        [TestCase("Complete", false, 0)]
        [TestCase("Complete", true, 2)]
        [TestCase("Cancelled", false, 3)]
        [TestCase("CompleteWithErrors", false, 3)]
        public void BatchExitCodesDistinguishFindingsAndIncompleteScans(string state, bool fail, int code) =>
            Assert.That(ReportWriter.ExitCode(new ScanReport { state = state, findings = new[] { new Finding() } }, fail), Is.EqualTo(code));

        [Test] public void SavedSearchRoundTripsAndReplacesByName()
        {
            var path = Path.Combine(Path.GetTempPath(), "assetscope-search-" + Guid.NewGuid() + ".json");
            try
            {
                var presets = new SearchPresets(path); presets.Save(new SearchPreset { name = "Demo", folder = "Assets", query = "old" });
                presets.Save(new SearchPreset { name = "Demo", folder = "Assets/Demo", query = "t:Prefab", options = new ScanOptions { inspectSavedScenes = true } });
                var read = new SearchPresets(path); Assert.That(read.Names.Length, Is.EqualTo(1));
                Assert.That(read.Get("Demo").query, Is.EqualTo("t:Prefab")); Assert.That(read.Get("Demo").options.inspectSavedScenes, Is.True);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }
    }
}
