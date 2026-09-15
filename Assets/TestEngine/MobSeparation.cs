using UnityEngine;
using System.Collections.Generic;

namespace RogZombie.TestEngine
{
    // Local steering only: the existing brain still owns targets, paths and attack decisions.
    [DefaultExecutionOrder(50)]
    public sealed class MobSeparation : MonoBehaviour
    {
        public TestAreaSettings Settings;
        private Combatant actor;
        private int movingFrame = -1;
        private struct NeighbourInfo
        {
            public Vector2 Normal;
            public float Gap, Spacing, Radius;
        }
        private readonly List<NeighbourInfo> neighbours = new List<NeighbourInfo>();
        private void ReadNeighbours(float displacement)
        {
            neighbours.Clear();
            Vector2 position = transform.position;
            float radius = actor.Radius;
            foreach (var other in Combatant.All)
            {
                if (!Neighbour(other)) continue;
                Vector2 away = position - (Vector2)other.transform.position;
                float gap = away.magnitude;
                float spacing = radius + other.Radius + Settings.MobSpacing;
                // The later projections cannot increase the initial displacement length.
                if (gap > Mathf.Max(spacing * 1.5f, spacing + displacement) + .000001f) continue;
                neighbours.Add(new NeighbourInfo { Normal = Normal(away, other), Gap = gap, Spacing = spacing, Radius = other.Radius });
            }
        }
        private void Awake() => actor = GetComponent<Combatant>();

        public Vector2 Steer(Vector2 desired)
        {
            if (Settings == null || !Settings.EnableMobSeparation || desired.sqrMagnitude == 0f) return desired;
            movingFrame = Time.frameCount;
            float distance = desired.magnitude;
            Vector2 direction = desired / distance;
            Vector2 avoidance = Vector2.zero;
            ReadNeighbours(distance);
            foreach (var other in neighbours)
            {
                float gap = other.Gap;
                float spacing = other.Spacing;
                if (gap > spacing * 1.5f) continue;
                Vector2 normal = other.Normal;
                float approaching = Mathf.Max(0f, -Vector2.Dot(direction, normal));
                float weight = Mathf.Clamp01((spacing * 1.5f - gap) / spacing);
                // A lateral component lets followers flow around congestion instead of making a rigid wall.
                Vector2 tangent = new Vector2(-normal.y, normal.x);
                if (Vector2.Dot(tangent, direction) < 0f) tangent = -tangent;
                avoidance += (normal + tangent * approaching) * weight;
            }
            Vector2 result = Vector2.ClampMagnitude(desired + avoidance * Settings.MobSeparationSpeed * Time.deltaTime, distance);
            // Keep the tangential motion, but prevent a fast step from crossing another centre.
            // Separation alone is insufficient when movement is faster than its correction budget.
            for (int pass = 0; pass < 2; pass++)
                foreach (var other in neighbours)
                {
                    float clearance = Mathf.Max(0f, other.Gap - actor.Radius - other.Radius - Settings.MobSpacing);
                    if (clearance >= result.magnitude) continue;
                    Vector2 normal = other.Normal;
                    float approach = -Vector2.Dot(result, normal);
                    if (approach > clearance) result += normal * (approach - clearance);
                }
            return result;
        }

        private void LateUpdate()
        {
            if (Settings == null || !Settings.EnableMobSeparation || Time.timeScale <= 0f || actor == null || !actor.IsActive || movingFrame != Time.frameCount) return;
            Vector2 correction = Vector2.zero;
            foreach (var other in Combatant.All)
            {
                if (!Neighbour(other)) continue;
                Vector2 away = (Vector2)transform.position - (Vector2)other.transform.position;
                float overlap = actor.Radius + other.Radius + Settings.MobSpacing - away.magnitude;
                if (overlap > 0f) correction += Normal(away, other) * overlap * 0.5f;
            }
            actor.Separate(Vector2.ClampMagnitude(correction, Settings.MobSeparationSpeed * Time.deltaTime));
        }

        private bool Neighbour(Combatant other) => other != null && other != actor && other.Faction == Faction.MOB && other.State != LifeState.Dead;
        private Vector2 Normal(Vector2 away, Combatant other)
        {
            if (away.sqrMagnitude > 0.000001f) return away.normalized;
            return actor.GetInstanceID() < other.GetInstanceID() ? Vector2.right : Vector2.left;
        }
    }
}
