using System;
using System.Collections;
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
    public static class PG04PyromaniaValidation
    {
        private const string Key = "ROG.PG04.Pyromania.";
        private static readonly string Request = Path.GetFullPath(".pg04-pyromania-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG04PyromaniaValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG04 PYROMANIA regression test")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
            // Select PG04 in memory for this suite only, preserving the user's Inspector choice.
            var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
            SessionState.SetInt(Key + "PreviousPlayer", (int)config.SelectedPlayer);
            SessionState.SetBool(Key + "RestoreSelection", true);
            config.SelectedPlayer = LoopPlayer.PG04;
            SessionState.SetBool(Key + "Running", true);
            EditorApplication.isPlaying = true;
        }

        private static void PlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            {
                results.Clear(); warnings = 0; loop = null;
                started = EditorApplication.timeSinceStartup;
                Application.runInBackground = true;
                routine = Checks();
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(Key + "RestoreSelection", false))
                {
                    var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
                    config.SelectedPlayer = (LoopPlayer)SessionState.GetInt(Key + "PreviousPlayer", 0);
                    SessionState.EraseBool(Key + "RestoreSelection");
                    SessionState.EraseInt(Key + "PreviousPlayer");
                }
                if (SessionState.GetBool(Key + "Running", false)) Finish(false, "Interrupted before completion.");
                string previous = SessionState.GetString(Key + "Previous", "");
                SessionState.EraseString(Key + "Previous");
                if (!string.IsNullOrEmpty(previous)) EditorApplication.delayCall += () => EditorSceneManager.OpenScene(previous);
            }
        }

        private static void Log(string message, string stack, LogType type)
        {
            if (!SessionState.GetBool(Key + "Running", false)) return;
            if (type == LogType.Warning) warnings++;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Finish(false, message + "\n" + stack);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && File.Exists(Request))
            {
                if (!SessionState.GetBool(Key + "Refreshed", false))
                {
                    SessionState.SetBool(Key + "Refreshed", true);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    return;
                }
                SessionState.EraseBool(Key + "Refreshed");
                SessionState.SetString(Key + "Report", File.ReadAllText(Request).Trim());
                File.Delete(Request);
                Run();
            }
            if (!SessionState.GetBool(Key + "Running", false) || !EditorApplication.isPlaying || routine == null) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 90) throw new TimeoutException("Ability test timeout.");
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop != null && loop.Player != null)
                {
                    var input = loop.Player.GetComponent<PG04AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "PG04 PYROMANIA: actual base/charged/rain explosions, timed damage, cover, rendering, overlap and RUN reset.");
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            results.Add("PASS " + message);
            File.WriteAllText("Builds/PG04-pyro-progress.txt", message);
        }

        private static void Near(float actual, float expected, string message, float tolerance = .001f)
            => Check(Mathf.Abs(actual - expected) <= tolerance, message + $" ({actual:0.###}/{expected:0.###})");

        private static void Finish(bool passed, string detail)
        {
            routine = null;
            SessionState.SetBool(Key + "Running", false);
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG04-pyromania-validation.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report, (passed ? "PASS" : "FAIL") + $" | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" +
                string.Join("\n", results) + "\n" + detail);
            SessionState.EraseString(Key + "Report");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        private static Combatant Probe(Vector2 position, Faction faction = Faction.MOB, float defence = 100)
        {
            var go = new GameObject("Ability test actor");
            go.transform.SetParent(loop.transform);
            go.transform.position = position;
            go.layer = LayerMask.NameToLayer(faction == Faction.PG ? "PG" : "MOB");
            go.AddComponent<CircleCollider2D>().radius = .35f;
            var actor = go.AddComponent<Combatant>();
            actor.Initialize(faction, new CombatStats { HP = 500, DEF = defence, ATK = 20, MoveSpeed = 100, Range = 400 });
            return actor;
        }

        private static GameObject Obstacle(Vector2 position, Vector2 size, bool wall)
        {
            var go = new GameObject("Ability test blocker");
            go.transform.SetParent(loop.transform);
            go.transform.position = position;
            go.layer = LayerMask.NameToLayer(wall ? "MURO" : "OSTACOLO");
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = wall;
            return go;
        }

        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            var original = loop.Definition;
            var config = UnityEngine.Object.Instantiate(original); loop.Definition = config;
            try
            {
                config.SelectedPlayer = LoopPlayer.PG04;
                config.SelectedPG04Passive = PG04Passive.Pyromania;
                config.SelectedPG04Ability = PG04Ability.ColpoGrosso;
                loop.RestartTest();
                while (loop.State != LoopState.Combat) yield return null;
                Isolate();
                Check(loop.Player.Definition.PlayerId == "PG04" && loop.PG04Ability.Passive == PG04Passive.Pyromania, "PG04 and PYROMANIA selected in actual RUN");
                var target = Probe(new Vector2(.4f, .2f));
                var outside = Probe(new Vector2(0, 1.3f));
                var weapon = loop.Player.GetComponent<PlayerWeapon>();
                Check(weapon.TryFireAt(Vector2.zero), "Actual base shot launched");
                Near(target.CurrentHP, 500, "No damage before delayed explosion");
                PG04BurningArea fire = null;
                while ((fire = UnityEngine.Object.FindFirstObjectByType<PG04BurningArea>()) == null) yield return null;
                Near(target.CurrentHP, 470, "Base HIT 25 plus immediate PYROMANIA tick 5");
                Near(fire.Ticks, 1, "Immediate tick counted once");
                Near(outside.CurrentHP, 500, "Outside radius untouched");
                var filter = fire.GetComponent<MeshFilter>(); var renderer = fire.GetComponent<MeshRenderer>();
                Check(filter != null && renderer != null && filter.sharedMesh.vertexCount > 0, "Persistent fire has mesh and renderer");
                Check(renderer.sharedMaterial.mainTexture != null && renderer.sharedMaterial.color.a > 0, "Fire material has texture and opacity");
                Check(filter.sharedMesh.colors32.Length == filter.sharedMesh.vertexCount, "Explicit vertex colors supplied");
                while (fire != null && fire.Ticks < 2) yield return null;
                Near(target.CurrentHP, 465, "Second tick at one second");
                Check(fire != null && fire.GetComponent<MeshFilter>().sharedMesh != null, "Mesh survives first visual rebuild");
                UnityEngine.ScreenCapture.CaptureScreenshot("Builds/PG04-pyromania-visible.png");
                while (fire != null && fire.Ticks < 3) yield return null;
                Near(target.CurrentHP, 460, "Third tick at two seconds");
                Check(fire != null && fire.GetComponent<MeshRenderer>().sharedMaterial != null, "Material survives repeated rebuild");
                while (fire != null) yield return null;
                Near(target.CurrentHP, 460, "Expires at three seconds without extra tick");
                float end = Time.time + .2f; while (Time.time < end) yield return null;
                Near(target.CurrentHP, 460, "No damage after expiry");
                var blocked = Probe(new Vector2(.9f, 0));
                var front = Probe(new Vector2(.2f, 0));
                var block = Obstacle(new Vector2(.6f, 0), new Vector2(.2f, 1), true);
                fire = Fire(Vector2.zero);
                Near(blocked.CurrentHP, 500, "Wall shields immediate fire tick");
                Near(front.CurrentHP, 495, "MOB before wall receives immediate fire tick");
                block.SetActive(false);
                while (fire != null && fire.Ticks < 2) yield return null;
                Near(blocked.CurrentHP, 495, "Removing cover exposes MOB on following tick");
                var second = Fire(Vector2.zero);
                Near(front.CurrentHP, 485, "Overlapping area contributes independent immediate tick");
                UnityEngine.Object.Destroy(fire.gameObject); UnityEngine.Object.Destroy(second.gameObject);
                yield return null;
                while (loop.PG04Ability.CooldownRemaining > 0) yield return null;
                Check(loop.PG04Ability.TryActivate(Vector2.zero), "COLPO GROSSO activated");
                float hp = target.CurrentHP;
                while (!weapon.TryFireAt(Vector2.zero)) yield return null;
                while ((fire = UnityEngine.Object.FindFirstObjectByType<PG04BurningArea>()) == null) yield return null;
                Near(target.CurrentHP, hp - 50, "Charged explosion 45 plus immediate fire 5");
                Near(fire.Radius, 1.875f, "Charged PYROMANIA radius retained");
                config.SelectedPG04Ability = PG04Ability.PioggiaDiGranate;
                loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                Check(UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None).Length == 0, "RUN restart removes burning areas");
                Isolate();
                while (loop.PG04Ability.CooldownRemaining > 0) yield return null;
                int emitted = 0;
                loop.PG04Ability.Explosion += (center, radius, damage) =>
                {
                    emitted++;
                    var areas = UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None);
                    bool found = false;
                    foreach (var area in areas)
                        if (((Vector2)area.transform.position - center).sqrMagnitude < .00001f && area.Ticks == 1) found = true;
                    Check(found, "Rain explosion " + emitted + " creates immediate burning area");
                };
                Check(loop.PG04Ability.TryActivate(Vector2.zero), "PIOGGIA DI GRANATE activated");
                while (loop.PG04Ability.EffectActive) yield return null;
                Near(emitted, 10, "All ten rain explosions generate PYROMANIA");
                while (UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None).Length > 0) yield return null;
                Check(true, "All rain burning areas expire");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(config); }
        }
        private static void Isolate()
        {
            foreach (var obstacle in TestObstacle.All.ToArray()) obstacle.gameObject.SetActive(false);
            foreach (var actor in Combatant.All.ToArray()) if (actor.Faction == Faction.MOB) actor.gameObject.SetActive(false);
            loop.RefreshNavigation(); loop.BonusAbilities.enabled = false;
            loop.Player.transform.position = new Vector2(-3, 0);
        }
        private static PG04BurningArea Fire(Vector2 center)
        {
            var go = new GameObject("PYROMANIA regression area");
            go.transform.SetParent(TestVisuals.Root, false); go.transform.position = center;
            var area = go.AddComponent<PG04BurningArea>();
            area.Initialize(loop, loop.Player.Actor, 1.25f, 3, 5, null);
            return area;
        }
    }
}
