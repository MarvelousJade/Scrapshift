using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    // Baked contextual edits. Reference recordings and original bank assignments stay intact.
    public sealed class CyberSoundLibrary : ScriptableObject
    {
        [Serializable] public sealed class Entry { public AudioClip source, edited; public int profile; }
        public Entry[] entries=Array.Empty<Entry>();
        Dictionary<(AudioClip,int),AudioClip> lookup;
        public void SetEntries(Entry[] value){entries=value;lookup=null;}
        public static int Profile(SoundCue cue)
        {
            switch(cue)
            {
                case SoundCue.Fire:return 0;
                case SoundCue.Impact:return 1;
                case SoundCue.Pulse:return 2;
                case SoundCue.Swing:return 3;
                case SoundCue.Win:return 4;
                case SoundCue.Lose:return 5;
                case SoundCue.Pickup:return -1;
                default:return 200+(int)cue;
            }
        }
        public AudioClip Resolve(AudioClip source,SoundCue cue,int? contextProfile=null)
        {
            int profile=cue==SoundCue.Pickup?-1:contextProfile??Profile(cue);
            if(source==null||profile<0)return source;
            if(lookup==null)
            {
                lookup=new Dictionary<(AudioClip,int),AudioClip>();
                foreach(var e in entries)if(e.source!=null&&e.edited!=null)lookup[(e.source,e.profile)]=e.edited;
            }
            return lookup.TryGetValue((source,profile),out var edited)?edited:source;
        }
    }
}
