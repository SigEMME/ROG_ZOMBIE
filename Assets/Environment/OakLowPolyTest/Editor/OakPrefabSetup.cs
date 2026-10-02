using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class OakPrefabSetup
    {
        private const string Root = "Assets/Environment/OakLowPolyTest";

        [MenuItem("ROG ZOMBIE/Art/Crea prefab quercia di test")]
        public static void CreatePrefab()
        {
            CreateModelPrefab("Quercia_Test_01");
        }

        [MenuItem("ROG ZOMBIE/Art/Crea prefab varianti quercia")]
        public static void CreateVariants()
        {
            CreateModelPrefab("Quercia_02_Espansa");
            CreateModelPrefab("Quercia_03_Slanciata");
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
                throw new System.InvalidOperationException("Attendere l'importazione del modello e dei materiali della quercia.");
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
