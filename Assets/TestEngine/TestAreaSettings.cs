using System;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [Serializable]
    public sealed class ObstaclePlacement
    {
        public Vector2 Position;
        public Vector2 Size;
        public bool Wall = true;
    }

    [CreateAssetMenu(menuName = "ROG ZOMBIE/Test AREA Settings")]
    public sealed class TestAreaSettings : ScriptableObject
    {
        public WeaponDefinition[] Weapons;
        public MobDefinition[] Mobs;
        public BonusCatalog BonusCatalog;
        public Sprite PlayerSprite;
        [Header("PG tuning — live during test")]
        [Min(0f), InspectorName("GLOBAL MOVE SPD BASE (m/s)")]
        public float PlayerMoveSpeedBase = 3f;
        [Header("TEST / DEBUG ONLY")]
        public bool ForceTestChestNearPlayer = true;
        public Vector2 TestChestOffset = new Vector2(3f, 2f);
        public bool ShowDamageNumbers = true;
        public bool ShowSpawnCounters = true;
        [Min(0.1f)] public float DamageNumberSeconds = 0.8f;
        [Header("MOB soft separation")]
        public bool EnableMobSeparation = true;
        [Min(0f)] public float MobSpacing = 0.05f;
        [Min(0f)] public float MobSeparationSpeed = 2f;
        [Header("GDD C3_A5 population; single test AREA, no transition")]
        public int TotalMobs = 330;
        public float FirstSpawnPercent = 30f;
        public float[] MobPercentages = { 32f, 33f, 21f, 10f, 4f };
        public float OffscreenExtraRange = 5f;
        public int[] ExperienceThresholds = { 100, 120, 130, 150, 170, 200, 230, 270, 310, 350 };
        [Header("CHEST / MEDI KIT")]
        public float ChestRate = 5f;
        public float[] ChestStatWeights = { 25f, 8f, 22f, 22f, 8f, 15f };
        public float PickupMinStartDistance = 50f;
        public float MediKitHealPercent = 10f;
        [Header("Temporary AREA geometry / technical test tuning")]
        public Vector2 AreaSize = new Vector2(160f, 160f);
        public Vector2 StartPosition = Vector2.zero;
        public float ActorRadius = 0.35f;
        public float PickupTriggerRadius = 0.6f;
        public float CameraSize = 9f;
        public ObstaclePlacement[] Obstacles;
        [Min(1)] public int SearchAttemptsPerFrame = 16;
    }
}
