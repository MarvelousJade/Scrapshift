using System;
using System.Collections.Generic;
using System.Linq;
namespace Scrapshift
{
    // Integer-second schedule. Processing in chronological order makes RNG frame-rate independent.
    public sealed class SpawnDirector
    {
        sealed class State
        {
            public SpawnGroup group;
            public float next,interval;
            public int left;
            public bool checkedChance;
        }
        readonly List<State> states=new List<State>();
        readonly Random random;
        double time;
        int processed=-1;
        readonly float enemyModifier,extraTrees;
        public SpawnDirector(WaveEntry wave,int seed,float enemyModifier=0,float extraTrees=0,int extraLootAliens=0)
        {
            this.enemyModifier=enemyModifier;this.extraTrees=extraTrees;
            random=new Random(seed);
            var picked=new HashSet<int>();
            foreach(var g in wave.groups)
            {
                if(g.bucket>0)
                {
                    if(!picked.Add(g.bucket))continue;
                    var options=wave.groups.Where(x=>x.bucket==g.bucket).ToArray();
                    Add(options[random.Next(options.Length)]);
                }
                else Add(g);
            }
            // Crash Zone loot group: one alien per copy, once between 5s and ten seconds before the end.
            for(int i=0;i<extraLootAliens;i++)Add(new SpawnGroup{start=5+(float)random.NextDouble()*Math.Max(0,wave.duration-15),interval=3,minimum=1,repeats=-1,chance=1,area=-1,inset=2,units=new[]{new UnitEntry{id="looter",min=1,max=1,chance=1}}});
        }
        void Add(SpawnGroup g)
        {
            states.Add(new State
            {
                group=g,next=g.start,interval=Math.Max(g.minimum,g.interval),left=Math.Max(0,g.repeats)+1
            })
            ;
        }
        public void Tick(float dt,Action<SpawnGroup,UnitEntry,int> emit,Action<SpawnGroup> beginGroup=null)
        {
            if(dt<=0)return;time+=dt;int whole=(int)Math.Floor(time+.0001);while(processed<whole)
            {
                processed++;foreach(var s in states)if(s.left>0&&processed>=s.next)
                {
                    bool first=!s.checkedChance;
                    bool spawn=!first||random.NextDouble()<s.group.chance;
                    if(spawn)beginGroup?.Invoke(s.group);
                    if(spawn)foreach(var u in s.group.units)
                    {
                        float number=random.Next(u.min,u.max+1);
                        if(u.id=="tree")number+=extraTrees;
                        else if(u.id!="looter")number=Math.Max(1,number*(1+enemyModifier/100));
                        float n=Math.Max(0,number)*u.chance;int count=(int)n;if(random.NextDouble()<n-count)count++;if(count>0)emit(s.group,u,count);
                    }
                    s.left--;
                    // First repeat keeps the original interval; later repeats reduce before scheduling.
                    if(!first)s.interval=Math.Max(s.group.minimum,s.interval-s.group.reduce);
                    s.next+=s.interval;s.checkedChance=true;
                }
            }
        }
    }
}
