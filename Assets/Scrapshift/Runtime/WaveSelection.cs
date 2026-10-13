using System;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class BaseWaveCatalog {public WaveEntry[] waves;public SpawnGroup[] hordes;}
    public static class WaveSelection
    {
        public static WaveEntry For(BaseWaveCatalog data,int wave,int difficulty,bool elite,bool horde)
        {
            var source=data.waves[wave-1];var groups=source.groups.AsEnumerable();if(horde)groups=groups.Concat(data.hordes);
            return new WaveEntry{duration=source.duration,cap=source.cap,groups=groups.Where(g=>!g.boss&&wave>=g.minWave&&wave<=g.maxWave&&difficulty>=g.minDifficulty&&difficulty<=g.maxDifficulty&&!(elite&&g.preventElite)&&!(horde&&g.preventHorde)).ToArray()};
        }
    }
    public sealed partial class ArenaGame
    {
        BaseWaveCatalog baseWaves;
        WaveEntry CurrentSourceWave()
        {if(baseWaves==null)baseWaves=JsonUtility.FromJson<BaseWaveCatalog>(Resources.Load<TextAsset>("base-waves").text);return WaveSelection.For(baseWaves,Rules.Wave,Rules.Difficulty,eliteWaves.ContainsKey(Rules.Wave),hordeWaves.Contains(Rules.Wave));}
    }
}
