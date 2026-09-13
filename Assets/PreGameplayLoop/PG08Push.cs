using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    // One motion per HIT: unlike MULTI SHOT, repeated hits never deduplicate or restart a prior push.
    public sealed class PG08Push : MonoBehaviour
    {
        private Combatant target;
        private LoopSession session;
        private Vector2 direction;
        private float distance, duration, elapsed, applied;
        public void Initialize(LoopSession owner, Combatant value, Vector2 forward, float metres, float seconds)
        {
            session = owner; target = value; direction = forward.normalized; distance = metres; duration = seconds;
            target.GetComponent<MobBrain>()?.InterruptForPush();
        }
        private void LateUpdate()
        {
            if (target == null || !target.IsActive) { Destroy(gameObject); return; }
            if (session == null || !session.GameplayRunning) return;
            Advance(Time.deltaTime);
        }
        public void Advance(float seconds)
        {
            if (target == null || !target.IsActive || seconds <= 0) return;
            elapsed += seconds;
            float t = Mathf.Clamp01(elapsed / duration);
            float desired = distance * (1 - (1 - t) * (1 - t));
            float step = Mathf.Max(0, desired - applied), travel = step;
            Physics2D.SyncTransforms();
            foreach (var hit in Physics2D.CircleCastAll(target.transform.position, target.Radius, direction, step))
                if (hit.collider.GetComponent<TestObstacle>() != null && Vector2.Dot(direction, hit.normal) < 0)
                    travel = Mathf.Min(travel, Mathf.Max(0, hit.distance - .001f));
            target.Separate(direction * travel);
            applied = desired;
            if (t >= 1 || travel < step) { enabled = false; Destroy(gameObject); }
        }
    }
}
