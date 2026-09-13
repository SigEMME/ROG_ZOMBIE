using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG06Vitamin : MonoBehaviour, ICombatStatModifier
    {
        private LoopSession session;
        private Combatant actor;
        private float bonus;
        public float Remaining { get; private set; }
        public void Refresh(LoopSession owner, float percent, float duration)
        {
            session = owner; actor = GetComponent<Combatant>();
            bonus = actor.InitialStats.AttackSpeed * percent / 100f;
            Remaining = duration; actor.SupportModifier = this;
        }
        public CombatStats Apply(CombatStats stats)
        {
            if (Remaining > 0) stats.AttackSpeed += bonus;
            return stats;
        }
        public void Clear() => Remaining = 0;
        private void Update()
        {
            if (session != null && session.GameplayRunning) Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
        }
        private void OnDestroy()
        {
            if (actor != null && ReferenceEquals(actor.SupportModifier, this)) actor.SupportModifier = null;
        }
    }
}
