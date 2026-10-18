using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RogZombie.TestEngine
{
    // Presentation only. Combat and navigation retain their circular XY collider.
    // Reapply world-space terrain placement after PlayerAim (0), separation (50)
    // and PlayerWeapon (100). Parent aim rotation must not move the mapped child.
    [DefaultExecutionOrder(200)]
    public sealed class ActorCapsuleVisual : MonoBehaviour
    {
        public static readonly Color PlayerColor = new Color32(255, 210, 65, 255);
        private static Mesh capsuleMesh;
        private static Material capsuleMaterial;
        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");
        private SpriteRenderer source;
        private MeshRenderer body;
        private MaterialPropertyBlock properties;
        private Color previousColor;
        private float radius;

        public void Initialize(float radius)
        {
            this.radius = radius;
            source = GetComponent<SpriteRenderer>();
            source.forceRenderingOff = true; // Keep the existing color/status API for MOB effects.
            if (capsuleMesh == null) capsuleMesh = CreateMesh();
            if (capsuleMaterial == null)
                capsuleMaterial = new Material(Resources.Load<Shader>("ActorCapsule"))
                { name = "Shared actor capsule", enableInstancing = true };
            var visual = new GameObject("Capsula 3D");
            visual.layer = gameObject.layer;
            visual.transform.SetParent(transform, false);
            // Combat lives in XY: standing above the ground means extending toward -Z.
            visual.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            visual.transform.localPosition = new Vector3(0, 0, -radius * 2);
            visual.transform.localScale = Vector3.one * (radius * 2);
            visual.AddComponent<MeshFilter>().sharedMesh = capsuleMesh;
            body = visual.AddComponent<MeshRenderer>();
            body.sharedMaterial = capsuleMaterial;
            body.shadowCastingMode = ShadowCastingMode.Off;
            body.receiveShadows = false;
            properties = new MaterialPropertyBlock();
            previousColor = new Color(-1, -1, -1, -1);
            Sync();
        }

        private void LateUpdate() => Sync();

        private void Sync()
        {
            if (source == null || body == null) return;
            body.enabled = source.enabled;
            var terrain = RogZombie.PlayTests.TerrainGameplayBridge.ActivePresentation;
            if (terrain != null)
            {
                // Render together with scenery so the same depth buffer occludes the capsule.
                body.transform.SetPositionAndRotation(terrain.ActorPresentationPosition(transform.position)
                    + Vector3.up * (radius * 2), Quaternion.identity);
                body.gameObject.layer = terrain.PresentationLayer;
                body.sortingLayerID = 0;
                body.sortingOrder = 0;
            }
            else
            {
                body.transform.localPosition = new Vector3(0, 0, -radius * 2);
                body.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                body.gameObject.layer = gameObject.layer;
                body.sortingLayerID = source.sortingLayerID;
                body.sortingOrder = source.sortingOrder;
            }
            if (previousColor == source.color) return;
            previousColor = source.color;
            properties.SetColor(ColorId, previousColor);
            body.SetPropertyBlock(properties);
        }

        private static Mesh CreateMesh()
        {
            const int sides = 16, rings = 10;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            for (int ring = 0; ring < rings; ring++)
            {
                bool upper = ring >= 5;
                float angle = (upper ? ring - 5 : ring - 4) * Mathf.PI / 8;
                for (int side = 0; side < sides; side++)
                {
                    float a = side * Mathf.PI * 2 / sides;
                    var normal = new Vector3(Mathf.Cos(angle) * Mathf.Cos(a), Mathf.Sin(angle), Mathf.Cos(angle) * Mathf.Sin(a));
                    vertices.Add(normal * .5f + Vector3.up * (upper ? .5f : -.5f));
                    normals.Add(normal);
                }
            }
            for (int ring = 0; ring < rings - 1; ring++)
                for (int side = 0; side < sides; side++)
                {
                    int a = ring * sides + side, b = ring * sides + (side + 1) % sides;
                    if (ring > 0) triangles.AddRange(new[] { a, a + sides, b });
                    if (ring < rings - 2) triangles.AddRange(new[] { b, a + sides, b + sides });
                }
            var mesh = new Mesh { name = "Shared low-poly capsule" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds(); mesh.UploadMeshData(false);
            return mesh;
        }
    }
}
