using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG04BurningArea : MonoBehaviour
    {
        private LoopSession session;
        private Combatant source;
        private PG04AreaVisual visual;
        private float radius, duration, damage, elapsed;
        private int nextTick = 1;
        public int Ticks { get; private set; }
        public float Remaining => Mathf.Max(0, duration - elapsed);
        public float Radius => radius;
        public void Initialize(LoopSession owner, Combatant actor, float areaRadius, float seconds, float tickDamage, bool[] sectors)
        {
            session = owner; source = actor; radius = areaRadius; duration = seconds; damage = tickDamage;
            visual = gameObject.AddComponent<PG04AreaVisual>();
            Tick(); // Owner confirmation: area clock, first tick immediately at activation.
        }
        private void Tick()
        {
            Ticks++;
            PG04ExplosionEffect.Hit(source, transform.position, radius, null, damage);
            visual.InitializeOccluded(transform.position, radius, new Color(1, .22f, .02f, .4f));
        }
        private void Update()
        {
            if (session == null || !session.GameplayRunning) return;
            elapsed += Time.deltaTime;
            // Lifetime [0, duration): default ticks 0, 1, 2; expiry at 3 seconds.
            while (nextTick < duration && elapsed >= nextTick) { Tick(); nextTick++; }
            if (elapsed >= duration) Destroy(gameObject);
        }
    }
}
