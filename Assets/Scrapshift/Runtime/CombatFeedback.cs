using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class Floater { public Vector2 pos; public string text; public Color color; public float age; public bool big; }
        readonly List<Floater> floaters = new List<Floater>();
        MaterialPropertyBlock flashBlock;
        GameObject gun;
        Vector2 gunDirection = Vector2.right;
        float recoil, playerFlash, shakeTime;
        readonly List<SpriteRenderer> playerFlashViews = new List<SpriteRenderer>();
        void Shake(float amount)
        {
            shake = Mathf.Max(shake, amount); shakeTime = .1f;
        }
        sealed class Impact { public GameObject obj; public float age, size; }
        readonly List<Impact> impacts = new List<Impact>();
        void ImpactBurst(Vector2 position, Vector2 direction, bool critical, bool playerHit = false)
        {
            if (impacts.Count >= 80) return;
            var obj = new GameObject("Impact star"); obj.transform.position = new Vector3(position.x, position.y, -3);
            obj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            if(playerHit)
            {
                // The reference has separate tapered white streaks and an open center.
                // Keep native geometry instead of importing its effect textures.
                float[] angles={0,108,182,278},lengths={1,.64f,.92f,.7f};
                for(int i=0;i<angles.Length;i++)foreach(bool outline in new[]{true,false})
                {
                    Vector3 ray=Quaternion.Euler(0,0,angles[i])*Vector3.right;
                    var line=MakeLine(outline?"Hit streak outline":"Hit streak",obj.transform,
                        outline?new Color(.12f,.09f,.13f):Color.white,1);
                    line.sortingOrder=outline?20:21;line.positionCount=2;
                    line.startWidth=outline?.09f:.055f;line.endWidth=0;
                    line.SetPosition(0,ray*.22f);line.SetPosition(1,ray*lengths[i]);
                }
                obj.transform.localScale=Vector3.one*.75f;
                impacts.Add(new Impact{obj=obj,size=.75f});return;
            }
            foreach (bool outline in new[] { true, false })
            {
                var line = MakeLine(outline ? "Impact outline" : "Impact light", obj.transform,
                    outline ? new Color(.12f,.09f,.13f) : critical ? amber : Color.white, outline ? .09f : .035f, true);
                line.sortingOrder = outline ? 20 : 21; line.positionCount = 16;
                for (int i = 0; i < 16; i++)
                {
                    float angle = i * Mathf.PI / 8, radius = i % 2 == 0 ? (i % 4 == 0 ? 1 : .65f) : .24f;
                    line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
                }
            }
            float size = critical ? .4f : .25f;
            obj.transform.localScale = Vector3.one * size;
            impacts.Add(new Impact { obj = obj, size = size });
        }
        void PlayerHitParticles()
        {
            // Source hit_particles: eight red fragments, 0.5 s, no gravity, 3-6 units/s.
            for(int i=0;i<8 && sparks.Count<200;i++)
            {
                var obj=Instantiate(sparkPrefab);
                Vector2 pos=playerPos+UnityEngine.Random.insideUnitCircle*.15f;
                obj.transform.position=new Vector3(pos.x,pos.y,-3);
                obj.transform.rotation=Quaternion.Euler(0,0,UnityEngine.Random.Range(0,360));
                var block=new MaterialPropertyBlock();block.SetColor("_Color",new Color(.623529f,.152941f,.152941f));
                obj.GetComponentInChildren<Renderer>().SetPropertyBlock(block);
                sparks.Add(new Spark{obj=obj,pos=pos,velocity=UnityEngine.Random.insideUnitCircle.normalized*UnityEngine.Random.Range(3f,6f),life=.5f,max=.5f});
            }
        }
        void UpdatePlayerFlash(float dt)
        {
            playerFlash = Mathf.Max(0, playerFlash - dt);
            if (flashBlock == null) flashBlock = new MaterialPropertyBlock();
            flashBlock.Clear(); flashBlock.SetFloat("_Flash", playerFlash > 0 ? 1 : 0);
            player.GetComponentsInChildren(true, playerFlashViews);
            foreach (var renderer in playerFlashViews) renderer.SetPropertyBlock(flashBlock);
        }
        void ClearImpacts()
        {
            foreach (var impact in impacts) Destroy(impact.obj);
            impacts.Clear(); playerFlash = 0; shake = shakeTime = 0;
            if (player != null) UpdatePlayerFlash(0);
        }
        void Flash(Enemy e, float amount)
        {
            if (flashBlock == null) flashBlock = new MaterialPropertyBlock();
            flashBlock.Clear(); flashBlock.SetFloat("_Flash", amount);
            foreach (var renderer in e.renderers) renderer.SetPropertyBlock(flashBlock);
        }
        void FloatText(Vector2 pos, string label, Color color, bool big)
        {
            if (floaters.Count >= 60) floaters.RemoveAt(0);
            floaters.Add(new Floater { pos = pos + UnityEngine.Random.insideUnitCircle * .15f, text = label, color = color, big = big });
        }
        void UpdateFeedback(float dt)
        {
            recoil = Mathf.Max(0, recoil - dt * 1.8f);
            for (int i = impacts.Count - 1; i >= 0; i--)
            {
                var impact = impacts[i]; impact.age += dt;
                // Three 50ms phases, matching the reference effect's short lifetime.
                float factor = reducedMotion ? 1 : impact.age < .05f ? 1 : impact.age < .1f ? .7f : .35f;
                impact.obj.transform.localScale = Vector3.one * impact.size * factor;
                if (impact.age >= .15f) { Destroy(impact.obj); impacts.RemoveAt(i); }
            }
            for (int i = floaters.Count - 1; i >= 0; i--)
            {
                var f = floaters[i]; f.age += dt; if (!reducedMotion) f.pos += Vector2.up * dt * 1.2f;
                if (f.age > .72f) floaters.RemoveAt(i);
            }
        }
        void DrawFeedback()
        {
            if (Rules.Phase == RunPhase.Title) return;
            foreach (var f in floaters)
            {
                var p = gameCamera.WorldToViewportPoint(f.pos);
                float width = f.text.Length > 5 ? 260 : 120;
                Vector2 canvas=PresentationLayout.ViewportToCanvas(p,Screen.width,Screen.height);
                float x=canvas.x-width/2,y=canvas.y-35;
                var style = f.big ? damageBig : damageSmall;
                var c = f.color; c.a = Mathf.Clamp01((.72f - f.age) * 5);
                style.normal.textColor = new Color(.1f,.08f,.1f,c.a);
                Text(x + 2, y + 2, width, 45, f.text, style);
                style.normal.textColor = c; Text(x, y, width, 45, f.text, style);
            }
        }
    }
}
