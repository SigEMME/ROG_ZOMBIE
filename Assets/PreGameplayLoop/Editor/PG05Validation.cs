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
    public static class PG05Validation
    {
        private const string Key = "ROG.PG05.";
        private static readonly string Request = Path.GetFullPath(".pg05-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG05Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG05 tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
            // Select PG05 in memory for this suite only, preserving the user's Inspector choice.
            var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
            SessionState.SetInt(Key + "PreviousPlayer", (int)config.SelectedPlayer);
            SessionState.SetBool(Key + "RestoreSelection", true);
            config.SelectedPlayer = LoopPlayer.PG05;
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
                    var input = loop.Player.GetComponent<PG05AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. PG05 only, four combinations: actual shots, abilities, passives and AREA transitions. Input feel/balancing are not tested.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG05-validation.txt"));
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
        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            Check(loop.Player.Definition.PlayerId == "PG05", "Suite starts with PG05 only");
            var original = loop.Definition; var config = UnityEngine.Object.Instantiate(original); loop.Definition = config;
            try
            {
                foreach (PG05Ability choice in Enum.GetValues(typeof(PG05Ability)))
                foreach (PG05Passive passive in Enum.GetValues(typeof(PG05Passive)))
                {
                    config.SelectedPlayer = LoopPlayer.PG05; config.SelectedPG05Ability = choice; config.SelectedPG05Passive = passive;
                    Check(config.Validate() == null, "Valid combination " + choice + "/" + passive);
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor; var ability = loop.PG05Ability;
                    loop.Player.transform.SetPositionAndRotation(Origin, Quaternion.identity);
                    Check(loop.Ability == null && loop.PG02Ability == null && loop.PG03Ability == null && loop.PG04Ability == null, "No other PG runtime");
                    Near(actor.Stats.HP, 100, "PG05 HP"); Near(actor.Stats.ATK, 30, "PG05 ATK"); Near(actor.Stats.DEF, 90, "PG05 DEF -10 percent");
                    Near(actor.Stats.AttackSpeed, 150, "PG05 cadence"); Near(actor.MovementMetresPerSecond, 2.6f, "PG05 MOVE SPD 130");
                    Near(actor.Stats.RangeMetres, 2.5f, "PG05 RANGE 2.5m");
                    Near(ability.CooldownRemaining, choice == PG05Ability.Invisibilita ? 15 : 10, "Initial full CD", .15f);
                    Check(!ability.TryActivate(Origin), "Not ready on RUN start");
                    config.SelectedPG05Passive = passive == PG05Passive.Ghosting ? PG05Passive.LamaDiCicuta : PG05Passive.Ghosting;
                    Check(ability.Passive == passive, "Selection snapshot survives configuration changes"); config.SelectedPG05Passive = passive;
                    var test = BaseChecks(); while (test.MoveNext()) yield return null;
                    if (passive == PG05Passive.Ghosting) { test = GhostChecks(); while (test.MoveNext()) yield return null; }
                    else { test = CicutaChecks(); while (test.MoveNext()) yield return null; }
                    test = PoisonChecks(); while (test.MoveNext()) yield return null;
                    while (ability.CooldownRemaining > 0) yield return null;
                    ReducedCD();
                    test = choice == PG05Ability.Invisibilita ? InvisibleChecks() : KnifeChecks(); while (test.MoveNext()) yield return null;
                    while (ability.CooldownRemaining > 0) yield return null;
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.IsActive && mob.Faction == Faction.MOB) mob.Die();
                        yield return null;
                    }
                    int count = ability.CicutaHits;
                    Check(ability.TryActivate(Origin + Vector2.up * 20), "Activate before AREA");
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    float cd = ability.CooldownRemaining;
                    double pause = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
                    Near(ability.CooldownRemaining, cd, "BONUS pause stops cooldown");
                    loop.Choices[0] = AreaStat.ATK; loop.SelectBonus(0); loop.ConfirmBonus();
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.PG05Ability == ability, "AREA keeps PG05 runtime and selections");
                    Near(ability.CicutaHits, count, "CICUTA counter persists across AREA"); Check(!actor.IsInvisible, "AREA ends invisibility");
                    Near(actor.Stats.ATK, 31.5f, "AREA preserves ATK bonus");
                    Check(UnityEngine.Object.FindObjectsByType<PG05Poison>(FindObjectsSortMode.None).Length == 0, "Old AREA poison removed");
                    while (ability.CooldownRemaining > 0) yield return null;
                    ability.ChangeArea(); Near(ability.CooldownRemaining, 0, "Ready CD preserved");
                    actor.Hit(1000, true); Check(!ability.TryActivate(Origin), "DOWN blocks ability");
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    Near(loop.Player.Actor.Stats.ATK, 30, "RUN resets bonus"); Near(loop.PG05Ability.CicutaHits, 0, "RUN resets counter");
                    Check(!loop.Player.Actor.IsInvisible, "RUN resets invisibility");
                }
                Check(loop.Player.Definition.PlayerId == "PG05", "Entire suite PG05 only");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(config); }
        }
        private static IEnumerator BaseChecks()
        {
            var actor = loop.Player.Actor;
            var a = Probe(Origin + Vector2.right); var edge = Probe(Origin + Vector2.right * 2.5f);
            var outside = Probe(Origin + Vector2.right * 2.51f); var behind = Probe(Origin + Vector2.left);
            var angle = Probe(Origin + AttackGeometry.Direction(67)); var beyondAngle = Probe(Origin + AttackGeometry.Direction(68));
            var ally = Probe(Origin + Vector2.right, Faction.PG);
            var firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
            Near(a.CurrentHP, 470, "Base actual damage"); Near(edge.CurrentHP, 470, "Radial edge included"); Near(angle.CurrentHP, 470, "Inside 135deg cone");
            Near(outside.CurrentHP, 500, "Outside RANGE excluded"); Near(behind.CurrentHP, 500, "Rear excluded"); Near(beyondAngle.CurrentHP, 500, "Outside angle excluded"); Near(ally.CurrentHP, 500, "No friendly fire");
            Check(!loop.Player.GetComponent<PlayerWeapon>().TryFireAt(Origin + Vector2.right), "Cadence blocks immediate repeat");
            float first = Time.time;
            Remove(a, edge, outside, behind, angle, beyondAngle, ally); yield return null;
            firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
            Near(Time.time - first, 100f / 150, "Actual attack interval", Mathf.Max(.05f, Time.deltaTime * 2));
            foreach (bool wall in new[] { true, false })
            {
                a = Probe(Origin + Vector2.right * 1.5f); var blocker = Obstacle(Origin + Vector2.right * .7f, new Vector2(.2f, 1), wall);
                firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
                Near(a.CurrentHP, 500, "MURO/OSTACOLO screens target"); Remove(a); UnityEngine.Object.Destroy(blocker); yield return null;
            }
        }
        private static IEnumerator GhostChecks()
        {
            var a = loop.Player.Actor;
            HP(a, 36); a.Hit(1, true); Check(!a.IsInvisible, "HP exactly 35 after HIT does not trigger GHOSTING");
            HP(a, 36); a.Hit(2, true); Check(a.IsInvisible, "HIT crossing from above to below 35 triggers GHOSTING");
            var effect = a.GetComponent<PG05Invisibility>(); Near(effect.Remaining, 2, "GHOSTING lasts 2s");
            Near(a.MovementMetresPerSecond, 2.99f, "Ghost MOVE +15 percent");
            var wait = Wait(.25f); while (wait.MoveNext()) yield return null;
            float before = effect.Remaining; a.Hit(1, true); Near(effect.Remaining, before, "Further HIT does not refresh GHOSTING");
            var stats = a.Stats; stats.MoveSpeed = 140; a.SetStats(stats); Near(a.MovementMetresPerSecond, 3.22f, "New permanent MOVE bonus remains independent");
            wait = Wait(2); while (wait.MoveNext()) yield return null;
            Check(!a.IsInvisible, "GHOSTING expires"); Near(a.MovementMetresPerSecond, 2.8f, "Expiry preserves permanent MOVE bonus");
            a.Hit(1, true); Check(a.IsInvisible, "No internal cooldown after expiry");
            a.NotifyAction(); Check(!a.IsInvisible, "Action cancels GHOSTING and speed");
            stats.MoveSpeed = 130; a.SetStats(stats); HP(a, 100);
        }
        private static IEnumerator CicutaChecks()
        {
            var a = loop.PG05Ability; var probes = new List<Combatant>();
            for (int i = 0; i < 16; i++) probes.Add(Probe(Origin + Vector2.right));
            var firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
            Near(a.CicutaHits, 15, "Multiple MOB per attack reaches cap, overflow discarded");
            Check(probes[0].GetComponent<PG05Poison>() == null, "Threshold attack does not poison");
            firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
            foreach (var p in probes) Check(p.GetComponent<PG05Poison>() != null, "Next attack poisons every target");
            Near(a.CicutaHits, 0, "Empowered attack resets without recounting its hits");
            Remove(probes.ToArray()); yield return null;
            var target = Probe(Origin + Vector2.right);
            firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
            Near(a.CicutaHits, 1, "New counter cycle"); Remove(target); yield return null;
        }
        private static IEnumerator PoisonChecks()
        {
            var target = Probe(Origin + Vector2.up * 5, defence: 120);
            var poison = target.gameObject.AddComponent<PG05Poison>(); poison.Apply(loop, loop.Player.Actor, 5, 3);
            Near(target.CurrentHP, 500, "Poison has no immediate tick");
            var wait = Wait(1.05f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 496, "First tick after 1 second and DEF");
            for (int i = 0; i < 6; i++) poison.Apply(loop, loop.Player.Actor, 5, 3);
            Near(poison.Stacks, 5, "Poison cap five stacks"); Near(poison.Remaining, 3, "Reapply refreshes shared duration even at cap");
            wait = Wait(.6f); while (wait.MoveNext()) yield return null; Near(target.CurrentHP, 496, "Reapply resets tick clock");
            float remain = poison.Remaining; Time.timeScale = 0; double pause = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
            Near(poison.Remaining, remain, "Pause stops poison"); Time.timeScale = 1;
            wait = Wait(2.5f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 436, "Three five-stack ticks with DEF"); Check(poison == null, "Poison expires after shared duration");
            Remove(target); yield return null;
            target = Probe(Origin + Vector2.up * 5); HP(target, 5); int kills = 0;
            Action<Combatant> credit = _ => kills++; loop.Player.Actor.Killed += credit;
            target.gameObject.AddComponent<PG05Poison>().Apply(loop, loop.Player.Actor, 5, 3);
            wait = Wait(1.1f); while (wait.MoveNext()) yield return null;
            Near(kills, 1, "Poison lethal tick credits caster once"); loop.Player.Actor.Killed -= credit; Remove(target); yield return null;
        }
        private static IEnumerator InvisibleChecks()
        {
            var ability = loop.PG05Ability; var a = loop.Player.Actor;
            var ally = Probe(Origin + Vector2.up * 50, Faction.PG); var down = Probe(Origin + Vector2.up * 51, Faction.PG); down.Hit(1000, true);
            Check(ability.TryActivate(Origin), "Activate party invisibility"); Near(ability.CooldownRemaining, 1.5f, "CD15 with reduction starts at activation");
            Check(a.IsInvisible && ally.IsInvisible && !down.IsInvisible, "Party range unlimited; DOWN excluded");
            Near(a.GetComponent<PG05Invisibility>().Remaining, 3.5f, "Invisibility 3.5s");
            a.transform.position += Vector3.right; Check(a.IsInvisible, "Movement does not cancel invisibility"); a.transform.position = Origin;
            ally.NotifyAction(); Check(!ally.IsInvisible && a.IsInvisible, "Interruption is individual");
            var firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
            Check(!a.IsInvisible, "Base attack cancels invisibility");
            var effect = a.GetComponent<PG05Invisibility>(); effect.Apply(loop, 3.5f, 15); effect.Apply(loop, 2, 15);
            Near(effect.Remaining, 2, "Last invisibility effect replaces duration"); Near(a.MovementMetresPerSecond, 2.99f, "Overlap has one speed bonus");
            var mob = Probe(Origin + Vector2.right); var brain = mob.gameObject.AddComponent<MobBrain>();
            brain.Initialize(loop.Definition.ZOMB01, loop.Navigation); brain.enabled = false;
            typeof(MobBrain).GetMethod("ChooseTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(brain, null);
            var targetField = typeof(MobBrain).GetField("target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Check(targetField.GetValue(brain) == ally, "MOB chooses visible ally over nearest invisible PG05");
            effect.Cancel();
            typeof(MobBrain).GetMethod("ChooseTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(brain, null);
            Check(targetField.GetValue(brain) == a, "MOB reacquires PG05 after invisibility");
            Remove(ally, down, mob); yield return null;
        }
        private static IEnumerator KnifeChecks()
        {
            var ability = loop.PG05Ability; var probes = new List<Combatant>(); var shots = new List<Projectile>();
            for (int i = 0; i < 8; i++) probes.Add(Probe(Origin + AttackGeometry.Direction(i * 45) * 3));
            var behind = Probe(Origin + Vector2.right * 5);
            Action<Projectile> record = shot => shots.Add(shot); ability.KnifeFired += record;
            Check(ability.TryActivate(Origin + Vector2.right * 10), "Activate eight knives"); ability.KnifeFired -= record;
            Near(shots.Count, 8, "Eight simultaneous physical knives"); Near(ability.CooldownRemaining, 1, "Knife CD10 starts at activation");
            for (int i = 0; i < 8; i++) Near(Vector2.Distance(shots[i].Direction, AttackGeometry.Direction(i * 45)), 0, "45 degree distribution and cursor alignment");
            var wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            foreach (var p in probes) { Near(p.CurrentHP, 470, "Knife fixed damage30"); Check(p.GetComponent<PG05Poison>() != null, "Knife applies poison"); }
            Near(behind.CurrentHP, 500, "Knife is not piercing");
            Remove(probes.ToArray()); Remove(behind); yield return null;
            while (ability.CooldownRemaining > 0) yield return null;
            var blocker = Obstacle(Origin + Vector2.right, Vector2.one * .4f, true); var screened = Probe(Origin + Vector2.right * 3);
            Check(ability.TryActivate(Origin + Vector2.right * 10), "Fire toward MURO");
            wait = Wait(.7f); while (wait.MoveNext()) yield return null;
            Near(screened.CurrentHP, 500, "MURO stops knives");
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "Other knives expire at RANGE10");
            Remove(screened); UnityEngine.Object.Destroy(blocker); yield return null;
        }
    }
}
