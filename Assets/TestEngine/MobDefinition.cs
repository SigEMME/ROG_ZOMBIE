using UnityEngine;

namespace RogZombie.TestEngine
{
    public enum MobKind { ZOMB01, ZOMB02, ZOMB03, ZOMB04, ZOMB05 }

    [CreateAssetMenu(menuName = "ROG ZOMBIE/MOB Definition")]
    public sealed class MobDefinition : ScriptableObject
    {
        public MobKind Kind;
        public CombatStats BaseStats;
        public int ExpDrop;
        public int GoldDrop;
        public float AggroInterval = 0.5f;
        public float DeathSpriteSeconds = 3f;
        [Header("ZOMB03")]
        public float StopDistance = 1f;
        public float WindupSeconds = 0.5f;
        public float ImpactRadius = 1.5f;
        [Header("ZOMB04")]
        public float PreferredDistance = 15f;
        public float RetreatDistance = 9f;
        public float ProjectileSpeed = 15f;
        [Header("Temporary projectile collider tuning")]
        public float ProjectileRadius = 0.06f;
        [Header("ZOMB05")]
        public float PreExplosionSeconds = 2f;
        public float ExplosionRadius = 4f;
        public float ExplosionDamagePG = 50f;
        public float ExplosionDamageMOB = 25f;
    }
}
