using System;
using UnityEngine;

namespace Scrapshift
{
    public enum RunPhase { Title, Playing, Upgrade, Paused, Won, Lost }

    // Pure rules shared by gameplay and tests. No wall-clock or scene dependencies.
    public sealed class RunRules
    {
        public const int WaveCount = 5;
        public const float WaveSeconds = 60;
        public RunPhase Phase { get; private set; } = RunPhase.Title;
        public int Wave { get; private set; }
        public float Remaining { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; } = 100;
        public int Kills { get; private set; }
        public int Scrap { get; private set; }
        public int Level { get; private set; } = 1;
        public int UpgradeCount { get; private set; }
        public float DamageScale { get; private set; } = 1;
        public float FireRate { get; private set; } = 1;
        public float MoveSpeed { get; private set; } = 5;
        public float PickupRadius { get; private set; } = 1.3f;
        public bool OrbitUnlocked { get; private set; }
        public bool PulseUnlocked { get; private set; }
        public bool WaveUpgrade { get; private set; }
        public int NextLevel => 7 + Level * 5;

        public void Start()
        {
            Phase = RunPhase.Playing; Wave = 1; Remaining = WaveSeconds;
            MaxHealth = Health = 100; Kills = Scrap = UpgradeCount = 0; Level = 1;
            DamageScale = FireRate = 1; MoveSpeed = 5; PickupRadius = 1.3f;
            OrbitUnlocked = PulseUnlocked = WaveUpgrade = false;
        }
        public void Tick(float seconds)
        {
            if (Phase != RunPhase.Playing || seconds <= 0) return;
            Remaining = Math.Max(0, Remaining - seconds);
            if (Remaining > 0) return;
            if (Wave == WaveCount) { Phase = RunPhase.Won; return; }
            Phase = RunPhase.Upgrade; WaveUpgrade = true;
        }
        public void Hurt(float amount)
        {
            if (Phase != RunPhase.Playing || amount <= 0) return;
            Health = Math.Max(0, Health - amount);
            if (Health == 0) Phase = RunPhase.Lost;
        }
        public void Kill() { if (Phase == RunPhase.Playing) Kills++; }
        public void Collect(int value)
        {
            if (Phase != RunPhase.Playing || value <= 0) return;
            Scrap += value;
            if (Scrap >= NextLevel)
            {
                Scrap -= NextLevel; Level++; Phase = RunPhase.Upgrade; WaveUpgrade = false;
            }
        }
        public void Choose(int choice)
        {
            if (Phase != RunPhase.Upgrade || choice < 0 || choice > 2) return;
            // Every third draft introduces a new weapon; other drafts offer clear tradeoffs.
            if (choice == 0)
            {
                if (!OrbitUnlocked) OrbitUnlocked = true;
                else if (!PulseUnlocked) PulseUnlocked = true;
                else DamageScale += .25f;
            }
            else if (choice == 1) { FireRate += .18f; MoveSpeed += .25f; }
            else { MaxHealth += 15; Health = Math.Min(MaxHealth, Health + 40); PickupRadius += .3f; }
            UpgradeCount++;
            if (WaveUpgrade) { Wave++; Remaining = WaveSeconds; Health = Math.Min(MaxHealth, Health + 15); }
            WaveUpgrade = false; Phase = RunPhase.Playing;
        }
        public void Pause()
        {
            if (Phase == RunPhase.Playing) Phase = RunPhase.Paused;
            else if (Phase == RunPhase.Paused) Phase = RunPhase.Playing;
        }
        public static Vector2 ClampPosition(Vector2 p) => new Vector2(Mathf.Clamp(p.x, -10.5f, 10.5f), Mathf.Clamp(p.y, -5.1f, 5.1f));
        public static bool Touches(Vector2 a, Vector2 b, float radius) => (a - b).sqrMagnitude <= radius * radius;
    }
}
