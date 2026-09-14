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

        private void Awake()
        {
            if (ApplyStatFallback != null) return;
            ApplyStatFallback = index =>
            {
                var pg = GetComponent<PlayerRuntime>();
                if (pg == null) throw new InvalidOperationException("STAT fallback requires a player.");
                pg.ApplyRunStat(index);
            };
        }

        public bool Owns(int index) => owned.Exists(item => item.DefinitionIndex == index);

        public event Action<BonusChoice> Changed;
        public Action<int> ApplyStatFallback;
        public OwnedBonus Find(string id) => owned.Find(item => Catalog.Bonuses[item.DefinitionIndex].Id == id);

        public bool CanUpgrade(int bonus, int upgrade)
        {
            var item = owned.Find(value => value.DefinitionIndex == bonus);
            if (item == null || upgrade < 0 || upgrade >= item.UpgradeCounts.Length) return false;
            string key = Catalog.Bonuses[bonus].Upgrades[upgrade].Key;
            if (key == "CD") return item.UpgradeCounts[upgrade] < 9;
            return !(Catalog.Bonuses[bonus].Id == "shield" && key == "DEF" && Value(item, "DEF") >= 90);
        }

        public float Value(OwnedBonus item, string key)
        {
            if (!TryGetValue(item, key, out float value)) throw new InvalidOperationException("Missing bonus parameter: " + key);
            return value;
        }

        public BonusChoice[] Generate(int count = 3, IEnumerable<int> excludedStats = null)
        {
            var result = new List<BonusChoice>();
            var stats = new HashSet<int>(excludedStats ?? Array.Empty<int>());
            float newChance = owned.Count == 0 ? Catalog.NewAtZeroSlots :
                owned.Count >= Catalog.Slots ? Catalog.NewAtFullSlots : Catalog.NewAtOneOrTwoSlots;
            for (int slot = 0; slot < count; slot++)
            {
                bool acquire = newChance >= 100 || newChance > 0 && UnityEngine.Random.value * 100 < newChance;
                var candidates = new List<BonusChoice>();
                var weights = new List<float>();
                if (acquire)
                {
                    for (int i = 0; i < Catalog.Bonuses.Length; i++)
                    {
                        var choice = new BonusChoice(i, -1);
                        if (Owns(i) || Contains(result, choice)) continue;
                        candidates.Add(choice); weights.Add(Catalog.Bonuses[i].Weight);
                    }
                }
                else
                {
                    // Two-stage draw: ability rate, then its remaining specific upgrade rates.
                    foreach (var item in owned)
                    {
                        var def = Catalog.Bonuses[item.DefinitionIndex];
                        float sum = 0;
                        for (int j = 0; j < def.Upgrades.Length; j++)
                            if (CanUpgrade(item.DefinitionIndex, j) && !Contains(result, new BonusChoice(item.DefinitionIndex, j))) sum += def.Upgrades[j].Weight;
                        if (sum <= 0) continue;
                        for (int j = 0; j < def.Upgrades.Length; j++)
                        {
                            var choice = new BonusChoice(item.DefinitionIndex, j);
                            if (!CanUpgrade(choice.Bonus, j) || Contains(result, choice)) continue;
                            candidates.Add(choice); weights.Add(def.Weight * def.Upgrades[j].Weight / sum);
                        }
                    }
                }
                int selected = WeightedSelection.Draw(weights, UnityEngine.Random.value);
                if (selected >= 0) result.Add(candidates[selected]);
                else
                {
                    if (acquire) throw new InvalidOperationException("No distinct new bonus ability available.");
                    float[] statWeights = { 25, 8, 22, 22, 8, 15 };
                    foreach (int stat in stats) statWeights[stat] = 0;
                    int statIndex = WeightedSelection.Draw(statWeights, UnityEngine.Random.value);
                    if (statIndex < 0) throw new InvalidOperationException("No distinct STAT banner available.");
                    stats.Add(statIndex); result.Add(new BonusChoice(-1, statIndex));
                }
            }
            return result.ToArray();
        }

        private static bool Contains(List<BonusChoice> list, BonusChoice choice)
            => list.Exists(item => item.Bonus == choice.Bonus && item.Upgrade == choice.Upgrade);

        public void Apply(BonusChoice choice)
        {
            if (choice.Bonus < 0)
            {
                if (ApplyStatFallback == null) throw new InvalidOperationException("STAT fallback handler missing.");
                ApplyStatFallback(choice.Upgrade); Changed?.Invoke(choice); return;
            }
            if (choice.Upgrade < 0)
            {
                if (Owns(choice.Bonus) || owned.Count >= Catalog.Slots) throw new InvalidOperationException("Invalid bonus acquisition.");
                owned.Add(new OwnedBonus { DefinitionIndex = choice.Bonus, UpgradeCounts = new int[Catalog.Bonuses[choice.Bonus].Upgrades.Length] });
            }
            else
            {
                var item = owned.Find(value => value.DefinitionIndex == choice.Bonus);
                if (item == null || !CanUpgrade(choice.Bonus, choice.Upgrade)) throw new InvalidOperationException("Invalid or capped bonus upgrade.");
                item.UpgradeCounts[choice.Upgrade]++;
            }
            Changed?.Invoke(choice);
        }

        public string Label(BonusChoice choice)
        {
            if (choice.Bonus < 0) return RogZombie.PreGameplayLoop.AreaStatBonus.Labels[choice.Upgrade];
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
            if (parameter == "CD") value = Mathf.Max(original.BaseValue * .1f, value);
            if (definition.Id == "shield" && parameter == "DEF") value = Mathf.Min(90, value);
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
