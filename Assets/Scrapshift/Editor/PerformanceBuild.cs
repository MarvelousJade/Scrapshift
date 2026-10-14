using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Scrapshift.Editor
{
    public static class PerformanceBuild
    {
        public static void Build()
        {
            Directory.CreateDirectory("Builds/Performance");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ContentBuilder.ScenePath },
                locationPathName = "Builds/Performance/Scrapshift.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0)
                throw new Exception("Performance player build failed.");
        }
    }
}
