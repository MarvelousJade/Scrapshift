using UnityEngine;

namespace Scrapshift
{
    // Bounded voices: twelve combat, one pickup, three notification/damage voices.
    public sealed class GameAudio : MonoBehaviour
    {
        public const int CombatVoices = 12, VoiceCount = 16;
        public SoundBank Bank { get; private set; }
        public int Played { get; private set; }
        public int Suppressed { get; private set; }
        public int PeakVoices { get; private set; }
        public bool Muted { get; private set; }
        public float Volume { get; private set; } = .7f;
        readonly AudioSource[] voices = new AudioSource[VoiceCount];
        readonly float[] gains = new float[VoiceCount], started = new float[VoiceCount];
        float[] last;
        int[] previous;
        float duckUntil, lastPickup;
        int pickupChain;

        public void Initialize(SoundBank bank, bool muted)
        {
            Bank = bank; Muted = muted;
            Volume = Mathf.Clamp01(PlayerPrefs.GetFloat("sfxVolume", .7f));
            int count = System.Enum.GetValues(typeof(SoundCue)).Length;
            last = new float[count]; previous = new int[count];
            for (int i=0;i<count;i++) { last[i] = -100; previous[i] = -1; }
            for (int i=0;i<VoiceCount;i++)
            {
                var obj = new GameObject("SFX voice " + i); obj.transform.SetParent(transform);
                var voice = obj.AddComponent<AudioSource>(); voice.playOnAwake = false;
                voice.spatialBlend = 0; voice.priority = i >= CombatVoices ? 32 : 160; voices[i] = voice;
            }
        }
        public static int VariantIndex(int count, int previous, int roll)
        {
            if (count <= 1) return 0;
            int selected = Mathf.Abs(roll % (count - 1));
            return previous < 0 ? Mathf.Abs(roll % count) : selected >= previous ? selected + 1 : selected;
        }
        public void Play(SoundCue cue, Vector2 position = default, AudioClip[] overrideClips = null, float? overrideGain = null, float? overridePitchVariation = null, bool independent = false)
        {
            if (Bank == null || Muted || Volume <= 0) return;
            var entry = Bank.Find(cue); int key = (int)cue; float now = Time.unscaledTime;
            if (entry == null || entry.variants == null || entry.variants.Length == 0) return;
            if (!independent && now - last[key] < entry.cooldown) { Suppressed++; return; }
            bool important = entry.priority >= 2;
            // One pickup voice survives dense combat without stealing damage/unlock cues.
            bool pickup=cue==SoundCue.Pickup;
            int min = pickup?CombatVoices:important ? CombatVoices+1 : 0, max = pickup?CombatVoices+1:important ? VoiceCount : CombatVoices;
            int index = -1;
            for (int i=min;i<max;i++) if (!voices[i].isPlaying) { index=i; break; }
            if (index < 0)
            {
                if (!important&&!pickup) { Suppressed++; return; }
                index = min;
                for (int i=min+1;i<max;i++) if (started[i] < started[index]) index = i;
            }
            var clips = overrideClips != null && overrideClips.Length > 0 ? overrideClips : entry.variants;
            int variant = VariantIndex(clips.Length, previous[key] % clips.Length, Random.Range(0, 100000));
            previous[key] = variant; last[key] = now;
            var voice = voices[index]; voice.Stop(); voice.clip = clips[variant];
            float pitchVariation=overridePitchVariation??entry.pitchVariation;
            voice.pitch = 1 + Random.Range(-pitchVariation, pitchVariation);
            if (cue == SoundCue.Pickup)
            {
                pickupChain = now - lastPickup < .45f ? (pickupChain + 1) % 5 : 0;
                lastPickup = now; voice.pitch *= Mathf.Pow(2, pickupChain/12f);
            }
            voice.panStereo = important ? 0 : Mathf.Clamp(position.x / 18, -.6f, .6f);
            gains[index] = overrideGain??entry.gain; started[index] = now;
            if (important) duckUntil = now + .22f;
            voice.volume = gains[index] * Volume * (!important && !pickup && now < duckUntil ? .4f : 1);
            voice.Play(); Played++;
        }
        void Update()
        {
            if (Bank == null) return;
            int active = 0;
            for (int i=0;i<VoiceCount;i++)
            {
                if (voices[i].isPlaying) active++;
                voices[i].volume = Muted ? 0 : gains[i] * Volume * (i < CombatVoices && Time.unscaledTime < duckUntil ? .4f : 1);
            }
            PeakVoices = Mathf.Max(PeakVoices, active);
        }
        public void StopCombat() { for (int i=0;i<CombatVoices;i++) voices[i].Stop(); }
        public void StopAll() { foreach (var voice in voices) voice.Stop(); }
        public void SetMuted(bool muted) { Muted=muted; if (muted) StopAll(); }
        public void SetVolume(float value) { Volume=Mathf.Clamp01(value); PlayerPrefs.SetFloat("sfxVolume",Volume);PlayerPrefs.Save(); }
    }
}
