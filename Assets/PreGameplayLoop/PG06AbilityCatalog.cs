using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PG06Ability { CuraAdArea, FuocoCurativo }
    public enum PG06Passive { Elemosina, VitaminaC }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG06 Abilities")]
    public sealed class PG06AbilityCatalog : ScriptableObject
    {
        public float AreaHeal = 40, AreaRadius = 5, AreaCooldown = 20;
        public int FireCharges = 6;
        public float FireHeal = 15, FireRadius = 15, FireCooldown = 15;
        public float DropChance = .03f, MedikitFraction = .1f;
        public float VitaminPercent = 35, VitaminDuration = 4;
        [Tooltip("Temporary trigger geometry, GDD section26 leaves this to development/testing.")]
        public float TestMedikitRadius = .5f;
        public float BaseCooldown(PG06Ability choice) => choice == PG06Ability.CuraAdArea ? AreaCooldown : FireCooldown;
        public bool IsValid => AreaHeal > 0 && AreaRadius > 0 && AreaCooldown > 0 && FireCharges > 0 && FireHeal > 0 &&
            FireRadius > 0 && FireCooldown > 0 && DropChance >= 0 && DropChance <= 1 && MedikitFraction > 0 &&
            VitaminPercent >= 0 && VitaminDuration > 0 && TestMedikitRadius > 0;
    }
}
