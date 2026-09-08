using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie
{
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;

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
            Mouse mouse = Mouse.current;
            if (mouse == null || aimCamera == null || !Application.isFocused)
                return;

            // Resolve the cursor on the PG's XY plane after movement has finished.
            Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
            Plane playerPlane = new Plane(Vector3.forward, transform.position);
            if (!playerPlane.Raycast(ray, out float distance))
                return;

            Vector2 direction = ray.GetPoint(distance) - transform.position;
            if (direction.sqrMagnitude < 0.000001f)
                return;

            // The temporary sprite faces local +X.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
