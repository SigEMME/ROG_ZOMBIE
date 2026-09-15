using UnityEngine;
using UnityEngine.AI;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    [DefaultExecutionOrder(50)]
    public sealed class CompanionFormation : MonoBehaviour
    {
        public const float FormationDistance = 1f;
        public LoopSession Context;
        private readonly Collider2D[] overlaps = new Collider2D[32];
        private NavMeshPath path, candidatePath;
        private Vector2 pathStart;
        private Vector3[] corners = new Vector3[16];
        private void Awake() { path = new NavMeshPath(); candidatePath = new NavMeshPath(); }
        private void KeepCandidatePath() { var previous = path; path = candidatePath; candidatePath = previous; }
        [SerializeField, Min(.01f)] private float aimSmoothTime = .25f;
        private float formationAngle, angularVelocity;
        private Vector2 previousGoal;
        private bool hasGoal;
        public Vector2 Goal { get; private set; }
        private void LateUpdate()
        {
            if (Context == null || Context.DirectlyControlled || !Context.GameplayRunning || !Context.Player.Actor.IsActive) { hasGoal = false; return; }
            var leader = Context.World.Controlled.Player;
            if (!leader.GetComponent<PlayerAim>().TryGetCursorWorldPosition(out var cursor)) return;
            Advance(cursor, Time.deltaTime);
        }
        public void Advance(Vector2 cursor, float seconds)
        {
            if (seconds <= 0 || Context == null || Context.DirectlyControlled || !Context.GameplayRunning || !Context.Player.Actor.IsActive) return;
            var leader = Context.World.Controlled.Player;
            Vector2 origin = leader.transform.position, direction = cursor - origin;
            if (direction.sqrMagnitude < .000001f) direction = leader.transform.right;
            Goal = origin - direction.normalized * FormationDistance;
            Vector2 relative = (Vector2)transform.position - origin;
            float desiredAngle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            if (!hasGoal)
            {
                formationAngle = relative.sqrMagnitude > .000001f ? Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg : desiredAngle;
                angularVelocity = 0;
            }
            // Smooth the formation angle, not the aim or the PLAYER translation.
            // Keeping the offset on a circle preserves the requested formation radius.
            formationAngle = Mathf.SmoothDampAngle(formationAngle, desiredAngle, ref angularVelocity, aimSmoothTime, Mathf.Infinity, seconds);
            float radians = formationAngle * Mathf.Deg2Rad;
            Vector2 smoothGoal = origin + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * FormationDistance;
            // A path through the leader is blocked by PG collision. Go around it when catching up.
            Vector2 towardGoal = smoothGoal - (Vector2)transform.position;
            Vector2 nearest = relative + towardGoal * Mathf.Clamp01(-Vector2.Dot(relative, towardGoal) / Mathf.Max(towardGoal.sqrMagnitude, .000001f));
            float clearance = Context.Player.Actor.Radius + leader.Actor.Radius + .001f;
            Vector2 waypoint = smoothGoal;
            if (relative.sqrMagnitude > clearance * clearance && nearest.sqrMagnitude < clearance * clearance)
            {
                float sign = Vector2.SignedAngle(relative, smoothGoal - origin) < 0 ? -1 : 1;
                waypoint = origin + new Vector2(-relative.y, relative.x).normalized * (FormationDistance * sign);
            }
            if (!TryResolvePosition(origin, waypoint, out _)) return;
            float follow = leader.Actor.MovementMetresPerSecond * seconds;
            if (hasGoal) follow = Mathf.Max(follow, Vector2.Distance(previousGoal, smoothGoal));
            previousGoal = smoothGoal; hasGoal = true;
            Vector2 position = transform.position;
            int count = path.GetCornersNonAlloc(corners);
            if (count == corners.Length)
            {
                var complete = path.corners;
                corners = new Vector3[complete.Length * 2];
                System.Array.Copy(complete, corners, complete.Length); count = complete.Length;
            }
            for (int i = 0; i < count; i++)
            {
                Vector2 delta = TestNavigation.FromNav(corners[i]) - position;
                if (delta.sqrMagnitude < .0001f) continue;
                Context.Player.Actor.Move(Vector2.ClampMagnitude(delta, follow));
                break;
            }
        }
        // Candidate search stays inside the PLAYER radius after NavMesh projection.
        public bool TryResolvePosition(Vector2 origin, Vector2 desired, out Vector2 result)
        {
            result = default;
            if (Context.Navigation == null || !Context.Navigation.Ready) return false;
            Physics2D.SyncTransforms();
            if (!Context.Navigation.Sample(transform.position, out pathStart)) return false;
            if (TryCandidate(origin, desired, out result)) { KeepCandidatePath(); return true; }
            float best = float.PositiveInfinity;
            Vector2 direction = desired - origin;
            float angle = Mathf.Atan2(direction.y, direction.x);
            for (int ring = 0; ring < 5; ring++)
            for (int step = 0; step < 72; step++)
            {
                float a = angle + step * Mathf.PI * 2 / 72;
                Vector2 candidate = origin + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (FormationDistance * (1 - ring * .2f));
                if (!Context.Navigation.Sample(candidate, out var sampled, .1f)) continue;
                float score = (sampled - desired).sqrMagnitude;
                if (score >= best || !TryCandidate(origin, sampled, out var valid)) continue;
                best = score; result = valid; KeepCandidatePath();
            }
            if (!float.IsPositiveInfinity(best)) return true;
            // Only when the entire local search fails, allow temporary separation.
            // Retry the one-metre search first on every update so the companion returns.
            float extent = Context.Settings.AreaSize.magnitude;
            for (float radius = FormationDistance + .25f; radius <= extent; radius += .25f)
            for (int step = 0; step < 72; step++)
            {
                float a = angle + step * Mathf.PI * 2 / 72;
                Vector2 candidate = origin + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                if (TryCandidate(origin, candidate, out result, false)) { KeepCandidatePath(); return true; }
            }
            return false;
        }
        private bool TryCandidate(Vector2 origin, Vector2 candidate, out Vector2 result, bool limitDistance = true)
        {
            result = default;
            if (!Context.Navigation.Sample(candidate, out var sampled, .1f) ||
                limitDistance && (sampled - origin).sqrMagnitude > FormationDistance * FormationDistance) return false;
            int count = Physics2D.OverlapCircle(sampled, Context.Player.Actor.Radius + .001f,
                ContactFilter2D.noFilter, overlaps);
            if (count == overlaps.Length) return false;
            for (int i = 0; i < count; i++)
            {
                var hit = overlaps[i];
                if (hit.gameObject == gameObject || hit.isTrigger) continue;
                var actor = hit.GetComponent<Combatant>();
                if (hit.GetComponent<TestObstacle>() != null || actor != null && actor.State != LifeState.Dead) return false;
            }
            if (!NavMesh.CalculatePath(TestNavigation.ToNav(pathStart), TestNavigation.ToNav(sampled), NavMesh.AllAreas, candidatePath) ||
                candidatePath.status != NavMeshPathStatus.PathComplete) return false;
            result = sampled;
            return true;
        }
    }
}
