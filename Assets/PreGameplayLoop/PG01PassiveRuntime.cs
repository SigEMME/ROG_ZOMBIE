using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum PG01Passive
    {
        [InspectorName("PASSIVA 1 — DEF +15%")] Passiva1,
        [InspectorName("PASSIVA 2 — ATK +15%")] Passiva2
    }

    [DisallowMultipleComponent]
    public sealed class PG01PassiveRuntime : MonoBehaviour, ICombatStatModifier
    {
        private Combatant actor;
        public PG01Passive Selected { get; private set; }
        public bool EffectActive => isActiveAndEnabled && actor != null && actor.IsActive &&
            actor.Stats.HP > 0f && actor.CurrentHP < actor.Stats.HP * .5f;
        public string Label => Selected == PG01Passive.Passiva1 ? "PASSIVA 1 — DEF +15%" : "PASSIVA 2 — ATK +15%";

        public void Initialize(Combatant owner, PG01Passive selected)
        {
            actor = owner;
            Selected = selected; // RUN snapshot, independent of ability and Inspector edits.
            actor.PassiveModifier = this;
        }

        public CombatStats Apply(CombatStats persistentStats)
        {
            // Evaluate against current HP/max HP on every use, including consecutive HITs.
            // Never write the temporary contribution back into persistent bonus values.
            persistentStats.DEF = Mathf.Min(190f, persistentStats.DEF);
            if (!EffectActive) return persistentStats;
            if (Selected == PG01Passive.Passiva1)
                persistentStats.DEF = Mathf.Min(190f, persistentStats.DEF + 15f);
            else
                persistentStats.ATK *= 1.15f;
            return persistentStats;
        }

        private void OnEnable() { if (actor != null) actor.PassiveModifier = this; }
        private void OnDisable()
        {
            if (actor != null && ReferenceEquals(actor.PassiveModifier, this)) actor.PassiveModifier = null;
        }
    }
}
