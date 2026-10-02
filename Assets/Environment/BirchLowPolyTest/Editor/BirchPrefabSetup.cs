using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class BirchPrefabSetup
    {
        private const string Root = "Assets/Environment/BirchLowPolyTest";

        [MenuItem("ROG ZOMBIE/Art/Crea prefab betulla di test")]
        public static void CreatePrefab()
        {
            CreateModelPrefab("Betulla_Test_01");
        }

        [MenuItem("ROG ZOMBIE/Art/Crea prefab varianti betulla")]
        public static void CreateVariants()
        {
            CreateModelPrefab("Betulla_02_Giovane");
            CreateModelPrefab("Betulla_03_Espansa");
        }

        private static void CreateModelPrefab(string name)
        {
            string path = Root + "/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                return;
            }
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + name + ".obj");
            var bark = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Bark.mat");
            var foliage = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Foliage.mat");
            if (model == null || bark == null || foliage == null)
                throw new System.InvalidOperationException("Attendere l'importazione del modello e dei materiali della betulla.");
            var instance = Object.Instantiate(model);
            try
            {
                instance.name = name;
                foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] == null)
                            throw new System.InvalidOperationException("Materiale OBJ non risolto: reimportare " + name + ".obj.");
                        materials[i] = materials[i].name.Contains("Foliage") ? foliage : bark;
                    }
                    renderer.sharedMaterials = materials;
                }
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
                EditorGUIUtility.PingObject(prefab);
            }
            finally { Object.DestroyImmediate(instance); }
        }
    }
}
