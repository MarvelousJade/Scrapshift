using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool artReview;
        string artPath;
        float artStart, artCaptureAt;
        int artCapture;
        readonly HashSet<int> idleSeen = new HashSet<int>(), walkSeen = new HashSet<int>();
        readonly HashSet<string> shapesSeen = new HashSet<string>();
        bool leftSeen, rightSeen, flashSeen, recoilSeen, scatterSeen, settledSeen, magnetSeen;
        int artStep;

        [Serializable] sealed class ArtReport
        {
            public string status, scope;
            public int idleFrames, walkFrames, scrapShapes, shots;
            public bool leftFacing, rightFacing, muzzleFlash, recoil, scatter, settled, magnet;
        }

        void BeginArtReview(string folder)
        {
            artReview = true; artPath = folder; Directory.CreateDirectory(folder);
            Directory.CreateDirectory(Path.Combine(folder, "frames"));
            artStart = Time.realtimeSinceStartup;
            Begin();
            Rules.Inventory.Add(catalog.Weapon("smg", 2));
            SyncEquipment();
            for (int i = 0; i < 24; i++)
                Drop(new Vector2(-3 + i % 6 * 1.2f, -2 + i / 6), 1, false);
        }

        Vector2 ArtReviewMovement()
        {
            float t = Time.realtimeSinceStartup - artStart;
            if (t < 3) return Vector2.zero;
            if (t < 5) return Vector2.right * .3f;
            if (t < 7) return Vector2.left * .3f;
            if (t < 10) return Vector2.zero;
            return new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * .5f;
        }

        void ArtReviewTick()
        {
            if (!artReview) return;
            float t = Time.realtimeSinceStartup - artStart;
            Rules.Heal(100); // Staged art fixture only; never normal gameplay.
            gameCamera.orthographicSize = t < 10 ? 3.8f : 7.5f;
            if (playerAnimation.Moving)
            {
                walkSeen.Add(playerAnimation.Frame);
                leftSeen |= playerAnimation.FacingLeft; rightSeen |= !playerAnimation.FacingLeft;
            }
            else idleSeen.Add(playerAnimation.Frame);
            for (int i = 0; i < muzzleViews.Length; i++)
            {
                flashSeen |= muzzleViews[i] != null && muzzleViews[i].enabled;
                recoilSeen |= swings[i] > .04f && swings[i] < .16f;
            }
            foreach (var p in pickups)
            {
                if (!p.fruit) shapesSeen.Add(p.obj.GetComponent<SpriteRenderer>().sprite.name);
                scatterSeen |= p.motion.height > .12f && p.motion.velocity.sqrMagnitude > .1f;
                settledSeen |= p.motion.age > 1 && p.motion.height < .01f && !p.motion.magnetized;
                magnetSeen |= p.motion.magnetized;
            }
            if (t < 7.3f && t >= artCaptureAt)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(artPath, "frames", "frame-" + artCapture++.ToString("D3") + ".png"));
                artCaptureAt = t + 1f / 12;
            }
            float[] times = { 2.8f, 3.7f, 5.7f, 12 };
            string[] names = { "idle.png", "walk-right.png", "walk-left.png", "combat.png" };
            if (artStep < times.Length && t >= times[artStep])
                ScreenCapture.CaptureScreenshot(Path.Combine(artPath, names[artStep++]));
            if (t < 16) return;
            bool passed = idleSeen.Count == 4 && walkSeen.Count == 4 && shapesSeen.Count == 6
                && leftSeen && rightSeen && flashSeen && recoilSeen && scatterSeen && settledSeen && magnetSeen;
            var report = new ArtReport { status = passed ? "Passed" : "Failed",
                scope = "Staged Windows art fixture: supplied SMG, 24 drops, scripted idle/right/left movement, restored health, temporary camera close-up. Counts observed states, not perceptual quality or balance. Screenshots inspected separately.",
                idleFrames = idleSeen.Count, walkFrames = walkSeen.Count, scrapShapes = shapesSeen.Count, shots = shotsFired,
                leftFacing = leftSeen, rightFacing = rightSeen, muzzleFlash = flashSeen, recoil = recoilSeen,
                scatter = scatterSeen, settled = settledSeen, magnet = magnetSeen };
            File.WriteAllText(Path.Combine(artPath, "art-review.json"), JsonUtility.ToJson(report, true));
            Application.Quit(passed ? 0 : 1); enabled = false;
        }
    }
}
