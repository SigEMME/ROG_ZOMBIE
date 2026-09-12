using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    // Opt-in engine smoke test: no gameplay cheats are added to the scene.
    [InitializeOnLoad]
    public static class LoopValidation
    {
        private const string Key = "ROG.PreLoop.";
        private const string ScenePath = "Assets/Scenes/PreGameplayLoopPrototype.unity";
        private static readonly string Request = Path.GetFullPath(".pre-gameplay-loop-test.request");
        private static int step, assertions;
        private static double started, pausedAt;
        private static float hpAfterBonus, atkAfterBonus, pausedTime;
        private static Vector3 pausedPosition;
        private static LoopSession loop;

        static LoopValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayState;
            Application.logMessageReceived += OnLog;
        }

        private static void Check(bool value, string message)
        {
            assertions++;
            if (!value) throw new InvalidOperationException(message);
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run engine smoke test")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            string output = SessionState.GetString(Key + "Report", "");
            if (string.IsNullOrEmpty(output))
                SessionState.SetString(Key + "Report", Path.GetFullPath("PreGameplayLoop-validation.txt"));
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Salvare la scena e lasciare una sola scena aperta prima del test."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene(ScenePath);
            SessionState.SetBool(Key + "Running", true);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            { step = 0; assertions = 0; started = EditorApplication.timeSinceStartup; loop = null; }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(Key + "Running", false)) Finish(false, "Test interrotto prima del completamento.");
                string previous = SessionState.GetString(Key + "Previous", "");
                SessionState.EraseString(Key + "Previous");
                if (!string.IsNullOrEmpty(previous)) EditorApplication.delayCall += () => EditorSceneManager.OpenScene(previous);
            }
        }

        private static void OnLog(string message, string stack, LogType type)
        {
            if (SessionState.GetBool(Key + "Running", false) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                Finish(false, message + "\n" + stack);
        }

        private static void Finish(bool passed, string detail)
        {
            SessionState.SetBool(Key + "Running", false);
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PreGameplayLoop-validation.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report, (passed ? "PASS" : "FAIL") + " | assertions=" + assertions + " | Unity=" + Application.unityVersion + "\n" + detail);
            SessionState.EraseString(Key + "Report");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && File.Exists(Request))
            {
                SessionState.SetString(Key + "Report", File.ReadAllText(Request).Trim());
                File.Delete(Request);
                Run();
            }
            if (!SessionState.GetBool(Key + "Running", false) || !EditorApplication.isPlaying || EditorApplication.isPaused) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 180) throw new TimeoutException("Engine smoke test timeout, step=" + step);
                if (loop == null) loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>();
                if (loop == null) return;
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop.State == LoopState.Error) throw new InvalidOperationException(loop.Failure);
                switch (step)
                {
                    case 0:
                        if (loop.State != LoopState.Combat) return;
                        Check(loop.Spawns.TotalSpawned == 30 && loop.Spawns.Alive == 30, "FIRST SPAWN A1 = 30");
                        Check(Combatant.All.FindAll(a => a.Faction == Faction.PG).Count == 1, "Exactly one PG");
                        Check(UnityEngine.Object.FindObjectsByType<AreaPickup>(FindObjectsSortMode.None).Length == 0, "No CHEST/MEDI KIT");
                        Check(loop.Player.GetComponent<ExperienceProgression>() == null, "No out-of-scope level-up pauses");
                        foreach (var actor in Combatant.All)
                        {
                            if (actor.Faction != Faction.MOB) continue;
                            Vector3 view = loop.GameCamera.WorldToViewportPoint(actor.transform.position);
                            Check(view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1, "FIRST SPAWN off-screen");
                            Check(loop.Navigation.Reachable(actor.transform.position, loop.Player.transform.position), "Reachable spawn");
                        }
                        var mob = Combatant.All.Find(a => a.Faction == Faction.MOB && a.IsActive);
                        mob.transform.position = loop.Player.transform.position + Vector3.right * 2;
                        Check(loop.Player.GetComponent<PlayerWeapon>().TryFireAt(mob.transform.position), "PG01 fires existing weapon");
                        Check(Mathf.Approximately(mob.CurrentHP, 40), "PG01 cone causes 20 damage to ZOMB01");
                        mob.Hit(10000);
                        mob.Die();
                        Check(loop.Spawns.Killed == 1 && !mob.GetComponent<CircleCollider2D>().enabled, "One effective death, collider disabled");
                        step = 1;
                        break;
                    case 1:
                        if (loop.Spawns.TotalSpawned != 31) return;
                        Check(loop.Spawns.Alive == 30 && loop.Gold == 10, "One death -> one replacement and one G drop");
                        step = 2;
                        break;
                    case 2:
                    case 7:
                        Check(loop.Spawns.Alive <= loop.Spawns.MaxSimultaneous, "Concurrent cap respected");
                        foreach (var actor in Combatant.All.ToArray())
                            if (actor.Faction == Faction.MOB && actor.IsActive) actor.Hit(10000);
                        if (loop.State != LoopState.AreaComplete) return;
                        Check(loop.Spawns.Killed == loop.Settings.TotalMobs && loop.Spawns.Alive == 0, "Exact completion totals");
                        Check(loop.Exit.Available && loop.Exit.GetComponent<CircleCollider2D>().isTrigger && loop.Exit.gameObject.layer == LayerMask.NameToLayer("TRIGGER_PG"), "Logical exit on TRIGGER_PG");
                        loop.Player.transform.position = loop.Exit.transform.position + Vector3.right * 4.1f;
                        step = step == 2 ? 3 : 8;
                        break;
                    case 3:
                    case 8:
                        Check(loop.State == LoopState.AreaComplete && !loop.Exit.ContainsActivePlayer(), "Outside trigger cannot advance");
                        loop.Player.transform.position = loop.Exit.transform.position + Vector3.right * 4;
                        Check(loop.Exit.ContainsActivePlayer(), "4m boundary included");
                        step = step == 3 ? 4 : 9;
                        break;
                    case 4:
                        if (loop.State != LoopState.Bonus) return;
                        Check(Time.timeScale == 0 && loop.Choices.Length == 3, "Bonus opens and pauses");
                        loop.ConfirmBonus();
                        Check(loop.State == LoopState.Bonus, "Cannot confirm without selection");
                        pausedPosition = loop.Player.transform.position;
                        pausedTime = Time.time;
                        pausedAt = EditorApplication.timeSinceStartup;
                        step = 5;
                        break;
                    case 5:
                        if (EditorApplication.timeSinceStartup - pausedAt < .3) return;
                        Check(loop.Player.transform.position == pausedPosition && Time.time == pausedTime, "Gameplay frozen during choice");
                        loop.Player.Actor.Hit(20); // Simulate an injured survivor for transition healing.
                        loop.SelectBonus(0);
                        loop.ConfirmBonus();
                        hpAfterBonus = loop.Player.Actor.CurrentHP;
                        atkAfterBonus = loop.Player.Actor.Stats.ATK;
                        loop.ConfirmBonus();
                        Check(loop.State == LoopState.Transition, "Double confirmation guarded");
                        step = 6;
                        break;
                    case 6:
                        if (loop.State != LoopState.Combat || loop.AreaIndex != 1) return;
                        Check(loop.Spawns.TotalSpawned == 36 && loop.Spawns.Alive == 36, "FIRST SPAWN A2 = 36");
                        Check(loop.Player.Actor.Stats.ATK == atkAfterBonus, "Runtime bonus persists");
                        Check(Mathf.Approximately(loop.Player.Actor.CurrentHP, Mathf.Min(loop.Player.Actor.Stats.HP, hpAfterBonus + loop.Player.Actor.Stats.HP * .15f)), "Transition heals 15% current max HP");
                        Check(UnityEngine.Object.FindObjectsByType<TestNavigation>(FindObjectsSortMode.None).Length == 1, "Old navigation disposed");
                        Check(loop.Player.transform.position == (Vector3)loop.Settings.StartPosition, "New entry position");
                        step = 7;
                        break;
                    case 9:
                        if (loop.State != LoopState.Bonus) return;
                        loop.SelectBonus(0);
                        loop.ConfirmBonus();
                        Check(loop.State == LoopState.Finished && loop.Gold == 2200, "Two cycles complete, exact G, no third AREA");
                        var stats = loop.Player.Actor.Stats;
                        float cd = 100;
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.CdReduction, ref cd);
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.CdReduction, ref cd);
                        Check(cd == 90, "CD REDUCTION rounded 100 -> 95 -> 90");
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.ATK, ref cd);
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.ATK, ref cd);
                        Check(Mathf.Abs(loop.Player.Actor.Stats.ATK - stats.ATK * 1.05f * 1.05f) < .001, "Repeated bonuses use current value");
                        loop.RestartTest();
                        step = 10;
                        break;
                    case 10:
                        if (loop.State != LoopState.Combat) return;
                        Check(loop.AreaIndex == 0 && loop.Gold == 0 && loop.Player.Actor.CurrentHP == 100, "Restart creates clean RUN");
                        loop.Player.Actor.Hit(10000);
                        Check(!loop.Exit.ContainsActivePlayer(), "DOWN cannot enter exit");
                        step = 11;
                        break;
                    case 11:
                        if (loop.State != LoopState.Defeat) return;
                        Check(Time.timeScale == 0, "Single PG DOWN ends slice");
                        Finish(true, "Engine Play Mode: 100 + 120 ZOMB01; first spawn; replacement; actual PG01 cone hit; death; off-screen/NavMesh; exit; bonus pause/confirmation; persistent stats; 15% heal; second cycle; restart; defeat. Mob AI disabled by test harness while draining populations; manual feel/visual QA still required.");
                        break;
                }
            }
            catch (Exception exception) { Finish(false, "step=" + step + "\n" + exception); }
        }
    }
}
