using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    // Filled previews and cover-clipped damage areas share the same temporary renderer.
    public sealed class PG04AreaVisual : MonoBehaviour
    {
        // Visual marker for the distribution area; it has no collider or damage logic.
        public static GameObject CreateAreaMarker(string label, Vector2 center, float radius, Color color)
        {
            var go = new GameObject(label);
            go.transform.SetParent(TestVisuals.Root, false);
            go.transform.position = center;
            go.AddComponent<PG04AreaVisual>().Initialize(radius, new[] { true, true, true, true }, color);
            go.GetComponent<MeshRenderer>().sortingOrder = 0;
            return go;
        }
        private Mesh mesh;
        private Material material;
        private MeshFilter filter;
        private MeshRenderer meshRenderer;
        private readonly List<float> angles = new List<float>();
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private readonly List<Color32> colors = new List<Color32>();
        private readonly List<Vector2> uv = new List<Vector2>();
        private readonly List<AttackGeometry.OcclusionBox> occluders = new List<AttackGeometry.OcclusionBox>();
        public void InitializeOccluded(Vector2 center, float radius, Color color, float rotation = 0, float sweep = 360, float innerRadius = 0)
        {
            angles.Clear(); occluders.Clear();
            for (int i = 0; i <= Mathf.CeilToInt(sweep); i++) angles.Add(Mathf.Min(i, sweep));
            // Extra rays on both sides of blocker corners keep narrow shadows visible.
            foreach (var obstacle in TestObstacle.All)
            {
                if (!obstacle.isActiveAndEnabled || obstacle.Bounds.SqrDistance(center) > radius * radius) continue;
                if (obstacle.Body.enabled) occluders.Add(new AttackGeometry.OcclusionBox(obstacle, center));
                foreach (Vector2 corner in obstacle.WorldOutline())
                {
                    Vector2 offset = corner - center;
                    float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                    for (int side = -1; side <= 1; side++)
                    {
                        float local = Mathf.Repeat(angle - rotation + side * .001f, 360);
                        if (local <= sweep) angles.Add(local);
                    }
                }
            }
            angles.Sort();
            vertices.Clear(); triangles.Clear();
            foreach (float angle in angles)
            {
                Vector2 direction = AttackGeometry.Direction(angle + rotation);
                float visible = AttackGeometry.VisibleDistance(direction, radius, occluders);
                vertices.Add(direction * Mathf.Min(innerRadius, visible));
                vertices.Add(direction * visible);
            }
            for (int i = 0; i < angles.Count - 1; i++)
            {
                int n = i * 2;
                triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 3);
                triangles.Add(n); triangles.Add(n + 3); triangles.Add(n + 2);
            }
            RenderMesh(vertices, triangles, color);
        }
        public void Initialize(float radius, bool[] valid, Color color)
        {
            vertices.Clear(); triangles.Clear();
            for (int sector = 0; sector < valid.Length; sector++)
            {
                if (!valid[sector]) continue;
                for (int part = 0; part < 18; part++)
                {
                    float angle = (sector + part / 18f) * 360f / valid.Length;
                    int start = vertices.Count;
                    vertices.Add(Vector3.zero);
                    vertices.Add(AttackGeometry.Direction(angle) * radius);
                    vertices.Add(AttackGeometry.Direction(angle + 360f / valid.Length / 18) * radius);
                    triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
                }
            }
            RenderMesh(vertices, triangles, color);
        }
        public void InitializePolygon(IList<Vector2> outline, Color color, int order)
        {
            vertices.Clear(); triangles.Clear(); vertices.Add(Vector3.zero);
            foreach (var point in outline) vertices.Add(point);
            for (int i = 0; i < outline.Count; i++)
            { triangles.Add(0); triangles.Add(i + 1); triangles.Add((i + 1) % outline.Count + 1); }
            RenderMesh(vertices, triangles, color); meshRenderer.sortingOrder = order;
        }
        private void RenderMesh(List<Vector3> vertices, List<int> triangles, Color color)
        {
            if (mesh == null) { mesh = new Mesh { name = "PG04 temporary area" }; mesh.MarkDynamic(); }
            else mesh.Clear();
            // Sprites/Default multiplies its tint by vertex COLOR and samples TEXCOORD0.
            // Supply both explicitly for procedural meshes, including each burning-area rebuild.
            colors.Clear(); uv.Clear();
            for (int i = 0; i < vertices.Count; i++)
            {
                colors.Add(new Color32(255, 255, 255, 255));
                uv.Add(new Vector2(.5f, .5f));
            }
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            // Unity's missing-component wrappers use its overloaded null comparison.
            // ?? can retain that wrapper and throw before the burning area is created.
            if (filter == null) filter = GetComponent<MeshFilter>();
            if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            if (material == null) material = new Material(Shader.Find("Sprites/Default"));
            material.mainTexture = Texture2D.whiteTexture; material.color = color;
            meshRenderer.sharedMaterial = material; meshRenderer.sortingOrder = 1;
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}
