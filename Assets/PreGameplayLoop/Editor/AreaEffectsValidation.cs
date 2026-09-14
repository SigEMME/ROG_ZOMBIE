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
    public static class AreaEffectsValidation
    {
        private const string Key = "ROG.Areas.";
        private static readonly string Request = Path.GetFullPath(".area-effects-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static AreaEffectsValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run AREA effects regression tests")]
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
                routine = Flatten(Checks());
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
                if (EditorApplication.timeSinceStartup - started > 300) throw new TimeoutException("Ability test timeout.");
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop != null && loop.Player != null)
                {
                    foreach (var component in loop.Player.GetComponents<MonoBehaviour>())
                        if (component.GetType().Name.EndsWith("AbilityInput")) component.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "AREA effects: PG01/03/05/06/07/08, circular BONUS effects, MINE, ZOMB03/05 and renderer regression. PG04 PYROMANIA has its dedicated regression suite.");
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            results.Add("PASS " + message);
            File.WriteAllText("Builds/AREA-progress.txt", message);
        }

        private static void Near(float actual, float expected, string message, float tolerance = .001f)
            => Check(Mathf.Abs(actual - expected) <= tolerance, message + $" ({actual:0.###}/{expected:0.###})");

        private static void Finish(bool passed, string detail)
        {
            routine = null;
            SessionState.SetBool(Key + "Running", false);
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("AREA-effects-validation.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report, (passed ? "PASS" : "FAIL") + $" | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" +
                string.Join("\n", results) + "\n" + detail);
            SessionState.EraseString(Key + "Report");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        private static Combatant Probe(Vector2 position, Faction faction = Faction.MOB, float defence = 100)
        {
            var go = new GameObject("Ability test actor");
            go.transform.SetParent(TestVisuals.Root);
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
            go.transform.SetParent(TestVisuals.Root);
            go.transform.position = position;
            go.layer = LayerMask.NameToLayer(wall ? "MURO" : "OSTACOLO");
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = wall;
            return go;
        }

        private static LoopDefinition config;
        private const System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        private static object Call(object owner, string method, params object[] args)
            => owner.GetType().GetMethod(method, Private).Invoke(owner, args);
        private static void Set(object owner, string field, object value) => owner.GetType().GetField(field, Private).SetValue(owner, value);
        private static void Ready(object owner) => ((AbilityCooldown)owner.GetType().GetField("cooldown", Private).GetValue(owner)).Tick(1000);
        private static IEnumerator Flatten(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0)
            {
                var top = stack.Peek();
                if (!top.MoveNext()) { (top as IDisposable)?.Dispose(); stack.Pop(); continue; }
                if (top.Current is IEnumerator nested) stack.Push(nested); else yield return top.Current;
            }
        }
        private static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
        private static IEnumerator Start(LoopPlayer pg)
        {
            config.SelectedPlayer = pg; loop.RestartTest();
            while (loop.State != LoopState.Combat) yield return null;
            foreach (var o in TestObstacle.All.ToArray()) o.gameObject.SetActive(false);
            foreach (var a in Combatant.All.ToArray()) if (a.Faction == Faction.MOB) a.gameObject.SetActive(false);
            loop.RefreshNavigation(); loop.BonusAbilities.enabled = false;
            loop.Player.transform.position = Vector2.zero;
            Check(loop.Player.Definition.PlayerId == pg.ToString(), "RUN fixture " + pg);
        }
        private static void Visual(GameObject go, string label)
        {
            Check(go != null, label + " created");
            var filter = go.GetComponent<MeshFilter>(); var renderer = go.GetComponent<MeshRenderer>();
            Check(filter != null && renderer != null && filter.sharedMesh != null && renderer.sharedMaterial != null, label + " components valid");
            var mesh = filter.sharedMesh;
            Check(mesh.triangles.Length > 0 && mesh.colors32.Length == mesh.vertexCount && mesh.uv.Length == mesh.vertexCount, label + " mesh colors/UV/triangles supplied");
            Check(renderer.sharedMaterial.mainTexture != null && renderer.sharedMaterial.color.a > 0, label + " visible material");
        }
        private static void Acquire(string id)
        {
            int index = Array.FindIndex(loop.Bonuses.Catalog.Bonuses, b => b.Id == id);
            loop.Bonuses.Apply(new BonusChoice(index, -1));
            Check(loop.Bonuses.Find(id) != null, "Acquired " + id);
        }
        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            var original = loop.Definition; config = UnityEngine.Object.Instantiate(original); loop.Definition = config;
            try
            {
                config.SelectedAbility = PG01Ability.Pestone;
                yield return Start(LoopPlayer.PG01);
                var near = Probe(new Vector2(1, 0)); var covered = Probe(new Vector2(3, 0)); var rear = Probe(new Vector2(-1, 0));
                var block = Obstacle(new Vector2(2, 0), new Vector2(.2f, 1), true);
                Check(loop.Player.GetComponent<PlayerWeapon>().TryFireAt(new Vector2(4, 0)), "PG01 actual cone attack");
                Near(near.CurrentHP, 480, "PG01 cone full damage"); Near(covered.CurrentHP, 500, "PG01 cone wall shield"); Near(rear.CurrentHP, 500, "PG01 cone excludes rear");
                Ready(loop.Ability); Check(loop.Ability.TryPestone(new Vector2(7, 0)), "Actual PESTONE activation");
                Near(near.CurrentHP, 440, "PESTONE damage"); Near(near.MovementMetresPerSecond, 1.4f, "PESTONE 30 percent SLOW"); Near(covered.CurrentHP, 500, "PESTONE covered MOB untouched");
                Check(covered.PestoneSlowRemaining == 0, "PESTONE no SLOW through cover");
                yield return Wait(3.1f); Near(near.MovementMetresPerSecond, 2, "PESTONE SLOW expires");
                config.SelectedAbility = PG01Ability.Barriera;
                yield return Start(LoopPlayer.PG01); Ready(loop.Ability);
                Check(loop.Ability.BeginAim(new Vector2(3, 0)), "BARRIERA preview");
                Check(GameObject.Find("Anteprima BARRIERA").GetComponent<Collider2D>() == null, "BARRIERA preview has no collider");
                Check(loop.Ability.ReleaseAim(new Vector2(3, 0)), "BARRIERA release activation");
                Check(loop.Ability.Barriers.Count == 1, "BARRIERA exists");
                var barrier = loop.Ability.Barriers[0];
                Check(barrier.GetComponent<BoxCollider2D>().enabled && barrier.GetComponent<SpriteRenderer>() != null, "BARRIERA collider and visual");
                Near(AttackGeometry.VisibleDistance(Vector2.zero, Vector2.right, 6), 2.5f, "BARRIERA blocks circular area line");
                yield return Wait(5.1f); Check(!loop.Ability.EffectActive, "BARRIERA expires");
                config.SelectedPG03Ability = PG03Ability.ColpoLaser;
                yield return Start(LoopPlayer.PG03);
                near = Probe(new Vector2(1, 0)); covered = Probe(new Vector2(3, 0)); rear = Probe(new Vector2(-1, 0));
                block = Obstacle(new Vector2(2, 0), new Vector2(.2f, 3), true);
                LaserEffect.Cast(loop.Player.Actor, Vector2.zero, Vector2.right, config.PG03Abilities);
                Near(near.CurrentHP, 465, "LASER 35 damage"); Near(covered.CurrentHP, 465, "LASER retains cover exception"); Near(rear.CurrentHP, 500, "LASER excludes rear");
                Check(GameObject.Find("COLPO LASER").GetComponent<SpriteRenderer>() != null, "LASER visible rectangle");
                yield return Start(LoopPlayer.PG05);
                near = Probe(new Vector2(1, 0)); covered = Probe(new Vector2(2, 0)); rear = Probe(new Vector2(-1, 0));
                block = Obstacle(new Vector2(1.5f, 0), new Vector2(.2f, 1), false);
                Check(loop.Player.GetComponent<PlayerWeapon>().TryFireAt(new Vector2(2.5f, 0)), "PG05 actual area attack");
                Near(near.CurrentHP, 470, "PG05 cone damage"); Near(covered.CurrentHP, 500, "PG05 cone cover"); Near(rear.CurrentHP, 500, "PG05 excludes rear");
                config.SelectedPG06Ability = PG06Ability.CuraAdArea; config.SelectedPG06Passive = PG06Passive.VitaminaC;
                yield return Start(LoopPlayer.PG06);
                var ally = Probe(new Vector2(3, 0), Faction.PG); var outside = Probe(new Vector2(6, 0), Faction.PG); near = Probe(new Vector2(1, 0));
                ally.Hit(100); outside.Hit(100); loop.Player.Actor.Hit(50);
                block = Obstacle(new Vector2(2, 0), new Vector2(.2f, 3), true);
                float hp = loop.Player.Actor.CurrentHP;
                Ready(loop.PG06Ability); Check(loop.PG06Ability.TryActivate(Vector2.zero), "CURA AD AREA activation");
                Near(ally.CurrentHP, 440, "CURA heals ally through wall"); Near(outside.CurrentHP, 400, "CURA excludes outside"); Near(near.CurrentHP, 500, "CURA excludes MOB");
                Near(loop.Player.Actor.CurrentHP, Mathf.Min(loop.Player.Actor.Stats.HP, hp + 40), "CURA includes source");
                Check(ally.GetComponent<PG06Vitamin>() != null && ally.GetComponent<PG06Vitamin>().Remaining > 0, "CURA applies VITAMINA C");
                config.SelectedPG07Ability = PG07Ability.PioggiaDiFrecce;
                yield return Start(LoopPlayer.PG07);
                near = Probe(new Vector2(.3f, .3f)); covered = Probe(new Vector2(1, 1)); rear = Probe(new Vector2(-3, 0));
                block = Obstacle(new Vector2(.65f, .65f), new Vector2(.2f, .6f), false);
                Call(loop.PG07Ability, "Lucky", Vector2.zero);
                float lucky = Mathf.Floor(loop.Player.Actor.EffectiveStats.ATK * config.PG07Abilities.LuckyPercent / 100 + .5f);
                Near(near.CurrentHP, 500 - lucky, "LUCKY SHOT exposed MOB damage"); Near(covered.CurrentHP, 500, "LUCKY SHOT covered MOB"); Near(rear.CurrentHP, 500, "LUCKY SHOT outside radius");
                Visual(GameObject.Find("LUCKY SHOT"), "LUCKY SHOT visual");
                var hit = Probe(new Vector2(.7f, .65f)); hp = hit.CurrentHP;
                loop.PG07Ability.HitRainPoint(new Vector2(.7f, .65f)); Near(hit.CurrentHP, hp - 10, "Arrow impact retains wall/obstacle exception");
                Ready(loop.PG07Ability); Check(loop.PG07Ability.BeginAim(new Vector2(8, 0)), "Arrow rain preview");
                Visual(GameObject.Find("Anteprima PIOGGIA DI FRECCE"), "Arrow rain preview visual");
                Check(loop.PG07Ability.ReleaseAim(new Vector2(8, 0)), "Arrow rain release");
                Visual(GameObject.Find("Area attiva PIOGGIA DI FRECCE"), "Arrow rain distribution visual");
                yield return Wait(3.2f); Check(GameObject.Find("Area attiva PIOGGIA DI FRECCE") == null, "Arrow distribution marker expires");
                yield return WireChecks();
                foreach (string id in new[] { "aura", "taser", "repulse", "slash", "mines" }) yield return BonusChecks(id);
                yield return MobChecks();
            }
            finally { Time.timeScale = 1; loop.Definition = original; UnityEngine.Object.Destroy(config); }
        }
        private static IEnumerator WireChecks()
        {
            config.SelectedPG08Ability = PG08Ability.FiloSpinato;
            yield return Start(LoopPlayer.PG08);
            var mob = Probe(new Vector2(4, 0)); var hole = Probe(new Vector2(2, 0));
            Ready(loop.PG08Ability); Check(loop.PG08Ability.TryActivate(), "FILO SPINATO activation");
            var ring = UnityEngine.Object.FindFirstObjectByType<PG08WireArea>();
            Visual(ring.gameObject, "FILO SPINATO visual"); Near(ring.ValidSections, 10, "Wire all sections valid in open space");
            Near(mob.CurrentHP, 500, "Wire no immediate damage"); Near(mob.MovementMetresPerSecond, 1.2f, "Wire 40 percent SLOW"); Near(hole.MovementMetresPerSecond, 2, "Wire central hole excluded");
            Ready(loop.PG08Ability); Check(loop.PG08Ability.TryActivate(), "Second overlapping wire");
            yield return Wait(1.1f); Near(mob.CurrentHP, 490, "Wire overlaps do not duplicate damage"); Near(mob.MovementMetresPerSecond, 1.2f, "Wire overlaps do not stack SLOW");
            UnityEngine.ScreenCapture.CaptureScreenshot("Builds/AREA-wire-visible.png");
            Time.timeScale = 0; float before = ring.Remaining; double until = EditorApplication.timeSinceStartup + .15;
            while (EditorApplication.timeSinceStartup < until) yield return null;
            Near(ring.Remaining, before, "Wire duration pauses"); Near(mob.CurrentHP, 490, "Wire ticks pause"); Time.timeScale = 1;
            mob.transform.position = new Vector2(0, 2); yield return null;
            Near(mob.MovementMetresPerSecond, 2, "Wire exit removes SLOW"); yield return Wait(1.1f); Near(mob.CurrentHP, 490, "Wire exit stops ticks");
            loop.PG08Ability.ChangeArea(); yield return null;
            Check(UnityEngine.Object.FindObjectsByType<PG08WireArea>(FindObjectsSortMode.None).Length == 0, "Wire area change removes rings");
        }
        private static IEnumerator BonusChecks(string id)
        {
            yield return Start(LoopPlayer.PG01); Acquire(id);
            var exposed = Probe(new Vector2(.3f, .3f)); var covered = Probe(new Vector2(1, 1)); var side = Probe(new Vector2(.1f, 1)); var outside = Probe(new Vector2(-20, 0));
            var block = Obstacle(new Vector2(.65f, .65f), new Vector2(.2f, .6f), true);
            var brain = exposed.gameObject.AddComponent<MobBrain>(); brain.Initialize(config.ZOMB01, loop.Navigation); brain.enabled = false;
            exposed.Initialize(Faction.MOB, new CombatStats { HP = 500, ATK = 20, DEF = 100, MoveSpeed = 100 });
            float damage = loop.BonusAbilities.Value(id, "DANNO");
            if (id == "mines")
            {
                var go = new GameObject("MINE regression"); go.transform.SetParent(TestVisuals.Root); go.transform.position = Vector2.zero;
                go.AddComponent<BonusMine>().Initialize(loop, loop.Player.Actor, damage, loop.BonusAbilities.Value(id, "RAGGIO"), 10);
                while (go != null && go.activeSelf) yield return null;
            }
            else if (id == "slash") Call(loop.BonusAbilities, "Slash", Vector2.zero, Vector2.right);
            else Call(loop.BonusAbilities, "Activate", id);
            Near(exposed.CurrentHP, 500 - damage, id + " damage before blocker"); Near(side.CurrentHP, 500 - damage, id + " exposed side damage"); Near(covered.CurrentHP, 500, id + " cover prevents HIT"); Near(outside.CurrentHP, 500, id + " range/shape excludes outside");
            Visual(GameObject.Find("ABILITA BONUS area"), id + " visual");
            if (id == "taser") Check((float)typeof(MobBrain).GetField("stunnedUntil", Private).GetValue(brain) > Time.time, "TASER STUN applied");
            if (id == "repulse")
            {
                Vector2 old = side.transform.position; yield return Wait(.12f);
                Check(((Vector2)side.transform.position - old).sqrMagnitude > .001f, "REPULSE moves exposed MOB smoothly");
                Near(((Vector2)covered.transform.position - new Vector2(1, 1)).magnitude, 0, "REPULSE does not move covered MOB");
            }
            if (id == "slash")
            {
                var rear = Probe(new Vector2(-1, 0)); Call(loop.BonusAbilities, "Slash", Vector2.zero, Vector2.right);
                Near(rear.CurrentHP, 500, "SCIABOLATA excludes rear semicircle");
            }
            yield return Wait(.3f);
            Check(GameObject.Find("ABILITA BONUS area") == null, id + " visual expires");
        }
        private static IEnumerator MobChecks()
        {
            foreach (string id in new[] { "ZOMB03", "ZOMB05" })
            {
                yield return Start(LoopPlayer.PG01); loop.Player.transform.position = new Vector2(-20, 0);
                var pg = Probe(new Vector2(.3f, .3f), Faction.PG); var covered = Probe(new Vector2(1, 1), Faction.PG); var mob = Probe(new Vector2(.1f, 1));
                var block = Obstacle(new Vector2(.65f, .65f), new Vector2(.2f, .6f), false);
                var source = Probe(Vector2.zero); source.gameObject.AddComponent<SpriteRenderer>();
                var data = AssetDatabase.LoadAssetAtPath<MobDefinition>("Assets/TestEngine/Data/" + id + ".asset");
                var brain = source.gameObject.AddComponent<MobBrain>(); brain.Initialize(data, loop.Navigation); brain.enabled = false;
                if (id == "ZOMB03")
                {
                    Set(brain, "nextAggro", Time.time + 100); Set(brain, "pendingImpact", Time.time); Set(brain, "fixedImpact", Vector2.zero);
                    Call(brain, "Update"); Near(pg.CurrentHP, 500 - data.BaseStats.ATK, "ZOMB03 actual delayed area damage"); Near(mob.CurrentHP, 500, "ZOMB03 excludes MOB");
                }
                else
                {
                    source.Hit(10000); Set(brain, "explosionAt", Time.time); Call(brain, "Update");
                    Near(pg.CurrentHP, 500 - data.ExplosionDamagePG, "ZOMB05 PG damage"); Near(mob.CurrentHP, 500 - data.ExplosionDamageMOB, "ZOMB05 MOB damage"); Check(!source.IsActive, "ZOMB05 dies after explosion");
                }
                Near(covered.CurrentHP, 500, id + " protects covered PG"); Visual(GameObject.Find("Visible damage area"), id + " clipped visual");
            }
        }
    }
}
