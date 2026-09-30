using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.BossTest
{
    [CreateAssetMenu(menuName = "ROG ZOMBIE/BOSS 01")]
    public sealed class BossDefinition : ScriptableObject
    {
        public CombatStats Stats = new CombatStats { HP = 15000, ATK = 80, DEF = 100, MoveSpeed = 90, AttackSpeed = 50, Range = 300 };
        public float Diameter = 5;
        public Vector2 ArenaSize = new Vector2(60, 40);
        public float CameraWidth = 55, CameraBorder = 3;
        public float SpecialInterval = 14;
        public int[] SpecialOrder = { 1, 2, 1, 4 };
        public float[] ChargeThresholds = { .8f, .6f, .4f, .2f };
        [Header("ATTACCO BASE")]
        public float BaseAngle = 110, BaseDepth = 4, BaseHitAt = 1.2f, BaseDuration = 1.5f, TrackSeconds = 1;
        [Header("ATTACCO_01")]
        public float WaveChannel = 2, WaveDamage = 70, WaveWidth = 2.5f, WaveLength = 12, WaveSideAngle = 30, WaveRecovery = 1;
        [Header("ATTACCO_02")]
        public int RockCount = 3;
        public float RockDelay = 3, RockRadius = 3, RockDamage = 40, RockStill = 2, RockStun = 1;
        [Header("ATTACCO_03")]
        public float ChargeRange = 18, ChargeLength = 18, ChargeSpeed = 8, ChargeChannel = 1.5f, ChargeDamage = 40;
        public float PushDistance = 3, PushSeconds = .25f, ChargeRecovery = .5f, WallStun = 1.5f;
        [Header("ATTACCO_04")]
        public float BileAngle = 150;
        public int BileSlices = 10;
        public float BileChannel = 1, BileInterval = .33f, BileSpeed = 10, BileRange = 12, BileDamage = 30, BileRecovery = .5f;
        public float PoisonDamage = 5, PoisonSeconds = 3; public float BileProjectileRadius = .5f;
    }
}
