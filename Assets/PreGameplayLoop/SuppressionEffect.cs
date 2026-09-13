using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public static class SuppressionEffect
    {
        // Five sector centres, right to left and back, without duplicated endpoints.
        public static float ShotAngle(int index, float coneAngle)
        {
            int step = index % 8;
            int sector = step <= 4 ? step : 8 - step;
            return (sector - 2) * coneAngle / 5f;
        }

        public static Projectile Fire(Combatant source, Vector2 origin, Vector2 direction,
            PG02AbilityCatalog data, WeaponDefinition weapon, int index)
        {
            float facing = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return TestVisuals.SpawnProjectile(source, origin,
                AttackGeometry.Direction(facing + ShotAngle(index, data.SuppressionAngle)),
                weapon.ProjectileSpeed, data.SuppressionRange, data.SuppressionDamage,
                weapon.ProjectileRadius, 0, false);
        }
    }
}
