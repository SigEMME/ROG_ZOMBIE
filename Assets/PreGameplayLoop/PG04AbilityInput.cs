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
            if (Application.isFocused && !TestHUD.PointerOverControls && Keyboard.current != null &&
                Keyboard.current.qKey.wasPressedThisFrame && ability.CanUse && aim.TryGetCursorWorldPosition(out var cursor))
                ability.TryActivate(cursor);
        }
    }
}
