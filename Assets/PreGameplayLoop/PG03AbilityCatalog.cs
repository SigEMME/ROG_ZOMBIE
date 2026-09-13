using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PG03Ability { ColpoLaser, TriploSparo }
    public enum PG03Passive { CalibroPerforante, PuntoDebole }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/PG03 Abilities")]
    public sealed class PG03AbilityCatalog : ScriptableObject
    {
        [Min(0)] public float LaserDamage = 35;
        [Min(0)] public float LaserRange = 20;
        [Min(0)] public float LaserWidth = 2;
        [Min(0)] public float LaserBaseCooldown = 18;
        [Min(0)] public float TripleDamage = 40;
        [Min(0)] public float TripleRange = 20;
        [Range(0, 360)] public float TripleAngle = 30;
        [Min(0)] public float TripleInterval = .5f;
        [Min(0)] public float TripleBaseCooldown = 10;
        public float BaseCooldown(PG03Ability selected) => selected == PG03Ability.ColpoLaser ? LaserBaseCooldown : TripleBaseCooldown;
        public bool IsValid => LaserDamage >= 0 && LaserRange > 0 && LaserWidth > 0 && LaserBaseCooldown > 0 &&
            TripleDamage >= 0 && TripleRange > 0 && TripleAngle > 0 && TripleAngle <= 360 && TripleInterval > 0 && TripleBaseCooldown > 0;
    }
}
