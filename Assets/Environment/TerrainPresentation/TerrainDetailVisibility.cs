using UnityEngine;

namespace RogZombie.PlayTests
{
    // Uses Terrain's native detail culling, without tracking individual grass instances.
    public static class TerrainDetailVisibility
    {
        public static float DistanceForView(Camera camera, Terrain terrain, float margin)
        {
            if (camera == null || terrain == null || terrain.terrainData == null) return 0;
            var data = terrain.terrainData;
            Bounds bounds = data.bounds;
            float bottom = terrain.transform.position.y + bounds.min.y;
            float top = terrain.transform.position.y + bounds.max.y + 1f;
            float farthest = 0;
            for (int level = 0; level < 2; level++)
            {
                var plane = new Plane(Vector3.up, new Vector3(0, level == 0 ? bottom : top, 0));
                for (int corner = 0; corner < 4; corner++)
                {
                    Ray ray = camera.ViewportPointToRay(new Vector3(corner & 1, corner >> 1, 0));
                    // A view reaching the horizon cannot be bounded by ground intersections.
                    if (!plane.Raycast(ray, out float alongRay)) return camera.farClipPlane;
                    farthest = Mathf.Max(farthest, Vector3.Distance(camera.transform.position, ray.GetPoint(alongRay)));
                }
            }
            // Details are culled in patches. Keep a whole patch beyond the visible corners.
            float patchDiagonal = data.detailPatchCount > 0
                ? new Vector2(data.size.x, data.size.z).magnitude / data.detailPatchCount : 0;
            return Mathf.Min(camera.farClipPlane, farthest + Mathf.Max(0, margin) + patchDiagonal);
        }
    }
}
