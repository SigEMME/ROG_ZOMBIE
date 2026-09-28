using UnityEngine;
using UnityEngine.InputSystem;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PartyCommands : MonoBehaviour
    {
        public LoopSession Context;
        private static bool IsCompanion(Component caller)
        {
            var commands = caller.GetComponent<PartyCommands>();
            return commands != null && commands.Context != null && !commands.Context.DirectlyControlled;
        }
        private static UnityEngine.InputSystem.Controls.KeyControl AbilityKey(Component caller, Keyboard keyboard)
        {
            var context = caller.GetComponent<PartyCommands>()?.Context;
            int slot = context != null ? context.PartySlot : 1;
            return slot == 2 ? keyboard.digit2Key : slot == 3 ? keyboard.digit3Key : keyboard.digit1Key;
        }
        public static bool Held(Component caller)
        {
            var k = Keyboard.current; if (k == null) return false;
            return IsCompanion(caller) ? k.spaceKey.isPressed && AbilityKey(caller, k).isPressed : k.qKey.isPressed;
        }
        public static bool Pressed(Component caller)
        {
            var k = Keyboard.current; if (k == null) return false;
            return IsCompanion(caller) ? Held(caller) && (k.spaceKey.wasPressedThisFrame || AbilityKey(caller, k).wasPressedThisFrame) : k.qKey.wasPressedThisFrame;
        }
        public static bool Released(Component caller)
        {
            var k = Keyboard.current; if (k == null) return false;
            return IsCompanion(caller) ? (k.spaceKey.wasReleasedThisFrame && (AbilityKey(caller, k).isPressed || AbilityKey(caller, k).wasReleasedThisFrame)) || (AbilityKey(caller, k).wasReleasedThisFrame && k.spaceKey.isPressed) : k.qKey.wasReleasedThisFrame;
        }
    }
}
