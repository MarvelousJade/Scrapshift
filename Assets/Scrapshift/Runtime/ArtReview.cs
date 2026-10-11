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
        bool leftSeen, rightSeen, flashSeen, recoilSeen, scatterSeen, settledSeen, magnetSeen, outfitAdded, outfitShop;
        int artStep;
        float idleScaleMin=10,idleScaleMax,walkScaleMin=10,walkScaleMax;

        [Serializable] sealed class ArtReport
        {
            public string status, scope;
            public int idleFrames, walkFrames, scrapShapes, shots;
            public int visibleItemAttachments, equippedWeapons;
            public bool leftFacing, rightFacing, muzzleFlash, recoil, scatter, settled, magnet;
            public float idleSpringRange,walkSpringRange;
        }

        void BeginArtReview(string folder)
        {
            artReview = true; artPath = folder; Directory.CreateDirectory(folder);
            reducedMotion = false; // Exercise bounce/recoil locally without changing the user's saved preference.
            Directory.CreateDirectory(Path.Combine(folder, "frames"));
            artStart = Time.realtimeSinceStartup;
            Begin();
            Rules.Inventory.Add(catalog.Weapon("smg", 2));
            SyncEquipment();
            for (int i = 0; i < 24; i++)
            {
                Drop(new Vector2(-3 + i % 6 * 1.2f, -2 + i / 6), 1, false);
                pickups[pickups.Count - 1].obj.GetComponent<SpriteRenderer>().sprite = catalog.scrapShapes[i % catalog.scrapShapes.Length];
            }
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
            gameCamera.orthographicSize = 3.8f;
            gameCamera.transform.position = new Vector3(playerPos.x, playerPos.y, -20);
            if (t > 10 && !outfitAdded)
            {
                outfitAdded = true;
                foreach (string id in new[] { "helmet", "glasses", "leather_vest", "cake", "blood_leech" })
                    Rules.Items.Add(Array.Find(catalog.data.items, i => i.id == id));
                Rules.Inventory.Clear();
                foreach (string id in new[] { "pistol", "smg", "double_barrel_shotgun", "laser_gun", "knife", "spear" })
                    Rules.Inventory.Add(catalog.Weapon(id, 2));
                SyncEquipment();
            }
            if (t > 13 && !outfitShop)
            {
                outfitShop = true;
                Rules.Tick(Rules.Remaining); ClearCombat();
                while (Rules.Phase == RunPhase.Loot) Rules.ResolveCrate(false);
                while (Rules.Phase == RunPhase.Upgrade) Rules.Choose(0);
            }
            if (playerAnimation.Moving)
            {
                walkSeen.Add(playerAnimation.Frame);
                walkScaleMin=Mathf.Min(walkScaleMin,playerAnimation.SpringPose.x);walkScaleMax=Mathf.Max(walkScaleMax,playerAnimation.SpringPose.x);
                leftSeen |= playerAnimation.FacingLeft; rightSeen |= !playerAnimation.FacingLeft;
            }
            else {idleSeen.Add(playerAnimation.Frame);idleScaleMin=Mathf.Min(idleScaleMin,playerAnimation.SpringPose.x);idleScaleMax=Mathf.Max(idleScaleMax,playerAnimation.SpringPose.x);}
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
            string[] names = { "idle.png", "walk-right.png", "walk-left.png", "outfit.png" };
            if (artStep < times.Length && t >= times[artStep])
                ScreenCapture.CaptureScreenshot(Path.Combine(artPath, names[artStep++]));
            if (artStep == 4 && t > 14)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(artPath, "outfit-shop.png"));
                artStep++;
            }
            if (t < 16) return;
            bool passed = idleSeen.Count == 1 && walkSeen.Count == 1 && idleScaleMax-idleScaleMin>.12f && walkScaleMax-walkScaleMin>.25f && shapesSeen.Count == 5
                && leftSeen && rightSeen && flashSeen && recoilSeen && scatterSeen && settledSeen && magnetSeen
                && wardrobe.VisibleCount == 5 && Rules.Inventory.Count == 6
                && File.Exists(Path.Combine(artPath, "outfit-shop.png"));
            var report = new ArtReport { status = passed ? "Passed" : "Failed",
                scope = "Staged Windows art fixture: supplied equipment, 24 drops spanning five shapes, six-weapon/five-item outfit, scripted idle/right/left movement, restored health, following close-up camera, reduced motion disabled only for this process. Outfit injected for visual coverage, not a purchase test. Screenshots inspected separately.",
                visibleItemAttachments = wardrobe.VisibleCount, equippedWeapons = Rules.Inventory.Count,
                idleFrames = idleSeen.Count, walkFrames = walkSeen.Count, idleSpringRange=idleScaleMax-idleScaleMin,walkSpringRange=walkScaleMax-walkScaleMin,scrapShapes = shapesSeen.Count, shots = shotsFired,
                leftFacing = leftSeen, rightFacing = rightSeen, muzzleFlash = flashSeen, recoil = recoilSeen,
                scatter = scatterSeen, settled = settledSeen, magnet = magnetSeen };
            File.WriteAllText(Path.Combine(artPath, "art-review.json"), JsonUtility.ToJson(report, true));
            Application.Quit(passed ? 0 : 1); enabled = false;
        }
    }
}
