using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class WeakPointMark : MonoBehaviour, ICombatHitEffect
    {
        private Combatant actor;
        private GameObject visual;
        public bool Active { get; private set; }
        public void ApplyTo(Combatant target)
        {
            actor = target;
            Active = true;
            actor.HitEffect = this;
            if (visual == null)
            {
                visual = TestVisuals.Box("MARCHIO", (Vector2)transform.position + Vector2.up * .65f,
                    Vector2.one * .18f, Color.magenta, 6);
                visual.transform.SetParent(transform, true);
            }
            visual.SetActive(true);
        }
        public CombatStats Apply(CombatStats stats)
        {
            // DEF percent points use the internal scale: 100 -> 80, 115 -> 95.
            if (Active) stats.DEF -= 20;
            return stats;
        }
        public void AfterHit()
        {
            Active = false;
            if (actor != null && ReferenceEquals(actor.HitEffect, this)) actor.HitEffect = null;
            if (visual != null) visual.SetActive(false);
        }
        private void OnDisable() => AfterHit();
    }
}
