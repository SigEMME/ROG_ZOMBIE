using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public static class PG08WireGeometry
    {
        public static bool CircleIntersects(Vector2 point, float radius, float inner, float outer, float start, float angle)
        {
            float theta = Mathf.Repeat(Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg - start, 360);
            if (theta <= angle)
            {
                float r = point.magnitude;
                return r + radius >= inner && r - radius <= outer;
            }
            return SegmentDistance(point, AttackGeometry.Direction(start) * inner, AttackGeometry.Direction(start) * outer) <= radius ||
                SegmentDistance(point, AttackGeometry.Direction(start + angle) * inner, AttackGeometry.Direction(start + angle) * outer) <= radius;
        }
        private static float SegmentDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 edge = b - a;
            return Vector2.Distance(point, a + edge * Mathf.Clamp01(Vector2.Dot(point - a, edge) / edge.sqrMagnitude));
        }
        public static bool BoxIntersects(Vector2 origin, float inner, float outer, float start, float angle, Bounds bounds)
        {
            var points = new List<Vector2> {
                new Vector2(bounds.min.x, bounds.min.y) - origin, new Vector2(bounds.max.x, bounds.min.y) - origin,
                new Vector2(bounds.max.x, bounds.max.y) - origin, new Vector2(bounds.min.x, bounds.max.y) - origin };
            Vector2 a = AttackGeometry.Direction(start), b = AttackGeometry.Direction(start + angle);
            points = Clip(points, new Vector2(-a.y, a.x)); points = Clip(points, new Vector2(b.y, -b.x));
            if (points.Count == 0) return false;
            float min = bounds.Contains(new Vector3(origin.x, origin.y, bounds.center.z)) ? 0 : float.PositiveInfinity, max = 0;
            for (int i = 0; i < points.Count; i++)
            {
                max = Mathf.Max(max, points[i].magnitude);
                Vector2 next = points[(i + 1) % points.Count];
                min = Mathf.Min(min, points[i] == next ? points[i].magnitude : SegmentDistance(Vector2.zero, points[i], next));
            }
            return min <= outer && max >= inner;
        }
        private static List<Vector2> Clip(List<Vector2> source, Vector2 normal)
        {
            var result = new List<Vector2>();
            for (int i = 0; i < source.Count; i++)
            {
                Vector2 p = source[i], q = source[(i + 1) % source.Count];
                float dp = Vector2.Dot(p, normal), dq = Vector2.Dot(q, normal);
                if (dp >= 0) result.Add(p);
                if ((dp >= 0) != (dq >= 0)) result.Add(Vector2.Lerp(p, q, dp / (dp - dq)));
            }
            return result;
        }
    }
}
