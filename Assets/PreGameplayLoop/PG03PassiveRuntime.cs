using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG03PassiveRuntime : MonoBehaviour, IProjectileHitEffect
    {
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
                target.Hit(damage * multiplier, roundDamage, source);
                return;
            }
            var mark = target.GetComponent<WeakPointMark>();
            bool alreadyMarked = mark != null && mark.Active;
            if (!target.Hit(damage, roundDamage, source) || alreadyMarked || !target.IsActive || target.Faction != Faction.MOB) return;
            if (mark == null) mark = target.gameObject.AddComponent<WeakPointMark>();
            mark.ApplyTo(target);
        }
    }
}
