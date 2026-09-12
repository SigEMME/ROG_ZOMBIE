using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum PG01Ability { Pestone, Barriera }

    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG01 Abilities")]
    public sealed class PG01AbilityCatalog : ScriptableObject
    {
        [Header("PESTONE — GDD 12; frontal rectangle 3 x 7 m")]
        [Min(0)] public float PestoneDamage = 40;
        [Min(0)] public float PestoneDepth = 7;
        [Min(0)] public float PestoneWidth = 3;
        [Min(0)] public float PestoneBaseCooldown = 10;
        [Range(0, 100)] public float PestoneSlowPercent = 30;
        [Min(0)] public float PestoneSlowDuration = 3;
        [Header("BARRIERA — GDD 12")]
        public Vector2 BarrierSize = new Vector2(7, 1);
        [Min(0)] public float BarrierDuration = 5;
        [Min(0)] public float BarrierBaseCooldown = 15;
        [Tooltip("Owner clarification: maximum PG-to-centre placement distance, metres.")]
        [Min(0)] public float BarrierPlacementRange = 8;

        public float BaseCooldown(PG01Ability selected)
            => selected == PG01Ability.Pestone ? PestoneBaseCooldown : BarrierBaseCooldown;

        public bool IsValid => PestoneDamage >= 0 && PestoneDepth > 0 && PestoneWidth > 0 &&
            PestoneSlowPercent >= 0 && PestoneSlowPercent <= 100 && PestoneSlowDuration > 0 &&
            PestoneBaseCooldown > 0 && BarrierSize.x > 0 && BarrierSize.y > 0 &&
            BarrierDuration > 0 && BarrierBaseCooldown > 0 && BarrierPlacementRange > 0;
    }
}
