using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG03PassiveRuntime : MonoBehaviour, IProjectileHitEffect
    {
        public readonly HudPulse MarkPulse = new HudPulse();
        private readonly System.Collections.Generic.List<WeakPointMark> marks = new System.Collections.Generic.List<WeakPointMark>();
        public bool MarkActive { get { foreach (var mark in marks) if (mark != null && mark.Active && mark.GetComponent<Combatant>().IsActive) return true; return false; } }
        public PG03Passive Selected { get; private set; }
        public string Label => Selected == PG03Passive.CalibroPerforante ? "CALIBRO PERFORANTE" : "PUNTO DEBOLE";
        public void Initialize(PG03Passive selected)
        {
            Selected = selected;
            var weapon = GetComponent<PlayerWeapon>();
            if (selected == PG03Passive.CalibroPerforante) weapon.BasePenetrationsOverride = 2;
            weapon.BaseProjectileEffect = this;
        }
        public void ResolveHit(Combatant target, float damage, bool roundDamage, Combatant source, int hitIndex)
        {
            if (Selected == PG03Passive.CalibroPerforante)
            {
                // Index belongs to this projectile. Scale its original damage, before target DEF.
                float multiplier = hitIndex == 0 ? 1f : hitIndex == 1 ? .7f : .4f;
                target.Hit(damage * multiplier, roundDamage, source, true);
                return;
            }
            var mark = target.GetComponent<WeakPointMark>();
            bool alreadyMarked = mark != null && mark.Active;
            bool hit = target.Hit(damage, roundDamage, source, true);
            if (hit && target.Faction == Faction.MOB) MarkPulse.Trigger();
            if (!hit || alreadyMarked || !target.IsActive || target.Faction != Faction.MOB) return;
            if (mark == null) mark = target.gameObject.AddComponent<WeakPointMark>();
            mark.ApplyTo(target);
            marks.RemoveAll(value => value == null || !value.Active);
            if (!marks.Contains(mark)) marks.Add(mark);
        }
    }
}
