using System;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    // One contact clock and one slow per MOB, shared by all overlapping FILO SPINATO rings.
    public sealed class PG08WireContact : MonoBehaviour
    {
        private Combatant actor;
        private Func<float> previousMovement;
        private readonly List<PG08WireArea> rings = new List<PG08WireArea>();
        private float nextTick;
        private void Awake()
        {
            actor = GetComponent<Combatant>(); previousMovement = actor.MovementMultiplier;
            actor.MovementMultiplier = Movement;
        }
        private float Movement()
        {
            float slow = 0;
            foreach (var ring in rings) if (ring != null && ring.Remaining > 0 && ring.Contains(actor)) slow = Mathf.Max(slow, ring.Slow);
            return (previousMovement != null ? previousMovement() : 1) * (1 - slow / 100);
        }
        public void Enter(PG08WireArea ring)
        {
            if (rings.Contains(ring)) return;
            Prune();
            if (rings.Count == 0) nextTick = Time.time + 1;
            rings.Add(ring);
        }
        public void Exit(PG08WireArea ring) => rings.Remove(ring);
        private void Prune() => rings.RemoveAll(r => r == null || !r.Contains(actor));
        private void LateUpdate()
        {
            Prune();
            if (rings.Count == 0 || !actor.IsActive) return;
            if (!rings[0].Running) return;
            float end = 0;
            foreach (var ring in rings) end = Mathf.Max(end, ring.ExpiresAt);
            while (nextTick <= Mathf.Min(Time.time, end) + .00001f)
            {
                // Choose a still existing source at the tick time; a shared effect never duplicates damage.
                var source = rings.Find(r => r.ExpiresAt + .00001f >= nextTick);
                if (source == null) break;
                actor.Hit(source.Damage, true, source.Source); nextTick += 1;
                if (!actor.IsActive) break;
            }
            rings.RemoveAll(r => r.Remaining <= 0);
        }
        private void OnDestroy() { if (actor != null && actor.MovementMultiplier == Movement) actor.MovementMultiplier = previousMovement; }
    }
}
