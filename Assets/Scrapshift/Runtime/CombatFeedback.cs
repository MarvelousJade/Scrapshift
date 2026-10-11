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
        float recoil, hitStop, hurtOverlay;
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
            recoil = Mathf.Max(0, recoil - dt * 1.8f); hitStop = Mathf.Max(0, hitStop - dt);
            hurtOverlay = Mathf.Max(0, hurtOverlay - dt);
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
                float x = p.x * 1440 - 60, y = (1 - p.y) * 900 - 35;
                var style = f.big ? damageBig : damageSmall;
                var c = f.color; c.a = Mathf.Clamp01((.72f - f.age) * 5);
                style.normal.textColor = new Color(.1f,.08f,.1f,c.a);
                Text(x + 2, y + 2, 120, 45, f.text, style);
                style.normal.textColor = c; Text(x, y, 120, 45, f.text, style);
            }
            if (hurtOverlay > 0)
            {
                Color c = new Color(1, .18f, .1f, hurtOverlay * .8f);
                Box(new Rect(0, 100, 1440, 9), c); Box(new Rect(0, 791, 1440, 9), c);
                Box(new Rect(0, 100, 9, 700), c); Box(new Rect(1431, 100, 9, 700), c);
            }
        }
    }
}
