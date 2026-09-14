using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public static class PG04ExplosionEffect
    {
        public static float VisibleDistance(Vector2 center, Vector2 direction, float distance)
            => AttackGeometry.VisibleDistance(center, direction, distance);
        public static int HitBase(Combatant source, Vector2 center, float radius, float damage)
        {
            return Hit(source, center, radius, null, damage, true);
        }
        public static void FlashBase(Vector2 center, float radius)
        {
            var go = new GameObject("PG04 visible base explosion");
            go.transform.SetParent(TestVisuals.Root, false); go.transform.position = center;
            go.AddComponent<PG04AreaVisual>().InitializeOccluded(center, radius, new Color(1, .7f, .1f, .4f));
            Object.Destroy(go, .15f);
        }
        public static bool[] Sectors(Vector2 center, float radius)
        {
            Physics2D.SyncTransforms();
            // Compatibility for existing callers; masks no longer decide occlusion.
            return new[] { true, true, true, true };
        }
        public static int Hit(Combatant source, Vector2 center, float radius, bool[] valid, float damage, bool baseAttack = false)
        {
            Physics2D.SyncTransforms();
            int hits = 0;
            foreach (var target in Combatant.All.ToArray())
                if (target != null && target.IsActive && target.Faction == Faction.MOB &&
                    AttackGeometry.InVisibleArea(center, radius, target) &&
                    target.Hit(damage, true, source, baseAttack)) hits++;
            return hits;
        }
        public static void Flash(Vector2 center, float radius, bool[] valid)
        {
            var go = new GameObject("PG04 explosion");
            go.transform.SetParent(TestVisuals.Root, false); go.transform.position = center;
            go.AddComponent<PG04AreaVisual>().InitializeOccluded(center, radius, new Color(1, .7f, .1f, .4f));
            Object.Destroy(go, .15f);
        }
    }
}
