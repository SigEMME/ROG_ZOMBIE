using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed class PG01AbilityRuntime : MonoBehaviour
    {
        private PG01Ability selected;
        private PG01AbilityCatalog data;
        private LoopSession session;
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        private readonly List<BarrierEffect> barriers = new List<BarrierEffect>();
        private readonly List<Collider2D> placementOverlaps = new List<Collider2D>();
        private readonly BarrierGeometry geometry = new BarrierGeometry();
        private GameObject preview;
        private Vector2 previewCentre;
        private float previewAngle;

        public PG01Ability Selected => selected;
        public float CooldownRemaining => cooldown.Remaining;
        public float FinalCooldown => AbilityCooldown.FinalSeconds(data.BaseCooldown(selected), session.CdReduction);
        public bool IsAiming => preview != null;
        public bool PreviewValid { get; private set; }
        public Vector2 PreviewCentre => previewCentre;
        public float PreviewAngle => previewAngle;
        public bool EffectActive => barriers.Exists(b => b != null && b.isActiveAndEnabled && b.Remaining > 0);
        public IReadOnlyList<BarrierEffect> Barriers => barriers;
        public bool CanUse => session != null && session.GameplayRunning && session.Player.Actor.IsActive;

        public void Initialize(LoopSession owner, PG01Ability choice, PG01AbilityCatalog catalog)
        {
            session = owner;
            selected = choice;
            // Selection and numerical data are a RUN snapshot, independent of Inspector edits mid-RUN.
            data = Instantiate(catalog);
            cooldown.Restart(data.BaseCooldown(selected), session.CdReduction);
        }

        private void Update()
        {
            if (!CanUse) { CancelAim(); return; }
            cooldown.Tick(Time.deltaTime);
            bool geometryChanged = false;
            for (int i = barriers.Count - 1; i >= 0; i--)
            {
                var barrier = barriers[i];
                if (barrier != null) barrier.Tick(Time.deltaTime);
                if (barrier != null && barrier.Remaining > 0) continue;
                if (barrier != null) barrier.Remove();
                barriers.RemoveAt(i);
                geometryChanged = true;
            }
            if (geometryChanged) session.RefreshNavigation();
        }

        public bool TryPestone(Vector2 cursor)
        {
            if (selected != PG01Ability.Pestone || !CanUse || !cooldown.Ready) return false;
            Vector2 origin = transform.position;
            Vector2 direction = (cursor - origin).normalized;
            if (direction == Vector2.zero) direction = transform.right;
            cooldown.Restart(data.PestoneBaseCooldown, session.CdReduction);
            PestoneEffect.Cast(origin, direction, data, session.Player.Actor);
            TestVisuals.FlashRectangle(origin, direction, data.PestoneDepth, data.PestoneWidth);
            return true;
        }

        public bool BeginAim(Vector2 cursor)
        {
            if (selected != PG01Ability.Barriera || !CanUse || !cooldown.Ready || IsAiming) return false;
            preview = new GameObject("Anteprima BARRIERA"); preview.transform.SetParent(TestVisuals.Root, false);
            preview.AddComponent<PG04AreaVisual>();
            // Presentation only; no Collider, TestObstacle or navigation changes until release.
            AimAt(cursor);
            return true;
        }

        public void AimAt(Vector2 cursor)
        {
            if (!IsAiming) return;
            if (!CanUse) { CancelAim(); return; }
            Vector2 origin = transform.position;
            Vector2 offset = cursor - origin;
            Vector2 heading = offset.sqrMagnitude > 0 ? offset.normalized : (Vector2)transform.right;
            previewCentre = origin + Vector2.ClampMagnitude(offset, data.BarrierPlacementRange);
            previewAngle = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg + 90;
            preview.transform.SetPositionAndRotation(previewCentre, Quaternion.Euler(0, 0, previewAngle));
            Physics2D.SyncTransforms();
            var filter = ContactFilter2D.noFilter;
            filter.SetLayerMask(LayerMask.GetMask("PG"));
            filter.useTriggers = false;
            Physics2D.OverlapBox(previewCentre, data.BarrierSize, previewAngle, filter, placementOverlaps);
            geometry.Build(previewCentre, data.BarrierSize, previewAngle);
            PreviewValid = geometry.HasArea;
            foreach (var body in placementOverlaps)
            {
                Vector2 local = preview.transform.InverseTransformPoint(body.bounds.center);
                float radius = Mathf.Max(body.bounds.extents.x, body.bounds.extents.y);
                if (geometry.OverlapsCircle(local, radius)) { PreviewValid = false; break; }
            }
            preview.GetComponent<PG04AreaVisual>().InitializePolygon(geometry.Points,
                PreviewValid ? new Color(0, 1, 1, .4f) : new Color(1, .1f, .1f, .4f), 5);
            var outline = preview.GetComponent<LineRenderer>();
            if (!geometry.HasArea)
            {
                if (outline == null)
                {
                    outline = preview.AddComponent<LineRenderer>(); outline.useWorldSpace = false;
                    outline.positionCount = 4; outline.loop = true; outline.startWidth = outline.endWidth = .025f;
                    outline.sharedMaterial = preview.GetComponent<MeshRenderer>().sharedMaterial; outline.sortingOrder = 5;
                }
                Vector2 half = data.BarrierSize * .5f;
                outline.SetPosition(0, new Vector2(-half.x,-half.y)); outline.SetPosition(1,new Vector2(half.x,-half.y));
                outline.SetPosition(2,new Vector2(half.x,half.y)); outline.SetPosition(3,new Vector2(-half.x,half.y));
            }
            if (outline != null) outline.enabled = !geometry.HasArea;
        }

        public bool ReleaseAim(Vector2 cursor)
        {
            if (!IsAiming) return false;
            AimAt(cursor); // Use the current cursor and PG position at release.
            if (!IsAiming || !CanUse || !cooldown.Ready || !PreviewValid) { CancelAim(); return false; }
            var barrier = BarrierEffect.Spawn(previewCentre, previewAngle, data, geometry);
            barriers.Add(barrier);
            cooldown.Restart(data.BarrierBaseCooldown, session.CdReduction);
            CancelAim();
            barrier.DisplaceOverlappingActors(session.Settings.AreaSize);
            session.RefreshNavigation();
            return true;
        }

        public void CancelAim()
        {
            if (preview != null) { preview.SetActive(false); Destroy(preview); }
            preview = null;
            PreviewValid = false;
        }

        public void ChangeArea()
        {
            cooldown.ChangeArea(EffectActive, data.BaseCooldown(selected), session.CdReduction);
            CancelAim();
            foreach (var barrier in barriers) if (barrier != null) barrier.Remove();
            barriers.Clear();
        }

        private void OnDisable() => CancelAim();
        private void OnDestroy()
        {
            CancelAim();
            foreach (var barrier in barriers) if (barrier != null) barrier.Remove();
            if (data != null) Destroy(data);
        }
    }
}
