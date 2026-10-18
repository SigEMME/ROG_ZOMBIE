using UnityEngine;
using UnityEngine.Rendering;

namespace ROGZOMBIE.EnvironmentArt
{
    // Scene RenderSettings are not part of a prefab. Apply them only while its map is active.
    public sealed class MapAmbientLighting : MonoBehaviour
    {
        public Light sun;
        public Color ambient = new Color(.40f, .45f, .54f);
        public AmbientMode mode = AmbientMode.Flat;
        Color previousAmbient;
        AmbientMode previousMode;
        Light previousSun;
        bool applied;
        void OnEnable()
        {
            if (applied) return;
            previousAmbient = RenderSettings.ambientLight;
            previousMode = RenderSettings.ambientMode;
            previousSun = RenderSettings.sun;
            RenderSettings.ambientMode = mode;
            RenderSettings.ambientLight = ambient;
            RenderSettings.sun = sun;
            applied = true;
        }
        void OnDisable()
        {
            if (!applied) return;
            RenderSettings.ambientMode = previousMode;
            RenderSettings.ambientLight = previousAmbient;
            RenderSettings.sun = previousSun;
            applied = false;
        }
    }
}
