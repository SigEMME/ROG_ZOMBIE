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
        private static readonly string WireRequest = Path.GetFullPath(".pg08-wire-test.request");
        private static readonly string CompanionRequest = Path.GetFullPath(".companion-test.request");
        private static readonly string WanderRequest = Path.GetFullPath(".mob-wander-test.request");
        private static readonly string CompositionRequest = Path.GetFullPath(".mob-composition-test.request");
        private static bool CompositionOnly => SessionState.GetBool(Key + "CompositionOnly", false);
        private static bool WanderOnly => SessionState.GetBool(Key + "WanderOnly", false);
        private static bool CompanionOnly => SessionState.GetBool(Key + "CompanionOnly", false);
        private static bool WireOnly => SessionState.GetBool(Key + "WireOnly", false);
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;
        private static bool companionInputPhase;

        static AreaEffectsValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run companion prototype tests")]
        public static void RunCompanion()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            SessionState.SetBool(Key + "CompanionOnly", true);
            SessionState.SetString(Key + "Report", Path.GetFullPath("COMPANION-validation.txt"));
            Run();
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
            config.SelectedPlayer = WireOnly ? LoopPlayer.PG08 : LoopPlayer.PG04;
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
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && (File.Exists(Request) || File.Exists(WireRequest) || File.Exists(CompanionRequest) || File.Exists(WanderRequest) || File.Exists(CompositionRequest)))
            {
                if (!SessionState.GetBool(Key + "Refreshed", false))
                {
                    SessionState.SetBool(Key + "Refreshed", true);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    return;
                }
                SessionState.EraseBool(Key + "Refreshed");
                string request = File.Exists(CompositionRequest) ? CompositionRequest : File.Exists(WanderRequest) ? WanderRequest : File.Exists(CompanionRequest) ? CompanionRequest : File.Exists(WireRequest) ? WireRequest : Request;
                SessionState.SetBool(Key + "CompanionOnly", request == CompanionRequest);
                SessionState.SetBool(Key + "WireOnly", request == WireRequest);
                SessionState.SetBool(Key + "WanderOnly", request == WanderRequest);
                SessionState.SetBool(Key + "CompositionOnly", request == CompositionRequest);
                SessionState.SetString(Key + "Report", File.ReadAllText(request).Trim());
                File.Delete(request);
                Run();
            }
            if (!SessionState.GetBool(Key + "Running", false) || !EditorApplication.isPlaying || routine == null) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 300) throw new TimeoutException("Ability test timeout.");
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop != null && loop.Player != null && !companionInputPhase)
                {
                    foreach (var component in loop.Player.GetComponents<MonoBehaviour>())
                        if (component.GetType().Name.EndsWith("AbilityInput")) component.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, CompositionOnly ? "Three actual AREA cycles, mixed MOB quotas/first spawn, shared circle/collider, colors, stats, drops, transition and third layout NavMesh. MOB brains disabled while draining populations; combat feel not tested." : WanderOnly ? "MOB ZOMB01-05: wandering without target, random 1-3 s direction, 30 percent speed, SLOW, collision, pause, STUN and target reacquisition." : CompanionOnly ? "Companion prototype: selection, formation, individual attacks, abilities, progression, AREA and RUN lifecycle." : WireOnly ? "PG08 FILO SPINATO only: continuous annulus, cover, damage, SLOW, lifetime and cleanup." : "AREA effects: PG01/03/05/06/07/08, circular BONUS effects, MINE, ZOMB03/05 and renderer regression. PG04 PYROMANIA has its dedicated regression suite.");
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
            SessionState.EraseBool(Key + "WireOnly");
            SessionState.EraseBool(Key + "WanderOnly");
            SessionState.EraseBool(Key + "CompositionOnly");
            SessionState.EraseBool(Key + "CompanionOnly");
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
                if (CompositionOnly) { yield return CompositionChecks(); yield break; }
                if (WanderOnly) { yield return WanderChecks(); yield break; }
                if (CompanionOnly) { yield return ResurrectionChecks(); yield return CompanionChecks(); yield return FullPartyChecks(); yield return PassageChecks(); yield break; }
                if (WireOnly) { yield return WireChecks(); yield break; }
                config.SelectedAbility = PG01Ability.Pestone;
                yield return Start(LoopPlayer.PG01);
                OptimizationChecks.Run(loop, Check); yield return null;
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
                Check(barrier.Solid.enabled && barrier.GetComponent<MeshRenderer>() != null, "BARRIERA collider and visual");
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
                Near(near.CurrentHP, 475, "PG05 cone damage (ATK 25)"); Near(covered.CurrentHP, 500, "PG05 cone cover"); Near(rear.CurrentHP, 500, "PG05 excludes rear");
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
        private static IEnumerator CompositionChecks()
        {
            config.EnableCompanion = false; config.SelectedPlayer = LoopPlayer.PG01;
            Check(config.Validate() == null, "Mixed MOB configuration valid");
            loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
            int expectedGold = 0;
            for (int area = 0; area < 3; area++)
            {
                loop.BonusAbilities.enabled = false;
                Check(loop.AreaIndex == area, "Correct AREA index " + area);
                Near(loop.Settings.AreaSize.x, area == 2 ? 130 : 100, "AREA width");
                Near(loop.Settings.AreaSize.y, area == 2 ? 130 : 100, "AREA height");
                Check(loop.Navigation.Ready && loop.Navigation.Reachable(loop.Settings.StartPosition,config.ExitForArea(area)), "Spawn and exit connected on actual NavMesh");
                var initial = new int[5]; foreach (var mob in Combatant.All) if (mob.Faction == Faction.MOB && mob.IsActive) initial[(int)mob.GetComponent<MobBrain>().Definition.Kind]++;
                var expectedFirst = SpawnManager.Allocate(loop.Spawns.MaxSimultaneous,config.AreaMobDistributions[area].Percentages);
                for (int type = 0; type < 5; type++) Near(initial[type],expectedFirst[type], "FIRST SPAWN type " + type);
                var counts = new int[5]; var seen = new HashSet<Combatant>();
                while (loop.State != LoopState.AreaComplete)
                {
                    if (loop.Experience.Choices != null) { loop.Experience.Choose(0); yield return null; continue; }
                    foreach (var mob in Combatant.All.ToArray())
                    {
                        if (mob.Faction != Faction.MOB || !mob.IsActive || !seen.Add(mob)) continue;
                        var definition = mob.GetComponent<MobBrain>().Definition; int type = (int)definition.Kind; counts[type]++;
                        if (counts[type] == 1)
                        {
                            Near(mob.Radius,loop.Settings.ActorRadius,"Shared collider radius " + definition.Kind);
                            var sprite = mob.GetComponent<SpriteRenderer>(); Near(sprite.sprite.bounds.extents.x,loop.Settings.ActorRadius,"Shared circular placeholder radius " + definition.Kind);
                            Color expected = type == 0 ? new Color(.4f,.7f,.4f) : type == 1 ? new Color(1,.55f,.15f) : new Color(.7f,.35f,.9f);
                            Check(sprite.color == expected,"Identification color " + definition.Kind);
                            Near(mob.Stats.HP,definition.BaseStats.HP,"Own HP " + definition.Kind);
                            Near(mob.Stats.MoveSpeed,type == 0 ? 90 : type == 1 ? 120 : 80,"GDD MOVE SPD " + definition.Kind);
                        }
                        expectedGold += definition.GoldDrop; mob.Die();
                    }
                    yield return null;
                }
                while (loop.Experience.Choices != null) { loop.Experience.Choose(0); yield return null; }
                var expectedCounts = SpawnManager.Allocate(config.AreaTotals[area],config.AreaMobDistributions[area].Percentages);
                for (int type = 0; type < 5; type++) Near(counts[type],expectedCounts[type],"Final AREA quota type " + type);
                Near(loop.Gold,expectedGold,"Gold from actual MOB definitions");
                loop.Player.transform.position=loop.Exit.transform.position; Call(loop,"OpenBonus");
                Check(loop.State == LoopState.Bonus,"End AREA opens bonus"); loop.SelectBonus(0); loop.ConfirmBonus();
                if (area < 2) { while (loop.State != LoopState.Combat) yield return null; }
                else Check(loop.State == LoopState.Finished,"Third AREA completes test sequence");
            }
        }

        private static IEnumerator WanderChecks()
        {
            config.EnableCompanion = false;
            yield return Start(LoopPlayer.PG01);
            var player = loop.Player.Actor; player.transform.position = Vector2.right * 20;
            player.InvisibleQuery = () => true;
            try
            {
                foreach (MobKind kind in Enum.GetValues(typeof(MobKind)))
                {
                    var definition = UnityEngine.Object.Instantiate(config.ZOMB01); definition.Kind = kind;
                    var mob = Probe(Vector2.zero); var brain = mob.gameObject.AddComponent<MobBrain>();
                    brain.Initialize(definition, loop.Navigation); brain.enabled = false;
                    try
                    {
                        Call(brain, "ChooseTarget");
                        Check(brain.GetType().GetField("target", Private).GetValue(brain) == null, kind + " ignores invisible PG");
                        for (int i = 0; i < 20; i++)
                        {
                            Set(brain, "wanderRemaining", 0f); mob.transform.position = Vector2.zero;
                            Call(brain, "Wander", .01f);
                            float remaining = (float)brain.GetType().GetField("wanderRemaining", Private).GetValue(brain);
                            Check(remaining >= .99f && remaining <= 2.99f, kind + " random duration in 1-3 s");
                        }
                        mob.transform.position = Vector2.zero; Set(brain, "wanderDirection", Vector2.up); Set(brain, "wanderRemaining", 2f);
                        Call(brain, "Wander", .5f);
                        Near(mob.transform.position.y, mob.MovementMetresPerSecond * .3f * .5f, kind + " moves at 30 percent speed");
                        Near(mob.Stats.MoveSpeed, definition.BaseStats.MoveSpeed, kind + " preserves persistent MOVE SPD");
                        Near((float)brain.GetType().GetField("wanderRemaining", Private).GetValue(brain), 1.5f, kind + " direction retained before interval expires");
                        mob.ApplyPestoneSlow(30, 5); mob.transform.position = Vector2.zero;
                        Call(brain, "Wander", .5f);
                        Near(mob.transform.position.y, definition.BaseStats.MetresPerSecond * .7f * .3f * .5f, kind + " SLOW combines with wandering");
                        mob.transform.position = Vector2.zero;
                        var block = Obstacle(Vector2.up, new Vector2(4, .2f), true);
                        yield return null; // Let Unity register the new physics shape before sweeping.
                        Call(brain, "Wander", 2f);
                        Check(mob.transform.position.y < .9f - mob.Radius + Physics2D.defaultContactOffset, kind + $" wall blocks movement (y={mob.transform.position.y:0.###}, radius={mob.Radius:0.###})");
                        block.SetActive(false); UnityEngine.Object.Destroy(block);
                        Vector3 before = mob.transform.position;
                        Time.timeScale = 0; Call(brain, "Update"); Time.timeScale = 1;
                        Near(Vector3.Distance(before, mob.transform.position), 0, kind + " pause stops wandering");
                        brain.Stun(1); Call(brain, "Update");
                        Near(Vector3.Distance(before, mob.transform.position), 0, kind + " STUN stops wandering");
                        Set(brain, "stunnedUntil", 0f);
                        player.InvisibleQuery = () => false; Call(brain, "ChooseTarget");
                        Check(brain.GetType().GetField("target", Private).GetValue(brain) == (object)player, kind + " visible PG reacquired");
                        Near((float)brain.GetType().GetField("wanderRemaining", Private).GetValue(brain), 0, kind + " reacquisition clears wandering timer");
                        player.InvisibleQuery = () => true; Call(brain, "ChooseTarget");
                        Check(brain.GetType().GetField("target", Private).GetValue(brain) == null, kind + " losing PG returns to wandering");
                    }
                    finally { mob.gameObject.SetActive(false); UnityEngine.Object.Destroy(mob.gameObject); UnityEngine.Object.Destroy(definition); }
                }
            }
            finally { player.InvisibleQuery = null; Time.timeScale = 1; }
        }

        private static IEnumerator ResurrectionChecks()
        {
            config.EnableCompanion = true; config.CompanionPlayer = LoopPlayer.PG02;
            yield return Start(LoopPlayer.PG01);
            var buddy = loop.Companion; var main = loop.Player.Actor; var ally = buddy.Player.Actor;
            buddy.Player.GetComponent<CompanionFormation>().enabled = false;
            buddy.Player.transform.position = Vector2.right;
            loop.enabled = false;
            try
            {
                main.Hit(100000);
                Near(loop.Resurrection.DownRemaining, 20, "DOWN starts at 20 s");
                Check(main.GetComponent<CircleCollider2D>().enabled, "DOWN retains physical collider");
                loop.TickResurrection(3, false);
                Near(loop.Resurrection.DownRemaining, 17, "DOWN time elapses");
                Check(loop.Controlled == buddy, "Control transfers to active IA");
                loop.TickResurrection(2, true);
                Near(loop.Resurrection.Progress, 2, "Controlled IA revives original PG");
                Near(loop.Resurrection.DownRemaining, 17, "Reviving freezes DOWN");
                loop.TickResurrection(1, false);
                Near(loop.Resurrection.Progress, 1, "Progress regresses at 1 s per second");
                Near(loop.Resurrection.DownRemaining, 16, "DOWN resumes exact residual timer");
                loop.TickResurrection(0, true);
                Near(loop.Resurrection.Progress, 1, "Zero delta preserves timers");
                buddy.Player.transform.position = Vector2.right * 2.01f;
                loop.TickResurrection(.5f, true);
                Near(loop.Resurrection.Progress, .5f, "Out of radius interrupts interaction");
                buddy.Player.transform.position = Vector2.right * 2;
                Time.timeScale = 0; loop.TickResurrection(2, true);
                Near(loop.Resurrection.Progress, .5f, "Pause freezes interaction"); Time.timeScale = 1;
                var stats = main.Stats; stats.HP += 40; main.SetStats(stats);
                loop.TickResurrection(4.5f, true);
                Check(main.IsActive, "At exactly 5 seconds target resurrects");
                Near(main.CurrentHP, main.EffectiveStats.HP * .5f, "Resurrection uses current maximum HP");
                Check(loop.Controlled == loop, "Original PG immediately regains control");
                Near(loop.Resurrection.Progress, 0, "Resurrection clears progress");
                Check(main.ResurrectionProtectionRemaining > 1.9f, "Two seconds protection starts");
                float health = main.CurrentHP; int hits = 0; System.Action onHit = () => hits++;
                main.BeforeHit += onHit; Check(!main.Hit(100000, true), "Invulnerability rejects HIT");
                Near(main.CurrentHP, health, "Invulnerability prevents damage"); Near(hits, 0, "Rejected HIT has no passive side effects"); main.BeforeHit -= onHit;
                Check(main.GetComponent<CircleCollider2D>().enabled, "Protection retains physical collider");
                yield return Wait(2.1f);
                Check(main.Hit(100000, true), "HIT resumes after protection expires");
                Near(loop.Resurrection.DownRemaining, 20, "Repeated DOWN resets timer");
                loop.TickResurrection(20, false);
                Check(main.State == LifeState.Dead, "DOWN expires into MORTE");
                Check(!main.GetComponent<CircleCollider2D>().enabled, "MORTE disables collision");
                loop.TickResurrection(5, true); Check(main.State == LifeState.Dead, "F cannot revive MORTE");
                typeof(LoopSession).GetMethod("RestoreMemberForArea", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, new object[] { loop });
                Near(main.CurrentHP, main.EffectiveStats.HP * .5f, "Area resurrection excludes extra 15 percent heal");
                Check(main.GetComponent<CircleCollider2D>().enabled, "Area resurrection restores collision");
                ally.Hit(100000); buddy.Player.transform.position = Vector2.right;
                loop.TickResurrection(5, true); Check(ally.IsActive, "Original PLAYER revives IA");
            }
            finally { loop.enabled = true; Time.timeScale = 1; }
            yield return Start(LoopPlayer.PG01);
            Near(loop.Resurrection.DownRemaining, 0, "RUN reset clears DOWN timer");
            Near(loop.Resurrection.Progress, 0, "RUN reset clears revival progress");
            Near(loop.Player.Actor.ResurrectionProtectionRemaining, 0, "RUN reset clears protection");
        }

        private static IEnumerator CompanionChecks()
        {
            var selection = new RunPreparationSelection(); selection.OpenPreparation();
            Check(selection.OpenBanner(0), "Open PLAYER banner"); selection.SelectPlayer(0); selection.SelectOption(false, 0); selection.SelectOption(true, 0); selection.ConfirmSelection();
            Check(selection.OpenBanner(1), "Open IA banner"); Check(!selection.IsAvailable(0), "Already selected PG blocked for IA");
            selection.SelectPlayer(1); selection.SelectOption(false, 0); selection.SelectOption(true, 1); selection.Back();
            Check(!selection.CanStart && selection.GetMember(1).Player == 1, "Back preserves IA draft without confirmation");
            selection.OpenBanner(1); selection.ConfirmSelection(); Check(selection.CanStart, "Both confirmed allow RUN");
            var run = selection.CreateRunDefinition(config); Check(run.EnableCompanion && run.SelectedPlayer == LoopPlayer.PG01 && run.CompanionPlayer == LoopPlayer.PG02 && run.CompanionPassive == 1, "Separate configuration snapshot"); UnityEngine.Object.Destroy(run);
            config.EnableCompanion = true;
            for (int pg = 0; pg < 8; pg++)
            for (int ability = 0; ability < 2; ability++)
            {
                config.CompanionPlayer = (LoopPlayer)pg; config.CompanionAbility = ability; config.CompanionPassive = ability;
                yield return Start(pg == 0 ? LoopPlayer.PG02 : LoopPlayer.PG01);
                var buddy = loop.Companion;
                Check(buddy != null && buddy.Player.Definition.PlayerId == ((LoopPlayer)pg).ToString(), "IA selected " + pg + "/" + ability);
                Check(buddy.Player.Actor != loop.Player.Actor && buddy.Bonuses != loop.Bonuses, "Independent actors and inventory");
                Check(!buddy.Player.GetComponent<PlayerMovement>().enabled, "IA does not read WASD");
                if (buddy.PG04Items != null) Near(buddy.PG04Items.SlotCount, 0, "IA has no item slots");
                var formation = buddy.Player.GetComponent<CompanionFormation>(); formation.enabled = false;
                buddy.Player.transform.SetPositionAndRotation(new Vector2(-1.5f, 0), Quaternion.identity);
                buddy.Player.GetComponent<PlayerAim>().enabled = false; buddy.Player.GetComponent<PlayerWeapon>().enabled = false;
                foreach (var c in buddy.Player.GetComponents<MonoBehaviour>()) if (c.GetType().Name.EndsWith("AbilityInput")) c.enabled = false;
                Physics2D.SyncTransforms();
                var weapon = buddy.Player.GetComponent<PlayerWeapon>();
                Check(weapon.TryFireAt(new Vector2(8, 0)), "IA base attack " + pg); Check(!weapon.TryFireAt(new Vector2(8, 0)), "IA own cadence blocks immediate second shot");
                if (buddy.Ability != null) { Ready(buddy.Ability); Check(ability == 0 ? buddy.Ability.TryPestone(new Vector2(5, 0)) : buddy.Ability.BeginAim(new Vector2(5, 0)) && buddy.Ability.ReleaseAim(new Vector2(5, 0)), "IA PG01 ability"); }
                if (buddy.PG02Ability != null) { Ready(buddy.PG02Ability); Check(buddy.PG02Ability.TryActivate(new Vector2(5, 0)), "IA PG02 ability"); }
                if (buddy.PG03Ability != null) { Ready(buddy.PG03Ability); Check(buddy.PG03Ability.TryActivate(new Vector2(5, 0)), "IA PG03 ability"); }
                if (buddy.PG04Ability != null) { Ready(buddy.PG04Ability); Check(ability == 0 ? buddy.PG04Ability.BeginAim(new Vector2(5, 0)) && buddy.PG04Ability.ReleaseAim(new Vector2(5, 0)) : buddy.PG04Ability.TryActivate(new Vector2(5, 0)), "IA PG04 ability"); }
                if (buddy.PG05Ability != null) { Ready(buddy.PG05Ability); Check(buddy.PG05Ability.TryActivate(new Vector2(5, 0)), "IA PG05 ability"); }
                if (buddy.PG06Ability != null) { buddy.Player.Actor.Hit(10); Ready(buddy.PG06Ability); Check(buddy.PG06Ability.TryActivate(new Vector2(5, 0)), "IA PG06 ability"); }
                if (buddy.PG07Ability != null) { Ready(buddy.PG07Ability); Check(ability == 1 ? buddy.PG07Ability.BeginAim(new Vector2(5, 0)) && buddy.PG07Ability.ReleaseAim(new Vector2(5, 0)) : buddy.PG07Ability.TryActivate(new Vector2(5, 0)), "IA PG07 ability"); }
                if (buddy.PG08Ability != null) { Ready(buddy.PG08Ability); Check(buddy.PG08Ability.TryActivate(), "IA PG08 ability"); }
                Check(loop.Player.Actor.HealthFraction == 1, "Companion actions preserve main HP");
            }
            config.CompanionPlayer = LoopPlayer.PG01; config.CompanionAbility = 1; config.CompanionPassive = 0;
            yield return Start(LoopPlayer.PG02);
            var controlledAI = loop.Companion; var adapter = controlledAI.Player.GetComponent<PG01AbilityInput>();
            controlledAI.Player.GetComponent<CompanionFormation>().enabled = false;
            controlledAI.Player.transform.position = new Vector2(-1.5f, 0);
            var keyboard = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            var mouse = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>();
            try
            {
                yield return null; yield return null; // Let the camera follow the relocated fixture before projecting the cursor.
                var screenPoint = loop.GameCamera.WorldToScreenPoint(new Vector3(4, 0, 0));
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = screenPoint });
                companionInputPhase = true;
                loop.Player.GetComponent<PlayerWeapon>().enabled = true;
                controlledAI.Player.GetComponent<PlayerWeapon>().enabled = true;
                int leaderShots = 0, followerShots = 0;
                System.Action leaderAction = () => leaderShots++, followerAction = () => followerShots++;
                loop.Player.Actor.PerformedAction += leaderAction; controlledAI.Player.Actor.PerformedAction += followerAction;
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = screenPoint, buttons = 1 });
                yield return Wait(.65f);
                Check(leaderShots > followerShots && followerShots > 0, "Shared LMB with independent ATK SPD: " + leaderShots + "/" + followerShots + " focused=" + Application.isFocused + " pointerBlocked=" + TestHUD.PointerOverControls);
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = screenPoint });
                yield return null; yield return null;
                int stopped = followerShots; yield return Wait(.2f); Check(followerShots == stopped, "IA stops firing when LMB released");
                loop.Player.Actor.PerformedAction -= leaderAction; controlledAI.Player.Actor.PerformedAction -= followerAction;
                Ready(controlledAI.Ability); adapter.enabled = true;
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Q));
                yield return null; yield return null;
                Check(!controlledAI.Ability.IsAiming, "Q does not activate companion ability");
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space, UnityEngine.InputSystem.Key.Digit1));
                yield return null; yield return null;
                Check(controlledAI.Ability.IsAiming, "SPACE+1 begins companion preview");
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState());
                yield return null; yield return null;
                Check(!controlledAI.Ability.IsAiming && controlledAI.Ability.CooldownRemaining > 0, "Chord release places companion BARRIERA");
            }
            finally { companionInputPhase = false; UnityEngine.InputSystem.InputSystem.RemoveDevice(mouse); UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard); adapter.enabled = false; }
            config.CompanionPlayer = LoopPlayer.PG02; config.CompanionAbility = 0; config.CompanionPassive = 0;
            yield return Start(LoopPlayer.PG01);
            var ai = loop.Companion; var follower = ai.Player.GetComponent<CompanionFormation>(); follower.enabled = false;
            ai.Player.GetComponent<PlayerWeapon>().enabled = false; ai.Player.GetComponent<PlayerAim>().enabled = false;
            foreach (var c in ai.Player.GetComponents<MonoBehaviour>()) if (c.GetType().Name.EndsWith("AbilityInput")) c.enabled = false;
            ai.Player.transform.position = new Vector2(-1f, 0); Physics2D.SyncTransforms();
            follower.Advance(new Vector2(10, 0), .02f); Near(Vector2.Distance(follower.Goal, new Vector2(-1f, 0)), 0, "Formation behind cursor aim");
            Vector2 beforeTurn = ai.Player.transform.position;
            follower.Advance(new Vector2(0, 10), .02f);
            Check(Vector2.Distance(beforeTurn, ai.Player.transform.position) < .1f, "Cursor turn starts gradually without a formation jump");
            for (int n = 0; n < 100; n++) follower.Advance(new Vector2(0, 10), .02f);
            Near(Vector2.Distance(ai.Player.transform.position, new Vector2(0, -1f)), 0, "Formation rotates behind aim", .05f);
            for (int n = 0; n < 100; n++) { follower.Advance(new Vector2(0, -10), .02f); Physics2D.SyncTransforms(); }
            Near(Vector2.Distance(ai.Player.transform.position, new Vector2(0, 1f)), 0, "Formation handles reversed aim without crossing PLAYER", .05f);
            var formationBlocker = Obstacle(new Vector2(-1, 0), new Vector2(.5f, .5f), true);
            loop.RefreshNavigation();
            Check(follower.TryResolvePosition(Vector2.zero, Vector2.left, out var alternative), "Blocked formation finds a NavMesh alternative");
            Check(alternative.sqrMagnitude <= 1f && Vector2.Distance(alternative, Vector2.left) > .35f, "Alternative is free and inside one metre");
            formationBlocker.SetActive(false); loop.RefreshNavigation();
            var crowd = new List<Combatant>();
            for (int n = 0; n < 16; n++) { float angle = n * Mathf.PI / 8; crowd.Add(Probe(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .8f, Faction.PG)); }
            ai.Player.transform.position = new Vector2(3, 0); Physics2D.SyncTransforms();
            Check(follower.TryResolvePosition(Vector2.zero, Vector2.left, out var distant) && distant.sqrMagnitude > 1f, "Temporary distance above one metre when all local positions occupied");
            foreach (var member in crowd) member.gameObject.SetActive(false);
            Check(follower.TryResolvePosition(Vector2.zero, Vector2.left, out var restored) && restored.sqrMagnitude <= 1f, "Returns to one-metre destination as soon as space is free");
            float baseMove = ai.Player.Actor.Stats.MoveSpeed;
            Time.timeScale = 0; Vector3 paused = ai.Player.transform.position; follower.Advance(new Vector2(-10, 0), 1); Near(Vector2.Distance(paused, ai.Player.transform.position), 0, "Formation pauses"); Time.timeScale = 1;
            Near(ai.Player.Actor.Stats.MoveSpeed, baseMove, "Formation preserves persistent MOVE SPD");
            ai.Experience.Award(225); loop.Experience.Award(225); yield return WaitRealtime(.1f);
            Check((ai.Experience.Choices != null) != (loop.Experience.Choices != null), "Simultaneous LEVEL UP serialized");
            for (int n = 0; n < 20 && (ai.Experience.PendingChoices > 0 || loop.Experience.PendingChoices > 0); n++)
            { var ctx = loop.ChoiceContext; if (ctx.Experience.Choices != null) ctx.Experience.Choose(0); yield return null; }
            Check(ai.Experience.PendingChoices == 0 && loop.Experience.PendingChoices == 0 && Time.timeScale > 0, "Both LEVEL UP queues resolve without stuck pause");
            var persistent = ai.Player; ai.Player.Actor.Hit(10); float health = ai.Player.Actor.CurrentHP;
            foreach (var mob in Combatant.All.ToArray()) if (mob.Faction == Faction.MOB) mob.gameObject.SetActive(false);
            Set(loop, "<State>k__BackingField", LoopState.AreaComplete); Call(loop, "OpenBonus");
            Check(loop.RewardContext == loop && loop.State == LoopState.Bonus, "End AREA first choice for PLAYER");
            loop.SelectBonus(3); loop.ConfirmBonus();
            Check(loop.RewardContext == ai && loop.State == LoopState.Bonus, "End AREA requires separate IA choice");
            loop.SelectBonus(3); loop.ConfirmBonus();
            while (loop.State != LoopState.Combat) yield return null;
            Check(loop.Companion.Player == persistent, "Companion persists across AREA"); Check(ai.Player.Actor.CurrentHP >= health, "Companion AREA heal retained");
            yield return null;
            UnityEngine.ScreenCapture.CaptureScreenshot("Builds/Companion-prototype.png");
            yield return null; yield return null;
            loop.Player.transform.position = loop.Exit.transform.position;
            ai.Player.transform.position = loop.Exit.transform.position + Vector3.right * 6;
            Check(!loop.PartyReadyForExit(), "Exit waits for companion inside radius");
            ai.Player.transform.position = loop.Exit.transform.position + Vector3.right * 1.5f;
            Check(loop.PartyReadyForExit(), "Both active PG inside exit are ready");
            loop.Player.Actor.Hit(10000); yield return null; yield return null;
            Check(loop.Controlled == ai && ai.Player.GetComponent<PlayerMovement>().enabled, "DOWN transfers direct control to companion");
            Check(!loop.PartyReadyForExit(), "DOWN member blocks AREA exit");
            ai.Player.Actor.Hit(10000); yield return null; yield return null;
            Check(loop.State == LoopState.Defeat, "Both DOWN ends RUN");
            yield return Start(LoopPlayer.PG01); Check(loop.Companion.Player != persistent && loop.Companion.Experience.Level == 1, "RUN resets companion state");
        }
        private static IEnumerator FullPartyChecks()
        {
            var selection = new RunPreparationSelection(); selection.OpenPreparation();
            for (int slot = 0; slot < 4; slot++)
            {
                Check(selection.OpenBanner(slot), "Four-member banner " + slot);
                for (int used = 0; used < slot; used++) Check(!selection.IsAvailable(used), "No duplicate roster choice " + used);
                selection.SelectPlayer(slot); selection.SelectOption(false, slot % 2); selection.SelectOption(true, slot % 2);
                selection.Back(); Check(!selection.CanStart, "Unconfirmed member blocks RUN " + slot);
                selection.OpenBanner(slot); Check(selection.ConfirmSelection(), "Confirm member " + slot);
            }
            var snapshot = selection.CreateRunDefinition(config);
            Check(snapshot != null && snapshot.CompanionAt(2).Player == LoopPlayer.PG03 && snapshot.CompanionAt(3).Ability == 1, "Snapshot holds all selections");
            selection.RemoveCompanion(2);
            Check(selection.CanStart && !selection.IsEnabled(2) && selection.IsEnabled(3) && snapshot.CompanionAt(2).Enabled, "Removal keeps slot 3 and preserves RUN snapshot");
            UnityEngine.Object.Destroy(snapshot);
            config.EnableCompanion = true; config.CompanionPlayer = LoopPlayer.PG02;
            config.CompanionAbility = 0; config.CompanionPassive = 0;
            config.AdditionalCompanions = new[] {
                new CompanionSelection { Enabled = true, Player = LoopPlayer.PG03 },
                new CompanionSelection { Enabled = true, Player = LoopPlayer.PG07, Ability = 1 }
            };
            yield return Start(LoopPlayer.PG01);
            Check(loop.Companions.Count == 3, "Three IA contexts created");
            var followers = new List<CompanionFormation>();
            foreach (var member in loop.Companions)
            {
                var formation = member.Player.GetComponent<CompanionFormation>(); formation.enabled = false; followers.Add(formation);
                member.Player.GetComponent<PlayerMovement>().enabled = false;
                member.Player.GetComponent<PlayerAim>().enabled = false; member.Player.GetComponent<PlayerWeapon>().enabled = false;
                foreach (var component in member.Player.GetComponents<MonoBehaviour>()) if (component.GetType().Name.EndsWith("AbilityInput")) component.enabled = false;
                member.Player.transform.position = CompanionFormation.Offset(3, member.PartySlot - 1);
                Check(!member.Player.GetComponent<PlayerWeapon>().TrajectoryVisibleWhen(), "IA trajectory hidden " + member.PartySlot);
            }
            Physics2D.SyncTransforms();
            Vector2 origin = loop.Player.transform.position;
            Near(Vector2.Distance(origin, loop.Companions[0].Player.transform.position), 1, "Diamond front-left side");
            Near(Vector2.Distance(origin, loop.Companions[1].Player.transform.position), 1, "Diamond front-right side");
            Near(Vector2.Distance(loop.Companions[2].Player.transform.position, loop.Companions[0].Player.transform.position), 1, "Diamond rear-left side");
            Near(Vector2.Distance(loop.Companions[2].Player.transform.position, loop.Companions[1].Player.transform.position), 1, "Diamond rear-right side");
            for (int frame = 0; frame < 250; frame++)
                foreach (var formation in followers) { formation.Advance(new Vector2(0, 10), .02f); Physics2D.SyncTransforms(); }
            foreach (var formation in followers)
            {
                Near(Vector2.Distance(formation.transform.position, formation.Goal), 0, "Diamond follows cursor " + formation.Context.PartySlot, .08f);
                Vector2 before = formation.transform.position; Time.timeScale = 0; formation.Advance(Vector2.down * 10, .1f); Time.timeScale = 1;
                Near(Vector2.Distance(before, formation.transform.position), 0, "Formation pause " + formation.Context.PartySlot);
            }
            var blocker = Obstacle(followers[2].Goal, new Vector2(.35f, .35f), true); loop.RefreshNavigation();
            Check(followers[2].TryResolvePosition(origin, followers[2].Goal, out var free), "Rear slot resolves cover on NavMesh");
            Check(Vector2.Distance(free, followers[2].Goal) > .35f, "Rear alternative clears cover");
            blocker.SetActive(false); loop.RefreshNavigation();
            var keyboard = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            try
            {
                for (int slot = 1; slot <= 3; slot++)
                {
                    var digit = slot == 1 ? UnityEngine.InputSystem.Key.Digit1 : slot == 2 ? UnityEngine.InputSystem.Key.Digit2 : UnityEngine.InputSystem.Key.Digit3;
                    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space, digit));
                    UnityEngine.InputSystem.InputSystem.Update(); keyboard.MakeCurrent();
                    for (int other = 1; other <= 3; other++) Check(PartyCommands.Held(loop.Companions[other - 1].Player) == (other == slot), "Isolated chord " + slot + " target " + other + " (slot=" + loop.Companions[other - 1].PartySlot + ", direct=" + loop.Companions[other - 1].DirectlyControlled + ", space=" + keyboard.spaceKey.isPressed + ")");
                    Check(!PartyCommands.Held(loop.Player), "IA chord does not trigger PLAYER Q");
                    Check(PartyCommands.Pressed(loop.Companions[slot - 1].Player), "IA chord press " + slot);
                    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState());
                    UnityEngine.InputSystem.InputSystem.Update(); keyboard.MakeCurrent();
                    Check(PartyCommands.Released(loop.Companions[slot - 1].Player) && !PartyCommands.Held(loop.Companions[slot - 1].Player), "IA chord release " + slot);
                    yield return null;
                }
            }
            finally { UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard); }
            loop.enabled = false;
            try
            {
                loop.Player.transform.position = Vector2.zero;
                loop.Companions[0].Player.transform.position = Vector2.right;
                loop.Companions[1].Player.transform.position = Vector2.up * 1.5f;
                loop.Companions[0].Player.Actor.Hit(100000); loop.Companions[1].Player.Actor.Hit(100000);
                loop.TickResurrection(2, true);
                Near(loop.Companions[0].Resurrection.Progress, 2, "F revives closest DOWN only");
                Near(loop.Companions[1].Resurrection.Progress, 0, "Other DOWN has no concurrent progress");
                loop.Companions[1].Player.transform.position = Vector2.up * .8f;
                loop.TickResurrection(1, true);
                Near(loop.Companions[0].Resurrection.Progress, 1, "Previous target regresses when nearer DOWN changes");
                Near(loop.Companions[1].Resurrection.Progress, 1, "New closest DOWN progresses");
                loop.TickResurrection(4, true); loop.TickResurrection(5, true);
                Check(loop.Companions[0].Player.Actor.IsActive && loop.Companions[1].Player.Actor.IsActive, "Both revived sequentially");
                loop.Player.Actor.Hit(100000); loop.TickResurrection(.01f, false);
                Check(loop.Controlled == loop.Companions[0], "First active IA takes control");
                loop.Companions[0].Player.Actor.Die(); loop.TickResurrection(.01f, false);
                Check(loop.Controlled == loop.Companions[1], "Next active IA takes control");
                loop.Player.Actor.Resurrect();
                loop.TickResurrection(.01f, false); Check(loop.Controlled == loop, "Original PG regains control");
            }
            finally { loop.enabled = true; }
            foreach (var member in loop.Members) member.Experience.Award(225);
            yield return WaitRealtime(.1f);
            for (int i = 0; i < 60; i++)
            {
                var choice = loop.ChoiceContext;
                if (choice.Experience.Choices != null) choice.Experience.Choose(0);
                yield return null;
            }
            foreach (var member in loop.Members) Check(member.Experience.PendingChoices == 0, "All individual LEVEL UP choices resolved " + member.PartySlot);
            var persistent = loop.Companions[2].Player;
            Set(loop, "<State>k__BackingField", LoopState.AreaComplete); Call(loop, "OpenBonus");
            foreach (var member in loop.Members)
            {
                if (member.Player.Actor.State == LifeState.Dead) continue;
                Check(loop.RewardContext == member, "End AREA rewards follow party order " + member.PartySlot);
                loop.SelectBonus(0); loop.ConfirmBonus();
            }
            while (loop.State != LoopState.Combat) yield return null;
            Check(loop.Companions.Count == 3 && loop.Companions[2].Player == persistent, "All three IA persist across AREA");
            Check(loop.Companions[0].Player.Actor.IsActive, "Dead IA resurrects on AREA transition");
            foreach (var member in loop.Members) member.Player.transform.position = loop.Exit.transform.position;
            Check(loop.PartyReadyForExit(), "All four PG ready at exit");
            loop.Companions[2].Player.transform.position += Vector3.right * 5;
            Check(!loop.PartyReadyForExit(), "Exit waits for third IA");
            yield return Start(LoopPlayer.PG01);
            Check(loop.Companions.Count == 3 && loop.Companions[2].Player != persistent, "RUN reset recreates all IA");
            foreach (var member in loop.Members) Near(member.Experience.Level, 1, "RUN resets member level " + member.PartySlot);
            config.AdditionalCompanions[1].Enabled = false;
            yield return Start(LoopPlayer.PG01);
            Check(loop.Companions.Count == 2, "Two IA configuration supported");
            var left = loop.FormationOffset(loop.Companions[0]); var right = loop.FormationOffset(loop.Companions[1]);
            Near(left.magnitude, 1, "Triangle first side"); Near(right.magnitude, 1, "Triangle second side"); Near(Vector2.Distance(left, right), 1, "Triangle IA spacing");
            config.EnableCompanion = false; config.AdditionalCompanions[0].Enabled = false; config.AdditionalCompanions[1].Enabled = true;
            yield return Start(LoopPlayer.PG01);
            Check(loop.Companions.Count == 1 && loop.Companion.PartySlot == 3, "Removing earlier IA preserves banner command 3");
            Near(Vector2.Distance(loop.FormationOffset(loop.Companion), Vector2.left), 0, "Single remaining IA uses one-metre rear slot");
            config.AdditionalCompanions = new CompanionSelection[0];
        }
        private static IEnumerator PassageChecks()
        {
            config.EnableCompanion = true; config.CompanionPlayer = LoopPlayer.PG02;
            config.AdditionalCompanions = new[] {
                new CompanionSelection { Enabled = true, Player = LoopPlayer.PG03 },
                new CompanionSelection { Enabled = true, Player = LoopPlayer.PG07 }
            };
            yield return Start(LoopPlayer.PG01);
            loop.enabled = false;
            try
            {
                var top = Obstacle(new Vector2(0, .8f), new Vector2(20, .5f), true);
                var bottom = Obstacle(new Vector2(0, -.8f), new Vector2(20, .5f), true);
                loop.RefreshNavigation();
                var formations = new List<CompanionFormation>();
                loop.Player.transform.position = Vector2.zero;
                foreach (var member in loop.Companions)
                {
                    var formation = member.Player.GetComponent<CompanionFormation>(); formation.enabled = false; formations.Add(formation);
                    member.Player.GetComponent<PlayerAim>().enabled = false; member.Player.GetComponent<PlayerWeapon>().enabled = false;
                    member.Player.transform.position = Vector2.right * member.PartySlot * .75f;
                }
                Physics2D.SyncTransforms();
                for (int frame = 0; frame < 100; frame++)
                {
                    loop.Player.Actor.Move(Vector2.right * .04f);
                    foreach (var formation in formations) formation.Advance((Vector2)loop.Player.transform.position + Vector2.right * 10, .02f, Vector2.right * 2);
                    Physics2D.SyncTransforms();
                }
                Check(loop.Player.transform.position.x > 3.8f, "Narrow corridor: PLAYER advances past formerly blocking IA");
                foreach (var formation in formations)
                {
                    Check(formation.transform.position.x > loop.Player.transform.position.x, "Queue moves forward outside formation " + formation.Context.PartySlot);
                    Check(Mathf.Abs(formation.transform.position.y) <= .2f, "Queue respects corridor walls " + formation.Context.PartySlot);
                    Near(formation.Context.Player.Actor.Stats.MoveSpeed, formation.Context.Player.Actor.InitialStats.MoveSpeed, "Yield preserves MOVE SPD " + formation.Context.PartySlot);
                }
                for (int i = 0; i < 2; i++) Check(Vector2.Distance(formations[i].transform.position, formations[i + 1].transform.position) >= .699f, "Queue retains PG collision " + i);
                var first = formations[0]; Vector2 paused = first.transform.position;
                Time.timeScale = 0; first.Advance(Vector2.right * 20, .02f, Vector2.right * 2); Time.timeScale = 1;
                Near(Vector2.Distance(paused, first.transform.position), 0, "Passage following pauses");
                var end = Obstacle(new Vector2(8, 0), new Vector2(.5f, 2), true); loop.RefreshNavigation();
                for (int frame = 0; frame < 100; frame++)
                {
                    loop.Player.Actor.Move(Vector2.right * .04f);
                    foreach (var formation in formations) formation.Advance(Vector2.right * 20, .02f, Vector2.right * 2);
                    Physics2D.SyncTransforms();
                }
                Check(formations[2].transform.position.x <= 7.41f, "Front IA stops at solid dead end");
                foreach (var member in loop.Members) Check(member.Player.transform.position.x < 7.75f, "No PG crosses end wall " + member.PartySlot);
                top.SetActive(false); bottom.SetActive(false); end.SetActive(false); loop.RefreshNavigation();
                // Once the passage is free, the ordinary formation must recover without further input.
                for (int frame = 0; frame < 500; frame++)
                    foreach (var formation in formations) { formation.Advance((Vector2)loop.Player.transform.position + Vector2.right * 10, .02f, Vector2.zero); Physics2D.SyncTransforms(); }
                foreach (var formation in formations) Near(Vector2.Distance(formation.transform.position, formation.Goal), 0, "Formation recovers after corridor " + formation.Context.PartySlot, .1f);
            }
            finally { loop.enabled = true; Time.timeScale = 1; config.AdditionalCompanions = new CompanionSelection[0]; }
        }
        private static IEnumerator WaitRealtime(float seconds)
        { double until = EditorApplication.timeSinceStartup + seconds; while (EditorApplication.timeSinceStartup < until) yield return null; }

        private static IEnumerator WireChecks()
        {
            config.SelectedPG08Ability = PG08Ability.FiloSpinato;
            yield return Start(LoopPlayer.PG08);
            var mob = Probe(new Vector2(4, 0)); var hole = Probe(new Vector2(2, 0));
            Ready(loop.PG08Ability); Check(loop.PG08Ability.TryActivate(), "FILO SPINATO activation");
            var ring = UnityEngine.Object.FindFirstObjectByType<PG08WireArea>();
            Visual(ring.gameObject, "FILO SPINATO visual"); Check(ring.Contains(mob), "Wire continuous annulus includes exposed target");
            Near(mob.CurrentHP, 500, "Wire no immediate damage"); Near(mob.MovementMetresPerSecond, 1.2f, "Wire 40 percent SLOW"); Near(hole.MovementMetresPerSecond, 2, "Wire central hole excluded");
            var blocker = Obstacle(new Vector2(2, 0), new Vector2(.2f, .2f), true);
            Physics2D.SyncTransforms();
            Check(!ring.Contains(mob), "Wire wall between center and ring shields target");
            Near(mob.MovementMetresPerSecond, 2, "Wire cover removes SLOW immediately");
            var adjacent = Probe(new Vector2(3.8f, .8f));
            Check(ring.Contains(adjacent), "Wire adjacent visible target remains affected");
            yield return Wait(1.1f); Near(mob.CurrentHP, 500, "Wire covered target receives no tick damage");
            Near(adjacent.CurrentHP, 490, "Wire adjacent visible target receives damage");
            blocker.SetActive(false); Physics2D.SyncTransforms();
            Check(ring.Contains(mob), "Wire removal of cover restores coverage");
            Ready(loop.PG08Ability); Check(loop.PG08Ability.TryActivate(), "Second overlapping wire");
            yield return Wait(1.1f); Near(mob.CurrentHP, 490, "Wire overlaps do not duplicate damage"); Near(mob.MovementMetresPerSecond, 1.2f, "Wire overlaps do not stack SLOW");
            UnityEngine.ScreenCapture.CaptureScreenshot("Builds/AREA-wire-visible.png");
            Time.timeScale = 0; float before = ring.Remaining; double until = EditorApplication.timeSinceStartup + .15;
            while (EditorApplication.timeSinceStartup < until) yield return null;
            Near(ring.Remaining, before, "Wire duration pauses"); Near(mob.CurrentHP, 490, "Wire ticks pause"); Time.timeScale = 1;
            mob.transform.position = new Vector2(0, 2); Physics2D.SyncTransforms(); yield return null;
            Near(mob.MovementMetresPerSecond, 2, "Wire exit removes SLOW"); yield return Wait(1.1f); Near(mob.CurrentHP, 490, "Wire exit stops ticks");
            loop.PG08Ability.ChangeArea(); yield return null;
            Check(UnityEngine.Object.FindObjectsByType<PG08WireArea>(FindObjectsSortMode.None).Length == 0, "Wire area change removes rings");
            if (WireOnly)
            {
                yield return Start(LoopPlayer.PG08);
                mob = Probe(new Vector2(4, 0));
                var outside = Probe(new Vector2(4.51f, 0));
                var inside = Probe(new Vector2(3.49f, 0));
                blocker = Obstacle(new Vector2(2, 0), new Vector2(.2f, .2f), false);
                Ready(loop.PG08Ability); Check(loop.PG08Ability.TryActivate(), "Wire obstacle fixture activation");
                ring = UnityEngine.Object.FindFirstObjectByType<PG08WireArea>();
                Check(!ring.Contains(mob), "OSTACOLO shields target from fixed center");
                Check(!ring.Contains(outside) && !ring.Contains(inside), "Wire excludes both radial boundaries beyond annulus");
                yield return Wait(1.1f); Near(mob.CurrentHP, 500, "OSTACOLO prevents tick damage");
                Near(mob.MovementMetresPerSecond, 2, "OSTACOLO prevents SLOW");
                blocker.SetActive(false); Physics2D.SyncTransforms();
                yield return Wait(.2f);
                UnityEngine.ScreenCapture.CaptureScreenshot("Builds/PG08-wire-only-visible.png");
                Near(mob.MovementMetresPerSecond, 1.2f, "Wire SLOW returns after obstacle removal");
                yield return Wait(4f);
                Check(UnityEngine.Object.FindObjectsByType<PG08WireArea>(FindObjectsSortMode.None).Length == 0, "Wire expires after five seconds");
                Near(mob.CurrentHP, 470, "Wire three complete contact seconds after late entry");
                Near(mob.MovementMetresPerSecond, 2, "Wire natural expiry removes SLOW");
                Ready(loop.PG08Ability); Check(loop.PG08Ability.TryActivate(), "Wire active before RUN reset");
                yield return Start(LoopPlayer.PG08);
                Check(UnityEngine.Object.FindObjectsByType<PG08WireArea>(FindObjectsSortMode.None).Length == 0, "RUN reset removes wire");
            }
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
