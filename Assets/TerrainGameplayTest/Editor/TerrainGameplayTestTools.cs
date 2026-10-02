using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PlayTests
{
    // Explicit validation request only. Never discard dirty scenes or interrupt an existing Play session.
    [InitializeOnLoad]
    public static class TerrainGameplayTestTools
    {
        private const string ScenePath = "Assets/Scenes/Terreno_GameplayTest.unity";
        private static readonly string Request = Path.Combine(Path.GetTempPath(), "ROG_TerrainGameplay.request");
        private static readonly string Report = Path.Combine(Path.GetTempPath(), "ROG_TerrainGameplay.report");
        private const string Running = "ROG.TerrainGameplay.Validation";
        private static double began;
        static TerrainGameplayTestTools()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Running, false))
                    began = EditorApplication.timeSinceStartup;
            };
        }
        [MenuItem("ROG ZOMBIE/Terreno/Apri prova gameplay PG e MOB")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }
        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (File.Exists(Request) && !SessionState.GetBool(Running, false))
            {
                if (File.ReadAllText(Request).Trim() != Application.dataPath) return;
                File.Delete(Request);
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                    if (EditorSceneManager.GetSceneAt(i).isDirty)
                    { File.WriteAllText(Report, "NOT RUN: open scene contains unsaved changes."); return; }
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                { File.WriteAllText(Report, "NOT RUN: existing Play session."); return; }
                EditorSceneManager.OpenScene(ScenePath);
                SessionState.SetBool(Running, true);
                EditorApplication.isPlaying = true;
                return;
            }
            if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
            var test = UnityEngine.Object.FindFirstObjectByType<TestEngineBootstrap>();
            if (test != null && test.Failure != null) Finish("FAIL: " + test.Failure);
            else if (test != null && !test.Loading && test.Player != null && test.Spawns != null &&
                     test.Spawns.InitialComplete && EditorApplication.timeSinceStartup - began > 3)
            {
                var terrainCamera = GameObject.Find("Terrain presentation camera")?.GetComponent<Camera>();
                bool rendered = terrainCamera != null && terrainCamera.targetTexture != null && terrainCamera.targetTexture.IsCreated();
                Finish((rendered ? "PASS" : "FAIL background") + ": PG=" + test.Player.Definition.PlayerId +
                       ", MOB=" + test.Spawns.Alive + ", obstacles=" + TestObstacle.All.Count +
                       ", navigation=" + test.Navigation.Ready + ", render target=" + rendered);
            }
            else if (EditorApplication.timeSinceStartup - began > 45) Finish("FAIL: loading timed out.");
        }
        private static void Finish(string result)
        {
            File.WriteAllText(Report, result);
            SessionState.SetBool(Running, false);
            EditorApplication.isPlaying = false;
        }
    }
}
