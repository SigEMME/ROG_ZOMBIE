using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    // A single clipped outline drives the preview, physical collider and navigation mesh.
    public sealed class BarrierGeometry
    {
        public readonly List<Vector2> Points = new List<Vector2>();
        private readonly List<float> angles = new List<float>();
        private readonly List<AttackGeometry.OcclusionBox> covers = new List<AttackGeometry.OcclusionBox>();
        public bool HasArea { get; private set; }
        public void Build(Vector2 center, Vector2 size, float rotation)
        {
            Points.Clear(); angles.Clear(); covers.Clear(); HasArea = false;
            Vector2 half = size * .5f; float radius = half.magnitude;
            for (int i = 0; i < 360; i++) angles.Add(i);
            void AddCorner(Vector2 local)
            {
                float angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
                for (int side = -1; side <= 1; side++) angles.Add(Mathf.Repeat(angle + side * .001f, 360));
            }
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2) AddCorner(new Vector2(x * half.x, y * half.y));
            Quaternion inverse = Quaternion.Euler(0, 0, -rotation), forward = Quaternion.Euler(0, 0, rotation);
            foreach (var obstacle in TestObstacle.All)
            {
                if (!obstacle.isActiveAndEnabled || !obstacle.Body.enabled || obstacle.Bounds.SqrDistance(center) > radius * radius) continue;
                covers.Add(new AttackGeometry.OcclusionBox(obstacle, center));
                foreach (var point in obstacle.WorldOutline()) AddCorner(inverse * (point - center));
            }
            angles.Sort();
            foreach (float angle in angles)
            {
                Vector2 direction = AttackGeometry.Direction(angle);
                float extent = Mathf.Min(Mathf.Abs(direction.x) > .0000001f ? half.x / Mathf.Abs(direction.x) : float.PositiveInfinity,
                    Mathf.Abs(direction.y) > .0000001f ? half.y / Mathf.Abs(direction.y) : float.PositiveInfinity);
                Vector2 point = direction * AttackGeometry.VisibleDistance(forward * direction, extent, covers);
                if (Points.Count == 0 || (Points[Points.Count - 1] - point).sqrMagnitude > .0000000001f) Points.Add(point);
            }
            if (Points.Count > 1 && (Points[0] - Points[Points.Count - 1]).sqrMagnitude < .0000000001f) Points.RemoveAt(Points.Count - 1);
            // Remove collinear samples; keep corners and narrow shadow edges.
            bool changed = true;
            while (changed && Points.Count > 3)
            {
                changed = false;
                for (int i = Points.Count - 1; i >= 0 && Points.Count > 3; i--)
                {
                    Vector2 a = Points[(i + Points.Count - 1) % Points.Count], b = Points[i], c = Points[(i + 1) % Points.Count];
                    Vector2 edge = c - a;
                    float t = edge.sqrMagnitude > 0 ? Vector2.Dot(b - a, edge) / edge.sqrMagnitude : -1;
                    if (t >= 0 && t <= 1 && (b - (a + edge * t)).sqrMagnitude < .0000000001f)
                    { Points.RemoveAt(i); changed = true; }
                }
            }
            float area = 0;
            for (int i = 0; i < Points.Count; i++) area += Cross(Points[i], Points[(i + 1) % Points.Count]);
            HasArea = Points.Count >= 3 && area > .000001f;
            if (!HasArea) Points.Clear();
        }
        public bool OverlapsCircle(Vector2 localCenter, float radius)
        {
            if (!HasArea) return false;
            if (AttackGeometry.PointInPolygon(localCenter, Points)) return true;
            for (int i = 0; i < Points.Count; i++)
            {
                Vector2 a = Points[i], edge = Points[(i + 1) % Points.Count] - a;
                float t = edge.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(localCenter - a, edge) / edge.sqrMagnitude) : 0;
                if ((a + edge * t - localCenter).sqrMagnitude <= radius * radius) return true;
            }
            return false;
        }
        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
