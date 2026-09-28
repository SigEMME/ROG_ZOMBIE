using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    // Presentation only: footprints, collision layers and navigation remain owned by LoopSession.
    public sealed class TopDownEnvironment : MonoBehaviour
    {
        public const float CameraHeight = 36;
        private readonly List<Material> materials = new List<Material>();
        private readonly Dictionary<Vector2, float> buildingHeights = new Dictionary<Vector2, float>();
        private Material facade, roof, trim, asphalt, pavement, red, boundaryMaterial;

        public void Initialize(Shader shader)
        {
            if (shader == null) shader = Shader.Find("ROG/TopDownStudy");
            facade = Material(shader, new Color(.42f, .4f, .34f), true);
            roof = Material(shader, new Color(.32f, .36f, .36f));
            trim = Material(shader, new Color(.24f, .28f, .28f));
            asphalt = Material(shader, new Color(.13f, .15f, .16f));
            pavement = Material(shader, new Color(.47f, .48f, .43f));
            red = Material(shader, new Color(.72f, .06f, .12f));
            boundaryMaterial = Material(shader, new Color(.1f, .11f, .12f));
        }

        private Material Material(Shader shader, Color color, bool windows = false)
        {
            var material = new Material(shader);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Facade", windows ? 1 : 0);
            materials.Add(material);
            return material;
        }

        public void Floor(Vector2 size)
        {
            // All free space remains street; no new road layout or walkability rules.
            Box(transform, "Asfalto", new Vector3(0, 0, .18f), new Vector3(size.x, size.y, .2f), asphalt, -10);
        }

        public void PrepareBuildings(RogZombie.TestEngine.ObstaclePlacement[] placements)
        {
            buildingHeights.Clear();
            if (placements == null) return;
            var buildings = new List<RogZombie.TestEngine.ObstaclePlacement>();
            foreach (var placement in placements) if (placement.Wall) buildings.Add(placement);
            var groups = new int[buildings.Count];
            for (int i = 0; i < groups.Length; i++) groups[i] = i;
            // Join touching pieces before assigning heights, including L/U-shaped footprints.
            // SAT uses the original rotation; a small tolerance absorbs reference-image rounding.
            for (int i = 0; i < groups.Length; i++)
                for (int j = i + 1; j < groups.Length; j++)
                    if (Touch(buildings[i], buildings[j]))
                    {
                        int from = groups[j], to = groups[i];
                        for (int k = 0; k < groups.Length; k++) if (groups[k] == from) groups[k] = to;
                    }
            var distinct = new List<int>();
            foreach (int group in groups) if (!distinct.Contains(group)) distinct.Add(group);
            for (int i = 0; i < groups.Length; i++)
                buildingHeights[buildings[i].Position] = Mathf.Lerp(5, 10,
                    distinct.Count <= 1 ? 0 : distinct.IndexOf(groups[i]) / (float)(distinct.Count - 1));
        }

        private static bool Touch(RogZombie.TestEngine.ObstaclePlacement a, RogZombie.TestEngine.ObstaclePlacement b)
        {
            Vector2 ax = Quaternion.Euler(0, 0, a.Rotation) * Vector3.right;
            Vector2 ay = new Vector2(-ax.y, ax.x);
            Vector2 bx = Quaternion.Euler(0, 0, b.Rotation) * Vector3.right;
            Vector2 by = new Vector2(-bx.y, bx.x);
            foreach (var axis in new[] { ax, ay, bx, by })
            {
                float extent = (Mathf.Abs(Vector2.Dot(axis, ax)) * a.Size.x + Mathf.Abs(Vector2.Dot(axis, ay)) * a.Size.y +
                    Mathf.Abs(Vector2.Dot(axis, bx)) * b.Size.x + Mathf.Abs(Vector2.Dot(axis, by)) * b.Size.y) * .5f;
                if (Mathf.Abs(Vector2.Dot(b.Position - a.Position, axis)) > extent + .03f) return false;
            }
            return true;
        }

        public void Obstacle(Transform footprint, Vector2 size, bool building, bool boundary)
        {
            if (boundary)
            {
                Box(footprint, "Bordo AREA", new Vector3(0, 0, -.25f), new Vector3(size.x, size.y, .5f), boundaryMaterial);
                return;
            }
            if (!building)
            {
                float height = Mathf.Min(size.x, size.y) < .9f ? 1.2f : .8f;
                Box(footprint, "Ostacolo rosso", new Vector3(0, 0, -height / 2), new Vector3(size.x, size.y, height), red);
                return;
            }
            float buildingHeight = buildingHeights.TryGetValue(footprint.position, out var heightForBuilding) ? heightForBuilding : 5;
            Box(footprint, "Marciapiede", new Vector3(0, 0, .04f), new Vector3(size.x + .6f, size.y + .6f, .04f), pavement, -10);
            Box(footprint, "Palazzo", new Vector3(0, 0, -buildingHeight / 2), new Vector3(size.x, size.y, buildingHeight), facade);
            Box(footprint, "Tetto", new Vector3(0, 0, -buildingHeight - .08f), new Vector3(size.x, size.y, .16f), roof);
            if (Mathf.Min(size.x, size.y) < 2) return;
            float ventSize = Mathf.Min(2, Mathf.Min(size.x, size.y) * .3f);
            Box(footprint, "Ventilazione tetto", new Vector3(0, 0, -buildingHeight - .4f), new Vector3(ventSize, ventSize, .6f), trim);
            for (int i = 0; i < 5; i++)
                Box(footprint, "Griglia ventilazione", new Vector3((i - 2) * ventSize / 5, 0, -buildingHeight - .72f),
                    new Vector3(ventSize / 15, ventSize * .8f, .04f), roof);
        }

        private static void Box(Transform parent, string name, Vector3 position, Vector3 size, Material material, int order = 1)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name; box.transform.SetParent(parent, false);
            box.transform.localPosition = position; box.transform.localScale = size;
            // Disable immediately, then dispose; visual volumes never take part in physics.
            var collider = box.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            var renderer = box.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material; renderer.sortingOrder = order;
        }

        private void OnDestroy()
        {
            foreach (var material in materials) if (material != null) Destroy(material);
        }
    }
}
