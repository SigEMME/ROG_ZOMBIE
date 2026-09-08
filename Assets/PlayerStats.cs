using System;
using UnityEngine;

namespace RogZombie
{
    [Serializable]
    public struct PlayerStats
    {
        [SerializeField, InspectorName("HP")]
        private float hp;

        [SerializeField, InspectorName("ATK")]
        private float atk;

        [SerializeField, InspectorName("DEF (%)")]
        [Tooltip("Signed percentage from the GDD roster: 15 = +15%, -10 = -10%. Not internal DEF.")]
        private float defPercent;

        [SerializeField, InspectorName("MOVE SPD")]
        [Tooltip("100 = 2 m/s. One Unity unit = one metre.")]
        private float moveSpeed;

        [SerializeField, InspectorName("ATK SPD")]
        [Tooltip("100 = 1 attack per second.")]
        private float attackSpeed;

        [SerializeField, InspectorName("CD REDUCTION")]
        [Tooltip("100 = neutral. Future final CD = ability base CD * (CD REDUCTION / 100). No seconds stored here.")]
        private float cdReduction;

        [SerializeField, InspectorName("RANGE (internal units)")]
        [Tooltip("100 = 1 metre. Example: 300 = 3 metres.")]
        private float range;

        public float HP => hp;
        public float ATK => atk;
        public float DefPercent => defPercent;
        public float MoveSpeed => moveSpeed;
        public float AttackSpeed => attackSpeed;
        public float CdReduction => cdReduction;
        public float Range => range;

        // Unit conversions only; these do not implement combat or runtime stats.
        public float MoveSpeedMetresPerSecond => moveSpeed / 100f * 2f;
        public float AttacksPerSecond => attackSpeed / 100f;
        public float RangeMetres => range / 100f;
    }
}
