using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie
{
    // Both test scenes supply their own actor factory and lifecycle.
    public interface ISpawnWorld
    {
        Camera GameCamera { get; }
        bool Loading { get; }
        Combatant CreateMob(int index, Vector2 position);
        void AddGold(int value);
    }
}
