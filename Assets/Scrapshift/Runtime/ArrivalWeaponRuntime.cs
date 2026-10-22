using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        float WeaponArrivalTime=>Arriving?arrivalAge:ArrivalMotion.Duration;
        void AdvanceArrivalPresentation(float dt)
        {
            if(!Arriving||Rules.Phase!=RunPhase.Playing)return;
            float previous=arrivalAge;TickArrival(dt);
            // Source AnimationPlayer updates the container after physics. Move
            // its rendered children without simulating cooldowns or shots twice.
            Vector2 before=playerPos+ArrivalWeaponMotion.Offset(previous),after=playerPos+ArrivalWeaponMotion.Offset(arrivalAge);
            float angle=ArrivalWeaponMotion.Rotation(arrivalAge)-ArrivalWeaponMotion.Rotation(previous);
            var rotation=Quaternion.Euler(0,0,angle);
            foreach(var view in equipmentViews)if(view!=null)
            {
                var p=view.transform.position;Vector2 relative=(Vector2)p-before;
                Vector2 next=after+(Vector2)(rotation*relative);
                view.transform.position=new Vector3(next.x,next.y,p.z);view.transform.rotation=rotation*view.transform.rotation;
            }
        }
    }
}
