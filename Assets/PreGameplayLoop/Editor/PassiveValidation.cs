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
    public static class PassiveValidation
    {
        private const string Key = "ROG.PG01Passives.";
        private static readonly string Request = Path.GetFullPath(".pg01-passives-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;

        static PassiveValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run PG01 passive tests")]
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
                if (EditorApplication.timeSinceStartup - started > 300) throw new TimeoutException("Passive test timeout.");
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop != null && loop.Player != null)
                {
                    var input = loop.Player.GetComponent<PG01AbilityInput>();
                    if (input != null) input.enabled = false;
                    var pg02Input = loop.Player.GetComponent<PG02AbilityInput>();
                    if (pg02Input != null) pg02Input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "Engine Play Mode tests completed. Both PG01 passives tested through runtime HIT, healing, stat bonuses, attacks, AREA transitions and fresh RUNs. Physical keyboard/mouse feel remains a manual check.");
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
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("PG01-passives-validation.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report, (passed ? "PASS" : "FAIL") + $" | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" +
                string.Join("\n", results) + "\n" + detail);
            SessionState.EraseString(Key + "Report");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        private static Combatant Probe(Vector2 position, Faction faction = Faction.MOB, float defence = 100)
        {
            var go = new GameObject("Passive test actor");
            go.transform.SetParent(loop.transform);
            go.transform.position = position;
            go.layer = LayerMask.NameToLayer(faction == Faction.PG ? "PG" : "MOB");
            go.AddComponent<CircleCollider2D>().radius = .35f;
            var actor = go.AddComponent<Combatant>();
            actor.Initialize(faction, new CombatStats { HP = 100, DEF = defence, ATK = 20, MoveSpeed = 100, Range = 400 });
            return actor;
        }

        private static void Health(Combatant actor, float hp)
            => actor.SetStats(actor.Stats, hp - actor.CurrentHP);

        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            var originalDefinition = loop.Definition;
            var definition = UnityEngine.Object.Instantiate(originalDefinition);
            definition.SelectedPlayer = LoopPlayer.PG01;
            loop.Definition = definition;
            try
            {
                definition.SelectedPassive = (PG01Passive)999;
                Check(definition.Validate() != null, "Invalid passive selection rejected");
                foreach (PG01Passive choice in new[] { PG01Passive.Passiva1, PG01Passive.Passiva2 })
                {
                    definition.SelectedPassive = choice;
                    definition.SelectedAbility = choice == PG01Passive.Passiva1 ? PG01Ability.Pestone : PG01Ability.Barriera;
                    Check(definition.Validate() == null, "Valid independent passive/ability configuration");
                    loop.RestartTest();
                    while (loop.State != LoopState.Combat) yield return null;
                    var actor = loop.Player.Actor;
                    var passive = loop.Passive;
                    var baseline = actor.Stats;
                    var ability = loop.Ability.Selected;
                    var other = choice == PG01Passive.Passiva1 ? PG01Passive.Passiva2 : PG01Passive.Passiva1;
                    Check(passive.Selected == choice && !passive.EffectActive, choice + " selected and inactive at full HP");
                    Check(loop.Player.GetComponents<PG01PassiveRuntime>().Length == 1, "Exactly one passive runtime");
                    definition.SelectedPassive = other;
                    definition.SelectedAbility = ability == PG01Ability.Pestone ? PG01Ability.Barriera : PG01Ability.Pestone;
                    Check(passive.Selected == choice && loop.Ability.Selected == ability, "Inspector edits do not alter current RUN");
                    foreach (float hp in new[] { 51f, 50f, 49f })
                    {
                        Health(actor, hp);
                        Check(passive.EffectActive == (hp < 50), choice + " strict HP threshold at " + hp);
                        Near(actor.EffectiveStats.DEF, hp < 50 && choice == PG01Passive.Passiva1 ? 130 : 115, "Effective DEF at threshold");
                        Near(actor.EffectiveStats.ATK, hp < 50 && choice == PG01Passive.Passiva2 ? 23 : 20, "Effective ATK at threshold");
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Health(actor, 50);
                        actor.Hit(1);
                        Near(actor.CurrentHP, 49, "Crossing HIT uses previous DEF and final rounding");
                        Check(passive.EffectActive, "Crossing HIT activates passive immediately");
                        actor.Hit(0);
                        Near(actor.EffectiveStats.DEF, choice == PG01Passive.Passiva1 ? 130 : 115, "Repeated activation never stacks DEF");
                        Near(actor.EffectiveStats.ATK, choice == PG01Passive.Passiva2 ? 23 : 20, "Repeated activation never stacks ATK");
                        actor.Heal(.01f);
                        Near(actor.CurrentHP, 50, "Heal reaches exact half HP");
                        Check(!passive.EffectActive, "Healing to half HP immediately deactivates");
                    }
                    Health(actor, 55);
                    actor.Hit(10);
                    Near(actor.CurrentHP, 46, "HIT crossing 50 percent is not retroactively mitigated");
                    actor.Hit(10);
                    Near(actor.CurrentHP, choice == PG01Passive.Passiva1 ? 39 : 37, "Next HIT uses selected passive DEF");
                    Health(actor, 50);
                    var changed = baseline;
                    changed.HP = 120;
                    actor.SetStats(changed);
                    Check(passive.EffectActive, "Increasing max HP immediately activates at unchanged current HP");
                    actor.SetStats(baseline);
                    Check(!passive.EffectActive, "Decreasing max HP immediately deactivates");
                    float cd = 100;
                    Health(actor, 49);
                    AreaStatBonus.Apply(actor, AreaStat.HP, ref cd);
                    Near(actor.CurrentHP, 54, "HP bonus grants correct health");
                    Near(actor.Stats.HP, 105, "HP bonus changes persistent maximum");
                    Check(!passive.EffectActive, "HP bonus recalculates condition including granted health");
                    actor.SetStats(baseline);
                    Health(actor, 40);
                    AreaStatBonus.Apply(actor, AreaStat.ATK, ref cd);
                    Near(actor.Stats.ATK, 21, "ATK bonus while active uses persistent 20 -> 21");
                    Near(actor.EffectiveStats.ATK, choice == PG01Passive.Passiva2 ? 24.15f : 21, "ATK passive remains separate after bonus");
                    actor.Heal(.6f);
                    Near(actor.EffectiveStats.ATK, 21, "Deactivation preserves ATK bonus at 21");
                    Health(actor, 40);
                    AreaStatBonus.Apply(actor, AreaStat.ATK, ref cd);
                    Near(actor.Stats.ATK, 22.05f, "Repeated persistent ATK bonus does not include passive");
                    actor.SetStats(baseline);
                    Health(actor, 40);
                    AreaStatBonus.Apply(actor, AreaStat.DEF, ref cd);
                    Near(actor.Stats.DEF, 120.75f, "Persistent DEF bonus excludes passive contribution");
                    Near(actor.EffectiveStats.DEF, choice == PG01Passive.Passiva1 ? 135.75f : 120.75f, "DEF passive adds exactly 15 internal points");
                    Health(actor, 50);
                    Near(actor.EffectiveStats.DEF, 120.75f, "Deactivation preserves DEF bonus");
                    changed = baseline;
                    changed.DEF = 180;
                    actor.SetStats(changed);
                    Health(actor, 40);
                    AreaStatBonus.Apply(actor, AreaStat.DEF, ref cd);
                    Near(actor.Stats.DEF, 189, "DEF bonus under temporary cap remains persistent");
                    Near(actor.EffectiveStats.DEF, choice == PG01Passive.Passiva1 ? 190 : 189, "DEF mitigation cap is 90 percent");
                    actor.Hit(100);
                    Near(actor.CurrentHP, choice == PG01Passive.Passiva1 ? 30 : 29, "Actual incoming HIT respects DEF cap");
                    Health(actor, 50);
                    Near(actor.EffectiveStats.DEF, 189, "Removing capped passive restores full persistent DEF");
                    changed.DEF = 200;
                    actor.SetStats(changed);
                    Near(actor.EffectiveStats.DEF, 190, "DEF cap applies when passive inactive too");
                    actor.SetStats(baseline);
                    Health(actor, 49);
                    Vector2 origin = new Vector2(30, 30);
                    loop.Player.transform.position = origin;
                    var target = Probe(origin + Vector2.right * 2);
                    var armoured = Probe(origin + new Vector2(2, .5f), defence: 115);
                    var weapon = loop.Player.GetComponent<PlayerWeapon>();
                    Check(weapon.TryFireAt(origin + Vector2.right * 8), "Actual PG01 ATTACCO BASE fires");
                    Near(target.CurrentHP, choice == PG01Passive.Passiva2 ? 77 : 80, "Actual outgoing HIT uses selected passive ATK");
                    Near(armoured.CurrentHP, choice == PG01Passive.Passiva2 ? 80 : 83, "Outgoing HIT rounds only after DEF");
                    UnityEngine.Object.Destroy(target.gameObject);
                    UnityEngine.Object.Destroy(armoured.gameObject);
                    yield return null;
                    target = Probe(origin + Vector2.right * 2);
                    PestoneEffect.Cast(origin, Vector2.right, definition.PG01Abilities);
                    Near(target.CurrentHP, 60, "Passive never scales fixed PESTONE 40 damage");
                    UnityEngine.Object.Destroy(target.gameObject);
                    yield return null;
                    // Real bonus screen and AREA teardown/heal, with passive still active.
                    actor.SetStats(baseline);
                    Health(actor, choice == PG01Passive.Passiva1 ? 40 : 20);
                    while (loop.State != LoopState.AreaComplete)
                    {
                        foreach (var mob in Combatant.All.ToArray()) if (mob.Faction == Faction.MOB && mob.IsActive) mob.Die();
                        yield return null;
                    }
                    loop.Player.transform.position = loop.Exit.transform.position;
                    while (loop.State != LoopState.Bonus) yield return null;
                    Check(passive.EffectActive, "Passive condition remains active during BONUS pause");
                    loop.Choices[0] = AreaStat.ATK;
                    loop.SelectBonus(0);
                    loop.ConfirmBonus();
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.Passive == passive && passive.Selected == choice && loop.Ability.Selected == ability,
                        "AREA preserves independent passive and ability RUN selections");
                    Near(actor.Stats.ATK, 21, "Actual AREA bonus persists independently of passive");
                    Near(actor.CurrentHP, choice == PG01Passive.Passiva1 ? 55 : 35, "AREA grants 15 percent max HP heal");
                    Check(passive.EffectActive == (choice == PG01Passive.Passiva2), "AREA heal reevaluates passive across threshold");
                    Near(actor.EffectiveStats.ATK, choice == PG01Passive.Passiva2 ? 24.15f : 21, "Correct effective ATK after AREA");
                    Health(actor, 1);
                    actor.Hit(1000);
                    Check(!passive.EffectActive && !actor.IsActive, "DOWN disables passive");
                    loop.RestartTest();
                    while (loop.State != LoopState.Combat) yield return null;
                    Check(loop.Passive != passive && loop.Passive.Selected == other, "New RUN acquires changed passive configuration");
                    Check(loop.Ability.Selected == definition.SelectedAbility, "New RUN independently acquires changed ability");
                    Near(loop.Player.Actor.CurrentHP, 100, "New RUN resets HP");
                    Near(loop.Player.Actor.Stats.ATK, 20, "New RUN resets bonuses");
                    Near(loop.Player.Actor.EffectiveStats.DEF, 115, "New RUN has no stale temporary DEF");
                    Check(!loop.Passive.EffectActive, "New RUN starts with passive inactive");
                }
            }
            finally
            {
                loop.Definition = originalDefinition;
                UnityEngine.Object.Destroy(definition);
            }
        }
    }
}
