using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public static class OriginalSupportAudio
    {
        [Serializable] public sealed class Entry {public string key;public string[] clips;public float db;[NonSerialized] public AudioClip[] loaded;}
        [Serializable] sealed class Catalog {public Entry[] entries;}
        static Dictionary<string,Entry> entries;
        public static Entry Find(string key)
        {
            if(entries==null){entries=new Dictionary<string,Entry>();foreach(var e in JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("original-support-audio").text).entries)entries[e.key]=e;}
            if(!entries.TryGetValue(key,out var entry))return null;
            if(entry.loaded==null)entry.loaded=Array.ConvertAll(entry.clips,p=>Resources.Load<AudioClip>(p));
            return entry;
        }
        public static void Play(GameAudio audio,string key,Vector2 position)
        {
            var e=Find(key);if(e==null||e.loaded.Length==0)return;
            audio.Play(SoundCue.Fire,position,e.loaded,Mathf.Pow(10,e.db/20),.2f);
        }
    }
}
