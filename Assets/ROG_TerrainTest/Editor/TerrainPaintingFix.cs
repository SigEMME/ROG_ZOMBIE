#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ROG.TerrainTest
{
    // Separate from the scene generator; preserves existing TerrainData and paint.
    public static class TerrainPaintingFix
    {
        const string Key = "ROG.TerrainPaintingFix.";
        const string Folder = "Assets/ROG_TerrainTest/RenderingTest3D";

        [MenuItem("ROG ZOMBIE/Terreno/Configura pittura terreno 3D")]
        public static void Enable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Test terreno", "Esci da Play Mode prima di continuare.", "OK");
                return;
            }
            Terrain terrain = null;
            foreach (var t in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
                if (t.name == "Ground_140x140_DipingiQui") { terrain = t; break; }
            if (!terrain || !terrain.terrainData || terrain.terrainData.terrainLayers.Length == 0)
            {
                EditorUtility.DisplayDialog("Test terreno", "Apri prima la scena Test_Terreno_140x140 con il terreno e i suoi layer.", "OK");
                return;
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
            if (!pipeline) throw new InvalidOperationException("Pipeline URP definitiva mancante.");
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.globalTextureMipmapLimit = 0;
            SessionState.SetBool(Key + "Active", false);
            Shader shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
            if (!shader) throw new InvalidOperationException("Shader URP Terrain/Lit non trovato.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Terrain3D.mat");
            if (!material)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, Folder + "/Terrain3D.mat");
            }
            Undo.RecordObject(terrain, "Configura terreno test");
            terrain.enabled = true;
            terrain.drawHeightmap = true;
            terrain.materialTemplate = material;
            terrain.basemapDistance = 500;
            var collider = terrain.GetComponent<TerrainCollider>();
            if (!collider) collider = Undo.AddComponent<TerrainCollider>(terrain.gameObject);
            Undo.RecordObject(collider, "Configura collider terreno");
            collider.enabled = true;
            collider.terrainData = terrain.terrainData;
            terrain.Flush();
            foreach (var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (camera.gameObject.scene != terrain.gameObject.scene) continue;
                var extra = camera.GetUniversalAdditionalCameraData();
                Undo.RecordObject(extra, "Seleziona renderer 3D");
                extra.SetRenderer(0);
            }
            Selection.activeGameObject = terrain.gameObject;
            Tools.current = Tool.None;
            var view = EditorWindow.GetWindow<SceneView>();
            view.in2DMode = false;
            view.cameraMode = SceneView.GetBuiltinCameraMode(DrawCameraMode.Textured);
            var center = terrain.transform.position + terrain.terrainData.size * .5f;
            center.y = terrain.transform.position.y;
            view.LookAt(center, Quaternion.Euler(90, 0, 0), 90, true, true);
            view.Focus();
            EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);
            AssetDatabase.SaveAssets();
            SceneView.RepaintAll();
            Debug.Log("ROG TEST 3D attivo. Seleziona Paint Texture, Asfalto, pennello circolare e trascina con il sinistro nella Scene. La pittura esistente non e stata modificata.");
            EditorUtility.DisplayDialog("Test pittura 3D attivo", "Renderer 3D attivo, mipmap a piena risoluzione, Terrain e collider verificati.\n\nSeleziona Paint Texture > Asfalto e dipingi tenendo premuto il sinistro nella Scene.\n\nIl renderer 3D e ora permanente; non viene ripristinata la pipeline 2D.", "OK");
        }

        // Older editor sessions may retain this delegate until domain reload.
        // It must never restore the retired 2D pipeline.
        public static void Restore() { SessionState.SetBool(Key + "Active", false); }

        [InitializeOnLoadMethod]
        static void RegisterPermanentRendering()
        {
            EditorApplication.quitting -= Restore;
            SessionState.SetBool(Key + "Active", false);
        }
    }
}
#endif