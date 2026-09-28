using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    [InitializeOnLoad]
    public static class TopDownLevelsValidation
    {
        private const string Key = "ROG.TopDownLevels";
        private static IEnumerator checks;
        private static readonly List<string> results = new List<string>();
        private static double deadline;
        private static int warnings;
        static TopDownLevelsValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
                { results.Clear(); warnings = 0; deadline = EditorApplication.timeSinceStartup + 150; checks = Checks(); }
            };
            Application.logMessageReceived += (message, stack, type) =>
            {
                if (checks == null) return;
                if (type == LogType.Warning) warnings++;
                if (type == LogType.Error || type == LogType.Exception) Finish(false, message + stack);
            };
        }
        private static void Tick()
        {
            const string request = ".top-down-levels-test.request";
            if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(request) &&
                !UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
            {
                SessionState.SetString(Key + "Report", File.ReadAllText(request).Trim()); File.Delete(request);
                SessionState.SetBool(Key + "Visual", File.Exists(".top-down-levels-visual.request"));
                if (File.Exists(".top-down-levels-visual.request")) File.Delete(".top-down-levels-visual.request");
                EditorSceneManager.OpenScene("Assets/Scenes/HubPrototype.unity");
                SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
            }
            if (checks == null) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timeout");
                if (!checks.MoveNext()) Finish(true, "Three real layouts tested with reduced runtime MOB totals; HUB restored. No build.");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }
        private static void Check(bool ok, string text)
        { if (!ok) throw new Exception(text); results.Add("PASS " + text); }
        private static void Finish(bool ok, string detail)
        {
            checks = null; SessionState.SetBool(Key, false);
            File.WriteAllText(SessionState.GetString(Key + "Report", "TOP-DOWN-levels.txt"),
                $"{(ok ? "PASS" : "FAIL")} | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" + string.Join("\n", results) + "\n" + detail);
            if (!ok) EditorApplication.isPlaying = false;
        }
        private static IEnumerator Checks()
        {
            yield return null;
            var hub = UnityEngine.Object.FindFirstObjectByType<HubPrototype>();
            var camera = Camera.main;
            bool originalProjection = camera.orthographic;
            var originalPosition = camera.transform.position;
            hub.Title.StartGame();
            hub.Selection.OpenPreparation(); hub.Selection.OpenBanner(0);
            hub.Selection.SelectPlayer(0); hub.Selection.SelectOption(false, 0); hub.Selection.SelectOption(true, 0); hub.Selection.ConfirmSelection();
            var original = hub.Definition;
            var fixture = UnityEngine.Object.Instantiate(original);
            fixture.AreaTotals = new[] { 10, 10, 10 };
            hub.Definition = fixture;
            Check(hub.BeginRun(), "HUB starts RUN: " + hub.Failure);
            var run = hub.Run;
            for (int area = 0; area < 3; area++)
            {
                while (run.Loading) yield return null;
                run.Experience.DropPercent = 0; // Keep this geometry fixture free from EXP choice interruptions.
                Check(run.State == LoopState.Combat && run.AreaIndex == area, "AREA " + (area + 1) + " loaded with FIRST SPAWN");
                Check(run.Navigation.Ready && run.Navigation.Reachable(run.Settings.StartPosition, run.Definition.ExitForArea(area)), "Navigation connects SPAWN and EXIT");
                Check(!camera.orthographic && camera.transform.forward == Vector3.forward && Mathf.Abs(SpawnManager.GroundHalfHeight(camera) - run.Settings.CameraSize) < .001f, "Perspective preserves ground coverage and spawn extent");
                Check(run.Definition.EnvironmentShader.isSupported && !ShaderUtil.ShaderHasError(run.Definition.EnvironmentShader), "Environment shader supported");
                var obstacles = UnityEngine.Object.FindObjectsByType<TestObstacle>(FindObjectsSortMode.None);
                Check(obstacles.Length == run.Settings.Obstacles.Length + 4, "Original obstacle count preserved");
                bool geometry = true;
                foreach (var expected in run.Settings.Obstacles)
                {
                    bool found = false;
                    foreach (var actual in obstacles)
                    {
                        var box = actual.GetComponent<BoxCollider2D>();
                        if (Vector2.Distance(actual.transform.position, expected.Position) < .001f && box.size == expected.Size &&
                            Mathf.Abs(Mathf.DeltaAngle(actual.transform.eulerAngles.z, expected.Rotation)) < .001f && actual.IsWall == expected.Wall)
                            found = true;
                    }
                    geometry &= found;
                }
                Check(geometry, "All footprint positions, sizes, rotations and wall rules preserved");
                Check(UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length == 0, "Visual volumes add no 3D collisions");
                Check(GameObject.Find("Palazzo") != null && GameObject.Find("Ostacolo rosso") != null && GameObject.Find("Griglia pavimento 1 m x 1 m") != null, "Buildings, red obstacles and metric grid present");
                float lowest = float.MaxValue, highest = 0;
                foreach (var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                    if (renderer.name == "Palazzo")
                    {
                        lowest = Mathf.Min(lowest, renderer.transform.localScale.z);
                        highest = Mathf.Max(highest, renderer.transform.localScale.z);
                        Check(renderer.sharedMaterial.GetColor("_BaseColor") == new Color(.42f, .4f, .34f), "Building uses study facade palette");
                    }
                Check(Mathf.Approximately(lowest, 5) && Mathf.Approximately(highest, 10), "Building heights span 5 to 10 metres");
                var screen = camera.WorldToScreenPoint(run.Player.transform.position + Vector3.up * 3);
                var ray = camera.ScreenPointToRay(screen);
                new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out float distance);
                Check(Vector3.Distance(ray.GetPoint(distance), run.Player.transform.position + Vector3.up * 3) < .001f, "Cursor ground projection remains accurate");
                if (SessionState.GetBool(Key + "Visual", false))
                {
                    Time.timeScale = 0;
                    double until = EditorApplication.timeSinceStartup + 20;
                    while (EditorApplication.timeSinceStartup < until) yield return null;
                    Time.timeScale = 1;
                }
                while (run.State == LoopState.Combat)
                {
                    foreach (var actor in Combatant.All.ToArray()) if (actor.Faction == Faction.MOB && actor.IsActive) actor.Hit(100000);
                    yield return null;
                }
                Check(run.State == LoopState.AreaComplete, "AREA completes normally");
                run.Player.transform.position = run.Exit.transform.position;
                while (run.State == LoopState.AreaComplete) yield return null;
                Check(run.State == LoopState.Bonus, "Exit opens bonus selection");
                run.SelectBonus(0); run.ConfirmBonus();
            }
            Check(run.State == LoopState.Finished, "Three AREA sequence finishes");
            hub.ReturnFromRun();
            yield return null; yield return null;
            Check(hub.Run == null && camera.orthographic == originalProjection && camera.transform.position == originalPosition, "Return to HUB restores camera");
            hub.Definition = original; UnityEngine.Object.Destroy(fixture);
        }
    }
}
