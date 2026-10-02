using UnityEditor;
using UnityEngine;
namespace ROGZOMBIE.EnvironmentArt
{
    public static class FarmBuildingPrefabSetup
    {
        private const string Root = "Assets/Environment/FarmBuildingsLowPoly";
        [MenuItem("ROG ZOMBIE/Art/Crea prefab edifici rurali")]
        public static void CreatePrefabs()
        {
            var atlas = AssetDatabase.LoadAssetAtPath<Material>(Root + "/FarmAtlas.mat");
            var dark = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Dark.mat");
            if (atlas == null || dark == null)
                throw new System.InvalidOperationException("Attendere l'importazione dei materiali rurali.");
            foreach (var baseName in new[] { "Fienile_01_Annesso", "Rimessa_02_Rurale", "Casa_03_Abbandonata" })
            foreach (var suffix in new[] { "", "_Salvia", "_Azzurro" })
            {
                var name = baseName + suffix;
                var variantAtlas = suffix.Length == 0 ? atlas :
                    AssetDatabase.LoadAssetAtPath<Material>(Root + "/FarmAtlas" + suffix + ".mat");
                if (variantAtlas == null)
                    throw new System.InvalidOperationException("Materiale variante non importato: " + suffix);
                var path = Root + "/" + name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + name + ".obj");
                if (model == null) throw new System.InvalidOperationException("Modello non importato: " + name);
                var instance = Object.Instantiate(model);
                try
                {
                    instance.name = name;
                    foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var materials = renderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i] == null) throw new System.InvalidOperationException("Materiale mancante: " + name);
                            materials[i] = materials[i].name.Contains("Dark") ? dark : variantAtlas;
                        }
                        renderer.sharedMaterials = materials;
                    }
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                }
                finally { Object.DestroyImmediate(instance); }
            }
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(atlas);
        }
    }
}
