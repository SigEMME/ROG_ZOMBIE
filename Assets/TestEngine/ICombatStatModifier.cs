namespace RogZombie.TestEngine
{
    public interface ICombatStatModifier
    {
        CombatStats Apply(CombatStats persistentStats);
    }
}
