using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed class ScrapMotion
    {
        public Vector2 position, velocity;
        public float height, verticalSpeed, angle, spin, age;
        public bool magnetized;
        public float physicsRate=60;
        public float collisionScale=1,collisionAngle;
        public bool CanCollect {get;private set;}
        public bool CanMoveTowardCollector=>CanCollect&&idleRemaining<=0;
        public float AttractionSpeed=>magnetSpeed;
        Vector2 pushDestination;
        public Vector2 PushDestination {get=>pushDestination;set{pushDestination=value;initialized=true;}}
        float magnetSpeed=5,idleRemaining=10f/60,physicsRemainder;
        bool initialized,pushing=true;
        public static readonly object PlayerCollector=new object();
        readonly HashSet<object> attractionOverlaps=new HashSet<object>(),contactOverlaps=new HashSet<object>();

        public static ScrapMotion CreateDrop(Vector2 position,float physicsRate=60,float spread=0)
        {
            // ZoneService.get_rand_pos_in_area uses a square with the sampled
            // 50..(100+spread) pixel AREA as its full side length, clamped inside
            // the map. The optional spread argument is already in world units.
            float side=UnityEngine.Random.Range(.5f,1f+Mathf.Max(0,spread)),half=side*.5f;
            var bounds=ArenaGeometry.HalfSize;
            var center=new Vector2(Mathf.Clamp(position.x,-bounds.x+half,bounds.x-half),Mathf.Clamp(position.y,-bounds.y+half,bounds.y-half));
            var destination=center+new Vector2(UnityEngine.Random.Range(-half,half),UnityEngine.Random.Range(-half,half));
            return new ScrapMotion {position=position,PushDestination=destination,initialized=true,physicsRate=physicsRate,velocity=(destination-position)*5};
        }

        public bool OverlapsCollector(Vector2 center,float radius)
        {
            if(radius<0)return false;
            Vector2 delta=center-position;float radians=-collisionAngle*Mathf.Deg2Rad;
            var local=new Vector2(delta.x*Mathf.Cos(radians)-delta.y*Mathf.Sin(radians),delta.x*Mathf.Sin(radians)+delta.y*Mathf.Cos(radians));
            float half=.16f*collisionScale;
            var nearest=new Vector2(Mathf.Max(0,Mathf.Abs(local.x)-half),Mathf.Max(0,Mathf.Abs(local.y)-half));
            return nearest.sqrMagnitude<=radius*radius;
        }
        public bool PlayerCanRetarget(Vector2 player,Vector2 currentCollector,float radius)=>CanCollect&&OverlapsCollector(player,radius)&&(player-position).sqrMagnitude<(player-currentCollector).sqrMagnitude;
        public void ReleaseCollector(){magnetized=false;magnetSpeed=5;}
        bool Entered(HashSet<object> overlaps,object collector,Vector2 center,float radius)
        {
            bool inside=CanCollect&&OverlapsCollector(center,radius);
            if(!inside){overlaps.Remove(collector);return false;}
            return overlaps.Add(collector);
        }
        public bool AttractionEntered(object collector,Vector2 center,float radius)=>Entered(attractionOverlaps,collector,center,radius);
        public bool ContactEntered(object collector,Vector2 center,float radius)=>Entered(contactOverlaps,collector,center,radius);

        public void Step(float dt, Vector2 target, float radius, bool reducedMotion,bool externalCollector=false,bool observePlayer=true)
        {
            age += dt;
            if(!initialized){PushDestination=position+velocity/6;initialized=true;}
            float tick=1/Mathf.Max(1,physicsRate);physicsRemainder+=dt;
            while(physicsRemainder+1e-7f>=tick)
            {
                physicsRemainder=Mathf.Max(0,physicsRemainder-tick);
                if(pushing&&(position-PushDestination).sqrMagnitude>.04f)
                {position=Vector2.Lerp(position,PushDestination,tick*5);continue;}
                // Source item.gd makes the pickup monitorable DURING the idle
                // interval. Contact and attraction acquisition can happen now;
                // movement toward its collector waits until idle has elapsed.
                if(idleRemaining>0)
                {CanCollect=true;pushing=false;idleRemaining=Mathf.Max(0,idleRemaining-tick);if(idleRemaining<1e-7f)idleRemaining=0;if(observePlayer)Acquire(target,radius);continue;}
                if(pushing){pushing=false;continue;}
                if(observePlayer)Acquire(target,radius);
                if(magnetized||externalCollector)
                {position=Vector2.MoveTowards(position,target,tick*magnetSpeed);magnetSpeed+=.2f;}
            }
            velocity *= Mathf.Exp(-dt * 6);
            if (reducedMotion) { height = 0; spin = 0; return; }
            verticalSpeed -= dt * 16;
            height += verticalSpeed * dt;
            if (height < 0)
            {
                height = 0;
                verticalSpeed = Mathf.Abs(verticalSpeed) > 1 ? -verticalSpeed * .28f : 0;
            }
            angle += spin * dt;
            spin *= Mathf.Exp(-dt * (height > 0 ? 2 : 14));
        }
        void Acquire(Vector2 target,float radius)
        {if(AttractionEntered(PlayerCollector,target,radius))magnetized=true;}

        // Last piece carries the remainder, even when only one slot remains.
        public static int[] SplitValue(int value, int availableSlots)
        {
            int count = Math.Min(Math.Min(6, Math.Max(0, value)), Math.Max(0, availableSlots));
            var pieces = new int[count];
            for (int i = 0; i < count; i++) pieces[i] = i == count - 1 ? value - count + 1 : 1;
            return pieces;
        }
    }
}
