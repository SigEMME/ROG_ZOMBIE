using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    [InitializeOnLoad]
    public static class PauseScreenValidation
    {
        private const string Key = "ROG.PauseTest";
        private static IEnumerator checks;
        private static readonly List<string> results = new List<string>();
        private static double deadline;
        private static int warnings;
        static PauseScreenValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
                { results.Clear(); warnings = 0; deadline = EditorApplication.timeSinceStartup + 90; checks = Checks(); }
            };
            Application.logMessageReceived += (message, stack, type) =>
            {
                if (checks == null) return;
                if (type == LogType.Warning) warnings++;
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Finish(false, message + stack);
            };
        }
        private static void Tick()
        {
            string request = Path.GetFullPath(".pause-screen-test.request");
            if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(request))
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) return;
                SessionState.SetString(Key + "Report", File.ReadAllText(request).Trim()); File.Delete(request);
                EditorSceneManager.OpenScene("Assets/Scenes/HubPrototype.unity");
                SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
            }
            if (checks == null) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timeout");
                if (!checks.MoveNext()) Finish(true, "Play Mode left paused for visual inspection. Standalone exit and multiplayer not tested.");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }
        private static void Check(bool ok, string message)
        { if (!ok) throw new Exception(message); results.Add("PASS " + message); }
        private static void Finish(bool ok, string detail)
        {
            checks = null; SessionState.SetBool(Key, false);
            File.WriteAllText(SessionState.GetString(Key + "Report", "PAUSE-validation.txt"), (ok ? "PASS" : "FAIL") +
                $" | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" + string.Join("\n", results) + "\n" + detail);
            if (!ok) EditorApplication.isPlaying = false;
        }
        private static IEnumerator Checks()
        {
            yield return null;
            var hub = UnityEngine.Object.FindFirstObjectByType<HubPrototype>();
            var pause = hub.Pause;
            Check(pause != null && !pause.IsOpen, "Menu initially closed");
            Check(pause.ResumeImage != null && pause.OptionsImage != null && pause.ReturnImage != null && pause.ExitImage != null, "Four supplied images assigned");
            pause.Open(); Check(!pause.IsOpen, "No pause on title");
            hub.Title.StartGame();
            pause.Back(); Check(pause.IsHubPause && pause.IsOpen && Time.timeScale == 0, "ESC route opens HUB pause");
            var hubPosition = hub.HubPosition;
            double hubUntil = EditorApplication.timeSinceStartup + .2;
            while (EditorApplication.timeSinceStartup < hubUntil) yield return null;
            Check(pause.IsOpen && hub.HubPosition == hubPosition, "HUB pause persists without RUN");
            pause.RequestReturn(); Check(pause.Page == PausePage.Menu, "HUB has no return-to-HUB action");
            pause.ShowOptions(); Check(pause.Page == PausePage.Options && Time.timeScale == 0, "HUB options stays paused");
            pause.Back(); pause.RequestExit(); Check(pause.Page == PausePage.ExitConfirmation, "HUB exit requires confirmation");
            pause.Back(); pause.Back(); Check(!pause.IsOpen && Time.timeScale == 1 && hub.Run == null, "HUB resume restores state without creating RUN");
            hub.Selection.OpenPreparation(); hub.Selection.OpenBanner(0);
            hub.Selection.SelectPlayer(0); hub.Selection.SelectOption(false, 0); hub.Selection.SelectOption(true, 0); hub.Selection.ConfirmSelection();
            Check(hub.BeginRun(), "RUN starts");
            while (hub.Run.Loading) yield return null;
            var run = hub.Run;
            Check(run.GameplayRunning, "Real RUN is running");
            run.AddGold(123);
            pause.Back(); Check(pause.Page == PausePage.Menu && Time.timeScale == 0 && !run.GameplayRunning, "ESC route pauses RUN");
            float hp = run.Player.Actor.CurrentHP, gameTime = Time.time;
            var position = run.Player.transform.position;
            double until = EditorApplication.timeSinceStartup + .3;
            while (EditorApplication.timeSinceStartup < until) yield return null;
            Check(Time.time == gameTime && run.Player.transform.position == position && run.Player.Actor.CurrentHP == hp, "Gameplay time, movement and HP frozen");
            pause.ShowOptions(); Check(pause.Page == PausePage.Options && Time.timeScale == 0, "Options retains pause");
            pause.Back(); Check(pause.Page == PausePage.Menu && Time.timeScale == 0, "ESC options returns to pause");
            pause.RequestReturn(); Check(pause.Page == PausePage.ReturnConfirmation && hub.Run == run && run.Gold == 123, "Return requires confirmation");
            pause.Back(); Check(pause.Page == PausePage.Menu && run.Gold == 123, "Cancel preserves RUN and gold");
            pause.RequestExit(); Check(pause.Page == PausePage.ExitConfirmation && Time.timeScale == 0, "Exit requires confirmation and retains pause");
            pause.Back(); pause.Resume(); Check(!pause.IsOpen && run.GameplayRunning, "Resume restores gameplay");
            for (int i = 0; i < 3; i++) { pause.Open(); pause.Open(); pause.Back(); Check(Time.timeScale == 1, "Repeated pause does not overwrite saved speed " + i); }
            Time.timeScale = 0; pause.Open(); pause.Resume(); Check(Time.timeScale == 0, "Existing BONUS pause is preserved"); Time.timeScale = 1;
            pause.Open(); pause.RequestReturn(); pause.Confirm();
            Check(run.Gold == 0 && hub.Run == null && !pause.IsOpen, "Confirmed abandonment clears RUN gold and removes session");
            while (hub.Selection.Page != PreparationPage.Hub) yield return null;
            Check(Time.timeScale == 1 && hub.Selection.GetMember(0).Confirmed, "HUB restored and preparation retained");
            hub.Selection.OpenPreparation(); Check(hub.BeginRun(), "New RUN starts after abandonment");
            while (hub.Run.Loading) yield return null;
            Check(hub.Run.Gold == 0 && hub.Run.AreaIndex == 0, "New RUN has no previous progress");
            pause.Open();
        }
    }
}
