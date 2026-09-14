using System;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [RequireComponent(typeof(Combatant))]
    public sealed class PlayerRuntime : MonoBehaviour
    {
        [SerializeField] private PlayerDefinition definition;
        [SerializeField, InspectorName("CD REDUCTION")] private float cdReduction;
        public PlayerDefinition Definition => definition;
        public Combatant Actor { get; private set; }
        public float CdReduction => cdReduction;
        public event Action<string> Notice;

        public void Initialize(PlayerDefinition value)
        {
            definition = value;
            Actor = GetComponent<Combatant>();
            Actor.Initialize(Faction.PG, CombatStats.FromPG(value.BaseStats));
            cdReduction = value.BaseStats.CdReduction;
            GetComponent<PlayerMovement>().Configure(value);
        }

        public void ApplyRunStat(int statIndex)
        {
            RogZombie.PreGameplayLoop.AreaStatBonus.Apply(Actor, (RogZombie.PreGameplayLoop.AreaStat)statIndex, ref cdReduction);
        }

        public void ApplyChest(int statIndex)
        {
            if (Actor.State == LifeState.Dead) return;
            CombatStats original = CombatStats.FromPG(definition.BaseStats);
            CombatStats stats = Actor.Stats;
            float health = 0f;
            string label;
            switch (statIndex)
            {
                case 0: health = original.HP * 0.05f; stats.HP += health; label = "HP"; break;
                case 1: stats.ATK += original.ATK * 0.05f; label = "ATK"; break;
                case 2: stats.DEF += original.DEF * 0.05f; label = "DEF"; break;
                case 3: stats.MoveSpeed += original.MoveSpeed * 0.05f; label = "MOVE SPD"; break;
                case 4: stats.AttackSpeed += original.AttackSpeed * 0.05f; label = "ATK SPD"; break;
                case 5: cdReduction -= definition.BaseStats.CdReduction * 0.05f; label = "CD REDUCTION"; break;
                default: throw new ArgumentOutOfRangeException(nameof(statIndex));
            }
            Actor.SetStats(stats, health);
            Notice?.Invoke("CHEST: " + label + (statIndex == 5 ? " -5% del BASE" : " +5% del BASE"));
        }
    }
}
