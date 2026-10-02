using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class RockPrefabSetup
    {
        private const string Root = "Assets/Environment/RocksLowPoly";

        [MenuItem("ROG ZOMBIE/Art/Crea prefab massi low poly")]
        public static void CreatePrefabs()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Stone.mat");
            if (material == null) throw new System.InvalidOperationException("Materiale Stone.mat mancante.");
            if (!AssetDatabase.IsValidFolder(Root + "/Prefabs"))
                AssetDatabase.CreateFolder(Root, "Prefabs");
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { Root }))
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (model == null) continue;
                string path = Root + "/Prefabs/" + model.name + ".prefab";
                // Preserve artist changes and add only missing colliders.
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                {
                    var contents = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        if (EnsureColliders(contents))
                            PrefabUtility.SaveAsPrefabAsset(contents, path);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(contents); }
                    continue;
                }
                var instance = Object.Instantiate(model);
                try
                {
                    instance.name = model.name;
                    foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                        renderer.sharedMaterial = material;
                    EnsureColliders(instance);
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                }
                finally { Object.DestroyImmediate(instance); }
            }
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(Root + "/Prefabs"));
            Debug.Log("Prefab massi creati/aggiornati con collider 3D statici; nessuna scena modificata.");
        }

        private static bool EnsureColliders(GameObject root)
        {
            bool changed = false;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null) continue;
                var collider = filter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
                collider.convex = false;
                collider.isTrigger = false;
                changed = true;
            }
            return changed;
        }
    }
}
