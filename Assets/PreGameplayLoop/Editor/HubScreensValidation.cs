using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop.Editor
{
    [InitializeOnLoad]
    public static class HubScreensValidation
    {
        private const string Key = "ROG.HubScreens.";
        private static readonly List<string> checks = new List<string>();
        private static int warnings;
        private static double finishAt;
        static HubScreensValidation()
        { EditorApplication.update += Update; EditorApplication.playModeStateChanged += State; Application.logMessageReceived += Log; }
        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run HUB screen tests")]
        public static void Run()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty) { Debug.LogWarning("Save the current scene before HUB screen tests."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/HubPrototype.unity");
            SessionState.SetBool(Key + "Running", true); EditorApplication.isPlaying = true;
        }
        private static void State(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            {
                checks.Clear(); warnings = 0;
                try { CheckScreens(); finishAt = EditorApplication.timeSinceStartup + 1; }
                catch (Exception ex) { Finish(false, ex.ToString()); }
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                string previous = SessionState.GetString(Key + "Previous", ""); SessionState.EraseString(Key + "Previous");
                if (!string.IsNullOrEmpty(previous)) EditorApplication.delayCall += () => EditorSceneManager.OpenScene(previous);
            }
        }
        private static void Update()
        {
            string request = Path.GetFullPath(".hub-screens-test.request");
            if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(request))
            { SessionState.SetString(Key + "Report", File.ReadAllText(request).Trim()); File.Delete(request); Run(); }
            if (SessionState.GetBool(Key + "Running", false) && EditorApplication.isPlaying && finishAt > 0 && EditorApplication.timeSinceStartup >= finishAt)
                Finish(true, "Only preparation/selection screens tested. Combat, RUN loading, AREA transitions and balancing not executed. Play Mode left open for visual inspection.");
        }
        private static void Log(string message, string stack, LogType type)
        {
            if (!SessionState.GetBool(Key + "Running", false)) return;
            if (type == LogType.Warning) warnings++;
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) Finish(false, message + "\n" + stack);
        }
        private static void Check(bool value, string message)
        { if (!value) throw new InvalidOperationException(message); checks.Add("PASS " + message); }
        private static void Finish(bool pass, string detail)
        {
            finishAt = 0; SessionState.SetBool(Key + "Running", false);
            File.WriteAllText(SessionState.GetString(Key + "Report", Path.GetFullPath("HUB-screens-validation.txt")),
                (pass ? "PASS" : "FAIL") + $" | Unity {Application.unityVersion} | checks={checks.Count} | warnings={warnings}\n" + string.Join("\n", checks) + "\n" + detail);
            if (!pass) EditorApplication.isPlaying = false;
        }
        private static void CheckScreens()
        {
            var hub = UnityEngine.Object.FindFirstObjectByType<HubPrototype>();
            Check(hub != null && hub.Failure == null, "HUB scene loads with definition and content");
            Check(UnityEngine.Object.FindFirstObjectByType<LoopSession>() == null && Combatant.All.Count == 0, "No gameplay session or combatants started");
            var model = hub.Selection;
            Check(model.Page == PreparationPage.Hub, "Initial HUB page");
            model.OpenPreparation(); Check(!model.CanStart, "No start with empty selection");
            for (int i = 1; i < 4; i++) Check(!model.OpenBanner(i), "Banner blocked " + (i + 1));
            Check(model.OpenBanner(0), "First banner opens selection");
            Check(!model.ConfirmSelection(), "Cannot confirm empty selection");
            for (int pg = 0; pg < 8; pg++)
            {
                model.SelectPlayer(pg);
                Check(model.Player == pg && model.Ability == -1 && model.Passive == -1, "PG selectable and options reset " + pg);
                var data = hub.Text.Characters[pg];
                Check(data.Options.Length == 4 && RunPreparationSelection.Weapon(hub.Definition, pg) != null, "Data available for PG " + pg);
                model.SelectOption(false, 0); Check(!model.Complete && !model.ConfirmSelection(), "Ability alone insufficient " + pg);
                model.SelectOption(true, 1); Check(model.DescriptionIndex == 3, "Last passive description selected " + pg);
                model.SelectOption(false, 1); Check(model.DescriptionIndex == 1, "Last ability description selected " + pg);
                model.Back(); Check(model.Page == PreparationPage.Preparation && !model.Confirmed && !model.CanStart, "Back does not confirm " + pg);
                model.Back(); Check(model.Page == PreparationPage.Hub && model.Player == pg && model.Ability == 1 && model.Passive == 1, "Preparation Back preserves draft in HUB " + pg);
                model.OpenPreparation(); model.OpenBanner(0); Check(model.Ability == 1 && model.Passive == 1, "Reopen retains draft " + pg);
                Check(model.ConfirmSelection() && model.CanStart, "Explicit selection confirmation unlocks preparation " + pg);
                var snapshot = model.CreateRunDefinition(hub.Definition);
                Check(snapshot != null && snapshot != hub.Definition && (int)snapshot.SelectedPlayer == pg && snapshot.Validate() == null, "RUN configuration valid without starting gameplay " + pg);
                UnityEngine.Object.Destroy(snapshot);
                model.Back(); model.OpenPreparation(); Check(model.CanStart, "HUB preserves confirmed configuration " + pg);
                model.OpenBanner(0); Check(!model.Confirmed && model.Ability == 1 && model.Passive == 1, "Reopening clears confirmation only " + pg);
            }
            model.SelectOption(false, 0); model.SelectOption(true, 0);
            Check(hub.Run == null && UnityEngine.Object.FindFirstObjectByType<LoopSession>() == null, "Screen tests did not launch RUN");
        }
    }
}
