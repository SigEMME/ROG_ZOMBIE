using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace RogZombie.TestEngine
{
    [DefaultExecutionOrder(100)]
    public sealed class PlayerWeapon : MonoBehaviour
    {
        public WeaponDefinition Definition;
        public Transform Muzzle;
        public bool ShowDebug;
        public System.Action<Vector2, Vector2, CombatStats> BaseAttackOverride { get; set; }
        public System.Func<bool> InputBlocked { get; set; }
        public int? BasePenetrationsOverride { get; set; }
        public IProjectileHitEffect BaseProjectileEffect { get; set; }
        private Combatant actor;
        private PlayerAim aim;
        private float readyAt;
        private Vector2 lastImpact;
        private float flashUntil;
        private struct PendingExplosion
        {
            public Vector2 Point;
            public float At, Radius, Damage;
        }
        private readonly List<PendingExplosion> explosions = new List<PendingExplosion>();

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                var shot = explosions[i];
                if (Time.time < shot.At) continue;
                explosions.RemoveAt(i);
                CombatAttacks.Circular(shot.Point, shot.Radius, 4, 0f, shot.Damage);
                TestVisuals.FlashCircle(shot.Point, shot.Radius, new Color(1f, 0.7f, 0.1f));
            }
        }

        private void Awake() { actor = GetComponent<Combatant>(); aim = GetComponent<PlayerAim>(); }

        private void LateUpdate()
        {
            if (Time.timeScale == 0f || actor == null || !actor.IsActive || Definition == null || Muzzle == null || TestHUD.PointerOverControls || (InputBlocked != null && InputBlocked())) return;
            if (Mouse.current == null || !Mouse.current.leftButton.isPressed || Time.time < readyAt) return;
            if (!aim.TryGetCursorWorldPosition(out var cursor)) return;
            TryFireAt(cursor);
        }

        public bool TryFireAt(Vector2 cursor)
        {
            if (actor == null || !actor.IsActive || Definition == null || Muzzle == null || Time.timeScale == 0f || Time.time < readyAt) return false;
            Vector2 origin = Muzzle.position;
            Vector2 offset = cursor - origin;
            // Degenerate aiming has no defined shot direction; wait for a valid point.
            var stats = actor.EffectiveStats;
            if (offset.sqrMagnitude < 0.000001f || stats.AttackSpeed <= 0f) return false;
            readyAt = Time.time + stats.AttackInterval;
            Vector2 direction = offset.normalized;
            float range = actor.Stats.RangeMetres;
            if (BaseAttackOverride != null) BaseAttackOverride(origin, cursor, stats);
            else switch (Definition.Kind)
            {
                case BaseAttackKind.PhysicalProjectile:
                    TestVisuals.SpawnProjectile(actor, origin, direction, Definition.ProjectileSpeed, range,
                        stats.ATK, Definition.ProjectileRadius, BasePenetrationsOverride ?? Definition.Penetrations, ShowDebug).HitEffect = BaseProjectileEffect;
                    break;
                case BaseAttackKind.HitscanArea:
                    lastImpact = AttackGeometry.WallImpact(origin, origin + direction * Mathf.Min(offset.magnitude, range));
                    explosions.Add(new PendingExplosion { Point = lastImpact, At = Time.time + Definition.GrenadeHitDelay,
                        Radius = Definition.ExplosionRadius, Damage = stats.ATK });
                    break;
                case BaseAttackKind.AreaHitscan:
                    CombatAttacks.Frontal(actor, transform.position, direction, Definition);
                    TestVisuals.FlashFront(transform.position, direction, range, Definition);
                    break;
            }
            flashUntil = Time.time + 0.15f;
            return true;
        }

        private void OnDrawGizmos()
        {
            if (!ShowDebug || actor == null || Definition == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, actor.Stats.RangeMetres);
            if (Definition.Kind == BaseAttackKind.HitscanArea && Time.time < flashUntil)
                Gizmos.DrawWireSphere(lastImpact, Definition.ExplosionRadius);
        }
    }
}
