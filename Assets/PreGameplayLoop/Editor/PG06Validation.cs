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
    public static class PG06Validation
    {
        private const string Key = "ROG.PG06.";
        private static readonly string Request = Path.GetFullPath(".pg06-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG06Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG06 tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
            // Select PG06 in memory for this suite only, preserving the user's Inspector choice.
            var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
            SessionState.SetInt(Key + "PreviousPlayer", (int)config.SelectedPlayer);
            SessionState.SetBool(Key + "RestoreSelection", true);
            config.SelectedPlayer = LoopPlayer.PG06;
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
                    var input = loop.Player.GetComponent<PG06AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. PG06 only, four combinations: actual shots, abilities, passives and AREA transitions. Input feel/balancing are not tested.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG06-validation.txt"));
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
        private static void ClearProjectiles()
        { foreach (var p in UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None)) UnityEngine.Object.Destroy(p.gameObject); }
        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            Check(loop.Player.Definition.PlayerId == "PG06", "Suite starts with PG06 only");
            var original = loop.Definition; var config = UnityEngine.Object.Instantiate(original); loop.Definition = config;
            try
            {
                foreach (PG06Ability choice in Enum.GetValues(typeof(PG06Ability)))
                foreach (PG06Passive passive in Enum.GetValues(typeof(PG06Passive)))
                {
                    config.SelectedPlayer = LoopPlayer.PG06; config.SelectedPG06Ability = choice; config.SelectedPG06Passive = passive;
                    Check(config.Validate() == null, "Valid combination " + choice + "/" + passive);
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor; var ability = loop.PG06Ability;
                    loop.Player.transform.SetPositionAndRotation(Origin, Quaternion.identity);
                    Check(loop.Ability == null && loop.PG02Ability == null && loop.PG03Ability == null && loop.PG04Ability == null && loop.PG05Ability == null, "No other PG runtime");
                    Near(actor.Stats.HP, 120, "PG06 HP120"); Near(actor.Stats.ATK, 40, "PG06 ATK40"); Near(actor.Stats.DEF, 100, "PG06 DEF neutral");
                    Near(actor.Stats.AttackSpeed, 80, "PG06 ATK SPD80"); Near(actor.MovementMetresPerSecond, 2, "PG06 MOVE100"); Near(actor.Stats.RangeMetres, 10, "PG06 RANGE10");
                    Near(ability.CooldownRemaining, choice == PG06Ability.CuraAdArea ? 20 : 15, "Initial full CD", .15f);
                    Check(!ability.TryActivate(Origin), "Not ready at RUN start");
                    config.SelectedPG06Passive = passive == PG06Passive.Elemosina ? PG06Passive.VitaminaC : PG06Passive.Elemosina;
                    Check(ability.Passive == passive, "RUN snapshot independent of Inspector"); config.SelectedPG06Passive = passive;
                    var test = BaseChecks(); while (test.MoveNext()) yield return null;
                    test = TargetChecks(); while (test.MoveNext()) yield return null;
                    test = MedikitChecks(); while (test.MoveNext()) yield return null;
                    test = KillChecks(passive); while (test.MoveNext()) yield return null;
                    while (ability.CooldownRemaining > 0) yield return null;
                    ReducedCD();
                    test = choice == PG06Ability.CuraAdArea ? AreaChecks(passive) : FireChecks(passive); while (test.MoveNext()) yield return null;
                    test = VitaminChecks(); while (test.MoveNext()) yield return null;
                    HP(actor, 120); ClearProjectiles(); yield return null;
                    while (ability.CooldownRemaining > 0) yield return null;
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.IsActive && mob.Faction == Faction.MOB) mob.Die();
                        yield return null;
                    }
                    Check(ability.TryActivate(Origin), "Activate before AREA");
                    var kit = CreateKit(Origin + Vector2.up * 30);
                    var vitamin = actor.GetComponent<PG06Vitamin>() ?? actor.gameObject.AddComponent<PG06Vitamin>(); vitamin.Refresh(loop, 25, 4);
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    float cd = ability.CooldownRemaining, remain = vitamin.Remaining;
                    double pause = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
                    Near(ability.CooldownRemaining, cd, "BONUS pause stops CD"); Near(vitamin.Remaining, remain, "BONUS pause stops vitamin");
                    loop.Choices[0] = AreaStat.ATK; loop.SelectBonus(0); loop.ConfirmBonus();
                    Near(ability.CooldownRemaining, choice == PG06Ability.FuocoCurativo ? 1.5f : cd, "AREA full CD if charged, residual otherwise");
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.PG06Ability == ability && ability.Selected == choice && ability.Passive == passive, "AREA retains PG06 selections");
                    Near(ability.Charges, 0, "AREA clears charges"); Near(vitamin.Remaining, 0, "AREA clears vitamin"); Check(kit == null, "AREA removes MEDI KIT");
                    Near(actor.Stats.ATK, 42, "AREA ATK bonus preserved");
                    while (ability.CooldownRemaining > 0) yield return null;
                    ability.ChangeArea(); Near(ability.CooldownRemaining, 0, "Ready CD preserved");
                    actor.Hit(1000, true); Check(!ability.TryActivate(Origin), "DOWN blocks ability");
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    Near(loop.Player.Actor.Stats.ATK, 40, "RUN resets bonus"); Near(loop.Player.Actor.CurrentHP, 120, "RUN restores HP");
                    Near(loop.PG06Ability.Charges, 0, "RUN resets charges"); Near(loop.PG06Ability.KillRolls, 0, "RUN resets kill rolls");
                    Near(loop.Player.Actor.EffectiveStats.AttackSpeed, 80, "RUN resets vitamin");
                    Check(UnityEngine.Object.FindObjectsByType<PG06Medikit>(FindObjectsSortMode.None).Length == 0, "RUN clears pickups");
                }
                Check(loop.Player.Definition.PlayerId == "PG06", "Entire suite PG06 only");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(config); }
        }
        private static IEnumerator BaseChecks()
        {
            var target = Probe(Origin + Vector2.right * 3, defence: 115); var behind = Probe(Origin + Vector2.right * 5);
            var ally = Probe(Origin + Vector2.right, Faction.PG);
            var firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null; float at = Time.time;
            Check(!loop.Player.GetComponent<PlayerWeapon>().TryFireAt(Origin + Vector2.right * 10), "Cadence blocks immediate repeat");
            var wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 466, "Base HIT40 with DEF15 percent"); Near(behind.CurrentHP, 500, "Non-piercing projectile"); Near(ally.CurrentHP, 120, "Projectile crosses friendly PG06");
            Remove(target, behind, ally); yield return null;
            firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
            Near(Time.time - at, 1.25f, "Actual base cadence .8 attacks/s", Mathf.Max(.05f, Time.deltaTime * 2));
            wait = Wait(.65f); while (wait.MoveNext()) yield return null;
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "Projectile expires at range10");
            foreach (bool wall in new[] { true, false })
            {
                target = Probe(Origin + Vector2.right * 3); var blocker = Obstacle(Origin + Vector2.right * 2, Vector2.one * .5f, wall);
                firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
                wait = Wait(.3f); while (wait.MoveNext()) yield return null;
                Near(target.CurrentHP, 500, "MURO/OSTACOLO blocks base projectile"); Remove(target); UnityEngine.Object.Destroy(blocker); yield return null;
            }
        }
        private static IEnumerator TargetChecks()
        {
            var a = Probe(Origin + Vector2.up * 2, Faction.PG); var b = Probe(Origin + Vector2.up * 3, Faction.PG);
            var list = new List<Combatant> { a, b };
            HP(a, 100); HP(b, 60); Check(PG06Healing.Select(list, Origin, true) == b, "Lowest HP fraction first");
            var stats = a.Stats; stats.HP = 80; a.SetStats(stats); HP(a, 40); Check(PG06Healing.Select(list, Origin, true) == a, "Equal fraction lower actual HP first");
            stats.HP = 120; a.SetStats(stats); HP(a, 60); Check(PG06Healing.Select(list, Origin, true) == a, "Equal fraction and HP nearest first");
            a.transform.position = b.transform.position; var seen = new HashSet<Combatant>(); var randomState = UnityEngine.Random.state;
            for (int i = 0; i < 64; i++) seen.Add(PG06Healing.Select(list, Origin, true)); UnityEngine.Random.state = randomState;
            Near(seen.Count, 2, "Perfect ties use random selection");
            HP(a, 120); Check(PG06Healing.Select(list, Origin, true) == b, "Injured PG before full HP");
            b.Hit(1000, true); Check(PG06Healing.Select(list, Origin, true) == a, "DOWN ignored; full HP remains valid");
            a.Die(); Check(PG06Healing.Select(list, Origin, true) == null, "DEAD excluded"); Remove(a, b); yield return null;
        }
        private static PG06Medikit CreateKit(Vector2 position)
        {
            var go = new GameObject("Test PG06 MEDI KIT"); go.transform.SetParent(TestVisuals.Root); go.transform.position = position;
            var kit = go.AddComponent<PG06Medikit>(); kit.Initialize(loop, .1f, .5f); return kit;
        }
        private static IEnumerator MedikitChecks()
        {
            Vector2 center = Origin + Vector2.up * 30;
            var a = Probe(center, Faction.PG); var b = Probe(center, Faction.PG); var kit = CreateKit(center);
            Check(kit.GetComponent<CircleCollider2D>().isTrigger && kit.gameObject.layer == LayerMask.NameToLayer("TRIGGER_PG"), "MEDI KIT is a nonblocking PG trigger");
            var wait = Wait(.1f); while (wait.MoveNext()) yield return null;
            Check(kit != null && !kit.Consumed, "Full HP does not consume MEDI KIT");
            HP(a, 60); HP(b, 30);
            wait = Wait(.1f); while (wait.MoveNext()) yield return null;
            Near(a.CurrentHP, 60, "Pickup selects one PG only"); Near(b.CurrentHP, 42, "Lowest HP pickup heals ten percent max HP"); Check(kit == null, "Pickup consumed once");
            Check(b.GetComponent<PG06Vitamin>() == null, "MEDI KIT does not trigger vitamin");
            a.transform.position += Vector3.right * 3; HP(b, 119); kit = CreateKit(center);
            wait = Wait(.1f); while (wait.MoveNext()) yield return null; Near(b.CurrentHP, 120, "No pickup overheal");
            b.Hit(1000, true); kit = CreateKit(center);
            wait = Wait(.1f); while (wait.MoveNext()) yield return null; Check(kit != null, "DOWN cannot consume pickup");
            Remove(a, b); UnityEngine.Object.Destroy(kit.gameObject); yield return null;
        }
        private static IEnumerator KillChecks(PG06Passive passive)
        {
            var ability = loop.PG06Ability; var saved = UnityEngine.Random.state;
            foreach (bool success in new[] { false, true })
            {
                int seed = 0;
                for (; seed < 10000; seed++) { UnityEngine.Random.InitState(seed); if ((UnityEngine.Random.value < .03f) == success) break; }
                var target = Probe(Origin + Vector2.up * 30); HP(target, 1);
                int rolls = ability.KillRolls, drops = ability.Drops;
                UnityEngine.Random.InitState(seed); target.Hit(100, true, loop.Player.Actor);
                Near(ability.KillRolls - rolls, passive == PG06Passive.Elemosina ? 1 : 0, "One roll per owned KILL only with ELEMOSINA");
                Near(ability.Drops - drops, passive == PG06Passive.Elemosina && success ? 1 : 0, "Actual random outcome below/above three percent");
                target.Die(); Near(ability.KillRolls - rolls, passive == PG06Passive.Elemosina ? 1 : 0, "Duplicate death cannot roll again"); Remove(target); yield return null;
            }
            var victim = Probe(Origin + Vector2.up * 30); int before = ability.KillRolls; victim.Hit(1000, true);
            Near(ability.KillRolls, before, "Unattributed kill does not roll"); Remove(victim);
            foreach (var kit in UnityEngine.Object.FindObjectsByType<PG06Medikit>(FindObjectsSortMode.None)) UnityEngine.Object.Destroy(kit.gameObject);
            UnityEngine.Random.state = saved; yield return null;
        }
        private static IEnumerator AreaChecks(PG06Passive passive)
        {
            var a = loop.Player.Actor; HP(a, 50);
            var edge = Probe(Origin + Vector2.up * 5, Faction.PG); HP(edge, 110);
            var outside = Probe(Origin + Vector2.up * 5.01f, Faction.PG); HP(outside, 10);
            var full = Probe(Origin + Vector2.down * 4, Faction.PG); var down = Probe(Origin + Vector2.down * 3, Faction.PG); down.Hit(1000, true);
            var wall = Obstacle(Origin + Vector2.up * 2, new Vector2(3, 1), true);
            Check(loop.PG06Ability.TryActivate(Origin), "Activate CURA AD AREA"); Near(loop.PG06Ability.CooldownRemaining, 2, "Area CD20 starts at activation with reduction");
            Near(a.CurrentHP, 90, "Self heal40"); Near(edge.CurrentHP, 120, "5m edge included, no overheal, MURO ignored"); Near(outside.CurrentHP, 10, "Outside5m excluded");
            Check(down.State == LifeState.Down, "Heal does not resurrect");
            Check((full.GetComponent<PG06Vitamin>() != null) == (passive == PG06Passive.VitaminaC), "Full HP valid for VITAMINA C");
            Near(a.EffectiveStats.AttackSpeed, passive == PG06Passive.VitaminaC ? 108 : 80, "Vitamin actual effective speed");
            Remove(edge, outside, full, down); UnityEngine.Object.Destroy(wall); yield return null;
        }
        private static IEnumerator FireChecks(PG06Passive passive)
        {
            var ability = loop.PG06Ability; var a = loop.Player.Actor; HP(a, 60);
            Check(ability.TryActivate(Origin), "Activate FUOCO CURATIVO"); Near(ability.Charges, 6, "Six charges"); Near(ability.CooldownRemaining, 0, "CD absent until sixth shot");
            var wait = Wait(3); while (wait.MoveNext()) yield return null; Near(ability.Charges, 6, "Charges do not expire"); Check(!ability.TryActivate(Origin), "Cannot stack charges");
            for (int i = 0; i < 6; i++)
            {
                var ally = Probe(Origin + Vector2.up * 15, Faction.PG); HP(ally, 10);
                var outside = Probe(Origin + Vector2.up * 15.01f, Faction.PG); HP(outside, 1);
                var target = i % 2 == 0 ? Probe(Origin + Vector2.right * 3) : null;
                var firing = Fire(Origin + Vector2.right * 10); while (firing.MoveNext()) yield return null;
                Near(ability.Charges, 5 - i, "Shot consumes charge including MISS"); Near(ability.CooldownRemaining, i == 5 ? 1.5f : 0, "Sixth launch starts full CD");
                wait = Wait(.7f); while (wait.MoveNext()) yield return null;
                Near(ally.CurrentHP, target != null ? 25 : 10, "Hit heals15; MISS does not heal"); Near(outside.CurrentHP, 1, "Outside15m cannot receive heal");
                Near(a.CurrentHP, 60, "More injured ally has priority over self");
                Check((ally.GetComponent<PG06Vitamin>() != null) == (target != null && passive == PG06Passive.VitaminaC), "Vitamin only selected target after valid HIT");
                if (target != null) Near(target.CurrentHP, 460, "Healing shot preserves base damage40"); Remove(ally, outside, target); yield return null;
            }
            Check(!ability.EffectActive, "Sixth shot ends charged effect"); float cd = ability.CooldownRemaining; ability.ChangeArea(); Near(ability.CooldownRemaining, cd, "Inactive cooldown preserves residue");
            while (ability.CooldownRemaining > 0) yield return null;
            HP(a, 120); var full = Probe(Origin + Vector2.up * 2, Faction.PG);
            var smallStats = full.Stats; smallStats.HP = 80; full.SetStats(smallStats);
            Check(PG06Healing.Select(new List<Combatant> { full, a }, Origin, true) == a, "All full HP chooses nearest, ignoring absolute HP");
            var enemy = Probe(Origin + Vector2.right * 3); Combatant healed = null;
            Action<Combatant> capture = pg => healed = pg; ability.Healed += capture;
            Check(ability.TryActivate(Origin), "Activate again for full-health HIT");
            var launch = Fire(Origin + Vector2.right * 10); while (launch.MoveNext()) yield return null;
            wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            Check(healed == a, "Full-health HIT selects nearest PG06 self"); Near(a.CurrentHP, 120, "Zero effective heal at full HP");
            Near(a.EffectiveStats.AttackSpeed, passive == PG06Passive.VitaminaC ? 108 : 80, "Zero heal still grants vitamin to selected PG");
            Check(full.GetComponent<PG06Vitamin>() == null, "Unselected full-health PG gets no vitamin");
            ability.Healed -= capture; ability.ChangeArea(); Remove(full, enemy); yield return null;
        }
        private static IEnumerator VitaminChecks()
        {
            var a = loop.Player.Actor; var b = Probe(Origin + Vector2.up * 30, Faction.PG);
            var vitamin = a.GetComponent<PG06Vitamin>() ?? a.gameObject.AddComponent<PG06Vitamin>();
            vitamin.Refresh(loop, 25, 4); Near(a.EffectiveStats.AttackSpeed, 100, "Vitamin adds25 percent BASE80");
            var stats = a.Stats; stats.AttackSpeed = 100; a.SetStats(stats);
            Near(a.EffectiveStats.AttackSpeed, 120, "Persistent bonus acquired during vitamin preserved separately");
            var wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            b.gameObject.AddComponent<PG06Vitamin>().Refresh(loop, 25, 4);
            vitamin.Refresh(loop, 25, 4); Near(vitamin.Remaining, 4, "Refresh duration4"); Near(a.EffectiveStats.AttackSpeed, 120, "Reapply does not stack or use current SPD as BASE");
            vitamin.Clear(); Near(a.EffectiveStats.AttackSpeed, 100, "Clear preserves acquired bonus"); Check(b.GetComponent<PG06Vitamin>().Remaining > 0, "Durations individual");
            wait = Wait(4.1f); while (wait.MoveNext()) yield return null;
            Near(b.EffectiveStats.AttackSpeed, 80, "Vitamin expires normally");
            stats.AttackSpeed = 80; a.SetStats(stats); Remove(b); yield return null;
        }
    }
}
