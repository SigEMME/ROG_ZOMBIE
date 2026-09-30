using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    [InitializeOnLoad]
    public static class RunHudValidation
    {
        private const string Key = "ROG.RunHud.";
        private static IEnumerator checks;
        private static readonly List<string> results = new List<string>();
        private static readonly List<string> warnings = new List<string>();
        private static double deadline;
        private static LoopSession run;
        static RunHudValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
                { results.Clear(); warnings.Clear(); deadline = EditorApplication.timeSinceStartup + 180; checks = Checks(); }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    if (checks != null) Finish(false, "Test interrupted.");
                    string previous = SessionState.GetString(Key + "Previous", "");
                    SessionState.EraseString(Key + "Previous");
                    if (previous.Length > 0) EditorApplication.delayCall += () => EditorSceneManager.OpenScene(previous);
                }
            };
            Application.logMessageReceived += (message, stack, type) =>
            {
                if (checks == null) return;
                if (type == LogType.Warning) warnings.Add(message);
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Finish(false, message + "\n" + stack);
            };
        }
        private static void Tick()
        {
            const string request = ".run-hud-test.request";
            if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(request))
            {
                if (SceneManager.GetActiveScene().isDirty) return;
                if (!SessionState.GetBool(Key + "Refreshed", false))
                {
                    SessionState.SetBool(Key + "Refreshed", true);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    return;
                }
                SessionState.EraseBool(Key + "Refreshed");
                SessionState.SetString(Key + "Report", File.ReadAllText(request).Trim()); File.Delete(request);
                SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
                EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
                SessionState.SetBool(Key + "Running", true); EditorApplication.isPlaying = true;
            }
            if (checks == null) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException();
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (!checks.MoveNext()) Finish(true, "RUN HUD integration verified. No build exported.");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }
        private static void Finish(bool ok, string detail)
        {
            checks = null; SessionState.SetBool(Key + "Running", false);
            File.WriteAllText(SessionState.GetString(Key + "Report", "RUN-HUD-validation.txt"),
                $"{(ok ? "PASS" : "FAIL")} | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings.Count}\n" +
                string.Join("\n", results) + "\n" + string.Join("\n", warnings) + "\n" + detail);
            if (!ok || !File.Exists(".run-hud-preview")) EditorApplication.isPlaying = false;
        }
        private static void Check(bool condition, string label)
        { if (!condition) throw new Exception(label); results.Add("PASS " + label); }
        private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .002f, label);
        private static void Invoke(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);

        private static IEnumerator Checks()
        {
            yield return null;
            run = UnityEngine.Object.FindFirstObjectByType<LoopSession>();
            while (run.Loading) yield return null;
            Check(run.GetComponent<LoopHUD>() != null, "HUD attached to RUN scene");
            for (int edge = 0; edge < 4; edge++)
            {
                Near(RunHudData.HealthEdge(1, edge), 1, "Full HP edge " + edge);
                Near(RunHudData.HealthEdge(0, edge), 0, "Empty HP edge " + edge);
                Near(RunHudData.HealthEdge(.5f, edge), edge < 2 ? 0 : 1, "Half HP empties bottom then left, edge " + edge);
            }
            Near(RunHudData.HealthEdge(.875f, 0), .5f, "First lost eighth starts at bottom-right");
            Near(RunHudData.HealthEdge(.125f, 3), .5f, "Last remaining eighth on right edge near bottom");
            Check(RunHudData.RemainingMobs(130, 20) == 110 && RunHudData.RemainingMobs(130, 130) == 0,
                "MOB counter uses total minus KILL, including future spawns");
            var timer = new AbilityCooldown();
            Near(timer.RecoveryProgress, 1, "Ready icon fully colored");
            timer.Restart(20, 100); Near(timer.RecoveryProgress, 0, "New CD icon gray");
            timer.Tick(5); Near(timer.RecoveryProgress, .25f, "Quarter CD elapsed reveals left quarter");
            timer.ChangeArea(false, 20, 50); Near(timer.RecoveryProgress, .25f, "Area preserves original CD duration after stat change");
            timer.Tick(15); Near(timer.RecoveryProgress, 1, "Completed CD restores full icon");
            for (int i = 1; i <= 7; i++)
                Check(Resources.Load<Texture2D>("MerchantItems/" + RunHudData.ItemAsset((PrototypeItem)i)) != null, "Existing item sprite " + (PrototypeItem)i);

            var original = run.Definition;
            var config = UnityEngine.Object.Instantiate(original);
            run.Definition = config;
            config.EnableCompanion = true;
            config.StartingItems = new[] { PrototypeItem.Granata, PrototypeItem.Molotov, PrototypeItem.Smoke, PrototypeItem.PozioneCurativa };
            config.StartingItemCounts = new[] { 1, 1, 1, 1 };
            config.PG04TestItemSlots = 4;
            for (int group = 0; group < 2; group++)
            {
                config.SelectedPlayer = (LoopPlayer)(group * 4);
                config.CompanionPlayer = (LoopPlayer)(group * 4 + 1);
                config.AdditionalCompanions = new[]
                {
                    new CompanionSelection { Enabled = true, Player = (LoopPlayer)(group * 4 + 2) },
                    new CompanionSelection { Enabled = true, Player = (LoopPlayer)(group * 4 + 3) }
                };
                run.RestartTest(); while (run.Loading) yield return null;
                Check(run.State == LoopState.Combat && run.Companions.Count == 3, "Four portrait party group " + group);
                run.Spawns.StopAllCoroutines();
                foreach (var member in run.Members)
                {
                    member.Experience.DropPercent = 0;
                    var ability = RunHudData.Ability(member);
                    Check(ability.Label != "—" && ability.Remaining > 0 && ability.Progress < 1, "Live ability CD " + member.Player.Definition.PlayerId);
                    RunHudData.Passive(member, out string label);
                    Check(label != "—", "Passive identity " + member.Player.Definition.PlayerId);
                }
                run.Experience.Award(run.Experience.NextThreshold / 2);
                Near(RunHudData.ExperienceFraction(run.Experience), (float)run.Experience.Experience / run.Experience.NextThreshold, "EXP fill from current level requirement");
                Check(run.PG04Items.Kind(0) == PrototypeItem.Granata && run.PG04Items.Count(0) == 1, "Loaded item inventory");
                run.PG04Items.TryConsume(0); Check(run.PG04Items.Kind(0) == PrototypeItem.None, "Consumed slot empties");
                if (group == 0)
                {
                    run.Player.Actor.Hit(80); Check(RunHudData.Passive(run, out _), "PG01 low HP lights passive");
                    run.Player.Actor.Heal(1); Check(!RunHudData.Passive(run, out _), "PG01 healing extinguishes passive");
                    Check(RunHudData.Passive(run.Companions[1], out _), "Permanent CALIBRO PERFORANTE stays lit");
                    Check(RunHudData.Passive(run.Companions[2], out _), "Permanent SCORTA ESPLOSIVA stays lit");
                    var sniper = run.Companions[1].PG03Passive;
                    typeof(PG03PassiveRuntime).GetProperty("Selected").SetValue(sniper, PG03Passive.PuntoDebole);
                    var probe = new GameObject("HUD target probe"); probe.transform.position = new Vector2(500, 500);
                    var target = probe.AddComponent<Combatant>(); target.Initialize(Faction.MOB, new CombatStats { HP = 1000, DEF = 100 });
                    sniper.ResolveHit(target, 1, true, run.Companions[1].Player.Actor, 0);
                    Check(sniper.MarkActive && RunHudData.Passive(run.Companions[1], out _), "PUNTO DEBOLE tracks active mark");
                    sniper.ResolveHit(target, 1, true, run.Companions[1].Player.Actor, 0);
                    Check(!sniper.MarkActive, "Second HIT removes HUD mark state");
                    UnityEngine.Object.DestroyImmediate(probe);
                    var pyro = run.Companions[2].PG04Ability;
                    typeof(PG04AbilityRuntime).GetProperty("Passive").SetValue(pyro, PG04Passive.Pyromania);
                    Check(!RunHudData.Passive(run.Companions[2], out _), "PYROMANIA dark without fires");
                    Invoke(pyro, "EmitExplosion", new Vector2(500, 500), 1f, 1f, false);
                    Check(RunHudData.Passive(run.Companions[2], out _), "Real PYROMANIA explosion lights passive for fire lifetime");
                    foreach (var area in UnityEngine.Object.FindObjectsByType<PG04BurningArea>(FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(area.gameObject);
                    Check(!RunHudData.Passive(run.Companions[2], out _), "Removing last fire extinguishes PYROMANIA");
                    var heal = run.Companions[0]; heal.Player.Actor.Hit(heal.Player.Actor.Stats.HP * .8f);
                    Invoke(heal.PG02Passive, "OnKill", new object[] { null });
                    Check(RunHudData.Passive(heal, out _), "PG02 real heal proc flashes passive");
                    float cd = RunHudData.Ability(run).Remaining;
                    float paused = Time.time; Time.timeScale = 0;
                    double until = EditorApplication.timeSinceStartup + .7;
                    while (EditorApplication.timeSinceStartup < until) yield return null;
                    Near(Time.time, paused, "HUD timing freezes in pause");
                    Near(RunHudData.Ability(run).Remaining, cd, "Ability CD freezes in pause");
                    Check(RunHudData.Passive(heal, out _), "Proc flash freezes in pause");
                    Time.timeScale = 1;
                    float end = Time.time + .6f; while (Time.time < end) yield return null;
                    Check(!RunHudData.Passive(heal, out _), "Proc flash expires after half second");
                }
                else
                {
                    typeof(PG05AbilityRuntime).GetProperty("Passive").SetValue(run.PG05Ability, PG05Passive.Ghosting);
                    run.Player.Actor.Hit(run.Player.Actor.Stats.HP * .8f);
                    Check(RunHudData.Passive(run, out _), "Real below-threshold HIT lights GHOSTING");
                    run.Player.Actor.NotifyAction(); Check(!RunHudData.Passive(run, out _), "Action cancels GHOSTING indicator");
                    run.Player.Actor.Heal(1);
                    typeof(PG05AbilityRuntime).GetProperty("Passive").SetValue(run.PG05Ability, PG05Passive.LamaDiCicuta);
                    typeof(PG05AbilityRuntime).GetProperty("CicutaHits").SetValue(run.PG05Ability, 15);
                    var probe = new GameObject("HUD poison target"); probe.transform.position = new Vector2(501, 500);
                    var target = probe.AddComponent<Combatant>(); target.Initialize(Faction.MOB, new CombatStats { HP = 1000, DEF = 100 });
                    var previous = run.Player.transform.position; run.Player.transform.position = new Vector2(500, 500);
                    Invoke(run.PG05Ability, "FireBase", new Vector2(500, 500), new Vector2(501, 500), run.Player.Actor.EffectiveStats);
                    Check(run.PG05Ability.CicutaActive && RunHudData.Passive(run, out _), "Real CICUTA HIT lights poison effect");
                    UnityEngine.Object.DestroyImmediate(probe);
                    Check(!run.PG05Ability.CicutaActive, "CICUTA tracked effect clears when target disappears");
                    run.Player.transform.position = previous;
                    var medic = run.Companions[0];
                    // Reinitialize a fresh vitamin on a different party member to test source attribution.
                    var vitamin = run.Player.gameObject.AddComponent<PG06Vitamin>();
                    vitamin.Refresh(medic, 35, 4);
                    typeof(PG06AbilityRuntime).GetProperty("Passive").SetValue(medic.PG06Ability, PG06Passive.VitaminaC);
                    Check(RunHudData.Passive(medic, out _), "PG06 tracks vitamin applied to another party member");
                    vitamin.Clear(); Check(!RunHudData.Passive(medic, out _), "Vitamin expiry extinguishes source passive");
                    Invoke(run.Companions[1].PG07Ability, "Roll", 1f);
                    Check(RunHudData.Passive(run.Companions[1], out _), "PG07 actual successful proc lights passive");
                    run.Companions[2].PG08Passive.BaseHit();
                    Check(RunHudData.Passive(run.Companions[2], out _), "PG08 TENACIA tracks real HIT bonus");
                }
                int bonusIndex = Array.FindIndex(run.Bonuses.Catalog.Bonuses, value => value.Id == "fire");
                run.Bonuses.Apply(new BonusChoice(bonusIndex, -1));
                run.BonusAbilities.enabled = false;
                Near(run.BonusAbilities.CooldownProgress("fire"), 0, "New bonus starts with gray CD icon");
                run.BonusAbilities.Advance(run.BonusAbilities.Cooldown("fire") / 2);
                Near(run.BonusAbilities.CooldownProgress("fire"), .5f, "Bonus CD reveals half icon");
                var bonus = run.Bonuses.Catalog.Bonuses[bonusIndex];
                int upgrade = Array.FindIndex(bonus.Upgrades, value => value.Key == "CD");
                run.Bonuses.Apply(new BonusChoice(bonusIndex, upgrade));
                Near(run.BonusAbilities.CooldownProgress("fire"), .5f, "Upgrade does not distort current bonus CD progress");
                yield return null;
            }
            run.RestartTest(); while (run.Loading) yield return null;
            Check(run.Bonuses.Owned.Count == 0 && RunHudData.ExperienceFraction(run.Experience) == 0, "RUN reset clears EXP and bonus icons");
            Check(run.PG04Items.Count(0) == 1 && run.Player.Actor.HealthFraction == 1, "RUN reset restores configured items and HP frame");
            yield return null; yield return null;
            if (File.Exists(".run-hud-preview"))
            {
                run.Spawns.StopAllCoroutines();
                run.Experience.Award(run.Experience.NextThreshold / 2);
                run.Player.Actor.Hit(run.Player.Actor.Stats.HP * .25f);
                int bonus = Array.FindIndex(run.Bonuses.Catalog.Bonuses, value => value.Id == "fire");
                run.Bonuses.Apply(new BonusChoice(bonus, -1));
                run.BonusAbilities.Advance(run.BonusAbilities.Cooldown("fire") / 2);
                Time.timeScale = 0;
            }
        }
    }
}
