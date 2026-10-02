using System;
using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    // Apply colliders during import so direct OBJ instances and newly created prefabs inherit them.
    public sealed class EnvironmentColliderImporter : AssetPostprocessor
    {
        internal static readonly string[] Roots =
        {
            "Assets/Environment/CratesLowPoly",
            "Assets/Environment/FarmBuildingsLowPoly",
            "Assets/Environment/TreeLowPolyTest",
            "Assets/Environment/OakLowPolyTest",
            "Assets/Environment/BirchLowPolyTest"
        };

        public override uint GetVersion() => 1;

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)) return;
            int category = Array.FindIndex(Roots, folder =>
                assetPath.StartsWith(folder + "/", StringComparison.Ordinal));
            if (category < 0) return;

            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = filter.sharedMesh;
                if (mesh == null || filter.GetComponent<Collider>() != null) continue;
                if (category == 0)
                {
                    var box = filter.gameObject.AddComponent<BoxCollider>();
                    box.center = mesh.bounds.center;
                    box.size = mesh.bounds.size;
                }
                else if (category == 1)
                {
                    var collider = filter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = mesh;
                    collider.convex = false;
                    collider.isTrigger = false;
                }
                else
                {
                    AddTrunkCollider(filter);
                }
            }
        }

        private static void AddTrunkCollider(MeshFilter filter)
        {
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            var mesh = filter.sharedMesh;
            var vertices = mesh.vertices;
            var materials = renderer.sharedMaterials;
            var indices = new System.Collections.Generic.HashSet<int>();
            for (int submesh = 0; submesh < mesh.subMeshCount && submesh < materials.Length; submesh++)
            {
                var material = materials[submesh];
                if (material == null || material.name.IndexOf("Bark", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                foreach (int index in mesh.GetTriangles(submesh)) indices.Add(index);
            }
            // Do not turn alpha-cutout foliage planes into physical obstacles.
            if (indices.Count == 0) return;
            bool first = true;
            Bounds wood = default;
            foreach (int index in indices)
            {
                if (first) { wood = new Bounds(vertices[index], Vector3.zero); first = false; }
                else wood.Encapsulate(vertices[index]);
            }
            // Fit the lower trunk, excluding the spreading branches above it.
            float cutoff = wood.min.y + wood.size.y * 0.2f;
            Bounds lower = default;
            first = true;
            foreach (int index in indices)
            {
                var point = vertices[index];
                if (point.y > cutoff) continue;
                if (first) { lower = new Bounds(point, Vector3.zero); first = false; }
                else lower.Encapsulate(point);
            }
            if (first) return;
            float radius = Mathf.Max(lower.extents.x, lower.extents.z);
            if (radius <= 0f) return;
            var collider = filter.gameObject.AddComponent<CapsuleCollider>();
            collider.direction = 1;
            collider.radius = radius;
            collider.height = Mathf.Max(radius * 2f, wood.size.y);
            collider.center = new Vector3(lower.center.x, wood.min.y + collider.height * 0.5f, lower.center.z);
            collider.isTrigger = false;
        }
    }

    [InitializeOnLoad]
    public static class EnvironmentColliderRefresh
    {
        private const string SessionKey = "ROG.EnvironmentColliders.Import.v1";
        static EnvironmentColliderRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool(SessionKey, false)) return;
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                ReimportModels();
                SessionState.SetBool(SessionKey, true);
            };
        }

        [MenuItem("ROG ZOMBIE/Art/Aggiorna collider casse edifici e alberi")]
        public static void ReimportModels()
        {
            int count = 0;
            foreach (string folder in EnvironmentColliderImporter.Roots)
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)) continue;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    count++;
                }
            }
            Debug.Log("ROG ZOMBIE: reimportati " + count + " modelli con collider 3D statici.");
        }
    }
}
