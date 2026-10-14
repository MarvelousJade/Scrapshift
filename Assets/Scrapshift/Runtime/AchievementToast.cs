using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    // Separate from the end-of-run reward list; viewing either never consumes the other.
    public sealed class AchievementToastQueue
    {
        readonly Queue<UnlockDefinition> waiting=new Queue<UnlockDefinition>();
        public UnlockDefinition Current {get;private set;}
        public float Age {get;private set;}
        public const float Duration=2.6f;
        public void Enqueue(UnlockDefinition achievement)
        {if(Current==null){Current=achievement;Age=0;}else waiting.Enqueue(achievement);}
        public int Tick(float dt)
        {
            if(Current==null)return 0;Age+=Mathf.Max(0,dt);int finished=0;
            while(Current!=null&&Age>=Duration)
            {Age-=Duration;Current=waiting.Count>0?waiting.Dequeue():null;finished++;}
            return finished;
        }
    }
    public sealed partial class ArenaGame
    {
        readonly AchievementToastQueue achievementToasts=new AchievementToastQueue();
        void DrawAchievementToast()
        {
            var achievement=achievementToasts.Current;if(achievement==null)return;
            var reward=RewardEntry(achievement.reward);
            float fade=Mathf.Min(Mathf.Clamp01(achievementToasts.Age/.2f),Mathf.Clamp01((AchievementToastQueue.Duration-achievementToasts.Age)/.3f));
            var r=new Rect(FullCanvas.xMin+20-(reducedMotion?0:30*(1-fade)),FullCanvas.yMax-114,360,94);
            var old=GUI.color;GUI.color=new Color(1,1,1,fade);
            RoundedBox(r,Color.black,10);RoundedBox(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),new Color(.13f,.15f,.13f),8);
            var imageRect=new Rect(r.x+12,r.y+15,62,62);
            DrawAchievementIcon(achievement,imageRect);
            Text(r.x+86,r.y+9,260,26,"Achievement unlocked!",new GUIStyle(body){fontSize=18,normal={textColor=new Color(.6f,1,.35f)}});
            Text(r.x+86,r.y+36,260,50,reward.title,new GUIStyle(body){fontSize=20});GUI.color=old;
        }
    }
}

