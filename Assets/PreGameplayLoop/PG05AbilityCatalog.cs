using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PG05Ability { Invisibilita, ColtelliAvvelenati }
    public enum PG05Passive { Ghosting, LamaDiCicuta }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG05 Abilities")]
    public sealed class PG05AbilityCatalog : ScriptableObject
    {
        public float InvisibleDuration = 3.5f, InvisibleCooldown = 15, MoveBonusPercent = 15;
        public int KnifeCount = 8;
        public float KnifeRange = 10, KnifeDamage = 30, KnifeCooldown = 10;
        public float PoisonDamage = 5, PoisonDuration = 3, GhostDuration = 2;
        public int CicutaThreshold = 15;
        public float BaseCooldown(PG05Ability choice) => choice == PG05Ability.Invisibilita ? InvisibleCooldown : KnifeCooldown;
        public bool IsValid => InvisibleDuration > 0 && InvisibleCooldown > 0 && MoveBonusPercent >= 0 &&
            KnifeCount > 0 && KnifeRange > 0 && KnifeDamage > 0 && KnifeCooldown > 0 && PoisonDamage > 0 &&
            PoisonDuration > 0 && GhostDuration > 0 && CicutaThreshold > 0;
    }
}
