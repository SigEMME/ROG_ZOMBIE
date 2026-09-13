namespace RogZombie.TestEngine
{
    // Incoming effects are consumed after mitigation; persistent stats remain untouched.
    public interface ICombatHitEffect : ICombatStatModifier { void AfterHit(); }
    public interface IProjectileHitEffect
    {
        void ResolveHit(Combatant target, float damage, bool roundDamage, Combatant source, int hitIndex);
    }
}
