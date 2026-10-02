using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class CratePrefabSetup
    {
        private const string Root = "Assets/Environment/CratesLowPoly";

        [MenuItem("ROG ZOMBIE/Art/Crea prefab casse low poly")]
        public static void CreatePrefabs()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/CrateAtlas.mat");
            if (material == null)
                throw new System.InvalidOperationException("Attendere l'importazione del materiale delle casse.");
            string[] names =
            {
                "Cassa_01_Rinforzata", "Cassa_02_Invecchiata", "Cassa_03_Lunga",
                "Cassa_04_Danneggiata", "Cassa_05_Lunga_Rotta", "Sacchi_06_Pallet"
            };
            foreach (string name in names)
            {
                string path = Root + "/" + name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                    continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + name + ".obj");
                if (model == null)
                    throw new System.InvalidOperationException("Modello non importato: " + name);
                var instance = Object.Instantiate(model);
                try
                {
                    instance.name = name;
                    foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                        renderer.sharedMaterial = material;
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                }
                finally { Object.DestroyImmediate(instance); }
            }
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(material);
        }
    }
}
