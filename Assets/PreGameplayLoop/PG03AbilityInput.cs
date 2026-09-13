using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie.PreGameplayLoop
{
    [RequireComponent(typeof(PG03AbilityRuntime), typeof(PlayerAim))]
    public sealed class PG03AbilityInput : MonoBehaviour
    {
        private PG03AbilityRuntime ability;
        private PlayerAim aim;
        private void Awake() { ability = GetComponent<PG03AbilityRuntime>(); aim = GetComponent<PlayerAim>(); }
        private void LateUpdate()
        {
            if (Application.isFocused && ability.CanUse && aim.TryGetCursorWorldPosition(out var currentCursor))
                ability.SetAimPoint(currentCursor);
            if (Application.isFocused && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame &&
                ability.CanUse && aim.TryGetCursorWorldPosition(out var cursor)) ability.TryActivate(cursor);
        }
    }
}
