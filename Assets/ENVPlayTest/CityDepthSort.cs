using UnityEngine;
using UnityEngine.Rendering;

namespace RogZombie.PlayTests
{
    // Visual depth only. The gameplay plane, transforms and colliders are never changed.
    [ExecuteAlways, RequireComponent(typeof(SortingGroup))]
    [DefaultExecutionOrder(400)]
    public sealed class CityDepthSort : MonoBehaviour
    {
        [Tooltip("World-space Y offset of the visual feet/base from the existing gameplay root.")]
        public float FootOffset;
        private SortingGroup group;
        private void OnEnable() => Apply();
        private void LateUpdate() => Apply();
        public void Apply()
        {
            if (group == null) group = GetComponent<SortingGroup>();
            group.sortingOrder = 500 - Mathf.RoundToInt((transform.position.y + FootOffset) * 100f);
        }
    }
}
