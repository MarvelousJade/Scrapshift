using System;
using UnityEngine;

namespace Scrapshift
{
    public enum SoundCue { Fire, Impact, Kill, Hurt, Critical, Pickup, LevelUp, Install, Pulse, Swing, Reroll, Win, Lose, WaveClear, Charge, Step, UiFocus, UiPress, Spawn, InventoryFocus, Consume }
    [Serializable] public sealed class SoundEntry
    {
        public SoundCue cue;
        public AudioClip[] variants;
        [Range(0, 1)] public float gain = .3f;
        public float cooldown = .04f;
        public float pitchVariation = .055f;
        public int priority;
    }
    [CreateAssetMenu(menuName = "Scrapshift/Sound bank")]
    public sealed class SoundBank : ScriptableObject
    {
        public SoundEntry[] entries;
        public SoundEntry Find(SoundCue cue) => Array.Find(entries, e => e.cue == cue);
    }
}
