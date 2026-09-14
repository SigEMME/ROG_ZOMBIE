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
    public static class PG03Validation
    {
        private const string Key = "ROG.PG03.";
        private static readonly string Request = Path.GetFullPath(".pg03-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG03Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG03 tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
            // Select PG03 in memory for this suite only, preserving the user's Inspector choice.
            var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
            SessionState.SetInt(Key + "PreviousPlayer", (int)config.SelectedPlayer);
            SessionState.SetBool(Key + "RestoreSelection", true);
            config.SelectedPlayer = LoopPlayer.PG03;
            SessionState.SetBool(Key + "Running", true);
            EditorApplication.isPlaying = true;
        }

        private static void PlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            {
                Application.runInBackground = true; results.Clear(); warnings = 0; loop = null;
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
                if (loop != null && loop.BonusAbilities != null)
                {
                    loop.BonusAbilities.enabled = false;
                    if (loop.Experience.Choices != null) { loop.Experience.Choose(0); return; }
                }
                if (loop != null && loop.Player != null)
                {
                    var input = loop.Player.GetComponent<PG03AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. PG03 only, four combinations: actual shots, abilities, passives and AREA transitions. Input feel/balancing are not tested.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG03-validation.txt"));
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
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }
        private static IEnumerator FireAt(Vector2 point)
        {
            while (!loop.Player.GetComponent<PlayerWeapon>().TryFireAt(point)) yield return null;
        }
        private static void Remove(params Combatant[] actors)
        {
            foreach (var actor in actors) if (actor != null) UnityEngine.Object.Destroy(actor.gameObject);
        }
        private static void ReducedCD() => typeof(LoopSession).GetField("cdReduction",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(loop, 10f);

        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            Check(loop.Player.Definition.PlayerId == "PG03", "Only PG03 is instantiated at test startup");
            var original = loop.Definition;
            var definition = UnityEngine.Object.Instantiate(original);
            loop.Definition = definition;
            try
            {
                definition.SelectedPlayer = LoopPlayer.PG03;
                definition.SelectedPG03Ability = (PG03Ability)999;
                Check(definition.Validate() != null, "Invalid PG03 ability rejected");
                definition.SelectedPG03Ability = PG03Ability.ColpoLaser;
                definition.SelectedPG03Passive = (PG03Passive)999;
                Check(definition.Validate() != null, "Invalid PG03 passive rejected");
                foreach (PG03Ability choice in new[] { PG03Ability.ColpoLaser, PG03Ability.TriploSparo })
                foreach (PG03Passive passiveChoice in new[] { PG03Passive.CalibroPerforante, PG03Passive.PuntoDebole })
                {
                    definition.SelectedPG03Ability = choice; definition.SelectedPG03Passive = passiveChoice;
                    Check(definition.Validate() == null, "Valid PG03 combination " + choice + "/" + passiveChoice);
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor;
                    var ability = loop.PG03Ability;
                    var passive = loop.PG03Passive;
                    var weapon = loop.Player.GetComponent<PlayerWeapon>();
                    var baseline = actor.Stats;
                    // These tests provide their own blockers; CITY buildings must not overlap the fixture.
                    foreach(var obstacle in TestObstacle.All.ToArray()) obstacle.gameObject.SetActive(false);
                    loop.RefreshNavigation();
                    loop.Player.transform.SetPositionAndRotation(Origin, Quaternion.identity);
                    Check(loop.Ability == null && loop.PG02Ability == null && loop.Passive == null && loop.PG02Passive == null,
                        "PG03 runtime only, no PG01/PG02 runtime");
                    Near(actor.Stats.HP, 80, "PG03 HP 80"); Near(actor.Stats.ATK, 50, "PG03 ATK 50");
                    Near(actor.Stats.DEF, 90, "PG03 DEF -10 percent"); Near(actor.Stats.MoveSpeed, 110, "PG03 MOVE SPD 110");
                    Near(actor.MovementMetresPerSecond, 2.2f, "PG03 moves at 2.2 m/s");
                    Near(actor.Stats.AttackSpeed, 60, "PG03 ATK SPD 60"); Near(actor.Stats.RangeMetres, 20, "PG03 range 20m");
                    Check(!ability.EffectActive && !ability.TryActivate(Origin + Vector2.right), "Fresh RUN begins in full cooldown without active ability");
                    Near(ability.CooldownRemaining, choice == PG03Ability.ColpoLaser ? 18 : 10, "Full initial CD", .08f);
                    actor.Hit(10, true); Near(actor.CurrentHP, 69, "Negative DEF increases incoming 10 damage to 11");
                    actor.SetStats(baseline, 80);
                    definition.SelectedPG03Ability = choice == PG03Ability.ColpoLaser ? PG03Ability.TriploSparo : PG03Ability.ColpoLaser;
                    definition.SelectedPG03Passive = passiveChoice == PG03Passive.CalibroPerforante ? PG03Passive.PuntoDebole : PG03Passive.CalibroPerforante;
                    Check(ability.Selected == choice && passive.Selected == passiveChoice, "Current RUN preserves selected ability/passive snapshots");
                    definition.SelectedPG03Ability = choice; definition.SelectedPG03Passive = passiveChoice;
                    // Physical base attack: four collinear targets and an ally in front.
                    var targets = new Combatant[4];
                    for (int i = 0; i < 4; i++) targets[i] = Probe(Origin + Vector2.right * (2 + i * 2));
                    var ally = Probe(Origin + Vector2.right, Faction.PG);
                    var firing = FireAt(Origin + Vector2.right * 25); while (firing.MoveNext()) yield return null;
                    float firedAt = Time.time;
                    Check(!weapon.TryFireAt(Origin + Vector2.right * 25), "Same-frame base shot blocked by cadence");
                    var projectile = UnityEngine.Object.FindFirstObjectByType<Projectile>();
                    Near(projectile.Speed, 20, "Base physical speed 20m/s");
                    Near(targets[0].CurrentHP, 500, "Base attack has real travel time");
                    var wait = Wait(.5f); while (wait.MoveNext()) yield return null;
                    for (int i = 0; i < 4; i++)
                        Near(targets[i].CurrentHP, i == 0 ? 450 : passiveChoice == PG03Passive.CalibroPerforante && i < 3 ? (i == 1 ? 465 : 480) : 500,
                            "Base projectile penetration target " + (i + 1));
                    Near(ally.CurrentHP, 500, "Base projectile ignores allied PG");
                    if (passiveChoice == PG03Passive.CalibroPerforante)
                    {
                        // Repeat with a real persistent bonus and target mitigation; each shot resets its index.
                        float cd = 100; AreaStatBonus.Apply(actor, AreaStat.ATK, ref cd);
                        Near(actor.Stats.ATK, 52.5f, "Calibro uses current ATK after persistent bonus");
                        foreach (var target in targets) { var values = target.Stats; values.DEF = 115; target.SetStats(values); }
                        var dead = Probe(Origin + Vector2.right * 1.3f); dead.Die();
                        firing = FireAt(Origin + Vector2.right * 25); while (firing.MoveNext()) yield return null;
                        Near(Time.time - firedAt, 100f / 60, "Calibro actual cadence 0.6 attacks/s", Mathf.Max(.035f, Time.deltaTime * 2));
                        wait = Wait(.5f); while (wait.MoveNext()) yield return null;
                        float[] remainingHP = { 405, 434, 462, 500 };
                        for (int i = 0; i < 4; i++) Near(targets[i].CurrentHP, remainingHP[i],
                            "New projectile: current ATK times 100/70/40 percent then DEF, target " + (i + 1));
                        Near(ally.CurrentHP, 500, "Ally does not consume penetration damage index");
                        Remove(dead); actor.SetStats(baseline);
                    }
                    if (passiveChoice == PG03Passive.PuntoDebole)
                    {
                        Check(targets[0].GetComponent<WeakPointMark>().Active, "First base HIT applies MARCHIO");
                        Near(targets[0].Stats.DEF, 100, "MARCHIO preserves persistent DEF");
                        Near(targets[0].EffectiveStats.DEF, 80, "MARCHIO applies -20 DEF points");
                        firing = FireAt(Origin + Vector2.right * 25); while (firing.MoveNext()) yield return null;
                        Near(Time.time - firedAt, 100f / 60, "Actual base attack interval 100/60", Mathf.Max(.035f, Time.deltaTime * 2));
                        wait = Wait(.3f); while (wait.MoveNext()) yield return null;
                        Near(targets[0].CurrentHP, 390, "Second base HIT uses reduced DEF: damage 60");
                        Check(!targets[0].GetComponent<WeakPointMark>().Active, "Second HIT consumes without reapplying MARCHIO");
                        Near(targets[0].EffectiveStats.DEF, 100, "DEF restored after second HIT");
                        firing = FireAt(Origin + Vector2.right * 25); while (firing.MoveNext()) yield return null;
                        wait = Wait(.3f); while (wait.MoveNext()) yield return null;
                        Near(targets[0].CurrentHP, 340, "Third HIT uses normal DEF and reapplies MARCHIO");
                        var modified = targets[0].Stats; modified.DEF = 115; targets[0].SetStats(modified);
                        Near(targets[0].EffectiveStats.DEF, 95, "Persistent DEF change while marked remains independent");
                        targets[0].Hit(40, true, actor);
                        Near(targets[0].CurrentHP, 298, "Next non-base HIT uses marked DEF: 42 damage");
                        Near(targets[0].EffectiveStats.DEF, 115, "Consumption preserves changed persistent DEF");
                        Check(!targets[0].GetComponent<WeakPointMark>().Active, "Any next HIT consumes MARCHIO");
                    }
                    Remove(targets); Remove(ally); yield return null;
                    foreach (bool wall in new[] { true, false })
                    {
                        var blocker = Obstacle(Origin + Vector2.right * 2, new Vector2(.3f, 2), wall);
                        var protectedMob = Probe(Origin + Vector2.right * 4);
                        firing = FireAt(Origin + Vector2.right * 25); while (firing.MoveNext()) yield return null;
                        wait = Wait(.4f); while (wait.MoveNext()) yield return null;
                        Near(protectedMob.CurrentHP, 500, "Base attack stopped by " + (wall ? "MURO" : "OSTACOLO"));
                        Remove(protectedMob); UnityEngine.Object.Destroy(blocker); yield return null;
                    }
                    var distant = Probe(Origin + Vector2.right * 21.5f);
                    firing = FireAt(Origin + Vector2.right * 25); while (firing.MoveNext()) yield return null;
                    wait = Wait(1.1f); while (wait.MoveNext()) yield return null;
                    Near(distant.CurrentHP, 500, "Base projectile expires at range 20m"); Remove(distant); yield return null;
                    while (ability.CooldownRemaining > 0) yield return null;
                    ReducedCD();
                    // Raise ATK to demonstrate abilities use their own fixed damage.
                    float reduction = 10; AreaStatBonus.Apply(actor, AreaStat.ATK, ref reduction);
                    Near(actor.Stats.ATK, 52.5f, "Persistent ATK bonus 5 percent");
                    var abilityChecks = choice == PG03Ability.ColpoLaser ? LaserChecks() : TripleChecks();
                    while (abilityChecks.MoveNext()) yield return null;
                    actor.SetStats(baseline, 80);
                    while (ability.CooldownRemaining > 0) yield return null;
                    // Drain this AREA without instantiating another playable PG.
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.Faction == Faction.MOB && mob.IsActive) mob.Die();
                        yield return null;
                    }
                    Check(ability.TryActivate(Origin + Vector2.right * 25), "Activate PG03 ability before AREA transition");
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    float pausedCd = ability.CooldownRemaining;
                    bool wasActive = ability.EffectActive;
                    if (choice == PG03Ability.TriploSparo) Check(wasActive, "Triple burst active at BONUS entry");
                    int shots = ability.ShotsFired;
                    double paused = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - paused < .15) yield return null;
                    Near(ability.CooldownRemaining, pausedCd, "BONUS freezes PG03 cooldown");
                    Near(ability.ShotsFired, shots, "BONUS freezes pending triple shots");
                    loop.Choices[0] = AreaStat.ATK; loop.SelectBonus(0); loop.ConfirmBonus();
                    Near(ability.CooldownRemaining, wasActive ? 1 : pausedCd, "AREA active restarts cooldown; inactive preserves exact residue");
                    Check(!ability.EffectActive, "AREA cancels pending shots");
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.Player.Definition.PlayerId == "PG03" && loop.PG03Ability == ability && loop.PG03Passive == passive,
                        "AREA retains PG03 and selected ability/passive instances");
                    Near(actor.Stats.ATK, 52.5f, "AREA preserves acquired bonus");
                    Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "AREA removes old physical projectiles");
                    while (ability.CooldownRemaining > 0) yield return null;
                    ability.ChangeArea(); Near(ability.CooldownRemaining, 0, "Available ability remains available on AREA change");
                    Check(ability.TryActivate((Vector2)weapon.Muzzle.position + Vector2.right * 25), "Activate before DOWN and RUN reset");
                    if (choice == PG03Ability.TriploSparo) Check(ability.EffectActive, "RUN reset starts with pending triple shots");
                    actor.Hit(1000, true);
                    Check(!ability.TryActivate(Origin) && !weapon.TryFireAt(Origin), "DOWN prevents PG03 ability and base attack");
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    Near(loop.Player.Actor.Stats.ATK, 50, "RUN reset removes previous bonuses");
                    Near(loop.Player.Actor.CurrentHP, 80, "RUN reset restores full PG03 HP");
                    Check(!loop.PG03Ability.EffectActive && loop.PG03Ability.ShotsFired == 0, "RUN reset clears pending shots");
                    Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "RUN reset removes in-flight projectiles");
                    Check(loop.PG03Ability.Selected == choice && loop.PG03Passive.Selected == passiveChoice, "New RUN acquires PG03 configuration");
                }
                Check(loop.Player.Definition.PlayerId == "PG03", "All tests and resets used PG03 only");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(definition); }
        }

        private static IEnumerator LaserChecks()
        {
            var ability = loop.PG03Ability;
            var actor = loop.Player.Actor;
            Vector2 forward = AttackGeometry.Direction(37);
            Vector2 side = new Vector2(-forward.y, forward.x);
            Vector2[] offsets = { forward * 20, forward * 20 + side, forward * 10 - side,
                forward * 20.01f, forward * 10 + side * 1.01f, -forward * .01f };
            var targets = new Combatant[offsets.Length];
            for (int i = 0; i < targets.Length; i++) targets[i] = Probe(Origin + offsets[i]);
            var ally = Probe(Origin + forward * 3, Faction.PG);
            var wall = Obstacle(Origin + forward * 5, Vector2.one * 2, true);
            var obstacle = Obstacle(Origin + forward * 8, Vector2.one * 2, false);
            Check(ability.TryActivate(Origin + forward * 25), "Laser activation with rotated rectangle");
            for (int i = 0; i < targets.Length; i++) Near(targets[i].CurrentHP, i < 3 ? 465 : 500, "Laser exact rectangle boundary " + i);
            Near(ally.CurrentHP, 500, "Laser ignores PG ally");
            Near(ability.CooldownRemaining, 1.8f, "Laser 18s base scaled by CD REDUCTION 10");
            Check(!ability.EffectActive && !ability.TryActivate(Origin), "Instant laser cannot repeat during CD");
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "Laser uses no physical projectile");
            foreach (var target in targets) Check(target.GetComponent<WeakPointMark>() == null, "Laser never applies PUNTO DEBOLE");
            var wait = Wait(.2f); while (wait.MoveNext()) yield return null;
            Near(targets[0].CurrentHP, 465, "Laser damage is applied once");
            Remove(targets); Remove(ally); UnityEngine.Object.Destroy(wall); UnityEngine.Object.Destroy(obstacle); yield return null;
            while (ability.CooldownRemaining > 0) yield return null;
            var marked = Probe(Origin + Vector2.right * 3, defence: 115);
            var mark = marked.gameObject.AddComponent<WeakPointMark>(); mark.ApplyTo(marked);
            Check(ability.TryActivate(Origin + Vector2.right * 25), "Laser can consume an existing MARCHIO");
            Near(marked.CurrentHP, 463, "Laser fixed 35 after marked DEF 95 rounds 36.75 to 37");
            Check(!mark.Active, "Laser consumes MARCHIO after damage");
            Near(marked.Stats.DEF, 115, "Laser mark consumption preserves DEF"); Remove(marked); yield return null;
        }

        private static IEnumerator TripleChecks()
        {
            var ability = loop.PG03Ability;
            var weapon = loop.Player.GetComponent<PlayerWeapon>();
            Vector2 origin = weapon.Muzzle.position;
            var times = new List<float>();
            var targets = new Combatant[3];
            var behind = new Combatant[3];
            for (int i = 0; i < 3; i++)
            {
                targets[i] = Probe(origin + AttackGeometry.Direction((1 - i) * 10) * 5);
                behind[i] = Probe(origin + AttackGeometry.Direction((1 - i) * 10) * 8);
            }
            System.Action<Projectile, int> observe = (projectile, index) =>
            {
                times.Add(Time.time);
                Near(Vector2.Distance(projectile.Direction, AttackGeometry.Direction((1 - index) * 10)), 0, "Triple sector centre " + index);
                Near(projectile.Speed, 20, "Triple projectile speed");
                Near(Vector2.Distance(projectile.transform.position, weapon.Muzzle.position), 0, "Triple spawns at muzzle");
            };
            ability.TripleShot += observe;
            Check(ability.TryActivate(origin + Vector2.right * 25), "Triple activation");
            Near(ability.CooldownRemaining, 1, "Triple 10s base scaled by CD REDUCTION 10");
            Check(!ability.TryActivate(origin), "Active triple cannot overlap");
            Near(targets[0].CurrentHP, 500, "Triple projectiles have travel time");
            int shots = ability.ShotsFired;
            Time.timeScale = 0; double paused = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - paused < .15) yield return null;
            Near(ability.ShotsFired, shots, "Pause freezes triple sequence"); Near(ability.CooldownRemaining, 1, "Pause freezes triple cooldown");
            Time.timeScale = 1;
            while (ability.EffectActive) yield return null;
            ability.TripleShot -= observe;
            Near(times.Count, 3, "Exactly three shots");
            Near(times[1] - times[0], .5f, "Second shot after 0.5s", Mathf.Max(.035f, Time.deltaTime * 2));
            Near(times[2] - times[0], 1, "Third shot after 1s", Mathf.Max(.035f, Time.deltaTime * 2));
            var wait = Wait(.5f); while (wait.MoveNext()) yield return null;
            for (int i = 0; i < 3; i++)
            {
                Near(targets[i].CurrentHP, 460, "Triple fixed 40 damage despite ATK bonus");
                Near(behind[i].CurrentHP, 500, "CALIBRO PERFORANTE does not extend to abilities");
                Check(targets[i].GetComponent<WeakPointMark>() == null, "Triple never applies PUNTO DEBOLE");
            }
            Remove(targets); Remove(behind); yield return null;
            var wall = Obstacle(origin + Vector2.right * 2, new Vector2(.3f, 3), true);
            var protectedMob = Probe(origin + Vector2.right * 5);
            Check(ability.TryActivate(origin + Vector2.right * 25), "Triple reactivation after completed duration/CD");
            wait = Wait(1.4f); while (wait.MoveNext()) yield return null;
            Near(protectedMob.CurrentHP, 500, "MURO blocks triple projectiles");
            Remove(protectedMob); UnityEngine.Object.Destroy(wall); yield return null;
            var marked = Probe(origin + Vector2.right * 5);
            var mark = marked.gameObject.AddComponent<WeakPointMark>(); mark.ApplyTo(marked);
            Check(ability.TryActivate(origin + Vector2.right * 25), "Triple can consume an existing MARCHIO");
            wait = Wait(1.4f); while (wait.MoveNext()) yield return null;
            Near(marked.CurrentHP, 452, "Triple fixed 40 against marked DEF 80 deals 48");
            Check(!mark.Active, "Triple consumes MARCHIO without reapplying");
            Near(marked.EffectiveStats.DEF, 100, "Triple mark consumption restores DEF");
            Remove(marked); yield return null;
        }
    }
}
