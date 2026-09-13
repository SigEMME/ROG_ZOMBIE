using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PG07Ability { MultiShot, PioggiaDiFrecce }
    public enum PG07Passive { LuckyShot, Concentrazione }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG07 Abilities")]
    public sealed class PG07AbilityCatalog : ScriptableObject
    {
        public int MultiCount = 9, RainCount = 20;
        public float MultiDamage = 20, MultiRange = 7, MultiAngle = 80, MultiPush = 5, MultiSpeed = 20, MultiCooldown = 13;
        public float MultiPushDuration = .25f, RainHitRadius = .25f;
        public float RainRadius = 3.5f, RainDuration = 3, RainDamage = 10, RainCooldown = 15;
        public float LuckyChance = .05f, LuckyPercent = 30, LuckyRadius = 2, ConcentrationChance = .1f;
        public float BaseCooldown(PG07Ability choice) => choice == PG07Ability.MultiShot ? MultiCooldown : RainCooldown;
        public bool IsValid => MultiCount > 1 && RainCount > 1 && MultiDamage > 0 && MultiRange > 0 && MultiAngle > 0 && MultiPush >= 0 &&
            MultiPushDuration > 0 && RainHitRadius > 0 && MultiSpeed > 0 && MultiCooldown > 0 && RainRadius > 0 && RainDuration > 0 && RainDamage > 0 && RainCooldown > 0 &&
            LuckyChance >= 0 && LuckyChance <= 1 && ConcentrationChance >= 0 && ConcentrationChance <= 1 && LuckyPercent >= 0 && LuckyRadius > 0;
    }
}
