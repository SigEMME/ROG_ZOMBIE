using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ROGZOMBIE.EnvironmentArt
{
    public static class C1AfternoonLighting
    {
        public const string ProfilePath = "Assets/C1/Lighting/C1_A2_TardoPomeriggio.asset";

        [MenuItem("ROG ZOMBIE/Art/C1_A2/Applica luce tardo pomeriggio")]
        public static void Apply()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/C1_A2.unity" || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Aprire C1_A2 fuori dal Play Mode.");
            Light sun = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var candidate in root.GetComponentsInChildren<Light>(true))
                    if (candidate.type == LightType.Directional && candidate.name == "Sole_Test") sun = candidate;
            if (!sun) throw new InvalidOperationException("Sole_Test non trovato: nessuna luce aggiunta automaticamente.");
            Undo.RecordObjects(new UnityEngine.Object[] { sun, sun.transform }, "Luce C1 pomeriggio");
            sun.color = new Color(1f, .86f, .72f);
            sun.useColorTemperature = false;
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = .8f;
            sun.transform.rotation = Quaternion.Euler(32f, -48f, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.40f, .45f, .54f);

            if (!AssetDatabase.IsValidFolder("Assets/C1/Lighting")) AssetDatabase.CreateFolder("Assets/C1", "Lighting");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (!profile)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
                var grading = profile.Add<ColorAdjustments>(false);
                grading.saturation.Override(-12f);
                grading.contrast.Override(5f);
                grading.postExposure.Override(0f);
                AssetDatabase.AddObjectToAsset(grading, profile);
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssetIfDirty(profile);
            }
            Volume volume = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "Atmosfera_C1_TardoPomeriggio") volume = root.GetComponent<Volume>();
            if (!volume)
            {
                var go = new GameObject("Atmosfera_C1_TardoPomeriggio");
                Undo.RegisterCreatedObjectUndo(go, "Profilo colore C1");
                volume = go.AddComponent<Volume>();
            }
            Undo.RecordObject(volume, "Profilo colore C1");
            volume.isGlobal = true;
            volume.priority = 10;
            volume.weight = 1;
            volume.sharedProfile = profile;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                {
                    var data = camera.GetComponent<UniversalAdditionalCameraData>();
                    if (!data) data = Undo.AddComponent<UniversalAdditionalCameraData>(camera.gameObject);
                    Undo.RecordObject(data, "Colore camera C1");
                    data.renderPostProcessing = true;
                }
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
