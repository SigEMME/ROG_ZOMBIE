using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    // No Combatant, HP or damage receiver: the obstacle cannot be destroyed by attacks.
    [RequireComponent(typeof(BoxCollider2D), typeof(TestObstacle))]
    public sealed class BarrierEffect : MonoBehaviour
    {
        public float Remaining { get; private set; }

        public void Initialize(float duration) => Remaining = duration;

        public void Tick(float delta) => Remaining = Mathf.Max(0, Remaining - delta);

        public void Remove()
        {
            // Disable immediately: Destroy alone would retain collision until end of frame.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        public void DisplaceOverlappingActors(Vector2 areaSize)
        {
            var box = GetComponent<BoxCollider2D>();
            Physics2D.SyncTransforms();
            var overlapping = Physics2D.OverlapBoxAll(transform.position, box.size, transform.eulerAngles.z);
            foreach (var body in overlapping)
            {
                if (body == box || body.isTrigger || !IsActor(body)) continue;
                Vector2 original = body.bounds.center;
                float radius = body is CircleCollider2D ? Mathf.Max(body.bounds.extents.x, body.bounds.extents.y) :
                    ((Vector2)body.bounds.extents).magnitude;
                Vector2 local = transform.InverseTransformPoint(original);
                Vector2 best = default;
                float bestDistance = float.PositiveInfinity;
                // Geometric search resolution, not a gameplay distance. Expand only if adjacent
                // positions are occupied by level geometry or other displaced actors.
                float step = Mathf.Max(radius, .05f);
                for (float extra = 0; extra <= areaSize.magnitude; extra += step)
                {
                    Vector2 extent = box.size * .5f + Vector2.one * (radius + .002f + extra);
                    Consider(new Vector2(-extent.x, Mathf.Clamp(local.y, -extent.y, extent.y)));
                    Consider(new Vector2(extent.x, Mathf.Clamp(local.y, -extent.y, extent.y)));
                    Consider(new Vector2(Mathf.Clamp(local.x, -extent.x, extent.x), -extent.y));
                    Consider(new Vector2(Mathf.Clamp(local.x, -extent.x, extent.x), extent.y));
                    for (float x = -extent.x; x <= extent.x; x += step)
                    { Consider(new Vector2(x, -extent.y)); Consider(new Vector2(x, extent.y)); }
                    for (float y = -extent.y; y <= extent.y; y += step)
                    { Consider(new Vector2(-extent.x, y)); Consider(new Vector2(extent.x, y)); }
                    if (!float.IsPositiveInfinity(bestDistance)) break;
                }
                if (float.IsPositiveInfinity(bestDistance))
                {
                    Debug.LogError("BARRIERA: nessuna posizione libera per espellere " + body.name + ". Verificare il level design.", this);
                    continue;
                }
                Vector2 displacement = best - original;
                if (body.attachedRigidbody != null) body.attachedRigidbody.position += displacement;
                else body.transform.position += (Vector3)displacement;
                Physics2D.SyncTransforms();

                void Consider(Vector2 candidateLocal)
                {
                    Vector2 candidate = transform.TransformPoint(candidateLocal);
                    if (Mathf.Abs(candidate.x) + radius >= areaSize.x * .5f ||
                        Mathf.Abs(candidate.y) + radius >= areaSize.y * .5f) return;
                    float distance = (candidate - original).sqrMagnitude;
                    if (distance >= bestDistance) return;
                    foreach (var other in Physics2D.OverlapCircleAll(candidate, radius))
                    {
                        if (other == body || other.isTrigger) continue;
                        if (other.GetComponent<TestObstacle>() != null || IsActor(other)) return;
                    }
                    best = candidate;
                    bestDistance = distance;
                }
            }
        }

        private static bool IsActor(Collider2D body)
        {
            var actor = body.GetComponent<Combatant>();
            if (actor != null) return actor.State != LifeState.Dead;
            return body.gameObject.layer == LayerMask.NameToLayer("PG") ||
                body.gameObject.layer == LayerMask.NameToLayer("MOB") ||
                body.gameObject.layer == LayerMask.NameToLayer("PET");
        }

        public static BarrierEffect Spawn(Vector2 centre, float angle, PG01AbilityCatalog data)
        {
            var go = TestVisuals.Box("BARRIERA PG01", centre, data.BarrierSize, new Color(.25f, .7f, .95f), 2);
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
            go.layer = LayerMask.NameToLayer("OSTACOLO");
            go.AddComponent<BoxCollider2D>().size = data.BarrierSize;
            go.AddComponent<TestObstacle>().IsWall = false;
            var effect = go.AddComponent<BarrierEffect>();
            effect.Initialize(data.BarrierDuration);
            Physics2D.SyncTransforms();
            return effect;
        }
    }
}
