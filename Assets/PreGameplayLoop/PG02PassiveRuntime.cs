using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    [DisallowMultipleComponent]
    public sealed class PG02PassiveRuntime : MonoBehaviour, ICombatStatModifier
    {
        private Combatant actor;
        private LoopSession session;
        public PG02Passive Selected { get; private set; }
        public int Kills { get; private set; }
        public int KillsTowardRage => Kills % 15;
        public float RageRemaining { get; private set; }
        public bool RageActive => RageRemaining > 0 && actor != null && actor.IsActive;
        private bool BelowHealThreshold => actor != null && actor.Stats.HP > 0 &&
            actor.CurrentHP * 10d < actor.Stats.HP * 3d;
        public bool EffectActive => Selected == PG02Passive.Passiva1
            ? actor != null && actor.IsActive && BelowHealThreshold : RageActive;
        public string Label => Selected == PG02Passive.Passiva1 ? "PASSIVA 1 — CURA SU KILL" : "PASSIVA 2 — RAGE";

        public void Initialize(LoopSession owner, PG02Passive selected)
        {
            session = owner;
            actor = owner.Player.Actor;
            Selected = selected;
            actor.PassiveModifier = this;
            actor.Killed += OnKill;
        }

        private void OnKill(Combatant target)
        {
            Kills++;
            if (!actor.IsActive) return;
            if (Selected == PG02Passive.Passiva1)
            {
                // Reevaluate per KILL: one multi-target HIT can cross the healing threshold.
                if (BelowHealThreshold) actor.Heal(.01f);
            }
            else if (KillsTowardRage == 0) RageRemaining = 2f;
        }

        public CombatStats Apply(CombatStats current)
        {
            current.DEF = Mathf.Min(190, current.DEF);
            if (isActiveAndEnabled && Selected == PG02Passive.Passiva2 && RageActive) current.ATK *= 1.3f;
            return current;
        }

        private void Update()
        {
            if (session.GameplayRunning && actor.IsActive) RageRemaining = Mathf.Max(0f, RageRemaining - Time.deltaTime);
        }

        public void ChangeArea() => RageRemaining = 0f; // Owner: KILL progress persists; RAGE ends.

        private void OnDestroy()
        {
            if (actor == null) return;
            actor.Killed -= OnKill;
            if (ReferenceEquals(actor.PassiveModifier, this)) actor.PassiveModifier = null;
        }
    }
}
