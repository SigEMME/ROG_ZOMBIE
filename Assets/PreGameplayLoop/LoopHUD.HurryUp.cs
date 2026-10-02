using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed partial class LoopHUD
    {
        private Texture2D hurryMask;
        private readonly System.Collections.Generic.List<Vector2> flameOrigins =
            new System.Collections.Generic.List<Vector2>();
        private float flameTime;
        private bool hurryImageLoaded;

        private void Update()
        {
            if (loop != null && loop.HurryUp != null && loop.HurryUp.Raging &&
                loop.GameplayRunning && !loop.PauseMenuOpen) flameTime += Time.deltaTime;
            else if (loop == null || loop.HurryUp == null || !loop.HurryUp.Raging) flameTime = 0;
        }

        private void LoadHurryMask()
        {
            hurryImageLoaded = true;
            var source = Resources.Load<Texture2D>("HurryUp");
            if (source == null) { Debug.LogWarning("HurryUp HUD image missing.", this); return; }
            // Keep the imported artwork unchanged. A white alpha mask allows tinting
            // a black silhouette red; normal multiplicative tinting cannot do that.
            var pixels = source.GetPixels32();
            int left = source.width, bottom = source.height, right = -1, top = -1;
            for (int y = 0; y < source.height; y++)
                for (int x = 0; x < source.width; x++)
                    if (pixels[y * source.width + x].a > 16)
                    { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
            if (right < left) return;
            int width = right - left + 1, height = top - bottom + 1;
            var mask = new Color32[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    mask[y * width + x] = new Color32(255, 255, 255, pixels[(y + bottom) * source.width + x + left].a);
            hurryMask = new Texture2D(width, height, TextureFormat.RGBA32, false);
            hurryMask.name = "HURRY UP runtime tint mask";
            hurryMask.filterMode = FilterMode.Point;
            hurryMask.wrapMode = TextureWrapMode.Clamp;
            hurryMask.SetPixels32(mask); hurryMask.Apply(false, true);
            // Flame bases follow the silhouette contour instead of a rectangular frame.
            int step = Mathf.Max(1, width / 28);
            for (int x = 0; x < width; x += step)
                for (int y = height - 1; y >= 0; y--)
                    if (mask[y * width + x].a > 128)
                    { flameOrigins.Add(new Vector2(x / (float)width, 1 - y / (float)height)); break; }
        }

        private void DrawHurryUp(Rect bounds)
        {
            var timer = loop.HurryUp;
            if (timer == null || loop.Loading) return;
            if (!hurryImageLoaded) LoadHurryMask();
            if (hurryMask == null) return;
            float factor = Mathf.Min(bounds.width / hurryMask.width, bounds.height / hurryMask.height);
            var rect = new Rect(bounds.center.x - hurryMask.width * factor / 2, bounds.y,
                hurryMask.width * factor, hurryMask.height * factor);
            float progress = Mathf.Clamp01(1 - timer.Remaining / HurryUpRuntime.Duration);
            var previous = GUI.color;
            if (timer.Raging)
            {
                for (int i = 0; i < flameOrigins.Count; i++)
                {
                    var origin = flameOrigins[i];
                    float pulse = .5f + .5f * Mathf.Sin(flameTime * 9 + i * 2.4f);
                    float length = 5 + pulse * 13;
                    float x = rect.x + origin.x * rect.width;
                    float y = rect.y + origin.y * rect.height;
                    // Stepped rising tongues, orange outside and yellow at the base.
                    Fill(new Rect(x - 3, y - length * .6f, 6, length * .6f + 3), new Color(1, .22f, .015f, .85f));
                    Fill(new Rect(x - 1 + Mathf.Sin(flameTime * 6 + i) * 2, y - length, 3, length * .65f), new Color(1, .48f, .02f, .8f));
                    Fill(new Rect(x - 1, y - length * .3f, 3, length * .3f + 2), new Color(1, .85f, .2f));
                }
            }
            // One HUD pixel around the alpha silhouette, independent of fill progress.
            GUI.color = Color.black;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (dx != 0 || dy != 0)
                        GUI.DrawTexture(new Rect(rect.x + dx, rect.y + dy, rect.width, rect.height), hurryMask);
            GUI.color = new Color(.32f, .32f, .32f, .95f);
            GUI.DrawTexture(rect, hurryMask);
            float filled = rect.height * progress;
            if (filled > 0)
            {
                GUI.BeginGroup(new Rect(rect.x, rect.yMax - filled, rect.width, filled));
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(0, filled - rect.height, rect.width, rect.height), hurryMask);
                GUI.EndGroup();
            }
            GUI.color = previous;
            Hint(bounds, timer.Raging ? "HURRY UP — RAGE ATTIVA" :
                "HURRY UP — " + Mathf.CeilToInt(timer.Remaining) + " s");
        }

        private void OnDestroy()
        {
            if (hurryMask != null) Destroy(hurryMask);
        }
    }
}
