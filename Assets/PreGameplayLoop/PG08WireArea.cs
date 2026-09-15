using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    [DefaultExecutionOrder(50)]
    public sealed class PG08WireArea : MonoBehaviour
    {
        private LoopSession session;
        private float inner, outer;
        private bool ended;
        private float nextVisualRefresh;
        private readonly HashSet<PG08WireContact> contacts = new HashSet<PG08WireContact>();
        public Combatant Source { get; private set; }
        public float ExpiresAt { get; private set; }
        public float Damage { get; private set; }
        public float Slow { get; private set; }
        public float Remaining => ended ? 0 : Mathf.Max(0, ExpiresAt - Time.time);
        public bool Running => !ended && session != null && session.GameplayRunning;
        public void Initialize(LoopSession owner, Combatant source, PG08AbilityCatalog data)
        {
            session = owner; Source = source; outer = data.WireOuterRadius; inner = outer - data.WireThickness;
            Damage = data.WireDamage; Slow = data.WireSlow; ExpiresAt = Time.time + data.WireDuration;
            Physics2D.SyncTransforms();
            BuildVisual(); DetectContacts();
        }
        public bool Contains(Combatant actor)
        {
            if (ended || actor == null || !actor.IsActive || actor.Faction != Faction.MOB) return false;
            Vector2 center = transform.position;
            return PG08WireGeometry.Contains(AttackGeometry.TargetCenter(actor) - center, inner, outer) &&
                AttackGeometry.InVisibleArea(center, outer, actor);
        }

        private void DetectContacts()
        {
            foreach (var actor in Combatant.All.ToArray())
            {
                if (!Contains(actor)) continue;
                var contact = actor.GetComponent<PG08WireContact>();
                if (contact == null) contact = actor.gameObject.AddComponent<PG08WireContact>();
                contact.Enter(this); contacts.Add(contact);
            }
        }
        private void Update()
        {
            if (!Running || Remaining <= 0) return;
            if (Time.time >= nextVisualRefresh)
            {
                BuildVisual(); nextVisualRefresh = Time.time + .1f;
            }
            DetectContacts();
        }
        // Contact components process the final whole second before this ring disappears.
        private void LateUpdate() { if (Running && Remaining <= 0) End(); }
        public void End()
        {
            if (ended) return;
            ended = true;
            foreach (var contact in contacts) if (contact != null) contact.Exit(this);
            contacts.Clear(); gameObject.SetActive(false); Destroy(gameObject);
        }
        private void OnDisable()
        { foreach (var contact in contacts) if (contact != null) contact.Exit(this); contacts.Clear(); }
        private void BuildVisual()
        {
            var visual = GetComponent<PG04AreaVisual>();
            if (visual == null) visual = gameObject.AddComponent<PG04AreaVisual>();
            visual.InitializeOccluded(transform.position, outer, new Color(.8f, .65f, .25f, .55f), innerRadius: inner);
        }
    }
}
