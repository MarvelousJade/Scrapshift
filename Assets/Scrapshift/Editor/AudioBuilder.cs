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
        static void EnsurePlayerHitAudio()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(Root+"/Definitions/GameSounds.asset");
            var hurt=bank.Find(SoundCue.Hurt);
            var clips=Enumerable.Range(1,4).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Reference/Impact_"+i.ToString("00")+".wav")).ToArray();
            if(clips.Any(c=>c==null))throw new Exception("Missing player impact sound variants");
            if(hurt.variants.SequenceEqual(clips) && Mathf.Approximately(hurt.pitchVariation,.2f))return;
            hurt.variants=clips;hurt.pitchVariation=.2f;
            EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();
        }
        static void EnsureMovementUiAudio()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(Root+"/Definitions/GameSounds.asset");
            var entries=bank.entries.ToList();
            foreach(var cue in new[]{SoundCue.Step,SoundCue.UiFocus,SoundCue.UiPress,SoundCue.Spawn,SoundCue.InventoryFocus,SoundCue.Consume})
            {
                var paths=Directory.GetFiles(Root+"/Audio",cue+"_*").Where(p=>p.EndsWith(".wav")||p.EndsWith(".mp3")).OrderBy(p=>p).ToArray();
                foreach(var path in paths){var importer=(AudioImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));if(!importer.forceToMono){importer.forceToMono=true;importer.SaveAndReimport();}}
                var clips=paths.Select(p=>AssetDatabase.LoadAssetAtPath<AudioClip>(p.Replace('\\','/'))).ToArray();
                if(clips.Length==0||clips.Any(c=>c==null))throw new Exception("Missing presentation cue "+cue);
                entries.RemoveAll(e=>e.cue==cue);entries.Add(new SoundEntry{cue=cue,variants=clips,gain=cue==SoundCue.Step?.18f:.35f,cooldown=cue==SoundCue.Step?.15f:.06f,pitchVariation=cue==SoundCue.Spawn?.2f:cue==SoundCue.Step?.1f:0,priority=cue==SoundCue.Step?0:2});
            }
            bank.entries=entries.ToArray();bank.Find(SoundCue.Pickup).gain=.42f;EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();
        }
        static void EnsureFeedbackAudio()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(Root+"/Definitions/GameSounds.asset");
            var entries=bank.entries.ToList();
            void Replace(SoundCue cue,string[] names,float gain,float pitchVariation=0)
            {
                var clips=names.Select(name=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Reference/"+name)).ToArray();
                if(clips.Any(c=>c==null))throw new Exception("Missing original feedback audio for "+cue);
                entries.RemoveAll(e=>e.cue==cue);
                entries.Add(new SoundEntry{cue=cue,variants=clips,gain=gain,cooldown=0,pitchVariation=pitchVariation,priority=2});
            }
            Replace(SoundCue.CratePickup,new[]{"CratePickup_01.wav"},1);
            Replace(SoundCue.WaveClear,new[]{"WaveClear_01.wav"},1);
            Replace(SoundCue.WaveLetter,new[]{"WaveLetter_01.wav","WaveLetter_02.wav"},Mathf.Pow(10,-10f/20f),.2f);
            Replace(SoundCue.Lose,Enumerable.Range(1,9).Select(i=>"Lose_"+i.ToString("00")+".mp3").ToArray(),Mathf.Pow(10,-5f/20f));
            Replace(SoundCue.Win,new[]{"Win_01.mp3"},Mathf.Pow(10,-5f/20f));
            bank.entries=entries.ToArray();EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();
        }
        static void EnsureWaveTickAudio()
        {
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(Root+"/Definitions/GameSounds.asset");
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/WaveTick_01.wav");
            if(clip==null)throw new Exception("Missing original wave countdown tick");
            var entry=bank.Find(SoundCue.WaveTick);
            if(entry!=null&&bank.entries.Count(e=>e.cue==SoundCue.WaveTick)==1&&entry.variants!=null&&entry.variants.Length==1&&entry.variants[0]==clip&&
               Mathf.Approximately(entry.gain,1)&&Mathf.Approximately(entry.cooldown,.9f)&&
               Mathf.Approximately(entry.pitchVariation,.1f)&&entry.priority==2)return;
            var entries=bank.entries.ToList();entries.RemoveAll(e=>e.cue==SoundCue.WaveTick);
            entries.Add(new SoundEntry{cue=SoundCue.WaveTick,variants=new[]{clip},gain=1,cooldown=.9f,pitchVariation=.1f,priority=2});
            bank.entries=entries.ToArray();EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();
        }
        static void EnsureAudio()
        {
            string path = Root + "/Definitions/GameSounds.asset";
            if (File.Exists(path)) return;
            AssetDatabase.Refresh();
            var bank = ScriptableObject.CreateInstance<SoundBank>();
            bank.entries = Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>().Where(cue=>cue!=SoundCue.WaveTick&&cue!=SoundCue.CratePickup&&cue!=SoundCue.WaveLetter).Select(cue =>
            {
                var clips = Directory.GetFiles(Root + "/Audio", cue + "_*").Where(p=>p.EndsWith(".wav")||p.EndsWith(".mp3")).OrderBy(p=>p).Select(file =>
                {
                    string asset = file.Replace('\\','/'); var importer = (AudioImporter)AssetImporter.GetAtPath(asset);
                    importer.forceToMono=true;
                    var settings=importer.defaultSampleSettings; settings.preloadAudioData=true; settings.loadType=AudioClipLoadType.DecompressOnLoad;
                    settings.compressionFormat=AudioCompressionFormat.PCM; importer.defaultSampleSettings=settings; importer.SaveAndReimport();
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(asset);
                }).ToArray();
                if (clips.Length == 0) throw new Exception("Missing generated audio for " + cue);
                bool important = cue == SoundCue.Hurt || cue == SoundCue.LevelUp || cue == SoundCue.Install || cue == SoundCue.Reroll || cue == SoundCue.Win || cue == SoundCue.Lose || cue == SoundCue.WaveClear || cue == SoundCue.AchievementPopup || cue == SoundCue.AchievementPopout;
                return new SoundEntry { cue=cue, variants=clips, gain=cue==SoundCue.AchievementPopup?Mathf.Pow(10,-5f/20f):cue==SoundCue.AchievementPopout?1f:cue == SoundCue.Fire ? .28f : cue == SoundCue.Pickup ? .42f : important ? .52f : cue == SoundCue.Pulse ? .43f : .30f,
                    cooldown=cue == SoundCue.Fire ? .025f : cue == SoundCue.Pickup ? .045f : cue == SoundCue.Impact || cue == SoundCue.Kill ? .06f : .1f,
                    pitchVariation=cue==SoundCue.AchievementPopup?.2f:important ? 0 : .06f, priority=important ? 2 : 0 };
            }).ToArray();
            AssetDatabase.CreateAsset(bank,path);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            foreach (var root in scene.GetRootGameObjects())
            { var game=root.GetComponent<ArenaGame>(); if (game != null) { game.soundBank=bank; EditorUtility.SetDirty(game); } }
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
    }
}
