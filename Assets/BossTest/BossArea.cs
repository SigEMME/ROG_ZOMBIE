using System.Collections.Generic;
using UnityEngine;
namespace RogZombie.BossTest
{
    // The same polygon outline supplies presentation and circle-vs-area HIT checks.
    public sealed class BossArea : MonoBehaviour
    {
        private Vector2[] outline;
        private Vector2 fillOrigin;
        private Mesh mesh;
        private Material material;
        private LineRenderer border;
        private MeshRenderer fill; private int[] triangles; private float innerRadius;
        public static BossArea Create(Transform parent, Vector2[] polygon, Vector2 origin, float inner = 0)
        {
            var go = new GameObject("Indicatore attacco"); go.transform.SetParent(parent, false);
            var area = go.AddComponent<BossArea>(); area.outline = polygon; area.fillOrigin = origin; area.innerRadius = inner; area.triangles = Triangulate(polygon);
            area.material = new Material(Shader.Find("Sprites/Default"));
            area.mesh = new Mesh(); go.AddComponent<MeshFilter>().sharedMesh = area.mesh;
            area.fill = go.AddComponent<MeshRenderer>(); area.fill.sharedMaterial = area.material; area.fill.sortingOrder = 1;
            area.border = go.AddComponent<LineRenderer>(); area.border.sharedMaterial = area.material;
            area.border.useWorldSpace = false; area.border.loop = true; area.border.widthMultiplier = .06f;
            area.border.startColor = area.border.endColor = Color.red; area.border.sortingOrder = 2;
            area.border.positionCount = polygon.Length;
            for (int i = 0; i < polygon.Length; i++) area.border.SetPosition(i, polygon[i]);
            area.Progress(0); return area;
        }
        public void Progress(float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            var vertices = new Vector3[outline.Length]; var colors = new Color[vertices.Length];
            for (int i = 0; i < outline.Length; i++)
            {
                Vector2 origin = innerRadius > 0 ? outline[i].normalized * innerRadius : fillOrigin;
                vertices[i] = Vector2.Lerp(origin, outline[i], fraction);
                colors[i] = new Color(1, 0, 0, .3f);
            }
            mesh.Clear(); mesh.vertices = vertices; mesh.colors = colors; mesh.triangles = triangles;
        }
        private static int[] Triangulate(Vector2[] points)
        {
            var remaining = new List<int>(); var result = new List<int>();
            for (int i = 0; i < points.Length; i++) remaining.Add(i);
            int guard = points.Length * points.Length;
            while (remaining.Count > 2 && guard-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int a = remaining[(i + remaining.Count - 1) % remaining.Count], b = remaining[i], c = remaining[(i + 1) % remaining.Count];
                    if (Cross(points[b] - points[a], points[c] - points[b]) <= .000001f) continue;
                    bool contains = false;
                    foreach (int p in remaining)
                    {
                        if (p == a || p == b || p == c) continue;
                        if (Cross(points[b] - points[a], points[p] - points[a]) >= 0 && Cross(points[c] - points[b], points[p] - points[b]) >= 0 && Cross(points[a] - points[c], points[p] - points[c]) >= 0) { contains = true; break; }
                    }
                    if (contains) continue;
                    result.Add(a); result.Add(b); result.Add(c); remaining.RemoveAt(i); clipped = true; break;
                }
                if (!clipped) break;
            }
            return result.ToArray();
        }
        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        public static Vector2[] Ring(float inner, float outer, float angle, float centre = 0)
        {
            int steps = Mathf.Max(2, Mathf.CeilToInt(angle / 3)); var points = new Vector2[(steps + 1) * 2];
            for (int i = 0; i <= steps; i++)
            {
                var direction = Direction(centre - angle / 2 + angle * i / steps);
                points[i] = direction * outer; points[points.Length - 1 - i] = direction * inner;
            }
            return points;
        }
        public bool Contains(Vector2 point, float radius)
        {
            point = transform.InverseTransformPoint(point);
            bool inside = false;
            for (int i = 0, j = outline.Length - 1; i < outline.Length; j = i++)
            {
                var a = outline[j]; var b = outline[i]; var edge = b - a;
                float t = edge.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(point - a, edge) / edge.sqrMagnitude) : 0;
                if ((point - a - edge * t).sqrMagnitude <= radius * radius) return true;
                if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }
        public static Vector2 Direction(float degrees) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
        public static Vector2[] Rectangle(float start, float length, float width, float angle = 0)
        {
            var forward = Direction(angle); var side = new Vector2(-forward.y, forward.x) * width / 2;
            return new[] { forward * start - side, forward * (start + length) - side, forward * (start + length) + side, forward * start + side };
        }
        public static Vector2[] Sector(float radius, float angle, float centre = 0)
        {
            int steps = Mathf.Max(8, Mathf.CeilToInt(angle / 3)); var points = new Vector2[steps + 2];
            for (int i = 0; i <= steps; i++) points[i + 1] = Direction(centre - angle / 2 + angle * i / steps) * radius;
            return points;
        }
        public static Vector2[] Circle(float radius)
        {
            var points = new Vector2[64]; for (int i = 0; i < points.Length; i++) points[i] = Direction(360f * i / points.Length) * radius; return points;
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}

