using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG05Poison : MonoBehaviour
    {
        private Combatant target, source;
        private LoopSession session;
        private float elapsed, duration, damage;
        private int nextTick;
        public int Stacks { get; private set; }
        public float Remaining => Mathf.Max(0, duration - elapsed);
        public void Apply(LoopSession owner, Combatant caster, float tickDamage, float seconds)
        {
            target = GetComponent<Combatant>();
            if (!target.IsActive) return;
            session = owner; source = caster; damage = tickDamage; duration = seconds;
            Stacks = Mathf.Min(5, Stacks + 1); elapsed = 0; nextTick = 1;
        }
        private void Update()
        {
            if (session == null || !session.GameplayRunning) return;
            if (!target.IsActive) { Destroy(this); return; }
            elapsed += Time.deltaTime;
            while (nextTick <= duration && elapsed >= nextTick)
            {
                nextTick++;
                target.Hit(damage * Stacks, true, source);
                if (!target.IsActive) break;
            }
            if (elapsed >= duration || !target.IsActive) { Stacks = 0; Destroy(this); }
        }
    }
}
