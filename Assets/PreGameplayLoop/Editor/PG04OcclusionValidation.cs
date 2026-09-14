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
    public static class PG04OcclusionValidation
    {
        private const string Key = "ROG.PG04.Occlusion.";
        private static readonly string Request = Path.GetFullPath(".pg04-occlusion-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG04OcclusionValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG04 base occlusion quick test")]
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
                if (!routine.MoveNext()) Finish(true, "PG04 only: base attack occlusion, delay, damage, visual mesh and preserved sector effects. Input feel/balancing are not tested.");
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            results.Add("PASS " + message);
        }

        private static void Near(float actual, float expected, string message, float tolerance = .001f)
            => Check(Mathf.Abs(actual - expected) <= tolerance, message + $" ({actual:0.###}/{expected:0.###})");

        private static void Finish(bool passed, string detail)
        {
            routine = null;
            SessionState.SetBool(Key + "Running", false);
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG04-occlusion-validation.txt"));
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
            Check(loop.Player.Definition.PlayerId == "PG04", "Only PG04 instantiated");
            var original = loop.Definition;
            var definition = UnityEngine.Object.Instantiate(original);
            loop.Definition = definition;
            try
            {
                definition.SelectedPG04Ability = PG04Ability.ColpoGrosso;
                definition.SelectedPG04Passive = PG04Passive.ScortaEsplosiva;
                loop.RestartTest();
                while (loop.State != LoopState.Combat) yield return null;
                foreach (var obstacle in TestObstacle.All.ToArray()) obstacle.gameObject.SetActive(false);
                foreach (var mob in Combatant.All.ToArray()) if (mob.Faction == Faction.MOB) mob.gameObject.SetActive(false);
                loop.RefreshNavigation();
                loop.BonusAbilities.enabled = false;
                Vector2 center = new Vector2(0, 0);
                loop.Player.transform.position = new Vector2(-3, 0);
                var front = Probe(new Vector2(.3f, .3f));
                var behind = Probe(new Vector2(.85f, .85f));
                var side = Probe(new Vector2(.15f, 1));
                var outside = Probe(new Vector2(0, 1.3f));
                var edge = Probe(new Vector2(-1.25f, 0), Faction.MOB, 150);
                var offset = Probe(new Vector2(4, 4));
                offset.GetComponent<CircleCollider2D>().offset = new Vector2(-4.3f, -4.3f);
                var block = Obstacle(new Vector2(.65f, .65f), new Vector2(.2f, .6f), false);
                Physics2D.SyncTransforms();
                int baseHits = 0;
                loop.Player.Actor.BaseHitLanded += (_, __) => baseHits++;
                var weapon = loop.Player.GetComponent<PlayerWeapon>();
                int explosions = 0; float impactTime = 0;
                loop.PG04Ability.Explosion += (_, __, ___) => { explosions++; impactTime = Time.time; };
                float launched = Time.time;
                Check(weapon.TryFireAt(center), "Actual PG04 base shot launched");
                Near(front.CurrentHP, 500, "No immediate HIT");
                while (explosions == 0) yield return null;
                Check(impactTime - launched >= .28f, "HIT retains 0.3 second delay (frame tolerance)");
                Near(front.CurrentHP, 475, "MOB before obstacle takes full damage in intercepted quadrant");
                Near(side.CurrentHP, 475, "Exposed MOB beside obstacle takes full damage in same quadrant");
                Near(behind.CurrentHP, 500, "MOB behind obstacle takes no damage");
                Near(outside.CurrentHP, 500, "MOB beyond 1.25m takes no damage");
                Near(edge.CurrentHP, 487, "Radius boundary included and DEF applied");
                Near(offset.CurrentHP, 475, "Collider center determines range and visibility");
                Near(baseHits, 4, "One base HIT event per valid MOB");
                var flash = GameObject.Find("PG04 visible base explosion");
                Check(flash != null, "Occluded visual emitted at actual impact");
                var mesh = flash.GetComponent<MeshFilter>().sharedMesh;
                bool clipped = false, full = false, meshValid = true;
                foreach (var v in mesh.vertices)
                {
                    if (v.sqrMagnitude < .000001f) continue;
                    float visible = PG04ExplosionEffect.VisibleDistance(center, ((Vector2)v).normalized, 1.25f);
                    meshValid &= v.magnitude <= visible + .0001f;
                    clipped |= v.magnitude < 1.2f; full |= v.magnitude > 1.24f;
                }
                Check(meshValid && clipped && full, "Visual preserves exposed area and clips shadow");
                foreach (bool wall in new[] { false, true })
                {
                    block.GetComponent<TestObstacle>().IsWall = wall;
                    block.layer = LayerMask.NameToLayer(wall ? "MURO" : "OSTACOLO");
                    block.transform.rotation = Quaternion.Euler(0, 0, 35);
                    Physics2D.SyncTransforms();
                    Near(PG04ExplosionEffect.HitBase(loop.Player.Actor, center, 1.25f, 25), 4, "Rotated blocker accepts only exposed MOB: wall=" + wall);
                    Near(behind.CurrentHP, 500, "Rotated blocker still protects behind MOB");
                }
                block.transform.SetPositionAndRotation(new Vector2(.5f, 0), Quaternion.identity);
                block.GetComponent<BoxCollider2D>().size = new Vector2(1, 2);
                Physics2D.SyncTransforms();
                Near(PG04ExplosionEffect.VisibleDistance(center, Vector2.left, 1.25f), 1.25f, "Wall impact leaves outward half unobstructed");
                Near(PG04ExplosionEffect.VisibleDistance(center, Vector2.right, 1.25f), 0, "Wall impact blocks inward half");
                block.transform.SetPositionAndRotation(new Vector2(.65f, .65f), Quaternion.identity);
                block.GetComponent<BoxCollider2D>().size = new Vector2(.2f, .6f);
                Physics2D.SyncTransforms();
                var sectors = PG04ExplosionEffect.Sectors(center, 1.25f);
                Check(!sectors[0], "Other PG04 effects retain entire intercepted sector exclusion");
                float before = front.CurrentHP;
                PG04ExplosionEffect.Hit(loop.Player.Actor, center, 1.25f, sectors, 25);
                Near(front.CurrentHP, before, "Legacy sector explosion still excludes front MOB");
                while (loop.PG04Ability.CooldownRemaining > 0) yield return null;
                Check(loop.PG04Ability.TryActivate(center), "COLPO GROSSO activated");
                before = front.CurrentHP; explosions = 0;
                while (!weapon.TryFireAt(center)) yield return null;
                while (explosions == 0) yield return null;
                Near(front.CurrentHP, before, "Actual COLPO GROSSO preserves sector rule");
                Near(loop.PG04Ability.Charges, 3, "COLPO GROSSO consumes one charge");
                Check(GameObject.Find("PG04 explosion") != null, "COLPO GROSSO keeps sector visual");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(definition); }
        }
    }
}
