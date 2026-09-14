using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PG08Ability { FiloSpinato, ColpiRespingenti }
    public enum PG08Passive { Tenacia, Rage }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG08 Abilities")]
    public sealed class PG08AbilityCatalog : ScriptableObject
    {
        public float WireOuterRadius = 4.5f, WireThickness = 1, WireDuration = 5, WireDamage = 10, WireSlow = 40, WireCooldown = 16;
        public int WireSections = 10;
        public float PushDistance = 1.5f, PushDuration = .25f, PushActiveDuration = 4, PushCooldown = 10;
        public float TenacityPerHit = .2f, TenacityCap = 30, RageThreshold = 5, RageAttackPercent = 30, RageMovePercent = 20;
        public float BaseCooldown(PG08Ability choice) => choice == PG08Ability.FiloSpinato ? WireCooldown : PushCooldown;
        public bool IsValid => WireOuterRadius > WireThickness && WireThickness > 0 && WireSections == 10 &&
            WireDuration > 0 && WireDamage > 0 && WireSlow >= 0 && WireSlow <= 100 && WireCooldown > 0 &&
            PushDistance > 0 && PushDuration > 0 && PushActiveDuration > 0 && PushCooldown > 0 &&
            TenacityPerHit > 0 && TenacityCap > 0 && RageThreshold > 0 && RageAttackPercent >= 0 && RageMovePercent >= 0 && RageMovePercent <= 100;
    }
}
