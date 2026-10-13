using UnityEngine;
namespace Scrapshift
{
    // Original score; a dedicated source keeps combat voice stealing away from music.
    public sealed class GameMusic : MonoBehaviour
    {
        AudioSource source;
        float phaseGain=.6f;
        public void Initialize()
        {
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=0;source.priority=64;
            source.clip=Resources.Load<AudioClip>("Music/NeonSalvage");source.volume=0;
            if(source.clip!=null)source.Play();
        }
        public void SetPhase(RunPhase phase)=>phaseGain=phase==RunPhase.Playing?1:phase==RunPhase.Paused?.32f:.6f;
        void Update()
        {
            if(source==null)return;
            float gain=GameSettings.IsFixture?0:GameSettings.Value("music",.35f)*phaseGain;
            source.volume=Mathf.MoveTowards(source.volume,gain,Time.unscaledDeltaTime*.65f);
        }
    }
    public sealed partial class ArenaGame {GameMusic music;}
}
