using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    public sealed class SpawnManager : MonoBehaviour
    {
        public bool ShowSpawnPoints;
        [SerializeField] private int totalSpawned;
        [SerializeField] private int alive;
        [SerializeField] private int killed;
        [SerializeField] private int maxSimultaneous;
        [SerializeField] private int[] remaining;
        private int[] initial;
        private int pending;
        private int reservedSpawn;
        private readonly HashSet<Combatant> tracked = new HashSet<Combatant>();
        private bool initialized;
        private TestAreaSettings settings;
        private TestNavigation navigation;
        private ISpawnWorld world;
        private Vector2 lastCandidate;
        private bool lastValid;
        public int TotalSpawned => totalSpawned;
        public int Alive => alive;
        public int Killed => killed;
        public int MaxSimultaneous => maxSimultaneous;
        public bool InitialComplete { get; private set; }

        public void Initialize(TestAreaSettings config, TestNavigation nav, ISpawnWorld owner)
        {
            if (initialized) throw new InvalidOperationException("SpawnManager is already initialized; refusing a duplicate spawn loop.");
            initialized = true;
            settings = config; navigation = nav; world = owner;
            maxSimultaneous = Mathf.CeilToInt(settings.TotalMobs * settings.FirstSpawnPercent / 100f);
            remaining = Allocate(settings.TotalMobs, settings.MobPercentages);
            initial = Allocate(maxSimultaneous, settings.MobPercentages);
            int excluded = initial[4]; initial[4] = 0;
            initial[0] += excluded / 2;
            initial[1] += excluded - excluded / 2;
            for (int i = 0; i < initial.Length; i++)
                if (initial[i] > remaining[i]) throw new InvalidOperationException("FIRST SPAWN exceeds type quota; review AREA tuning.");
            StartCoroutine(SpawnLoop());
        }

        public static int[] Allocate(int total, float[] percentages)
        {
            var counts = new int[percentages.Length];
            counts[0] = total;
            for (int i = 1; i < counts.Length; i++)
            {
                counts[i] = Mathf.CeilToInt(total * percentages[i] / 100f);
                counts[0] -= counts[i];
            }
            if (counts[0] < 0) throw new InvalidOperationException("AREA rounding leaves a negative ZOMB01 quota.");
            return counts;
        }

        private IEnumerator SpawnLoop()
        {
            for (int i = 0; i < initial.Length; i++)
                for (int n = 0; n < initial[i]; n++) yield return SpawnOne(i);
            InitialComplete = true;
            while (totalSpawned < settings.TotalMobs)
            {
                if (pending <= 0 || alive >= maxSimultaneous || Time.timeScale == 0f) { yield return null; continue; }
                var weights = new float[remaining.Length];
                for (int i = 0; i < weights.Length; i++) weights[i] = remaining[i] > 0 ? settings.MobPercentages[i] : 0f;
                int type = WeightedSelection.Draw(weights, UnityEngine.Random.value);
                if (type < 0) yield break;
                pending--;
                reservedSpawn = 1;
                yield return SpawnOne(type);
                reservedSpawn = 0;
            }
        }

        private IEnumerator SpawnOne(int type)
        {
            Combatant reference = null;
            int attempts = 0;
            float extra = settings.OffscreenExtraRange;
            while (true)
            {
                if (!world.Loading && Time.timeScale == 0f) { yield return null; continue; }
                if (reference == null || !reference.IsActive)
                {
                    var players = Combatant.All.FindAll(a => a.Faction == Faction.PG && a.IsActive);
                    if (players.Count == 0) { yield return null; continue; }
                    reference = players[UnityEngine.Random.Range(0, players.Count)];
                }
                Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
                Camera camera = world.GameCamera;
                float halfHeight = camera.orthographicSize;
                float halfWidth = halfHeight * camera.aspect;
                float edge = Mathf.Min(halfWidth / Mathf.Max(Mathf.Abs(direction.x), 0.00001f), halfHeight / Mathf.Max(Mathf.Abs(direction.y), 0.00001f));
                Vector2 candidate = (Vector2)reference.transform.position + direction * (edge + UnityEngine.Random.Range(0.05f, extra));
                lastCandidate = candidate;
                lastValid = navigation.Sample(candidate, out var valid, 0.5f) && OffscreenForAll(valid) && HasRoom(valid) && ReachableFromAny(valid);
                if (lastValid)
                {
                    if (totalSpawned >= settings.TotalMobs || alive >= maxSimultaneous || remaining[type] <= 0) yield break;
                    var mob = world.CreateMob(type, valid);
                    tracked.Add(mob);
                    mob.Died += OnDeath;
                    remaining[type]--; alive++; totalSpawned++;
                    reservedSpawn = 0;
                    yield break;
                }
                attempts++;
                if (attempts % settings.SearchAttemptsPerFrame == 0)
                {
                    // Expand the candidate domain, but keep retrying inside the finite AREA as well.
                    extra = Mathf.Min(extra + settings.OffscreenExtraRange, settings.AreaSize.magnitude);
                    yield return null;
                }
            }
        }

        private bool ReachableFromAny(Vector2 point)
        {
            foreach (var actor in Combatant.All)
                if (actor.Faction == Faction.PG && actor.IsActive && navigation.Reachable(point, actor.transform.position)) return true;
            return false;
        }

        private bool HasRoom(Vector2 point)
        {
            if (!settings.EnableMobSeparation) return true;
            foreach (var actor in Combatant.All)
                if (actor != null && actor.Faction == Faction.MOB && actor.State != LifeState.Dead &&
                    Vector2.Distance(point, actor.transform.position) < settings.ActorRadius + actor.Radius + settings.MobSpacing) return false;
            return true;
        }

        private bool OffscreenForAll(Vector2 point)
        {
            float halfHeight = world.GameCamera.orthographicSize;
            float halfWidth = halfHeight * world.GameCamera.aspect;
            foreach (var actor in Combatant.All)
            {
                if (actor.Faction != Faction.PG || !actor.IsActive) continue;
                Vector2 delta = point - (Vector2)actor.transform.position;
                if (Mathf.Abs(delta.x) <= halfWidth + settings.ActorRadius && Mathf.Abs(delta.y) <= halfHeight + settings.ActorRadius) return false;
            }
            return true;
        }

        private void OnDeath(Combatant actor)
        {
            if (actor == null || actor.State != LifeState.Dead || !tracked.Remove(actor)) return;
            actor.Died -= OnDeath;
            alive--; killed++;
            var definition = actor.GetComponent<MobBrain>().Definition;
            foreach (var player in Combatant.All.ToArray())
            {
                if (player == null || player.Faction != Faction.PG || player.State == LifeState.Dead) continue;
                player.GetComponent<ExperienceProgression>()?.Award(definition.ExpDrop);
            }
            if (Combatant.All.Exists(a => a.Faction == Faction.PG && a.State != LifeState.Dead)) world.AddGold(definition.GoldDrop);
            if (totalSpawned + pending + reservedSpawn < settings.TotalMobs) pending++;
        }

        private void OnDestroy()
        {
            foreach (var mob in tracked) if (mob != null) mob.Died -= OnDeath;
            tracked.Clear();
        }

        private void OnDrawGizmos()
        {
            if (!ShowSpawnPoints) return;
            Gizmos.color = lastValid ? Color.green : Color.red;
            Gizmos.DrawWireSphere(lastCandidate, 0.4f);
        }
    }
}
