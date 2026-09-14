using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class TestObstacle : MonoBehaviour
    {
        public static readonly List<TestObstacle> All = new List<TestObstacle>();
        public bool IsWall = true;
        public Bounds Bounds => GetComponent<BoxCollider2D>().bounds;
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
