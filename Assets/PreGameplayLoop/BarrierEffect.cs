using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    // No Combatant, HP or damage receiver: the obstacle cannot be destroyed by attacks.
    [RequireComponent(typeof(BoxCollider2D), typeof(TestObstacle))]
    public sealed class BarrierEffect : MonoBehaviour
    {
        public PolygonCollider2D Solid { get; private set; }
        private Mesh navigationMesh;
        public float Remaining { get; private set; }

        public void Initialize(float duration) => Remaining = duration;

        public void Tick(float delta) => Remaining = Mathf.Max(0, Remaining - delta);

        public void Remove()
        {
            // Disable immediately: Destroy alone would retain collision until end of frame.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        public void DisplaceOverlappingActors(Vector2 areaSize)
        {
            var box = GetComponent<BoxCollider2D>();
            Physics2D.SyncTransforms();
            var overlapping = Physics2D.OverlapBoxAll(transform.position, box.size, transform.eulerAngles.z);
            foreach (var body in overlapping)
            {
                if (body == Solid || body.isTrigger || !IsActor(body) || !Physics2D.Distance(body, Solid).isOverlapped) continue;
                Vector2 original = body.bounds.center;
                float radius = body is CircleCollider2D ? Mathf.Max(body.bounds.extents.x, body.bounds.extents.y) :
                    ((Vector2)body.bounds.extents).magnitude;
                Vector2 local = transform.InverseTransformPoint(original);
                Vector2 best = default;
                float bestDistance = float.PositiveInfinity;
                // Geometric search resolution, not a gameplay distance. Expand only if adjacent
                // positions are occupied by level geometry or other displaced actors.
                float step = Mathf.Max(radius, .05f);
                for (float extra = 0; extra <= areaSize.magnitude; extra += step)
                {
                    Vector2 extent = box.size * .5f + Vector2.one * (radius + .002f + extra);
                    Consider(new Vector2(-extent.x, Mathf.Clamp(local.y, -extent.y, extent.y)));
                    Consider(new Vector2(extent.x, Mathf.Clamp(local.y, -extent.y, extent.y)));
                    Consider(new Vector2(Mathf.Clamp(local.x, -extent.x, extent.x), -extent.y));
                    Consider(new Vector2(Mathf.Clamp(local.x, -extent.x, extent.x), extent.y));
                    for (float x = -extent.x; x <= extent.x; x += step)
                    { Consider(new Vector2(x, -extent.y)); Consider(new Vector2(x, extent.y)); }
                    for (float y = -extent.y; y <= extent.y; y += step)
                    { Consider(new Vector2(-extent.x, y)); Consider(new Vector2(extent.x, y)); }
                    if (!float.IsPositiveInfinity(bestDistance)) break;
                }
                if (float.IsPositiveInfinity(bestDistance))
                {
                    Debug.LogError("BARRIERA: nessuna posizione libera per espellere " + body.name + ". Verificare il level design.", this);
                    continue;
                }
                Vector2 displacement = best - original;
                if (body.attachedRigidbody != null) body.attachedRigidbody.position += displacement;
                else body.transform.position += (Vector3)displacement;
                Physics2D.SyncTransforms();

                void Consider(Vector2 candidateLocal)
                {
                    Vector2 candidate = transform.TransformPoint(candidateLocal);
                    if (Mathf.Abs(candidate.x) + radius >= areaSize.x * .5f ||
                        Mathf.Abs(candidate.y) + radius >= areaSize.y * .5f) return;
                    float distance = (candidate - original).sqrMagnitude;
                    if (distance >= bestDistance) return;
                    foreach (var other in Physics2D.OverlapCircleAll(candidate, radius))
                    {
                        if (other == body || other.isTrigger) continue;
                        if (other.GetComponent<TestObstacle>() != null || IsActor(other)) return;
                    }
                    best = candidate;
                    bestDistance = distance;
                }
            }
        }

        private static bool IsActor(Collider2D body)
        {
            var actor = body.GetComponent<Combatant>();
            if (actor != null) return actor.State != LifeState.Dead;
            return body.gameObject.layer == LayerMask.NameToLayer("PG") ||
                body.gameObject.layer == LayerMask.NameToLayer("MOB") ||
                body.gameObject.layer == LayerMask.NameToLayer("PET");
        }

        public static BarrierEffect Spawn(Vector2 centre, float angle, PG01AbilityCatalog data, BarrierGeometry geometry = null)
        {
            if (geometry == null) { geometry = new BarrierGeometry(); Physics2D.SyncTransforms(); geometry.Build(centre, data.BarrierSize, angle); }
            if (!geometry.HasArea) return null;
            var go = new GameObject("BARRIERA PG01"); go.transform.SetParent(TestVisuals.Root, false);
            go.transform.SetPositionAndRotation(centre, Quaternion.Euler(0, 0, angle));
            go.layer = LayerMask.NameToLayer("OSTACOLO");
            // Retain the bounding box only for the established displacement search; it is never physical.
            var box = go.AddComponent<BoxCollider2D>(); box.size = data.BarrierSize; box.enabled = false;
            var solid = go.AddComponent<PolygonCollider2D>(); solid.SetPath(0, geometry.Points);
            var obstacle = go.AddComponent<TestObstacle>(); obstacle.IsWall = false; obstacle.UsePolygon(solid);
            go.AddComponent<PG04AreaVisual>().InitializePolygon(geometry.Points, new Color(.25f, .7f, .95f), 2);
            var effect = go.AddComponent<BarrierEffect>(); effect.Solid = solid;
            effect.navigationMesh = BuildNavigationMesh(geometry.Points); obstacle.NavigationMesh = effect.navigationMesh;
            effect.Initialize(data.BarrierDuration);
            Physics2D.SyncTransforms();
            return effect;
        }
        private static Mesh BuildNavigationMesh(IList<Vector2> outline)
        {
            var vertices = new List<Vector3> { Vector3.zero, new Vector3(0, 2, 0) };
            var triangles = new List<int>();
            foreach (var point in outline) { vertices.Add(new Vector3(point.x, 0, point.y)); vertices.Add(new Vector3(point.x, 2, point.y)); }
            for (int i = 0; i < outline.Count; i++)
            {
                int a = 2 + i * 2, b = 2 + ((i + 1) % outline.Count) * 2;
                triangles.AddRange(new[] { 0, a, b, 1, b + 1, a + 1, a, a + 1, b + 1, a, b + 1, b });
            }
            var mesh = new Mesh { name = "BARRIERA clipped navigation" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds(); return mesh;
        }
        private void OnDestroy() { if (navigationMesh != null) Destroy(navigationMesh); }
    }
}
