using System;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG05AbilityRuntime : MonoBehaviour
    {
        private LoopSession session;
        private PG05AbilityCatalog data;
        private Combatant actor;
        private PlayerWeapon weapon;
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        private float ghostRemaining;
        private readonly List<PG05Invisibility> partyEffects = new List<PG05Invisibility>();
        public bool EffectActive
        {
            get { foreach (var effect in partyEffects) if (effect != null && effect.Active) return true; return false; }
        }
        public PG05Ability Selected { get; private set; }
        public PG05Passive Passive { get; private set; }
        public int CicutaHits { get; private set; }
        public float CooldownRemaining => cooldown.Remaining;
        public bool CanUse => session != null && session.GameplayRunning && actor.IsActive;
        public string Label => Selected == PG05Ability.Invisibilita ? "INVISIBILITA" : "COLTELLI AVVELENATI";
        public string PassiveLabel => Passive == PG05Passive.Ghosting ? "GHOSTING" : "LAMA DI CICUTA";
        public event Action<Projectile> KnifeFired;
        public void Initialize(LoopSession owner, PG05Ability choice, PG05Passive passive, PG05AbilityCatalog catalog)
        {
            session = owner; Selected = choice; Passive = passive; data = Instantiate(catalog);
            actor = GetComponent<Combatant>(); weapon = GetComponent<PlayerWeapon>();
            weapon.BaseAttackOverride = FireBase;
            Combatant.DamageApplied += AfterDamage; actor.PerformedAction += CancelGhost;
            cooldown.Restart(data.BaseCooldown(Selected), session.CdReduction);
        }
        private void CancelGhost() => ghostRemaining = 0;
        private void AfterDamage(Combatant target, float damage)
        {
            // Evaluate this PG after damage, including the HIT that crosses the threshold.
            if (target != actor || !isActiveAndEnabled || !actor.IsActive || actor.CurrentHP <= 0) return;
            if (Passive != PG05Passive.Ghosting || actor.CurrentHP * 100d >= actor.Stats.HP * 35d || ghostRemaining > 0) return;
            ghostRemaining = data.GhostDuration;
            (GetComponent<PG05Invisibility>() ?? gameObject.AddComponent<PG05Invisibility>())
                .Apply(session, data.GhostDuration, data.MoveBonusPercent);
        }
        private void Update()
        {
            if (!CanUse) return;
            if (!EffectActive) partyEffects.Clear();
            cooldown.Tick(Time.deltaTime); ghostRemaining = Mathf.Max(0, ghostRemaining - Time.deltaTime);
        }
        public bool TryActivate(Vector2 cursor)
        {
            if (!CanUse || !cooldown.Ready) return false;
            actor.NotifyAction();
            cooldown.Restart(data.BaseCooldown(Selected), session.CdReduction);
            if (Selected == PG05Ability.Invisibilita)
            {
                foreach (var pg in Combatant.All.ToArray())
                    if (pg != null && pg.Faction == Faction.PG && pg.IsActive)
                    {
                        var effect = pg.GetComponent<PG05Invisibility>() ?? pg.gameObject.AddComponent<PG05Invisibility>();
                        effect.Apply(session, data.InvisibleDuration, data.MoveBonusPercent);
                        if (!partyEffects.Contains(effect)) partyEffects.Add(effect);
                    }
            }
            else
            {
                Vector2 origin = transform.position;
                Vector2 direction = (cursor - origin).normalized;
                if (direction == Vector2.zero) direction = transform.right;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                for (int i = 0; i < data.KnifeCount; i++)
                {
                    var shot = TestVisuals.SpawnProjectile(actor, origin, AttackGeometry.Direction(angle + i * 360f / data.KnifeCount),
                        weapon.Definition.ProjectileSpeed, data.KnifeRange, data.KnifeDamage, weapon.Definition.ProjectileRadius, 0, false);
                    shot.HitEffect = new PG05KnifeHit(session, data.PoisonDamage, data.PoisonDuration);
                    KnifeFired?.Invoke(shot);
                }
            }
            return true;
        }
        private void FireBase(Vector2 muzzle, Vector2 cursor, CombatStats stats)
        {
            Vector2 origin = transform.position, direction = (cursor - origin).normalized;
            if (direction == Vector2.zero) direction = transform.right;
            bool poison = Passive == PG05Passive.LamaDiCicuta && CicutaHits >= data.CicutaThreshold;
            int hits = 0;
            Physics2D.SyncTransforms();
            foreach (var target in Combatant.All.ToArray())
            {
                if (target == null || !target.IsActive || target.Faction != Faction.MOB) continue;
                Vector2 offset = (Vector2)target.transform.position - origin;
                if (offset.sqrMagnitude > stats.RangeMetres * stats.RangeMetres || Vector2.Angle(direction, offset) > 67.5f ||
                    !AttackGeometry.ClearLine(origin, target.transform.position)) continue;
                if (!target.Hit(stats.ATK, true, actor, true)) continue;
                hits++;
                if (poison && target.IsActive)
                    (target.GetComponent<PG05Poison>() ?? target.gameObject.AddComponent<PG05Poison>())
                        .Apply(session, actor, data.PoisonDamage, data.PoisonDuration);
            }
            if (Passive == PG05Passive.LamaDiCicuta)
                CicutaHits = poison ? 0 : Mathf.Min(data.CicutaThreshold, CicutaHits + hits);
            TestVisuals.FlashFront(origin, direction, stats.RangeMetres, weapon.Definition);
        }
        public void ChangeArea()
        {
            bool active = EffectActive;
            foreach (var effect in FindObjectsByType<PG05Invisibility>(FindObjectsSortMode.None))
            { effect.Cancel(); }
            cooldown.ChangeArea(Selected == PG05Ability.Invisibilita && active, data.BaseCooldown(Selected), session.CdReduction);
            partyEffects.Clear();
            ghostRemaining = 0; // CICUTA counter deliberately persists across AREA.
        }
        private void OnDestroy()
        {
            Combatant.DamageApplied -= AfterDamage;
            if (actor != null) actor.PerformedAction -= CancelGhost;
            if (data != null) Destroy(data);
        }
    }
}
