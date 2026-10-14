using UnityEngine;

namespace Scrapshift
{
    // Values transcribed from Brotato's explosion_smoke.tscn (100 source pixels per world unit).
    public static class ReferenceExplosionSmoke
    {
        static Material dirtMaterial,fleshMaterial;
        public static ParticleSystem Emit(Vector2 impact,float explosionScale,int amount,bool flesh=false)
        {
            if(amount<=0)return null;
            var texture=Resources.Load<Texture2D>(flesh?"WeaponReference/particle_1":"WeaponReference/particle_3");
            if(texture==null)return null;
            var material=flesh?fleshMaterial:dirtMaterial;
            if(material==null)
            {
                material=new Material(Shader.Find("Sprites/Default"));
                material.mainTexture=texture;
                if(flesh)fleshMaterial=material;else dirtMaterial=material;
            }
            var obj=new GameObject(flesh?"Flesh explosion smoke":"Explosion smoke");
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
            // FleshExplosion disables the background tint override; other explosions use the dirt outline.
            main.startColor=flesh?new Color(.607843f,.266667f,.266667f,1):new Color(.470588f,.403922f,.345098f,1);
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
