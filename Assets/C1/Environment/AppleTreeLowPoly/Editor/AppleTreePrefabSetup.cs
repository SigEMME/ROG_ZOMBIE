using System;
using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class AppleTreePrefabSetup
    {
        private const string Root = "Assets/C1/Environment/AppleTreeLowPoly";

        [MenuItem("ROG ZOMBIE/Art/Crea variante melo grande")]
        public static void CreateLargePrefab()
        {
            string path = Root + "/Melo_03_Maturo.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) { EditorGUIUtility.PingObject(existing); return; }
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Melo_03_Maturo.obj");
            if (source == null) throw new InvalidOperationException("Attendere l'importazione di Melo_03_Maturo.obj.");
            var instance = UnityEngine.Object.Instantiate(source);
            try
            {
                instance.name = "Melo_03_Maturo";
                var trunk = instance.AddComponent<CapsuleCollider>();
                trunk.direction = 1;
                trunk.radius = .36f;
                trunk.height = 3.5f;
                trunk.center = new Vector3(0, 1.75f, 0);
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
                EditorGUIUtility.PingObject(prefab);
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        [MenuItem("ROG ZOMBIE/Art/Crea prefab melo")]
        public static void CreatePrefab()
        {
            string path = Root + "/Melo_01.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) { EditorGUIUtility.PingObject(existing); return; }
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Melo_01.obj");
            if (model == null) throw new InvalidOperationException("Attendere l'importazione di Melo_01.obj.");
            var instance = UnityEngine.Object.Instantiate(model);
            try
            {
                instance.name = "Melo_01";
                foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
                {
                    foreach (var material in renderer.sharedMaterials)
                        if (material == null) throw new InvalidOperationException("Materiale del melo mancante.");
                }
                // The canopy and decorative apples must not become physical obstacles.
                var trunk = instance.AddComponent<CapsuleCollider>();
                trunk.direction = 1;
                trunk.radius = .23f;
                trunk.height = 2.5f;
                trunk.center = new Vector3(0, 1.25f, 0);
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
                EditorGUIUtility.PingObject(prefab);
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
    }
}
