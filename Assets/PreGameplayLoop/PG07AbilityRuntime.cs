using System;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class PG07AbilityRuntime : MonoBehaviour
    {
        private LoopSession session;
        private PG07AbilityCatalog data;
        private PlayerWeapon weapon;
        private Combatant actor;
        private readonly AbilityCooldown cooldown = new AbilityCooldown();
        private Vector2[] rainPoints;
        private float elapsed;
        private GameObject preview;
        private GameObject activeAreaMarker;
        public bool IsAiming => preview != null;
        public PG07Ability Selected { get; private set; }
        public PG07Passive Passive { get; private set; }
        public int RainEmitted { get; private set; }
        public int PassiveRolls { get; private set; }
        public int PassiveTriggers { get; private set; }
        public bool EffectActive => rainPoints != null && RainEmitted < rainPoints.Length;
        public float CooldownRemaining => cooldown.Remaining;
        public bool CanUse => session != null && session.GameplayRunning && actor.IsActive;
        public string Label => Selected == PG07Ability.MultiShot ? "MULTI SHOT" : "PIOGGIA DI FRECCE";
        public string PassiveLabel => Passive == PG07Passive.LuckyShot ? "LUCKY SHOT" : "CONCENTRAZIONE";
        public event Action<Projectile, bool> BaseShot;
        public event Action<Projectile> MultiShot;
        public event Action<Vector2, float> RainImpact;
        public void Initialize(LoopSession owner, PG07Ability choice, PG07Passive passive, PG07AbilityCatalog catalog)
        {
            session = owner; Selected = choice; Passive = passive; data = Instantiate(catalog);
            actor = GetComponent<Combatant>(); weapon = GetComponent<PlayerWeapon>(); weapon.BaseAttackOverride = FireBase;
            cooldown.Restart(data.BaseCooldown(Selected), session.CdReduction);
        }
        private bool Roll(float chance)
        { PassiveRolls++; bool result = UnityEngine.Random.value < chance; if (result) PassiveTriggers++; return result; }
        private void FireBase(Vector2 origin, Vector2 cursor, CombatStats stats)
        {
            bool piercing = Passive == PG07Passive.Concentrazione && Roll(data.ConcentrationChance);
            var shot = TestVisuals.SpawnProjectile(actor, origin, (cursor - origin).normalized, weapon.Definition.ProjectileSpeed,
                stats.RangeMetres, stats.ATK, weapon.Definition.ProjectileRadius, piercing ? 2 : 0, false);
            shot.HitEffect = new BaseHit(this, piercing, weapon.BaseProjectileEffect);
            BaseShot?.Invoke(shot, piercing);
        }
        private sealed class BaseHit : IProjectileHitEffect
        {
            private readonly PG07AbilityRuntime owner;
            private readonly bool piercing;
            private readonly IProjectileHitEffect extra;
            public BaseHit(PG07AbilityRuntime value, bool pierce, IProjectileHitEffect additional) { owner = value; piercing = pierce; extra = additional; }
            public void ResolveHit(Combatant target, float damage, bool round, Combatant source, int index)
            {
                Vector2 point = target.transform.position;
                float hitDamage = piercing && index > 0 ? Mathf.Floor(damage * .5f + .5f) : damage;
                bool valid = target.IsActive;
                if (extra != null) extra.ResolveHit(target, hitDamage, true, source, index);
                else valid = target.Hit(hitDamage, true, source, true);
                if (valid && owner != null && owner.Passive == PG07Passive.LuckyShot && owner.Roll(owner.data.LuckyChance)) owner.Lucky(point);
            }
        }
        private void Lucky(Vector2 center)
        {
            float damage = Mathf.Floor(actor.EffectiveStats.ATK * data.LuckyPercent / 100f + .5f);
            Physics2D.SyncTransforms();
            foreach (var target in Combatant.All.ToArray())
                if (target != null && target.IsActive && target.Faction == Faction.MOB &&
                    AttackGeometry.InVisibleArea(center, data.LuckyRadius, target)) target.Hit(damage, true, actor);
            var go = new GameObject("LUCKY SHOT"); go.transform.SetParent(TestVisuals.Root, false); go.transform.position = center;
            go.AddComponent<PG04AreaVisual>().InitializeOccluded(center, data.LuckyRadius, new Color(1, .9f, .1f, .4f)); Destroy(go, .15f);
        }
        private void Update()
        {
            if (!CanUse) { CancelAim(); if (session != null && session.Player != null && !session.Player.Actor.IsActive) ClearAreaMarker(); return; }
            cooldown.Tick(Time.deltaTime);
            if (!EffectActive) return;
            elapsed += Time.deltaTime;
            while (RainEmitted < rainPoints.Length && elapsed >= RainEmitted * data.RainDuration / (data.RainCount - 1)) EmitRain();
            if (!EffectActive) ClearAreaMarker();
        }
        private void ClearAreaMarker()
        {
            if (activeAreaMarker != null) { activeAreaMarker.SetActive(false); Destroy(activeAreaMarker); }
            activeAreaMarker = null;
        }
        public bool BeginAim(Vector2 cursor)
        {
            if (Selected != PG07Ability.PioggiaDiFrecce || !CanUse || !cooldown.Ready || EffectActive || IsAiming) return false;
            preview = new GameObject("Anteprima PIOGGIA DI FRECCE");
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
            actor.NotifyAction(); cooldown.Restart(data.BaseCooldown(Selected), session.CdReduction);
            if (Selected == PG07Ability.MultiShot)
            {
                var go = new GameObject("MULTI SHOT volley"); go.transform.SetParent(TestVisuals.Root, false);
                var volley = go.AddComponent<PG07MultiVolley>(); volley.Initialize(data.MultiPush, data.MultiPushDuration);
                Vector2 origin = weapon.Muzzle.position, direction = (cursor - origin).normalized;
                if (direction == Vector2.zero) direction = transform.right;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                for (int i = 0; i < data.MultiCount; i++)
                {
                    var shot = TestVisuals.SpawnProjectile(actor, origin, AttackGeometry.Direction(angle - data.MultiAngle * .5f + i * data.MultiAngle / (data.MultiCount - 1)),
                        data.MultiSpeed, data.MultiRange, data.MultiDamage, weapon.Definition.ProjectileRadius, 0, false);
                    shot.HitEffect = volley.ForShot(shot); MultiShot?.Invoke(shot);
                }
                Destroy(go, data.MultiRange / data.MultiSpeed + data.MultiPushDuration + .2f);
            }
            else
            {
                ClearAreaMarker();
                activeAreaMarker = PG04AreaVisual.CreateAreaMarker("Area attiva PIOGGIA DI FRECCE", cursor, data.RainRadius, new Color(1, .85f, .1f, .18f));
                rainPoints = new Vector2[data.RainCount];
                for (int i = 0; i < rainPoints.Length; i++) rainPoints[i] = cursor + UnityEngine.Random.insideUnitCircle * data.RainRadius;
                elapsed = 0; RainEmitted = 0; EmitRain();
            }
            return true;
        }
        private void EmitRain()
        {
            Vector2 point = rainPoints[RainEmitted++];
            HitRainPoint(point);
            var visual = TestVisuals.Box("PIOGGIA DI FRECCE impact", point + Vector2.up * .25f, new Vector2(.1f, .5f), Color.yellow, 4);
            Destroy(visual, .15f);
            RainImpact?.Invoke(point, (RainEmitted - 1) * data.RainDuration / (data.RainCount - 1));
        }
        public Combatant HitRainPoint(Vector2 point)
        {
            Physics2D.SyncTransforms(); Combatant chosen = null;
            foreach (var target in Combatant.All.ToArray())
            {
                if (target == null || !target.IsActive || target.Faction != Faction.MOB) continue;
                var collider = target.GetComponent<CircleCollider2D>();
                if (collider == null || !collider.enabled ||
                    (collider.ClosestPoint(point) - point).sqrMagnitude > data.RainHitRadius * data.RainHitRadius) continue;
                if (target.Hit(data.RainDamage, true, actor) && chosen == null) chosen = target;
            }
            TestVisuals.FlashCircle(point, data.RainHitRadius, new Color(1, .8f, .1f));
            return chosen;
        }
        public void ChangeArea()
        { CancelAim(); ClearAreaMarker(); cooldown.ChangeArea(EffectActive, data.BaseCooldown(Selected), session.CdReduction); rainPoints = null; RainEmitted = 0; }
        private void OnDisable() { CancelAim(); ClearAreaMarker(); }
        private void OnDestroy() { CancelAim(); ClearAreaMarker(); if (data != null) Destroy(data); }
    }
}
