using UnityEngine;
using UnityEngine.InputSystem;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed class ItemRuntime : MonoBehaviour
    {
        private LoopSession session;
        private PG04ItemSlots slots;
        private PlayerAim aim;
        private GameObject preview;
        private float nextPreview;
        private int aiming = -1;
        public int AimingSlot => aiming;
        public bool CanUse => session != null && session.ParentSession == null && session.DirectlyControlled && session.GameplayRunning && session.Player.Actor.IsActive;
        public static string Label(PrototypeItem kind)
        {
            switch (kind)
            {
                case PrototypeItem.Granata: return "GRANATA";
                case PrototypeItem.Molotov: return "MOLOTOV";
                case PrototypeItem.Smoke: return "SMOKE";
                case PrototypeItem.PozioneCurativa: return "POZIONE CURATIVA";
                case PrototypeItem.Trappola: return "TRAPPOLA";
                case PrototypeItem.BombaVelenosa: return "BOMBA VELENOSA";
                case PrototypeItem.MinaElettrica: return "MINA ELETTRICA";
                default: return "VUOTO";
            }
        }
        public void Initialize(LoopSession owner, PG04ItemSlots inventory)
        {
            session = owner; slots = inventory; aim = GetComponent<PlayerAim>();
            gameObject.AddComponent<ItemSmokeStatus>();
        }
        public bool BeginAim(int slot, Vector2 cursor)
        {
            if (!CanUse || slot < 0 || slot >= slots.SlotCount || slots.Count(slot) <= 0) return false;
            CancelAim(); aiming = slot; nextPreview = 0; AimAt(cursor); return true;
        }
        public void AimAt(Vector2 cursor)
        {
            if (aiming < 0 || !CanUse) { CancelAim(); return; }
            var kind = slots.Kind(aiming);
            Vector2 position = Position(kind, transform.position, cursor);
            if (Time.unscaledTime < nextPreview) return;
            nextPreview = Time.unscaledTime + .05f;
            if (preview != null) Destroy(preview);
            preview = new GameObject("Anteprima ITEM"); preview.transform.SetParent(TestVisuals.Root, false); preview.transform.position = position;
            ItemArea.Draw(preview, kind, Angle(transform.position, cursor), true);
        }
        public bool ReleaseAim(Vector2 cursor)
        {
            int slot = aiming; CancelAim(); return Use(slot, cursor);
        }
        public bool Use(int slot, Vector2 cursor)
        {
            if (!CanUse || slot < 0 || slot >= slots.SlotCount || slots.Count(slot) <= 0) return false;
            var kind = slots.Kind(slot);
            if (!slots.TryConsume(slot)) return false;
            session.Player.Actor.NotifyAction();
            var go = new GameObject("ITEM " + Label(kind)); go.transform.SetParent(TestVisuals.Root, false);
            go.transform.position = Position(kind, transform.position, cursor);
            go.AddComponent<ItemArea>().Initialize(session, session.Player.Actor, kind, Angle(transform.position, cursor));
            return true;
        }
        public static Vector2 Position(PrototypeItem kind, Vector2 origin, Vector2 cursor) =>
            kind == PrototypeItem.Trappola || kind == PrototypeItem.MinaElettrica ? origin + Vector2.ClampMagnitude(cursor - origin, 7) : cursor;
        public static float Angle(Vector2 origin, Vector2 cursor) => Mathf.Atan2(cursor.y - origin.y, cursor.x - origin.x) * Mathf.Rad2Deg;
        public void CancelAim() { aiming = -1; if (preview != null) { Destroy(preview); preview = null; } }
        private void LateUpdate()
        {
            var k = Keyboard.current;
            if (!CanUse || !Application.isFocused || k == null || k.spaceKey.isPressed || TestHUD.PointerOverControls || !aim.TryGetCursorWorldPosition(out var cursor))
            { CancelAim(); return; }
            var keys = new[] { k.digit1Key, k.digit2Key, k.digit3Key, k.digit4Key };
            for (int i = 0; i < keys.Length; i++) if (keys[i].wasPressedThisFrame) BeginAim(i, cursor);
            if (aiming < 0) return;
            AimAt(cursor);
            if (aiming < 0) return;
            if (keys[aiming].wasReleasedThisFrame) ReleaseAim(cursor);
            else if (!keys[aiming].isPressed) CancelAim();
        }
        private void OnDisable() => CancelAim();
        private void OnApplicationFocus(bool focused) { if (!focused) CancelAim(); }
    }
}
