using UnityEngine;
using UnityEngine.AI;

namespace RogZombie.TestEngine
{
    [RequireComponent(typeof(Combatant))]
    public sealed class MobBrain : MonoBehaviour
    {
        public MobDefinition Definition;
        public bool ShowAggro;
        public bool ShowAttackArea;
        [Tooltip("A separate visual component replaces the temporary death tint/squash.")]
        public bool HasAnimatedPresentation;
        public event System.Action<Combatant> MeleeAttackPerformed;
        [Min(8)] public int PositionSearchSamples = 32;
        [SerializeField] private Combatant target;
        private Combatant actor;
        private TestNavigation navigation;
        private NavMeshPath path;
        private Vector3[] corners;
        private int corner;
        private float nextAggro;
        private float nextPath;
        private float attackReady;
        private float pendingImpact = -1f;
        private float explosionAt = -1f;
        private float stunnedUntil;
        private Vector2 fixedImpact;
        private bool positioned;
        private bool retreating;
        private bool died;

        public void Initialize(MobDefinition definition, TestNavigation nav)
        {
            Definition = definition;
            navigation = nav;
            actor = GetComponent<Combatant>();
            actor.Initialize(Faction.MOB, definition.BaseStats, definition.Kind == MobKind.ZOMB05);
            actor.StateChanged += OnState;
            path = new NavMeshPath();
            nextAggro = Time.time + Random.value * Definition.AggroInterval;
        }

        private void Update()
        {
            if (Definition == null || actor == null || Time.timeScale == 0f) return;
            if (actor.State == LifeState.Dead) return;
            if (actor.State == LifeState.PreExplosion)
            {
                if (Time.time >= explosionAt)
                {
                    CombatAttacks.Circular(transform.position, Definition.ExplosionRadius, 8,
                        Definition.ExplosionDamagePG, Definition.ExplosionDamageMOB, actor);
                    TestVisuals.FlashCircle(transform.position, Definition.ExplosionRadius, Color.red);
                    actor.Die();
                }
                return;
            }
            if (Time.time < stunnedUntil) return;
            if (Time.time >= nextAggro) ChooseTarget();
            if (pendingImpact >= 0f)
            {
                if (Time.time < pendingImpact) return;
                pendingImpact = -1f;
                CombatAttacks.Circular(fixedImpact, Definition.ImpactRadius, 8, actor.Stats.ATK, 0f);
                TestVisuals.FlashCircle(fixedImpact, Definition.ImpactRadius, new Color(0.7f, 0.2f, 0.9f));
            }
            if (target == null || !target.IsActive) return;
            float distance = Vector2.Distance(transform.position, target.transform.position);
            switch (Definition.Kind)
            {
                case MobKind.ZOMB03: UpdateAreaMob(distance); break;
                case MobKind.ZOMB04: UpdateRangedMob(distance); break;
                default:
                    Follow(target.transform.position);
                    if (distance <= actor.Stats.RangeMetres && ReadyToAttack())
                    {
                        target.Hit(actor.Stats.ATK);
                        attackReady = Time.time + actor.Stats.AttackInterval;
                        MeleeAttackPerformed?.Invoke(target);
                    }
                    break;
            }
        }

        private bool ReadyToAttack() => actor.Stats.AttackSpeed > 0f && Time.time >= attackReady;

        private void UpdateAreaMob(float distance)
        {
            if (positioned && distance > actor.Stats.RangeMetres) positioned = false;
            if (!positioned && distance <= Definition.StopDistance) positioned = true;
            if (!positioned) { Follow(target.transform.position); return; }
            if (!ReadyToAttack() || distance > actor.Stats.RangeMetres) return;
            fixedImpact = target.transform.position;
            pendingImpact = Time.time + Definition.WindupSeconds;
            attackReady = Time.time + actor.Stats.AttackInterval;
        }

        private void UpdateRangedMob(float distance)
        {
            if (!positioned && distance < Definition.RetreatDistance) retreating = true;
            bool clear = AttackGeometry.ClearLine(transform.position, target.transform.position);
            if (positioned && (distance > actor.Stats.RangeMetres || distance < Definition.RetreatDistance || !clear))
            {
                positioned = false;
                retreating = distance < Definition.RetreatDistance;
                nextPath = 0f;
            }
            if (!positioned && clear && ((!retreating && distance <= Definition.PreferredDistance && distance >= Definition.RetreatDistance) ||
                (retreating && distance >= Definition.PreferredDistance && distance <= actor.Stats.RangeMetres)))
            {
                positioned = true;
                retreating = false;
            }
            if (!positioned)
            {
                if (Time.time >= nextPath)
                {
                    nextPath = Time.time + Definition.AggroInterval;
                    FindFiringPosition();
                }
                WalkPath();
                return;
            }
            if (!ReadyToAttack()) return;
            Vector2 direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
            TestVisuals.SpawnProjectile(actor, transform.position, direction, Definition.ProjectileSpeed,
                actor.Stats.RangeMetres, actor.Stats.ATK, Definition.ProjectileRadius, 0, ShowAttackArea);
            attackReady = Time.time + actor.Stats.AttackInterval;
        }

        private void FindFiringPosition()
        {
            corners = null;
            float shortest = float.PositiveInfinity;
            for (int i = 0; i < PositionSearchSamples; i++)
            {
                Vector2 candidate = (Vector2)target.transform.position + AttackGeometry.Direction(i * 360f / PositionSearchSamples) * (Definition.PreferredDistance + 0.01f);
                if (!navigation.Sample(candidate, out candidate) || !AttackGeometry.ClearLine(candidate, target.transform.position)) continue;
                if (Vector2.Distance(candidate, target.transform.position) < Definition.PreferredDistance) continue;
                if (!navigation.Path(transform.position, candidate, path)) continue;
                var route = path.corners;
                float length = 0f;
                for (int j = 1; j < route.Length; j++) length += Vector3.Distance(route[j - 1], route[j]);
                if (length >= shortest) continue;
                shortest = length;
                corners = route;
            }
            corner = 1;
            // No reachable clear position: IDLE, then search again at the next interval.
        }

        private void Follow(Vector2 destination)
        {
            if (Time.time >= nextPath)
            {
                nextPath = Time.time + Definition.AggroInterval;
                corners = navigation.Path(transform.position, destination, path) ? path.corners : null;
                corner = 1;
            }
            if (corners == null)
                actor.Move((destination - (Vector2)transform.position).normalized * actor.MovementMetresPerSecond * Time.deltaTime);
            else WalkPath();
        }

        private void WalkPath()
        {
            if (corners == null) return;
            while (corner < corners.Length && Vector2.Distance(transform.position, TestNavigation.FromNav(corners[corner])) < 0.001f) corner++;
            if (corner >= corners.Length) return;
            Vector2 offset = TestNavigation.FromNav(corners[corner]) - (Vector2)transform.position;
            actor.Move(Vector2.ClampMagnitude(offset, actor.MovementMetresPerSecond * Time.deltaTime));
        }

        private void ChooseTarget()
        {
            Combatant selected = null;
            float best = float.PositiveInfinity;
            int ties = 0;
            foreach (var candidate in Combatant.All)
            {
                if (candidate.Faction != Faction.PG || !candidate.IsActive) continue;
                float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distance < best - 0.000001f) { selected = candidate; best = distance; ties = 1; }
                else if (Mathf.Abs(distance - best) < 0.000001f && Random.Range(0, ++ties) == 0) selected = candidate;
            }
            if (selected != target)
            {
                if (target != null) target.StateChanged -= TargetStateChanged;
                target = selected;
                if (target != null) target.StateChanged += TargetStateChanged;
                nextPath = 0f;
            }
            nextAggro = Time.time + Definition.AggroInterval;
        }

        private void TargetStateChanged(Combatant changed) { if (!changed.IsActive) ChooseTarget(); }
        public void InterruptForPush() { pendingImpact = -1f; nextPath = 0f; }
        public void Stun(float seconds)
        {
            if (!actor.IsActive) return;
            pendingImpact = -1f;
            stunnedUntil = Time.time + seconds;
            attackReady = stunnedUntil;
            positioned = false;
            nextPath = stunnedUntil;
        }

        private void OnState(Combatant changed)
        {
            if (changed.State == LifeState.PreExplosion)
            {
                pendingImpact = -1f;
                explosionAt = Time.time + Definition.PreExplosionSeconds;
                GetComponent<SpriteRenderer>().color = Color.red;
            }
            if (changed.State == LifeState.Dead && !died)
            {
                died = true;
                pendingImpact = -1f;
                if (!HasAnimatedPresentation)
                {
                    GetComponent<SpriteRenderer>().color = new Color(0.25f, 0.22f, 0.2f);
                    transform.localScale = new Vector3(1f, 0.3f, 1f);
                }
                Destroy(gameObject, Definition.DeathSpriteSeconds);
            }
        }

        private void OnDestroy()
        {
            if (target != null) target.StateChanged -= TargetStateChanged;
            if (actor != null) actor.StateChanged -= OnState;
        }

        private void OnDrawGizmos()
        {
            if (ShowAggro && target != null) { Gizmos.color = Color.red; Gizmos.DrawLine(transform.position, target.transform.position); }
            if (ShowAttackArea && Definition != null)
            {
                Gizmos.color = Color.magenta;
                if (pendingImpact >= 0f) Gizmos.DrawWireSphere(fixedImpact, Definition.ImpactRadius);
                if (explosionAt >= 0f && !died) Gizmos.DrawWireSphere(transform.position, Definition.ExplosionRadius);
            }
        }
    }
}
