using UnityEngine;

namespace RogZombie.TestEngine
{
    public enum BaseAttackKind { PhysicalProjectile, HitscanArea, AreaHitscan }
    public enum AttackShape { Cone, Semicircle }

    [CreateAssetMenu(menuName = "ROG ZOMBIE/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public PlayerDefinition PG;
        public BaseAttackKind Kind;
        public AttackShape Shape;
        public float ProjectileSpeed = 20f;
        [Min(0), Tooltip("0: stops at first MOB; 2: hits three MOBs then stops.")]
        public int Penetrations;
        [Tooltip("TEST ENGINE #2 specification: cone width at its far end.")]
        public float ConeWidth = 3f;
        public float ExplosionRadius = 1.5f;
        [Min(0f), InspectorName("GRENADE HIT DELAY (s)")]
        public float GrenadeHitDelay = 0.5f;
        [Header("Temporary presentation / collider tuning")]
        public float ProjectileRadius = 0.06f;
    }
}
