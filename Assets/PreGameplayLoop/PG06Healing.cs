using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public static class PG06Healing
    {
        public static bool Valid(Combatant pg) => pg != null && pg.IsActive && pg.Faction == Faction.PG;
        public static Combatant Select(IList<Combatant> candidates, Vector2 origin, bool distanceTie)
        {
            Combatant best = null; int ties = 0;
            foreach (var pg in candidates)
            {
                if (!Valid(pg)) continue;
                int comparison = best == null ? -1 : pg.HealthFraction.CompareTo(best.HealthFraction);
                if (best != null && comparison == 0 && pg.HealthFraction < 1) comparison = pg.CurrentHP.CompareTo(best.CurrentHP);
                if (best != null && comparison == 0 && distanceTie)
                    comparison = ((Vector2)pg.transform.position - origin).sqrMagnitude.CompareTo(((Vector2)best.transform.position - origin).sqrMagnitude);
                if (comparison < 0) { best = pg; ties = 1; }
                else if (comparison == 0 && Random.Range(0, ++ties) == 0) best = pg;
            }
            return best;
        }
        public static void Heal(LoopSession session, Combatant target, float hp, bool vitamin, float percent, float duration)
        {
            if (!Valid(target)) return;
            target.Heal(hp / target.Stats.HP);
            if (vitamin)
                (target.GetComponent<PG06Vitamin>() ?? target.gameObject.AddComponent<PG06Vitamin>()).Refresh(session, percent, duration);
        }
    }
}
