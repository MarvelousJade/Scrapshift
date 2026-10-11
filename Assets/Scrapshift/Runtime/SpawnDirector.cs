using System;
using System.Collections.Generic;
using System.Linq;
namespace Scrapshift {
// Integer-second schedule. Processing in chronological order makes RNG frame-rate independent.
public sealed class SpawnDirector {
 sealed class State {public SpawnGroup group;public float next,interval;public int left;public bool checkedChance;}
 readonly List<State> states=new List<State>();readonly Random random;double time;int processed=-1;
 public SpawnDirector(WaveEntry wave,int seed){random=new Random(seed);var picked=new HashSet<int>();foreach(var g in wave.groups){if(g.bucket>0){if(!picked.Add(g.bucket))continue;var options=wave.groups.Where(x=>x.bucket==g.bucket).ToArray();Add(options[random.Next(options.Length)]);}else Add(g);}}
 void Add(SpawnGroup g){states.Add(new State{group=g,next=g.start,interval=Math.Max(g.minimum,g.interval),left=g.repeats+1});}
 public void Tick(float dt,Action<SpawnGroup,UnitEntry,int> emit){if(dt<=0)return;time+=dt;int whole=(int)Math.Floor(time+.0001);while(processed<whole){processed++;foreach(var s in states)if(s.left>0&&processed>=s.next){if(!s.checkedChance){s.checkedChance=true;if(random.NextDouble()>=s.group.chance){s.left=0;continue;}}foreach(var u in s.group.units){float n=random.Next(u.min,u.max+1)*u.chance;int count=(int)n;if(random.NextDouble()<n-count)count++;if(count>0)emit(s.group,u,count);}s.left--;s.next=processed+s.interval;s.interval=Math.Max(s.group.minimum,s.interval-s.group.reduce);}}}
}}
