using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private float range;
        [SerializeField] private float radius;
        [SerializeField] private float damage;
        [SerializeField] private int penetrations;
        [SerializeField] private bool showDebug;
        private Vector2 direction;
        private Vector2 start;
        private float travelled;
        public float Travelled => travelled;
        public Vector2 Direction => direction;
        public float Speed => speed;
        private Faction sourceFaction;
        private Combatant source;
        private readonly HashSet<Combatant> hitActors = new HashSet<Combatant>();

        public void Initialize(Combatant owner, Vector2 heading, float velocity, float maxRange, float attack, float colliderRadius, int piercing, bool debug)
        {
            source = owner;
            sourceFaction = owner.Faction;
            direction = heading.normalized;
            speed = velocity; range = maxRange; damage = attack; radius = colliderRadius;
            penetrations = piercing; showDebug = debug; start = transform.position;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f || Time.deltaTime <= 0f) return;
            float step = Mathf.Min(speed * Time.deltaTime, range - travelled);
            if (range - travelled <= 0f) { Destroy(gameObject); return; }
            // A zero simulation step is suspension, not expiry (also on pause/resume frames).
            if (step <= 0f) return;
            Physics2D.SyncTransforms();
            var hits = Physics2D.CircleCastAll(transform.position, radius, direction, step);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var actor = hit.collider.GetComponent<Combatant>();
                if (actor == source) continue;
                if (hit.collider.GetComponent<TestObstacle>() != null)
                {
                    Destroy(gameObject); return;
                }
                if (actor == null || actor.State == LifeState.Dead || actor.State == LifeState.Down || hitActors.Contains(actor)) continue;
                // Normal MOB attacks target PGs; friendly MOB damage is explicitly reserved for ZOMB05.
                if (actor.Faction == sourceFaction) continue;
                hitActors.Add(actor);
                actor.Hit(damage);
                if (penetrations-- <= 0) { Destroy(gameObject); return; }
            }
            transform.position += (Vector3)(direction * step);
            travelled += step;
            if (travelled >= range) Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            if (!showDebug) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, start + direction * range);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
