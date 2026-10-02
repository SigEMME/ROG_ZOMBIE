using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ROGZOMBIE.EnvironmentArt
{
    [InitializeOnLoad]
    public static class VehiclePrefabSetup
    {
        const string Request = "Art/Environment/Vehicles-v1/create.request";
        static readonly string[] Names = { "CAR01", "BUS01", "TRACK01", "Tractor01", "Pickup01" };
        static VehiclePrefabSetup() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            try { Create(); }
            catch (Exception e) { File.WriteAllText("Art/Environment/Vehicles-v1/ERROR.txt", e.ToString()); Debug.LogException(e); }
        }
        [MenuItem("ROG ZOMBIE/Art/Crea prefab veicoli importati")]
        public static void Create()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit non disponibile.");
            var report = new StringBuilder("VEICOLI - verifica importazione Unity\n");
            foreach (string name in Names)
            {
                string folder = "Assets/Environment/" + name;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/" + name + "_Unity.fbx");
                if (model == null) throw new InvalidOperationException("FBX non importato: " + name);
                foreach (string png in Directory.GetFiles(folder, "*_BaseColor_*.png"))
                {
                    string texturePath = png.Replace('\\', '/');
                    string color = Path.GetFileNameWithoutExtension(png).Split(new[] { "_BaseColor_" }, StringSplitOptions.None)[1];
                    string prefabPath = folder + "/" + name + "_" + color + ".prefab";
                    if (File.Exists(prefabPath)) { report.AppendLine("Preservato prefab esistente: " + prefabPath); continue; }
                    var textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                    if (textureImporter.filterMode != FilterMode.Point || !textureImporter.mipmapEnabled)
                    {
                        textureImporter.filterMode = FilterMode.Point;
                        textureImporter.mipmapEnabled = true;
                        textureImporter.SaveAndReimport();
                    }
                    string materialPath = folder + "/" + name + "_" + color + ".mat";
                    var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    if (material == null)
                    {
                        material = new Material(shader) { name = name + "_" + color, enableInstancing = true };
                        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
                        material.SetFloat("_Smoothness", 0f);
                        material.SetFloat("_Metallic", 0f);
                        AssetDatabase.CreateAsset(material, materialPath);
                    }
                    var root = new GameObject(name + "_" + color);
                    try
                    {
                        var instance = UnityEngine.Object.Instantiate(model, root.transform);
                        instance.name = name + "_Model";
                        var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                        if (renderers.Length == 0) throw new InvalidOperationException("Nessun renderer: " + name);
                        Bounds bounds = renderers[0].bounds;
                        int triangles = 0;
                        foreach (var renderer in renderers)
                        {
                            var mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
                            if (mesh == null) throw new InvalidOperationException("Mesh mancante: " + renderer.name);
                            if (mesh.vertexCount == 0 || mesh.uv.Length != mesh.vertexCount)
                                throw new InvalidOperationException("Vertici o UV non validi: " + name);
                            for (int s = 0; s < mesh.subMeshCount; s++) triangles += (int)mesh.GetIndexCount(s) / 3;
                            renderer.sharedMaterials = Enumerable.Repeat(material, mesh.subMeshCount).ToArray();
                            bounds.Encapsulate(renderer.bounds);
                        }
                        // Center ground pivot without altering the imported FBX or its scale.
                        instance.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                        foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
                            UnityEngine.Object.DestroyImmediate(collider);
                        var box = root.AddComponent<BoxCollider>();
                        box.center = new Vector3(0, bounds.size.y * .5f, 0);
                        box.size = bounds.size;
                        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                        if (prefab == null) throw new InvalidOperationException("Salvataggio fallito: " + prefabPath);
                        report.AppendLine($"{name}_{color}: {triangles} triangoli; dimensioni XYZ {bounds.size.ToString("F3")} m; {renderers.Length} renderer; UV OK; BoxCollider statico; pivot a terra.");
                    }
                    finally { UnityEngine.Object.DestroyImmediate(root); }
                }
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText("Art/Environment/Vehicles-v1/Unity-validation.txt", report.ToString());
            Debug.Log(report.ToString());
        }
    }
}
