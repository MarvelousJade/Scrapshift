using UnityEngine;

namespace Scrapshift
{
    // Values transcribed from Brotato's explosion_smoke.tscn (100 source pixels per world unit).
    public static class RocketExplosionSmoke
    {
        static Material material;
        public static ParticleSystem Emit(Vector2 impact,float explosionScale,int amount)
        {
            var texture=Resources.Load<Texture2D>("WeaponReference/particle_3");
            if(texture==null)return null;
            if(material==null)
            {
                material=new Material(Shader.Find("Sprites/Default"));
                material.mainTexture=texture;
            }
            var obj=new GameObject("Explosion smoke");
            obj.transform.position=new Vector3(impact.x,impact.y-.24f*explosionScale,-1);
            var particles=obj.AddComponent<ParticleSystem>();
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;
            main.playOnAwake=false;
            main.loop=false;
            main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.maxParticles=amount;
            main.startLifetime=.75f;
            main.startSpeed=new ParticleSystem.MinMaxCurve(.15f*explosionScale,.3f*explosionScale);
            main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
            main.startSize=new ParticleSystem.MinMaxCurve(.24f*explosionScale,.4f*explosionScale);
            // RunningSmoke._ready() replaces the scene tint with the dirt arena's outline color.
            main.startColor=new Color(.470588f,.403922f,.345098f,1);
            var shape=particles.shape;
            shape.enabled=true;
            shape.shapeType=ParticleSystemShapeType.Circle;
            shape.radius=1.28f*explosionScale;
            shape.radiusThickness=1;
            var emission=particles.emission;
            emission.enabled=false;
            var rotation=particles.rotationOverLifetime;
            rotation.enabled=true;
            rotation.z=new ParticleSystem.MinMaxCurve(Mathf.PI*2*.54f,Mathf.PI*4);
            var size=particles.sizeOverLifetime;
            size.enabled=true;
            size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(1,0)));
            var renderer=particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial=material;
            renderer.sortingOrder=14;
            particles.Play();
            particles.Emit(amount);
            return particles;
        }
    }
}
