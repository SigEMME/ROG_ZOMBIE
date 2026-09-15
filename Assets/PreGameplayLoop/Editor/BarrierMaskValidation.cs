using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop.Editor
{
    // Run only in an isolated batch editor: this entry point exits that editor after testing.
    [InitializeOnLoad]
    public static class BarrierMaskValidation
    {
        private const string Key = "ROG.BarrierMask.Batch";
        static BarrierMaskValidation() { EditorApplication.playModeStateChanged += State; }
        public static void RunBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Isolated batch editor required.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
        }
        private static void State(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Key, false)) return;
            SessionState.EraseBool(Key);
            var checks = new List<string>(); int exit = 0;
            try { RunChecks((value, label) => { if (!value) throw new Exception(label); checks.Add("PASS " + label); }); }
            catch (Exception error) { checks.Add(error.ToString()); exit = 1; }
            string report = Path.GetFullPath("BARRIERA-mask-validation.txt");
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-barrierReport") report = args[i + 1];
            File.WriteAllText(report, (exit == 0 ? "PASS" : "FAIL") + " | Unity " + Application.unityVersion + " | checks=" + checks.FindAll(x => x.StartsWith("PASS ")).Count + "\n" + string.Join("\n", checks));
            EditorApplication.Exit(exit);
        }
        private static void RunChecks(Action<bool,string> check)
        {
            var root = new GameObject("Barrier mask checks"); TestVisuals.Root = root.transform;
            var data = ScriptableObject.CreateInstance<PG01AbilityCatalog>();
            try
            {
                foreach (float angle in new[] { 0f, 37f, 90f })
                foreach (bool wall in new[] { true, false })
                {
                    Quaternion rotation = Quaternion.Euler(0,0,angle);
                    var block = TestVisuals.Box("Cover", rotation * new Vector2(1.2f,0), new Vector2(.2f,2), Color.blue);
                    block.transform.rotation = rotation;
                    block.AddComponent<BoxCollider2D>().size = new Vector2(.2f,2);
                    block.AddComponent<TestObstacle>().IsWall = wall; Physics2D.SyncTransforms();
                    var geometry = new BarrierGeometry(); geometry.Build(Vector2.zero, data.BarrierSize, angle);
                    check(geometry.HasArea, "Partial cover retains area " + angle + " wall=" + wall);
                    check(!geometry.OverlapsCircle(new Vector2(3,0),.2f), "Masked PG does not block remaining area");
                    check(geometry.OverlapsCircle(Vector2.zero,.2f), "PG on remaining area blocks placement");
                    var barrier = BarrierEffect.Spawn(Vector2.zero, angle, data, geometry);
                    check(barrier != null && barrier.Solid.enabled && !barrier.GetComponent<BoxCollider2D>().enabled, "Only clipped collider enabled");
                    for (int x = -6; x <= 6; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2 local = new Vector2(x * .5f, y * .3f); Vector2 point = rotation * local;
                        check(barrier.Solid.OverlapPoint(point) == (local.x < 1.1f), "Collider matches cover shadow at " + point);
                    }
                    check(Mathf.Abs(AttackGeometry.VisibleDistance(rotation * new Vector2(3,-2), rotation * Vector2.up, 4) - 4) < .001f, "No invisible area shielding in removed part");
                    check(Mathf.Abs(AttackGeometry.VisibleDistance(rotation * new Vector2(0,-2), rotation * Vector2.up, 4) - 1.5f) < .001f, "Remaining polygon shields other areas");
                    var obstacle = barrier.GetComponent<TestObstacle>();
                    var snapshots = new List<AttackGeometry.OcclusionBox> { new AttackGeometry.OcclusionBox(obstacle, rotation * new Vector2(0,-2)) };
                    check(Mathf.Abs(AttackGeometry.VisibleDistance(rotation * Vector2.up,4,snapshots) - 1.5f) < .001f, "Prepared polygon shielding matches live geometry");
                    var mesh = barrier.GetComponent<MeshFilter>().sharedMesh;
                    check(mesh.vertexCount == geometry.Points.Count + 1, "Render mesh uses collider outline");
                    var probe = new GameObject("Moving PG"); probe.layer = LayerMask.NameToLayer("PG");
                    probe.transform.position = rotation * new Vector2(3,-2); probe.AddComponent<CircleCollider2D>().radius = .2f;
                    var actor = probe.AddComponent<Combatant>(); actor.Initialize(Faction.PG,new CombatStats { HP=100, DEF=100 });
                    actor.Move(rotation * Vector2.up * 4);
                    check(Vector2.Distance(probe.transform.position, rotation * new Vector2(3,2)) < .01f, "PG crosses removed part");
                    UnityEngine.Object.DestroyImmediate(probe);
                    if (angle == 0)
                    {
                        var navObject = new GameObject("Navigation check"); var nav = navObject.AddComponent<TestNavigation>(); nav.Build(Vector2.one * 20);
                        check(nav.Ready, "Clipped navigation builds");
                        check(nav.Sample(new Vector2(3,0),out _, .1f), "Removed part remains navigable");
                        check(!nav.Sample(Vector2.zero,out _, .1f), "Remaining part blocks navigation");
                        UnityEngine.Object.DestroyImmediate(navObject);
                    }
                    barrier.Tick(5); check(barrier.Remaining == 0, "Clipped barrier keeps five-second lifetime");
                    barrier.gameObject.SetActive(false); check(!TestObstacle.All.Contains(obstacle), "Removing barrier clears shielding registry");
                    UnityEngine.Object.DestroyImmediate(barrier.gameObject);
                    block.transform.position = Vector3.zero; Physics2D.SyncTransforms(); geometry.Build(Vector2.zero,data.BarrierSize,angle);
                    check(!geometry.HasArea && BarrierEffect.Spawn(Vector2.zero,angle,data,geometry) == null, "Fully covered center creates no barrier");
                    UnityEngine.Object.DestroyImmediate(block);
                }
                var clear = new BarrierGeometry(); clear.Build(Vector2.zero,data.BarrierSize,0);
                check(clear.HasArea && clear.Points.Count == 4, "Clear rectangle reduces to four exact corners");
            }
            finally { TestVisuals.Root=null; UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(data); }
        }
    }
}
