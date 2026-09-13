using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG08PassiveRuntime : MonoBehaviour, ICombatStatModifier
    {
        private Combatant actor;
        private LoopSession session;
        private PG08AbilityCatalog data;
        private bool held, started, stunned;
        private float attackBonus, movePenalty;
        public PG08Passive Selected { get; private set; }
        public int Hits { get; private set; }
        public float TenacityBonus => Mathf.Min(data.TenacityCap, Hits * data.TenacityPerHit);
        public float ContinuousFire { get; private set; }
        public bool RageActive { get; private set; }
        public bool IsStunned => stunned;
        public bool EffectActive => Selected == PG08Passive.Tenacia ? Hits > 0 : RageActive;
        public string Label => Selected == PG08Passive.Tenacia ? "TENACIA" : "RAGE";
        public void Initialize(LoopSession owner, PG08Passive choice, PG08AbilityCatalog catalog)
        {
            session = owner; actor = GetComponent<Combatant>(); data = Instantiate(catalog); Selected = choice;
            actor.PassiveModifier = this; actor.BeforeHit += ReceivedHit; actor.StateChanged += StateChanged;
        }
        // Combatant invokes BeforeHit after calculating damage with EffectiveStats.
        // Owner-confirmed: the resetting HIT still benefits from TENACIA; following HITs do not.
        private void ReceivedHit() => Hits = 0;
        private void StateChanged(Combatant value) { if (!value.IsActive) StopFire(); }
        public void BaseHit()
        {
            if (actor.IsActive && Selected == PG08Passive.Tenacia)
                Hits = Mathf.Min(Hits + 1, Mathf.CeilToInt(data.TenacityCap / data.TenacityPerHit));
        }
        public void SetFireHeld(bool value)
        {
            held = value;
            if (!held) StopFire();
        }
        public void BaseShot() { if (held && !stunned && actor.IsActive) started = true; }
        public void StopFire()
        { started = false; ContinuousFire = 0; RageActive = false; attackBonus = movePenalty = 0; }
        // Hook for a future PG STUN source; the prototype currently has no such enemy/item.
        public void SetStunned(bool value) { stunned = value; if (value) StopFire(); }
        private void Update() { if (session.GameplayRunning) Advance(Time.deltaTime); }
        public void Advance(float seconds)
        {
            if (Selected != PG08Passive.Rage || !held || !started || stunned || !actor.IsActive || seconds <= 0) return;
            ContinuousFire = Mathf.Min(data.RageThreshold, ContinuousFire + seconds);
            if (RageActive || ContinuousFire < data.RageThreshold) return;
            var current = actor.EffectiveStats;
            attackBonus = current.ATK * data.RageAttackPercent / 100;
            movePenalty = current.MoveSpeed * data.RageMovePercent / 100;
            RageActive = true;
        }
        public CombatStats Apply(CombatStats current)
        {
            if (isActiveAndEnabled && actor.IsActive)
            {
                if (Selected == PG08Passive.Tenacia) current.DEF += TenacityBonus;
                else if (RageActive) { current.ATK += attackBonus; current.MoveSpeed -= movePenalty; }
            }
            current.DEF = Mathf.Min(190, current.DEF);
            return current;
        }
        public void ChangeArea() { Hits = 0; StopFire(); }
        private void OnDisable() => StopFire();
        private void OnDestroy()
        {
            if (actor != null)
            {
                actor.BeforeHit -= ReceivedHit; actor.StateChanged -= StateChanged;
                if (ReferenceEquals(actor.PassiveModifier, this)) actor.PassiveModifier = null;
            }
            if (data != null) Destroy(data);
        }
    }
}
