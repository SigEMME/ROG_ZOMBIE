using System;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    // Run-owned inventory/timers survive AREA teardown; spawned objects belong to the AREA.
    public sealed class BonusAbilityRuntime : MonoBehaviour
    {
        private sealed class Timer { public float Cooldown, Active; }
        private readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        private readonly List<BonusMine> mines = new List<BonusMine>();
        private readonly List<BonusPet> pets = new List<BonusPet>();
        private LoopSession session;
        private Combatant actor;
        private BonusInventory inventory;
        private GameObject shieldVisual;
        private bool shield;
        public bool ShieldActive => shield;
        public bool FireActive => timers.TryGetValue("fire", out var t) && t.Active > 0;
        public int RicochetRolls { get; private set; }
        public int RicochetHits { get; private set; }
        public float Cooldown(string id) => timers.TryGetValue(id, out var t) ? t.Cooldown : 0;
        public float ActiveRemaining(string id) => timers.TryGetValue(id, out var t) ? t.Active : 0;
        public float Value(string id, string key) => inventory.Value(inventory.Find(id), key);
        public void Initialize(LoopSession owner, BonusInventory bonuses)
        {
            session = owner; actor = GetComponent<Combatant>(); inventory = bonuses;
            inventory.Changed += Changed;
            actor.BaseHitLanded += BaseHit;
            actor.BaseHitDamage = BaseDamage;
            actor.IncomingHitDamage = Mitigate;
            actor.StateChanged += StateChanged;
        }
        private float Cd(string id) => Mathf.Floor(Value(id, "CD") * 10 + .5f) / 10;
        private void Changed(BonusChoice choice)
        {
            if (choice.Bonus < 0 || choice.Upgrade >= 0) return;
            string id = inventory.Catalog.Bonuses[choice.Bonus].Id;
            timers[id] = new Timer { Cooldown = id == "pet" || id == "ricochet" ? 0 : Cd(id) };
        }
        private float BaseDamage(float attack) => FireActive ? attack * (1 + Value("fire", "ATK_PERCENT") / 100) : attack;
        private float Mitigate(float attack)
        {
            if (!shield || attack <= 0) return attack;
            float remaining = attack * (1 - Value("shield", "DEF") / 100);
            EndShield();
            return remaining;
        }
        private void EndShield()
        {
            if (!shield) return;
            shield = false; timers["shield"].Cooldown = Cd("shield");
            if (shieldVisual != null) Destroy(shieldVisual);
        }
        private void StateChanged(Combatant value)
        {
            if (value.IsActive) return;
            EndShield();
            EndFire();
        }
        private void EndFire()
        {
            if (!FireActive) return;
            timers["fire"].Active = 0; timers["fire"].Cooldown = Cd("fire");
        }
        private void Update()
        {
            if (session.GameplayRunning && actor.IsActive) Advance(Time.deltaTime);
        }
        public void Advance(float seconds)
        {
            if (seconds <= 0 || !actor.IsActive) return;
            EnsurePets();
            foreach (var pair in timers)
            {
                string id = pair.Key; var timer = pair.Value;
                if (id == "pet" || id == "ricochet" || id == "shield" && shield) continue;
                if (timer.Active > 0)
                {
                    timer.Active = Mathf.Max(0, timer.Active - seconds);
                    if (timer.Active == 0) timer.Cooldown = Cd(id);
                    continue;
                }
                timer.Cooldown = Mathf.Max(0, timer.Cooldown - seconds);
                if (timer.Cooldown > .00001f) continue;
                if (Activate(id) && id != "shield" && id != "fire") timer.Cooldown = Cd(id);
            }
        }
        private Vector2 AimDirection()
        {
            var aim = GetComponent<PlayerAim>();
            return aim != null && aim.TryGetCursorWorldPosition(out var cursor) && ((Vector2)cursor - (Vector2)transform.position).sqrMagnitude > .000001f
                ? ((Vector2)cursor - (Vector2)transform.position).normalized : (Vector2)transform.right;
        }
        private bool Activate(string id)
        {
            Vector2 center = transform.position, direction = AimDirection();
            switch (id)
            {
                case "knives":
                    int count = Mathf.RoundToInt(Value(id, "N_COLTELLI"));
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    for (int i = 0; i < count; i++)
                        TestVisuals.SpawnProjectile(actor, center, AttackGeometry.Direction(angle + KnifeAngle(i, count)),
                            20, Value(id, "RANGE"), Value(id, "DANNO"), .06f, 0, false);
                    return true;
                case "mines":
                    if (!BonusMine.TryPosition(center - direction, session.Settings.AreaSize, out var point)) return false;
                    var mine = TestVisuals.Circle("MINE", point, .22f, Color.yellow, 2);
                    mine.layer = LayerMask.NameToLayer("TRIGGER_MOB");
                    mine.AddComponent<CircleCollider2D>().isTrigger = true;
                    mine.GetComponent<CircleCollider2D>().radius = 1;
                    var placed = mine.AddComponent<BonusMine>();
                    placed.Initialize(session, actor, Value(id, "DANNO"), Value(id, "RAGGIO"), Value(id, "DURATA"));
                    mines.Add(placed);
                    return true;
                case "shield":
                    shield = true;
                    shieldVisual = TestVisuals.Circle("SCUDO", center, .65f, new Color(.2f,.8f,1,.3f), 4);
                    shieldVisual.transform.SetParent(transform, true);
                    return true;
                case "fire": timers[id].Active = Value(id, "DURATA"); return true;
                case "aura": Area(id, center, 8, Color.green); return true;
                case "taser": Area(id, center, 6, Color.cyan); return true;
                case "repulse": Area(id, center, 8, Color.magenta); return true;
                case "slash": Slash(center, direction); return true;
            }
            return false;
        }
        public static float KnifeAngle(int index, int count) => (index - (count - 1) * .5f) * 5;
        private void Area(string id, Vector2 center, int count, Color color)
        {
            Physics2D.SyncTransforms();
            float radius = Value(id, "RAGGIO");
            foreach (var target in Combatant.All.ToArray())
            {
                if (target == null || !target.IsActive || target.Faction != Faction.MOB ||
                    !AttackGeometry.InVisibleArea(center, radius, target)) continue;
                if (!target.Hit(Value(id, "DANNO"), true, actor) || !target.IsActive) continue;
                if (id == "taser") target.GetComponent<MobBrain>()?.Stun(Value(id, "BLOCK"));
                if (id == "repulse")
                {
                    var motion = new GameObject("REPULSE motion"); motion.transform.SetParent(TestVisuals.Root, false);
                    motion.AddComponent<PG08Push>().Initialize(session, target, (Vector2)target.transform.position - center, Value(id, "RESPINTA"), .25f);
                }
            }
            Flash(center, radius, null, color);
        }
        private void Slash(Vector2 center, Vector2 direction)
        {
            Physics2D.SyncTransforms();
            float radius = Value("slash", "RANGE");
            float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            foreach (var target in Combatant.All.ToArray())
                if (target != null && target.IsActive && target.Faction == Faction.MOB &&
                    AttackGeometry.InSemicircle(AttackGeometry.TargetCenter(target) - center, direction, radius) &&
                    AttackGeometry.InVisibleArea(center, radius, target))
                    target.Hit(Value("slash", "DANNO"), true, actor);
            Flash(center, radius, null, Color.white, rotation, 180);
        }
        internal static void Flash(Vector2 center, float radius, bool[] sectors, Color color, float rotation = 0, float sweep = 360)
        {
            var go = new GameObject("ABILITA BONUS area"); go.transform.SetParent(TestVisuals.Root, false);
            go.transform.position = center; color.a = .3f;
            go.AddComponent<PG04AreaVisual>().InitializeOccluded(center, radius, color, rotation, sweep); Destroy(go, .2f);
        }
        private void BaseHit(Combatant target, float damage)
        {
            if (FireActive && target.IsActive)
                (target.GetComponent<BonusBurn>() ?? target.gameObject.AddComponent<BonusBurn>())
                    .Apply(session, actor, Value("fire", "BRUCIATURA"), Value("fire", "DURATA_BRUCIATURA"));
            if (inventory.Find("ricochet") == null) return;
            RicochetRolls++;
            if (UnityEngine.Random.value < .4f) Bounce(target, damage);
        }
        public void Bounce(Combatant first, float originalDamage)
        {
            Combatant last = first;
            for (int i = 0; i < Mathf.RoundToInt(Value("ricochet", "RIMBALZI")); i++)
            {
                Combatant next = Closest(last.transform.position, Value("ricochet", "RANGE"), last);
                if (next == null) break;
                next.Hit(originalDamage * Value("ricochet", "DANNO") / 100, true, actor);
                TestVisuals.FlashCircle(next.transform.position, .3f, Color.yellow);
                RicochetHits++; last = next;
            }
        }
        internal static Combatant Closest(Vector2 center, float range, Combatant exclude = null)
        {
            Combatant selected = null; float best = range * range;
            foreach (var mob in Combatant.All)
            {
                if (mob == null || !mob.IsActive || mob.Faction != Faction.MOB || mob == exclude) continue;
                float distance = ((Vector2)mob.transform.position - center).sqrMagnitude;
                if (distance > best) continue;
                best = distance; selected = mob;
            }
            return selected;
        }
        private void EnsurePets()
        {
            pets.RemoveAll(pet => pet == null);
            if (inventory.Find("pet") == null) return;
            while (pets.Count < Mathf.RoundToInt(Value("pet", "N_PET")))
            {
                if (!PetPosition(out var position)) break;
                var go = TestVisuals.Circle("PET", position, session.Settings.ActorRadius, new Color(.8f,.5f,1), 3);
                go.layer = LayerMask.NameToLayer("PET"); go.AddComponent<CircleCollider2D>().radius = session.Settings.ActorRadius;
                var pet = go.AddComponent<BonusPet>(); pet.Initialize(session, this, actor); pets.Add(pet);
            }
        }
        private bool PetPosition(out Vector2 point)
        {
            point = transform.position;
            // Reuse the prototype actor radius, also used by the shared NavMesh.
            float radius = session.Settings.ActorRadius;
            for (int sample = 0; sample < 96; sample++)
            {
                Vector2 candidate = (Vector2)transform.position + AttackGeometry.Direction(sample * 137.508f) * (3 * Mathf.Sqrt(sample / 96f));
                if (!session.Navigation.Sample(candidate, out var valid, .3f) || Vector2.Distance(valid, transform.position) > 3) continue;
                bool occupied = false;
                foreach (var pet in pets) if (pet != null && Vector2.Distance(valid, pet.transform.position) < radius * 2 + .01f) { occupied = true; break; }
                if (occupied) continue;
                foreach (var hit in Physics2D.OverlapCircleAll(valid, radius))
                    if (hit.GetComponent<TestObstacle>() != null || hit.GetComponent<Combatant>() is Combatant mob && mob.Faction == Faction.MOB && mob.IsActive)
                    { occupied = true; break; }
                if (!occupied) { point = valid; return true; }
            }
            return false;
        }
        public void ChangeArea()
        {
            EndFire();
            if (mines.Exists(mine => mine != null && mine.gameObject.activeSelf)) timers["mines"].Cooldown = Cd("mines");
            foreach (var mine in mines) if (mine != null) { mine.gameObject.SetActive(false); Destroy(mine.gameObject); }
            mines.Clear();
            // Mines/projectiles/PET belong to the old AREA. Existing cooldowns and SCUDO persist.
            foreach (var pet in pets) if (pet != null) Destroy(pet.gameObject);
            pets.Clear();
        }
        public string Status()
        {
            var labels = new List<string>();
            foreach (var item in inventory.Owned)
            {
                var def = inventory.Catalog.Bonuses[item.DefinitionIndex];
                string state = def.Id == "shield" && shield || def.Id == "fire" && FireActive || def.Id == "pet" ? "ATTIVA" :
                    def.Id == "ricochet" ? "SU HIT" : $"CD {Cooldown(def.Id):0.0}s";
                labels.Add(def.Name + " — " + state);
            }
            return labels.Count == 0 ? "SLOT BONUS 0/3" : string.Join(" | ", labels);
        }
        private void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= Changed;
            if (actor != null)
            {
                actor.BaseHitLanded -= BaseHit; actor.StateChanged -= StateChanged;
                actor.BaseHitDamage = null; actor.IncomingHitDamage = null;
            }
        }
    }
}
