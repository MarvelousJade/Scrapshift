using UnityEngine;
namespace Scrapshift
{
    public static class SupportBehavior
    {
        // Lootworm Animation is at raw (0,-24), attraction at raw origin and
        // pickup collision at raw (0,-10), relative to its adapted body center.
        public static Vector2 LootwormOriginOffset=>Vector2.down*.24f;
        public static Vector2 LootwormPickupOffset=>Vector2.down*.14f;
        public static Vector2 PetOriginOffset=>Vector2.down*.24f;
        public static Vector2 ScapegoatOriginOffset=>Vector2.down*.24f;
        public static Vector2 ScapegoatBodyOffset=>Vector2.down*.19f;
        public static Vector2 ScapegoatHurtOffset=>ScapegoatBodyOffset;
        public static Vector2 ScapegoatHealingOffset=>Vector2.down*.04f;
        public static Vector2 StructureOriginOffset(string kind)=>Vector2.down*(kind=="tyler"?.4f:.24f);
        public static Vector2 StructureMuzzleOffset(string kind)=>kind=="tyler"?Vector2.up*.16f:kind=="builder"?Vector2.down*.02f:Vector2.up*.12f;
        // Utils.get_nearest_no_max stores source pixel-squared distance in an
        // inferred int. Its is_between caller also takes an int distance.
        public static int StructureTargetDistance(Vector2 delta)=>(int)(delta.sqrMagnitude*10000);
        public static bool StructureFiringReach(Vector2 delta,float range)=>(int)Mathf.Sqrt(StructureTargetDistance(delta))<=(int)(range*100);
        public static bool StructureBodyInRange(Vector2 origin,float range,string id,Vector2 enemy)
            =>(enemy+CombatGeometry.BodyOffset(id)-origin).sqrMagnitude<=Mathf.Pow(range+CombatGeometry.Enemy(id).body,2);
        public static bool StructurePlayerInRange(Vector2 origin,float range,Vector2 player)
            =>(player+CombatGeometry.PlayerHurtOffset-origin).sqrMagnitude<=Mathf.Pow(range+.24f,2);
        public static float ExplosionRadius(float baseRadius,float sizePercent)=>Mathf.Max(.14734f,baseRadius*(1+sizePercent/100));
        public static bool ExplosionOverlaps(Vector2 origin,float radius,string enemyId,Vector2 enemy)
        {
            Vector2 center=origin+Vector2.down*(.03f*radius/1.4734f);
            float reach=radius+CombatGeometry.Enemy(enemyId).hurt;
            return (enemy+CombatGeometry.HurtOffset(enemyId)-center).sqrMagnitude<=reach*reach;
        }
        public static bool CanReviveScapegoat(Vector2 player,Vector2 goat)
            =>(player+CombatGeometry.PlayerHurtOffset-goat-ScapegoatHealingOffset).sqrMagnitude<=1.24f*1.24f;
        public static bool CatBoostFromPlayer(Vector2 cat,Vector2 player)
            =>(player+CombatGeometry.PlayerHurtOffset-cat-PetOriginOffset).sqrMagnitude<=1.74f*1.74f;
        public static bool CatBoostFromPet(Vector2 cat,string kind,Vector2 other)
        {
            // PlayerTriggerZone mask 514 sees layer-512 pets. The noncombat
            // Lootworm, Moth, Scapegoat and Jellyshield have body layer zero.
            if(kind!="ratzilla"&&kind!="bonk_dog"&&kind!="blazemander"&&kind!="bot_o_mine")return false;
            float radius=kind=="ratzilla"?.2f:.4f;
            Vector2 bodyOffset=kind=="ratzilla"?Vector2.down*.34f:Vector2.zero;
            return (other+bodyOffset-cat-PetOriginOffset).sqrMagnitude<=Mathf.Pow(1.5f+radius,2);
        }
        // Shape positions are source pixels relative to the unanimated root.
        public static Vector2 PetHitOffset(string kind)=>Vector2.down*(kind=="ratzilla"?.34f:kind=="lootworm"?.12f:.04f);
        public static bool PetBodyZone(Vector2 pet,float radius,string enemyId,Vector2 enemy,Vector2? offset=null)
        {
            if((CombatGeometry.BodyLayer(enemyId)&4)==0)return false;
            float reach=radius+CombatGeometry.Enemy(enemyId).body;
            return (enemy+CombatGeometry.BodyOffset(enemyId)-pet-(offset??PetOriginOffset)).sqrMagnitude<=reach*reach;
        }
        public static bool PetHitOverlaps(string kind,Vector2 pet,string enemyId,Vector2 enemy)
        {
            float reach=(kind=="ratzilla"?.3339f:.498f)+CombatGeometry.Enemy(enemyId).hurt;
            return (enemy+CombatGeometry.HurtOffset(enemyId)-pet-PetHitOffset(kind)).sqrMagnitude<=reach*reach;
        }
        public static bool WanderingZoneOverlaps(Vector2 bot,string enemyId,Vector2 enemy)
        {
            float reach=1.71f+CombatGeometry.Enemy(enemyId).hurt;
            return (enemy+CombatGeometry.HurtOffset(enemyId)-bot-new Vector2(.01f,-.04f)).sqrMagnitude<=reach*reach;
        }
        // Reference Lootworm keeps an object target (or itself when idle) until
        // a spawn event or that target's removal. Null is our idle sentinel.
        public sealed class TargetCache<T> where T:class
        {
            T current;bool initialized;
            public T Resolve(System.Func<T> select,System.Func<T,bool> valid)
            {
                if(!initialized||current!=null&&!valid(current)){current=select();initialized=true;}
                return current;
            }
            public void Invalidate(){current=null;initialized=false;}
        }
        // Godot Tween TRANS_EXPO, EASE_IN_OUT used by Bonk Dog's half-second leap.
        public static float JumpProgress(float t)=>t<=0?0:t>=1?1:t<.5f?Mathf.Pow(2,20*t-10)/2:(2-Mathf.Pow(2,-20*t+10))/2;
        public static float CatInterval(float interval,bool boosted)=>interval*(boosted?.5f:1);
        public static bool MayReserve(string kind,bool boss,bool elite,bool claimed)=>kind=="ratzilla"&&boss?false:!claimed||(kind=="blazemander"?elite:boss);
        // Adapted body center is player-down*.12; zone origin is moth-down*.24.
        // Their relative vector is therefore player+up*.12-moth.
        public static bool InsideMothZone(Vector2 player,Vector2 moth,float zone)
            =>(player+Vector2.up*.12f-moth).sqrMagnitude<(zone+.24f)*(zone+.24f);
        // Bot O' Mine requests a 100px-wide spawn area. ZoneService clamps the
        // area's center before sampling, retaining a 64px margin at map edges.
        public static Vector2 BotMineSpawnCenter(Vector2 origin)
        {
            var half=ArenaGeometry.HalfSize;
            var center=new Vector2(Mathf.Clamp(origin.x,-half.x+1.14f,half.x-1.14f),Mathf.Clamp(origin.y,-half.y+1.14f,half.y-1.14f));
            return center;
        }
    }
}
