using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static class WebBuild
    {
        public static void Build()
        {
            bool review=Environment.GetCommandLineArgs().Contains("--web-review");
            PlayerSettings.WebGL.template="PROJECT:Scrapshift";
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.initialMemorySize=256;
            PlayerSettings.WebGL.maximumMemorySize=2048;
            // Focus/visibility events pause and mute explicitly; preserve the desktop setting.
            var audio=(AudioImporter)AssetImporter.GetAtPath("Assets/Scrapshift/Resources/Music/NeonSalvage.wav");
            var originalAudio=audio.defaultSampleSettings;
            var settings=originalAudio;
            settings.quality=.45f;settings.loadType=AudioClipLoadType.CompressedInMemory;
            // Web rejects per-platform overrides and performs its own AAC encoding.
            // Apply the import settings only for this build, then restore desktop imports.
            audio.defaultSampleSettings=settings;
            audio.SaveAndReimport();
            Directory.CreateDirectory("artifacts");
            BuildReport report;
            try { report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{ContentBuilder.ScenePath},locationPathName="Builds/Web",
                target=BuildTarget.WebGL,options=BuildOptions.None,
                extraScriptingDefines=review?new[]{"SCRAPSHIFT_WEB_REVIEW"}:Array.Empty<string>()
            }); }
            finally {audio.defaultSampleSettings=originalAudio;audio.SaveAndReimport();}
            File.WriteAllText("artifacts/web-build.txt",$"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nReview: {review}\n");
            File.WriteAllLines("artifacts/web-build-messages.txt",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Warning||m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.type+": "+m.content));
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Web build failed");
        }
    }
}
