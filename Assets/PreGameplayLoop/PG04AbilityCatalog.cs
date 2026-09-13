using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PG04Ability { PioggiaDiGranate, ColpoGrosso }
    public enum PG04Passive { ScortaEsplosiva, Pyromania }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG04 Abilities")]
    public sealed class PG04AbilityCatalog : ScriptableObject
    {
        [Min(1)] public int RainCount = 10;
        [Min(0)] public float RainRadius = 5;
        [Min(0)] public float RainDuration = 3;
        [Min(0)] public float RainBaseCooldown = 12;
        [Min(1)] public int BigShotCharges = 4;
        [Min(0)] public float BigShotAttackPercent = 80;
        [Min(0)] public float BigShotRadiusPercent = 50;
        [Min(0)] public float BigShotBaseCooldown = 10;
        [Min(0)] public float FireDuration = 3;
        [Min(0)] public float FireDamage = 5;
        public float BaseCooldown(PG04Ability ability) => ability == PG04Ability.PioggiaDiGranate ? RainBaseCooldown : BigShotBaseCooldown;
        public bool IsValid => RainCount > 0 && RainRadius > 0 && RainDuration > 0 && RainBaseCooldown > 0 &&
            BigShotCharges > 0 && BigShotAttackPercent >= 0 && BigShotRadiusPercent >= 0 && BigShotBaseCooldown > 0 && FireDuration > 0 && FireDamage >= 0;
    }
}
