using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
using RogZombie.PreGameplayLoop;
namespace RogZombie.BossTest
{
    public sealed class BossBrain : MonoBehaviour, IProjectileHitEffect
    {
        public Combatant Actor { get; private set; }
        public string Status { get; private set; } = "Inseguimento";
        public float SpecialRemaining => Mathf.Max(0, specialAt - Time.time);
        private BossDefinition data;
        private LoopSession session;
        private Combatant target;
        private float specialAt, baseAt, wanderAt;
        private Vector2 wander;
        private int cycle, threshold, activeKind = -1; private float stunnedUntil;
        private bool busy, pendingCharge, rocksPending;
        private readonly List<BossArea> areas = new List<BossArea>();
        public void Initialize(LoopSession owner, BossDefinition definition)
        { session = owner; data = definition; Actor = GetComponent<Combatant>(); specialAt = Time.time + data.SpecialInterval;
            var facing = TestVisuals.Box("Direzione BOSS", Vector2.zero, new Vector2(.7f, .3f), Color.yellow, 5);
            facing.transform.SetParent(transform, false); facing.transform.localPosition = Vector3.right * Actor.Radius;
        }
        public void StartFightTimer()
        {
            specialAt = Time.time + data.SpecialInterval;
            baseAt = Time.time;
        }
        private List<Combatant> Targets(bool includeInvisible = false)
        {
            var list = new List<Combatant>();
            foreach (var member in session.Members)
                if (member.Player != null && member.Player.Actor.IsActive && (includeInvisible || !member.Player.Actor.IsInvisible)) list.Add(member.Player.Actor);
            return list;
        }
        private float Distance(Combatant pg) => Mathf.Max(0, Vector2.Distance(transform.position, pg.transform.position) - Actor.Radius - pg.Radius);
        private Combatant Nearest()
        {
            Combatant selected = null; float best = float.PositiveInfinity; int ties = 0;
            foreach (var pg in Targets())
            {
                float distance = Distance(pg);
                if (distance < best - .0001f) { selected = pg; best = distance; ties = 1; }
                else if (Mathf.Abs(distance - best) <= .0001f && Random.Range(0, ++ties) == 0) selected = pg;
            }
            return selected;
        }
        private Combatant RandomTarget(float range = float.PositiveInfinity)
        { var list = Targets(); list.RemoveAll(pg => Distance(pg) > range); return list.Count > 0 ? list[Random.Range(0, list.Count)] : null; }
        private void Face(Combatant pg)
        {
            if (pg == null) return;
            Vector2 direction = pg.transform.position - transform.position;
            if (direction.sqrMagnitude > .00001f) transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
        private void Update()
        {
            if (Actor == null || !session.GameplayRunning) return;
            if (!Actor.IsActive) { Status = "BOSS sconfitto"; ClearAreas(); StopAllCoroutines(); return; }
            bool crossed = false;
            while (threshold < data.ChargeThresholds.Length && Actor.HealthFraction <= data.ChargeThresholds[threshold]) { threshold++; crossed = true; }
            if (crossed) pendingCharge = true;
            if (Time.time < stunnedUntil) { Status = "STUN"; return; } if (busy) return;
            target = Nearest();
            if (pendingCharge && !rocksPending)
            {
                var selected = RandomTarget(data.ChargeRange);
                if (selected != null) { pendingCharge = false; StartCoroutine(Charge(selected)); return; }
                Pursue(); return;
            }
            if (!rocksPending && Time.time >= specialAt && target != null)
            {
                int attack = data.SpecialOrder[cycle]; cycle = (cycle + 1) % data.SpecialOrder.Length;
                if (attack == 1) StartCoroutine(Wave(target));
                else if (attack == 2) session.StartCoroutine(Rocks());
                else StartCoroutine(Bile(RandomTarget()));
                return;
            }
            if (target != null && Distance(target) <= data.Stats.RangeMetres && Time.time >= baseAt) StartCoroutine(BaseAttack(target));
            else Pursue();
        }
        private void Pursue()
        {
            Status = pendingCharge ? "Ricerca bersaglio per ATTACCO_03" : "Inseguimento";
            if (target != null)
            {
                Face(target);
                if (Distance(target) > data.Stats.RangeMetres || pendingCharge)
                    Actor.Move(((Vector2)(target.transform.position - transform.position)).normalized * Actor.MovementMetresPerSecond * Time.deltaTime);
            }
            else
            {
                if (Time.time >= wanderAt) { wander = Random.insideUnitCircle.normalized; wanderAt = Time.time + Random.Range(1f, 3f); }
                Actor.Move(wander * Actor.MovementMetresPerSecond * .3f * Time.deltaTime);
            }
        }
        private IEnumerator Channel(float seconds, Combatant pg = null, float track = 0)
        {
            float elapsed = 0;
            while (elapsed < seconds)
            {
                if (pg != null && elapsed < track) Face(pg);
                foreach (var area in areas) if (area != null) area.Progress(elapsed / seconds);
                yield return null; elapsed += Time.deltaTime;
            }
            foreach (var area in areas) if (area != null) area.Progress(1);
        }
        private void ClearAreas() { foreach (var area in areas) if (area != null) Destroy(area.gameObject); areas.Clear(); }
        private void Finish(bool special)
        { ClearAreas(); busy = false; activeKind = -1; if (special) specialAt = Time.time + data.SpecialInterval; Status = "Inseguimento"; }
        private IEnumerator BaseAttack(Combatant pg)
        {
            busy = true; activeKind = 0; Status = "ATTACCO BASE"; Face(pg); baseAt = Time.time + Actor.EffectiveStats.AttackInterval; float endsAt = Time.time + data.BaseDuration;
            yield return Channel(data.BaseHitAt, pg, data.TrackSeconds);
            var arc = BossArea.Create(transform, BossArea.Ring(Actor.Radius, Actor.Radius + data.BaseDepth, data.BaseAngle), Vector2.zero, Actor.Radius);
            arc.Progress(1);
            foreach (var victim in Targets(true))
                if (arc.Contains(victim.transform.position, victim.Radius)) victim.Hit(Actor.EffectiveStats.ATK, true, Actor, true);
            Destroy(arc.gameObject, .15f); // Existing prototype HIT flash duration; no gameplay duration.
            if (Time.time < endsAt) yield return new WaitForSeconds(endsAt - Time.time);
            Finish(false);
        }
        private IEnumerator Wave(Combatant pg)
        {
            busy = true; activeKind = 1; Status = "ATTACCO_01"; Face(pg);
            foreach (float angle in new[] { -data.WaveSideAngle, 0f, data.WaveSideAngle })
                areas.Add(BossArea.Create(transform, BossArea.Rectangle(Actor.Radius, data.WaveLength, data.WaveWidth, angle), BossArea.Direction(angle) * Actor.Radius));
            yield return Channel(data.WaveChannel, pg, data.TrackSeconds);
            foreach (var victim in Targets(true))
                foreach (var area in areas) if (area.Contains(victim.transform.position, victim.Radius)) { victim.Hit(data.WaveDamage, true, Actor); break; }
            ClearAreas(); yield return new WaitForSeconds(data.WaveRecovery); Finish(true);
        }
        private IEnumerator Rocks()
        {
            busy = true; rocksPending = true; Status = "ATTACCO_02";
            var available = Targets(); var rocks = new List<BossArea>();
            for (int i = 0; i < data.RockCount && available.Count > 0; i++)
            {
                int index = Random.Range(0, available.Count); Vector2 centre = available[index].transform.position; available.RemoveAt(index);
                var area = BossArea.Create(transform.parent, BossArea.Circle(data.RockRadius), Vector2.zero);
                area.transform.position = centre; rocks.Add(area);
            }
            float elapsed = 0;
            bool released = false; while (elapsed < data.RockDelay)
            {
                if (!released && elapsed >= data.RockStill) { busy = false; released = true; }
                foreach (var area in rocks) area.Progress(elapsed / data.RockDelay);
                yield return null; elapsed += Time.deltaTime;
            }
            foreach (var area in rocks)
            {
                foreach (var victim in Targets(true)) if (area.Contains(victim.transform.position, victim.Radius) && victim.Hit(data.RockDamage, true, Actor))
                    victim.GetComponent<BossPlayerStatus>()?.Stun(data.RockStun);
                Destroy(area.gameObject);
            }
            rocksPending = false; specialAt = Time.time + data.SpecialInterval;
        }
        private IEnumerator Charge(Combatant pg)
        {
            busy = true; activeKind = 3; Status = "ATTACCO_03"; Face(pg);
            Vector2 heading = transform.right;
            areas.Add(BossArea.Create(transform, BossArea.Rectangle(0, data.ChargeLength + Actor.Radius, data.Diameter), Vector2.zero));
            yield return Channel(data.ChargeChannel); ClearAreas();
            float remaining = data.ChargeLength; bool wall = false; var hitPG = new HashSet<Combatant>();
            while (remaining > .0001f && !wall)
            {
                float step = Mathf.Min(data.ChargeSpeed * Time.deltaTime, remaining);
                Physics2D.SyncTransforms();
                foreach (var hit in Physics2D.CircleCastAll(transform.position, Actor.Radius, heading, step))
                    if (hit.collider.GetComponent<TestObstacle>() != null && Vector2.Dot(heading, hit.normal) < 0)
                    { step = Mathf.Min(step, Mathf.Max(0, hit.distance - .001f)); wall = true; }
                foreach (var hit in Physics2D.CircleCastAll(transform.position, Actor.Radius, heading, step))
                {
                    var victim = hit.collider.GetComponent<Combatant>();
                    if (victim == null || victim.Faction != Faction.PG || !victim.IsActive || !hitPG.Add(victim)) continue;
                    if (victim.Hit(data.ChargeDamage, true, Actor))
                    {
                        var push = new GameObject("RESPINTA BOSS"); push.transform.SetParent(transform.parent);
                        push.AddComponent<PG08Push>().Initialize(session, victim, heading, data.PushDistance, data.PushSeconds);
                    }
                }
                // Charge explicitly crosses PGs; only walls/obstacles stop its sweep.
                transform.position += (Vector3)(heading * step); remaining -= step;
                yield return null;
            }
            Status = wall ? "STUN da collisione" : "Recupero carica";
            yield return new WaitForSeconds(wall ? data.WallStun : data.ChargeRecovery); Finish(true);
        }
        private IEnumerator Bile(Combatant pg)
        {
            busy = true; activeKind = 4; Status = "ATTACCO_04"; Face(pg);
            float slice = data.BileAngle / data.BileSlices;
            for (int i = 0; i < data.BileSlices; i++)
                areas.Add(BossArea.Create(transform, BossArea.Ring(Actor.Radius, Actor.Radius + data.BileRange, slice, data.BileAngle / 2 - slice * (i + .5f)), Vector2.zero, Actor.Radius));
            float elapsed = 0; int fired = 0;
            while (fired < data.BileSlices)
            {
                for (int i = fired; i < areas.Count; i++) areas[i].Progress(elapsed / (data.BileChannel + i * data.BileInterval));
                while (fired < data.BileSlices && elapsed >= data.BileChannel + fired * data.BileInterval)
                {
                    Vector2 direction = transform.TransformDirection(BossArea.Direction(data.BileAngle / 2 - slice * (fired + .5f)));
                    var projectile = TestVisuals.Circle("Bile BOSS", (Vector2)transform.position + direction * Actor.Radius, data.BileProjectileRadius, Color.green, 4).AddComponent<Projectile>();
                    projectile.Initialize(Actor, direction, data.BileSpeed, data.BileRange, data.BileDamage, data.BileProjectileRadius, 0, false);
                    projectile.HitEffect = this;
                    Destroy(areas[fired].gameObject); fired++;
                }
                if (fired == data.BileSlices) break;
                yield return null; elapsed += Time.deltaTime;
            }
            areas.Clear(); yield return new WaitForSeconds(data.BileRecovery); Finish(true);
        }
        public void ResolveHit(Combatant victim, float damage, bool roundDamage, Combatant source, int hitIndex)
        {
            if (victim.Hit(damage, roundDamage, source)) victim.GetComponent<BossPlayerStatus>()?.PoisonHit(source, data.PoisonDamage, data.PoisonSeconds);
        }
        public void Stun(float seconds)
        {
            if (!Actor.IsActive) return;
            stunnedUntil = Time.time + seconds;
            if (activeKind == 1 || activeKind == 4)
            { cycle = (cycle + data.SpecialOrder.Length - 1) % data.SpecialOrder.Length; specialAt = stunnedUntil; }
            if (activeKind == 3) pendingCharge = true;
            StopAllCoroutines(); ClearAreas(); busy = false; activeKind = -1; baseAt = stunnedUntil;
        }
        private void OnDisable() { ClearAreas(); }
    }
}

