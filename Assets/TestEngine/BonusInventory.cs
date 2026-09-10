using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [Serializable]
    public sealed class OwnedBonus
    {
        public int DefinitionIndex;
        public int[] UpgradeCounts;
    }

    public struct BonusChoice
    {
        public int Bonus;
        public int Upgrade; // -1 means acquisition of a new ability.
        public BonusChoice(int bonus, int upgrade) { Bonus = bonus; Upgrade = upgrade; }
    }

    public sealed class BonusInventory : MonoBehaviour
    {
        public BonusCatalog Catalog;
        [SerializeField] private List<OwnedBonus> owned = new List<OwnedBonus>();
        public IReadOnlyList<OwnedBonus> Owned => owned;

        public bool Owns(int index) => owned.Exists(item => item.DefinitionIndex == index);

        public BonusChoice[] Generate()
        {
            var result = new List<BonusChoice>();
            // Reject duplicates without an unbounded reroll: condition the weights on unselected results.
            var newChoices = new List<BonusChoice>();
            var upgrades = new List<BonusChoice>();
            for (int i = 0; i < Catalog.Bonuses.Length; i++)
            {
                if (!Owns(i)) newChoices.Add(new BonusChoice(i, -1));
                else for (int j = 0; j < Catalog.Bonuses[i].Upgrades.Length; j++) upgrades.Add(new BonusChoice(i, j));
            }
            float newChance = owned.Count == 0 ? Catalog.NewAtZeroSlots :
                owned.Count >= Catalog.Slots ? Catalog.NewAtFullSlots : Catalog.NewAtOneOrTwoSlots;
            for (int slot = 0; slot < 3; slot++)
            {
                var candidates = new List<BonusChoice>();
                var weights = new List<float>();
                float sumNew = 0f, sumOwned = 0f;
                foreach (var choice in newChoices) sumNew += Catalog.Bonuses[choice.Bonus].Weight;
                foreach (var item in owned) sumOwned += Catalog.Bonuses[item.DefinitionIndex].Weight;
                foreach (var choice in newChoices)
                {
                    if (Contains(result, choice) || owned.Count >= Catalog.Slots) continue;
                    candidates.Add(choice);
                    weights.Add(newChance * Catalog.Bonuses[choice.Bonus].Weight / sumNew);
                }
                foreach (var choice in upgrades)
                {
                    if (Contains(result, choice)) continue;
                    var bonus = Catalog.Bonuses[choice.Bonus];
                    float upgradeSum = 0f;
                    foreach (var upgrade in bonus.Upgrades) upgradeSum += upgrade.Weight;
                    candidates.Add(choice);
                    weights.Add((100f - newChance) * bonus.Weight / sumOwned * bonus.Upgrades[choice.Upgrade].Weight / upgradeSum);
                }
                int selected = WeightedSelection.Draw(weights, UnityEngine.Random.value);
                if (selected < 0) throw new InvalidOperationException("Bonus catalog cannot produce three distinct banners.");
                result.Add(candidates[selected]);
            }
            return result.ToArray();
        }

        private static bool Contains(List<BonusChoice> list, BonusChoice choice)
            => list.Exists(item => item.Bonus == choice.Bonus && item.Upgrade == choice.Upgrade);

        public void Apply(BonusChoice choice)
        {
            if (choice.Upgrade < 0)
            {
                if (Owns(choice.Bonus) || owned.Count >= Catalog.Slots) throw new InvalidOperationException("Invalid bonus acquisition.");
                owned.Add(new OwnedBonus { DefinitionIndex = choice.Bonus, UpgradeCounts = new int[Catalog.Bonuses[choice.Bonus].Upgrades.Length] });
            }
            else
            {
                var item = owned.Find(value => value.DefinitionIndex == choice.Bonus);
                if (item == null) throw new InvalidOperationException("Cannot upgrade an unowned bonus.");
                item.UpgradeCounts[choice.Upgrade]++;
            }
        }

        public string Label(BonusChoice choice)
        {
            var bonus = Catalog.Bonuses[choice.Bonus];
            if (choice.Upgrade < 0) return bonus.Name + "\nNUOVA ABILITÀ BONUS";
            var upgrade = bonus.Upgrades[choice.Upgrade];
            // The PG RANGE stat is hidden. A named BONUS RANGE upgrade is explicitly required on banners.
            return bonus.Name + "\n" + upgrade.Label;
        }

        public bool TryGetValue(OwnedBonus item, string parameter, out float value)
        {
            var definition = Catalog.Bonuses[item.DefinitionIndex];
            var original = Array.Find(definition.Parameters, entry => entry.Key == parameter);
            value = 0f;
            if (original == null) return false; // Undefined values (e.g. FIRE BULLET DANNO) are never invented.
            value = original.BaseValue;
            for (int i = 0; i < definition.Upgrades.Length; i++)
            {
                var upgrade = definition.Upgrades[i];
                if (upgrade.Key == parameter)
                    value += item.UpgradeCounts[i] * (original.BaseValue * upgrade.BasePercent / 100f + upgrade.FlatAmount);
            }
            return true;
        }
    }

    public static class WeightedSelection
    {
        public static int Draw(IReadOnlyList<float> weights, float unitRandom)
        {
            float sum = 0f;
            for (int i = 0; i < weights.Count; i++) sum += Mathf.Max(0f, weights[i]);
            if (sum <= 0f) return -1;
            float roll = Mathf.Clamp01(unitRandom) * sum;
            int last = -1;
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] <= 0f) continue;
                last = i;
                if (roll < weights[i]) return i;
                roll -= weights[i];
            }
            return last;
        }
    }
}
