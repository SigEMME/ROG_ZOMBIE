using UnityEngine;
using UnityEngine.InputSystem;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    [RequireComponent(typeof(PG04AbilityRuntime), typeof(PlayerAim))]
    public sealed class PG04AbilityInput : MonoBehaviour
    {
        private PG04AbilityRuntime ability;
        private PlayerAim aim;
        private void Awake() { ability = GetComponent<PG04AbilityRuntime>(); aim = GetComponent<PlayerAim>(); }
        private void LateUpdate()
        {
            var keyboard = Keyboard.current;
            if (!Application.isFocused || TestHUD.PointerOverControls || keyboard == null || !ability.CanUse ||
                !aim.TryGetCursorWorldPosition(out var cursor)) { ability.CancelAim(); return; }
            if (keyboard.qKey.wasPressedThisFrame)
            {
                if (ability.Selected == PG04Ability.PioggiaDiGranate) ability.BeginAim(cursor);
                else ability.TryActivate(cursor);
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
