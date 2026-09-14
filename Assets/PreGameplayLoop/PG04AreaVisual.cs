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
        public void InitializeOccluded(Vector2 center, float radius, Color color, float rotation = 0, float sweep = 360)
        {
            var angles = new List<float>();
            for (int i = 0; i <= Mathf.CeilToInt(sweep); i++) angles.Add(Mathf.Min(i, sweep));
            // Extra rays on both sides of blocker corners keep narrow shadows visible.
            foreach (var obstacle in TestObstacle.All)
            {
                if (!obstacle.isActiveAndEnabled || obstacle.Bounds.SqrDistance(center) > radius * radius) continue;
                var box = obstacle.GetComponent<BoxCollider2D>();
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                {
                    Vector2 corner = obstacle.transform.TransformPoint(box.offset + Vector2.Scale(box.size * .5f, new Vector2(x, y)));
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
            var vertices = new List<Vector3> { Vector3.zero }; var triangles = new List<int>();
            foreach (float angle in angles)
            {
                Vector2 direction = AttackGeometry.Direction(angle + rotation);
                vertices.Add(direction * AttackGeometry.VisibleDistance(center, direction, radius));
            }
            for (int i = 0; i < angles.Count - 1; i++)
            { triangles.Add(0); triangles.Add(i + 1); triangles.Add(i + 2); }
            RenderMesh(vertices, triangles, color);
        }
        public void Initialize(float radius, bool[] valid, Color color)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
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
        private void RenderMesh(List<Vector3> vertices, List<int> triangles, Color color)
        {
            if (mesh != null) Destroy(mesh);
            if (material != null) Destroy(material);
            mesh = new Mesh { name = "PG04 temporary area" };
            // Sprites/Default multiplies its tint by vertex COLOR and samples TEXCOORD0.
            // Supply both explicitly for procedural meshes, including each burning-area rebuild.
            var colors = new Color32[vertices.Count];
            var uv = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                colors[i] = new Color32(255, 255, 255, 255);
                uv[i] = new Vector2(.5f, .5f);
            }
            mesh.SetVertices(vertices); mesh.colors32 = colors; mesh.uv = uv;
            mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            // Unity's missing-component wrappers use its overloaded null comparison.
            // ?? can retain that wrapper and throw before the burning area is created.
            var filter = GetComponent<MeshFilter>();
            if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();
            material = new Material(Shader.Find("Sprites/Default"));
            material.mainTexture = Texture2D.whiteTexture; material.color = color;
            renderer.sharedMaterial = material; renderer.sortingOrder = 1;
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}
