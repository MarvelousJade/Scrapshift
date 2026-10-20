using UnityEngine;
namespace Scrapshift
{
    public sealed partial class RunRules
    {
        // main.clean_up_room optimized mode runs only the common material
        // multiplier, once per pile. It does not execute player pickup effects.
        public int BankCleanupMaterials(int value,bool optimized)
        {
            value=Mathf.Max(0,value);
            float chance=Stat("chance_double_gold")/100;
            int boost=optimized&&chance!=0&&random.NextDouble()<=chance?2:1;
            int bankedValue=value*boost;BankMaterials(bankedValue);return bankedValue;
        }
        public const float DeathCleanupDuration=.5f;
        public bool DeathCleanupPending {get;private set;}
        public float DeathCleanupAge {get;private set;}
        public System.Action BeforeDeathCleanup {get;set;}
        bool enteringDeathCleanup;
        void ResetDeathCleanup(){DeathCleanupPending=false;DeathCleanupAge=0;}
        void BeginDeathCleanup()
        {
            if(enteringDeathCleanup)return;
            enteringDeathCleanup=true;
            try
            {
                // Earlier enemy die requests precede the player's deferred die in
                // the source. Drain them before the run becomes Lost/cleaning up.
                BeforeDeathCleanup?.Invoke();
                // main._on_player_died enters cleanup immediately. The default
                // retry/OK gate appears only when its half-second timer expires.
                Phase=RunPhase.Lost;DeathCleanupPending=true;DeathCleanupAge=0;
                ResetWaveSettlement();ResetConditionalEffects();ResetSurvival();
            }
            finally{enteringDeathCleanup=false;}
        }
        public void AdvanceDeathCleanup(float seconds)
        {
            if(Phase!=RunPhase.Lost||!DeathCleanupPending||seconds<=0)return;
            DeathCleanupAge+=seconds;
            if(DeathCleanupAge+1e-6f>=DeathCleanupDuration)DeathCleanupPending=false;
        }
    }
    public sealed partial class ArenaGame
    {
        bool deathSweepStarted,lossSoundPlayed;
        void RetryCurrentWave()
        {
            if(!Rules.CanRetryWave)return;
            ClearCombat();
            if(!Rules.RetryWave())return;
            deathSweepStarted=lossSoundPlayed=false;waveSweeping=false;endSummary=false;
            selectedWeapon=-1;pauseOptions=pauseCodex=false;lastAttacker=null;recordedFinish=false;
            elapsed=spawnClock=shootClock=orbitClock=pulseClock=invulnerable=pulseAge=shake=0;combatAccumulator=0;
            campaignWaveStart=0;
            if(parachute!=null)Destroy(parachute);arrivalAge=ArrivalMotion.Duration;
            playerPos=CombatGeometry.PlayerBodyStart;player.transform.position=playerPos;
            gameCamera.orthographicSize=ArenaGeometry.CameraHalfHeight;
            StartWave();sound.StopAll();sound.Play(SoundCue.Install);
        }
        void TickDeathSweep(float dt)
        {
            if(Rules.Phase!=RunPhase.Lost)return;
            if(!deathSweepStarted)
            {
                if(!Rules.DeathCleanupPending)return;
                deathSweepStarted=true;waveSweeping=false;
                ClearPets();ClearReferenceCombat();ClearImpacts();ClearFootDust();floaters.Clear();
                foreach(var enemy in enemies){Destroy(enemy.obj);if(enemy.marker!=null)Destroy(enemy.marker);}enemies.Clear();
                foreach(var shot in shots)Destroy(shot.obj);shots.Clear();
                foreach(var spark in sparks)Destroy(spark.obj);sparks.Clear();
                Rules.LivingEnemies=Rules.LivingTrees=Rules.BurningEnemies=Rules.StructureCount=0;
                BankOptimizedCleanupMaterials();
                sound.StopCombat();
            }
            // There are no living players to attract consumables. Raw material
            // value goes to the bank, never currency/XP/heal/pickup triggers.
            float scale=PresentationLayout.CanvasScale(Screen.width,Screen.height);
            Vector2 target=gameCamera.ScreenToWorldPoint(new Vector3(42*scale,Screen.height-140.5f*scale,20));
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var pickup=pickups[i];if(pickup.fruit)continue;
                pickup.motion.position=Vector2.MoveTowards(pickup.motion.position,target,(8+Rules.DeathCleanupAge*45)*Mathf.Max(0,dt));
                pickup.obj.transform.position=new Vector3(pickup.motion.position.x,pickup.motion.position.y,-2);
                if(Vector2.Distance(pickup.motion.position,target)>.15f)continue;
                Rules.BankMaterials(pickup.value);Destroy(pickup.obj);pickups.RemoveAt(i);
            }
            Rules.AdvanceDeathCleanup(dt);
        }
    }
}
