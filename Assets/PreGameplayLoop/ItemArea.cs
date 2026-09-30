using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed class ItemArea : MonoBehaviour
    {
        private static readonly List<ItemArea> active = new List<ItemArea>();
        private LoopSession session;
        private Combatant source;
        private float elapsed, angle, detonateAt = -1, nextVisual;
        private int nextTick = 1;
        private GameObject visual;
        public PrototypeItem Kind { get; private set; }
        public int Ticks { get; private set; }
        public bool Triggered => detonateAt >= 0;
        public float Remaining => Mathf.Max(0, Duration(Kind) - elapsed);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => active.Clear();
        public static float Radius(PrototypeItem kind) => kind == PrototypeItem.Granata ? 2 : kind == PrototypeItem.Molotov ? 3 : kind == PrototypeItem.MinaElettrica ? 2.5f : 4;
        private static float Duration(PrototypeItem kind) => kind == PrototypeItem.Molotov || kind == PrototypeItem.Trappola ? 5 : kind == PrototypeItem.Smoke || kind == PrototypeItem.PozioneCurativa ? 4 : .2f;
        public void Initialize(LoopSession owner, Combatant caster, PrototypeItem kind, float facing)
        {
            session = owner; source = caster; Kind = kind; angle = facing;
            gameObject.layer = LayerMask.NameToLayer(kind == PrototypeItem.Smoke || kind == PrototypeItem.PozioneCurativa ? "AREA_EFFECT_PG" : "AREA_EFFECT_MOB");
            active.Add(this); RefreshVisual();
            if (kind != PrototypeItem.MinaElettrica && kind != PrototypeItem.Smoke) Tick();
        }
        private void OnDisable() => active.Remove(this);
        public static bool InSmoke(Combatant actor)
        {
            foreach (var area in active) if (area != null && area.Kind == PrototypeItem.Smoke && area.Remaining > 0 && area.Contains(actor)) return true;
            return false;
        }
        public static float SlowMultiplier(Combatant actor)
        {
            foreach (var area in active) if (area != null && area.Kind == PrototypeItem.Trappola && area.Remaining > 0 && area.Contains(actor)) return .6f;
            return float.PositiveInfinity;
        }
        public bool Contains(Combatant actor)
        {
            if (actor == null || !actor.IsActive) return false;
            bool beneficial = Kind == PrototypeItem.Smoke || Kind == PrototypeItem.PozioneCurativa;
            if (actor.Faction != (beneficial ? Faction.PG : Faction.MOB)) return false;
            Vector2 targetCenter = actor.transform.TransformPoint(actor.GetComponent<CircleCollider2D>().offset);
            if (Kind == PrototypeItem.Trappola)
            {
                Vector2 local = Quaternion.Euler(0, 0, -angle) * (targetCenter - (Vector2)transform.position);
                if (Mathf.Abs(local.x) > .5f || Mathf.Abs(local.y) > 2) return false;
                int section = Mathf.Clamp(Mathf.FloorToInt(local.y + 2), 0, 3);
                return SectionFree(transform.position, angle, section);
            }
            return beneficial ? Vector2.Distance(transform.position, targetCenter) <= Radius(Kind) :
                AttackGeometry.InVisibleArea(transform.position, Radius(Kind), actor);
        }
        public static bool SectionFree(Vector2 center, float angle, int section)
        {
            Vector2 position = center + (Vector2)(Quaternion.Euler(0, 0, angle) * new Vector3(0, section - 1.5f));
            Physics2D.SyncTransforms();
            foreach (var hit in Physics2D.OverlapBoxAll(position, Vector2.one, angle))
                if (hit.GetComponent<TestObstacle>() != null) return false;
            return true;
        }
        private void Tick()
        {
            Physics2D.SyncTransforms();
            Ticks++;
            foreach (var target in Combatant.All.ToArray())
            {
                if (!Contains(target)) continue;
                switch (Kind)
                {
                    case PrototypeItem.Granata: target.Hit(50, true, source); break;
                    case PrototypeItem.Molotov: target.Hit(7, true, source); break;
                    case PrototypeItem.Trappola: target.Hit(5, true, source); break;
                    case PrototypeItem.PozioneCurativa: target.Heal(10 / target.Stats.HP); break;
                    case PrototypeItem.BombaVelenosa:
                        var poison = target.GetComponent<PG05Poison>();
                        if (poison == null) poison = target.gameObject.AddComponent<PG05Poison>();
                        poison.Apply(session, source, 5, 5); break;
                    case PrototypeItem.MinaElettrica:
                        target.Hit(10, true, source); if (target.IsActive) target.Stun(2.5f); break;
                }
            }
        }
        private void Update()
        {
            if (session == null || !session.GameplayRunning) return;
            elapsed += Time.deltaTime;
            if (Kind == PrototypeItem.MinaElettrica)
            {
                if (detonateAt < 0)
                    foreach (var actor in Combatant.All)
                        if (actor != null && actor.IsActive && actor.Faction == Faction.MOB &&
                            Vector2.Distance(transform.position, AttackGeometry.TargetCenter(actor)) <= .5f + actor.Radius)
                        { detonateAt = elapsed + 1; break; }
                if (detonateAt >= 0 && elapsed >= detonateAt)
                {
                    Tick();
                    var flash = new GameObject("Esplosione MINA ELETTRICA"); flash.transform.SetParent(transform.parent, false); flash.transform.position = transform.position;
                    Draw(flash, Kind, angle, false); Destroy(flash, .2f); Destroy(gameObject); return;
                }
            }
            else
            {
                if (Kind == PrototypeItem.Molotov || Kind == PrototypeItem.Trappola || Kind == PrototypeItem.PozioneCurativa)
                    while (nextTick < Duration(Kind) && elapsed >= nextTick) { Tick(); nextTick++; }
                if (elapsed >= Duration(Kind)) { Destroy(gameObject); return; }
            }
            if (elapsed >= nextVisual) { RefreshVisual(); nextVisual = elapsed + .1f; }
        }
        private void RefreshVisual()
        {
            if (visual != null && Kind == PrototypeItem.Trappola) { Destroy(visual); visual = null; }
            if (visual == null) { visual = new GameObject("Area ITEM"); visual.transform.SetParent(transform, false); }
            Draw(visual, Kind, angle, Kind == PrototypeItem.MinaElettrica);
        }
        public static void Draw(GameObject go, PrototypeItem kind, float angle, bool preview)
        {
            Color color = kind == PrototypeItem.PozioneCurativa ? new Color(.1f, 1, .3f, .35f) :
                kind == PrototypeItem.Smoke ? new Color(.6f, .65f, .7f, .4f) : kind == PrototypeItem.BombaVelenosa ? new Color(.6f, .1f, .8f, .4f) :
                kind == PrototypeItem.MinaElettrica ? new Color(.15f, .7f, 1, .4f) : new Color(1, .35f, .05f, .4f);
            if (kind == PrototypeItem.Trappola)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (!SectionFree(go.transform.position, angle, i)) continue;
                    var section = new GameObject("Sezione TRAPPOLA"); section.transform.SetParent(go.transform, false);
                    section.transform.localPosition = Quaternion.Euler(0, 0, angle) * new Vector3(0, i - 1.5f);
                    section.transform.rotation = Quaternion.Euler(0, 0, angle);
                    section.AddComponent<PG04AreaVisual>().InitializePolygon(new[] { new Vector2(-.5f,-.5f), new Vector2(.5f,-.5f), new Vector2(.5f,.5f), new Vector2(-.5f,.5f) }, color, 1);
                }
                return;
            }
            var area = go.GetComponent<PG04AreaVisual>();
            if (area == null) area = go.AddComponent<PG04AreaVisual>();
            if (kind == PrototypeItem.Smoke || kind == PrototypeItem.PozioneCurativa)
                area.Initialize(Radius(kind), new[] { true, true, true, true }, color);
            else area.InitializeOccluded(go.transform.position, Radius(kind), color);
            if (kind == PrototypeItem.MinaElettrica && preview && go.transform.Find("Trigger 0.5m") == null)
            {
                var trigger = new GameObject("Trigger 0.5m"); trigger.transform.SetParent(go.transform, false);
                trigger.AddComponent<PG04AreaVisual>().Initialize(.5f, new[] { true, true, true, true }, Color.cyan);
            }
        }
    }
}
