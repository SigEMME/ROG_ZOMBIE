using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum PG02Ability { FuocoRapido, FuocoDiSoppressione }
    public enum PG02Passive { Passiva1, Passiva2 }

    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG02 Abilities")]
    public sealed class PG02AbilityCatalog : ScriptableObject
    {
        [Min(0)] public float RapidAttackSpeedPercent = 40;
        [Min(0)] public float RapidDuration = 4;
        [Min(0)] public float RapidBaseCooldown = 10;
        [Min(0)] public float SuppressionDamage = 3;
        [Min(2)] public int SuppressionProjectileCount = 50;
        [Min(0)] public float SuppressionDuration = 3;
        [Min(0)] public float SuppressionRange = 10;
        [Range(0, 360)] public float SuppressionAngle = 50;
        [Min(0)] public float SuppressionBaseCooldown = 12;

        public float BaseCooldown(PG02Ability selected) => selected == PG02Ability.FuocoRapido ? RapidBaseCooldown : SuppressionBaseCooldown;
        public bool IsValid => RapidAttackSpeedPercent >= 0 && RapidDuration > 0 && RapidBaseCooldown > 0 &&
            SuppressionProjectileCount >= 2 && SuppressionDuration > 0 && SuppressionDamage >= 0 && SuppressionRange > 0 && SuppressionAngle > 0 && SuppressionAngle <= 360 && SuppressionBaseCooldown > 0;
    }
}
