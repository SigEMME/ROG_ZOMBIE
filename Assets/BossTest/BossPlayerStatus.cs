using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.BossTest
{
    public sealed class BossPlayerStatus : MonoBehaviour
    {
        private struct Poison { public Combatant Source; public float At, Damage; public int Remaining; }
        private readonly List<Poison> poisons = new List<Poison>();
        private Combatant actor;
        private float stunnedUntil;
        public bool Stunned => Time.time < stunnedUntil;
        private void Awake() { actor = GetComponent<Combatant>(); }
        public void Stun(float seconds)
        {
            // Refresh, do not add durations, when multiple rocks hit together.
            stunnedUntil = Mathf.Max(stunnedUntil, Time.time + seconds);
            GetComponent<RogZombie.PreGameplayLoop.PG08PassiveRuntime>()?.SetStunned(true);
        }
        public void PoisonHit(Combatant source, float damage, float duration)
        {
            poisons.Add(new Poison { Source = source, Damage = damage, At = Time.time + 1, Remaining = Mathf.FloorToInt(duration) });
        }
        private void Update()
        {
            if (!actor.IsActive) { poisons.Clear(); return; }
            if (!Stunned) GetComponent<RogZombie.PreGameplayLoop.PG08PassiveRuntime>()?.SetStunned(false);
            for (int i = poisons.Count - 1; i >= 0; i--)
            {
                var poison = poisons[i];
                while (poison.Remaining > 0 && Time.time >= poison.At && actor.IsActive)
                { actor.Hit(poison.Damage, true, poison.Source); poison.Remaining--; poison.At += 1; }
                if (poison.Remaining == 0) poisons.RemoveAt(i); else poisons[i] = poison;
            }
        }
        public static bool Blocks(Component component) => component != null && (component.GetComponent<BossPlayerStatus>()?.Stunned ?? false);
    }
}
