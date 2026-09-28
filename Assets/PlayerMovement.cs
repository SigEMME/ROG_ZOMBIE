using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Tooltip("PG asset supplying the base MOVE SPD.")]
        private PlayerDefinition playerDefinition;
        private TestEngine.Combatant combatant;
        public TestEngine.TestAreaSettings TestSettings;
        private Vector2 requestedVelocity;
        private int requestFrame = -1;
        // Preserve movement intent even when a companion blocks the actual displacement.
        public Vector2 RequestedVelocity => requestFrame == Time.frameCount && enabled ? requestedVelocity : Vector2.zero;

        public void Configure(PlayerDefinition definition)
        {
            playerDefinition = definition;
            combatant = GetComponent<TestEngine.Combatant>();
            enabled = definition != null;
        }

        private void Awake()
        {
            combatant = GetComponent<TestEngine.Combatant>();
            if (playerDefinition == null)
            {
                Debug.LogError("PlayerMovement requires a PG Definition asset.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            requestedVelocity = Vector2.zero; requestFrame = Time.frameCount;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || playerDefinition == null || !Application.isFocused)
                return;
            if (combatant != null && !combatant.IsActive) return;

            Vector2 input = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));

            float metresPerSecond = combatant != null ? combatant.EffectiveStats.MetresPerSecond : playerDefinition.BaseStats.MoveSpeedMetresPerSecond;
            if (TestSettings != null)
                metresPerSecond = (combatant != null ? combatant.EffectiveStats.MoveSpeed : playerDefinition.BaseStats.MoveSpeed) / 100f * TestSettings.PlayerMoveSpeedBase;
            if (combatant != null) metresPerSecond *= combatant.CurrentMovementMultiplier;
            requestedVelocity = Vector2.ClampMagnitude(input, 1f) * metresPerSecond;
            Vector2 displacement = requestedVelocity * Time.deltaTime;
            // World-space movement stays independent of the mouse-facing rotation.
            if (combatant != null) combatant.Move(displacement);
            else transform.position += new Vector3(displacement.x, displacement.y, 0f);
        }
    }
}
