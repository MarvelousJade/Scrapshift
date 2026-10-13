using UnityEngine;

namespace Scrapshift
{
    public static class ActorPresentation
    {
        public const float PlayerScale = .59f;
        // Presentation is separate from contact/separation radii and combat balance.
        public static float EnemyScale(EnemyEntry enemy) => EnemyWidth(enemy.id)/enemy.image.bounds.size.x;
        public static float EnemyWidth(string id)
        {
            switch(id)
            {
                case "baby_alien": return .73f;
                case "bruiser": return 1.2f;
                case "horned_bruiser": return 1.4f;
                case "buffer": case "looter": return .87f;
                case "charger": case "chaser": case "fin_alien": case "horned_charger": return .86f;
                case "helmet_alien": return .93f;
                case "junkie": return .76f;
                case "predator":case "invoker": return 1.98f;
                case "butcher":case "colossus":case "croc":case "gargoyle":case "mantis":case "mom":case "monk":case "rhino":return 1.8f;
                case "pursuer": return .96f;
                case "slasher": return 1.78f;
                case "spawner": return 1.34f;
                case "spitter": return .83f;
                case "tree": return 1.3f;
                case "evil_mob": return 1.26f;
                default: return .86f;
            }
        }
        public static float SpriteLengthScale(Sprite sprite,float length) =>
            length/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y);
        public static float WeaponScale(Equipment weapon)
        {
            return SpriteLengthScale(weapon.image,WeaponVisualMetrics.Length(weapon.id));
        }
        static readonly Vector2[][] mounts={
            new[]{new Vector2(0,-.30f)},
            new[]{new Vector2(.40f,-.20f),new Vector2(-.40f,-.20f)},
            new[]{new Vector2(.40f,-.20f),new Vector2(-.40f,-.20f),new Vector2(0,.45f)},
            new[]{new Vector2(.40f,-.35f),new Vector2(-.40f,-.35f),new Vector2(-.40f,.30f),new Vector2(.40f,.30f)},
            new[]{new Vector2(.35f,-.35f),new Vector2(-.35f,-.35f),new Vector2(-.55f,.20f),new Vector2(.55f,.20f),new Vector2(0,.55f)},
            new[]{new Vector2(.35f,-.45f),new Vector2(-.35f,-.45f),new Vector2(-.60f,0),new Vector2(.60f,0),new Vector2(-.35f,.45f),new Vector2(.35f,.45f)}
        };
        // The loaded reference resolves Sprite/Attach at zero; individual sprites offset from that pivot.
        public static float WeaponPivot(string id) => id=="spear"?.48f:id=="knife"?.32f:.24f;
        public static Vector2 RestDirection(bool facingLeft) => facingLeft?Vector2.left:Vector2.right;
        public static int WeaponOrder(int count,int index) => WeaponMount(count,index).y>0?8:12;
        public static Vector2 WeaponMount(int count,int index)
        {
            if(count<=6)return mounts[Mathf.Clamp(count,1,6)-1][index];
            float angle=-Mathf.PI/2+index*Mathf.PI*2/count;
            return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(count>12?.95f:.75f);
        }
    }
}
