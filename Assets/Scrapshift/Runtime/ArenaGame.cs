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
        sealed class Enemy { public GameObject obj, marker; public GroundShadow shadow; public EnemyEntry type; public float hp, age, flash, action; public Vector2 pos, knockback, charge; public Renderer[] renderers; public float speed, maxHp, cooldown, deathAge, burnClock,burnDamage,slowUntil; public int burnTicks; public bool boosted; public Vector2 wander; public EnemyAttack attack; public LineRenderer telegraph; public float activeAge, spacing, supportFlash, orbitAngle, contactSlow; public EnemyBehavior behavior; public Vector2 destination; public bool hasDestination, secondPhase; public int phaseCheck, orbitSign, gold, evolution; public GameObject[] orbitProjectiles; }
        sealed class Shot { public GameObject obj; public Vector2 pos, velocity; public float life, damage, crit, knockback; public int pierce,bounces; public bool hostile,secondary; public Equipment weapon; public EnemyEntry attacker; public readonly HashSet<Enemy> hit = new HashSet<Enemy>(); }
        sealed class Pickup { public GameObject obj; public ScrapMotion motion; public int value; public bool fruit; public ConsumableKind consumable; }
        sealed class Spark { public GameObject obj; public Vector2 pos, velocity; public float life, max; }
        readonly List<Enemy> enemies = new List<Enemy>();
        readonly List<Shot> shots = new List<Shot>();
        readonly List<Pickup> pickups = new List<Pickup>();
        readonly List<Spark> sparks = new List<Spark>();
        readonly GameObject[] orbits = new GameObject[2];
        GameObject player, pulse;
        SpriteFrameAnimator playerAnimation;
        GroundShadow playerShadow;
        CharacterWardrobe wardrobe;
        CharacterWardrobe titleWardrobe;
        Vector2 playerPos;
        float elapsed, spawnClock, shootClock, orbitClock, pulseClock, invulnerable, pulseAge, shake;
        float smokeStart, screenshotAt;
        bool smoke, captured, muted, reducedMotion;
        bool previewCapture;
        string smokePath;
        int peakEnemies, shotsFired, pulseHits, orbitHits;
        public SoundBank soundBank; public ReferenceCatalog catalog;
        GameAudio sound;
        RunPhase audioPhase = RunPhase.Title;
        GUIStyle tiny, body, heading, title, button, number;
        readonly Color mint = new Color(.48f, 1, .77f), amber = new Color(1, .76f, .36f), coral = new Color(1, .40f, .38f);

        void Start()
        {
            Application.targetFrameRate = 120; Rules.Configure(catalog);

            player = Instantiate(playerPrefab); player.name = "Scavenger";
            playerAnimation = player.GetComponent<SpriteFrameAnimator>();
            wardrobe = player.AddComponent<CharacterWardrobe>();
            if(titleModel!=null)titleWardrobe=titleModel.AddComponent<CharacterWardrobe>();
            playerShadow = AddGroundShadow(player, .9f, -.58f);
            gun = Instantiate(gunPrefab); gun.name = "Visible rivet pistol";
            pulse = Instantiate(weapons[2].effectPrefab); pulse.SetActive(false);
            for (int i = 0; i < 2; i++) { orbits[i] = Instantiate(weapons[1].effectPrefab); orbits[i].SetActive(false); }
            muted = PlayerPrefs.GetInt("muted", 0) != 0;
            sound = gameObject.AddComponent<GameAudio>(); sound.Initialize(soundBank, muted);
            reducedMotion = PlayerPrefs.GetInt("reducedMotion", 0) != 0;
            string[] args = Environment.GetCommandLineArgs();
            InitializeDisplay(args);
            int inspectIndex=Array.IndexOf(args,"--inspection-review");
            if(inspectIndex>=0&&inspectIndex+1<args.Length)BeginInspectionReview(args[inspectIndex+1]);
            int starterIndex=Array.IndexOf(args,"--starter-review");
            if(starterIndex>=0&&starterIndex+1<args.Length)BeginStarterReview(args[starterIndex+1]);
            int audioCheck = Array.IndexOf(args, "--audio-check");
            if (audioCheck >= 0 && audioCheck + 1 < args.Length)
                gameObject.AddComponent<AudioCheck>().Begin(sound, args[audioCheck + 1]);
            int index = Array.IndexOf(args, "--smoke"); smoke = index >= 0;
            int preview = Array.IndexOf(args, "--preview-capture");
            if (preview >= 0 && preview + 1 < args.Length)
            {
                previewCapture = true; smokePath = args[preview + 1]; Directory.CreateDirectory(smokePath);
            }
            int reviewIndex=Array.IndexOf(args,"--reference-review");
            if(reviewIndex>=0 && reviewIndex+1<args.Length) BeginReview(args[reviewIndex+1]);
            if (smoke)
            {
                smokePath = index + 1 < args.Length ? args[index + 1] : Application.persistentDataPath;
                Directory.CreateDirectory(smokePath); smokeStart = Time.realtimeSinceStartup;
                Begin();
            }
            int artIndex = Array.IndexOf(args, "--art-review");
            if (artIndex >= 0 && artIndex + 1 < args.Length) BeginArtReview(args[artIndex + 1]);
            int enemyIndex = Array.IndexOf(args, "--enemy-review");
            if (enemyIndex >= 0 && enemyIndex + 1 < args.Length) BeginEnemyReview(args[enemyIndex + 1]);
            int campaignIndex = Array.IndexOf(args, "--campaign-check");
            if(campaignIndex>=0&&campaignIndex+1<args.Length)BeginCampaignCheck(args[campaignIndex+1]);
            int lootIndex=Array.IndexOf(args,"--loot-review");
            if(lootIndex>=0&&lootIndex+1<args.Length)BeginLootReview(args[lootIndex+1]);
            int galleryIndex=Array.IndexOf(args,"--spring-gallery");
            if(galleryIndex>=0&&galleryIndex+1<args.Length)BeginSpringGallery(args[galleryIndex+1]);
            int presentationIndex=Array.IndexOf(args,"--presentation-review");
            if(presentationIndex>=0&&presentationIndex+1<args.Length)BeginPresentationReview(args[presentationIndex+1]);
            int feelIndex=Array.IndexOf(args,"--combat-feel-review");
            if(feelIndex>=0&&feelIndex+1<args.Length)BeginCombatFeelReview(args[feelIndex+1]);
            int worldIndex=Array.IndexOf(args,"--world-review");
            if(worldIndex>=0&&worldIndex+1<args.Length)BeginWorldReview(args[worldIndex+1]);
            int scaleIndex=Array.IndexOf(args,"--scale-review");
            if(scaleIndex>=0&&scaleIndex+1<args.Length)BeginScaleReview(args[scaleIndex+1]);
            int appearanceIndex=Array.IndexOf(args,"--appearance-review");
            if(appearanceIndex>=0&&appearanceIndex+1<args.Length)BeginAppearanceReview(args[appearanceIndex+1]);
        }

        public void Begin()
        {
            if(parachute!=null)Destroy(parachute);arrivalAge=ArrivalMotion.Duration;endSummary=false;lastAttacker=null;
            sound.StopAll(); sound.Play(SoundCue.Install);
            gameCamera.orthographicSize = ArenaGeometry.CameraHalfHeight;
            ClearCombat(); ghostKills.Clear(); Rules.Start(); StartWave(); playerPos = Vector2.zero;
            elapsed = spawnClock = shootClock = orbitClock = pulseClock = invulnerable = pulseAge = shake = 0;
            peakEnemies = shotsFired = pulseHits = orbitHits = 0;
            floaters.Clear();
            player.transform.position = Vector3.zero; player.transform.localScale = Vector3.one * ActorPresentation.PlayerScale;
            playerShadow.Place(playerPos, ActorPresentation.PlayerScale);
            foreach (var o in orbits) o.SetActive(false); pulse.SetActive(false);
        }

        void Update()
        {
            gameCamera.rect = PresentationLayout.Viewport(Screen.width, Screen.height);
            if (previewCapture && Time.realtimeSinceStartup > 3)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(smokePath, "preview.png")); previewCapture = false;
            }
            if (titleModel != null) titleModel.SetActive(false);
            gun.SetActive(false); SetEquipmentVisible(Rules.Phase == RunPhase.Playing || Rules.Phase == RunPhase.Paused || Rules.Phase==RunPhase.Won || Rules.Phase==RunPhase.Lost);
            if(Input.GetKeyDown(KeyCode.F11))ToggleFullscreen();
            if (Input.GetKeyDown(KeyCode.M)) ToggleSound();
            if (Input.GetKeyDown(KeyCode.Escape)) { if(Rules.Phase==RunPhase.Title)menuPage=menuPage==4?0:Mathf.Max(0,menuPage-1);else if(Rules.Phase==RunPhase.Paused&&pauseOptions)pauseOptions=false;else Rules.Pause(); }
            if (Input.GetKeyDown(KeyCode.Return) && (Rules.Phase == RunPhase.Title || Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost)) ConfirmMenu();
            if (Rules.Phase == RunPhase.Upgrade)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUpgrade(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUpgrade(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUpgrade(2);
                if (Input.GetKeyDown(KeyCode.Alpha4)) SelectUpgrade(3);
                if (Input.GetKeyDown(KeyCode.R)) RerollUpgrade();
                if (smoke) SelectUpgrade(Rules.UpgradeCount < 2 ? 0 : 2);
            }
            if (Rules.Phase == RunPhase.Shop) { if(Input.GetKeyDown(KeyCode.Return)) AdvanceWave(); if(Input.GetKeyDown(KeyCode.R)) RerollUpgrade(); }
            // Process after Shop input so taking the final crate cannot also start a wave.
            if(Rules.Phase==RunPhase.Loot)
            { if(Input.GetKeyDown(KeyCode.Return)||smoke)ResolveCrate(true);else if(Input.GetKeyDown(KeyCode.X))ResolveCrate(false); }
            if (Rules.Phase == RunPhase.Playing && !campaignCheck && !springGallery && !appearanceReview && !scaleReview && !combatFeelReview && !presentationReview && !starterReview && !inspectionReview) { float dt = Mathf.Min(Time.deltaTime, .05f); if(Arriving)TickArrival(dt);else {UpdateFeedback(dt); Simulate(dt);} }
            if (Rules.Phase != RunPhase.Playing && Rules.Phase != RunPhase.Paused && Rules.Phase != RunPhase.Won && Rules.Phase != RunPhase.Lost)
            { gameCamera.orthographicSize=7.5f; gameCamera.transform.position=new Vector3(0,0,-20); }
            if (Rules.Phase == RunPhase.Title && titleModel != null)
            {
                titleModel.GetComponent<SpriteFrameAnimator>().Tick(Vector2.zero, Time.deltaTime, reducedMotion);
                titleWardrobe.Sync(Array.Empty<ItemEntry>(),catalog.spriteMaterial,false);
            }
            if (Rules.Phase == RunPhase.Title) player.SetActive(false);
            if (audioPhase != Rules.Phase)
            {
                if (Rules.Phase != RunPhase.Playing) sound.StopCombat();
                if (Rules.Phase == RunPhase.Won) sound.Play(SoundCue.Win);
                if (Rules.Phase == RunPhase.Lost) sound.Play(SoundCue.Lose);
                audioPhase = Rules.Phase;
            }
            if (smoke) SmokeTick();
            wardrobe.Sync(Rules.Items, catalog.spriteMaterial, playerAnimation.FacingLeft);
            UpdatePlayerFlash(Rules.Phase == RunPhase.Playing ? Mathf.Min(Time.deltaTime, .05f) : 0);
            FinishInspectionReview();
            if(enabled)InspectionReviewTick();
            StarterReviewTick();
            ReviewTick();
            ArtReviewTick();
            EnemyReviewTick();
            CampaignCheckTick();
            LootReviewTick();
            SpringGalleryTick();
            AppearanceReviewTick();
            ScaleReviewTick();
            WorldReviewTick();
            CombatFeelReviewTick();
            PresentationReviewTick();
        }

        void Simulate(float dt)
        {
            var before=Rules.Phase;
            elapsed += dt; invulnerable = Mathf.Max(0, invulnerable - dt);
            Vector2 movement = new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
            if (smoke || review || campaignCheck) movement = new Vector2(Mathf.Cos(elapsed * .8f), Mathf.Sin(elapsed * .8f)) * .5f;
            if(worldReview) movement=WorldReviewMovement();
            if (artReview) movement = ArtReviewMovement();
            if (enemyReview || lootReview) movement = Vector2.zero;
            playerPos = RunRules.ClampPosition(playerPos + Vector2.ClampMagnitude(movement, 1) * (Rules.MoveSpeed * dt));
            player.transform.position = new Vector3(playerPos.x, playerPos.y, -1);
            playerAnimation.Tick(movement, dt, reducedMotion);
            player.transform.localScale = Vector3.one * ActorPresentation.PlayerScale;
            playerShadow.Place(playerPos, ActorPresentation.PlayerScale);
            player.transform.rotation = Quaternion.identity;
            player.SetActive(true);
            director.Tick(dt, QueueGroup, BeginSpawnGroup); UpdateBirthQueue(dt); UpdateEquipment(dt);
            UpdateEnemies(dt);
            UpdateWeapons(dt); UpdateStarterEffects(dt); UpdateSlashes(dt);
            UpdatePickups(dt);
            UpdateSparks(dt); UpdateCorpses(dt);
            shakeTime = Mathf.Max(0, shakeTime - dt);
            if (shakeTime <= 0) shake = 0;
            Vector2 cameraCenter = ArenaGeometry.CameraPosition(playerPos,gameCamera.aspect);
            gameCamera.transform.position = new Vector3(cameraCenter.x + (reducedMotion ? 0 : Mathf.Sin(elapsed * 111) * shake), cameraCenter.y + (reducedMotion ? 0 : Mathf.Cos(elapsed * 94) * shake), -20);
            if(Rules.Phase==RunPhase.Playing && Rules.Remaining<=dt)FinishWavePickups();
            float healthBefore=Rules.Health;Rules.Tick(dt);
            if(Rules.Health>healthBefore)FloatText(playerPos,$"+{Rules.Health-healthBefore:0} HP",mint,false);
            if (Rules.Phase != before && (Rules.Phase == RunPhase.Loot || Rules.Phase == RunPhase.Upgrade || Rules.Phase == RunPhase.Shop || Rules.Phase == RunPhase.Won)) { ClearCombat(); sound.Play(SoundCue.WaveClear); }
            if (Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost) player.SetActive(true);
        }

        public static float SegmentDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = ab.sqrMagnitude < .00001f ? 0 : Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(point, a + ab * t);
        }
        void UpdatePickups(float dt)
        {
            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                var p = pickups[i];
                p.motion.Step(dt, playerPos, p.fruit && Rules.Health>=Rules.MaxHealth ? -1 : Rules.PickupRadius, reducedMotion);
                Vector2 position = p.motion.position;
                p.obj.transform.position = new Vector3(position.x, position.y + p.motion.height, -1.5f);
                p.obj.transform.rotation = Quaternion.Euler(0, 0, p.motion.angle);
                if (p.motion.age > .16f && RunRules.Touches(position, playerPos, .3f))
                {
                    CollectPickup(p);Destroy(p.obj);pickups.RemoveAt(i);
                    if (Rules.Phase == RunPhase.Upgrade) { sound.Play(SoundCue.LevelUp); break; }
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
                s.obj.transform.position = new Vector3(s.pos.x, s.pos.y, -3); s.obj.transform.localScale = Vector3.one * Mathf.Max(0, s.life / s.max) * .9f;
                if (s.life <= 0) { Destroy(s.obj); sparks.RemoveAt(i); }
            }
        }
        void ClearCombat()
        {
            ClearReferenceCombat(); ClearImpacts(); floaters.Clear();
            foreach (var e in enemies) { Destroy(e.obj); if (e.marker != null) Destroy(e.marker); } enemies.Clear();
            foreach (var s in shots) Destroy(s.obj); shots.Clear();
            foreach (var p in pickups) Destroy(p.obj); pickups.Clear();
            foreach (var s in sparks) Destroy(s.obj); sparks.Clear();
        }
        void SelectUpgrade(int choice) { Rules.Choose(choice); FloatText(playerPos, "UPGRADED!", mint, true); Burst(playerPos, 24, mint); invulnerable = 1; player.SetActive(true); sound.Play(SoundCue.Install); }
        void ToggleSound() { muted = !muted; sound.SetMuted(muted); PlayerPrefs.SetInt("muted", muted ? 1 : 0); }
        void RerollUpgrade() { if (Rules.CanReroll) { Rules.Reroll(); sound.Play(SoundCue.Reroll); } }

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
    }
}
