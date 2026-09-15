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
        public static bool Held(Component caller)
        {
            var k = Keyboard.current; if (k == null) return false;
            return IsCompanion(caller) ? k.spaceKey.isPressed && k.digit1Key.isPressed : k.qKey.isPressed;
        }
        public static bool Pressed(Component caller)
        {
            var k = Keyboard.current; if (k == null) return false;
            return IsCompanion(caller) ? Held(caller) && (k.spaceKey.wasPressedThisFrame || k.digit1Key.wasPressedThisFrame) : k.qKey.wasPressedThisFrame;
        }
        public static bool Released(Component caller)
        {
            var k = Keyboard.current; if (k == null) return false;
            return IsCompanion(caller) ? (k.spaceKey.wasReleasedThisFrame && (k.digit1Key.isPressed || k.digit1Key.wasReleasedThisFrame)) || (k.digit1Key.wasReleasedThisFrame && k.spaceKey.isPressed) : k.qKey.wasReleasedThisFrame;
        }
    }
}
