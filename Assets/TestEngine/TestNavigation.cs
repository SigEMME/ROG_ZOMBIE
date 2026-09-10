using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RogZombie.TestEngine
{
    public sealed class TestNavigation : MonoBehaviour
    {
        public bool ShowNavMesh;
        public float AgentRadius = 0.35f;
        private NavMeshData data;
        private NavMeshDataInstance instance;
        private NavMeshTriangulation triangles;
        public bool Ready { get; private set; }

        public static Vector3 ToNav(Vector2 xy) => new Vector3(xy.x, 0f, xy.y);
        public static Vector2 FromNav(Vector3 xz) => new Vector2(xz.x, xz.z);

        public void Build(Vector2 size)
        {
            // Build on Unity's XZ navigation plane, then map path corners back to the game's XY plane.
            // No renderer, camera, ProjectSettings or navigation package changes are needed.
            var sources = new List<NavMeshBuildSource>
            {
                new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = Matrix4x4.TRS(new Vector3(0f, -0.2f, 0f), Quaternion.identity, Vector3.one),
                    size = new Vector3(size.x, 0.4f, size.y), area = 0
                }
            };
            Physics2D.SyncTransforms();
            foreach (var wall in TestObstacle.All)
            {
                Bounds bounds = wall.Bounds;
                sources.Add(new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = Matrix4x4.TRS(new Vector3(bounds.center.x, 1f, bounds.center.y), Quaternion.identity, Vector3.one),
                    size = new Vector3(bounds.size.x, 2f, bounds.size.y), area = 1
                });
            }
            var settings = NavMesh.GetSettingsByIndex(0);
            settings.agentRadius = AgentRadius;
            settings.overrideVoxelSize = true;
            settings.voxelSize = AgentRadius / 3f;
            settings.minRegionArea = 0f;
            data = NavMeshBuilder.BuildNavMeshData(settings, sources,
                new Bounds(Vector3.zero, new Vector3(size.x + 4f, 10f, size.y + 4f)), Vector3.zero, Quaternion.identity);
            if (data == null) { Debug.LogError("TEST ENGINE: NavMesh build failed.", this); return; }
            instance = NavMesh.AddNavMeshData(data);
            Ready = instance.valid;
            triangles = NavMesh.CalculateTriangulation();
        }

        public bool Sample(Vector2 point, out Vector2 valid, float maxDistance = 1f)
        {
            valid = point;
            if (!Ready || !NavMesh.SamplePosition(ToNav(point), out var hit, maxDistance, NavMesh.AllAreas)) return false;
            valid = FromNav(hit.position);
            return true;
        }

        public bool Path(Vector2 from, Vector2 to, NavMeshPath path)
        {
            if (!Sample(from, out var start) || !Sample(to, out var end)) return false;
            return NavMesh.CalculatePath(ToNav(start), ToNav(end), NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        public bool Reachable(Vector2 from, Vector2 to) => Path(from, to, new NavMeshPath());

        private void OnDestroy()
        {
            if (instance.valid) instance.Remove();
            if (data != null) Destroy(data);
        }

        private void OnDrawGizmos()
        {
            if (!ShowNavMesh || !Ready) return;
            Gizmos.color = new Color(0.1f, 0.8f, 0.9f, 0.4f);
            for (int i = 0; i < triangles.indices.Length; i += 3)
                for (int j = 0; j < 3; j++)
                    Gizmos.DrawLine(FromNav(triangles.vertices[triangles.indices[i + j]]),
                        FromNav(triangles.vertices[triangles.indices[i + (j + 1) % 3]]));
        }
    }
}
