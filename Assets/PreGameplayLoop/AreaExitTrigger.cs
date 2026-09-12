using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class AreaExitTrigger : MonoBehaviour
    {
        public const float Radius = 4;
        public bool Available { get; private set; }
        public System.Action Entered;
        private Combatant player;
        private bool consumed;
        private Material ringMaterial;

        public void Initialize(Combatant value)
        {
            player = value;
            gameObject.layer = LayerMask.NameToLayer("TRIGGER_PG");
            var trigger = GetComponent<CircleCollider2D>();
            trigger.radius = Radius;
            trigger.isTrigger = true;
            trigger.enabled = false;
            var ring = gameObject.AddComponent<LineRenderer>();
            ringMaterial = new Material(Shader.Find("Sprites/Default"));
            ring.sharedMaterial = ringMaterial;
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 64;
            ring.startWidth = ring.endWidth = .06f;
            ring.startColor = ring.endColor = Color.gray;
            ring.sortingOrder = 2;
            for (int i = 0; i < 64; i++)
            {
                float angle = i * 2 * Mathf.PI / 64;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * Radius);
            }
        }

        public void Open()
        {
            Available = true;
            GetComponent<CircleCollider2D>().enabled = true;
            GetComponent<SpriteRenderer>().color = Color.green;
            var ring = GetComponent<LineRenderer>();
            ring.startColor = ring.endColor = Color.green;
        }

        public bool ContainsActivePlayer() => player != null && player.IsActive &&
            ((Vector2)(player.transform.position - transform.position)).sqrMagnitude <= Radius * Radius;

        private void Update()
        {
            // Existing actors move using swept queries, without dynamic Rigidbodies.
            // Explicit logical detection also handles already standing inside when opened.
            if (!Available || consumed || Time.timeScale <= 0 || !ContainsActivePlayer()) return;
            consumed = true;
            Entered?.Invoke();
        }

        private void OnDestroy() { if (ringMaterial != null) Destroy(ringMaterial); }
    }
}
