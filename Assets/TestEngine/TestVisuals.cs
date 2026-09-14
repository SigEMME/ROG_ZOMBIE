using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    // Temporary code-generated presentation. No art assets or gameplay data are modified.
    public static class TestVisuals
    {
        public static void FlashOccludedArea(Vector2 center, float radius, Color color)
        {
            var go = new GameObject("Visible damage area");
            go.transform.SetParent(Root, false); go.transform.position = center; color.a = .4f;
            go.AddComponent<RogZombie.PreGameplayLoop.PG04AreaVisual>().InitializeOccluded(center, radius, color);
            Object.Destroy(go, .15f);
        }
        public static Transform Root;
        private static Sprite square;
        private static Material lineMaterial;
        public static Sprite Square
        {
            get
            {
                if (square == null) square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f, 0, SpriteMeshType.FullRect);
                return square;
            }
        }

        private static Texture2D circleTexture;
        private static readonly Dictionary<float, Sprite> circleSprites = new Dictionary<float, Sprite>();

        public static GameObject Circle(string name, Vector2 position, float radius, Color color, int order = 0)
        {
            const int resolution = 128;
            if (circleTexture == null)
            {
                circleTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
                { name = "Temporary circular MOB", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var pixels = new Color[resolution * resolution];
                for (int y = 0; y < resolution; y++)
                    for (int x = 0; x < resolution; x++)
                    {
                        float distance = new Vector2(x + .5f - resolution * .5f, y + .5f - resolution * .5f).magnitude;
                        pixels[y * resolution + x] = new Color(1, 1, 1, Mathf.Clamp01(resolution * .5f - distance));
                    }
                circleTexture.SetPixels(pixels);
                circleTexture.Apply(false, true);
            }
            if (!circleSprites.TryGetValue(radius, out var sprite) || sprite == null)
            {
                // Size the sprite in world metres; keep the actor transform at unit scale for its collider.
                sprite = Sprite.Create(circleTexture, new Rect(0, 0, resolution, resolution), Vector2.one * .5f,
                    resolution / (radius * 2), 0, SpriteMeshType.FullRect);
                circleSprites[radius] = sprite;
            }
            var go = new GameObject(name);
            if (Root != null) go.transform.SetParent(Root, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return go;
        }

        public static GameObject Box(string name, Vector2 position, Vector2 size, Color color, int order = 0)
        {
            var go = new GameObject(name);
            if (Root != null) go.transform.SetParent(Root, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Square;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = order;
            return go;
        }

        // One Unity world unit is one metre in the prototype. No collision components.
        public static void FloorGrid(Vector2 size)
        {
            var grid = new GameObject("Griglia pavimento 1 m x 1 m");
            if (Root != null) grid.transform.SetParent(Root, false);
            const float thickness = .025f;
            var color = new Color(.32f, .35f, .36f, .45f);
            for (int x = Mathf.CeilToInt(-size.x * .5f); x <= Mathf.FloorToInt(size.x * .5f); x++)
                Line(new Vector2(x, 0), new Vector2(thickness, size.y));
            for (int y = Mathf.CeilToInt(-size.y * .5f); y <= Mathf.FloorToInt(size.y * .5f); y++)
                Line(new Vector2(0, y), new Vector2(size.x, thickness));

            void Line(Vector2 position, Vector2 dimensions)
            {
                var go = new GameObject("Linea 1 m");
                go.transform.SetParent(grid.transform, false);
                go.transform.position = position;
                go.transform.localScale = new Vector3(dimensions.x, dimensions.y, 1);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = Square;
                renderer.color = color;
                renderer.sortingOrder = -9;
            }
        }

        public static Projectile SpawnProjectile(Combatant owner, Vector2 position, Vector2 direction, float speed,
            float range, float damage, float radius, int penetrations, bool debug)
        {
            var go = Box("Projectile", position, Vector2.one * radius * 2f,
                owner.Faction == Faction.PG ? Color.yellow : new Color(1f, 0.3f, 0.7f), 4);
            int layer = LayerMask.NameToLayer(owner.Faction == Faction.PG ? "PROJECTILE_PG" : "PROJECTILE_MOB");
            if (layer >= 0) go.layer = layer;
            var projectile = go.AddComponent<Projectile>();
            projectile.Initialize(owner, direction, speed, range, damage, radius, penetrations, debug);
            return projectile;
        }

        public static void FlashCircle(Vector2 center, float radius, Color color)
        {
            var points = new Vector3[49];
            for (int i = 0; i < points.Length; i++) points[i] = center + AttackGeometry.Direction(i * 360f / 48f) * radius;
            FlashLine(points, color);
        }

        public static void FlashFront(Vector2 origin, Vector2 direction, float range, WeaponDefinition definition)
        {
            Vector2 side = new Vector2(-direction.y, direction.x);
            if (definition.Shape == AttackShape.Cone && definition.ConeAngle > 0)
            {
                var sector = new Vector3[27];
                sector[0] = sector[26] = origin;
                float facing = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                for (int i = 0; i <= 24; i++)
                    sector[i + 1] = origin + AttackGeometry.Direction(facing - definition.ConeAngle / 2 + i * definition.ConeAngle / 24) * range;
                FlashLine(sector, Color.yellow);
                return;
            }
            if (definition.Shape == AttackShape.Cone)
                FlashLine(new Vector3[] { origin, origin + direction * range + side * definition.ConeWidth * 0.5f,
                    origin + direction * range - side * definition.ConeWidth * 0.5f, origin }, Color.yellow);
            else
            {
                var points = new Vector3[27];
                points[0] = origin;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                for (int i = 0; i <= 24; i++) points[i + 1] = origin + AttackGeometry.Direction(angle - 90f + i * 180f / 24f) * range;
                points[26] = origin;
                FlashLine(points, Color.yellow);
            }
        }

        public static void FlashRectangle(Vector2 origin, Vector2 direction, float depth, float width)
        {
            direction.Normalize();
            Vector2 side = new Vector2(-direction.y, direction.x) * width * .5f;
            Vector2 front = origin + direction * depth;
            FlashLine(new Vector3[] { origin - side, front - side, front + side, origin + side, origin - side }, Color.yellow);
        }

        public static void FlashCone(Vector2 origin, Vector2 direction, float range, float angle)
        {
            var points = new Vector3[27];
            points[0] = points[26] = origin;
            float facing = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            for (int i = 0; i <= 24; i++)
                points[i + 1] = origin + AttackGeometry.Direction(facing - angle * .5f + i * angle / 24f) * range;
            FlashLine(points, Color.yellow);
        }

        private static void FlashLine(Vector3[] points, Color color)
        {
            var go = new GameObject("Attack flash");
            if (Root != null) go.transform.SetParent(Root, false);
            var line = go.AddComponent<LineRenderer>();
            if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial = lineMaterial;
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.startWidth = line.endWidth = 0.045f;
            line.startColor = line.endColor = color;
            line.sortingOrder = 5;
            Object.Destroy(go, 0.15f);
        }
    }
}
