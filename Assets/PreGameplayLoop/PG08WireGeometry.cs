using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public static class PG08WireGeometry
    {
        // The target collider center must be inside the continuous annulus.
        // Walls and obstacles are checked separately by the shared visibility query.
        public static bool Contains(Vector2 offset, float inner, float outer)
        {
            float distance = offset.magnitude;
            return distance >= inner && distance <= outer;
        }
    }
}
