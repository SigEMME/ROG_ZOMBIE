using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    // Opt-in engine smoke test: no gameplay cheats are added to the scene.
    [InitializeOnLoad]
    public static class LoopValidation
    {
        private const string Key = "ROG.PreLoop.";
        private const string ScenePath = "Assets/Scenes/PreGameplayLoopPrototype.unity";
        private static readonly string Request = Path.GetFullPath(".pre-gameplay-loop-test.request");
        private static int step, assertions;
        private static double started, pausedAt;
        private static float hpAfterBonus, atkAfterBonus, pausedTime;
        private static float pausedSprint;
        private static Vector3 pausedPosition;
        private static LoopSession loop;
        private static bool configured;

        static LoopValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayState;
            Application.logMessageReceived += OnLog;
        }

        private static void Check(bool value, string message)
        {
            assertions++;
            if (!value) throw new InvalidOperationException(message);
        }

        private static void CheckMobGrowth()
        {
            var sequence = UnityEngine.Object.Instantiate(loop.Definition);
            try
            {
                sequence.BossAreaNumbers = new[] { 4, 9, 15, 21, 28 };
                Check(sequence.OrdinaryAreasBefore(0) == 0, "No growth at RUN start");
                Check(sequence.OrdinaryAreasBefore(3) == 3 && sequence.OrdinaryAreasBefore(4) == 3,
                    "BOSS01 AREA adds no growth before CITY 2");
                Check(sequence.OrdinaryAreasBefore(9) == 7 && sequence.OrdinaryAreasBefore(15) == 12 &&
                    sequence.OrdinaryAreasBefore(21) == 17, "Growth continues across cities excluding each BOSS AREA");
                Check(sequence.OrdinaryAreasBefore(26) == 22 && sequence.OrdinaryAreasBefore(28) == 23,
                    "Full RUN counts 23 ordinary AREAS and excludes all five BOSS AREAS");
                Check(MobDefinition.ScaleForRun(100, sequence.OrdinaryAreasBefore(4)) == 115,
                    "CITY 2 first AREA receives +15 percent, not +20 percent");
            }
            finally { UnityEngine.Object.DestroyImmediate(sequence); }
            Check(MobDefinition.ScaleForRun(100, 0) == 100 && MobDefinition.ScaleForRun(100, 1) == 105 &&
                MobDefinition.ScaleForRun(100, 2) == 110, "Growth is linear from base, not compounded");
            Check(MobDefinition.ScaleForRun(100, 3) == 115 && MobDefinition.ScaleForRun(100, 22) == 210,
                "Global AREA count continues across cities through the last RUN area");
            Check(MobDefinition.ScaleForRun(25, 1) == 26 && MobDefinition.ScaleForRun(50, 1) == 53,
                "Round fractions below half down and exact half up");
            Check(MobDefinition.ScaleForRun(90, 3) == 104, "Decimal midpoint 103.5 rounds up without floating-point drift");
            for (int type = 0; type < 5; type++)
            {
                var definition = loop.Definition.MobAt(type);
                var original = definition.BaseStats;
                var go = new GameObject("Growth validation MOB");
                go.AddComponent<SpriteRenderer>();
                GameObject pg = null, otherMob = null;
                try
                {
                    go.transform.position = new Vector2(500, 500);
                    var actor = go.AddComponent<Combatant>();
                    var brain = go.AddComponent<MobBrain>();
                    brain.Initialize(definition, loop.Navigation, 3);
                    var actual = actor.Stats;
                    Check(actual.HP == Mathf.Floor(original.HP * 1.15f + .5f) && actor.CurrentHP == actual.HP &&
                        actual.ATK == Mathf.Floor(original.ATK * 1.15f + .5f) &&
                        actual.MoveSpeed == Mathf.Floor(original.MoveSpeed * 1.15f + .5f) &&
                        actual.AttackSpeed == Mathf.Floor(original.AttackSpeed * 1.15f + .5f), "Scaled runtime stats for " + definition.Kind);
                    Check(actual.DEF == original.DEF && actual.Range == original.Range && definition.BaseStats.Equals(original),
                        "DEF/RANGE and shared base asset preserved for " + definition.Kind);
                    if (type != 4) continue;
                    pg = new GameObject("Explosion PG probe"); otherMob = new GameObject("Explosion MOB probe");
                    pg.transform.position = go.transform.position + Vector3.right;
                    otherMob.transform.position = go.transform.position + Vector3.left;
                    var victimPG = pg.AddComponent<Combatant>(); var victimMob = otherMob.AddComponent<Combatant>();
                    var health = new CombatStats { HP = 1000, DEF = 100 };
                    victimPG.Initialize(Faction.PG, health); victimMob.Initialize(Faction.MOB, health);
                    actor.Hit(10000);
                    typeof(MobBrain).GetField("explosionAt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(brain, Time.time);
                    go.SendMessage("Update");
                    Check(victimPG.CurrentHP == 1000 - Mathf.Floor(definition.ExplosionDamagePG * 1.15f + .5f) &&
                        victimMob.CurrentHP == 1000 - Mathf.Floor(definition.ExplosionDamageMOB * 1.15f + .5f),
                        "Actual ZOMB05 explosion scales damage against both PG and MOB");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(go);
                    if (pg != null) UnityEngine.Object.DestroyImmediate(pg);
                    if (otherMob != null) UnityEngine.Object.DestroyImmediate(otherMob);
                }
            }
        }

        private static void CheckSprint()
        {
            var sprint = loop.Sprint;
            var actor = loop.Player.Actor;
            var original = actor.Stats;
            var advance = typeof(SprintRuntime).GetMethod("Advance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Action<float> tick = seconds => advance.Invoke(sprint, new object[] { seconds });
            var invisibility = actor.gameObject.AddComponent<PG05Invisibility>();
            invisibility.Apply(loop, 4, 15);
            Check(actor.IsInvisible && sprint.TryActivate() && !actor.IsInvisible, "SHIFT action interrupts PG05 invisibility");
            Check(sprint.ActiveRemaining == 2 && sprint.CooldownRemaining == 0 && !sprint.TryActivate(), "Two second sprint without stacking or early recovery");
            Check(Mathf.Approximately(actor.MovementMetresPerSecond, original.MetresPerSecond * 2), "Sprint doubles current speed after invisibility cancellation");
            var upgraded = original; upgraded.MoveSpeed += 20; actor.SetStats(upgraded);
            Check(Mathf.Approximately(actor.MovementMetresPerSecond, upgraded.MetresPerSecond * 2), "MOVE SPD acquired during sprint is applied dynamically");
            tick(1.5f); Check(sprint.Active && sprint.CooldownRemaining == 0, "No cooldown before effect expires");
            tick(.5f); Check(!sprint.Active && sprint.CooldownRemaining == 15 && actor.Stats.Equals(upgraded), "Expiry starts fifteen second recovery and preserves persistent stats");
            Check(Mathf.Approximately(actor.MovementMetresPerSecond, upgraded.MetresPerSecond), "Expiry removes only sprint multiplier");
            tick(14.5f); Check(!sprint.TryActivate(), "Reactivation blocked before recovery completes");
            tick(.5f); Check(sprint.TryActivate(), "Reactivation available exactly after fifteen seconds");
            sprint.ChangeArea(); Check(!sprint.Active && sprint.CooldownRemaining == 15, "AREA change ends active sprint and starts recovery");
            tick(3); sprint.ChangeArea(); Check(sprint.CooldownRemaining == 12, "AREA change preserves existing recovery");
            tick(12); sprint.ChangeArea(); Check(sprint.CooldownRemaining == 0, "AREA change preserves ready sprint");
            float previousTimeScale = Time.timeScale; Time.timeScale = 0;
            Check(!sprint.TryActivate(), "Paused gameplay blocks activation"); Time.timeScale = previousTimeScale;
            var cdField = typeof(LoopSession).GetField("cdReduction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            float previousCd = loop.CdReduction; cdField.SetValue(loop, 10f);
            Check(sprint.TryActivate(), "Sprint available with max CD REDUCTION"); tick(2);
            Check(sprint.CooldownRemaining == 15, "CD REDUCTION does not reduce recovery"); cdField.SetValue(loop, previousCd);
            tick(15);
            var smoke = new GameObject("Sprint SMOKE validation"); smoke.transform.position = actor.transform.position;
            smoke.AddComponent<ItemArea>().Initialize(loop, actor, PrototypeItem.Smoke, 0);
            Check(actor.IsInvisible && sprint.TryActivate() && !actor.IsInvisible, "Sprint interrupts SMOKE and invokes reacquisition delay");
            UnityEngine.Object.DestroyImmediate(smoke);
            tick(17); actor.SetStats(original); UnityEngine.Object.DestroyImmediate(invisibility);
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run engine smoke test")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            string output = SessionState.GetString(Key + "Report", "");
            if (string.IsNullOrEmpty(output))
                SessionState.SetString(Key + "Report", Path.GetFullPath("PreGameplayLoop-validation.txt"));
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Salvare la scena e lasciare una sola scena aperta prima del test."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene(ScenePath);
            SessionState.SetBool(Key + "Running", true);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            { Application.runInBackground = true; step = 0; assertions = 0; started = EditorApplication.timeSinceStartup; loop = null; configured = false; }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(Key + "Running", false)) Finish(false, "Test interrotto prima del completamento.");
                string previous = SessionState.GetString(Key + "Previous", "");
                SessionState.EraseString(Key + "Previous");
                if (!string.IsNullOrEmpty(previous)) EditorApplication.delayCall += () => EditorSceneManager.OpenScene(previous);
            }
        }

        private static void OnLog(string message, string stack, LogType type)
        {
            if (SessionState.GetBool(Key + "Running", false) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                Finish(false, message + "\n" + stack);
        }

        private static void Finish(bool passed, string detail)
        {
            SessionState.SetBool(Key + "Running", false);
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PreGameplayLoop-validation.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report, (passed ? "PASS" : "FAIL") + " | assertions=" + assertions + " | Unity=" + Application.unityVersion + "\n" + detail);
            SessionState.EraseString(Key + "Report");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
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
            if (!SessionState.GetBool(Key + "Running", false) || !EditorApplication.isPlaying || EditorApplication.isPaused) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 180) throw new TimeoutException("Engine smoke test timeout, step=" + step);
                if (loop == null) loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>();
                if (loop == null) return;
                if (!configured && loop.State == LoopState.Combat)
                {
                    configured = true;
                    var definition = UnityEngine.Object.Instantiate(loop.Definition);
                    definition.SelectedPlayer = LoopPlayer.PG01;
                    definition.EnableCompanion = false;
                definition.AreaTotals = new[] { 100, 120 };
                definition.AreaMobDistributions = new[] { new AreaMobDistribution(), new AreaMobDistribution() }; // Preserve this legacy ZOMB01-only fixture.
                    loop.Definition = definition;
                    loop.RestartTest();
                    return;
                }
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop != null && loop.BonusAbilities != null)
                {
                    loop.BonusAbilities.enabled = false;
                    if (loop.Experience.Choices != null) { loop.Experience.Choose(0); return; }
                }
                if (loop.State == LoopState.Error) throw new InvalidOperationException(loop.Failure);
                switch (step)
                {
                    case 0:
                        if (loop.State != LoopState.Combat) return;
                        CheckMobGrowth();
                        CheckSprint();
                        Check(!SpawnManager.HasOffscreenClearance(new Vector2(34.99f, 0), 10, 5, 0), "Reject less than 25m beyond edge");
                        Check(SpawnManager.HasOffscreenClearance(new Vector2(35, 0), 10, 5, 0), "Accept exactly 25m beyond edge");
                        Check(SpawnManager.HasOffscreenClearance(new Vector2(45, 0), 10, 5, 0), "Accept farther than 25m");
                        Check(!SpawnManager.HasOffscreenClearance(new Vector2(35, 0), 10, 5, .5f), "Whole MOB remains beyond clearance");
                        Check(!SpawnManager.HasOffscreenClearance(new Vector2(27, 22), 10, 5, 0), "Reject diagonal point closer than 25m to corner");
                        Check(SpawnManager.HasOffscreenClearance(new Vector2(25, 25), 10, 5, 0), "Accept corner distance exactly 25m");
                        Check(loop.Spawns.TotalSpawned == 30 && loop.Spawns.Alive == 30, "FIRST SPAWN A1 = 30");
                        Check(Combatant.All.FindAll(a => a.Faction == Faction.PG).Count == 1, "Exactly one PG");
                        Check(UnityEngine.Object.FindObjectsByType<AreaPickup>(FindObjectsSortMode.None).Length == 0, "No CHEST/MEDI KIT");
                        Check(loop.Player.GetComponent<ExperienceProgression>() != null, "RUN includes EXP and LEVEL UP choices");
                        foreach (var actor in Combatant.All)
                        {
                            if (actor.Faction != Faction.MOB) continue;
                            Vector3 view = loop.GameCamera.WorldToViewportPoint(actor.transform.position);
                            Check(view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1, "FIRST SPAWN off-screen");
                            float halfHeight = SpawnManager.GroundHalfHeight(loop.GameCamera);
                            Check(SpawnManager.HasOffscreenClearance(actor.transform.position - loop.Player.transform.position,
                                halfHeight * loop.GameCamera.aspect, halfHeight, loop.Settings.ActorRadius), "FIRST SPAWN at least 25m beyond visible border");
                            Check(loop.Navigation.Reachable(actor.transform.position, loop.Player.transform.position), "Reachable spawn");
                        }
                        // Clear street fixture for cone checks, independent of the CITY reference layout.
                        loop.Player.transform.position = new Vector2(-6,-10);
                        var mob = Combatant.All.Find(a => a.Faction == Faction.MOB && a.IsActive);
                        mob.transform.position = loop.Player.transform.position + Vector3.right * 2;
                        var weapon = loop.Player.GetComponent<PlayerWeapon>();
                        Check(weapon.Definition.Kind == BaseAttackKind.AreaHitscan && weapon.Definition.Shape == AttackShape.Cone, "Runtime PG01 uses instant area hitscan cone");
                        Check(Mathf.Approximately(loop.Player.Actor.Stats.RangeMetres, 4) && Mathf.Approximately(weapon.Definition.ConeAngle, 75), "Runtime PG01 cone is 4 m x 75 degrees");
                        var probes = Combatant.All.FindAll(a => a.Faction == Faction.MOB && a.IsActive && a != mob).GetRange(0, 7);
                        float[] distances = { 3.5f, 4f, 4.01f, 3.5f, 3.5f, 3f, 3f };
                        float[] angles = { 0, 0, 0, 37.4f, -37.4f, 37.6f, -37.6f };
                        bool[] hits = { true, true, false, true, true, false, false };
                        var positions = new Vector3[probes.Count];
                        int projectileCount = UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length;
                        for (int i = 0; i < probes.Count; i++)
                        {
                            positions[i] = probes[i].transform.position;
                            probes[i].transform.position = loop.Player.transform.position + (Vector3)(AttackGeometry.Direction(angles[i]) * distances[i]);
                        }
                        Check(weapon.TryFireAt((Vector2)weapon.Muzzle.position + Vector2.right * 10), "PG01 fires existing weapon");
                        for (int i = 0; i < probes.Count; i++)
                        {
                            Check(Mathf.Approximately(probes[i].CurrentHP, hits[i] ? 40 : 60), $"PG01 actual hit at {distances[i]} m / {angles[i]} degrees: expected hit={hits[i]}");
                            probes[i].transform.position = positions[i];
                        }
                        Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length == projectileCount, "PG01 spawns no physical projectile");
                        Check(!weapon.TryFireAt(mob.transform.position), "PG01 attack cooldown preserved");
                        Check(Mathf.Approximately(mob.CurrentHP, 40), "PG01 cone causes 20 damage to ZOMB01");
                        mob.Hit(10000);
                        mob.Die();
                        Check(loop.Spawns.Killed == 1 && !mob.GetComponent<CircleCollider2D>().enabled, "One effective death, collider disabled");
                        step = 1;
                        break;
                    case 1:
                        if (loop.Spawns.TotalSpawned != 31) return;
                        Check(loop.Spawns.Alive == 30 && loop.Gold == 10, "One death -> one replacement and one G drop");
                        step = 2;
                        break;
                    case 2:
                    case 7:
                        Check(loop.Spawns.Alive <= loop.Spawns.MaxSimultaneous, "Concurrent cap respected");
                        foreach (var actor in Combatant.All.ToArray())
                            if (actor.Faction == Faction.MOB && actor.IsActive) actor.Hit(10000);
                        if (loop.State != LoopState.AreaComplete) return;
                        Check(loop.Spawns.Killed == loop.Settings.TotalMobs && loop.Spawns.Alive == 0, "Exact completion totals");
                        Check(loop.Exit.Available && loop.Exit.GetComponent<CircleCollider2D>().isTrigger && loop.Exit.gameObject.layer == LayerMask.NameToLayer("TRIGGER_PG"), "Logical exit on TRIGGER_PG");
                        if (step == 2) Check(loop.Sprint.TryActivate(), "Sprint before real AREA transition");
                        loop.Player.transform.position = loop.Exit.transform.position + Vector3.right * 4.1f;
                        step = step == 2 ? 3 : 8;
                        break;
                    case 3:
                    case 8:
                        Check(loop.State == LoopState.AreaComplete && !loop.Exit.ContainsActivePlayer(), "Outside trigger cannot advance");
                        loop.Player.transform.position = loop.Exit.transform.position + Vector3.right * 4;
                        Check(loop.Exit.ContainsActivePlayer(), "4m boundary included");
                        step = step == 3 ? 4 : 9;
                        break;
                    case 4:
                        if (loop.State != LoopState.Bonus) return;
                        Check(Time.timeScale == 0 && loop.Choices.Length == 3, "Bonus opens and pauses");
                        loop.ConfirmBonus();
                        Check(loop.State == LoopState.Bonus, "Cannot confirm without selection");
                        pausedPosition = loop.Player.transform.position;
                        pausedTime = Time.time;
                        pausedSprint = loop.Sprint.ActiveRemaining;
                        pausedAt = EditorApplication.timeSinceStartup;
                        step = 5;
                        break;
                    case 5:
                        if (EditorApplication.timeSinceStartup - pausedAt < .3) return;
                        Check(loop.Player.transform.position == pausedPosition && Time.time == pausedTime, "Gameplay frozen during choice");
                        Check(loop.Sprint.ActiveRemaining == pausedSprint, "Pause freezes sprint duration");
                        loop.Player.Actor.Hit(20); // Simulate an injured survivor for transition healing.
                        loop.SelectBonus(0);
                        loop.ConfirmBonus();
                        hpAfterBonus = loop.Player.Actor.CurrentHP;
                        atkAfterBonus = loop.Player.Actor.Stats.ATK;
                        loop.ConfirmBonus();
                        Check(loop.State == LoopState.Transition, "Double confirmation guarded");
                        step = 6;
                        break;
                    case 6:
                        if (loop.State != LoopState.Combat || loop.AreaIndex != 1) return;
                        foreach (var actor in Combatant.All)
                            if (actor.Faction == Faction.MOB && actor.IsActive)
                                Check(actor.Stats.Equals(loop.Definition.MobAt(0).StatsForRun(1)), "AREA 2 FIRST SPAWN uses +5% base stats");
                        Check(loop.Spawns.TotalSpawned == 36 && loop.Spawns.Alive == 36, "FIRST SPAWN A2 = 36");
                        Check(!loop.Sprint.Active && loop.Sprint.CooldownRemaining > 14, "Real transition starts fixed sprint recovery");
                        Check(loop.Player.Actor.Stats.ATK == atkAfterBonus, "Runtime bonus persists");
                        Check(Mathf.Approximately(loop.Player.Actor.CurrentHP, Mathf.Min(loop.Player.Actor.Stats.HP, hpAfterBonus + loop.Player.Actor.Stats.HP * .15f)), "Transition heals 15% current max HP");
                        Check(UnityEngine.Object.FindObjectsByType<TestNavigation>(FindObjectsSortMode.None).Length == 1, "Old navigation disposed");
                        Check(loop.Player.transform.position == (Vector3)loop.Settings.StartPosition, "New entry position");
                        step = 7;
                        break;
                    case 9:
                        if (loop.State != LoopState.Bonus) return;
                        loop.SelectBonus(0);
                        loop.ConfirmBonus();
                        Check(loop.State == LoopState.Finished && loop.Gold == 2200, "Two cycles complete, exact G, no third AREA");
                        var stats = loop.Player.Actor.Stats;
                        float cd = 100;
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.CdReduction, ref cd);
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.CdReduction, ref cd);
                        Check(cd == 90, "CD REDUCTION rounded 100 -> 95 -> 90");
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.ATK, ref cd);
                        AreaStatBonus.Apply(loop.Player.Actor, AreaStat.ATK, ref cd);
                        Check(Mathf.Abs(loop.Player.Actor.Stats.ATK - stats.ATK * 1.05f * 1.05f) < .001, "Repeated bonuses use current value");
                        loop.RestartTest();
                        step = 10;
                        break;
                    case 10:
                        if (loop.State != LoopState.Combat) return;
                        foreach (var actor in Combatant.All)
                            if (actor.Faction == Faction.MOB && actor.IsActive)
                                Check(actor.Stats.Equals(loop.Definition.MobAt(0).BaseStats), "Restart resets MOB growth to base stats");
                        Check(loop.AreaIndex == 0 && loop.Gold == 0 && loop.Player.Actor.CurrentHP == 120, "Restart creates clean RUN (PG01 HP 120)");
                        Check(!loop.Sprint.Active && loop.Sprint.CooldownRemaining == 0 && loop.Sprint.TryActivate(), "Restart restores sprint ready");
                        loop.Player.Actor.Hit(10000);
                        Check(!loop.Sprint.Active && loop.Sprint.CooldownRemaining == 15, "DOWN cancels sprint and starts fixed recovery");
                        Check(!loop.Exit.ContainsActivePlayer(), "DOWN cannot enter exit");
                        step = 11;
                        break;
                    case 11:
                        if (loop.State != LoopState.Defeat) return;
                        Check(Time.timeScale == 0, "Single PG DOWN ends slice");
                        Finish(true, "Engine Play Mode: 100 + 120 ZOMB01; first spawn; replacement; actual PG01 4 m x 75 degree cone: hits at 3.5/4 m and +/-37.4 degrees, misses at 4.01 m and +/-37.6 degrees; no projectile; cooldown; death; off-screen/NavMesh; exit; bonus pause/confirmation; persistent stats; 15% heal; second cycle; restart; defeat. Mob AI disabled by test harness while draining populations; manual feel/visual QA still required.");
                        break;
                }
            }
            catch (Exception exception) { Finish(false, "step=" + step + "\n" + exception); }
        }
    }
}
