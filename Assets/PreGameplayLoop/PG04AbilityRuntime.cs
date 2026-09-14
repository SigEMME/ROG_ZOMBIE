using System;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG04AbilityRuntime : MonoBehaviour
    {
        private LoopSession session;
        private PG04AbilityCatalog data;
        private PlayerWeapon weapon;
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        private Vector2 rainCenter;
        private Vector2[] rainPositions;
        private float[] rainTimes;
        private float rainElapsed;
        private bool raining;
        private GameObject preview;
        private GameObject activeAreaMarker;
        public bool IsAiming => preview != null;
        private sealed class PendingBaseHit
        {
            public Vector2 Center;
            public float Radius, Damage, Remaining;
        }
        private readonly List<PendingBaseHit> pendingBaseHits = new List<PendingBaseHit>();
        public PG04Ability Selected { get; private set; }
        public PG04Passive Passive { get; private set; }
        public int Charges { get; private set; }
        public int ExplosionsEmitted { get; private set; }
        public bool EffectActive => raining || Charges > 0;
        public float ActiveRemaining => raining ? Mathf.Max(0, data.RainDuration - rainElapsed) : 0;
        public float CooldownRemaining => cooldown.Remaining;
        public bool CanUse => session != null && session.GameplayRunning && session.Player.Actor.IsActive;
        public string Label => Selected == PG04Ability.PioggiaDiGranate ? "PIOGGIA DI GRANATE" : "COLPO GROSSO";
        public string PassiveLabel => Passive == PG04Passive.ScortaEsplosiva ? "SCORTA ESPLOSIVA" : "PYROMANIA";
        public event Action<Vector2, float, float> Explosion;
        public void Initialize(LoopSession owner, PG04Ability selected, PG04Passive passive, PG04AbilityCatalog catalog)
        {
            session = owner; Selected = selected; Passive = passive; data = Instantiate(catalog);
            weapon = GetComponent<PlayerWeapon>(); weapon.BaseAttackOverride = FireBase;
            weapon.InputBlocked = () => session.PG04Items.PointerOverControls;
            cooldown.Restart(data.BaseCooldown(Selected), session.CdReduction);
        }
        private void Update()
        {
            if (!CanUse) { CancelAim(); if (session != null && session.Player != null && !session.Player.Actor.IsActive) ClearAreaMarker(); return; }
            for (int i = 0; i < pendingBaseHits.Count;)
            {
                var hit = pendingBaseHits[i];
                hit.Remaining -= Time.deltaTime;
                if (hit.Remaining > 0) { i++; continue; }
                pendingBaseHits.RemoveAt(i);
                EmitExplosion(hit.Center, hit.Radius, hit.Damage, true);
            }
            if (Selected != PG04Ability.ColpoGrosso || Charges == 0) cooldown.Tick(Time.deltaTime);
            if (!raining) return;
            rainElapsed += Time.deltaTime;
            while (ExplosionsEmitted < rainTimes.Length && rainElapsed >= rainTimes[ExplosionsEmitted])
            {
                EmitExplosion(rainPositions[ExplosionsEmitted], weapon.Definition.ExplosionRadius, session.Player.Actor.EffectiveStats.ATK);
                ExplosionsEmitted++;
            }
            if (rainElapsed >= data.RainDuration) { raining = false; ClearAreaMarker(); }
        }
        private void ClearAreaMarker()
        {
            if (activeAreaMarker != null) { activeAreaMarker.SetActive(false); Destroy(activeAreaMarker); }
            activeAreaMarker = null;
        }
        public bool BeginAim(Vector2 cursor)
        {
            if (Selected != PG04Ability.PioggiaDiGranate || !CanUse || !cooldown.Ready || EffectActive || IsAiming) return false;
            preview = new GameObject("Anteprima PIOGGIA DI GRANATE");
            preview.transform.SetParent(TestVisuals.Root, false);
            preview.AddComponent<PG04AreaVisual>().Initialize(data.RainRadius, new[] { true, true, true, true }, new Color(0, 1, 1, .25f));
            preview.GetComponent<MeshRenderer>().sortingOrder = 5;
            AimAt(cursor);
            return true;
        }
        public void AimAt(Vector2 cursor)
        {
            if (!IsAiming) return;
            if (!CanUse) { CancelAim(); return; }
            preview.transform.position = cursor;
        }
        public bool ReleaseAim(Vector2 cursor)
        {
            if (!IsAiming) return false;
            CancelAim();
            return TryActivate(cursor);
        }
        public void CancelAim()
        {
            if (preview != null) { preview.SetActive(false); Destroy(preview); }
            preview = null;
        }
        public bool TryActivate(Vector2 cursor)
        {
            if (!CanUse || !cooldown.Ready || EffectActive) return false;
            CancelAim();
            if (Selected == PG04Ability.ColpoGrosso) { Charges = data.BigShotCharges; return true; }
            cooldown.Restart(data.RainBaseCooldown, session.CdReduction);
            ClearAreaMarker();
            activeAreaMarker = PG04AreaVisual.CreateAreaMarker("Area attiva PIOGGIA DI GRANATE", cursor, data.RainRadius, new Color(1, .55f, .1f, .18f));
            rainCenter = cursor; rainElapsed = 0; ExplosionsEmitted = 0; raining = true;
            rainTimes = new float[data.RainCount]; rainPositions = new Vector2[data.RainCount];
            for (int i = 0; i < data.RainCount; i++)
            {
                rainTimes[i] = UnityEngine.Random.value * data.RainDuration;
                rainPositions[i] = rainCenter + UnityEngine.Random.insideUnitCircle * data.RainRadius;
            }
            Array.Sort(rainTimes);
            return true;
        }
        private void FireBase(Vector2 muzzle, Vector2 cursor, CombatStats stats)
        {
            // GDD RANGE is measured from PG; only MURO may anticipate the intended impact.
            Vector2 origin = transform.position;
            Vector2 offset = cursor - origin;
            Vector2 destination = origin + Vector2.ClampMagnitude(offset, stats.RangeMetres);
            Physics2D.SyncTransforms();
            destination = AttackGeometry.WallImpact(origin, destination);
            float radius = weapon.Definition.ExplosionRadius;
            float attack = stats.ATK;
            if (Charges > 0)
            {
                attack *= 1 + data.BigShotAttackPercent / 100f;
                radius *= 1 + data.BigShotRadiusPercent / 100f;
                Charges--;
                if (Charges == 0) cooldown.Restart(data.BigShotBaseCooldown, session.CdReduction);
            }
            // Snapshot the launched attack; targets and cover are evaluated at impact.
            pendingBaseHits.Add(new PendingBaseHit
            {
                Center = destination, Radius = radius, Damage = attack,
                Remaining = weapon.Definition.GrenadeHitDelay
            });
        }
        private void EmitExplosion(Vector2 center, float radius, float damage, bool baseAttack = false)
        {
            var valid = PG04ExplosionEffect.Sectors(center, radius);
            PG04ExplosionEffect.Hit(session.Player.Actor, center, radius, valid, damage, baseAttack);
            PG04ExplosionEffect.Flash(center, radius, valid);
            if (Passive == PG04Passive.Pyromania)
            {
                var go = new GameObject("PYROMANIA");
                go.transform.SetParent(TestVisuals.Root, false); go.transform.position = center;
                go.AddComponent<PG04BurningArea>().Initialize(session, session.Player.Actor, radius, data.FireDuration, data.FireDamage, valid);
            }
            Explosion?.Invoke(center, radius, damage);
        }
        public void ChangeArea()
        {
            CancelAim(); ClearAreaMarker();
            cooldown.ChangeArea(EffectActive, data.BaseCooldown(Selected), session.CdReduction);
            Charges = 0; raining = false; rainTimes = null; rainPositions = null;
            pendingBaseHits.Clear();
        }
        private void OnDisable() { CancelAim(); ClearAreaMarker(); }
        private void OnDestroy() { CancelAim(); ClearAreaMarker(); if (data != null) Destroy(data); }
    }
}
