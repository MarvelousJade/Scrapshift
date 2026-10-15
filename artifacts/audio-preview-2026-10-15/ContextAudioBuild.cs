using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static class ContextAudioBuild
    {
        public static void Build()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>("Assets/Scrapshift/Content/Definitions/GameSounds.asset");
            foreach(var cue in new[]{SoundCue.Win,SoundCue.Lose})
            {
                var entry=bank.Find(cue);int count=cue==SoundCue.Win?1:9;
                if(entry.variants.Length!=count||entry.pitchVariation!=0||Mathf.Abs(entry.gain-Mathf.Pow(10,-5f/20))>.00001f)throw new Exception("Incorrect end-run cue settings: "+cue);
                foreach(var clip in entry.variants)
                    if(clip==null||clip.length<=0||!AssetDatabase.GetAssetPath(clip).Contains("/Reference/"))throw new Exception("Invalid imported clip: "+cue);
            }
            typeof(ContentBuilder).GetMethod("EnsureAlienEyesAudio",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,null);
            var pulse=bank.Find(SoundCue.Pulse);
            if(pulse.variants.Length!=2||Mathf.Abs(pulse.gain-1)>.00001f||Mathf.Abs(pulse.pitchVariation-.1f)>.00001f)throw new Exception("Incorrect Alien Eyes cue settings");
            for(int i=0;i<2;i++)
            {
                var clip=pulse.variants[i];
                if(clip==null||clip.length<=0||!AssetDatabase.GetAssetPath(clip).EndsWith("/Reference/Pulse_"+(i+1).ToString("00")+".wav"))throw new Exception("Incorrect Alien Eyes recording");
                var samples=new float[clip.samples*clip.channels];
                if(!clip.GetData(samples,0)||!samples.Any(s=>Mathf.Abs(s)>.00001f))throw new Exception("Unreadable or silent Alien Eyes recording");
            }
            CyberSoundBuilder.Generate();
            PlayerSettings.productName="Scrapshift";
            string output="E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/Builds/ContextAudioV1";
            Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scrapshift/Content/Scenes/Deck.unity"},locationPathName=output+"/Scrapshift.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC});
            File.WriteAllText(output+"/build-result.txt",$"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nValidated: 1 victory + 9 defeat + 2 Alien Eyes clips; reference gains and pitch variation.");
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("End-run audio build failed");
        }
    }
}
