using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class ItemSmokeStatus : MonoBehaviour
    {
        private Combatant actor;
        private float availableAt = float.NegativeInfinity;
        public bool Invisible => actor != null && actor.IsActive && Time.time >= availableAt && ItemArea.InSmoke(actor);
        private void Awake() { actor = GetComponent<Combatant>(); actor.PerformedAction += Action; }
        private void Action() { if (ItemArea.InSmoke(actor)) availableAt = Time.time + 1; }
        private void OnDestroy() { if (actor != null) actor.PerformedAction -= Action; }
    }
}
