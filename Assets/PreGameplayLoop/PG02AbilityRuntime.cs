using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed class PG02AbilityRuntime : MonoBehaviour, ICombatStatModifier
    {
        private LoopSession session;
        private PG02AbilityCatalog data;
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        private PlayerWeapon weapon;
        private Vector2 aimPoint;
        private float burstElapsed;
        public int ShotsFired { get; private set; }
        public event System.Action<Projectile, int> SuppressionShot;
        public PG02Ability Selected { get; private set; }
        public float ActiveRemaining { get; private set; }
        public bool EffectActive => ActiveRemaining > 0;
        public float CooldownRemaining => cooldown.Remaining;
        public bool CanUse => session != null && session.GameplayRunning && session.Player.Actor.IsActive;
        public string Label => Selected == PG02Ability.FuocoRapido ? "FUOCO RAPIDO" : "FUOCO DI SOPPRESSIONE";

        public void Initialize(LoopSession owner, PG02Ability selected, PG02AbilityCatalog catalog)
        {
            session = owner;
            Selected = selected;
            data = Instantiate(catalog);
            weapon = GetComponent<PlayerWeapon>();
            cooldown.Restart(data.BaseCooldown(selected), session.CdReduction);
            session.Player.Actor.AbilityModifier = this;
        }

        public CombatStats Apply(CombatStats current)
        {
            if (Selected == PG02Ability.FuocoRapido && isActiveAndEnabled && EffectActive && session.Player.Actor.IsActive)
                current.AttackSpeed *= 1f + data.RapidAttackSpeedPercent / 100f;
            return current;
        }

        private void Update()
        {
            if (!CanUse) return;
            if (Selected == PG02Ability.FuocoDiSoppressione)
            {
                cooldown.Tick(Time.deltaTime);
                if (!EffectActive) return;
                burstElapsed += Time.deltaTime;
                // Include both endpoints: first shot at activation, last at 3 seconds.
                float interval = data.SuppressionDuration / (data.SuppressionProjectileCount - 1);
                while (ShotsFired < data.SuppressionProjectileCount && burstElapsed >= ShotsFired * interval)
                    FireSuppression();
                ActiveRemaining = Mathf.Max(0, data.SuppressionDuration - burstElapsed);
                return;
            }
            if (!EffectActive) { cooldown.Tick(Time.deltaTime); return; }
            ActiveRemaining = Mathf.Max(0, ActiveRemaining - Time.deltaTime);
            if (!EffectActive) cooldown.Restart(data.RapidBaseCooldown, session.CdReduction);
        }

        public bool TryActivate(Vector2 cursor)
        {
            if (!CanUse || !cooldown.Ready || EffectActive) return false;
            if (Selected == PG02Ability.FuocoRapido) ActiveRemaining = data.RapidDuration;
            else
            {
                cooldown.Restart(data.SuppressionBaseCooldown, session.CdReduction);
                aimPoint = cursor;
                ShotsFired = 0;
                burstElapsed = 0;
                ActiveRemaining = data.SuppressionDuration;
                FireSuppression();
            }
            return true;
        }

        public void SetAimPoint(Vector2 cursor) => aimPoint = cursor;

        private void FireSuppression()
        {
            Vector2 origin = weapon.Muzzle.position;
            Vector2 direction = (aimPoint - origin).normalized;
            if (direction == Vector2.zero) direction = transform.right;
            var projectile = SuppressionEffect.Fire(session.Player.Actor, origin, direction, data, weapon.Definition, ShotsFired);
            SuppressionShot?.Invoke(projectile, ShotsFired);
            ShotsFired++;
        }

        public void ChangeArea()
        {
            cooldown.ChangeArea(EffectActive, data.BaseCooldown(Selected), session.CdReduction);
            ActiveRemaining = 0;
        }

        private void OnDestroy()
        {
            if (session != null && session.Player != null && ReferenceEquals(session.Player.Actor.AbilityModifier, this))
                session.Player.Actor.AbilityModifier = null;
            if (data != null) Destroy(data);
        }
    }
}
