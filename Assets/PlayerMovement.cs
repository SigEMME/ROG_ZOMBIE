using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("MOVE SPD: 100 = 2 m/s. One Unity unit = one metre.")]
        private float moveSpeed = 100f;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !Application.isFocused)
                return;

            Vector2 input = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));

            Vector2 displacement = Vector2.ClampMagnitude(input, 1f) * (moveSpeed * 0.02f) * Time.deltaTime;
            // World-space movement stays independent of the mouse-facing rotation.
            transform.position += new Vector3(displacement.x, displacement.y, 0f);
        }
    }
}
