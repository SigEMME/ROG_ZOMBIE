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
    public static class PG08Validation
    {
        private const string Key = "ROG.PG08.";
        private static readonly string Request = Path.GetFullPath(".pg08-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;
        private static readonly HashSet<string> warningMessages = new HashSet<string>();

        static PG08Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG08 tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
            // Select PG08 in memory for this suite only, preserving the user's Inspector choice.
            var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
            SessionState.SetInt(Key + "PreviousPlayer", (int)config.SelectedPlayer);
            SessionState.SetBool(Key + "RestoreSelection", true);
            config.SelectedPlayer = LoopPlayer.PG08;
            SessionState.SetBool(Key + "Running", true);
            EditorApplication.isPlaying = true;
        }

        private static void PlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            {
                results.Clear(); warningMessages.Clear(); warnings = 0; loop = null;
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
            if (type == LogType.Warning) { warnings++; warningMessages.Add(message); }
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
                    var input = loop.Player.GetComponent<PG08AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. PG08 only, four combinations: actual shots, abilities, passives and AREA transitions. Input feel/balancing are not tested.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG08-validation.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report, (passed ? "PASS" : "FAIL") + $" | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" +
                string.Join("\n", results) + "\n" + detail + "\nWarnings:\n" + string.Join("\n", warningMessages));
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
        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            Check(loop.Player.Definition.PlayerId == "PG08", "Suite starts PG08 only");
            var original = loop.Definition; var config = UnityEngine.Object.Instantiate(original); loop.Definition = config;
            try
            {
                GeometryChecks();
                foreach (PG08Ability choice in Enum.GetValues(typeof(PG08Ability)))
                foreach (PG08Passive passiveChoice in Enum.GetValues(typeof(PG08Passive)))
                {
                    config.SelectedPlayer = LoopPlayer.PG08; config.SelectedPG08Ability = choice; config.SelectedPG08Passive = passiveChoice;
                    Check(config.Validate() == null, "Valid combination " + choice + "/" + passiveChoice);
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor; var ability = loop.PG08Ability; var passive = loop.PG08Passive;
                    loop.Player.transform.SetPositionAndRotation(Origin, Quaternion.identity);
                    Check(loop.Ability == null && loop.PG02Ability == null && loop.PG03Ability == null && loop.PG04Ability == null && loop.PG05Ability == null && loop.PG06Ability == null && loop.PG07Ability == null, "Only PG08 runtime instantiated");
                    Near(actor.Stats.HP, 100, "HP100"); Near(actor.Stats.ATK, 7, "ATK7"); Near(actor.Stats.DEF, 110, "DEF+10%");
                    Near(actor.Stats.AttackSpeed, 200, "ATK SPD200"); Near(actor.MovementMetresPerSecond, 1.8f, "MOVE90=1.8m/s"); Near(actor.Stats.RangeMetres, 8, "RANGE8");
                    Near(ability.CooldownRemaining, choice == PG08Ability.FiloSpinato ? 16 : 10, "Full CD at RUN start", .2f);
                    Check(!ability.TryActivate(), "Initial CD prevents activation");
                    config.SelectedPG08Ability = choice == PG08Ability.FiloSpinato ? PG08Ability.ColpiRespingenti : PG08Ability.FiloSpinato;
                    config.SelectedPG08Passive = passiveChoice == PG08Passive.Tenacia ? PG08Passive.Rage : PG08Passive.Tenacia;
                    Check(ability.Selected == choice && passive.Selected == passiveChoice, "RUN snapshots both selections");
                    config.SelectedPG08Ability = choice; config.SelectedPG08Passive = passiveChoice;
                    var task = BaseChecks(); while (task.MoveNext()) yield return null;
                    if (passiveChoice == PG08Passive.Tenacia) TenacityChecks();
                    else { task = RageChecks(); while (task.MoveNext()) yield return null; }
                    task = choice == PG08Ability.FiloSpinato ? WireChecks() : PushChecks(); while (task.MoveNext()) yield return null;
                    passive.SetFireHeld(false); passive.ChangeArea();
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.IsActive && mob.Faction == Faction.MOB) mob.Die();
                        yield return null;
                    }
                    ability.Advance(100); Check(ability.TryActivate(), "Activate before transition");
                    if (passiveChoice == PG08Passive.Tenacia) passive.BaseHit();
                    else { passive.SetFireHeld(true); passive.BaseShot(); passive.Advance(5); }
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    float cd = ability.CooldownRemaining, duration = ability.ActiveRemaining, fire = passive.ContinuousFire;
                    double pause = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - pause < .2) yield return null;
                    Near(ability.CooldownRemaining, cd, "BONUS pauses CD"); Near(ability.ActiveRemaining, duration, "BONUS pauses ability duration"); Near(passive.ContinuousFire, fire, "BONUS pauses fire counter");
                    loop.Choices[0] = AreaStat.ATK; loop.SelectBonus(0); loop.ConfirmBonus();
                    Near(ability.CooldownRemaining, choice == PG08Ability.FiloSpinato ? 16 : 10, "AREA restarts active CD");
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.PG08Ability == ability && ability.Selected == choice && passive.Selected == passiveChoice, "AREA preserves choices and PG");
                    Check(!ability.EffectActive && !passive.EffectActive, "AREA clears effects"); Near(passive.ContinuousFire, 0, "AREA resets held-fire progress"); Near(passive.Hits, 0, "AREA resets TENACIA");
                    Near(actor.Stats.ATK, 7.35f, "AREA retains acquired ATK bonus");
                    Check(UnityEngine.Object.FindObjectsByType<PG08WireArea>(FindObjectsSortMode.None).Length == 0 && UnityEngine.Object.FindObjectsByType<PG08Push>(FindObjectsSortMode.None).Length == 0, "AREA removes rings and motions");
                    Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "AREA removes projectiles");
                    cd = ability.CooldownRemaining; ability.ChangeArea(); Near(ability.CooldownRemaining, cd, "Inactive CD residue preserved");
                    ability.Advance(100); ability.ChangeArea(); Near(ability.CooldownRemaining, 0, "Ready stays ready");
                    passive.SetFireHeld(true); passive.BaseShot(); passive.Advance(5); actor.Hit(10000, true);
                    Check(!passive.RageActive && passive.ContinuousFire == 0, "DOWN immediately clears RAGE"); Check(!ability.TryActivate(), "DOWN blocks Q");
                    loop.RestartTest(); while (loop.State != LoopState.Combat) yield return null;
                    Near(loop.Player.Actor.Stats.ATK, 7, "RUN resets persistent bonus"); Near(loop.PG08Passive.Hits, 0, "RUN resets hits"); Near(loop.PG08Passive.ContinuousFire, 0, "RUN resets fire progress");
                    Check(!loop.PG08Ability.EffectActive && !loop.PG08Passive.EffectActive, "RUN resets effects");
                }
                Check(loop.Player.Definition.PlayerId == "PG08", "Entire suite PG08 only");
            }
            finally { loop.Definition = original; UnityEngine.Object.Destroy(config); }
        }
        private static void GeometryChecks()
        {
            Check(PG08WireGeometry.CircleIntersects(new Vector2(6.5f, 0), 0, 6, 7, 0, 36), "Annulus middle inside");
            Check(!PG08WireGeometry.CircleIntersects(new Vector2(5.99f, 0), 0, 6, 7, 0, 36), "Annulus inner hole excluded");
            Check(!PG08WireGeometry.CircleIntersects(new Vector2(7.01f, 0), 0, 6, 7, 0, 36), "Annulus outside excluded");
            Check(PG08WireGeometry.CircleIntersects(new Vector2(5.7f, 0), .35f, 6, 7, 0, 36), "Body overlaps inner rim");
            Check(PG08WireGeometry.CircleIntersects(new Vector2(7.3f, 0), .35f, 6, 7, 0, 36), "Body overlaps outer rim");
            Check(!PG08WireGeometry.BoxIntersects(Vector2.zero, 6, 7, 0, 36, new Bounds(Vector3.zero, Vector3.one)), "Blocker in hole does not delete ring");
            Check(PG08WireGeometry.BoxIntersects(Vector2.zero, 6, 7, 0, 36, new Bounds(new Vector3(6.5f, .5f), Vector3.one * .1f)), "Tiny blocker deletes intersected sector");
            Check(!PG08WireGeometry.BoxIntersects(Vector2.zero, 6, 7, 0, 36, new Bounds(new Vector3(6.5f, -.5f), Vector3.one * .1f)), "Other sector unaffected");
            Near(AbilityCooldown.FinalSeconds(16, 10), 1.6f, "Wire min CD1.6"); Near(AbilityCooldown.FinalSeconds(10, 95), 9.5f, "Push CD modifier");
        }
        private static IEnumerator BaseChecks()
        {
            var ability = loop.PG08Ability; var passive = loop.PG08Passive;
            var target = Probe(Origin + Vector2.right * 3, defence:115); var behind = Probe(Origin + Vector2.right * 5); var ally = Probe(Origin + Vector2.right, Faction.PG);
            Projectile shot = null; Action<Projectile> capture = value => shot = value; ability.BaseShot += capture;
            var fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            float at = Time.time; Vector2 initial = shot.transform.position;
            Check(!loop.Player.GetComponent<PlayerWeapon>().TryFireAt(Origin + Vector2.right * 20), "Cadence blocks immediate repeat");
            yield return null;
            if (shot != null && Time.time > at) Near(Vector2.Distance(shot.transform.position, initial) / (Time.time - at), 20, "Physical projectile speed20", 1);
            var wait = Wait(.35f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 494, "ATK7 against DEF15% rounds to6"); Near(behind.CurrentHP, 500, "Base never pierces"); Near(ally.CurrentHP, 100, "Ally crossed without damage");
            Near(passive.Hits, passive.Selected == PG08Passive.Tenacia ? 1 : 0, "Only TENACIA counts base HIT");
            Remove(target, behind, ally); yield return null;
            fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            Near(Time.time - at, .5f, "Base actual interval0.5s", Mathf.Max(.08f, Time.deltaTime * 2));
            wait = Wait(.6f); while (wait.MoveNext()) yield return null;
            Check(shot == null, "MISS expires at range8"); Near(passive.Hits, passive.Selected == PG08Passive.Tenacia ? 1 : 0, "MISS does not add TENACIA");
            var blocker = Obstacle(Origin + Vector2.right * 2, new Vector2(.2f, 3), false); target = Probe(Origin + Vector2.right * 3);
            fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            wait = Wait(.4f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 500, "Obstacle blocks base projectile");
            UnityEngine.Object.Destroy(blocker); Remove(target); ability.BaseShot -= capture; yield return null;
            passive.ChangeArea();
        }
        private static void TenacityChecks()
        {
            var actor = loop.Player.Actor; var passive = loop.PG08Passive; var original = actor.Stats;
            passive.BaseHit(); Near(actor.EffectiveStats.DEF, 110.2f, "First HIT fractional DEF0.2");
            for (int i = 1; i < 150; i++) passive.BaseHit();
            Near(passive.Hits, 150, "TENACIA150 hits"); Near(actor.EffectiveStats.DEF, 140, "TENACIA additive cap +30");
            for (int i = 0; i < 30; i++) passive.BaseHit(); Near(actor.EffectiveStats.DEF, 140, "No accumulation past cap");
            float cd = 100; AreaStatBonus.Apply(actor, AreaStat.DEF, ref cd);
            Near(actor.Stats.DEF, 115.5f, "DEF bonus acquired on persistent value"); Near(actor.EffectiveStats.DEF, 145.5f, "TENACIA preserves acquired bonus");
            actor.Hit(0, true); Near(passive.Hits, 0, "Zero-damage HIT clears TENACIA"); Near(actor.EffectiveStats.DEF, 115.5f, "No permanent bonus lost");
            actor.SetStats(original); for (int i = 0; i < 150; i++) passive.BaseHit();
            float hp = actor.CurrentHP; actor.Hit(10, true);
            Near(hp - actor.CurrentHP, 6, "Resetting HIT uses DEF40% including TENACIA");
            Near(passive.Hits, 0, "Damaging HIT clears TENACIA after mitigation");
            Near(actor.EffectiveStats.DEF, 110, "Incoming HIT removes only TENACIA");
            hp = actor.CurrentHP; actor.Hit(10, true);
            Near(hp - actor.CurrentHP, 9, "Next HIT uses only base DEF10%"); HP(actor, 100);
            var stats = original; stats.DEF = 185; actor.SetStats(stats); for (int i = 0; i < 150; i++) passive.BaseHit();
            Near(actor.EffectiveStats.DEF, 190, "Global DEF cap90%"); hp = actor.CurrentHP; actor.Hit(10, true);
            Near(hp - actor.CurrentHP, 1, "Resetting HIT respects global DEF cap90%");
            Near(actor.EffectiveStats.DEF, 185, "After reset only persistent DEF85% remains");
            hp = actor.CurrentHP; actor.Hit(10, true); Near(hp - actor.CurrentHP, 2, "Next HIT uses persistent DEF85% with final rounding");
            Near(actor.Stats.DEF, 185, "Persistent DEF preserved after cap reset"); actor.SetStats(original); HP(actor, 100); passive.ChangeArea();
        }
        private static IEnumerator RageChecks()
        {
            var actor = loop.Player.Actor; var passive = loop.PG08Passive; var original = actor.Stats;
            passive.SetFireHeld(true); passive.Advance(10); Check(!passive.RageActive, "Holding without an initial shot does not start RAGE");
            var fire = Fire(Origin + Vector2.up * 20); while (fire.MoveNext()) yield return null;
            passive.Advance(4.99f); Check(!passive.RageActive, "RAGE below5s inactive"); passive.Advance(.01f); Check(passive.RageActive, "RAGE at5s active on MISS");
            Near(actor.EffectiveStats.ATK, 9.1f, "RAGE ATK+30%"); Near(actor.EffectiveStats.MoveSpeed, 72, "RAGE MOVE-20%");
            passive.Advance(50); Near(actor.EffectiveStats.ATK, 9.1f, "Continuous RAGE does not accumulate");
            var target = Probe(Origin + Vector2.right * 3);
            fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            var wait = Wait(.35f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 491, "RAGE actual projectile damage9 after final rounding"); Remove(target);
            float cd = 100; AreaStatBonus.Apply(actor, AreaStat.ATK, ref cd); AreaStatBonus.Apply(actor, AreaStat.MoveSpeed, ref cd);
            Near(actor.EffectiveStats.ATK, 9.45f, "RAGE keeps activation snapshot plus new bonus");
            passive.SetFireHeld(false); Near(actor.Stats.ATK, 7.35f, "RAGE release retains new ATK bonus"); Near(actor.EffectiveStats.ATK, 7.35f, "RAGE removes only its modifier"); Near(actor.EffectiveStats.MoveSpeed, 94.5f, "RAGE release retains MOVE bonus");
            passive.SetFireHeld(true); passive.BaseShot(); passive.Advance(5); Near(actor.EffectiveStats.ATK, 9.555f, "RAGE reactivation uses updated current stats");
            passive.SetStunned(true); Check(!passive.RageActive && passive.ContinuousFire == 0, "STUN clears RAGE and counter"); Check(!loop.PG08Ability.TryActivate(), "STUN blocks ability");
            passive.Advance(10); Check(!passive.RageActive, "STUN cannot accumulate fire"); passive.SetStunned(false); passive.Advance(5); Check(!passive.RageActive, "STUN recovery requires new shot");
            passive.BaseShot(); passive.Advance(5); passive.ChangeArea(); passive.Advance(5); Check(!passive.RageActive, "AREA held LMB requires fresh fire count");
            passive.SetFireHeld(false); actor.SetStats(original); HP(actor, 100);
            wait = Wait(.6f); while (wait.MoveNext()) yield return null;
        }
        private static PG08WireArea Ring(Vector2 point)
        {
            var go = new GameObject("PG08 test ring"); go.transform.SetParent(TestVisuals.Root); go.transform.position = point;
            var ring = go.AddComponent<PG08WireArea>(); ring.Initialize(loop, loop.Player.Actor, loop.Definition.PG08Abilities); return ring;
        }
        private static IEnumerator WireChecks()
        {
            var ability = loop.PG08Ability; var passive = loop.PG08Passive;
            if (passive.Selected == PG08Passive.Rage) { passive.SetFireHeld(true); passive.BaseShot(); passive.Advance(5); }
            var middle = Probe(Origin + AttackGeometry.Direction(18) * 6.5f, defence:115);
            var hole = Probe(Origin + Vector2.up * 5); var outside = Probe(Origin + Vector2.up * 8); var ally = Probe(Origin + Vector2.up * 6.5f, Faction.PG);
            ability.Advance(100); Check(ability.TryActivate(), "Wire activation");
            var ring = UnityEngine.Object.FindFirstObjectByType<PG08WireArea>(); Check(ring != null && ring.ValidSections == 10, "All10 valid sections");
            Near(middle.CurrentHP, 500, "Wire has no immediate damage tick"); Near(middle.MovementMetresPerSecond, 1.5f, "Wire slow immediate"); Near(hole.MovementMetresPerSecond, 2, "Hole has no slow");
            loop.Player.transform.position = Origin + Vector2.left * 3; Near(Vector2.Distance(ring.transform.position, Origin), 0, "Ring does not follow PG");
            var overlapping = Ring(Origin); Near(middle.MovementMetresPerSecond, 1.5f, "Overlapping SLOW stays25%");
            var wait = Wait(1.1f); while (wait.MoveNext()) yield return null;
            Near(middle.CurrentHP, 491, "One shared tick10 DEF15% rounds9"); Near(hole.CurrentHP, 500, "Hole no damage"); Near(outside.CurrentHP, 500, "Outside no damage"); Near(ally.CurrentHP, 100, "Ally no damage"); Near(passive.Hits, 0, "Wire HIT does not add TENACIA");
            middle.transform.position = Origin; yield return null; Near(middle.MovementMetresPerSecond, 2, "Exit immediately clears SLOW");
            wait = Wait(.25f); while (wait.MoveNext()) yield return null;
            middle.transform.position = Origin + AttackGeometry.Direction(18) * 6.5f; yield return null;
            wait = Wait(.7f); while (wait.MoveNext()) yield return null; Near(middle.CurrentHP, 491, "Reentry restarts one-second clock");
            wait = Wait(.4f); while (wait.MoveNext()) yield return null; Near(middle.CurrentHP, 482, "Reentry gets its own tick");
            ring.End(); overlapping.End(); Near(middle.MovementMetresPerSecond, 2, "Ring removal clears slow");
            Remove(middle, hole, outside, ally); loop.Player.transform.position = Origin; yield return null;
            // Exact full lifetime includes ticks after each completed second: 1, 2, 3.
            middle = Probe(Origin + Vector2.right * 6.5f); ring = Ring(Origin);
            wait = Wait(3.15f); while (wait.MoveNext()) yield return null; Near(middle.CurrentHP, 470, "Wire three ticks over3s"); Near(middle.MovementMetresPerSecond, 2, "Natural expiry clears slow"); Remove(middle); yield return null;
            var blocker = Obstacle(Origin + AttackGeometry.Direction(18) * 6.5f, Vector2.one * .1f, false);
            ring = Ring(Origin); Near(ring.ValidSections, 9, "Partial overlap removes entire36-degree section");
            middle = Probe(Origin + AttackGeometry.Direction(25) * 6.5f); Check(!ring.Contains(middle), "Deleted section has no contact away from blocker");
            ring.End(); Remove(middle); UnityEngine.Object.Destroy(blocker); yield return null;
            blocker = Obstacle(Origin, Vector2.one * 16, true); ability.Advance(100); Check(ability.TryActivate(), "Zero-section cast still used");
            Near(ability.CooldownRemaining, 16, "Zero-section cast starts full CD"); Check(!ability.EffectActive, "Zero-section ring not active");
            UnityEngine.Object.Destroy(blocker); yield return null;
            ability.Advance(100); ReducedCD(); Check(ability.TryActivate(), "Reduced-CD wire cast"); ability.Advance(1.6f); Check(ability.TryActivate(), "Reduced CD allows overlapping rings");
            blocker = Obstacle(Origin, Vector2.one * 16, true); ability.Advance(1.6f); Check(ability.TryActivate(), "Zero-section cast while prior rings exist");
            Check(ability.EffectActive && ability.DurationRemaining > 0, "Zero-section cast preserves HUD state of prior rings");
            UnityEngine.Object.Destroy(blocker); ability.ChangeArea();
            typeof(LoopSession).GetField("cdReduction", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(loop, 100f);
        }
        private static IEnumerator PushChecks()
        {
            var ability = loop.PG08Ability; var target = Probe(Origin + Vector2.right * 3);
            ability.Advance(100); Check(ability.TryActivate(), "Push ability activates"); Near(ability.CooldownRemaining, 0, "Push CD waits until end"); Check(!ability.TryActivate(), "Cannot reactivate during3s");
            var fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            while (target.CurrentHP == 500) yield return null;
            Check(target.transform.position.x < Origin.x + 4.5f, "Push not instantaneous");
            var wait = Wait(.35f); while (wait.MoveNext()) yield return null;
            Near(target.CurrentHP, 493, "Push keeps base damage7"); Near(target.transform.position.x, Origin.x + 4.5f, "First HIT pushes1.5m", .02f);
            fire = Fire(Origin + Vector2.right * 20); while (fire.MoveNext()) yield return null;
            wait = Wait(.65f); while (wait.MoveNext()) yield return null; Near(target.CurrentHP, 486, "Second HIT normal damage"); Near(target.transform.position.x, Origin.x + 6, "Second HIT adds full independent1.5m", .02f);
            ability.Advance(100); ability.Advance(100); Check(ability.TryActivate(), "Push available after CD"); ability.Advance(2.99f); Near(ability.CooldownRemaining, 0, "CD not running before3s"); ability.Advance(.01f); Near(ability.CooldownRemaining, 10, "CD begins exactly after3s", .001f);
            ability.Advance(2); float cd = ability.CooldownRemaining; ability.ChangeArea(); Near(ability.CooldownRemaining, cd, "Push inactive AREA retains CD");
            target.transform.position = Origin + Vector2.right * 3;
            var motion = ability.Push(target, Vector2.right); motion.enabled = false; motion.Advance(.125f); Near(target.transform.position.x, Origin.x + 4.125f, "Half-time eased distance1.125", .002f); motion.Advance(.125f); Near(target.transform.position.x, Origin.x + 4.5f, "Full-time distance1.5", .002f);
            target.transform.position = Origin + Vector2.right * 3;
            var first = ability.Push(target, Vector2.right); var second = ability.Push(target, Vector2.right); first.enabled = second.enabled = false;
            first.Advance(.25f); second.Advance(.25f); Near(target.transform.position.x, Origin.x + 6, "Simultaneous HIT motions remain independent", .002f);
            target.transform.position = Origin + Vector2.right * 3;
            var blocker = Obstacle(Origin + Vector2.right * 4, new Vector2(.2f, 4), true);
            motion = ability.Push(target, Vector2.right); motion.enabled = false; motion.Advance(.25f); Near(target.transform.position.x, Origin.x + 3.549f, "Wall stops push at body boundary", .01f);
            UnityEngine.Object.Destroy(blocker); Remove(target); yield return null;
        }
    }
}
