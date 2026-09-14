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
        private float pestoneSlowUntil;
        private float pestoneSlowPercent;
        private Combatant lethalSource;

        public event Action<Combatant, float> BaseHitLanded;
        public Func<float, float> BaseHitDamage { get; set; }
        public Func<float, float> IncomingHitDamage { get; set; }
        public event Action BeforeHit;
        public event Action PerformedAction;
        public Func<bool> InvisibleQuery { get; set; }
        public Func<float> MovementMultiplier { get; set; }
        public bool IsInvisible => InvisibleQuery != null && InvisibleQuery();
        public float CurrentMovementMultiplier => MovementMultiplier != null ? MovementMultiplier() : 1;
        public void NotifyAction() => PerformedAction?.Invoke();
        public event Action<Combatant> Died;
        public event Action<Combatant> Killed;
        public event Action<Combatant> StateChanged;
        public static event Action<Combatant, float> DamageApplied;
        public static event Action<Combatant, float> HealingApplied;
        public CombatStats Stats => stats;
        public CombatStats InitialStats { get; private set; }
        public ICombatStatModifier SupportModifier { get; set; }
        // Stats/SetStats are persistent. Combat consumes a separate temporary view.
        public ICombatStatModifier PassiveModifier { get; set; }
        public ICombatStatModifier AbilityModifier { get; set; }
        public ICombatHitEffect HitEffect { get; set; }
        public CombatStats EffectiveStats
        {
            get
            {
                var current = AbilityModifier != null ? AbilityModifier.Apply(stats) : stats;
                current = PassiveModifier != null ? PassiveModifier.Apply(current) : current;
                current = SupportModifier != null ? SupportModifier.Apply(current) : current;
                return HitEffect != null ? HitEffect.Apply(current) : current;
            }
        }
        public bool RoundFinalDamage { get; set; }
        public float CurrentHP => currentHP;
        public float HealthFraction => stats.HP > 0f ? currentHP / stats.HP : 0f;
        public Faction Faction => faction;
        public LifeState State => state;
        public bool IsActive => state == LifeState.Active;
        public float Radius => body != null ? body.radius : 0f;
        public float PestoneSlowRemaining => Mathf.Max(0f, pestoneSlowUntil - Time.time);
        public float MovementMetresPerSecond => EffectiveStats.MetresPerSecond * CurrentMovementMultiplier *
            (PestoneSlowRemaining > 0f ? 1f - pestoneSlowPercent / 100f : 1f);

        public void ApplyPestoneSlow(float percent, float seconds)
        {
            if (!IsActive || faction != Faction.MOB) return;
            // GDD 10.6: this effect refreshes, never stacks. Stats remain independent.
            pestoneSlowPercent = Mathf.Clamp(percent, 0f, 100f);
            pestoneSlowUntil = Time.time + Mathf.Max(0f, seconds);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() { All.Clear(); DamageApplied = null; HealingApplied = null; }
        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);

        public void Initialize(Faction side, CombatStats values, bool explodes = false)
        {
            faction = side;
            lethalSource = null;
            HitEffect?.AfterHit();
            pestoneSlowUntil = 0f;
            pestoneSlowPercent = 0f;
            stats = values;
            InitialStats = values;
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

        public void Hit(float attack) => Hit(attack, false);

        // New GDD-compliant effects can round the final mitigated HIT without changing
        // the already validated legacy attacks in this prototype.
        public bool Hit(float attack, bool roundFinalDamage, Combatant source = null, bool baseAttack = false)
        {
            if (!IsActive) return false;
            if (baseAttack && source != null && source.BaseHitDamage != null) attack = source.BaseHitDamage(attack);
            float incoming = IncomingHitDamage != null ? IncomingHitDamage(attack) : attack;
            float damage = DamageMath.Calculate(incoming, EffectiveStats.DEF);
            // Invalid tuning must never turn an incoming HIT into healing.
            if (damage < 0f || float.IsNaN(damage) || float.IsInfinity(damage))
            {
                Debug.LogError("HIT rejected: damage outside the defined non-negative domain. Check ATK/DEF tuning.", this);
                return false;
            }
            if (roundFinalDamage || RoundFinalDamage) damage = Mathf.Floor(damage + .5f);
            BeforeHit?.Invoke();
            float previousHP = currentHP;
            currentHP = Mathf.Max(0f, currentHP - damage);
            if (currentHP <= 0f) lethalSource = source;
            HitEffect?.AfterHit();
            DamageApplied?.Invoke(this, previousHP - currentHP);
            if (currentHP <= 0f)
            {
                if (faction == Faction.PG) ChangeState(LifeState.Down);
                else if (preExplodes) ChangeState(LifeState.PreExplosion);
                else Die();
            }
            if (baseAttack && source != null) source.BaseHitLanded?.Invoke(this, attack);
            return true;
        }

        public bool Heal(float fraction)
        {
            if (!IsActive || currentHP >= stats.HP) return false;
            float previousHP = currentHP;
            currentHP = Mathf.Min(stats.HP, currentHP + stats.HP * fraction);
            float recovered = currentHP - previousHP;
            if (recovered > 0) HealingApplied?.Invoke(this, recovered);
            return true;
        }

        public void Die()
        {
            if (state == LifeState.Dead) return;
            if (body != null) body.enabled = false;
            ChangeState(LifeState.Dead);
            Died?.Invoke(this);
            // Credit only a lethal HIT with a known PG source, once at effective death.
            var killer = lethalSource;
            lethalSource = null;
            if (faction == Faction.MOB && killer != null && killer.Faction == Faction.PG)
                killer.Killed?.Invoke(this);
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
                        (faction == Faction.MOB && hit.collider.GetComponent<RogZombie.PreGameplayLoop.BonusPet>() != null) ||
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
