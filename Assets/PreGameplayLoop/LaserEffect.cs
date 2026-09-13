using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public static class LaserEffect
    {
        public static int Cast(Combatant source, Vector2 origin, Vector2 direction, PG03AbilityCatalog data)
        {
            if (direction.sqrMagnitude == 0) return 0;
            direction.Normalize();
            int hits = 0;
            foreach (var target in Combatant.All.ToArray())
            {
                if (target == null || !target.IsActive || target.Faction != Faction.MOB) continue;
                Vector2 offset = (Vector2)target.transform.position - origin;
                float along = Vector2.Dot(offset, direction);
                float across = Mathf.Abs(offset.x * direction.y - offset.y * direction.x);
                // Numeric tolerance includes exact boundaries after world-space rotation.
                if (along < -.0001f || along > data.LaserRange + .0001f || across > data.LaserWidth * .5f + .0001f) continue;
                if (target.Hit(data.LaserDamage, true, source)) hits++;
            }
            // Temporary rectangle matches the logical area. Laser intentionally ignores blockers.
            var view = TestVisuals.Box("COLPO LASER", origin + direction * data.LaserRange * .5f,
                new Vector2(data.LaserRange, data.LaserWidth), new Color(0, 1, 1, .3f), 2);
            view.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            Object.Destroy(view, .15f);
            return hits;
        }
    }
}
