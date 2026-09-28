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
        // Local x points toward aim; y points left. Adjacent diamond vertices are 1 m apart.
        public static Vector2 Offset(int followers, int index)
        {
            if (followers <= 1) return Vector2.left * FormationDistance;
            if (followers == 2) return new Vector2(-Mathf.Sqrt(3) * .5f, index == 0 ? .5f : -.5f) * FormationDistance;
            float diagonal = Mathf.Sqrt(.5f) * FormationDistance;
            return index < 2 ? new Vector2(-diagonal, index == 0 ? diagonal : -diagonal) : new Vector2(-2 * diagonal, 0);
        }
        private float searchRadius = FormationDistance;
        private float LocalRadius => Context != null ? Context.World.FormationOffset(Context).magnitude : FormationDistance;
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
            Vector2 intent = Context != null && Context.World.Controlled != null
                ? Context.World.Controlled.Player.GetComponent<PlayerMovement>().RequestedVelocity : Vector2.zero;
            Advance(cursor, seconds, intent);
        }
        public void Advance(Vector2 cursor, float seconds, Vector2 leaderVelocity)
        {
            if (seconds <= 0 || Context == null || Context.DirectlyControlled || !Context.GameplayRunning || !Context.Player.Actor.IsActive) return;
            var leader = Context.World.Controlled.Player;
            Vector2 origin = leader.transform.position, direction = cursor - origin;
            if (direction.sqrMagnitude < .000001f) direction = leader.transform.right;
            Vector2 forward = direction.normalized;
            Vector2 offset = Context.World.FormationOffset(Context);
            float radius = offset.magnitude;
            Goal = origin + forward * offset.x + new Vector2(-forward.y, forward.x) * offset.y;
            Vector2 relative = (Vector2)transform.position - origin;
            if (TryFollowThroughPassage(leader, relative, leaderVelocity, seconds))
            {
                // Restart angular interpolation from the real location when space opens up.
                hasGoal = false;
                return;
            }
            float desiredAngle = Mathf.Atan2(Goal.y - origin.y, Goal.x - origin.x) * Mathf.Rad2Deg;
            if (!hasGoal)
            {
                formationAngle = relative.sqrMagnitude > .000001f ? Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg : desiredAngle;
                angularVelocity = 0;
            }
            // Smooth the formation angle, not the aim or the PLAYER translation.
            // Keeping the offset on a circle preserves the requested formation radius.
            formationAngle = Mathf.SmoothDampAngle(formationAngle, desiredAngle, ref angularVelocity, aimSmoothTime, Mathf.Infinity, seconds);
            float radians = formationAngle * Mathf.Deg2Rad;
            Vector2 smoothGoal = origin + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
            // A path through the leader is blocked by PG collision. Go around it when catching up.
            Vector2 towardGoal = smoothGoal - (Vector2)transform.position;
            Vector2 nearest = relative + towardGoal * Mathf.Clamp01(-Vector2.Dot(relative, towardGoal) / Mathf.Max(towardGoal.sqrMagnitude, .000001f));
            float clearance = Context.Player.Actor.Radius + leader.Actor.Radius + .001f;
            Vector2 waypoint = smoothGoal;
            if (relative.sqrMagnitude > clearance * clearance && nearest.sqrMagnitude < clearance * clearance)
            {
                float sign = Vector2.SignedAngle(relative, smoothGoal - origin) < 0 ? -1 : 1;
                waypoint = origin + new Vector2(-relative.y, relative.x).normalized * (radius * sign);
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
        private bool TryFollowThroughPassage(PlayerRuntime leader, Vector2 relative, Vector2 velocity, float seconds)
        {
            if (velocity.sqrMagnitude < .000001f || Context.Navigation == null || !Context.Navigation.Ready) return false;
            Vector2 direction = velocity.normalized;
            float clearance = Context.Player.Actor.Radius + leader.Actor.Radius + .02f;
            float ahead = Vector2.Dot(relative, direction);
            float lateral = Mathf.Abs(relative.x * direction.y - relative.y * direction.x);
            // Include the queue in front, so one companion does not trap the next one.
            if (ahead <= 0 || ahead > FormationDistance * (Context.World.Companions.Count + 1) || lateral > clearance) return false;
            Physics2D.SyncTransforms();
            Vector2 side = new Vector2(-direction.y, direction.x) * clearance;
            Vector2 origin = leader.transform.position, position = transform.position;
            if (PassingRoom(origin + side, position + side) || PassingRoom(origin - side, position - side)) return false;
            if (!Context.Navigation.Sample(position, out var start, .1f)) return true;
            Vector2 desired = position + velocity * seconds;
            // Clip to the walkable segment; never project or teleport through a wall.
            bool blocked = NavMesh.Raycast(TestNavigation.ToNav(start), TestNavigation.ToNav(desired), out var hit, NavMesh.AllAreas);
            Vector2 target = blocked ? TestNavigation.FromNav(hit.position) : desired;
            Vector2 displacement = Vector2.ClampMagnitude(target - position, velocity.magnitude * seconds);
            if (Vector2.Dot(displacement, direction) > 0) Context.Player.Actor.Move(displacement);
            return true;
        }
        private bool PassingRoom(Vector2 besideLeader, Vector2 besideCompanion)
        {
            if (!FreePassagePoint(besideLeader) || !FreePassagePoint(besideCompanion)) return false;
            return !NavMesh.Raycast(TestNavigation.ToNav(besideLeader), TestNavigation.ToNav(besideCompanion), out _, NavMesh.AllAreas);
        }
        private bool FreePassagePoint(Vector2 point)
        {
            if (!Context.Navigation.Sample(point, out var sampled, .05f) || Vector2.Distance(point, sampled) > .02f) return false;
            int count = Physics2D.OverlapCircle(point, Context.Player.Actor.Radius + .001f, ContactFilter2D.noFilter, overlaps);
            if (count == overlaps.Length) return false;
            for (int i = 0; i < count; i++)
            {
                var hit = overlaps[i];
                if (hit.isTrigger || hit.gameObject == gameObject) continue;
                var actor = hit.GetComponent<Combatant>();
                if (hit.GetComponent<TestObstacle>() != null || actor != null && actor.State != LifeState.Dead) return false;
            }
            return true;
        }
        // Candidate search respects the current slot radius, including the rear diamond vertex.
        public bool TryResolvePosition(Vector2 origin, Vector2 desired, out Vector2 result)
        {
            result = default;
            searchRadius = LocalRadius;
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
                Vector2 candidate = origin + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (searchRadius * (1 - ring * .2f));
                if (!Context.Navigation.Sample(candidate, out var sampled, .1f)) continue;
                float score = (sampled - desired).sqrMagnitude;
                if (score >= best || !TryCandidate(origin, sampled, out var valid)) continue;
                best = score; result = valid; KeepCandidatePath();
            }
            if (!float.IsPositiveInfinity(best)) return true;
            // Only when the entire local search fails, allow temporary separation.
            // Retry the formation-radius search first on every update so the companion returns.
            float extent = Context.Settings.AreaSize.magnitude;
            for (float radius = searchRadius + .25f; radius <= extent; radius += .25f)
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
                limitDistance && (sampled - origin).sqrMagnitude > searchRadius * searchRadius + .00001f) return false;
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
