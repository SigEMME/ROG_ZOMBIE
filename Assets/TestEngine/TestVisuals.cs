using UnityEngine;

namespace RogZombie.TestEngine
{
    // Temporary code-generated presentation. No art assets or gameplay data are modified.
    public static class TestVisuals
    {
        public static Transform Root;
        private static Sprite square;
        private static Material lineMaterial;
        public static Sprite Square
        {
            get
            {
                if (square == null) square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
                return square;
            }
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

        public static void SpawnProjectile(Combatant owner, Vector2 position, Vector2 direction, float speed,
            float range, float damage, float radius, int penetrations, bool debug)
        {
            var go = Box("Projectile", position, Vector2.one * radius * 2f,
                owner.Faction == Faction.PG ? Color.yellow : new Color(1f, 0.3f, 0.7f), 4);
            go.AddComponent<Projectile>().Initialize(owner, direction, speed, range, damage, radius, penetrations, debug);
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
