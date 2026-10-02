using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ROG.TerrainTest
{
    public static class TerrainTestSetup
    {
        const string Root = "Assets/ROG_TerrainTest";
        static readonly string[] Names = { "Prato_Chiaro", "Prato_Scuro", "Asfalto", "Campo_01", "Campo_02", "Terra" };

        [MenuItem("ROG ZOMBIE/Terreno/Crea scena test 140 x 140")]
        public static void Create()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ConfigureTextures();
            string folder = AssetDatabase.GenerateUniqueAssetPath(Root + "/Test_140x140");
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var layers = new TerrainLayer[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                string texturePath = Root + "/Textures/Texture_" + Names[i];
                layers[i] = new TerrainLayer
                {
                    name = Names[i],
                    diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath + ".png"),
                    normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath + "_Normal.png"),
                    tileSize = new Vector2(8, 8),
                    normalScale = 1,
                    metallic = 0,
                    smoothness = 0.05f
                };
                if (!layers[i].diffuseTexture || !layers[i].normalMapTexture)
                    throw new InvalidOperationException("Texture mancante: " + Names[i]);
                AssetDatabase.CreateAsset(layers[i], folder + "/" + Names[i] + ".terrainlayer");
            }
            var data = new TerrainData
            {
                name = "Terreno_140x140",
                heightmapResolution = 513,
                alphamapResolution = 1024,
                baseMapResolution = 1024,
                size = new Vector3(140, 20, 140),
                terrainLayers = layers
            };
            data.SetHeights(0, 0, new float[513, 513]);
            var paint = new float[1024, 1024, layers.Length];
            for (int y = 0; y < 1024; y++)
                for (int x = 0; x < 1024; x++) paint[y, x, 0] = 1;
            data.SetAlphamaps(0, 0, paint);
            AssetDatabase.CreateAsset(data, folder + "/Terreno_140x140.asset");
            var ground = Terrain.CreateTerrainGameObject(data);
            ground.name = "Ground_140x140_DipingiQui";
            ground.transform.position = new Vector3(-70, 0, -70);
            var terrain = ground.GetComponent<Terrain>();
            terrain.drawInstanced = true;
            terrain.basemapDistance = 250;
            terrain.heightmapPixelError = 3;
            SetMaterial(terrain, folder);
            var sun = new GameObject("Sole_Test").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, -30, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.45f, .45f, .45f);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 130, -95);
            camera.transform.LookAt(Vector3.zero);
            camera.orthographic = true;
            camera.orthographicSize = 95;
            camera.farClipPlane = 500;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.22f, .25f, .29f);
            camera.gameObject.AddComponent<AudioListener>();
            Selection.activeGameObject = ground;
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.Euler(90, 0, 0), 100, true);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, folder + "/Test_Terreno_140x140.unity");
            if (data.size != new Vector3(140, 20, 140) || data.terrainLayers.Length != 6 || !ground.GetComponent<TerrainCollider>())
                throw new InvalidOperationException("Verifica Terrain fallita");
            Debug.Log("ROG_TERRAIN_VERIFIED: 140x140 m, 6 layers, 12 textures, collider, scene saved: " + folder);
        }

        static void ConfigureTextures()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Root + "/Textures" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                bool normal = path.EndsWith("_Normal.png", StringComparison.OrdinalIgnoreCase);
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.convertToNormalmap = false;
                importer.sRGBTexture = !normal;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        static void SetMaterial(Terrain terrain, string folder)
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            string type = pipeline ? pipeline.GetType().Name : "";
            string shaderName = type.Contains("Universal") ? "Universal Render Pipeline/Terrain/Lit"
                : type.Contains("HDRender") ? "HDRP/TerrainLit" : "Nature/Terrain/Standard";
            Shader shader = Shader.Find(shaderName);
            if (!shader) throw new InvalidOperationException("Shader Terrain non trovato: " + shaderName);
            var material = new Material(shader) { name = "Terreno_Materiale" };
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/Terreno_Materiale.mat");
            AssetDatabase.CreateAsset(material, path);
            terrain.materialTemplate = material;
        }

        [MenuItem("ROG ZOMBIE/Terreno/Adatta materiale alla pipeline attuale")]
        public static void Adapt()
        {
            foreach (var terrain in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                string path = AssetDatabase.GetAssetPath(terrain.terrainData);
                if (!path.StartsWith(Root + "/", StringComparison.Ordinal)) continue;
                Undo.RecordObject(terrain, "Adatta materiale Terrain");
                SetMaterial(terrain, Path.GetDirectoryName(path).Replace('\\', '/'));
                EditorUtility.SetDirty(terrain);
                EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);
            }
            AssetDatabase.SaveAssets();
        }

        public static void BuildPackage()
        {
            Create();
            string destination = Environment.GetEnvironmentVariable("ROG_TERRAIN_PACKAGE");
            if (string.IsNullOrEmpty(destination)) throw new InvalidOperationException("Missing package output path");
            AssetDatabase.ExportPackage(Root, destination, ExportPackageOptions.Recurse);
            Debug.Log("ROG_PACKAGE_EXPORTED: " + destination);
        }
    }
}
