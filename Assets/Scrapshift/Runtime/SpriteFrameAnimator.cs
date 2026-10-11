using UnityEngine;

namespace Scrapshift
{
    // Driven by the simulation so pause and hit-stop also pause the animation.
    public sealed class SpriteFrameAnimator : MonoBehaviour
    {
        public Sprite[] idleFrames, moveFrames;
        public SpriteRenderer bodyView;
        public SpriteRenderer leftFoot, rightFoot;
        public Sprite footSprite;
        public PlayerLook[] looks;
        public int lookIndex;
        public PlayerLook Look => looks != null && looks.Length > 0 ? looks[Mathf.Clamp(lookIndex, 0, looks.Length - 1)] : null;
        public Transform BodyRoot=>bodyView!=null?bodyView.transform:transform;
        public Sprite CurrentSprite=>bodyView!=null?bodyView.sprite:view!=null?view.sprite:null;
        public Vector3 SpringPose {get;private set;}=new Vector3(1,1,0);
        [Range(3, 12)] public float walkFramesPerSecond = 7;
        SpriteRenderer view;
        float clock;
        bool moving;
        public int Frame { get; private set; }
        public bool Moving => moving;
        public bool FacingLeft { get; private set; }

        public void Tick(Vector2 movement, float dt, bool reducedMotion)
        {
            if (view == null) view = GetComponent<SpriteRenderer>();
            bool next = movement.sqrMagnitude > .01f;
            if (next != moving) { clock = 0; moving = next; }
            clock += Mathf.Max(0, dt);
            if (Mathf.Abs(movement.x) > .05f) FacingLeft = movement.x < 0;
            if(bodyView!=null)
            {
                Frame=0;SpringPose=SpringMotion.Pose(clock,moving,reducedMotion);
                bodyView.flipX=FacingLeft;
                bodyView.transform.localScale=new Vector3(SpringPose.x,SpringPose.y,1);
                bodyView.transform.localPosition=new Vector3(0,SpringPose.z,0);
                PoseFoot(leftFoot,-1,reducedMotion);PoseFoot(rightFoot,1,reducedMotion);
                return;
            }
            Frame = FrameAt(clock, moving, moveFrames.Length, walkFramesPerSecond);
            var frames = moving ? moveFrames : idleFrames;
            if (frames != null && frames.Length > 0) view.sprite = frames[Frame % frames.Length];
            view.flipX = FacingLeft;
        }

        void PoseFoot(SpriteRenderer foot,int side,bool reducedMotion)
        {
            if(foot==null)return;
            Vector2 offset=SpringMotion.Foot(clock,side,moving,reducedMotion);
            float facing=FacingLeft?-1:1;
            foot.transform.localPosition=new Vector3(offset.x*facing,SpringMotion.LegY+offset.y,.01f);
            foot.transform.localRotation=Quaternion.Euler(0,0,SpringMotion.LegAngle(clock,side,moving,reducedMotion)*facing);
            foot.flipX=(side<0)^FacingLeft;
        }

        public static int FrameAt(float time, bool moving, int walkCount = 6, float walkFps = 7)
        {
            if (moving) return Mathf.FloorToInt(time * walkFps) % Mathf.Max(1, walkCount);
            float cycle = Mathf.Repeat(time, 3);
            if (cycle < 2.64f) return 0;
            return Mathf.Min(3, 1 + Mathf.FloorToInt((cycle - 2.64f) / .12f));
        }

        // Short kick followed by a slower return. Zero before/after the firing interval.
        public static float Recoil(float age)
        {
            if (age < 0 || age >= .16f) return 0;
            return age < .025f ? age / .025f : 1 - (age - .025f) / .135f;
        }
    }
}
