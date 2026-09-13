using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG06Medikit : MonoBehaviour
    {
        private LoopSession session;
        private float fraction, radius;
        private bool consumed;
        public bool Consumed => consumed;
        public void Initialize(LoopSession owner, float healFraction, float triggerRadius)
        {
            session = owner; fraction = healFraction; radius = triggerRadius;
            gameObject.layer = LayerMask.NameToLayer("TRIGGER_PG");
            var collider = gameObject.AddComponent<CircleCollider2D>(); collider.radius = radius; collider.isTrigger = true;
        }
        private void Update()
        {
            if (consumed || session == null || !session.GameplayRunning) return;
            var candidates = new List<Combatant>();
            foreach (var pg in Combatant.All)
                if (PG06Healing.Valid(pg) && pg.HealthFraction < 1 &&
                    Vector2.Distance(pg.transform.position, transform.position) <= radius + pg.Radius) candidates.Add(pg);
            var target = PG06Healing.Select(candidates, transform.position, false);
            if (target == null) return;
            consumed = true;
            target.Heal(fraction); // MEDI KIT is not an ability heal and never grants VITAMINA C.
            Destroy(gameObject);
        }
    }
}
