using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Tooltip("PG asset supplying the base MOVE SPD.")]
        private PlayerDefinition playerDefinition;

        private void Awake()
        {
            if (playerDefinition == null)
            {
                Debug.LogError("PlayerMovement requires a PG Definition asset.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || playerDefinition == null || !Application.isFocused)
                return;

            Vector2 input = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));

            float metresPerSecond = playerDefinition.BaseStats.MoveSpeedMetresPerSecond;
            Vector2 displacement = Vector2.ClampMagnitude(input, 1f) * metresPerSecond * Time.deltaTime;
            // World-space movement stays independent of the mouse-facing rotation.
            transform.position += new Vector3(displacement.x, displacement.y, 0f);
        }
    }
}
