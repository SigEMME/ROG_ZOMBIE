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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => All.Clear();
        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);
    }
}
