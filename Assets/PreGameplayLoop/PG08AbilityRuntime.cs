using System;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG08AbilityRuntime : MonoBehaviour
    {
        private LoopSession session;
        private Combatant actor;
        private PlayerWeapon weapon;
        private PG08PassiveRuntime passive;
        private PG08AbilityCatalog data;
        private readonly List<PG08WireArea> wires = new List<PG08WireArea>();
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        public PG08Ability Selected { get; private set; }
        public float ActiveRemaining { get; private set; }
        public bool EffectActive => DurationRemaining > 0;
        public float DurationRemaining
        {
            get
            {
                if (Selected == PG08Ability.ColpiRespingenti) return ActiveRemaining;
                float remaining = 0;
                foreach (var ring in wires) if (ring != null) remaining = Mathf.Max(remaining, ring.Remaining);
                return remaining;
            }
        }
        public float CooldownRemaining => cooldown.Remaining;
        public bool CanUse => session != null && session.GameplayRunning && actor.IsActive && !passive.IsStunned;
        public string Label => Selected == PG08Ability.FiloSpinato ? "FILO SPINATO" : "COLPI RESPINGENTI";
        public event Action<Projectile> BaseShot;
        public void Initialize(LoopSession owner, PG08Ability choice, PG08AbilityCatalog catalog)
        {
            session = owner; Selected = choice; data = Instantiate(catalog); actor = GetComponent<Combatant>();
            passive = GetComponent<PG08PassiveRuntime>(); weapon = GetComponent<PlayerWeapon>();
            weapon.BaseAttackOverride = FireBase; weapon.InputBlocked = () => !CanUse;
            cooldown.Restart(data.BaseCooldown(choice), session.CdReduction);
        }
        private void FireBase(Vector2 origin, Vector2 cursor, CombatStats stats)
        {
            passive.BaseShot();
            var shot = TestVisuals.SpawnProjectile(actor, origin, (cursor - origin).normalized, weapon.Definition.ProjectileSpeed,
                stats.RangeMetres, stats.ATK, weapon.Definition.ProjectileRadius, 0, false);
            shot.HitEffect = new Hit(this, shot, weapon.BaseProjectileEffect); BaseShot?.Invoke(shot);
        }
        private sealed class Hit : IProjectileHitEffect
        {
            private readonly PG08AbilityRuntime owner;
            private readonly Projectile shot;
            private readonly IProjectileHitEffect extra;
            public Hit(PG08AbilityRuntime value, Projectile projectile, IProjectileHitEffect additional)
            { owner = value; shot = projectile; extra = additional; }
            public void ResolveHit(Combatant target, float damage, bool round, Combatant source, int index)
            {
                bool valid = target.IsActive;
                if (extra != null) extra.ResolveHit(target, damage, true, source, index);
                else valid = target.Hit(damage, true, source);
                if (!valid || owner == null) return;
                owner.passive.BaseHit();
                if (owner.Selected == PG08Ability.ColpiRespingenti && owner.ActiveRemaining > 0 && target.IsActive)
                    owner.Push(target, shot.Direction);
            }
        }
        public PG08Push Push(Combatant target, Vector2 direction)
        {
            var go = new GameObject("COLPI RESPINGENTI motion"); go.transform.SetParent(TestVisuals.Root, false);
            var motion = go.AddComponent<PG08Push>();
            motion.Initialize(session, target, direction, data.PushDistance, data.PushDuration); return motion;
        }
        private void Update()
        {
            if (!session.GameplayRunning || !actor.IsActive) return;
            Advance(Time.deltaTime);
        }
        public void Advance(float seconds)
        {
            if (seconds <= 0) return;
            if (Selected == PG08Ability.ColpiRespingenti && ActiveRemaining > 0)
            {
                float consumed = Mathf.Min(seconds, ActiveRemaining);
                ActiveRemaining -= consumed;
                if (ActiveRemaining <= 0) { cooldown.Restart(data.PushCooldown, session.CdReduction); cooldown.Tick(seconds - consumed); }
            }
            else cooldown.Tick(seconds);
        }
        public bool TryActivate()
        {
            if (!CanUse || !cooldown.Ready || (Selected == PG08Ability.ColpiRespingenti && EffectActive)) return false;
            actor.NotifyAction();
            if (Selected == PG08Ability.ColpiRespingenti) ActiveRemaining = data.PushActiveDuration;
            else
            {
                cooldown.Restart(data.WireCooldown, session.CdReduction);
                var go = new GameObject("FILO SPINATO"); go.transform.SetParent(TestVisuals.Root, false); go.transform.position = transform.position;
                var wire = go.AddComponent<PG08WireArea>(); wire.Initialize(session, actor, data);
                wires.RemoveAll(ring => ring == null || ring.Remaining <= 0); wires.Add(wire);
            }
            return true;
        }
        public void ChangeArea()
        {
            bool active = EffectActive;
            // A reduced CD may allow multiple simultaneous rings from this PG.
            foreach (var ring in FindObjectsByType<PG08WireArea>(FindObjectsSortMode.None))
                if (ring.Source == actor) { active |= ring.Remaining > 0; ring.End(); }
            cooldown.ChangeArea(active, data.BaseCooldown(Selected), session.CdReduction);
            ActiveRemaining = 0; wires.Clear();
        }
        private void OnDestroy() { if (data != null) Destroy(data); }
    }
}
