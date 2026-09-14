using UnityEngine;
using UnityEngine.AI;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class BonusPet : MonoBehaviour
    {
        private LoopSession session;
        private BonusAbilityRuntime bonus;
        private Combatant owner, target;
        private CircleCollider2D body;
        private NavMeshPath path;
        private Vector2 idlePoint, previousOwner;
        private bool hasIdle;
        private float ready;
        public void Initialize(LoopSession loop, BonusAbilityRuntime runtime, Combatant pg)
        { path = new NavMeshPath(); session = loop; bonus = runtime; owner = pg; body = GetComponent<CircleCollider2D>(); previousOwner = pg.transform.position; }
        private void Update()
        {
            if (!session.GameplayRunning || !owner.IsActive) return;
            ready = Mathf.Max(0, ready - Time.deltaTime);
            Vector2 position = transform.position, pg = owner.transform.position;
            float distance = Vector2.Distance(position, pg);
            if (target == null || !target.IsActive) target = BonusAbilityRuntime.Closest(position, bonus.Value("pet", "RICERCA"));
            if (distance > 5) target = null;
            if (target != null)
            {
                if (!session.Navigation.Path(position, target.transform.position, path) || !WithinLeash(path, pg)) target = null;
                else
                {
                    if (Vector2.Distance(position, target.transform.position) <= bonus.Value("pet", "RANGE") &&
                        AttackGeometry.ClearLine(position, target.transform.position))
                    {
                        if (ready <= 0)
                        {
                            target.Hit(bonus.Value("pet", "DANNO"), true, owner);
                            TestVisuals.FlashCircle(position, .3f, Color.magenta);
                            ready = 100 / bonus.Value("pet", "ATK_SPD");
                        }
                    }
                    else Follow(path);
                    previousOwner = pg; return;
                }
            }
            bool moving = (pg - previousOwner).sqrMagnitude > .000001f;
            previousOwner = pg;
            Vector2 goal = pg;
            if (!moving && distance <= 5)
            {
                if (!hasIdle || Vector2.Distance(idlePoint, pg) > 3)
                {
                    Vector2 desired = pg + Random.insideUnitCircle * 3;
                    hasIdle = session.Navigation.Sample(desired, out idlePoint) && Vector2.Distance(idlePoint, pg) <= 3;
                }
                if (hasIdle) goal = idlePoint;
            }
            else hasIdle = false;
            if (Vector2.Distance(position, goal) > .05f && session.Navigation.Path(position, goal, path)) Follow(path);
        }
        public static bool WithinLeash(NavMeshPath candidate, Vector2 pg)
        {
            foreach (var corner in candidate.corners)
                if (Vector2.Distance(TestNavigation.FromNav(corner), pg) > 5) return false;
            return true;
        }
        private void Follow(NavMeshPath route)
        {
            Vector2 position = transform.position;
            foreach (var corner in route.corners)
            {
                Vector2 delta = TestNavigation.FromNav(corner) - position;
                if (delta.magnitude < .05f) continue;
                Vector2 step = Vector2.ClampMagnitude(delta, bonus.Value("pet", "MOVE_SPD") / 100 * 2 * Time.deltaTime);
                Physics2D.SyncTransforms(); float travel = step.magnitude;
                foreach (var hit in Physics2D.CircleCastAll(position, body.radius, step.normalized, travel))
                {
                    if (hit.collider == body) continue;
                    var actor = hit.collider.GetComponent<Combatant>();
                    bool blocks = hit.collider.GetComponent<TestObstacle>() != null || hit.collider.GetComponent<BonusPet>() != null ||
                        actor != null && actor.Faction == Faction.MOB && actor.State != LifeState.Dead;
                    if (blocks && Vector2.Dot(step.normalized, hit.normal) < 0) travel = Mathf.Min(travel, Mathf.Max(0, hit.distance - .001f));
                }
                transform.position = position + step.normalized * travel;
                return;
            }
        }
    }
}
