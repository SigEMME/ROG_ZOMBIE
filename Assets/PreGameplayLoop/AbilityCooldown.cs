using System;

namespace RogZombie.PreGameplayLoop
{
    // Simulation time is supplied by the session, so loading and BONUS never consume CD.
    public sealed class AbilityCooldown
    {
        public float Remaining { get; private set; }
        public float Duration { get; private set; }
        public float RecoveryProgress => Ready ? 1f : Duration > 0 ? UnityEngine.Mathf.Clamp01(1f - Remaining / Duration) : 0f;
        public bool Ready => Remaining <= 0f;

        public static float FinalSeconds(float baseSeconds, float cdReduction)
        {
            float stat = Math.Max(10f, (float)Math.Round(cdReduction, MidpointRounding.AwayFromZero));
            return (float)Math.Round(baseSeconds * (double)stat / 100d, 1, MidpointRounding.AwayFromZero);
        }

        public void Restart(float baseSeconds, float cdReduction)
            => Remaining = Duration = FinalSeconds(baseSeconds, cdReduction);

        public void Tick(float simulationDelta)
        {
            if (simulationDelta > 0f) Remaining = Math.Max(0f, Remaining - simulationDelta);
        }

        public void ChangeArea(bool effectActive, float baseSeconds, float cdReduction)
        {
            // Available stays available; inactive cooldown retains its exact remaining time.
            if (effectActive) Restart(baseSeconds, cdReduction);
        }
    }
}
