using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Profiling;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [Serializable] sealed class PerformanceSample
        {
            public int enemies, frames, shotsFired, remainingShots;
            public bool allocationCounterValid;
            public double simulationMeanMs, simulationP95Ms, simulationMaxMs, allocatedBytesPerFrame;
            public double positionChecksum, healthChecksum;
        }
        [Serializable] sealed class PerformanceReport
        {
            public string scope = "Controlled headless Windows player CPU benchmark: 100/200 durable enemies, six tier-II weapons, fixed 60 Hz simulation, fixed seeds, 120 warmup + 360 measured steps. CPU times measure Simulate and UpdateFeedback only; excludes rendering, UI, startup and setup. Optional Unity allocation counter covers entire frames and is -1 if its positive control fails. Not a whole-game FPS measurement.";
            public string unity, cpu;
            public PerformanceSample[] samples;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartPerformanceReview()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--performance-review");
            if (index < 0 || index + 1 >= args.Length) return;
            var game = UnityEngine.Object.FindFirstObjectByType<ArenaGame>();
            if (game != null) game.StartCoroutine(game.PerformanceReview(args[index + 1]));
        }

        IEnumerator PerformanceReview(string folder)
        {
            // Start must finish creating scene dependencies before the controlled simulation begins.
            yield return null;
            enabled = false;
            Application.runInBackground = true;
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            Directory.CreateDirectory(folder);
            var results = new List<PerformanceSample>();
            foreach (int count in new[] { 100, 200 })
            {
                using var allocationRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
                var allocationProbe = new byte[65536];
                yield return null;
                bool allocationCounterValid = allocationRecorder.Valid && allocationRecorder.Count > 0 && allocationRecorder.LastValue >= allocationProbe.Length;
                GC.KeepAlive(allocationProbe);
                UnityEngine.Random.InitState(771);
                placementRandom = new System.Random(771);
                Begin();
                ClearCombat();
                Rules.SetShowcaseWave(19);
                Rules.Inventory.Clear();
                foreach (string id in new[] { "pistol", "smg", "double_barrel_shotgun", "laser_gun", "knife", "spear" })
                    Rules.Inventory.Add(catalog.Weapon(id, 2));
                SyncEquipment();
                director = new SpawnDirector(new WaveEntry { groups = Array.Empty<SpawnGroup>() }, 771);
                sound.SetMuted(true);
                titleModel?.SetActive(false);
                gun.SetActive(false);
                string[] kinds = { "baby_alien", "chaser", "charger", "spitter" };
                for (int i = 0; i < count; i++)
                {
                    float angle = i * 2.399963f, radius = 3 + i % 8;
                    Spawn(catalog.Enemy(kinds[i % kinds.Length]), new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                    var enemy = enemies[enemies.Count - 1];
                    Destroy(enemy.marker); enemy.marker = null; enemy.obj.SetActive(true);
                    enemy.hp = enemy.maxHp = 1000000;
                }
                // Let deferred destruction from setup finish before warmup and measurement.
                yield return null;
                const int warmup = 120, measured = 360;
                var times = new double[measured];
                long allocated = 0;
                for (int frame = 0; frame < warmup + measured; frame++)
                {
                    invulnerable = 1;
                    long start = Stopwatch.GetTimestamp();
                    UpdateFeedback(1f / 60);
                    Simulate(1f / 60);
                    long end = Stopwatch.GetTimestamp();
                    if (frame >= warmup)
                    {
                        times[frame - warmup] = (end - start) * 1000.0 / Stopwatch.Frequency;
                    }
                    yield return null;
                    if (frame >= warmup && allocationCounterValid) allocated += allocationRecorder.LastValue;
                }
                double sum = 0, positions = 0, health = 0;
                foreach (double time in times) sum += time;
                for (int i = 0; i < enemies.Count; i++)
                {
                    positions += (i + 1) * (enemies[i].pos.x + enemies[i].pos.y * 31.0);
                    health += enemies[i].hp;
                }
                Array.Sort(times);
                results.Add(new PerformanceSample { enemies = enemies.Count, frames = measured, shotsFired = shotsFired,
                    remainingShots = shots.Count, allocationCounterValid = allocationCounterValid, simulationMeanMs = sum / measured,
                    simulationP95Ms = times[(int)((measured - 1) * .95)], simulationMaxMs = times[measured - 1],
                    allocatedBytesPerFrame = allocationCounterValid ? allocated / (double)measured : -1, positionChecksum = positions, healthChecksum = health });
                File.WriteAllText(Path.Combine(folder, "performance.json"), JsonUtility.ToJson(new PerformanceReport {
                    unity = Application.unityVersion, cpu = SystemInfo.processorType, samples = results.ToArray() }, true));
            }
            Application.Quit(0);
        }
    }
}
