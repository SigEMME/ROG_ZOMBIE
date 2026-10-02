using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class FencePrefabSetup
    {
        private const string Root = "Assets/Environment/FencesLowPoly";

        [MenuItem("ROG ZOMBIE/Art/Crea prefab staccionate low poly")]
        public static void CreatePrefabs()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Wood.mat");
            if (material == null) throw new System.InvalidOperationException("Materiale Wood.mat mancante.");
            if (!AssetDatabase.IsValidFolder(Root + "/Prefabs"))
                AssetDatabase.CreateFolder(Root, "Prefabs");
            foreach (string variant in new[] { "Piena", "Aperta", "Rinforzata", "Danneggiata" })
            {
                string name = "Staccionata_" + variant;
                string path = Root + "/Prefabs/" + name + ".prefab";
                // Update only missing colliders, preserving existing artist changes.
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
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + name + ".obj");
                if (model == null) throw new System.InvalidOperationException("Modello non importato: " + name);
                var instance = Object.Instantiate(model);
                try
                {
                    instance.name = name;
                    foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                        renderer.sharedMaterial = material;
                    EnsureColliders(instance);
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                }
                finally { Object.DestroyImmediate(instance); }
            }
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(Root + "/Prefabs"));
            Debug.Log("Staccionate: prefab creati/aggiornati con collider 3D statici.");
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
