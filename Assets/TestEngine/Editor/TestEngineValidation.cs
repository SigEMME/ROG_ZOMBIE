using System;
using System.Collections.Generic;
using System.IO;
using RogZombie.TestEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogZombie.EditorTests
{
    // Menu-driven checks plus a project-scoped request file for unattended local validation.
    [InitializeOnLoad]
    public static class TestEngineValidation
    {
        private const string Prefix = "ROG.TestEngine2.";
        private static readonly string RequestPath = Path.Combine(Path.GetTempPath(), "ROG_ZOMBIE_TestEngine2.request");
        public static readonly string ReportPath = Path.Combine(Path.GetTempPath(), "ROG_ZOMBIE_TestEngine2_validation.txt");
        private static double nextPoll;
        private static int assertions;
        private static Projectile pausePG, pauseMOB;
        private static Vector3 pausePGPosition, pauseMOBPosition;
        private static float pausePGTravel, pauseMOBTravel;
        private static double pauseStarted;

        static TestEngineValidation()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += PlayStateChanged;
        }

        private static void Check(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Near(float actual, float expected, string message)
            => Check(Mathf.Abs(actual - expected) < 0.0001f, message + $" ({actual} != {expected})");

        [MenuItem("ROG ZOMBIE/Test Engine 2/Validate data and rules")]
        public static void Validate()
        {
            assertions = 0;
            var settings = AssetDatabase.LoadAssetAtPath<TestAreaSettings>("Assets/TestEngine/Data/TestAreaSettings.asset");
            Check(settings != null, "AREA settings must import.");
            Check(settings.Weapons.Length == 8 && settings.Mobs.Length == 5, "Roster counts.");
            float[] ranges = { 3, 10, 20, 6, 2, 10, 15, 8 };
            for (int i = 0; i < 8; i++)
            {
                var weapon = settings.Weapons[i];
                Check(weapon != null && weapon.PG != null, "Weapon/PG imported.");
                Check(weapon.PG.PlayerId == "PG0" + (i + 1), "PG identity/order.");
                Near(weapon.PG.BaseStats.RangeMetres, ranges[i], "GDD range.");
                Near(weapon.PG.BaseStats.MoveSpeedMetresPerSecond, weapon.PG.BaseStats.MoveSpeed / 100f * 2f, "Legacy movement conversion preserves Inspector tuning.");
                if (weapon.Kind == BaseAttackKind.PhysicalProjectile) Near(weapon.ProjectileSpeed, 20, "PG projectile speed.");
                Check(weapon.Penetrations == 0, "No unselected passive penetration.");
            }
            Near(settings.Mobs[3].ProjectileSpeed, 8, "Approved ZOMB04 speed.");
            Near(settings.Mobs[3].BaseStats.RangeMetres, 20, "ZOMB04 tuned range.");
            Near(settings.Weapons[1].PG.BaseStats.AttackSpeed, 400, "PG02 tuned ATK SPD.");
            Near(settings.Weapons[7].PG.BaseStats.AttackSpeed, 600, "PG08 tuned ATK SPD.");
            Near(settings.Weapons[3].PG.BaseStats.AttackSpeed, 75, "PG04 validated ATK SPD.");
            Near(settings.Weapons[4].PG.BaseStats.AttackSpeed, 150, "PG05 validated ATK SPD.");
            Near(settings.Mobs[1].BaseStats.MoveSpeed, 120, "ZOMB02 tuned MOVE SPD.");
            Near(settings.Mobs[2].BaseStats.MoveSpeed, 90, "ZOMB03 tuned MOVE SPD.");
            Near(settings.Mobs[4].BaseStats.MoveSpeed, 100, "ZOMB05 tuned MOVE SPD.");
            Check(AttackGeometry.CircleIntersectsSemicircle(new Vector2(2.2f, 0), 0.35f, Vector2.right, 2), "KNIFE hits body entering curved edge.");
            Check(!AttackGeometry.CircleIntersectsSemicircle(new Vector2(2.4f, 0), 0.35f, Vector2.right, 2), "KNIFE excludes body beyond curved edge.");
            Check(AttackGeometry.CircleIntersectsSemicircle(new Vector2(-0.2f, 1), 0.35f, Vector2.right, 2), "KNIFE hits body crossing diameter.");
            Check(!AttackGeometry.CircleIntersectsSemicircle(new Vector2(-0.4f, 1), 0.35f, Vector2.right, 2), "KNIFE excludes body entirely behind PG.");
            Near(settings.PlayerMoveSpeedBase, 3, "Global PG speed base.");
            Near(110f / 100f * settings.PlayerMoveSpeedBase, 3.3f, "Global speed scales PG MOVE SPD.");
            Near(settings.Weapons[3].GrenadeHitDelay, 0.5f, "PG04 delay.");
            Near(DamageMath.Calculate(100, 100), 100, "Neutral DEF.");
            Near(DamageMath.Calculate(100, 115), 85, "Positive DEF.");
            Near(DamageMath.Calculate(100, 90), 110, "Negative DEF.");
            Near(Vector2.ClampMagnitude(Vector2.one, 1f).magnitude * 2, 2, "Normalized diagonal speed.");
            Check(AttackGeometry.InCone(new Vector2(3, 1.5f), Vector2.right, 3, 3), "Cone boundary included.");
            Check(!AttackGeometry.InCone(new Vector2(3, 1.6f), Vector2.right, 3, 3), "Outside cone excluded.");
            Check(!AttackGeometry.InSemicircle(Vector2.left, Vector2.right, 2), "Back of knife excluded.");
            var bounds = new Bounds(new Vector3(1, 1, 0), new Vector3(0.2f, 0.2f, 1));
            Check(AttackGeometry.SectorIntersectsBox(Vector2.zero, 2, 0, 90, bounds), "Wall inside circular sector.");
            Check(!AttackGeometry.SectorIntersectsBox(Vector2.zero, 2, 180, 90, bounds), "Opposite sector unaffected.");
            Check(AttackGeometry.InValidSector(new Vector2(1, 0), 2, new[] { false, false, false, true }), "Boundary target can use either valid sector.");

            var quotas = SpawnManager.Allocate(330, settings.MobPercentages);
            var first = SpawnManager.Allocate(99, settings.MobPercentages);
            first[0] += first[4] / 2; first[1] += first[4] - first[4] / 2; first[4] = 0;
            Check(first[0] == 33 && first[1] == 35 && first[2] == 21 && first[3] == 10 && first[4] == 0, "FIRST SPAWN composition.");
            int sum = 0;
            for (int i = 0; i < 5; i++) { sum += quotas[i]; Check(first[i] <= quotas[i], "FIRST SPAWN within quota."); }
            Check(sum == 330, "Exact total allocation.");
            Check(WeightedSelection.Draw(new float[] { 0, 25, 0 }, 0.7f) == 1, "Exhausted types excluded.");

            var go = new GameObject("Temporary rule validation") { hideFlags = HideFlags.HideAndDontSave };
            var randomState = UnityEngine.Random.state;
            try
            {
                var actor = go.AddComponent<Combatant>();
                var pg = settings.Weapons[0].PG;
                actor.Initialize(Faction.PG, CombatStats.FromPG(pg.BaseStats));
                actor.Hit(100);
                Near(actor.CurrentHP, 15, "HIT reduces HP using DEF.");
                actor.Heal(0.1f);
                Near(actor.CurrentHP, 25, "MEDI KIT heals percentage of maximum.");
                actor.Heal(10f);
                Near(actor.CurrentHP, 100, "Healing cannot overheal.");
                actor.Hit(1000);
                Check(actor.State == LifeState.Down, "PG enters DOWN.");
                Check(!actor.Heal(0.1f), "MEDI KIT ignores DOWN.");
                actor.Initialize(Faction.MOB, settings.Mobs[4].BaseStats, true);
                float reported = -1f;
                Action<Combatant, float> captureDamage = (hit, actual) => { if (hit == actor) reported = actual; };
                Combatant.DamageApplied += captureDamage;
                actor.Hit(1000);
                Combatant.DamageApplied -= captureDamage;
                Near(reported, settings.Mobs[4].BaseStats.HP, "Damage numbers report actual HP loss, not overkill.");
                Check(actor.State == LifeState.PreExplosion, "ZOMB05 does not die at zero HP.");
                Check(go.GetComponent<CircleCollider2D>().enabled, "ZOMB05 retains collision.");
                actor.Hit(1000);
                Check(actor.State == LifeState.PreExplosion, "Further HIT cannot bypass pre-explosion.");
                int deathEvents = 0;
                actor.Died += _ => deathEvents++;
                actor.Die(); actor.Die();
                Check(deathEvents == 1 && !go.GetComponent<CircleCollider2D>().enabled, "One death event; no dead collider.");

                var inventory = go.AddComponent<BonusInventory>();
                inventory.Catalog = settings.BonusCatalog;
                UnityEngine.Random.InitState(1122);
                for (int stage = 0; stage < 4; stage++)
                {
                    for (int roll = 0; roll < 100; roll++)
                    {
                        var choices = inventory.Generate();
                        Check(choices.Length == 3, "Three banners.");
                        var keys = new HashSet<string>();
                        foreach (var choice in choices)
                        {
                            Check(keys.Add(choice.Bonus + ":" + choice.Upgrade), "Distinct banners.");
                            Check(choice.Upgrade < 0 ? !inventory.Owns(choice.Bonus) : inventory.Owns(choice.Bonus), "Only eligible choices.");
                            if (stage == 0) Check(choice.Upgrade == -1, "No upgrades before acquisition.");
                            if (stage == 3) Check(choice.Upgrade >= 0, "Full slots only offer upgrades.");
                        }
                    }
                    if (stage < 3) inventory.Apply(new BonusChoice(stage, -1));
                }
                inventory.Apply(new BonusChoice(0, 0)); inventory.Apply(new BonusChoice(0, 0));
                Check(inventory.TryGetValue(inventory.Owned[0], "DANNO", out float value), "Known bonus value.");
                Near(value, 12, "Repeated upgrades use original base (not compounded).");
            }
            finally { UnityEngine.Random.state = randomState; UnityEngine.Object.DestroyImmediate(go); }
            File.WriteAllText(ReportPath, $"PASS: {assertions} data / rule assertions.\n");
            Debug.Log($"TEST ENGINE #2: {assertions} assertions passed.");
        }

        [MenuItem("ROG ZOMBIE/Test Engine 2/Validate and smoke test")]
        public static void StartSmoke()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop the existing Play session first.");
            if (SceneManager.sceneCount != 1) throw new InvalidOperationException("Smoke test requires a single saved scene; multiple open scenes are preserved.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Unsaved scene: smoke test did not change the open scene.");
            Validate();
            SessionState.SetString(Prefix + "PreviousScene", SceneManager.GetActiveScene().path);
            SessionState.SetString(Prefix + "Stage", "loading");
            SessionState.SetBool(Prefix + "Failed", false);
            SessionState.SetFloat(Prefix + "Started", (float)EditorApplication.timeSinceStartup);
            EditorSceneManager.OpenScene("Assets/Scenes/TestEngine2.unity");
            EditorApplication.isPlaying = true;
        }

        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextPoll) return;
            nextPoll = EditorApplication.timeSinceStartup + 0.2;
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating && File.Exists(RequestPath))
            {
                string request = File.ReadAllText(RequestPath).Trim().Replace('\\', '/');
                if (string.Equals(request, Path.GetDirectoryName(Application.dataPath).Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(RequestPath);
                    try { StartSmoke(); }
                    catch (Exception error) { File.AppendAllText(ReportPath, "BLOCKED/FAILED: " + error + "\n"); Debug.LogException(error); }
                }
            }
            string stage = SessionState.GetString(Prefix + "Stage", "");
            if (stage == "" || stage == "stopping" || !EditorApplication.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup - SessionState.GetFloat(Prefix + "Started", 0) > 90)
                    throw new TimeoutException("Smoke test timed out loading the AREA.");
                var world = UnityEngine.Object.FindFirstObjectByType<TestEngineBootstrap>();
                if (world == null) return;
                if (world.Failure != null) throw new InvalidOperationException(world.Failure);
                if (world.Loading) return;
                if (stage == "loading")
                {
                    Check(world.Navigation.Ready, "NavMesh built in Play mode.");
                    Check(world.Spawns.TotalSpawned == 99 && world.Spawns.Alive == 99, "Actual FIRST SPAWN count.");
                    Check(!Combatant.All.Exists(a => a.GetComponent<MobBrain>() != null && a.GetComponent<MobBrain>().Definition.Kind == MobKind.ZOMB05), "Actual FIRST SPAWN excludes ZOMB05.");
                    var pickups = UnityEngine.Object.FindObjectsByType<AreaPickup>(FindObjectsSortMode.None);
                    int kits = 0;
                    foreach (var pickup in pickups)
                    {
                        if (!pickup.IsChest) kits++;
                        if (pickup.IsChest && world.Settings.ForceTestChestNearPlayer)
                            Check(Vector2.Distance(pickup.transform.position, world.Settings.StartPosition) < 6, "TEST CHEST near player.");
                        else Check(Vector2.Distance(pickup.transform.position, world.Settings.StartPosition) >= 50, "Actual pickup distance.");
                        Check(world.Navigation.Reachable(pickup.transform.position, world.Settings.StartPosition), "Pickup reachable by NavMesh.");
                    }
                    Check(kits >= 1 && kits <= 3, "Actual MEDI KIT count.");
                    Check(Array.FindAll(pickups, p => p.IsChest).Length == 1, "Exactly one guaranteed test CHEST.");
                    foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                    foreach (var projectile in UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None)) UnityEngine.Object.Destroy(projectile.gameObject);
                    var target = MakeTestActor("Validation target", new Vector2(2, 0), Faction.MOB);
                    MakeTestActor("Validation ally", new Vector2(1.1f, 0), Faction.PG);
                    Capture(world.GameCamera);
                    SessionState.SetInt(Prefix + "Weapon", 0);
                    SessionState.SetString(Prefix + "Stage", "fire");
                }
                else if (stage == "fire")
                {
                    int index = SessionState.GetInt(Prefix + "Weapon", 0);
                    var target = Combatant.All.Find(a => a != null && a.name == "Validation target");
                    var weapon = world.Player.GetComponent<PlayerWeapon>();
                    world.Player.Initialize(world.Settings.Weapons[index].PG);
                    weapon.Definition = world.Settings.Weapons[index];
                    if (!weapon.TryFireAt(target.transform.position)) return;
                    Check(!weapon.TryFireAt(target.transform.position), "ATK SPD blocks immediate second shot.");
                    if (index == 3)
                    {
                        Near(target.CurrentHP, 1000, "Grenade must not damage at firing time.");
                        target.transform.position = new Vector2(6, 0);
                        MakeTestActor("Grenade arrival", new Vector2(2, 0), Faction.MOB);
                    }
                    SessionState.SetFloat(Prefix + "Fired", Time.time);
                    SessionState.SetString(Prefix + "Stage", "hit");
                }
                else if (stage == "hit")
                {
                    int index = SessionState.GetInt(Prefix + "Weapon", 0);
                    float hitWait = index == 3 ? world.Settings.Weapons[index].GrenadeHitDelay + 0.15f : 0.3f;
                    if (Time.time - SessionState.GetFloat(Prefix + "Fired", 0) < hitWait) return;
                    var target = Combatant.All.Find(a => a != null && a.name == "Validation target");
                    var ally = Combatant.All.Find(a => a != null && a.name == "Validation ally");
                    if (index == 3)
                    {
                        Near(target.CurrentHP, 1000, "MOB leaving the grenade area avoids damage.");
                        var arrival = Combatant.All.Find(a => a != null && a.name == "Grenade arrival");
                        Near(arrival.CurrentHP, 970, "Grenade hits a MOB entering the fixed area during delay.");
                        UnityEngine.Object.Destroy(arrival.gameObject);
                        target.transform.position = new Vector2(2, 0);
                    }
                    else Near(target.CurrentHP, 1000 - world.Settings.Weapons[index].PG.BaseStats.ATK, "Actual PG0" + (index + 1) + " attack HIT.");
                    Near(ally.CurrentHP, 1000, "PG projectile/area ignores ally.");
                    target.Initialize(Faction.MOB, target.Stats);
                    if (index < 7)
                    {
                        SessionState.SetInt(Prefix + "Weapon", index + 1);
                        SessionState.SetString(Prefix + "Stage", "fire");
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(target.gameObject);
                        UnityEngine.Object.Destroy(ally.gameObject);
                        File.AppendAllText(ReportPath, "PASS: actual HIT for all 8 PG attacks, rate gate, ally exclusion.\n");
                        world.Player.Initialize(world.Settings.Weapons[0].PG);
                        var zombie = world.CreateMob(4, new Vector2(3, 0));
                        zombie.name = "Validation ZOMB05";
                        zombie.Hit(1000);
                        Check(zombie.State == LifeState.PreExplosion, "Play-mode pre-explosion starts.");
                        SessionState.SetFloat(Prefix + "ExplosionStart", Time.time);
                        SessionState.SetString(Prefix + "Stage", "explosion");
                    }
                }
                else if (stage == "explosion")
                {
                    float elapsed = Time.time - SessionState.GetFloat(Prefix + "ExplosionStart", 0);
                    var zombie = Combatant.All.Find(a => a != null && a.name == "Validation ZOMB05");
                    if (elapsed < 1.9f) Check(zombie != null && zombie.State == LifeState.PreExplosion, "Pre-explosion does not end early.");
                    if (elapsed < 2.2f) return;
                    Check(zombie != null && zombie.State == LifeState.Dead, "ZOMB05 explodes and dies after two seconds.");
                    Check(world.Player.Actor.CurrentHP < world.Player.Actor.Stats.HP, "Explosion damages PG using runtime HP.");
                    File.AppendAllText(ReportPath, "PASS: Play mode, NavMesh, 99 initial MOB, pickup placement, ZOMB05 timer/explosion.\n");
                    world.Restart(7);
                    SessionState.SetString(Prefix + "Stage", "restart");
                }
                else if (stage == "restart")
                {
                    Check(world.Player.Definition.PlayerId == "PG08", "Restart selects PG08.");
                    Check(world.Spawns.TotalSpawned == 99 && world.Spawns.Alive == 99, "Restart resets population.");
                    Check(Combatant.All.FindAll(a => a.Faction == Faction.PG).Count == 1, "Restart does not duplicate PG.");
                    File.AppendAllText(ReportPath, "PASS: restart as PG08, new population, no duplicate PG.\n");
                    foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                    var victim = Combatant.All.Find(a => a.Faction == Faction.MOB && a.IsActive);
                    victim.Hit(10000);
                    victim.Die();
                    Check(world.Spawns.Killed == 1, "Duplicate MORTE does not duplicate accounting.");
                    Check(world.Spawns.Alive == 98, "Active count decreases only once.");
                    SessionState.SetString(Prefix + "Stage", "replacement");
                }
                else if (stage == "replacement")
                {
                    if (world.Spawns.TotalSpawned < 100) return;
                    Check(world.Spawns.TotalSpawned == 100 && world.Spawns.Alive == 99 && world.Spawns.Killed == 1, "One MORTE produces exactly one replacement.");
                    foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                    var mob = Combatant.All.Find(a => a.Faction == Faction.MOB && a.IsActive);
                    pausePG = MakePauseProjectile(world.Player.Actor, new Vector2(0, 3));
                    pauseMOB = MakePauseProjectile(mob, new Vector2(0, 5));
                    world.Player.GetComponent<ExperienceProgression>().Award(100);
                    SessionState.SetString(Prefix + "Stage", "pauseStart");
                }
                else if (stage == "pauseStart")
                {
                    if (Time.timeScale != 0f) return;
                    Check(pausePG != null && pauseMOB != null, "Both projectile factions survive entry into LVL UP.");
                    pausePGPosition = pausePG.transform.position; pauseMOBPosition = pauseMOB.transform.position;
                    pausePGTravel = pausePG.Travelled; pauseMOBTravel = pauseMOB.Travelled;
                    pauseStarted = EditorApplication.timeSinceStartup;
                    SessionState.SetString(Prefix + "Stage", "paused");
                }
                else if (stage == "paused")
                {
                    Check(pausePG != null && pauseMOB != null, "Paused projectiles remain the same instances.");
                    Check(pausePG.transform.position == pausePGPosition && pauseMOB.transform.position == pauseMOBPosition, "No projectile movement in pause.");
                    Near(pausePG.Travelled, pausePGTravel, "PG range does not advance in pause.");
                    Near(pauseMOB.Travelled, pauseMOBTravel, "MOB range does not advance in pause.");
                    if (EditorApplication.timeSinceStartup - pauseStarted < 1) return;
                    world.Player.GetComponent<ExperienceProgression>().Choose(0);
                    SessionState.SetString(Prefix + "Stage", "resumed");
                }
                else if (stage == "resumed")
                {
                    Check(pausePG != null && pauseMOB != null, "Resume does not recreate projectiles.");
                    if (pausePG.Travelled == pausePGTravel) return;
                    Check(pausePG.Travelled > pausePGTravel && pauseMOB.Travelled > pauseMOBTravel, "Both projectiles resume travel.");
                    Near(pausePG.Speed, 20, "PG projectile retains speed.");
                    Near(pauseMOB.Speed, 15, "MOB projectile retains speed.");
                    Check(pausePG.Direction == Vector2.up && pauseMOB.Direction == Vector2.up, "Projectile directions preserved.");
                    File.AppendAllText(ReportPath, "PASS: one replacement per MORTE; both projectile factions freeze and resume across actual LVL UP.\n");
                    FinishSmoke();
                }
            }
            catch (Exception error)
            {
                SessionState.SetBool(Prefix + "Failed", true);
                File.AppendAllText(ReportPath, "SMOKE FAILED: " + error + "\n");
                Debug.LogException(error);
                FinishSmoke();
            }
        }

        private static void Capture(Camera camera)
        {
            int width = Mathf.RoundToInt(540f * camera.aspect);
            var target = new RenderTexture(width, 540, 24);
            var previous = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var image = new Texture2D(width, 540, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, 540), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "ROG_ZOMBIE_TestEngine2.png"), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = previousActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        private static void FinishSmoke()
        {
            SessionState.SetString(Prefix + "Stage", "stopping");
            EditorApplication.isPlaying = false;
        }

        private static Combatant MakeTestActor(string name, Vector2 position, Faction side)
        {
            var go = TestVisuals.Box(name, position, Vector2.one * 0.5f, Color.white);
            go.AddComponent<CircleCollider2D>().radius = 0.25f;
            var actor = go.AddComponent<Combatant>();
            actor.Initialize(side, new CombatStats { HP = 1000, DEF = 100 });
            return actor;
        }

        private static Projectile MakePauseProjectile(Combatant owner, Vector2 point)
        {
            var go = TestVisuals.Box("Pause projectile test", point, Vector2.one * 0.12f, Color.white);
            var projectile = go.AddComponent<Projectile>();
            projectile.Initialize(owner, Vector2.up, owner.Faction == Faction.PG ? 20 : 15, 100, 1, 0.06f, 0, false);
            return projectile;
        }

        private static void PlayStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode || SessionState.GetString(Prefix + "Stage", "") != "stopping") return;
            SessionState.EraseString(Prefix + "Stage");
            string previous = SessionState.GetString(Prefix + "PreviousScene", "");
            if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
            if (Application.isBatchMode) EditorApplication.Exit(SessionState.GetBool(Prefix + "Failed", false) ? 1 : 0);
        }
    }
}
