using UnityEngine;
using UnityEngine.InputSystem;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    [RequireComponent(typeof(PG05AbilityRuntime), typeof(PlayerAim))]
    public sealed class PG05AbilityInput : MonoBehaviour
    {
        private PG05AbilityRuntime ability;
        private PlayerAim aim;
        private void Awake() { ability = GetComponent<PG05AbilityRuntime>(); aim = GetComponent<PlayerAim>(); }
        private void LateUpdate()
        {
            if (Application.isFocused && !TestHUD.PointerOverControls && Keyboard.current != null &&
                Keyboard.current.qKey.wasPressedThisFrame && ability.CanUse && aim.TryGetCursorWorldPosition(out var cursor))
                ability.TryActivate(cursor);
        }
    }
}
