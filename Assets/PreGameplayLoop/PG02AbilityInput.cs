using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie.PreGameplayLoop
{
    [RequireComponent(typeof(PG02AbilityRuntime), typeof(PlayerAim))]
    public sealed class PG02AbilityInput : MonoBehaviour
    {
        private PG02AbilityRuntime ability;
        private PlayerAim aim;
        private void Awake() { ability = GetComponent<PG02AbilityRuntime>(); aim = GetComponent<PlayerAim>(); }
        private void LateUpdate()
        {
            if (Application.isFocused && ability.CanUse && aim.TryGetCursorWorldPosition(out var currentCursor))
                ability.SetAimPoint(currentCursor);
            if (Application.isFocused && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame &&
                ability.CanUse && aim.TryGetCursorWorldPosition(out var cursor)) ability.TryActivate(cursor);
        }
    }
}
