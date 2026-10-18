using UnityEngine;
using UnityEngine.Rendering;

namespace ROGZOMBIE.EnvironmentArt
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class GroundMist : MonoBehaviour
    {
        [Header("Aspetto")]
        [InspectorName("Materiale foschia")] public Material material;
        [InspectorName("Colore")] public Color color = new Color(.64f, .72f, .8f, 1);
        [Range(0, .5f), InspectorName("Densita")] public float density = .14f;
        [Min(.05f), InspectorName("Scala delle nuvole")] public float noiseScale = 3f;
        [Header("Distribuzione (metri)")]
        [Range(1, 32), InspectorName("Numero banchi")] public int bankCount = 12;
        [InspectorName("Estensione area X/Z")] public Vector2 areaSize = new Vector2(130, 130);
        [InspectorName("Dimensioni banco X/Z")] public Vector2 bankSize = new Vector2(27, 19);
        [Min(0), InspectorName("Altezza sul terreno")] public float height = .65f;
        [InspectorName("Terreno di riferimento")] public Terrain terrain;
        [InspectorName("Seme distribuzione")] public int seed = 31;
        [Header("Movimento")]
        [InspectorName("Velocita X/Z"), Tooltip("Scorrimento delle nuvole in metri al secondo. Zero per fermarle.")]
        public Vector2 velocity = new Vector2(.18f, .07f);
        [InspectorName("Animazione in Editor")] public bool animateInEditor = true;
        [Header("Altezza casuale")]
        [InspectorName("Anima altezza")] public bool animateHeight = true;
        [InspectorName("Range altezza min/max (m)")] public Vector2 heightRange = new Vector2(.4f, 1.6f);
        [Min(0), InspectorName("Velocita cambio altezza"), Tooltip("Transizioni al secondo. 0,15 = una nuova quota ogni 6,7 secondi. Zero mantiene la quota casuale iniziale.")]
        public float heightChangeSpeed = .15f;

        GameObject visual;
        Mesh mesh;
        MeshRenderer meshRenderer;
        MaterialPropertyBlock properties;
        bool rebuild = true;
        Vector3[] baseVertices;
        Vector3[] animatedVertices;
        static readonly int Tint = Shader.PropertyToID("_Tint");
        static readonly int Density = Shader.PropertyToID("_Density");
        static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
        static readonly int Drift = Shader.PropertyToID("_Drift");

        void OnEnable() { rebuild = true; }
        void OnValidate()
        {
            bankCount = Mathf.Clamp(bankCount, 1, 32);
            areaSize = new Vector2(Mathf.Max(1, areaSize.x), Mathf.Max(1, areaSize.y));
            bankSize = new Vector2(Mathf.Max(1, bankSize.x), Mathf.Max(1, bankSize.y));
            height = Mathf.Max(0, height);
            heightRange.x = Mathf.Max(0, heightRange.x);
            heightRange.y = Mathf.Max(heightRange.x, heightRange.y);
            heightChangeSpeed = Mathf.Max(0, heightChangeSpeed);
            rebuild = true;
        }
        void Update()
        {
            if (!material) { if (visual) visual.SetActive(false); return; }
            if (rebuild || !visual) Build();
            visual.SetActive(true);
            visual.layer = gameObject.layer;
            meshRenderer.sharedMaterial = material;
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.SetColor(Tint, color);
            properties.SetFloat(Density, density);
            properties.SetFloat(NoiseScale, Mathf.Max(.05f, noiseScale));
            float time = Application.isPlaying ? Time.time : (animateInEditor ? Time.realtimeSinceStartup : 0);
            UpdateHeight(time);
            properties.SetVector(Drift, new Vector4(velocity.x * time, velocity.y * time, 0, 0));
            meshRenderer.SetPropertyBlock(properties);
        }
        void Build()
        {
            Release();
            visual = new GameObject("Foschia generata") { hideFlags = HideFlags.HideAndDontSave, layer = gameObject.layer };
            visual.transform.SetParent(transform, false);
            var vertices = new Vector3[bankCount * 4];
            var uv = new Vector2[vertices.Length];
            var indices = new int[bankCount * 6];
            var random = new System.Random(seed);
            for (int i = 0; i < bankCount; i++)
            {
                float x = ((float)random.NextDouble() - .5f) * areaSize.x;
                float z = ((float)random.NextDouble() - .5f) * areaSize.y;
                // Leave the central combat space clearer; spread banks toward the edges.
                if (Mathf.Abs(x) < areaSize.x * .23f && Mathf.Abs(z) < areaSize.y * .23f)
                    x += (x >= 0 ? 1 : -1) * areaSize.x * .26f;
                float angle = (float)random.NextDouble() * 360;
                float scale = .8f + (float)random.NextDouble() * .4f;
                for (int c = 0; c < 4; c++)
                {
                    var corner = new Vector2(c == 0 || c == 3 ? 0 : 1, c < 2 ? 0 : 1);
                    var p = new Vector3(x, 0, z) + Quaternion.Euler(0, angle, 0) *
                        new Vector3((corner.x - .5f) * bankSize.x * scale, 0, (corner.y - .5f) * bankSize.y * scale);
                    var world = transform.TransformPoint(p);
                    world.y = terrain ? terrain.SampleHeight(world) + terrain.transform.position.y + height : world.y + height;
                    vertices[i * 4 + c] = transform.InverseTransformPoint(world);
                    uv[i * 4 + c] = corner;
                }
                int v = i * 4, t = i * 6;
                indices[t] = v; indices[t+1] = v+2; indices[t+2] = v+1;
                indices[t+3] = v; indices[t+4] = v+3; indices[t+5] = v+2;
            }
            mesh = new Mesh { name = "Banchi foschia", hideFlags = HideFlags.HideAndDontSave };
            mesh.MarkDynamic();
            baseVertices = vertices;
            animatedVertices = new Vector3[vertices.Length];
            mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = indices; mesh.RecalculateBounds();
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;
            meshRenderer = visual.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            rebuild = false;
        }
        float HeightAt(int bank, double time)
        {
            if (!animateHeight) return height;
            double phase = time * heightChangeSpeed;
            int step = (int)System.Math.Floor(phase);
            float blend = (float)(phase - step);
            blend = blend * blend * (3f - 2f * blend);
            return Mathf.Lerp(heightRange.x, heightRange.y,
                Mathf.Lerp(RandomHeight(bank, step), RandomHeight(bank, step + 1), blend));
        }
        float RandomHeight(int bank, int step)
        {
            // Independent deterministic sequence per bank; never alter gameplay's Random state.
            unchecked
            {
                uint n = (uint)seed ^ (uint)(bank + 1) * 374761393u ^ (uint)step * 668265263u;
                n = (n ^ (n >> 13)) * 1274126177u;
                n ^= n >> 16;
                return (n & 0x00ffffff) / 16777215f;
            }
        }
        void UpdateHeight(double time)
        {
            for (int bank = 0; bank < bankCount; bank++)
            {
                var offset = transform.InverseTransformVector(Vector3.up * (HeightAt(bank, time) - height));
                for (int c = 0; c < 4; c++)
                {
                    int index = bank * 4 + c;
                    animatedVertices[index] = baseVertices[index] + offset;
                }
            }
            mesh.vertices = animatedVertices;
            // Keep culling correct throughout the vertical movement.
            mesh.RecalculateBounds();
        }
        void OnDisable() { Release(); }
        void OnDestroy() { Release(); }
        void Release()
        {
            if (visual) { visual.SetActive(false); Dispose(visual); }
            if (mesh) Dispose(mesh);
            visual = null; mesh = null; meshRenderer = null;
            baseVertices = null; animatedVertices = null;
        }
        static void Dispose(Object obj)
        {
            if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
        }
    }
}
