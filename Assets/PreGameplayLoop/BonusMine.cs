using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class BonusMine : MonoBehaviour
    {
        private LoopSession session;
        private Combatant source;
        public const float TriggerRadius = .5f;
        public const float DetonationDelay = 1f;
        private float damage, radius, remaining, detonationRemaining;
        private bool activated;
        public void Initialize(LoopSession owner, Combatant caster, float attack, float metres, float duration)
        { session = owner; source = caster; damage = attack; radius = metres; remaining = duration; activated = false; }
        private void Update()
        {
            if (!session.GameplayRunning) return;
            if (activated)
            {
                detonationRemaining -= Time.deltaTime;
                if (detonationRemaining > 0) return;
            }
            else
            {
                foreach (var mob in Combatant.All)
                    if (mob != null && mob.IsActive && mob.Faction == Faction.MOB &&
                        Vector2.Distance(transform.position, mob.transform.position) <= TriggerRadius + mob.Radius)
                    {
                        activated = true;
                        detonationRemaining = DetonationDelay;
                        return;
                    }
                remaining -= Time.deltaTime;
                if (remaining > 0) return;
            }
            var valid = PG04ExplosionEffect.Sectors(transform.position, radius);
            PG04ExplosionEffect.Hit(source, transform.position, radius, valid, damage);
            BonusAbilityRuntime.Flash(transform.position, radius, valid, Color.yellow);
            gameObject.SetActive(false); Destroy(gameObject);
        }
        private struct Edge { public Vector2 A, B; public Edge(Vector2 a, Vector2 b) { A = a; B = b; } }
        public static bool TryPosition(Vector2 original, Vector2 size, out Vector2 result)
        {
            Physics2D.SyncTransforms();
            result = original;
            if (Free(original, size)) return true;
            // Closest point on the free boundary of the union of obstacle polygons.
            // Split edges at intersections; the closest point lies on one of the resulting segments.
            var edges = new List<Edge>();
            foreach (var obstacle in TestObstacle.All)
            {
                if (!obstacle.Body.enabled) continue;
                var corners = obstacle.WorldOutline();
                for (int i = 0; i < corners.Length; i++) edges.Add(new Edge(corners[i], corners[(i + 1) % corners.Length]));
            }
            float best = float.PositiveInfinity;
            foreach (var edge in edges)
            {
                var cuts = new List<float> { 0, 1 };
                Vector2 v = edge.B - edge.A;
                foreach (var other in edges)
                {
                    Vector2 w = other.B - other.A; float cross = Cross(v, w);
                    if (Mathf.Abs(cross) < .000001f) continue;
                    float t = Cross(other.A - edge.A, w) / cross, u = Cross(other.A - edge.A, v) / cross;
                    if (t > 0 && t < 1 && u >= 0 && u <= 1) cuts.Add(t);
                }
                cuts.Sort();
                for (int i = 1; i < cuts.Count; i++)
                {
                    float t = Mathf.Clamp(Vector2.Dot(original - edge.A, v) / v.sqrMagnitude, cuts[i-1], cuts[i]);
                    Vector2 point = edge.A + v * t;
                    // Small collision tolerance, not a gameplay placement radius.
                    for (int j = 0; j < 8; j++)
                    {
                        Vector2 candidate = point + AttackGeometry.Direction(j * 45) * .002f;
                        float distance = (candidate - original).sqrMagnitude;
                        if (distance >= best || !Free(candidate, size)) continue;
                        best = distance; result = candidate;
                    }
                }
            }
            return !float.IsPositiveInfinity(best);
        }
        private static float Cross(Vector2 a, Vector2 b) => a.x*b.y-a.y*b.x;
        private static bool Free(Vector2 point, Vector2 size)
        {
            if (Mathf.Abs(point.x) >= size.x*.5f || Mathf.Abs(point.y) >= size.y*.5f) return false;
            foreach (var obstacle in TestObstacle.All)
                if (obstacle.Body.enabled && obstacle.Body.OverlapPoint(point)) return false;
            return true;
        }
    }
}
