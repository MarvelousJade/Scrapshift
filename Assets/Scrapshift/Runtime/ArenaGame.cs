using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Scrapshift
{
    public sealed class ArenaGame : MonoBehaviour
    {
        public GameObject playerPrefab, scrapPrefab, sparkPrefab;
        public WeaponDefinition[] weapons;
        public EnemyDefinition[] enemyTypes;
        public Camera gameCamera;
        public GameObject titleModel;
        public RunRules Rules { get; } = new RunRules();
        sealed class Enemy { public GameObject obj; public EnemyDefinition type; public float hp, age, flash; public Vector2 pos; }
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
            player = Instantiate(playerPrefab); player.name = "Repair bot";
            pulse = Instantiate(weapons[2].effectPrefab); pulse.SetActive(false);
            for (int i = 0; i < 2; i++) { orbits[i] = Instantiate(weapons[1].effectPrefab); orbits[i].SetActive(false); }
            muted = PlayerPrefs.GetInt("muted", 0) != 0;
            reducedMotion = PlayerPrefs.GetInt("reducedMotion", 0) != 0;
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--smoke"); smoke = index >= 0;
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
            player.transform.position = Vector3.zero; player.transform.localScale = Vector3.one;
            foreach (var o in orbits) o.SetActive(false); pulse.SetActive(false);
        }

        void Update()
        {
            if (titleModel != null) titleModel.SetActive(Rules.Phase == RunPhase.Title);
            if (Input.GetKeyDown(KeyCode.M)) ToggleSound();
            if (Input.GetKeyDown(KeyCode.Escape)) Rules.Pause();
            if (Input.GetKeyDown(KeyCode.Return) && (Rules.Phase == RunPhase.Title || Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost)) Begin();
            if (Rules.Phase == RunPhase.Upgrade)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUpgrade(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUpgrade(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUpgrade(2);
                if (smoke) SelectUpgrade(Rules.UpgradeCount < 2 ? 0 : 2);
            }
            if (Rules.Phase == RunPhase.Playing) Simulate(Mathf.Min(Time.deltaTime, .05f));
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
            if (nearest != null && shootClock <= 0)
            {
                var direction = (nearest.pos - playerPos).normalized;
                var obj = Instantiate(weapons[0].effectPrefab);
                obj.transform.position = new Vector3(playerPos.x, playerPos.y, -2);
                obj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                shots.Add(new Shot { obj = obj, pos = playerPos, velocity = direction * 15, life = 1.4f, damage = weapons[0].damage * Rules.DamageScale });
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
            enemies.Add(new Enemy { obj = obj, type = def, hp = def.health * (1 + .12f * (Rules.Wave - 1)), pos = pos });
            peakEnemies = Mathf.Max(peakEnemies, enemies.Count);
        }
        void UpdateEnemies(float dt)
        {
            foreach (var e in enemies)
            {
                e.age += dt; e.flash = Mathf.Max(0, e.flash - dt);
                // A short spawn telegraph prevents immediate invisible edge damage.
                if (e.age > .6f) e.pos += (playerPos - e.pos).normalized * (e.type.speed * dt);
                e.obj.transform.position = new Vector3(e.pos.x, e.pos.y, -.5f);
                float scale = e.age < .6f ? Mathf.Lerp(.1f, 1, e.age / .6f) : 1;
                float squash = !reducedMotion ? Mathf.Sin(e.age * 12) * .035f : 0;
                e.obj.transform.localScale = new Vector3(scale + squash + e.flash, scale - squash + e.flash, 1);
                if (e.age > .6f && invulnerable <= 0 && RunRules.Touches(e.pos, playerPos, e.type.radius + .30f))
                {
                    Rules.Hurt(12); invulnerable = .9f; shake = .10f; Play(hitSound, 1);
                    Burst(playerPos, 8); e.pos += (e.pos - playerPos).normalized * 1.0f;
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
            var e = enemies[index]; e.hp -= damage; e.flash = .13f;
            if (e.hp > 0) return;
            Rules.Kill(); Burst(e.pos, 4);
            if (pickups.Count < 180)
            {
                var obj = Instantiate(scrapPrefab); obj.transform.position = new Vector3(e.pos.x, e.pos.y, -1.5f);
                pickups.Add(new Pickup { obj = obj, pos = e.pos, value = e.type.scrap });
            }
            Destroy(e.obj); enemies.RemoveAt(index);
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
        void Burst(Vector2 pos, int count)
        {
            for (int i = 0; i < count && sparks.Count < 200; i++)
            {
                var obj = Instantiate(sparkPrefab); obj.transform.position = new Vector3(pos.x, pos.y, -3);
                float life = UnityEngine.Random.Range(.15f, .4f);
                sparks.Add(new Spark { obj = obj, pos = pos, velocity = UnityEngine.Random.insideUnitCircle * 5, life = life, max = life });
            }
        }
        void UpdateSparks(float dt)
        {
            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                var s = sparks[i]; s.life -= dt; s.pos += s.velocity * dt;
                s.obj.transform.position = new Vector3(s.pos.x, s.pos.y, -3); s.obj.transform.localScale = Vector3.one * Mathf.Max(0, s.life / s.max);
                if (s.life <= 0) { Destroy(s.obj); sparks.RemoveAt(i); }
            }
        }
        void ClearCombat()
        {
            foreach (var e in enemies) Destroy(e.obj); enemies.Clear();
            foreach (var s in shots) Destroy(s.obj); shots.Clear();
            foreach (var p in pickups) Destroy(p.obj); pickups.Clear();
            foreach (var s in sparks) Destroy(s.obj); sparks.Clear();
        }
        void SelectUpgrade(int choice) { Rules.Choose(choice); invulnerable = 1; player.SetActive(true); Play(upgradeSound, .5f); }
        void ToggleSound() { muted = !muted; PlayerPrefs.SetInt("muted", muted ? 1 : 0); }
        void Play(AudioClip clip, float volume) { if (!muted && audioSource != null) audioSource.PlayOneShot(clip, volume); }
        static AudioClip Tone(float frequency, float duration)
        {
            const int rate = 22050; var samples = new float[(int)(rate * duration)];
            for (int i = 0; i < samples.Length; i++) { float t = (float)i / rate; samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t * (1 - t * 1.2f)) * Mathf.Sin(Mathf.PI * i / samples.Length) * .3f; }
            var clip = AudioClip.Create("Synth " + frequency, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }

        void EnsureStyles()
        {
            if (body != null) return;
            body = Style(20, Color.white); tiny = Style(13, new Color(.57f, .66f, .73f));
            heading = Style(28, Color.white, FontStyle.Bold); title = Style(76, Color.white, FontStyle.Bold);
            number = Style(48, mint, FontStyle.Bold);
            button = Style(18, mint, FontStyle.Bold); button.alignment = TextAnchor.MiddleCenter;
        }
        static GUIStyle Style(int size, Color color, FontStyle weight = FontStyle.Normal)
        {
            return new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight, normal = { textColor = color }, wordWrap = true };
        }
        static void Box(Rect rect, Color color) { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        void Text(float x, float y, float w, float h, string text, GUIStyle style) => GUI.Label(new Rect(x, y, w, h), text, style);
        bool Button(Rect r, string label)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Box(r, hover ? new Color(.15f, .29f, .28f) : new Color(.10f, .18f, .20f));
            Box(new Rect(r.x, r.y, 3, r.height), mint);
            return GUI.Button(r, label, button);
        }
        void OnGUI()
        {
            EnsureStyles(); float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1440 * scale) / 2, (Screen.height - 900 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            Box(new Rect(0, 0, 1440, 112), new Color(.025f, .045f, .065f, .96f));
            Text(42, 23, 300, 34, "SCRAPSHIFT", heading); Text(44, 67, 350, 24, "SALVAGE DIVISION  /  DECK 07", tiny);
            if (Rules.Phase != RunPhase.Title)
            {
                Text(500, 22, 200, 24, "HULL INTEGRITY", tiny);
                Box(new Rect(500, 55, 280, 12), new Color(.18f, .23f, .27f));
                Box(new Rect(500, 55, 280 * Rules.Health / Rules.MaxHealth, 12), Rules.Health > 30 ? mint : coral);
                Text(500, 76, 280, 25, $"{Mathf.CeilToInt(Rules.Health)} / {Rules.MaxHealth:0}     LEVEL {Rules.Level:00}", tiny);
                Text(890, 23, 200, 24, $"WAVE {Rules.Wave:00} / 05", tiny);
                Text(885, 46, 160, 60, $"{Mathf.CeilToInt(Rules.Remaining):00}", number);
                Text(1160, 23, 240, 24, "DRONES SCRAPPED", tiny); Text(1155, 46, 200, 60, Rules.Kills.ToString("000"), number);
            }
            Box(new Rect(0, 805, 1440, 95), new Color(.025f, .045f, .065f, .96f));
            Text(44, 824, 720, 25, "WASD / ARROWS   MOVE       ESC   PAUSE       M   SOUND", tiny);
            Text(44, 853, 820, 25, "RIVET GUN" + (Rules.OrbitUnlocked ? "   +   ORBIT BLADES" : "") + (Rules.PulseUnlocked ? "   +   PULSE CORE" : ""), body);
            Text(1070, 824, 325, 25, "RECOVER SCRAP TO UPGRADE", tiny);
            Box(new Rect(1070, 860, 320, 6), new Color(.18f, .23f, .27f));
            Box(new Rect(1070, 860, 320 * Mathf.Clamp01((float)Rules.Scrap / Rules.NextLevel), 6), amber);
            if (Rules.Phase == RunPhase.Title) TitleUI();
            else if (Rules.Phase == RunPhase.Upgrade) UpgradeUI();
            else if (Rules.Phase == RunPhase.Paused) PauseUI();
            else if (Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost) EndUI();
            if (smoke) Text(44, 118, 1000, 25, "AUTOMATED RUNTIME SMOKE  /  scripted movement", tiny);
        }
        void Panel(float x, float y, float w, float h)
        {
            Box(new Rect(0, 112, 1440, 693), new Color(.015f, .03f, .045f, .65f));
            Box(new Rect(x, y, w, h), new Color(.035f, .065f, .09f, .98f));
            Box(new Rect(x, y, w, 3), mint);
        }
        void TitleUI()
        {
            Panel(110, 190, 770, 485);
            Text(155, 226, 680, 30, "ONE BOT. ONE SHIFT. A LOT OF SCRAP.", tiny);
            Text(148, 275, 690, 95, "SCRAPSHIFT", title);
            Text(155, 387, 630, 88, "The ship is overrun. Keep moving, salvage fallen drones, and turn spare parts into firepower.", body);
            Text(155, 487, 640, 48, "Survive five 60-second waves. Your weapons fire automatically.", tiny);
            if (Button(new Rect(155, 560, 315, 62), "START SHIFT   [ENTER]")) Begin();
            Text(505, 568, 300, 45, "WASD / ARROWS TO MOVE\nCOLLECT THE GOLD SCRAP", tiny);
        }
        void UpgradeUI()
        {
            Panel(145, 220, 1150, 460);
            Text(190, 249, 950, 40, Rules.WaveUpgrade ? "SECTOR CLEAR. REFIT YOUR BOT." : "SALVAGE SECURED. CHOOSE AN UPGRADE.", heading);
            Text(190, 300, 950, 30, Rules.WaveUpgrade ? "The next wave brings stronger drones. Repair +15 after your choice." : "Simulation paused. Choose one upgrade, then get back to work.", tiny);
            string first = !Rules.OrbitUnlocked ? "ORBIT BLADES" : !Rules.PulseUnlocked ? "PULSE CORE" : "HOT RIVETS";
            string detail = !Rules.OrbitUnlocked ? "Two blades orbit your bot. Cut through nearby drones." : !Rules.PulseUnlocked ? "A periodic shockwave damages every drone nearby." : "Increase all weapon damage by 25 percentage points.";
            string[] names = { first, "OVERCLOCK", "FIELD REPAIR" };
            string[] details = { detail, "+18% base firing rate.\n+0.25 movement speed.", "+15 maximum hull. Repair 40.\nIncrease scrap pickup range." };
            for (int i = 0; i < 3; i++)
            {
                float x = 190 + i * 355; Box(new Rect(x, 353, 330, 210), new Color(.065f, .105f, .14f));
                Text(x + 20, 374, 290, 45, names[i], body); Text(x + 20, 431, 290, 100, details[i], body);
                if (Button(new Rect(x, 582, 330, 55), $"INSTALL  [{i + 1}]")) SelectUpgrade(i);
            }
        }
        void PauseUI()
        {
            Panel(465, 220, 510, 440); Text(510, 265, 430, 55, "SHIFT PAUSED", heading);
            if (Button(new Rect(510, 350, 420, 55), "RESUME   [ESC]")) Rules.Pause();
            if (Button(new Rect(510, 425, 420, 55), muted ? "SOUND: OFF" : "SOUND: ON")) ToggleSound();
            if (Button(new Rect(510, 500, 420, 55), reducedMotion ? "REDUCED MOTION: ON" : "REDUCED MOTION: OFF"))
            { reducedMotion = !reducedMotion; PlayerPrefs.SetInt("reducedMotion", reducedMotion ? 1 : 0); }
            Text(510, 580, 420, 35, "Your wave timer is stopped.", tiny);
        }
        void EndUI()
        {
            Panel(365, 225, 710, 440); bool won = Rules.Phase == RunPhase.Won;
            Text(410, 265, 630, 60, won ? "SHIFT COMPLETE" : "BOT OFFLINE", heading);
            Text(410, 342, 620, 100, won ? "Deck secured. Your little repair bot made it through the entire shift." : "Too many dents. Keep moving and collect scrap early to unlock another weapon.", body);
            Text(410, 456, 620, 50, $"{Rules.Kills} DRONES SCRAPPED    /    {Rules.UpgradeCount} UPGRADES", tiny);
            if (Button(new Rect(410, 544, 620, 62), "ANOTHER SHIFT   [ENTER]")) Begin();
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
            bool passed = shotsFired > 0 && Rules.Kills > 0 && peakEnemies > 0 && captured && t > screenshotAt + 1;
            var report = new SmokeReport { status = passed ? "Passed" : "Failed", phase = Rules.Phase.ToString(), kills = Rules.Kills,
                shots = shotsFired, peakEnemies = peakEnemies, upgrades = Rules.UpgradeCount, orbitHits = orbitHits, pulseHits = pulseHits,
                elapsed = elapsed, health = Rules.Health, scope = "24-second rendered Windows runtime; scripted normal-speed movement; no invulnerability override. Not a full-run balance or manual-input test." };
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
