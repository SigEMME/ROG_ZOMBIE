using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie
{
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField, Tooltip("Show the aim direction and projected cursor in the Scene view.")]
        private bool showAimGizmos;

        public Vector3 CursorWorldPosition { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public bool HasAimPoint { get; private set; }

        private void Awake()
        {
            if (aimCamera == null)
                aimCamera = Camera.main;

            if (aimCamera == null)
            {
                Debug.LogError("PlayerAim requires a camera.", this);
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            var actor = GetComponent<TestEngine.Combatant>();
            if (actor != null && !actor.IsActive) { HasAimPoint = false; return; }
            if (Time.timeScale == 0f) return;
            HasAimPoint = TryGetCursorWorldPosition(out Vector3 cursorPosition);
            if (!HasAimPoint)
                return;

            CursorWorldPosition = cursorPosition;
            Vector2 direction = cursorPosition - transform.position;
            if (direction.sqrMagnitude < 0.000001f)
                return;

            AimDirection = direction.normalized;
            // The temporary sprite faces local +X.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Query at firing time to avoid using the previous frame's cursor position.
        // A weapon must calculate its shot direction from its own muzzle to this point.
        public bool TryGetCursorWorldPosition(out Vector3 cursorPosition)
        {
            cursorPosition = default;
            Mouse mouse = Mouse.current;
            if (mouse == null || aimCamera == null || !Application.isFocused)
                return false;

            Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
            Plane playerPlane = new Plane(Vector3.forward, transform.position);
            if (!playerPlane.Raycast(ray, out float distance))
                return false;

            cursorPosition = ray.GetPoint(distance);
            return true;
        }

        private void OnDrawGizmos()
        {
            if (!showAimGizmos || !Application.isPlaying || !HasAimPoint)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, CursorWorldPosition);
            Gizmos.DrawWireSphere(CursorWorldPosition, 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, AimDirection);
        }
    }
}
