using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    [InitializeOnLoad]
    public static class ItemsValidation
    {
        private const string Key = "ROG.Items";
        private static IEnumerator checks;
        private static readonly List<string> results = new List<string>();
        private static double deadline;
        private static int warnings;
        static ItemsValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
                { results.Clear(); warnings = 0; deadline = EditorApplication.timeSinceStartup + 150; checks = Checks(); }
            };
            Application.logMessageReceived += (message, stack, type) =>
            {
                if (checks == null) return;
                if (type == LogType.Warning) warnings++;
                if (type == LogType.Error || type == LogType.Exception) Finish(false, message + stack);
            };
        }
        private static void Tick()
        {
            const string request = ".items-test.request";
            if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(request) &&
                !UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
            {
                SessionState.SetString(Key + "Report", File.ReadAllText(request).Trim()); File.Delete(request);
                EditorSceneManager.OpenScene("Assets/Scenes/HubPrototype.unity");
                SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
            }
            if (checks == null) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timeout");
                if (!checks.MoveNext()) Finish(true, "ITEMS integration verified; no build.");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }
        private static void Check(bool ok, string text)
        { if (!ok) throw new Exception(text); results.Add("PASS " + text); }
        private static LoopSession run;
        private static Combatant mob;
        private static readonly Vector2 origin = new Vector2(200, 200);
        private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .02f, label + $" ({actual:0.##})");
        private static void Load(PrototypeItem kind) { while (run.PG04Items.TryConsume(0)) { } Check(run.PG04Items.TryAdd(0, kind), "Load " + kind); }
        private static void ClearAreas()
        {
            foreach (var area in UnityEngine.Object.FindObjectsByType<ItemArea>(FindObjectsSortMode.None))
            { area.gameObject.SetActive(false); UnityEngine.Object.Destroy(area.gameObject); }
        }
        private static void ResetMob(Vector2 position, float def = 100)
        {
            mob.transform.position = position;
            mob.Initialize(Faction.MOB, new CombatStats { HP = 1000, DEF = def, MoveSpeed = 100 });
        }
        private static void Finish(bool ok, string detail)
        {
            checks = null; SessionState.SetBool(Key, false);
            File.WriteAllText(SessionState.GetString(Key + "Report", "TOP-DOWN-levels.txt"),
                $"{(ok ? "PASS" : "FAIL")} | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" + string.Join("\n", results) + "\n" + detail);
            if (!ok) EditorApplication.isPlaying = false;
        }
        private static IEnumerator Checks()
        {
            yield return null;
            var hub = UnityEngine.Object.FindFirstObjectByType<HubPrototype>(); hub.Title.StartGame();
            var selection = hub.Selection; selection.OpenPreparation(); selection.OpenBanner(0);
            selection.SelectPlayer(3); selection.SelectOption(false, 0); selection.SelectOption(true, 0); selection.ConfirmSelection();
            for (int i = 1; i <= 7; i++) { selection.SetItem(0, (PrototypeItem)i); Check(selection.Items[0] == (PrototypeItem)i, "Selectable " + (PrototypeItem)i); }
            selection.SetItem(0, PrototypeItem.Granata, 3); selection.SetItem(1, PrototypeItem.Molotov, 3);
            selection.SwapItems(0, 1); Check(selection.Items[0] == PrototypeItem.Molotov && selection.ItemCounts[0] == 3, "Drag model swaps kind and count");
            selection.OpenBanner(0); selection.SelectPlayer(2); selection.SelectOption(false, 0); selection.SelectOption(true, 0); selection.ConfirmSelection();
            Check(selection.ItemCounts[0] == 1 && selection.ItemCounts[1] == 1, "Losing SCORTA reduces capacity");
            selection.SetItem(0, PrototypeItem.Granata); selection.SetItem(2, PrototypeItem.Smoke); selection.SetItem(3, PrototypeItem.PozioneCurativa);
            Check(hub.BeginRun(), "Start RUN from selected loadout"); run = hub.Run;
            while (run.Loading) yield return null;
            Check(run.State == LoopState.Combat && run.PG04Items.SlotCount == 4 && run.PG04Items.Kind(0) == PrototypeItem.Granata, "Four slots copied into RUN");
            run.Spawns.StopAllCoroutines(); run.Experience.DropPercent = 0;
            foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
            run.Player.transform.position = origin;
            mob = run.CreateMob(0, origin + Vector2.right); mob.GetComponent<MobBrain>().enabled = false;
            ResetMob(origin + Vector2.right, 120);
            Check(run.Items.BeginAim(0, mob.transform.position) && run.PG04Items.Count(0) == 1, "Preview does not consume");
            Check(run.Items.ReleaseAim(mob.transform.position), "Release activates"); Near(mob.CurrentHP, 960, "Grenade respects DEF");
            Check(!run.Items.Use(0, origin), "Empty slot rejected"); ClearAreas();
            Load(PrototypeItem.Granata); run.Items.BeginAim(0, origin); hub.Pause.Open();
            Check(!run.Items.ReleaseAim(origin) && run.PG04Items.Count(0) == 1, "Pause cancels use without consumption"); hub.Pause.Resume();
            ResetMob(origin + Vector2.right * 2);
            var wall = TestVisuals.Box("Item test wall", origin + Vector2.right, new Vector2(.2f, 4), Color.red);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.2f, 4); wall.AddComponent<TestObstacle>().IsWall = true;
            run.Items.Use(0, origin); Near(mob.CurrentHP, 1000, "Wall shields grenade");
            wall.SetActive(false); UnityEngine.Object.Destroy(wall); ClearAreas();
            ResetMob(origin);
            Load(PrototypeItem.Molotov); run.Items.Use(0, origin); Near(mob.CurrentHP, 993, "Molotov immediate tick");
            Load(PrototypeItem.Molotov); run.Items.Use(0, origin); Near(mob.CurrentHP, 986, "Overlapping areas add damage");
            float until = Time.time + 1.1f; while (Time.time < until) yield return null;
            Near(mob.CurrentHP, 972, "Each area ticks once after one second");
            mob.transform.position = origin + Vector2.right * 8; until = Time.time + 1.05f; while (Time.time < until) yield return null;
            Near(mob.CurrentHP, 972, "Leaving fire stops damage"); ClearAreas();
            ResetMob(origin);
            Load(PrototypeItem.BombaVelenosa); run.Items.Use(0, origin); Near(mob.CurrentHP, 1000, "Poison has no immediate damage");
            until = Time.time + 1.1f; while (Time.time < until) yield return null;
            Near(mob.CurrentHP, 995, "Poison first tick after one second");
            for (int i = 0; i < 6; i++) { Load(PrototypeItem.BombaVelenosa); run.Items.Use(0, origin); }
            Check(mob.GetComponent<PG05Poison>().Stacks == 5, "Poison stack cap five");
            UnityEngine.Object.Destroy(mob.GetComponent<PG05Poison>()); ClearAreas(); yield return null;
            ResetMob(origin);
            Load(PrototypeItem.Trappola); run.Items.Use(0, origin + Vector2.right * .01f);
            Near(mob.CurrentHP, 995, "Trap immediate tick"); Near(mob.MovementMetresPerSecond, 1.2f, "Trap slow 40 percent");
            Load(PrototypeItem.Trappola); run.Items.Use(0, origin + Vector2.right * .01f);
            Near(mob.MovementMetresPerSecond, 1.2f, "Trap slow does not stack");
            mob.transform.position = origin + Vector2.up * 4; Near(mob.MovementMetresPerSecond, 2, "Trap slow ends on exit"); ClearAreas();
            Near(Vector2.Distance(origin, ItemRuntime.Position(PrototypeItem.Trappola, origin, origin + Vector2.right * 20)), 7, "Trap range clamp");
            Check(ItemArea.SectionFree(origin, 0, 0), "Trap section initially free");
            wall = TestVisuals.Box("Section blocker", origin + Vector2.down * 1.5f, Vector2.one * .2f, Color.red);
            wall.AddComponent<BoxCollider2D>().size = Vector2.one * .2f; wall.AddComponent<TestObstacle>();
            Check(!ItemArea.SectionFree(origin, 0, 0) && ItemArea.SectionFree(origin, 0, 3), "Only obstructed trap section removed");
            wall.SetActive(false); UnityEngine.Object.Destroy(wall);
            run.Player.Actor.Initialize(Faction.PG, new CombatStats { HP = 200, DEF = 100 }); run.Player.Actor.Hit(50);
            Load(PrototypeItem.PozioneCurativa); run.Items.Use(0, origin); Near(run.Player.Actor.CurrentHP, 160, "Potion immediate 10 HP heal");
            Load(PrototypeItem.PozioneCurativa); run.Items.Use(0, origin); Near(run.Player.Actor.CurrentHP, 170, "Overlapping healing adds"); ClearAreas();
            Load(PrototypeItem.Smoke); run.Items.Use(0, origin);
            Check(run.Player.Actor.IsInvisible, "New smoke applies immediately to PG inside");
            until = Time.time + 1.05f; while (Time.time < until) yield return null;
            Check(run.Player.Actor.IsInvisible, "Smoke grants invisibility after action delay");
            run.Player.transform.position = origin + Vector2.right * 5; Check(!run.Player.Actor.IsInvisible, "Smoke ends immediately outside area");
            run.Player.transform.position = origin; Check(run.Player.Actor.IsInvisible, "Smoke returns on reentry");
            run.Player.Actor.NotifyAction(); Check(!run.Player.Actor.IsInvisible, "Action cancels smoke");
            until = Time.time + .5f; while (Time.time < until) yield return null;
            Check(!run.Player.Actor.IsInvisible, "Smoke cannot return before one second"); ClearAreas();
            ResetMob(origin + Vector2.right * 3);
            Load(PrototypeItem.MinaElettrica); run.Items.Use(0, origin);
            until = Time.time + .2f; while (Time.time < until) yield return null;
            Near(mob.CurrentHP, 1000, "Mine waits outside trigger");
            mob.transform.position = origin;
            until = Time.time + .5f; while (Time.time < until) yield return null;
            Near(mob.CurrentHP, 1000, "Mine delay before explosion");
            until = Time.time + .65f; while (Time.time < until) yield return null;
            Near(mob.CurrentHP, 990, "Mine explosion 10 HP");
            float stunEnd = (float)typeof(MobBrain).GetField("stunnedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(mob.GetComponent<MobBrain>());
            Check(stunEnd - Time.time > 2.2f, "Mine applies 2.5 second stun");
            ClearAreas(); run.Player.transform.position = run.Settings.StartPosition;
            while (run.PG04Items.TryConsume(0)) { } run.PG04Items.TryAdd(0, PrototypeItem.Smoke);
            var inventory = run.PG04Items;
            var next = (IEnumerator)typeof(LoopSession).GetMethod("NextArea", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(run, null);
            run.StartCoroutine(next); yield return null; while (run.Loading) yield return null;
            Check(run.AreaIndex == 1 && run.PG04Items == inventory && inventory.Kind(0) == PrototypeItem.Smoke, "Area transition keeps remaining inventory");
            Check(UnityEngine.Object.FindObjectsByType<ItemArea>(FindObjectsSortMode.None).Length == 0, "Area transition clears effects");
            run.RestartTest(); yield return null; while (run.Loading) yield return null;
            Check(run.AreaIndex == 0 && run.PG04Items != inventory && run.PG04Items.Kind(0) == PrototypeItem.Granata, "Restart restores selected loadout");
            hub.AbandonRun(); yield return null; yield return null;
            Check(hub.Run == null && selection.ItemCounts[0] == 0, "Abandon discards RUN items");
            selection.OpenPreparation();
        }
    }
}
