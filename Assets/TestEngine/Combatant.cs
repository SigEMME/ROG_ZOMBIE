using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class Combatant : MonoBehaviour
    {
        public static readonly List<Combatant> All = new List<Combatant>();
        [SerializeField] private Faction faction;
        [SerializeField] private CombatStats stats;
        [SerializeField] private float currentHP;
        [SerializeField] private LifeState state;
        [SerializeField] private bool preExplodes;
        [SerializeField] private bool showCollider;
        private CircleCollider2D body;

        public event Action<Combatant> Died;
        public event Action<Combatant> StateChanged;
        public static event Action<Combatant, float> DamageApplied;
        public CombatStats Stats => stats;
        public float CurrentHP => currentHP;
        public float HealthFraction => stats.HP > 0f ? currentHP / stats.HP : 0f;
        public Faction Faction => faction;
        public LifeState State => state;
        public bool IsActive => state == LifeState.Active;
        public float Radius => body != null ? body.radius : 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() { All.Clear(); DamageApplied = null; }
        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);

        public void Initialize(Faction side, CombatStats values, bool explodes = false)
        {
            faction = side;
            stats = values;
            currentHP = values.HP;
            state = LifeState.Active;
            preExplodes = explodes;
            body = GetComponent<CircleCollider2D>();
            body.enabled = true;
        }

        public void SetStats(CombatStats values, float addedHealth = 0f)
        {
            stats = values;
            currentHP = Mathf.Min(stats.HP, currentHP + addedHealth);
        }

        public void Hit(float attack)
        {
            if (!IsActive) return;
            float damage = DamageMath.Calculate(attack, stats.DEF);
            // Invalid tuning must never turn an incoming HIT into healing.
            if (damage < 0f || float.IsNaN(damage) || float.IsInfinity(damage))
            {
                Debug.LogError("HIT rejected: damage outside the defined non-negative domain. Check ATK/DEF tuning.", this);
                return;
            }
            float previousHP = currentHP;
            currentHP = Mathf.Max(0f, currentHP - damage);
            DamageApplied?.Invoke(this, previousHP - currentHP);
            if (currentHP > 0f) return;
            if (faction == Faction.PG) ChangeState(LifeState.Down);
            else if (preExplodes) ChangeState(LifeState.PreExplosion);
            else Die();
        }

        public bool Heal(float fraction)
        {
            if (!IsActive || currentHP >= stats.HP) return false;
            currentHP = Mathf.Min(stats.HP, currentHP + stats.HP * fraction);
            return true;
        }

        public void Die()
        {
            if (state == LifeState.Dead) return;
            if (body != null) body.enabled = false;
            ChangeState(LifeState.Dead);
            Died?.Invoke(this);
        }

        private void ChangeState(LifeState next)
        {
            state = next;
            StateChanged?.Invoke(this);
        }

        public void Move(Vector2 displacement)
        {
            if (!IsActive) return;
            var separation = GetComponent<MobSeparation>();
            if (separation != null) displacement = separation.Steer(displacement);
            MoveBody(displacement);
        }

        internal void Separate(Vector2 displacement) { if (IsActive) MoveBody(displacement); }

        public void Push(Vector2 displacement)
        {
            if (state == LifeState.Dead || state == LifeState.Down) return;
            GetComponent<MobBrain>()?.InterruptForPush();
            MoveBody(displacement);
        }

        private void MoveBody(Vector2 displacement)
        {
            // Hard sweeps handle walls and PG contact; MobSeparation steers around MOB neighbours.
            Physics2D.SyncTransforms();
            Vector2 position = transform.position;
            for (int pass = 0; pass < 2 && displacement.sqrMagnitude > 0.0000001f; pass++)
            {
                float distance = displacement.magnitude;
                Vector2 direction = displacement / distance;
                RaycastHit2D? closest = null;
                foreach (var hit in Physics2D.CircleCastAll(position, Radius, direction, distance))
                {
                    if (hit.collider == body) continue;
                    var other = hit.collider.GetComponent<Combatant>();
                    bool blocks = hit.collider.GetComponent<TestObstacle>() != null ||
                        (other != null && other.state != LifeState.Dead &&
                         (faction == Faction.PG || other.faction == Faction.PG));
                    if (!blocks || Vector2.Dot(direction, hit.normal) >= -0.001f) continue;
                    if (!closest.HasValue || hit.distance < closest.Value.distance) closest = hit;
                }
                if (!closest.HasValue) { position += displacement; break; }
                var collision = closest.Value;
                float travel = Mathf.Max(0f, collision.distance - 0.001f);
                position += direction * travel;
                displacement -= direction * travel;
                displacement -= collision.normal * Vector2.Dot(displacement, collision.normal);
            }
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private void OnDrawGizmos()
        {
            if (!showCollider) return;
            var circle = GetComponent<CircleCollider2D>();
            if (circle == null) return;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, circle.radius);
        }
    }
}
