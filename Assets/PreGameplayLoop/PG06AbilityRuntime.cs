using System;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG06AbilityRuntime : MonoBehaviour
    {
        private LoopSession session;
        private PG06AbilityCatalog data;
        private PlayerWeapon weapon;
        private Combatant actor;
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        public PG06Ability Selected { get; private set; }
        public PG06Passive Passive { get; private set; }
        public int Charges { get; private set; }
        public int KillRolls { get; private set; }
        public int Drops { get; private set; }
        public bool EffectActive => Charges > 0;
        public float CooldownRemaining => cooldown.Remaining;
        public bool CanUse => session != null && session.GameplayRunning && actor.IsActive;
        public string Label => Selected == PG06Ability.CuraAdArea ? "CURA AD AREA" : "FUOCO CURATIVO";
        public string PassiveLabel => Passive == PG06Passive.Elemosina ? "ELEMOSINA" : "VITAMINA C";
        public event Action<Combatant> Healed;
        public event Action<Projectile, bool> ShotFired;
        public void Initialize(LoopSession owner, PG06Ability choice, PG06Passive passive, PG06AbilityCatalog catalog)
        {
            session = owner; Selected = choice; Passive = passive; data = Instantiate(catalog);
            actor = GetComponent<Combatant>(); weapon = GetComponent<PlayerWeapon>(); weapon.BaseAttackOverride = FireBase;
            actor.Killed += OnKill;
            cooldown.Restart(data.BaseCooldown(Selected), session.CdReduction);
        }
        private void Update()
        {
            if (CanUse && !EffectActive) cooldown.Tick(Time.deltaTime);
        }
        public bool TryActivate(Vector2 cursor)
        {
            if (!CanUse || !cooldown.Ready || EffectActive) return false;
            actor.NotifyAction();
            if (Selected == PG06Ability.FuocoCurativo) { Charges = data.FireCharges; return true; }
            cooldown.Restart(data.AreaCooldown, session.CdReduction);
            foreach (var pg in Combatant.All.ToArray())
                if (PG06Healing.Valid(pg) && Vector2.Distance(transform.position, pg.transform.position) <= data.AreaRadius) Heal(pg, data.AreaHeal);
            TestVisuals.FlashCircle(transform.position, data.AreaRadius, Color.green);
            return true;
        }
        private void Heal(Combatant pg, float amount)
        {
            PG06Healing.Heal(session, pg, amount, Passive == PG06Passive.VitaminaC, data.VitaminPercent, data.VitaminDuration);
            Healed?.Invoke(pg);
        }
        private void FireBase(Vector2 origin, Vector2 cursor, CombatStats stats)
        {
            bool healing = Charges > 0;
            if (healing && --Charges == 0) cooldown.Restart(data.FireCooldown, session.CdReduction);
            var shot = TestVisuals.SpawnProjectile(actor, origin, (cursor - origin).normalized, weapon.Definition.ProjectileSpeed,
                stats.RangeMetres, stats.ATK, weapon.Definition.ProjectileRadius, 0, false);
            if (healing) shot.HitEffect = new HealingHit(this);
            ShotFired?.Invoke(shot, healing);
        }
        private sealed class HealingHit : IProjectileHitEffect
        {
            private readonly PG06AbilityRuntime owner;
            public HealingHit(PG06AbilityRuntime value) { owner = value; }
            public void ResolveHit(Combatant target, float damage, bool round, Combatant source, int index)
            {
                if (target.Hit(damage, true, source) && owner != null) owner.HealAfterHit();
            }
        }
        private void HealAfterHit()
        {
            var candidates = new List<Combatant>();
            foreach (var pg in Combatant.All)
                if (PG06Healing.Valid(pg) && Vector2.Distance(transform.position, pg.transform.position) <= data.FireRadius) candidates.Add(pg);
            var target = PG06Healing.Select(candidates, transform.position, true);
            if (target != null) Heal(target, data.FireHeal);
        }
        private void OnKill(Combatant victim)
        {
            if (Passive != PG06Passive.Elemosina) return;
            KillRolls++;
            if (UnityEngine.Random.value >= data.DropChance) return;
            Drops++;
            var go = TestVisuals.Box("ELEMOSINA MEDI KIT", victim.transform.position, Vector2.one * .5f, Color.green, 2);
            go.AddComponent<PG06Medikit>().Initialize(session, data.MedikitFraction, data.TestMedikitRadius);
        }
        public void ChangeArea()
        {
            cooldown.ChangeArea(EffectActive, data.BaseCooldown(Selected), session.CdReduction); Charges = 0;
            foreach (var vitamin in FindObjectsByType<PG06Vitamin>(FindObjectsSortMode.None)) vitamin.Clear();
        }
        private void OnDestroy()
        {
            if (actor != null) actor.Killed -= OnKill;
            if (data != null) Destroy(data);
        }
    }
}
