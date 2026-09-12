using System;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum AreaStat { HP, ATK, DEF, MoveSpeed, AttackSpeed, CdReduction }

    public static class AreaStatBonus
    {
        public static readonly string[] Labels = { "HP +5%", "ATK +5%", "DEF +5%", "MOVE SPD +5%", "ATK SPD +5%", "CD REDUCTION -5%" };

        public static AreaStat[] Draw()
        {
            // GDD 19/21 weights, normalized after each selected proposal.
            float[] weights = { 25, 8, 22, 22, 8, 15 };
            var result = new AreaStat[3];
            for (int i = 0; i < result.Length; i++)
            {
                int index = WeightedSelection.Draw(weights, UnityEngine.Random.value);
                result[i] = (AreaStat)index;
                weights[index] = 0;
            }
            return result;
        }

        public static void Apply(Combatant actor, AreaStat stat, ref float cdReduction)
        {
            var values = actor.Stats;
            float health = 0;
            switch (stat)
            {
                case AreaStat.HP: health = values.HP * .05f; values.HP += health; break;
                case AreaStat.ATK: values.ATK *= 1.05f; break;
                case AreaStat.DEF: values.DEF = Mathf.Min(190, values.DEF * 1.05f); break;
                case AreaStat.MoveSpeed: values.MoveSpeed *= 1.05f; break;
                case AreaStat.AttackSpeed: values.AttackSpeed *= 1.05f; break;
                case AreaStat.CdReduction: cdReduction = Mathf.Max(10, Mathf.Floor(cdReduction * .95f + .5f)); break;
                default: throw new ArgumentOutOfRangeException(nameof(stat));
            }
            actor.SetStats(values, health);
        }
    }
}
