using System;
using System.Collections.Generic;
namespace Scrapshift
{
    public enum BirthLane { Regular, Tree, Boss, Summon }
    public sealed class ReferenceBirthQueue<T>
    {
        readonly Stack<T>[] lanes = { new Stack<T>(), new Stack<T>(), new Stack<T>(), new Stack<T>() };
        float clock;
        public static int RegularBatch(int count) => count >= 120 ? 2 : 1;
        public void Push(T entry, BirthLane lane = BirthLane.Regular) => lanes[(int)lane].Push(entry);
        public void Clear() { foreach(var lane in lanes) lane.Clear(); clock = 0; }
        public void Tick(float dt, Action<T> spawn)
        {
            clock += dt;
            while(clock >= .05f - .000001f)
            {
                clock -= .05f;
                for(int lane=1;lane<4;lane++) if(lanes[lane].Count>0) spawn(lanes[lane].Pop());
                int count=RegularBatch(lanes[0].Count);
                for(int i=0;i<count && lanes[0].Count>0;i++) spawn(lanes[0].Pop());
            }
        }
    }
}
