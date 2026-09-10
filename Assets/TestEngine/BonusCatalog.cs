using System;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [Serializable]
    public sealed class BonusParameter
    {
        public string Key;
        public float BaseValue;
    }

    [Serializable]
    public sealed class BonusUpgrade
    {
        public string Key;
        public string Label;
        public float Weight;
        public float BasePercent;
        public float FlatAmount;
    }

    [Serializable]
    public sealed class BonusDefinition
    {
        public string Id;
        public string Name;
        public float Weight;
        public BonusParameter[] Parameters;
        public BonusUpgrade[] Upgrades;
    }

    [CreateAssetMenu(menuName = "ROG ZOMBIE/Bonus Catalog")]
    public sealed class BonusCatalog : ScriptableObject
    {
        public BonusDefinition[] Bonuses;
        public int Slots = 3;
        public float NewAtZeroSlots = 100f;
        public float NewAtOneOrTwoSlots = 50f;
        public float NewAtFullSlots = 0f;
    }
}
