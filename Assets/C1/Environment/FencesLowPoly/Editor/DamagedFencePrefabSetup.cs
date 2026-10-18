using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class DamagedFencePrefabSetup
    {
        public const string Root = "Assets/C1/Environment/FencesLowPoly";
        [MenuItem("ROG ZOMBIE/Art/Crea staccionate aperta e rinforzata danneggiate")]
        public static void CreatePrefabs()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Wood.mat");
            if (!material) throw new System.InvalidOperationException("Wood.mat mancante.");
            foreach (var kind in new[] { "Aperta", "Rinforzata" })
            {
                string name = "Staccionata_" + kind + "_Danneggiata";
                string path = Root + "/Prefabs/" + name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path)) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + name + ".obj");
                if (!model) throw new System.InvalidOperationException("Modello non importato: " + name);
                var instance = Object.Instantiate(model);
                try
                {
                    instance.name = name;
                    foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
                        renderer.sharedMaterial = material;
                    foreach (var filter in instance.GetComponentsInChildren<MeshFilter>())
                    {
                        // Match the existing fences: static collider follows the actual wood.
                        var collider = filter.GetComponent<MeshCollider>();
                        if (!collider) collider = filter.gameObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = filter.sharedMesh;
                        collider.convex = false;
                    }
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                }
                finally { Object.DestroyImmediate(instance); }
            }
        }
    }
}
