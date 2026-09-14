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
        public bool IsBaseAttack { get; set; }
        public float Travelled => travelled;
        public float LastHitSimulationTime { get; private set; }
        public Vector2 Direction => direction;
        public float Speed => speed;
        private Faction sourceFaction;
        private Combatant source;
        private bool roundFinalDamage;
        public IProjectileHitEffect HitEffect { private get; set; }
        private readonly HashSet<Combatant> hitActors = new HashSet<Combatant>();

        public void Initialize(Combatant owner, Vector2 heading, float velocity, float maxRange, float attack, float colliderRadius, int piercing, bool debug)
        {
            source = owner;
            roundFinalDamage = owner.RoundFinalDamage;
            sourceFaction = owner.Faction;
            direction = heading.normalized;
            speed = velocity; range = maxRange; damage = attack; radius = colliderRadius;
            penetrations = piercing; showDebug = debug; start = transform.position;
        }

        public void ConfigureBaseAttack(IProjectileHitEffect effect = null) { IsBaseAttack = true; HitEffect = effect; }

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
                LastHitSimulationTime = Time.time - Time.deltaTime + (speed > 0 ? hit.distance / speed : 0);
                hitActors.Add(actor);
                if (HitEffect != null) HitEffect.ResolveHit(actor, damage, roundFinalDamage, source, hitActors.Count - 1);
                else actor.Hit(damage, roundFinalDamage, source, IsBaseAttack);
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
