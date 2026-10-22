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
        sealed class Enemy { public GameObject obj, marker; public GroundShadow shadow; public EnemyEntry type; public float hp, age, flash, action; public Vector2 pos, knockback, charge; public Renderer[] renderers; public float speed, maxHp, cooldown, deathAge, burnClock,burnDamage,slowUntil; public int burnTicks,burnSpread; public bool boosted,iceApplied;public float iceClock,icePercent;public string burnStat; public Vector2 wander; public EnemyAttack attack; public LineRenderer telegraph; public float activeAge, spacing, supportFlash, orbitAngle, contactSlow,itemSlow; public EnemyBehavior behavior; public Vector2 destination; public bool hasDestination, secondPhase; public int phaseCheck, orbitSign, gold, evolution; public GameObject[] orbitProjectiles;public float[] orbitDistances;public bool[] orbitPlayerContacts;public HashSet<Pet>[] orbitPetContacts;public float moveRange,moveUpdate,boostDamage=25;public Pet targetPet;public bool targetInitialized;public float targetUpdate; }
        sealed class Shot { public GameObject obj; public Vector2 pos, velocity,sinusoidal,sinSpeed; public float age; public float life, damage, crit, knockback; public int pierce,bounces;public int weaponIndex=-1; public BurnPayload burnPayload; public bool hostile,secondary,structure,pillar;public StructureSpec structureSpec;public ProjectileReference projectileRef;public WeaponProjectileView projectileView;public int critBounces,critPierces;public bool projectileStopped,deleteOnOwnerDeath,delayedHitboxPending; public float hitDelay,slow; public readonly HashSet<Enemy> slowed=new HashSet<Enemy>(); public Equipment weapon; public EnemyEntry attacker;public Enemy sourceEnemy; public readonly HashSet<Enemy> hit = new HashSet<Enemy>(); }
        sealed class Pickup { public GameObject obj; public ScrapMotion motion; public Pet petCollector;public Enemy enemyCollector; public int value; public bool fruit; public ConsumableKind consumable;public float petAttractSpeed=5; }
        sealed class Spark { public GameObject obj; public Vector2 pos, velocity,sinusoidal,sinSpeed; public float age; public float life, max, scale; }
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
        bool? activePlayerContactBatch;
        bool PlayerHurtboxActive=>activePlayerContactBatch??(invulnerable<=0);
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
            SupportArt.Apply(catalog);
            InitializeProgression();WeaponReference.Apply(catalog);ApplyAudioSettings();Application.targetFrameRate = GameSettings.Flag("limitFps",false)?60:120; Rules.Configure(catalog);
            Rules.CaptureRandomDamageTarget=CaptureImplantDamageTarget;
            gameObject.AddComponent<GameCursor>().Initialize(this);

            player = Instantiate(playerPrefab); player.name = "Scavenger";
            playerAnimation = player.GetComponent<SpriteFrameAnimator>();
            wardrobe = player.AddComponent<CharacterWardrobe>();
            if(titleModel!=null)titleWardrobe=titleModel.AddComponent<CharacterWardrobe>();
            playerShadow = AddGroundShadow(player, .9f, -.58f);
            gun = Instantiate(gunPrefab); gun.name = "Visible rivet pistol";
            pulse = Instantiate(weapons[2].effectPrefab); pulse.SetActive(false);
            for (int i = 0; i < 2; i++) { orbits[i] = Instantiate(weapons[1].effectPrefab); orbits[i].SetActive(false); }
            muted = !GameSettings.IsFixture&&PlayerPrefs.GetInt("muted", 0) != 0;
            sound = gameObject.AddComponent<GameAudio>(); sound.Initialize(soundBank, muted);
            music=gameObject.AddComponent<GameMusic>();music.Initialize();
            reducedMotion = !GameSettings.IsFixture&&PlayerPrefs.GetInt("reducedMotion", 0) != 0;
            string[] args = Environment.GetCommandLineArgs();
            int appearanceParityIndex=Array.IndexOf(args,"--appearance-parity-review");
            if(appearanceParityIndex>=0&&appearanceParityIndex+1<args.Length)BeginAppearanceParityReview(args[appearanceParityIndex+1]);
            int itemIconsIndex=Array.IndexOf(args,"--item-icons-review");
            if(itemIconsIndex>=0&&itemIconsIndex+1<args.Length)BeginItemIconReview(args[itemIconsIndex+1]);
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
            int supportIndex=Array.IndexOf(args,"--support-motion-review");if(supportIndex>=0&&supportIndex+1<args.Length)BeginSupportReview(args[supportIndex+1]);
            int weaponReviewIndex=Array.IndexOf(args,"--weapon-fidelity-review");if(weaponReviewIndex>=0&&weaponReviewIndex+1<args.Length)BeginWeaponFidelityReview(args[weaponReviewIndex+1]);
            int rocketReviewIndex=Array.IndexOf(args,"--rocket-explosion-review");if(rocketReviewIndex>=0&&rocketReviewIndex+1<args.Length)BeginRocketExplosionReview(args[rocketReviewIndex+1]);
            int explosionParityIndex=Array.IndexOf(args,"--explosion-parity-review");if(explosionParityIndex>=0&&explosionParityIndex+1<args.Length)BeginExplosionParityReview(args[explosionParityIndex+1]);
            int projectileParityIndex=Array.IndexOf(args,"--projectile-parity-review");if(projectileParityIndex>=0&&projectileParityIndex+1<args.Length)BeginProjectileParityReview(args[projectileParityIndex+1]);
            int petIndex=Array.IndexOf(args,"--pet-review");if(petIndex>=0&&petIndex+1<args.Length)BeginPetReview(args[petIndex+1]);
            int artIndex = Array.IndexOf(args, "--art-review");
            if (artIndex >= 0 && artIndex + 1 < args.Length) BeginArtReview(args[artIndex + 1]);
            int fidelityIndex=Array.IndexOf(args,"--enemy-fidelity-review");if(fidelityIndex>=0&&fidelityIndex+1<args.Length)BeginEnemyFidelityReview(args[fidelityIndex+1]);
            int enemyIndex = Array.IndexOf(args, "--enemy-review");
            if (enemyIndex >= 0 && enemyIndex + 1 < args.Length) BeginEnemyReview(args[enemyIndex + 1]);
            int campaignIndex = Array.IndexOf(args, "--campaign-check");
            if(campaignIndex>=0&&campaignIndex+1<args.Length)BeginCampaignCheck(args[campaignIndex+1]);
            int controllerIndex=Array.IndexOf(args,"--controller-check");
            if(controllerIndex>=0&&controllerIndex+1<args.Length)BeginControllerCheck(args[controllerIndex+1]);
            int lootIndex=Array.IndexOf(args,"--loot-review");
            if(lootIndex>=0&&lootIndex+1<args.Length)BeginLootReview(args[lootIndex+1]);
            int economyIndex=Array.IndexOf(args,"--economy-native-review");
            if(economyIndex>=0&&economyIndex+1<args.Length)BeginEconomyNativeReview(args[economyIndex+1]);
            int galleryIndex=Array.IndexOf(args,"--spring-gallery");
            if(galleryIndex>=0&&galleryIndex+1<args.Length)BeginSpringGallery(args[galleryIndex+1]);
            int presentationIndex=Array.IndexOf(args,"--presentation-review");
            if(presentationIndex>=0&&presentationIndex+1<args.Length)BeginPresentationReview(args[presentationIndex+1]);
            int winReviewIndex=Array.IndexOf(args,"--win-review");
            if(winReviewIndex>=0&&winReviewIndex+1<args.Length)BeginWinReview(args[winReviewIndex+1]);
            int feelIndex=Array.IndexOf(args,"--combat-feel-review");
            if(feelIndex>=0&&feelIndex+1<args.Length)BeginCombatFeelReview(args[feelIndex+1]);
            int worldIndex=Array.IndexOf(args,"--world-review");
            if(worldIndex>=0&&worldIndex+1<args.Length)BeginWorldReview(args[worldIndex+1]);
            int scaleIndex=Array.IndexOf(args,"--scale-review");
            if(scaleIndex>=0&&scaleIndex+1<args.Length)BeginScaleReview(args[scaleIndex+1]);
            int appearanceIndex=Array.IndexOf(args,"--appearance-review");
            if(appearanceIndex>=0&&appearanceIndex+1<args.Length)BeginAppearanceReview(args[appearanceIndex+1]);
#if !UNITY_WEBGL
            if(Array.IndexOf(args,"--support-showcase")>=0)BeginSupportShowcase();
            int loadoutIndex=Array.IndexOf(args,"--loadout-showcase");
            if(loadoutIndex>=0&&loadoutIndex+1<args.Length)BeginLoadoutShowcase(ShowcaseLoadouts.Index(args[loadoutIndex+1]));
            if(loadoutShowcase>=0&&Array.IndexOf(args,"--showcase-clean")>=0)showcaseCleanFrame=true;
#endif
        }

        public void Begin()
        {
            combatAccumulator=0;deathSweepStarted=lossSoundPlayed=false;
            wardrobe?.Reset();ClearVictoryRing();
                        ArenaGeometry.SetMapSize(0);ScaleMapArt();waveSweeping=false;waveSweepAge=0;waveLettersRemaining=0;selectedWeapon=-1;
            if(parachute!=null)Destroy(parachute);arrivalAge=ArrivalMotion.Duration;endSummary=false;lastAttacker=null;
            sound.StopAll(); sound.Play(SoundCue.Install);
            gameCamera.orthographicSize = ArenaGeometry.CameraHalfHeight;
            ClearCombat(); ghostKills.Clear(); playerPos = CombatGeometry.PlayerBodyStart; Rules.Start(); StartWave();
            elapsed = spawnClock = shootClock = orbitClock = pulseClock = invulnerable = pulseAge = shake = 0;
            peakEnemies = shotsFired = pulseHits = orbitHits = 0;
            floaters.Clear();
            player.transform.position = playerPos; player.transform.localScale = Vector3.one * ActorPresentation.PlayerScale;
            playerShadow.Place(playerPos, ActorPresentation.PlayerScale);
            foreach (var o in orbits) o.SetActive(false); pulse.SetActive(false);
        }

        double combatAccumulator;
        // Set only by the opt-in deterministic performance fixture.
        bool performanceInputIsolation;
        void Update()
        {
            CombatInput.Poll();
            TickMenuNavigation();
            if(appearanceParity){AppearanceParityTick();return;}
            if(itemIconReview){ItemIconReviewTick();return;}
#if !UNITY_WEBGL
            if(supportShowcase&&Input.GetKeyDown(KeyCode.F5))BeginSupportShowcase();
            if(loadoutShowcase>=0&&Input.GetKeyDown(KeyCode.F5))BeginLoadoutShowcase(loadoutShowcase);
            if(Input.GetKeyDown(KeyCode.F6))BeginLoadoutShowcase(0);
            if(Input.GetKeyDown(KeyCode.F7))BeginLoadoutShowcase(1);
            if(Input.GetKeyDown(KeyCode.F9))BeginLoadoutShowcase(2);
            if(Input.GetKeyDown(KeyCode.F10))BeginLoadoutShowcase(3);
            if(loadoutShowcase>=0&&Input.GetKeyDown(KeyCode.F8))showcaseCleanFrame=!showcaseCleanFrame;
#endif
#if UNITY_WEBGL && SCRAPSHIFT_WEB_REVIEW
            TickWebReview();
#endif
            if(loadoutShowcase<0)TickProgression(Time.unscaledDeltaTime);
            music.SetPhase(Rules.Phase);
            gameCamera.rect = PresentationLayout.Viewport(Screen.width, Screen.height);
            if (previewCapture && Time.realtimeSinceStartup > 3)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(smokePath, "preview.png")); previewCapture = false;
            }
            TickAmbientPresentation();
            if (titleModel != null) titleModel.SetActive(false);
            gun.SetActive(false); SetEquipmentVisible((Rules.Phase == RunPhase.Playing || Rules.Phase == RunPhase.Paused || Rules.Phase==RunPhase.Won)&&Rules.Health>0);
#if !UNITY_WEBGL || UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.F11))ToggleFullscreen();
#endif
            if (Input.GetKeyDown(KeyCode.M)) ToggleSound();
            if (Input.GetKeyUp(KeyCode.Escape)||Input.GetKeyUp(KeyCode.P)||CombatInput.PauseReleased||CombatInput.CancelReleased&&(Rules.Phase==RunPhase.Title||Rules.Phase==RunPhase.Paused||selectedWeapon>=0)) { sound.Play(SoundCue.UiPress); if(Rules.Phase==RunPhase.Title)menuPage=menuPage>=4?0:Mathf.Max(0,menuPage-1);else if(Rules.Phase==RunPhase.Paused&&pauseOptions)pauseOptions=false;else if(Rules.Phase==RunPhase.Paused&&pauseCodex)pauseCodex=false;else if(selectedWeapon>=0)selectedWeapon=-1;else Rules.Pause(); }
            TickControllerShortcuts(Time.unscaledDeltaTime);
            if (!menuNavigationActive&&Input.GetKeyDown(KeyCode.Return) && (Rules.Phase == RunPhase.Title || Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost)) ConfirmMenu();
            if (Rules.Phase == RunPhase.Upgrade)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUpgrade(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUpgrade(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUpgrade(2);
                if (Input.GetKeyDown(KeyCode.Alpha4)) SelectUpgrade(3);
                if ((Input.GetKeyDown(KeyCode.R)||Input.GetKeyDown(KeyCode.F))) RerollUpgrade();
                if (smoke) SelectUpgrade(Rules.UpgradeCount < 2 ? 0 : 2);
            }
            if (Rules.Phase == RunPhase.Shop && selectedWeapon<0) { if(!menuNavigationActive&&Input.GetKeyDown(KeyCode.Return)) AdvanceWave(); if((Input.GetKeyDown(KeyCode.R)||Input.GetKeyDown(KeyCode.F))) RerollUpgrade(); }
            // Process after Shop input so taking the final crate cannot also start a wave.
            if(Rules.Phase==RunPhase.Loot)
            { if(!menuNavigationActive&&Input.GetKeyDown(KeyCode.Return)||smoke)ResolveCrate(true);else if(Input.GetKeyDown(KeyCode.X))ResolveCrate(false); }
            if (Rules.Phase == RunPhase.Playing && !economyNativeReview && !campaignCheck && !springGallery && !appearanceReview && !scaleReview && !combatFeelReview && !presentationReview && !starterReview && !petReview && !weaponFidelityReview && !rocketExplosionReview && !explosionParityReview && !projectileParityReview && !enemyFidelityReview && !inspectionReview && supportReviewPath==null)
            {
                int steps=ReferencePhysicsClock.Consume(ref combatAccumulator,Time.deltaTime);
                for(int step=0;step<steps&&Rules.Phase==RunPhase.Playing;step++)
                {
                    const float dt=ReferencePhysicsClock.Step;
                    if(waveSweeping)TickWaveSweep(dt);else {UpdateFeedback(dt);Simulate(dt);AdvanceArrivalPresentation(dt);}
                }
            }
            else combatAccumulator=0;
            TickDeathSweep(Time.deltaTime);
            if (Rules.Phase == RunPhase.Title)
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
                if (Rules.Phase == RunPhase.Won) endSummary=true;
                if (Rules.Phase == RunPhase.Lost&&!lossSoundPlayed){lossSoundPlayed=true;sound.Play(SoundCue.Lose);}
                audioPhase = Rules.Phase;
            }
            TickEndAudio(Time.deltaTime);
            if (smoke) SmokeTick();
            foreach(var pet in pets)if(pet.reviveSound!=null){pet.reviveSound.volume=sound.Muted?0:sound.Volume;if(Rules.Phase!=RunPhase.Playing)pet.reviveSound.Stop();}
            foreach(var pet in pets)if(pet.obj!=null)foreach(var renderer in pet.obj.GetComponentsInChildren<SpriteRenderer>()){var c=renderer.color;c.a=GameSettings.Value("petOpacity");renderer.color=c;}
            foreach(var shot in shots)if(shot.obj!=null){var renderer=shot.obj.GetComponent<SpriteRenderer>();if(renderer!=null){var c=renderer.color;c.a=GameSettings.Value("projectileOpacity");renderer.color=c;}}
            wardrobe.Sync(Rules.Items, catalog.spriteMaterial, playerAnimation.FacingLeft, GameSettings.Flag("hideItems",false));
            UpdatePlayerFlash(Rules.Phase == RunPhase.Playing ? Mathf.Min(Time.deltaTime, .05f) : 0);
            FinishInspectionReview();
            if(enabled)InspectionReviewTick();
            SupportReviewTick();PetReviewTick();WeaponFidelityReviewTick();StarterReviewTick();
            RocketExplosionReviewTick();
            ExplosionParityReviewTick();
            ProjectileParityReviewTick();
            ReviewTick();
            ArtReviewTick();
            EnemyReviewTick();EnemyFidelityReviewTick();
            CampaignCheckTick();
            LootReviewTick();
            EconomyNativeReviewTick();
            SpringGalleryTick();
            AppearanceReviewTick();
            ScaleReviewTick();
            WorldReviewTick();
            CombatFeelReviewTick();
            PresentationReviewTick();
            TickControllerCheck();WinReviewTick();
        }

        static readonly Unity.Profiling.ProfilerMarker ProfileEquipment=new Unity.Profiling.ProfilerMarker("Scrapshift.Equipment");
        static readonly Unity.Profiling.ProfilerMarker ProfileEnemies=new Unity.Profiling.ProfilerMarker("Scrapshift.Enemies");
        static readonly Unity.Profiling.ProfilerMarker ProfileProjectiles=new Unity.Profiling.ProfilerMarker("Scrapshift.Projectiles");
        static readonly Unity.Profiling.ProfilerMarker ProfileStarterEffects=new Unity.Profiling.ProfilerMarker("Scrapshift.StarterEffects");
        static readonly Unity.Profiling.ProfilerMarker ProfileSlashes=new Unity.Profiling.ProfilerMarker("Scrapshift.Slashes");
        static readonly Unity.Profiling.ProfilerMarker ProfileImplantAttacks=new Unity.Profiling.ProfilerMarker("Scrapshift.ImplantAttacks");
        static readonly Unity.Profiling.ProfilerMarker ProfilePets=new Unity.Profiling.ProfilerMarker("Scrapshift.Pets");
        static readonly Unity.Profiling.ProfilerMarker ProfileNightmare=new Unity.Profiling.ProfilerMarker("Scrapshift.Nightmare");
        static readonly Unity.Profiling.ProfilerMarker ProfilePickups=new Unity.Profiling.ProfilerMarker("Scrapshift.Pickups");
        static readonly Unity.Profiling.ProfilerMarker ProfileRules=new Unity.Profiling.ProfilerMarker("Scrapshift.Rules");
        void Simulate(float dt)
        {
            try{SimulateCombatStep(dt);}
            finally{ResolveCombatCallbacks();RefreshRuntimeCensus();}
        }
        void RefreshRuntimeCensus()
        {
            int livingEnemies=0,livingTrees=0,burningEnemies=0;
            foreach(var e in enemies){if(e.marker==null){if(e.type.role=="Tree")livingTrees++;else livingEnemies++;}if(e.burnTicks>0)burningEnemies++;}
            Rules.LivingEnemies=livingEnemies;Rules.LivingTrees=livingTrees;Rules.BurningEnemies=burningEnemies;Rules.StructureCount=starterTurrets.Count+mines.Count;
        }
        void SimulateCombatStep(float dt)
        {
            if(waveSweeping){TickWaveSweep(dt);return;}
            var before=Rules.Phase;
            elapsed += dt; invulnerable = Mathf.Max(0, invulnerable - dt);
            var negativeButtons=new Vector2(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0,Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
            var positiveButtons=new Vector2(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0,Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0);
            bool gamepadMovement=GameSettings.Flag("gamepadMove");
            Vector2 movement=CombatInput.PlayerMovement(negativeButtons,positiveButtons,gamepadMovement?CombatInput.DpadRaw:Vector2.zero,gamepadMovement?CombatInput.MoveRaw:Vector2.zero);
            if(GameSettings.Flag("mouseMove",false))movement=CombatInput.MouseMovement((Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition)-playerPos,Input.GetMouseButton(0));
            // The reference campaign fixture resets wave_age with each Main
            // scene. Keep global elapsed for traces, but restart its input path.
            float scriptedAge=campaignCheck?elapsed-campaignWaveStart:elapsed;
            if (smoke || review || campaignCheck) movement = new Vector2(Mathf.Cos(scriptedAge * .8f), Mathf.Sin(scriptedAge * .8f)) * .5f;
            if(worldReview) movement=WorldReviewMovement();
            if (artReview) movement = ArtReviewMovement();
            if (enemyReview || lootReview || performanceInputIsolation) movement = Vector2.zero;
            Rules.SetMoving(movement.sqrMagnitude>0);
            RefreshRuntimeCensus();
            playerVelocity=CombatInput.Velocity(movement,Rules.MoveSpeed);playerPos = CombatGeometry.ClampPlayerBody(playerPos + playerVelocity * dt);
            player.transform.position = new Vector3(playerPos.x, playerPos.y, -1);
            playerAnimation.Tick(movement, dt, reducedMotion);TickFootsteps(Arriving?Vector2.zero:movement,dt);
            player.transform.localScale = Vector3.one * ActorPresentation.PlayerScale;
            playerShadow.Place(playerPos, ActorPresentation.PlayerScale);
            player.transform.rotation = Quaternion.identity;
            player.SetActive(true);
            if(supportShowcase)TickSupportShowcase(dt);else {if(loadoutShowcase>=0){invulnerable=10;TickShowcaseSupportQueue(dt);}director.Tick(dt, QueueGroup, BeginSpawnGroup);}
            UpdateBirthQueue(dt);
            activePlayerContactBatch=invulnerable<=0;
            try
            {
                using(ProfileEquipment.Auto())UpdateEquipment(dt);
                if(Rules.Phase!=RunPhase.Playing)return;
                using(ProfileEnemies.Auto())UpdateEnemies(dt);
                if(Rules.Phase!=RunPhase.Playing)return;
                using(ProfileProjectiles.Auto())UpdateWeapons(dt);
                if(Rules.Phase!=RunPhase.Playing)return;
                using(ProfileStarterEffects.Auto())UpdateStarterEffects(dt);using(ProfileSlashes.Auto())UpdateSlashes(dt);using(ProfileImplantAttacks.Auto())TickImplantAttacks(dt);
                using(ProfilePets.Auto())TickPets(dt);using(ProfileNightmare.Auto())TickNightmare(dt);
                // player.die immediately disables its pickup areas. Do not run
                // later combat/pickup work after a lethal contact in this tick.
                if(Rules.Phase!=RunPhase.Playing)return;
            }
            finally{activePlayerContactBatch=null;}
            using(ProfilePickups.Auto())UpdatePickups(dt);
            ResolveCombatCallbacks();RefreshRuntimeCensus();
            UpdateSparks(dt); UpdateCorpses(dt);
            Vector2 cameraShake = ReferenceScreenShake.Tick(ref shake, ref shakeTime, dt,
                new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
            if(loadoutShowcase>=0)gameCamera.orthographicSize=Mathf.MoveTowards(gameCamera.orthographicSize,showcaseCleanFrame?3.2f:4.5f,dt*5);
            Vector2 cameraCenter = ArenaGeometry.CameraPosition(playerPos+CombatGeometry.PlayerOriginOffset,gameCamera.aspect);
            if(reducedMotion)cameraShake=Vector2.zero;
            gameCamera.transform.position = new Vector3(cameraCenter.x + cameraShake.x, cameraCenter.y + cameraShake.y, -20);
            if(supportShowcase)return;
            if(loadoutShowcase>=0&&Rules.Remaining<=dt){BeginLoadoutShowcase(loadoutShowcase);return;}
            if(Rules.Phase==RunPhase.Playing && Rules.Remaining<=dt){BeginWaveSweep();return;}
            float healthBefore=Rules.Health,remainingBefore=Rules.Remaining;using(ProfileRules.Auto())Rules.Tick(dt);
            if(WaveCountdownFeedback.TickDue(remainingBefore,Rules.Remaining))sound.Play(SoundCue.WaveTick);
            ResolveCombatCallbacks();RefreshRuntimeCensus();
            if(Rules.Health<healthBefore){AttractNearbyConsumables();FloatText(playerPos,$"-{Mathf.CeilToInt(healthBefore-Rules.Health)}",coral,true);playerFlash=CombatGeometry.FlashDuration;sound.Play(SoundCue.Hurt);PlayerHitParticles();}
            if(Rules.Health>healthBefore)FloatText(playerPos,$"+{Rules.Health-healthBefore:0} HP",mint,false);
            if (Rules.Phase != before && (Rules.Phase == RunPhase.Loot || Rules.Phase == RunPhase.Upgrade || Rules.Phase == RunPhase.Shop || Rules.Phase == RunPhase.Won)) { ClearCombat(); sound.Play(SoundCue.WaveClear); BeginWaveLetters(14); }
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
                if(p.petCollector!=null&&(p.petCollector.dead||p.petCollector.obj==null))
                {p.petCollector=null;p.motion.ReleaseCollector();}
                if(p.enemyCollector!=null&&(p.enemyCollector.hp<=0||p.enemyCollector.obj==null))
                {p.enemyCollector=null;p.motion.ReleaseCollector();}
                bool external=p.petCollector!=null||p.enemyCollector!=null;
                Vector2 playerOrigin=playerPos+CombatGeometry.PlayerOriginOffset;
                Vector2 collector=p.petCollector!=null?p.petCollector.pos+SupportBehavior.LootwormOriginOffset:p.enemyCollector!=null?p.enemyCollector.pos+CombatGeometry.OriginOffset(p.enemyCollector.type.id):playerOrigin;
                p.motion.Step(dt,collector,-1,reducedMotion,external,false);
                if(p.motion.AttractionEntered(ScrapMotion.PlayerCollector,playerOrigin,Rules.PickupRadius))
                {
                    if(!external&&(!p.fruit||Rules.Health<Rules.MaxHealth))p.motion.magnetized=true;
                    else if(!p.fruit&&external&&p.motion.PlayerCanRetarget(playerOrigin,collector,Rules.PickupRadius))
                    {p.petCollector=null;p.enemyCollector=null;p.motion.magnetized=true;external=false;}
                }
                Vector2 position = p.motion.position;
                p.obj.transform.position = new Vector3(position.x, position.y + p.motion.height, -1.5f);
                p.obj.transform.rotation = Quaternion.Euler(0, 0, p.motion.angle);
                if (p.motion.ContactEntered(ScrapMotion.PlayerCollector,playerPos,.32f)&&!external)
                {
                    CollectPickup(p);Destroy(p.obj);pickups.RemoveAt(i);
                    if (Rules.Phase == RunPhase.Upgrade) { sound.Play(SoundCue.LevelUp); break; }
                }
            }
        }
        void AttractNearbyConsumables()
        {
            foreach(var pickup in pickups)
                if(pickup.fruit&&pickup.petCollector==null&&pickup.enemyCollector==null&&!pickup.motion.magnetized&&pickup.motion.CanCollect&&pickup.motion.OverlapsCollector(playerPos+CombatGeometry.PlayerOriginOffset,Rules.PickupRadius))pickup.motion.magnetized=true;
        }
        void Burst(Vector2 pos, int count, Color tint)
        {
            if(!GameSettings.Flag("effects"))return;
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
                s.obj.transform.position = new Vector3(s.pos.x, s.pos.y, -3); s.obj.transform.localScale = Vector3.one * Mathf.Max(0, s.life / s.max) * (s.scale > 0 ? s.scale : .9f);
                if (s.life <= 0) { Destroy(s.obj); sparks.RemoveAt(i); }
            }
        }
        void ClearCombat()
        {
            ClearShowcaseSupportQueue();
            ClearPets();
            ClearReferenceCombat(); ClearImpacts(); ClearFootDust(); floaters.Clear();
            foreach (var e in enemies) { Destroy(e.obj); if (e.marker != null) Destroy(e.marker); } enemies.Clear();
            foreach (var s in shots) Destroy(s.obj); shots.Clear();
            foreach (var p in pickups) Destroy(p.obj); pickups.Clear();
            foreach (var s in sparks) Destroy(s.obj); sparks.Clear();
        }
        void SelectUpgrade(int choice) { Rules.Choose(choice); FloatText(playerPos, "UPGRADED!", mint, true); Burst(playerPos, 24, mint); invulnerable = 1; player.SetActive(true); sound.Play(SoundCue.Install); }
        void ToggleSound() { muted = !muted; sound.SetMuted(muted); if(!GameSettings.IsFixture){PlayerPrefs.SetInt("muted", muted ? 1 : 0);PlayerPrefs.Save();} }
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
