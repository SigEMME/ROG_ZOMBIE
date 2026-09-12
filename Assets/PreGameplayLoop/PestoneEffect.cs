using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public static class PestoneEffect
    {
        public static bool Contains(Vector2 offset, Vector2 direction, float depth, float width)
        {
            if (direction.sqrMagnitude == 0 || depth <= 0 || width <= 0) return false;
            direction.Normalize();
            float along = Vector2.Dot(offset, direction);
            float across = Mathf.Abs(offset.x * direction.y - offset.y * direction.x);
            return along >= 0 && along <= depth && across <= width * .5f;
        }

        // Instant MOB query, with the same per-target obstruction rule as the PG01 cone.
        // GDD 10.3 applies sector removal only to explicitly listed effects, not PESTONE.
        public static int Cast(Vector2 origin, Vector2 direction, PG01AbilityCatalog data)
        {
            if (direction.sqrMagnitude == 0) return 0;
            direction.Normalize();
            Physics2D.SyncTransforms();
            int hits = 0;
            foreach (var target in Combatant.All.ToArray())
            {
                if (target == null || !target.IsActive || target.Faction != Faction.MOB) continue;
                Vector2 position = target.transform.position;
                if (!Contains(position - origin, direction, data.PestoneDepth, data.PestoneWidth) ||
                    !AttackGeometry.ClearLine(origin, position)) continue;
                if (!target.Hit(data.PestoneDamage, roundFinalDamage: true)) continue;
                target.ApplyPestoneSlow(data.PestoneSlowPercent, data.PestoneSlowDuration);
                hits++;
            }
            return hits;
        }
    }
}
