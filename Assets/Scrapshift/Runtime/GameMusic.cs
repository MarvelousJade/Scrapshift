using UnityEngine;
namespace Scrapshift
{
    // Original score; a dedicated source keeps combat voice stealing away from music.
    public sealed class GameMusic : MonoBehaviour
    {
        AudioSource source;
        float phaseGain=1;
        bool started;
        public bool IsPlaying=>source!=null&&source.isPlaying;
        public float PlaybackTime=>source==null?0:source.time;
        public void Initialize()
        {
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=0;source.priority=64;
            source.clip=Resources.Load<AudioClip>("Music/NeonSalvage");source.volume=GameSettings.IsFixture?0:GameSettings.Value("music",.35f);

        }
        public void StartOnce() { if(started)return;started=true;if(source!=null&&source.clip!=null)source.Play(); }
        public void SetPhase(RunPhase phase)=>phaseGain=phase==RunPhase.Paused?.32f:1;
        void Update()
        {
            if(source==null||!started)return;
            if(source.clip!=null&&!source.isPlaying&&source.clip.loadState==AudioDataLoadState.Loaded)source.Play();
            float gain=GameSettings.IsFixture?0:GameSettings.Value("music",.35f)*phaseGain;
            source.volume=Mathf.MoveTowards(source.volume,gain,Time.unscaledDeltaTime*.65f);
        }
    }
    public sealed partial class ArenaGame {GameMusic music;}
}
