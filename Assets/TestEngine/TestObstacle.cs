using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class TestObstacle : MonoBehaviour
    {
        public static readonly List<TestObstacle> All = new List<TestObstacle>();
        public bool IsWall = true;
        public Mesh NavigationMesh { get; set; }
        public Collider2D Body => polygon != null ? (Collider2D)polygon : GetComponent<BoxCollider2D>();
        private PolygonCollider2D polygon;
        private readonly List<Vector2> localOutline = new List<Vector2>();
        private Vector2[] worldOutline;
        public bool IsPolygon => polygon != null;
        public void UsePolygon(PolygonCollider2D value) => polygon = value;
        public Bounds Bounds => Body.bounds;
        public Vector2[] WorldOutline()
        {
            Vector2 offset;
            if (polygon != null) { polygon.GetPath(0, localOutline); offset = polygon.offset; }
            else
            {
                var box = GetComponent<BoxCollider2D>(); Vector2 h = box.size * .5f; offset = box.offset;
                localOutline.Clear(); localOutline.Add(new Vector2(-h.x,-h.y)); localOutline.Add(new Vector2(h.x,-h.y));
                localOutline.Add(new Vector2(h.x,h.y)); localOutline.Add(new Vector2(-h.x,h.y));
            }
            if (worldOutline == null || worldOutline.Length != localOutline.Count) worldOutline = new Vector2[localOutline.Count];
            for (int i = 0; i < worldOutline.Length; i++) worldOutline[i] = transform.TransformPoint(localOutline[i] + offset);
            return worldOutline;
        }
        // Rotate queries into a world-sized, axis-aligned frame for exact oriented-box checks.
        public Vector2 UnrotatePoint(Vector2 point)
        {
            Vector3 centre = GetComponent<BoxCollider2D>().bounds.center;
            return centre + Quaternion.Euler(0, 0, -transform.eulerAngles.z) * ((Vector3)point - centre);
        }
        public Bounds UnrotatedBounds
        {
            get
            {
                var box = GetComponent<BoxCollider2D>();
                Vector3 scale = transform.lossyScale;
                return new Bounds(box.bounds.center, new Vector3(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y), 1));
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => All.Clear();
        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);
    }
}
