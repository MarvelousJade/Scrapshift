using System;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    // Opt-in --audio-check <folder>; checks real mixer output, not subjective listening quality.
    public sealed class AudioCheck : MonoBehaviour
    {
        GameAudio audioPlayer;
        string folder;
        float start, next, peak;
        int cue, nonSilentFrames;
        bool muteChecked, mutePassed;
        readonly float[] samples = new float[1024];
        readonly bool[] audible = new bool[Enum.GetValues(typeof(SoundCue)).Length];
        public void Begin(GameAudio player, string output)
        {
            audioPlayer=player; folder=output; Directory.CreateDirectory(folder);
            audioPlayer.SetMuted(false); start=Time.unscaledTime; next=start+.5f;
        }
        void Update()
        {
            if (audioPlayer == null) return;
            float now=Time.unscaledTime;
            AudioListener.GetOutputData(samples,0);
            float framePeak=0;
            foreach (float sample in samples) framePeak=Mathf.Max(framePeak,Mathf.Abs(sample));
            peak=Mathf.Max(peak,framePeak);
            if (framePeak>.00001f) { nonSilentFrames++; if (cue>0 && cue<=audible.Length) audible[cue-1]=true; }
            if (now>=next && cue<audible.Length)
            { audioPlayer.Play((SoundCue)cue); cue++; next=now+1.7f; }
            if (cue==audible.Length && now>next && !muteChecked)
            {
                audioPlayer.SetMuted(true); int before=audioPlayer.Played; audioPlayer.Play(SoundCue.Fire);
                mutePassed=audioPlayer.Played==before; muteChecked=true; next=now+.3f;
            }
            if (!muteChecked || now<next) return;
            bool passed=Array.TrueForAll(audible,a=>a) && mutePassed && audioPlayer.PeakVoices<=GameAudio.VoiceCount && peak<1;
            File.WriteAllText(Path.Combine(folder,"audio-runtime.json"),JsonUtility.ToJson(new Report
            { passed=passed, cues=cue, played=audioPlayer.Played, nonSilentFrames=nonSilentFrames, sampledPeak=peak,
                peakVoices=audioPlayer.PeakVoices, mutePassed=mutePassed, audibleCues=audible, seconds=now-start },true));
            enabled=false; Application.Quit(passed ? 0 : 1);
        }
        [Serializable] sealed class Report
        {
            public bool passed, mutePassed; public bool[] audibleCues;
            public int cues, played, nonSilentFrames, peakVoices; public float sampledPeak, seconds;
            public string scope="Isolated runtime cue playback with sampled Unity mixer output and mute suppression. Not continuous peak capture, hearing verification, or busy-combat mix certification.";
        }
    }
}
