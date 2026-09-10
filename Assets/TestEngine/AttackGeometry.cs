using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    public static class AttackGeometry
    {
        public static bool InCone(Vector2 offset, Vector2 forward, float depth, float width)
        {
            float along = Vector2.Dot(offset, forward);
            float across = Mathf.Abs(offset.x * forward.y - offset.y * forward.x);
            return depth > 0f && along >= 0f && along <= depth && across <= width * 0.5f * along / depth;
        }

        public static bool InSemicircle(Vector2 offset, Vector2 forward, float radius)
            => offset.sqrMagnitude <= radius * radius && Vector2.Dot(offset, forward) >= 0f;

        public static bool CircleIntersectsSemicircle(Vector2 offset, float targetRadius, Vector2 forward, float radius)
        {
            if (radius <= 0f || targetRadius < 0f || forward.sqrMagnitude == 0f) return false;
            forward.Normalize();
            Vector2 closest;
            if (Vector2.Dot(offset, forward) >= 0f)
                closest = Vector2.ClampMagnitude(offset, radius);
            else
            {
                // Behind the diameter, the closest point belongs to the diameter segment,
                // not the full circle. This avoids turning the KNIFE into a rear attack.
                Vector2 side = new Vector2(-forward.y, forward.x);
                closest = side * Mathf.Clamp(Vector2.Dot(offset, side), -radius, radius);
            }
            return (offset - closest).sqrMagnitude <= targetRadius * targetRadius;
        }

        public static bool ClearLine(Vector2 from, Vector2 to)
        {
            foreach (var hit in Physics2D.LinecastAll(from, to))
                if (hit.collider.GetComponent<TestObstacle>() != null) return false;
            return true;
        }

        public static Vector2 WallImpact(Vector2 from, Vector2 target)
        {
            Vector2 result = target;
            float nearest = (target - from).sqrMagnitude;
            foreach (var hit in Physics2D.LinecastAll(from, target))
            {
                var obstacle = hit.collider.GetComponent<TestObstacle>();
                if (obstacle == null || !obstacle.IsWall) continue;
                float distance = (hit.point - from).sqrMagnitude;
                if (distance <= nearest) { nearest = distance; result = hit.point; }
            }
            return result;
        }

        public static bool[] ValidSectors(Vector2 origin, float radius, int count, float rotation = 0f)
        {
            var valid = new bool[count];
            for (int i = 0; i < count; i++)
            {
                valid[i] = true;
                float start = rotation + i * 360f / count;
                foreach (var obstacle in TestObstacle.All)
                {
                    if (!obstacle.isActiveAndEnabled) continue;
                    if (!SectorIntersectsBox(origin, radius, start, 360f / count, obstacle.Bounds)) continue;
                    valid[i] = false;
                    break;
                }
            }
            return valid;
        }

        public static bool InValidSector(Vector2 offset, float radius, bool[] valid, float rotation = 0f)
        {
            if (offset.sqrMagnitude > radius * radius) return false;
            if (offset.sqrMagnitude < 0.0000001f)
            {
                foreach (bool value in valid) if (value) return true;
                return false;
            }
            float sector = Mathf.Repeat(Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg - rotation, 360f) / (360f / valid.Length);
            int index = Mathf.FloorToInt(sector) % valid.Length;
            if (valid[index]) return true;
            // A target on a boundary belongs to either adjoining sector, but gets only one HIT.
            if (Mathf.Abs(sector - Mathf.Round(sector)) < 0.00001f)
                return valid[(Mathf.RoundToInt(sector) + valid.Length - 1) % valid.Length];
            return false;
        }

        // Exact clipping for the axis-aligned box walls used in the test AREA; no ray sampling gaps.
        public static bool SectorIntersectsBox(Vector2 origin, float radius, float start, float angle, Bounds bounds)
        {
            var polygon = new List<Vector2>
            {
                new Vector2(bounds.min.x, bounds.min.y) - origin,
                new Vector2(bounds.max.x, bounds.min.y) - origin,
                new Vector2(bounds.max.x, bounds.max.y) - origin,
                new Vector2(bounds.min.x, bounds.max.y) - origin
            };
            Vector2 a = Direction(start), b = Direction(start + angle);
            polygon = Clip(polygon, new Vector2(-a.y, a.x));
            polygon = Clip(polygon, new Vector2(b.y, -b.x));
            if (polygon.Count < 3) return false;
            float area = 0f;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p = polygon[i], q = polygon[(i + 1) % polygon.Count];
                area += p.x * q.y - p.y * q.x;
            }
            if (Mathf.Abs(area) < 0.000001f) return false;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p = polygon[i], edge = polygon[(i + 1) % polygon.Count] - p;
                float t = edge.sqrMagnitude > 0f ? Mathf.Clamp01(-Vector2.Dot(p, edge) / edge.sqrMagnitude) : 0f;
                if ((p + edge * t).sqrMagnitude <= radius * radius) return true;
            }
            return bounds.Contains(new Vector3(origin.x, origin.y, bounds.center.z));
        }

        private static List<Vector2> Clip(List<Vector2> source, Vector2 normal)
        {
            var result = new List<Vector2>();
            for (int i = 0; i < source.Count; i++)
            {
                Vector2 p = source[i], q = source[(i + 1) % source.Count];
                float dp = Vector2.Dot(p, normal), dq = Vector2.Dot(q, normal);
                if (dp >= 0f) result.Add(p);
                if ((dp >= 0f) != (dq >= 0f)) result.Add(Vector2.Lerp(p, q, dp / (dp - dq)));
            }
            return result;
        }

        public static Vector2 Direction(float degrees)
            => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
    }
}
