using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static class WebBuild
    {
        const string MusicPath="Assets/Scrapshift/Resources/Music/NeonSalvage.wav";
        [Serializable] sealed class AudioReceipt
        {
            public string path,sourceSha256Before,sourceSha256After,importerBefore,importerDuring,importerAfter,error;
            public bool music,changed,restored,sourceUnchanged;
            [NonSerialized] public AudioImporter importer;
            [NonSerialized] public AudioImporterSampleSettings original;
        }
        [Serializable] sealed class AudioReport
        {public string output,error,requestedCodeOptimization,previousCodeOptimization,restoredCodeOptimization;public bool originalSfxAudio,sfxVorbisCompression,restorationPassed,codeOptimizationRestored;public AudioReceipt[] entries;}
        [Serializable] sealed class PackedReceipt {public string path;public ulong bytes;}
        [Serializable] sealed class BuildReceipt
        {public string result,output,requestedCodeOptimization,previousCodeOptimization,restoredCodeOptimization;public bool codeOptimizationRestored;public int errors,warnings;public ulong bytes;public bool review,originalSfxAudio,sfxVorbisCompression;public PackedReceipt[] packedAssets;}
        static string Hash(string path)
        {using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
        static string Output(string[] args)
        {
            int i=Array.IndexOf(args,"--web-output");
            if(i<0)return "Builds/Web";
            if(i+1>=args.Length||args[i+1].StartsWith("--"))throw new ArgumentException("--web-output requires a path");
            return args[i+1];
        }
        static string RequestedCodeOptimization(string[] args)
        {
            int i=Array.IndexOf(args,"--web-code-optimization");
            if(i<0)return "BuildTimes";
            if(i+1>=args.Length||args[i+1].StartsWith("--"))throw new ArgumentException("--web-code-optimization requires a WasmCodeOptimization name");
            return args[i+1];
        }
        static string CurrentCodeOptimization()
        {
#if UNITY_WEBGL
            return UnityEditor.WebGL.UserBuildSettings.codeOptimization.ToString();
#else
            throw new InvalidOperationException("WebBuild requires the WebGL build target (use -buildTarget WebGL).");
#endif
        }
        static void ApplyCodeOptimization(string name)
        {
#if UNITY_WEBGL
            if(!Enum.TryParse<UnityEditor.WebGL.WasmCodeOptimization>(name,false,out var mode)||!Enum.IsDefined(typeof(UnityEditor.WebGL.WasmCodeOptimization),mode))
                throw new ArgumentException("Invalid Web WasmCodeOptimization: "+name);
            UnityEditor.WebGL.UserBuildSettings.codeOptimization=mode;
#else
            throw new InvalidOperationException("WebBuild requires the WebGL build target (use -buildTarget WebGL).");
#endif
        }
        static IEnumerable<string> AudioPaths()
        {
            var paths=new HashSet<string>(StringComparer.Ordinal);
            void Add(AudioClip clip){if(clip!=null){string path=AssetDatabase.GetAssetPath(clip);if(!string.IsNullOrEmpty(path))paths.Add(path);}}
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            if(bank==null)throw new Exception("Missing game sound bank");
            foreach(var entry in bank.entries)foreach(var clip in entry.variants??Array.Empty<AudioClip>())Add(clip);
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>("Assets/Scrapshift/Resources/CyberSoundLibrary.asset");
            if(library==null)throw new Exception("Missing contextual sound library");
            foreach(var entry in library.entries){Add(entry.source);Add(entry.edited);}
            foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Scrapshift/Resources/WeaponReference","Assets/Scrapshift/Resources/OriginalSupportAudio"}))
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            // Canonical preview waveforms retain their authored import settings.
            paths.RemoveWhere(path=>path.StartsWith("Assets/Scrapshift/Content/ApprovedContextAudio/",StringComparison.Ordinal));
            paths.Add(MusicPath);
            return paths.OrderBy(path=>path,StringComparer.Ordinal);
        }
        public static void Build()
        {
            var args=Environment.GetCommandLineArgs();bool review=args.Contains("--web-review"),originalSfx=args.Contains("--web-original-audio"),vorbis=args.Contains("--web-vorbis-sfx");
            string output=Output(args),requestedOptimization=RequestedCodeOptimization(args);Directory.CreateDirectory("artifacts");
            PlayerSettings.WebGL.template="PROJECT:Scrapshift";
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.initialMemorySize=256;
            PlayerSettings.WebGL.maximumMemorySize=2048;
            var receipts=new List<AudioReceipt>();BuildReport report=null;Exception failure=null;
            WebTextureImports textures=null;
            var previousExceptions=PlayerSettings.WebGL.exceptionSupport;
            string previousOptimization=null,restoredOptimization=null;bool optimizationCaptured=false,optimizationRestored=false;
            try
            {
                previousOptimization=CurrentCodeOptimization();optimizationCaptured=true;
                ApplyCodeOptimization(requestedOptimization);
                if(review&&args.Contains("--web-full-stacktraces"))PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.FullWithStacktrace;
                if(!args.Contains("--web-original-textures"))textures=WebTextureImports.Begin("artifacts/web-texture-imports.json");
                foreach(string path in AudioPaths())
                {
                    var importer=AssetImporter.GetAtPath(path) as AudioImporter;
                    if(importer==null)throw new Exception("Missing audio importer: "+path);
                    var receipt=new AudioReceipt{path=path,importer=importer,original=importer.defaultSampleSettings,music=path==MusicPath,
                        importerBefore=EditorJsonUtility.ToJson(importer),sourceSha256Before=Hash(path)};
                    // Register before mutation: a failed import must still be restored.
                    receipts.Add(receipt);
                    var settings=receipt.original;
                    if(receipt.music){settings.quality=.45f;settings.loadType=AudioClipLoadType.CompressedInMemory;}
                    else if(!originalSfx){if(vorbis)settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;}
                    receipt.changed=!settings.Equals(receipt.original);
                    // Web uses AAC and rejects per-platform overrides in this Unity version.
                    // SFX preserve load/preload/sample-rate/channel settings and their
                    // authored codec unless the explicit codec experiment is requested.
                    // The complete original struct is restored after the build.
                    if(receipt.changed){importer.defaultSampleSettings=settings;importer.SaveAndReimport();}
                    receipt.importerDuring=EditorJsonUtility.ToJson(importer);
                }
                report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes=new[]{ContentBuilder.ScenePath},locationPathName=output,
                    target=BuildTarget.WebGL,options=BuildOptions.DetailedBuildReport,
                    extraScriptingDefines=review?new[]{"SCRAPSHIFT_WEB_REVIEW"}:Array.Empty<string>()
                });
                textures?.CaptureDuring();
            }
            catch(Exception ex){failure=ex;}
            finally
            {
                try{textures?.Restore();}catch(Exception ex){failure=failure==null?ex:new AggregateException(failure,ex);}
                PlayerSettings.WebGL.exceptionSupport=previousExceptions;
                foreach(var receipt in receipts)
                {
                    try
                    {
                        var importer=AssetImporter.GetAtPath(receipt.path) as AudioImporter;
                        if(importer==null)throw new Exception("Audio importer disappeared: "+receipt.path);
                        if(receipt.changed){importer.defaultSampleSettings=receipt.original;importer.SaveAndReimport();}
                        receipt.importerAfter=EditorJsonUtility.ToJson(importer);
                        receipt.sourceSha256After=Hash(receipt.path);
                        receipt.restored=receipt.importerBefore==receipt.importerAfter;
                        receipt.sourceUnchanged=receipt.sourceSha256Before==receipt.sourceSha256After;
                        if(!receipt.restored||!receipt.sourceUnchanged)throw new Exception("Audio restoration verification failed: "+receipt.path);
                    }
                    catch(Exception ex){receipt.error=ex.ToString();failure=failure==null?ex:new AggregateException(failure,ex);}
                }
                if(optimizationCaptured)
                {
                    try
                    {
                        ApplyCodeOptimization(previousOptimization);
                        restoredOptimization=CurrentCodeOptimization();optimizationRestored=restoredOptimization==previousOptimization;
                        if(!optimizationRestored)throw new Exception("Web code optimization setting restoration failed");
                    }
                    catch(Exception ex){failure=failure==null?ex:new AggregateException(failure,ex);}
                }
                File.WriteAllText("artifacts/web-audio-imports.json",JsonUtility.ToJson(new AudioReport{output=output,originalSfxAudio=originalSfx,sfxVorbisCompression=!originalSfx&&vorbis,requestedCodeOptimization=requestedOptimization,previousCodeOptimization=previousOptimization,restoredCodeOptimization=restoredOptimization,codeOptimizationRestored=optimizationRestored,
                    error=failure?.ToString(),restorationPassed=optimizationRestored&&receipts.All(row=>row.restored&&row.sourceUnchanged),entries=receipts.ToArray()},true));
            }
            if(report!=null)
            {
                File.WriteAllText("artifacts/web-build.txt",$"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nReview: {review}\nOriginalSfxAudio: {originalSfx}\nSfxVorbisCompression: {!originalSfx&&vorbis}\nOutput: {output}\nCodeOptimization: {requestedOptimization}\nPreviousCodeOptimization: {previousOptimization}\nCodeOptimizationRestored: {optimizationRestored}\n");
                File.WriteAllLines("artifacts/web-build-messages.txt",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Warning||m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.type+": "+m.content));
                var packed=report.packedAssets.SelectMany(asset=>asset.contents).GroupBy(info=>info.sourceAssetPath).Select(group=>new PackedReceipt{path=group.Key,bytes=group.Aggregate(0UL,(sum,info)=>sum+info.packedSize)}).OrderByDescending(row=>row.bytes).ToArray();
                File.WriteAllText("artifacts/web-build.json",JsonUtility.ToJson(new BuildReceipt{result=report.summary.result.ToString(),output=output,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,
                    bytes=report.summary.totalSize,review=review,originalSfxAudio=originalSfx,sfxVorbisCompression=!originalSfx&&vorbis,requestedCodeOptimization=requestedOptimization,previousCodeOptimization=previousOptimization,restoredCodeOptimization=restoredOptimization,codeOptimizationRestored=optimizationRestored,packedAssets=packed},true));
            }
            else
            {
                File.WriteAllText("artifacts/web-build.txt","Result: Failed before build report\nOutput: "+output+"\nCodeOptimization: "+requestedOptimization+"\n"+failure);
                File.WriteAllText("artifacts/web-build-messages.txt",failure?.ToString()??"No build report");
                File.WriteAllText("artifacts/web-build.json",JsonUtility.ToJson(new BuildReceipt{result="FailedBeforeReport",output=output,review=review,
                    originalSfxAudio=originalSfx,sfxVorbisCompression=!originalSfx&&vorbis,requestedCodeOptimization=requestedOptimization,previousCodeOptimization=previousOptimization,restoredCodeOptimization=restoredOptimization,codeOptimizationRestored=optimizationRestored,packedAssets=Array.Empty<PackedReceipt>()},true));
            }
            if(failure!=null)throw new Exception("Web build/import preparation or restoration failed",failure);
            if(report==null||report.summary.result!=BuildResult.Succeeded)throw new Exception("Web build failed");
        }
    }
}
