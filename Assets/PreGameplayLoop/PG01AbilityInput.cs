using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie.PreGameplayLoop
{
    // Only the directly controlled PG gets this adapter; selection/runtime do not depend on keys.
    [RequireComponent(typeof(PG01AbilityRuntime), typeof(PlayerAim))]
    public sealed class PG01AbilityInput : MonoBehaviour
    {
        private PG01AbilityRuntime ability;
        private PlayerAim aim;
        private void Awake() { ability = GetComponent<PG01AbilityRuntime>(); aim = GetComponent<PlayerAim>(); }

        private void LateUpdate()
        {
            var keyboard = Keyboard.current;
            if (!Application.isFocused || keyboard == null || !ability.CanUse ||
                !aim.TryGetCursorWorldPosition(out var cursor)) { ability.CancelAim(); return; }
            if (keyboard.qKey.wasPressedThisFrame)
            {
                if (ability.Selected == PG01Ability.Pestone) ability.TryPestone(cursor);
                else ability.BeginAim(cursor);
            }
            if (ability.IsAiming)
            {
                ability.AimAt(cursor);
                if (keyboard.qKey.wasReleasedThisFrame) ability.ReleaseAim(cursor);
                else if (!keyboard.qKey.isPressed) ability.CancelAim();
            }
        }

        private void OnApplicationFocus(bool focused) { if (!focused && ability != null) ability.CancelAim(); }
        private void OnDisable() { if (ability != null) ability.CancelAim(); }
    }
}
