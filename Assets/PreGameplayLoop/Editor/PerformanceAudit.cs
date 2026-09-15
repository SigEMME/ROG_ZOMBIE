using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Profiling;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    // Editor-only diagnostics. Fixtures and temporary settings are discarded on leaving Play Mode.
    [InitializeOnLoad]
    public static class PerformanceAudit
    {
        private const string Key = "ROG.PerformanceAudit.";
        private static IEnumerator routine;
        private static readonly List<string> lines = new List<string>();
        private static int lastFrame = -1, warnings;
        private static bool allocationCounterWorks;
        private static double began;
        private static LoopSession loop;
        private static readonly List<Combatant> mobs = new List<Combatant>();
        static PerformanceAudit()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += State;
            Application.logMessageReceived += Log;
        }
        private static void Update()
        {
            const string request = ".performance-audit.request";
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !EditorApplication.isUpdating && File.Exists(request))
            {
                if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty)
                { Debug.LogWarning("Save the scene before performance audit."); return; }
                SessionState.SetString(Key + "Report", File.ReadAllText(request).Trim()); File.Delete(request);
                SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
                EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
                SessionState.SetBool(Key + "Running", true); EditorApplication.isPlaying = true;
            }
            if (routine == null || !EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            try
            {
                if (EditorApplication.timeSinceStartup - began > 300) throw new TimeoutException("Audit timeout");
                if (!routine.MoveNext()) Finish("COMPLETE");
            }
            catch (Exception ex) { Finish("FAIL " + ex); }
        }
        private static void State(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            {
                lines.Clear(); mobs.Clear(); warnings = 0; lastFrame = -1;
                began = EditorApplication.timeSinceStartup; Application.runInBackground = true;
                lines.Add($"Unity {Application.unityVersion} | version {Application.version} | Editor Play Mode, not standalone FPS");
                lines.Add($"CPU {SystemInfo.processorType} | logical cores {SystemInfo.processorCount} | RAM {SystemInfo.systemMemorySize} MB | GPU {SystemInfo.graphicsDeviceName}");
                lines.Add($"Resolution {Screen.width}x{Screen.height} | VSync {QualitySettings.vSyncCount} | targetFPS {Application.targetFrameRate}");
                routine = Flatten(Run());
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (routine != null) Finish("INTERRUPTED");
                string previous = SessionState.GetString(Key + "Previous", ""); SessionState.EraseString(Key + "Previous");
                if (!string.IsNullOrEmpty(previous)) EditorApplication.delayCall += () => EditorSceneManager.OpenScene(previous);
            }
        }
        private static void Log(string message, string stack, LogType type)
        {
            if (!SessionState.GetBool(Key + "Running", false)) return;
            if (type == LogType.Warning) warnings++;
            if (type == LogType.Error || type == LogType.Exception) Finish("FAIL " + message + "\n" + stack);
        }
        private static void Finish(string status)
        {
            (routine as IDisposable)?.Dispose(); routine = null;
            SessionState.SetBool(Key + "Running", false);
            lines.Add(status + " | warnings=" + warnings);
            File.WriteAllLines(SessionState.GetString(Key + "Report", "PERFORMANCE-audit.txt"), lines);
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }
        private static void Note(string value)
        {
            lines.Add(value); File.WriteAllText("Builds/Performance-progress.txt", value);
        }
        private static IEnumerator Flatten(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            try
            {
                while (stack.Count > 0)
                {
                    var top = stack.Peek();
                    if (!top.MoveNext()) { (top as IDisposable)?.Dispose(); stack.Pop(); continue; }
                    if (top.Current is IEnumerator nested) stack.Push(nested); else yield return null;
                }
            }
            finally { foreach (var item in stack) (item as IDisposable)?.Dispose(); }
        }
        private static IEnumerator Wait(int frames) { for (int i = 0; i < frames; i++) yield return null; }
        private static IEnumerator Run()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            var definition = UnityEngine.Object.Instantiate(loop.Definition);
            definition.SelectedPlayer = LoopPlayer.PG01; definition.EnableCompanion = true;
            definition.CompanionPlayer = LoopPlayer.PG02; definition.CompanionAbility = 0; definition.CompanionPassive = 0;
            loop.Definition = definition; loop.RestartTest();
            while (loop.State != LoopState.Combat) yield return null;
            loop.Spawns.StopAllCoroutines(); loop.Spawns.enabled = false;
            foreach (var actor in Combatant.All.ToArray()) if (actor.Faction == Faction.MOB) actor.gameObject.SetActive(false);
            foreach (var context in new[] { loop, loop.Companion })
            {
                foreach (var component in context.Player.GetComponents<MonoBehaviour>())
                    if (component is PlayerMovement || component is PlayerAim || component is PlayerWeapon || component is CompanionFormation || component.GetType().Name.EndsWith("AbilityInput")) component.enabled = false;
                var stats = context.Player.Actor.Stats; stats.HP = 1000000; context.Player.Actor.SetStats(stats, 1000000);
                context.BonusAbilities.enabled = false;
            }
            long beforeCalibration = GC.GetAllocatedBytesForCurrentThread();
            var calibration = new byte[65536]; GC.KeepAlive(calibration);
            long calibrationBytes = GC.GetAllocatedBytesForCurrentThread() - beforeCalibration;
            allocationCounterWorks = calibrationBytes >= 65536;
            Note($"Allocation counter calibration: requested 65536 bytes, observed {calibrationBytes}; usable={allocationCounterWorks}. Zero per-call values from unsupported counters are NOT zero allocations.");
            Note("FRAME scenarios: city geometry, stationary high-HP PGs, companion formation/input disabled, autonomous ZOMB01 movement/attacks, no player firing; 30 warm-up + 180 measured frames.");
            yield return Frames("CITY_0_MOB");
            SpawnMobs(30); yield return Frames("CITY_30_MOB"); ClearMobs(); yield return Wait(3);
            SpawnMobs(100); yield return Frames("CITY_100_MOB_STRESS"); ClearMobs(); yield return Wait(3);

            var cityVisualObject = new GameObject("Audit city area renderer");
            var cityVisual = cityVisualObject.AddComponent<PG04AreaVisual>();
            Vector2 cityCenter = loop.Player.transform.position;
            Note("City visual fixture cover count within 4m: " + TestObstacle.All.Count(o => o.Bounds.SqrDistance(cityCenter) <= 16));
            yield return Bench("AREA_VISUAL_R4_CITY_SPAWN", () => cityVisual.InitializeOccluded(cityCenter, 4, Color.red));
            UnityEngine.Object.Destroy(cityVisualObject);
            foreach (var obstacle in TestObstacle.All.ToArray()) obstacle.gameObject.SetActive(false);
            loop.RefreshNavigation(); loop.Player.transform.position = Vector2.zero;
            var companion = loop.Companion.Player;
            var formation = companion.GetComponent<CompanionFormation>();
            companion.transform.position = Vector2.left; Physics2D.SyncTransforms();
            Note("MICRO scenarios: isolated calls, Stopwatch CPU wall time + current-thread managed allocation. Not full frame time. Warm-up 5, then 60 calls, one per frame.");
            yield return Bench("IA_RESOLVE_FREE", () => formation.TryResolvePosition(Vector2.zero, Vector2.left, out _));
            var wall = Block(new Vector2(-1, 0), new Vector2(.5f, .5f)); loop.RefreshNavigation();
            companion.transform.position = Vector2.up;
            yield return Bench("IA_RESOLVE_WALL", () => formation.TryResolvePosition(Vector2.zero, Vector2.left, out _));
            wall.SetActive(false); loop.RefreshNavigation();
            var crowd = new List<GameObject>();
            for (int n = 0; n < 16; n++)
            {
                float a = n * Mathf.PI / 8;
                var go = new GameObject("Audit crowd"); go.transform.position = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .8f;
                go.AddComponent<CircleCollider2D>().radius = .35f;
                go.AddComponent<Combatant>().Initialize(Faction.PG, new CombatStats { HP = 100, DEF = 100 }); crowd.Add(go);
            }
            companion.transform.position = Vector2.right * 3;
            yield return Bench("IA_RESOLVE_FULL_LOCAL_CROWD", () => formation.TryResolvePosition(Vector2.zero, Vector2.left, out _));
            foreach (var go in crowd) go.SetActive(false);
            companion.transform.position = Vector2.left;
            yield return Bench("IA_ADVANCE_ROTATING_CURSOR", () => formation.Advance(new Vector2(Mathf.Cos(Time.time * 2), Mathf.Sin(Time.time * 2)) * 10, 1f / 60));
            var visualObject = new GameObject("Audit area renderer"); var visual = visualObject.AddComponent<PG04AreaVisual>();
            yield return Bench("AREA_VISUAL_R4_NO_COVER", () => visual.InitializeOccluded(Vector2.zero, 4, Color.red));
            var fourCovers = new List<GameObject>();
            for (int n = 0; n < 4; n++) { float a = n * Mathf.PI / 2; fourCovers.Add(Block(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 3, new Vector2(.4f, .4f))); }
            yield return Bench("AREA_VISUAL_R4_4_COVERS", () => visual.InitializeOccluded(Vector2.zero, 4, Color.red));
            foreach (var cover in fourCovers) cover.SetActive(false);
            for (int n = 0; n < 12; n++) { float a = n * Mathf.PI / 6; Block(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 3, new Vector2(.4f, .4f)); }
            yield return Bench("AREA_VISUAL_R4_12_COVERS", () => visual.InitializeOccluded(Vector2.zero, 4, Color.red));
            yield return Bench("AREA_FLASH_R4_CREATE", () => TestVisuals.FlashOccludedArea(Vector2.zero, 4, Color.red));
            Note("No gameplay optimization, build export, or persistent character-stat change performed by this audit.");
        }
        private static GameObject Block(Vector2 position, Vector2 size)
        {
            var go = new GameObject("Audit wall"); go.transform.SetParent(TestVisuals.Root); go.transform.position = position;
            go.layer = LayerMask.NameToLayer("MURO"); go.AddComponent<BoxCollider2D>().size = size; go.AddComponent<TestObstacle>().IsWall = true;
            return go;
        }
        private static void SpawnMobs(int count)
        {
            Vector2 origin = loop.Player.transform.position;
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2 * Mathf.PI / count;
                if (loop.Navigation.Sample(origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (5 + i % 8), out var point, 4)) mobs.Add(loop.CreateMob(0, point));
            }
            Note("Spawned audit MOB: " + mobs.Count);
        }
        private static void ClearMobs() { foreach (var mob in mobs) { mob.gameObject.SetActive(false); UnityEngine.Object.Destroy(mob.gameObject); } mobs.Clear(); }
        private static string Stats(double[] values)
        {
            Array.Sort(values);
            return $"mean={values.Average():F3}, median={values[values.Length / 2]:F3}, p95={values[(int)(values.Length * .95)]:F3}, max={values[values.Length - 1]:F3}";
        }
        private static IEnumerator Bench(string label, Action action)
        {
            for (int i = 0; i < 5; i++) { action(); yield return null; }
            var milliseconds = new double[60]; var bytes = new double[60];
            for (int i = 0; i < milliseconds.Length; i++)
            {
                long allocated = GC.GetAllocatedBytesForCurrentThread(); long start = System.Diagnostics.Stopwatch.GetTimestamp();
                action();
                milliseconds[i] = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                bytes[i] = GC.GetAllocatedBytesForCurrentThread() - allocated;
                yield return null;
            }
            Note(label + " | ms/call " + Stats(milliseconds) + " | managed bytes/call " + (allocationCounterWorks ? Stats(bytes) : "UNAVAILABLE (calibration failed)"));
        }
        private static IEnumerator Frames(string label)
        {
            using (var main = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1))
            using (var gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1))
            using (var memory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1))
            {
                yield return Wait(30);
                var frame = new double[180]; var cpu = new double[180]; var bytes = new double[180];
                for (int i = 0; i < frame.Length; i++)
                {
                    yield return null;
                    frame[i] = Time.unscaledDeltaTime * 1000; cpu[i] = main.LastValue / 1000000.0; bytes[i] = gc.LastValue;
                }
                Note(label + " | observed frame ms " + Stats(frame));
                Note(label + $" | main-thread recorder valid={main.Valid} ms " + Stats(cpu));
                Note(label + $" | GC recorder valid={gc.Valid} bytes/frame " + Stats(bytes) + $" | memory valid={memory.Valid} usedMiB={memory.LastValue / 1048576.0:F1}");
            }
        }
    }
}
