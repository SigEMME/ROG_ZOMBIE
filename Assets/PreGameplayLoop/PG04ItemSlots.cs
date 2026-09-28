using UnityEngine;
using UnityEngine.InputSystem;
namespace RogZombie.PreGameplayLoop
{
    public enum PrototypeItem { None, Granata, Molotov, Smoke, PozioneCurativa, Trappola, BombaVelenosa, MinaElettrica }
    // Inventory shared by the seven prototype items; SCORTA changes only per-slot capacity.
    public sealed class PG04ItemSlots : MonoBehaviour
    {
        private PrototypeItem[] kinds;
        private int[] amounts;
        private bool scorta;
        public int SlotCount => kinds != null ? kinds.Length : 0;
        public bool PointerOverControls
        {
            get
            {
                return false; // The runtime inventory HUD is display-only.
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
