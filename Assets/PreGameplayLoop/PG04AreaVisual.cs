using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    // Temporary filled sectors show exactly which parts of an explosion remain valid.
    public sealed class PG04AreaVisual : MonoBehaviour
    {
        private Mesh mesh;
        private Material material;
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
            mesh = new Mesh { name = "PG04 temporary sectors" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();
            material = new Material(Shader.Find("Sprites/Default")); material.color = color;
            renderer.sharedMaterial = material; renderer.sortingOrder = 1;
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}
