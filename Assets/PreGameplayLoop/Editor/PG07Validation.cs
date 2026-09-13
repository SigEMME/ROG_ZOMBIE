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
    public static class PG07Validation
    {
        private const string Key = "ROG.PG07.";
        private static readonly string Request = Path.GetFullPath(".pg07-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG07Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG07 tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
            // Select PG07 in memory for this suite only, preserving the user's Inspector choice.
            var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
            SessionState.SetInt(Key + "PreviousPlayer", (int)config.SelectedPlayer);
            SessionState.SetBool(Key + "RestoreSelection", true);
            config.SelectedPlayer = LoopPlayer.PG07;
            SessionState.SetBool(Key + "Running", true);
            EditorApplication.isPlaying = true;
        }

        private static void PlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            {
                results.Clear(); warnings = 0; loop = null;
                started = EditorApplication.timeSinceStartup;
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
                if (EditorApplication.timeSinceStartup - started > 300) throw new TimeoutException("Ability test timeout.");
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop != null && loop.Player != null)
                {
                    var input = loop.Player.GetComponent<PG07AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. PG07 only, four combinations: actual shots, abilities, passives and AREA transitions. Input feel/balancing are not tested.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG07-validation.txt"));
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
            actor.Initialize(faction, faction == Faction.PG ? CombatStats.FromPG(loop.Player.Definition.BaseStats) : new CombatStats { HP = 500, DEF = defence, ATK = 20, MoveSpeed = 100, Range = 400 });
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

        private static readonly Vector2 Origin = new Vector2(30, 30);
        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds; while (Time.time < end) yield return null;
        }
        private static IEnumerator Fire(Vector2 cursor)
        {
            while (!loop.Player.GetComponent<PlayerWeapon>().TryFireAt(cursor)) yield return null;
        }
        private static void Remove(params Combatant[] actors)
        {
            foreach (var actor in actors) if (actor != null) UnityEngine.Object.Destroy(actor.gameObject);
        }
        private static void HP(Combatant actor, float hp) => actor.SetStats(actor.Stats, hp - actor.CurrentHP);
        private static void ReducedCD() => typeof(LoopSession).GetField("cdReduction", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(loop, 10f);
        private static void SeedRoll(float chance, bool success)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if ((UnityEngine.Random.value < chance) != success) continue;
                UnityEngine.Random.InitState(seed); return;
            }
            throw new Exception("No deterministic seed");
        }
        private sealed class ExtraHit : IProjectileHitEffect
        {
            public int Count;
            public void ResolveHit(Combatant target, float damage, bool round, Combatant source, int index)
            { Count++; target.Hit(damage, true, source); }
        }
        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            Check(loop.Player.Definition.PlayerId == "PG07", "Suite starts PG07 only");
            var original = loop.Definition; var config = UnityEngine.Object.Instantiate(original); loop.Definition = config;
            try
            {
                foreach (PG07Ability choice in Enum.GetValues(typeof(PG07Ability)))
                foreach (PG07Passive passive in Enum.GetValues(typeof(PG07Passive)))
                {
                    config.SelectedPlayer = LoopPlayer.PG07; config.SelectedPG07Ability = choice; config.SelectedPG07Passive = passive;
                    Check(config.Validate() == null, "Valid combination " + choice + "/" + passive);
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor; var ability = loop.PG07Ability;
                    loop.Player.transform.SetPositionAndRotation(Origin, Quaternion.identity);
                    Check(loop.Ability == null && loop.PG02Ability == null && loop.PG03Ability == null && loop.PG04Ability == null && loop.PG05Ability == null && loop.PG06Ability == null, "Only PG07 runtime instantiated");
                    Near(actor.Stats.HP, 100, "PG07 HP100"); Near(actor.Stats.ATK, 30, "PG07 ATK30"); Near(actor.Stats.DEF, 100, "PG07 DEF neutral");
                    Near(actor.Stats.AttackSpeed, 110, "PG07 ATK SPD110"); Near(actor.MovementMetresPerSecond, 2.2f, "PG07 MOVE110"); Near(actor.Stats.RangeMetres, 15, "PG07 RANGE15");
                    Near(ability.CooldownRemaining, choice == PG07Ability.MultiShot ? 10 : 15, "Initial full CD", .15f);
                    Check(!ability.TryActivate(Origin), "RUN starts with cooldown");
                    config.SelectedPG07Passive = passive == PG07Passive.LuckyShot ? PG07Passive.Concentrazione : PG07Passive.LuckyShot;
                    Check(ability.Passive == passive, "RUN snapshots passive"); config.SelectedPG07Passive = passive;
                    var test = BaseChecks(passive); while (test.MoveNext()) yield return null;
                    test = passive == PG07Passive.LuckyShot ? LuckyChecks() : ConcentrationChecks(); while (test.MoveNext()) yield return null;
                    while (ability.CooldownRemaining > 0) yield return null; ReducedCD();
                    int rolls = ability.PassiveRolls;
                    test = choice == PG07Ability.MultiShot ? MultiChecks() : RainChecks(); while (test.MoveNext()) yield return null;
                    Near(ability.PassiveRolls, rolls, "Abilities never roll base-attack passives");
                    while (ability.CooldownRemaining > 0) yield return null;
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.IsActive && mob.Faction == Faction.MOB) mob.Die();
                        yield return null;
                    }
                    Check(ability.TryActivate(Origin + Vector2.up * 40), "Activate before AREA transition");
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    float cd = ability.CooldownRemaining; int arrows = ability.RainEmitted;
                    double pause = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
                    Near(ability.CooldownRemaining, cd, "BONUS pauses CD"); Near(ability.RainEmitted, arrows, "BONUS pauses pending arrows");
                    loop.Choices[0] = AreaStat.ATK; loop.SelectBonus(0); loop.ConfirmBonus();
                    Near(ability.CooldownRemaining, choice == PG07Ability.PioggiaDiFrecce ? 1.5f : cd, "AREA restarts active rain CD, preserves inactive CD");
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.PG07Ability == ability && ability.Selected == choice && ability.Passive == passive, "AREA preserves PG07 selections");
                    Check(!ability.EffectActive, "AREA cancels remaining rain"); Near(actor.Stats.ATK, 31.5f, "AREA preserves bonus ATK");
                    Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "AREA clears old projectiles");
                    while (ability.CooldownRemaining > 0) yield return null; ability.ChangeArea(); Near(ability.CooldownRemaining, 0, "Ready CD preserved");
                    actor.Hit(1000, true); Check(!ability.TryActivate(Origin), "DOWN prevents abilities");
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    Near(loop.Player.Actor.Stats.ATK, 30, "RUN resets bonus"); Near(loop.PG07Ability.PassiveRolls, 0, "RUN resets diagnostics");
                    Check(!loop.PG07Ability.EffectActive, "RUN resets ability");
                }
                Check(loop.Player.Definition.PlayerId == "PG07", "Entire suite PG07 only");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(config); }
        }
        private static IEnumerator BaseChecks(PG07Passive passive)
        {
            var ability = loop.PG07Ability; var target = Probe(Origin + Vector2.right * 3, defence: 115);
            var behind = Probe(Origin + Vector2.right * 5); var ally = Probe(Origin + Vector2.right, Faction.PG);
            SeedRoll(passive == PG07Passive.LuckyShot ? .05f : .1f, false);
            var fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null; float at = Time.time;
            Check(!loop.Player.GetComponent<PlayerWeapon>().TryFireAt(Origin + Vector2.right * 20), "Cadence prevents immediate repeat");
            var wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 474, "Base HIT30 DEF15 final rounding26"); Near(behind.CurrentHP, 500, "Failed concentration/default shot not piercing"); Near(ally.CurrentHP, 100, "No friendly fire");
            Near(ability.PassiveRolls, 1, "Exactly one roll at valid trigger"); Near(ability.PassiveTriggers, 0, "Probability failure path");
            Remove(target, behind, ally); yield return null;
            int rolls = ability.PassiveRolls;
            fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            Near(Time.time - at, 100f / 110, "Actual cadence1.1/s", Mathf.Max(.05f, Time.deltaTime * 2));
            wait = Wait(.85f); while (wait.MoveNext()) yield return null;
            Near(ability.PassiveRolls - rolls, passive == PG07Passive.LuckyShot ? 0 : 1, "MISS rolls only CONCENTRAZIONE");
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "RANGE15 expires projectile");
            foreach (bool wall in new[] { true, false })
            {
                target = Probe(Origin + Vector2.right * 3); var blocker = Obstacle(Origin + Vector2.right * 2, Vector2.one * .5f, wall);
                fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
                wait = Wait(.3f); while (wait.MoveNext()) yield return null; Near(target.CurrentHP, 500, "MURO/OSTACOLO stops arrows");
                Remove(target); UnityEngine.Object.Destroy(blocker); yield return null;
            }
        }
        private static IEnumerator ConcentrationChecks()
        {
            var ability = loop.PG07Ability; var weapon = loop.Player.GetComponent<PlayerWeapon>();
            var stats = loop.Player.Actor.Stats; stats.ATK = 31; loop.Player.Actor.SetStats(stats);
            var extra = new ExtraHit(); weapon.BaseProjectileEffect = extra;
            var targets = new List<Combatant>(); for (int i = 0; i < 4; i++) targets.Add(Probe(Origin + Vector2.right * (2 + i), defence: 115));
            while (!weapon.TryFireAt(Origin + Vector2.up * 20)) yield return null;
            var wait = Wait(1); while (wait.MoveNext()) yield return null;
            SeedRoll(.1f, true); var fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            wait = Wait(.4f); while (wait.MoveNext()) yield return null;
            Near(targets[0].CurrentHP, 474, "Concentration first HIT100 percent before DEF");
            Near(targets[1].CurrentHP, 486, "Second HIT31/2 rounded16 then DEF gives14"); Near(targets[2].CurrentHP, 486, "Third HIT50 percent without further reduction");
            Near(targets[3].CurrentHP, 500, "Fourth MOB untouched"); Near(extra.Count, 3, "Additional HIT effects propagate to all three MOB");
            Check(ability.PassiveTriggers >= 1, "Concentration success path10 percent");
            weapon.BaseProjectileEffect = null; stats.ATK = 30; loop.Player.Actor.SetStats(stats); Remove(targets.ToArray()); yield return null;
        }
        private static IEnumerator LuckyChecks()
        {
            var ability = loop.PG07Ability;
            foreach (bool lethal in new[] { false, true })
            {
                Vector2 center = Origin + Vector2.right * 3;
                var target = Probe(center); if (lethal) HP(target, 1);
                var near = Probe(center + Vector2.up, defence: 115); var outside = Probe(center + Vector2.up * 2.01f);
                int rolls = ability.PassiveRolls, triggers = ability.PassiveTriggers;
                var weapon = loop.Player.GetComponent<PlayerWeapon>();
                var fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null; SeedRoll(.05f, true);
                var wait = Wait(.3f); while (wait.MoveNext()) yield return null;
                Near(near.CurrentHP, 492, "Lucky30 percent ATK30=9 then DEF gives8"); Near(outside.CurrentHP, 500, "Lucky radius2 excludes outside");
                Near(ability.PassiveRolls - rolls, 1, "Lucky area does not recursively roll"); Near(ability.PassiveTriggers - triggers, 1, "Lucky success5 percent including lethal base HIT");
                if (!lethal) Near(target.CurrentHP, 461, "Surviving trigger receives separate base and area HIT");
                Remove(target, near, outside); yield return null;
            }
            Vector2 impact = Origin + Vector2.right * 3;
            var mob = Probe(impact); var screened = Probe(impact + new Vector2(.2f, .2f));
            var blocker = Obstacle(impact + new Vector2(1, 1), Vector2.one * .3f, false);
            var shot = Fire(Origin + Vector2.right * 20); while (shot.MoveNext()) yield return null; SeedRoll(.05f, true);
            var delay = Wait(.3f); while (delay.MoveNext()) yield return null;
            Near(screened.CurrentHP, 500, "Lucky removes full blocked quadrant even for otherwise clear target");
            Remove(mob, screened); UnityEngine.Object.Destroy(blocker); yield return null;
        }
        private static IEnumerator MultiChecks()
        {
            var ability = loop.PG07Ability; var shots = new List<Projectile>();
            Action<Projectile> record = p => shots.Add(p); ability.MultiShot += record;
            Vector2 muzzle = loop.Player.GetComponent<PlayerWeapon>().Muzzle.position;
            var target = Probe(muzzle + Vector2.right * 3); Vector2 before = target.transform.position;
            Check(ability.TryActivate(muzzle + Vector2.right * 20), "Activate MULTI SHOT"); ability.MultiShot -= record;
            Near(shots.Count, 9, "Nine simultaneous projectiles"); Near(ability.CooldownRemaining, 1, "CD10 starts at activation with reduction");
            for (int i = 0; i < 9; i++) { Near(Vector2.Distance(shots[i].Direction, AttackGeometry.Direction(-40 + i * 10)), 0, "Exact ten degree fan and centered cursor"); Near(shots[i].Speed, 20, "Projectile speed20"); }
            var wait = Wait(.25f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 480, "Multi fixed damage20"); Near(Vector2.Distance(before, target.transform.position), 5, "Multi pushes5m");
            Remove(target); yield return null;
            wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "Multi range7 expires remaining shots");
            while (ability.CooldownRemaining > 0) yield return null;
            target = Probe(muzzle + Vector2.right * 3); before = target.transform.position;
            var wall = Obstacle(muzzle + Vector2.right * 4, new Vector2(.2f, 4), true);
            Check(ability.TryActivate(muzzle + Vector2.right * 20), "Multi toward push blocker");
            wait = Wait(.25f); while (wait.MoveNext()) yield return null;
            Check(target.transform.position.x > before.x && target.transform.position.x < muzzle.x + 4, "MURO interrupts push before5m");
            Remove(target); UnityEngine.Object.Destroy(wall); yield return null;
            // Drive separate physical projectiles into a large common collider at equal travel time.
            var go = new GameObject("Simultaneous volley test"); go.transform.SetParent(TestVisuals.Root);
            var volley = go.AddComponent<PG07MultiVolley>(); volley.Initialize(5);
            target = Probe(Origin + Vector2.up * 20); target.GetComponent<CircleCollider2D>().radius = 1;
            before = target.transform.position;
            foreach (float angle in new[] { -20f, 20f })
            {
                Vector2 dir = AttackGeometry.Direction(angle);
                var p = TestVisuals.SpawnProjectile(loop.Player.Actor, before - dir * 2, dir, 20, 7, 20, .06f, 0, false); p.HitEffect = volley.ForShot(p);
            }
            wait = Wait(.15f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 460, "Simultaneous projectiles apply separate HITs"); Near(volley.Pushes, 1, "Maximum one push per MOB per volley");
            Near(target.transform.position.x - before.x, 5, "Simultaneous directions averaged", .02f); Near(target.transform.position.y, before.y, "Average direction cancels vertical components", .02f);
            Remove(target); UnityEngine.Object.Destroy(go); yield return null;
        }
        private static IEnumerator RainChecks()
        {
            var ability = loop.PG07Ability; Vector2 center = Origin + Vector2.up * 40;
            var times = new List<float>(); var points = new List<Vector2>(); float start = Time.time;
            Action<Vector2, float> record = (point, scheduled) => { points.Add(point); times.Add(Time.time - start); Near(scheduled, (points.Count - 1) * 3f / 19, "Rain scheduled regular interval"); };
            ability.RainImpact += record; Check(ability.TryActivate(center), "Rain unlimited activation range"); Near(points.Count, 1, "First rain arrow at T0");
            Near(ability.CooldownRemaining, 1.5f, "CD15 starts at activation");
            Time.timeScale = 0; double pause = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
            Near(points.Count, 1, "Pause suspends rain"); Time.timeScale = 1;
            while (ability.EffectActive) yield return null; ability.RainImpact -= record;
            Near(points.Count, 20, "Exactly20 arrows"); Near(times[19], 3, "Last arrow at3s", Mathf.Max(.05f, Time.deltaTime * 2));
            foreach (var point in points) Check(Vector2.Distance(point, center) <= 3.501f, "Every point within radius3.5m");
            Check(points[0] != points[1], "Positions randomized");
            Check(ability.HitRainPoint(center) == null, "Empty impact is MISS");
            var a = Probe(center, defence: 115); var b = Probe(center, defence: 115); var ally = Probe(center, Faction.PG);
            var wall = Obstacle(center, Vector2.one, true);
            var seen = new HashSet<Combatant>();
            for (int i = 0; i < 20; i++) seen.Add(ability.HitRainPoint(center));
            Near(seen.Count, 2, "Overlapping MOB selection randomized"); Near(a.CurrentHP + b.CurrentHP, 820, "One HIT10 with DEF per arrow, no area splash"); Near(ally.CurrentHP, 100, "Rain excludes allies");
            Check(ability.HitRainPoint(center + Vector2.up * .36f) == null, "Impact outside body is MISS despite nearby MOB");
            Remove(a, b, ally); UnityEngine.Object.Destroy(wall); yield return null;
        }
    }
}
