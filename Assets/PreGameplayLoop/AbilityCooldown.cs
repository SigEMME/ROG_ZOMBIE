using System;

namespace RogZombie.PreGameplayLoop
{
    // Simulation time is supplied by the session, so loading and BONUS never consume CD.
    public sealed class AbilityCooldown
    {
        public float Remaining { get; private set; }
        public bool Ready => Remaining <= 0f;

        public static float FinalSeconds(float baseSeconds, float cdReduction)
        {
            float stat = Math.Max(10f, (float)Math.Round(cdReduction, MidpointRounding.AwayFromZero));
            return (float)Math.Round(baseSeconds * (double)stat / 100d, 1, MidpointRounding.AwayFromZero);
        }

        public void Restart(float baseSeconds, float cdReduction)
            => Remaining = FinalSeconds(baseSeconds, cdReduction);

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
