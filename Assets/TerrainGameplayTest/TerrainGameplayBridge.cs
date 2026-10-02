using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PlayTests
{
    // Isolated presentation/physics bridge: the terrain stays in XZ; existing combat stays in XY.
    [DefaultExecutionOrder(-300)]
    public sealed class TerrainGameplayBridge : MonoBehaviour
    {
        public Shader BackgroundShader;
        private TestEngineBootstrap session;
        private RogZombie.PreGameplayLoop.LoopSession loop;
        private bool ownsSettings;
        private Terrain ground;
        private Camera terrainCamera;
        private Camera gameCamera;
        private RenderTexture target;
        private Material backgroundMaterial;
        private Transform background;
        private Vector3 mapCenter;
        private Collider[] scenery;
        private TestAreaSettings runtimeSettings;
        private const int SceneryLayer = 31;
        public float CameraDistance => RogZombie.PreGameplayLoop.TopDownEnvironment.CameraHeight;

        public void ConfigureCamera(Camera camera)
        {
            camera.orthographic = false;
            // Preserve the previous ground coverage using the existing top-down camera height.
            camera.fieldOfView = 2f * Mathf.Atan(runtimeSettings.CameraSize / CameraDistance) * Mathf.Rad2Deg;
            Vector3 position = camera.transform.position;
            position.z = -CameraDistance;
            camera.transform.position = position;
        }

        private void Awake()
        {
            session = GetComponent<TestEngineBootstrap>();
            if (session == null) return;
            ground = FindFirstObjectByType<Terrain>();
            gameCamera = Camera.main;
            if (ground == null || gameCamera == null || session.Settings == null || BackgroundShader == null)
            {
                Debug.LogError("Terrain gameplay: Terrain, camera, settings or background shader missing.", this);
                session.enabled = false;
                enabled = false;
                return;
            }
            scenery = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            mapCenter = ground.transform.position + ground.terrainData.size * .5f;
            runtimeSettings = Instantiate(session.Settings);
            ownsSettings = true;
            runtimeSettings.AreaSize = new Vector2(ground.terrainData.size.x, ground.terrainData.size.z);
            runtimeSettings.StartPosition = Vector2.zero;
            runtimeSettings.PlayerMoveSpeedBase = 2f; // Approved GDD conversion: MOVE SPD 100 = 2 m/s.
            session.Settings = runtimeSettings;

            // Layer isolation is runtime-only and does not alter ProjectSettings or the source map.
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                if (root == gameObject || root == gameCamera.gameObject) continue;
                foreach (var item in root.GetComponentsInChildren<Transform>(true))
                    item.gameObject.layer = SceneryLayer;
            }
            SetupCamera();
        }

        public void Initialize(RogZombie.PreGameplayLoop.LoopSession world, GameObject map, Shader shader)
        {
            loop = world;
            BackgroundShader = shader;
            gameCamera = world.GameCamera;
            runtimeSettings = world.Settings;
            ground = map.GetComponentInChildren<Terrain>();
            scenery = map.GetComponentsInChildren<Collider>(true);
            mapCenter = ground.transform.position + ground.terrainData.size * .5f;
            runtimeSettings.AreaSize = new Vector2(ground.terrainData.size.x, ground.terrainData.size.z);
            foreach (var item in map.GetComponentsInChildren<Transform>(true)) item.gameObject.layer = SceneryLayer;
            SetupCamera();
        }

        private void SetupCamera()
        {
            gameCamera.transform.rotation = Quaternion.identity;
            ConfigureCamera(gameCamera);
            gameCamera.cullingMask = ~(1 << SceneryLayer);
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.farClipPlane = 200;
            var cameraObject = new GameObject("Terrain presentation camera");
            terrainCamera = cameraObject.AddComponent<Camera>();
            terrainCamera.orthographic = false;
            terrainCamera.cullingMask = 1 << SceneryLayer;
            terrainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            terrainCamera.nearClipPlane = .1f;
            terrainCamera.farClipPlane = 500;
            terrainCamera.depth = gameCamera.depth - 1;
            terrainCamera.clearFlags = CameraClearFlags.SolidColor;
            terrainCamera.backgroundColor = new Color(.12f, .15f, .10f);
            terrainCamera.allowHDR = false;
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Terrain presentation (combat XY)";
            Destroy(quad.GetComponent<Collider>());
            background = quad.transform;
            background.SetParent(gameCamera.transform, false);
            background.localPosition = new Vector3(0, 0, CameraDistance * 2f);
            backgroundMaterial = new Material(BackgroundShader);
            quad.GetComponent<MeshRenderer>().sharedMaterial = backgroundMaterial;
            // Allocate the target before URP collects its camera list.
            PrepareTerrainCamera(terrainCamera);
        }

        public void BuildObstacles(Transform root)
        {
            foreach (var collider in scenery)
            {
                if (collider == null || !collider.enabled || collider.isTrigger ||
                    !collider.gameObject.activeInHierarchy || collider is TerrainCollider) continue;
                Bounds local;
                if (collider is BoxCollider box) local = new Bounds(box.center, box.size);
                else if (collider is MeshCollider mesh && mesh.sharedMesh != null) local = mesh.sharedMesh.bounds;
                else if (collider is CapsuleCollider capsule)
                {
                    // Trees block only the footprint of their trunk.
                    local = new Bounds(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
                }
                else continue;
                // Exclude terrain reference planes and flat decorative surfaces.
                if (collider.bounds.size.y < .15f) continue;
                Vector3 centre = collider.transform.TransformPoint(local.center) - mapCenter;
                Vector3 right = collider.transform.TransformVector(Vector3.right);
                float angle = Mathf.Atan2(right.z, right.x) * Mathf.Rad2Deg;
                Quaternion inverseYaw = Quaternion.Euler(0, angle, 0);
                Vector3 extents = Vector3.zero;
                foreach (Vector3 axis in new[] { Vector3.right * local.extents.x,
                    Vector3.up * local.extents.y, Vector3.forward * local.extents.z })
                {
                    Vector3 v = inverseYaw * collider.transform.TransformVector(axis);
                    extents += new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
                }
                AddObstacle(root, collider.name, new Vector2(centre.x, centre.z),
                    new Vector2(extents.x * 2, extents.z * 2), angle);
            }
            Vector2 size = runtimeSettings.AreaSize;
            AddObstacle(root, "Map boundary W", new Vector2(-size.x/2,0), new Vector2(1,size.y),0);
            AddObstacle(root, "Map boundary E", new Vector2(size.x/2,0), new Vector2(1,size.y),0);
            AddObstacle(root, "Map boundary S", new Vector2(0,-size.y/2), new Vector2(size.x,1),0);
            AddObstacle(root, "Map boundary N", new Vector2(0,size.y/2), new Vector2(size.x,1),0);
            Physics2D.SyncTransforms();
        }

        private static void AddObstacle(Transform root, string name, Vector2 position, Vector2 size, float angle)
        {
            if (size.x < .01f || size.y < .01f) return;
            var go = new GameObject("Map collision: " + name);
            go.transform.SetParent(root, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0,0,angle));
            go.layer = LayerMask.NameToLayer("MURO");
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = true;
        }

        public bool ResolveStart(TestNavigation navigation)
        {
            if (!navigation.Sample(runtimeSettings.StartPosition, out var point, runtimeSettings.AreaSize.magnitude))
                return false;
            runtimeSettings.StartPosition = point;
            return true;
        }

        private void LateUpdate() { if (terrainCamera != null) PrepareTerrainCamera(terrainCamera); }

        private void PrepareTerrainCamera(Camera camera)
        {
            // Use the actor position directly: Bootstrap follows it later in LateUpdate.
            if (camera != terrainCamera || gameCamera == null) return;
            int width = Mathf.Max(1, gameCamera.pixelWidth);
            int height = Mathf.Max(1, gameCamera.pixelHeight);
            if (target == null || target.width != width || target.height != height)
            {
                if (target != null) { terrainCamera.targetTexture = null; target.Release(); Destroy(target); }
                target = new RenderTexture(width, height, 24) { name = "Terrain gameplay background" };
                target.Create();
                terrainCamera.targetTexture = target;
                backgroundMaterial.mainTexture = target;
            }
            terrainCamera.fieldOfView = gameCamera.fieldOfView;
            terrainCamera.aspect = gameCamera.aspect;
            var actor = loop != null ? loop.Controlled?.Player : session.Player;
            Vector3 p = actor != null ? actor.transform.position : gameCamera.transform.position;
            terrainCamera.transform.position = new Vector3(mapCenter.x + p.x,
                ground.transform.position.y + CameraDistance, mapCenter.z + p.y);
            float halfHeight = background.localPosition.z * Mathf.Tan(gameCamera.fieldOfView * .5f * Mathf.Deg2Rad);
            background.localScale = new Vector3(halfHeight * 2f * gameCamera.aspect, halfHeight * 2f, 1);
        }

        private void OnDestroy()
        {
            if (terrainCamera != null) Destroy(terrainCamera.gameObject);
            if (background != null) Destroy(background.gameObject);
            if (target != null) { target.Release(); Destroy(target); }
            if (backgroundMaterial != null) Destroy(backgroundMaterial);
            if (ownsSettings && runtimeSettings != null) Destroy(runtimeSettings);
        }
    }
}
