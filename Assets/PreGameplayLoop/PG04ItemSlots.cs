using UnityEngine;
using UnityEngine.InputSystem;
namespace RogZombie.PreGameplayLoop
{
    public enum PrototypeItem { None, Granata, Molotov, Smoke, PozioneCurativa, Trappola }
    // Minimal slot storage for testing SCORTA ESPLOSIVA. No merchant or item combat is simulated.
    public sealed class PG04ItemSlots : MonoBehaviour
    {
        private PrototypeItem[] kinds;
        private int[] amounts;
        private bool scorta;
        public int SlotCount => kinds.Length;
        public bool PointerOverControls
        {
            get
            {
                if (Mouse.current == null) return false;
                Vector2 pointer = Mouse.current.position.ReadValue();
                return pointer.y >= 110 && pointer.y <= 185 && pointer.x >= 20 && pointer.x <= 20 + SlotCount * 230;
            }
        }
        public void Initialize(int slots, PG04Passive passive)
        {
            kinds = new PrototypeItem[slots]; amounts = new int[slots];
            scorta = passive == PG04Passive.ScortaEsplosiva;
        }
        public int Capacity(PrototypeItem kind) => scorta && (kind == PrototypeItem.Granata || kind == PrototypeItem.Molotov) ? 3 : 1;
        public PrototypeItem Kind(int slot) => kinds[slot];
        public int Count(int slot) => amounts[slot];
        public bool TryAdd(int slot, PrototypeItem kind)
        {
            if (slot < 0 || slot >= SlotCount || kind == PrototypeItem.None || !System.Enum.IsDefined(typeof(PrototypeItem), kind) ||
                (amounts[slot] > 0 && kinds[slot] != kind) || amounts[slot] >= Capacity(kind)) return false;
            kinds[slot] = kind; amounts[slot]++; return true;
        }
        public bool TryConsume(int slot)
        {
            if (slot < 0 || slot >= SlotCount || amounts[slot] == 0) return false;
            if (--amounts[slot] == 0) kinds[slot] = PrototypeItem.None;
            return true;
        }
    }
}
