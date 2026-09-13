using UnityEngine;
using UnityEngine.InputSystem;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    [RequireComponent(typeof(PG06AbilityRuntime), typeof(PlayerAim))]
    public sealed class PG06AbilityInput : MonoBehaviour
    {
        private PG06AbilityRuntime ability;
        private PlayerAim aim;
        private void Awake() { ability = GetComponent<PG06AbilityRuntime>(); aim = GetComponent<PlayerAim>(); }
        private void LateUpdate()
        {
            if (Application.isFocused && !TestHUD.PointerOverControls && Keyboard.current != null &&
                Keyboard.current.qKey.wasPressedThisFrame && ability.CanUse && aim.TryGetCursorWorldPosition(out var cursor))
                ability.TryActivate(cursor);
        }
    }
}
