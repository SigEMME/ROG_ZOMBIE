using UnityEngine;

namespace RogZombie
{
    [CreateAssetMenu(fileName = "PG", menuName = "ROG ZOMBIE/PG Definition")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [SerializeField, InspectorName("PG ID")]
        private string playerId = string.Empty;

        [SerializeField, InspectorName("Base STATS")]
        private PlayerStats baseStats = default;

        public string PlayerId => playerId;

        // Return a value copy: runtime consumers cannot mutate the shared base data.
        public PlayerStats BaseStats => baseStats;
    }
}
