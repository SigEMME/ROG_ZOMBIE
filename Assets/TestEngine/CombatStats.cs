using System;
using UnityEngine;

namespace RogZombie.TestEngine
{
    // MOBs deliberately have no CD REDUCTION. PG-only data stays on PlayerRuntime.
    [Serializable]
    public struct CombatStats
    {
        public float HP;
        public float ATK;
        [Tooltip("100 = 0%; 110 = +10%; 90 = -10%.")] public float DEF;
        [InspectorName("MOVE SPD")] public float MoveSpeed;
        [InspectorName("ATK SPD")] public float AttackSpeed;
        [Tooltip("Internal units: 100 = 1 m. Never display in player UI.")] public float Range;

        public float MetresPerSecond => MoveSpeed / 100f * 2f;
        public float RangeMetres => Range / 100f;
        public float AttackInterval => AttackSpeed > 0f ? 100f / AttackSpeed : float.PositiveInfinity;

        public static CombatStats FromPG(PlayerStats value) => new CombatStats
        {
            HP = value.HP, ATK = value.ATK, DEF = 100f + value.DefPercent,
            MoveSpeed = value.MoveSpeed, AttackSpeed = value.AttackSpeed, Range = value.Range
        };
    }

    public static class DamageMath
    {
        public static float Calculate(float attack, float internalDef)
            => attack * (1f - (internalDef - 100f) / 100f);
    }

    public enum Faction { PG, MOB }
    public enum LifeState { Active, Down, PreExplosion, Dead }
}
