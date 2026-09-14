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
    public static class PG02Validation
    {
        private const string Key = "ROG.PG02.";
        private static readonly string Request = Path.GetFullPath(".pg02-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG02Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG02 tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
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
                    var input = loop.Player.GetComponent<PG01AbilityInput>();
                    if (input != null) input.enabled = false;
                    var pg02Input = loop.Player.GetComponent<PG02AbilityInput>();
                    if (pg02Input != null) pg02Input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. PG02 four combinations: runtime firing, projectile HITs, abilities, personal KILLs, passives and AREA transitions. Input feel/balancing are not tested.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG02-validation.txt"));
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
            actor.Initialize(faction, new CombatStats { HP = 100, DEF = defence, ATK = 20, MoveSpeed = 100, Range = 400 });
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
        private static void Health(float hp) => loop.Player.Actor.SetStats(loop.Player.Actor.Stats, hp - loop.Player.Actor.CurrentHP);

        private static void CreditPersonalKills(int count)
        {
            var targets = new List<Combatant>();
            for (int i = 0; i < count; i++)
            {
                var target = Probe(Origin + new Vector2(2, (i % 5 - 2) * .1f));
                target.SetStats(new CombatStats { HP = 1, DEF = 100 });
                targets.Add(target);
            }
            int before = loop.PG02Passive.Kills;
            foreach (var target in targets) target.Hit(3, true, loop.Player.Actor);
            Near(loop.PG02Passive.Kills, before + count, "Sourced lethal HIT credits each MOB death exactly once");
            foreach (var target in targets)
            {
                target.Die(); // Must not credit the same corpse again.
                target.Hit(100, true, loop.Player.Actor);
                UnityEngine.Object.Destroy(target.gameObject);
            }
            Near(loop.PG02Passive.Kills, before + count, "Duplicate death/HIT gives no extra KILL");
        }

        private static IEnumerator Cadence(float attacksPerSecond)
        {
            var weapon = loop.Player.GetComponent<PlayerWeapon>();
            float previous = -1;
            int fired = 0;
            double began = EditorApplication.timeSinceStartup;
            while (fired < 6)
            {
                if (EditorApplication.timeSinceStartup - began >= 8) throw new TimeoutException("Cadence test stalled");
                if (weapon.TryFireAt(Origin + Vector2.right * 20))
                {
                    if (previous >= 0)
                    {
                        float interval = Time.time - previous;
                        Check(interval >= 1f / attacksPerSecond - .0001f &&
                            interval <= 1f / attacksPerSecond + Mathf.Max(.025f, Time.deltaTime * 2),
                            $"Actual projectile cadence {attacksPerSecond:0.##}/s: interval={interval:0.####}");
                    }
                    Check(!weapon.TryFireAt(Origin + Vector2.right * 20), "Same-frame repeat shot blocked");
                    previous = Time.time;
                    fired++;
                }
                yield return null;
            }
            foreach (var p in UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None)) UnityEngine.Object.Destroy(p.gameObject);
            yield return null;
        }

        private static IEnumerator SuppressionChecks()
        {
            var ability = loop.PG02Ability;
            var actor = loop.Player.Actor;
            var weapon = loop.Player.GetComponent<PlayerWeapon>();
            var data = loop.Definition.PG02Abilities;
            var times = new List<float>();
            int[] sequence = { 0, 1, 2, 3, 4, 3, 2, 1 };
            float facing = 37;
            System.Action<Projectile, int> observe = (projectile, index) =>
            {
                times.Add(Time.time);
                Vector2 expected = AttackGeometry.Direction(facing + (sequence[index % 8] - 2) * 10);
                Check(Vector2.Distance(projectile.Direction, expected) < .001f, "Sector centre direction for shot " + (index + 1));
                Near(Vector2.Distance(projectile.transform.position, weapon.Muzzle.position), 0, "Spawn at current muzzle");
                Near(projectile.Speed, 20, "Suppression uses weapon projectile speed");
            };
            ability.SuppressionShot += observe;
            Check(ability.TryActivate((Vector2)weapon.Muzzle.position + AttackGeometry.Direction(facing) * 20), "Physical burst activation");
            Near(ability.ActiveRemaining, 3, "Burst duration 3 seconds");
            Near(ability.CooldownRemaining, 12, "Suppression CD begins at activation");
            Near(actor.EffectiveStats.AttackSpeed, 400, "Suppression does not apply Rapid modifier");
            Check(!ability.TryActivate(Origin), "Active burst cannot retrigger");
            while (times.Count < 10) yield return null;
            float pausedDuration = ability.ActiveRemaining;
            float pausedCooldown = ability.CooldownRemaining;
            int pausedShots = times.Count;
            Time.timeScale = 0;
            double pause = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
            Near(times.Count, pausedShots, "Pause stops burst emissions");
            Near(ability.ActiveRemaining, pausedDuration, "Pause freezes burst duration");
            Near(ability.CooldownRemaining, pausedCooldown, "Pause freezes burst cooldown");
            Time.timeScale = 1;
            // New shots use the current aim; already emitted projectiles keep their heading.
            var flying = UnityEngine.Object.FindFirstObjectByType<Projectile>();
            Vector2 oldDirection = flying.Direction;
            facing = 90;
            ability.SetAimPoint((Vector2)weapon.Muzzle.position + Vector2.up * 20);
            yield return null;
            if (flying != null) Near(Vector2.Distance(flying.Direction, oldDirection), 0, "Emitted projectile does not follow cursor");
            while (ability.EffectActive) yield return null;
            ability.SuppressionShot -= observe;
            Near(times.Count, 50, "Exactly 50 physical projectiles");
            Near(times[49] - times[0], 3, "First-to-last emission spans 3 seconds", Mathf.Max(.035f, Time.deltaTime * 2));
            for (int i = 1; i < times.Count; i++)
                Near(times[i] - times[0], i * 3f / 49, "Uniform scheduled emission " + i, Mathf.Max(.035f, Time.deltaTime * 2));
            Near(ability.CooldownRemaining, 9, "Cooldown continued during burst", .06f);
            float end = Time.time + .6f; while (Time.time < end) yield return null;
            Near(ability.ShotsFired, 50, "No extra shots after duration");
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "All burst projectiles expire at range");
            foreach (float defence in new[] { 100f, 115f, 190f })
            {
                var first = Probe(Origin + Vector2.right * 3, defence: defence);
                var behind = Probe(Origin + Vector2.right * 5);
                var ally = Probe(Origin + Vector2.right, Faction.PG);
                var shot = SuppressionEffect.Fire(actor, Origin, Vector2.right, data, weapon.Definition, 2);
                Near(first.CurrentHP, 100, "Projectile damage is not instant");
                end = Time.time + .3f; while (Time.time < end) yield return null;
                Near(first.CurrentHP, defence == 190 ? 100 : 97, "Three fixed damage after DEF and final rounding " + defence);
                Near(behind.CurrentHP, 100, "Suppression cannot pierce first MOB");
                Near(ally.CurrentHP, 100, "Suppression passes through allied PG");
                UnityEngine.Object.Destroy(first.gameObject); UnityEngine.Object.Destroy(behind.gameObject); UnityEngine.Object.Destroy(ally.gameObject);
                yield return null;
            }
            foreach (bool wall in new[] { true, false })
            {
                var blocker = Obstacle(Origin + Vector2.right * 2, new Vector2(.3f, 2), wall);
                var target = Probe(Origin + Vector2.right * 3);
                SuppressionEffect.Fire(actor, Origin, Vector2.right, data, weapon.Definition, 2);
                end = Time.time + .3f; while (Time.time < end) yield return null;
                Near(target.CurrentHP, 100, "Suppression blocked by " + (wall ? "MURO" : "OSTACOLO"));
                UnityEngine.Object.Destroy(blocker); UnityEngine.Object.Destroy(target.gameObject); yield return null;
            }
            var distant = Probe(Origin + Vector2.right * 11);
            SuppressionEffect.Fire(actor, Origin, Vector2.right, data, weapon.Definition, 2);
            end = Time.time + .6f; while (Time.time < end) yield return null;
            Near(distant.CurrentHP, 100, "Suppression range remains 10m");
            UnityEngine.Object.Destroy(distant.gameObject);
            if (loop.PG02Passive.Selected == PG02Passive.Passiva2)
            {
                CreditPersonalKills(15 - loop.PG02Passive.KillsTowardRage);
                Check(loop.PG02Passive.RageActive, "RAGE active before fixed damage test");
            }
            int credited = loop.PG02Passive.Kills;
            var victim = Probe(Origin + Vector2.right * 3); victim.SetStats(new CombatStats { HP = 3, DEF = 100 });
            SuppressionEffect.Fire(actor, Origin, Vector2.right, data, weapon.Definition, 2);
            var fixedTarget = Probe(Origin + Vector2.up * 3);
            SuppressionEffect.Fire(actor, Origin, Vector2.up, data, weapon.Definition, 2);
            end = Time.time + .3f; while (Time.time < end) yield return null;
            Near(loop.PG02Passive.Kills, credited + 1, "Physical suppression kill attributed to PG02");
            Near(fixedTarget.CurrentHP, 97, "Fixed damage remains 3 including during RAGE");
            SuppressionEffect.Fire(actor, Origin, Vector2.up, data, weapon.Definition, 2);
            end = Time.time + .3f; while (Time.time < end) yield return null;
            Near(fixedTarget.CurrentHP, 94, "Same MOB can receive separate projectile HITs");
            UnityEngine.Object.Destroy(victim.gameObject); UnityEngine.Object.Destroy(fixedTarget.gameObject); yield return null;
        }

        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            var original = loop.Definition;
            var definition = UnityEngine.Object.Instantiate(original);
            loop.Definition = definition;
            definition.SelectedPlayer = LoopPlayer.PG02;
            definition.SelectedPG02Ability = (PG02Ability)999;
            Check(definition.Validate() != null, "Invalid PG02 ability rejected");
            definition.SelectedPG02Ability = PG02Ability.FuocoRapido;
            definition.SelectedPG02Passive = (PG02Passive)999;
            Check(definition.Validate() != null, "Invalid PG02 passive rejected");
            try
            {
                foreach (PG02Ability choice in new[] { PG02Ability.FuocoRapido, PG02Ability.FuocoDiSoppressione })
                foreach (PG02Passive passiveChoice in new[] { PG02Passive.Passiva1, PG02Passive.Passiva2 })
                {
                    definition.SelectedPlayer = LoopPlayer.PG02;
                    definition.SelectedPG02Ability = choice;
                    definition.SelectedPG02Passive = passiveChoice;
                    Check(definition.Validate() == null, "Valid combination " + choice + " / " + passiveChoice);
                    loop.RestartTest();
                    while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor;
                    var baseline = actor.Stats;
                    var ability = loop.PG02Ability;
                    var passive = loop.PG02Passive;
                    loop.Player.transform.SetPositionAndRotation(Origin, Quaternion.identity);
                    Check(loop.Player.Definition.PlayerId == "PG02" && loop.Ability == null && loop.Passive == null,
                        "Exactly PG02 runtime, no PG01 ability or passive attached");
                    Near(actor.Stats.HP, 100, "PG02 HP 100"); Near(actor.Stats.ATK, 10, "PG02 ATK 10");
                    Near(actor.Stats.DEF, 100, "PG02 internal DEF 100 = zero mitigation");
                    Near(actor.MovementMetresPerSecond, 2, "PG02 MOVE SPD 100 = 2m/s");
                    Near(actor.Stats.AttackSpeed, 400, "PG02 base ATK SPD 400"); Near(actor.Stats.RangeMetres, 10, "PG02 RANGE 10m");
                    Near(loop.CdReduction, 100, "Initial CD REDUCTION 100");
                    Check(!ability.TryActivate(Origin + Vector2.right), "Ability begins RUN unavailable");
                    Check(ability.CooldownRemaining > (choice == PG02Ability.FuocoRapido ? 9 : 11), "Full initial ability CD");
                    Near(passive.Kills, 0, "New RUN has no personal KILLs");
                    definition.SelectedPlayer = LoopPlayer.PG01;
                    Check(loop.Player.Definition.PlayerId == "PG02", "Inspector PG change does not alter current RUN");
                    definition.SelectedPlayer = LoopPlayer.PG02;
                    definition.SelectedPG02Ability = choice == PG02Ability.FuocoRapido ? PG02Ability.FuocoDiSoppressione : PG02Ability.FuocoRapido;
                    definition.SelectedPG02Passive = passiveChoice == PG02Passive.Passiva1 ? PG02Passive.Passiva2 : PG02Passive.Passiva1;
                    Check(ability.Selected == choice && passive.Selected == passiveChoice, "Ability/passive are RUN snapshots");
                    definition.SelectedPG02Ability = choice;
                    definition.SelectedPG02Passive = passiveChoice;

                    if (passiveChoice == PG02Passive.Passiva1)
                    {
                        foreach (float hp in new[] { 31f, 30f, 29f })
                        {
                            Health(hp); CreditPersonalKills(1);
                            Near(actor.CurrentHP, hp < 30 ? 30 : hp, "Heal threshold at " + hp);
                        }
                        Health(28.5f); CreditPersonalKills(3);
                        Near(actor.CurrentHP, 30.5f, "Multi-KILL reevaluates threshold after each 1 percent max HP heal");
                        var bigger = baseline; bigger.HP = 200; actor.SetStats(bigger); Health(58);
                        CreditPersonalKills(2);
                        Near(actor.CurrentHP, 60, "Modified max HP: heal 2, next KILL at 30 percent does not heal");
                        actor.SetStats(baseline); Health(100);
                    }
                    else
                    {
                        CreditPersonalKills(14); Check(!passive.RageActive, "14 KILLs do not trigger RAGE");
                        CreditPersonalKills(1); Near(passive.Kills, 15, "15 personal KILL threshold");
                        Near(passive.RageRemaining, 4, "RAGE starts at 4 seconds"); Near(actor.EffectiveStats.ATK, 13, "RAGE +30 percent ATK");
                        float cd = 100; AreaStatBonus.Apply(actor, AreaStat.ATK, ref cd);
                        Near(actor.Stats.ATK, 10.5f, "RAGE does not contaminate persistent ATK bonus");
                        Near(actor.EffectiveStats.ATK, 13.65f, "RAGE applies to current ATK after bonus");
                        float wait = Time.time + .4f; while (Time.time < wait) yield return null;
                        CreditPersonalKills(15); Near(passive.Kills, 30, "30 KILL threshold");
                        Near(passive.RageRemaining, 4, "New RAGE refreshes full duration"); Near(actor.EffectiveStats.ATK, 13.65f, "RAGE refresh never stacks");
                        Time.timeScale = 0; double paused = EditorApplication.timeSinceStartup;
                        while (EditorApplication.timeSinceStartup - paused < .15) yield return null;
                        Near(passive.RageRemaining, 4, "Pause freezes RAGE"); Time.timeScale = 1;
                        wait = Time.time + 4.05f; while (Time.time < wait) yield return null;
                        Near(passive.RageRemaining, 0, "RAGE expires after four seconds"); Near(actor.EffectiveStats.ATK, 10.5f, "RAGE expiry preserves acquired bonus");
                        actor.SetStats(baseline);
                    }
                    yield return null;
                    int credited = passive.Kills;
                    var uncredited = Probe(Origin + Vector2.down * 3); uncredited.Die();
                    var noSource = Probe(Origin + Vector2.down * 4); noSource.Hit(1000);
                    Near(passive.Kills, credited, "Test deaths and sourceless damage give no personal credit");
                    var otherPG = Probe(Origin + Vector2.down * 6, Faction.PG);
                    var otherVictim = Probe(Origin + Vector2.down * 7);
                    otherVictim.Hit(1000, true, otherPG);
                    Near(passive.Kills, credited, "Another PG source gives no credit to PG02");
                    UnityEngine.Object.Destroy(otherPG.gameObject); UnityEngine.Object.Destroy(otherVictim.gameObject);
                    UnityEngine.Object.Destroy(uncredited.gameObject); UnityEngine.Object.Destroy(noSource.gameObject);
                    yield return null;

                    var cadence = Cadence(4); while (cadence.MoveNext()) yield return null;
                    // Actual projectile trajectory, ally exclusion and first valid MOB stop.
                    var front = Probe(Origin + Vector2.right * 3);
                    var behind = Probe(Origin + Vector2.right * 5);
                    var ally = Probe(Origin + Vector2.right * 1.5f, Faction.PG);
                    var weapon = loop.Player.GetComponent<PlayerWeapon>();
                    while (!weapon.TryFireAt(Origin + Vector2.right * 20)) yield return null;
                    var shot = UnityEngine.Object.FindFirstObjectByType<Projectile>();
                    Near(shot.Speed, 20, "Actual projectile speed 20m/s");
                    float until = Time.time + .3f; while (Time.time < until) yield return null;
                    Near(front.CurrentHP, 90, "Actual projectile HIT damage 10"); Near(behind.CurrentHP, 100, "Non-piercing projectile stops at first MOB");
                    Near(ally.CurrentHP, 100, "Projectile ignores PG ally");
                    UnityEngine.Object.Destroy(front.gameObject); UnityEngine.Object.Destroy(behind.gameObject); UnityEngine.Object.Destroy(ally.gameObject);
                    yield return null;
                    foreach (bool wall in new[] { true, false })
                    {
                        var blocker = Obstacle(Origin + Vector2.right * 2, new Vector2(.3f, 2), wall);
                        var shielded = Probe(Origin + Vector2.right * 3);
                        while (!weapon.TryFireAt(Origin + Vector2.right * 20)) yield return null;
                        until = Time.time + .25f; while (Time.time < until) yield return null;
                        Near(shielded.CurrentHP, 100, (wall ? "MURO" : "OSTACOLO") + " stops projectile");
                        UnityEngine.Object.Destroy(blocker); UnityEngine.Object.Destroy(shielded.gameObject); yield return null;
                    }
                    var outOfRange = Probe(Origin + Vector2.right * 11.5f);
                    while (!weapon.TryFireAt(Origin + Vector2.right * 20)) yield return null;
                    shot = UnityEngine.Object.FindFirstObjectByType<Projectile>();
                    float flightStart = Time.time;
                    until = flightStart + .2f; while (Time.time < until) yield return null;
                    Check(shot != null, "Projectile remains in flight before 10m");
                    Near(shot.Travelled, 20 * (Time.time - flightStart), "Actual travelled distance follows 20m/s", Mathf.Max(.1f, 20 * Time.deltaTime * 2));
                    until = flightStart + .55f; while (Time.time < until) yield return null;
                    Check(shot == null, "Projectile expires at 10m / 0.5s flight"); Near(outOfRange.CurrentHP, 100, "Beyond-range MOB untouched");
                    UnityEngine.Object.Destroy(outOfRange.gameObject); yield return null;
                    credited = passive.Kills;
                    var victim = Probe(Origin + Vector2.right * 3); victim.SetStats(new CombatStats { HP = 1, DEF = 100 });
                    while (!weapon.TryFireAt(Origin + Vector2.right * 20)) yield return null;
                    until = Time.time + .25f; while (Time.time < until) yield return null;
                    Near(passive.Kills, credited + 1, "Projectile retains PG02 identity through lethal HIT");
                    victim.Die(); Near(passive.Kills, credited + 1, "Projectile corpse gives no duplicate credit");
                    UnityEngine.Object.Destroy(victim.gameObject); yield return null;

                    while (ability.CooldownRemaining > 0) yield return null;
                    if (choice == PG02Ability.FuocoRapido)
                    {
                        Check(ability.TryActivate(Origin), "FUOCO RAPIDO activation");
                        float rapidStarted = Time.time;
                        Near(ability.ActiveRemaining, 4, "Rapid duration starts at 4s"); Near(ability.CooldownRemaining, 0, "Rapid CD does not start at activation");
                        Near(actor.EffectiveStats.AttackSpeed, 600, "400 x 1.50 = 600");
                        Check(!ability.TryActivate(Origin), "Rapid cannot stack/retrigger while active");
                        if (passiveChoice == PG02Passive.Passiva2)
                        {
                            CreditPersonalKills(15 - passive.KillsTowardRage);
                            Near(actor.EffectiveStats.ATK, 13, "RAGE and Rapid coexist: ATK 13");
                            Near(actor.EffectiveStats.AttackSpeed, 600, "RAGE and Rapid coexist: ATK SPD 600");
                            var rageTarget = Probe(Origin + Vector2.right * 3);
                            while (!weapon.TryFireAt(Origin + Vector2.right * 20)) yield return null;
                            until = Time.time + .2f; while (Time.time < until) yield return null;
                            Near(rageTarget.CurrentHP, 87, "RAGE changes actual projectile damage to 13");
                            UnityEngine.Object.Destroy(rageTarget.gameObject); yield return null;
                        }
                        cadence = Cadence(6f); while (cadence.MoveNext()) yield return null;
                        float cd = 100; AreaStatBonus.Apply(actor, AreaStat.AttackSpeed, ref cd);
                        Near(actor.Stats.AttackSpeed, 420, "ATK SPD bonus uses persistent 400");
                        Near(actor.EffectiveStats.AttackSpeed, 630, "Rapid retains 50 percent on modified current speed");
                        float remaining = ability.ActiveRemaining;
                        Time.timeScale = 0; double paused = EditorApplication.timeSinceStartup;
                        while (EditorApplication.timeSinceStartup - paused < .15) yield return null;
                        Near(ability.ActiveRemaining, remaining, "Pause freezes Rapid duration"); Near(ability.CooldownRemaining, 0, "Pause does not begin Rapid CD");
                        Time.timeScale = 1;
                        while (ability.EffectActive) yield return null;
                        Near(Time.time - rapidStarted, 4, "Rapid lasts 4 simulation seconds", Mathf.Max(.025f, Time.deltaTime * 2));
                        Near(actor.EffectiveStats.AttackSpeed, 420, "Rapid expiry removes only temporary bonus");
                        Near(ability.CooldownRemaining, 10, "Rapid CD starts on expiry", .05f);
                        actor.SetStats(baseline);
                    }
                    else
                    {
                        var burst = SuppressionChecks(); while (burst.MoveNext()) yield return null;
                    }
                    // Shorten the final CD through the real reduction formula for transition checks.
                    typeof(LoopSession).GetField("cdReduction", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(loop, 10f);
                    while (ability.CooldownRemaining > 0) yield return null;
                    Check(ability.TryActivate(Origin + Vector2.right * 15), "Reuse ability after completed CD");
                    if (choice == PG02Ability.FuocoDiSoppressione)
                    {
                        Near(ability.CooldownRemaining, 1.2f, "Suppression applies CD REDUCTION 10");
                        while (ability.CooldownRemaining > 0) yield return null;
                        Check(ability.EffectActive && !ability.TryActivate(Origin), "Completed reduced cooldown cannot overlap active burst");
                    }
                    if (passiveChoice == PG02Passive.Passiva2)
                    {
                        CreditPersonalKills(15 - passive.KillsTowardRage);
                        CreditPersonalKills(14);
                        Check(passive.RageActive && passive.KillsTowardRage == 14, "Active RAGE with 14 kills toward next threshold");
                    }
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.Faction == Faction.MOB && mob.IsActive) mob.Die();
                        yield return null;
                    }
                    int areaKills = passive.Kills;
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    float pausedCd = ability.CooldownRemaining;
                    float pausedActive = ability.ActiveRemaining;
                    float pausedRage = passive.RageRemaining;
                    Check(pausedActive > 0, "Selected ability is still active at AREA exit");
                    if (passiveChoice == PG02Passive.Passiva2) Check(pausedRage > 0, "RAGE is still active at AREA exit");
                    double pauseBegan = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - pauseBegan < .15) yield return null;
                    Near(ability.CooldownRemaining, pausedCd, "BONUS freezes CD"); Near(ability.ActiveRemaining, pausedActive, "BONUS freezes ability duration");
                    Near(passive.RageRemaining, pausedRage, "BONUS freezes RAGE");
                    loop.Choices[0] = AreaStat.ATK; loop.SelectBonus(0); loop.ConfirmBonus();
                    Near(ability.CooldownRemaining, pausedActive > 0 ? (choice == PG02Ability.FuocoRapido ? 1 : 1.2f) : pausedCd,
                        "AREA active ability restarts full reduced CD; inactive retains residue");
                    Near(passive.RageRemaining, 0, "AREA ends RAGE immediately");
                    Check(!ability.EffectActive, "AREA cancels remaining burst/duration");
                    while (loop.State != LoopState.Combat) yield return null;
                    loop.Player.transform.position = Origin;
                    Check(loop.PG02Ability == ability && loop.PG02Passive == passive, "AREA preserves selected runtime instances");
                    Near(passive.Kills, areaKills, "AREA preserves personal KILL count; test deaths give no credit");
                    Near(actor.EffectiveStats.ATK, 10.5f, "AREA ends temporary ATK but preserves real bonus");
                    Near(actor.EffectiveStats.AttackSpeed, 400, "AREA removes Rapid modifier");
                    if (passiveChoice == PG02Passive.Passiva2)
                    {
                        Near(passive.KillsTowardRage, 14, "14 KILL progress survives AREA"); CreditPersonalKills(1);
                        Check(passive.RageActive, "One KILL in new AREA reaches retained 15 threshold");
                    }
                    while (ability.CooldownRemaining > 0) yield return null;
                    ability.ChangeArea(); Near(ability.CooldownRemaining, 0, "Available ability stays available on AREA change");
                }
                Check(loop.PG02Ability.TryActivate(Origin + Vector2.right * 20), "Start burst immediately before RUN reset");
                Check(loop.PG02Ability.EffectActive, "RUN reset begins with active burst");
                definition.SelectedPlayer = LoopPlayer.PG01;
                loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                Check(loop.Player.Definition.PlayerId == "PG01" && loop.PG02Ability == null && loop.PG02Passive == null, "Restart acquires PG01 and disposes PG02 state");
                Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "RUN reset removes in-flight burst projectiles");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(definition); }
        }
    }
}
