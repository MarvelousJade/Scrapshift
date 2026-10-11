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
        readonly System.Random random;
        public UpgradeOffer[] Offers { get; private set; } = new UpgradeOffer[0];
        public bool CanReroll { get; private set; }
        public float CritChance { get; private set; } = .1f;
        public RunRules(int seed = -1) { random = seed < 0 ? new System.Random() : new System.Random(seed); }

        public void Start()
        {
            Phase = RunPhase.Playing; Wave = 1; Remaining = WaveSeconds;
            MaxHealth = Health = 100; Kills = Scrap = UpgradeCount = 0; Level = 1;
            DamageScale = FireRate = 1; MoveSpeed = 5; PickupRadius = 1.3f;
            OrbitUnlocked = PulseUnlocked = WaveUpgrade = false;
            CritChance = .1f; Offers = new UpgradeOffer[0]; CanReroll = false;
        }
        public void Tick(float seconds)
        {
            if (Phase != RunPhase.Playing || seconds <= 0) return;
            Remaining = Math.Max(0, Remaining - seconds);
            if (Remaining > 0) return;
            if (Wave == WaveCount) { Phase = RunPhase.Won; return; }
            Phase = RunPhase.Upgrade; WaveUpgrade = true;
            Draft();
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
                Draft();
            }
        }
        public void Choose(int choice)
        {
            if (Phase != RunPhase.Upgrade || choice < 0 || choice > 2) return;
            var offer = Offers[choice]; int tier = offer.Tier;
            switch (offer.Kind)
            {
                case UpgradeKind.Orbit: OrbitUnlocked = true; break;
                case UpgradeKind.Pulse: PulseUnlocked = true; break;
                case UpgradeKind.Damage: DamageScale += .15f * tier; break;
                case UpgradeKind.Speed: FireRate += .18f * tier; MoveSpeed += .25f; break;
                case UpgradeKind.Repair: MaxHealth += 15 * tier; Health = Math.Min(MaxHealth, Health + 40); PickupRadius += .3f; break;
                case UpgradeKind.Critical: CritChance = Math.Min(.75f, CritChance + .08f * tier); break;
                case UpgradeKind.Magnet: PickupRadius += .65f * tier; MoveSpeed += .35f; break;
            }
            UpgradeCount++;
            if (WaveUpgrade) { Wave++; Remaining = WaveSeconds; Health = Math.Min(MaxHealth, Health + 15); }
            WaveUpgrade = false; Phase = RunPhase.Playing;
        }
        void Draft(bool reroll = false)
        {
            CanReroll = !reroll;
            // First choice teaches weapons, speed and sustain; subsequent drafts vary.
            if (UpgradeCount == 0 && !reroll)
            { Offers = new[] { new UpgradeOffer(UpgradeKind.Orbit, 1), new UpgradeOffer(UpgradeKind.Speed, 1), new UpgradeOffer(UpgradeKind.Repair, 1) }; return; }
            var pool = new System.Collections.Generic.List<UpgradeKind> { UpgradeKind.Damage, UpgradeKind.Speed, UpgradeKind.Repair, UpgradeKind.Critical, UpgradeKind.Magnet };
            if (!OrbitUnlocked) pool.Add(UpgradeKind.Orbit);
            if (!PulseUnlocked) pool.Add(UpgradeKind.Pulse);
            Offers = new UpgradeOffer[3];
            for (int i = 0; i < 3; i++)
            {
                int index = random.Next(pool.Count); var kind = pool[index]; pool.RemoveAt(index);
                int roll = random.Next(100); int tier = roll < 12 ? 3 : roll < 45 ? 2 : 1;
                Offers[i] = new UpgradeOffer(kind, kind == UpgradeKind.Orbit || kind == UpgradeKind.Pulse ? 1 : tier);
            }
        }
        public void Reroll() { if (Phase == RunPhase.Upgrade && CanReroll) Draft(true); }
        public void Pause()
        {
            if (Phase == RunPhase.Playing) Phase = RunPhase.Paused;
            else if (Phase == RunPhase.Paused) Phase = RunPhase.Playing;
        }
        public static Vector2 ClampPosition(Vector2 p) => new Vector2(Mathf.Clamp(p.x, -10.5f, 10.5f), Mathf.Clamp(p.y, -5.1f, 5.1f));
        public static bool Touches(Vector2 a, Vector2 b, float radius) => (a - b).sqrMagnitude <= radius * radius;
    }
}
