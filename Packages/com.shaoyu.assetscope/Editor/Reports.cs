using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetScope
{
    public static class ReportWriter
    {
        public static string FindingsCsv(ScanReport report)
        {
            var csv = new StringBuilder("ScanState,Folder,Code,Severity,Asset,Hierarchy,Component,Property,Message\r\n");
            var findings = report.findings ?? Array.Empty<Finding>();
            // Even an empty/partial scan needs its completion status in the CSV.
            if (findings.Length == 0) findings = new[] { new Finding { code = "SCAN_SUMMARY", severity = "Info", message = $"{report.processed}/{report.total} assets processed; no findings recorded. Check ScanState before interpreting this result." } };
            foreach (var finding in findings)
                csv.AppendLine(string.Join(",", new[] { report.state, report.folder, finding.code, finding.severity,
                    finding.assetPath, finding.hierarchyPath, finding.componentType, finding.propertyPath, finding.message }.Select(Cell)));
            return csv.ToString();
        }

        static string Cell(string value)
        {
            value = value ?? "";
            var trimmed = value.TrimStart();
            if (trimmed.Length > 0 && "=+-@".Contains(trimmed[0])) value = "'" + value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public static void WriteAtomic(string path, string content)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, content, new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        public static void WriteJson(string path, ScanReport report) => WriteAtomic(path, JsonUtility.ToJson(report, true));
        public static int ExitCode(ScanReport report, bool failOnFindings) => report.state != ScanState.Complete.ToString() ? 3 : failOnFindings && report.findings.Length > 0 ? 2 : 0;
    }

    public static class AssetScopeBatch
    {
        public static void Validate()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("AssetScopeBatch.Validate is only for a separate batch-mode Editor.");
            var args = Environment.GetCommandLineArgs();
            string Value(string key, string fallback)
            {
                var index = Array.IndexOf(args, key);
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
            }
            try
            {
                var options = new ScanOptions { inspectSavedScenes = args.Contains("-assetScopeScenes"), useCache = false, excludedFolders = Value("-assetScopeExclude", "") };
                var session = ScanSession.Create(Value("-assetScopeFolder", "Assets"), options);
                while (session.State == ScanState.Running) session.Step();
                var report = ScanReport.From(session);
                var output = Value("-assetScopeReport", "artifacts/assetscope-report.json");
                ReportWriter.WriteJson(output, report);
                ReportWriter.WriteAtomic(Path.ChangeExtension(output, ".csv"), ReportWriter.FindingsCsv(report));
                Debug.Log($"AssetScope: {report.state}, {report.processed}/{report.total} assets, {report.findings.Length} findings. {output}");
                EditorApplication.Exit(ReportWriter.ExitCode(report, args.Contains("-assetScopeFailOnFindings")));
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
    }
}
