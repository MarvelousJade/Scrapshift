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
        static void EnsureAudio()
        {
            string path = Root + "/Definitions/GameSounds.asset";
            if (File.Exists(path)) return;
            AssetDatabase.Refresh();
            var bank = ScriptableObject.CreateInstance<SoundBank>();
            bank.entries = Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>().Select(cue =>
            {
                var clips = Directory.GetFiles(Root + "/Audio", cue + "_*.wav").OrderBy(p=>p).Select(file =>
                {
                    string asset = file.Replace('\\','/'); var importer = (AudioImporter)AssetImporter.GetAtPath(asset);
                    importer.forceToMono=true;
                    var settings=importer.defaultSampleSettings; settings.preloadAudioData=true; settings.loadType=AudioClipLoadType.DecompressOnLoad;
                    settings.compressionFormat=AudioCompressionFormat.PCM; importer.defaultSampleSettings=settings; importer.SaveAndReimport();
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(asset);
                }).ToArray();
                if (clips.Length == 0) throw new Exception("Missing generated audio for " + cue);
                bool important = cue == SoundCue.Hurt || cue == SoundCue.LevelUp || cue == SoundCue.Install || cue == SoundCue.Reroll || cue == SoundCue.Win || cue == SoundCue.Lose || cue == SoundCue.WaveClear;
                return new SoundEntry { cue=cue, variants=clips, gain=cue == SoundCue.Fire ? .28f : cue == SoundCue.Pickup ? .22f : important ? .52f : cue == SoundCue.Pulse ? .43f : .30f,
                    cooldown=cue == SoundCue.Fire ? .025f : cue == SoundCue.Pickup ? .045f : cue == SoundCue.Impact || cue == SoundCue.Kill ? .06f : .1f,
                    pitchVariation=important ? 0 : .06f, priority=important ? 2 : 0 };
            }).ToArray();
            AssetDatabase.CreateAsset(bank,path);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            foreach (var root in scene.GetRootGameObjects())
            { var game=root.GetComponent<ArenaGame>(); if (game != null) { game.soundBank=bank; EditorUtility.SetDirty(game); } }
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
    }
}
