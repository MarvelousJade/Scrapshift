using UnityEngine;

namespace Scrapshift
{
    public static class ArenaGeometry
    {
        // Base Crash Zone: 32 x 24 tiles of 64 pixels, at 100 reference pixels per unit.
        public static readonly Vector2 BaseHalfSize = new Vector2(10.24f,7.68f);
        public static Vector2 HalfSize {get;private set;}=BaseHalfSize;
        public static void SetMapSize(float percent) {float factor=1+percent/100;HalfSize=new Vector2(Mathf.Max(12,(int)(32*factor)),Mathf.Max(12,(int)(24*factor)))*.32f;}
        public const float CameraHalfHeight = 5.4f;
        public static Vector2 Clamp(Vector2 p) => new Vector2(
            Mathf.Clamp(p.x,-HalfSize.x,HalfSize.x),Mathf.Clamp(p.y,-HalfSize.y,HalfSize.y));

        public static Vector2 CameraPosition(Vector2 player, float aspect, float halfHeight = CameraHalfHeight)
        {
            // Reference camera includes 96px borders and 192px above the map for HUD space.
            float halfWidth=halfHeight*aspect;
            return new Vector2(Fit(player.x,-HalfSize.x-.96f,HalfSize.x+.96f,halfWidth),
                Fit(player.y,-HalfSize.y-.96f,HalfSize.y+1.92f,halfHeight));
        }
        static float Fit(float value,float min,float max,float halfView) =>
            max-min<=halfView*2?(min+max)*.5f:Mathf.Clamp(value,min+halfView,max-halfView);
    }
}
