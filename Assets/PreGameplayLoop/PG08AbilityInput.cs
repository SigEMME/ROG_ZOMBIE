using UnityEngine;
using UnityEngine.InputSystem;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    [DefaultExecutionOrder(-10)]
    public sealed class PG08AbilityInput : MonoBehaviour
    {
        private PG08AbilityRuntime ability;
        private PG08PassiveRuntime passive;
        private PlayerAim aim;
        private void Awake() { ability = GetComponent<PG08AbilityRuntime>(); passive = GetComponent<PG08PassiveRuntime>(); aim = GetComponent<PlayerAim>(); }
        private void Update()
        {
            bool held = Application.isFocused && !TestHUD.PointerOverControls && Mouse.current != null && Mouse.current.leftButton.isPressed;
            if (!held) passive.SetFireHeld(false);
            if (!ability.CanUse) return;
            passive.SetFireHeld(held && aim.TryGetCursorWorldPosition(out _));
            if (Application.isFocused && !TestHUD.PointerOverControls && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
                ability.TryActivate();
        }
        private void OnApplicationFocus(bool focus) { if (!focus && passive != null) passive.SetFireHeld(false); }
        private void OnDisable() { if (passive != null) passive.SetFireHeld(false); }
    }
}
