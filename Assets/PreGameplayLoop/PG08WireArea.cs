using System.Collections.Generic;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    [DefaultExecutionOrder(50)]
    public sealed class PG08WireArea : MonoBehaviour
    {
        private LoopSession session;
        private bool[] valid;
        private float inner, outer;
        private Mesh mesh;
        private Material material;
        private bool ended;
        private readonly HashSet<PG08WireContact> contacts = new HashSet<PG08WireContact>();
        public Combatant Source { get; private set; }
        public float ExpiresAt { get; private set; }
        public float Damage { get; private set; }
        public float Slow { get; private set; }
        public int ValidSections { get; private set; }
        public float Remaining => ended ? 0 : Mathf.Max(0, ExpiresAt - Time.time);
        public bool Running => !ended && session != null && session.GameplayRunning;
        public void Initialize(LoopSession owner, Combatant source, PG08AbilityCatalog data)
        {
            session = owner; Source = source; outer = data.WireOuterRadius; inner = outer - data.WireThickness;
            Damage = data.WireDamage; Slow = data.WireSlow; ExpiresAt = Time.time + data.WireDuration;
            valid = new bool[data.WireSections]; Physics2D.SyncTransforms();
            for (int i = 0; i < valid.Length; i++)
            {
                valid[i] = true;
                foreach (var obstacle in TestObstacle.All)
                    if (obstacle.isActiveAndEnabled && PG08WireGeometry.BoxIntersects(transform.position, inner, outer, i * 360f / valid.Length, 360f / valid.Length, obstacle.Bounds))
                    { valid[i] = false; break; }
                if (valid[i]) ValidSections++;
            }
            if (ValidSections == 0) { End(); return; }
            BuildVisual(); DetectContacts();
        }
        public bool Contains(Combatant actor)
        {
            if (ended || actor == null || !actor.IsActive || actor.Faction != Faction.MOB) return false;
            Vector2 offset = actor.transform.position - transform.position;
            for (int i = 0; i < valid.Length; i++)
                if (valid[i] && PG08WireGeometry.CircleIntersects(offset, actor.Radius, inner, outer, i * 360f / valid.Length, 360f / valid.Length)) return true;
            return false;
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
        private void Update() { if (Running && Remaining > 0) DetectContacts(); }
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
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int sector = 0; sector < valid.Length; sector++)
            {
                if (!valid[sector]) continue;
                for (int part = 0; part < 18; part++)
                {
                    Vector2 a = AttackGeometry.Direction((sector + part / 18f) * 360f / valid.Length);
                    Vector2 b = AttackGeometry.Direction((sector + (part + 1) / 18f) * 360f / valid.Length);
                    int n = vertices.Count;
                    vertices.Add(a * inner); vertices.Add(a * outer); vertices.Add(b * outer); vertices.Add(b * inner);
                    triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 2); triangles.Add(n); triangles.Add(n + 2); triangles.Add(n + 3);
                }
            }
            mesh = new Mesh { name = "FILO SPINATO annulus" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            material = new Material(Shader.Find("Sprites/Default")) { color = new Color(.8f, .65f, .25f, .55f) };
            var renderer = gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = 1;
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}
