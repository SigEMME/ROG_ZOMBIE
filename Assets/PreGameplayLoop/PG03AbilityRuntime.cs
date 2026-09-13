using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG03AbilityRuntime : MonoBehaviour
    {
        private LoopSession session;
        private PG03AbilityCatalog data;
        private PlayerWeapon weapon;
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        private Vector2 aimPoint;
        private float elapsed;
        public PG03Ability Selected { get; private set; }
        public int ShotsFired { get; private set; }
        public bool EffectActive => Selected == PG03Ability.TriploSparo && ShotsFired > 0 && ShotsFired < 3;
        public float ActiveRemaining => EffectActive ? Mathf.Max(0, data.TripleInterval * 2 - elapsed) : 0;
        public float CooldownRemaining => cooldown.Remaining;
        public bool CanUse => session != null && session.GameplayRunning && session.Player.Actor.IsActive;
        public string Label => Selected == PG03Ability.ColpoLaser ? "COLPO LASER" : "TRIPLO SPARO";
        public event System.Action<Projectile, int> TripleShot;
        public void Initialize(LoopSession owner, PG03Ability selected, PG03AbilityCatalog catalog)
        {
            session = owner; Selected = selected; data = Instantiate(catalog);
            weapon = GetComponent<PlayerWeapon>();
            cooldown.Restart(data.BaseCooldown(selected), session.CdReduction);
        }
        public void SetAimPoint(Vector2 cursor) => aimPoint = cursor;
        private void Update()
        {
            if (!CanUse) return;
            cooldown.Tick(Time.deltaTime);
            if (!EffectActive) return;
            elapsed += Time.deltaTime;
            while (ShotsFired < 3 && elapsed >= ShotsFired * data.TripleInterval) FireTriple();
        }
        public bool TryActivate(Vector2 cursor)
        {
            if (!CanUse || !cooldown.Ready || EffectActive) return false;
            aimPoint = cursor;
            cooldown.Restart(data.BaseCooldown(Selected), session.CdReduction);
            if (Selected == PG03Ability.ColpoLaser)
            {
                Vector2 origin = transform.position;
                Vector2 direction = (cursor - origin).normalized;
                if (direction == Vector2.zero) direction = transform.right;
                LaserEffect.Cast(session.Player.Actor, origin, direction, data);
            }
            else { ShotsFired = 0; elapsed = 0; FireTriple(); }
            return true;
        }
        private void FireTriple()
        {
            Vector2 origin = weapon.Muzzle.position;
            Vector2 direction = (aimPoint - origin).normalized;
            if (direction == Vector2.zero) direction = transform.right;
            float facing = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Left centre, centre, right centre of three ten-degree sectors.
            var shot = TestVisuals.SpawnProjectile(session.Player.Actor, origin,
                AttackGeometry.Direction(facing + (1 - ShotsFired) * data.TripleAngle / 3),
                weapon.Definition.ProjectileSpeed, data.TripleRange, data.TripleDamage,
                weapon.Definition.ProjectileRadius, 0, false);
            TripleShot?.Invoke(shot, ShotsFired);
            ShotsFired++;
        }
        public void ChangeArea()
        {
            cooldown.ChangeArea(EffectActive, data.BaseCooldown(Selected), session.CdReduction);
            ShotsFired = 0;
        }
        private void OnDestroy() { if (data != null) Destroy(data); }
    }
}
