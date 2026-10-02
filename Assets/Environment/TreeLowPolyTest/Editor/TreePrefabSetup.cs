using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class TreePrefabSetup
    {
        private const string Root = "Assets/Environment/TreeLowPolyTest";

        [MenuItem("ROG ZOMBIE/Art/Crea prefab varianti abete")]
        public static void CreateVariants()
        {
            foreach (string name in new[] { "Abete_02_Slanciato", "Abete_04_Giovane", "Abete_05_Irregolare", "Abete_06_Salvia", "Abete_07_Oliva", "Abete_08_Salvia_Espanso" })
                CreateModelPrefab(name);
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
            string foliageName = name.Contains("Salvia") ? "FoliageSalvia" :
                name == "Abete_07_Oliva" ? "FoliageOliva" : "Foliage";
            var foliage = AssetDatabase.LoadAssetAtPath<Material>(Root + "/" + foliageName + ".mat");
            if (model == null || bark == null || foliage == null)
                throw new System.InvalidOperationException("Attendere l'importazione del modello e dei materiali dell'abete.");
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
