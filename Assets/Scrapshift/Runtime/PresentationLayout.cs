using UnityEngine;

namespace Scrapshift
{
    public static class PresentationLayout
    {
        public const float Aspect = 1440f / 900f;
        public static Rect Viewport(int width, int height)
        {
            return new Rect(0,0,1,1);
        }
        public static float CanvasScale(int width,int height)=>Mathf.Max(.001f,Mathf.Min(width/1440f,height/900f));
        public static Rect CanvasBounds(int width,int height)
        {
            float scale=CanvasScale(width,height),w=width/scale,h=height/scale;
            return new Rect((1440-w)/2,(900-h)/2,w,h);
        }
        public static Vector2 ViewportToCanvas(Vector2 point,int width,int height)
        {
            var r=CanvasBounds(width,height);
            return new Vector2(r.x+point.x*r.width,r.y+(1-point.y)*r.height);
        }
    }
}
