using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    [DisallowMultipleComponent]
    public sealed class TitleScreen : MonoBehaviour
    {
        public Texture2D Logo;
        public Texture2D StartImage;
        public Texture2D ExitImage;
        public bool IsVisible { get; private set; } = true;
        public static readonly Rect LogoRect = new Rect(348, 0, 840, 560);
        public static readonly Rect StartRect = new Rect(528, 530, 480, 160);
        public static readonly Rect ExitRect = new Rect(528, 680, 480, 160);

        public void StartGame()
        {
            if (!IsVisible) return;
            IsVisible = false;
        }

        public void ExitGame()
        {
            if (!IsVisible) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnGUI()
        {
            if (!IsVisible) return;
            var previousMatrix = GUI.matrix;
            var previousColor = GUI.color;
            GUI.matrix = Matrix4x4.identity;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float scale = Mathf.Min(Screen.width / 1536f, Screen.height / 864f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1536 * scale) / 2, (Screen.height - 864 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            if (Logo != null) GUI.DrawTexture(LogoRect, Logo, ScaleMode.ScaleToFit, true);
            bool start = ImageButton(StartRect, StartImage, "START");
            bool exit = ImageButton(ExitRect, ExitImage, "ESCI");
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
            if (start) StartGame();
            else if (exit) ExitGame();
        }

        private static bool ImageButton(Rect rect, Texture2D image, string label)
        {
            // The supplied artwork includes transparent margins; only the visible plate is clickable.
            var hit = new Rect(rect.x + rect.width * .06f, rect.y + rect.height * .17f, rect.width * .88f, rect.height * .65f);
            GUI.SetNextControlName(label);
            bool clicked = GUI.Button(hit, GUIContent.none, GUIStyle.none);
            var previousColor = GUI.color;
            bool highlighted = hit.Contains(Event.current.mousePosition) || GUI.GetNameOfFocusedControl() == label;
            GUI.color = highlighted ? new Color(1f, 1f, .85f) : Color.white;
            if (image != null) GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
            return clicked;
        }
    }
}
