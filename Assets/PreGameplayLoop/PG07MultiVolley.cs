using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG07MultiVolley : MonoBehaviour
    {
        private sealed class Impact { public float Time; public Vector2 Direction; }
        private readonly Dictionary<Combatant, Impact> pending = new Dictionary<Combatant, Impact>();
        private readonly HashSet<Combatant> pushed = new HashSet<Combatant>();
        private sealed class Motion
        {
            public Combatant Target;
            public Vector2 Direction;
            public float Elapsed, Applied;
        }
        private readonly List<Motion> motions = new List<Motion>();
        private float distance, duration;
        public int Pushes { get; private set; }
        public void Initialize(float push, float seconds = .25f) { distance = push; duration = seconds; }
        public IProjectileHitEffect ForShot(Projectile shot) => new ShotHit(this, shot);
        private sealed class ShotHit : IProjectileHitEffect
        {
            private readonly PG07MultiVolley volley;
            private readonly Projectile shot;
            public ShotHit(PG07MultiVolley owner, Projectile value) { volley = owner; shot = value; }
            public void ResolveHit(Combatant target, float damage, bool round, Combatant source, int index)
            {
                if (target.Hit(damage, true, source) && volley != null)
                    volley.Record(target, shot.Direction, shot.LastHitSimulationTime);
            }
        }
        private void Record(Combatant target, Vector2 direction, float time)
        {
            if (pushed.Contains(target)) return;
            if (!pending.TryGetValue(target, out var first) || time < first.Time - .00001f)
                pending[target] = new Impact { Time = time, Direction = direction };
            else if (Mathf.Abs(time - first.Time) <= .00001f) first.Direction += direction;
        }
        private void LateUpdate()
        {
            if (Time.timeScale <= 0) return;
            foreach (var pair in pending)
            {
                if (pair.Key == null || !pair.Key.IsActive || !pushed.Add(pair.Key)) continue;
                pair.Key.GetComponent<MobBrain>()?.InterruptForPush();
                motions.Add(new Motion { Target = pair.Key, Direction = pair.Value.Direction.normalized });
                Pushes++;
            }
            pending.Clear();
            for (int i = motions.Count - 1; i >= 0; i--)
            {
                var motion = motions[i];
                if (motion.Target == null || !motion.Target.IsActive) { motions.RemoveAt(i); continue; }
                motion.Elapsed += Time.deltaTime;
                float t = duration > 0 ? Mathf.Clamp01(motion.Elapsed / duration) : 1;
                // Quadratic ease-out: fast start, progressively slower arrival.
                float desired = distance * (1 - (1 - t) * (1 - t));
                float step = Mathf.Max(0, desired - motion.Applied);
                float travel = step;
                Physics2D.SyncTransforms();
                foreach (var hit in Physics2D.CircleCastAll(motion.Target.transform.position, motion.Target.Radius, motion.Direction, step))
                    if (hit.collider.GetComponent<TestObstacle>() != null && Vector2.Dot(motion.Direction, hit.normal) < 0)
                        travel = Mathf.Min(travel, Mathf.Max(0, hit.distance - .001f));
                motion.Target.Separate(motion.Direction * travel);
                motion.Applied = desired;
                if (t >= 1 || travel < step) motions.RemoveAt(i);
            }
        }
    }
}
