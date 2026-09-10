using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    public sealed class AreaPickup : MonoBehaviour
    {
        public bool IsChest;
        public float TriggerRadius;
        public float HealPercent;
        public float[] ChestWeights;
        public bool ShowTrigger;
        private bool consumed;

        private void LateUpdate()
        {
            if (consumed || Time.timeScale == 0f) return;
            var candidates = new List<Combatant>();
            foreach (var actor in Combatant.All)
            {
                if (actor.Faction != Faction.PG || !actor.IsActive) continue;
                if (Vector2.Distance(actor.transform.position, transform.position) > TriggerRadius + actor.Radius) continue;
                if (IsChest || actor.CurrentHP < actor.Stats.HP) candidates.Add(actor);
            }
            if (candidates.Count == 0) return;
            consumed = true; // Consume once before callbacks can affect any other actor.
            if (IsChest)
            {
                foreach (var actor in Combatant.All.ToArray())
                    if (actor != null && actor.Faction == Faction.PG && actor.State != LifeState.Dead)
                        actor.GetComponent<PlayerRuntime>()?.ApplyChest(WeightedSelection.Draw(ChestWeights, Random.value));
            }
            else
            {
                var best = SelectHealTarget(candidates, Random.value);
                best.Heal(HealPercent / 100f);
            }
            Destroy(gameObject);
        }

        public static Combatant SelectHealTarget(List<Combatant> candidates, float roll)
        {
            float fraction = float.PositiveInfinity, hp = float.PositiveInfinity;
            var ties = new List<Combatant>();
            foreach (var actor in candidates)
            {
                float ratio = actor.HealthFraction;
                if (ratio < fraction || (ratio == fraction && actor.CurrentHP < hp))
                {
                    fraction = ratio; hp = actor.CurrentHP; ties.Clear(); ties.Add(actor);
                }
                else if (ratio == fraction && actor.CurrentHP == hp) ties.Add(actor);
            }
            return ties.Count > 0 ? ties[Mathf.Min((int)(roll * ties.Count), ties.Count - 1)] : null;
        }

        private void OnDrawGizmos()
        {
            if (!ShowTrigger) return;
            Gizmos.color = IsChest ? Color.yellow : Color.green;
            Gizmos.DrawWireSphere(transform.position, TriggerRadius);
        }
    }
}
