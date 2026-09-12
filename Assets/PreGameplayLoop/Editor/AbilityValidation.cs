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
    public static class AbilityValidation
    {
        private const string Key = "ROG.PG01Abilities.";
        private static readonly string Request = Path.GetFullPath(".pg01-abilities-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static AbilityValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG01 ability tests")]
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
                    loop.Player.GetComponent<PG01AbilityInput>().enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. Uses runtime ability commands; physical keyboard/mouse feel remains a manual check. PET collision uses a physical probe because PET gameplay is outside this prototype.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG01-abilities-validation.txt"));
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

        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            var originalDefinition = loop.Definition;
            var testDefinition = UnityEngine.Object.Instantiate(originalDefinition);
            loop.Definition = testDefinition;
            testDefinition.SelectedAbility = PG01Ability.Pestone;
            loop.RestartTest();
            while (loop.State != LoopState.Combat) yield return null;
            // Place probes in a clear part of the actual AREA without touching saved scene geometry.
            Vector2 origin = new Vector2(30, 30);
            loop.Player.transform.position = origin;
            var ability = loop.Ability;
            Check(ability.Selected == PG01Ability.Pestone, "RUN selects PESTONE only");
            Check(!ability.TryPestone(origin + Vector2.right), "PESTONE unavailable during initial CD");
            Check(ability.CooldownRemaining > 9, "PESTONE starts RUN with full 10-second CD");
            Check(!ability.BeginAim(origin), "BARRIERA unavailable when PESTONE is selected");
            testDefinition.SelectedAbility = PG01Ability.Barriera;
            Check(ability.Selected == PG01Ability.Pestone, "Inspector choice cannot switch current RUN ability");

            var timer = new AbilityCooldown();
            timer.Restart(10, 100); timer.Tick(3); timer.ChangeArea(false, 10, 95);
            Near(timer.Remaining, 7, "Inactive CD preserves exact residue across AREA");
            timer.Tick(7); timer.ChangeArea(false, 10, 100);
            Check(timer.Ready, "Available ability stays available across AREA");
            timer.Restart(15, 100); timer.Tick(2); timer.ChangeArea(true, 15, 95);
            Near(timer.Remaining, 14.3f, "Active ability restarts full CD with current reduction");
            Near(AbilityCooldown.FinalSeconds(15, 94.5f), 14.3f, "Stat and seconds use mathematical rounding");
            Near(AbilityCooldown.FinalSeconds(15, 1), 1.5f, "CD REDUCTION minimum 10");

            while (ability.CooldownRemaining > 0) yield return null;
            Near(testDefinition.PG01Abilities.PestoneDepth, 7, "PESTONE depth asset = 7m");
            Near(testDefinition.PG01Abilities.PestoneWidth, 3, "PESTONE frontal width asset = 3m");
            Near(testDefinition.PG01Abilities.PestoneSlowPercent, 30, "PESTONE SLOW asset = 30 percent");
            Near(testDefinition.PG01Abilities.PestoneSlowDuration, 3, "PESTONE SLOW duration asset = 3s");
            Vector2[] offsets = { new Vector2(6.5f,0), new Vector2(7,1.5f), new Vector2(7,-1.5f), new Vector2(7.01f,0),
                new Vector2(7,1.51f), new Vector2(-.01f,0), new Vector2(.5f,1.49f), new Vector2(.5f,-1.49f),
                new Vector2(0,1.5f), new Vector2(3,-1.51f) };
            bool[] expected = { true, true, true, false, false, false, true, true, true, false };
            var probes = new List<Combatant>();
            foreach (var offset in offsets) probes.Add(Probe(origin + offset));
            var ally = Probe(origin + Vector2.right, Faction.PG);
            int colliderCount = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length;
            int projectileCount = UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length;
            Check(ability.TryPestone(origin + Vector2.right * 8), "PESTONE activation succeeds");
            for (int i = 0; i < probes.Count; i++) Near(probes[i].CurrentHP, expected[i] ? 60 : 100, "PESTONE rectangle 7m-depth/3m-width boundary " + offsets[i]);
            for (int i = 0; i < probes.Count; i++)
                Near(probes[i].MovementMetresPerSecond, expected[i] ? 1.4f : 2f, "SLOW only inside rectangle " + offsets[i]);
            Near(ally.CurrentHP, 100, "PESTONE does not damage PG");
            Near(ally.MovementMetresPerSecond, 2, "PESTONE does not slow PG");
            Near(ability.CooldownRemaining, 10, "PESTONE CD begins on activation");
            Check(!ability.TryPestone(origin + Vector2.right), "PESTONE repeated activation rejected during CD");
            Check(UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length == colliderCount &&
                UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == projectileCount, "PESTONE adds no physical collider/projectile");
            foreach (var probe in probes) UnityEngine.Object.Destroy(probe.gameObject);
            UnityEngine.Object.Destroy(ally.gameObject);
            yield return null;

            // The whole rectangle rotates with aim, including its constant near-edge width.
            Vector2 rotatedForward = AttackGeometry.Direction(37);
            Vector2 rotatedSide = new Vector2(-rotatedForward.y, rotatedForward.x);
            var rotatedInside = Probe(origin + rotatedForward * .5f + rotatedSide * 1.49f);
            var rotatedOutside = Probe(origin + rotatedForward * 3 + rotatedSide * 1.51f);
            PestoneEffect.Cast(origin, rotatedForward, testDefinition.PG01Abilities);
            Near(rotatedInside.CurrentHP, 60, "Rotated rectangle hits near-side target excluded by the old cone");
            Near(rotatedOutside.CurrentHP, 100, "Rotated rectangle excludes target beyond side edge");
            Near(rotatedInside.MovementMetresPerSecond, 1.4f, "Rotated rectangle applies SLOW");
            Near(rotatedOutside.PestoneSlowRemaining, 0, "Rotated outside target has no SLOW");
            UnityEngine.Object.Destroy(rotatedInside.gameObject);
            UnityEngine.Object.Destroy(rotatedOutside.gameObject);
            yield return null;

            foreach (bool wall in new[] { true, false })
            {
                var blocker = Obstacle(origin + new Vector2(2, 0), new Vector2(.3f, .3f), wall);
                var shielded = Probe(origin + new Vector2(4, 0));
                var exposed = Probe(origin + new Vector2(5, 1.4f));
                var armoured = Probe(origin + new Vector2(4, -1), Faction.MOB, 115);
                var fractional = Probe(origin + new Vector2(4, -1.1f), Faction.MOB, 111.25f);
                PestoneEffect.Cast(origin, Vector2.right, testDefinition.PG01Abilities);
                Near(shielded.CurrentHP, 100, (wall ? "MURO" : "OSTACOLO") + " shields MOB from PESTONE");
                Near(shielded.PestoneSlowRemaining, 0, "Shielded MOB receives no SLOW");
                Near(exposed.MovementMetresPerSecond, 1.4f, "Unshielded MOB receives SLOW");
                Near(exposed.CurrentHP, 60, "Unshielded part of PESTONE stays effective");
                Near(armoured.CurrentHP, 66, "PESTONE passes 40 damage through existing DEF evaluation");
                Near(fractional.CurrentHP, 64, "PESTONE rounds final mitigated damage 35.5 to 36");
                UnityEngine.Object.Destroy(blocker); UnityEngine.Object.Destroy(shielded.gameObject);
                UnityEngine.Object.Destroy(exposed.gameObject); UnityEngine.Object.Destroy(armoured.gameObject);
                UnityEngine.Object.Destroy(fractional.gameObject);
                yield return null;
            }

            var slowed = Probe(origin + Vector2.right);
            var movementStats = slowed.Stats;
            var slowBrain = slowed.gameObject.AddComponent<MobBrain>();
            slowBrain.Initialize(loop.Settings.Mobs[0], loop.Navigation);
            slowBrain.enabled = false;
            slowed.Initialize(Faction.MOB, movementStats);
            movementStats.MoveSpeed = 150;
            slowed.SetStats(movementStats);
            PestoneEffect.Cast(origin, Vector2.right, testDefinition.PG01Abilities);
            Near(slowed.PestoneSlowRemaining, 3, "SLOW starts with 3 simulation seconds");
            Near(slowed.Stats.MoveSpeed, 150, "SLOW preserves existing MOVE SPD modifier");
            Near(slowed.MovementMetresPerSecond, 2.1f, "SLOW reduces current speed by 30 percent");
            Vector2 beforeMove = slowed.transform.position;
            slowed.Move(Vector2.up * slowed.MovementMetresPerSecond * .5f);
            Near(Vector2.Distance(beforeMove, slowed.transform.position), 1.05f, "Actual movement uses slowed speed");
            beforeMove = slowed.transform.position;
            // Exercise the real AI movement path once, without its automatic attack/target selection.
            slowBrain.SendMessage("Follow", beforeMove + Vector2.up * 10);
            Near(Vector2.Distance(beforeMove, slowed.transform.position), 2.1f * Time.deltaTime,
                "MOB AI movement uses 70 percent of current speed");
            float slowStarted = Time.time;
            while (Time.time - slowStarted < 1f) yield return null;
            PestoneEffect.Cast(origin, Vector2.right, testDefinition.PG01Abilities);
            Near(slowed.PestoneSlowRemaining, 3, "Reapplication refreshes full SLOW duration");
            Near(slowed.MovementMetresPerSecond, 2.1f, "Reapplication does not stack SLOW");
            movementStats.MoveSpeed = 200;
            slowed.SetStats(movementStats);
            Near(slowed.MovementMetresPerSecond, 2.8f, "MOVE SPD changes remain independent during SLOW");
            float originalScale = Time.timeScale;
            Time.timeScale = 0;
            double slowPauseStarted = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - slowPauseStarted < .2) yield return null;
            Near(slowed.PestoneSlowRemaining, 3, "Pause freezes SLOW duration");
            Time.timeScale = originalScale;
            float refreshedAt = Time.time;
            while (Time.time - refreshedAt < 2.8f) yield return null;
            Near(slowed.MovementMetresPerSecond, 2.8f, "SLOW survives original expiry after refresh");
            while (Time.time - refreshedAt < 3.05f) yield return null;
            Near(slowed.PestoneSlowRemaining, 0, "SLOW expires after refreshed 3 seconds");
            Near(slowed.MovementMetresPerSecond, 4, "Expiry restores current speed including later modifiers");
            Near(slowed.Stats.MoveSpeed, 200, "Expiry does not overwrite MOVE SPD");
            slowed.ApplyPestoneSlow(30, 3);
            slowed.Initialize(Faction.MOB, movementStats);
            Near(slowed.PestoneSlowRemaining, 0, "Actor initialization clears old SLOW");
            UnityEngine.Object.Destroy(slowed.gameObject);
            yield return null;

            // Actual loop transition while an inactive CD is paused in BONUS.
            while (ability.CooldownRemaining > 0) yield return null;
            Check(ability.TryPestone(origin + Vector2.up), "PESTONE second use after elapsed CD");
            foreach (var actor in Combatant.All.ToArray()) if (actor.Faction == Faction.MOB && actor.GetComponent<MobBrain>() != null) actor.Die();
            while (loop.State != LoopState.AreaComplete)
            {
                foreach (var actor in Combatant.All.ToArray()) if (actor.Faction == Faction.MOB && actor.IsActive) actor.Die();
                yield return null;
            }
            loop.Player.transform.position = loop.Exit.transform.position;
            while (loop.State != LoopState.Bonus) yield return null;
            float pausedCd = ability.CooldownRemaining;
            double pause = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - pause < .2) yield return null;
            Near(ability.CooldownRemaining, pausedCd, "BONUS freezes CD");
            Check(!ability.TryPestone(origin), "BONUS blocks ability activation");
            loop.SelectBonus(0); loop.ConfirmBonus();
            Near(ability.CooldownRemaining, pausedCd, "AREA transition keeps exact inactive residual CD");
            while (loop.State != LoopState.Combat) yield return null;
            Check(loop.Ability == ability && ability.Selected == PG01Ability.Pestone, "Ability selection/runtime persists into AREA 2");
            foreach (var actor in Combatant.All)
                if (actor.Faction == Faction.MOB) Near(actor.PestoneSlowRemaining, 0, "New AREA MOB has no inherited SLOW");

            loop.RestartTest();
            while (loop.State != LoopState.Combat) yield return null;
            ability = loop.Ability;
            loop.Player.transform.position = origin;
            Check(ability.Selected == PG01Ability.Barriera, "New RUN selects BARRIERA only");
            Check(ability.CooldownRemaining > 14 && !ability.BeginAim(origin), "BARRIERA starts RUN in 15-second CD");
            Check(!ability.TryPestone(origin), "PESTONE unavailable when BARRIERA selected");
            while (ability.CooldownRemaining > 0) yield return null;
            Check(ability.BeginAim(origin + Vector2.right * 20), "Q hold begins barrier preview");
            Near(Vector2.Distance(origin, ability.PreviewCentre), 8, "Barrier centre clamps at 8m");
            Near(ability.PreviewAngle, 90, "7m side is perpendicular to PG-cursor direction");
            Check(ability.Barriers.Count == 0 && ability.CooldownRemaining == 0, "Preview has no barrier and consumes no CD");
            ability.AimAt(origin + new Vector2(4, 4));
            Near(ability.PreviewAngle, 135, "Preview follows diagonal cursor");
            ability.CancelAim();
            Check(!ability.IsAiming && ability.CooldownRemaining == 0, "Cancel preview costs no CD");

            Vector2 centre = origin + Vector2.right * 4;
            var mob = Probe(centre);
            loop.Player.transform.position = centre;
            var pet = new GameObject("PET collision probe");
            pet.transform.SetParent(loop.transform);
            pet.layer = LayerMask.NameToLayer("PET");
            pet.transform.position = centre + Vector2.up;
            var petCollider = pet.AddComponent<CircleCollider2D>(); petCollider.radius = .25f;
            var petBody = pet.AddComponent<Rigidbody2D>(); petBody.gravityScale = 0; petBody.freezeRotation = true;
            var overlapWall = Obstacle(centre, new Vector2(.25f, .25f), true);
            Check(ability.BeginAim(centre), "Preview permits overlapping PG/MOB/PET");
            Check(ability.ReleaseAim(centre), "Q release places barrier despite overlapping actors");
            Near(ability.CooldownRemaining, 15, "BARRIERA CD begins on release/spawn");
            var barrier = ability.Barriers[0];
            var box = barrier.GetComponent<BoxCollider2D>();
            Near(barrier.Remaining, 5, "Barrier starts with full 5-second duration");
            Near(box.size.x, 7, "Barrier long side 7m"); Near(box.size.y, 1, "Barrier depth 1m");
            Check(barrier.gameObject.layer == LayerMask.NameToLayer("OSTACOLO") && !box.isTrigger, "Barrier is physical OSTACOLO");
            Check(barrier.GetComponent<Combatant>() == null, "Barrier has no HP/damage receiver");
            Check(Physics2D.Distance(overlapWall.GetComponent<Collider2D>(), box).isOverlapped, "Barrier placement permits overlap with MURO");
            Check(!loop.Navigation.Sample(centre, out _, .1f), "Navigation excludes active barrier centre");
            foreach (var body in new Collider2D[] { loop.Player.GetComponent<Collider2D>(), mob.GetComponent<Collider2D>(), petCollider })
                Check(!Physics2D.Distance(body, box).isOverlapped, "Overlapping " + body.gameObject.layer + " displaced outside barrier");
            Check(!ability.BeginAim(centre), "BARRIERA repeat blocked during CD");
            foreach (string layer in new[] { "PG", "MOB", "PET", "PROJECTILE_PG", "PROJECTILE_MOB" })
                Check(!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("OSTACOLO"), LayerMask.NameToLayer(layer)), "OSTACOLO matrix blocks " + layer);

            // Place actors on the exterior of the actual rotated collider and attempt to cross it.
            Vector2 normal = barrier.transform.up;
            loop.Player.transform.position = centre + normal * 2;
            mob.transform.position = centre - normal * 2;
            petBody.position = centre + (Vector2)barrier.transform.right * 2 + normal * 2;
            loop.Player.Actor.Move(-normal * 4);
            mob.Move(normal * 4);
            Check(Vector2.Dot((Vector2)loop.Player.transform.position - centre, normal) > .5f, "PG cannot cross BARRIERA");
            Check(Vector2.Dot((Vector2)mob.transform.position - centre, normal) < -.5f, "MOB cannot cross BARRIERA");
            petBody.linearVelocity = -normal * 6;
            TestVisuals.SpawnProjectile(loop.Player.Actor, centre + normal * 2, -normal, 20, 10, 40, .06f, 0, false);
            TestVisuals.SpawnProjectile(mob, centre - normal * 2, normal, 20, 10, 40, .06f, 0, false);
            float initialLifetime = barrier.Remaining;
            float waitUntil = Time.time + .5f;
            while (Time.time < waitUntil) yield return null;
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == 0, "Both PG/MOB projectiles stop at barrier");
            float petSide = Vector2.Dot(petBody.position - centre, normal);
            float petContactDistance = Physics2D.Distance(petCollider, box).distance;
            // Dynamic contacts allow a tiny solver penetration; centre crossing is never allowed.
            Check(petSide > .5f && petContactDistance >= -Physics2D.defaultContactOffset,
                $"Physical PET probe blocked: side={petSide:0.####}, contact={petContactDistance:0.####}, tolerance={Physics2D.defaultContactOffset:0.####}");
            petBody.linearVelocity = Vector2.zero;
            UnityEngine.Object.Destroy(overlapWall);
            Check(barrier != null && barrier.Remaining < initialLifetime, "Barrier lifetime counts during gameplay");
            Time.timeScale = 0;
            float frozenLifetime = barrier.Remaining; pausedCd = ability.CooldownRemaining;
            pause = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - pause < .2) yield return null;
            Near(barrier.Remaining, frozenLifetime, "Pause freezes barrier duration");
            Near(ability.CooldownRemaining, pausedCd, "Pause freezes barrier CD");
            Time.timeScale = 1;
            while (ability.EffectActive) yield return null;
            Check(barrier == null || !barrier.gameObject.activeSelf, "Barrier disappears after 5 simulation seconds");
            Check(loop.Navigation.Sample(centre, out _, .1f), "Navigation restores traversable space after barrier expiry");
            Check(ability.CooldownRemaining > 9 && ability.CooldownRemaining < 10.2f, "Duration and CD run concurrently (about 10s CD left on expiry)");
            UnityEngine.Object.Destroy(mob.gameObject); UnityEngine.Object.Destroy(pet);
            yield return null;
            loop.Player.transform.position = centre + normal * 2;
            loop.Player.Actor.Move(-normal * 4);
            Check(Vector2.Dot((Vector2)loop.Player.transform.position - centre, normal) < 0, "Expired barrier no longer blocks PG");

            var edgeActor = Probe(new Vector2(loop.Settings.AreaSize.x * .5f - .2f, 40));
            var edgeBarrier = BarrierEffect.Spawn(edgeActor.transform.position, 90, testDefinition.PG01Abilities);
            edgeBarrier.DisplaceOverlappingActors(loop.Settings.AreaSize);
            Check(Mathf.Abs(edgeActor.transform.position.x) + edgeActor.Radius < loop.Settings.AreaSize.x * .5f &&
                !Physics2D.Distance(edgeActor.GetComponent<Collider2D>(), edgeBarrier.GetComponent<Collider2D>()).isOverlapped,
                "Boundary overlap expels actor inside AREA, outside barrier");
            edgeBarrier.Remove(); UnityEngine.Object.Destroy(edgeActor.gameObject);

            while (ability.CooldownRemaining > 0) yield return null;
            // Test-only stat injection; gameplay exposes no cheat or mid-RUN ability switch.
            typeof(LoopSession).GetField("cdReduction", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(loop, 10f);
            loop.Player.transform.position = origin;
            Check(ability.BeginAim(centre) && ability.ReleaseAim(centre), "Barrier can activate again after cooldown");
            Near(ability.CooldownRemaining, 1.5f, "Runtime BARRIERA applies CD REDUCTION 10");
            barrier = ability.Barriers[0];
            while (ability.CooldownRemaining > 0) yield return null;
            Check(ability.BeginAim(origin + Vector2.up * 4) && ability.ReleaseAim(origin + Vector2.up * 4), "Reduced CD permits another barrier while first remains active");
            Check(ability.Barriers.Count == 2 && barrier.Remaining > 0, "Each simultaneous barrier retains independent lifetime");
            // Finish A1 fast and exercise the real active-effect transition path.
            while (loop.State != LoopState.AreaComplete)
            {
                foreach (var actor in Combatant.All.ToArray()) if (actor.Faction == Faction.MOB && actor.IsActive) actor.Die();
                yield return null;
            }
            loop.Player.transform.position = loop.Exit.transform.position;
            while (loop.State != LoopState.Bonus) yield return null;
            Check(ability.EffectActive, "Barrier still active at AREA exit");
            loop.SelectBonus(0); loop.ConfirmBonus();
            Check(!ability.EffectActive && (barrier == null || !barrier.gameObject.activeSelf), "AREA change removes active barrier immediately");
            Near(ability.CooldownRemaining, ability.FinalCooldown, "AREA change restarts full current final CD for active barrier");
            while (loop.State != LoopState.Combat) yield return null;
            Check(loop.AreaIndex == 1 && loop.Ability == ability, "Barrier selection persists in AREA 2");
            loop.Player.Actor.Hit(10000);
            yield return null;
            Check(!ability.BeginAim(centre) && !ability.TryPestone(centre), "DOWN/defeat prevents ability use");
            loop.Definition = originalDefinition;
            UnityEngine.Object.Destroy(testDefinition);
        }
    }
}
