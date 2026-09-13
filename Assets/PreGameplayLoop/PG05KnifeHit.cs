using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG05KnifeHit : IProjectileHitEffect
    {
        private readonly LoopSession session;
        private readonly float poisonDamage, poisonDuration;
        public PG05KnifeHit(LoopSession owner, float damage, float duration)
        { session = owner; poisonDamage = damage; poisonDuration = duration; }
        public void ResolveHit(Combatant target, float damage, bool roundDamage, Combatant source, int hitIndex)
        {
            if (target.Hit(damage, true, source) && target.IsActive)
                (target.GetComponent<PG05Poison>() ?? target.gameObject.AddComponent<PG05Poison>())
                    .Apply(session, source, poisonDamage, poisonDuration);
        }
    }
}
