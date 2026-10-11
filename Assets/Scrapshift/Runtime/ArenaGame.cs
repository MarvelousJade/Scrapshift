using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame : MonoBehaviour
    {
        public GameObject playerPrefab, scrapPrefab, sparkPrefab, gunPrefab;
        public WeaponDefinition[] weapons;
        public EnemyDefinition[] enemyTypes;
        public Camera gameCamera;
        public GameObject titleModel;
        public RunRules Rules { get; } = new RunRules();
        sealed class Enemy { public GameObject obj, marker; public EnemyDefinition type; public float hp, age, flash, action; public Vector2 pos, knockback, charge; public Renderer[] renderers; }
        sealed class Shot { public GameObject obj; public Vector2 pos, velocity; public float life, damage; }
        sealed class Pickup { public GameObject obj; public Vector2 pos; public int value; }
        sealed class Spark { public GameObject obj; public Vector2 pos, velocity; public float life, max; }
        readonly List<Enemy> enemies = new List<Enemy>();
        readonly List<Shot> shots = new List<Shot>();
        readonly List<Pickup> pickups = new List<Pickup>();
        readonly List<Spark> sparks = new List<Spark>();
        readonly GameObject[] orbits = new GameObject[2];
        GameObject player, pulse;
        Vector2 playerPos;
        float elapsed, spawnClock, shootClock, orbitClock, pulseClock, invulnerable, pulseAge, shake;
        float smokeStart, screenshotAt;
        bool smoke, captured, muted, reducedMotion;
        bool previewCapture;
        string smokePath;
        int peakEnemies, shotsFired, pulseHits, orbitHits;
        AudioSource audioSource;
        AudioClip fireSound, hitSound, collectSound, upgradeSound;
        GUIStyle tiny, body, heading, title, button, number;
        readonly Color mint = new Color(.48f, 1, .77f), amber = new Color(1, .76f, .36f), coral = new Color(1, .40f, .38f);

        void Start()
        {
            Application.targetFrameRate = 120;
            audioSource = gameObject.AddComponent<AudioSource>(); audioSource.volume = .22f;
            fireSound = Tone(730, .055f); hitSound = Tone(130, .10f); collectSound = Tone(1250, .045f); upgradeSound = Tone(520, .24f);
            player = Instantiate(playerPrefab); player.name = "Scavenger";
            gun = Instantiate(gunPrefab); gun.name = "Visible rivet pistol";
            pulse = Instantiate(weapons[2].effectPrefab); pulse.SetActive(false);
            for (int i = 0; i < 2; i++) { orbits[i] = Instantiate(weapons[1].effectPrefab); orbits[i].SetActive(false); }
            muted = PlayerPrefs.GetInt("muted", 0) != 0;
            reducedMotion = PlayerPrefs.GetInt("reducedMotion", 0) != 0;
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--smoke"); smoke = index >= 0;
            int preview = Array.IndexOf(args, "--preview-capture");
            if (preview >= 0 && preview + 1 < args.Length)
            {
                previewCapture = true; smokePath = args[preview + 1]; Directory.CreateDirectory(smokePath);
            }
            if (smoke)
            {
                smokePath = index + 1 < args.Length ? args[index + 1] : Application.persistentDataPath;
                Directory.CreateDirectory(smokePath); smokeStart = Time.realtimeSinceStartup;
                Begin();
            }
        }

        public void Begin()
        {
            ClearCombat(); Rules.Start(); playerPos = Vector2.zero;
            elapsed = spawnClock = shootClock = orbitClock = pulseClock = invulnerable = pulseAge = shake = 0;
            peakEnemies = shotsFired = pulseHits = orbitHits = 0;
            floaters.Clear(); hitStop = hurtOverlay = 0;
            player.transform.position = Vector3.zero; player.transform.localScale = Vector3.one;
            foreach (var o in orbits) o.SetActive(false); pulse.SetActive(false);
        }

        void Update()
        {
            gameCamera.rect = PresentationLayout.Viewport(Screen.width, Screen.height);
            if (previewCapture && Time.realtimeSinceStartup > 3)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(smokePath, "preview.png")); previewCapture = false;
            }
            if (titleModel != null) titleModel.SetActive(Rules.Phase == RunPhase.Title);
            gun.SetActive(Rules.Phase != RunPhase.Title);
            if (Input.GetKeyDown(KeyCode.M)) ToggleSound();
            if (Input.GetKeyDown(KeyCode.Escape)) Rules.Pause();
            if (Input.GetKeyDown(KeyCode.Return) && (Rules.Phase == RunPhase.Title || Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost)) Begin();
            if (Rules.Phase == RunPhase.Upgrade)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUpgrade(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUpgrade(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUpgrade(2);
                if (Input.GetKeyDown(KeyCode.R)) Rules.Reroll();
                if (smoke) SelectUpgrade(Rules.UpgradeCount < 2 ? 0 : 2);
            }
            if (Rules.Phase == RunPhase.Playing) { float dt = Mathf.Min(Time.deltaTime, .05f); UpdateFeedback(dt); if (hitStop <= 0) Simulate(dt); }
            if (Rules.Phase == RunPhase.Title)
                player.transform.localScale = new Vector3(1 + Mathf.Sin(Time.time * 2) * .035f, 1 - Mathf.Sin(Time.time * 2) * .035f, 1);
            if (smoke) SmokeTick();
        }

        void Simulate(float dt)
        {
            elapsed += dt; invulnerable = Mathf.Max(0, invulnerable - dt);
            Vector2 movement = new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
            if (smoke) movement = new Vector2(Mathf.Cos(elapsed * .8f), Mathf.Sin(elapsed * .8f)) * .5f;
            playerPos = RunRules.ClampPosition(playerPos + Vector2.ClampMagnitude(movement, 1) * (Rules.MoveSpeed * dt));
            player.transform.position = new Vector3(playerPos.x, playerPos.y, -1);
            float bob = !reducedMotion && movement.sqrMagnitude > .01f ? Mathf.Sin(elapsed * 22) * .045f : 0;
            player.transform.localScale = new Vector3(1 + bob, 1 - bob, 1);
            player.transform.rotation = Quaternion.Euler(0, 0, reducedMotion ? 0 : -movement.x * 8);
            player.SetActive(invulnerable <= 0 || (int)(invulnerable * 16) % 2 == 0);
            spawnClock -= dt; shootClock -= dt; orbitClock -= dt; pulseClock -= dt;
            if (spawnClock <= 0 && enemies.Count < 140) { Spawn(); spawnClock = Mathf.Max(.15f, .78f - Rules.Wave * .10f - (60 - Rules.Remaining) * .003f); }
            Enemy nearest = null; float distance = weapons[0].range * weapons[0].range;
            foreach (var e in enemies)
            {
                float d = (e.pos - playerPos).sqrMagnitude;
                if (d < distance) { distance = d; nearest = e; }
            }
            if (nearest != null) gunDirection = (nearest.pos - playerPos).normalized;
            gun.transform.position = new Vector3(playerPos.x + gunDirection.x * (.6f - recoil), playerPos.y + gunDirection.y * (.6f - recoil), -2);
            gun.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(gunDirection.y, gunDirection.x) * Mathf.Rad2Deg);
            if (nearest != null && shootClock <= 0)
            {
                var direction = (nearest.pos - playerPos).normalized;
                var obj = Instantiate(weapons[0].effectPrefab);
                obj.transform.position = new Vector3(playerPos.x, playerPos.y, -2);
                obj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                shots.Add(new Shot { obj = obj, pos = playerPos, velocity = direction * 15, life = 1.4f, damage = weapons[0].damage * Rules.DamageScale });
                recoil = .14f; Burst(playerPos + direction * .85f, 3, amber);
                shootClock = weapons[0].interval / Rules.FireRate; shotsFired++; Play(fireSound, .28f);
            }
            UpdateEnemies(dt);
            UpdateWeapons(dt);
            UpdatePickups(dt);
            UpdateSparks(dt);
            shake = Mathf.Max(0, shake - dt * 2);
            gameCamera.transform.position = new Vector3(reducedMotion ? 0 : Mathf.Sin(elapsed * 111) * shake, reducedMotion ? 0 : Mathf.Cos(elapsed * 94) * shake, -20);
            var before = Rules.Phase; Rules.Tick(dt);
            if (Rules.Phase != before && Rules.Phase == RunPhase.Upgrade) { ClearCombat(); Play(upgradeSound, .8f); }
            if (Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost) player.SetActive(true);
        }

        void Spawn()
        {
            int type = Rules.Wave == 1 ? 0 : UnityEngine.Random.Range(0, Rules.Wave == 2 ? 2 : 3);
            var def = enemyTypes[type];
            bool vertical = UnityEngine.Random.value > .5f; float side = UnityEngine.Random.value > .5f ? 1 : -1;
            Vector2 pos = vertical ? new Vector2(side * 11.1f, UnityEngine.Random.Range(-5.2f, 5.2f)) : new Vector2(UnityEngine.Random.Range(-10.5f, 10.5f), side * 5.7f);
            var obj = Instantiate(def.prefab); obj.transform.position = new Vector3(pos.x, pos.y, -.5f);
            var marker = Instantiate(weapons[2].effectPrefab); marker.transform.position = new Vector3(pos.x, pos.y, -3);
            var tint = new MaterialPropertyBlock(); tint.SetColor("_Color", coral); marker.GetComponentInChildren<Renderer>().SetPropertyBlock(tint);
            enemies.Add(new Enemy { obj = obj, marker = marker, type = def, hp = def.health * (1 + .12f * (Rules.Wave - 1)), pos = pos, renderers = obj.GetComponentsInChildren<Renderer>() });
            peakEnemies = Mathf.Max(peakEnemies, enemies.Count);
        }
        void UpdateEnemies(float dt)
        {
            foreach (var e in enemies)
            {
                e.age += dt; e.flash = Mathf.Max(0, e.flash - dt);
                if (e.marker != null)
                {
                    e.marker.transform.localScale = Vector3.one * (1.5f - e.age);
                    if (e.age >= .6f) { Destroy(e.marker); e.marker = null; }
                }
                // A short spawn telegraph prevents immediate invisible edge damage.
                if (e.age > .6f)
                {
                    var toward = (playerPos - e.pos).normalized;
                    float speed = e.type.speed;
                    if (e.type == enemyTypes[1])
                    {
                        e.action += dt;
                        if (e.action < 1.5f) e.charge = toward;
                        else if (e.action < 2.05f) { speed = 0; Flash(e, .65f); }
                        else if (e.action < 2.5f) { toward = e.charge; speed = 6; }
                        else e.action = 0;
                    }
                    e.pos += (toward * speed + e.knockback) * dt;
                    e.knockback = Vector2.Lerp(e.knockback, Vector2.zero, 10 * dt);
                    e.pos = new Vector2(Mathf.Clamp(e.pos.x, -11.2f, 11.2f), Mathf.Clamp(e.pos.y, -5.6f, 5.6f));
                }
                if (!(e.type == enemyTypes[1] && e.action >= 1.5f && e.action < 2.05f)) Flash(e, e.flash > 0 ? 1 : 0);
                e.obj.transform.position = new Vector3(e.pos.x, e.pos.y, -.5f);
                float scale = e.age < .6f ? Mathf.Lerp(.1f, 1, e.age / .6f) : 1;
                float squash = !reducedMotion ? Mathf.Sin(e.age * 12) * .035f : 0;
                e.obj.transform.localScale = new Vector3(scale + squash + e.flash, scale - squash + e.flash, 1);
                if (e.age > .6f && invulnerable <= 0 && RunRules.Touches(e.pos, playerPos, e.type.radius + .30f))
                {
                    Rules.Hurt(12); hurtOverlay = .45f; FloatText(playerPos, "-12", coral, true); hitStop = reducedMotion ? 0 : .045f; invulnerable = .9f; shake = .10f; Play(hitSound, 1);
                    Burst(playerPos, 12, coral); e.pos += (e.pos - playerPos).normalized * 1.0f;
                }
            }
        }
        void UpdateWeapons(float dt)
        {
            for (int i = shots.Count - 1; i >= 0; i--)
            {
                var s = shots[i]; Vector2 old = s.pos; s.pos += s.velocity * dt; s.life -= dt;
                s.obj.transform.position = new Vector3(s.pos.x, s.pos.y, -2);
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var e = enemies[j]; if (e.age < .6f) continue;
                    if (SegmentDistance(e.pos, old, s.pos) < e.type.radius + .12f) { Damage(j, s.damage); s.life = 0; break; }
                }
                if (s.life <= 0) { Destroy(s.obj); shots.RemoveAt(i); }
            }
            for (int i = 0; i < 2; i++)
            {
                orbits[i].SetActive(Rules.OrbitUnlocked);
                float angle = elapsed * 2.8f + i * Mathf.PI;
                Vector2 p = playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f;
                orbits[i].transform.position = new Vector3(p.x, p.y, -2);
                orbits[i].transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                if (Rules.OrbitUnlocked && orbitClock <= 0)
                    for (int j = enemies.Count - 1; j >= 0; j--)
                        if (RunRules.Touches(p, enemies[j].pos, enemies[j].type.radius + .5f)) { Damage(j, weapons[1].damage * Rules.DamageScale); orbitHits++; }
            }
            if (orbitClock <= 0) orbitClock = weapons[1].interval / Rules.FireRate;
            if (Rules.PulseUnlocked && pulseClock <= 0)
            {
                pulseClock = weapons[2].interval / Rules.FireRate; pulseAge = .45f;
                pulse.transform.position = new Vector3(playerPos.x, playerPos.y, -1.8f); pulse.SetActive(true);
                for (int j = enemies.Count - 1; j >= 0; j--)
                    if (RunRules.Touches(playerPos, enemies[j].pos, weapons[2].range)) { Damage(j, weapons[2].damage * Rules.DamageScale); pulseHits++; }
                Play(upgradeSound, .12f);
            }
            if (pulseAge > 0) { pulseAge -= dt; pulse.transform.localScale = Vector3.one * (1 - pulseAge / .45f) * weapons[2].range * 2; }
            else pulse.SetActive(false);
        }
        public static float SegmentDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = ab.sqrMagnitude < .00001f ? 0 : Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(point, a + ab * t);
        }
        void Damage(int index, float damage)
        {
            var e = enemies[index];
            bool critical = UnityEngine.Random.value < Rules.CritChance;
            if (critical) damage *= 2;
            e.hp -= damage; e.flash = .10f;
            e.knockback = (e.pos - playerPos).normalized * (e.type == enemyTypes[2] ? 1.4f : 4.5f);
            Flash(e, 1); FloatText(e.pos, Mathf.CeilToInt(damage).ToString() + (critical ? "!" : ""), critical ? amber : Color.white, critical);
            Burst(e.pos, critical ? 10 : 5, critical ? amber : e.type.color);
            Play(hitSound, critical ? .36f : .14f);
            if (critical) { hitStop = reducedMotion ? 0 : .028f; shake = .045f; }
            if (e.hp > 0) return;
            Rules.Kill(); Burst(e.pos, 12, e.type.color);
            if (pickups.Count < 180)
            {
                var obj = Instantiate(scrapPrefab); obj.transform.position = new Vector3(e.pos.x, e.pos.y, -1.5f);
                pickups.Add(new Pickup { obj = obj, pos = e.pos, value = e.type.scrap });
            }
            if (e.marker != null) Destroy(e.marker); Destroy(e.obj); enemies.RemoveAt(index);
        }
        void UpdatePickups(float dt)
        {
            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                var p = pickups[i]; p.obj.transform.Rotate(0, 0, 80 * dt);
                if (RunRules.Touches(p.pos, playerPos, Rules.PickupRadius)) p.pos = Vector2.MoveTowards(p.pos, playerPos, dt * 10);
                p.obj.transform.position = new Vector3(p.pos.x, p.pos.y, -1.5f);
                if (RunRules.Touches(p.pos, playerPos, .4f))
                {
                    Rules.Collect(p.value); Play(collectSound, .22f); Destroy(p.obj); pickups.RemoveAt(i);
                    if (Rules.Phase == RunPhase.Upgrade) { Play(upgradeSound, .65f); break; }
                }
            }
        }
        void Burst(Vector2 pos, int count, Color tint)
        {
            for (int i = 0; i < count && sparks.Count < 200; i++)
            {
                var obj = Instantiate(sparkPrefab); obj.transform.position = new Vector3(pos.x, pos.y, -3);
                var renderer = obj.GetComponentInChildren<Renderer>();
                var block = new MaterialPropertyBlock(); block.SetColor("_Color", tint); renderer.SetPropertyBlock(block);
                obj.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
                float life = UnityEngine.Random.Range(.20f, .48f);
                sparks.Add(new Spark { obj = obj, pos = pos, velocity = UnityEngine.Random.insideUnitCircle * 5, life = life, max = life });
            }
        }
        void UpdateSparks(float dt)
        {
            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                var s = sparks[i]; s.life -= dt; s.pos += s.velocity * dt;
                s.obj.transform.position = new Vector3(s.pos.x, s.pos.y, -3); s.obj.transform.localScale = Vector3.one * Mathf.Max(0, s.life / s.max) * 2.4f;
                if (s.life <= 0) { Destroy(s.obj); sparks.RemoveAt(i); }
            }
        }
        void ClearCombat()
        {
            foreach (var e in enemies) { Destroy(e.obj); if (e.marker != null) Destroy(e.marker); } enemies.Clear();
            foreach (var s in shots) Destroy(s.obj); shots.Clear();
            foreach (var p in pickups) Destroy(p.obj); pickups.Clear();
            foreach (var s in sparks) Destroy(s.obj); sparks.Clear();
        }
        void SelectUpgrade(int choice) { Rules.Choose(choice); FloatText(playerPos, "UPGRADED!", mint, true); Burst(playerPos, 24, mint); invulnerable = 1; player.SetActive(true); Play(upgradeSound, .5f); }
        void ToggleSound() { muted = !muted; PlayerPrefs.SetInt("muted", muted ? 1 : 0); }
        void Play(AudioClip clip, float volume) { if (!muted && audioSource != null) audioSource.PlayOneShot(clip, volume); }
        static AudioClip Tone(float frequency, float duration)
        {
            const int rate = 22050; var samples = new float[(int)(rate * duration)];
            for (int i = 0; i < samples.Length; i++) { float t = (float)i / rate; samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t * (1 - t * 1.2f)) * Mathf.Sin(Mathf.PI * i / samples.Length) * .3f; }
            var clip = AudioClip.Create("Synth " + frequency, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }

        [Serializable] sealed class SmokeReport
        {
            public string status, phase, scope;
            public int kills, shots, peakEnemies, upgrades, orbitHits, pulseHits;
            public float elapsed, health;
        }
        void SmokeTick()
        {
            float t = Time.realtimeSinceStartup - smokeStart;
            if (!captured && t > 12)
            {
                captured = true; screenshotAt = t;
                ScreenCapture.CaptureScreenshot(Path.Combine(smokePath, "gameplay.png"));
            }
            if (t < 24) return;
            bool passed = shotsFired > 0 && Rules.Kills > 0 && peakEnemies > 0 && captured && t > screenshotAt + 1
                && File.Exists(Path.Combine(smokePath, "gameplay.png"));
            var report = new SmokeReport { status = passed ? "Passed" : "Failed", phase = Rules.Phase.ToString(), kills = Rules.Kills,
                shots = shotsFired, peakEnemies = peakEnemies, upgrades = Rules.UpgradeCount, orbitHits = orbitHits, pulseHits = pulseHits,
                elapsed = elapsed, health = Rules.Health, scope = "24-second Windows runtime; scripted normal-speed movement; no invulnerability override. Screenshot existence checked; pixel content requires separate visual validation. Not a full-run balance or manual-input test." };
            File.WriteAllText(Path.Combine(smokePath, "runtime-smoke.json"), JsonUtility.ToJson(report, true));
            Application.Quit(passed ? 0 : 1); enabled = false;
        }
        void OnDestroy()
        {
            if (fireSound != null) Destroy(fireSound); if (hitSound != null) Destroy(hitSound);
            if (collectSound != null) Destroy(collectSound); if (upgradeSound != null) Destroy(upgradeSound);
        }
    }
}
