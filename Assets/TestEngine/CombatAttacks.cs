using UnityEngine;

namespace RogZombie.TestEngine
{
    // Shared HIT evaluation. Rendering and input are intentionally outside this service.
    public static class CombatAttacks
    {
        public static void Circular(Vector2 center, float radius, int sectors, float pgDamage, float mobDamage, Combatant excluded = null)
        {
            Physics2D.SyncTransforms();
            bool[] valid = AttackGeometry.ValidSectors(center, radius, sectors);
            var targets = Combatant.All.ToArray();
            foreach (var target in targets)
            {
                if (target == null || target == excluded || !target.IsActive) continue;
                float damage = target.Faction == Faction.PG ? pgDamage : mobDamage;
                if (damage <= 0f) continue;
                if (AttackGeometry.InValidSector((Vector2)target.transform.position - center, radius, valid)) target.Hit(damage);
            }
        }

        public static void Frontal(Combatant source, Vector2 origin, Vector2 direction, WeaponDefinition weapon)
        {
            Physics2D.SyncTransforms();
            foreach (var target in Combatant.All.ToArray())
            {
                if (target == null || !target.IsActive || target.Faction == source.Faction) continue;
                Vector2 offset = (Vector2)target.transform.position - origin;
                bool inside;
                if (weapon.Shape == AttackShape.Cone)
                    inside = weapon.ConeAngle > 0
                        ? offset.sqrMagnitude <= source.Stats.RangeMetres * source.Stats.RangeMetres &&
                          Vector2.Angle(direction, offset) <= weapon.ConeAngle * .5f
                        : AttackGeometry.InCone(offset, direction, source.Stats.RangeMetres, weapon.ConeWidth);
                else
                {
                    // Use the actual MOB body, including collider offset and world scale.
                    // The attack keeps the same origin, facing and radius as FlashFront.
                    var collider = target.GetComponent<CircleCollider2D>();
                    if (collider == null || !collider.enabled) continue;
                    Bounds bounds = collider.bounds;
                    inside = AttackGeometry.CircleIntersectsSemicircle((Vector2)bounds.center - origin,
                        Mathf.Max(bounds.extents.x, bounds.extents.y), direction, source.Stats.RangeMetres);
                }
                if (inside && AttackGeometry.ClearLine(origin, target.transform.position)) target.Hit(source.Stats.ATK);
            }
        }
    }
}
