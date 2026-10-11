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
                case "predator": return 1.98f;
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
            float length=weapon.id=="spear"?1.62f:weapon.id=="knife"?.77f:
                weapon.id=="double_barrel_shotgun"?.87f:weapon.id=="laser_gun"?.64f:.65f;
            return SpriteLengthScale(weapon.image,length);
        }
        static readonly Vector2[][] mounts={
            new[]{new Vector2(0,-.30f)},
            new[]{new Vector2(.40f,-.20f),new Vector2(-.40f,-.20f)},
            new[]{new Vector2(.40f,-.20f),new Vector2(-.40f,-.20f),new Vector2(0,.45f)},
            new[]{new Vector2(.40f,-.35f),new Vector2(-.40f,-.35f),new Vector2(-.40f,.30f),new Vector2(.40f,.30f)},
            new[]{new Vector2(.35f,-.35f),new Vector2(-.35f,-.35f),new Vector2(-.55f,.20f),new Vector2(.55f,.20f),new Vector2(0,.55f)},
            new[]{new Vector2(.35f,-.45f),new Vector2(-.35f,-.45f),new Vector2(-.60f,0),new Vector2(.60f,0),new Vector2(-.35f,.45f),new Vector2(.35f,.45f)}
        };
        public static Vector2 WeaponMount(int count,int index) => mounts[Mathf.Clamp(count,1,6)-1][index];
    }
}
