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
    public static class PG04Validation
    {
        private const string Key = "ROG.PG04.";
        private static readonly string Request = Path.GetFullPath(".pg04-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PG04Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG04 tests")]
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
                    var input = loop.Player.GetComponent<PG04AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. PG04 only, four combinations: actual shots, abilities, passives and AREA transitions. Input feel/balancing are not tested.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG04-validation.txt"));
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
        private static void ClearFire()
        {
            foreach (var area in UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None))
                UnityEngine.Object.Destroy(area.gameObject);
        }
        private static void ReducedCD() => typeof(LoopSession).GetField("cdReduction",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(loop, 10f);
        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            Check(loop.Player.Definition.PlayerId == "PG04", "Only PG04 instantiated at suite startup");
            var original = loop.Definition; var definition = UnityEngine.Object.Instantiate(original); loop.Definition = definition;
            try
            {
                definition.SelectedPlayer = LoopPlayer.PG04;
                definition.SelectedPG04Ability = (PG04Ability)999;
                Check(definition.Validate() != null, "Invalid PG04 ability rejected");
                definition.SelectedPG04Ability = PG04Ability.PioggiaDiGranate;
                definition.SelectedPG04Passive = (PG04Passive)999;
                Check(definition.Validate() != null, "Invalid PG04 passive rejected");
                foreach (PG04Ability choice in new[] { PG04Ability.PioggiaDiGranate, PG04Ability.ColpoGrosso })
                foreach (PG04Passive passive in new[] { PG04Passive.ScortaEsplosiva, PG04Passive.Pyromania })
                {
                    definition.SelectedPG04Ability = choice; definition.SelectedPG04Passive = passive; definition.PG04TestItemSlots = 2;
                    Check(definition.Validate() == null, "Valid PG04 combination " + choice + "/" + passive);
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor; var baseline = actor.Stats; var ability = loop.PG04Ability;
                    var weapon = loop.Player.GetComponent<PlayerWeapon>();
                    loop.Player.transform.SetPositionAndRotation(Origin, Quaternion.identity);
                    Check(loop.Ability == null && loop.PG02Ability == null && loop.PG03Ability == null, "No PG01/PG02/PG03 abilities attached");
                    Near(actor.Stats.HP, 100, "PG04 HP 100"); Near(actor.Stats.ATK, 25, "PG04 ATK 25");
                    Near(actor.Stats.DEF, 100, "PG04 DEF neutral"); Near(actor.MovementMetresPerSecond, 2, "PG04 MOVE SPD 100 = 2m/s");
                    Near(actor.Stats.AttackSpeed, 65, "PG04 ATK SPD 65"); Near(actor.Stats.RangeMetres, 7, "Corrected PG04 RANGE 7m");
                    Near(ability.CooldownRemaining, choice == PG04Ability.PioggiaDiGranate ? 12 : 10, "Full initial cooldown", .08f);
                    Check(!ability.TryActivate(Origin) && !ability.EffectActive, "Fresh RUN starts unavailable without active effect");
                    definition.SelectedPG04Ability = choice == PG04Ability.PioggiaDiGranate ? PG04Ability.ColpoGrosso : PG04Ability.PioggiaDiGranate;
                    definition.SelectedPG04Passive = passive == PG04Passive.ScortaEsplosiva ? PG04Passive.Pyromania : PG04Passive.ScortaEsplosiva;
                    Check(ability.Selected == choice && ability.Passive == passive, "PG04 selections are RUN snapshots");
                    definition.SelectedPG04Ability = choice; definition.SelectedPG04Passive = passive;
                    ItemChecks(passive);
                    var baseChecks = BaseChecks(passive); while (baseChecks.MoveNext()) yield return null;
                    if (passive == PG04Passive.Pyromania)
                    {
                        var fireChecks = BurningChecks(); while (fireChecks.MoveNext()) yield return null;
                    }
                    ClearFire(); yield return null;
                    while (ability.CooldownRemaining > 0) yield return null;
                    ReducedCD();
                    var special = choice == PG04Ability.PioggiaDiGranate ? RainChecks(passive) : BigShotChecks(passive);
                    while (special.MoveNext()) yield return null;
                    actor.SetStats(baseline, 100); ClearFire(); yield return null;
                    while (ability.CooldownRemaining > 0) yield return null;
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.IsActive && mob.Faction == Faction.MOB) mob.Die();
                        yield return null;
                    }
                    Check(ability.TryActivate(Origin + Vector2.up * 50), "Activate before AREA transition");
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    Check(ability.EffectActive, "PG04 ability active at BONUS entry");
                    float cd = ability.CooldownRemaining, remaining = ability.ActiveRemaining; int charges = ability.Charges;
                    double paused = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - paused < .15) yield return null;
                    Near(ability.CooldownRemaining, cd, "BONUS pauses CD"); Near(ability.ActiveRemaining, remaining, "BONUS pauses rain");
                    Near(ability.Charges, charges, "BONUS preserves charges");
                    var slots = loop.PG04Items;
                    Check(slots.TryAdd(0, PrototypeItem.Granata), "Load test item before AREA transition");
                    loop.Choices[0] = AreaStat.ATK; loop.SelectBonus(0); loop.ConfirmBonus();
                    Near(ability.CooldownRemaining, choice == PG04Ability.PioggiaDiGranate ? 1.2f : 1, "Active AREA change restarts full reduced CD");
                    Check(!ability.EffectActive && ability.Charges == 0, "AREA cancels rain and residual charges");
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.Player.Definition.PlayerId == "PG04" && loop.PG04Ability == ability && loop.PG04Items == slots, "AREA retains PG04 selection and inventory instances");
                    Near(slots.Count(0), 1, "AREA preserves stored items"); Near(actor.Stats.ATK, 26.25f, "AREA preserves real stat bonus");
                    Check(UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None).Length == 0, "AREA removes old burning areas");
                    while (ability.CooldownRemaining > 0) yield return null;
                    ability.ChangeArea(); Near(ability.CooldownRemaining, 0, "Available ability stays available on AREA change");
                    Check(ability.TryActivate((Vector2)actor.transform.position + Vector2.right * 20), "Activate before DOWN/reset");
                    actor.Hit(1000, true); Check(!ability.TryActivate(Origin) && !weapon.TryFireAt(Origin), "DOWN prevents PG04 commands");
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    Near(loop.Player.Actor.Stats.ATK, 25, "RUN reset removes previous bonuses"); Near(loop.Player.Actor.CurrentHP, 100, "RUN reset restores HP");
                    Check(!loop.PG04Ability.EffectActive && loop.PG04Ability.Charges == 0, "RUN reset removes pending rain/charges");
                    Near(loop.PG04Items.Count(0), 0, "RUN reset clears technical inventory");
                    Check(UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None).Length == 0, "RUN reset clears burning areas");
                }
                Check(loop.Player.Definition.PlayerId == "PG04", "Entire suite ran PG04 only");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(definition); }
        }
        private static void ItemChecks(PG04Passive passive)
        {
            var items = loop.PG04Items; int cap = passive == PG04Passive.ScortaEsplosiva ? 3 : 1;
            Near(items.SlotCount, 2, "Two independently configured test slots");
            for (int i = 0; i < cap; i++) Check(items.TryAdd(0, PrototypeItem.Granata) && items.TryAdd(1, PrototypeItem.Molotov), "Both explosive item types stack to passive limit");
            Check(!items.TryAdd(0, PrototypeItem.Granata) && !items.TryAdd(1, PrototypeItem.Molotov), "No overflow above per-slot cap");
            Check(!items.TryAdd(0, PrototypeItem.Molotov), "Cannot mix item kinds within a slot");
            for (int i = cap; i > 0; i--) { Check(items.TryConsume(0) && items.TryConsume(1), "Each consumption removes one unit"); Near(items.Count(0), i - 1, "Remaining stack"); }
            Check(items.Kind(0) == PrototypeItem.None && !items.TryConsume(0), "Exhausted slot is empty");
            foreach (var kind in new[] { PrototypeItem.Smoke, PrototypeItem.PozioneCurativa, PrototypeItem.Trappola })
            {
                Check(items.TryAdd(0, kind) && !items.TryAdd(0, kind), "Other item capacity remains one: " + kind); items.TryConsume(0);
            }
        }
        private static IEnumerator BaseChecks(PG04Passive passive)
        {
            var ability = loop.PG04Ability; var weapon = loop.Player.GetComponent<PlayerWeapon>();
            Vector2 center = Origin + Vector2.right * 6;
            Vector2 impact = Vector2.zero; float lastRadius = 0, lastDamage = 0;
            Action<Vector2, float, float> record = (p, r, d) => { impact = p; lastRadius = r; lastDamage = d; };
            ability.Explosion += record;
            var target = Probe(center); var edge = Probe(center + Vector2.up * 1.25f); var outside = Probe(center + Vector2.up * 1.26f);
            var ally = Probe(center, Faction.PG); var pathMob = Probe(Origin + Vector2.right * 2);
            var pathObstacle = Obstacle(Origin + Vector2.right * 3, Vector2.one * .3f, false);
            var firing = Fire(center); while (firing.MoveNext()) yield return null;
            float firedAt = Time.time;
            Near(Vector2.Distance(impact, center), 0, "MOB and OSTACOLO do not anticipate base impact");
            Near(lastRadius, 1.25f, "Base explosion radius 1.25m"); Near(lastDamage, 25, "Base explosion damage 25");
            Near(target.CurrentHP, passive == PG04Passive.Pyromania ? 470 : 475, "Instant impact plus optional immediate PYROMANIA tick");
            Near(edge.CurrentHP, target.CurrentHP, "Exact radial boundary included"); Near(outside.CurrentHP, 500, "Outside explosion radius excluded");
            Near(ally.CurrentHP, 500, "No friendly fire"); Near(pathMob.CurrentHP, 500, "No damage along hitscan path");
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "Base attack creates no travelling projectile");
            Check(!weapon.TryFireAt(center), "Base shot cadence enforced");
            Remove(target, edge, outside, ally, pathMob); UnityEngine.Object.Destroy(pathObstacle); ClearFire(); yield return null;
            firing = Fire(Origin + Vector2.right * 20); while (firing.MoveNext()) yield return null;
            Near(Time.time - firedAt, 100f / 65, "Actual base cadence 0.65 attacks/s", Mathf.Max(.04f, Time.deltaTime * 2));
            Near(Vector2.Distance(impact, Origin + Vector2.right * 7), 0, "Far cursor clamped at RANGE 7m from PG");
            ClearFire(); yield return null;
            var wall = Obstacle(Origin + Vector2.right * 3, new Vector2(.5f, 2), true);
            firing = Fire(center); while (firing.MoveNext()) yield return null;
            Near(impact.x - Origin.x, 2.75f, "MURO anticipates impact at its near face");
            UnityEngine.Object.Destroy(wall); ClearFire(); yield return null;
            foreach (bool isWall in new[] { true, false })
            {
                var blocker = Obstacle(center + new Vector2(.7f, .7f), Vector2.one * .2f, isWall);
                var blocked = Probe(center + new Vector2(.15f, .15f)); // Clear ray, same intercepted quadrant.
                var exposed = Probe(center + new Vector2(-.7f, -.7f), defence: 115);
                firing = Fire(center); while (firing.MoveNext()) yield return null;
                Near(blocked.CurrentHP, 500, "Whole sector removed by " + (isWall ? "MURO" : "OSTACOLO"));
                Near(exposed.CurrentHP, passive == PG04Passive.Pyromania ? 475 : 479, "Valid sector damage after DEF and final rounding");
                Remove(blocked, exposed); UnityEngine.Object.Destroy(blocker); ClearFire(); yield return null;
            }
            ability.Explosion -= record;
        }
        private static IEnumerator BurningChecks()
        {
            ClearFire(); yield return null;
            Vector2 center = Origin + Vector2.up * 5;
            var target = Probe(center); var entrant = Probe(center + Vector2.right * 3); var ally = Probe(center, Faction.PG);
            var firing = Fire(center); while (firing.MoveNext()) yield return null;
            var area = UnityEngine.Object.FindFirstObjectByType<PG04BurningArea>();
            Check(area != null && area.Ticks == 1, "PYROMANIA first tick at activation"); Near(target.CurrentHP, 470, "Immediate 25 explosion + 5 fire");
            var wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            entrant.transform.position = center;
            wait = Wait(.3f); while (wait.MoveNext()) yield return null;
            Near(entrant.CurrentHP, 500, "Entering between ticks does not cause immediate damage");
            target.transform.position = center + Vector2.right * 3;
            wait = Wait(.45f); while (wait.MoveNext()) yield return null;
            Near(entrant.CurrentHP, 495, "Area clock ticks at 1 second independent of entry"); Near(target.CurrentHP, 470, "Outside MOB receives no tick");
            target.transform.position = center;
            Time.timeScale = 0; double pause = EditorApplication.timeSinceStartup; float remaining = area.Remaining;
            while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
            Near(area.Remaining, remaining, "Pause freezes burning area clock"); Near(entrant.CurrentHP, 495, "Pause causes no fire damage"); Time.timeScale = 1;
            wait = Wait(1.05f); while (wait.MoveNext()) yield return null;
            Near(entrant.CurrentHP, 490, "Second periodic tick at 2 seconds"); Near(target.CurrentHP, 465, "Reentry joins existing area clock");
            Near(ally.CurrentHP, 500, "PYROMANIA ignores PG ally");
            wait = Wait(1.05f); while (wait.MoveNext()) yield return null;
            Check(area == null, "Fire expires at 3 seconds"); Near(entrant.CurrentHP, 490, "No tick at expiry or afterward");
            Remove(target, entrant, ally); yield return null;
            var victim = Probe(center); victim.SetStats(new CombatStats { HP = 5, DEF = 100 }); int kills = 0;
            Action<Combatant> credit = mob => { if (mob == victim) kills++; }; loop.Player.Actor.Killed += credit;
            var a = new GameObject("Fire kill probe"); a.transform.SetParent(TestVisuals.Root); a.transform.position = center;
            a.AddComponent<PG04BurningArea>().Initialize(loop, loop.Player.Actor, 1.25f, 3, 5, new[] { true, true, true, true });
            Near(kills, 1, "Burning area lethal tick credits PG04 exactly once"); loop.Player.Actor.Killed -= credit;
            Remove(victim); ClearFire(); yield return null;
            var overlap = Probe(center);
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject("Overlapping fire"); go.transform.SetParent(TestVisuals.Root); go.transform.position = center;
                go.AddComponent<PG04BurningArea>().Initialize(loop, loop.Player.Actor, 1.25f, 3, 5, new[] { true, true, true, true });
            }
            Near(overlap.CurrentHP, 490, "Overlapping areas sum their immediate damage");
            wait = Wait(1.05f); while (wait.MoveNext()) yield return null;
            Near(overlap.CurrentHP, 480, "Overlapping areas retain independent periodic damage"); Remove(overlap); ClearFire(); yield return null;
        }
        private static IEnumerator RainChecks(PG04Passive passive)
        {
            var ability = loop.PG04Ability; var actor = loop.Player.Actor; float cd = 10; AreaStatBonus.Apply(actor, AreaStat.ATK, ref cd);
            Vector2 center = Origin + Vector2.up * 50;
            var positions = new List<Vector2>(); var times = new List<float>();
            float start = Time.time;
            Action<Vector2, float, float> record = (p, r, d) =>
            {
                positions.Add(p); times.Add(Time.time - start);
                Check(Vector2.Distance(p, center) <= 5.001f, "Rain explosion within 5m of activation cursor");
                Near(r, 1.25f, "Rain inherits base explosion radius"); Near(d, 26.25f, "Rain inherits current base ATK after bonus");
            };
            ability.Explosion += record;
            Check(ability.TryActivate(center), "Rain accepts cursor beyond base attack range");
            Near(ability.CooldownRemaining, 1.2f, "Rain starts CD at activation using CD REDUCTION");
            Check(!ability.TryActivate(center), "Active rain cannot retrigger");
            Time.timeScale = 0; double pause = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - pause < .15) yield return null;
            Near(positions.Count, 0, "Pause prevents pending rain explosions"); Time.timeScale = 1;
            while (ability.EffectActive) yield return null;
            ability.Explosion -= record;
            Near(positions.Count, 10, "Exactly ten rain explosions");
            Check(times[0] >= 0 && times[9] <= 3.05f, "All rain explosions occur within 3 seconds");
            Check(Vector2.Distance(positions[0], positions[1]) > .001f, "Rain positions are distinct random samples");
            Near(ability.CooldownRemaining, 0, "Rain cooldown progresses during duration");
            int fires = UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None).Length;
            Near(fires, passive == PG04Passive.Pyromania ? 10 : 0, "Each rain explosion creates fire only with PYROMANIA");
            ClearFire(); yield return null;
        }
        private static IEnumerator BigShotChecks(PG04Passive passive)
        {
            var ability = loop.PG04Ability; var actor = loop.Player.Actor; var weapon = loop.Player.GetComponent<PlayerWeapon>();
            Check(ability.TryActivate(Origin), "Activate COLPO GROSSO"); Near(ability.Charges, 4, "Four charges");
            Near(ability.CooldownRemaining, 0, "COLPO GROSSO CD waits for fourth shot");
            Near(actor.Stats.ATK, 25, "Charges do not rewrite persistent ATK");
            var wait = Wait(3.1f); while (wait.MoveNext()) yield return null;
            Near(ability.Charges, 4, "Unused charges do not expire"); Check(!ability.TryActivate(Origin), "Cannot stack active charges");
            float cd = 10; AreaStatBonus.Apply(actor, AreaStat.ATK, ref cd);
            float damage = 0, radius = 0;
            Action<Vector2, float, float> record = (p, r, d) => { damage = d; radius = r; };
            ability.Explosion += record;
            for (int i = 0; i < 4; i++)
            {
                Vector2 center = Origin + Vector2.right * 5;
                Combatant target = i == 0 ? Probe(center + Vector2.up * 1.875f) : null;
                var firing = Fire(center); while (firing.MoveNext()) yield return null;
                Near(damage, 47.25f, "COLPO GROSSO +80 percent on current ATK"); Near(radius, 1.875f, "COLPO GROSSO radius +50 percent");
                Near(actor.Stats.ATK, 26.25f, "Permanent bonus remains independent");
                Near(ability.Charges, 3 - i, "Every shot consumes a charge including MISS");
                if (i < 3) Near(ability.CooldownRemaining, 0, "Cooldown still absent before fourth shot");
                else Near(ability.CooldownRemaining, 1, "Fourth shot begins full reduced 10s cooldown");
                if (target != null) { Near(target.CurrentHP, passive == PG04Passive.Pyromania ? 448 : 453, "Expanded explosion actual damage"); Remove(target); }
                if (passive == PG04Passive.Pyromania) Near(UnityEngine.Object.FindFirstObjectByType<PG04BurningArea>().Radius, 1.875f, "Fire inherits expanded radius");
                ClearFire(); yield return null;
            }
            Check(!ability.EffectActive, "Fourth charge ends COLPO GROSSO");
            float residual = ability.CooldownRemaining; ability.ChangeArea(); Near(ability.CooldownRemaining, residual, "Inactive cooldown retains residue");
            var next = Fire(Origin + Vector2.right * 5); while (next.MoveNext()) yield return null;
            Near(damage, 26.25f, "Next base shot retains permanent bonus without temporary boost"); Near(radius, 1.25f, "Base radius restored after fourth shot");
            ability.Explosion -= record; ClearFire(); yield return null;
        }
    }
}
