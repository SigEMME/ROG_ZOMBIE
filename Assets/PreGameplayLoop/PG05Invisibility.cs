using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG05Invisibility : MonoBehaviour
    {
        private LoopSession session;
        private Combatant actor;
        private float remaining, bonus;
        public float Remaining => Active ? remaining : 0;
        public bool Active => remaining > 0 && actor != null && actor.IsActive;
        public void Apply(LoopSession owner, float seconds, float moveBonus)
        {
            session = owner; actor = GetComponent<Combatant>(); remaining = seconds; bonus = moveBonus;
            actor.InvisibleQuery = () => Active;
            actor.MovementMultiplier = () => Active ? 1 + bonus / 100f : 1;
            actor.PerformedAction -= Cancel;
            actor.PerformedAction += Cancel;
        }
        public void Cancel() => remaining = 0;
        private void Update()
        {
            if (session != null && session.GameplayRunning) remaining = Mathf.Max(0, remaining - Time.deltaTime);
            if (actor != null && !actor.IsActive) Cancel();
        }
        private void OnDestroy()
        {
            if (actor != null) { actor.PerformedAction -= Cancel; actor.InvisibleQuery = null; actor.MovementMultiplier = null; }
        }
    }
}
