using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public static class PG04ExplosionEffect
    {
        public static bool[] Sectors(Vector2 center, float radius)
        {
            Physics2D.SyncTransforms();
            return AttackGeometry.ValidSectors(center, radius, 4);
        }
        public static int Hit(Combatant source, Vector2 center, float radius, bool[] valid, float damage)
        {
            int hits = 0;
            foreach (var target in Combatant.All.ToArray())
                if (target != null && target.IsActive && target.Faction == Faction.MOB &&
                    AttackGeometry.InValidSector((Vector2)target.transform.position - center, radius, valid) &&
                    target.Hit(damage, true, source)) hits++;
            return hits;
        }
        public static void Flash(Vector2 center, float radius, bool[] valid)
        {
            var go = new GameObject("PG04 explosion");
            go.transform.SetParent(TestVisuals.Root, false); go.transform.position = center;
            go.AddComponent<PG04AreaVisual>().Initialize(radius, valid, new Color(1, .7f, .1f, .4f));
            Object.Destroy(go, .15f);
        }
    }
}
