using System;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    public static class OptimizationChecks
    {
        public static void Run(LoopSession loop, Action<bool, string> check)
        {
            var objects = new List<GameObject>();
            try
            {
                var wall = new GameObject("Optimization rotated wall"); objects.Add(wall);
                wall.layer = LayerMask.NameToLayer("MURO"); wall.transform.position = new Vector2(2, .3f);
                wall.transform.rotation = Quaternion.Euler(0, 0, 37);
                var box = wall.AddComponent<BoxCollider2D>(); box.size = new Vector2(.6f, 3); box.offset = new Vector2(.1f, .2f);
                var obstacle = wall.AddComponent<TestObstacle>();
                var visualObject = new GameObject("Optimization visual"); objects.Add(visualObject);
                var visual = visualObject.AddComponent<PG04AreaVisual>();
                Mesh originalMesh = null; Material originalMaterial = null;
                var snapshots = new List<AttackGeometry.OcclusionBox>();
                float error = 0;
                for (int state = 0; state < 5; state++)
                {
                    wall.transform.position = new Vector2(2 - state * .3f, .3f);
                    wall.transform.localScale = new Vector3(1 + state * .1f, 1, 1);
                    box.enabled = state != 3; wall.SetActive(state != 4); Physics2D.SyncTransforms();
                    snapshots.Clear();
                    if (wall.activeSelf && box.enabled) snapshots.Add(new AttackGeometry.OcclusionBox(obstacle, Vector2.zero));
                    for (int ray = 0; ray < 720; ray++)
                    {
                        var direction = AttackGeometry.Direction(ray * .5f);
                        error = Mathf.Max(error, Mathf.Abs(AttackGeometry.VisibleDistance(Vector2.zero, direction, 4) - AttackGeometry.VisibleDistance(direction, 4, snapshots)));
                    }
                    visual.InitializeOccluded(Vector2.zero, 4, Color.red, 0, state % 2 == 0 ? 360 : 180, state == 2 ? 3 : 0);
                    var filter = visualObject.GetComponent<MeshFilter>(); var renderer = visualObject.GetComponent<MeshRenderer>();
                    if (state == 0) { originalMesh = filter.sharedMesh; originalMaterial = renderer.sharedMaterial; }
                    check(filter.sharedMesh == originalMesh && renderer.sharedMaterial == originalMaterial, "Visual reuses mesh/material after geometry change " + state);
                    check(filter.sharedMesh.vertexCount == filter.sharedMesh.colors32.Length && filter.sharedMesh.uv.Length == filter.sharedMesh.vertexCount, "Rebuilt visual retains colors and UV " + state);
                }
                check(error < .00001f, "3600 occlusion rays match live geometry (moved/scaled/disabled/removed cover)");
                wall.SetActive(true); box.enabled = true; Physics2D.SyncTransforms();
                var mesh = visualObject.GetComponent<MeshFilter>().sharedMesh;
                var fullVertices = mesh.vertices;
                visual.InitializeOccluded(Vector2.zero, 4, Color.red);
                bool changed = false;
                var clipped = mesh.vertices;
                for (int i = 0; i < Mathf.Min(fullVertices.Length, clipped.Length); i++) if (Vector3.Distance(fullVertices[i], clipped[i]) > .01f) { changed = true; break; }
                check(changed, "Visual refresh restores shadow when cover reappears");
                wall.SetActive(false);

                var random = new System.Random(310);
                var subjects = new List<Combatant>();
                for (int i = 0; i < 32; i++)
                {
                    var go = new GameObject("Optimization actor"); objects.Add(go);
                    go.layer = LayerMask.NameToLayer("MOB"); go.AddComponent<CircleCollider2D>().radius = .2f + i % 3 * .1f;
                    var actor = go.AddComponent<Combatant>(); actor.Initialize(i == 31 ? Faction.PG : Faction.MOB, new CombatStats { HP = 100, DEF = 100 }); subjects.Add(actor);
                }
                var source = subjects[0]; var separation = source.gameObject.AddComponent<MobSeparation>(); separation.Settings = loop.Settings;
                float maxDifference = 0;
                for (int sample = 0; sample < 128; sample++)
                {
                    foreach (var actor in subjects) actor.transform.position = new Vector2((float)random.NextDouble() * 6 - 3, (float)random.NextDouble() * 6 - 3);
                    source.transform.position = Vector2.zero;
                    if (sample % 8 == 0) subjects[1].transform.position = Vector2.zero;
                    Vector2 desired = new Vector2((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1) * (sample % 3 == 0 ? 2 : .04f);
                    Vector2 expected = LegacySteer(source, loop.Settings, desired);
                    maxDifference = Mathf.Max(maxDifference, Vector2.Distance(expected, separation.Steer(desired)));
                }
                check(maxDifference < .00001f, "128 MOB steering cases match original algorithm, including overlap and long movement");
                foreach (var actor in subjects) actor.gameObject.SetActive(false);
                wall.SetActive(true); wall.transform.position = new Vector2(1, 0); Physics2D.SyncTransforms();
                var player = loop.Player.Actor;
                Vector3 previous = player.transform.position;
                for (int i = 0; i < 8; i++)
                {
                    player.transform.position = Vector2.zero; Physics2D.SyncTransforms();
                    Vector2 direction = new Vector2(2, (i - 4) * .2f);
                    var expected = Physics2D.CircleCastAll(Vector2.zero, player.Radius, direction.normalized, direction.magnitude);
                    var actual = new List<RaycastHit2D>(); var filter = ContactFilter2D.noFilter;
                    filter.SetLayerMask(Physics2D.DefaultRaycastLayers); filter.useTriggers = Physics2D.queriesHitTriggers;
                    Physics2D.CircleCast(Vector2.zero, player.Radius, direction.normalized, filter, actual, direction.magnitude);
                    check(expected.Length == actual.Count, "Buffered movement query retains all hits " + i);
                    for (int hit = 0; hit < expected.Length; hit++) check(expected[hit].collider == actual[hit].collider && Mathf.Abs(expected[hit].distance - actual[hit].distance) < .00001f, "Buffered movement hit matches original " + i + "/" + hit);
                }
                player.transform.position = previous;
            }
            finally { foreach (var go in objects) { go.SetActive(false); UnityEngine.Object.Destroy(go); } Physics2D.SyncTransforms(); }
        }
        // Reference algorithm from before optimization: intentionally retains full registry scans.
        private static Vector2 LegacySteer(Combatant actor, TestAreaSettings settings, Vector2 desired)
        {
            if (!settings.EnableMobSeparation || desired.sqrMagnitude == 0) return desired;
            bool Neighbour(Combatant other) => other != null && other != actor && other.Faction == Faction.MOB && other.State != LifeState.Dead;
            Vector2 Normal(Vector2 away, Combatant other) => away.sqrMagnitude > .000001f ? away.normalized : actor.GetInstanceID() < other.GetInstanceID() ? Vector2.right : Vector2.left;
            float distance = desired.magnitude; Vector2 direction = desired / distance, avoidance = Vector2.zero;
            foreach (var other in Combatant.All)
            {
                if (!Neighbour(other)) continue;
                Vector2 away = (Vector2)actor.transform.position - (Vector2)other.transform.position;
                float gap = away.magnitude, spacing = actor.Radius + other.Radius + settings.MobSpacing;
                if (gap > spacing * 1.5f) continue;
                Vector2 normal = Normal(away, other);
                float approaching = Mathf.Max(0, -Vector2.Dot(direction, normal));
                float weight = Mathf.Clamp01((spacing * 1.5f - gap) / spacing);
                Vector2 tangent = new Vector2(-normal.y, normal.x);
                if (Vector2.Dot(tangent, direction) < 0) tangent = -tangent;
                avoidance += (normal + tangent * approaching) * weight;
            }
            Vector2 result = Vector2.ClampMagnitude(desired + avoidance * settings.MobSeparationSpeed * Time.deltaTime, distance);
            for (int pass = 0; pass < 2; pass++) foreach (var other in Combatant.All)
            {
                if (!Neighbour(other)) continue;
                Vector2 away = (Vector2)actor.transform.position - (Vector2)other.transform.position;
                float clearance = Mathf.Max(0, away.magnitude - actor.Radius - other.Radius - settings.MobSpacing);
                if (clearance >= result.magnitude) continue;
                Vector2 normal = Normal(away, other); float approach = -Vector2.Dot(result, normal);
                if (approach > clearance) result += normal * (approach - clearance);
            }
            return result;
        }
    }
}
