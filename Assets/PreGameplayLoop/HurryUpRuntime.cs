using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    // Owned by the ordinary AREA root: destruction resets both timer and registry.
    public sealed class HurryUpRuntime : MonoBehaviour
    {
        public const float Duration = 180f;
        public const float RageMultiplier = 1.5f;
        public float Remaining { get; private set; } = Duration;
        public bool Raging { get; private set; }
        private LoopSession session;
        private readonly HashSet<Combatant> zombies = new HashSet<Combatant>();
        public void Initialize(LoopSession owner) { session = owner; }

        public void Register(Combatant zombie)
        {
            if (zombie == null || !zombies.Add(zombie)) return;
            if (Raging) ApplyRage(zombie);
        }

        private void Update()
        {
            // Loading, bonus screens, pause and defeat never consume AREA time.
            if (Raging || session == null || session.State != LoopState.Combat ||
                !session.GameplayRunning || session.PauseMenuOpen) return;
            Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
            if (Remaining > 0) return;
            Raging = true;
            foreach (var zombie in zombies) ApplyRage(zombie);
        }

        private static void ApplyRage(Combatant zombie)
        {
            if (zombie == null || !zombie.IsActive) return;
            // Change only these runtime values once, preserving HP and AREA growth.
            var stats = zombie.Stats;
            stats.MoveSpeed *= RageMultiplier;
            stats.AttackSpeed *= RageMultiplier;
            zombie.SetStats(stats);
        }
    }
}
